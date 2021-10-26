using System;
using System.Collections.Generic;
using System.Text;

namespace VehicleNotificationService.EventHubWorker.Model
{
    public class EventHubMessage
    {
        public string SubscriptionId { get; set; }
        public string ThirdPartyIntegrationId { get; set; }
        public string MessageClass { get; set; }

        public string CountryCode { get; set; }

        public string Version { get; set; }

        public Data Data { get; set; }
    }
}
