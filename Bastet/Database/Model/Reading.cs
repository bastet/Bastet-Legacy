using System;

namespace Bastet.Database.Model
{
    public class Reading
    {
        /// <summary>
        /// The unique ID of this reading
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// The timestamp of this reading
        /// </summary>
        public virtual DateTime Timestamp { get; set; }

        /// <summary>
        /// The sensor which took this reading
        /// </summary>
        public virtual Sensor Sensor { get; set; }
    }
}
