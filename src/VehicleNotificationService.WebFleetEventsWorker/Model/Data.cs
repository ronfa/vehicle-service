using System;
using System.Collections.Generic;
using System.Text;

namespace VehicleNotificationService.EventHubWorker.Model
{
     public class Data
    {
        public VehicleIdentification VehicleIdentification { get; set; }
        public StandStillEnd StandStillEnd { get; set; }
    }

    public class VehicleIdentification
    {
        public string VehicleId { get; set; }
        public string ThirdPartyReferenceNumber { get; set; }
    }

    public class StandStillEnd
    {
        public string Time { get; set; }
    }
}
