using ServiceStack.DataAnnotations;

namespace Bastet.Database.Model
{
    public class Sensor
    {
        /// <summary>
        /// The unique ID of this reading
        /// </summary>
        [AutoIncrement]
// ReSharper disable UnusedAutoPropertyAccessor.Global
        public long Id { get; set; }
// ReSharper restore UnusedAutoPropertyAccessor.Global

        /// <summary>
        /// The device this sensor is attached to
        /// </summary>
// ReSharper disable UnusedAutoPropertyAccessor.Global
        public long DeviceId { get; set; }
// ReSharper restore UnusedAutoPropertyAccessor.Global
    }
}
