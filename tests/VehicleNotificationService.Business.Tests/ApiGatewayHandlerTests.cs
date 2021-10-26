using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Amazon.SQS.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Overleaf.Configuration;
using Overleaf.Logging.Lambda;
using VehicleNotificationService.Business.Model;
using VehicleNotificationService.Business.Services;
using Xunit;
using LambdaLoggerOptions = Overleaf.Logging.Lambda.LambdaLoggerOptions;

namespace VehicleNotificationService.Business.Tests
{
    public class ApiGatewayHandlerTests
    {
        readonly VehicleMessageEndpointHandler _vehicleMessageEndpointHandler = null;

        public ApiGatewayHandlerTests()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(
                    new Dictionary<string, string>
                    {
                        { "Application:TargetUrl", "http://nluk.parkmobile.nl/test/test"},
                        { "Application:XApiKey", "74f55d96-3653-4e67-9f2b-6d92a016314d"},
                        { "Logging:LogLevel:Default", "Information"}
                    })
                .Build();

            ILogger _logger;
            var lambdaLoggerOptions = configuration.GetSections("Application", "Logging")
                .Get<LambdaLoggerOptions>(options => options.IncludeChildren = true);
            _logger = new LambdaLoggerPropertiesContextProvider(lambdaLoggerOptions).CreateLogger("");

            var sqsService = ConfigureSqsService();

            _vehicleMessageEndpointHandler = new VehicleMessageEndpointHandler(_logger, sqsService.Object);
        }


        private Mock<ISqsMessageService> ConfigureSqsService()
        {
            var response = new SendMessageResponse
            {
                HttpStatusCode = HttpStatusCode.OK
            };

            var sqsServiceMock = new Mock<ISqsMessageService>();

            sqsServiceMock.Setup(t => t.SendToMessageQueue(It.IsAny<VehicleEvent>()))
                .Returns(Task.FromResult(response));

            return sqsServiceMock;
        }

        [Fact]
        public void GetHandlerTest()
        {
            var request = new VehicleEvent();
            var response =
                _vehicleMessageEndpointHandler.QueueVehicleNotification(JsonConvert.SerializeObject(request));
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.Result);
        }
    }
}
