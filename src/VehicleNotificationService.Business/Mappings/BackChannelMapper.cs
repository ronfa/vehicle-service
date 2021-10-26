using System;
using VehicleNotificationService.Business.Model.BackChannel;
using VehicleNotificationService.Business.Model.Phonixx;

namespace VehicleNotificationService.Business.Mappings
{
    public static class BackChannelMapper
    {
        private const string PMNLCode = "PMNL";
        private const string PMBECode = "PMBE";
        private const string PMUKCode = "PMUK";
        private const string ParkNowCode = "PN";
        private const string ParklineCode = "PL";
        private const string StadspasCode = "SP";

        public static object ToBackChannelMessageFormat(this BackChannelRequest request)
        {
            var backChannelMessage = new BackChannelBaseMessage();
            backChannelMessage.Version = request.Version;
            Enum.TryParse(request.Brand, out Brand brand);
            var brandCode = MapBrandToBrandCode(brand);

            var vrn = request.LicensePlate != null ? request.LicensePlate : request.Vrn != null ? request.Vrn : string.Empty;
            var vehicleIdentification = new VehicleIdentification
            {
                LicensePlate = vrn,
                ParkNowCustomerNumber = string.Format("{0}-{1}-{2}", brandCode, request.CountryCode, request.ClientId),
                ThirdPartyReferenceNumber = string.Format("{0}-{1}-{2}|{3}", brandCode, request.CountryCode, request.ClientId, vrn)
            };

            switch (request.Event)
            {
                case "BackgroundJobs.ParkingAction.Activated":
                    return new ParkingStartMessage
                    {
                        MessageClass = "PARKING_START",
                        Version = request.Version,
                        Data = new Data
                        {
                            VehicleIdentification = vehicleIdentification,
                            ParkingStart = new ParkingStart
                            {
                                MaxParkingTime = request.MaxStopTimeUtc,
                                Time = request.StartTimeUtc,
                                ParkingSessionId = request.ParkingActionId
                            }
                        }
                    };
                case "BackgroundJobs.ParkingAction.Deactivated":
                    return new ParkingEndMessage
                    {
                        MessageClass = "PARKING_END",
                        Version = request.Version,
                        Data = new Data
                        {
                            VehicleIdentification = vehicleIdentification,
                            ParkingEnd = new ParkingEnd
                            {
                                ParkingSessionId = request.ParkingActionId,
                                Time = request.StopTimeUtc
                            }
                        }
                    };
                case "Account.Vehicle.Added":
                case "Account.Vehicle.Removed":
                case "Account.Vehicle.Updated":
                    return GetVehicleEventMessage(request, vehicleIdentification);
            } 
            return backChannelMessage;
        }

        private static object GetVehicleEventMessage(BackChannelRequest request, VehicleIdentification vehicleIdentification) => request.TrackingType
                == (int)TrackingTypeEnum.AutoStop
                ? new VehicleActivatedMessage
                {
                    MessageClass = "PARKNOW_DATA_FEED_PARKING_STOP_ACTIVATED",
                    Version = request.Version,
                    Data = new Data
                    {
                        VehicleIdentification = vehicleIdentification,
                        VehicleActivated = new VehicleActivated
                        {
                            Time = DateTime.UtcNow
                        }
                    }
                }
                :(object)new VehicleDeactivatedMessage
                {
                    MessageClass = "PARKNOW_DATA_FEED_PARKING_STOP_DEACTIVATED",
                    Version = request.Version,
                    Data = new Data
                    {
                        VehicleIdentification = vehicleIdentification,
                        VehicleDeactivated = new VehicleDeactivated
                        {
                            Time = DateTime.UtcNow
                        }
                    }
                };

        private static string MapBrandToBrandCode(Brand brand)
        {
            switch (brand)
            {
                case Brand.ParkMobileNetherlands:
                    return PMNLCode;
                case Brand.ParkMobileBelgium:
                    return PMBECode;
                case Brand.ParkMobileUnitedKingdom:
                    return PMUKCode;
                case Brand.ParkLine:
                    return ParklineCode;
                case Brand.Stadspas:
                    return StadspasCode;
                case Brand.ParkNow:
                    return ParkNowCode;
                default:
                    return string.Empty;
            }
        }        
    }
}
