using VehicleNotificationService.Business.Extensions;
using VehicleNotificationService.Business.Helpers;
using VehicleNotificationService.Business.Model;
using Xunit;

namespace VehicleNotificationService.Business.Tests
{
    public class PhonixxEnvironmentHelperTests
    {

        [Theory]
        [InlineData("NL", null, PhonixxLabel.ParkmobileNetherlands)]
        [InlineData("NL", "PL", PhonixxLabel.Parkline)]
        [InlineData("DE", null, PhonixxLabel.Parknow)]
        [InlineData("BE", null, PhonixxLabel.ParkmobileBelgium)]
        [InlineData("GB", null, PhonixxLabel.ParkmobileUnitedKingdom)]
        [InlineData("UK", null, PhonixxLabel.ParkmobileUnitedKingdom)]
        [InlineData("", null, PhonixxLabel.Unknown)]
        [InlineData(null, null, PhonixxLabel.Unknown)]
        public void ResolveSupplier(string countryCode, string brand, PhonixxLabel expected)
        {

            var result = PhonixxEnvironmentHelper.ResolveSupplierId(new VehicleEvent
            {
                CountryCode = countryCode,
                Brand = brand
            });

            Assert.Equal(expected.AsInt(), result);
        }
    }
}
