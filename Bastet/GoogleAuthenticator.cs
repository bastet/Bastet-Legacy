using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Bastet
{
    public class GoogleAuthenticator
    {
        const int INTERVAL_LENGTH = 30;
        const int PIN_LENGTH = 6;
        static readonly int _pinModulo = (int)Math.Pow(10, PIN_LENGTH);
        static readonly DateTime _unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        ///   Number of intervals that have elapsed.
        /// </summary>
        static long CurrentInterval
        {
            get
            {
                var elapsedSeconds = (long)Math.Floor((DateTime.UtcNow - _unixEpoch).TotalSeconds);

                return elapsedSeconds / INTERVAL_LENGTH;
            }
        }

        /// <summary>
        ///   Generates a QR code bitmap for provisioning.
        /// </summary>
        public byte[] GenerateProvisioningImage(string identifier, byte[] key, int width, int height)
        {
            var keyString = Encoder.Base32Encode(key);
            var provisionUrl = Encoder.UrlEncode(string.Format("otpauth://totp/{0}?secret={1}", identifier, keyString));

            var chartUrl = string.Format("http://chart.apis.google.com/chart?cht=qr&chs={0}x{1}&chl={2}", width, height, provisionUrl);

            Process.Start(chartUrl);

            using (var client = new WebClient())
            {
                return client.DownloadData(chartUrl);
            }
        }

        /// <summary>
        ///   Generates a pin for the given key.
        /// </summary>
        public string GeneratePin(byte[] key)
        {
            return GeneratePin(key, CurrentInterval);
        }

        /// <summary>
        ///   Generates a pin by hashing a key and counter.
        /// </summary>
        private static string GeneratePin(byte[] key, long counter)
        {
            //Get counter bytes (in big endian order)
            var counterBytes = BitConverter.GetBytes(counter);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(counterBytes);

            byte[] hash;
            using (var hmac = new HMACSHA1(key))
                hash = hmac.ComputeHash(counterBytes);

            var offset = hash[hash.Length - 1] & 0xF;

            var selectedBytes = new byte[sizeof(int)];
            Buffer.BlockCopy(hash, offset, selectedBytes, 0, sizeof(int));

            //spec interprets bytes in big-endian order
            if (BitConverter.IsLittleEndian)
                Array.Reverse(selectedBytes);

            var selectedInteger = BitConverter.ToInt32(selectedBytes, 0);

            //remove the most significant bit for interoperability per spec
            var truncatedHash = selectedInteger & 0x7FFFFFFF;

            //generate number of digits for given pin length
            var pin = truncatedHash % _pinModulo;

            return pin.ToString(CultureInfo.InvariantCulture).PadLeft(PIN_LENGTH, '0');
        }

        #region Nested type: Encoder

        static class Encoder
        {
            /// <summary>
            ///   Url Encoding (with upper-case hexadecimal per OATH specification)
            /// </summary>
            public static string UrlEncode(string value)
            {
                const string urlEncodeAlphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~";

                var builder = new StringBuilder();

                foreach (var symbol in value)
                {
                    if (urlEncodeAlphabet.IndexOf(symbol) != -1)
                    {
                        builder.Append(symbol);
                    }
                    else
                    {
                        builder.Append('%');
                        builder.Append(((int)symbol).ToString("X2"));
                    }
                }

                return builder.ToString();
            }

            /// <summary>
            ///   Base-32 Encoding
            /// </summary>
            public static string Base32Encode(IList<byte> data)
            {
                const int inByteSize = 8;
                const int outByteSize = 5;
                const string base32Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

                var builder = new StringBuilder((data.Count + 7) * inByteSize / outByteSize);

                int i = 0, index = 0;
                while (i < data.Count)
                {
                    int currentByte = data[i];
                    int digit;

                    //Is the current digit going to span a byte boundary?
                    if (index > (inByteSize - outByteSize))
                    {
                        int nextByte = (i + 1) < data.Count ? data[i + 1] : 0;

                        digit = currentByte & (0xFF >> index);
                        index = (index + outByteSize) % inByteSize;
                        digit <<= index;
                        digit |= nextByte >> (inByteSize - index);
                        i++;
                    }
                    else
                    {
                        digit = (currentByte >> (inByteSize - (index + outByteSize))) & 0x1F;
                        index = (index + outByteSize) % inByteSize;

                        if (index == 0)
                        {
                            i++;
                        }
                    }

                    builder.Append(base32Alphabet[digit]);
                }

                return builder.ToString();
            }
        }

        #endregion
    }
}