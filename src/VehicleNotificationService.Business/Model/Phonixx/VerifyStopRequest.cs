using System;
using System.Collections.Generic;

namespace VehicleNotificationService.Business.Model.Phonixx
{
    public class VerifyStopRequest
    {
        public DateTime TimestampUtc { get; set; }

        public string CountryCode { get; set; }
        public int SupplierId { get; set; }
        public Guid TripId { get; set; }
        public string SourceApplication { get; set; }
        public List<Identification> Identification { get; set; }
    }
}
