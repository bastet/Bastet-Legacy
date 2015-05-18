using Bastet.Annotations;
using ServiceStack.DataAnnotations;

namespace Bastet.Database.Model
{
    public class Device
    {
        /// <summary>
        /// The unique ID of this device
        /// </summary>
        [AutoIncrement]
        public long Id { get; [UsedImplicitly]set; }

        /// <summary>
        /// The URL to speak to this device
        /// </summary>
        public string Url { get; [UsedImplicitly]set; }

        /// <summary>
        /// The type of the backen to use to communicate with this device
        /// </summary>
        public string Backend { get; [UsedImplicitly]set; }
    }
}
