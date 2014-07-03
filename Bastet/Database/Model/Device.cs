using ServiceStack.DataAnnotations;

namespace Bastet.Database.Model
{
    public class Device
    {
        /// <summary>
        /// The unique ID of this device
        /// </summary>
        [AutoIncrement]
        public long Id { get; set; }

        /// <summary>
        /// The URL to speakto this device (using CoAP)
        /// </summary>
        public string Url { get; set; }
    }
}
