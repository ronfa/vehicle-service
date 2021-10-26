using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VehicleNotificationService.EventHubWorker
{
    class EventHubHostedService : IHostedService
    {
        private readonly IEventHubConsumer _eventHubConsumer;
        private readonly ILogger _logger;
        public EventHubHostedService(IEventHubConsumer eventHubConsumer, ILogger logger)
        {
            _eventHubConsumer = eventHubConsumer;
            _logger = logger;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("WebFleetEventHub worker is starting.");
            _logger.LogInformation("WebFleetEventHub worker is starting.");
            await _eventHubConsumer.StartAsync(cancellationToken);
            _logger.LogInformation("WebFleetEventHub worker has started.");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("WebFleetEventHub worker is stopping.");
            _logger.LogInformation("WebFleetEventHub worker is stopping.");
            await _eventHubConsumer.StopAsync(cancellationToken);
            _logger.LogInformation("WebFleetEventHub worker has stopped.");
        }
    }
}
