using System.Collections.Generic;

namespace VehicleNotificationService.Business.Model
{
    public class VerifyParkingResponse
    {
        public string StatusCode { get; set; }
        public int SupplierId { get; set; }
        public int ClientId { get; set; }
        public List<ParkingAction> ActiveSessions { get; set; }
    }
}
