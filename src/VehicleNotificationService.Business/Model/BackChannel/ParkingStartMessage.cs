using System;
using System.Collections.Generic;
using System.Text;

namespace VehicleNotificationService.Business.Model.BackChannel
{
    class ParkingStartMessage : BackChannelBaseMessage
    {
        public Data Data { get; set; }
    }

    public partial class Data
    {
        public ParkingStart ParkingStart { get; set; }
    }
    public class ParkingStart
    {
        public DateTime Time { get; set; }
        public DateTime MaxParkingTime { get; set; }
        public string ParkingSessionId { get; set; }
    }
}
