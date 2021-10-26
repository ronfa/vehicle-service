using System;
using System.Collections.Generic;
using System.Text;

namespace VehicleNotificationService.Business.Model.BackChannel
{
    public class VehicleDeactivatedMessage : BackChannelBaseMessage
    {
        public Data Data { get; set; }
    }
    public partial class Data
    {
        public VehicleDeactivated VehicleDeactivated { get; set; }
    }
    public class VehicleDeactivated
    {
        public DateTime Time { get; set; }
    }
}
