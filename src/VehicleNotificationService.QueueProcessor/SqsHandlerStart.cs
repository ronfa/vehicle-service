using Microsoft.Extensions.Logging;
using Overleaf.Lambda;
using VehicleNotificationService.Business;
using VehicleNotificationService.Business.Model;
using VehicleNotificationService.Business.Wrappers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VehicleNotificationService.Business.Engines;
using VehicleNotificationService.Business.Model.Configuration;
using VehicleNotificationService.Business.Services;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
namespace VehicleNotificationService.QueueProcessor
{
    partial class SqsHandler : LambdaBase
    {
        public SqsHandler()
        {
            Init();
            _vehicleQueueMessageHandler = serviceProvider.GetService<IVehicleQueueMessageHandler>();
            _backChannelMessageManager = serviceProvider.GetService<IBackChannelMessageManager>();
        }

        protected override ServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ILogger>(logger);
            services.AddSingleton<IHttpMessageSender, HttpMessageSender>();

            services.AddTransient<IVehicleQueueMessageHandler, VehicleQueueMessageHandler>();
            services.AddTransient<ISqsMessageService, SqsMessageService>();
            services.AddTransient<IParkingService, ParkingService>();
            services.AddTransient<IPhonixxMessageSender, PhonixxMessageSender>();
            services.AddTransient<IParkingEngine, AutomaticStopEngine>();
            services.AddTransient<IBackChannelMessageManager, BackChannelMessageManager>();
            services.AddTransient<IEventHubSenderClient, EventHubSenderClient>();


            services.Configure<ApplicationConfig>(options => configuration.GetSection("Application").Bind(options));

            services.Configure<PhonixxReceiveEndpointConfig>(options =>
                configuration.GetSection("PhonixxReceiveEndpointConfig").Bind(options));
            services.Configure<EventHubConfig>(options => configuration.GetSection("EventHubConfig").Bind(options));

            return services.BuildServiceProvider();
        }
    }
}