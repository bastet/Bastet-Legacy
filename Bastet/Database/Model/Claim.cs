using ServiceStack.DataAnnotations;

namespace Bastet.Database.Model
{
    public class Claim
    {
        /// <summary>
        /// The unique ID of this device
        /// </summary>
        [AutoIncrement]
        public long Id { get; set; }

        /// <summary>
        /// The ID of the user this session is for
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// The name of the claim
        /// </summary>
        public string Name { get; set; }
    }
}
