/*
 * Copyright (c) 2006-2016, openmetaverse.co
 * Copyright (c) Contributors, http://opensimulator.org/
 * See CONTRIBUTORS.TXT for a full list of copyright holders.
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Neither the name of the openmetaverse.co nor the names
 *   of its contributors may be used to endorse or promote products derived from
 *   this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace OpenMetaverse
{
    public static partial class Utils
    {
        /// <summary>
        /// Operating system
        /// </summary>
        public enum Platform
        {
            /// <summary>Unknown</summary>
            Unknown,
            /// <summary>Microsoft Windows</summary>
            Windows,
            /// <summary>Microsoft Windows CE</summary>
            WindowsCE,
            /// <summary>Linux</summary>
            Linux,
            /// <summary>Apple OSX</summary>
            OSX
        }

        /// <summary>
        /// Runtime platform
        /// </summary>
        public enum Runtime
        {
            /// <summary>.NET runtime</summary>
            Windows,
            /// <summary>Mono runtime: http://www.mono-project.com/</summary>
            Mono
        }

        public const float E = (float)Math.E;
        public const float LOG10E = 0.4342945f;
        public const float LOG2E = 1.442695f;
        public const float PI = (float)Math.PI;
        public const float TWO_PI = (float)(Math.PI * 2.0d);
        public const float PI_OVER_TWO = (float)(Math.PI / 2.0d);
        public const float PI_OVER_FOUR = (float)(Math.PI / 4.0d);
        /// <summary>Used for converting degrees to radians</summary>
        public const float DEG_TO_RAD = (float)(Math.PI / 180.0d);
        /// <summary>Used for converting radians to degrees</summary>
        public const float RAD_TO_DEG = (float)(180.0d / Math.PI);

        /// <summary>Provide a single instance of the CultureInfo class to
        /// help parsing in situations where the grid assumes an en-us 
        /// culture</summary>
        public static readonly System.Globalization.CultureInfo EnUsCulture =
            new System.Globalization.CultureInfo("en-us", false);

        /// <summary>UNIX epoch in DateTime format</summary>
        public static readonly DateTime Epoch = new DateTime(1970, 1, 1);

        public static readonly byte[] EmptyBytes = Array.Empty<byte>();

        /// <summary>Provide a single instance of the MD5 class to avoid making
        /// duplicate copies and handle thread safety</summary>
        private static readonly System.Security.Cryptography.MD5 MD5Builder =
            new System.Security.Cryptography.MD5CryptoServiceProvider();

        /// <summary>Provide a single instance of the SHA-1 class to avoid
        /// making duplicate copies and handle thread safety</summary>
        private static readonly System.Security.Cryptography.SHA1 SHA1Builder =
            new System.Security.Cryptography.SHA1CryptoServiceProvider();

        private static readonly System.Security.Cryptography.SHA256 SHA256Builder =
            new System.Security.Cryptography.SHA256Managed();

        /// <summary>Provide a single instance of a random number generator
        /// to avoid making duplicate copies and handle thread safety</summary>
        private static readonly Random RNG = new Random();

        #region Math

        /// <summary>
        /// Clamp a given value between a range
        /// </summary>
        /// <param name="value">Value to clamp</param>
        /// <param name="min">Minimum allowable value</param>
        /// <param name="max">Maximum allowable value</param>
        /// <returns>A value inclusively between lower and upper</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Clamp(float value, float min, float max)
        {
            return (value < max) ? (value > min ? value : min) : max;
        }

        /// <summary>
        /// Clamp a given value between a range
        /// </summary>
        /// <param name="value">Value to clamp</param>
        /// <param name="min">Minimum allowable value</param>
        /// <param name="max">Maximum allowable value</param>
        /// <returns>A value inclusively between lower and upper</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Clamp(double value, double min, double max)
        {
            return (value < max) ? (value > min ? value : min) : max;
        }

        /// <summary>
        /// Clamp a given value between a range
        /// </summary>
        /// <param name="value">Value to clamp</param>
        /// <param name="min">Minimum allowable value</param>
        /// <param name="max">Maximum allowable value</param>
        /// <returns>A value inclusively between lower and upper</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Clamp(int value, int min, int max)
        {
            return (value < max) ? (value > min ? value : min) : max;
        }

        /// <summary>
        /// Round a floating-point value to the nearest integer
        /// </summary>
        /// <param name="val">Floating point number to round</param>
        /// <returns>Integer</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Round(float val)
        {
            return (int)Math.Floor(val + 0.5f);
        }

        /// <summary>
        /// Test if a single precision float is a finite number
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsFinite(float value)
        {
            return !(Single.IsNaN(value) || Single.IsInfinity(value));
        }

        /// <summary>
        /// Test if a double precision float is a finite number
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsFinite(double value)
        {
            return !(Double.IsNaN(value) || Double.IsInfinity(value));
        }

        /// <summary>
        /// Get the distance between two floating-point values
        /// </summary>
        /// <param name="value1">First value</param>
        /// <param name="value2">Second value</param>
        /// <returns>The distance between the two values</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Distance(float value1, float value2)
        {
            return Math.Abs(value1 - value2);
        }

        public static float Hermite(float value1, float tangent1, float value2, float tangent2, float amount)
        {
            if (amount <= 0f)
                return value1;
            if (amount >= 1f)
                return value2;

            // All transformed to double not to lose precission
            // Otherwise, for high numbers of param:amount the result is NaN instead of Infinity
            double v1 = value1, v2 = value2, t1 = tangent1, t2 = tangent2, s = amount;
            double sSquared = s * s;
            double sCubed = sSquared * s;
            
            return (float)((2d * v1 - 2d * v2 + t2 + t1) * sCubed +
                    (3d * v2 - 3d * v1 - 2d * t1 - t2) * sSquared +
                    t1 * s + v1);
        }

        public static double Hermite(double value1, double tangent1, double value2, double tangent2, double amount)
        {
            if (amount <= 0d)
                return value1;
            if (amount >= 1f)
                return value2;

            // All transformed to double not to lose precission
            // Otherwise, for high numbers of param:amount the result is NaN instead of Infinity
            double v1 = value1, v2 = value2, t1 = tangent1, t2 = tangent2, s = amount;
            double sSquared = s * s;
            double sCubed = sSquared * s;

            return (2d * v1 - 2d * v2 + t2 + t1) * sCubed +
                    (3d * v2 - 3d * v1 - 2d * t1 - t2) * sSquared +
                    t1 * s + v1;
        }

        public static float Lerp(float value1, float value2, float amount)
        {
            return value1 + (value2 - value1) * amount;
        }

        public static double Lerp(double value1, double value2, double amount)
        {
            return value1 + (value2 - value1) * amount;
        }

        public static float SmoothStep(float value1, float value2, float amount)
        {
            return Utils.Hermite(value1, 0f, value2, 0f, amount);
        }

        public static double SmoothStep(double value1, double value2, double amount)
        {
            return Utils.Hermite(value1, 0f, value2, 0f, amount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ToDegrees(float radians)
        {
            // This method uses double precission internally,
            // though it returns single float
            // Factor = 180 / pi
            return (float)(radians * 57.295779513082320876798154814105);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ToRadians(float degrees)
        {
            // This method uses double precission internally,
            // though it returns single float
            // Factor = pi / 180
            return (float)(degrees * 0.017453292519943295769236907684886);
        }

        /// <summary>
        /// Compute the MD5 hash for a byte array
        /// </summary>
        /// <param name="data">Byte array to compute the hash for</param>
        /// <returns>MD5 hash of the input data</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] MD5(byte[] data)
        {
            lock (MD5Builder)
                return MD5Builder.ComputeHash(data);
        }

        /// <summary>
        /// Compute the SHA1 hash for a byte array
        /// </summary>
        /// <param name="data">Byte array to compute the hash for</param>
        /// <returns>SHA1 hash of the input data</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] SHA1(byte[] data)
        {
            lock (SHA1Builder)
                return SHA1Builder.ComputeHash(data);
        }

        /// <summary>
        /// Calculate the SHA1 hash of a given string
        /// </summary>
        /// <param name="value">The string to hash</param>
        /// <returns>The SHA1 hash as a string</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string SHA1String(string value)
        {
            StringBuilder digest = new StringBuilder(40);
            byte[] hash = SHA1(Encoding.UTF8.GetBytes(value));

            // Convert the hash to a hex string
            foreach (byte b in hash)
                digest.AppendFormat(Utils.EnUsCulture, "{0:x2}", b);

            return digest.ToString();
        }

        /// <summary>
        /// Compute the SHA256 hash for a byte array
        /// </summary>
        /// <param name="data">Byte array to compute the hash for</param>
        /// <returns>SHA256 hash of the input data</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] SHA256(byte[] data)
        {
            lock (SHA256Builder)
                return SHA256Builder.ComputeHash(data);
        }

        /// <summary>
        /// Calculate the SHA256 hash of a given string
        /// </summary>
        /// <param name="value">The string to hash</param>
        /// <returns>The SHA256 hash as a string</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string SHA256String(string value)
        {
            StringBuilder digest = new StringBuilder(64);
            byte[] hash = SHA256(Encoding.UTF8.GetBytes(value));

            // Convert the hash to a hex string
            foreach (byte b in hash)
                digest.AppendFormat(Utils.EnUsCulture, "{0:x2}", b);

            return digest.ToString();
        }

        /// <summary>
        /// Calculate the MD5 hash of a given string
        /// </summary>
        /// <param name="password">The password to hash</param>
        /// <returns>An MD5 hash in string format, with $1$ prepended</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string MD5(string password)
        {
            StringBuilder digest = new StringBuilder(32);
            byte[] hash = MD5(ASCIIEncoding.Default.GetBytes(password));

            // Convert the hash to a hex string
            foreach (byte b in hash)
                digest.AppendFormat(Utils.EnUsCulture, "{0:x2}", b);

            return "$1$" + digest.ToString();
        }

        /// <summary>
        /// Calculate the MD5 hash of a given string
        /// </summary>
        /// <param name="value">The string to hash</param>
        /// <returns>The MD5 hash as a string</returns>
        public static string MD5String(string value)
        {
            StringBuilder digest = new StringBuilder(32);
            byte[] hash = MD5(Encoding.UTF8.GetBytes(value));

            // Convert the hash to a hex string
            foreach (byte b in hash)
                digest.AppendFormat(Utils.EnUsCulture, "{0:x2}", b);

            return digest.ToString();
        }

        /// <summary>
        /// Generate a random double precision floating point value
        /// </summary>
        /// <returns>Random value of type double</returns>
        public static double RandomDouble()
        {
            lock (RNG)
                return RNG.NextDouble();
        }

        #endregion Math

        #region Platform

        /// <summary>
        /// Get the current running platform
        /// </summary>
        /// <returns>Enumeration of the current platform we are running on</returns>
        public static Platform GetRunningPlatform()
        {
            const string OSX_CHECK_FILE = "/Library/Extensions.kextcache";
            switch(Environment.OSVersion.Platform)
            {
                case PlatformID.WinCE:
                    return Platform.WindowsCE;
                case PlatformID.Unix:
                case PlatformID.MacOSX:
                {
                    if (System.IO.File.Exists(OSX_CHECK_FILE))
                        return Platform.OSX;
                    else
                        return Platform.Linux;
                }
                default:
                    return Platform.Windows;
            }
        }

        /// <summary>
        /// Get the current running runtime
        /// </summary>
        /// <returns>Enumeration of the current runtime we are running on</returns>
        public static Runtime GetRunningRuntime()
        {
            return (Type.GetType("Mono.Runtime") == null) ? Runtime.Windows : Runtime.Mono;
        }

        #endregion Platform
    }
}
