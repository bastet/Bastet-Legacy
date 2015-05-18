using Bastet.Annotations;
using ServiceStack.DataAnnotations;

namespace Bastet.Database.Model
{
    public class Sensor
    {
        /// <summary>
        /// The unique ID of this reading
        /// </summary>
        [AutoIncrement]
        public long Id { get; [UsedImplicitly]set; }

        /// <summary>
        /// The device this sensor is attached to
        /// </summary>
        public long DeviceId { get; set; }
    }
}
