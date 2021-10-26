using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Amazon.SQS.Model;
using VehicleNotificationService.Business;
using VehicleNotificationService.Business.Model;
using VehicleNotificationService.Business.Wrappers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;
using Moq;
using VehicleNotificationService.Business.Engines;
using VehicleNotificationService.Business.Model.Configuration;
using VehicleNotificationService.Business.Model.Phonixx;
using VehicleNotificationService.Business.Services;

namespace VehicleNotificationService.QueueProcessor.Tests
{
    public class SqsHandlerTestOverride : SqsHandler
    {
        protected override void SetupLogger()
        {
            logger = new DebugLogger("test");
        }

        public static MemoryStream GenerateStreamFromString(string value)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(value ?? ""));
        }

        protected override void SetupConfiguration(Overleaf.Configuration.ApplicationSettings initialSettings, string configurationBasePath = "", bool useJsonFileConfiguration = false)
        {
            configuration = new ConfigurationBuilder()
                 .AddInMemoryCollection(
                     new Dictionary<string, string>
                     {
                        { "Application:TargetUrl", "http://nluk.parkmobile.nl/test/test"},
                        { "Application:XApiKey", "74f55d96-3653-4e67-9f2b-6d92a016314d"},
                        { "Application:Environment", @"dev" },
                        { "Application:LogLevel", "Information" },
                        { "PhonixxReceiveEndpointConfig:RelativeVerifyUrl", "/managementapi/verify"},
                        { "PhonixxReceiveEndpointConfig:RelativeStopUrl", "/managementapi/stop"},
                        { "PhonixxReceiveEndpointConfig:Suppliers:0:SupplierId", "349"},
                        { "PhonixxReceiveEndpointConfig:Suppliers:0:Url", "http://phonixx.de.test.parkmobile.nl"},
                        { "PhonixxReceiveEndpointConfig:Suppliers:1:SupplierId", "20"},
                        { "PhonixxReceiveEndpointConfig:Suppliers:1:Url", "http://nluk.parkmobile.nl"},
                        { "PhonixxReceiveEndpointConfig:XApiKey", "74f55d96-3653-4e67-9f2b-6d92a016314d"},
                        { "EventHubConfig:ConnectionString", "Endpoint=sb://parking.servicebus.windows.net/;SharedAccessKeyName=parkNow-admin;SharedAccessKey={0};EntityPath=park-now-backchannel-test"},
                        { "PhonixxReceiveEndpointConfig:EventHubName", "park-now-backchannel-test"},
                        { "PhonixxReceiveEndpointConfig:SharedAccessKey", "f4wIxLaVqifPIpvXhtx8KZKMF+G7bhraT6qjDE5zVGQ="}
                     })
                 .Build();
        }

        protected override ServiceProvider ConfigureServices(IServiceCollection services)
        {
            var messageSender = new Mock<IHttpMessageSender>();
            messageSender
                .Setup(t => t.SendMessage<VerifyParkingResponse>(It.IsAny<Uri>(),
                    It.IsAny<object>(), It.IsAny<KeyValuePair<string, string>>()))
                .Returns(Task.FromResult(new VerifyParkingResponse { StatusCode = "OK"}));

            messageSender
                .Setup(t => t.SendMessage<StopParkingResponse>(It.IsAny<Uri>(),
                    It.IsAny<object>(), It.IsAny<KeyValuePair<string, string>>()))
                .Returns(Task.FromResult(new StopParkingResponse { StatusCode = HttpStatusCode.OK }));

            var sqsServiceMock = new Mock<ISqsMessageService>();
            sqsServiceMock.Setup(x => x.SendToMessageQueue(It.IsAny<VehicleEvent>()))
                .Returns(Task.FromResult(new SendMessageResponse()));
            sqsServiceMock.Setup(x => x.SendToDeadletterQueue(It.IsAny<string>()))
                .Returns(Task.FromResult(new SendMessageResponse()));

            var eventHubSenderMock = new Mock<IEventHubSenderClient>();
            eventHubSenderMock.Setup(x => x.SendAsync(It.IsAny<List<string>>()));

            services.AddSingleton<ILogger>(logger);
            services.AddSingleton<IHttpMessageSender>(messageSender.Object);
            services.AddSingleton<ISqsMessageService>(sqsServiceMock.Object);
            services.AddSingleton<IEventHubSenderClient>(eventHubSenderMock.Object);

            services.AddTransient<IVehicleQueueMessageHandler, VehicleQueueMessageHandler>();
            services.AddTransient<IBackChannelMessageManager, BackChannelMessageManager>();
            services.Configure<ApplicationConfig>(options => configuration.GetSection("Application").Bind(options));

//            services.AddTransient<ISqsMessageService, SqsMessageService>();
            services.AddTransient<IParkingService, ParkingService>();
            services.AddTransient<IPhonixxMessageSender, PhonixxMessageSender>();

            services.AddTransient<IParkingEngine, AutomaticStopEngine>();

            services.Configure<PhonixxReceiveEndpointConfig>(options => configuration.GetSection("PhonixxReceiveEndpointConfig").Bind(options));
            services.Configure<EventHubConfig>(options => configuration.GetSection("EventHubConfig").Bind(options));

            return services.BuildServiceProvider();
        }
    }
}
