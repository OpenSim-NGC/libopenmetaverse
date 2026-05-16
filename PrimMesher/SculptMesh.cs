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

// SkiaSharp bitmap/pixel APIs are used for sculpt image loading and sampling.

using System;
using System.Collections.Generic;
using System.IO;

using OpenMetaverse;
using SkiaSharp;

namespace PrimMesher
{
    public class SculptMesh
    {
        public List<Vector3> coords;
        public List<Face> faces;

        public List<ViewerFace> viewerFaces;
        public List<Vector3> normals;
        public List<UVCoord> uvs;

        public enum SculptType { sphere = 1, torus = 2, plane = 3, cylinder = 4 };

        public SculptMesh SculptMeshFromFile(string fileName, SculptType sculptType, int lod, bool viewerMode)
        {
            using SKBitmap bitmap = SKBitmap.Decode(fileName) ?? throw new Exception("Failed to decode sculpt image");
            SculptMesh sculptMesh = new SculptMesh(bitmap, sculptType, lod, viewerMode);
            return sculptMesh;
        }


        public SculptMesh(string fileName, int sculptType, int lod, int viewerMode, int mirror, int invert)
        {
            using SKBitmap bitmap = SKBitmap.Decode(fileName) ?? throw new Exception("Failed to decode sculpt image");
            _SculptMesh(bitmap, (SculptType)sculptType, lod, viewerMode != 0, mirror != 0, invert != 0);
        }

        /// <summary>
        /// ** Experimental ** May disappear from future versions ** not recommeneded for use in applications
        /// Construct a sculpt mesh from a 2D array of floats
        /// </summary>
        /// <param name="zMap"></param>
        /// <param name="xBegin"></param>
        /// <param name="xEnd"></param>
        /// <param name="yBegin"></param>
        /// <param name="yEnd"></param>
        /// <param name="viewerMode"></param>
        public SculptMesh(float[,] zMap, float xBegin, float xEnd, float yBegin, float yEnd, bool viewerMode)
        {
            float xStep, yStep;
            float uStep, vStep;

            int numYElements = zMap.GetLength(0);
            int numXElements = zMap.GetLength(1);

            try
            {
                xStep = (xEnd - xBegin) / (float)(numXElements - 1);
                yStep = (yEnd - yBegin) / (float)(numYElements - 1);

                uStep = 1.0f / (numXElements - 1);
                vStep = 1.0f / (numYElements - 1);
            }
            catch (DivideByZeroException)
            {
                return;
            }

            coords = [];
            faces = [];
            normals = [];
            uvs = [];

            viewerFaces = [];

            int p1, p2, p3, p4;

            int x, y;
            int xStart = 0, yStart = 0;

            for (y = yStart; y < numYElements; y++)
            {
                int rowOffset = y * numXElements;

                for (x = xStart; x < numXElements; x++)
                {
                    /*
                    *   p1-----p2
                    *   | \ f2 |
                    *   |   \  |
                    *   | f1  \|
                    *   p3-----p4
                    */

                    p4 = rowOffset + x;
                    p3 = p4 - 1;

                    p2 = p4 - numXElements;
                    p1 = p3 - numXElements;

                    Vector3 c = new(xBegin + x * xStep, yBegin + y * yStep, zMap[y, x]);
                    this.coords.Add(c);
                    if (viewerMode)
                    {
                        normals.Add(Vector3.Zero);
                        uvs.Add(new UVCoord(uStep * x, 1.0f - vStep * y));
                    }

                    if (y > 0 && x > 0)
                    {
                        Face f1, f2;

                        if (viewerMode)
                        {
                            f1 = new Face(p1, p4, p3, p1, p4, p3);
                            f1.uv1 = p1;
                            f1.uv2 = p4;
                            f1.uv3 = p3;

                            f2 = new Face(p1, p2, p4, p1, p2, p4);
                            f2.uv1 = p1;
                            f2.uv2 = p2;
                            f2.uv3 = p4;
                        }
                        else
                        {
                            f1 = new Face(p1, p4, p3);
                            f2 = new Face(p1, p2, p4);
                        }

                        faces.Add(f1);
                        faces.Add(f2);
                    }
                }
            }

            if (viewerMode)
                calcVertexNormals(SculptType.plane, numXElements, numYElements);
        }

        public SculptMesh(SKBitmap sculptBitmap, SculptType sculptType, int lod, bool viewerMode)
        {
            _SculptMesh(sculptBitmap, sculptType, lod, viewerMode, false, false);
        }

        public SculptMesh(SKBitmap sculptBitmap, SculptType sculptType, int lod, bool viewerMode, bool mirror, bool invert)
        {
            _SculptMesh(sculptBitmap, sculptType, lod, viewerMode, mirror, invert);
        }

        public SculptMesh(List<List<Vector3>> rows, SculptType sculptType, bool viewerMode, bool mirror, bool invert)
        {
            _SculptMesh(rows, sculptType, viewerMode, mirror, invert);
        }

        /// <summary>
        /// converts a bitmap to a list of lists of coords, while scaling the image.
        /// the scaling is done in floating point so as to allow for reduced vertex position
        /// quantization as the position will be averaged between pixel values. this routine will
        /// likely fail if the bitmap width and height are not powers of 2.
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="scale"></param>
        /// <param name="mirror"></param>
        /// <returns></returns>
        private List<List<Vector3>> bitmap2Coords(SKBitmap bitmap, int scale, bool mirror)
        {
            int numRows = bitmap.Height / scale;
            int numCols = bitmap.Width / scale;
            List<List<Vector3>> rows = new List<List<Vector3>>(numRows);

            float pixScale = 1.0f / (scale * scale);
            pixScale /= 255;

            int imageX, imageY = 0;

            int rowNdx, colNdx;

            for (rowNdx = 0; rowNdx < numRows; rowNdx++)
            {
                List<Vector3> row = new List<Vector3>(numCols);
                for (colNdx = 0; colNdx < numCols; colNdx++)
                {
                    imageX = colNdx * scale;
                    int imageYStart = rowNdx * scale;
                    int imageYEnd = imageYStart + scale;
                    int imageXEnd = imageX + scale;
                    float rSum = 0.0f;
                    float gSum = 0.0f;
                    float bSum = 0.0f;
                    for (; imageX < imageXEnd; imageX++)
                    {
                        for (imageY = imageYStart; imageY < imageYEnd; imageY++)
                        {
                            SKColor c = bitmap.GetPixel(imageX, imageY);
                            if (c.Alpha != 255)
                            {
                                bitmap.SetPixel(imageX, imageY, new SKColor(c.Red, c.Green, c.Blue, 255));
                                c = bitmap.GetPixel(imageX, imageY);
                            }
                            rSum += c.Red;
                            gSum += c.Green;
                            bSum += c.Blue;
                        }
                    }
                    if (mirror)
                        row.Add(new(-(rSum * pixScale - 0.5f), gSum * pixScale - 0.5f, bSum * pixScale - 0.5f));
                    else
                        row.Add(new(rSum * pixScale - 0.5f, gSum * pixScale - 0.5f, bSum * pixScale - 0.5f));

                }
                rows.Add(row);
            }
            return rows;
        }

        private List<List<Vector3>> bitmap2CoordsSampled(SKBitmap bitmap, int scale, bool mirror)
        {
            int numRows = bitmap.Height / scale;
            int numCols = bitmap.Width / scale;
            List<List<Vector3>> rows = new List<List<Vector3>>(numRows);

            float pixScale = 1.0f / 256.0f;

            int imageX, imageY = 0;

            int rowNdx, colNdx;

            for (rowNdx = 0; rowNdx <= numRows; rowNdx++)
            {
                List<Vector3> row = new List<Vector3>(numCols);
                imageY = rowNdx * scale;
                if (rowNdx == numRows) imageY--;
                for (colNdx = 0; colNdx <= numCols; colNdx++)
                {
                    imageX = colNdx * scale;
                    if (colNdx == numCols) imageX--;

                    SKColor c = bitmap.GetPixel(imageX, imageY);
                    if (c.Alpha != 255)
                    {
                        bitmap.SetPixel(imageX, imageY, new SKColor(c.Red, c.Green, c.Blue, 255));
                        c = bitmap.GetPixel(imageX, imageY);
                    }

                    if (mirror)
                        row.Add(new(-(c.Red * pixScale - 0.5f), c.Green * pixScale - 0.5f, c.Blue * pixScale - 0.5f));
                    else
                        row.Add(new(c.Red * pixScale - 0.5f, c.Green * pixScale - 0.5f, c.Blue * pixScale - 0.5f));

                }
                rows.Add(row);
            }
            return rows;
        }

        void _SculptMesh(SKBitmap sculptBitmap, SculptType sculptType, int lod, bool viewerMode, bool mirror, bool invert)
        {
            _SculptMesh(new SculptMap(sculptBitmap, lod).ToRows(mirror), sculptType, viewerMode, mirror, invert);
        }

        void _SculptMesh(List<List<Vector3>> rows, SculptType sculptType, bool viewerMode, bool mirror, bool invert)
        {
            coords = [];
            faces = [];
            normals = [];
            uvs = [];

            sculptType = (SculptType)(((int)sculptType) & 0x07);

            if (mirror)
                invert = !invert;

            viewerFaces = new List<ViewerFace>();

            int width = rows[0].Count;

            int p1, p2, p3, p4;

            int imageX, imageY;

            if (sculptType != SculptType.plane)
            {
                if (rows.Count % 2 == 0)
                {
                    for (int rowNdx = 0; rowNdx < rows.Count; rowNdx++)
                        rows[rowNdx].Add(rows[rowNdx][0]);
                }
                else
                {
                    int lastIndex = rows[0].Count - 1;

                    for (int i = 0; i < rows.Count; i++)
                        rows[i][0] = rows[i][lastIndex];
                }
            }

            Vector3 topPole = rows[0][width / 2];
            Vector3 bottomPole = rows[rows.Count - 1][width / 2];

            if (sculptType == SculptType.sphere)
            {
                if (rows.Count % 2 == 0)
                {
                    int count = rows[0].Count;
                    List<Vector3> topPoleRow = [];
                    List<Vector3> bottomPoleRow = [];

                    for (int i = 0; i < count; i++)
                    {
                        topPoleRow.Add(topPole);
                        bottomPoleRow.Add(bottomPole);
                    }
                    rows.Insert(0, topPoleRow);
                    rows.Add(bottomPoleRow);
                }
                else
                {
                    int count = rows[0].Count;

                    List<Vector3> topPoleRow = rows[0];
                    List<Vector3> bottomPoleRow = rows[rows.Count - 1];

                    for (int i = 0; i < count; i++)
                    {
                        topPoleRow[i] = topPole;
                        bottomPoleRow[i] = bottomPole;
                    }
                }
            }

            if (sculptType == SculptType.torus)
                rows.Add(rows[0]);

            int coordsDown = rows.Count;
            int coordsAcross = rows[0].Count;
            int lastColumn = coordsAcross - 1;

            float widthUnit = 1.0f / (coordsAcross - 1);
            float heightUnit = 1.0f / (coordsDown - 1);

            for (imageY = 0; imageY < coordsDown; imageY++)
            {
                int rowOffset = imageY * coordsAcross;

                for (imageX = 0; imageX < coordsAcross; imageX++)
                {
                    /*
                    *   p1-----p2
                    *   | \ f2 |
                    *   |   \  |
                    *   | f1  \|
                    *   p3-----p4
                    */

                    p4 = rowOffset + imageX;
                    p3 = p4 - 1;

                    p2 = p4 - coordsAcross;
                    p1 = p3 - coordsAcross;

                    coords.Add(rows[imageY][imageX]);
                    if (viewerMode)
                    {
                        normals.Add(Vector3.Zero);
                        uvs.Add(new(widthUnit * imageX, heightUnit * imageY));
                    }

                    if (imageY > 0 && imageX > 0)
                    {
                        Face f1, f2;

                        if (viewerMode)
                        {
                            if (invert)
                            {
                                f1 = new Face(p1, p4, p3, p1, p4, p3);
                                f1.uv1 = p1;
                                f1.uv2 = p4;
                                f1.uv3 = p3;

                                f2 = new Face(p1, p2, p4, p1, p2, p4);
                                f2.uv1 = p1;
                                f2.uv2 = p2;
                                f2.uv3 = p4;
                            }
                            else
                            {
                                f1 = new Face(p1, p3, p4, p1, p3, p4);
                                f1.uv1 = p1;
                                f1.uv2 = p3;
                                f1.uv3 = p4;

                                f2 = new Face(p1, p4, p2, p1, p4, p2);
                                f2.uv1 = p1;
                                f2.uv2 = p4;
                                f2.uv3 = p2;
                            }
                        }
                        else
                        {
                            if (invert)
                            {
                                f1 = new Face(p1, p4, p3);
                                f2 = new Face(p1, p2, p4);
                            }
                            else
                            {
                                f1 = new Face(p1, p3, p4);
                                f2 = new Face(p1, p4, p2);
                            }
                        }

                        this.faces.Add(f1);
                        this.faces.Add(f2);
                    }
                }
            }

            if (viewerMode)
                calcVertexNormals(sculptType, coordsAcross, coordsDown);
        }

        /// <summary>
        /// Duplicates a SculptMesh object. All object properties are copied by value, including lists.
        /// </summary>
        /// <returns></returns>
        public SculptMesh Copy()
        {
            return new SculptMesh(this);
        }

        public SculptMesh(SculptMesh sm)
        {
            coords = new List<Vector3>(sm.coords);
            faces = new List<Face>(sm.faces);
            viewerFaces = new List<ViewerFace>(sm.viewerFaces);
            normals = new List<Vector3>(sm.normals);
            uvs = new List<UVCoord>(sm.uvs);
        }

        private void calcVertexNormals(SculptType sculptType, int xSize, int ySize)
        {  // compute vertex normals by summing all the surface normals of all the triangles sharing
            // each vertex and then normalizing
            for (int i = 0; i < faces.Count; i++)
            {
                Face face = faces[i];
                Vector3 surfaceNormal = face.SurfaceNormal(coords);
                normals[face.n1] += surfaceNormal;
                normals[face.n2] += surfaceNormal;
                normals[face.n3] += surfaceNormal;
            }

            for (int i = 0; i < normals.Count; i++)
            {
                Vector3 n = normals[i];
                n.Normalize();
                normals[i] = n;
            }

            if (sculptType != SculptType.plane)
            { // blend the vertex normals at the cylinder seam
                for (int rowOffsetA = 0; rowOffsetA < xSize * ySize; rowOffsetA += xSize)
                {
                    int rowOffsetB = rowOffsetA + xSize - 1;
                    Vector3 v = normals[rowOffsetA] + normals[rowOffsetB];
                    v.Normalize();
                    normals[rowOffsetA] = normals[rowOffsetB] = v;
                }
            }

            foreach (Face face in faces)
            {
                ViewerFace vf = new(0)
                {
                    v1 = coords[face.v1],
                    v2 = coords[face.v2],
                    v3 = coords[face.v3],

                    coordIndex1 = face.v1,
                    coordIndex2 = face.v2,
                    coordIndex3 = face.v3,

                    n1 = normals[face.n1],
                    n2 = normals[face.n2],
                    n3 = normals[face.n3],

                    uv1 = uvs[face.uv1],
                    uv2 = uvs[face.uv2],
                    uv3 = uvs[face.uv3]
                };

                viewerFaces.Add(vf);
            }
        }

        /// <summary>
        /// Adds a value to each XYZ vertex coordinate in the mesh
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void AddPos(float x, float y, float z)
        {
            Vector3 v = new(x, y, z);
            for (int i = 0; i < coords.Count; i++)
                coords[i] += v;

            if (viewerFaces != null)
            {
                for (int i = 0; i < viewerFaces.Count; i++)
                {
                    ViewerFace vf = viewerFaces[i];
                    vf.AddPos(v);
                    viewerFaces[i] = vf;
                }
            }
        }

        /// <summary>
        /// Rotates the mesh
        /// </summary>
        /// <param name="q"></param>
        public void AddRot(Quaternion q)
        {
            if(q.IsIdentity())
                return;
            for (int i = 0; i < coords.Count; i++)
                coords[i] *= q;

            for (int i = 0; i < normals.Count; i++)
                normals[i] *= q;

            if (viewerFaces != null)
            {
                for (int i = 0; i < viewerFaces.Count; i++)
                {
                    ViewerFace vf = viewerFaces[i];
                    vf.AddRot(q);
                    viewerFaces[i] = vf;
                }
            }
        }

        public void Scale(float x, float y, float z)
        {
            Vector3 m = new(x, y, z);
            for (int i = 0; i < coords.Count; i++)
                coords[i] *= m;

            if (viewerFaces != null)
            {
                for (int i = 0; i < viewerFaces.Count; i++)
                {
                    ViewerFace vf = viewerFaces[i];
                    vf.Scale(m);
                    viewerFaces[i] = vf;
                }
            }
        }
        public void DumpRaw(string path, string name, string title)
        {
            if (path == null)
                return;
            string fileName = name + "_" + title + ".raw";
            string completePath = System.IO.Path.Combine(path, fileName);
            StreamWriter sw = new StreamWriter(completePath);

            for (int i = 0; i < faces.Count; i++)
            {
                Face face = faces[i];
                string s = $"{coords[face.v1]} {coords[face.v2]} {coords[face.v3]}";
                sw.WriteLine(s);
            }

            sw.Close();
        }
    }
}
