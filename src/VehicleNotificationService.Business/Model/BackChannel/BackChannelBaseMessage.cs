using System;
using System.Collections.Generic;
using System.Text;

namespace VehicleNotificationService.Business.Model.BackChannel
{
    public class BackChannelBaseMessage
    {
        public string Version { get; set; }
        public string MessageClass { get; set; }
    }
}
