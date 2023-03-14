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
using System.Text;

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(Vector3 v)
        {
            X += v.X;
            Y += v.Y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Sub(Vector3 v)
        {
            X -= v.X;
            Y -= v.Y;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsNotZero()
        {
            if (X != 0)
                return true;
            if (Y != 0)
                return true;
            return false;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Dot(Vector3 value2)
        {
            return (X * value2.X) + (Y * value2.Y);
        }

        /// <summary>
        /// Builds a vector from a byte array
        /// </summary>
        /// <param name="byteArray">Byte array containing two four-byte floats</param>
        /// <param name="pos">Beginning position in the byte array</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromBytes(byte[] byteArray, int pos)
        {
            X = Utils.BytesToFloatSafepos(byteArray, pos);
            Y = Utils.BytesToFloatSafepos(byteArray, pos + 4);
        }

        /// <summary>
        /// Returns the raw bytes for this vector
        /// </summary>
        /// <returns>An eight-byte array containing X and Y</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void ToBytes(byte[] dest, int pos)
        {
            if (Utils.CanDirectCopyLE)
            {
                fixed (byte* d = &dest[0])
                    *(Vector2*)(d + pos) = this;
            }
            else
            {
                Utils.FloatToBytesSafepos(X, dest, pos);
                Utils.FloatToBytesSafepos(Y, dest, pos + 4);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void ToBytes(byte* dest)
        {
            if (Utils.CanDirectCopyLE)
                *(Vector2*)(dest) = this;
            else
            {
                Utils.FloatToBytes(X, dest);
                Utils.FloatToBytes(Y, dest + 4);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Length()
        {
            return MathF.Sqrt(X * X + Y * Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float LengthSquared()
        {
            return X * X + Y * Y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Normalize()
        {
            float factor = LengthSquared();
            if (factor > 1e-6)
            {
                factor = 1f / MathF.Sqrt(factor);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Clamp(Vector2 value1, Vector2 min, Vector2 max)
        {
            return new Vector2(
                Utils.Clamp(value1.X, min.X, max.X),
                Utils.Clamp(value1.Y, min.Y, max.Y));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Distance(Vector2 value1, Vector2 value2)
        {
            return MathF.Sqrt(DistanceSquared(value1, value2));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DistanceSquared(Vector2 value1, Vector2 value2)
        {
            return
                (value1.X - value2.X) * (value1.X - value2.X) +
                (value1.Y - value2.Y) * (value1.Y - value2.Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Divide(Vector2 value1, Vector2 value2)
        {
            return new Vector2(value1.X / value2.X, value1.Y / value2.Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Divide(Vector2 value1, float divider)
        {
            float factor = 1 / divider;
            return new Vector2(value1.X * factor, value1.Y * factor);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Max(Vector2 value1, Vector2 value2)
        {
            return new Vector2(
                MathF.Max(value1.X, value2.X),
                MathF.Max(value1.Y, value2.Y));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Min(Vector2 value1, Vector2 value2)
        {
            return new Vector2(
                MathF.Min(value1.X, value2.X),
                MathF.Min(value1.Y, value2.Y));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Multiply(Vector2 value1, Vector2 value2)
        {
            return new Vector2(value1.X * value2.X, value1.Y * value2.Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Multiply(Vector2 value1, float scaleFactor)
        {
            return new Vector2(value1.X * scaleFactor, value1.Y * scaleFactor);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Negate(Vector2 value)
        {
            return new Vector2(-value.X, -value.Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Normalize(Vector2 value)
        {
            float factor = value.LengthSquared();
            if (factor > 1e-6)
            {
                factor = 1f / MathF.Sqrt(factor);
                return new Vector2(value.X * factor, value.Y * factor);
            }
            return new Vector2();
        }

        /// <summary>
        /// Parse a vector from a string
        /// </summary>
        /// <param name="val">A string representation of a 2D vector, enclosed 
        /// in arrow brackets and separated by commas</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Vector2 Parse(string val)
        {
            return Parse(val.AsSpan());
        }

        public static Vector2 Parse(ReadOnlySpan<char> sp)
        {
            if (sp.Length < 3)
                throw new FormatException("Invalid Vector2");

            int start = 0;
            int comma = 0;
            char c;

            do
            {
                c = Unsafe.Add(ref MemoryMarshal.GetReference(sp), comma);
                if (c == ',' || c == '<')
                    break;
            }
            while (++comma < sp.Length);

            if (c == '<')
            {
                start = ++comma;
                while (++comma < sp.Length)
                {
                    if (Unsafe.Add(ref MemoryMarshal.GetReference(sp), comma) == ',')
                        break;
                }
            }
            if (comma > sp.Length - 1)
                throw new FormatException("Invalid Vector2");

            if (!float.TryParse(sp[start..comma], NumberStyles.Float, Utils.EnUsCulture, out float x))
                throw new FormatException("Invalid Vector2");

            start = ++comma;
            while (++comma < sp.Length)
            {
                if (Unsafe.Add(ref MemoryMarshal.GetReference(sp), comma) == '>')
                    break;
            }
            if (!float.TryParse(sp[start..comma], NumberStyles.Float, Utils.EnUsCulture, out float y))
                throw new FormatException("Invalid Vector2");

            return new Vector2(x, y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryParse(string val, out Vector2 result)
        {
            return TryParse(val.AsSpan(), out result);
        }
        public static bool TryParse(ReadOnlySpan<char> sp, out Vector2 result)
        {
            if (sp.Length < 3)
            {
                result = Zero;
                return false;
            }

            int start = 0;
            int comma = 0;
            char c;
            do
            {
                c = Unsafe.Add(ref MemoryMarshal.GetReference(sp), comma);
                if (c == ',' || c == '<')
                    break;
            }
            while (++comma < sp.Length);

            if (c == '<')
            {
                start = ++comma;
                while (++comma < sp.Length)
                {
                    if (Unsafe.Add(ref MemoryMarshal.GetReference(sp), comma) == ',')
                        break;
                }
            }
            if (comma > sp.Length - 1)
            {
                result = Zero;
                return false;
            }

            if (!float.TryParse(sp[start..comma], NumberStyles.Float, Utils.EnUsCulture, out float x))
            {
                result = Zero;
                return false;
            }

            start = ++comma;
            while (++comma < sp.Length)
            {
                c = Unsafe.Add(ref MemoryMarshal.GetReference(sp), comma);
                if (c == ' ' || c == '>')
                    break;
            }
            if (!float.TryParse(sp[start..comma], NumberStyles.Float, Utils.EnUsCulture, out float y))
            {
                result = Zero;
                return false;
            }

            result = new Vector2(x, y);
            return true;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Vector2 other)
        {
            if (X != other.X)
                return false;
            if (Y != other.Y)
                return false;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool NotEqual(Vector3 other)
        {
            if (X != other.X)
                return true;
            if (Y != other.Y)
                return true;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
            StringBuilder sb = new();
            sb.Append('<');
            sb.Append(X.ToString(Utils.EnUsCulture));
            sb.Append(", ");
            sb.Append(Y.ToString(Utils.EnUsCulture));
            sb.Append('>');
            return sb.ToString();
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

            StringBuilder sb = new();
            sb.Append(X.ToString(enUs));
            sb.Append(' ');
            sb.Append(Y.ToString(enUs));
            sb.Append(' ');
            return sb.ToString();
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
