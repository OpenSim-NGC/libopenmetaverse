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
using System.Runtime.Intrinsics.X86;
using System.Runtime.Intrinsics;

namespace OpenMetaverse
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Quaternion : IEquatable<Quaternion>
    {
        /// <summary>X value</summary>
        public float X;
        /// <summary>Y value</summary>
        public float Y;
        /// <summary>Z value</summary>
        public float Z;
        /// <summary>W value</summary>
        public float W;

        public enum MainAxis : int
        {
            X = 0,
            Y = 1,
            Z = 2
        }

        #region Constructors

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Quaternion(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Quaternion(Vector3 vectorPart, float scalarPart)
        {
            X = vectorPart.X;
            Y = vectorPart.Y;
            Z = vectorPart.Z;
            W = scalarPart;
        }

        /// <summary>
        /// Build a quaternion from normalized float values
        /// </summary>
        /// <param name="x">X value from -1.0 to 1.0</param>
        /// <param name="y">Y value from -1.0 to 1.0</param>
        /// <param name="z">Z value from -1.0 to 1.0</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Quaternion(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;

            float xyzsum = 1f - (X * X) - (Y * Y) - (Z * Z);
            W = (xyzsum > 1e-6f) ? MathF.Sqrt(xyzsum) : 0;
        }

        /// <summary>
        /// Constructor, builds a quaternion object from a byte array
        /// </summary>
        /// <param name="byteArray">Byte array containing four four-byte floats</param>
        /// <param name="pos">Offset in the byte array to start reading at</param>
        /// <param name="normalized">Whether the source data is normalized or
        /// not. If this is true 12 bytes will be read, otherwise 16 bytes will
        /// be read.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Quaternion(byte[] byteArray, int pos, bool normalized)
        {
            X = Y = Z = 0;
            W = 1;
            FromBytes(byteArray, pos, normalized);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Quaternion(Quaternion q)
        {
            this = q;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Quaternion(Vector128<float> q) : this()
        {
            Unsafe.As<Quaternion, Vector128<float>>(ref Unsafe.AsRef(this)) = q;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Quaternion(MainAxis BaseAxis, float angle)
        {
            switch (BaseAxis)
            {
                case MainAxis.X:
                    W = MathF.Cos(0.5f * angle);
                    X = MathF.Sqrt(1.0f - W * W);
                    Y = 0;
                    Z = 0;
                    break;
                case MainAxis.Y:
                    W = MathF.Cos(0.5f * angle);
                    Y = MathF.Sqrt(1.0f - W * W);
                    X = 0;
                    Z = 0;
                    break;
                case MainAxis.Z:
                    W = MathF.Cos(0.5f * angle);
                    Z = MathF.Sqrt(1.0f - W * W);
                    X = 0;
                    Y = 0;
                    break;
                default: //error
                    X = 0;
                    Y = 0;
                    Z = 0;
                    W = 1;
                    break;
            }
        }

        #endregion Constructors

        #region Public Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool ApproxEquals(Quaternion quat)
        {
            // assume normalized
            return MathF.Abs(quat.W - W) < 1e-6f &&
                   MathF.Abs(quat.Z - Z) < 1e-6f &&
                   MathF.Abs(quat.X - X) < 1e-6f;
        }
        /* not faster
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool ApproxEquals2(in Quaternion quat)
        {
            if (Sse.IsSupported)
            {
                Vector128<float> tol = Vector128.Create(1e-6f);

                Vector128<float> a = Unsafe.As<Quaternion, Vector128<float>>(ref Unsafe.AsRef(this));
                Vector128<float> c = Sse.CompareEqual(tol, tol);

                Vector128<float> b = Unsafe.As<Quaternion, Vector128<float>>(ref Unsafe.AsRef(quat));
                c = Sse2.ShiftRightLogical(c.AsInt32(), 1).AsSingle();

                a = Sse.Subtract(a, b);
                a = Sse.And(a, c);
                a = Sse.CompareLessThan(a, tol);
                int res = Sse.MoveMask(a);
                return res == 0x0f;
            }
            // assume normalized
            return MathF.Abs(quat.X - X) < 1e-6f &&
                    MathF.Abs(quat.Y - Y) < 1e-6f &&
                    MathF.Abs(quat.Z - Z) < 1e-6f;
        }
        */

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool ApproxEquals(Quaternion quat, float tolerance)
        {
            // assume normalized
            return MathF.Abs(quat.W - W) < tolerance &&
                   MathF.Abs(quat.Z - Z) < tolerance &&
                   MathF.Abs(quat.X - X) < tolerance;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool IsIdentity()
        {
            // assume normalized
            return MathF.Abs(W) > (1.0f - 1e-6f);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool IsIdentityOrZero()
        {
            // assume normalized
            if (X != 0)
                return false;
            if (Y != 0)
                return false;
            if (Z != 0)
                return false;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe float Length()
        {
            if (Sse41.IsSupported)
            {
                Vector128<float> q = Unsafe.As<Quaternion, Vector128<float>>(ref Unsafe.AsRef(this));
                q = Sse41.DotProduct(q, q, 0xf1);
                return MathF.Sqrt(q.ToScalar());
            }
            else
                return MathF.Sqrt((X * X) + (Y * Y) + (Z * Z) + (W * W));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe float LengthSquared()
        {
            if (Sse41.IsSupported)
            {
                Vector128<float> q = Unsafe.As<Quaternion, Vector128<float>>(ref Unsafe.AsRef(this));
                q = Sse41.DotProduct(q, q, 0xf1);
                return q.ToScalar();
            }
            else
                return (X * X) + (Y * Y) + (Z * Z) + (W * W);
        }

        /// <summary>
        /// Normalizes the quaternion
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Normalize()
        {
            if (Sse41.IsSupported)
            {
                Vector128<float> q = Unsafe.As<Quaternion, Vector128<float>>(ref Unsafe.AsRef(this));
                Vector128<float> d = Sse41.DotProduct(q, q, 0xff);
                float m = d.ToScalar();
                if (m > 1e-6f)
                {
                    d = Sse.Sqrt(d);
                    q = Sse.Divide(q, d);
                    //d = Sse.ReciprocalSqrt(d);
                    //q = Sse.Multiply(q, d);
                    Unsafe.As<Quaternion, Vector128<float>>(ref this) = q;
                    return;
                }
                q = Vector128.Create(0f, 0f, 0f, 1f);
                Unsafe.As<Quaternion, Vector128<float>>(ref this) = q;
                return;
            }

            float mag = LengthSquared();
            if (mag > 1e-6f)
            {
                float oomag = 1f / MathF.Sqrt(mag);
                X *= oomag;
                Y *= oomag;
                Z *= oomag;
                W *= oomag;
            }
            else
            {
                X = 0f;
                Y = 0f;
                Z = 0f;
                W = 1f;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Conjugate()
        {
            X = -X;
            Y = -Y;
            Z = -Z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invert()
        {
            float len = LengthSquared();
            if (len > 1e-6f)
            {
                len = -1.0f / len;
                X *= len;
                Y *= len;
                Z *= len;
                W *= -len;
            }
            else
            {
                X = 0f;
                Y = 0f;
                Z = 0f;
                W = 1f;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float Dot(Quaternion q2)
        {
            if (Sse41.IsSupported)
            {
                Vector128<float> q = Unsafe.As<Quaternion, Vector128<float>>(ref Unsafe.AsRef(this));
                Vector128<float> d = Unsafe.As<Quaternion, Vector128<float>>(ref q2);
                d = Sse41.DotProduct(q, d, 0xf1);
                return d.ToScalar();
            }
            return (X * q2.X) + (Y * q2.Y) + (Z * q2.Z) + (W * q2.W);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(Quaternion quaternion2)
        {
            if (Sse41.IsSupported)
            {
                Vector128<float> q = Unsafe.As<Quaternion, Vector128<float>>(ref Unsafe.AsRef(this));
                Vector128<float> d = Unsafe.As<Quaternion, Vector128<float>>(ref quaternion2);
                d = Sse.Add(q, d);
                Unsafe.As<Quaternion, Vector128<float>>(ref Unsafe.AsRef(this)) = d;
            }
            else
            {
                X += quaternion2.X;
                Y += quaternion2.Y;
                Z += quaternion2.Z;
                W += quaternion2.W;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Sub(Quaternion quaternion2)
        {
            if (Sse41.IsSupported)
            {
                Vector128<float> q = Unsafe.As<Quaternion, Vector128<float>>(ref Unsafe.AsRef(this));
                Vector128<float> d = Unsafe.As<Quaternion, Vector128<float>>(ref quaternion2);
                d = Sse.Subtract(q, d);
                Unsafe.As<Quaternion, Vector128<float>>(ref Unsafe.AsRef(this)) = d;
            }
            else
            {
                X -= quaternion2.X;
                Y -= quaternion2.Y;
                Z -= quaternion2.Z;
                W -= quaternion2.W;
            }
        }

        /// <summary>
        /// Builds a quaternion object from a byte array
        /// </summary>
        /// <param name="byteArray">The source byte array</param>
        /// <param name="pos">Offset in the byte array to start reading at</param>
        /// <param name="normalized">Whether the source data is normalized or
        /// not. If this is true 12 bytes will be read, otherwise 16 bytes will
        /// be read.</param>
        public void FromBytes(byte[] byteArray, int pos, bool normalized)
        {
            X = Utils.BytesToFloatSafepos(byteArray, pos);
            Y = Utils.BytesToFloatSafepos(byteArray, pos + 4);
            Z = Utils.BytesToFloatSafepos(byteArray, pos + 8);
            if (normalized)
            {
                float xyzsum = 1f - (X * X) - (Y * Y) - (Z * Z);
                W = (xyzsum > 1e-6f) ? MathF.Sqrt(xyzsum) : 0f;
            }
            else
            {
                W = Utils.BytesToFloatSafepos(byteArray, pos + 12);
            }
        }

        /// <summary>
        /// Normalize this quaternion and serialize it to a byte array
        /// </summary>
        /// <returns>A 12 byte array containing normalized X, Y, and Z floating
        /// point values in order using little endian byte ordering</returns>
        public readonly byte[] GetBytes()
        {
            byte[] bytes = new byte[12];
            float norm = LengthSquared();
            if (norm > 1e-6f || W < 0.9999f)
            {
                if (W < 0f)
                    norm = -1f / MathF.Sqrt(norm);
                else
                    norm = 1f / MathF.Sqrt(norm);
                Utils.FloatToBytesSafepos(norm * X, bytes, 0);
                Utils.FloatToBytesSafepos(norm * Y, bytes, 4);
                Utils.FloatToBytesSafepos(norm * Z, bytes, 8);
            }
            else
            {
                Utils.FloatToBytesSafepos(0, bytes, 0);
                Utils.FloatToBytesSafepos(0, bytes, 4);
                Utils.FloatToBytesSafepos(0, bytes, 8);
            }
            return bytes;
        }

        /// <summary>
        /// Writes the raw bytes for this quaternion to a byte array
        /// </summary>
        /// <param name="dest">Destination byte array</param>
        /// <param name="pos">Position in the destination array to start
        /// writing. Must be at least 12 bytes before the end of the array</param>
        public readonly unsafe void ToBytes(byte[] dest, int pos)
        {
            float norm = LengthSquared();
            if (norm > 1e-6f || norm < 0.9999f)
            {
                if (W < 0f)
                    norm = -1f / MathF.Sqrt(norm);
                else
                    norm = 1f / MathF.Sqrt(norm);
                if (Utils.CanDirectCopyLE)
                {
                    fixed (byte* d = &dest[0])
                    {
                        *(float*)(d + pos) = norm * X;
                        *(float*)(d + pos + 4) = norm * Y;
                        *(float*)(d + pos + 8) = norm * Z;
                    }
                }
                else
                {
                    Utils.FloatToBytesSafepos(norm * X, dest, pos);
                    Utils.FloatToBytesSafepos(norm * Y, dest, pos + 4);
                    Utils.FloatToBytesSafepos(norm * Z, dest, pos + 8);
                }
            }
            else
            {
                if (Utils.CanDirectCopyLE)
                {
                    fixed (byte* d = &dest[0])
                    {
                        *(long*)(d + pos) = 0;
                        *(int*)(d + pos + 8) = 0;
                    }
                }
                else
                {
                    Utils.FloatToBytesSafepos(0, dest, pos);
                    Utils.FloatToBytesSafepos(0, dest, pos + 4);
                    Utils.FloatToBytesSafepos(0, dest, pos + 8);
                }
            }
        }

        public readonly unsafe void ToBytes(byte* dest)
        {
            float norm = LengthSquared();
            if (norm > 1e-6f || norm < 0.9999f)
            {
                if (W < 0f)
                    norm = -1f / MathF.Sqrt(norm);
                else
                    norm = 1f / MathF.Sqrt(norm);
                if (Utils.CanDirectCopyLE)
                {
                    *(float*)(dest) = norm * X;
                    *(float*)(dest + 4) = norm * Y;
                    *(float*)(dest + 8) = norm * Z;
                }
                else
                {
                    Utils.FloatToBytes(norm * X, dest);
                    Utils.FloatToBytes(norm * Y, dest + 4);
                    Utils.FloatToBytes(norm * Z, dest + 8);
                }
            }
            else
            {
                if (Utils.CanDirectCopyLE)
                {
                    *(long*)dest = 0;
                    *(int*)(dest + 8) = 0;
                }
                else
                {
                    Utils.FloatToBytes(0, dest);
                    Utils.FloatToBytes(0, dest + 4);
                    Utils.FloatToBytes(0, dest + 8);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe void ToShortsBytes(byte[] dest, int pos)
        {
            ushort sx = Utils.FloatToUnitUInt16(X);
            ushort sy = Utils.FloatToUnitUInt16(Y);
            ushort sz = Utils.FloatToUnitUInt16(Z);
            ushort sw = Utils.FloatToUnitUInt16(W);

            if (Utils.CanDirectCopyLE)
            {
                fixed (byte* d = &dest[0])
                {
                    *(ushort*)(d + pos) = sx;
                    *(ushort*)(d + pos + 2) = sy;
                    *(ushort*)(d + pos + 4) = sz;
                    *(ushort*)(d + pos + 6) = sw;
                }
            }
            else
            {
                Utils.UInt16ToBytes(sx, dest, pos);
                Utils.UInt16ToBytes(sy, dest, pos + 2);
                Utils.UInt16ToBytes(sz, dest, pos + 4);
                Utils.UInt16ToBytes(sw, dest, pos + 6);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe void ToShortsBytes(byte* dest, int pos)
        {
            ushort sx = Utils.FloatToUnitUInt16(X);
            ushort sy = Utils.FloatToUnitUInt16(Y);
            ushort sz = Utils.FloatToUnitUInt16(Z);
            ushort sw = Utils.FloatToUnitUInt16(W);

            if (Utils.CanDirectCopyLE)
            {
                *(ushort*)(dest + pos) = sx;
                *(ushort*)(dest + pos + 2) = sy;
                *(ushort*)(dest + pos + 4) = sz;
                *(ushort*)(dest + pos + 6) = sw;
            }
            else
            {
                Utils.UInt16ToBytes(sx, dest, pos);
                Utils.UInt16ToBytes(sy, dest, pos + 2);
                Utils.UInt16ToBytes(sz, dest, pos + 4);
                Utils.UInt16ToBytes(sw, dest, pos + 6);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe void ToShortsBytes(byte* dest)
        {
            ushort sx = Utils.FloatToUnitUInt16(X);
            ushort sy = Utils.FloatToUnitUInt16(Y);
            ushort sz = Utils.FloatToUnitUInt16(Z);
            ushort sw = Utils.FloatToUnitUInt16(W);

            if (Utils.CanDirectCopyLE)
            {
                *(ushort*)(dest) = sx;
                *(ushort*)(dest + 2) = sy;
                *(ushort*)(dest + 4) = sz;
                *(ushort*)(dest + 6) = sw;
            }
            else
            {
                Utils.UInt16ToBytes(sx, dest);
                Utils.UInt16ToBytes(sy, dest + 2);
                Utils.UInt16ToBytes(sz, dest + 4);
                Utils.UInt16ToBytes(sw, dest + 6);
            }
        }

        /// <summary>
        /// Convert this quaternion to euler angles
        /// </summary>
        /// <param name="roll">X euler angle</param>
        /// <param name="pitch">Y euler angle</param>
        /// <param name="yaw">Z euler angle</param>
        public readonly void GetEulerAngles(out float roll, out float pitch, out float yaw)
        {
            const float halfpi = MathF.PI / 2f;

            roll = 0f;
            pitch = 0f;
            yaw = 0f;

            if(W > 0.999999f)
                return;

            float tX = X * X;
            float tY = Y * Y;
            float tZ = Z * Z;
            float tW = W * W;
            float m = tX + tY + tZ + tW;
            if (MathF.Abs(m) < 0.0001f)
                return;

            float n = 2 * (Y * W + X * Z);
            float p = m * m - n * n;

            if (p > 0f)
            {
                roll = MathF.Atan2(2.0f * (X * W - Y * Z), (-tX - tY + tZ + tW));
                pitch = MathF.Atan2(n, MathF.Sqrt(p));
                yaw = MathF.Atan2(2.0f * (Z * W - X * Y), tX - tY - tZ + tW);
            }
            else if (n > 0f)
            {
                roll = 0f;
                pitch = halfpi;
                yaw = (float)Math.Atan2((Z * W + X * Y), 0.5f - tX - tY);
            }
            else
            {
                roll = 0f;
                pitch = -halfpi;
                yaw = MathF.Atan2((Z * W + X * Y), 0.5f - tX - tZ);
            }
        }

        /// <summary>
        /// Convert this quaternion to an angle around an axis
        /// </summary>
        /// <param name="axis">Unit vector describing the axis</param>
        /// <param name="angle">Angle around the axis, in radians</param>
        public readonly void GetAxisAngle(out Vector3 axis, out float angle)
        {
            //Normalize();
            float ww = W * W;
            if (ww > 0.9999f)
            {
                axis = Vector3.UnitX;
                angle = 0;
                return;
            }
            if(ww < 0.0001f)
            {
                if (W < 0f)
                    axis = new Vector3(-X, -Y, -Z);
                else
                    axis = new Vector3(X, Y, Z);
                angle = MathF.PI;
                return;
            }

            float sin = MathF.Sqrt(1.0f - ww);
            float invSin = 1.0f / sin;
            if (W < 0)
                invSin = -invSin;
            axis = new Vector3(X, Y, Z) * invSin;

            angle = 2.0f * MathF.Acos(W);
            if (angle > MathF.PI)
                angle = 2.0f * MathF.PI - angle;
        }

        #endregion Public Methods

        #region Static Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quaternion Add(in Quaternion quaternion1, in Quaternion quaternion2)
        {
            if (Sse.IsSupported)
            {
                Vector128<float> a = Unsafe.As<Quaternion, Vector128<float>>(ref Unsafe.AsRef(quaternion1));
                Vector128<float> b = Unsafe.As<Quaternion, Vector128<float>>(ref Unsafe.AsRef(quaternion2));
                a = Sse.Add(a, b);
                return new Quaternion(a);
            }

            return new Quaternion(
                quaternion1.X + quaternion2.X,
                quaternion1.Y + quaternion2.Y,
                quaternion1.Z + quaternion2.Z,
                quaternion1.W + quaternion2.W);
        }

        /// <summary>
        /// Returns the conjugate (spatial inverse) of a quaternion
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quaternion Conjugate(in Quaternion quaternion)
        {
            if (Sse.IsSupported)
            {
                Vector128<float> d = Unsafe.As<Quaternion, Vector128<float>>(ref Unsafe.AsRef(quaternion));
                Vector128<float> Mask = Vector128.Create(0x80000000, 0x80000000, 0x80000000, 0).AsSingle();
                d = Sse.Xor(d, Mask);
                return new Quaternion(d);
            }
            return new Quaternion(-quaternion.X, -quaternion.Y, -quaternion.Z, quaternion.W);
        }

        /// <summary>
        /// Build a quaternion from an axis and an angle of rotation around
        /// that axis
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quaternion CreateFromAxisAngle(float axisX, float axisY, float axisZ, float angle)
        {
            Vector3 axis = new(axisX, axisY, axisZ);
            return CreateFromAxisAngle(axis, angle);
        }

        /// <summary>
        /// Build a quaternion from an axis and an angle of rotation around
        /// that axis
        /// </summary>
        /// <param name="axis">Axis of rotation</param>
        /// <param name="angle">Angle of rotation</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quaternion CreateFromAxisAngle(Vector3 axis, float angle)
        {
            axis.Normalize();

            angle *= 0.5f;
            float c = MathF.Cos(angle);
            float s = MathF.Sin(angle);

            return new Quaternion(axis.X * s, axis.Y * s, axis.Z * s, c);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quaternion CreateRotationX(float angle)
        {
            angle *= 0.5f;
            float c = MathF.Cos(angle);
            float s = MathF.Sin(angle);

            return new Quaternion(s, 0, 0, c);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quaternion CreateRotationY(float angle)
        {
            angle *= 0.5f;
            float c = MathF.Cos(angle);
            float s = MathF.Sin(angle);

            return new Quaternion(0, s, 0, c);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quaternion CreateRotationZ(float angle)
        {
            angle *= 0.5f;
            float c = MathF.Cos(angle);
            float s = MathF.Sin(angle);

            return new Quaternion(0, 0, s, c);
        }

        /// <summary>
        /// Creates a quaternion from a vector containing roll, pitch, and yaw
        /// in radians
        /// </summary>
        /// <param name="eulers">Vector representation of the euler angles in
        /// radians</param>
        /// <returns>Quaternion representation of the euler angles</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quaternion CreateFromEulers(Vector3 eulers)
        {
            return CreateFromEulers(eulers.X, eulers.Y, eulers.Z);
        }

        /// <summary>
        /// Creates a quaternion from roll, pitch, and yaw euler angles in
        /// radians
        /// </summary>
        /// <param name="roll">X angle in radians</param>
        /// <param name="pitch">Y angle in radians</param>
        /// <param name="yaw">Z angle in radians</param>
        /// <returns>Quaternion representation of the euler angles</returns>
        public static Quaternion CreateFromEulers(float roll, float pitch, float yaw)
        {
            if (roll > Utils.TWO_PI || pitch > Utils.TWO_PI || yaw > Utils.TWO_PI)
                throw new ArgumentException("Euler angles must be in radians");
            
            roll *= 0.5f;
            float atCos = MathF.Cos(roll);
            float atSin = MathF.Sin(roll);
            pitch *= 0.5f;
            float leftCos = MathF.Cos(pitch);
            float leftSin = MathF.Sin(pitch);
            yaw *= 0.5f;
            float upCos = MathF.Cos(yaw);
            float upSin = MathF.Sin(yaw);

            float atLeftCos = atCos * leftCos;
            float atLeftSin = atSin * leftSin;
            return new Quaternion(
                atSin * leftCos * upCos + atCos * leftSin * upSin,
                atCos * leftSin * upCos - atSin * leftCos * upSin,
                atLeftCos * upSin + atLeftSin * upCos,
                atLeftCos * upCos - atLeftSin * upSin
            );
        }

        public static Quaternion CreateFromRotationMatrix(Matrix3x3 matrix)
        {
            float num = matrix.M11 + matrix.M22 + matrix.M33;
            float n2;
            if (num >= 0f)
            {
                num = MathF.Sqrt((num + 1f));
                n2 = 0.5f / num;
                return new Quaternion(
                        (matrix.M23 - matrix.M32) * n2,
                        (matrix.M31 - matrix.M13) * n2,
                        (matrix.M12 - matrix.M21) * n2,
                        num * 0.5f);
            }
            if ((matrix.M11 >= matrix.M22) && (matrix.M11 >= matrix.M33))
            {
                num = MathF.Sqrt((((1f + matrix.M11) - matrix.M22) - matrix.M33));
                n2 = 0.5f / num;
                return new Quaternion(
                        0.5f * num,
                        (matrix.M12 + matrix.M21) * n2,
                        (matrix.M13 + matrix.M31) * n2,
                        (matrix.M23 - matrix.M32) * n2);
            }
            if (matrix.M22 > matrix.M33)
            {
                num = MathF.Sqrt((((1f + matrix.M22) - matrix.M11) - matrix.M33));
                n2 = 0.5f / num;
                return new Quaternion(
                        (matrix.M21 + matrix.M12) * n2,
                        0.5f * num,
                        (matrix.M32 + matrix.M23) * n2,
                        (matrix.M31 - matrix.M13) * n2);
            }

            num = MathF.Sqrt((((1f + matrix.M33) - matrix.M11) - matrix.M22));
            n2 = 0.5f / num;
            return new Quaternion(
                        (matrix.M31 + matrix.M13) * n2,
                        (matrix.M32 + matrix.M23) * n2,
                        0.5f * num,
                        (matrix.M12 - matrix.M21) * n2);
        }

        public static Quaternion CreateFromRotationMatrix(Matrix4 matrix)
        {
            float num = matrix.M11 + matrix.M22 + matrix.M33;
            float n2;
            if (num >= 0f)
            {
                num = MathF.Sqrt((num + 1f));
                n2 = 0.5f / num;
                return new Quaternion(
                        (matrix.M23 - matrix.M32) * n2,
                        (matrix.M31 - matrix.M13) * n2,
                        (matrix.M12 - matrix.M21) * n2,
                        num * 0.5f);
            }
            if ((matrix.M11 >= matrix.M22) && (matrix.M11 >= matrix.M33))
            {
                num = MathF.Sqrt((((1f + matrix.M11) - matrix.M22) - matrix.M33));
                n2 = 0.5f / num;
                return new Quaternion(
                        0.5f * num,
                        (matrix.M12 + matrix.M21) * n2,
                        (matrix.M13 + matrix.M31) * n2,
                        (matrix.M23 - matrix.M32) * n2);
            }
            if (matrix.M22 > matrix.M33)
            {
                num = MathF.Sqrt((((1f + matrix.M22) - matrix.M11) - matrix.M33));
                n2 = 0.5f / num;
                return new Quaternion(
                        (matrix.M21 + matrix.M12) * n2,
                        0.5f * num,
                        (matrix.M32 + matrix.M23) * n2,
                        (matrix.M31 - matrix.M13) * n2);
            }

            num = MathF.Sqrt((((1f + matrix.M33) - matrix.M11) - matrix.M22));
            n2 = 0.5f / num;
            return new Quaternion(
                        (matrix.M31 + matrix.M13) * n2,
                        (matrix.M32 + matrix.M23) * n2,
                        0.5f * num,
                        (matrix.M12 - matrix.M21) * n2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quaternion Divide(Quaternion q1, Quaternion q2)
        {
            return Quaternion.Inverse(q1) * q2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Dot(Quaternion q1, Quaternion q2)
        {
            if (Sse41.IsSupported)
            {
                Vector128<float> q = Unsafe.As<Quaternion, Vector128<float>>(ref q1);
                Vector128<float> d = Unsafe.As<Quaternion, Vector128<float>>(ref q2);
                d = Sse41.DotProduct(q, d, 0xf1);
                return d.ToScalar();
            }
            return (q1.X * q2.X) + (q1.Y * q2.Y) + (q1.Z * q2.Z) + (q1.W * q2.W);
        }

        /// <summary>
        /// inverse of a quaternion
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quaternion Inverse(Quaternion quaternion)
        {
            float normsq = quaternion.LengthSquared();

            if (normsq < 1e-6f)
                return Quaternion.Identity;

            float oonorm = -1f / normsq;
            return new Quaternion(
                    quaternion.X * oonorm,
                    quaternion.Y * oonorm,
                    quaternion.Z * oonorm,
                    -quaternion.W * oonorm);
        }

        /// <summary>
        /// Spherical linear interpolation between two quaternions
        /// </summary>
        public static Quaternion Slerp(Quaternion q1, in Quaternion q2, float amount)
        {
            float angle = Dot(q1, q2);

            if (angle < 0f)
            {
                q1 *= -1f;
                angle *= -1f;
            }

            float scale;
            float invscale;

            if ((angle + 1f) > 0.05f)
            {
                if ((1f - angle) >= 0.05f)
                {
                    // slerp
                    float theta = MathF.Acos(angle);
                    float invsintheta = 1f / MathF.Sin(theta);
                    scale = MathF.Sin(theta * (1f - amount)) * invsintheta;
                    invscale = MathF.Sin(theta * amount) * invsintheta;
                }
                else
                {
                    // lerp
                    scale = 1f - amount;
                    invscale = amount;
                }
            }
            else
            {
                scale = MathF.Sin(Utils.PI * (0.5f - amount));
                invscale = MathF.Sin(Utils.PI * amount);
                return new Quaternion(
                    q1.X * scale - q1.Y * invscale,
                    q1.Y * scale + q1.X * invscale,
                    q1.Z * scale - q1.W * invscale,
                    q1.W * scale + q1.Z * invscale
                    );
            }

            return new Quaternion(
                q1.X * scale + q2.X * invscale,
                q1.Y * scale + q2.Y * invscale,
                q1.Z * scale + q2.Z * invscale,
                q1.W * scale + q2.W * invscale
                );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quaternion Subtract(Quaternion quaternion1, Quaternion quaternion2)
        {
            return new Quaternion(
                quaternion1.X - quaternion2.X,
                quaternion1.Y - quaternion2.Y,
                quaternion1.Z - quaternion2.Z,
                quaternion1.W - quaternion2.W);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quaternion Multiply(Quaternion a, Quaternion b)
        {
            return new Quaternion(
                a.W * b.X + a.X * b.W + a.Y * b.Z - a.Z * b.Y,
                a.W * b.Y + a.Y * b.W + a.Z * b.X - a.X * b.Z,
                a.W * b.Z + a.Z * b.W + a.X * b.Y - a.Y * b.X,
                a.W * b.W - a.X * b.X - a.Y * b.Y - a.Z * b.Z
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quaternion Multiply(Quaternion quaternion, float scaleFactor)
        {
            return new Quaternion(
                quaternion.X * scaleFactor,
                quaternion.Y * scaleFactor,
                quaternion.Z * scaleFactor,
                quaternion.W * scaleFactor);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quaternion Negate(Quaternion quaternion)
        {
            return new Quaternion(
                -quaternion.X,
                -quaternion.Y,
                -quaternion.Z,
                -quaternion.W);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quaternion Normalize(Quaternion q)
        {
            float mag = q.LengthSquared();
            if (mag > 1e-6f)
            {
                float oomag = 1f / MathF.Sqrt(mag);
                return new Quaternion(
                    q.X * oomag,
                    q.Y * oomag,
                    q.Z * oomag,
                    q.W * oomag);
            }
            return Quaternion.Identity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]

        public unsafe static Quaternion Parse(string val)
        {
            return Parse(val.AsSpan());
        }

        public unsafe static Quaternion Parse(ReadOnlySpan<char> sp)
        {
            if (sp.Length < 7)
                throw new FormatException("Invalid Quaternion");

            int start = 0;
            fixed (char* p = sp)
            {
                while (start < sp.Length)
                {
                    if (p[start++] == '<')
                        break;
                }
                if (start > sp.Length - 6)
                    throw new FormatException("Invalid Quaternion");

                int comma1 = start + 1;
                while (comma1 < sp.Length)
                {
                    if (p[comma1] == ',')
                        break;
                    comma1++;
                }
                if (comma1 > sp.Length - 5)
                    throw new FormatException("Invalid Quaternion");

                if (!float.TryParse(sp[start..comma1], NumberStyles.Float, Utils.EnUsCulture, out float x))
                    throw new FormatException("Invalid Quaternion");

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
                    throw new FormatException("Invalid Quaternion");
                if (!float.TryParse(sp[start..comma1], NumberStyles.Float, Utils.EnUsCulture, out float y))
                    throw new FormatException("Invalid Quaternion");

                comma1++;
                start = comma1;
                comma1++;
                while (comma1 < sp.Length)
                {
                    if (p[comma1] == '>' || p[comma1] == ',')
                        break;
                    comma1++;
                }
                if (comma1 >= sp.Length)
                    throw new FormatException("Invalid Quaternion");

                if (!float.TryParse(sp[start..comma1], NumberStyles.Float, Utils.EnUsCulture, out float z))
                    throw new FormatException("Invalid Quaternion");

                if (p[comma1] == '>')
                    return new Quaternion(x, y, z);

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
                    throw new FormatException("Invalid Quaternion");

                if (!float.TryParse(sp[start..comma1], NumberStyles.Float, Utils.EnUsCulture, out float w))
                    throw new FormatException("Invalid Quaternion");

                return new Quaternion(x, y, z, w);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static bool TryParse(string val, out Quaternion result)
        {
            return TryParse(val.AsSpan(), out result);
        }

        public unsafe static bool TryParse(ReadOnlySpan<char> sp, out Quaternion result)
        {
            if (sp.Length < 7)
            {
                result = Identity;
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
                    result = Identity;
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
                    result = Identity;
                    return false;
                }

                if (!float.TryParse(sp[start..comma1], NumberStyles.Float, Utils.EnUsCulture, out float x))
                {
                    result = Identity;
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
                    result = Identity;
                    return false;
                }
                if (!float.TryParse(sp[start..comma1], NumberStyles.Float, Utils.EnUsCulture, out float y))
                {
                    result = Identity;
                    return false;
                }

                comma1++;
                start = comma1;
                comma1++;
                while (comma1 < sp.Length)
                {
                    if (p[comma1] == '>' || p[comma1] == ',')
                        break;
                    comma1++;
                }
                if (comma1 >= sp.Length)
                {
                    result = Identity;
                    return false;
                }

                if (!float.TryParse(sp[start..comma1], NumberStyles.Float, Utils.EnUsCulture, out float z))
                {
                    result = Identity;
                    return false;
                }

                if (p[comma1] == '>')
                {
                    result = new Quaternion(x, y, z);
                    return true;
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
                    result = Identity;
                    return false;
                }

                if (!float.TryParse(sp[start..comma1], NumberStyles.Float, Utils.EnUsCulture, out float w))
                {
                    result = Identity;
                    return false;
                }

                result = new Quaternion(x, y, z, w);
                return true;
            }
        }

        #endregion Static Methods

        #region Overrides

        public readonly override bool Equals(object obj)
        {
            if((obj is not Quaternion))
                return false;
            Quaternion other = (Quaternion)obj;
            if (X != other.X)
                return false;
            if (Y != other.Y)
                return false;
            if (Z != other.Z)
                return false;
            if (W != other.W)
                return false;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(Quaternion other)
        {
            if (X != other.X)
                return false;
            if (Y != other.Y)
                return false;
            if (Z != other.Z)
                return false;
            if (W != other.W)
                return false;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool NotEqual(Quaternion other)
        {
            if (X != other.X)
                return true;
            if (Y != other.Y)
                return true;
            if (Z != other.Z)
                return true;
            if (W != other.W)
                return true;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override int GetHashCode()
        {
            int hash = X.GetHashCode();
            hash = Utils.CombineHash(hash, Y.GetHashCode());
            hash = Utils.CombineHash(hash, Z.GetHashCode());
            hash = Utils.CombineHash(hash, W.GetHashCode());
            return hash;
        }

        public readonly override string ToString()
        {
            StringBuilder sb = new();
            sb.Append('<');
            sb.Append(X.ToString(Utils.EnUsCulture));
            sb.Append(", ");
            sb.Append(Y.ToString(Utils.EnUsCulture));
            sb.Append(", ");
            sb.Append(Z.ToString(Utils.EnUsCulture));
            sb.Append(", ");
            sb.Append(W.ToString(Utils.EnUsCulture));
            sb.Append('>');
            return sb.ToString();
        }

        /// <summary>
        /// Get a string representation of the quaternion elements with up to three
        /// decimal digits and separated by spaces only
        /// </summary>
        /// <returns>Raw string representation of the quaternion</returns>
        public readonly string ToRawString()
        {
            CultureInfo enUs = new("en-us");
            enUs.NumberFormat.NumberDecimalDigits = 3;

            StringBuilder sb = new();
            sb.Append(X.ToString(enUs));
            sb.Append(' ');
            sb.Append(Y.ToString(enUs));
            sb.Append(' ');
            sb.Append(Z.ToString(enUs));
            sb.Append(' ');
            sb.Append(W.ToString(enUs));
            return sb.ToString();
        }

        #endregion Overrides

        #region Operators

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Quaternion quaternion1, Quaternion quaternion2)
        {
            if (quaternion1.X != quaternion2.X)
                return false;
            if (quaternion1.Y != quaternion2.Y)
                return false;
            if (quaternion1.Z != quaternion2.Z)
                return false;
            if (quaternion1.W != quaternion2.W)
                return false;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Quaternion quaternion1, Quaternion quaternion2)
        {
            if (quaternion1.X != quaternion2.X)
                return true;
            if (quaternion1.Y != quaternion2.Y)
                return true;
            if (quaternion1.Z != quaternion2.Z)
                return true;
            if (quaternion1.W != quaternion2.W)
                return true;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quaternion operator +(Quaternion quaternion1, Quaternion quaternion2)
        {
            return Add(quaternion1, quaternion2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quaternion operator -(Quaternion quaternion)
        {
            return Negate(quaternion);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quaternion operator -(Quaternion quaternion1, Quaternion quaternion2)
        {
            return Subtract(quaternion1, quaternion2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quaternion operator *(Quaternion a, Quaternion b)
        {
            return new Quaternion(
                a.W * b.X + a.X * b.W + a.Y * b.Z - a.Z * b.Y,
                a.W * b.Y + a.Y * b.W + a.Z * b.X - a.X * b.Z,
                a.W * b.Z + a.Z * b.W + a.X * b.Y - a.Y * b.X,
                a.W * b.W - a.X * b.X - a.Y * b.Y - a.Z * b.Z
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quaternion operator *(Quaternion quaternion, float scaleFactor)
        {
            return new Quaternion(
                quaternion.X * scaleFactor,
                quaternion.Y * scaleFactor,
                quaternion.Z * scaleFactor,
                quaternion.W * scaleFactor);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quaternion operator /(Quaternion quaternion1, Quaternion quaternion2)
        {
            return Divide(quaternion1, quaternion2);
        }

        #endregion Operators

        /// <summary>A quaternion with a value of 0,0,0,1</summary>
        public readonly static Quaternion Identity = new(0f, 0f, 0f, 1f);
    }
}
