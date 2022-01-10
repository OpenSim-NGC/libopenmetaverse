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
            W = (xyzsum > 1e-6f) ? (float)Math.Sqrt(xyzsum) : 0;
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
            X = q.X;
            Y = q.Y;
            Z = q.Z;
            W = q.W;
        }

        #endregion Constructors

        #region Public Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ApproxEquals(Quaternion quat)
        {
            // assume normalized
            return Math.Abs(quat.X - X) < 1e-6f &&
                    Math.Abs(quat.Y - Y) < 1e-6f &&
                    Math.Abs(quat.Z - Z) < 1e-6f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ApproxEquals(Quaternion quat, float tolerance)
        {
            // assume normalized
            return Math.Abs(quat.X - X) < tolerance &&
                    Math.Abs(quat.Y - Y) < tolerance &&
                    Math.Abs(quat.Z - Z) < tolerance;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsIdentity()
        {
            // assume normalized
            if(W > (1.0f - 1e-6f))
                return true;
            if (W < -(1.0f - 1e-6f))
                return true;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsIdentityOrZero()
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
        public float Length()
        {
            return (float)Math.Sqrt((X * X) + (Y * Y) + (Z * Z) + (W * W));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float LengthSquared()
        {
            return (X * X) + (Y * Y) + (Z * Z) + (W * W);
        }

        /// <summary>
        /// Normalizes the quaternion
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Normalize()
        {
            float mag = LengthSquared();
            if (mag > 1e-6f)
            {
                float oomag = 1f / (float)Math.Sqrt(mag);
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
                X = X * len;
                Y = Y * len;
                Z = Z * len;
                W = -W * len;
            }
            else
            {
                X = 0f;
                Y = 0f;
                Z = 0f;
                W = 1f;
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
                W = (xyzsum > 1e-6f) ? (float)Math.Sqrt(xyzsum) : 0f;
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
        public byte[] GetBytes()
        {
            byte[] bytes = new byte[12];
            float norm = LengthSquared();
            if (norm > 1e-6f || W < 0.9999f)
            {
                if (W < 0f)
                    norm = -1f / (float)Math.Sqrt(norm);
                else
                    norm = 1f / (float)Math.Sqrt(norm);
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
        public void ToBytes(byte[] dest, int pos)
        {
            float norm = LengthSquared();
            if (norm > 1e-6f || norm < 0.9999f)
            {
                if (W < 0f)
                    norm = -1f / (float)Math.Sqrt(norm);
                else
                    norm = 1f / (float)Math.Sqrt(norm);
                Utils.FloatToBytesSafepos(norm * X, dest, pos);
                Utils.FloatToBytesSafepos(norm * Y, dest, pos + 4);
                Utils.FloatToBytesSafepos(norm * Z, dest, pos + 8);
            }
            else
            {
                Utils.FloatToBytesSafepos(0, dest, pos);
                Utils.FloatToBytesSafepos(0, dest, pos + 4);
                Utils.FloatToBytesSafepos(0, dest, pos + 8);
            }
        }

        /// <summary>
        /// Convert this quaternion to euler angles
        /// </summary>
        /// <param name="roll">X euler angle</param>
        /// <param name="pitch">Y euler angle</param>
        /// <param name="yaw">Z euler angle</param>
        public void GetEulerAngles(out float roll, out float pitch, out float yaw)
        {
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
            if (Math.Abs(m) < 0.0001f)
                return;

            float n = 2 * (Y * W + X * Z);
            float p = m * m - n * n;

            if (p > 0f)
            {
                roll = (float)Math.Atan2(2.0f * (X * W - Y * Z), (-tX - tY + tZ + tW));
                pitch = (float)Math.Atan2(n, Math.Sqrt(p));
                yaw = (float)Math.Atan2(2.0f * (Z * W - X * Y), tX - tY - tZ + tW);
            }
            else if (n > 0f)
            {
                roll = 0f;
                pitch = (float)(Math.PI / 2d);
                yaw = (float)Math.Atan2((Z * W + X * Y), 0.5f - tX - tY);
            }
            else
            {
                roll = 0f;
                pitch = -(float)(Math.PI / 2d);
                yaw = (float)Math.Atan2((Z * W + X * Y), 0.5f - tX - tZ);
            }
        }

        /// <summary>
        /// Convert this quaternion to an angle around an axis
        /// </summary>
        /// <param name="axis">Unit vector describing the axis</param>
        /// <param name="angle">Angle around the axis, in radians</param>
        public void GetAxisAngle(out Vector3 axis, out float angle)
        {
            Normalize();
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
                angle = (float)Math.PI;
                return;
            }

            float sin = (float)Math.Sqrt(1.0f - ww);
            float invSin = 1.0f / sin;
            if (W < 0)
                invSin = -invSin;
            axis = new Vector3(X, Y, Z) * invSin;

            angle = 2.0f * (float)Math.Acos(W);
            if (angle > Math.PI)
                angle = 2.0f * (float)Math.PI - angle;
        }

        #endregion Public Methods

        #region Static Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quaternion Add(Quaternion quaternion1, Quaternion quaternion2)
        {
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
        public static Quaternion Conjugate(Quaternion quaternion)
        {
            return new Quaternion( -quaternion.X, -quaternion.Y, -quaternion.Z, quaternion.W);
        }

        /// <summary>
        /// Build a quaternion from an axis and an angle of rotation around
        /// that axis
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quaternion CreateFromAxisAngle(float axisX, float axisY, float axisZ, float angle)
        {
            Vector3 axis = new Vector3(axisX, axisY, axisZ);
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
            float c = (float)Math.Cos(angle);
            float s = (float)Math.Sin(angle);

            return new Quaternion(axis.X * s, axis.Y * s, axis.Z * s, c);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quaternion CreateRotationX(float angle)
        {
            angle *= 0.5f;
            float c = (float)Math.Cos(angle);
            float s = (float)Math.Sin(angle);

            return new Quaternion(s, 0, 0, c);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quaternion CreateRotationY(float angle)
        {
            angle *= 0.5f;
            float c = (float)Math.Cos(angle);
            float s = (float)Math.Sin(angle);

            return new Quaternion(0, s, 0, c);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quaternion CreateRotationZ(float angle)
        {
            angle *= 0.5f;
            float c = (float)Math.Cos(angle);
            float s = (float)Math.Sin(angle);

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
            double atCos = Math.Cos(roll);
            double atSin = Math.Sin(roll);
            pitch *= 0.5f;
            double leftCos = Math.Cos(pitch);
            double leftSin = Math.Sin(pitch);
            yaw *= 0.5f;
            double upCos = Math.Cos(yaw);
            double upSin = Math.Sin(yaw);

            double atLeftCos = atCos * leftCos;
            double atLeftSin = atSin * leftSin;
            return new Quaternion(
                (float)(atSin * leftCos * upCos + atCos * leftSin * upSin),
                (float)(atCos * leftSin * upCos - atSin * leftCos * upSin),
                (float)(atLeftCos * upSin + atLeftSin * upCos),
                (float)(atLeftCos * upCos - atLeftSin * upSin)
            );
        }

        public static Quaternion CreateFromRotationMatrix(Matrix3x3 matrix)
        {
            float num = matrix.M11 + matrix.M22 + matrix.M33;
            float n2;
            if (num >= 0f)
            {
                num = (float)Math.Sqrt((num + 1f));
                n2 = 0.5f / num;
                return new Quaternion(
                        (matrix.M23 - matrix.M32) * n2,
                        (matrix.M31 - matrix.M13) * n2,
                        (matrix.M12 - matrix.M21) * n2,
                        num * 0.5f);
            }
            if ((matrix.M11 >= matrix.M22) && (matrix.M11 >= matrix.M33))
            {
                num = (float)Math.Sqrt((((1f + matrix.M11) - matrix.M22) - matrix.M33));
                n2 = 0.5f / num;
                return new Quaternion(
                        0.5f * num,
                        (matrix.M12 + matrix.M21) * n2,
                        (matrix.M13 + matrix.M31) * n2,
                        (matrix.M23 - matrix.M32) * n2);
            }
            if (matrix.M22 > matrix.M33)
            {
                num = (float)Math.Sqrt((((1f + matrix.M22) - matrix.M11) - matrix.M33));
                n2 = 0.5f / num;
                return new Quaternion(
                        (matrix.M21 + matrix.M12) * n2,
                        0.5f * num,
                        (matrix.M32 + matrix.M23) * n2,
                        (matrix.M31 - matrix.M13) * n2);
            }

            num = (float)Math.Sqrt((((1f + matrix.M33) - matrix.M11) - matrix.M22));
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
                num = (float)Math.Sqrt((num + 1f));
                n2 = 0.5f / num;
                return new Quaternion(
                        (matrix.M23 - matrix.M32) * n2,
                        (matrix.M31 - matrix.M13) * n2,
                        (matrix.M12 - matrix.M21) * n2,
                        num * 0.5f);
            }
            if ((matrix.M11 >= matrix.M22) && (matrix.M11 >= matrix.M33))
            {
                num = (float)Math.Sqrt((((1f + matrix.M11) - matrix.M22) - matrix.M33));
                n2 = 0.5f / num;
                return new Quaternion(
                        0.5f * num,
                        (matrix.M12 + matrix.M21) * n2,
                        (matrix.M13 + matrix.M31) * n2,
                        (matrix.M23 - matrix.M32) * n2);
            }
            if (matrix.M22 > matrix.M33)
            {
                num = (float)Math.Sqrt((((1f + matrix.M22) - matrix.M11) - matrix.M33));
                n2 = 0.5f / num;
                return new Quaternion(
                        (matrix.M21 + matrix.M12) * n2,
                        0.5f * num,
                        (matrix.M32 + matrix.M23) * n2,
                        (matrix.M31 - matrix.M13) * n2);
            }

            num = (float)Math.Sqrt((((1f + matrix.M33) - matrix.M11) - matrix.M22));
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
        public static Quaternion Slerp(Quaternion q1, Quaternion q2, float amount)
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
                    float theta = (float)Math.Acos(angle);
                    float invsintheta = 1f / (float)Math.Sin(theta);
                    scale = (float)Math.Sin(theta * (1f - amount)) * invsintheta;
                    invscale = (float)Math.Sin(theta * amount) * invsintheta;
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
                q2.X = -q1.Y;
                q2.Y = q1.X;
                q2.Z = -q1.W;
                q2.W = q1.Z;

                scale = (float)Math.Sin(Utils.PI * (0.5f - amount));
                invscale = (float)Math.Sin(Utils.PI * amount);
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
                float oomag = 1f / (float)Math.Sqrt(mag);
                return new Quaternion(
                    q.X * oomag,
                    q.Y * oomag,
                    q.Z * oomag,
                    q.W * oomag);
            }
            return Quaternion.Identity;
        }

        public static Quaternion Parse(string val)
        {
            char[] splitChar = { ',' };
            string[] split = val.Replace("<", String.Empty).Replace(">", String.Empty).Split(splitChar);
            if (split.Length == 3)
            {
                return new Quaternion(
                    float.Parse(split[0].Trim(), Utils.EnUsCulture),
                    float.Parse(split[1].Trim(), Utils.EnUsCulture),
                    float.Parse(split[2].Trim(), Utils.EnUsCulture));
            }
            else
            {
                return new Quaternion(
                    float.Parse(split[0].Trim(), Utils.EnUsCulture),
                    float.Parse(split[1].Trim(), Utils.EnUsCulture),
                    float.Parse(split[2].Trim(), Utils.EnUsCulture),
                    float.Parse(split[3].Trim(), Utils.EnUsCulture));
            }
        }

        public static bool TryParse(string val, out Quaternion result)
        {
            try
            {
                result = Parse(val);
                return true;
            }
            catch (Exception)
            {
                result = new Quaternion();
                return false;
            }
        }

        #endregion Static Methods

        #region Overrides

        public override bool Equals(object obj)
        {
            if(!(obj is Quaternion))
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

        public bool Equals(Quaternion other)
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

        public override int GetHashCode()
        {
            int hash = X.GetHashCode();
            hash = Utils.CombineHash(hash, Y.GetHashCode());
            hash = Utils.CombineHash(hash, Z.GetHashCode());
            hash = Utils.CombineHash(hash, W.GetHashCode());
            return hash;
        }

        public override string ToString()
        {
            return String.Format(Utils.EnUsCulture, "<{0}, {1}, {2}, {3}>", X, Y, Z, W);
        }

        /// <summary>
        /// Get a string representation of the quaternion elements with up to three
        /// decimal digits and separated by spaces only
        /// </summary>
        /// <returns>Raw string representation of the quaternion</returns>
        public string ToRawString()
        {
            CultureInfo enUs = new CultureInfo("en-us");
            enUs.NumberFormat.NumberDecimalDigits = 3;

            return String.Format(enUs, "{0} {1} {2} {3}", X, Y, Z, W);
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
        public readonly static Quaternion Identity = new Quaternion(0f, 0f, 0f, 1f);
    }
}
