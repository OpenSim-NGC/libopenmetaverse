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

using CoreJ2K;
using SkiaSharp;
using System;

namespace OpenMetaverse.Imaging
{
    /// <summary>
    /// JPEG2000 codec wrapper implemented with CoreJ2K.
    /// </summary>
    public static class OpenJPEG
    {
        /// <summary>TGA Header size</summary>
        public const int TGA_HEADER_SIZE = 32;

        /// <summary>
        /// Defines the beginning and ending file positions of a layer in an
        /// LRCP-progression JPEG2000 file
        /// </summary>
        [System.Diagnostics.DebuggerDisplay("Start = {Start} End = {End} Size = {End - Start}")]
        public struct J2KLayerInfo
        {
            public int Start;
            public int End;
        }

        public static byte[] Encode(ManagedImage image, bool lossless)
        {
            if ((image.Channels & ManagedImage.ImageChannels.Color) == 0 ||
                ((image.Channels & ManagedImage.ImageChannels.Bump) != 0 && (image.Channels & ManagedImage.ImageChannels.Alpha) == 0))
                throw new ArgumentException("JPEG2000 encoding is not supported for this channel combination");

            using SKBitmap bitmap = image.ExportBitmap();
            return J2kImage.ToBytes(bitmap);
        }

        public static byte[] Encode(ManagedImage image)
        {
            return Encode(image, false);
        }

        /// <summary>
        /// Decode JPEG2000 data to a ManagedImage and SKBitmap.
        /// </summary>
        public static bool DecodeToImage(byte[] encoded, out ManagedImage managedImage, out SKBitmap image)
        {
            managedImage = null;
            image = null;

            try
            {
                var decoded = J2kImage.FromBytes(encoded);
                image = decoded.As<SKBitmap>();
                managedImage = new ManagedImage(image);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log("Failed to decode JPEG2000 data", Helpers.LogLevel.Error, ex);
                image?.Dispose();
                image = null;
                managedImage = null;
                return false;
            }
        }

        public static bool DecodeToImage(byte[] encoded, out ManagedImage managedImage)
        {
            managedImage = null;

            try
            {
                var decoded = J2kImage.FromBytes(encoded);
                using SKBitmap image = decoded.As<SKBitmap>();
                managedImage = new ManagedImage(image);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log("Failed to decode JPEG2000 data", Helpers.LogLevel.Error, ex);
                managedImage = null;
                return false;
            }
        }

        public static bool DecodeLayerBoundaries(byte[] encoded, out J2KLayerInfo[] layerInfo, out int components)
        {
            layerInfo = null;
            components = 0;

            if (!DecodeToImage(encoded, out ManagedImage managedImage) || managedImage == null)
                return false;

            if ((managedImage.Channels & ManagedImage.ImageChannels.Color) != 0) components += 3;
            if ((managedImage.Channels & ManagedImage.ImageChannels.Gray) != 0) components += 1;
            if ((managedImage.Channels & ManagedImage.ImageChannels.Alpha) != 0) components += 1;
            if ((managedImage.Channels & ManagedImage.ImageChannels.Bump) != 0) components += 1;

            layerInfo = new J2KLayerInfo[1];
            layerInfo[0].Start = 0;
            layerInfo[0].End = encoded?.Length ?? 0;
            return true;
        }

        public static byte[] EncodeFromImage(SKBitmap bitmap, bool lossless)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap));

            return J2kImage.ToBytes(bitmap);
        }
    }
}
