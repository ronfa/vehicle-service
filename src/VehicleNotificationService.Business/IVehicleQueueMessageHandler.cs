
using System.Threading.Tasks;
using Amazon.Lambda.SQSEvents;

namespace VehicleNotificationService.Business
{
    public interface IVehicleQueueMessageHandler
    {
        Task<int> HandleSQSEvent(SQSEvent sqsEvent);
    }
}
