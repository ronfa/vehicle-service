using System.Collections.Generic;

namespace VehicleNotificationService.Business.Model.Configuration
{
    public class PhonixxReceiveEndpointConfig
    {
        public string XApiKey { get; set; }
        public List<Supplier> Suppliers { get; set; }
        public string RelativeVerifyUrl { get; set; }
        public string RelativeStopUrl { get; set; }
    }

    public class Supplier
    {
        public int SupplierId { get; set; }
        public string Url { get; set; }
    }
}