using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using Overleaf.Configuration;
using Overleaf.Logging.Lambda;
using VehicleNotificationService.Business.Model;
using VehicleNotificationService.Business.Model.Configuration;
using VehicleNotificationService.Business.Services;
using VehicleNotificationService.Business.Wrappers;
using Xunit;
using LambdaLoggerOptions = Overleaf.Logging.Lambda.LambdaLoggerOptions;

namespace VehicleNotificationService.Business.Tests
{
    public class PhonixxMessageSenderTests
    {
        PhonixxMessageSender _messageHandler = null;

        public PhonixxMessageSenderTests()
        {
            PhonixxReceiveEndpointConfig conf = new PhonixxReceiveEndpointConfig()
            {
                RelativeVerifyUrl = "/managementapi/verify",
                RelativeStopUrl = "/managementapi/stop",
                XApiKey = "74f55d96-3653-4e67-9f2b-6d92a016314d",
                Suppliers = new List<Supplier>()
                {
                    new Supplier
                    {
                        SupplierId = 349,
                        Url ="http://parknow.test.parkmobile.nl"
                    },
                    new Supplier
                    {
                        SupplierId = 20,
                        Url ="http://phonixx.test.parkmobile.nl"
                    }

                }
            };

            var config = new ConfigurationBuilder().AddEnvironmentVariables().Build();
            var lambdaLoggerOptions = config.GetSections("Application", "Logging")
              .Get<LambdaLoggerOptions>(options => options.IncludeChildren = true);

            Mock<IOptions<PhonixxReceiveEndpointConfig>> configuredSection = new Mock<IOptions<PhonixxReceiveEndpointConfig>>();
            configuredSection.Setup(t=>t.Value).Returns(conf);

            var messageSender = new Mock<IHttpMessageSender>();
            messageSender.Setup(t => t.SendMessage<VerifyParkingResponse>(It.IsAny<Uri>(), It.IsAny<object>(),
                    It.IsAny<KeyValuePair<string, string>>()))
                .Returns(Task.FromResult(new VerifyParkingResponse { StatusCode = "OK" }));

            messageSender.Setup(t => t.SendMessage<StopParkingResponse>(It.IsAny<Uri>(), It.IsAny<object>(),
                    It.IsAny<KeyValuePair<string, string>>()))
                .Returns(Task.FromResult(new StopParkingResponse { StatusCode = HttpStatusCode.OK }));

            var logger = new LambdaLoggerPropertiesContextProvider(lambdaLoggerOptions).CreateLogger("");
            _messageHandler = new PhonixxMessageSender(configuredSection.Object, messageSender.Object, logger);

        }


        [Fact]
        public void VerifyParking()
        {
            var message = new VehicleEvent {CountryCode = "NL"};
            var tripId = Guid.NewGuid();
            var result = _messageHandler.VerifyStop(message, 20, tripId);
            Assert.NotNull(result);
            Assert.True(result.Result.StatusCode == "OK");
        }

        [Fact]
        public void StopParking()
        {
            var message = new VehicleEvent { CountryCode = "NL" };
            var tripId = Guid.NewGuid();
            var result = _messageHandler.StopParking(message, 20, tripId, new ParkingAction());
            Assert.NotNull(result);
            Assert.True(result.Result.StatusCode == HttpStatusCode.OK);
        }


        [Fact]
        public void HandleSqsMessageForNotExistingSuplierConfigurationTest()
        {
            var message = new VehicleEvent { CountryCode = "RU" };
            var tripId = Guid.NewGuid();
           
            var exception = Assert.ThrowsAsync<ConfigurationErrorsException>(
                () => _messageHandler.VerifyStop(message, 17, tripId));
        }
    }
}
