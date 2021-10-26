using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VehicleNotificationService.Business.Model;
using VehicleNotificationService.Business.Model.Phonixx;

namespace VehicleNotificationService.Business
{
    public interface IBackChannelMessageManager
    {
        Task<int> SendBackChannelEventAsync(List<string> messages);
    }
}
