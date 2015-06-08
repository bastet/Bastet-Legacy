using Bastet.Annotations;
using ServiceStack.DataAnnotations;

namespace Bastet.Database.Model
{
    public class Claim
    {
        /// <summary>
        /// The unique ID of this claim
        /// </summary>
        [AutoIncrement]
        public long Id { get; [UsedImplicitly]set; }

        /// <summary>
        /// The ID of the user this claim is for
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// The name of the claim
        /// </summary>
        public string Name { get; set; }

        public Claim(User user, string claimName)
        {
            UserId = user.Id;
            Name = claimName;
        }
    }
}
