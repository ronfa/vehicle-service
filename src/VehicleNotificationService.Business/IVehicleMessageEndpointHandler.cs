using System.Net;
using System.Threading.Tasks;

namespace VehicleNotificationService.Business
{
    public interface IVehicleMessageEndpointHandler
    {
        Task<HttpStatusCode> QueueVehicleNotification(string messageBody);
    }
}
