using System;
using System.Threading.Tasks;
using VehicleNotificationService.Business.Model;

namespace VehicleNotificationService.Business.Services
{
    public interface IPhonixxMessageSender
    {
        Task<VerifyParkingResponse> VerifyStop(VehicleEvent message, int supplierId, Guid tripId);

        Task<StopParkingResponse> StopParking(VehicleEvent message, int supplierId, Guid tripId, ParkingAction parkingAction);
    }
}
