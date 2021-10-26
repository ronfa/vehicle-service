using System.Threading.Tasks;
using Amazon.SQS.Model;
using VehicleNotificationService.Business.Model;

namespace VehicleNotificationService.Business.Services
{
    public interface ISqsMessageService
    {
        Task<SendMessageResponse> SendToMessageQueue(VehicleEvent message);

        Task<SendMessageResponse> SendToDeadletterQueue(string message);
    }
}
