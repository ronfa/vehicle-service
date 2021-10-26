using System;
using VehicleNotificationService.Business.Model;
using VehicleNotificationService.EventHubWorker.Model;

namespace VehicleNotificationService.EventHubWorker.Mapping
{
    public static class EventHubMapper
    {
        public const string SourceApplication = "WebFleet";
        public const string CountryCode = "NL";
        public static VehicleEvent ToVehicleEventRequest(this EventHubMessage request)
        {
            var referenceNumber = request.Data.VehicleIdentification.ThirdPartyReferenceNumber;
            string licensePlate = string.Empty;
            if(!string.IsNullOrWhiteSpace(referenceNumber))
            {
                licensePlate = referenceNumber.FormatLicensePlate();
            }
            var response = new VehicleEvent
            {
                LicensePlate = licensePlate,
                SourceApplication = SourceApplication,
                CountryCode = CountryCode
            };

            if (!string.IsNullOrWhiteSpace(request.Data.StandStillEnd.Time) &&
                DateTime.TryParse(request.Data.StandStillEnd.Time, out DateTime timestamp))
            {
                response.Timestamp = timestamp.ToString();
            }
            else
            {
                response.Timestamp = DateTime.UtcNow.ToString();
            }

            return response;
        }

        private static string FormatLicensePlate(this string stringToFormat) => stringToFormat.Contains("|") ? 
            stringToFormat.Substring(stringToFormat.LastIndexOf("|") + 1).Replace("-","").Trim() : 
            stringToFormat.Substring(stringToFormat.IndexOf("-") + 1).Replace("-", "").Trim();
    }
}
