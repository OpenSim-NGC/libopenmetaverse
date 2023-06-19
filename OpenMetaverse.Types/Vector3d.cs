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
using System.Runtime.InteropServices;
using System.Globalization;
using System.Text;
using System.Runtime.CompilerServices;

namespace OpenMetaverse
{
    /// <summary>
    /// A three-dimensional vector with doubleing-point values
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector3d : IComparable<Vector3d>, IEquatable<Vector3d>
    {
        /// <summary>X value</summary>
        public double X;
        /// <summary>Y value</summary>
        public double Y;
        /// <summary>Z value</summary>
        public double Z;

        #region Constructors

        public Vector3d(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Vector3d(double value)
        {
            X = value;
            Y = value;
            Z = value;
        }

        /// <summary>
        /// Constructor, builds a vector from a byte array
        /// </summary>
        /// <param name="byteArray">Byte array containing three eight-byte doubles</param>
        /// <param name="pos">Beginning position in the byte array</param>
        public Vector3d(byte[] byteArray, int pos)
        {
            X = Y = Z = 0d;
            FromBytes(byteArray, pos);
        }

        public Vector3d(Vector3 vector)
        {
            X = vector.X;
            Y = vector.Y;
            Z = vector.Z;
        }

        public Vector3d(Vector3d vector)
        {
            X = vector.X;
            Y = vector.Y;
            Z = vector.Z;
        }

        #endregion Constructors

        #region Public Methods

        public double Length()
        {
            return Math.Sqrt(DistanceSquared(this, Zero));
        }

        public double LengthSquared()
        {
            return DistanceSquared(this, Zero);
        }

        public void Normalize()
        {
            this = Normalize(this);
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
        public bool ApproxEquals(Vector3d vec, double tolerance)
        {
            Vector3d diff = this - vec;
            return (diff.LengthSquared() <= tolerance * tolerance);
        }

        /// <summary>
        /// IComparable.CompareTo implementation
        /// </summary>
        public int CompareTo(Vector3d vector)
        {
            return this.Length().CompareTo(vector.Length());
        }

        /// <summary>
        /// Test if this vector is composed of all finite numbers
        /// </summary>
        public bool IsFinite()
        {
            return (Utils.IsFinite(X) && Utils.IsFinite(Y) && Utils.IsFinite(Z));
        }

        /// <summary>
        /// Builds a vector from a byte array
        /// </summary>
        /// <param name="byteArray">Byte array containing a 24 byte vector</param>
        /// <param name="pos">Beginning position in the byte array</param>
        public void FromBytes(byte[] byteArray, int pos)
        {
            X = Utils.BytesToDouble(byteArray, pos);
            Y = Utils.BytesToDouble(byteArray, pos + 8);
            Z = Utils.BytesToDouble(byteArray, pos + 16);
        }

        /// <summary>
        /// Returns the raw bytes for this vector
        /// </summary>
        /// <returns>A 24 byte array containing X, Y, and Z</returns>
        public byte[] GetBytes()
        {
            byte[] byteArray = new byte[24];
            Utils.DoubleToBytes(X, byteArray, 0);
            Utils.DoubleToBytes(Y, byteArray, 8);
            Utils.DoubleToBytes(Z, byteArray, 16);
            return byteArray;
        }

        /// <summary>
        /// Writes the raw bytes for this vector to a byte array
        /// </summary>
        /// <param name="dest">Destination byte array</param>
        /// <param name="pos">Position in the destination array to start
        /// writing. Must be at least 24 bytes before the end of the array</param>
        public void ToBytes(byte[] dest, int pos)
        {
            Utils.DoubleToBytes(X, dest, pos);
            Utils.DoubleToBytes(Y, dest, pos + 8);
            Utils.DoubleToBytes(Z, dest, pos + 16);
        }

        #endregion Public Methods

        #region Static Methods

        public static Vector3d Add(Vector3d value1, Vector3d value2)
        {
            value1.X += value2.X;
            value1.Y += value2.Y;
            value1.Z += value2.Z;
            return value1;
        }

        public static Vector3d Clamp(Vector3 value1, double min, double max)
        {
            return new Vector3d(
                Utils.Clamp(value1.X, min, max),
                Utils.Clamp(value1.Y, min, max),
                Utils.Clamp(value1.Z, min, max));
        }

        public static Vector3d Clamp(Vector3d value1, Vector3d min, Vector3d max)
        {
            return new Vector3d(
                Utils.Clamp(value1.X, min.X, max.X),
                Utils.Clamp(value1.Y, min.Y, max.Y),
                Utils.Clamp(value1.Z, min.Z, max.Z));
        }

        public static Vector3d Cross(Vector3d value1, Vector3d value2)
        {
            return new Vector3d(
                value1.Y * value2.Z - value2.Y * value1.Z,
                value1.Z * value2.X - value2.Z * value1.X,
                value1.X * value2.Y - value2.X * value1.Y);
        }

        public static double Distance(Vector3d value1, Vector3d value2)
        {
            return Math.Sqrt(DistanceSquared(value1, value2));
        }

        public static double DistanceSquared(Vector3d value1, Vector3d value2)
        {
            return
                (value1.X - value2.X) * (value1.X - value2.X) +
                (value1.Y - value2.Y) * (value1.Y - value2.Y) +
                (value1.Z - value2.Z) * (value1.Z - value2.Z);
        }

        public static Vector3d Divide(Vector3d value1, Vector3d value2)
        {
            value1.X /= value2.X;
            value1.Y /= value2.Y;
            value1.Z /= value2.Z;
            return value1;
        }

        public static Vector3d Divide(Vector3d value1, double value2)
        {
            double factor = 1d / value2;
            value1.X *= factor;
            value1.Y *= factor;
            value1.Z *= factor;
            return value1;
        }

        public static double Dot(Vector3d value1, Vector3d value2)
        {
            return value1.X * value2.X + value1.Y * value2.Y + value1.Z * value2.Z;
        }

        public static Vector3d Lerp(Vector3d value1, Vector3d value2, double amount)
        {
            return new Vector3d(
                Utils.Lerp(value1.X, value2.X, amount),
                Utils.Lerp(value1.Y, value2.Y, amount),
                Utils.Lerp(value1.Z, value2.Z, amount));
        }

        public static Vector3d Max(Vector3d value1, Vector3d value2)
        {
            return new Vector3d(
                Math.Max(value1.X, value2.X),
                Math.Max(value1.Y, value2.Y),
                Math.Max(value1.Z, value2.Z));
        }

        public static Vector3d Min(Vector3d value1, Vector3d value2)
        {
            return new Vector3d(
                Math.Min(value1.X, value2.X),
                Math.Min(value1.Y, value2.Y),
                Math.Min(value1.Z, value2.Z));
        }

        public static Vector3d Multiply(Vector3d value1, Vector3d value2)
        {
            value1.X *= value2.X;
            value1.Y *= value2.Y;
            value1.Z *= value2.Z;
            return value1;
        }

        public static Vector3d Multiply(Vector3d value1, double scaleFactor)
        {
            value1.X *= scaleFactor;
            value1.Y *= scaleFactor;
            value1.Z *= scaleFactor;
            return value1;
        }

        public static Vector3d Negate(Vector3d value)
        {
            value.X = -value.X;
            value.Y = -value.Y;
            value.Z = -value.Z;
            return value;
        }

        public static Vector3d Normalize(Vector3d value)
        {
            double factor = Distance(value, Zero);
            if (factor > Double.Epsilon)
            {
                factor = 1d / factor;
                return new Vector3d(value.X * factor, value.Y * factor, value.Z * factor);
            }
            return Vector3.Zero;
        }

        /// <summary>
        /// Parse a vector from a string
        /// </summary>
        /// <param name="val">A string representation of a 3D vector, enclosed 
        /// in arrow brackets and separated by commas</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Vector3d Parse(string val)
        {
            return Parse(val.AsSpan());
        }

        public static unsafe Vector3d Parse(ReadOnlySpan<char> sp)
        {
            if (sp.Length < 7)
                throw new FormatException("Invalid Vector3");

            int start = 0;
            fixed (char* p = sp)
            {
                while (start < sp.Length)
                {
                    if (p[start++] == '<')
                        break;
                }
                if (start > sp.Length - 6)
                    throw new FormatException("Invalid Vector3");

                int comma1 = start + 1;
                while (comma1 < sp.Length)
                {
                    if (p[comma1] == ',')
                        break;
                    comma1++;
                }
                if (comma1 > sp.Length - 5)
                    throw new FormatException("Invalid Vector3");

                if (!double.TryParse(sp[start..comma1], NumberStyles.Float, Utils.EnUsCulture, out double x))
                    throw new FormatException("Invalid Vector3");

                comma1++;
                start = comma1;
                comma1++;
                while (comma1 < sp.Length)
                {
                    if (p[comma1] == ',')
                        break;
                    comma1++;
                }
                if (comma1 > sp.Length - 3)
                    throw new FormatException("Invalid Vector3");
                if (!double.TryParse(sp[start..comma1], NumberStyles.Float, Utils.EnUsCulture, out double y))
                    throw new FormatException("Invalid Vector3");

                comma1++;
                start = comma1;
                comma1++;
                while (comma1 < sp.Length)
                {
                    if (p[comma1] == '>')
                        break;
                    comma1++;
                }
                if (comma1 >= sp.Length)
                    throw new FormatException("Invalid Vector3");

                if (!double.TryParse(sp[start..comma1], NumberStyles.Float, Utils.EnUsCulture, out double z))
                    throw new FormatException("Invalid Vector3");
                return new Vector3d(x, y, z);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static bool TryParse(string val, out Vector3d result)
        {
            return TryParse(val.AsSpan(), out result);
        }

        public unsafe static bool TryParse(ReadOnlySpan<char> sp, out Vector3d result)
        {
            if (sp.Length < 7)
            {
                result = Zero;
                return false;
            }

            int start = 0;
            fixed (char* p = sp)
            {
                while (start < sp.Length)
                {
                    if (p[start++] == '<')
                        break;
                }
                if (start > sp.Length - 6)
                {
                    result = Zero;
                    return false;
                }

                int comma1 = start + 1;
                while (comma1 < sp.Length)
                {
                    if (p[comma1] == ',')
                        break;
                    comma1++;
                }
                if (comma1 > sp.Length - 5)
                {
                    result = Zero;
                    return false;
                }

                if (!double.TryParse(sp[start..comma1], NumberStyles.Float, Utils.EnUsCulture, out double x))
                {
                    result = Zero;
                    return false;
                }

                comma1++;
                start = comma1;
                comma1++;
                while (comma1 < sp.Length)
                {
                    if (p[comma1] == ',')
                        break;
                    comma1++;
                }
                if (comma1 > sp.Length - 3)
                {
                    result = Zero;
                    return false;
                }
                if (!double.TryParse(sp[start..comma1], NumberStyles.Float, Utils.EnUsCulture, out double y))
                {
                    result = Zero;
                    return false;
                }

                comma1++;
                start = comma1;
                comma1++;
                while (comma1 < sp.Length)
                {
                    if (p[comma1] == '>')
                        break;
                    comma1++;
                }
                if (comma1 >= sp.Length)
                {
                    result = Zero;
                    return false;
                }

                if (!double.TryParse(sp[start..comma1], NumberStyles.Float, Utils.EnUsCulture, out double z))
                {
                    result = Zero;
                    return false;
                }
                result = new Vector3d(x, y, z);
                return true;
            }
        }

        /// <summary>
        /// Interpolates between two vectors using a cubic equation
        /// </summary>
        public static Vector3d SmoothStep(Vector3d value1, Vector3d value2, double amount)
        {
            return new Vector3d(
                Utils.SmoothStep(value1.X, value2.X, amount),
                Utils.SmoothStep(value1.Y, value2.Y, amount),
                Utils.SmoothStep(value1.Z, value2.Z, amount));
        }

        public static Vector3d Subtract(Vector3d value1, Vector3d value2)
        {
            value1.X -= value2.X;
            value1.Y -= value2.Y;
            value1.Z -= value2.Z;
            return value1;
        }

        #endregion Static Methods

        #region Overrides

        public override bool Equals(object obj)
        {
            return (obj is Vector3d) ? this == (Vector3d)obj : false;
        }

        public bool Equals(Vector3d other)
        {
            return this == other;
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();
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
            sb.Append(", ");
            sb.Append(Z.ToString(Utils.EnUsCulture));
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
            CultureInfo enUs = new("en-us");
            enUs.NumberFormat.NumberDecimalDigits = 3;

            StringBuilder sb = new();
            sb.Append(X.ToString(enUs));
            sb.Append(' ');
            sb.Append(Y.ToString(enUs));
            sb.Append(' ');
            sb.Append(Z.ToString(enUs));
            return sb.ToString();
        }

        #endregion Overrides

        #region Operators

        public static bool operator ==(Vector3d value1, Vector3d value2)
        {
            return value1.X == value2.X
                && value1.Y == value2.Y
                && value1.Z == value2.Z;
        }

        public static bool operator !=(Vector3d value1, Vector3d value2)
        {
            return !(value1 == value2);
        }

        public static Vector3d operator +(Vector3d value1, Vector3d value2)
        {
            value1.X += value2.X;
            value1.Y += value2.Y;
            value1.Z += value2.Z;
            return value1;
        }

        public static Vector3d operator -(Vector3d value)
        {
            value.X = -value.X;
            value.Y = -value.Y;
            value.Z = -value.Z;
            return value;
        }

        public static Vector3d operator -(Vector3d value1, Vector3d value2)
        {
            value1.X -= value2.X;
            value1.Y -= value2.Y;
            value1.Z -= value2.Z;
            return value1;
        }

        public static Vector3d operator *(Vector3d value1, Vector3d value2)
        {
            value1.X *= value2.X;
            value1.Y *= value2.Y;
            value1.Z *= value2.Z;
            return value1;
        }

        public static Vector3d operator *(Vector3d value, double scaleFactor)
        {
            value.X *= scaleFactor;
            value.Y *= scaleFactor;
            value.Z *= scaleFactor;
            return value;
        }

        public static Vector3d operator /(Vector3d value1, Vector3d value2)
        {
            value1.X /= value2.X;
            value1.Y /= value2.Y;
            value1.Z /= value2.Z;
            return value1;
        }

        public static Vector3d operator /(Vector3d value, double divider)
        {
            double factor = 1d / divider;
            value.X *= factor;
            value.Y *= factor;
            value.Z *= factor;
            return value;
        }

        /// <summary>
        /// Cross product between two vectors
        /// </summary>
        public static Vector3d operator %(Vector3d value1, Vector3d value2)
        {
            return Cross(value1, value2);
        }

        /// <summary>
        /// Implicit casting for Vector3 > Vector3d
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator Vector3d(Vector3 value)
        {
            return new Vector3d(value);
        }

        #endregion Operators

        /// <summary>A vector with a value of 0,0,0</summary>
        public readonly static Vector3d Zero = new Vector3d();
        /// <summary>A vector with a value of 1,1,1</summary>
        public readonly static Vector3d One = new Vector3d();
        /// <summary>A unit vector facing forward (X axis), value of 1,0,0</summary>
        public readonly static Vector3d UnitX = new Vector3d(1d, 0d, 0d);
        /// <summary>A unit vector facing left (Y axis), value of 0,1,0</summary>
        public readonly static Vector3d UnitY = new Vector3d(0d, 1d, 0d);
        /// <summary>A unit vector facing up (Z axis), value of 0,0,1</summary>
        public readonly static Vector3d UnitZ = new Vector3d(0d, 0d, 1d);
    }
}
