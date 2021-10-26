using System.Net;

namespace VehicleNotificationService.Business.Model
{
    public class StopParkingResponse
    {
        public HttpStatusCode StatusCode { get; set; }
        public int? ParkingActionId { get; set; }
        public string StatusMessage { get; set; }
    }
}
