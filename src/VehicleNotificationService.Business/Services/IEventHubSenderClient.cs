using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VehicleNotificationService.Business.Model;

namespace VehicleNotificationService.Business.Services
{
    public interface IEventHubSenderClient
    {
        Task<int> SendAsync(List<string> message);
    }
}
