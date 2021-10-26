using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VehicleNotificationService.Business.Model;
using VehicleNotificationService.Business.Model.Configuration;

namespace VehicleNotificationService.Business.Services
{
    public class AuthorizationService : IAuthorizationService
    {

        private readonly AuthorizationConfig _authConfig;
        public AuthorizationService(IOptions<AuthorizationConfig> auth, ILogger logger)
        {
            _authConfig = auth.Value;
        }

        public AuthResponse Authorize(IDictionary<string, string> headers)
        {
            if (headers == null || _authConfig == null || _authConfig.Clients == null)
            {
                return new AuthResponse { Authorized = false }; ;
            }

            // Try api key auth first
            if (headers.ContainsKey("x-api-key"))
            {
                var apiKey = headers["x-api-key"];

                var match = _authConfig.Clients.FirstOrDefault(x => x.ApiKey == apiKey);

                if (match != null)
                {
                    return new AuthResponse
                    {
                        Authorized = true,
                        SourceApplication = match.ClientName
                    };
                }
            }

            // Try basic auth
            if (headers.ContainsKey("Authorization"))
            {

                var authArray = Encoding.UTF8.GetString(Convert.FromBase64String(headers["Authorization"].Split(" ")[1]));
                var userPassArray = authArray.Split(":");
                if (userPassArray.Length != 2)
                {
                    return new AuthResponse { Authorized = false };
                }

                var userName = userPassArray[0];
                var password = userPassArray[1];

                var basicMatch =
                    _authConfig.Clients.FirstOrDefault(x => userName.Equals(x.Username) && password.Equals(x.Password));

                if (basicMatch != null) {
                    return new AuthResponse
                    {
                        Authorized = true,
                        SourceApplication = basicMatch.ClientName
                    };
                }
            }

            return new AuthResponse { Authorized = false };
        }
    }
}
