using System.Threading;
using System.Threading.Tasks;

namespace VehicleNotificationService.EventHubWorker
{
    public interface IEventHubConsumer
    {        
        Task StartAsync(CancellationToken cancellationToken);
        Task StopAsync(CancellationToken cancellationToken);
    }
}
