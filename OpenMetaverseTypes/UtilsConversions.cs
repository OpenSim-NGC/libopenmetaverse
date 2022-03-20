/*
 * Copyright (c) 2006-2016, openmetaverse.co
 * Copyright (c) Contributors, http://opensimulator.org/
 * See CONTRIBUTORS.TXT for a full list of copyright holders.
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
using System.Net;
using System.Text;
using System.Reflection;
using System.IO;
using System.Runtime.CompilerServices;

namespace OpenMetaverse
{
    public static partial class Utils
    {
//        public static readonly bool NoAlignment = CheckNeedAlignment();
        public static readonly bool CanDirectCopyLE = CheckNeedAlignment();
//        public static readonly bool CanDirectCopyBE = CheckDirectCopyBE();

        public unsafe static bool CheckNeedAlignment()
        {
            if(!BitConverter.IsLittleEndian)
                return false;

            byte[] bytes = new byte[4096];
            long ll = 0x55AA33EE55FFCCAA;
            long l;
            try
            {
                for (int i = 0; i < 512; i += 3)
                {
                    fixed (byte* ptr = &bytes[253 + i])
                        *(long*)ptr = ll;

                    fixed (byte* ptr = &bytes[1027 + i])
                    {
                        byte* p = ptr;
                        *p++ = (byte)ll;
                        *p++ = (byte)(ll >> 8);
                        *p++ = (byte)(ll >> 16);
                        *p++ = (byte)(ll >> 24);
                        *p++ = (byte)(ll >> 32);
                        *p++ = (byte)(ll >> 40);
                        *p++ = (byte)(ll >> 48);
                        *p = (byte)(ll >> 56);
                    }

                    fixed (byte* ptr = &bytes[253 + i])
                        l = *(long*)ptr;
                    if (l != ll)
                        return false;

                    fixed (byte* ptr = &bytes[1027 + i])
                        l = *(long*)ptr;
                    if (l != ll)
                        return false;
                }
                return true;
            }
            catch { }
            return false;

        }
/*
        static bool CheckDirectCopyLE()
        {
            return BitConverter.IsLittleEndian && NoAlignment;
        }

        static bool CheckDirectCopyBE()
        {
            return !BitConverter.IsLittleEndian && NoAlignment;
        }
*/
        #region String Arrays

        private static readonly string[] _AssetTypeNames = new string[]
        {
            "texture",    //  0
            "sound",      //  1
            "callcard",   //  2
            "landmark",   //  3
            "script",     //  4
            "clothing",   //  5
            "object",     //  6
            "notecard",   //  7
            "category",   //  8
            string.Empty, //  9
            "lsltext",    // 10
            "lslbyte",    // 11
            "txtr_tga",   // 12
            "bodypart",   // 13
            string.Empty, // 14
            string.Empty, // 15
            string.Empty, // 16
            "snd_wav",    // 17
            "img_tga",    // 18
            "jpeg",       // 19
            "animatn",    // 20
            "gesture",    // 21
            "simstate",   // 22
            string.Empty, // 23
            "link",       // 24
            "link_f",     // 25
            string.Empty, // 26
            string.Empty, // 27
            string.Empty, // 28
            string.Empty, // 29
            string.Empty, // 30
            string.Empty, // 31
            string.Empty, // 32
            string.Empty, // 33
            string.Empty, // 34
            string.Empty, // 35
            string.Empty, // 36
            string.Empty, // 37
            string.Empty, // 38
            string.Empty, // 39
            string.Empty, // 40
            string.Empty, // 41
            string.Empty, // 42
            string.Empty, // 43
            string.Empty, // 44
            string.Empty, // 45
            string.Empty, // 46
            string.Empty, // 47
            string.Empty, // 48
            "mesh",       // 49
            string.Empty, // 50
            string.Empty, // 51
            string.Empty, // 52
            string.Empty, // 53
            string.Empty, // 54
            string.Empty, // 55
            "settings",   // 56
            string.Empty, // 57
        };

        private static readonly string[] _FolderTypeNames = new string[]
        {
            "texture",    //  0
            "sound",      //  1
            "callcard",   //  2
            "landmark",   //  3
            string.Empty, //  4
            "clothing",   //  5
            "object",     //  6
            "notecard",   //  7
            "root_inv",   //  8
            string.Empty, //  9
            "lsltext",    // 10
            string.Empty, // 11
            string.Empty, // 12
            "bodypart",   // 13
            "trash",      // 14
            "snapshot",   // 15
            "lstndfnd",   // 16
            string.Empty, // 17
            string.Empty, // 18
            string.Empty, // 19
            "animatn",    // 20
            "gesture",    // 21
            string.Empty, // 22
            "favorite",   // 23
            string.Empty, // 24
            "settings",   // 25
            "ensemble",   // 26
            "ensemble",   // 27
            "ensemble",   // 28
            "ensemble",   // 29
            "ensemble",   // 30
            "ensemble",   // 31
            "ensemble",   // 32
            "ensemble",   // 33
            "ensemble",   // 34
            "ensemble",   // 35
            "ensemble",   // 36
            "ensemble",   // 37
            "ensemble",   // 38
            "ensemble",   // 39
            "ensemble",   // 40
            "ensemble",   // 41
            "ensemble",   // 42
            "ensemble",   // 43
            "ensemble",   // 44
            "ensemble",   // 45
            "current",    // 46
            "outfit",     // 47
            "my_otfts",   // 48
            "mesh",       // 49
            "inbox",      // 50
            "outbox",     // 51
            "basic_rt",   // 52
            "merchant",   // 53
            "stock",      // 54
        };

        private static readonly string[] _InventoryTypeNames = new string[]
        {
            "texture",    //  0
            "sound",      //  1
            "callcard",   //  2
            "landmark",   //  3
            string.Empty, //  4
            string.Empty, //  5
            "object",     //  6
            "notecard",   //  7
            "category",   //  8
            "root",       //  9
            "script",     // 10
            string.Empty, // 11
            string.Empty, // 12
            string.Empty, // 13
            string.Empty, // 14
            "snapshot",   // 15
            string.Empty, // 16
            "attach",     // 17
            "wearable",   // 18
            "animation",  // 19
            "gesture",    // 20
            string.Empty, // 21
            "mesh",       // 22
            string.Empty, // 23
            string.Empty, // 24
            "settings",   // 25
            string.Empty, // 26
        };

        private static readonly string[] _SaleTypeNames = new string[]
        {
            "not",
            "orig",
            "copy",
            "cntn"
        };

        private static readonly string[] _AttachmentPointNames = new string[]
        {
            string.Empty,
            "ATTACH_CHEST",
            "ATTACH_HEAD",
            "ATTACH_LSHOULDER",
            "ATTACH_RSHOULDER",
            "ATTACH_LHAND",
            "ATTACH_RHAND",
            "ATTACH_LFOOT",
            "ATTACH_RFOOT",
            "ATTACH_BACK",
            "ATTACH_PELVIS",
            "ATTACH_MOUTH",
            "ATTACH_CHIN",
            "ATTACH_LEAR",
            "ATTACH_REAR",
            "ATTACH_LEYE",
            "ATTACH_REYE",
            "ATTACH_NOSE",
            "ATTACH_RUARM",
            "ATTACH_RLARM",
            "ATTACH_LUARM",
            "ATTACH_LLARM",
            "ATTACH_RHIP",
            "ATTACH_RULEG",
            "ATTACH_RLLEG",
            "ATTACH_LHIP",
            "ATTACH_LULEG",
            "ATTACH_LLLEG",
            "ATTACH_BELLY",
            "ATTACH_LPEC",
            "ATTACH_RPEC",
            "ATTACH_HUD_CENTER_2",
            "ATTACH_HUD_TOP_RIGHT",
            "ATTACH_HUD_TOP_CENTER",
            "ATTACH_HUD_TOP_LEFT",
            "ATTACH_HUD_CENTER_1",
            "ATTACH_HUD_BOTTOM_LEFT",
            "ATTACH_HUD_BOTTOM",
            "ATTACH_HUD_BOTTOM_RIGHT",
            "ATTACH_NECK",
            "ATTACH_AVATAR_CENTER",
            "ATTACH_LHAND_RING1",
            "ATTACH_RHAND_RING1",
            "ATTACH_TAIL_BASE",
            "ATTACH_TAIL_TIP",
            "ATTACH_LWING",
            "ATTACH_RWING",
            "ATTACH_FACE_JAW",
            "ATTACH_FACE_LEAR",
            "ATTACH_FACE_REAR",
            "ATTACH_FACE_LEYE",
            "ATTACH_FACE_REYE",
            "ATTACH_FACE_TONGUE",
            "ATTACH_GROIN",
            "ATTACH_HIND_LFOOT",
            "ATTACH_HIND_RFOOT"
    };

        public static bool InternStrings = false;

        #endregion String Arrays

        #region BytesTo

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ApproxEqual(float a, float b, float tolerance, float reltolerance = float.Epsilon)
        {
            float dif = Math.Abs(a - b);
            if (dif <= tolerance)
                return true;

            a = Math.Abs(a);
            b = Math.Abs(b);
            if (b > a)
                a = b;
            return dif <= a * reltolerance;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ApproxZero(float a, float tolerance)
        {
            return Math.Abs(a) <= tolerance;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ApproxZero(float a)
        {
            return Math.Abs(a) <= 1e-6;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ApproxEqual(float a, float b)
        {
            float dif = Math.Abs(a - b);
            if (dif <= 1e-6f)
                return true;

            a = Math.Abs(a);
            b = Math.Abs(b);
            if (b > a)
                a = b;
            return dif <= a * float.Epsilon;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CombineHash(int a, int b)
        {
            return ((a << 5) + a) ^ b;
        }

        /// <summary>
        /// Convert the first two bytes starting in the byte array in
        /// little endian ordering to a signed short integer
        /// </summary>
        /// <param name="bytes">An array two bytes or longer</param>
        /// <returns>A signed short integer, will be zero if a short can't be
        /// read at the given position</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe short BytesToInt16(byte[] bytes)
        {
            //if (bytes.Length < 2 ) return 0;
            if (CanDirectCopyLE)
            {
                fixed (byte* p = bytes)
                    return *(short*)p;
            }
            else
                return (short)(bytes[0] | (bytes[1] << 8));
        }

        /// <summary>
        /// Convert the first two bytes starting at the given position in
        /// little endian ordering to a signed short integer
        /// </summary>
        /// <param name="bytes">An array two bytes or longer</param>
        /// <param name="pos">Position in the array to start reading</param>
        /// <returns>A signed short integer, will be zero if a short can't be
        /// read at the given position</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe short BytesToInt16(byte[] bytes, int pos)
        {
            //if (bytes.Length < pos + 2) return 0;
            if (CanDirectCopyLE)
            {
                fixed (byte* p = &bytes[pos])
                    return *(short*)p;
            }
            else
                return (short)(bytes[pos] | (bytes[pos + 1] << 8));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe short BytesToInt16(byte* bytes)
        {
            //if (bytes.Length < pos + 2) return 0;
            if (CanDirectCopyLE)
            {
                return *(short*)bytes;
            }
            else
                return (short)(*bytes | (bytes[1] << 8));
        }

        /// <summary>
        /// Convert the first four bytes starting at the given position in
        /// little endian ordering to a signed integer
        /// </summary>
        /// <param name="bytes">An array four bytes or longer</param>
        /// <param name="pos">Position to start reading the int from</param>
        /// <returns>A signed integer, will be zero if an int can't be read
        /// at the given position</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static int BytesToInt(byte[] bytes, int pos)
        {
            if (CanDirectCopyLE)
            {
                if (bytes.Length < pos + 4) return 0;
                fixed (byte* p = &bytes[pos])
                    return *(int*)p;
            }

            return bytes[pos] |
                    (bytes[pos + 1] << 8) |
                    (bytes[pos + 2] << 16) |
                    (bytes[pos + 3] << 24);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static int BytesToInt(byte* bytes)
        {
            if (CanDirectCopyLE)
            {
                    return *(int*)bytes;
            }

            return *bytes |
                    (bytes[1] << 8) |
                    (bytes[2] << 16) |
                    (bytes[3] << 24);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static int BytesToIntSafepos(byte[] bytes, int pos)
        {
            if (CanDirectCopyLE)
            {
                fixed (byte* p = &bytes[pos])
                    return *(int*)p;
            }

            return bytes[pos] |
                    (bytes[pos + 1] << 8) |
                    (bytes[pos + 2] << 16) |
                    (bytes[pos + 3] << 24);
        }

        /// <summary>
        /// Convert the first four bytes of the given array in little endian
        /// ordering to a signed integer
        /// </summary>
        /// <param name="bytes">An array four bytes or longer</param>
        /// <returns>A signed integer, will be zero if the array contains
        /// less than four bytes</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static int BytesToInt(byte[] bytes)
        {
            if (CanDirectCopyLE)
            {
                fixed (byte* p = bytes)
                    return *(int*)p;
            }
            else
                return bytes[0]      |
                    (bytes[1] << 8)  |
                    (bytes[2] << 16) |
                    (bytes[3] << 24);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BytesToIntBig(byte[] bytes)
        {
            return (bytes[0] << 24) |
                   (bytes[1] << 16) |
                   (bytes[2] << 8) |
                   bytes[3];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BytesToIntBig(byte[] bytes, int pos)
        {
            return (bytes[pos] << 24) |
                   (bytes[pos + 1] << 16) |
                   (bytes[pos + 2] << 8) |
                   bytes[pos + 3];
        }

        /// <summary>
        /// Convert the first eight bytes of the given array in little endian
        /// ordering to a signed long integer
        /// </summary>
        /// <param name="bytes">An array eight bytes or longer</param>
        /// <returns>A signed long integer, will be zero if the array contains
        /// less than eight bytes</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static long BytesToInt64(byte[] bytes)
        {
            if (CanDirectCopyLE)
            {
                fixed (byte* p = bytes)
                    return *(long*)p;
            }
            else
                return
                    bytes[0] |
                    ((long)bytes[1] << 8) |
                    ((long)bytes[2] << 16) |
                    ((long)bytes[3] << 24) |
                    ((long)bytes[4] << 32) |
                    ((long)bytes[5] << 40) |
                    ((long)bytes[6] << 48) |
                    ((long)bytes[7] << 56);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long BytesToInt64Big(byte[] bytes)
        {
            return
                ((long)bytes[0] << 56) |
                ((long)bytes[1] << 48) |
                ((long)bytes[2] << 40) |
                ((long)bytes[3] << 32) |
                ((long)bytes[4] << 24) |
                ((long)bytes[5] << 16) |
                ((long)bytes[6] << 8) |
                ((long)bytes[7]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static long BytesToInt64(byte* bytes)
        {
            if (CanDirectCopyLE)
            {
                return *(long*)bytes;
            }
            else
                return
                    bytes[0] |
                    ((long)bytes[1] << 8) |
                    ((long)bytes[2] << 16) |
                    ((long)bytes[3] << 24) |
                    ((long)bytes[4] << 32) |
                    ((long)bytes[5] << 40) |
                    ((long)bytes[6] << 48) |
                    ((long)bytes[7] << 56);
        }

        /// <summary>
        /// Convert the first eight bytes starting at the given position in
        /// little endian ordering to a signed long integer
        /// </summary>
        /// <param name="bytes">An array eight bytes or longer</param>
        /// <param name="pos">Position to start reading the long from</param>
        /// <returns>A signed long integer, will be zero if a long can't be read
        /// at the given position</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static long BytesToInt64(byte[] bytes, int pos)
        {
            if (CanDirectCopyLE)
            {
                if (bytes.Length < pos + 8) return 0;
                fixed (byte* p = &bytes[pos])
                    return *(long*)p;
            }
            else
                return
                    (bytes[pos] |
                    ((long)bytes[pos + 1] << 8)) |
                    ((long)bytes[pos + 2] << 16) |
                    ((long)bytes[pos + 3] << 24) |
                    ((long)bytes[pos + 4] << 32) |
                    ((long)bytes[pos + 5] << 40) |
                    ((long)bytes[pos + 6] << 48) |
                    ((long)bytes[pos + 7] << 56);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static long BytesToInt64Safepos(byte[] bytes, int pos)
        {
            if (CanDirectCopyLE)
            {
                fixed (byte* p = &bytes[pos])
                    return *(long*)p;
            }
            else
                return
                    (bytes[pos] |
                    ((long)bytes[pos + 1] << 8)) |
                    ((long)bytes[pos + 2] << 16) |
                    ((long)bytes[pos + 3] << 24) |
                    ((long)bytes[pos + 4] << 32) |
                    ((long)bytes[pos + 5] << 40) |
                    ((long)bytes[pos + 6] << 48) |
                    ((long)bytes[pos + 7] << 56);
        }

        /// <summary>
        /// Convert the first two bytes starting at the given position in
        /// little endian ordering to an unsigned short
        /// </summary>
        /// <param name="bytes">Byte array containing the ushort</param>
        /// <param name="pos">Position to start reading the ushort from</param>
        /// <returns>An unsigned short, will be zero if a ushort can't be read
        /// at the given position</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe ushort BytesToUInt16(byte[] bytes, int pos)
        {
            //if (bytes.Length < pos + 2) return 0;
            if (CanDirectCopyLE)
            {
                fixed (byte* p = &bytes[pos])
                    return *(ushort*)p;
            }
            else
                return (ushort)(bytes[pos] + (bytes[pos + 1] << 8));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe ushort BytesToUInt16(byte* bytes)
        {
            //if (bytes.Length < pos + 2) return 0;
            if (CanDirectCopyLE)
                return *(ushort*)bytes;
            else
                return (ushort)(*bytes + (bytes[1] << 8));
        }

        /// <summary>
        /// Convert two bytes in little endian ordering to an unsigned short
        /// </summary>
        /// <param name="bytes">Byte array containing the ushort</param>
        /// <returns>An unsigned short, will be zero if a ushort can't be
        /// read</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe ushort BytesToUInt16(byte[] bytes)
        {
            //if (bytes.Length < 2) return 0;
            if (CanDirectCopyLE)
            {
                fixed (byte* p = bytes)
                    return *(ushort*)p;
            }
            else
                return (ushort)(bytes[0] + (bytes[1] << 8));
        }

        /// <summary>
        /// Convert the first four bytes starting at the given position in
        /// little endian ordering to an unsigned integer
        /// </summary>
        /// <param name="bytes">Byte array containing the uint</param>
        /// <param name="pos">Position to start reading the uint from</param>
        /// <returns>An unsigned integer, will be zero if a uint can't be read
        /// at the given position</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static uint BytesToUInt(byte[] bytes, int pos)
        {
            if (CanDirectCopyLE)
            {
                if (bytes.Length < pos + 4) return 0;
                fixed (byte* p = &bytes[pos])
                    return *(uint*)p;
            }
            else
                return (uint)(
                    (bytes[pos]) |
                    (bytes[pos + 1] << 8) |
                    (bytes[pos + 2] << 16) |
                    (bytes[pos + 3] << 24));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static uint BytesToUIntSafepos(byte[] bytes, int pos)
        {
            if (CanDirectCopyLE)
            {
                fixed (byte* p = &bytes[pos])
                    return *(uint*)p;
            }
            else
                return (uint)(
                    (bytes[pos]) |
                    (bytes[pos + 1] << 8) |
                    (bytes[pos + 2] << 16) |
                    (bytes[pos + 3] << 24));
        }

        /// <summary>
        /// Convert the first four bytes of the given array in little endian
        /// ordering to an unsigned integer
        /// </summary>
        /// <param name="bytes">An array four bytes or longer</param>
        /// <returns>An unsigned integer, will be zero if the array contains
        /// less than four bytes</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static uint BytesToUInt(byte[] bytes)
        {
            if (CanDirectCopyLE)
            {
                fixed (byte* p = bytes)
                    return *(uint*)p;
            }
            else
                return (uint)(
                    bytes[0] |
                    (bytes[1] << 8) |
                    (bytes[2] << 16) |
                    (bytes[3] << 24));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static uint BytesToUInt(byte* bytes)
        {
            if (CanDirectCopyLE)
            {
                return *(uint*)bytes;
            }
            else
                return (uint)(
                    bytes[0] |
                    (bytes[1] << 8) |
                    (bytes[2] << 16) |
                    (bytes[3] << 24));
        }

        /// <summary>
        /// Convert the first eight bytes starting at the given position of the given array in little endian
        /// ordering to an unsigned 64-bit integer
        /// </summary>
        /// <param name="bytes">An array eight bytes or longer</param>
        /// <returns>An unsigned 64-bit integer, will be zero if the array
        /// contains less than eight bytes</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static ulong BytesToUInt64(byte[] bytes, int pos)
        {
            if (CanDirectCopyLE)
            {
                if (bytes.Length < pos + 8) return 0;
                fixed (byte* p = &bytes[pos])
                    return *(ulong*)p;
            }
            else
                return (ulong)(
                    bytes[pos] |
                    ((long)bytes[pos + 1] << 8) |
                    ((long)bytes[pos + 2] << 16) |
                    ((long)bytes[pos + 3] << 24) |
                    ((long)bytes[pos + 4] << 32) |
                    ((long)bytes[pos + 5] << 40) |
                    ((long)bytes[pos + 6] << 48) |
                    ((long)bytes[pos + 7] << 56));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static ulong BytesToUInt64Safepos(byte[] bytes, int pos)
        {
            if (CanDirectCopyLE)
            {
                fixed (byte* p = &bytes[pos])
                    return *(ulong*)p;
            }
            else
                return (ulong)(
                    bytes[pos] |
                    ((long)bytes[pos + 1] << 8) |
                    ((long)bytes[pos + 2] << 16) |
                    ((long)bytes[pos + 3] << 24) |
                    ((long)bytes[pos + 4] << 32) |
                    ((long)bytes[pos + 5] << 40) |
                    ((long)bytes[pos + 6] << 48) |
                    ((long)bytes[pos + 7] << 56));
        }

        /// <summary>
        /// Convert the first eight bytes of the given array in little endian
        /// ordering to an unsigned 64-bit integer
        /// </summary>
        /// <param name="bytes">An array eight bytes or longer</param>
        /// <returns>An unsigned 64-bit integer, will be zero if the array
        /// contains less than eight bytes</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static ulong BytesToUInt64(byte[] bytes)
        {
            if (CanDirectCopyLE)
            {
                fixed (byte* p = bytes)
                    return *(ulong*)p;
            }
            else
                return (ulong)(
                    bytes[0]               |
                    ((long)bytes[1] << 8)  |
                    ((long)bytes[2] << 16) |
                    ((long)bytes[3] << 24) |
                    ((long)bytes[4] << 32) |
                    ((long)bytes[5] << 40) |
                    ((long)bytes[6] << 48) |
                    ((long)bytes[7] << 56));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static ulong BytesToUInt64(byte* bytes)
        {
            if (CanDirectCopyLE)
            {
                return *(ulong*)bytes;
            }
            else
                return (ulong)(
                    bytes[0] |
                    ((long)bytes[1] << 8) |
                    ((long)bytes[2] << 16) |
                    ((long)bytes[3] << 24) |
                    ((long)bytes[4] << 32) |
                    ((long)bytes[5] << 40) |
                    ((long)bytes[6] << 48) |
                    ((long)bytes[7] << 56));
        }

        /// <summary>
        /// Convert four bytes in little endian ordering to a floating point
        /// value
        /// </summary>
        /// <param name="bytes">Byte array containing a little ending floating
        /// point value</param>
        /// <param name="pos">Starting position of the floating point value in
        /// the byte array</param>
        /// <returns>Single precision value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static float BytesToFloat(byte[] bytes)
        {
                int tmp = BytesToInt(bytes);
                return *(float*)&tmp;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static float BytesToFloat(byte* bytes)
        {
            int tmp = BytesToInt(bytes);
            return *(float*)&tmp;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static float BytesToFloat(byte[] bytes, int pos)
        {
            int tmp = BytesToInt(bytes, pos);
            return *(float*)&tmp;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static float BytesToFloatSafepos(byte[] bytes, int pos)
        {
            int tmp = BytesToIntSafepos(bytes, pos);
            return *(float*)&tmp;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static double BytesToDouble(byte[] bytes)
        {
            long tmp = BytesToInt64(bytes);
            return *(double*)&tmp;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static double BytesToDoubleBig(byte[] bytes)
        {
            long tmp = BytesToInt64Big(bytes);
            return *(double*)&tmp;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static double BytesToDouble(byte* bytes)
        {
            long tmp = BytesToInt64(bytes);
            return *(double*)&tmp;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static double BytesToDouble(byte[] bytes, int pos)
        {
            long tmp = BytesToInt64(bytes, pos);
            return *(double*)&tmp;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static double BytesToDoubleSafepos(byte[] bytes, int pos)
        {
            long tmp = BytesToInt64Safepos(bytes, pos);
            return *(double*)&tmp;
        }

        #endregion BytesTo

        #region ToBytes
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] Int16ToBytes(short value)
        {
            byte[] bytes = new byte[2];
            bytes[0] = (byte)(value);
            bytes[1] = (byte)((value >> 8));
            return bytes;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Int16ToBytes(Stream ms, short value)
        {
            ms.WriteByte((byte)value);
            ms.WriteByte((byte)(value >> 8));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Int16ToBytes(short value, byte[] dest, int pos)
        {
            dest[pos] = (byte)(value);
            dest[pos + 1] = (byte)((value >> 8));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Int16ToBytes(short value, byte* dest)
        {
            *dest = (byte)(value);
            dest[1] = (byte)((value >> 8));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] UInt16ToBytes(ushort value)
        {
            byte[] bytes = new byte[2];
            bytes[0] = (byte)(value);
            bytes[1] = (byte)((value >> 8));
            return bytes;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void UInt16ToBytes(ushort value, byte[] dest, int pos)
        {
            dest[pos] = (byte)(value);
            dest[pos + 1] = (byte)((value >> 8));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void UInt16ToBytes(ushort value, byte* dest)
        {
            *dest = (byte)(value);
            dest[1] = (byte)((value >> 8));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void UInt16ToBytesBig(ushort value, byte[] dest, int pos)
        {
            dest[pos] = (byte)((value >> 8));
            dest[pos + 1] = (byte)(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void UInt16ToBytesBig(ushort value, byte* dest)
        {
            *dest = (byte)((value >> 8));
            dest[1] = (byte)(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IntToBytes(Stream ms, int value)
        {
            ms.WriteByte((byte)value);
            ms.WriteByte((byte)(value >> 8));
            ms.WriteByte((byte)(value >> 16));
            ms.WriteByte((byte)(value >> 24));
        }

        /// <summary>
        /// Convert an integer to a byte array in little endian format
        /// </summary>
        /// <param name="value">The integer to convert</param>
        /// <returns>A four byte little endian array</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static byte[] IntToBytes(int value)
        {
            byte[] bytes = new byte[4];
            if (CanDirectCopyLE)
            {
                fixed (byte* p = bytes)
                    *(int*)p = value;
            }
            else
            {
                bytes[0] = (byte)value;
                bytes[1] = (byte)(value >> 8);
                bytes[2] = (byte)(value >> 16);
                bytes[3] = (byte)(value >> 24);
            }
            return bytes;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void IntToBytes(int value, byte[] dest, int pos)
        {
            if (CanDirectCopyLE)
            {
                if (dest.Length < pos + 4) return;
                fixed (byte* p = &dest[pos])
                    *(int*)p = value;
            }
            else
            {
                dest[pos] = (byte)(value);
                dest[pos + 1] = (byte)((value >> 8));
                dest[pos + 2] = (byte)((value >> 16));
                dest[pos + 3] = (byte)((value >> 24));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void IntToBytes(int value, byte* dest)
        {
            if (CanDirectCopyLE)
            {
                *(int*)dest = value;
            }
            else
            {
                *dest = (byte)(value);
                dest[1] = (byte)((value >> 8));
                dest[2] = (byte)((value >> 16));
                dest[3] = (byte)((value >> 24));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void IntToBytesSafepos(int value, byte[] dest, int pos)
        {
            if (CanDirectCopyLE)
            {
                fixed (byte* p = &dest[pos])
                    *(int*)p = value;
            }
            else
            {
                dest[pos] = (byte)(value);
                dest[pos + 1] = (byte)((value >> 8));
                dest[pos + 2] = (byte)((value >> 16));
                dest[pos + 3] = (byte)((value >> 24));
            }
        }

        /// <summary>
        /// Convert an integer to a byte array in big endian format
        /// </summary>
        /// <param name="value">The integer to convert</param>
        /// <returns>A four byte big endian array</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] IntToBytesBig(int value)
        {
            byte[] bytes = new byte[4];

            bytes[0] = (byte)(value >> 24);
            bytes[1] = (byte)(value >> 16);
            bytes[2] = (byte)(value >> 8);
            bytes[3] = (byte)value;

            return bytes;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IntToBytesBig(int value, byte[] bytes, int pos)
        {
            if (bytes.Length < pos + 4) return;
            bytes[pos] = (byte)(value >> 24);
            bytes[pos + 1] = (byte)(value >> 16);
            bytes[pos + 2] = (byte)(value >> 8);
            bytes[pos + 3] = (byte)value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void IntToBytesBig(int value, byte* bytes)
        {
            *bytes = (byte)(value >> 24);
            bytes[1] = (byte)(value >> 16);
            bytes[2] = (byte)(value >> 8);
            bytes[3] = (byte)value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static byte[] UIntToBytes(uint value)
        {
            return IntToBytes((int)value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void UIntToBytes(uint value, byte[] dest, int pos)
        {
            IntToBytes((int)value, dest, pos);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void UIntToBytesSafepos(uint value, byte[] dest, int pos)
        {
            IntToBytesSafepos((int)value, dest, pos);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void UIntToBytesBig(uint value, byte[] dest, int pos)
        {
           IntToBytesBig((int)value, dest, pos);
        }

        /// <summary>
        /// Convert a 64-bit integer to a byte array in little endian format
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <returns>An 8 byte little endian array</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static byte[] Int64ToBytes(long value)
        {
            byte[] bytes = new byte[8];
            if (CanDirectCopyLE)
            {
                fixed (byte* p = bytes)
                    *(long*)p = value;
            }
            else
            {
                bytes[0] = (byte)value;
                bytes[1] = (byte)(value >> 8);
                bytes[2] = (byte)(value >> 16);
                bytes[3] = (byte)(value >> 24);
                bytes[4] = (byte)(value >> 32);
                bytes[5] = (byte)(value >> 40);
                bytes[6] = (byte)(value >> 48);
                bytes[7] = (byte)(value >> 56);
            }
            return bytes;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void Int64ToBytes(long value, byte[] dest, int pos)
        {
            if (CanDirectCopyLE)
            {
                if (dest.Length < pos + 8) return;
                fixed (byte* p = &dest[pos])
                    *(long*)p = value;
            }
            else
            {
                dest[pos] = (byte)value;
                dest[pos + 1] = (byte)(value >> 8);
                dest[pos + 2] = (byte)(value >> 16);
                dest[pos + 3] = (byte)(value >> 24);
                dest[pos + 4] = (byte)(value >> 32);
                dest[pos + 5] = (byte)(value >> 40);
                dest[pos + 6] = (byte)(value >> 48);
                dest[pos + 7] = (byte)(value >> 56);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void Int64ToBytes(long value, byte* dest)
        {
            if (CanDirectCopyLE)
            {
                *(long*)dest = value;
            }
            else
            {
                *dest = (byte)value;
                dest[1] = (byte)(value >> 8);
                dest[2] = (byte)(value >> 16);
                dest[3] = (byte)(value >> 24);
                dest[4] = (byte)(value >> 32);
                dest[5] = (byte)(value >> 40);
                dest[6] = (byte)(value >> 48);
                dest[7] = (byte)(value >> 56);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void Int64ToBytesSafepos(long value, byte[] dest, int pos)
        {
            if (CanDirectCopyLE)
            {
                fixed (byte* p = &dest[pos])
                    *(long*)p = value;
            }
            else
            {
                dest[pos] = (byte)value;
                dest[pos + 1] = (byte)(value >> 8);
                dest[pos + 2] = (byte)(value >> 16);
                dest[pos + 3] = (byte)(value >> 24);
                dest[pos + 4] = (byte)(value >> 32);
                dest[pos + 5] = (byte)(value >> 40);
                dest[pos + 6] = (byte)(value >> 48);
                dest[pos + 7] = (byte)(value >> 56);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Int64ToBytes(Stream ms, long value)
        {
            ms.WriteByte((byte)value);
            ms.WriteByte((byte)(value >> 8));
            ms.WriteByte((byte)(value >> 16));
            ms.WriteByte((byte)(value >> 24));
            ms.WriteByte((byte)(value >> 32));
            ms.WriteByte((byte)(value >> 40));
            ms.WriteByte((byte)(value >> 48));
            ms.WriteByte((byte)(value >> 56));
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] Int64ToBytesBig(long value)
        {
            byte[] bytes = new byte[8];
            bytes[0] = (byte)((value >> 56));
            bytes[1] = (byte)((value >> 48));
            bytes[2] = (byte)((value >> 40));
            bytes[3] = (byte)((value >> 32));
            bytes[4] = (byte)((value >> 24));
            bytes[5] = (byte)((value >> 16));
            bytes[6] = (byte)((value >> 8));
            bytes[7] = (byte)(value);
            return bytes;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Int64ToBytesBig(long value, byte[] dest, int pos)
        {
            dest[pos] = (byte)(value >> 56);
            dest[pos + 1] = (byte)(value >> 48);
            dest[pos + 2] = (byte)(value >> 40);
            dest[pos + 3] = (byte)(value >> 32);
            dest[pos + 4] = (byte)(value >> 24);
            dest[pos + 5] = (byte)(value >> 16);
            dest[pos + 6] = (byte)(value >> 8);
            dest[pos + 7] = (byte)value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Int64ToBytesBig(long value, byte* dest)
        {
            *dest = (byte)(value >> 56);
            dest[1] = (byte)(value >> 48);
            dest[2] = (byte)(value >> 40);
            dest[3] = (byte)(value >> 32);
            dest[4] = (byte)(value >> 24);
            dest[5] = (byte)(value >> 16);
            dest[6] = (byte)(value >> 8);
            dest[7] = (byte)value;
        }

        /// <summary>
        /// Convert a 64-bit unsigned integer to a byte array in little endian
        /// format
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <returns>An 8 byte little endian array</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static byte[] UInt64ToBytes(ulong value)
        {
            return Int64ToBytes((long)value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] UInt64ToBytesBig(ulong value)
        {
            return Int64ToBytesBig((long)value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void UInt64ToBytes(ulong value, byte[] dest, int pos)
        {
            Int64ToBytes((long)value, dest, pos);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void UInt64ToBytesSafepos(ulong value, byte[] dest, int pos)
        {
            Int64ToBytesSafepos((long)value, dest, pos);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void UInt64ToBytesBig(ulong value, byte[] dest, int pos)
        {
            Int64ToBytesBig((long) value, dest, pos);
        }

        /// <summary>
        /// Convert a floating point value to four bytes in little endian
        /// ordering
        /// </summary>
        /// <param name="value">A floating point value</param>
        /// <returns>A four byte array containing the value in little endian
        /// ordering</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static byte[] FloatToBytes(float value)
        {
            return IntToBytes(*(int*)&value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void FloatToBytes(Stream ms, float value)
        {
            IntToBytes(ms, *(int*)&value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void FloatToBytes(float value, byte[] dest, int pos)
        {
            IntToBytes(*(int*)&value, dest, pos);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void FloatToBytes(float value, byte* dest)
        {
            IntToBytes(*(int*)&value, dest);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void FloatToBytesSafepos(float value, byte[] dest, int pos)
        {
            IntToBytesSafepos(*(int*)&value, dest, pos);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static byte[] DoubleToBytes(double value)
        {
            return Int64ToBytes(*(long*)&value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void DoubleToBytes(Stream ms, float value)
        {
            Int64ToBytes(ms, *(long*)&value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static byte[] DoubleToBytesBig(double value)
        {
            return Int64ToBytesBig(*(long*)&value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void DoubleToBytes(double value, byte[] dest, int pos)
        {
            Int64ToBytes(*(long*)&value, dest, pos);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void DoubleToBytes(double value, byte* dest)
        {
            Int64ToBytes(*(long*)&value, dest);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void DoubleToBytesSafepos(double value, byte[] dest, int pos)
        {
            Int64ToBytesSafepos(*(long*)&value, dest, pos);
        }

        #endregion ToBytes

        #region Strings

        /// <summary>
        /// Converts an unsigned integer to a hexadecimal string
        /// </summary>
        /// <param name="i">An unsigned integer to convert to a string</param>
        /// <returns>A hexadecimal string 10 characters long</returns>
        /// <example>0x7fffffff</example>
        public static string UIntToHexString(uint i)
        {
            return string.Format("{0:x8}", i);
        }

        /// <summary>
        /// Convert a variable length UTF8 byte array to a string
        /// </summary>
        /// <param name="bytes">The UTF8 encoded byte array to convert</param>
        /// <returns>The decoded string</returns>
        public static string BytesToString(byte[] bytes)
        {
            if(bytes.Length <= 0)
                return string.Empty;
            if (bytes[bytes.Length - 1] == 0x00)
                return Encoding.UTF8.GetString(bytes, 0, bytes.Length - 1);
            else
                return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
        }

        public static string BytesToString(byte[] bytes, int index, int count)
        {
            if (bytes.Length <= index + count)
                return string.Empty;
            if (bytes[index + count - 1] == 0x00)
                return Encoding.UTF8.GetString(bytes, index, count - 1);
            else
                return Encoding.UTF8.GetString(bytes, index, count);
        }

        private static string GetString(byte[] bytes, int index, int count)
        {
            return Encoding.UTF8.GetString(bytes, index, count);
        }

        /// <summary>
        /// Converts a byte array to a string containing hexadecimal characters
        /// </summary>
        /// <param name="bytes">The byte array to convert to a string</param>
        /// <param name="fieldName">The name of the field to prepend to each
        /// line of the string</param>
        /// <returns>A string containing hexadecimal characters on multiple
        /// lines. Each line is prepended with the field name</returns>
        public static string BytesToHexString(byte[] bytes, string fieldName)
        {
            return BytesToHexString(bytes, bytes.Length, fieldName);
        }

        /// <summary>
        /// Converts a byte array to a string containing hexadecimal characters
        /// </summary>
        /// <param name="bytes">The byte array to convert to a string</param>
        /// <param name="length">Number of bytes in the array to parse</param>
        /// <param name="fieldName">A string to prepend to each line of the hex
        /// dump</param>
        /// <returns>A string containing hexadecimal characters on multiple
        /// lines. Each line is prepended with the field name</returns>
        public static string BytesToHexString(byte[] bytes, int length, string fieldName)
        {
            StringBuilder output = new StringBuilder();

            for (int i = 0; i < length; i += 16)
            {
                if (i != 0)
                    output.Append('\n');

                if (!String.IsNullOrEmpty(fieldName))
                {
                    output.Append(fieldName);
                    output.Append(": ");
                }

                for (int j = 0, k = i; j < 16; ++j,++k)
                {
                    if(k >= length)
                        break;

                    if (j != 0)
                        output.Append(' ');

                    output.Append(String.Format("{0:X2}", bytes[k]));
                }
            }

            return output.ToString();
        }

        /// <summary>
        /// Convert a string to a UTF8 encoded byte array
        /// </summary>
        /// <param name="str">The string to convert</param>
        /// <returns>A null-terminated UTF8 byte array</returns>
        public static byte[] StringToBytes(string str)
        {
            if (string.IsNullOrEmpty(str))
                return Array.Empty<byte>();

            int nbytes = osUTF8GetBytesCount(str, out int sourcelen);
            if (nbytes == 0)
                return Array.Empty<byte>(); ;
            byte[] dstarray = new byte[nbytes + 1];
            osUTF8Getbytes(str, sourcelen, dstarray, nbytes);
            dstarray[nbytes] = 0;
            return dstarray;
        }

        public static byte[] StringToBytes(string str, int maxlen)
        {
            if (string.IsNullOrEmpty(str))
                return Array.Empty<byte>();

            int nbytes = osUTF8GetBytesCount(str, maxlen -1, out int sourcelen);
            if (nbytes == 0)
                return Array.Empty<byte>();
            byte[] dstarray = new byte[nbytes + 1];
            osUTF8Getbytes(str, sourcelen, dstarray, nbytes);
            dstarray[nbytes] = 0;
            return dstarray;
        }

        public static byte[] StringToBytesNoTerm(string str)
        {
            if (string.IsNullOrEmpty(str))
                return Array.Empty<byte>();

            int nbytes = osUTF8GetBytesCount(str, out int sourcelen);
            if (nbytes == 0)
                return Array.Empty<byte>();
            byte[] dstarray = new byte[nbytes];
            osUTF8Getbytes(str, sourcelen, dstarray, nbytes);
            return dstarray;
        }

        public static byte[] StringToBytesNoTerm(string str, int maxlen)
        {
            if (string.IsNullOrEmpty(str))
                return Array.Empty<byte>();

            int nbytes = osUTF8GetBytesCount(str, maxlen, out int sourcelen);
            if (nbytes == 0)
                return Array.Empty<byte>();
            byte[] dstarray = new byte[nbytes];
            osUTF8Getbytes(str, sourcelen, dstarray, nbytes);
            return dstarray;
        }

        public static unsafe int osUTF8Getbytes(string source, int srclenght, byte[] destiny, int maxdstlen)
        {
            int ret = 0;
            char c;
            fixed(char* srcarray = source)
            {
                fixed(byte* dstarray = destiny)
                {
                    char* src = srcarray;
                    char* srcend = src + srclenght;
                    char* srcend1 = srcend - 1;

                    byte* dst = dstarray;
                    byte* dstend = dst + maxdstlen;
                    byte* dstend2 = dstend - 2;
                    byte* dstend3 = dstend - 3;
                    byte* dstend4 = dstend - 4;

                    while (src < srcend && dst < dstend)
                    {
                        c = *src++;
                        if(c == 0)
                            break;

                        if (c <= 0x7f)
                        {
                            *dst++ = (byte)c;
                            continue;
                        }

                        if (c < 0x800)
                        {
                            if (dst > dstend2)
                                break;

                            *dst++ = (byte)(0xC0 | (c >> 6));
                            *dst++ = (byte)(0x80 | (c & 0x3F));
                            continue;
                        }

                        if (c >= 0xD800 && c < 0xE000)
                        {
                            if (c >= 0xDC00)
                                continue; // ignore invalid
                            if (src > srcend1 || dst > dstend4)
                                break;

                            int a = c;

                            c = *src++;

                            if (c < 0xDC00 || c > 0xDFFF)
                                continue; // ignore invalid

                            a = (a << 10) + c - 0x35fdc00;

                            *dst++ = (byte)(0xF0 | (a >> 18));
                            *dst++ = (byte)(0x80 | ((a >> 12) & 0x3f));
                            *dst++ = (byte)(0x80 | ((a >> 6) & 0x3f));
                            *dst++ = (byte)(0x80 | (a & 0x3f));
                            continue;
                        }
                        if (dst > dstend3)
                            break;

                        *dst++ = (byte)(0xE0 | (c >> 12));
                        *dst++ = (byte)(0x80 | ((c >> 6) & 0x3f));
                        *dst++ = (byte)(0x80 | (c & 0x3f));
                    }
                    ret = (int)(dst - dstarray);
                }
            }
            return ret;
        }

        public static unsafe bool osUTF8TryGetbytes(string srcstr, ref int srcstart, byte[] dstarray, ref int pos)
        {
            if (string.IsNullOrEmpty(srcstr))
                return true;

            bool ret = false;
            int free = dstarray.Length - pos;
            fixed (char* srcbase = srcstr)
            {
                fixed (byte* dstbase = dstarray)
                {
                    char* src = srcbase + srcstart;
                    char* srcend = srcbase + srcstr.Length;
                    char* srcend1 = srcend - 1;
                    byte* dst = dstbase + pos;

                    char c;
                    while (src < srcend && free > 0)
                    {
                        c = *src++;
                        if(c == 0)
                            break;

                        if (c <= 0x7f)
                        {
                            *dst++ = (byte)c;
                            --free;
                            continue;
                        }

                        if (c < 0x800)
                        {
                            free -= 2;
                            if (free <= 0)
                            {
                                --src;
                                break;
                            }
                            *dst++ = (byte)(0xC0 | (c >> 6));
                            *dst++ = (byte)(0x80 | (c & 0x3F));
                            continue;
                        }

                        if (c >= 0xD800 && c < 0xE000)
                        {
                            if (c >= 0xDC00)
                                continue; // ignore invalid
                            if (src > srcend1)
                                break;

                            int a = c;
                            c = *src++;

                            if (c < 0xDC00 || c > 0xDFFF)
                                continue; // ignore invalid

                            free -= 4;
                            if (free <= 0)
                            {
                                src -= 2;
                                break;
                            }

                            a = (a << 10) + c - 0x35fdc00;

                            *dst++ = (byte)(0xF0 | (a >> 18));
                            *dst++ = (byte)(0x80 | ((a >> 12) & 0x3f));
                            *dst++ = (byte)(0x80 | ((a >> 6) & 0x3f));
                            *dst++ = (byte)(0x80 | (a & 0x3f));
                            continue;
                        }

                        free -= 3;
                        if (free <= 0)
                        {
                            --src;
                            break;
                        }

                        *dst++ = (byte)(0xE0 | (c >> 12));
                        *dst++ = (byte)(0x80 | ((c >> 6) & 0x3f));
                        *dst++ = (byte)(0x80 | (c & 0x3f));
                    }

                    pos = (int)(dst - dstbase);
                    srcstart = (int)(src - srcbase);
                    if (src == srcend)
                        ret = true;
                }
            }
            return ret;
        }

        public static int osUTF8GetBytesCount(string str, out int maxsource)
        {
            maxsource = 0;
            char c;
            char lastc = (char)0;
            int nbytes = 0;
            int i = 0;
            while(i < str.Length)
            {
                c = str[i++];

                if (c <= 0x7f)
                {
                    lastc = c;
                    ++nbytes;
                    continue;
                }

                if (c < 0x800)
                {
                    lastc = c;
                    nbytes += 2;
                    continue;
                }

                if (c >= 0xD800 && c < 0xE000)
                {
                    if (c >= 0xDC00)
                        continue;
                    if (i == str.Length)
                        break;
                    c = str[i++];
                    if (c < 0xDC00 || c > 0xDFFF)
                        continue;
                    lastc = c;
                    nbytes += 4;
                    continue;
                }
                lastc = c;
                nbytes += 3;
            }

            if (i > 0 && lastc == 0)
            {
                --nbytes;
                --i;
            }
            maxsource = i;
            return nbytes;
        }

        public static int osUTF8GetBytesCount(string str, int maxnbytes, out int maxsourcelen)
        {
            maxsourcelen = 0;
            int max2 = maxnbytes - 2;
            int max3 = maxnbytes - 3;
            int max4 = maxnbytes - 4;

            char c = ' ';
            int nbytes = 0;
            int i = 0;
            while(i < str.Length && nbytes < maxnbytes)
            {
                c = str[i++];
                if(c == 0)
                    break;

                if (c <= 0x7f)
                {
                    ++nbytes;
                    continue;
                }

                if (c < 0x800)
                {
                    if (nbytes > max2)
                        break;
                    nbytes += 2;
                    continue;
                }

                if (c >= 0xD800 && c < 0xE000)
                {
                    if (c >= 0xDC00)
                        continue;
                    if (i == str.Length)
                        break;
                    c = str[i++];
                    if (c < 0xDC00 || c > 0xDFFF)
                        continue;
                    if (nbytes > max4)
                        break;
                    nbytes += 4;
                    continue;
                }
                if (nbytes > max3)
                    break;
                nbytes += 3;
            }

            maxsourcelen = i;
            return nbytes;
        }

        /// <summary>
        /// Returns true is c is a hexadecimal digit (A-F, a-f, 0-9)
        /// </summary>
        /// <param name="c">Character to test</param>
        /// <returns>true if hex digit, false if not</returns>
        public static bool IsHexDigit(Char c)
        {

            const int numA = 65;
            const int numa = 97;
            const int num0 = 48;

            if (c < num0)
                return false;
            if (c < (num0 + 10))
                return true;

            if (c > numa + 5)
                return false;
            if (c >= numa)
                return true;

            if (c >= numA && c < numA + 6)
                return true;

            return false;
        }

        public static int HexNibbleWithChk(Char c)
        {
            const int numA = 65;
            const int numa = 97;
            const int num0 = 48;

            if (c < num0)
                return -1;
            if (c < (num0 + 10))
                return c - num0;

            if (c > numa + 5)
                return -1;
            if (c >= numa)
                return c - numa + 10;

            if (c >= numA && c < numA + 6)
                return c - numA + 10;

            return -1;
        }

        public static int HexNibbleWithChk(byte c)
        {
            const int numA = 65;
            const int numa = 97;
            const int num0 = 48;

            if (c < num0)
                return -1;
            if (c < (num0 + 10))
                return c - num0;

            if (c > numa + 5)
                return -1;
            if (c >= numa)
                return c - numa + 10;

            if (c >= numA && c < numA + 6)
                return c - numA + 10;

            return -1;
        }

        public static int HexNibble(Char c)
        {

            const char numA = 'A';
            const char numF = 'F';
            const char numa = 'a';
            const char numf = 'f';
            const char num0 = '0';
            const char num9 = '9';

            if (c < num0)
                throw new Exception("invalid hex char");
            if (c <= num9)
                return c - num0;

            if (c > numf)
                throw new Exception("invalid hex char");
            if (c >= numa)
                return c - numa + 10;

            if (c >= numA && c <= numF)
                return c - numA + 10;

            throw new Exception("invalid hex char");
        }

        public static int HexNibble(byte c)
        {
            const byte numA = (byte)'A';
            const byte numF = (byte)'F';
            const byte numa = (byte)'a';
            const byte numf = (byte)'f';
            const byte num0 = (byte)'0';
            const byte num9 = (byte)'9';

            if (c < num0)
                throw new Exception("invalid hex char");
            if (c <= num9)
                return c - num0;

            if (c > numf)
                throw new Exception("invalid hex char");
            if (c >= numa)
                return c - numa + 10;

            if (c >= numA && c <= numF)
                return c - numA + 10;

            throw new Exception("invalid hex char");
        }

        public static bool TryHexToInt(byte[] data, int start, int len, out int res)
        {
            res = 0;
            int n;
            if(len > 8)
                return false;
            for (int i = start; i < start + len; ++i)
            {
                if ((n = HexNibbleWithChk(data[i])) < 0)
                    return false;
                res *= 16;
                res |= n;
            }
            return true;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static bool TryHexToByte(byte[] data, int pos, out byte res)
        {
            int a,b;
            if ((a = HexNibbleWithChk(data[pos])) < 0)
            {
                res = 0;
                return false;
            }
            if ((b = HexNibbleWithChk(data[pos + 1])) < 0)
            {
                res = 0;
                return false;
            }
            res = (byte)(a << 4 | b);
            return true;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static byte HexToByte(byte[] data, int pos)
        {
            return (byte)(HexNibble(data[pos]) << 4 | HexNibble(data[pos + 1]));
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public unsafe static byte HexToByte(byte* data, int pos)
        {
            return (byte)(HexNibble(data[pos]) << 4 | HexNibble(data[pos + 1]));
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public unsafe static byte HexToByte(char* hex, int pos)
        {
            return (byte)(HexNibble(hex[pos]) << 4 | HexNibble(hex[pos + 1]));
        }

        /// <summary>
        /// Converts a string containing hexadecimal characters to a byte array
        /// </summary>
        /// <param name="hexString">String containing hexadecimal characters</param>
        /// <param name="handleDirty">If true, gracefully handles null, empty and
        /// uneven strings as well as stripping unconvertable characters</param>
        /// <returns>The converted byte array</returns>
        public unsafe static byte[] HexStringToBytes(string hexString, bool handleDirty)
        {
            if (String.IsNullOrEmpty(hexString))
                return EmptyBytes;
            if (handleDirty)
            {
                StringBuilder stripped = new StringBuilder(hexString.Length);
                // remove all non A-F, 0-9, characters
                foreach (char c in hexString)
                {
                    if (IsHexDigit(c))
                        stripped.Append(c);
                }
                hexString = stripped.ToString();

                // if odd number of characters, discard last character
                if (hexString.Length % 2 != 0)
                {
                    hexString = hexString.Substring(0, hexString.Length - 1);
                }
            }

            int byteLength = hexString.Length / 2;
            byte[] bytes = new byte[byteLength];
            int j = 0;

            fixed(char* c = hexString)
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    bytes[i] = HexToByte(c, j);
                    j += 2;
                }
            }

            return bytes;
        }

        #endregion Strings

        #region Packed Values

        /// <summary>
        /// Convert a float value to a byte given a minimum and maximum range
        /// </summary>
        /// <param name="val">Value to convert to a byte</param>
        /// <param name="lower">Minimum value range</param>
        /// <param name="upper">Maximum value range</param>
        /// <returns>A single byte representing the original float value</returns>
        public static byte FloatToByte(float val, float lower, float upper)
        {
            val -= lower;
            if (val <= 0)
                return 0;

            val /= (upper - lower);
            if (val >= 1.0f)
                return 255;

            return (byte)(255 * val);
        }

        /// <summary>
        /// Convert a byte to a float value given a minimum and maximum range
        /// </summary>
        /// <param name="bytes">Byte array to get the byte from</param>
        /// <param name="pos">Position in the byte array the desired byte is at</param>
        /// <param name="lower">Minimum value range</param>
        /// <param name="upper">Maximum value range</param>
        /// <returns>A float value inclusively between lower and upper</returns>
        public static float ByteToFloat(byte[] bytes, int pos, float lower, float upper)
        {
            if (bytes.Length <= pos) return 0;
            return ByteToFloat(bytes[pos], lower, upper);
        }

        /// <summary>
        /// Convert a byte to a float value given a minimum and maximum range
        /// </summary>
        /// <param name="val">Byte to convert to a float value</param>
        /// <param name="lower">Minimum value range</param>
        /// <param name="upper">Maximum value range</param>
        /// <returns>A float value inclusively between lower and upper</returns>
        public static float ByteToFloat(byte val, float lower, float upper)
        {
            float delta = (upper - lower) * 0.00392157f;
            float fval = val * delta;
            fval += lower;

            // Test for values very close to zero
            if (Math.Abs(fval) < delta)
                fval = 0.0f;

            return fval;
        }

        public static float BytesUInt16ToFloat(byte[] bytes, int pos, float lower, float upper)
        {
            ushort val = BytesToUInt16(bytes, pos);
            return UInt16ToFloat(val, lower, upper);
        }

        public static float UInt16ToFloat(ushort val, float lower, float upper)
        {
            const float ONE_OVER_U16_MAX = 1.0f / UInt16.MaxValue;

            float delta = (upper - lower) * ONE_OVER_U16_MAX;
            float fval = val * delta;
            fval += lower;

            if (Math.Abs(fval) < delta)
                fval = 0.0f;

            return fval;
        }

        public static ushort FloatToUInt16(float value, float lower, float upper)
        {
            value -= lower;
            if (value <= 0)
                return 0;

            value /= upper - lower;
            if (value >= 1.0f)
                return 0xffff;

            return (ushort)(value * 0xffff);
        }

        public static ushort FloatToUInt16(float value, float range)
        {
            value += range;
            if (value <= 0)
                return 0;

            value /= range;
            if (value >= 2.0f)
                return 0xffff;

            return (ushort)(value * 32767.5);
        }

        public static void FloatToUInt16Bytes(float value, float range, byte[] dest, int pos)
        {
            value += range;
            if (value <= 0)
            {
                dest[pos] = 0;
                dest[pos + 1] = 0;
                return;
            }

            value /= range;
            if (value >= 2.0f)
            {
                dest[pos] = 0xff;
                dest[pos + 1] = 0xff;
                return;
            }

            ushort s = (ushort)(value * 32767.5);
            dest[pos] = (byte)s;
            dest[pos + 1] = (byte)(s >> 8);
        }

        public static byte[] FloatToUInt16Bytes(float value, float range)
        {
            byte[] dest = new byte[2];

            value += range;
            if (value <= 0)
            {
                dest[0] = 0;
                dest[1] = 0;
                return dest;
            }

            value /= range;
            if (value >= 2.0f)
            {
                dest[0] = 0xff;
                dest[1] = 0xff;
                return dest;
            }

            ushort s = (ushort)(value * 32767.5);
            dest[0] = (byte)s;
            dest[1] = (byte)(s >> 8);
            return dest;
        }

        #endregion Packed Values

        #region TryParse

        /// <summary>
        /// Attempts to parse a floating point value from a string, using an
        /// EN-US number format
        /// </summary>
        /// <param name="s">String to parse</param>
        /// <param name="result">Resulting floating point number</param>
        /// <returns>True if the parse was successful, otherwise false</returns>
        public static bool TryParseSingle(string s, out float result)
        {
            return Single.TryParse(s, System.Globalization.NumberStyles.Float, EnUsCulture.NumberFormat, out result);
        }

        /// <summary>
        /// Attempts to parse a floating point value from a string, using an
        /// EN-US number format
        /// </summary>
        /// <param name="s">String to parse</param>
        /// <param name="result">Resulting floating point number</param>
        /// <returns>True if the parse was successful, otherwise false</returns>
        public static bool TryParseDouble(string s, out double result)
        {
            // NOTE: Double.TryParse can't parse Double.[Min/Max]Value.ToString(), see:
            // http://blogs.msdn.com/bclteam/archive/2006/05/24/598169.aspx
            return Double.TryParse(s, System.Globalization.NumberStyles.Float, EnUsCulture.NumberFormat, out result);
        }

        /// <summary>
        /// Tries to parse an unsigned 32-bit integer from a hexadecimal string
        /// </summary>
        /// <param name="s">String to parse</param>
        /// <param name="result">Resulting integer</param>
        /// <returns>True if the parse was successful, otherwise false</returns>
        public static bool TryParseHex(string s, out uint result)
        {
            return UInt32.TryParse(s, System.Globalization.NumberStyles.HexNumber, EnUsCulture.NumberFormat, out result);
        }

        #endregion TryParse

        #region Enum String Conversion

        /// <summary>
        /// Returns text specified in EnumInfo attribute of the enumerator
        /// To add the text use [EnumInfo(Text = "Some nice text here")] before declaration
        /// of enum values
        /// </summary>
        /// <param name="value">Enum value</param>
        /// <returns>Text representation of the enum</returns>
        public static string EnumToText(Enum value)
        {
            // Get the type
            Type type = value.GetType();

            // Get fieldinfo for this type
            FieldInfo fieldInfo = type.GetField(value.ToString());

            // Find extended attributes, if any
            EnumInfoAttribute[] attribs = (EnumInfoAttribute[])fieldInfo.GetCustomAttributes(typeof(EnumInfoAttribute), false);

            return attribs.Length > 0 ? attribs[0].Text : value.ToString();
        }

        /// <summary>
        /// Takes an AssetType and returns the string representation
        /// </summary>
        /// <param name="type">The source <seealso cref="AssetType"/></param>
        /// <returns>The string version of the AssetType</returns>
        public static string AssetTypeToString(AssetType type)
        {
            return _AssetTypeNames[(int)type];
        }

        /// <summary>
        /// Translate a string name of an AssetType into the proper Type
        /// </summary>
        /// <param name="type">A string containing the AssetType name</param>
        /// <returns>The AssetType which matches the string name, or AssetType.Unknown if no match was found</returns>
        public static AssetType StringToAssetType(string type)
        {
            for (int i = 0; i < _AssetTypeNames.Length; i++)
            {
                if (_AssetTypeNames[i] == type)
                    return (AssetType)i;
            }

            return AssetType.Unknown;
        }

        /// <summary>
        /// Takes a FolderType and returns the string representation
        /// </summary>
        /// <param name="type">The source <seealso cref="FolderType"/></param>
        /// <returns>The string version of the FolderType</returns>
        public static string FolderTypeToString(FolderType type)
        {
            return _FolderTypeNames[(int)type];
        }

        /// <summary>
        /// Translate a string name of an FolderType into the proper Type
        /// </summary>
        /// <param name="type">A string containing the FolderType name</param>
        /// <returns>The FolderType which matches the string name, or FolderType. None if no match was found</returns>
        public static FolderType StringToFolderType(string type)
        {
            for (int i = 0; i < _FolderTypeNames.Length; i++)
            {
                if (_FolderTypeNames[i] == type)
                    return (FolderType)i;
            }

            return FolderType.None;
        }

        /// <summary>
        /// Convert an InventoryType to a string
        /// </summary>
        /// <param name="type">The <seealso cref="T:InventoryType"/> to convert</param>
        /// <returns>A string representation of the source</returns>
        public static string InventoryTypeToString(InventoryType type)
        {
            return _InventoryTypeNames[(int)type];
        }

        /// <summary>
        /// Convert a string into a valid InventoryType
        /// </summary>
        /// <param name="type">A string representation of the InventoryType to convert</param>
        /// <returns>A InventoryType object which matched the type</returns>
        public static InventoryType StringToInventoryType(string type)
        {
            for (int i = 0; i < _InventoryTypeNames.Length; i++)
            {
                if (_InventoryTypeNames[i] == type)
                    return (InventoryType)i;
            }

            return InventoryType.Unknown;
        }

        /// <summary>
        /// Convert a SaleType to a string
        /// </summary>
        /// <param name="type">The <seealso cref="T:SaleType"/> to convert</param>
        /// <returns>A string representation of the source</returns>
        public static string SaleTypeToString(SaleType type)
        {
            return _SaleTypeNames[(int)type];
        }

        /// <summary>
        /// Convert a string into a valid SaleType
        /// </summary>
        /// <param name="value">A string representation of the SaleType to convert</param>
        /// <returns>A SaleType object which matched the type</returns>
        public static SaleType StringToSaleType(string value)
        {
            for (int i = 0; i < _SaleTypeNames.Length; i++)
            {
                if (value == _SaleTypeNames[i])
                    return (SaleType)i;
            }

            return SaleType.Not;
        }

        /// <summary>
        /// Converts a string used in LLSD to AttachmentPoint type
        /// </summary>
        /// <param name="value">String representation of AttachmentPoint to convert</param>
        /// <returns>AttachmentPoint enum</returns>
        public static AttachmentPoint StringToAttachmentPoint(string value)
        {
            for (int i = 0; i < _AttachmentPointNames.Length; i++)
            {
                if (value == _AttachmentPointNames[i])
                    return (AttachmentPoint)i;
            }

            return AttachmentPoint.Default;
        }

        #endregion Enum String Conversion

        #region Miscellaneous

        /// <summary>
        /// Copy a byte array
        /// </summary>
        /// <param name="bytes">Byte array to copy</param>
        /// <returns>A copy of the given byte array</returns>
        public static byte[] CopyBytes(byte[] bytes)
        {
            if (bytes == null)
                return null;

            byte[] newBytes = new byte[bytes.Length];
            Buffer.BlockCopy(bytes, 0, newBytes, 0, bytes.Length);
            return newBytes;
        }

        /// <summary>
        /// Packs to 32-bit unsigned integers in to a 64-bit unsigned integer
        /// </summary>
        /// <param name="a">The left-hand (or X) value</param>
        /// <param name="b">The right-hand (or Y) value</param>
        /// <returns>A 64-bit integer containing the two 32-bit input values</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong UIntsToLong(uint a, uint b)
        {
            return ((ulong)a << 32) | (ulong)b;
        }

        /// <summary>
        /// Unpacks two 32-bit unsigned integers from a 64-bit unsigned integer
        /// </summary>
        /// <param name="a">The 64-bit input integer</param>
        /// <param name="b">The left-hand (or X) output value</param>
        /// <param name="c">The right-hand (or Y) output value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LongToUInts(ulong a, out uint b, out uint c)
        {
            b = (uint)(a >> 32);
            c = (uint)(a & 0x00000000FFFFFFFF);
        }

        /// <summary>
        /// Convert an IP address object to an unsigned 32-bit integer
        /// </summary>
        /// <param name="address">IP address to convert</param>
        /// <returns>32-bit unsigned integer holding the IP address bits</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint IPToUInt(System.Net.IPAddress address)
        {
            byte[] bytes = address.GetAddressBytes();
            return (uint)((bytes[3] << 24) + (bytes[2] << 16) + (bytes[1] << 8) + bytes[0]);
        }

        /// <summary>
        /// Gets a unix timestamp for the current time
        /// </summary>
        /// <returns>An unsigned integer representing a unix timestamp for now</returns>
        public static uint GetUnixTime()
        {
            return (uint)(DateTime.UtcNow - Epoch).TotalSeconds;
        }

        /// <summary>
        /// Convert a UNIX timestamp to a native DateTime object
        /// </summary>
        /// <param name="timestamp">An unsigned integer representing a UNIX
        /// timestamp</param>
        /// <returns>A DateTime object containing the same time specified in
        /// the given timestamp</returns>
        public static DateTime UnixTimeToDateTime(uint timestamp)
        {
            DateTime dateTime = Epoch;

            // Add the number of seconds in our UNIX timestamp
            dateTime = dateTime.AddSeconds(timestamp);

            return dateTime;
        }

        /// <summary>
        /// Convert a UNIX timestamp to a native DateTime object
        /// </summary>
        /// <param name="timestamp">A signed integer representing a UNIX
        /// timestamp</param>
        /// <returns>A DateTime object containing the same time specified in
        /// the given timestamp</returns>
        public static DateTime UnixTimeToDateTime(int timestamp)
        {
            return UnixTimeToDateTime((uint)timestamp);
        }

        /// <summary>
        /// Convert a native DateTime object to a UNIX timestamp
        /// </summary>
        /// <param name="time">A DateTime object you want to convert to a 
        /// timestamp</param>
        /// <returns>An unsigned integer representing a UNIX timestamp</returns>
        public static uint DateTimeToUnixTime(DateTime time)
        {
            TimeSpan ts = (time - new DateTime(1970, 1, 1, 0, 0, 0));
            return (uint)ts.TotalSeconds;
        }

        /// <summary>
        /// Swap two values
        /// </summary>
        /// <typeparam name="T">Type of the values to swap</typeparam>
        /// <param name="lhs">First value</param>
        /// <param name="rhs">Second value</param>
        public static void Swap<T>(ref T lhs, ref T rhs)
        {
            T temp = lhs;
            lhs = rhs;
            rhs = temp;
        }

        /// <summary>
        /// Try to parse an enumeration value from a string
        /// </summary>
        /// <typeparam name="T">Enumeration type</typeparam>
        /// <param name="strType">String value to parse</param>
        /// <param name="result">Enumeration value on success</param>
        /// <returns>True if the parsing succeeded, otherwise false</returns>
        public static bool EnumTryParse<T>(string strType, out T result)
        {
            Type t = typeof(T);

            if (Enum.IsDefined(t, strType))
            {
                result = (T)Enum.Parse(t, strType, true);
                return true;
            }
            else
            {
                foreach (string value in Enum.GetNames(typeof(T)))
                {
                    if (value.Equals(strType, StringComparison.OrdinalIgnoreCase))
                    {
                        result = (T)Enum.Parse(typeof(T), value);
                        return true;
                    }
                }
                result = default(T);
                return false;
            }
        }

        /// <summary>
        /// Swaps the high and low nibbles in a byte. Converts aaaabbbb to bbbbaaaa
        /// </summary>
        /// <param name="value">Byte to swap the words in</param>
        /// <returns>Byte value with the words swapped</returns>
        public static byte SwapNibbles(byte value)
        {
            return (byte)(((value & 0xF0) >> 4) | ((value & 0x0F) << 4));
        }

        /// <summary>
        /// Attempts to convert a string representation of a hostname or IP
        /// address to a <seealso cref="System.Net.IPAddress"/>
        /// </summary>
        /// <param name="hostname">Hostname to convert to an IPAddress</param>
        /// <returns>Converted IP address object, or null if the conversion
        /// failed</returns>
        public static IPAddress HostnameToIPv4(string hostname)
        {
            // Is it already a valid IP?
            IPAddress ip;
            if (IPAddress.TryParse(hostname, out ip))
                return ip;

            IPAddress[] hosts = Dns.GetHostEntry(hostname).AddressList;

            for (int i = 0; i < hosts.Length; i++)
            {
                IPAddress host = hosts[i];

                if (host.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    return host;
            }

            return null;
        }

        private const byte ASCIIzero = (byte)'0';
        private const byte ASCIIminus = (byte)'-';

        private static unsafe int UintToStrBytes_reversed(uint value, byte* dst)
        {
            int n = 0;
            do
            {
                byte a = ASCIIzero;
                a += (byte)(value % 10);
                dst[n] = a;
                ++n;
                value /= 10;
            }
            while(value > 0);
            return n;
        }

        private static unsafe int ULongToStrBytes_reversed(ulong value, byte* dst)
        {
            int n = 0;
            do
            {
                byte a = ASCIIzero;
                a += (byte)(value % 10);
                dst[n] = a;
                ++n;
                value /= 10;
            }
            while (value > 0);
            return n;
        }

        public static unsafe int UIntToByteString(uint value, byte* dst)
        {
            if (value == 0)
            {
                *dst = ASCIIzero;
                return 1;
            }

            byte* tmp = stackalloc byte[16];
            int n = UintToStrBytes_reversed(value, tmp);
            for (int i = 0, j = n - 1; i < n; ++i, --j)
                dst[i] = tmp[j];
            return n;
        }

        public static unsafe int ULongToByteString(ulong value, byte* dst)
        {
            if (value == 0)
            {
                *dst = ASCIIzero;
                return 1;
            }

            byte* tmp = stackalloc byte[32];
            int n = ULongToStrBytes_reversed(value, tmp);
            for (int i = 0, j = n - 1; i < n; ++i, --j)
                dst[i] = tmp[j];
            return n;
        }

        public static unsafe int UIntToByteString(uint value, Stream st)
        {
            if (value == 0)
            {
                st.WriteByte(ASCIIzero);
                return 1;
            }

            byte* tmp = stackalloc byte[32];
            int n = UintToStrBytes_reversed(value, tmp);
            for (int j = n - 1; j <= 0; --j)
                st.WriteByte(tmp[j]);
            return n;
        }

        public static unsafe int ULongToByteString(ulong value, Stream st)
        {
            if (value == 0)
            {
                st.WriteByte(ASCIIzero);
                return 1;
            }

            byte* tmp = stackalloc byte[32];
            int n = ULongToStrBytes_reversed(value, tmp);
            for (int j = n - 1; j <= 0; --j)
                st.WriteByte(tmp[j]);
            return n;
        }

        public static unsafe int UIntToByteString(uint value, byte[] dst, int pos)
        {
            if (value == 0)
            {
                dst[pos] = ASCIIzero;
                return 1;
            }

            fixed (byte* d = &dst[pos])
            {
                return UIntToByteString(value, d);
            }
        }

        public static unsafe int ULongToByteString(ulong value, byte[] dst, int pos)
        {
            if (value == 0)
            {
                dst[pos] = ASCIIzero;
                return 1;
            }

            fixed (byte* d = &dst[pos])
            {
                return ULongToByteString(value, d);
            }
        }

        public static unsafe int IntToByteString(int value, byte* dst)
        {
            if (value == 0)
            {
                *dst = ASCIIzero;
                return 1;
            }

            byte* tmp = stackalloc byte[16];
            int n;
            if (value > 0)
            {
                n = UintToStrBytes_reversed((uint)value, tmp);
                for (int i = 0, j = n - 1; i < n; ++i, --j)
                    dst[i] = tmp[j];
            }
            else
            {
                dst[0] = ASCIIminus;
                n = UintToStrBytes_reversed((uint)(-value), tmp);
                for (int i = 1, j = n - 1; i < n + 1; ++i, --j)
                    dst[i] = tmp[j];
                ++n;
            }
            return n;
        }

        public static unsafe int LongToByteString(long value, byte* dst)
        {
            if (value == 0)
            {
                *dst = ASCIIzero;
                return 1;
            }

            byte* tmp = stackalloc byte[32];
            int n;
            if (value > 0)
            {
                n = ULongToStrBytes_reversed((ulong)value, tmp);
                for (int i = 0, j = n - 1; i < n; ++i, --j)
                    dst[i] = tmp[j];
            }
            else
            {
                dst[0] = ASCIIminus;
                n = ULongToStrBytes_reversed((ulong)(-value), tmp);
                for (int i = 1, j = n - 1; i < n + 1; ++i, --j)
                    dst[i] = tmp[j];
                ++n;
            }
            return n;
        }

        public static unsafe int IntToByteString(int value, Stream st)
        {
            if (value == 0)
            {
                st.WriteByte(ASCIIzero);
                return 1;
            }

            byte* tmp = stackalloc byte[32];
            int n;
            if (value > 0)
            {
                n = UintToStrBytes_reversed((uint)value, tmp);
                for (int j = n - 1; j >= 0; --j)
                    st.WriteByte(tmp[j]);
            }
            else
            {
                st.WriteByte(ASCIIminus);
                n = UintToStrBytes_reversed((uint)(-value), tmp);
                for (int j = n - 1; j >= 0; --j)
                    st.WriteByte(tmp[j]);
                ++n;
            }
            return n;
        }

        public static unsafe int LongToByteString(long value, Stream st)
        {
            if (value == 0)
            {
                st.WriteByte(ASCIIzero);
                return 1;
            }

            byte* tmp = stackalloc byte[32];
            int n;
            if (value > 0)
            {
                n = ULongToStrBytes_reversed((ulong)value, tmp);
                for (int j = n - 1; j >= 0; --j)
                    st.WriteByte(tmp[j]);
            }
            else
            {
                st.WriteByte(ASCIIminus);
                n = ULongToStrBytes_reversed((ulong)(-value), tmp);
                for (int j = n - 1; j >= 0; --j)
                    st.WriteByte(tmp[j]);
                ++n;
            }
            return n;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe int IntToByteString(int value, byte[] dst, int pos)
        {
            if (value == 0)
            {
                dst[pos] = ASCIIzero;
                return 1;
            }

            fixed (byte* d = &dst[pos])
            {
                return IntToByteString(value, d);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe int LongToByteString(long value, byte[] dst, int pos)
        {
            if (value == 0)
            {
                dst[pos] = ASCIIzero;
                return 1;
            }

            fixed (byte* d = &dst[pos])
            {
                return LongToByteString(value, d);
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe osUTF8 IntToosUTF8(int v)
        {
            osUTF8 ret = new osUTF8(16);
            fixed (byte* d = ret.m_data)
                ret.m_len = IntToByteString(v, d);
            return ret;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe osUTF8 UIntToosUTF8(uint v)
        {
            osUTF8 ret = new osUTF8(16);
            fixed (byte* d = ret.m_data)
                ret.m_len = UIntToByteString(v, d);
            return ret;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe osUTF8 LongToosUTF8(long v)
        {
            osUTF8 ret = new osUTF8(32);
            fixed (byte* d = ret.m_data)
                ret.m_len = LongToByteString(v, d);
            return ret;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe osUTF8 ULongToosUTF8(ulong v)
        {
            osUTF8 ret = new osUTF8(32);
            fixed (byte* d = ret.m_data)
                ret.m_len = ULongToByteString(v, d);
            return ret;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte NibbleToHexUpper(byte b)
        {
            b &= 0x0f;
            return (byte)(b > 9 ? b + 0x37 : b + ASCIIzero);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte LowNibbleToHexByteChar(byte b)
        {
            b &= 0x0f;
            return (byte)(b > 9 ? b + 0x57 : b + ASCIIzero);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte HighNibbleToHexByteChar(byte b)
        {
            b >>= 4;
            return (byte)(b > 9 ? b + 0x57 : b + ASCIIzero);
        }

        public static unsafe void UUIDToByteDashString(ref UUID u, byte* dst)
        {
            byte b;
            if (BitConverter.IsLittleEndian)
            {
                //a
                b = u.bytea3;
                *dst = HighNibbleToHexByteChar(b);
                *(dst + 1) = LowNibbleToHexByteChar(b);
                b = u.bytea2;
                *(dst + 2) = HighNibbleToHexByteChar(b);
                *(dst + 3) = LowNibbleToHexByteChar(b);
                b = u.bytea1;
                *(dst + 4) = HighNibbleToHexByteChar(b);
                *(dst + 5) = LowNibbleToHexByteChar(b);
                b = u.bytea0;
                *(dst + 6) = HighNibbleToHexByteChar(b);
                *(dst + 7) = LowNibbleToHexByteChar(b);

                *(dst + 8) = ASCIIminus;
                //b
                b = u.byteb1;
                *(dst + 9) = HighNibbleToHexByteChar(b);
                *(dst + 10) = LowNibbleToHexByteChar(b);
                b = u.byteb0;
                *(dst + 11) = HighNibbleToHexByteChar(b);
                *(dst + 12) = LowNibbleToHexByteChar(b);

                *(dst + 13) = ASCIIminus;

                //c
                b = u.bytec1;
                *(dst + 14) = HighNibbleToHexByteChar(b);
                *(dst + 15) = LowNibbleToHexByteChar(b);
                b = u.bytec0;
                *(dst + 16) = HighNibbleToHexByteChar(b);
                *(dst + 17) = LowNibbleToHexByteChar(b);
            }
            else
            {
                //a
                b = u.bytea0;
                *dst = HighNibbleToHexByteChar(b);
                *(dst + 1) = LowNibbleToHexByteChar(b);
                b = u.bytea1;
                *(dst + 2) = HighNibbleToHexByteChar(b);
                *(dst + 3) = LowNibbleToHexByteChar(b);
                b = u.bytea2;
                *(dst + 4) = HighNibbleToHexByteChar(b);
                *(dst + 5) = LowNibbleToHexByteChar(b);
                b = u.bytea3;
                *(dst + 6) = HighNibbleToHexByteChar(b);
                *(dst + 7) = LowNibbleToHexByteChar(b);

                *(dst + 8) = ASCIIminus;
                //b
                b = u.byteb0;
                *(dst + 9) = HighNibbleToHexByteChar(b);
                *(dst + 10) = LowNibbleToHexByteChar(b);
                b = u.byteb1;
                *(dst + 11) = HighNibbleToHexByteChar(b);
                *(dst + 12) = LowNibbleToHexByteChar(b);

                *(dst + 13) = ASCIIminus;

                //c
                b = u.bytec0;
                *(dst + 14) = HighNibbleToHexByteChar(b);
                *(dst + 15) = LowNibbleToHexByteChar(b);
                b = u.bytec1;
                *(dst + 16) = HighNibbleToHexByteChar(b);
                *(dst + 17) = LowNibbleToHexByteChar(b);
            }

            *(dst + 18) = ASCIIminus;

            b = u.d; //d
            *(dst + 19) = HighNibbleToHexByteChar(b);
            *(dst + 20) = LowNibbleToHexByteChar(b);
            b = u.e; //e
            *(dst + 21) = HighNibbleToHexByteChar(b);
            *(dst + 22) = LowNibbleToHexByteChar(b);

            *(dst + 23) = ASCIIminus;

            b = u.f; //f
            *(dst + 24) = HighNibbleToHexByteChar(b);
            *(dst + 25) = LowNibbleToHexByteChar(b);
            b = u.g; //g
            *(dst + 26) = HighNibbleToHexByteChar(b);
            *(dst + 27) = LowNibbleToHexByteChar(b);
            b = u.h; //h
            *(dst + 28) = HighNibbleToHexByteChar(b);
            *(dst + 29) = LowNibbleToHexByteChar(b);
            b = u.i; //i
            *(dst + 30) = HighNibbleToHexByteChar(b);
            *(dst + 31) = LowNibbleToHexByteChar(b);
            b = u.j; //j
            *(dst + 32) = HighNibbleToHexByteChar(b);
            *(dst + 33) = LowNibbleToHexByteChar(b);
            b = u.k; //k
            *(dst + 34) = HighNibbleToHexByteChar(b);
            *(dst + 35) = LowNibbleToHexByteChar(b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe osUTF8 UUIDToosUTF8(UUID v)
        {
            osUTF8 ret = new osUTF8(36);
            fixed (byte* d = ret.m_data)
                UUIDToByteDashString(ref v, d);
            ret.m_len = 36;
            return ret;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe osUTF8 UUIDToosUTF8(ref UUID v)
        {
            osUTF8 ret = new osUTF8(36);
            fixed (byte* d = ret.m_data)
                UUIDToByteDashString(ref v, d);
            ret.m_len = 36;
            return ret;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static char charNibbleToHexUpper(byte b)
        {
            b &= 0x0f;
            return (char)(b > 9 ? b + 0x37 : b + ASCIIzero);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static char charLowNibbleToHexChar(byte b)
        {
            b &= 0x0f;
            return (char)(b > 9 ? b + 0x57 : b + ASCIIzero);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static char charHighNibbleToHexChar(byte b)
        {
            b >>= 4;
            return (char)(b > 9 ? b + 0x57 : b + ASCIIzero);
        }

        public static string UUIDToDashString(ref UUID u)
        {
            char[] dst = new char[36];
            byte b;
            if (BitConverter.IsLittleEndian)
            {
                //a
                b = u.bytea3;
                dst[0] = charHighNibbleToHexChar(b);
                dst[1] = charLowNibbleToHexChar(b);
                b = u.bytea2;
                dst[2] = charHighNibbleToHexChar(b);
                dst[3] = charLowNibbleToHexChar(b);
                b = u.bytea1;
                dst[4] = charHighNibbleToHexChar(b);
                dst[5] = charLowNibbleToHexChar(b);
                b = u.bytea0;
                dst[6] = charHighNibbleToHexChar(b);
                dst[7] = charLowNibbleToHexChar(b);

                dst[8] = '-';

                //b
                b = u.byteb1;
                dst[9] = charHighNibbleToHexChar(b);
                dst[10] = charLowNibbleToHexChar(b);
                b = u.byteb0;
                dst[11] = charHighNibbleToHexChar(b);
                dst[12] = charLowNibbleToHexChar(b);

                dst[13] = '-';

                //c
                b = u.bytec1;
                dst[14] = charHighNibbleToHexChar(b);
                dst[15] = charLowNibbleToHexChar(b);
                b = u.bytec0;
                dst[16] = charHighNibbleToHexChar(b);
                dst[17] = charLowNibbleToHexChar(b);
            }
            else
            {
                //a
                b = u.bytea0;
                dst[0] = charHighNibbleToHexChar(b);
                dst[1] = charLowNibbleToHexChar(b);
                b = u.bytea1;
                dst[2] = charHighNibbleToHexChar(b);
                dst[3] = charLowNibbleToHexChar(b);
                b = u.bytea2;
                dst[4] = charHighNibbleToHexChar(b);
                dst[5] = charLowNibbleToHexChar(b);
                b = u.bytea3;
                dst[6] = charHighNibbleToHexChar(b);
                dst[7] = charLowNibbleToHexChar(b);

                dst[8] = '-';

                //b
                b = u.byteb0;
                dst[9] = charHighNibbleToHexChar(b);
                dst[10] = charLowNibbleToHexChar(b);
                b = u.byteb1;
                dst[11] = charHighNibbleToHexChar(b);
                dst[12] = charLowNibbleToHexChar(b);

                dst[13] = '-';

                //c
                b = u.bytec0;
                dst[14] = charHighNibbleToHexChar(b);
                dst[15] = charLowNibbleToHexChar(b);
                b = u.bytec1;
                dst[16] = charHighNibbleToHexChar(b);
                dst[17] = charLowNibbleToHexChar(b);
            }


            dst[18] = '-';

            b = u.d; //d
            dst[19] = charHighNibbleToHexChar(b);
            dst[20] = charLowNibbleToHexChar(b);
            b = u.e; //e
            dst[21] = charHighNibbleToHexChar(b);
            dst[22] = charLowNibbleToHexChar(b);

            dst[23] = '-';

            b = u.f; //f
            dst[24] = charHighNibbleToHexChar(b);
            dst[25] = charLowNibbleToHexChar(b);
            b = u.g; //g
            dst[26] = charHighNibbleToHexChar(b);
            dst[27] = charLowNibbleToHexChar(b);
            b = u.h; //h
            dst[28] = charHighNibbleToHexChar(b);
            dst[29] = charLowNibbleToHexChar(b);
            b = u.i; //i
            dst[30] = charHighNibbleToHexChar(b);
            dst[31] = charLowNibbleToHexChar(b);
            b = u.j; //j
            dst[32] = charHighNibbleToHexChar(b);
            dst[33] = charLowNibbleToHexChar(b);
            b = u.k; //k
            dst[34] = charHighNibbleToHexChar(b);
            dst[35] = charLowNibbleToHexChar(b);
            return new string(dst);
        }

        #endregion Miscellaneous
    }
}
