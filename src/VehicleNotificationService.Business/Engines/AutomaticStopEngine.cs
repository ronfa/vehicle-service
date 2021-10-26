using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VehicleNotificationService.Business.Model;
using VehicleNotificationService.Business.Model.Configuration;

namespace VehicleNotificationService.Business.Engines
{
    public class AutomaticStopEngine : IParkingEngine
    {
        private ILogger _logger;
        private readonly AutomaticStopConfig _autoStopConfig;

        private const int GracePeriodSecondsFromStartTime = 60;

        public AutomaticStopEngine(IOptions<AutomaticStopConfig> appConfig, ILogger logger)
        {
            _logger = logger;
            _autoStopConfig = appConfig.Value;
        }

        public List<ParkingAction> Evaluate(VerifyParkingResponse verifyParking, out List<string> messages)
        {
            // Possible rules
            // 1. Provide grace period of X minutes from start time (in case user started parking session ahead of arrival)
            // 2. Geo check with threshold? 
            // 3. Check stop time, if within the next X minutes, let phonixx stop it instead?
            // 4. If more than one parking action found, determine which one to stop based on location

            messages = new List<string>();

            var parkingActions = verifyParking.ActiveSessions;
            
            if (parkingActions == null || !parkingActions.Any())
            {
                // nothing in list, abort
                messages.Add("No active parking sessions found");
                return parkingActions;
            }

            // Filter any parking actions by start time grace period
            parkingActions = FilterByStartTimeGracePeriod(parkingActions, out var graceReasoning);
            messages.AddRange(graceReasoning);



            return parkingActions;
        }

        private List<ParkingAction> FilterByStartTimeGracePeriod(List<ParkingAction> parkingActions,
            out List<string> reasoning)
        {
            reasoning = new List<string>();

            var graceFromStartInSeconds = _autoStopConfig.GracePeriodSecondsFromStartTime;

            if (graceFromStartInSeconds == 0)
            {
                graceFromStartInSeconds = GracePeriodSecondsFromStartTime;
            }

            var filteredActions = new List<ParkingAction>();

            foreach (var parkingAction in parkingActions)
            {
                if (parkingAction.StartTimeUtc.AddSeconds(graceFromStartInSeconds) < DateTime.UtcNow)
                {
                    filteredActions.Add(parkingAction);
                }
                else
                {
                    reasoning.Add(
                        $"Parking action id {parkingAction.Id} will not be stopped as it was started less than {graceFromStartInSeconds} seconds ago.");
                }
            }

            return filteredActions;
        }
    }
}
