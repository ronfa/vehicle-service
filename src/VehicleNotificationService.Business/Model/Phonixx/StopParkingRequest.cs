using System;
using System.Collections.Generic;

namespace VehicleNotificationService.Business.Model.Phonixx
{
    public class StopParkingRequest
    {
        public int Id { get; set; }
        public DateTime StopTimeUtc { get; set; }
        public Guid TripId { get; set; }
        public string SourceApplication { get; set; }
        public List<Identification> Identification { get; set; }
    }
}
