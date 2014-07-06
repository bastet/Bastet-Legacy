using ServiceStack.DataAnnotations;

namespace Bastet.Database.Model
{
    public class Device
    {
        /// <summary>
        /// The unique ID of this device
        /// </summary>
        [AutoIncrement]
// ReSharper disable UnusedAutoPropertyAccessor.Global
        public long Id { get; set; }
// ReSharper restore UnusedAutoPropertyAccessor.Global

        /// <summary>
        /// The URL to speakto this device (using CoAP)
        /// </summary>
// ReSharper disable UnusedAutoPropertyAccessor.Global
        public string Url { get; set; }
// ReSharper restore UnusedAutoPropertyAccessor.Global
    }
}
