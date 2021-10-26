using Amazon.SQS.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;
using Moq;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using VehicleNotificationService.Business;
using VehicleNotificationService.Business.Model;
using VehicleNotificationService.Business.Model.Configuration;
using VehicleNotificationService.Business.Services;
using VehicleNotificationService.Business.Wrappers;
using VehicleNotificationService.WebApi;

namespace VehicleNotificationService.WebApi.Tests
{
    public class ApiGatewayHandlerTestOverride : ApiGatewayHandler
    {
        protected override void SetupLogger()
        {
            logger = new DebugLogger("test");
        }

        protected override void SetupConfiguration(Overleaf.Configuration.ApplicationSettings initialSettings,
            string configurationBasePath = "", bool useJsonFileConfiguration = false)
        {
            configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(
                    new Dictionary<string, string>
                    {
                        {"Application:TargetUrl", "http://nluk.parkmobile.nl/test/test"},
                        {"Application:XApiKey", "74f55d96-3653-4e67-9f2b-6d92a016314d"},
                        {"Authorization:Clients:[0]:ApiKey", "testapikey"},
                        {"Authorization:Clients:[0]:ClientName", "Webfleet"}
                    })
                .Build();
        }

        protected override ServiceProvider ConfigureServices(IServiceCollection services)
        {
            var responseMock = new SendMessageResponse
            {
                HttpStatusCode = HttpStatusCode.OK
            };

            var sqsServiceMock = new Mock<ISqsMessageService>();
            sqsServiceMock.Setup(t => t.SendToMessageQueue(It.IsAny<VehicleEvent>()))
                .Returns(Task.FromResult(responseMock));


            services.AddSingleton<ILogger>(logger);
            services.AddSingleton<ISqsMessageService>(sqsServiceMock.Object);

            services.Configure<ApplicationConfig>(options => configuration.GetSection("Application").Bind(options));
            services.Configure<AuthorizationConfig>(options => configuration.GetSection("Authorization").Bind(options));

            services.AddTransient<IVehicleMessageEndpointHandler, VehicleMessageEndpointHandler>();
            services.AddTransient<IAuthorizationService, AuthorizationService>();

            return services.BuildServiceProvider();
        }
    }
}
