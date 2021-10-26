using System;

namespace VehicleNotificationService.Business.Model.Phonixx
{
    public class BackChannelRequest
    {
        public string ClientId { get; set; }
        public string JobType { get; set; }
        public string Event { get; set; }
        public string Version { get; set; }
        public DateTime StartTimeUtc { get; set; }
        public string LicensePlate { get; set; }
        public DateTime StopTimeUtc { get; set; }
        public DateTime MaxStopTimeUtc { get; set; }
        public string  ZoneType { get; set; }
        public string CountryCode { get; set; }
        public string TrackingProvider { get; set; }
        public string Vrn { get; set; }
        public string ParkingActionId { get; set; }
        public int TrackingType { get; set; }
        public string Brand { get; set; }


    }
}
