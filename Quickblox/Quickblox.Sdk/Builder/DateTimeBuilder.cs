﻿using System;
using Quickblox.Sdk.Logger;

namespace Quickblox.Sdk.Builder
{
    /// <summary>
    /// DateTimeBuilder class.
    /// </summary>
    internal static class DateTimeBuilder
    {
        #region Fields

        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0);

        #endregion

        #region Public Members

        public static Int64 ToUnixEpoch(this DateTime dateTime)
        {
            var timeSpan = dateTime.Subtract(UnixEpoch);
            return (Int64)timeSpan.TotalSeconds;
        }

        public static DateTime ToDateTime(this long unixTime)
        {
            try
            {
                return UnixEpoch.AddSeconds(unixTime).ToLocalTime();
            }
            catch (Exception ex)
            {
                LoggerHolder.LoggerInstance.Log(LogLevel.Error, $"Failed to convert {unixTime} to DateTime. Exception: {ex}");
                return default(DateTime);
            }
            
        }

        #endregion
    }
}
