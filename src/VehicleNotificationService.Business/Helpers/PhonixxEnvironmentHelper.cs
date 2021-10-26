using VehicleNotificationService.Business.Extensions;
using VehicleNotificationService.Business.Model;

namespace VehicleNotificationService.Business.Helpers
{
    public static class PhonixxEnvironmentHelper
    {
        public static int ResolveSupplierId(VehicleEvent message)
        {
            if (message == null || string.IsNullOrWhiteSpace(message.CountryCode))
            {
                return 0;
            }

            switch (message.CountryCode.ToLower())
            {
                case "nl":
                    return
                        (!string.IsNullOrWhiteSpace(message.Brand) && message.Brand.ToLower().Contains("pl")
                            ? PhonixxLabel.Parkline
                            : PhonixxLabel.ParkmobileNetherlands).AsInt();
                case "uk":
                case "gb":
                    return PhonixxLabel.ParkmobileUnitedKingdom.AsInt();
                case "be":
                    return PhonixxLabel.ParkmobileBelgium.AsInt();
                default:
                    return PhonixxLabel.Parknow.AsInt();
            }
        }
    }

    public enum PhonixxLabel : int
    {
        Unknown = 0,
        ParkmobileUnitedKingdom = 1,
        ParkmobileNetherlands = 20,
        ParkmobileBelgium = 202,
        Parkline = 206,
        Parknow = 349
    }
}
