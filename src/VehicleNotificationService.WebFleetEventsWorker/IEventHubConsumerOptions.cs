using System;
using System.Collections.Generic;
using System.Text;

namespace VehicleNotificationService.EventHubWorker
{
    public interface IEventHubConsumerOptions
    {        
        string FullyQualifiedNamespace { get; set; }
        string EventHubName { get; set; }
        string EventHubConnectionString { get; set; }
        int OffsetPeriodInMinutes { get; set; }
        int MaximumRetries { get; set; }
        int DelayInMilliSeconds { get; set; }
        int MaximumDelayInSeconds { get; set; }

        void AssertOptionsAreValid();
    
    }
}
