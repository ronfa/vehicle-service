using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting; 
using Microsoft.Extensions.Logging;
using Overleaf.Logging;
using Overleaf.Configuration;
using System;
using VehicleNotificationService.EventHubWorker.Model;
using VehicleNotificationService.Business;
using System.Configuration;
using VehicleNotificationService.Business.Services;
using Environment = System.Environment;
using System.Reflection;

namespace VehicleNotificationService.EventHubWorker
{
    public class Program
    {
        private static IConfigurationRoot configuration;

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                 .UseWindowsService()
                 .ConfigureAppConfiguration(c =>
                 {
                     c.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                     var basicConfiguration = c.Build();
                     c.SetBasePath(basicConfiguration.GetSection("Settings:BaseConfigPath").Value);
                     c.AddSecureJson($"appsettings.{basicConfiguration.GetSection("Settings:EnvironmentName").Value}.json", false);
                     c.AddEnvironmentVariables();
                     configuration = c.Build(); 
                 }) 
                .UseStructuredLogging()
                .UseApplicationSettings()
                .ConfigureServices((hostContext, services) =>
                {
                    SetEventHubSharedAccessKey(configuration);
                    SetQueueConfig(configuration);
                    services.AddHostedService<EventHubHostedService>();
                    services.Configure<EventHubConfig>(configuration.GetSection(nameof(EventHubConfig)));
                    services.AddSingleton<IVehicleMessageEndpointHandler, VehicleMessageEndpointHandler>();
                    services.AddSingleton<ISqsMessageService, SqsMessageService>();
                    services.AddSingleton<IEventHubConsumer, EventHubConsumer>();
                    services.AddSingleton<IEventHubConsumerOptions>(new EventHubConsumerOptions
                    {
                        EventHubConnectionString = configuration.GetValue<string>("EventHubConfig:EventHubConnectionString"),
                        EventHubName = configuration.GetValue<string>("EventHubConfig:EventHubName"),
                        FullyQualifiedNamespace = configuration.GetValue<string>("EventHubConfig:FullyQualifiedNamespace"),
                        OffsetPeriodInMinutes = configuration.GetValue<int>("EventHubConfig:OffsetPeriodInMinutes"),
                        MaximumRetries = configuration.GetValue<int>("EventHubConfig:MaximumRetries"),
                        DelayInMilliSeconds = configuration.GetValue<int>("EventHubConfig:DelayInMilliSeconds"),
                        MaximumDelayInSeconds = configuration.GetValue<int>("EventHubConfig:MaximumDelayInSeconds")
                    });
                    //services.AddApplication();                   

                    services.AddLogging(configure => configure.AddConsole());
                });

        private static void SetEventHubSharedAccessKey(IConfiguration settings)
        {
            var eventHubConfig = settings.GetSection("EventHubConfig").Get<EventHubConfig>();
            var sharedAccessKey = settings.GetValue<string>("EventHubConfig:SharedAccessKey");
            if (!string.IsNullOrWhiteSpace(sharedAccessKey))
            {
                settings["EventHubConfig:EventHubConnectionString"] = string.Format(eventHubConfig.EventHubConnectionString, sharedAccessKey);
            }
        }

        private static void SetQueueConfig(IConfiguration settings)
        {
            var targetQueue = settings.GetValue<string>("QueueConfig:TargetQueueUrl");
            var deadletterQueue = settings.GetValue<string>("QueueConfig:DeadletterQueueUrl");
            Environment.SetEnvironmentVariable("TARGET_QUEUE_URL", targetQueue);
            Environment.SetEnvironmentVariable("DEADLETTER_QUEUE_URL", deadletterQueue);
        }
    }
}
