using System;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using MoreLinq;
using ServiceStack.DataAnnotations;

namespace Bastet.Database.Model
{
    public class User
    {
        /// <summary>
        /// The unique ID of this user
        /// </summary>
        [AutoIncrement]
// ReSharper disable UnusedAutoPropertyAccessor.Global
        public long Id { get; set; }
// ReSharper restore UnusedAutoPropertyAccessor.Global

        /// <summary>
        /// The username for logging in
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// The password salt for this user
        /// </summary>
        public long Salt { get; set; }

        /// <summary>
        /// The salted and hashed password of this user
        /// </summary>
        public string PasswordHash { get; set; }

        public User(string name, string password)
        {
            Username = name;

            Salt = SecurityHelpers.GenerateSecureRandomNumber();
            PasswordHash = ComputeSaltedHash(password);
        }

        /// <summary>
        /// Compute the salted hash of the given password
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public string ComputeSaltedHash(string password)
        {
            if (password == null)
                throw new ArgumentNullException("password");

            //Encoder for password (text->bytes)
            UTF32Encoding encoder = new UTF32Encoding();

            //Put password bytes and then saly bytes into byte array
            var toHash = encoder.GetBytes(password)
                .Concat(BitConverter.GetBytes(Salt))
                .ToArray();
            
            // Hash the salted password
            using (SHA512Managed sha1 = new SHA512Managed())
                return BitConverter.ToString(sha1.ComputeHash(toHash)).Replace("-", "");
        }
    }
}
