using Amazon.Lambda.SQSEvents;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using VehicleNotificationService.Business.Engines;
using VehicleNotificationService.Business.Errors;
using VehicleNotificationService.Business.Extensions;
using VehicleNotificationService.Business.Helpers;
using VehicleNotificationService.Business.Model;
using VehicleNotificationService.Business.Model.Configuration;
using VehicleNotificationService.Business.Services;

namespace VehicleNotificationService.Business
{
    public class VehicleQueueMessageHandler : IVehicleQueueMessageHandler
    {
        private ILogger _logger;
        private readonly ApplicationConfig _applicationConfig;
        private readonly ISqsMessageService _sqsService;
        private readonly IParkingService _parkingService;
        private readonly IParkingEngine _automaticStopEngine;

        public VehicleQueueMessageHandler(IOptions<ApplicationConfig> appConfig, ILogger logger,
            ISqsMessageService sqsService, IParkingService parkingService, IParkingEngine automaticStopEngine)
        {
            _logger = logger;
            _applicationConfig = appConfig.Value;
            _sqsService = sqsService;
            _parkingService = parkingService;
            _automaticStopEngine = automaticStopEngine;
        }

        public async Task<int> HandleSQSEvent(SQSEvent sqsEvent)
        {
            var records = sqsEvent.Records.Select(t => t.Body).ToList();

            var baseLogPrefix = $"{nameof(HandleSQSEvent)}:";
            _logger.LogInformation($"{baseLogPrefix}): Processing {records.Count} records...");

            var count = 0;
            foreach (var record in records)
            {
                _logger.LogInformation($"{baseLogPrefix}): Start processing record {count}");

                VehicleEvent message = null;

                try
                {
                    message = JsonConvert.DeserializeObject<VehicleEvent>(record);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        $"{baseLogPrefix} Fatal error occurred. Message format is invalid and cannot be processed. Message will be moved to the dead letter queue.");
                    await _sqsService.SendToDeadletterQueue(record);
                    continue;
                }

                try
                {
                    var logPrefix = $"{baseLogPrefix} [{message.SourceApplication ?? "Unknown Source"}] -";
                    _logger.LogInformation($"{logPrefix} Processing message {record}");

                    // Resolve supplier by country and brand
                    var supplierId = PhonixxEnvironmentHelper.ResolveSupplierId(message);

                    _logger.LogInformation($"{logPrefix} Supplier resolved, supplier id: {supplierId}");

                    if (supplierId == 0)
                    {
                        _logger.LogError(
                            $"{logPrefix} Fatal error occurred. Cannot determine phonixx supplier. Message will be moved to the dead letter queue.");
                        await _sqsService.SendToDeadletterQueue(record);
                        continue;
                    }

                    _logger.LogInformation($"{logPrefix} Verifying parking against phonixx supplier id: {supplierId}");

                    var tripId = Guid.NewGuid();
                    var verifyResponse = await _parkingService.VerifyParking(message, supplierId, tripId);

                    if (verifyResponse == null || string.IsNullOrWhiteSpace(verifyResponse.StatusCode))
                    {
                        _logger.LogError(
                            $"{logPrefix} Error occurred while verifying parking against Phonixx. Message will be moved to the dead letter queue.");
                        await _sqsService.SendToDeadletterQueue(record);
                        continue;
                    }

                    // Evaluating whether any parking sessions should be auto-stopped
                    var parkingActionsToStop = _automaticStopEngine.Evaluate(verifyResponse, out var messages);


                    _logger.LogInformation(
                        $"{logPrefix} Engine evaluated {parkingActionsToStop?.Count ?? 0} parking actions to be stopped with supplier: {supplierId}." +
                        $"Messages: {messages.FormatAsString()}");

                    if (parkingActionsToStop == null || !parkingActionsToStop.Any())
                    {
                        count++;
                        continue;
                    }

                    // We have an identified running parking session in phonixx, lets stop it!
                    _logger.LogInformation(
                        $"{logPrefix} Active parking sessions found to stop with supplier id: {supplierId}");

                    var stopResponse =
                        _parkingService.StopParking(message, verifyResponse, parkingActionsToStop, supplierId, tripId);

                    if (stopResponse == null || stopResponse.Result.Any(x => x.StatusCode != HttpStatusCode.OK))
                    {
                        _logger.LogError(
                            $"{logPrefix} Error occurred while stopping parking against Phonixx. Message will be moved to the dead letter queue.");
                        await _sqsService.SendToDeadletterQueue(record);
                        continue;
                    }

                    _logger.LogInformation(
                        $"{logPrefix} {parkingActionsToStop.Count} active parking session(s) successfully stopped");
                    count += parkingActionsToStop.Count;

                }
                catch (UserNotFoundException)
                {
                    _logger.LogWarning($"Could not find user for given license plate {message?.LicensePlate} ");
                }
                catch (TimeoutException ex)
                {
                    // In case of timeouts with external services, re-queue message for retry
                    _logger.LogWarning(ex,
                        $"{baseLogPrefix} Timeout error occurred while processing vehicle event. Message will be re-queued.");

                    await _sqsService.SendToMessageQueue(message);
                }
                catch (Exception ex)
                {
                    // an error occurred when processing a vehicle notification. Moving message to dead letter queue ,
                    _logger.LogError(ex, $"{baseLogPrefix} A fatal error occurred while processing vehicle event. The message will be sent to the dead letter queue.");
                    await _sqsService.SendToDeadletterQueue(record);
                }
            }

            _logger.LogInformation($"{baseLogPrefix} Processing complete, successfully processed {count}/{records.Count} records.");

            return count;
        }
    }
}
