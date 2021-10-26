using System;
using System.Collections.Generic;
using System.Text;

namespace VehicleNotificationService.EventHubWorker
{
    public class EventHubConsumerOptions : IEventHubConsumerOptions
    {
        public string FullyQualifiedNamespace { get; set; }
        public string EventHubName { get; set; }
        public string EventHubConnectionString { get; set; }
        public void AssertOptionsAreValid()
        {

            if (string.IsNullOrWhiteSpace(FullyQualifiedNamespace) && string.IsNullOrWhiteSpace(EventHubName) && string.IsNullOrWhiteSpace(EventHubConnectionString))
                throw new ArgumentException("FullyQualifiedNamespace, EventHubName and EventHubConnectionString should not be null or empty");
        }

        public int OffsetPeriodInMinutes { get; set; }

        public int MaximumRetries { get; set; }
        public int DelayInMilliSeconds { get; set; }
        public int MaximumDelayInSeconds { get; set; }
    }
}
