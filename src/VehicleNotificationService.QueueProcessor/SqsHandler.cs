using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Microsoft.Extensions.Logging;
using Overleaf.Logging.Lambda;
using Overleaf.Lambda;
using VehicleNotificationService.Business;
using VehicleNotificationService.Business.Model;
using System;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using VehicleNotificationService.Business.Model.Phonixx;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
namespace VehicleNotificationService.QueueProcessor
{
    public partial class SqsHandler : LambdaBase
    {
        private readonly IVehicleQueueMessageHandler _vehicleQueueMessageHandler;
        private readonly IBackChannelMessageManager _backChannelMessageManager;

        public string HandleSQSEvent(SQSEvent sqsEvent, ILambdaContext context)
        {
            logger.LogDebug($"{nameof(HandleSQSEvent)}: Lambda handler called");

            var count = _vehicleQueueMessageHandler.HandleSQSEvent(sqsEvent).Result;

            return $"Processed {count} records.";
        }

        public string HandleBackChannelSQSEventAsync(SQSEvent sqsEvent, ILambdaContext context)
        {
            logger.LogDebug($"{nameof(HandleBackChannelSQSEventAsync)}: BackChannel lambda handler called");

            var request = ExtractBackChannelMessage(sqsEvent);

            logger.LogInformation($"{nameof(HandleBackChannelSQSEventAsync)} Sending event to eventhub");
            var count = _backChannelMessageManager.SendBackChannelEventAsync(request).Result;
            return $"Processed {count} records.";
        }

        private List<string> ExtractBackChannelMessage(SQSEvent sqsEvent)
        {
            var records = sqsEvent.Records.Select(t => t.Body).ToList();
            return records;
        }
    }
}