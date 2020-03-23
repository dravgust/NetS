using System;
using NetS.Core.Utilities;

namespace NetS.Core
{
    /// <inheritdoc />
    public class DateTimeProvider : IDateTimeProvider
    {
        /// <summary>Static instance of the object to prevent the need of creating new instance.</summary>
        private static readonly IDateTimeProvider DefaultInstance;

        /// <summary>Static instance of the object to prevent the need of creating new instance.</summary>
        public static IDateTimeProvider Default
        {
            get => DefaultInstance;
        }

        /// <summary>UTC adjusted timestamp, or null if no adjusted time is set.</summary>
        private TimeSpan _adjustedTimeOffset;

        /// <summary>
        /// Initializes a default instance of the object.
        /// </summary>
        static DateTimeProvider()
        {
            DefaultInstance = new DateTimeProvider();
        }

        public DateTimeProvider()
        {
            _adjustedTimeOffset = TimeSpan.Zero;
        }

        /// <inheritdoc />
        public long GetTime()
        {
            return DateTime.UtcNow.ToUnixTimestamp();
        }

        /// <inheritdoc />
        public DateTimeOffset GetTimeOffset()
        {
            return DateTimeOffset.UtcNow;
        }

        /// <inheritdoc />
        public DateTime GetUtcNow()
        {
            return DateTime.UtcNow;
        }

        /// <inheritdoc />
        public DateTime GetAdjustedTime()
        {
            return this.GetUtcNow().Add(_adjustedTimeOffset);
        }

        /// <inheritdoc />
        public long GetAdjustedTimeAsUnixTimestamp()
        {
            return new DateTimeOffset(this.GetAdjustedTime()).ToUnixTimeSeconds();
        }

        /// <inheritdoc />
        public void SetAdjustedTimeOffset(TimeSpan adjustedTimeOffset)
        {
            this._adjustedTimeOffset = adjustedTimeOffset;
        }
    }
}
