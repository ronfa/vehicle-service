using System;
using System.Collections.Generic;
using System.Text;

namespace VehicleNotificationService.Business.Model.BackChannel
{
    public class VehicleIdentification
    {
        public string ThirdPartyReferenceNumber { get; set; }
        public string LicensePlate { get; set; }
        public string ParkNowCustomerNumber { get; set; }
    }
}
