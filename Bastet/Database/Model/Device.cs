using System;

namespace Bastet.Database.Model
{
    public class Device
    {
        /// <summary>
        /// The unique ID of this device
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// The URL to speakto this device (using CoAP)
        /// </summary>
        public virtual string Url { get; set; }
    }
}
