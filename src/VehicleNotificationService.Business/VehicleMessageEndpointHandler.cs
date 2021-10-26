using System;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Threading.Tasks;
using Amazon.SQS.Model;
using Newtonsoft.Json;
using VehicleNotificationService.Business.Model;
using VehicleNotificationService.Business.Services;

namespace VehicleNotificationService.Business
{
     public class VehicleMessageEndpointHandler : IVehicleMessageEndpointHandler
    {
        private readonly ILogger _logger;
        private readonly ISqsMessageService _sqsMessageService;

        public VehicleMessageEndpointHandler(ILogger logger, ISqsMessageService sqsMessageService)
        {
            _logger = logger;
            _sqsMessageService = sqsMessageService;
        }

        public async Task<HttpStatusCode> QueueVehicleNotification(string messageBody)
        {
            _logger.LogInformation(
                $"{nameof(QueueVehicleNotification)}: Incoming vehicle event from 3rd party : {messageBody} ");

            HttpStatusCode result;

            var request = DeserializeMessage(messageBody);

            if (request == null)
            {
                _logger.LogError(
                    $"{nameof(QueueVehicleNotification)} -> Message format is invalid, could not process request");
                return HttpStatusCode.BadRequest;
            }

            try
            {
                result = await QueueMessage(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(QueueVehicleNotification)} -> {ex.Message}");
                return HttpStatusCode.InternalServerError;
            }

            return result;
        }

        private VehicleEvent DeserializeMessage(string messageBody)
        {
            var formatSettings = new JsonSerializerSettings
            {
                DateFormatHandling = DateFormatHandling.MicrosoftDateFormat,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc
            };

            return JsonConvert.DeserializeObject<VehicleEvent>(messageBody, formatSettings);
        }


        private async Task<HttpStatusCode> QueueMessage(VehicleEvent message)
        {

            SendMessageResponse response = null;
            try
            {
                response = await _sqsMessageService.SendToMessageQueue(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred when sending messages to the queue");
                return HttpStatusCode.InternalServerError;
            }

            return response.HttpStatusCode == HttpStatusCode.OK ? HttpStatusCode.OK : HttpStatusCode.BadRequest;
        }
    }
}