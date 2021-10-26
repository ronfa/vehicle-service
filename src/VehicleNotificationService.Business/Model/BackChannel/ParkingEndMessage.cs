using System;
using System.Collections.Generic;
using System.Text;

namespace VehicleNotificationService.Business.Model.BackChannel
{
    class ParkingEndMessage : BackChannelBaseMessage
    {
        public Data Data { get; set; }
    }

    public partial class Data
    {
        public ParkingEnd ParkingEnd { get; set; }
    }
    public class ParkingEnd
    {
        public string ParkingSessionId { get; set; }
        public DateTime Time { get; set; }
    }
}
