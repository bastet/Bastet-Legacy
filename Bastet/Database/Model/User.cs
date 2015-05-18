using System;
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

            //Get data to hash
            UTF32Encoding encoder = new UTF32Encoding();
            Byte[] passwordBytes = encoder.GetBytes(password);
            Byte[] saltBytes = BitConverter.GetBytes(Salt);

            // aggregate password and salt into one array
            Byte[] toHash = new Byte[passwordBytes.Length + saltBytes.Length];
            Array.Copy(passwordBytes, 0, toHash, 0, passwordBytes.Length);
            Array.Copy(saltBytes, 0, toHash, passwordBytes.Length, saltBytes.Length);

            // Hash the password
            using (SHA512Managed sha1 = new SHA512Managed())
                return BitConverter.ToString(sha1.ComputeHash(toHash)).Replace("-", "");
        }
    }
}
