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

namespace OpenMetaverse
{
    /// <summary>
    /// A three-dimensional vector with floating-point values
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector3 : IComparable<Vector3>, IEquatable<Vector3>
    {
        /// <summary>X value</summary>
        public float X;
        /// <summary>Y value</summary>
        public float Y;
        /// <summary>Z value</summary>
        public float Z;

        #region Constructors

        public Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Vector3(float value)
        {
            X = value;
            Y = value;
            Z = value;
        }

        public Vector3(Vector2 value, float z)
        {
            X = value.X;
            Y = value.Y;
            Z = z;
        }

        public Vector3(Vector3d vector)
        {
            X = (float)vector.X;
            Y = (float)vector.Y;
            Z = (float)vector.Z;
        }

        /// <summary>
        /// Constructor, builds a vector from a byte array
        /// </summary>
        /// <param name="byteArray">Byte array containing three four-byte floats</param>
        /// <param name="pos">Beginning position in the byte array</param>
        public Vector3(byte[] byteArray, int pos)
        {
            X = Y = Z = 0f;
            FromBytes(byteArray, pos);
        }

        public Vector3(Vector3 vector)
        {
            X = vector.X;
            Y = vector.Y;
            Z = vector.Z;
        }

        #endregion Constructors

        #region Public Methods

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public float Length()
        {
            return (float)Math.Sqrt((X * X) + (Y * Y) + (Z * Z));
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public float LengthSquared()
        {
            return (X * X) + (Y * Y) + (Z * Z);
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void Normalize()
        {
            float factor = LengthSquared();
            if (factor > 1e-6f)
            {
                factor = 1f / (float)Math.Sqrt(factor);
                X *= factor;
                Y *= factor;
                Z *= factor;
            }
            else
            {
                X = 0f;
                Y = 0f;
                Z = 0f;
            }
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
        public bool ApproxEquals(Vector3 vec, float tolerance)
        {
            return Utils.ApproxEqual(X, vec.X, tolerance) &&
                    Utils.ApproxEqual(Y, vec.Y, tolerance) &&
                    Utils.ApproxEqual(Z, vec.Z, tolerance);
        }

        /// <summary>
        /// IComparable.CompareTo implementation
        /// </summary>
        public int CompareTo(Vector3 vector)
        {
            return Length().CompareTo(vector.Length());
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
        /// <param name="byteArray">Byte array containing a 12 byte vector</param>
        /// <param name="pos">Beginning position in the byte array</param>
        public void FromBytes(byte[] byteArray, int pos)
        {
            X = Utils.BytesToFloatSafepos(byteArray, pos);
            Y = Utils.BytesToFloatSafepos(byteArray, pos + 4);
            Z = Utils.BytesToFloatSafepos(byteArray, pos + 8);
        }

        /// <summary>
        /// Returns the raw bytes for this vector
        /// </summary>
        /// <returns>A 12 byte array containing X, Y, and Z</returns>
        public byte[] GetBytes()
        {
            byte[] dest = new byte[12];
            Utils.FloatToBytesSafepos(X, dest, 0);
            Utils.FloatToBytesSafepos(Y, dest, 4);
            Utils.FloatToBytesSafepos(Z, dest, 8);
            return dest;
        }

        /// <summary>
        /// Writes the raw bytes for this vector to a byte array
        /// </summary>
        /// <param name="dest">Destination byte array</param>
        /// <param name="pos">Position in the destination array to start
        /// writing. Must be at least 12 bytes before the end of the array</param>
        public void ToBytes(byte[] dest, int pos)
        {
            Utils.FloatToBytesSafepos(X, dest, pos);
            Utils.FloatToBytesSafepos(Y, dest, pos + 4);
            Utils.FloatToBytesSafepos(Z, dest, pos + 8);
        }

        #endregion Public Methods

        #region Static Methods
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Vector3 Add(Vector3 value1, Vector3 value2)
        {
            value1.X += value2.X;
            value1.Y += value2.Y;
            value1.Z += value2.Z;
            return value1;
        }

        public static Vector3 Clamp(Vector3 value1, Vector3 min, Vector3 max)
        {
            return new Vector3(
                Utils.Clamp(value1.X, min.X, max.X),
                Utils.Clamp(value1.Y, min.Y, max.Y),
                Utils.Clamp(value1.Z, min.Z, max.Z));
        }

        public static Vector3 Cross(Vector3 value1, Vector3 value2)
        {
            return new Vector3(
                value1.Y * value2.Z - value2.Y * value1.Z,
                value1.Z * value2.X - value2.Z * value1.X,
                value1.X * value2.Y - value2.X * value1.Y);
        }

        public static float Distance(Vector3 value1, Vector3 value2)
        {
            return (float)Math.Sqrt(DistanceSquared(value1, value2));
        }

        public static float DistanceSquared(Vector3 value1, Vector3 value2)
        {
            return
                (value1.X - value2.X) * (value1.X - value2.X) +
                (value1.Y - value2.Y) * (value1.Y - value2.Y) +
                (value1.Z - value2.Z) * (value1.Z - value2.Z);
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Vector3 Divide(Vector3 value1, Vector3 value2)
        {
            value1.X /= value2.X;
            value1.Y /= value2.Y;
            value1.Z /= value2.Z;
            return value1;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Vector3 Divide(Vector3 value1, float value2)
        {
            float factor = 1f / value2;
            value1.X *= factor;
            value1.Y *= factor;
            value1.Z *= factor;
            return value1;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static float Dot(Vector3 value1, Vector3 value2)
        {
            return (value1.X * value2.X) + (value1.Y * value2.Y) + (value1.Z * value2.Z);
        }

        public static Vector3 Lerp(Vector3 value1, Vector3 value2, float amount)
        {
            return new Vector3(
                Utils.Lerp(value1.X, value2.X, amount),
                Utils.Lerp(value1.Y, value2.Y, amount),
                Utils.Lerp(value1.Z, value2.Z, amount));
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static float Mag(Vector3 value)
        {
            return value.Length();
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Vector3 Max(Vector3 value1, Vector3 value2)
        {
            return new Vector3(
                Math.Max(value1.X, value2.X),
                Math.Max(value1.Y, value2.Y),
                Math.Max(value1.Z, value2.Z));
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Vector3 Min(Vector3 value1, Vector3 value2)
        {
            return new Vector3(
                Math.Min(value1.X, value2.X),
                Math.Min(value1.Y, value2.Y),
                Math.Min(value1.Z, value2.Z));
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Vector3 Multiply(Vector3 value1, Vector3 value2)
        {
            value1.X *= value2.X;
            value1.Y *= value2.Y;
            value1.Z *= value2.Z;
            return value1;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Vector3 Multiply(Vector3 value1, float scaleFactor)
        {
            value1.X *= scaleFactor;
            value1.Y *= scaleFactor;
            value1.Z *= scaleFactor;
            return value1;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Vector3 Negate(Vector3 value)
        {
            value.X = -value.X;
            value.Y = -value.Y;
            value.Z = -value.Z;
            return value;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Vector3 Normalize(Vector3 value)
        {
            float factor = value.LengthSquared();
            if (factor > 1e-6f)
            {
                factor = 1f / (float)Math.Sqrt(factor);
                value.X *= factor;
                value.Y *= factor;
                value.Z *= factor;
            }
            else
            {
                value.X = 0f;
                value.Y = 0f;
                value.Z = 0f;
            }
            return value;
        }

        /// <summary>
        /// Parse a vector from a string
        /// </summary>
        /// <param name="val">A string representation of a 3D vector, enclosed 
        /// in arrow brackets and separated by commas</param>
        public static Vector3 Parse(string val)
        {
            char[] splitChar = { ',' };
            string[] split = val.Replace("<", String.Empty).Replace(">", String.Empty).Split(splitChar);
            return new Vector3(
                Single.Parse(split[0].Trim(), Utils.EnUsCulture),
                Single.Parse(split[1].Trim(), Utils.EnUsCulture),
                Single.Parse(split[2].Trim(), Utils.EnUsCulture));
        }

        public static bool TryParse(string val, out Vector3 result)
        {
            try
            {
                result = Parse(val);
                return true;
            }
            catch (Exception)
            {
                result = Vector3.Zero;
                return false;
            }
        }

        /// <summary>
        /// Calculate the rotation between two vectors
        /// </summary>
        /// <param name="a">Normalized directional vector (such as 1,0,0 for forward facing)</param>
        /// <param name="b">Normalized target vector</param>
        public static Quaternion RotationBetween(Vector3 a, Vector3 b)
        {
            const double piOverfour = 0.25 * Math.PI;
            double magProduct = Math.Sqrt(a.LengthSquared() * b.LengthSquared());
            double angle;
            if(magProduct > 1e-6)
            {
                float dotProduct = Dot(a, b);
                if(dotProduct < 1e-6)
                    angle = piOverfour;
                else
                   angle = 0.5 * Math.Acos(dotProduct / magProduct);
            }
            else
                angle = piOverfour;

            Vector3 axis = Cross(a, b);
            axis.Normalize();

            float s = (float)Math.Sin(angle);
            return new Quaternion(
                axis.X * s,
                axis.Y * s,
                axis.Z * s,
                (float)Math.Cos(angle));
        }

        /// <summary>
        /// Interpolates between two vectors using a cubic equation
        /// </summary>
        public static Vector3 SmoothStep(Vector3 value1, Vector3 value2, float amount)
        {
            return new Vector3(
                Utils.SmoothStep(value1.X, value2.X, amount),
                Utils.SmoothStep(value1.Y, value2.Y, amount),
                Utils.SmoothStep(value1.Z, value2.Z, amount));
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Vector3 Subtract(Vector3 value1, Vector3 value2)
        {
            value1.X -= value2.X;
            value1.Y -= value2.Y;
            value1.Z -= value2.Z;
            return value1;
        }

        public static Vector3 Transform(Vector3 position, Matrix4 matrix)
        {
            return new Vector3(
                (position.X * matrix.M11) + (position.Y * matrix.M21) + (position.Z * matrix.M31) + matrix.M41,
                (position.X * matrix.M12) + (position.Y * matrix.M22) + (position.Z * matrix.M32) + matrix.M42,
                (position.X * matrix.M13) + (position.Y * matrix.M23) + (position.Z * matrix.M33) + matrix.M43);
        }

        public static Vector3 TransformNormal(Vector3 position, Matrix4 matrix)
        {
            return new Vector3(
                (position.X * matrix.M11) + (position.Y * matrix.M21) + (position.Z * matrix.M31),
                (position.X * matrix.M12) + (position.Y * matrix.M22) + (position.Z * matrix.M32),
                (position.X * matrix.M13) + (position.Y * matrix.M23) + (position.Z * matrix.M33));
        }

        #endregion Static Methods

        #region Overrides

        public override bool Equals(object obj)
        {
            return (obj is Vector3) ? this == (Vector3)obj : false;
        }

        public bool Equals(Vector3 other)
        {
            return this == other;
        }

        public override int GetHashCode()
        {
            int hash = X.GetHashCode();
            hash = Utils.CombineHash(hash, Y.GetHashCode());
            hash = Utils.CombineHash(hash, Y.GetHashCode()); 
            return hash;
        }

        /// <summary>
        /// Get a formatted string representation of the vector
        /// </summary>
        /// <returns>A string representation of the vector</returns>
        public override string ToString()
        {
            return String.Format(Utils.EnUsCulture, "<{0}, {1}, {2}>", X, Y, Z);
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

            return String.Format(enUs, "{0} {1} {2}", X, Y, Z);
        }

        #endregion Overrides

        #region Operators

        public static bool operator ==(Vector3 value1, Vector3 value2)
        {
            return value1.X == value2.X
                && value1.Y == value2.Y
                && value1.Z == value2.Z;
        }

        public static bool operator !=(Vector3 value1, Vector3 value2)
        {
            return !(value1 == value2);
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator +(Vector3 value1, Vector3 value2)
        {
            value1.X += value2.X;
            value1.Y += value2.Y;
            value1.Z += value2.Z;
            return value1;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator -(Vector3 value)
        {
            value.X = -value.X;
            value.Y = -value.Y;
            value.Z = -value.Z;
            return value;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator -(Vector3 value1, Vector3 value2)
        {
            value1.X -= value2.X;
            value1.Y -= value2.Y;
            value1.Z -= value2.Z;
            return value1;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator *(Vector3 value1, Vector3 value2)
        {
            value1.X *= value2.X;
            value1.Y *= value2.Y;
            value1.Z *= value2.Z;
            return value1;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator *(Vector3 value, float scaleFactor)
        {
            value.X *= scaleFactor;
            value.Y *= scaleFactor;
            value.Z *= scaleFactor;
            return value;
        }

        public static Vector3 operator *(Vector3 vec, Quaternion rot)
        {
            float rx = rot.W * vec.X + rot.Y * vec.Z - rot.Z * vec.Y;
            float ry = rot.W * vec.Y + rot.Z * vec.X - rot.X * vec.Z;
            float rz = rot.W * vec.Z + rot.X * vec.Y - rot.Y * vec.X;

            vec.X += 2.0f * (rz * rot.Y - ry * rot.Z);
            vec.Y += 2.0f * (rx * rot.Z - rz * rot.X);
            vec.Z += 2.0f * (ry * rot.X - rx * rot.Y);

            return vec;
        }

        public static Vector3 operator *(Vector3 vector, Matrix4 matrix)
        {
            return Transform(vector, matrix);
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator /(Vector3 value1, Vector3 value2)
        {
            value1.X /= value2.X;
            value1.Y /= value2.Y;
            value1.Z /= value2.Z;
            return value1;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator /(Vector3 value, float divider)
        {
            float factor = 1f / divider;
            value.X *= factor;
            value.Y *= factor;
            value.Z *= factor;
            return value;
        }

        /// <summary>
        /// Cross product between two vectors
        /// </summary>
        public static Vector3 operator %(Vector3 value1, Vector3 value2)
        {
            return Cross(value1, value2);
        }

        /// <summary>
        /// Explicit casting for Vector3d > Vector3
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator Vector3(Vector3d value)
        {
            return new Vector3(value);
        }

        #endregion Operators

        /// <summary>A vector with a value of 0,0,0</summary>
        public readonly static Vector3 Zero = new Vector3();
        /// <summary>A vector with a value of 1,1,1</summary>
        public readonly static Vector3 One = new Vector3(1f, 1f, 1f);
        /// <summary>A unit vector facing forward (X axis), value 1,0,0</summary>
        public readonly static Vector3 UnitX = new Vector3(1f, 0f, 0f);
        /// <summary>A unit vector facing left (Y axis), value 0,1,0</summary>
        public readonly static Vector3 UnitY = new Vector3(0f, 1f, 0f);
        /// <summary>A unit vector facing up (Z axis), value 0,0,1</summary>
        public readonly static Vector3 UnitZ = new Vector3(0f, 0f, 1f);
    }
}
