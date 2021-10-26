using System;

namespace VehicleNotificationService.Business.Model.BackChannel
{
    public class BackChannelMessage
    {
        public string Version { get; set; }
        public string MessageClass { get; set; }
        public Data Data { get; set; }
    }

    
}
