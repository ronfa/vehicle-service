using System;
using System.Collections.Generic;
using Amazon.Lambda.SQSEvents;
using Newtonsoft.Json;
using VehicleNotificationService.Business.Model;
using VehicleNotificationService.Business.Model.BackChannel;
using VehicleNotificationService.Business.Model.Phonixx;
using Xunit;

namespace VehicleNotificationService.QueueProcessor.Tests
{
    public class VehicleQueueProcessorTests
    {
        public VehicleQueueProcessorTests()
        {
        }

        [Fact]
        public void GetHandler_SuccessTest()
        {

            var vehicleEvent = JsonConvert.SerializeObject(new VehicleEvent
            {
                CountryCode = "NL",
                LicensePlate = "RRTTEE"
            });

            var function = new SqsHandlerTestOverride();
            var myEvent = new SQSEvent()
            {
                Records = new List<SQSEvent.SQSMessage>() {
                    new SQSEvent.SQSMessage()
                    {
                        Body = vehicleEvent,
                        ReceiptHandle = "MessageReceiptHandle",
                        MessageId = "19dd0b57-b21e-4ac1-bd88-01bbb068cb78"
                    }
                }
            };

            function.HandleSQSEvent(myEvent, null);
            Assert.Equal(200, 200);
        }

        [Fact]
        public void GetBackChannelHandler_SuccessTest()
        {
            var backChannelEvent = JsonConvert.SerializeObject(new BackChannelRequest
            {
                CountryCode = "NL",
                Vrn = "RTNL90",
                ClientId = "1224",
                StartTimeUtc = DateTime.UtcNow.AddHours(-2),
                StopTimeUtc = DateTime.UtcNow,
                MaxStopTimeUtc = DateTime.UtcNow.AddHours(2),
                JobType = "ParkingActionActivated"
            });

            var function = new SqsHandlerTestOverride();
            var myEvent = new SQSEvent()
            {
                Records = new List<SQSEvent.SQSMessage>() {
                    new SQSEvent.SQSMessage()
                    {
                        Body = backChannelEvent,
                        ReceiptHandle = "MessageReceiptHandle",
                        MessageId = "19dd0b57-b21e-4ac1-bd88-01bbb068cb78"
                    }
                }
            };

            function.HandleBackChannelSQSEventAsync(myEvent, null);
            Assert.Equal(200,200);
        }
    }
}
