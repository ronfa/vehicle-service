using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VehicleNotificationService.Business.Model;
using VehicleNotificationService.Business.Model.Configuration;

namespace VehicleNotificationService.Business.Services
{
    public class ParkingService : IParkingService
    {
        private ILogger _logger;
        private readonly IPhonixxMessageSender _messageSender;
        
        public ParkingService(IOptions<ApplicationConfig> appConfig, ILogger logger, IPhonixxMessageSender messageSender)
        {
            _logger = logger;
            _messageSender = messageSender;
        }

        public async Task<VerifyParkingResponse> VerifyParking(VehicleEvent message, int supplierId, Guid tripId)
        {
            return await _messageSender.VerifyStop(message, supplierId, tripId);
        }

        public async Task<StopParkingResponse[]> StopParking(VehicleEvent message, VerifyParkingResponse verifyResponse,
            List<ParkingAction> parkingActions, int supplierId, Guid tripId)
        {
            var stopTasks = parkingActions
                .Select(parkingAction => _messageSender.StopParking(message, supplierId, tripId, parkingAction)).ToList();

            return await Task.WhenAll<StopParkingResponse>(stopTasks);
        }

        public StartParkingResponse StartParking(VehicleEvent message, VerifyParkingResponse verifyResponse, int supplierId)
        {
            throw new NotImplementedException();
        }
    }
}
