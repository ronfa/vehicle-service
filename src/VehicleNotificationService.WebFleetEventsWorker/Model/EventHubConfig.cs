using System;
using System.Collections.Generic;
using System.Text;

namespace VehicleNotificationService.EventHubWorker.Model
{
    public class EventHubConfig
    {
        public string FullyQualifiedNamespace { get; set; }
        public string EventHubName { get; set; }
        public string EventHubConnectionString { get; set; }
        public string SharedAccessKey { get; set; }
    }
}
