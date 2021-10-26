using System;
using System.Collections.Generic;
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
    public class AuthorizationServiceTests
    {

        IAuthorizationService _authorizationService = null;

        public AuthorizationServiceTests()
        {
            var conf = new AuthorizationConfig()
            {
                Clients = new ClientConfig[]
                {
                    new ClientConfig
                    {
                        ClientName= "Webfleet",
                        Username = "webfleet",
                        Password = "webfleetpass",
                        ApiKey = "webfleetapikey"
                    },
                    new ClientConfig
                    {
                        ClientName= "Fleetcomplete",
                        Username = "fleetcomplete",
                        Password = "fleetcompletepass",
                        ApiKey = "fleetcompleteapikey"
                    },

                }
            };

            var config = new ConfigurationBuilder().AddEnvironmentVariables().Build();
            var lambdaLoggerOptions = config.GetSections("Application", "Logging")
              .Get<LambdaLoggerOptions>(options => options.IncludeChildren = true);

            Mock<IOptions<AuthorizationConfig>> configuredSection = new Mock<IOptions<AuthorizationConfig>>();
            configuredSection.Setup(t=>t.Value).Returns(conf);

            var messageSender = new Mock<IHttpMessageSender>();
            messageSender.Setup(t => t.SendMessage<VerifyParkingResponse>(It.IsAny<Uri>(), It.IsAny<object>(),
                    It.IsAny<KeyValuePair<string, string>>()))
                .Returns(Task.FromResult(new VerifyParkingResponse { StatusCode = "OK"}));

            messageSender.Setup(t => t.SendMessage<StopParkingResponse>(It.IsAny<Uri>(), It.IsAny<object>(),
                    It.IsAny<KeyValuePair<string, string>>()))
                .Returns(Task.FromResult(new StopParkingResponse { StatusCode = HttpStatusCode.OK }));

            var logger = new LambdaLoggerPropertiesContextProvider(lambdaLoggerOptions).CreateLogger("");

            _authorizationService = new AuthorizationService(configuredSection.Object, logger);
        }


        [Fact]
        public void EmptyHeadersFailsAuth()
        {
            var headers = new Dictionary<string, string>();
            var result = _authorizationService.Authorize(headers);
            Assert.NotNull(result);
            Assert.False(result.Authorized);
        }

        [Fact]
        public void IncorrectApiKeyFailsAuth()
        {
            var headers = new Dictionary<string, string>
            {
                {"x-api-key","blabla"}
            };
            var result = _authorizationService.Authorize(headers);
            Assert.NotNull(result);
            Assert.False(result.Authorized);
        }

        [Fact]
        public void CorrectApiKeyIsAuthorized()
        {
            var headers = new Dictionary<string, string>
            {
                {"x-api-key","fleetcompleteapikey"}
            };
            var result = _authorizationService.Authorize(headers);
            Assert.NotNull(result);
            Assert.True(result.Authorized);
            Assert.Equal("Fleetcomplete", result.SourceApplication);
        }

        [Fact]
        public void IncorrectBasicAuthFailsAuth()
        {
            string username = "user", password = "pass";
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes($"{username}:{password}");
            var encoded = System.Convert.ToBase64String(plainTextBytes);
            var basicAuth = $"Basic {encoded}";
            var headers = new Dictionary<string, string>
            {
                {"Authorization", basicAuth}
            };
            var result = _authorizationService.Authorize(headers);
            Assert.NotNull(result);
            Assert.False(result.Authorized);
        }

        [Fact]
        public void CorrectBasicAuthIsAuthorized()
        {
            string username = "webfleet", password = "webfleetpass";
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes($"{username}:{password}");
            var encoded = System.Convert.ToBase64String(plainTextBytes);
            var basicAuth = $"Basic {encoded}";
            var headers = new Dictionary<string, string>
            {
                {"Authorization", basicAuth}
            };
            var result = _authorizationService.Authorize(headers);
            Assert.NotNull(result);
            Assert.True(result.Authorized);
            Assert.Equal("Webfleet", result.SourceApplication);

        }



    }
}
