using System.Collections.Generic;
using VehicleNotificationService.Business.Model;

namespace VehicleNotificationService.Business.Services
{
    public interface IAuthorizationService
    {
        AuthResponse Authorize(IDictionary<string, string> headers);
    }
}