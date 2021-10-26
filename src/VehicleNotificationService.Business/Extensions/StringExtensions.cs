using System;
using System.Collections.Generic;

namespace VehicleNotificationService.Business.Extensions
{
    public static class StringExtensions
    {

        public static string FormatAsString(this List<string> listOfStrings)
        {
            return $" [ {string.Join(Environment.NewLine, listOfStrings)}] ";
        }

        public static DateTime FormatDateTime(this string input, DateTime? defaultDateTime = null)
        {
            if (!defaultDateTime.HasValue)
            {
                defaultDateTime = DateTime.UtcNow;
            }
            
            if (string.IsNullOrWhiteSpace(input))
            {
                return defaultDateTime.Value;
            }

            if (!DateTime.TryParse(input, out var timestamp))
            {
                return defaultDateTime.Value;
            }

            return timestamp;
        }

    }
}
