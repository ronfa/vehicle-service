using System.Collections.Generic;
using System.Net;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NewRelic.OpenTracing.AmazonLambda;
using OpenTracing.Util;
using Overleaf.Lambda;
using VehicleNotificationService.Business;
using VehicleNotificationService.Business.Engines;
using VehicleNotificationService.Business.Model.Configuration;
using VehicleNotificationService.Business.Services;
using VehicleNotificationService.Business.Wrappers;

namespace VehicleNotificationService.WebApi
{
    public partial class ApiGatewayHandler : ApiGatewayLambdaBase
    {
        public ApiGatewayHandler()
        {
            Init ();
        }

        #region StartUp
        public void Init()
        {
            base.Init();
            _vehicleMessageEndpointHandler = serviceProvider.GetService<IVehicleMessageEndpointHandler>();
            _authService = serviceProvider.GetService<IAuthorizationService>();
        }

        protected override ServiceProvider ConfigureServices(IServiceCollection services)
        {
            GlobalTracer.Register(LambdaTracer.Instance);
            AWSSDKHandler.RegisterXRayForAllServices(); // All AWS SDK requests will be traced

            // Alphabetic order by implementation class name.
            services.AddTransient<IVehicleMessageEndpointHandler, VehicleMessageEndpointHandler>();
            services.AddTransient<IAuthorizationService, AuthorizationService>();
            services.AddTransient<ISqsMessageSender, SqsMessageSender>();
            services.AddTransient<IParkingEngine, AutomaticStopEngine>();
            services.AddTransient<ISqsMessageService, SqsMessageService>();

            services.AddSingleton(logger);
            services.AddSingleton(configuration);
            services.AddSingleton(GlobalTracer.Instance);

            services.Configure<ApplicationConfig>(options => configuration.GetSection("Application").Bind(options));
            services.Configure<AuthorizationConfig>(options => configuration.GetSection("Authorization").Bind(options));

            return services.BuildServiceProvider();
        }

        public bool ValidateRequest(APIGatewayProxyRequest request, out APIGatewayProxyResponse response)
        {
            response = null;
            var valid = true;

            if (request == null || string.IsNullOrWhiteSpace(request.Body))
            {
                logger.LogError("Request is null");
                response = CreateResponse(HttpStatusCode.BadRequest);
                valid = false;
            }

            return valid;
        }

        public APIGatewayProxyResponse CreateResponse(HttpStatusCode statusCode, string body = null)
        {
            return new APIGatewayProxyResponse
            {
                Body = body,
                StatusCode = (int)statusCode,
                Headers = new Dictionary<string, string>
                {
                    {"Content-Type", "application/json"},
                }
            };
        }


        #endregion
    }
}