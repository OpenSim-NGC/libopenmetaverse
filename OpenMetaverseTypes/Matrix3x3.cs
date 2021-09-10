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

namespace OpenMetaverse
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct  Matrix3x3 : IEquatable< Matrix3x3>
    {
        public float M11, M12, M13;
        public float M21, M22, M23;
        public float M31, M32, M33;

        #region Properties

        public Vector3 AtAxis
        {
            get
            {
                return new Vector3(M11, M21, M31);
            }
            set
            {
                M11 = value.X;
                M21 = value.Y;
                M31 = value.Z;
            }
        }

        public Vector3 LeftAxis
        {
            get
            {
                return new Vector3(M12, M22, M32);
            }
            set
            {
                M12 = value.X;
                M22 = value.Y;
                M32 = value.Z;
            }
        }

        public Vector3 UpAxis
        {
            get
            {
                return new Vector3(M13, M23, M33);
            }
            set
            {
                M13 = value.X;
                M23 = value.Y;
                M33 = value.Z;
            }
        }

        #endregion Properties

        #region Constructors

        public  Matrix3x3(
            float m11, float m12, float m13,
            float m21, float m22, float m23,
            float m31, float m32, float m33)
        {
            M11 = m11;
            M12 = m12;
            M13 = m13;

            M21 = m21;
            M22 = m22;
            M23 = m23;

            M31 = m31;
            M32 = m32;
            M33 = m33;
        }

        public  Matrix3x3(float roll, float pitch, float yaw)
        {
            this = CreateFromEulers(roll, pitch, yaw);
        }

        public  Matrix3x3( Matrix3x3 m)
        {
            M11 = m.M11;
            M12 = m.M12;
            M13 = m.M13;

            M21 = m.M21;
            M22 = m.M22;
            M23 = m.M23;

            M31 = m.M31;
            M32 = m.M32;
            M33 = m.M33;
        }

        #endregion Constructors

        #region Public Methods

        public float Determinant()
        {
            return Determinant3x3();
        }

        public float Determinant3x3()
        {
            float diag1 = M11 * M22 * M33;
            float diag2 = M12 * M23 * M31;
            float diag3 = M13 * M21 * M32;
            float diag4 = M31 * M22 * M13;
            float diag5 = M32 * M23 * M11;
            float diag6 = M33 * M21 * M12;

            return diag1 + diag2 + diag3 - (diag4 + diag5 + diag6);
        }

        public float Trace()
        {
            return M11 + M22 + M33;
        }

        /// <summary>
        /// Convert this matrix to euler rotations
        /// </summary>
        /// <param name="roll">X euler angle</param>
        /// <param name="pitch">Y euler angle</param>
        /// <param name="yaw">Z euler angle</param>
        public void GetEulerAngles(out float roll, out float pitch, out float yaw)
        {
            double angleX, angleY, angleZ;
            double cx, cy, cz; // cosines
            double sx, sz; // sines

            angleY = Math.Asin(Utils.Clamp(M13, -1f, 1f));
            cy = Math.Cos(angleY);

            if (Math.Abs(cy) > 0.005f)
            {
                // No gimbal lock
                cx = M33 / cy;
                sx = (-M23) / cy;

                angleX = (float)Math.Atan2(sx, cx);

                cz = M11 / cy;
                sz = (-M12) / cy;

                angleZ = (float)Math.Atan2(sz, cz);
            }
            else
            {
                // Gimbal lock
                angleX = 0;

                cz = M22;
                sz = M21;

                angleZ = Math.Atan2(sz, cz);
            }

            // Return only positive angles in [0,360]
            if (angleX < 0) angleX += 360d;
            if (angleY < 0) angleY += 360d;
            if (angleZ < 0) angleZ += 360d;

            roll = (float)angleX;
            pitch = (float)angleY;
            yaw = (float)angleZ;
        }

        /// <summary>
        /// Convert this matrix to a quaternion rotation
        /// </summary>
        /// <returns>A quaternion representation of this rotation matrix</returns>
        public Quaternion GetQuaternion()
        {
            Quaternion quat = new Quaternion();
            float trace = Trace() + 1f;

            if (trace > Single.Epsilon)
            {
                float s = 0.5f / (float)Math.Sqrt(trace);

                quat.X = (M32 - M23) * s;
                quat.Y = (M13 - M31) * s;
                quat.Z = (M21 - M12) * s;
                quat.W = 0.25f / s;
            }
            else
            {
                if (M11 > M22 && M11 > M33)
                {
                    float s = 2.0f * (float)Math.Sqrt(1.0f + M11 - M22 - M33);

                    quat.X = 0.25f * s;
                    quat.Y = (M12 + M21) / s;
                    quat.Z = (M13 + M31) / s;
                    quat.W = (M23 - M32) / s;
                }
                else if (M22 > M33)
                {
                    float s = 2.0f * (float)Math.Sqrt(1.0f + M22 - M11 - M33);

                    quat.X = (M12 + M21) / s;
                    quat.Y = 0.25f * s;
                    quat.Z = (M23 + M32) / s;
                    quat.W = (M13 - M31) / s;
                }
                else
                {
                    float s = 2.0f * (float)Math.Sqrt(1.0f + M33 - M11 - M22);

                    quat.X = (M13 + M31) / s;
                    quat.Y = (M23 + M32) / s;
                    quat.Z = 0.25f * s;
                    quat.W = (M12 - M21) / s;
                }
            }

            return quat;
        }

        public bool Decompose(out Vector3 scale, out Quaternion rotation)
        {
            float sx = (float)Math.Sqrt(M11 * M11 + M12 * M12 + M13 * M13);
            float sy = (float)Math.Sqrt(M21 * M21 + M22 * M22 + M23 * M23);
            float sz = (float)Math.Sqrt(M31 * M31 + M32 * M32 + M33 * M33);

            scale = new Vector3(sx, sy, sz);
            if (sx == 0.0 || sy == 0.0 || sz == 0.0)
            {
                rotation = Quaternion.Identity;
                return false;
            }

             Matrix3x3 m1 = new  Matrix3x3(
                                    M11 / sx, M12 / sx, M13 / sx,
                                    M21 / sy, M22 / sy, M23 / sy,
                                    M31 / sz, M32 / sz, M33 / sz);

            rotation = Quaternion.CreateFromRotationMatrix(m1);
            return true;
        }	

        #endregion Public Methods

        #region Static Methods

        public static  Matrix3x3 Add( Matrix3x3 matrix1,  Matrix3x3 matrix2)
        {
            return new Matrix3x3(
                matrix1.M11 + matrix2.M11,
                matrix1.M12 + matrix2.M12,
                matrix1.M13 + matrix2.M13,

                matrix1.M21 + matrix2.M21,
                matrix1.M22 + matrix2.M22,
                matrix1.M23 + matrix2.M23,

                matrix1.M31 + matrix2.M31,
                matrix1.M32 + matrix2.M32,
                matrix1.M33 + matrix2.M33
                );
        }

        public static  Matrix3x3 CreateFromAxisAngle(Vector3 axis, float angle)
        {

            float x = axis.X;
            float y = axis.Y;
            float z = axis.Z;
            float sin = (float)Math.Sin(angle);
            float cos = (float)Math.Cos(angle);
            float xx = x * x;
            float yy = y * y;
            float zz = z * z;
            float xy = x * y;
            float xz = x * z;
            float yz = y * z;

            return new Matrix3x3(
                xx + (cos * (1f - xx)),
                xy - (cos * xy) + (sin * z),
                xz - (cos * xz) - (sin * y),

                xy - (cos * xy) - (sin * z),
                yy + (cos * (1f - yy)),
                yz - (cos * yz) + (sin * x),

                xz - (cos * xz) + (sin * y),
                yz - (cos * yz) - (sin * x),
                zz + (cos * (1f - zz))
                );
        }

        /// <summary>
        /// Construct a matrix from euler rotation values in radians
        /// </summary>
        /// <param name="roll">X euler angle in radians</param>
        /// <param name="pitch">Y euler angle in radians</param>
        /// <param name="yaw">Z euler angle in radians</param>
        public static  Matrix3x3 CreateFromEulers(float roll, float pitch, float yaw)
        {
            float a = (float)Math.Cos(roll);
            float b = (float)Math.Sin(roll);
            float c = (float)Math.Cos(pitch);
            float d = (float)Math.Sin(pitch);
            float e = (float)Math.Cos(yaw);
            float f = (float)Math.Sin(yaw);

            float ad = a * d;
            float bd = b * d;
            return new Matrix3x3(
                c * e,
                -c * f,
                d,

                bd * e + a * f,
                -bd * f + a * e,
                -b * c,

                -ad * e + b * f,
                ad * f + b * e,
                a * c
                );
        }

        public static  Matrix3x3 CreateFromQuaternion(Quaternion rot)
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

            return new Matrix3x3(
                1.0f - yy2 - zz2,
                xy2 - wz2,
                xz2 + wy2,

                xy2 + wz2,
                1.0f - xx2 - zz2,
                yz2 - wx2,

                xz2 - wy2,
                yz2 + wx2,
                1.0f - xx2 - yy2
            );
        }

        public static  Matrix3x3 CreateRotationX(float radians)
        {
            float cos = (float)Math.Cos(radians);
            float sin = (float)Math.Sin(radians);

            return new Matrix3x3(
                1f, 0f, 0f,
                0f, cos, sin,
                0f, -sin, cos
                );
        }

        public static  Matrix3x3 CreateRotationY(float radians)
        {
            float cos = (float)Math.Cos(radians);
            float sin = (float)Math.Sin(radians);

            return new Matrix3x3(
                cos, 0f, -sin,
                0f, 1f, 0f,
                sin, 0f, cos
                );
        }

        public static  Matrix3x3 CreateRotationZ(float radians)
        {
            float cos = (float)Math.Cos(radians);
            float sin = (float)Math.Sin(radians);

            return new Matrix3x3(
                cos, sin, 0f,
                -sin, cos, 0f,
                0f, 0f, 1f
                );
        }

        public static  Matrix3x3 CreateScale(Vector3 scale)
        {
            return new Matrix3x3(
                scale.X, 0f, 0f,
                0f, scale.Y, 0f,
                0f, 0f, scale.Z
                );
        }

        public static  Matrix3x3 Divide( Matrix3x3 matrix1,  Matrix3x3 matrix2)
        {
            return new Matrix3x3(
                matrix1.M11 / matrix2.M11,
                matrix1.M12 / matrix2.M12,
                matrix1.M13 / matrix2.M13,

                matrix1.M21 / matrix2.M21,
                matrix1.M22 / matrix2.M22,
                matrix1.M23 / matrix2.M23,

                matrix1.M31 / matrix2.M31,
                matrix1.M32 / matrix2.M32,
                matrix1.M33 / matrix2.M33
            );
        }

        public static  Matrix3x3 Divide( Matrix3x3 matrix1, float divider)
        {
            float oodivider = 1f / divider;
            return new Matrix3x3(
                matrix1.M11 * oodivider,
                matrix1.M12 * oodivider,
                matrix1.M13 * oodivider,

                matrix1.M21 * oodivider,
                matrix1.M22 * oodivider,
                matrix1.M23 * oodivider,

                matrix1.M31 * oodivider,
                matrix1.M32 * oodivider,
                matrix1.M33 * oodivider
            );
        }

        public static  Matrix3x3 Lerp( Matrix3x3 matrix1,  Matrix3x3 matrix2, float amount)
        {
            return new Matrix3x3(
                matrix1.M11 + ((matrix2.M11 - matrix1.M11) * amount),
                matrix1.M12 + ((matrix2.M12 - matrix1.M12) * amount),
                matrix1.M13 + ((matrix2.M13 - matrix1.M13) * amount),

                matrix1.M21 + ((matrix2.M21 - matrix1.M21) * amount),
                matrix1.M22 + ((matrix2.M22 - matrix1.M22) * amount),
                matrix1.M23 + ((matrix2.M23 - matrix1.M23) * amount),

                matrix1.M31 + ((matrix2.M31 - matrix1.M31) * amount),
                matrix1.M32 + ((matrix2.M32 - matrix1.M32) * amount),
                matrix1.M33 + ((matrix2.M33 - matrix1.M33) * amount)
                );
        }

        public static  Matrix3x3 Multiply( Matrix3x3 matrix1,  Matrix3x3 matrix2)
        {
            return new Matrix3x3(
                matrix1.M11 * matrix2.M11 + matrix1.M12 * matrix2.M21 + matrix1.M13 * matrix2.M31,
                matrix1.M11 * matrix2.M12 + matrix1.M12 * matrix2.M22 + matrix1.M13 * matrix2.M32,
                matrix1.M11 * matrix2.M13 + matrix1.M12 * matrix2.M23 + matrix1.M13 * matrix2.M33,

                matrix1.M21 * matrix2.M11 + matrix1.M22 * matrix2.M21 + matrix1.M23 * matrix2.M31,
                matrix1.M21 * matrix2.M12 + matrix1.M22 * matrix2.M22 + matrix1.M23 * matrix2.M32,
                matrix1.M21 * matrix2.M13 + matrix1.M22 * matrix2.M23 + matrix1.M23 * matrix2.M33,

                matrix1.M31 * matrix2.M11 + matrix1.M32 * matrix2.M21 + matrix1.M33 * matrix2.M31,
                matrix1.M31 * matrix2.M12 + matrix1.M32 * matrix2.M22 + matrix1.M33 * matrix2.M32,
                matrix1.M31 * matrix2.M13 + matrix1.M32 * matrix2.M23 + matrix1.M33 * matrix2.M33
            );
        }

        public static  Matrix3x3 Multiply( Matrix3x3 matrix1, float scaleFactor)
        {
            Matrix3x3 matrix;
            matrix.M11 = matrix1.M11 * scaleFactor;
            matrix.M12 = matrix1.M12 * scaleFactor;
            matrix.M13 = matrix1.M13 * scaleFactor;

            matrix.M21 = matrix1.M21 * scaleFactor;
            matrix.M22 = matrix1.M22 * scaleFactor;
            matrix.M23 = matrix1.M23 * scaleFactor;

            matrix.M31 = matrix1.M31 * scaleFactor;
            matrix.M32 = matrix1.M32 * scaleFactor;
            matrix.M33 = matrix1.M33 * scaleFactor;

            return matrix;
        }

        public static  Matrix3x3 Negate( Matrix3x3 matrix)
        {
            return new Matrix3x3(
                -matrix.M11, -matrix.M12, -matrix.M13,
                -matrix.M21, -matrix.M22, -matrix.M23,
                -matrix.M31, -matrix.M32, -matrix.M33
                );
        }

        public static  Matrix3x3 Subtract( Matrix3x3 matrix1,  Matrix3x3 matrix2)
        {
            return new Matrix3x3(
                matrix1.M11 - matrix2.M11,
                matrix1.M12 - matrix2.M12,
                matrix1.M13 - matrix2.M13,

                matrix1.M21 - matrix2.M21,
                matrix1.M22 - matrix2.M22,
                matrix1.M23 - matrix2.M23,

                matrix1.M31 - matrix2.M31,
                matrix1.M32 - matrix2.M32,
                matrix1.M33 - matrix2.M33
                );
        }

        public static  Matrix3x3 Transform( Matrix3x3 value, Quaternion rotation)
        {
            float x2 = rotation.X + rotation.X;
            float y2 = rotation.Y + rotation.Y;
            float z2 = rotation.Z + rotation.Z;

            float a = (1f - rotation.Y * y2) - rotation.Z * z2;
            float b = rotation.X * y2 - rotation.W * z2;
            float c = rotation.X * z2 + rotation.W * y2;
            float d = rotation.X * y2 + rotation.W * z2;
            float e = (1f - rotation.X * x2) - rotation.Z * z2;
            float f = rotation.Y * z2 - rotation.W * x2;
            float g = rotation.X * z2 - rotation.W * y2;
            float h = rotation.Y * z2 + rotation.W * x2;
            float i = (1f - rotation.X * x2) - rotation.Y * y2;

            return new Matrix3x3(
                value.M11 * a + value.M12 * b + value.M13 * c,
                value.M11 * d + value.M12 * e + value.M13 * f,
                value.M11 * g + value.M12 * h + value.M13 * i,

                value.M21 * a + value.M22 * b + value.M23 * c,
                value.M21 * d + value.M22 * e + value.M23 * f,
                value.M21 * g + value.M22 * h + value.M23 * i,

                value.M31 * a + value.M32 * b + value.M33 * c,
                value.M31 * d + value.M32 * e + value.M33 * f,
                value.M31 * g + value.M32 * h + value.M33 * i
                );
        }

        public static  Matrix3x3 Transpose( Matrix3x3 matrix)
        {
            return new Matrix3x3(
                matrix.M11,
                matrix.M21,
                matrix.M31,
                matrix.M12,
                matrix.M22,
                matrix.M32,
                matrix.M13,
                matrix.M23,
                matrix.M33
            );
        }

        public static  Matrix3x3 Inverse3x3(Matrix3x3 matrix)
        {
            if (matrix.Determinant3x3() == 0f)
                throw new ArgumentException("Singular matrix inverse not possible");

            return (Adjoint3x3(matrix) / matrix.Determinant3x3());
        }

        public static  Matrix3x3 Adjoint3x3( Matrix3x3 matrix)
        {
             Matrix3x3 adjointMatrix = new  Matrix3x3();
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                    adjointMatrix[i,j] = (float)(Math.Pow(-1, i + j) * (Minor(matrix, i, j).Determinant3x3()));
            }

            adjointMatrix = Transpose(adjointMatrix);
            return adjointMatrix;
        }

        public static  Matrix3x3 Inverse( Matrix3x3 matrix)
        {
            if (matrix.Determinant() == 0f)
                throw new ArgumentException("Singular matrix inverse not possible");

            return (Adjoint(matrix) / matrix.Determinant());
        }

        public static  Matrix3x3 Adjoint( Matrix3x3 matrix)
        {
             Matrix3x3 adjointMatrix = new  Matrix3x3();
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                    adjointMatrix[i,j] = (float)(Math.Pow(-1, i + j) * ((Minor(matrix, i, j)).Determinant3x3()));
            }

            adjointMatrix = Transpose(adjointMatrix);
            return adjointMatrix;
        }

        public static  Matrix3x3 Minor(Matrix3x3 matrix, int row, int col)
        {
            Matrix3x3 minor = new  Matrix3x3();
            int m = 0, n = 0;

            for (int i = 0; i < 3; i++)
            {
                if (i == row)
                    continue;
                n = 0;
                for (int j = 0; j < 3; j++)
                {
                    if (j == col)
                        continue;
                    minor[m,n] = matrix[i,j];
                    n++;
                }
                m++;
            }

            return minor;
        }
        
        #endregion Static Methods

        #region Overrides

        public override bool Equals(object obj)
        {
            return (obj is  Matrix3x3) ? this.Equals(( Matrix3x3)obj) : false;
        }

        public bool Equals( Matrix3x3 other)
        {
            return M11 == other.M11 && M12 == other.M12 && M13 == other.M13 &&
                   M21 == other.M21 && M22 == other.M22 && M23 == other.M23 &&
                   M31 == other.M31 && M32 == other.M32 && M33 == other.M33;
        }

        public override int GetHashCode()
        {
            return
                M11.GetHashCode() ^ M12.GetHashCode() ^ M13.GetHashCode() ^
                M21.GetHashCode() ^ M22.GetHashCode() ^ M23.GetHashCode() ^
                M31.GetHashCode() ^ M32.GetHashCode() ^ M33.GetHashCode();
        }

        /// <summary>
        /// Get a formatted string representation of the vector
        /// </summary>
        /// <returns>A string representation of the vector</returns>
        public override string ToString()
        {
            return string.Format(Utils.EnUsCulture,
                "|{0}, {1}, {2}|\n|{3}, {4}, {5}|\n|{6}, {7} {8}|\n",
                M11, M12, M13, M21, M22, M23, M31, M32, M33);
        }

        #endregion Overrides

        #region Operators

        public static bool operator ==( Matrix3x3 left,  Matrix3x3 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=( Matrix3x3 left,  Matrix3x3 right)
        {
            return !left.Equals(right);
        }

        public static  Matrix3x3 operator +( Matrix3x3 left,  Matrix3x3 right)
        {
            return Add(left, right);
        }

        public static  Matrix3x3 operator -( Matrix3x3 matrix)
        {
            return Negate(matrix);
        }

        public static  Matrix3x3 operator -( Matrix3x3 left,  Matrix3x3 right)
        {
            return Subtract(left, right);
        }

        public static  Matrix3x3 operator *( Matrix3x3 left,  Matrix3x3 right)
        {
            return Multiply(left, right);
        }

        public static  Matrix3x3 operator *( Matrix3x3 left, float scalar)
        {
            return Multiply(left, scalar);
        }

        public static  Matrix3x3 operator /( Matrix3x3 left,  Matrix3x3 right)
        {
            return Divide(left, right);
        }

        public static  Matrix3x3 operator /( Matrix3x3 matrix, float divider)
        {
            return Divide(matrix, divider);
        }

        public Vector3 this[int row]
        {
            get
            {
                switch (row)
                {
                    case 0:
                        return new Vector3(M11, M12, M13);
                    case 1:
                        return new Vector3(M21, M22, M23);
                    case 2:
                        return new Vector3(M31, M32, M33);
                    default:
                        throw new IndexOutOfRangeException("Matrix3x3 row index must be from 0-2");
                }
            }
            set
            {
                switch (row)
                {
                    case 0:
                        M11 = value.X;
                        M12 = value.Y;
                        M13 = value.Z;
                        break;
                    case 1:
                        M21 = value.X;
                        M22 = value.Y;
                        M23 = value.Z;
                        break;
                    case 2:
                        M31 = value.X;
                        M32 = value.Y;
                        M33 = value.Z;
                        break;
                    default:
                        throw new IndexOutOfRangeException("Matrix3x3 row index must be from 0-2");
                }
            }
        }

        public float this[int row, int column]
        {
            get
            {
                switch (row)
                {
                    case 0:
                        switch (column)
                        {
                            case 0:
                                return M11;
                            case 1:
                                return M12;
                            case 2:
                                return M13;
                            default:
                                throw new IndexOutOfRangeException(" Matrix3x3 row and column values must be from 0-2");
                        }
                    case 1:
                        switch (column)
                        {
                            case 0:
                                return M21;
                            case 1:
                                return M22;
                            case 2:
                                return M23;
                            default:
                                throw new IndexOutOfRangeException(" Matrix3x3 row and column values must be from 0-2");
                        }
                    case 2:
                        switch (column)
                        {
                            case 0:
                                return M31;
                            case 1:
                                return M32;
                            case 2:
                                return M33;
                            default:
                                throw new IndexOutOfRangeException(" Matrix3x3 row and column values must be from 0-2");
                        }
                    default:
                        throw new IndexOutOfRangeException(" Matrix3x3 row and column values must be from 0-2");
                }
            }
            set
            {
                switch (row)
                {
                    case 0:
                        switch (column)
                        {
                            case 0:
                                M11 = value; return;
                            case 1:
                                M12 = value; return;
                            case 2:
                                M13 = value; return;
                            default:
                                throw new IndexOutOfRangeException(" Matrix3x3 row and column values must be from 0-2");
                        }
                    case 1:
                        switch (column)
                        {
                            case 0:
                                M21 = value; return;
                            case 1:
                                M22 = value; return;
                            case 2:
                                M23 = value; return;
                            default:
                                throw new IndexOutOfRangeException(" Matrix3x3 row and column values must be from 0-2");
                        }
                    case 2:
                        switch (column)
                        {
                            case 0:
                                M31 = value; return;
                            case 1:
                                M32 = value; return;
                            case 2:
                                M33 = value; return;
                            default:
                                throw new IndexOutOfRangeException(" Matrix3x3 row and column values must be from 0-2");
                        }
                    default:
                        throw new IndexOutOfRangeException(" Matrix3x3 row and column values must be from 0-2");
                }
            }
        }

        #endregion Operators

        /// <summary>A 3x3 matrix containing all zeroes</summary>
        public static readonly  Matrix3x3 Zero = new  Matrix3x3();

        /// <summary>A 3x3 identity matrix</summary>
        public static readonly  Matrix3x3 Identity = new  Matrix3x3(
            1f, 0f, 0f,
            0f, 1f, 0f,
            0f, 0f, 1f);
    }
}
