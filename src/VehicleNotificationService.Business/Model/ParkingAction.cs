using System;

namespace VehicleNotificationService.Business.Model
{
    public class ParkingAction
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Location { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime StartTimeUtc { get; set; }
        public decimal DistanceInMeters { get; set; }
        public DateTime? AutoStopTimeUtc { get; set; }
    }
}
