/*
 * Copyright (c) Contributors
 * See CONTRIBUTORS.TXT for a full list of copyright holders.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of the OpenSimulator Project nor the
 *       names of its contributors may be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE DEVELOPERS ``AS IS'' AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE CONTRIBUTORS BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace PrimMesher
{
    public struct UVCoord(float u, float v)
    {
        public float U = u;
        public float V = v;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UVCoord Flip()
        {
            U = 1.0f - U;
            V = 1.0f - V;
            return this;
        }
    }

    public struct Face
    {
        // vertices
        public int v1;
        public int v2;
        public int v3;

        //normals
        public int n1;
        public int n2;
        public int n3;

        // uvs
        public int uv1;
        public int uv2;
        public int uv3;

        public Face(int v1, int v2, int v3)
        {
            this.v1 = v1;
            this.v2 = v2;
            this.v3 = v3;

            n1 = 0;
            n2 = 0;
            n3 = 0;

            uv1 = 0;
            uv2 = 0;
            uv3 = 0;
        }

        public Face(int v1, int v2, int v3, int n1, int n2, int n3)
        {
            this.v1 = v1;
            this.v2 = v2;
            this.v3 = v3;

            this.n1 = n1;
            this.n2 = n2;
            this.n3 = n3;

            uv1 = 0;
            uv2 = 0;
            uv3 = 0;
        }

        public Vector3 SurfaceNormal(List<Vector3> coordList)
        {
            Vector3 c1 = coordList[v1];
            Vector3 c2 = coordList[v2];
            Vector3 c3 = coordList[v3];

            Vector3 edge1 = c2 - c1;
            Vector3 edge2 = c3 - c1;
            c1 = Vector3.Cross(edge1, edge2);
            c1.Normalize();
            return c1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddVerticesIndexesOffset(int o)
        {
            v1 += o;
            v2 += o;
            v3 += o;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddNormalsIndexesOffset(int o)
        {
            n1 += o;
            n2 += o;
            n3 += o;
        }
    }

    public struct ViewerFace
    {
        public int primFaceNumber;

        public Vector3 v1;
        public Vector3 v2;
        public Vector3 v3;

        public int coordIndex1;
        public int coordIndex2;
        public int coordIndex3;

        public Vector3 n1;
        public Vector3 n2;
        public Vector3 n3;

        public UVCoord uv1;
        public UVCoord uv2;
        public UVCoord uv3;

        public ViewerFace(int primFaceNumber)
        {
            this.primFaceNumber = primFaceNumber;

            v1 = new();
            v2 = new();
            v3 = new();

            coordIndex1 = coordIndex2 = coordIndex3 = -1; // -1 means not assigned yet

            n1 = new();
            n2 = new();
            n3 = new();
            
            uv1 = new();
            uv2 = new();
            uv3 = new();
        }

        public void Scale(float x, float y, float z)
        {
            v1.X *= x;
            v1.Y *= y;
            v1.Z *= z;

            v2.X *= x;
            v2.Y *= y;
            v2.Z *= z;

            v3.X *= x;
            v3.Y *= y;
            v3.Z *= z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Scale(Vector3 scale)
        {
            v1 *= scale;
            v2 *= scale;
            v3 *= scale;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddPos(float x, float y, float z)
        {
            v1.X += x;
            v2.X += x;
            v3.X += x;

            v1.Y += y;
            v2.Y += y;
            v3.Y += y;

            v1.Z += z;
            v2.Z += z;
            v3.Z += z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddPos(Vector3 v)
        {
            v1 += v;
            v2 += v;
            v3 += v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRot(Quaternion q)
        {
            v1 *= q;
            v2 *= q;
            v3 *= q;
            
            n1 *= q;
            n2 *= q;
            n3 *= q;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CalcSurfaceNormal()
        {
            Vector3 edge1 = v2 - v1;
            Vector3 edge2 = v3 - v1;
            n1= Vector3.Cross(edge1, edge2);
            n1.Normalize();

            n2 = n3 = n1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool IsValid()
        {
            return
	            (MathF.Abs(n1.X) + MathF.Abs(n1.Y) + MathF.Abs(n1.Z) > 0.2f) &&
	            (MathF.Abs(n2.X) + MathF.Abs(n2.Y) + MathF.Abs(n2.Z) > 0.2f) &&
    	        (MathF.Abs(n3.X) + MathF.Abs(n3.Y) + MathF.Abs(n3.Z) > 0.2f)
                ;
        }
    }

    internal struct Angle
    {
        internal float angle;
        internal float X;
        internal float Y;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Angle(float _angle, float x, float y)
        {
            angle = _angle;
            X = x;
            Y = y;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Angle(float _angle)
        {
            angle = _angle;
            X = MathF.Cos(angle); // cos
            Y = MathF.Sin(angle); // sin
        }
    }

    internal class AngleList
    {
        private float iX, iY; // intersection point

        private static readonly Angle[] angles3 =
        {
            new(0.0f, 1.0f, 0.0f),
            new(0.33333333333333333f, -0.5f, 0.86602540378443871f),
            new(0.66666666666666667f, -0.5f, -0.86602540378443837f),
            new(1.0f, 1.0f, 0.0f)
        };

        private static Vector3[] normals3 =
        {
            Vector3.Normalize(0.25f, 0.4330127019f, 0.0f),
            Vector3.Normalize(-0.5f, 0.0f, 0.0f),
            Vector3.Normalize(0.25f, -0.4330127019f, 0.0f),
            Vector3.Normalize(0.25f, 0.4330127019f, 0.0f)
        };

        private static Angle[] angles4 =
        {
            new(0.0f, 1.0f, 0.0f),
            new(0.25f, 0.0f, 1.0f),
            new(0.5f, -1.0f, 0.0f),
            new(0.75f, 0.0f, -1.0f),
            new(1.0f, 1.0f, 0.0f)
        };

        private static readonly Vector3[] normals4 = 
        {
            Vector3.Normalize(0.5f, 0.5f, 0.0f),
            Vector3.Normalize(-0.5f, 0.5f, 0.0f),
            Vector3.Normalize(-0.5f, -0.5f, 0.0f),
            Vector3.Normalize(0.5f, -0.5f, 0.0f),
            Vector3.Normalize(0.5f, 0.5f, 0.0f)
        };

        private static readonly Angle[] angles6 =
        {
            new(0.0f, 1.0f, 0.0f),
            new(0.16666666666666667f, 0.5f, 0.8660254037844386f),
            new(0.33333333333333333f, -0.5f, 0.86602540378443871f),
            new(0.5f, -1.0f, 0.0f),
            new(0.66666666666666667f, -0.5f, -0.86602540378443837f),
            new(0.83333333333333326f, 0.5f, -0.86602540378443904f),
            new(1.0f, 1.0f, 0.0f)
        };

        private static readonly Angle[] angles12 =
        {
            new(0.0f, 1.0f, 0.0f),
            new(0.083333333333333329f, 0.86602540378443871f, 0.5f),
            new(0.16666666666666667f, 0.5f, 0.8660254037844386f),
            new(0.25f, 0.0f, 1.0f),
            new(0.33333333333333333f, -0.5f, 0.86602540378443871f),
            new(0.41666666666666663f, -0.86602540378443849f, 0.5f),
            new(0.5f, -1.0f, 0.0f),
            new(0.58333333333333326f, -0.86602540378443882f, -0.5f),
            new(0.66666666666666667f, -0.5f, -0.86602540378443837f),
            new(0.75f, 0.0f, -1.0f),
            new(0.83333333333333326f, 0.5f, -0.86602540378443904f),
            new(0.91666666666666663f, 0.86602540378443837f, -0.5f),
            new(1.0f, 1.0f, 0.0f)
        };

        private static readonly Angle[] angles24 =
        {
            new(0.0f, 1.0f, 0.0f),
            new(0.041666666666666664f, 0.96592582628906831f, 0.25881904510252074f),
            new(0.083333333333333329f, 0.86602540378443871f, 0.5f),
            new(0.125f, 0.70710678118654757f, 0.70710678118654746f),
            new(0.16666666666666667f, 0.5f, 0.8660254037844386f),
            new(0.20833333333333331f, 0.25881904510252096f, 0.9659258262890682f),
            new(0.25f, 0.0f, 1.0f),
            new(0.29166666666666663f, -0.25881904510252063f, 0.96592582628906831f),
            new(0.33333333333333333f, -0.5f, 0.86602540378443871f),
            new(0.375f, -0.70710678118654746f, 0.70710678118654757f),
            new(0.41666666666666663f, -0.86602540378443849f, 0.5f),
            new(0.45833333333333331f, -0.9659258262890682f, 0.25881904510252102f),
            new(0.5f, -1.0f, 0.0f),
            new(0.54166666666666663f, -0.96592582628906842f, -0.25881904510252035f),
            new(0.58333333333333326f, -0.86602540378443882f, -0.5f),
            new(0.62499999999999989f, -0.70710678118654791f, -0.70710678118654713f),
            new(0.66666666666666667f, -0.5f, -0.86602540378443837f),
            new(0.70833333333333326f, -0.25881904510252152f, -0.96592582628906809f),
            new(0.75f, 0.0f, -1.0f),
            new(0.79166666666666663f, 0.2588190451025203f, -0.96592582628906842f),
            new(0.83333333333333326f, 0.5f, -0.86602540378443904f),
            new(0.875f, 0.70710678118654735f, -0.70710678118654768f),
            new(0.91666666666666663f, 0.86602540378443837f, -0.5f),
            new(0.95833333333333326f, 0.96592582628906809f, -0.25881904510252157f),
            new(1.0f, 1.0f, 0.0f)
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Angle interpolatePoints(float newPoint, Angle p1, Angle p2)
        {
            float m = (newPoint - p1.angle) / (p2.angle - p1.angle);
            return new(newPoint, p1.X + m * (p2.X - p1.X), p1.Y + m * (p2.Y - p1.Y));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void intersection(float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4)
        { // ref: https://paulbourke.net/geometry/pointlineplane/
            float denom = (y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1);
            if (denom != 0.0f)
            {
                float uaNumerator = (x4 - x3) * (y1 - y3) - (y4 - y3) * (x1 - x3);
                float ua = uaNumerator / denom;
                iX = x1 + ua * (x2 - x1);
                iY = y1 + ua * (y2 - y1);
            }
        }

        // if p3 is origin:
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void intersection(float x1, float y1, float x2, float y2, float x4, float y4)
        { // https://paulbourke.net/geometry/pointlineplane/
            float denom = y4 * (x2 - x1) - x4 * (y2 - y1);
            if (denom != 0.0f)
            {
                float uaNumerator = x4 * y1 - y4 * x1;
                float ua = uaNumerator / denom;
                iX = x1 + ua * (x2 - x1);
                iY = y1 + ua * (y2 - y1);
            }
        }

        internal List<Angle> angles;
        internal List<Vector3> normals;

        internal void makeAngles(int sides, float startAngle, float stopAngle)
        {
            angles = new List<Angle>();
            normals = new List<Vector3>();

            const float twoPi = MathF.PI * 2.0f;
            const float twoPiInv = 1.0f / (float)twoPi;

            if (sides < 1)
                throw new Exception("number of sides not greater than zero");
            if (stopAngle <= startAngle)
                throw new Exception("stopAngle not greater than startAngle");

            Angle[] sourceAngles = sides switch
            {
                3 => angles3,
                4 => angles4,
                6 => angles6,
                12 => angles12,
                24 => angles24,
                _ => null
            };

            if (sourceAngles != null)
            {
                startAngle *= twoPiInv;
                stopAngle *= twoPiInv;

                int startAngleIndex = (int)(startAngle * sides);
                int endAngleIndex = sourceAngles.Length - 1;
                if (stopAngle < 1.0f)
                    endAngleIndex = (int)(stopAngle * sides) + 1;
                if (endAngleIndex == startAngleIndex)
                    endAngleIndex++;

                for (int angleIndex = startAngleIndex; angleIndex <= endAngleIndex; angleIndex++)
                {
                    angles.Add(sourceAngles[angleIndex]);
                    if (sides == 3)
                        normals.Add(normals3[angleIndex]);
                    else if (sides == 4)
                        normals.Add(normals4[angleIndex]);
                }

                if (startAngle > 0.0f)
                    angles[0] = interpolatePoints(startAngle, angles[0], angles[1]);

                if (stopAngle < 1.0f)
                {
                    int lastAngleIndex = angles.Count - 1;
                    angles[lastAngleIndex] = interpolatePoints(stopAngle, angles[lastAngleIndex - 1], angles[lastAngleIndex]);
                }
            }
            else
            {
                double stepSize = twoPi / sides;

                int startStep = (int)(startAngle / stepSize);
                double angle = (float)(stepSize * startStep);
                int step = startStep;
                double stopAngleTest = stopAngle;
                if (stopAngle < twoPi)
                {
                    stopAngleTest = stepSize * ((int)(stopAngle / stepSize) + 1);
                    if (stopAngleTest < stopAngle)
                        stopAngleTest += stepSize;
                    if (stopAngleTest > twoPi)
                        stopAngleTest = twoPi;
                }

                while (angle <= stopAngleTest)
                {
                    Angle newAngle = new((float)angle);
                    angles.Add(newAngle);
                    step += 1;
                    angle = stepSize * step;
                }

                if (startAngle > angles[0].angle)
                {
                    intersection(angles[0].X, angles[0].Y, angles[1].X, angles[1].Y, MathF.Cos(startAngle), MathF.Sin(startAngle));
                    angles[0] = new(startAngle, iX, iY);
                }

                int index = angles.Count - 1;
                if (stopAngle < angles[index].angle)
                {
                    int indxMinus1 = index - 1;
                    intersection(angles[indxMinus1].X, angles[indxMinus1].Y, angles[index].X, angles[index].Y, MathF.Cos(stopAngle), MathF.Sin(stopAngle));
                    angles[index] =  new(stopAngle, iX, iY);;
                }
            }
        }
    }

    /// <summary>
    /// generates a profile for extrusion
    /// </summary>
    public class Profile
    {
        private const float twoPi = 2.0f * MathF.PI;

        public string errorMessage = null;

        public List<Vector3> coords;
        public List<Face> faces;
        public List<Vector3> vertexNormals;
        public List<float> us;
        public List<UVCoord> faceUVs;
        public List<int> faceNumbers;

        // use these for making individual meshes for each prim face
        public List<int> outerCoordIndices = null;
        public List<int> hollowCoordIndices = null;
        public List<int> cut1CoordIndices = null;
        public List<int> cut2CoordIndices = null;

        public Vector3 faceNormal = Vector3.UnitZ;
        public Vector3 cutNormal1 = new();
        public Vector3 cutNormal2 = new();

        public int numOuterVerts = 0;
        public int numHollowVerts = 0;

        public int outerFaceNumber = -1;
        public int hollowFaceNumber = -1;

        public bool calcVertexNormals = false;
        public int bottomFaceNumber = 0;
        public int numPrimFaces = 0;

        public Profile()
        {
            coords = [];
            faces = [];
            vertexNormals = [];
            us = [];
            faceUVs = [];
            faceNumbers = [];
        }

        public Profile(int sides, float profileStart, float profileEnd,
            float hollow, int hollowSides, bool createFaces, bool calcVertexNormals)
        {
            this.calcVertexNormals = calcVertexNormals;
            coords = [];
            faces = [];
            vertexNormals = [];
            us = [];
            faceUVs = [];
            faceNumbers = [];

            List<Vector3> hollowCoords = [];
            List<Vector3> hollowNormals = [];
            List<float> hollowUs = new List<float>();

            if (calcVertexNormals)
            {
                outerCoordIndices = [];
                hollowCoordIndices = [];
                cut1CoordIndices = [];
                cut2CoordIndices = [];
            }

            bool hasHollow = hollow > 0.0f;
            bool hasProfileCut = profileStart > 0.0f || profileEnd < 1.0f;

            AngleList angles = new();
            AngleList hollowAngles = new();

            float xScale;
            float yScale;
            if (sides == 4)  // corners of a square are sqrt(2) from center
            {
                xScale = 0.707107f;
                yScale = 0.707107f;
            }
            else
            {
                xScale = 0.5f;
                yScale = 0.5f;
            }

            float startAngle = profileStart * twoPi;
            float stopAngle = profileEnd * twoPi;

            try { angles.makeAngles(sides, startAngle, stopAngle); }
            catch (Exception ex)
            {
                errorMessage = "makeAngles failed: Exception: " + ex.ToString()
                + "\nsides: " + sides.ToString() + " startAngle: " + startAngle.ToString() + " stopAngle: " + stopAngle.ToString();

                return;
            }

            numOuterVerts = angles.angles.Count;

            // flag to create as few triangles as possible for 3 or 4 side profile
            bool simpleFace = sides < 5 && !hasHollow && !hasProfileCut;

            if (hasHollow)
            {
                if (sides == hollowSides)
                    hollowAngles = angles;
                else
                {
                    try { hollowAngles.makeAngles(hollowSides, startAngle, stopAngle); }
                    catch (Exception ex)
                    {
                        errorMessage = "makeAngles failed: Exception: " + ex.ToString()
                        + "\nsides: " + sides.ToString() + " startAngle: " + startAngle.ToString() + " stopAngle: " + stopAngle.ToString();

                        return;
                    }
                }
                numHollowVerts = hollowAngles.angles.Count;
            }
            else if (!simpleFace)
            {
                coords.Add(Vector3.Zero);
                if (calcVertexNormals)
                    vertexNormals.Add(Vector3.UnitZ);
                us.Add(0.0f);
            }

            Angle angle;
            Vector3 newVert = new();

            if (hasHollow && hollowSides != sides)
            {
                for (int i = 0; i < hollowAngles.angles.Count; i++)
                {
                    angle = hollowAngles.angles[i];
                    newVert.X = hollow * xScale * angle.X;
                    newVert.Y = hollow * yScale * angle.Y;
                    hollowCoords.Add(newVert);

                    if (calcVertexNormals)
                    {
                        if (hollowSides < 5)
                            hollowNormals.Add(-hollowAngles.normals[i]);
                        else
                            hollowNormals.Add(new Vector3(-angle.X, -angle.Y, 0.0f));

                        if (hollowSides == 4)
                            hollowUs.Add(angle.angle * hollow * 0.707107f);
                        else
                            hollowUs.Add(angle.angle * hollow);
                    }
                }
            }

            int index = 0;
            for (int i = 0; i < angles.angles.Count; i++)
            {
                angle = angles.angles[i];
                newVert.X = angle.X * xScale;
                newVert.Y = angle.Y * yScale;
                coords.Add(newVert);

                if (calcVertexNormals)
                {
                    outerCoordIndices.Add(coords.Count - 1);

                    if (sides < 5)
                    {
                        vertexNormals.Add(angles.normals[i]);
                        us.Add(angle.angle);
                    }
                    else
                    {
                        vertexNormals.Add(new(angle.X, angle.Y, 0.0f));
                        us.Add(angle.angle);
                    }
                }

                if (hasHollow)
                {
                    if (hollowSides == sides)
                    {
                        newVert.X *= hollow;
                        newVert.Y *= hollow;
                        hollowCoords.Add(newVert);
                        if (calcVertexNormals)
                        {
                            if (sides < 5)
                            {
                                hollowNormals.Add(-angles.normals[i]);
                            }

                            else
                                hollowNormals.Add(new Vector3(-angle.X, -angle.Y, 0.0f));

                            hollowUs.Add(angle.angle * hollow);
                        }
                    }
                }
                else if (!simpleFace && createFaces && angle.angle > 0.0001f)
                {
                    faces.Add(new( 0, index, index + 1));
                }
                index += 1;
            }

            if (hasHollow)
            {
                hollowCoords.Reverse();
                if (calcVertexNormals)
                {
                    hollowNormals.Reverse();
                    hollowUs.Reverse();
                }

                if (createFaces)
                {
                    int numTotalVerts = numOuterVerts + numHollowVerts;

                    if (numOuterVerts == numHollowVerts)
                    {
                        for (int coordIndex = 0; coordIndex < numOuterVerts - 1; coordIndex++)
                        {
                            int fromTotal = numTotalVerts - coordIndex - 1;
                            faces.Add(new( coordIndex, coordIndex + 1, fromTotal ));
                            faces.Add(new( coordIndex + 1, fromTotal - 1, fromTotal ));
                        }
                    }
                    else
                    {
                        if (numOuterVerts < numHollowVerts)
                        {
                            int j = 0; // j is the index for outer vertices
                            int maxJ = numOuterVerts - 1;
                            for (int i = 0; i < numHollowVerts; i++) // i is the index for inner vertices
                            {
                                int fromTotal = numTotalVerts - i - 1;
                                if (j < maxJ)
                                { 
                                    if (angles.angles[j + 1].angle - hollowAngles.angles[i].angle < hollowAngles.angles[i].angle - angles.angles[j].angle + 0.000001f)
                                    {
                                        faces.Add(new( fromTotal, j, j + 1));
                                        j++;
                                    }
                                }

                                faces.Add(new( j, fromTotal - 1, fromTotal));
                            }
                        }
                        else // numHollowVerts < numOuterVerts
                        {
                            int j = 0; // j is the index for inner vertices
                            int maxJ = numHollowVerts - 1;
                            for (int i = 0; i < numOuterVerts; i++)
                            {
                                int fromTotal = numTotalVerts - j - 1;
                                if (j < maxJ)
                                { 
                                    if (hollowAngles.angles[j + 1].angle - angles.angles[i].angle < angles.angles[i].angle - hollowAngles.angles[j].angle + 0.000001f)
                                    {
                                        faces.Add(new( i, fromTotal - 1, fromTotal ));
                                        j++;
                                        fromTotal--;
                                    }
                                }

                                faces.Add(new( fromTotal, i, i + 1 ));
                            }
                        }
                    }
                }

                if (calcVertexNormals)
                {
                    foreach (Vector3 hc in hollowCoords)
                    {
                        hollowCoordIndices.Add(coords.Count);
                        coords.Add(hc);
                    }
                    vertexNormals.AddRange(hollowNormals);
                    us.AddRange(hollowUs);
                }
                else
                    coords.AddRange(hollowCoords);
            }

            if (simpleFace && createFaces)
            {
                if (sides == 3)
                    faces.Add(new(0, 1, 2));
                else if (sides == 4)
                {
                    faces.Add(new(0, 1, 2));
                    faces.Add(new(0, 2, 3));
                }
            }

            if (calcVertexNormals && hasProfileCut)
            {
                int lastOuterVertIndex = numOuterVerts - 1;

                if (hasHollow)
                {
                    cut1CoordIndices.Add(0);
                    cut1CoordIndices.Add(coords.Count - 1);

                    int lastOuterVertIndexPlus1 = lastOuterVertIndex + 1;
                    cut2CoordIndices.Add(lastOuterVertIndexPlus1);
                    cut2CoordIndices.Add(lastOuterVertIndex);

                    cutNormal1.X = coords[0].Y - coords[^1].Y;
                    cutNormal1.Y = -(coords[0].X - coords[^1].X);

                    cutNormal2.X = coords[lastOuterVertIndexPlus1].Y - coords[lastOuterVertIndex].Y;
                    cutNormal2.Y = -(coords[lastOuterVertIndexPlus1].X - coords[lastOuterVertIndex].X);
                }

                else
                {
                    cut1CoordIndices.Add(0);
                    cut1CoordIndices.Add(1);

                    cut2CoordIndices.Add(lastOuterVertIndex);
                    cut2CoordIndices.Add(0);

                    cutNormal1.X = vertexNormals[1].Y;
                    cutNormal1.Y = -vertexNormals[1].X;

                    cutNormal2.X = -vertexNormals[^2].Y;
                    cutNormal2.Y = vertexNormals[^2].X;
                }
                cutNormal1.Normalize();
                cutNormal2.Normalize();
            }

            MakeFaceUVs();

            if (calcVertexNormals)
            { // calculate prim face numbers

                // face number order is top, outer, hollow, bottom, start cut, end cut
                // I know it's ugly but so is the whole concept of prim face numbers

                int faceNum = 1; // start with outer faces
                outerFaceNumber = faceNum;

                if (hasProfileCut && !hasHollow)
                    faceNumbers.Add(-1);

                if(sides < 5)
                {
                    for (int i = 0; i < numOuterVerts - 1; i++)
                        faceNumbers.Add(i <= sides ? faceNum++ : faceNum);
                }
                else
                {
                    for (int i = 0; i < numOuterVerts - 1; i++)
                        faceNumbers.Add(faceNum);
                }

                faceNumbers.Add(hasProfileCut ? -1 : faceNum++);

                if(hasHollow || hasProfileCut)
                { 
                    if (sides > 4)
                        faceNum++;

                    if (sides < 5 && numOuterVerts < sides)
                        faceNum++;
                }

                if (hasHollow)
                {
                    for (int i = 0; i < numHollowVerts; i++)
                        faceNumbers.Add(faceNum);

                    hollowFaceNumber = faceNum++;
                }

                bottomFaceNumber = faceNum++;

                if (hasHollow && hasProfileCut)
                    faceNumbers.Add(faceNum++);

                for (int i = 0; i < faceNumbers.Count; i++)
                    if (faceNumbers[i] == -1)
                        faceNumbers[i] = faceNum++;

                numPrimFaces = faceNum;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MakeFaceUVs()
        {
            faceUVs = new(coords.Count);
            foreach (Vector3 c in coords)
                faceUVs.Add(new UVCoord(0.5f - c.X, 0.5f + c.Y));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Profile Copy()
        {
            return Copy(true);
        }

        public Profile Copy(bool needFaces)
        {
            Profile copy = new();

            copy.coords.AddRange(coords);
            copy.faceUVs.AddRange(faceUVs);

            copy.calcVertexNormals = calcVertexNormals;

            if (needFaces)
                copy.faces.AddRange(faces);

            if (calcVertexNormals)
            {
                copy.vertexNormals.AddRange(vertexNormals);
                copy.faceNormal = faceNormal;
                copy.cutNormal1 = cutNormal1;
                copy.cutNormal2 = cutNormal2;
                copy.us.AddRange(us);
                copy.faceNumbers.AddRange(faceNumbers);

                copy.cut1CoordIndices = new(cut1CoordIndices);
                copy.cut2CoordIndices = new(cut2CoordIndices);
                copy.hollowCoordIndices = new(hollowCoordIndices);
                copy.outerCoordIndices = new(outerCoordIndices);
            }
            copy.numOuterVerts = numOuterVerts;
            copy.numHollowVerts = numHollowVerts;

            return copy;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddPos(Vector3 v)
        {
           for (int i = 0; i < coords.Count; i++)
               coords[i] += v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddPos(float x, float y, float z)
        {
            Vector3 v = new(x, y, z);
            AddPos(v);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRot(Quaternion q)
        {
            if(q.IsIdentity()) return;

            for (int i = 0; i < coords.Count; i++)
                coords[i] *= q;

            if (calcVertexNormals)
            {
                for (int i = 0; i < vertexNormals.Count; i++)
                    vertexNormals[i] *= q;

                faceNormal *= q;
                cutNormal1 *= q;
                cutNormal2 *= q;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Scale(float x, float y)
        {
            Vector3 vert;
            for (int i = 0; i < coords.Count; i++)
            {
                vert = coords[i];
                vert.X *= x;
                vert.Y *= y;
                coords[i] = vert;
            }
        }

        /// <summary>
        /// Changes order of the vertex indices and negates the center vertex normal. Does not alter vertex normals of radial vertices
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FlipNormals()
        {
            Face tmpFace;
            int tmp;

            for (int i = 0; i < faces.Count; i++)
            {
                tmpFace = faces[i];
                tmp = tmpFace.v3;
                tmpFace.v3 = tmpFace.v1;
                tmpFace.v1 = tmp;
                faces[i] = tmpFace;
            }

            if (calcVertexNormals)
            {
                if (vertexNormals.Count > 0)
                {
                    Vector3 n = vertexNormals[^1];
                    n.Z = -n.Z;
                    vertexNormals[^1] = n;
                }
            }

            faceNormal = -faceNormal;

            for (int i = 0; i < faceUVs.Count; i++)
            {
                UVCoord uv = faceUVs[i];
                uv.V = 1.0f - uv.V;
                faceUVs[i] = uv;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddValue2FaceVertexIndices(int num)
        {
            for (int i = 0; i < faces.Count; i++)
            {
                Face face = faces[i];
                face.AddVerticesIndexesOffset(num);
                faces[i] = face;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddValue2FaceNormalIndices(int num)
        {
            for (int i = 0; i < faces.Count; i++)
            {
                Face face = faces[i];
                faces[i].AddNormalsIndexesOffset(num);
                faces[i] = face;
            }
        }

        public void DumpRaw(string path, string name, string title)
        {
            if (path == null)
                return;
            string fileName = name + "_" + title + ".raw";
            string completePath = System.IO.Path.Combine(path, fileName);
            StreamWriter sw = new(completePath);

            for (int i = 0; i < faces.Count; i++)
                sw.WriteLine($"{coords[faces[i].v1]} {coords[faces[i].v2]} {coords[faces[i].v3]}");

            sw.Close();
        }
    }

    public struct PathNode
    {
        public Vector3 position;
        public Quaternion rotation;
        public float xScale;
        public float yScale;
        public float percentOfPath;
    }

    public enum PathType { Linear = 0, Circular = 1, Flexible = 2 }

    public class Path
    {
        public List<PathNode> pathNodes = [];

        public float twistBegin = 0.0f;
        public float twistEnd = 0.0f;
        public float topShearX = 0.0f;
        public float topShearY = 0.0f;
        public float pathCutBegin = 0.0f;
        public float pathCutEnd = 1.0f;
        public float dimpleBegin = 0.0f;
        public float dimpleEnd = 1.0f;
        public float skew = 0.0f;
        public float holeSizeX = 1.0f; // called pathScaleX in pbs
        public float holeSizeY = 0.25f;
        public float taperX = 0.0f;
        public float taperY = 0.0f;
        public float radius = 0.0f;
        public float revolutions = 1.0f;
        public int stepsPerRevolution = 24;

        private const float twoPi = 2.0f * MathF.PI;

        public void Create(PathType pathType, int steps)
        {
            if(taperX < -0.999f)
                taperX = -0.999f;
            else if(taperX > 0.999f)
                taperX = 0.999f;

            if(taperY < -0.999f)
                taperY = -0.999f;
            else if(taperY > 0.999f)
                taperY = 0.999f;

            if (pathType == PathType.Linear || pathType == PathType.Flexible)
            {
                int step = 0;

                float length = pathCutEnd - pathCutBegin;
                float twistTotal = twistEnd - twistBegin;
                float twistTotalAbs = MathF.Abs(twistTotal);
                if (twistTotalAbs > 0.01f)
                    steps += (int)(twistTotalAbs * 3.66f); //  dahlia's magic number

                float start = -0.5f;
                float stepSize = length / (float)steps;
                float percentOfPathMultiplier = stepSize * 0.999999f;
                float xOffset = topShearX * pathCutBegin;
                float yOffset = topShearY * pathCutBegin;
                float zOffset = start;
                float xOffsetStepIncrement = topShearX * length / steps;
                float yOffsetStepIncrement = topShearY * length / steps;

                float percentOfPath = pathCutBegin;
                zOffset += percentOfPath;

                // sanity checks

                while (true)
                {
                    PathNode newNode = new();

                    if (taperX == 0.0f)
                        newNode.xScale = 1.0f;
                    else if (taperX > 0.0f)
                        newNode.xScale = 1.0f - percentOfPath * taperX;
                    else
                        newNode.xScale = 1.0f + (1.0f - percentOfPath) * taperX;

                    if (taperY == 0.0f)
                        newNode.yScale = 1.0f;
                    else if (taperY > 0.0f)
                        newNode.yScale = 1.0f - percentOfPath * taperY;
                    else
                        newNode.yScale = 1.0f + (1.0f - percentOfPath) * taperY;

                    float twist = twistBegin + twistTotal * percentOfPath;

                    newNode.rotation = twist == 0f ? Quaternion.Identity :
                            new Quaternion(Quaternion.MainAxis.Z, twist);
                    newNode.position = new Vector3(xOffset, yOffset, zOffset);
                    newNode.percentOfPath = percentOfPath;

                    pathNodes.Add(newNode);

                    if (step >= steps)
                        break;

                    percentOfPath += percentOfPathMultiplier;
                    if (percentOfPath > pathCutEnd)
                        break;

                    step++;
                    xOffset += xOffsetStepIncrement;
                    yOffset += yOffsetStepIncrement;
                    zOffset += stepSize;
                }
            } // end of linear path code

            else // pathType == Circular
            {
                float twistTotal = twistEnd - twistBegin;

                // if the profile has a lot of twist, add more layers otherwise the layers may overlap
                // and the resulting mesh may be quite inaccurate. This method is arbitrary and doesn't
                // accurately match the viewer
                /*
                float twistTotalAbs = MathF.Abs(twistTotal);
                if (twistTotalAbs > 0.01f)
                {
                    if (twistTotalAbs > MathF.PI * 1.5f)
                        steps *= 2;
                    if (twistTotalAbs > MathF.PI * 3.0f)
                        steps *= 2;
                }
                */

                float yPathScale = holeSizeY * 0.5f;
                float pathLength = pathCutEnd - pathCutBegin;
                float totalSkew = skew * 2.0f * pathLength;
                float skewStart = pathCutBegin * 2.0f * skew - skew;
                float xOffsetTopShearXFactor = topShearX * (0.25f + 0.5f * (0.5f - holeSizeY));
                float yShearCompensation = 1.0f + MathF.Abs(topShearY) * 0.25f;

                // It's not quite clear what pushY (Y top shear) does, but subtracting it from the start and end
                // angles appears to approximate it's effects on path cut. Likewise, adding it to the angle used
                // to calculate the sine for generating the path radius appears to approximate it's effects there
                // too, but there are some subtle differences in the radius which are noticeable as the prim size
                // increases and it may affect megaprims quite a bit. The effect of the Y top shear parameter on
                // the meshes generated with this technique appear nearly identical in shape to the same prims when
                // displayed by the viewer.

                float startAngle = (twoPi * pathCutBegin * revolutions) - topShearY * 0.9f;
                float endAngle = (twoPi * pathCutEnd * revolutions) - topShearY * 0.9f;
                float stepSize = twoPi / stepsPerRevolution;

                float angle = startAngle;

                while (true) // loop through the length of the path and add the layers
                {
                    PathNode newNode = new();

                    float xProfileScale = (1.0f - MathF.Abs(skew)) * holeSizeX;
                    float yProfileScale = holeSizeY;

                    float percentOfPath = angle / (twoPi * revolutions);
                    float percentOfAngles = (angle - startAngle) / (endAngle - startAngle);

                    if (taperX > 0.01f)
                        xProfileScale *= 1.0f - percentOfPath * taperX;
                    else if (taperX < -0.01f)
                        xProfileScale *= 1.0f + (1.0f - percentOfPath) * taperX;

                    if (taperY > 0.01f)
                        yProfileScale *= 1.0f - percentOfPath * taperY;
                    else if (taperY < -0.01f)
                        yProfileScale *= 1.0f + (1.0f - percentOfPath) * taperY;

                    newNode.xScale = xProfileScale;
                    newNode.yScale = yProfileScale;

                    float radiusScale;
                    if (radius > 0.001f)
                        radiusScale = 1.0f - radius * percentOfPath;
                    else if (radius < 0.001f)
                        radiusScale = 1.0f + radius * (1.0f - percentOfPath);
                    else
                        radiusScale = 1.0f;

                    float twist = twistBegin + twistTotal * percentOfPath;

                    float xOffset = 0.5f * (skewStart + totalSkew * percentOfAngles);
                    xOffset += MathF.Sin(angle) * xOffsetTopShearXFactor;

                    float yOffset = yShearCompensation * MathF.Cos(angle) * (0.5f - yPathScale) * radiusScale;

                    float zOffset = MathF.Sin(angle + topShearY) * (0.5f - yPathScale) * radiusScale;

                    newNode.position = new(xOffset, yOffset, zOffset);

                    // now orient the rotation of the profile layer relative to it's position on the path
                    // adding taperY to the angle used to generate the quat appears to approximate the viewer

                    newNode.rotation = angle + topShearY == 0f ? Quaternion.Identity :
                            new Quaternion(Quaternion.MainAxis.X, angle + topShearY);

                    // next apply twist rotation to the profile layer
                    if (twistTotal != 0.0f || twistBegin != 0.0f)
                        newNode.rotation *= new Quaternion(Quaternion.MainAxis.Z, twist);

                    newNode.percentOfPath = percentOfPath;

                    pathNodes.Add(newNode);

                    // calculate terms for next iteration
                    // calculate the angle for the next iteration of the loop

                    if (angle >= endAngle - 0.01)
                        break;
                    
                    angle += stepSize;
                    if (angle > endAngle)
                        angle = endAngle;
                }
            }
        }
    }

    public class PrimMesh
    {
        public string errorMessage = "";
        private const float twoPi = 2.0f * MathF.PI;
        private const float DegToRad = twoPi / 360f;

        public List<Vector3> coords;
        public List<Vector3> normals;
        public List<Face> faces;

        public List<ViewerFace> viewerFaces;

        private readonly int sides = 4;
        private readonly int hollowSides = 4;
        private readonly float profileStart = 0.0f;
        private readonly float profileEnd = 1.0f;
        private readonly float hollow = 0.0f;
        public int twistBegin = 0;
        public int twistEnd = 0;
        public float topShearX = 0.0f;
        public float topShearY = 0.0f;
        public float pathCutBegin = 0.0f;
        public float pathCutEnd = 1.0f;
        public float dimpleBegin = 0.0f;
        public float dimpleEnd = 1.0f;
        public float skew = 0.0f;
        public float holeSizeX = 1.0f; // called pathScaleX in pbs
        public float holeSizeY = 0.25f;
        public float taperX = 0.0f;
        public float taperY = 0.0f;
        public float radius = 0.0f;
        public float revolutions = 1.0f;
        public int stepsPerRevolution = 24;

        private int profileOuterFaceNumber = -1;
        private int profileHollowFaceNumber = -1;

        private bool hasProfileCut = false;
        private bool hasHollow = false;
        public bool calcVertexNormals = false;
        private bool normalsProcessed = false;
        public bool viewerMode = false;
        public bool sphereMode = false;

        public int numPrimFaces = 0;

        /// <summary>
        /// Human readable string representation of the parameters used to create a mesh.
        /// </summary>
        /// <returns></returns>
        public string ParamsToDisplayString()
        {
            string s = "";
            s += "sides..................: " + sides.ToString();
            s += "\nhollowSides..........: " + hollowSides.ToString();
            s += "\nprofileStart.........: " + profileStart.ToString();
            s += "\nprofileEnd...........: " + profileEnd.ToString();
            s += "\nhollow...............: " + hollow.ToString();
            s += "\ntwistBegin...........: " + twistBegin.ToString();
            s += "\ntwistEnd.............: " + twistEnd.ToString();
            s += "\ntopShearX............: " + topShearX.ToString();
            s += "\ntopShearY............: " + topShearY.ToString();
            s += "\npathCutBegin.........: " + pathCutBegin.ToString();
            s += "\npathCutEnd...........: " + pathCutEnd.ToString();
            s += "\ndimpleBegin..........: " + dimpleBegin.ToString();
            s += "\ndimpleEnd............: " + dimpleEnd.ToString();
            s += "\nskew.................: " + skew.ToString();
            s += "\nholeSizeX............: " + holeSizeX.ToString();
            s += "\nholeSizeY............: " + holeSizeY.ToString();
            s += "\ntaperX...............: " + taperX.ToString();
            s += "\ntaperY...............: " + taperY.ToString();
            s += "\nradius...............: " + radius.ToString();
            s += "\nrevolutions..........: " + revolutions.ToString();
            s += "\nstepsPerRevolution...: " + stepsPerRevolution.ToString();
            s += "\nsphereMode...........: " + sphereMode.ToString();
            s += "\nhasProfileCut........: " + hasProfileCut.ToString();
            s += "\nhasHollow............: " + hasHollow.ToString();
            s += "\nviewerMode...........: " + viewerMode.ToString();

            return s;
        }

        public int ProfileOuterFaceNumber
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return profileOuterFaceNumber; }
        }

        public int ProfileHollowFaceNumber
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return profileHollowFaceNumber; }
        }

        public bool HasProfileCut
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return hasProfileCut; }
        }

        public bool HasHollow
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return hasHollow; }
        }


        /// <summary>
        /// Constructs a PrimMesh object and creates the profile for extrusion.
        /// </summary>
        /// <param name="sides"></param>
        /// <param name="profileStart"></param>
        /// <param name="profileEnd"></param>
        /// <param name="hollow"></param>
        /// <param name="hollowSides"></param>
        public PrimMesh(int _sides, float _profileStart, float _profileEnd, float _hollow, int _hollowSides)
        {
            coords = [];
            faces = [];

            sides = _sides < 3 ? 3 : _sides;
            hollowSides = _hollowSides < 3 ? 3 : _hollowSides;
            profileStart = _profileStart;
            profileEnd = _profileEnd;
            hollow = _hollow;

            if(hollow < 0f)
                hollow = 0f;
            else if(hollow > 0.99f)
                hollow = 0.99f;

            if(profileEnd < 0.02f)
                profileEnd = 0.02f;
            else if( profileEnd > 1f)
                profileEnd = 1f;

            if (profileStart < 0.0f)
                profileStart = 0.0f;
            else if (profileStart >= profileEnd)
                profileStart = profileEnd - 0.02f;
        }

        /// <summary>
        /// Extrudes a profile along a path.
        /// </summary>
        public void Extrude(PathType pathType)
        {

            coords = [];
            faces = [];

            if (viewerMode)
            {
                viewerFaces = [];
                calcVertexNormals = true;
            }

            if (calcVertexNormals)
                normals = [];

            normalsProcessed = false;

            int steps = 1;
            float length = pathCutEnd - pathCutBegin;

            if (viewerMode && sides == 3)
            {
                // prisms don't taper well so add some vertical resolution
                // other prims may benefit from this but just do prisms for now
                if (MathF.Abs(taperX) > 0.01f || MathF.Abs(taperY) > 0.01f)
                    steps = (int)(steps * 4.5f * length);
            }

            if (sphereMode)
                hasProfileCut = profileEnd - profileStart < 0.4999f;
            else
                hasProfileCut = profileEnd - profileStart < 0.9999f;

            float hollow = this.hollow;
            hasHollow = hollow > 0.001f;

            float twistBegin = this.twistBegin * DegToRad;
            float twistEnd = this.twistEnd * DegToRad;
            float twistTotal = twistEnd - twistBegin;
            float twistTotalAbs = MathF.Abs(twistTotal);
            if (twistTotalAbs > 0.01f)
                steps += (int)(twistTotalAbs * 3.66f); //  dahlia's magic number

            bool needEndFaces =
                        pathType != PathType.Circular ||
                        pathCutBegin > float.Epsilon || pathCutEnd < 1.0f - float.Epsilon ||
                        taperX > 0.01f || taperY > 0.01f ||
                        MathF.Abs(skew) > 0.01f ||
                        MathF.Abs(twistTotal) > 0.001f ||
                        MathF.Abs(radius) > 0.0005f;

            // sanity checks
            float initialProfileRot = 0.0f;

            if (pathType == PathType.Circular)
            {
                if (sides == 3)
                {
                    initialProfileRot = MathF.PI;
                    if (hollowSides == 4)
                    {
                        if (hollow > 0.7f)
                            hollow = 0.7f;
                        hollow *= 0.707f;
                    }
                    else hollow *= 0.5f;
                }
                else if (sides == 4)
                {
                    initialProfileRot = 0.25f * MathF.PI;
                    if (hollowSides != 4)
                        hollow *= 0.707f;
                }
                else if (sides > 4)
                {
                    initialProfileRot = MathF.PI;
                    if (hollowSides == 4)
                    {
                        if (hollow > 0.7f)
                            hollow = 0.7f;
                        hollow /= 0.7f;
                    }
                }
            }
            else
            {
                if (sides == 3)
                {
                    if (hollowSides == 4)
                    {
                        if (hollow > 0.7f)
                            hollow = 0.7f;
                        hollow *= 0.707f;
                    }
                    else hollow *= 0.5f;
                }
                else if (sides == 4)
                {
                    initialProfileRot = 1.25f * MathF.PI;
                    if (hollowSides != 4)
                        hollow *= 0.707f;
                }
                else if (sides > 4 && hollowSides == 4)
                    hollow *= 1.414f;
            }

            Profile profile = new(sides, profileStart, profileEnd, hollow, hollowSides, true, calcVertexNormals);
            errorMessage = profile.errorMessage;

            numPrimFaces = profile.numPrimFaces;

            int cut1FaceNumber;
            if(needEndFaces)
            {
                cut1FaceNumber  = profile.bottomFaceNumber + 1;
                profileOuterFaceNumber = profile.outerFaceNumber;
            }
            else
            { 
                cut1FaceNumber  = profile.bottomFaceNumber - 1;
                profileOuterFaceNumber = profile.outerFaceNumber - 1;
            }
            int cut2FaceNumber = cut1FaceNumber + 1;

            if (hasHollow)
            {
                profileHollowFaceNumber = needEndFaces ? profile.hollowFaceNumber : profile.hollowFaceNumber - 1;
            }

            int cut1Vert;
            int cut2Vert;
            if (hasProfileCut)
            {
                cut1Vert = hasHollow ? profile.coords.Count - 1 : 0;
                cut2Vert = hasHollow ? profile.numOuterVerts - 1 : profile.numOuterVerts;
            }
            else
            {
                cut1Vert = -1;
                cut2Vert = -1;
            }

            if (initialProfileRot != 0.0f)
            {
                profile.AddRot(new Quaternion(Quaternion.MainAxis.Z, initialProfileRot));
                if (viewerMode)
                    profile.MakeFaceUVs();
            }

            Vector3 lastCutNormal1 = new();
            Vector3 lastCutNormal2 = new();
            float lastV = 0.0f;
            float thisV;

            Path path = new()
            {
                twistBegin = twistBegin,
                twistEnd = twistEnd,
                topShearX = topShearX,
                topShearY = topShearY,
                pathCutBegin = pathCutBegin,
                pathCutEnd = pathCutEnd,
                dimpleBegin = dimpleBegin,
                dimpleEnd = dimpleEnd,
                skew = skew,
                holeSizeX = holeSizeX,
                holeSizeY = holeSizeY,
                taperX = taperX,
                taperY = taperY,
                radius = radius,
                revolutions = revolutions,
                stepsPerRevolution = stepsPerRevolution
            };

            path.Create(pathType, steps);

            for (int nodeIndex = 0; nodeIndex < path.pathNodes.Count; nodeIndex++)
            {
                PathNode node = path.pathNodes[nodeIndex];
                Profile newLayer = profile.Copy();
                newLayer.Scale(node.xScale, node.yScale);

                newLayer.AddRot(node.rotation);
                newLayer.AddPos(node.position);

                if (needEndFaces && nodeIndex == 0)
                {
                    newLayer.FlipNormals();

                    // add the bottom faces to the viewerFaces list
                    if (viewerMode)
                    {
                        Vector3 faceNormal = newLayer.faceNormal;
                        ViewerFace newViewerFace = new(profile.bottomFaceNumber);
                        List<Face> faces = newLayer.faces;

                        for (int i = 0; i < faces.Count; i++)
                        {
                            Face face = faces[i];
                            newViewerFace.v1 = newLayer.coords[face.v1];
                            newViewerFace.v2 = newLayer.coords[face.v2];
                            newViewerFace.v3 = newLayer.coords[face.v3];

                            newViewerFace.coordIndex1 = face.v1;
                            newViewerFace.coordIndex2 = face.v2;
                            newViewerFace.coordIndex3 = face.v3;

                            newViewerFace.n1 = faceNormal;
                            newViewerFace.n2 = faceNormal;
                            newViewerFace.n3 = faceNormal;

                            newViewerFace.uv1 = newLayer.faceUVs[face.v1];
                            newViewerFace.uv2 = newLayer.faceUVs[face.v2];
                            newViewerFace.uv3 = newLayer.faceUVs[face.v3];

                            if (pathType == PathType.Linear)
                            {
                                newViewerFace.uv1.Flip();
                                newViewerFace.uv2.Flip();
                                newViewerFace.uv3.Flip();
                            }

                            viewerFaces.Add(newViewerFace);
                        }
                    }
                } // if (nodeIndex == 0)

                // append this layer
                int coordsLen = coords.Count;
                if(coordsLen > 0)
                    newLayer.AddValue2FaceVertexIndices(coordsLen);
                coords.AddRange(newLayer.coords);

                if (calcVertexNormals)
                {
                    if(normals.Count > 0)
                        newLayer.AddValue2FaceNormalIndices(normals.Count);
                    normals.AddRange(newLayer.vertexNormals);
                }

                if (needEndFaces)
                {
                    if (nodeIndex == 0 || nodeIndex == path.pathNodes.Count - 1) 
                        faces.AddRange(newLayer.faces);
                }

                if(nodeIndex == 0)
                {
                    lastCutNormal1 = newLayer.cutNormal1;
                    lastCutNormal2 = newLayer.cutNormal2;
                    lastV = 1.0f - node.percentOfPath;
                    continue;
                }
                // fill faces between layers

                int numVerts = newLayer.coords.Count;
                Face newFace1 = new();
                Face newFace2 = new();

                thisV = 1.0f - node.percentOfPath;

                int startVert = coordsLen + 1;
                int endVert = coords.Count;

                if (sides < 5 || hasProfileCut || hasHollow)
                    startVert--;

                for (int i = startVert; i < endVert; i++)
                {
                    int iNext = i == endVert - 1 ? startVert : i + 1;

                    newFace1.v1 = i;
                    newFace1.v2 = i - numVerts;
                    newFace1.v3 = iNext;

                    newFace1.n1 = newFace1.v1;
                    newFace1.n2 = newFace1.v2;
                    newFace1.n3 = newFace1.v3;
                    faces.Add(newFace1);

                    newFace2.v1 = iNext;
                    newFace2.v2 = i - numVerts;
                    newFace2.v3 = iNext - numVerts;

                    newFace2.n1 = newFace2.v1;
                    newFace2.n2 = newFace2.v2;
                    newFace2.n3 = newFace2.v3;
                    faces.Add(newFace2);

                    if (viewerMode)
                    {
                        // add the side faces to the list of viewerFaces here
                        int whichVert = i - startVert;

                        int primFaceNum = profile.faceNumbers[whichVert];
                        if (!needEndFaces)
                            primFaceNum -= 1;

                        ViewerFace newViewerFace1 = new(primFaceNum);
                        ViewerFace newViewerFace2 = new(primFaceNum);

                        int uIndex = whichVert;
                        if (!hasHollow && sides > 4 && uIndex < newLayer.us.Count - 1)
                        {
                            uIndex++;
                        }

                        float u1 = newLayer.us[uIndex];
                        float u2 = 1.0f;
                        if (uIndex < newLayer.us.Count - 1)
                            u2 = newLayer.us[uIndex + 1];

                        if (whichVert == cut1Vert || whichVert == cut2Vert)
                        {
                            u1 = 0.0f;
                            u2 = 1.0f;
                        }
                        else if (sides < 5)
                        {
                            if (whichVert < profile.numOuterVerts)
                            { // boxes and prisms have one texture face per side of the prim, so the U values have to be scaled
                                // to reflect the entire texture width
                                u1 *= sides;
                                u2 *= sides;
                                u2 -= (int)u1;
                                u1 -= (int)u1;
                                if (u2 < 0.1f)
                                    u2 = 1.0f;
                            }
                        }

                        if (sphereMode)
                        {
                            if (whichVert != cut1Vert && whichVert != cut2Vert)
                            {
                                u1 = u1 * 2.0f - 1.0f;
                                u2 = u2 * 2.0f - 1.0f;

                                if (whichVert >= newLayer.numOuterVerts)
                                {
                                    u1 -= hollow;
                                    u2 -= hollow;
                                }
                            }
                        }

                        newViewerFace1.uv1.U = u1;
                        newViewerFace1.uv2.U = u1;
                        newViewerFace1.uv3.U = u2;

                        newViewerFace1.uv1.V = thisV;
                        newViewerFace1.uv2.V = lastV;
                        newViewerFace1.uv3.V = thisV;

                        newViewerFace2.uv1.U = u2;
                        newViewerFace2.uv2.U = u1;
                        newViewerFace2.uv3.U = u2;

                        newViewerFace2.uv1.V = thisV;
                        newViewerFace2.uv2.V = lastV;
                        newViewerFace2.uv3.V = lastV;

                        newViewerFace1.v1 = coords[newFace1.v1];
                        newViewerFace1.v2 = coords[newFace1.v2];
                        newViewerFace1.v3 = coords[newFace1.v3];
                            
                        newViewerFace2.v1 = coords[newFace2.v1];
                        newViewerFace2.v2 = coords[newFace2.v2];
                        newViewerFace2.v3 = coords[newFace2.v3];

                        newViewerFace1.coordIndex1 = newFace1.v1;
                        newViewerFace1.coordIndex2 = newFace1.v2;
                        newViewerFace1.coordIndex3 = newFace1.v3;

                        newViewerFace2.coordIndex1 = newFace2.v1;
                        newViewerFace2.coordIndex2 = newFace2.v2;
                        newViewerFace2.coordIndex3 = newFace2.v3;

                        // profile cut faces
                        if (whichVert == cut1Vert)
                        {
                            newViewerFace1.primFaceNumber = cut1FaceNumber;
                            newViewerFace2.primFaceNumber = cut1FaceNumber;
                            newViewerFace1.n1 = newLayer.cutNormal1;
                            newViewerFace1.n2 = lastCutNormal1;
                            newViewerFace1.n3 = lastCutNormal1;

                            newViewerFace2.n1 = newLayer.cutNormal1;
                            newViewerFace2.n3 = newLayer.cutNormal1;
                            newViewerFace2.n2 = lastCutNormal1;
                        }
                        else if (whichVert == cut2Vert)
                        {
                            newViewerFace1.primFaceNumber = cut2FaceNumber;
                            newViewerFace2.primFaceNumber = cut2FaceNumber;
                            newViewerFace1.n1 = newLayer.cutNormal2;
                            newViewerFace1.n2 = lastCutNormal2;
                            newViewerFace1.n3 = lastCutNormal2;

                            newViewerFace2.n1 = newLayer.cutNormal2;
                            newViewerFace2.n3 = newLayer.cutNormal2;
                            newViewerFace2.n2 = lastCutNormal2;
                        }

                        else // outer and hollow faces
                        {
                            if ((sides < 5 && whichVert < newLayer.numOuterVerts) || (hollowSides < 5 && whichVert >= newLayer.numOuterVerts))
                            { // looks terrible when path is twisted... need vertex normals here
                                newViewerFace1.CalcSurfaceNormal();
                                newViewerFace2.CalcSurfaceNormal();
                            }
                            else
                            {
                                newViewerFace1.n1 = normals[newFace1.n1];
                                newViewerFace1.n2 = normals[newFace1.n2];
                                newViewerFace1.n3 = normals[newFace1.n3];
                                    
                                newViewerFace2.n1 = normals[newFace2.n1];
                                newViewerFace2.n2 = normals[newFace2.n2];
                                newViewerFace2.n3 = normals[newFace2.n3];
                            }
                        }

                        viewerFaces.Add(newViewerFace1);
                        viewerFaces.Add(newViewerFace2);
                    }
                }

                lastCutNormal1 = newLayer.cutNormal1;
                lastCutNormal2 = newLayer.cutNormal2;
                lastV = thisV;

                if (needEndFaces && nodeIndex == path.pathNodes.Count - 1 && viewerMode)
                {
                    // add the top faces to the viewerFaces list here
                    Vector3 faceNormal = newLayer.faceNormal;
                    ViewerFace newViewerFace = new(0);
                    List<Face> faces = newLayer.faces;

                    for (int i = 0; i < faces.Count; i++)
                    {
                        Face face = faces[i];
                        newViewerFace.v1 = newLayer.coords[face.v1 - coordsLen];
                        newViewerFace.v2 = newLayer.coords[face.v2 - coordsLen];
                        newViewerFace.v3 = newLayer.coords[face.v3 - coordsLen];

                        newViewerFace.coordIndex1 = face.v1 - coordsLen;
                        newViewerFace.coordIndex2 = face.v2 - coordsLen;
                        newViewerFace.coordIndex3 = face.v3 - coordsLen;

                        newViewerFace.n1 = faceNormal;
                        newViewerFace.n2 = faceNormal;
                        newViewerFace.n3 = faceNormal;

                        newViewerFace.uv1 = newLayer.faceUVs[face.v1 - coordsLen];
                        newViewerFace.uv2 = newLayer.faceUVs[face.v2 - coordsLen];
                        newViewerFace.uv3 = newLayer.faceUVs[face.v3 - coordsLen];

                        if (pathType == PathType.Linear)
                        {
                            newViewerFace.uv1.Flip();
                            newViewerFace.uv2.Flip();
                            newViewerFace.uv3.Flip();
                        }

                        viewerFaces.Add(newViewerFace);
                    }
                }
            } // for (int nodeIndex = 0; nodeIndex < path.pathNodes.Count; nodeIndex++)

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExtrudeLinear()
        {
            Extrude(PathType.Linear);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExtrudeCircular()
        {
            Extrude(PathType.Circular);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector3 SurfaceNormal(Vector3 c1, Vector3 c2, Vector3 c3)
        {
            Vector3 edge1 = c2 - c1;
            Vector3 edge2 = c3 - c1;

            Vector3 normal = Vector3.Cross(edge1, edge2);
            normal.Normalize();

            return normal;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Vector3 SurfaceNormal(Face face)
        {
            return SurfaceNormal(coords[face.v1], coords[face.v2], coords[face.v3]);
        }

        /// <summary>
        /// Calculate the surface normal for a face in the list of faces
        /// </summary>
        /// <param name="faceIndex"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 SurfaceNormal(int faceIndex)
        {
            if (faceIndex < 0 || faceIndex >= faces.Count)
                throw new Exception("faceIndex out of range");

            return SurfaceNormal(faces[faceIndex]);
        }

        /// <summary>
        /// Duplicates a PrimMesh object. All object properties are copied by value, including lists.
        /// </summary>
        /// <returns></returns>
        public PrimMesh Copy()
        {
            PrimMesh copy = new(sides, profileStart, profileEnd, hollow, hollowSides)
            {
                twistBegin = twistBegin,
                twistEnd = twistEnd,
                topShearX = topShearX,
                topShearY = topShearY,
                pathCutBegin = pathCutBegin,
                pathCutEnd = pathCutEnd,
                dimpleBegin = dimpleBegin,
                dimpleEnd = dimpleEnd,
                skew = skew,
                holeSizeX = holeSizeX,
                holeSizeY = holeSizeY,
                taperX = taperX,
                taperY = taperY,
                radius = radius,
                revolutions = revolutions,
                stepsPerRevolution = stepsPerRevolution,
                calcVertexNormals = calcVertexNormals,
                normalsProcessed = normalsProcessed,
                viewerMode = viewerMode,
                numPrimFaces = numPrimFaces,
                errorMessage = errorMessage,

                coords = new List<Vector3>(coords),
                faces = new List<Face>(faces),
                viewerFaces = new List<ViewerFace>(viewerFaces),
                normals = new List<Vector3>(normals)
            };

            return copy;
        }

        /// <summary>
        /// Calculate surface normals for all of the faces in the list of faces in this mesh
        /// </summary>
        public void CalcNormals()
        {
            if (normalsProcessed)
                return;

            normalsProcessed = true;

            if (!calcVertexNormals)
                normals = [];

            for (int i = 0; i < faces.Count; i++)
            {
                int normIndex = normals.Count;

                Face face = faces[i];
                Vector3 n = SurfaceNormal(face);
                n.Normalize();
                normals.Add(n);

                face.n1 = normIndex;
                face.n2 = normIndex;
                face.n3 = normIndex;

                faces[i] = face;
            }
        }

        /// <summary>
        /// Adds a value to each XYZ vertex coordinate in the mesh
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddPos(float x, float y, float z)
        {
            Vector3 vert = new(x, y, z);

            for (int i = 0; i < coords.Count; i++)
                coords[i] += vert;

            if (viewerFaces != null)
            {
                for (int i = 0; i < viewerFaces.Count; i++)
                {
                    ViewerFace face = viewerFaces[i];
                    face.AddPos(vert);
                    viewerFaces[i] = face;
                }
            }
        }

        /// <summary>
        /// Rotates the mesh
        /// </summary>
        /// <param name="q"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRot(Quaternion q)
        {
            if(q.IsIdentity())
                return;

            for (int i = 0; i < coords.Count; i++)
                coords[i] *= q;

            if (normals != null)
            {
                for (int i = 0; i < normals.Count; i++)
                    normals[i] *= q;
            }

            if (viewerFaces != null)
            {
                for (int i = 0; i < viewerFaces.Count; i++)
                {
                    ViewerFace face = viewerFaces[i];
                    face.AddRot(q);
                    viewerFaces[i] = face;
                }
            }
        }

        public VertexIndexer GetVertexIndexer()
        {
            return (viewerMode && viewerFaces.Count > 0) ? new VertexIndexer(this) : null;
        }

        /// <summary>
        /// Scales the mesh
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void Scale(float x, float y, float z)
        {
            Vector3 scale = new(x, y, z);
            for (int i = 0; i < coords.Count; i++)
                coords[i] *= scale;

            if (viewerFaces != null)
            {
                for (int i = 0; i < viewerFaces.Count; i++)
                {
                    ViewerFace face = viewerFaces[i];
                    face.Scale(scale);
                    viewerFaces[i] = face;
                }
            }
        }

        /// <summary>
        /// Dumps the mesh to a Blender compatible "Raw" format file
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        /// <param name="title"></param>
        public void DumpRaw(string path, string name, string title)
        {
            if (path == null)
                return;
            string fileName = name + "_" + title + ".raw";
            string completePath = System.IO.Path.Combine(path, fileName);

            using StreamWriter sw = new(completePath);

            for (int i = 0; i < faces.Count; i++)
            {
                Face face = faces[i];
                string s = $"{coords[face.v1]} {coords[face.v2]} {coords[face.v3]}";
                sw.WriteLine(s);
            }
        }
    }
}
