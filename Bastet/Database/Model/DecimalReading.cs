using System;
using ServiceStack.DataAnnotations;

namespace Bastet.Database.Model
{
    public class DecimalReading
        : IReading<decimal>
    {
        /// <summary>
        /// The unique ID of this reading
        /// </summary>
        [AutoIncrement]
// ReSharper disable UnusedAutoPropertyAccessor.Global
        public long Id { get; set; }
// ReSharper restore UnusedAutoPropertyAccessor.Global

        /// <summary>
        /// The timestamp of this reading
        /// </summary>
// ReSharper disable UnusedAutoPropertyAccessor.Global
        public DateTime Timestamp { get; set; }
// ReSharper restore UnusedAutoPropertyAccessor.Global

        /// <summary>
        /// The sensor which took this reading
        /// </summary>
        public long SensorId { get; set; }

        /// <summary>
        /// The value of this reading
        /// </summary>
// ReSharper disable UnusedAutoPropertyAccessor.Global
        public decimal Value { get; set; }
// ReSharper restore UnusedAutoPropertyAccessor.Global
    }
}
