// Copyright (c) 2017 Shaun Sales. All rights reserved.
// Use of this source code is governed by a MIT license that can be found
// in the LICENSE file in the project root or at https://opensource.org/licenses/MIT

using System;

namespace MerkleTree
{
    public static class ExtensionMethods
    {
        private static readonly uint[] m_HexStringLookup = CreateHexStringLookup();

        private static uint[] CreateHexStringLookup()
        {
            var result = new uint[256];

            for (var i = 0; i < 256; i++)
            {
                var s = $"{i:X2}";
                result[i] = s[0] + ((uint)s[1] << 16);
            }

            return result;
        }

        public static string ToHexString(this byte[] bytes)
        {
            var result = new char[bytes.Length * 2];

            for (var i = 0; i < bytes.Length; i++)
            {
                var val = m_HexStringLookup[bytes[i]];

                result[2 * i] = (char)val;
                result[2 * i + 1] = (char)(val >> 16);
            }

            return new string(result);
        }

        /// <summary>
        /// Returns the human-readable file size for an arbitrary 32-bit file size 
        /// The default format is "0.### XB"
        /// </summary>
        /// <param name="sizeInBytes">Size of the data in bytes</param>
        /// <returns>"4.2 KB" or "1.434 GB</returns>
        public static string ToFileSizeString(this int sizeInBytes)
        {
            return ((long)sizeInBytes).ToFileSizeString();
        }

        /// <summary>
        /// Returns the human-readable file size for an arbitrary 64-bit file size 
        /// The default format is "0.### XB"
        /// </summary>
        /// <param name="sizeInBytes">Size of the data in bytes</param>
        /// <returns>"4.2 KB" or "1.434 GB</returns>
        /// <remarks>
        /// Modified code based on http://www.somacon.com/p576.php
        /// </remarks>
        public static string ToFileSizeString(this long sizeInBytes)
        {
            // Get absolute value
            sizeInBytes = Math.Abs(sizeInBytes);

            // Determine the suffix and readable value
            string suffix;
            double readable;

            if (sizeInBytes >= 0x1000000000000000) // Exabyte
            {
                suffix = "EB";
                readable = sizeInBytes >> 50;
            }
            else if (sizeInBytes >= 0x4000000000000) // Petabyte
            {
                suffix = "PB";
                readable = sizeInBytes >> 40;
            }
            else if (sizeInBytes >= 0x10000000000) // Terabyte
            {
                suffix = "TB";
                readable = sizeInBytes >> 30;
            }
            else if (sizeInBytes >= 0x40000000) // Gigabyte
            {
                suffix = "GB";
                readable = sizeInBytes >> 20;
            }
            else if (sizeInBytes >= 0x100000) // Megabyte
            {
                suffix = "MB";
                readable = sizeInBytes >> 10;
            }
            else if (sizeInBytes >= 0x400) // Kilobyte
            {
                suffix = "KB";
                readable = sizeInBytes;
            }
            else
            {
                return $"{sizeInBytes:0B}"; // Byte
            }

            // Divide by 1024 to get fractional value
            readable = readable / 1024;

            // Return formatted number with suffix
            return $"{readable:0.###}{suffix}";
        }
    }
}
