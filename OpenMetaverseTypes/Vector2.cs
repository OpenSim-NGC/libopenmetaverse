/*
 * Copyright (c) 2006-2016, openmetaverse.co
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
using System.Runtime.InteropServices;
using System.Globalization;

namespace OpenMetaverse
{
    /// <summary>
    /// A two-dimensional vector with floating-point values
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector2 : IComparable<Vector2>, IEquatable<Vector2>
    {
        /// <summary>X value</summary>
        public float X;
        /// <summary>Y value</summary>
        public float Y;

        #region Constructors

        public Vector2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public Vector2(float value)
        {
            X = value;
            Y = value;
        }

        public Vector2(Vector2 vector)
        {
            X = vector.X;
            Y = vector.Y;
        }

        #endregion Constructors

        #region Public Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Abs()
        {
            if (X < 0f)
                X = -X;
            if (Y < 0f)
                Y = -Y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Min(Vector2 v)
        {
            if (v.X < X) X = v.X;
            if (v.Y < Y) Y = v.Y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Max(Vector2 v)
        {
            if (v.X > X) X = v.X;
            if (v.Y > Y) Y = v.Y;
        }

        /// <summary>
        /// Test if this vector is equal to another vector, within a given
        /// tolerance range
        /// </summary>
        /// <param name="vec">Vector to test against</param>
        /// <param name="tolerance">The acceptable magnitude of difference
        /// between the two vectors</param>
        /// <returns>True if the magnitude of difference between the two vectors
        /// is less than the given tolerance, otherwise false</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ApproxEquals(Vector2 vec, float tolerance)
        {
            return Utils.ApproxEqual(X, vec.X, tolerance) &&
                    Utils.ApproxEqual(Y, vec.Y, tolerance);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ApproxEquals(Vector2 vec)
        {
            return Utils.ApproxEqual(X, vec.X) &&
                    Utils.ApproxEqual(Y, vec.Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ApproxZero()
        {
            if (!Utils.ApproxZero(X))
                return false;
            if (!Utils.ApproxZero(Y))
                return false;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ApproxZero(float tolerance)
        {
            if (!Utils.ApproxZero(X, tolerance))
                return false;
            if (!Utils.ApproxZero(Y, tolerance))
                return false;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsZero()
        {
            if (X != 0)
                return false;
            if (Y != 0)
                return false;
            return true;
        }

        /// <summary>
        /// Test if this vector is composed of all finite numbers
        /// </summary>
        public bool IsFinite()
        {
            return Utils.IsFinite(X) && Utils.IsFinite(Y);
        }

        /// <summary>
        /// IComparable.CompareTo implementation
        /// </summary>
        public int CompareTo(Vector2 vector)
        {
            return LengthSquared().CompareTo(vector.LengthSquared());
        }

        /// <summary>
        /// Builds a vector from a byte array
        /// </summary>
        /// <param name="byteArray">Byte array containing two four-byte floats</param>
        /// <param name="pos">Beginning position in the byte array</param>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void FromBytes(byte[] byteArray, int pos)
        {
            X = Utils.BytesToFloatSafepos(byteArray, pos);
            Y = Utils.BytesToFloatSafepos(byteArray, pos + 4);
        }

        /// <summary>
        /// Returns the raw bytes for this vector
        /// </summary>
        /// <returns>An eight-byte array containing X and Y</returns>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public byte[] GetBytes()
        {
            byte[] dest = new byte[8];
            Utils.FloatToBytesSafepos(X, dest, 0);
            Utils.FloatToBytesSafepos(Y, dest, 4);
            return dest;
        }

        /// <summary>
        /// Writes the raw bytes for this vector to a byte array
        /// </summary>
        /// <param name="dest">Destination byte array</param>
        /// <param name="pos">Position in the destination array to start
        /// writing. Must be at least 8 bytes before the end of the array</param>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void ToBytes(byte[] dest, int pos)
        {
            Utils.FloatToBytesSafepos(X, dest, pos);
            Utils.FloatToBytesSafepos(Y, dest, pos + 4);
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public float Length()
        {
            return (float)Math.Sqrt(X * X + Y * Y);
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public float LengthSquared()
        {
            return X * X + Y * Y;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void Normalize()
        {
            float factor = LengthSquared();
            if (factor > 1e-6)
            {
                factor = 1f / (float)Math.Sqrt(factor);
                X *= factor;
                Y *= factor;
            }
            else
            {
                X = 0f;
                Y = 0f;
            }
        }

        #endregion Public Methods

        #region Static Methods

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Vector2 Add(Vector2 value1, Vector2 value2)
        {
            return new Vector2(value1.X + value2.X, value1.Y + value2.Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Clamp(Vector3 value1, float min, float max)
        {
            return new Vector2(
                Utils.Clamp(value1.X, min, max),
                Utils.Clamp(value1.Y, min, max));
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Vector2 Clamp(Vector2 value1, Vector2 min, Vector2 max)
        {
            return new Vector2(
                Utils.Clamp(value1.X, min.X, max.X),
                Utils.Clamp(value1.Y, min.Y, max.Y));
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static float Distance(Vector2 value1, Vector2 value2)
        {
            return (float)Math.Sqrt(DistanceSquared(value1, value2));
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static float DistanceSquared(Vector2 value1, Vector2 value2)
        {
            return
                (value1.X - value2.X) * (value1.X - value2.X) +
                (value1.Y - value2.Y) * (value1.Y - value2.Y);
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Vector2 Divide(Vector2 value1, Vector2 value2)
        {
            return new Vector2(value1.X / value2.X, value1.Y / value2.Y);
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Vector2 Divide(Vector2 value1, float divider)
        {
            float factor = 1 / divider;
            return new Vector2(value1.X * factor, value1.Y * factor);
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static float Dot(Vector2 value1, Vector2 value2)
        {
            return (value1.X * value2.X) + (value1.Y * value2.Y);
        }

        public static Vector2 Lerp(Vector2 value1, Vector2 value2, float amount)
        {
            return new Vector2(
                Utils.Lerp(value1.X, value2.X, amount),
                Utils.Lerp(value1.Y, value2.Y, amount));
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Vector2 Max(Vector2 value1, Vector2 value2)
        {
            return new Vector2(
                Math.Max(value1.X, value2.X),
                Math.Max(value1.Y, value2.Y));
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Vector2 Min(Vector2 value1, Vector2 value2)
        {
            return new Vector2(
                Math.Min(value1.X, value2.X),
                Math.Min(value1.Y, value2.Y));
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Vector2 Multiply(Vector2 value1, Vector2 value2)
        {
            return new Vector2(value1.X * value2.X, value1.Y * value2.Y);
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Vector2 Multiply(Vector2 value1, float scaleFactor)
        {
            return new Vector2(value1.X * scaleFactor, value1.Y * scaleFactor);
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Vector2 Negate(Vector2 value)
        {
            return new Vector2(-value.X, -value.Y);
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Vector2 Normalize(Vector2 value)
        {
            float factor = value.LengthSquared();
            if (factor > 1e-6)
            {
                factor = 1f / (float)Math.Sqrt(factor);
                return new Vector2(value.X * factor, value.Y * factor);
            }
            return new Vector2();
        }

        /// <summary>
        /// Parse a vector from a string
        /// </summary>
        /// <param name="val">A string representation of a 2D vector, enclosed 
        /// in arrow brackets and separated by commas</param>
        public static Vector2 Parse(string val)
        {
            char[] splitChar = { ',' };
            string[] split = val.Replace("<", String.Empty).Replace(">", String.Empty).Split(splitChar);
            return new Vector2(
                float.Parse(split[0].Trim(), Utils.EnUsCulture),
                float.Parse(split[1].Trim(), Utils.EnUsCulture));
        }

        public static bool TryParse(string val, out Vector2 result)
        {
            try
            {
                result = Parse(val);
                return true;
            }
            catch (Exception)
            {
                result = new Vector2();
                return false;
            }
        }

        /// <summary>
        /// Interpolates between two vectors using a cubic equation
        /// </summary>
        public static Vector2 SmoothStep(Vector2 value1, Vector2 value2, float amount)
        {
            return new Vector2(
                Utils.SmoothStep(value1.X, value2.X, amount),
                Utils.SmoothStep(value1.Y, value2.Y, amount));
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Vector2 Subtract(Vector2 value1, Vector2 value2)
        {
            return new Vector2(value1.X - value2.X, value1.Y - value2.Y);
        }

        public static Vector2 Transform(Vector2 position, Matrix4 matrix)
        {
            return new Vector2(
                (position.X * matrix.M11) + (position.Y * matrix.M21) + matrix.M41,
                (position.X * matrix.M12) + (position.Y * matrix.M22) + matrix.M42);
        }

        public static Vector2 TransformNormal(Vector2 position, Matrix4 matrix)
        {
            return new Vector2(
                (position.X * matrix.M11) + (position.Y * matrix.M21),
                (position.X * matrix.M12) + (position.Y * matrix.M22));
        }

        #endregion Static Methods

        #region Overrides

        public override bool Equals(object obj)
        {
            if (!(obj is Vector2))
                return false;

            Vector2 other = (Vector2)obj;
            if (X != other.X)
                return false;
            if (Y != other.Y)
                return false;
            return true;
        }

        public bool Equals(Vector2 other)
        {
            if (X != other.X)
                return false;
            if (Y != other.Y)
                return false;
            return true;
        }

        public override int GetHashCode()
        {
            int hash = X.GetHashCode();
            hash = Utils.CombineHash(hash, Y.GetHashCode());
            return hash;
        }

        /// <summary>
        /// Get a formatted string representation of the vector
        /// </summary>
        /// <returns>A string representation of the vector</returns>
        public override string ToString()
        {
            return String.Format(Utils.EnUsCulture, "<{0}, {1}>", X, Y);
        }

        /// <summary>
        /// Get a string representation of the vector elements with up to three
        /// decimal digits and separated by spaces only
        /// </summary>
        /// <returns>Raw string representation of the vector</returns>
        public string ToRawString()
        {
            CultureInfo enUs = new CultureInfo("en-us");
            enUs.NumberFormat.NumberDecimalDigits = 3;

            return String.Format(enUs, "{0} {1}", X, Y);
        }

        #endregion Overrides

        #region Operators

        public static bool operator ==(Vector2 value1, Vector2 value2)
        {
            if (value1.X != value2.X)
                return false;
            if (value1.Y != value2.Y)
                return false;
            return true;
        }

        public static bool operator !=(Vector2 value1, Vector2 value2)
        {
            if (value1.X != value2.X)
                return true;
            if (value1.Y != value2.Y)
                return true;
            return false;
        }

        public static Vector2 operator +(Vector2 value1, Vector2 value2)
        {
            return new Vector2(value1.X + value2.X, value1.Y + value2.Y);
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Vector2 operator -(Vector2 value)
        {
            return new Vector2(-value.X, -value.Y);
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Vector2 operator -(Vector2 value1, Vector2 value2)
        {
            return new Vector2(value1.X - value2.X, value1.Y - value2.Y);
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Vector2 operator *(Vector2 value1, Vector2 value2)
        {
            return new Vector2(value1.X * value2.X, value1.Y * value2.Y);
        }


        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Vector2 operator *(Vector2 value, float scaleFactor)
        {
            return new Vector2(value.X * scaleFactor, value.Y * scaleFactor);
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Vector2 operator /(Vector2 value1, Vector2 value2)
        {
            return new Vector2(value1.X / value2.X, value1.Y / value2.Y);
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Vector2 operator /(Vector2 value, float divider)
        {
            float factor = 1 / divider;
            return new Vector2(value.X * factor, value.Y * factor);
        }

        #endregion Operators

        /// <summary>A vector with a value of 0,0</summary>
        public readonly static Vector2 Zero = new Vector2();
        /// <summary>A vector with a value of 1,1</summary>
        public readonly static Vector2 One = new Vector2(1f, 1f);
        /// <summary>A vector with a value of 1,0</summary>
        public readonly static Vector2 UnitX = new Vector2(1f, 0f);
        /// <summary>A vector with a value of 0,1</summary>
        public readonly static Vector2 UnitY = new Vector2(0f, 1f);
        public readonly static Vector2 MinValue = new Vector2(float.MinValue, float.MinValue);
        public readonly static Vector2 MaxValue = new Vector2(float.MaxValue, float.MaxValue);
    }
}
