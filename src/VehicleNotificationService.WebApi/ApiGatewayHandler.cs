using System.Net;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Microsoft.Extensions.Logging;
using Overleaf.Lambda;
using VehicleNotificationService.Business;
using VehicleNotificationService.Business.Extensions;
using VehicleNotificationService.Business.Services;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
namespace VehicleNotificationService.WebApi
{
    public partial class ApiGatewayHandler : ApiGatewayLambdaBase
    {
        private IVehicleMessageEndpointHandler _vehicleMessageEndpointHandler;
        private IAuthorizationService _authService;

        public async Task<APIGatewayProxyResponse> PostHandler(APIGatewayProxyRequest apigProxyEvent, ILambdaContext context)
        {
            var authResponse = _authService.Authorize(apigProxyEvent.Headers);

            if (authResponse == null || !authResponse.Authorized)
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = HttpStatusCode.Unauthorized.AsInt()
                };
            }

            logger.LogInformation("Request Body :" + apigProxyEvent.Body);

            var statusCode = await _vehicleMessageEndpointHandler.QueueVehicleNotification(apigProxyEvent.Body);

            return new APIGatewayProxyResponse
            {
                StatusCode = statusCode.AsInt(),
            };
        }
    }
}
