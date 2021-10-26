using System.Collections.Generic;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using VehicleNotificationService.Business.Model;
using Xunit;

namespace VehicleNotificationService.WebApi.Tests
{
    public class ApiGatewayHandlerTests
    {
        public ApiGatewayHandlerTests()
        {
        }

        [Fact]
        public void HandlerReturnsUnauthorized()
        {
            var request = new APIGatewayProxyRequest
            {
                Body = JsonConvert.SerializeObject(new VehicleEvent())
            };
            var context = new TestLambdaContext();
            var function = new ApiGatewayHandlerTestOverride();
            var response = function.PostHandler(request, context);
            Assert.NotNull(response);
            Assert.Equal(StatusCodes.Status401Unauthorized, response.Result.StatusCode);
        }


        [Fact]
        public void HandlerReturnsOK()
        {
            var request = new APIGatewayProxyRequest
            {
                Headers = new Dictionary<string, string> { { "x-api-key", "testapikey" } },
                Body = JsonConvert.SerializeObject(new VehicleEvent())
            };
            var context = new TestLambdaContext();
            var function = new ApiGatewayHandlerTestOverride();
            var response = function.PostHandler(request, context);
            Assert.NotNull(response);
            Assert.Equal(StatusCodes.Status200OK, response.Result.StatusCode);
        }
    }
}
