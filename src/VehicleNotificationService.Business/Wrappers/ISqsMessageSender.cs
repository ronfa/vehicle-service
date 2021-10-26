using Amazon.SQS.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VehicleNotificationService.Business.Wrappers
{
    public interface ISqsMessageSender
    {
        Task<List<SendMessageResponse>> SendQueueMessagesAsync(List<object> messages);
    }
}
