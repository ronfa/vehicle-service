using Microsoft.Extensions.Hosting;
using Overleaf.Logging.Model;
using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Overleaf.Configuration;
using Overleaf.Logging.Model;
using Overleaf.Logging.Validation;
using Overleaf.Logging;

namespace VehicleNotificationService.EventHubWorker
{
    public static class HostingExtensions
    {
        public static IHostBuilder UseStructuredLogging(this IHostBuilder hostBuilder)
        {
            return hostBuilder
                    .ConfigureServices((context, services) =>
                    {
                        // Expose ApplicationSettings in case it is needed elsewhere
                        services.AddOptions();
                        services.Configure<LoggingSettings>(context.Configuration.GetSection(LoggingSettings.SectionKey));
                        services.AddSingleton(s => s.GetRequiredService<IOptions<LoggingSettings>>().Value);
                        services.AddSingleton<ILoggerFactory, SerilogLoggerFactory>();
                        services.AddSingleton(x => x.GetRequiredService<ILoggerFactory>().CreateLogger(SourceContext.Functional));
                        services.AddSingleton<IValidatable>(s => s.GetRequiredService<LoggingSettings>());
                        services.AddSingleton<IValidator, LoggingSettingsValidator>();
                    });
        }

        public static IHostBuilder UseApplicationSettings(this IHostBuilder hostBuilder)
        {
            return hostBuilder
                    .ConfigureServices((context, services) =>
                    {
                        // Expose ApplicationSettings in case it is needed elsewhere
                        services.AddOptions();
                        services.Configure<ApplicationSettings>(context.Configuration.GetSection(ApplicationSettings.SectionKey));
                        services.AddSingleton(s => s.GetRequiredService<IOptions<ApplicationSettings>>().Value);

                        services.AddSingleton<IValidatable>(s => s.GetRequiredService<ApplicationSettings>());
                        services.AddSingleton<IValidator, ApplicationSettingsValidator>();
                    })
                ;
        }
    }
}
