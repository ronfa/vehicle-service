using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.Lambda.SQSEvents;
using Amazon.SQS.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using Overleaf.Configuration;
using Overleaf.Logging.Lambda;
using VehicleNotificationService.Business.Engines;
using VehicleNotificationService.Business.Errors;
using VehicleNotificationService.Business.Model;
using VehicleNotificationService.Business.Model.Configuration;
using VehicleNotificationService.Business.Services;
using Xunit;
using ApplicationConfig = VehicleNotificationService.Business.Model.Configuration.ApplicationConfig;
using LambdaLoggerOptions = Overleaf.Logging.Lambda.LambdaLoggerOptions;

namespace VehicleNotificationService.Business.Tests
{
    public class VehicleQueueMessageHandlerTests
    {
        readonly VehicleQueueMessageHandler _vehicleQueueMessageHandler = null;
        private readonly Mock<ISqsMessageService> _sqsMessageService = null;
        private readonly Mock<IParkingService> _parkingService = null;

        public VehicleQueueMessageHandlerTests()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(
                    new Dictionary<string, string>
                    {
                        { "Application:TargetUrl", "http://nluk.parkmobile.nl/test/test"},
                        { "Application:XApiKey", "74f55d96-3653-4e67-9f2b-6d92a016314d"},
                        { "Logging:LogLevel:Default", "Information"},
                        { "PhonixxReceiveEndpointConfig:RelativeVerifyUrl", "/managementapi/verifystop"},
                        { "PhonixxReceiveEndpointConfig:RelativeStopUrl", "/managementapi/stopparking"},
                        { "PhonixxReceiveEndpointConfig:Suppliers:0:SupplierId", "349"},
                        { "PhonixxReceiveEndpointConfig:Suppliers:0:Url", "http://phonixx.de.test.parkmobile.nl"},
                        { "PhonixxReceiveEndpointConfig:Suppliers:1:SupplierId", "20"},
                        { "PhonixxReceiveEndpointConfig:Suppliers:1:Url", "http://nluk.parkmobile.nl"},
                        { "PhonixxReceiveEndpointConfig:XApiKey", "74f55d96-3653-4e67-9f2b-6d92a016314d"},
                        { "AutomaticStopConfig:GracePeriodSecondsFromStartTime", "60"}
                    })
                .Build();

            ILogger _logger;
            var lambdaLoggerOptions = configuration.GetSections("Application", "Logging")
                .Get<LambdaLoggerOptions>(options => options.IncludeChildren = true);
            _logger = new LambdaLoggerPropertiesContextProvider(lambdaLoggerOptions).CreateLogger("");

            var appConfig = 
                Options.Create(configuration.GetSections("Application").Get<ApplicationConfig>());

            var autoStopConfig =
                Options.Create(configuration.GetSections("AutomaticStopConfig").Get<AutomaticStopConfig>());

            _sqsMessageService = ConfigureSqsMessageService();

            _parkingService = new Mock<IParkingService>();

            _vehicleQueueMessageHandler = new VehicleQueueMessageHandler(appConfig, _logger, _sqsMessageService.Object,
                _parkingService.Object, new AutomaticStopEngine(autoStopConfig, _logger));
        }

        private Mock<ISqsMessageService> ConfigureSqsMessageService()
        {
            var sqsService = new Mock<ISqsMessageService>();
            sqsService.Setup(t => t.SendToMessageQueue(It.IsAny<VehicleEvent>()))
                .Returns(Task.FromResult(new SendMessageResponse()));

            sqsService.Setup(t => t.SendToDeadletterQueue(It.IsAny<string>()))
                .Returns(Task.FromResult(new SendMessageResponse()));

            return sqsService;
        }

        [Fact]
        public void EmptyMessageReturnsZeroRecordsProcessed()
        {
            var sqsEvent = SerializeAndWrap(new VehicleEvent());
            var response = _vehicleQueueMessageHandler.HandleSQSEvent(sqsEvent);
            Assert.Equal(0, response.Result);
        }

        [Fact]
        public void InvalidJson_MovesMessageToDeadletter()
        {
            var sqsEvent = SerializeAndWrap(new  { Prop1 = 1, Prop2 = 2} );
            var response = _vehicleQueueMessageHandler.HandleSQSEvent(sqsEvent);
            Assert.Equal(0, response.Result);
            _sqsMessageService.Verify(m => m.SendToDeadletterQueue(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void MissingCountryCode_MovesMessageToDeadletter()
        {
            var sqsEvent = SerializeAndWrap(new VehicleEvent
            {
                CountryCode = string.Empty
            });

            var response = _vehicleQueueMessageHandler.HandleSQSEvent(sqsEvent);
            _sqsMessageService.Verify(m => m.SendToDeadletterQueue(It.IsAny<string>()), Times.Once);
            Assert.Equal(0, response.Result);
        }
        [Fact]
        public void MissingVRN_ShouldNotMoveMessageToDeadletter()
        {
            var sqsEvent = SerializeAndWrap(new VehicleEvent
            {
                CountryCode = "NL",
                LicensePlate = "UNDEFINED-VRN"
            });
            _parkingService.Setup(x => x.VerifyParking(It.IsAny<VehicleEvent>(), It.IsAny<int>(), It.IsAny<Guid>())).Returns(Task.FromResult(new VerifyParkingResponse
            {
                StatusCode = "VrnNotFound"
            }));

            var response = _vehicleQueueMessageHandler.HandleSQSEvent(sqsEvent);
            _sqsMessageService.Verify(m => m.SendToDeadletterQueue(It.IsAny<string>()), Times.Never);
            Assert.Equal(1, response.Result);
        }

        [Fact]
        public void UnknownVRN_ShouldNotMoveMessageToDeadletter()
        {
            var sqsEvent = SerializeAndWrap(new VehicleEvent
            {
                CountryCode = "NL",
                LicensePlate = "UNDEFINED-VRN"
            });

            _parkingService.Setup(x => x.VerifyParking(It.IsAny<VehicleEvent>(), It.IsAny<int>(), It.IsAny<Guid>()))
                .Throws<UserNotFoundException>();

            var response = _vehicleQueueMessageHandler.HandleSQSEvent(sqsEvent);
            _sqsMessageService.Verify(m => m.SendToDeadletterQueue(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void WhenVerify_IsTimingOut_MessageIsRequeued()
        {
            _parkingService.Setup(x => x.VerifyParking(It.IsAny<VehicleEvent>(), It.IsAny<int>(), It.IsAny<Guid>()))
                .Throws<TimeoutException>();

            var sqsEvent = SerializeAndWrap(new VehicleEvent
            {
                CountryCode = "NL",
                LicensePlate = "RRTTEE"
            });

            var response = _vehicleQueueMessageHandler.HandleSQSEvent(sqsEvent);
            _sqsMessageService.Verify(m => m.SendToMessageQueue(It.IsAny<VehicleEvent>()), Times.Once);
            Assert.Equal(0, response.Result);
        }

        [Fact]
        public void WhenVerifyParking_ReturnsNoParkingActions_StopParkingIsNotCalled()
        {
            _parkingService.Setup(x => x.VerifyParking(It.IsAny<VehicleEvent>(), It.IsAny<int>(), It.IsAny<Guid>()))
                .Returns(Task.FromResult(new VerifyParkingResponse()));

            var sqsEvent = SerializeAndWrap(new VehicleEvent
            {
                CountryCode = "NL",
                LicensePlate = "RRTTEE"
            });

            var response = _vehicleQueueMessageHandler.HandleSQSEvent(sqsEvent);
            _parkingService.Verify(
                m => m.StopParking(It.IsAny<VehicleEvent>(), It.IsAny<VerifyParkingResponse>(),
                    It.IsAny<List<ParkingAction>>(), It.IsAny<int>(), It.IsAny<Guid>()), Times.Never);

            Assert.Equal(0, response.Result);
        }


        [Fact]
        public void WhenVerifyParking_ReturnsActiveParkingAction_StopParkingIsCalled()
        {
            _parkingService.Setup(x => x.VerifyParking(It.IsAny<VehicleEvent>(), It.IsAny<int>(), It.IsAny<Guid>()))
                .Returns(Task.FromResult(new VerifyParkingResponse
                {
                    StatusCode = "OK",
                    ClientId = 1,
                    ActiveSessions = new List<ParkingAction>
                    {
                        new ParkingAction
                        {
                            StartTimeUtc = DateTime.UtcNow.AddMinutes(-20)
                        }
                    }
                }));

            var sqsEvent = SerializeAndWrap(new VehicleEvent
            {
                CountryCode = "NL",
                LicensePlate = "RRTTEE"
            });

            var response = _vehicleQueueMessageHandler.HandleSQSEvent(sqsEvent);

            _parkingService.Verify(
                m => m.StopParking(It.IsAny<VehicleEvent>(), It.IsAny<VerifyParkingResponse>(),
                    It.IsAny<List<ParkingAction>>(), It.IsAny<int>(), It.IsAny<Guid>()), Times.Once);

            Assert.Equal(1, response.Result);
        }

        [Fact]
        public void WhenParkingActionsFound_AndWithinGracePeriod_StopParkingIsNotCalled()
        {
            _parkingService.Setup(x => x.VerifyParking(It.IsAny<VehicleEvent>(), It.IsAny<int>(), It.IsAny<Guid>()))
                .Returns(Task.FromResult(new VerifyParkingResponse
                {
                    StatusCode = "OK",
                    ClientId = 1,
                    ActiveSessions = new List<ParkingAction>
                    {
                        new ParkingAction
                        {
                            StartTimeUtc = DateTime.UtcNow.AddSeconds(-10)
                        }
                    }
                }));

            var sqsEvent = SerializeAndWrap(new VehicleEvent
            {
                CountryCode = "NL",
                LicensePlate = "RRTTEE"
            });

            var response = _vehicleQueueMessageHandler.HandleSQSEvent(sqsEvent);
            _parkingService.Verify(
                m => m.StopParking(It.IsAny<VehicleEvent>(), It.IsAny<VerifyParkingResponse>(),
                    It.IsAny<List<ParkingAction>>(), It.IsAny<int>(), It.IsAny<Guid>()), Times.Never);

            Assert.Equal(1, response.Result);
        }


        private static SQSEvent SerializeAndWrap(object message)
        {
            var body = JsonConvert.SerializeObject(message);

            return new SQSEvent
            {
                Records = new List<SQSEvent.SQSMessage>
                {
                    new SQSEvent.SQSMessage {Body = body}
                }
            };

        }


    }
}
