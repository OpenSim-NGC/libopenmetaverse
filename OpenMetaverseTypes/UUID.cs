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

namespace OpenMetaverse
{
    /// <summary>
    /// A 128-bit Universally Unique Identifier
    /// Only diference from Guid is on to and from bytes, where it uses network byte order unlike Guid
    /// </summary>

    [StructLayout(LayoutKind.Explicit)]
    [Serializable]
    public struct UUID : IComparable,IComparable<UUID>, IEquatable<UUID>
    {
        // still a big piece of *** because how stupid .net structs are.
        // .net5 unsafe may remove the need for this union
        [FieldOffset(0)] public int a;
        [FieldOffset(4)] public short b;
        [FieldOffset(6)] public short c;

        [FieldOffset(8)] public byte d;
        [FieldOffset(9)] public byte e;      
        [FieldOffset(10)] public byte f;
        [FieldOffset(11)] public byte g;
        [FieldOffset(12)] public byte h;
        [FieldOffset(13)] public byte i;
        [FieldOffset(14)] public byte j;
        [FieldOffset(15)] public byte k;

        [FieldOffset(0)] public byte bytea0;
        [FieldOffset(1)] public byte bytea1;
        [FieldOffset(2)] public byte bytea2;
        [FieldOffset(3)] public byte bytea3;

        [FieldOffset(4)] public byte byteb0;
        [FieldOffset(5)] public byte byteb1;

        [FieldOffset(6)] public byte bytec0;
        [FieldOffset(7)] public byte bytec1;

        [FieldOffset(4)] public int intb;
        [FieldOffset(8)] public int intc;
        [FieldOffset(12)] public int intd;

        [FieldOffset(0)] public ulong ulonga;
        [FieldOffset(8)] public ulong ulongb;

        [FieldOffset(0)] public Guid Guid;

        #region Constructors

        /// <summary>
        /// Constructor that takes a string UUID representation
        /// </summary>
        /// <param name="val">A string representation of a UUID, case 
        /// insensitive and can either be hyphenated or non-hyphenated</param>
        /// <example>UUID("11f8aa9c-b071-4242-836b-13b7abe0d489")</example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe UUID(string val)
        {
            if (!string.IsNullOrEmpty(val))
            {
                try
                {
                    if (Guid.TryParse(val, out Guid gg))
                    {
                        this = *(UUID *) &gg;
                        return;
                    }
                }
                catch { }
            }
            this = new UUID();
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
            if (BitConverter.IsLittleEndian)
            {
                bytea3 = source[pos + 0];
                bytea2 = source[pos + 1];
                bytea1 = source[pos + 2];
                bytea0 = source[pos + 3];

                byteb1 = source[pos + 4];
                byteb0 = source[pos + 5];

                bytec1 = source[pos + 6];
                bytec0 = source[pos + 7];
            }
            else
                ulonga = Utils.BytesToUInt64(source, pos);

            ulongb = Utils.BytesToUInt64(source, pos + 8);
        }

        /// <summary>
        /// Constructor that takes an unsigned 64-bit unsigned integer to 
        /// convert to a UUID
        /// </summary>
        /// <param name="val">64-bit unsigned integer to convert to a UUID</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UUID(ulong val) : this()
        {
            if(BitConverter.IsLittleEndian)
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
            if(val == null)
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
            if (BitConverter.IsLittleEndian)
            {
                bytea3 = source[pos + 0];
                bytea2 = source[pos + 1];
                bytea1 = source[pos + 2];
                bytea0 = source[pos + 3];

                byteb1 = source[pos + 4];
                byteb0 = source[pos + 5];

                bytec1 = source[pos + 6];
                bytec0 = source[pos + 7];
            }
            else
                ulonga = Utils.BytesToUInt64(source, pos);

            ulongb = Utils.BytesToUInt64(source, pos + 8);
        }

        /// <summary>
        /// Returns a copy of the raw bytes for this UUID
        /// </summary>
        /// <returns>A 16 byte array containing this UUID</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte[] GetBytes()
        {
            byte[] dest = new byte[16];
            if (BitConverter.IsLittleEndian)
            {
                dest[0] = bytea3;
                dest[1] = bytea2;
                dest[2] = bytea1;
                dest[3] = bytea0;

                dest[4] = byteb1;
                dest[5] = byteb0;

                dest[6] = bytec1;
                dest[7] = bytec0;
            }
            else
                Utils.UInt64ToBytesSafepos(ulonga, dest, 0);

            Utils.UInt64ToBytesSafepos(ulongb, dest, 8);
            return dest;
        }

        /// <summary>
        /// Writes the raw bytes for this UUID to a byte array
        /// </summary>
        /// <param name="dest">Destination byte array</param>
        /// <param name="pos">Position in the destination array to start
        /// writing. Must be at least 16 bytes before the end of the array</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ToBytes(byte[] dest, int pos)
        {
            if (BitConverter.IsLittleEndian)
            {
                dest[pos + 0] = bytea3;
                dest[pos + 1] = bytea2;
                dest[pos + 2] = bytea1;
                dest[pos + 3] = bytea0;
                dest[pos + 4] = byteb1;
                dest[pos + 5] = byteb0;
                dest[pos + 6] = bytec1;
                dest[pos + 7] = bytec0;
            }
            else
                Utils.UInt64ToBytesSafepos(ulonga, dest, pos);

            Utils.UInt64ToBytesSafepos(ulongb, dest, pos + 8);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsZero()
        {
            if (ulonga != 0)
                return false;
            if (ulongb != 0)
                return false;
            return  true;
        }

        /// <summary>
        /// Create a 64-bit integer representation from the second half of this UUID
        /// </summary>
        /// <returns>An integer created from the last eight bytes of this UUID</returns>
        public ulong GetULong()
        {
            if(BitConverter.IsLittleEndian)
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
        public static UUID Parse(string val)
        {
            Guid gg = Guid.Parse(val);
            return new UUID(gg);          
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
        public static bool TryParse(string val, out UUID result)
        {
            if (!string.IsNullOrEmpty(val))
            {
                try
                {
                    if (Guid.TryParse(val, out Guid gg))
                    {
                        result = new UUID(gg);
                        return true;
                    }
                }
                catch { }
            }
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
        public bool NotEqual(UUID uuid)
        {
            if (ulonga != uuid.ulonga)
                return true;
            if (ulongb != uuid.ulongb)
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
