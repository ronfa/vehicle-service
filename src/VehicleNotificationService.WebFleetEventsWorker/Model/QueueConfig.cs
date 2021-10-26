using System;
using System.Collections.Generic;
using System.Text;

namespace VehicleNotificationService.EventHubWorker.Model
{
    public class QueueConfig
    {
        public string TargetQueueUrl { get; set; }
        public string DeadletterQueueUrl { get; set; }
    }
}
