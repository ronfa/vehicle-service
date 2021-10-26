using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using VehicleNotificationService.Business.Services;

namespace VehicleNotificationService.Business
{
    public class BackChannelMessageManager : IBackChannelMessageManager
    {
        private ILogger Logger { get; }
        private readonly IEventHubSenderClient _eventHubSenderClient;
        

        public BackChannelMessageManager(ILogger logger, IEventHubSenderClient eventHubSenderClient)
        {
            Logger = logger;
            _eventHubSenderClient = eventHubSenderClient;            
        }

        public async Task<int> SendBackChannelEventAsync(List<string> messages)
        {
            Logger.LogInformation($"{nameof(SendBackChannelEventAsync)} Calling eventhub producer client");
            return await _eventHubSenderClient.SendAsync(messages);
        }
    }
}
