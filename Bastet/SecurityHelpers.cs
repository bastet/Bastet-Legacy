using System;
using System.Security.Cryptography;

namespace Bastet
{
    public static class SecurityHelpers
    {
        public static bool SlowEquals(this string a, string b)
        {
            uint diff = (uint)a.Length ^ (uint)b.Length;
            for (int i = 0; i < a.Length && i < b.Length; i++)
                diff |= (uint)(a[i] ^ b[i]);
            return diff == 0;
        }

        public static long GenerateSecureRandomNumber()
        {
            byte[] saltBytes = new byte[sizeof(long)];
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
                rng.GetBytes(saltBytes);

            return BitConverter.ToInt64(saltBytes, 0);
        }
    }
}
