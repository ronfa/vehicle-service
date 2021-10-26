using System.Collections.Generic;

namespace VehicleNotificationService.Business.Model.Configuration
{
    public class AuthorizationConfig
    {
        public ClientConfig[] Clients { get; set; }
    }

    public class ClientConfig
    {
        public string ClientName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string ApiKey { get; set; }

    }
}
