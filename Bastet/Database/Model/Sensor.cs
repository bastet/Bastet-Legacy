using System;

namespace Bastet.Database.Model
{
    public class Sensor
    {
        /// <summary>
        /// The unique ID of this reading
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// The device this sensor is attached to
        /// </summary>
        public virtual Device Device { get; set; }
    }
}
