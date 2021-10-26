using System;

namespace VehicleNotificationService.Business.Model.BackChannel
{
    public class VehicleActivatedMessage : BackChannelBaseMessage
    {
        public Data Data { get; set; }
    }
    public partial class Data
    {
        public VehicleActivated VehicleActivated { get; set; }
    }
    public class VehicleActivated
    {
        public DateTime Time { get; set; }
    }
}
