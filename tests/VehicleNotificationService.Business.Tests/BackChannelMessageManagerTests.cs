using Amazon.SQS.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using Overleaf.Configuration;
using Overleaf.Logging.Lambda;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VehicleNotificationService.Business.Model;
using VehicleNotificationService.Business.Model.Configuration;
using VehicleNotificationService.Business.Model.Phonixx;
using VehicleNotificationService.Business.Services;
using Xunit;
using LambdaLoggerOptions = Overleaf.Logging.Lambda.LambdaLoggerOptions;

namespace VehicleNotificationService.Business.Tests
{
    public class BackChannelMessageManagerTests
    {
        readonly BackChannelMessageManager _backChannelMessageManager = null;
        private readonly Mock<IEventHubSenderClient> _eventHubSenderClient = null;
        private readonly Mock<ISqsMessageService> _sqsMessageService = null;

        public BackChannelMessageManagerTests()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(
                    new Dictionary<string, string>
                    {                        
                        { "Logging:LogLevel:Default", "Information"},
                        { "EventHubConfig:ConnectionString", "Endpoint=sb://parking.servicebus.windows.net/;SharedAccessKeyName=parkNow-admin;SharedAccessKey=f4wIxLaVqifPIpvXhtx8KZKMF+G7bhraT6qjDE5zVGQ=;EntityPath=park-now-backchannel-test"},
                        { "EventHubConfig:EventHubName", "park-now-backchannel-test"},
                        { "EventHubConfig:SharedAccessKey", "f4wIxLaVqifPIpvXhtx8KZKMF+G7bhraT6qjDE5zVGQ="}
                    })
                .Build();

            ILogger _logger;
            var lambdaLoggerOptions = configuration.GetSections("Application", "Logging")
                .Get<LambdaLoggerOptions>(options => options.IncludeChildren = true);
            _logger = new LambdaLoggerPropertiesContextProvider(lambdaLoggerOptions).CreateLogger("");

            var eventhubConfig =
                Options.Create(configuration.GetSections("EventHubConfig").Get<EventHubConfig>());

            _eventHubSenderClient = ConfigureEventHubSenderClient();
            _sqsMessageService = ConfigureSqsMessageService();

            _backChannelMessageManager = new BackChannelMessageManager(_logger, _eventHubSenderClient.Object);
        }

        private Mock<IEventHubSenderClient> ConfigureEventHubSenderClient()
        {
            var eventhubSender = new Mock<IEventHubSenderClient>();
            eventhubSender.Setup(t => t.SendAsync(It.IsAny<List<string>>()));

            return eventhubSender;
        }

        private Mock<ISqsMessageService> ConfigureSqsMessageService()
        {
            var sqsService = new Mock<ISqsMessageService>();           

            sqsService.Setup(t => t.SendToDeadletterQueue(It.IsAny<string>()))
                .Returns(Task.FromResult(new SendMessageResponse()));

            return sqsService;
        }

        [Fact]
        public void CorrectMessageFormat_ProcessesMessage()
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

            List<string> request = new List<string>();
            request.Add(backChannelEvent);

            var response = _backChannelMessageManager.SendBackChannelEventAsync(request);
            _eventHubSenderClient.Verify(m => m.SendAsync(It.IsAny<List<string>>()), Times.Once);
        }
    }
}
