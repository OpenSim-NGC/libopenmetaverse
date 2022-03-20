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
using System.Xml.Serialization;

namespace OpenMetaverse
{
    /// <summary>
    /// A 128-bit Universally Unique Identifier
    /// Only diference from Guid is on to and from bytes, where it uses network byte order unlike Guid
    /// </summary>

    [StructLayout(LayoutKind.Explicit)]
    [Serializable()]
    public struct UUID : IComparable,IComparable<UUID>, IEquatable<UUID>
    {
        // still a big piece of *** because how stupid .net structs are.
        // .net5 unsafe may remove the need for this union
        [XmlIgnore] [NonSerialized()] [FieldOffset(0)] public int a;
        [XmlIgnore] [NonSerialized()] [FieldOffset(4)] public short b;
        [XmlIgnore] [NonSerialized()] [FieldOffset(6)] public short c;

        [XmlIgnore] [NonSerialized()] [FieldOffset(8)] public byte d;
        [XmlIgnore] [NonSerialized()] [FieldOffset(9)] public byte e;
        [XmlIgnore] [NonSerialized()] [FieldOffset(10)] public byte f;
        [XmlIgnore] [NonSerialized()] [FieldOffset(11)] public byte g;
        [XmlIgnore] [NonSerialized()] [FieldOffset(12)] public byte h;
        [XmlIgnore] [NonSerialized()] [FieldOffset(13)] public byte i;
        [XmlIgnore] [NonSerialized()] [FieldOffset(14)] public byte j;
        [XmlIgnore] [NonSerialized()] [FieldOffset(15)] public byte k;

        [XmlIgnore] [NonSerialized()] [FieldOffset(0)] public byte bytea0;
        [XmlIgnore] [NonSerialized()] [FieldOffset(1)] public byte bytea1;
        [XmlIgnore] [NonSerialized()] [FieldOffset(2)] public byte bytea2;
        [XmlIgnore] [NonSerialized()] [FieldOffset(3)] public byte bytea3;

        [XmlIgnore] [NonSerialized()] [FieldOffset(4)] public byte byteb0;
        [XmlIgnore] [NonSerialized()] [FieldOffset(5)] public byte byteb1;

        [XmlIgnore] [NonSerialized()] [FieldOffset(6)] public byte bytec0;
        [XmlIgnore] [NonSerialized()] [FieldOffset(7)] public byte bytec1;

        [XmlIgnore] [NonSerialized()] [FieldOffset(4)] public int intb;
        [XmlIgnore] [NonSerialized()] [FieldOffset(8)] public int intc;
        [XmlIgnore] [NonSerialized()] [FieldOffset(12)] public int intd;

        [XmlIgnore] [NonSerialized()] [FieldOffset(0)] public ulong ulonga;
        [XmlIgnore] [NonSerialized()] [FieldOffset(8)] public ulong ulongb;

        [FieldOffset(0)] public Guid Guid;

        #region Constructors

        /// <summary>
        /// Constructor that takes a string UUID representation
        /// </summary>
        /// <param name="val">A string representation of a UUID, case 
        /// insensitive and can either be hyphenated or non-hyphenated</param>
        /// <example>UUID("11f8aa9c-b071-4242-836b-13b7abe0d489")</example>
        /*
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe UUID(string val)
        {
            if (!string.IsNullOrEmpty(val))
            {
                try
                {
                    if (Guid.TryParse(val, out Guid gg))
                    {
                        this = *(UUID*)&gg;
                        return;
                    }
                }
                catch { }
            }
            this = new UUID();
        }
        */

        public unsafe UUID(string sval)
        {
            this = new UUID();
            if (string.IsNullOrEmpty(sval))
                throw new System.FormatException("Invalid UUID");

            try
            {
                fixed (char* bval = sval)
                {
                    char *val = bval;
                    while (*val == ' ') ++val;

                    if (val[8] == '-')
                    {
                        if (val[13] != '-' || val[18] != '-' || val[23] != '-')
                            throw new Exception();

                        if (BitConverter.IsLittleEndian)
                        {
                            bytea3 = (byte)Utils.HexToByte(val, 0);
                            bytea2 = (byte)Utils.HexToByte(val, 2);
                            bytea1 = (byte)Utils.HexToByte(val, 4);
                            bytea0 = (byte)Utils.HexToByte(val, 6);

                            byteb1 = (byte)Utils.HexToByte(val, 9);
                            byteb0 = (byte)Utils.HexToByte(val, 11);

                            bytec1 = (byte)Utils.HexToByte(val, 14);
                            bytec0 = (byte)Utils.HexToByte(val, 16);
                        }
                        else
                        {
                            bytea0 = (byte)Utils.HexToByte(val, 0);
                            bytea1 = (byte)Utils.HexToByte(val, 2);
                            bytea2 = (byte)Utils.HexToByte(val, 4);
                            bytea3 = (byte)Utils.HexToByte(val, 6);

                            byteb0 = (byte)Utils.HexToByte(val, 9);
                            byteb1 = (byte)Utils.HexToByte(val, 11);

                            bytec0 = (byte)Utils.HexToByte(val, 14);
                            bytec1 = (byte)Utils.HexToByte(val, 16);
                        }


                        d = (byte)Utils.HexToByte(val, 19);
                        e = (byte)Utils.HexToByte(val, 21);


                        f = (byte)Utils.HexToByte(val, 24);
                        g = (byte)Utils.HexToByte(val, 26);
                        h = (byte)Utils.HexToByte(val, 28);
                        i = (byte)Utils.HexToByte(val, 30);
                        j = (byte)Utils.HexToByte(val, 32);
                        k = (byte)Utils.HexToByte(val, 34);
                        return;
                    }
                    else
                    {
                        if (BitConverter.IsLittleEndian)
                        {
                            bytea3 = Utils.HexToByte(val, 0);
                            bytea2 = Utils.HexToByte(val, 2);
                            bytea1 = Utils.HexToByte(val, 4);
                            bytea0 = Utils.HexToByte(val, 6);

                            byteb1 = Utils.HexToByte(val, 8);
                            byteb0 = Utils.HexToByte(val, 10);

                            bytec1 = Utils.HexToByte(val, 12);
                            bytec0 = Utils.HexToByte(val, 14);
                        }
                        else
                        {
                            bytea0 = Utils.HexToByte(val, 0);
                            bytea1 = Utils.HexToByte(val, 2);
                            bytea2 = Utils.HexToByte(val, 4);
                            bytea3 = Utils.HexToByte(val, 6);

                            byteb0 = Utils.HexToByte(val, 8);
                            byteb1 = Utils.HexToByte(val, 10);

                            bytec0 = Utils.HexToByte(val, 12);
                            bytec1 = Utils.HexToByte(val, 14);
                        }

                        d = Utils.HexToByte(val, 16);
                        e = Utils.HexToByte(val, 18);

                        f = Utils.HexToByte(val, 20);
                        g = Utils.HexToByte(val, 22);
                        h = Utils.HexToByte(val, 24);
                        i = Utils.HexToByte(val, 26);
                        j = Utils.HexToByte(val, 28);
                        k = Utils.HexToByte(val, 30);
                        return;
                    }
                }
            }
            catch { }
            this = new UUID();
            throw new System.FormatException("Invalid UUID");
        }

        /// <summary>
        /// Constructor that takes a System.Guid object
        /// </summary>
        /// <param name="val">A Guid object that contains the unique identifier
        /// to be represented by this UUID</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe UUID(Guid val)
        {
            this = *(UUID*)&val;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe UUID(ulong _la, ulong _lb) : this()
        {
            ulonga = _la;
            ulongb = _lb;
        }

        /// <summary>
        /// Constructor that takes a byte array containing a UUID
        /// </summary>
        /// <param name="source">Byte array containing a 16 byte UUID</param>
        /// <param name="pos">Beginning offset in the array</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe UUID(byte[] source, int pos) : this()
        {
            fixed (byte* ptr = &source[pos])
            {
                if (BitConverter.IsLittleEndian)
                {
                    bytea3 = *ptr;
                    bytea2 = *(ptr + 1);
                    bytea1 = *(ptr + 2);
                    bytea0 = *(ptr + 3);

                    byteb1 = *(ptr + 4);
                    byteb0 = *(ptr + 5);

                    bytec1 = *(ptr + 6);
                    bytec0 = *(ptr + 7);

                    ulongb = *(ulong*)(ptr + 8);
                }
                else
                {
                    ulonga = *(ulong*)(ptr);

                    d = *(ptr + 8);
                    e = *(ptr + 9);
                    f = *(ptr + 10);
                    g = *(ptr + 11);
                    h = *(ptr + 12);
                    i = *(ptr + 13);
                    j = *(ptr + 14);
                    k = *(ptr + 15);
                }
            }
        }

        /// <summary>
        /// Constructor that takes an unsigned 64-bit unsigned integer to 
        /// convert to a UUID
        /// </summary>
        /// <param name="val">64-bit unsigned integer to convert to a UUID</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UUID(ulong val) : this()
        {
            if (BitConverter.IsLittleEndian)
                ulongb = val;
            else
            {
                d = (byte)val;
                e = (byte)(val >> 8);
                f = (byte)(val >> 16);
                g = (byte)(val >> 24);
                h = (byte)(val >> 32);
                i = (byte)(val >> 40);
                j = (byte)(val >> 48);
                k = (byte)(val >> 56);
            }
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="val">UUID to copy</param>
        public UUID(UUID val)
        {
            this = val;
        }

        #endregion Constructors

        #region Public Methods

        /// <summary>
        /// IComparable.CompareTo implementation
        /// </summary>
        public int CompareTo(object val)
        {
            if (val == null)
                return 1;

            UUID id = (UUID)val;

            if (id.a != a)
                return (uint)id.a > (uint)a ? -1 : 1;
            if (id.b != b)
                return (uint)id.b > (uint)b ? -1 : 1;
            if (id.c != c)
                return (uint)id.c > (uint)c ? -1 : 1;

            if (id.d != d)
                return id.d > d ? -1 : 1;
            if (id.e != e)
                return id.e > e ? -1 : 1;
            if (id.f != f)
                return id.f > f ? -1 : 1;
            if (id.g != g)
                return id.g > g ? -1 : 1;
            if (id.h != h)
                return id.h > h ? -1 : 1;
            if (id.i != i)
                return id.i > i ? -1 : 1;
            if (id.j != j)
                return id.j > j ? -1 : 1;
            if (id.k != k)
                return id.k > k ? -1 : 1;
            return 0;
        }

        public int CompareTo(UUID id)
        {
            if (id.a != a)
                return (uint)id.a > (uint)a ? -1 : 1;
            if (id.b != b)
                return (uint)id.b > (uint)b ? -1 : 1;
            if (id.c != c)
                return (uint)id.c > (uint)c ? -1 : 1;

            if (id.d != d)
                return id.d > d ? -1 : 1;
            if (id.e != e)
                return id.e > e ? -1 : 1;
            if (id.f != f)
                return id.f > f ? -1 : 1;
            if (id.g != g)
                return id.g > g ? -1 : 1;
            if (id.h != h)
                return id.h > h ? -1 : 1;
            if (id.i != i)
                return id.i > i ? -1 : 1;
            if (id.j != j)
                return id.j > j ? -1 : 1;
            if (id.k != k)
                return id.k > k ? -1 : 1;
            return 0;
        }

        /// <summary>
        /// Assigns this UUID from 16 bytes out of a byte array
        /// </summary>
        /// <param name="source">Byte array containing the UUID to assign this UUID to</param>
        /// <param name="pos">Starting position of the UUID in the byte array</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void FromBytes(byte[] source, int pos)
        {
            fixed (byte* ptr = &source[pos])
            {
                if (BitConverter.IsLittleEndian)
                {
                    bytea3 = *ptr;
                    bytea2 = *(ptr + 1);
                    bytea1 = *(ptr + 2);
                    bytea0 = *(ptr + 3);

                    byteb1 = *(ptr + 4);
                    byteb0 = *(ptr + 5);

                    bytec1 = *(ptr + 6);
                    bytec0 = *(ptr + 7);
                    ulongb = *(ulong*)(ptr + 8);
                }
                else
                {
                    ulonga = *(ulong*)(ptr);
                    d = *(ptr + 8);
                    e = *(ptr + 9);
                    f = *(ptr + 10);
                    g = *(ptr + 11);
                    h = *(ptr + 12);
                    i = *(ptr + 13);
                    j = *(ptr + 14);
                    k = *(ptr + 15);
                }
            }
        }

        /// <summary>
        /// Returns a copy of the raw bytes for this UUID
        /// </summary>
        /// <returns>A 16 byte array containing this UUID</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe byte[] GetBytes()
        {
            byte[] dest = new byte[16];
            fixed (byte* ptr = &dest[0])
            {
                if (BitConverter.IsLittleEndian)
                {
                    *ptr = bytea3;
                    *(ptr + 1) = bytea2;
                    *(ptr + 2) = bytea1;
                    *(ptr + 3) = bytea0;
                    *(ptr + 4) = byteb1;
                    *(ptr + 5) = byteb0;
                    *(ptr + 6) = bytec1;
                    *(ptr + 7) = bytec0;
                    *(ulong*)(ptr + 8) = ulongb;
                }
                else
                {
                    *(ulong*)(ptr) = ulonga;
                    *(ptr + 8) = d;
                    *(ptr + 9) = e;
                    *(ptr + 10) = f;
                    *(ptr + 11) = g;
                    *(ptr + 12) = h;
                    *(ptr + 13) = i;
                    *(ptr + 14) = j;
                    *(ptr + 15) = k;
                }
            }
            return dest;
        }

        /// <summary>
        /// Writes the raw bytes for this UUID to a byte array
        /// </summary>
        /// <param name="dest">Destination byte array</param>
        /// <param name="pos">Position in the destination array to start
        /// writing. Must be at least 16 bytes before the end of the array</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void ToBytes(byte[] dest, int pos)
        {
            fixed (byte* ptr = &dest[pos])
            {
                if (BitConverter.IsLittleEndian)
                {
                    *ptr = bytea3;
                    *(ptr + 1) = bytea2;
                    *(ptr + 2) = bytea1;
                    *(ptr + 3) = bytea0;
                    *(ptr + 4) = byteb1;
                    *(ptr + 5) = byteb0;
                    *(ptr + 6) = bytec1;
                    *(ptr + 7) = bytec0;
                    *(ulong*)(ptr + 8) = ulongb;
                }
                else
                {
                    *(ulong*)(ptr) = ulonga;
                    *(ptr + 8) = d;
                    *(ptr + 9) = e;
                    *(ptr + 10) = f;
                    *(ptr + 11) = g;
                    *(ptr + 12) = h;
                    *(ptr + 13) = i;
                    *(ptr + 14) = j;
                    *(ptr + 15) = k;
                }
            }
        }

        /// <summary>
        /// Calculate an LLCRC (cyclic redundancy check) for this UUID
        /// </summary>
        /// <returns>The CRC checksum for this UUID</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint CRC()
        {
            return (uint)a + (uint)intb + (uint)intc + (uint)intd;
        }

        /// <summary>
        /// Create a 64-bit integer representation from the second half of this UUID
        /// </summary>
        /// <returns>An integer created from the last eight bytes of this UUID</returns>
        public ulong GetULong()
        {
            if (BitConverter.IsLittleEndian)
                return ulongb;
            else
                return d +
                    ((ulong)e << 8) +
                    ((ulong)f << 16) +
                    ((ulong)g << 24) +
                    ((ulong)h << 32) +
                    ((ulong)i << 40) +
                    ((ulong)j << 48) +
                    ((ulong)k << 56);
        }

        #endregion Public Methods

        #region Static Methods

        /// <summary>
        /// Generate a UUID from a string
        /// </summary>
        /// <param name="val">A string representation of a UUID, case 
        /// insensitive and can either be hyphenated or non-hyphenated</param>
        /// <example>UUID.Parse("11f8aa9c-b071-4242-836b-13b7abe0d489")</example>
        /*
        public static UUID Parse(string val)
        {
            Guid gg = Guid.Parse(val);
            return new UUID(gg);          
        }
        */

        public unsafe static UUID Parse(string sval)
        {
            UUID result = new UUID();
            if (string.IsNullOrEmpty(sval))
                throw new System.FormatException("Invalid UUID");
            try
            {
                fixed (char* bval = sval)
                {
                    char* val = bval;

                    while (*val == ' ') ++val;
                    if (val[8] == '-')
                    {
                        if (val[13] != '-' || val[18] != '-' || val[23] != '-')
                            throw new Exception();

                        if (BitConverter.IsLittleEndian)
                        {
                            result.bytea3 = (byte)Utils.HexToByte(val, 0);
                            result.bytea2 = (byte)Utils.HexToByte(val, 2);
                            result.bytea1 = (byte)Utils.HexToByte(val, 4);
                            result.bytea0 = (byte)Utils.HexToByte(val, 6);

                            result.byteb1 = (byte)Utils.HexToByte(val, 9);
                            result.byteb0 = (byte)Utils.HexToByte(val, 11);

                            result.bytec1 = (byte)Utils.HexToByte(val, 14);
                            result.bytec0 = (byte)Utils.HexToByte(val, 16);
                        }
                        else
                        {
                            result.bytea0 = (byte)Utils.HexToByte(val, 0);
                            result.bytea1 = (byte)Utils.HexToByte(val, 2);
                            result.bytea2 = (byte)Utils.HexToByte(val, 4);
                            result.bytea3 = (byte)Utils.HexToByte(val, 6);

                            result.byteb0 = (byte)Utils.HexToByte(val, 9);
                            result.byteb1 = (byte)Utils.HexToByte(val, 11);

                            result.bytec0 = (byte)Utils.HexToByte(val, 14);
                            result.bytec1 = (byte)Utils.HexToByte(val, 16);
                        }

                        result.d = (byte)Utils.HexToByte(val, 19);
                        result.e = (byte)Utils.HexToByte(val, 21);


                        result.f = (byte)Utils.HexToByte(val, 24);
                        result.g = (byte)Utils.HexToByte(val, 26);
                        result.h = (byte)Utils.HexToByte(val, 28);
                        result.i = (byte)Utils.HexToByte(val, 30);
                        result.j = (byte)Utils.HexToByte(val, 32);
                        result.k = (byte)Utils.HexToByte(val, 34);
                        return result;
                    }
                    else
                    {
                        if (BitConverter.IsLittleEndian)
                        {
                            result.bytea3 = Utils.HexToByte(val, 0);
                            result.bytea2 = Utils.HexToByte(val, 2);
                            result.bytea1 = Utils.HexToByte(val, 4);
                            result.bytea0 = Utils.HexToByte(val, 6);

                            result.byteb1 = Utils.HexToByte(val, 8);
                            result.byteb0 = Utils.HexToByte(val, 10);

                            result.bytec1 = Utils.HexToByte(val, 12);
                            result.bytec0 = Utils.HexToByte(val, 14);
                        }
                        else
                        {
                            result.bytea0 = Utils.HexToByte(val, 0);
                            result.bytea1 = Utils.HexToByte(val, 2);
                            result.bytea2 = Utils.HexToByte(val, 4);
                            result.bytea3 = Utils.HexToByte(val, 6);

                            result.byteb0 = Utils.HexToByte(val, 8);
                            result.byteb1 = Utils.HexToByte(val, 10);

                            result.bytec0 = Utils.HexToByte(val, 12);
                            result.bytec1 = Utils.HexToByte(val, 14);
                        }

                        result.d = Utils.HexToByte(val, 16);
                        result.e = Utils.HexToByte(val, 18);

                        result.f = Utils.HexToByte(val, 20);
                        result.g = Utils.HexToByte(val, 22);
                        result.h = Utils.HexToByte(val, 24);
                        result.i = Utils.HexToByte(val, 26);
                        result.j = Utils.HexToByte(val, 28);
                        result.k = Utils.HexToByte(val, 30);
                        return result;
                    }
                }
            }
            catch { }
            result = new UUID();
            throw new System.FormatException("Invalid UUID");
        }

        /// <summary>
        /// Generate a UUID from a string
        /// </summary>
        /// <param name="val">A string representation of a UUID, case 
        /// insensitive and can either be hyphenated or non-hyphenated</param>
        /// <param name="result">Will contain the parsed UUID if successful,
        /// otherwise null</param>
        /// <returns>True if the string was successfully parse, otherwise false</returns>
        /// <example>UUID.TryParse("11f8aa9c-b071-4242-836b-13b7abe0d489", result)</example>

        public unsafe static bool TryParse(string sval, out UUID result)
        {
            result = new UUID();
            if (string.IsNullOrEmpty(sval))
                return false;

            try
            {
                fixed (char* bval = sval)
                {
                    char* val = bval;

                    while (*val == ' ') ++val;
                    if (val[8] == '-')
                    {
                        if (val[13] != '-' || val[18] != '-' || val[23] != '-')
                            return false;

                        if (BitConverter.IsLittleEndian)
                        {
                            result.bytea3 = (byte)Utils.HexToByte(val, 0);
                            result.bytea2 = (byte)Utils.HexToByte(val, 2);
                            result.bytea1 = (byte)Utils.HexToByte(val, 4);
                            result.bytea0 = (byte)Utils.HexToByte(val, 6);

                            result.byteb1 = (byte)Utils.HexToByte(val, 9);
                            result.byteb0 = (byte)Utils.HexToByte(val, 11);

                            result.bytec1 = (byte)Utils.HexToByte(val, 14);
                            result.bytec0 = (byte)Utils.HexToByte(val, 16);
                        }
                        else
                        {
                            result.bytea0 = (byte)Utils.HexToByte(val, 0);
                            result.bytea1 = (byte)Utils.HexToByte(val, 2);
                            result.bytea2 = (byte)Utils.HexToByte(val, 4);
                            result.bytea3 = (byte)Utils.HexToByte(val, 6);

                            result.byteb0 = (byte)Utils.HexToByte(val, 9);
                            result.byteb1 = (byte)Utils.HexToByte(val, 11);

                            result.bytec0 = (byte)Utils.HexToByte(val, 14);
                            result.bytec1 = (byte)Utils.HexToByte(val, 16);
                        }

                        result.d = (byte)Utils.HexToByte(val, 19);
                        result.e = (byte)Utils.HexToByte(val, 21);


                        result.f = (byte)Utils.HexToByte(val, 24);
                        result.g = (byte)Utils.HexToByte(val, 26);
                        result.h = (byte)Utils.HexToByte(val, 28);
                        result.i = (byte)Utils.HexToByte(val, 30);
                        result.j = (byte)Utils.HexToByte(val, 32);
                        result.k = (byte)Utils.HexToByte(val, 34);
                        return true;
                    }
                    else
                    {
                        if (BitConverter.IsLittleEndian)
                        {
                            result.bytea3 = Utils.HexToByte(val, 0);
                            result.bytea2 = Utils.HexToByte(val, 2);
                            result.bytea1 = Utils.HexToByte(val, 4);
                            result.bytea0 = Utils.HexToByte(val, 6);

                            result.byteb1 = Utils.HexToByte(val, 8);
                            result.byteb0 = Utils.HexToByte(val, 10);

                            result.bytec1 = Utils.HexToByte(val, 12);
                            result.bytec0 = Utils.HexToByte(val, 14);
                        }
                        else
                        {
                            result.bytea0 = Utils.HexToByte(val, 0);
                            result.bytea1 = Utils.HexToByte(val, 2);
                            result.bytea2 = Utils.HexToByte(val, 4);
                            result.bytea3 = Utils.HexToByte(val, 6);

                            result.byteb0 = Utils.HexToByte(val, 8);
                            result.byteb1 = Utils.HexToByte(val, 10);

                            result.bytec0 = Utils.HexToByte(val, 12);
                            result.bytec1 = Utils.HexToByte(val, 14);
                        }

                        result.d = Utils.HexToByte(val, 16);
                        result.e = Utils.HexToByte(val, 18);

                        result.f = Utils.HexToByte(val, 20);
                        result.g = Utils.HexToByte(val, 22);
                        result.h = Utils.HexToByte(val, 24);
                        result.i = Utils.HexToByte(val, 26);
                        result.j = Utils.HexToByte(val, 28);
                        result.k = Utils.HexToByte(val, 30);
                        return true;
                    }
                }
            }
            catch { }
            result = new UUID();
            return false;
        }

        /// <summary>
        /// Combine two UUIDs together by taking the MD5 hash of a byte array
        /// containing both UUIDs
        /// </summary>
        /// <param name="first">First UUID to combine</param>
        /// <param name="second">Second UUID to combine</param>
        /// <returns>The UUID product of the combination</returns>
        public static UUID Combine(UUID first, UUID second)
        {
            // Construct the buffer that MD5ed
            byte[] input = new byte[32];
            Buffer.BlockCopy(first.GetBytes(), 0, input, 0, 16);
            Buffer.BlockCopy(second.GetBytes(), 0, input, 16, 16);

            return new UUID(Utils.MD5(input), 0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static UUID Random()
        {
            Guid g = Guid.NewGuid();
            return *(UUID*)&g;
        }

        #endregion Static Methods

        #region Overrides

        /// <summary>
        /// Return a hash code for this UUID, used by .NET for hash tables
        /// </summary>
        /// <returns>An integer composed of all the UUID bytes XORed together</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            return a ^ intb ^ intc ^ intd;
        }

        /// <summary>
        /// Comparison function
        /// </summary>
        /// <param name="o">An object to compare to this UUID</param>
        /// <returns>True if the object is a UUID and both UUIDs are equal</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object o)
        {
            if (!(o is UUID))
                return false;

            UUID uuid = (UUID)o;
            if (ulonga != uuid.ulonga)
                return false;
            if (ulongb != uuid.ulongb)
                return false;
            return true;
        }

        /// <summary>
        /// Comparison function
        /// </summary>
        /// <param name="uuid">UUID to compare to</param>
        /// <returns>True if the UUIDs are equal, otherwise false</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(UUID uuid)
        {
            if (ulonga != uuid.ulonga)
                return false;
            if (ulongb != uuid.ulongb)
                return false;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsZero()
        {
            if (ulonga != 0)
                return false;
            if (ulongb != 0)
                return false;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool NotEqual(UUID uuid)
        {
            if (ulonga != uuid.ulonga)
                return true;
            if (ulongb != uuid.ulongb)
                return true;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsNotZero()
        {
            if (ulonga != 0)
                return true;
            if (ulongb != 0)
                return true;
            return false;
        }

        /// <summary>
        /// Get a hyphenated string representation of this UUID
        /// </summary>
        /// <returns>A string representation of this UUID, lowercase and 
        /// with hyphens</returns>
        /// <example>11f8aa9c-b071-4242-836b-13b7abe0d489</example>

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
        {
            return Utils.UUIDToDashString(ref this);
        }

        #endregion Overrides

        #region Operators

        /// <summary>
        /// Equals operator
        /// </summary>
        /// <param name="lhs">First UUID for comparison</param>
        /// <param name="rhs">Second UUID for comparison</param>
        /// <returns>True if the UUIDs are byte for byte equal, otherwise false</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(UUID lhs, UUID rhs)
        {
            if (lhs.ulonga != rhs.ulonga)
                return false;
            if (lhs.ulongb != rhs.ulongb)
                return false;
            return true;
        }

        /// <summary>
        /// Not equals operator
        /// </summary>
        /// <param name="lhs">First UUID for comparison</param>
        /// <param name="rhs">Second UUID for comparison</param>
        /// <returns>True if the UUIDs are not equal, otherwise true</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(UUID lhs, UUID rhs)
        {
            if (lhs.ulonga != rhs.ulonga)
                return true;
            if (lhs.ulongb != rhs.ulongb)
                return true;
            return false;
        }

        /// <summary>
        /// XOR operator
        /// </summary>
        /// <param name="lhs">First UUID</param>
        /// <param name="rhs">Second UUID</param>
        /// <returns>A UUID that is a XOR combination of the two input UUIDs</returns>
        public static UUID operator ^(UUID lhs, UUID rhs)
        {
            UUID ret = new UUID();
            ret.ulonga = lhs.ulonga ^ rhs.ulonga;
            ret.ulongb = lhs.ulongb ^ rhs.ulongb;
            return ret;
        }

        /// <summary>
        /// String typecasting operator
        /// </summary>
        /// <param name="val">A UUID in string form. Case insensitive, 
        /// hyphenated or non-hyphenated</param>
        /// <returns>A UUID built from the string representation</returns>
        public static explicit operator UUID(string val)
        {
            return new UUID(val);
        }

        #endregion Operators

        /// <summary>An UUID with a value of all zeroes</summary>
        public static readonly UUID Zero = new UUID();

        /// <summary>A cache of UUID.Zero as a string to optimize a common path</summary>
        public static readonly string ZeroString = "00000000-0000-0000-0000-000000000000";
    }
}
