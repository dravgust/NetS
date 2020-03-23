using System;

namespace NetS.Core.Utilities
{
    public static class DateTimeUtils
    {
        private static readonly DateTimeOffset UnixRef = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

        public static uint DateTimeToUnixTime(this DateTimeOffset dt)
        {
            return (uint)DateTimeUtils.DateTimeToUnixTimeLong(dt);
        }

        internal static ulong DateTimeToUnixTimeLong(DateTimeOffset dateTime)
        {
            dateTime = dateTime.ToUniversalTime();
            if (dateTime < DateTimeUtils.UnixRef)
                throw new ArgumentOutOfRangeException(nameof(dateTime), "The supplied datetime can't be expressed in unix timestamp");

            double totalSeconds = dateTime.Subtract(DateTimeUtils.UnixRef).TotalSeconds;

            var maxValue = (double)uint.MaxValue;
            if (totalSeconds > maxValue)
                throw new ArgumentOutOfRangeException(nameof(totalSeconds), "The supplied datetime can't be expressed in unix timestamp");

            return (ulong)totalSeconds;
        }

        public static DateTimeOffset UnixTimeToDateTime(uint timestamp)
        {
            var timeSpan = TimeSpan.FromSeconds((double)timestamp);
            return DateTimeUtils.UnixRef + timeSpan;
        }

        public static DateTimeOffset UnixTimeToDateTime(ulong timestamp)
        {
            var timeSpan = TimeSpan.FromSeconds((double)timestamp);
            return DateTimeUtils.UnixRef + timeSpan;
        }

        public static DateTimeOffset UnixTimeToDateTime(long timestamp)
        {
            var timeSpan = TimeSpan.FromSeconds((double)timestamp);
            return DateTimeUtils.UnixRef + timeSpan;
        }

	    /// <summary>
	    /// Converts a given DateTime into a Unix timestamp
	    /// </summary>
	    /// <param name="value">Any DateTime</param>
	    /// <returns>The given DateTime in Unix timestamp format</returns>
	    public static int ToUnixTimestamp(this DateTime value)
	    {
		    return (int)Math.Truncate((value.ToUniversalTime().Subtract(new DateTime(1970, 1, 1))).TotalSeconds);
	    }

	    /// <summary>
	    /// Gets a Unix timestamp representing the current moment
	    /// </summary>
	    /// <param name="ignored">Parameter ignored</param>
	    /// <returns>Now expressed as a Unix timestamp</returns>
	    public static int UnixTimestamp(this DateTime ignored)
	    {
		    return (int)Math.Truncate((DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds);
	    }
	}
}
