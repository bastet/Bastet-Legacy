﻿using System;
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
        public long Id { get; set; }

        /// <summary>
        /// The timestamp of this reading
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// The sensor which took this reading
        /// </summary>
        public long SensorId { get; set; }

        /// <summary>
        /// The value of this reading
        /// </summary>
        public decimal Value { get; set; }
    }
}
