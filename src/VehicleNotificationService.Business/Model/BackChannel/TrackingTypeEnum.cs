using System;

namespace VehicleNotificationService.Business.Model.BackChannel
{
    [Flags]
    public enum TrackingTypeEnum : int
    {
        None = 0,
        AutoStart = 1,
        AutoStop = 2
    }
}
