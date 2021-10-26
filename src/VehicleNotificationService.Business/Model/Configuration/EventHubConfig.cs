namespace VehicleNotificationService.Business.Model.Configuration
{
    public class EventHubConfig
    {
        public string ConnectionString{ get; set; }
        public string EventHubName { get; set; }
        public string SharedAccessKey { get; set; }
    }
}
