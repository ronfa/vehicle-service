using System.Collections.Generic;
using VehicleNotificationService.Business.Model;

namespace VehicleNotificationService.Business.Engines
{
    public interface IParkingEngine
    {
        List<ParkingAction> Evaluate(VerifyParkingResponse verifyParking, out List<string> messages);
    }
}
