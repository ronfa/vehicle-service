using System.Collections.Generic;

namespace VehicleNotificationService.Business.Model
{
    public class VehicleEvent
    {
        public string LicensePlate { get; set; }
        public string Timestamp { get; set; }
        public string CountryCode { get; set; }
        public string Brand { get; set; }
        public string SourceApplication { get; set; }
        public List<KeyValuePair<string, string>> EventDetails { get; set; }
    }
}
