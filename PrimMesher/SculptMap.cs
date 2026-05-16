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

using System;
using System.Collections.Generic;
using OpenMetaverse;
using SkiaSharp;

namespace PrimMesher
{
    public class SculptMap
    {
        public int width;
        public int height;
        public byte[] redBytes;
        public byte[] greenBytes;
        public byte[] blueBytes;

        public SculptMap()
        {
        }

        public SculptMap(SKBitmap bm, int lod)
        {
            int bmW = bm.Width;
            int bmH = bm.Height;

            if (bmW == 0 || bmH == 0)
                throw new Exception("SculptMap: bitmap has no data");

            int numLodPixels = lod * 2 * lod * 2;  // (32 * 2)^2  = 64^2 pixels for default sculpt map image

            bool needsScaling = false;

            bool smallMap = bmW * bmH <= lod * lod;

            width = bmW;
            height = bmH;
            while (width * height > numLodPixels)
            {
                width >>= 1;
                height >>= 1;
                needsScaling = true;
            }

            try
            {
                if (needsScaling)
                    bm = ScaleImage(bm, width, height,
                        SKFilterMode.Nearest);
            }

            catch (Exception e)
            {
                throw new Exception("Exception in ScaleImage(): e: " + e.ToString());
            }

            if (width * height > lod * lod)
            {
                width >>= 1;
                height >>= 1;
            }

            int numBytes = smallMap ? width * height : (width + 1) * (height + 1);
            redBytes = new byte[numBytes];
            greenBytes = new byte[numBytes];
            blueBytes = new byte[numBytes];

            int byteNdx = 0;

            try
            {
                if (smallMap)
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            SKColor c = bm.GetPixel(x, y);

                            redBytes[byteNdx] = c.Red;
                            greenBytes[byteNdx] = c.Green;
                            blueBytes[byteNdx] = c.Blue;

                            ++byteNdx;
                        }
                    }
                else
                    for (int y = 0; y <= height; y++)
                    {
                        for (int x = 0; x <= width; x++)
                        {
                            SKColor c = bm.GetPixel(x < width ? x * 2 : x * 2 - 1,
                                                y < height ? y * 2 : y * 2 - 1);

                            redBytes[byteNdx] = c.Red;
                            greenBytes[byteNdx] = c.Green;
                            blueBytes[byteNdx] = c.Blue;

                            ++byteNdx;
                        }
                    }
            }
            catch (Exception e)
            {
                throw new Exception("Caught exception processing byte arrays in SculptMap(): e: " + e.ToString());
            }

            if (!smallMap)
            {
                width++;
                height++;
            }
        }

        public List<List<Vector3>> ToRows(bool mirror)
        {
            int numRows = height;
            int numCols = width;

            List<List<Vector3>> rows = new List<List<Vector3>>(numRows);

            const float pixScale = 1.0f / 255.0f;

            int rowNdx, colNdx;
            int smNdx = 0;

            for (rowNdx = 0; rowNdx < numRows; rowNdx++)
            {
                List<Vector3> row = new List<Vector3>(numCols);
                for (colNdx = 0; colNdx < numCols; colNdx++)
                {
                    if (mirror)
                        row.Add(new Vector3(-(redBytes[smNdx] * pixScale - 0.5f), (greenBytes[smNdx] * pixScale - 0.5f), blueBytes[smNdx] * pixScale - 0.5f));
                    else
                        row.Add(new Vector3(redBytes[smNdx] * pixScale - 0.5f, greenBytes[smNdx] * pixScale - 0.5f, blueBytes[smNdx] * pixScale - 0.5f));

                    ++smNdx;
                }
                rows.Add(row);
            }
            return rows;
        }

        private SKBitmap ScaleImage(SKBitmap srcImage, int destWidth, int destHeight,
                SKFilterMode filterMode)
        {
            SKBitmap scaledImage = new SKBitmap(destWidth, destHeight, srcImage.ColorType, srcImage.AlphaType,
                srcImage.ColorSpace);
            SKSamplingOptions sampling = new SKSamplingOptions(filterMode, SKMipmapMode.None);
            srcImage.ScalePixels(scaledImage, sampling);
            return scaledImage;
        }
    }
}

