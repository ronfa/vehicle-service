using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VehicleNotificationService.Business;
using VehicleNotificationService.EventHubWorker.Mapping;
using VehicleNotificationService.EventHubWorker.Model;

namespace VehicleNotificationService.EventHubWorker
{
    public class EventHubConsumer : IEventHubConsumer
    {
        private EventHubConsumerClient _eventHubConsumerClient;
        private readonly IVehicleMessageEndpointHandler _vehicleMessageEndpointHandler;
        private readonly ILogger _logger;
        private readonly IEventHubConsumerOptions _eventHubConsumerOptions;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        public EventHubConsumer(ILogger logger, IVehicleMessageEndpointHandler vehicleMessageEndpointHandler, IEventHubConsumerOptions eventHubConsumerOptions)
        {
            _logger = logger;
            _vehicleMessageEndpointHandler = vehicleMessageEndpointHandler;
            _eventHubConsumerOptions = eventHubConsumerOptions;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var eventHubOptions = _eventHubConsumerOptions;
            CreateClient(eventHubOptions);

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    _logger.LogInformation($"Polling the eventhub for new events from {_eventHubConsumerOptions.EventHubName}...");
                    Console.WriteLine("Waiting for incoming events...");
                    var partitionIds = await _eventHubConsumerClient.GetPartitionIdsAsync(cancellationToken);
                    foreach (var id in partitionIds)
                    {
                        DateTimeOffset timeOffset = GetPartitionLastReadOffset(_eventHubConsumerOptions.OffsetPeriodInMinutes);
                        await foreach (var partitionEvent in _eventHubConsumerClient.ReadEventsFromPartitionAsync(id, EventPosition.FromEnqueuedTime(timeOffset), cancellationToken).ConfigureAwait(false))
                        {
                            string readFromPartition = partitionEvent.Partition.PartitionId;
                            var eventBody = partitionEvent.Data.EventBody.ToString();

                            _logger.LogInformation($"Reading event request body: {eventBody} from timeOffset: {timeOffset}");

                            var message = DeserializeMessage(eventBody);

                            if (message == null)
                            {
                                _logger.LogError(
                                    $"{nameof(StartAsync)} -> Message format is invalid, could not process request");
                                throw new InvalidOperationException($"Message format is invalid, could not process request");
                            }

                            var request = JsonConvert.SerializeObject(message.ToVehicleEventRequest());

                            await _vehicleMessageEndpointHandler.QueueVehicleNotification(request);

                        }
                    }

                    Console.WriteLine("Press any key to shutdown");
                    Console.ReadLine();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(EventHubConsumer)} error occurred while reading eventhub events -> {ex.Message}");
            }
            finally
            {
                await _eventHubConsumerClient.CloseAsync();
            }
        }

        private DateTimeOffset GetPartitionLastReadOffset(int offsetPeriodInMinutes)
        {
            return DateTimeOffset.UtcNow.Subtract(TimeSpan.FromMinutes(offsetPeriodInMinutes));
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Stopping eventhub receiver  {_eventHubConsumerOptions?.EventHubName}.");

            _cancellationTokenSource.Cancel();

            try
            {
                _eventHubConsumerClient?.DisposeAsync();
            }
            catch (AggregateException ex)
            {
                if (!ex.InnerExceptions.All(x => x is TaskCanceledException))
                    _logger.LogError(ex.ToString());
            }
            finally
            {
                _cancellationTokenSource?.Dispose();
            }

            _logger.LogDebug($"Stopped eventhub receiver {_eventHubConsumerOptions?.EventHubName}.");
            return Task.CompletedTask;
        }

        private void CreateClient(IEventHubConsumerOptions eventHubConsumerOptions)
        {
            string consumerGroup = EventHubConsumerClient.DefaultConsumerGroupName;
            var consumerOptions = new EventHubConsumerClientOptions();
            consumerOptions.ConnectionOptions.TransportType = EventHubsTransportType.AmqpWebSockets;

            consumerOptions.RetryOptions = new EventHubsRetryOptions
            {
                Mode = EventHubsRetryMode.Exponential,
                MaximumRetries = eventHubConsumerOptions.MaximumRetries,
                Delay = TimeSpan.FromMilliseconds(eventHubConsumerOptions.DelayInMilliSeconds),
                MaximumDelay = TimeSpan.FromSeconds(eventHubConsumerOptions.MaximumDelayInSeconds),
            };
            _eventHubConsumerClient = new EventHubConsumerClient(
                consumerGroup,
                eventHubConsumerOptions.EventHubConnectionString,
                eventHubConsumerOptions.EventHubName,
                consumerOptions);
        }

        private EventHubMessage DeserializeMessage(string messageBody)
        {
            var formatSettings = new JsonSerializerSettings
            {
                DateFormatHandling = DateFormatHandling.MicrosoftDateFormat,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc
            };

            return JsonConvert.DeserializeObject<EventHubMessage>(messageBody, formatSettings);
        }
    }
}
