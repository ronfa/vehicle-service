using System;
using System.Collections.Generic;
using System.Linq;
using VehicleNotificationService.Business.Extensions;
using VehicleNotificationService.Business.Model;
using VehicleNotificationService.Business.Model.Phonixx;

namespace VehicleNotificationService.Business.Mappings
{
    public static class ParkingMapper
    {
        public static VerifyStopRequest ToVerifyRequest(this VehicleEvent request, int supplierId, Guid TripId)
        {

            var identifications = new List<Identification>();

            if (request.EventDetails != null && request.EventDetails.Any())
            {
                identifications.AddRange(request.EventDetails.Select(ToIdentification));
            }

            identifications.Add(new Identification
                {Type = IdentificationEnum.LicensePlate.ToString(), Value = request.LicensePlate});

            var response = new VerifyStopRequest
            {
                CountryCode = request.CountryCode,
                SourceApplication = request.SourceApplication,
                SupplierId = supplierId,
                Identification = identifications,
                TripId = TripId
            };

            if (!string.IsNullOrWhiteSpace(request.Timestamp) &&
                DateTime.TryParse(request.Timestamp, out DateTime timestamp))
            {
                response.TimestampUtc = timestamp;
            }
            else
            {
                response.TimestampUtc = DateTime.UtcNow;
            }

            return response;
        }

        public static StopParkingRequest ToStopParkingRequest(this VehicleEvent request, int supplierId, Guid tripId,
            ParkingAction parkingAction)
        {
            var identifications = new List<Identification>();

            if (request.EventDetails != null && request.EventDetails.Any())
            {
                identifications.AddRange(request.EventDetails.Select(eventDetail =>
                    eventDetail.ToIdentification()));
            }

            identifications.Add(new Identification
            {
                Type = IdentificationEnum.LicensePlate.ToString(),
                Value = request.LicensePlate
            });
            
            var timestamp = DateTime.UtcNow;            

            return new StopParkingRequest
            {
                Id = parkingAction.Id,
                SourceApplication = request.SourceApplication,
                StopTimeUtc = timestamp,
                Identification = identifications,
                TripId = tripId
            };
        }

        public static Identification ToIdentification(this KeyValuePair<string, string> input)
        {
            return new Identification
            {
                Type = input.Key,
                Value = input.Value
            };
        }
    }
}
