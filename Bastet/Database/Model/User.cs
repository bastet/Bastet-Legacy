
using System;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Bastet.Database.Model
{
    public class User
    {
        /// <summary>
        /// The unique ID of this user
        /// </summary>
        [AutoIncrement]
        public long Id { get; set; }

        /// <summary>
        /// The nickname to use when addressing this user
        /// </summary>
        public string Nick { get; set; }

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

        /// <summary>
        /// Generate a new random salt value
        /// </summary>
        /// <returns></returns>
        public static long GenerateSalt()
        {
            byte[] saltBytes = new byte[8];
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
                rng.GetBytes(saltBytes);

            return BitConverter.ToInt64(saltBytes, 0);
        }

        /// <summary>
        /// Compute the salted hash of the given password
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public string ComputeSaltedHash(string password)
        {
            //Get data to hash
            UTF32Encoding encoder = new UTF32Encoding();
            Byte[] passwordBytes = encoder.GetBytes(password);
            Byte[] saltBytes = BitConverter.GetBytes(Salt);

            // aggregate password and salt into one array
            Byte[] toHash = new Byte[passwordBytes.Length + saltBytes.Length];
            Array.Copy(passwordBytes, 0, toHash, 0, passwordBytes.Length);
            Array.Copy(saltBytes, 0, toHash, saltBytes.Length, saltBytes.Length);

            // Hash the password
            using (SHA512Managed sha1 = new SHA512Managed())
                return encoder.GetString(sha1.ComputeHash(toHash));
        }
    }
}
