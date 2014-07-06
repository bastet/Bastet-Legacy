using System;
using System.Security.Cryptography;
using ServiceStack.DataAnnotations;

namespace Bastet.Database.Model
{
    public class Session
    {
        /// <summary>
        /// The unique ID of this device
        /// </summary>
        [AutoIncrement]
// ReSharper disable UnusedAutoPropertyAccessor.Global
        public long Id { get; set; }
// ReSharper restore UnusedAutoPropertyAccessor.Global

        /// <summary>
        /// The API key of this session
        /// </summary>
        public string SessionKey { get; set; }

        /// <summary>
        /// The ID of the user this session is for
        /// </summary>
        public long UserId { get; set; }

        public Session(User user)
        {
            UserId = user.Id;

            using (RandomNumberGenerator rng = new RNGCryptoServiceProvider())
            {
                byte[] tokenData = new byte[32];
                rng.GetBytes(tokenData);
                SessionKey = BitConverter.ToString(tokenData).Replace("-", "");
            }
        }
    }
}
