using System;

namespace VehicleNotificationService.Business.Extensions
{
    public static class EnumExtensions
    {
        public static int AsInt(this Enum enumValue)
        {
            return (int)((object)enumValue);
        }
    }
}
