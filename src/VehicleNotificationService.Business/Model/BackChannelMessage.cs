using System;

namespace VehicleNotificationService.Business.Model
{
    public class BackChannelMessage
    {
        public string Version { get; set; }
        public string MessageClass { get; set; }
        public Data Data { get; set; }
    }

    public class Data
    {
        public VehicleIdentification VehicleIdentification { get; set; }
        public ParkingStart ParkingStart { get; set; }

        public StandStillEnd StandStillEnd { get; set; }
    }

    public class StandStillEnd
    {
        public DateTime Time { get; set; }
    }

    public class VehicleIdentification
    {
        public string ReferenceNumber { get; set; }
        public string LicensePlate { get; set; }
        public string ParkNowCustomerNumber { get; set; }
    }

    public class ParkingStart
    {
        public DateTime Time { get; set; }
        public DateTime MaxParkingTime { get; set; }
    }
}
