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
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Intrinsics;
using System.Text;
using System.Collections;

namespace OpenMetaverse
{
    /// <summary>
    /// A three-dimensional vector with floating-point values
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector3 : IComparable<Vector3>, IEquatable<Vector3>
    {
        /// <summary>x value</summary>
        public float X;
        /// <summary>Y value</summary>
        public float Y;
        /// <summary>Z value</summary>
        public float Z;
        
        #region Constructors

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3(float value)
        {
            X = value;
            Y = value;
            Z = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3(Vector2 value, float z)
        {
            X = value.X;
            Y = value.Y;
            Z = z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3(Vector3d vector)
        {
            X = (float)vector.X;
            Y = (float)vector.Y;
            Z = (float)vector.Z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3(Vector3 vector)
        {
            X = vector.X;
            Y = vector.Y;
            Z = vector.Z;
        }

        /// <summary>
        /// Constructor, builds a vector from a byte array
        /// </summary>
        /// <param name="byteArray">Byte array containing three four-byte floats</param>
        /// <param name="pos">Beginning position in the byte array</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3(byte[] byteArray, int pos)
        {
            X = Utils.BytesToFloatSafepos(byteArray, pos);
            Y = Utils.BytesToFloatSafepos(byteArray, pos + 4);
            Z = Utils.BytesToFloatSafepos(byteArray, pos + 8);
        }

        #endregion Constructors

        #region Public Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Abs()
        {
            if (X < 0)
                X = -X;
            if (Y < 0)
                Y = -Y;
            if (Z < 0)
                Z = -Z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Min(Vector3 v)
        {
            if (v.X < X) X = v.X;
            if (v.Y < Y) Y = v.Y;
            if (v.Z < Z) Z = v.Z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Max(Vector3 v)
        {
            if (v.X > X) X = v.X;
            if (v.Y > Y) Y = v.Y;
            if (v.Z > Z) Z = v.Z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(Vector3 v)
        {
            X += v.X;
            Y += v.Y;
            Z += v.Z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Sub(Vector3 v)
        {
            X -= v.X;
            Y -= v.Y;
            Z -= v.Z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clamp(float min, float max)
        {
            if (X > max)
                X = max;
            else if (X < min)
                X = min;

            if (Y > max)
                Y = max;
            else if (Y < min)
                Y = min;

            if (Z > max)
                Z = max;
            else if (Z < min)
                Z = min;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float Length()
        {
            return MathF.Sqrt(X * X + Y * Y + Z * Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float LengthSquared()
        {
            return (X * X + Y * Y + Z * Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Normalize()
        {
            float factor = X * X + Y * Y + Z * Z;
            if (factor > 1e-6f)
            {
                factor = 1f / MathF.Sqrt(factor);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool ApproxEquals(Vector3 vec)
        {
            return Utils.ApproxEqual(X, vec.X) &&
                    Utils.ApproxEqual(Y, vec.Y) &&
                    Utils.ApproxEqual(Z, vec.Z);
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
        public readonly bool ApproxEquals(Vector3 vec, float tolerance)
        {
            return Utils.ApproxEqual(X, vec.X, tolerance) &&
                   Utils.ApproxEqual(Y, vec.Y, tolerance) &&
                   Utils.ApproxEqual(Z, vec.Z, tolerance);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool ApproxZero()
        {
            if (!Utils.ApproxZero(X))
                return false;
            if (!Utils.ApproxZero(Y))
                return false;
            if (!Utils.ApproxZero(Z))
                return false;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool ApproxZero(float tolerance)
        {
            if (!Utils.ApproxZero(X, tolerance))
                return false;
            if (!Utils.ApproxZero(Y, tolerance))
                return false;
            if (!Utils.ApproxZero(Z, tolerance))
                return false;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool IsZero()
        {
            if (X != 0)
                return false;
            if (Y != 0)
                return false;
            if (Z != 0)
                return false;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool IsNotZero()
        {
            if (X != 0)
                return true;
            if (Y != 0)
                return true;
            if (Z != 0)
                return true;
            return false;
        }

        /// <summary>
        /// IComparable.CompareTo implementation
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int CompareTo(Vector3 vector)
        {
            return LengthSquared().CompareTo(vector.LengthSquared());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float Dot(Vector3 value2)
        {
            return (X * value2.X) + (Y * value2.Y) + (Z * value2.Z);
        }
        public readonly float AbsDot(Vector3 value2)
        {
            return MathF.Abs(X * value2.X) + MathF.Abs(Y * value2.Y) + MathF.Abs(Z * value2.Z);
        }
        /// <summary>
        /// Test if this vector is composed of all finite numbers
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool IsFinite()
        {
            return (Utils.IsFinite(X) && Utils.IsFinite(Y) && Utils.IsFinite(Z));
        }

        /// <summary>
        /// Builds a vector from a byte array
        /// </summary>
        /// <param name="byteArray">Byte array containing a 12 byte vector</param>
        /// <param name="pos">Beginning position in the byte array</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromBytes(byte[] byteArray)
        {
            this = Unsafe.ReadUnaligned<Vector3>(ref MemoryMarshal.GetArrayDataReference(byteArray));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromBytes(byte[] byteArray, int pos)
        {
            this = Unsafe.ReadUnaligned<Vector3>(ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(byteArray), pos));
        }

        /// <summary>
        /// Returns the raw bytes for this vector
        /// </summary>
        /// <returns>A 12 byte array containing X, Y, and Z</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly byte[] GetBytes()
        {
            byte[] dest = new byte[12];
            Unsafe.WriteUnaligned<Vector3>(ref MemoryMarshal.GetArrayDataReference(dest), this);
            return dest;
        }

        /// <summary>
        /// Writes the raw bytes for this vector to a byte array
        /// </summary>
        /// <param name="dest">Destination byte array</param>
        /// <param name="pos">Position in the destination array to start
        /// writing. Must be at least 12 bytes before the end of the array</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe void ToBytes(byte[] dest, int pos)
        {
            if (Utils.CanDirectCopyLE)
            {
                fixed (byte* d = &dest[0])
                    *(Vector3*)(d + pos) = this;
            }
            else
            {
                Utils.FloatToBytesSafepos(X, dest, pos);
                Utils.FloatToBytesSafepos(Y, dest, pos + 4);
                Utils.FloatToBytesSafepos(Z, dest, pos + 8);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe void ToBytes(byte* dest)
        {
            if (Utils.CanDirectCopyLE)
                *(Vector3*)dest = this;
            else
            {
                Utils.FloatToBytes(X, dest);
                Utils.FloatToBytes(Y, dest + 4);
                Utils.FloatToBytes(Z, dest + 8);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe void ClampedToShortsBytes(float range, byte[] dest, int pos)
        {
            float a, b;

            a = MathF.Abs(X);
            b = MathF.Abs(Y);
            if (b > a)
                a = b;
            b = MathF.Abs(Z);
            if (b > a)
                a = b;

            ushort sx, sy, sz;
            if (a > range)
            {
                a = range / a;
                sx = Utils.FloatToUInt16(X * a, range);
                sy = Utils.FloatToUInt16(Y * a, range);
                sz = Utils.FloatToUInt16(Z * a, range);
            }
            else
            {
                sx = Utils.FloatToUInt16(X, range);
                sy = Utils.FloatToUInt16(Y, range);
                sz = Utils.FloatToUInt16(Z, range);
            }

            if (Utils.CanDirectCopyLE)
            {
                fixed (byte* d = &dest[0])
                {
                    *(ushort*)(d + pos) = sx;
                    *(ushort*)(d + pos + 2) = sy;
                    *(ushort*)(d + pos + 4) = sz;
                }
            }
            else
            {
                Utils.UInt16ToBytes(sx, dest, pos);
                Utils.UInt16ToBytes(sy, dest, pos + 2);
                Utils.UInt16ToBytes(sz, dest, pos + 4);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe void ClampedToShortsBytes(float range, byte* dest, int pos)
        {
            float a, b;

            a = MathF.Abs(X);
            b = MathF.Abs(Y);
            if (b > a)
                a = b;
            b = MathF.Abs(Z);
            if (b > a)
                a = b;

            ushort sx, sy, sz;
            if (a > range)
            {
                a = range / a;
                sx = Utils.FloatToUInt16(X * a, range);
                sy = Utils.FloatToUInt16(Y * a, range);
                sz = Utils.FloatToUInt16(Z * a, range);
            }
            else
            {
                sx = Utils.FloatToUInt16(X, range);
                sy = Utils.FloatToUInt16(Y, range);
                sz = Utils.FloatToUInt16(Z, range);
            }

            if (Utils.CanDirectCopyLE)
            {
                *(ushort*)(dest + pos) = sx;
                *(ushort*)(dest + pos + 2) = sy;
                *(ushort*)(dest + pos + 4) = sz;
            }
            else
            {
                Utils.UInt16ToBytes(sx, dest, pos);
                Utils.UInt16ToBytes(sy, dest, pos + 2);
                Utils.UInt16ToBytes(sz, dest, pos + 4);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe void ClampedToShortsBytes(float range, byte* dest)
        {
            float a, b;

            a = MathF.Abs(X);
            b = MathF.Abs(Y);
            if (b > a)
                a = b;
            b = MathF.Abs(Z);
            if (b > a)
                a = b;

            ushort sx, sy, sc;
            if (a > range)
            {
                a = range / a;
                sx = Utils.FloatToUInt16(X * a, range);
                sy = Utils.FloatToUInt16(Y * a, range);
                sc = Utils.FloatToUInt16(Z * a, range);
            }
            else
            {
                sx = Utils.FloatToUInt16(X, range);
                sy = Utils.FloatToUInt16(Y, range);
                sc = Utils.FloatToUInt16(Z, range);
            }

            if (Utils.CanDirectCopyLE)
            {
                *(ushort*)(dest) = sx;
                *(ushort*)(dest + 2) = sy;
                *(ushort*)(dest + 4) = sc;
            }
            else
            {
                Utils.UInt16ToBytes(sx, dest);
                Utils.UInt16ToBytes(sy, dest + 2);
                Utils.UInt16ToBytes(sc, dest + 4);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Rotate(Quaternion rot)
        {
            float x2 = rot.X + rot.X;
            float y2 = rot.Y + rot.Y;
            float z2 = rot.Z + rot.Z;

            float wx2 = rot.W * x2;
            float wy2 = rot.W * y2;
            float wz2 = rot.W * z2;
            float xx2 = rot.X * x2;
            float xy2 = rot.X * y2;
            float xz2 = rot.X * z2;
            float yy2 = rot.Y * y2;
            float yz2 = rot.Y * z2;
            float zz2 = rot.Z * z2;

            x2 = X;
            y2 = Y;
            z2 = Z;

            X = x2 * (1.0f - yy2 - zz2) + y2 * (xy2 - wz2) + z2 * (xz2 + wy2);
            Y = x2 * (xy2 + wz2) + y2 * (1.0f - xx2 - zz2) + z2 * (yz2 - wx2);
            Z = x2 * (xz2 - wy2) + y2 * (yz2 + wx2) + z2 * (1.0f - xx2 - yy2);
        }

        public void InverseRotate(Quaternion rot)
        {
            float x2 = rot.X + rot.X;
            float y2 = rot.Y + rot.Y;
            float z2 = rot.Z + rot.Z;

            float wx2 = rot.W * x2;
            float wy2 = rot.W * y2;
            float wz2 = rot.W * z2;

            float xx2 = rot.X * x2;
            float xy2 = rot.X * y2;
            float xz2 = rot.X * z2;
            float yy2 = rot.Y * y2;
            float yz2 = rot.Y * z2;
            float zz2 = rot.Z * z2;

            x2 = X;
            y2 = Y;
            z2 = Z;

            X = x2 * (1.0f - yy2 - zz2) + y2 * (xy2 + wz2) + z2 * (xz2 - wy2);
            Y = x2 * (xy2 - wz2) + y2 * (1.0f - xx2 - zz2) + z2 * (yz2 + wx2);
            Z = x2 * (xz2 + wy2) + y2 * (yz2 - wx2) + z2 * (1.0f - xx2 - yy2);
        }

        //quaternion must be normalized <0,0,z,w>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RotateByQZ(Quaternion rot)
        {
            float z2 = rot.Z + rot.Z;
            float zz2 = 1.0f - rot.Z * z2;
            float wz2 = rot.W * z2;

            float ox = X;
            float oy = Y;

            X = ox * zz2 - oy * wz2;
            Y = ox * wz2 + oy * zz2;
        }

        //quaternion must be normalized <0,0,z,w>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InverseRotateByQZ(Quaternion rot)
        {
            float z2 = rot.Z + rot.Z;
            float zz2 = 1.0f - rot.Z * z2;
            float wz2 = rot.W * z2;

            float ox = X;
            float oy = Y;

            X = ox * zz2 + oy * wz2;
            Y = oy * zz2 - ox * wz2;
        }

        //shortQuaternion must be normalized <z,w>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RotateByShortQZ(Vector2 shortQuaternion)
        {
            float z2 = shortQuaternion.X + shortQuaternion.X;
            float zz2 = 1.0f - shortQuaternion.X * z2;
            float wz2 = shortQuaternion.Y * z2;

            float ox = X;
            float oy = Y;

            X = ox * zz2 - oy * wz2;
            Y = ox * wz2 + oy * zz2;
        }

        //quaternion must be normalized <0,0,z,w>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InverseRotateByShortQZ(Vector2 shortQuaternion)
        {
            float z2 = shortQuaternion.X + shortQuaternion.X;
            float zz2 = 1.0f - shortQuaternion.X * z2;
            float wz2 = shortQuaternion.Y * z2;

            float ox = X;
            float oy = Y;

            X = ox * zz2 + oy * wz2;
            Y = oy * zz2 - ox * wz2;
        }
        #endregion Public Methods

        #region Static Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Add(Vector3 value1, Vector3 value2)
        {
            return new Vector3(value1.X + value2.X, value1.Y + value2.Y, value1.Z + value2.Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Abs(Vector3 value1)
        {
            return new Vector3(MathF.Abs(value1.X), MathF.Abs(value1.Y), MathF.Abs(value1.Z));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Clamp(Vector3 value1, float min, float max)
        {
            return new Vector3(
                Utils.Clamp(value1.X, min, max),
                Utils.Clamp(value1.Y, min, max),
                Utils.Clamp(value1.Z, min, max));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Clamp(Vector3 value1, Vector3 min, Vector3 max)
        {
            return new Vector3(
                Utils.Clamp(value1.X, min.X, max.X),
                Utils.Clamp(value1.Y, min.Y, max.Y),
                Utils.Clamp(value1.Z, min.Z, max.Z));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Cross(Vector3 value1, Vector3 value2)
        {
            return new Vector3(
                 value1.Y * value2.Z - value2.Y * value1.Z,
                 value1.Z * value2.X - value2.Z * value1.X,
                 value1.X * value2.Y - value2.X * value1.Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Distance(Vector3 value1, Vector3 value2)
        {
            return (float)Math.Sqrt(DistanceSquared(value1, value2));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DistanceSquared(Vector3 value1, Vector3 value2)
        {
            float x = value1.X - value2.X;
            float y = value1.Y - value2.Y;
            float z = value1.Z - value2.Z;

            return x * x + y * y + z * z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Divide(Vector3 value1, Vector3 value2)
        {
            return new Vector3(value1.X / value2.X, value1.Y / value2.Y, value1.Z / value2.Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Divide(Vector3 value1, float value2)
        {
            float factor = 1f / value2;
            return new Vector3(value1.X * factor, value1.Y * factor, value1.Z * factor);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Dot(Vector3 value1, Vector3 value2)
        {
            return (value1.X * value2.X) + (value1.Y * value2.Y) + (value1.Z * value2.Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float AbsDot(Vector3 value1, Vector3 value2)
        {
            return MathF.Abs(value1.X * value2.X) + MathF.Abs(value1.Y * value2.Y) + MathF.Abs(value1.Z * value2.Z);
        }

        public static Vector3 Lerp(Vector3 value1, Vector3 value2, float amount)
        {
            return new Vector3(
                Utils.Lerp(value1.X, value2.X, amount),
                Utils.Lerp(value1.Y, value2.Y, amount),
                Utils.Lerp(value1.Z, value2.Z, amount));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Mag(Vector3 value)
        {
            return value.Length();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Max(Vector3 value1, Vector3 value2)
        {
            return new Vector3(
                Math.Max(value1.X, value2.X),
                Math.Max(value1.Y, value2.Y),
                Math.Max(value1.Z, value2.Z));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Min(Vector3 value1, Vector3 value2)
        {
            return new Vector3(
                Math.Min(value1.X, value2.X),
                Math.Min(value1.Y, value2.Y),
                Math.Min(value1.Z, value2.Z));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Multiply(Vector3 value1, Vector3 value2)
        {
            return new Vector3(value1.X * value2.X, value1.Y * value2.Y, value1.Z * value2.Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Multiply(Vector3 value1, float scaleFactor)
        {
            return new Vector3(value1.X * scaleFactor, value1.Y * scaleFactor, value1.Z * scaleFactor);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Negate(Vector3 value)
        {
            return new Vector3(-value.X, -value.Y, -value.Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Normalize(Vector3 value)
        {
            float factor = value.LengthSquared();
            if (factor > 1e-6f)
            {
                factor = 1f / MathF.Sqrt(factor);
                return value * factor;
            }
            return new Vector3();
        }

        /// <summary>
        /// Parse a vector from a string
        /// </summary>
        /// <param name="val">A string representation of a 3D vector, enclosed 
        /// in arrow brackets and separated by commas</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Vector3 Parse(string val)
        {
            return Parse(val.AsSpan());
        }

        public static unsafe Vector3 Parse(ReadOnlySpan<char> sp)
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

                if (!float.TryParse(sp[start..comma1], NumberStyles.Float, Utils.EnUsCulture, out float x))
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
                if (!float.TryParse(sp[start..comma1], NumberStyles.Float, Utils.EnUsCulture, out float y))
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

                if (!float.TryParse(sp[start..comma1], NumberStyles.Float, Utils.EnUsCulture, out float z))
                    throw new FormatException("Invalid Vector3");
                return new Vector3(x, y, z);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static bool TryParse(string val, out Vector3 result)
        {
            return TryParse(val.AsSpan(), out result);
        }

        public unsafe static bool TryParse(ReadOnlySpan<char> sp, out Vector3 result)
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

                if (!float.TryParse(sp[start..comma1], NumberStyles.Float, Utils.EnUsCulture, out float x))
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
                if (!float.TryParse(sp[start..comma1], NumberStyles.Float, Utils.EnUsCulture, out float y))
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

                if (!float.TryParse(sp[start..comma1], NumberStyles.Float, Utils.EnUsCulture, out float z))
                {
                    result = Zero;
                    return false;
                }
                result = new Vector3(x, y, z);
                return true;
            }
        }

        /// <summary>
        /// Calculate the rotation between two vectors
        /// </summary>
        /// <param name="a">Normalized directional vector (such as 1,0,0 for forward facing)</param>
        /// <param name="b">Normalized target vector</param>
        public static Quaternion RotationBetween(Vector3 a, Vector3 b)
        {
            const float piOverfour = 0.25f * MathF.PI;
            float magProduct = MathF.Sqrt(a.LengthSquared() * b.LengthSquared());
            float angle;
            if(magProduct > 1e-6)
            {
                float dotProduct = Dot(a, b);
                if(dotProduct < 1e-6f)
                    angle = piOverfour;
                else
                   angle = 0.5f * MathF.Acos(dotProduct / magProduct);
            }
            else
                angle = piOverfour;

            Vector3 axis = Cross(a, b);
            axis.Normalize();

            float s = MathF.Sin(angle);
            return new Quaternion(
                axis.X * s,
                axis.Y * s,
                axis.Z * s,
                (float)Math.Cos(angle));
        }

        /// <summary>
        /// Interpolates between two vectors using a cubic equation
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 SmoothStep(Vector3 value1, Vector3 value2, float amount)
        {
            return new Vector3(
                Utils.SmoothStep(value1.X, value2.X, amount),
                Utils.SmoothStep(value1.Y, value2.Y, amount),
                Utils.SmoothStep(value1.Z, value2.Z, amount));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Subtract(Vector3 value1, Vector3 value2)
        {
            return new Vector3(value1.X - value2.X, value1.Y - value2.Y, value1.Z - value2.Z);
        }
        /*
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static Vector3 SubtractS(Vector3 value1, Vector3 value2)
        {
            if (Sse2.IsSupported)
            {
                Vector128<float> ma = Sse2.LoadScalarVector128((double*)&value1.X).AsSingle();
                ma = Sse2.Shuffle(ma, Sse2.LoadScalarVector128((float*)&value1.Z), 0x44);

                Vector128<float>  mb = Sse2.LoadScalarVector128((double*)&value2.X).AsSingle();
                mb = Sse2.Shuffle(mb, Sse2.LoadScalarVector128((float*)&value2.Z), 0x44);

                ma = Sse.Subtract(ma, mb);
                Vector3 ret = new();
                Sse2.StoreScalar((double*)&ret.X, ma.AsDouble());
                Sse2.StoreScalar(&ret.Z, Sse2.Shuffle(ma.AsInt32(), 0x02).AsSingle());
                return ret;
            }
            else
                return Subtract(value1, value2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 AddS(Vector3 value1, Vector3 value2)
        {
            if (Sse2.IsSupported)
            {
                unsafe
                {
                    Vector128<float> ma = Sse2.LoadScalarVector128((double*)&value1.X).AsSingle();
                    ma = Sse2.Shuffle(ma, Sse2.LoadScalarVector128(&value1.Z), 0x44);

                    Vector128<float> mb = Sse2.LoadScalarVector128((double*)&value2.X).AsSingle();
                    mb = Sse2.Shuffle(mb, Sse2.LoadScalarVector128(&value2.Z), 0x44);

                    ma = Sse.Add(ma, mb);
                    Vector3 ret = new();
                    Sse2.StoreScalar((double*)&ret.X, ma.AsDouble());
                    Sse2.StoreScalar(&ret.Z, Sse2.Shuffle(ma.AsInt32(), 0x02).AsSingle());
                    return ret;
                }
            }
            else
                return Subtract(value1, value2);
        }
        */
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Transform(Vector3 vec, Quaternion rot)
        {
            return Rotate(vec, rot);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Rotate(Vector3 vec, Quaternion rot)
        {
            float x2 = rot.X + rot.X;
            float y2 = rot.Y + rot.Y;
            float z2 = rot.Z + rot.Z;

            float wx2 = rot.W * x2;
            float wy2 = rot.W * y2;
            float wz2 = rot.W * z2;
            float xx2 = rot.X * x2;
            float xy2 = rot.X * y2;
            float xz2 = rot.X * z2;
            float yy2 = rot.Y * y2;
            float yz2 = rot.Y * z2;
            float zz2 = rot.Z * z2;

            x2 = vec.X;
            y2 = vec.Y;
            z2 = vec.Z;

            return new Vector3(
                x2 * (1.0f - yy2 - zz2) + y2 * (xy2 - wz2) + z2 * (xz2 + wy2),
                x2 * (xy2 + wz2) + y2 * (1.0f - xx2 - zz2) + z2 * (yz2 - wx2),
                x2 * (xz2 - wy2) + y2 * (yz2 + wx2) + z2 * (1.0f - xx2 - yy2));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 InverseRotate(Vector3 vec, Quaternion rot)
        {
            float x2 = rot.X + rot.X;
            float y2 = rot.Y + rot.Y;
            float z2 = rot.Z + rot.Z;

            float wx2 = rot.W * x2;
            float wy2 = rot.W * y2;
            float wz2 = rot.W * z2;
            float xx2 = rot.X * x2;
            float xy2 = rot.X * y2;
            float xz2 = rot.X * z2;
            float yy2 = rot.Y * y2;
            float yz2 = rot.Y * z2;
            float zz2 = rot.Z * z2;

            x2 = vec.X;
            y2 = vec.Y;
            z2 = vec.Z;

            return new Vector3(
                x2 * (1.0f - yy2 - zz2) + y2 * (xy2 + wz2) + z2 * (xz2 - wy2),
                x2 * (xy2 - wz2) + y2 * (1.0f - xx2 - zz2) + z2 * (yz2 + wx2),
                x2 * (xz2 + wy2) + y2 * (yz2 - wx2) + z2 * (1.0f - xx2 - yy2));
        }


        //quaternion must be normalized <0,0,z,w>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 RotateByQZ(Vector3 vec, Quaternion rot)
        {
            float z2 = rot.Z + rot.Z;
            float wz2 = rot.W * z2;
            float zz2 = 1.0f - rot.Z * z2;

            return new Vector3(
                vec.X * zz2 - vec.Y * wz2,
                vec.X * wz2 + vec.Y * zz2,
                vec.Z);;
        }

        //quaternion must be normalized <0,0,z,w>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 InverseRotateByQZ(Vector3 vec, Quaternion rot)
        {
            float z2 = rot.Z + rot.Z;
            float wz2 = rot.W * z2;
            float zz2 = 1.0f - rot.Z * z2;

            return new Vector3(
                vec.X * zz2 + vec.Y * wz2,
                vec.Y * zz2 - vec.X * wz2,
                vec.Z);
        }

        //shortQuaternion must be normalized <z,w>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 RotateByShortQZ(Vector3 vec, Vector2 shortQuaternion)
        {
            float z2 = shortQuaternion.X + shortQuaternion.X;
            float zz2 = 1.0f - shortQuaternion.X * z2;
            float wz2 = shortQuaternion.Y * z2;

            return new Vector3(
                vec.X * zz2 - vec.Y * wz2,
                vec.X * wz2 + vec.Y * zz2,
                vec.Z); ;
        }

        //shortQuaternion must be normalized <z,w>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 InverseRotateByShortQZ(Vector3 vec, Vector2 shortQuaternion)
        {
            float z2 = shortQuaternion.X + shortQuaternion.X;
            float zz2 = 1.0f - shortQuaternion.X * z2;
            float wz2 = shortQuaternion.Y * z2;

            return new Vector3(
                vec.X * zz2 + vec.Y * wz2,
                vec.Y * zz2 - vec.X * wz2,
                vec.Z);
        }
        #endregion Static Methods

        #region Overrides

        public readonly override bool Equals(object obj)
        {
            if (obj is not Vector3)
                return false;

            Vector3 other = (Vector3)obj;
            if (X != other.X)
                return false;
            if (Y != other.Y)
                return false;
            if (Z != other.Z)
                return false;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(Vector3 other)
        {
            if (X != other.X)
                return false;
            if (Y != other.Y)
                return false;
            if (Z != other.Z)
                return false;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool NotEqual(Vector3 other)
        {
            if (X != other.X)
                return true;
            if (Y != other.Y)
                return true;
            if (Z != other.Z)
                return true;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override int GetHashCode()
        {
            int hash = X.GetHashCode();
            hash = Utils.CombineHash(hash, Y.GetHashCode());
            hash = Utils.CombineHash(hash, Z.GetHashCode()); 
            return hash;
        }

        /// <summary>
        /// Get a formatted string representation of the vector
        /// </summary>
        /// <returns>A string representation of the vector</returns>
        public readonly override string ToString()
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
        public readonly string ToRawString()
        {
            CultureInfo enUs = new CultureInfo("en-us");
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Vector3 value1, Vector3 value2)
        {
            if (value1.X != value2.X)
                return false;
            if (value1.Y != value2.Y)
                return false;
            if (value1.Z != value2.Z)
                return false;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Vector3 value1, Vector3 value2)
        {
            if (value1.X != value2.X)
                return true;
            if (value1.Y != value2.Y)
                return true;
            if (value1.Z != value2.Z)
                return true;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator +(Vector3 value1, Vector3 value2)
        {
            return new Vector3(value1.X + value2.X, value1.Y + value2.Y, value1.Z + value2.Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator -(Vector3 value)
        {
            return new Vector3(-value.X , -value.Y, -value.Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator -(Vector3 value1, Vector3 value2)
        {
            return new Vector3(value1.X - value2.X, value1.Y - value2.Y, value1.Z - value2.Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator *(Vector3 value1, Vector3 value2)
        {
            return new Vector3(value1.X * value2.X, value1.Y * value2.Y, value1.Z * value2.Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator *(Vector3 value, float scaleFactor)
        {
            return new Vector3(value.X * scaleFactor, value.Y * scaleFactor, value.Z * scaleFactor);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator *(Vector3 vec, Quaternion rot)
        {
            float x2 = rot.X + rot.X;
            float y2 = rot.Y + rot.Y;
            float z2 = rot.Z + rot.Z;

            float wx2 = rot.W * x2;
            float wy2 = rot.W * y2;
            float wz2 = rot.W * z2;
            float xx2 = rot.X * x2;
            float xy2 = rot.X * y2;
            float xz2 = rot.X * z2;
            float yy2 = rot.Y * y2;
            float yz2 = rot.Y * z2;
            float zz2 = rot.Z * z2;

            x2 = vec.X;
            y2 = vec.Y;
            z2 = vec.Z;

            return new Vector3(
                x2 * (1.0f - yy2 - zz2) + y2 * (xy2 - wz2) + z2 * (xz2 + wy2),
                x2 * (xy2 + wz2) + y2 * (1.0f - xx2 - zz2) + z2 * (yz2 - wx2),
                x2 * (xz2 - wy2) + y2 * (yz2 + wx2) + z2 * (1.0f - xx2 - yy2));

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator *(Vector3 vector, Matrix4 matrix)
        {
            return Transform(vector, matrix);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator /(Vector3 value1, Vector3 value2)
        {
            return new Vector3(value1.X / value2.X, value1.Y / value2.Y, value1.Z / value2.Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator /(Vector3 value, float divider)
        {
            float factor = 1f / divider;
            return new Vector3(value.X * factor, value.Y * factor, value.Z * factor);
        }

        /// <summary>
        /// Cross product between two vectors
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator %(Vector3 value1, Vector3 value2)
        {
            return Cross(value1, value2);
        }

        /// <summary>
        /// Explicit casting for Vector3d > Vector3
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Vector3(Vector3d value)
        {
            return new Vector3(value);
        }

        #endregion Operators

        /// <summary>A vector with a value of 0,0,0</summary>
        public readonly static Vector3 Zero = new();
        /// <summary>A vector with a value of 1,1,1</summary>
        public readonly static Vector3 One = new(1f);
        /// <summary>A unit vector facing forward (X axis), value 1,0,0</summary>
        public readonly static Vector3 UnitX = new(1f, 0f, 0f);
        /// <summary>A unit vector facing left (Y axis), value 0,1,0</summary>
        public readonly static Vector3 UnitY = new(0f, 1f, 0f);
        /// <summary>A unit vector facing up (Z axis), value 0,0,1</summary>
        public readonly static Vector3 UnitZ = new(0f, 0f, 1f);
        public readonly static Vector3 MinValue = new(float.MinValue, float.MinValue, float.MinValue);
        public readonly static Vector3 MaxValue = new(float.MaxValue, float.MaxValue, float.MaxValue);
    }
}
