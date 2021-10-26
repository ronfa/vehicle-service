using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VehicleNotificationService.Business.Mappings;
using VehicleNotificationService.Business.Model.Configuration;
using VehicleNotificationService.Business.Model.Phonixx;

namespace VehicleNotificationService.Business.Services
{
    public class EventHubSenderClient : IEventHubSenderClient
    {
        private ILogger Logger { get; }
        private readonly EventHubConfig _eventHubConfig;
        private readonly ISqsMessageService _sqsService;

        public EventHubSenderClient(IOptions<EventHubConfig> config, ILogger logger, ISqsMessageService sqsService)
        {
            _eventHubConfig = config.Value;
            Logger = logger;
            _sqsService = sqsService;
        }
        public async Task<int> SendAsync(List<string> messages)
        {
            Logger.LogInformation($"Connectionstring: {_eventHubConfig.ConnectionString}, EventhubName : {_eventHubConfig.EventHubName}");

            await using (var producerClient = new EventHubProducerClient(
                _eventHubConfig.ConnectionString, 
                _eventHubConfig.EventHubName, 
                // Make sure to use web sockets, port 443
                new EventHubProducerClientOptions 
                {
                    ConnectionOptions = new EventHubConnectionOptions
                    {
                        TransportType = EventHubsTransportType.AmqpWebSockets
                    }
                }))
            {
                var count = 0;
                //EventDataBatch eventBatch = producerClient.CreateBatchAsync().Result;

                foreach (var message in messages)
                {
                    try
                    {
                        Logger.LogInformation($"{nameof(SendAsync)} Processing message body: {message}");
                        var request = JsonConvert.DeserializeObject<BackChannelRequest>(message);

                        if (!string.IsNullOrEmpty(request.ZoneType) && request.ZoneType.Contains("OffStreet"))
                        {
                            Logger.LogInformation($"{nameof(SendAsync)} Invalid zoneType {request.ZoneType}. Message will be moved to the dead letter queue.");
                            await _sqsService.SendToDeadletterQueue(message);
                        }

                        var backChannelMessage = request.ToBackChannelMessageFormat();
                        var record = JsonConvert.SerializeObject(backChannelMessage, Formatting.None,
                            new JsonSerializerSettings
                            {
                                NullValueHandling = NullValueHandling.Ignore,
                                ContractResolver = new CamelCasePropertyNamesContractResolver()
                            });

                        string recordString = string.Join(Environment.NewLine, record);

                        EventData eventData = new EventData(Encoding.UTF8.GetBytes(recordString));
                        Logger.LogInformation($"sending message {recordString}");
                        eventData.Properties.Add("Format", "json");
                        producerClient.SendAsync(new[] {eventData}).Wait();
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"An error occurred while sending event", ex);
                        throw;
                    }
                    count++;
                }

                Logger.LogInformation($"A batch of {count} events has been published");

                return count;
            }
        }
    }
}
