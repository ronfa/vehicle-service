using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VehicleNotificationService.Business.Model;

namespace VehicleNotificationService.Business.Services
{
    public interface IParkingService
    {
        Task<VerifyParkingResponse> VerifyParking(VehicleEvent message, int supplierId, Guid tripId);

        Task<StopParkingResponse[]> StopParking(VehicleEvent message, VerifyParkingResponse verifyResponse,
            List<ParkingAction> parkingActions, int supplierId, Guid tripIds);

        StartParkingResponse StartParking(VehicleEvent message, VerifyParkingResponse verifyResponse, int supplierId);
    }
}
