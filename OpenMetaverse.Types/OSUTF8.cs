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

/*
    More compact Storage and manipulation of utf8 byte char strings, that .net core may have one day
    Not this is basicly a wrapper around a byte array that is shared
    Will get more things, in time.
    based on some ideas like FastStrings https://github.com/dhasenan/FastString
    Ubit Umarov (Leal Duarte) 2020
*/

using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace OpenMetaverse
{
    public class osUTF8
    {
        internal byte[] m_data;
        internal int m_len;

        public static readonly osUTF8 Empty = new osUTF8();

        public osUTF8()
        {
            m_data = new byte[0];
            m_len = 0;
        }

        public osUTF8(int capacity)
        {
            m_data = new byte[capacity];
            m_len = 0;
        }

        public osUTF8(byte[] source)
        {
            m_data = source;
            m_len = source.Length;
        }

        public osUTF8(byte[] source, int len)
        {
            m_data = source;
            m_len = len;
        }

        public osUTF8(osUTF8Slice source)
        {
            m_data = source.ToArray();
            m_len = m_data.Length;
        }

        public osUTF8(osUTF8 source)
        {
            m_data = source.ToArray();
            m_len = source.Length;
        }

        public osUTF8(string source)
        {
            m_data = Utils.StringToBytesNoTerm(source);
            m_len = m_data.Length;
        }

        public osUTF8(string source, int maxlen)
        {
            m_data = Utils.StringToBytesNoTerm(source, maxlen);
            m_len = m_data.Length;
        }

        public osUTF8(string source, bool isascii)
        {
            if(isascii)
            {
                m_len = 0;
                m_data = new byte[source.Length];
                AppendASCII(source);
            }
            else
            {
                m_data = Utils.StringToBytesNoTerm(source);
                m_len = m_data.Length;
            }
        }

        public byte this[int i]
        {
            get
            {
                if (i >= m_len)
                    i = m_len - 1;
                if (i < 0)
                    i = 0;
                else if (i >= m_data.Length)
                    i = m_data.Length - 1;
                return m_data[i];
            }
            set
            {
                if (i > 0 && i < m_len)
                    m_data[i] = value;
            }
        }

        public int Length
        {
            get { return m_len; }
        }

        public int Capacity
        {
            get { return m_data.Length; }
        }

        public unsafe override int GetHashCode()
        {
            int hash = m_len;
            for (int i = 0; i < m_data.Length; ++i)
            {
                hash += m_data[i];
                hash <<= 3;
                hash += hash >> 26;
            }
            return hash & 0x7fffffff;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
        {
            if (m_len == 0)
                return string.Empty;
            return Encoding.UTF8.GetString(m_data, 0, m_len);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj is osUTF8)
                return Equals((osUTF8)obj);

            if (obj is osUTF8Slice)
                return Equals((osUTF8Slice)obj);

            if (obj is string)
                return Equals((string)obj);

            if (obj is byte[])
                return Equals((byte[])obj);

            return false;
        }

        public unsafe bool Equals(osUTF8Slice o)
        {
            if (o is null || m_len != o.m_len)
                return false;

            byte[] otherdata = o.m_data;

            if (m_len < 8)
            {
                for (int i = 0, j = o.m_offset; i <  m_len; ++i, ++j)
                {
                    if (m_data[i] != otherdata[j])
                        return false;
                }
                return true;
            }

            fixed (byte* a = m_data, b = &otherdata[o.m_offset])
            {
                for (int i = 0; i < m_len; ++i)
                {
                    if (a[i] != b[i])
                        return false;
                }
            }

            return true;
        }

        public unsafe bool Equals(byte[] o)
        {
            if (o == null || m_len != o.Length)
                return false;

            if (m_len < 8)
            {
                for (int i = 0; i < m_len; ++i)
                {
                    if (m_data[i] != o[i])
                        return false;
                }
                return true;
            }

            fixed (byte* a = m_data, b = o)
            {
                for (int i = 0; i < m_len; ++i)
                {
                    if (a[i] != b[i])
                        return false;
                }
            }
            return true;
        }

        public unsafe bool Equals(osUTF8 o)
        {
            if (o is null || m_len != o.m_len)
                return false;

            byte[] otherdata = o.m_data;

            fixed (byte* a = m_data, b = otherdata)
            {
                byte* ptr = a;
                byte* end = a + m_len;
                byte* ptrb = b;
                while (ptr < end)
                {
                    if (*ptr != *ptrb)
                        return false;
                    ++ptr;
                    ++ptrb;
                }
            }
            return true;
        }

        public bool Equals(string s)
        {
            if (string.IsNullOrEmpty(s))
                return m_len == 0;
            osUTF8 o = new osUTF8(s);
            return Equals(o);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(osUTF8 value1, osUTF8 value2)
        {
            if (value1 is null)
                return value2 is null;
            return value1.Equals(value2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(osUTF8 value1, osUTF8 value2)
        {
            if (value1 is null)
                return !(value2 is null);
            return !value1.Equals(value2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(char c)
        {
            return m_len == 1 && m_data[0] == (byte)c;
        }

        public unsafe bool ACSIILowerEquals(osUTF8 o)
        {
            if (o is null || m_len != o.m_len)
                return false;

            fixed (byte* a = m_data, b = o.m_data)
            {
                byte* ptr = a;
                byte* end = a + m_len;
                byte* ptrb = b;
                while (ptr < end)
                {
                    byte c = *ptr;
                    if (c >= 0x41 && c <= 0x5a)
                        c |= 0x20;
                    if (c != *ptrb)
                        return false;
                    ++ptr;
                    ++ptrb;
                }
            }
            return true;
        }


        public unsafe bool ACSIILowerEquals(osUTF8Slice o)
        {
            if (o is null || m_len != o.m_len)
                return false;

            fixed (byte* a = m_data, b = o.m_data)
            {
                byte* ptr = a;
                byte* end = ptr + m_len;
                byte* ptrb = b + o.m_offset;
                while (ptr < end)
                {
                    byte c = *ptr;
                    if (c >= 0x41 && c <= 0x5a)
                        c |= 0x20;
                    if (c != *ptrb)
                        return false;
                    ++ptr;
                    ++ptrb;
                }
            }
            return true;
        }

        public unsafe bool ACSIILowerEquals(byte[] o)
        {
            if (o == null || m_len != o.Length)
                return false;

            fixed (byte* a = m_data, b = o)
            {
                byte* ptr = a;
                byte* end = a + m_len;
                byte* ptrb = b;
                while (ptr < end)
                {
                    byte c = *ptr;
                    if (c >= 0x41 && c <= 0x5a)
                        c |= 0x20;
                    if (c != *ptrb)
                        return false;
                    ++ptr;
                    ++ptrb;
                }
            }
            return true;
        }

        public unsafe bool ACSIILowerEquals(string o)
        {
            if (string.IsNullOrEmpty(o) || m_len != o.Length)
                return false;

            fixed (byte* a = m_data)
            fixed (char* b = o)
            {
                byte* ptr = a;
                byte* end = a + m_len;
                char* ptrb = b;
                while (ptr < end)
                {
                    byte c = *ptr;
                    if (c >= 0x41 && c <= 0x5a)
                        c |= 0x20;
                    if (c != (byte)*ptrb)
                        return false;
                    ++ptr;
                    ++ptrb;
                }
            }
            return true;
        }

        public void Clear()
        {
            m_len = 0;
        }

        public osUTF8 Clone()
        {
            byte[] b = new byte[m_data.Length];
            Array.Copy(m_data, 0, b, 0, m_data.Length);
            return new osUTF8(b);
        }

        public osUTF8 Extract()
        {
            byte[] b = new byte[m_len];
            Array.Copy(m_data, 0, b, 0, m_len);
            return new osUTF8(b);
        }

        public byte[] ToArray()
        {
            byte[] b = new byte[m_len];
            Array.Copy(m_data, 0, b, 0, m_len);
            return b;
        }

        public byte[] GetArray()
        {
            return m_data;
        }

        public osUTF8 Concat(osUTF8 other)
        {
            byte[] b = new byte[m_len + other.m_len];
            Array.Copy(m_data, 0, b, 0, m_len);
            Array.Copy(other.m_data, 0, b, m_len, other.m_len);
            return new osUTF8(b);
        }

        public void CheckCapacity(int needed)
        {
            int needlimit = m_len + needed;
            int cur = m_data.Length;
            if (needlimit > cur)
            {
                cur *= 2;
                if (needlimit < cur)
                    needlimit = cur;

                if ((uint)needlimit > 0x7FFFFFC7)
                    needlimit = 0x7FFFFFC7;

                byte[] b = new byte[needlimit];
                Array.Copy(m_data, 0, b, 0, m_len);

                m_data = b;
            }
        }

        public void AppendASCII(char c)
        {
            CheckCapacity(1);
            m_data[m_len] = (byte)c;
            ++m_len;
        }

        public void Append(byte c)
        {
            CheckCapacity(1);
            m_data[m_len] = c;
            ++m_len;
        }

        public unsafe void AppendASCII(string asciiString)
        {
            int nbytes = asciiString.Length;
            CheckCapacity(nbytes);
            fixed (byte* bdst = &m_data[m_len])
            {
                fixed (char* bsrc = asciiString)
                {
                    char* src = bsrc;
                    char* scrend = bsrc + nbytes;
                    byte* dst = bdst;
                    while (src < scrend)
                    {
                        *dst = (byte)*src;
                        ++src;
                        ++dst;
                    }
                }
            }
            m_len += nbytes;
        }

        public void Append(string s)
        {
            int indx = m_len;
            int srcindx = 0;

            CheckCapacity(s.Length);
            while (!Utils.osUTF8TryGetbytes(s, ref srcindx, m_data, ref indx))
            {
                m_len = indx;
                CheckCapacity(s.Length - srcindx + 256);
            }

            m_len = indx;
        }

        public void Append(byte[] b)
        {
            int nbytes = b.Length;
            CheckCapacity(nbytes);
            Array.Copy(b, 0, m_data, m_len, nbytes);
            m_len += nbytes;
        }

        public void Append(osUTF8 b)
        {
            int nbytes = b.m_len;
            CheckCapacity(nbytes);
            Array.Copy(b.m_data, 0, m_data, m_len, nbytes);
            m_len += nbytes;
        }

        public unsafe void AppendInt(sbyte v)
        {
            CheckCapacity(4);
            fixed (byte* d = m_data)
                m_len += Utils.IntToByteString(v, d + m_len);
        }

        public unsafe void AppendInt(byte v)
        {
            CheckCapacity(4);
            fixed (byte* d = m_data)
                m_len += Utils.UIntToByteString(v, d + m_len);
        }
        public unsafe void AppendInt(int v)
        {
            CheckCapacity(16);
            fixed (byte* d = m_data)
                m_len += Utils.IntToByteString(v, d + m_len);
        }

        public unsafe void AppendInt(uint v)
        {
            CheckCapacity(16);
            fixed (byte* d = m_data)
                m_len += Utils.UIntToByteString(v, d + m_len);
        }

        public unsafe void AppendInt(long v)
        {
            CheckCapacity(32);
            fixed (byte* d = m_data)
                m_len += Utils.LongToByteString(v, d + m_len);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void AppendInt(ulong v)
        {
            CheckCapacity(32);
            fixed (byte* d = m_data)
                m_len += Utils.ULongToByteString(v, d + m_len);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void AppendUUID(UUID u)
        {
            CheckCapacity(36);
            fixed (byte* d = m_data)
                Utils.UUIDToByteDashString(u, d + m_len);
            m_len += 36;
        }

        public unsafe void ToASCIILowerSelf()
        {
            if (m_len == 0)
                return;
            fixed (byte* baseptr = m_data)
            {
                byte* ptr = baseptr;
                byte* end = ptr + m_len;
                while (ptr < end)
                {
                    if (*ptr >= 0x41 && *ptr <= 0x5a)
                        *ptr |= 0x20;
                    ++ptr;
                }
            }
        }

        public unsafe void ToASCIIUpperSelf()
        {
            if (m_len == 0)
                return;
            fixed (byte* baseptr = m_data)
            {
                byte* ptr = baseptr;
                byte* end = ptr + m_len;
                while (ptr < end)
                {
                    if (*ptr >= 0x61 && *ptr <= 0x7a)
                        *ptr &= 0xdf;
                    ++ptr;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public osUTF8Slice SubUTF8(int start)
        {
            return SubUTF8(start, m_len - start);
        }

        //returns a segment view of main buffer
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public osUTF8Slice SubUTF8(int start, int len)
        {
            osUTF8Slice oss = new osUTF8Slice(this);
            return oss.SubUTF8(start, len);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string SubString(int start)
        {
            return SubString(start, m_len - start);
        }

        //returns a segment view of main buffer
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string SubString(int start, int len)
        {
            osUTF8Slice oss = new osUTF8Slice(this);
            return oss.SubString(start, len);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public osUTF8Slice TrimStart()
        {
            osUTF8Slice ret = new osUTF8Slice(this);
            return ret.TrimStart();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public osUTF8Slice TrimEnd()
        {
            osUTF8Slice ret = new osUTF8Slice(this);
            return ret.TrimEnd();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public osUTF8Slice Trim()
        {
            osUTF8Slice ret = new osUTF8Slice(this);
            return ret.Trim();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public osUTF8Slice TrimStart(byte b)
        {
            osUTF8Slice ret = new osUTF8Slice(this);
            return ret.TrimStart(b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public osUTF8Slice TrimEnd(byte b)
        {
            osUTF8Slice ret = new osUTF8Slice(this);
            return ret.TrimEnd(b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public osUTF8Slice Trim(byte b)
        {
            osUTF8Slice ret = new osUTF8Slice(this);
            return ret.Trim(b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public osUTF8Slice TrimStart(byte[] v)
        {
            osUTF8Slice ret = new osUTF8Slice(this);
            return ret.TrimStart(v);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public osUTF8Slice TrimEnd(byte[] v)
        {
            osUTF8Slice ret = new osUTF8Slice(this);
            return ret.TrimEnd(v);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public osUTF8Slice Trim(byte[] v)
        {
            osUTF8Slice ret = new osUTF8Slice(this);
            return ret.Trim(v);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public osUTF8Slice TrimStart(char[] v)
        {
            osUTF8Slice ret = new osUTF8Slice(this);
            return ret.TrimStart(v);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public osUTF8Slice TrimEnd(char[] v)
        {
            osUTF8Slice ret = new osUTF8Slice(this);
            return ret.TrimEnd(v);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public osUTF8Slice Trim(char[] v)
        {
            osUTF8Slice ret = new osUTF8Slice(this);
            return ret.Trim(v);
        }

        public unsafe bool StartsWith(osUTF8 other)
        {
            int otherlen = other.m_len;
            if (otherlen > m_len)
                return false;

            fixed (byte* a = m_data, b = other.m_data)
            {
                for (int i = 0; i < otherlen; ++i)
                {
                    if (a[i] != b[i])
                        return false;
                }
            }
            return true;
        }

        public bool StartsWith(string s)
        {
            osUTF8 other = new osUTF8(s); // yeack
            return StartsWith(other);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool StartsWith(byte b)
        {
            return m_data[0] == b;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool StartsWith(char b)
        {
            return m_data[0] == (byte)b;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool EndsWith(byte b)
        {
            return m_data[m_len - 1] == b;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool EndsWith(char b)
        {
            return m_data[m_len - 1] == (byte)b;
        }

        public unsafe bool EndsWith(osUTF8 other)
        {
            int otherlen = other.m_len;
            if (otherlen > m_len)
                return false;

            fixed (byte* a = m_data, b = other.m_data)
            {
                for (int i = otherlen - 1, j = m_len - 1; i >= 0; --i, --j)
                {
                    if (a[j] != b[i])
                        return false;
                }
                return true;
            }
        }

        public bool EndsWith(string s)
        {
            osUTF8 other = new osUTF8(s); // yeack
            return EndsWith(other);
        }

        public unsafe int IndexOf(byte b)
        {
            if (m_len > 8)
            {
                fixed (byte* a = m_data)
                {
                    for (int i = 0; i < m_len; ++i)
                    {
                        if (a[i] == b)
                            return i;
                    }
                    return -1;
                }
            }

            for (int i = 0; i < m_len; ++i)
            {
                if (m_data[i] == b)
                    return i;
            }
            return -1;
        }

        public int IndexOf(char b)
        {
            if (b < 0x80)
                return IndexOf((byte)b);
            string s = new string(new char[] { b });
            return IndexOf(s);
        }

        public unsafe int IndexOf(osUTF8 other)
        {
            int otherlen = other.m_len;
            if (otherlen > m_len || otherlen == 0)
                return -1;

            byte[] otherdata = other.m_data;
            fixed (byte* a = m_data, b = otherdata)
            {
                for (int i = 0; i < m_len - otherlen; ++i)
                {
                    int k = 0;
                    for (int j = i; k < otherlen; ++k, ++j)
                    {
                        if (a[j] != b[k])
                           break;
                    }
                    if (k == otherlen)
                        return i;
                }
                return -1;
            }
        }

        public unsafe int IndexOf(byte[] other)
        {
            if (other == null)
                return -1;
            int otherlen = other.Length;
            if (otherlen > m_len || otherlen == 0)
                return -1;

            fixed (byte* a = m_data, b = other)
            {
                for (int i = 0; i < m_len - otherlen; ++i)
                {
                    int k = 0;
                    for (int j = i; k < otherlen; ++k, ++j)
                    {
                        if (a[j] != b[k])
                            break;
                    }
                    if (k == otherlen)
                        return i;
                }
                return -1;
            }
        }
        public int IndexOf(string s)
        {
            if (string.IsNullOrEmpty(s))
                return -1;
            osUTF8 o = new osUTF8(s);
            return IndexOf(o);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool checkAny(byte b, byte[] bytes)
        {
            for (int i = 0; i < bytes.Length; ++i)
            {
                if (b == bytes[i])
                    return true;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool checkAny(byte b, char[] chars)
        {
            for (int i = 0; i < chars.Length; ++i)
            {
                if (b == (byte)chars[i])
                    return true;
            }
            return false;
        }

        public unsafe int IndexOfAny(byte[] b)
        {
            if (m_len < 8)
            {
                for (int i = 0; i < m_len; ++i)
                {
                    if (checkAny(m_data[i], b))
                        return i;
                }
                return -1;
            }
            fixed (byte* a = m_data)
            {
                for (int i = 0; i < m_len; ++i)
                {
                    if (checkAny(a[i], b))
                        return i;
                }
                return -1;
            }
        }

        public unsafe int IndexOfAny(char[] b)
        {
            if (m_len < 8)
            {
                for (int i = 0; i <m_len; ++i)
                {
                    if (checkAny(m_data[i], b))
                        return i;
                }
                return -1;
            }
            fixed (byte* a = m_data)
            {
                for (int i = 0; i < m_len; ++i)
                {
                    if (checkAny(a[i], b))
                        return i;
                }
                return -1;
            }
        }

        public bool Contains(osUTF8 other)
        {
            return IndexOf(other) > 0;
        }

        public bool Contains(string s)
        {
            return IndexOf(s) > 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public osUTF8Slice[] Split(byte b, bool ignoreEmpty = true)
        {
            return new osUTF8Slice(this).Split(b, ignoreEmpty);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public osUTF8Slice[] Split(byte[] b, bool ignoreEmpty = true)
        {
            return new osUTF8Slice(this).Split(b, ignoreEmpty);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public osUTF8Slice[] Split(char[] b, bool ignoreEmpty = true)
        {
            return new osUTF8Slice(this).Split(b, ignoreEmpty);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public osUTF8Slice[] Split(char b, bool ignoreEmpty = true)
        {
            if (b < 0x80)
                return Split((byte)b, ignoreEmpty);

            return new osUTF8Slice[0];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryParseInt(osUTF8 t, out int res)
        {
            return osUTF8Slice.TryParseInt(new osUTF8Slice(t), out res);
        }

        public unsafe static bool TryParseUUID(osUTF8 inp, out UUID result)
        {
            result = new UUID();
            int len = inp.m_len;
            if (len < 32)
                return false;

            try
            {
                fixed (byte* bval = inp.m_data)
                {
                    byte* val = bval;

                    while (*val == ' ')
                    {
                        ++val;
                        --len;
                        if (len < 32)
                            return false;
                    }

                    if (val[8] == '-')
                    {
                        if (len < 36)
                            return false;
                        if (val[13] != '-' || val[18] != '-' || val[23] != '-')
                            return false;

                        if (Sse42.IsSupported)
                        {
                            Vector128<byte> input = Unsafe.As<byte, Vector128<byte>>(ref Unsafe.AsRef<byte>(val));
                            Vector128<byte> upper = Ssse3.Shuffle(input, Vector128.Create(0, 2, 4, 6, 9, 11, 14, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff));
                            Vector128<byte> lower = Ssse3.Shuffle(input, Vector128.Create(1, 3, 5, 7, 10, 12, 15, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff));
                            input = Unsafe.As<byte, Vector128<byte>>(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(val), 16 + 3));
                            Vector128<byte> upperhalf = Ssse3.Shuffle(input, Vector128.Create(0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0, 2, 5, 7, 9, 11, 13, 15));
                            Vector128<byte> lowerhalf = Ssse3.Shuffle(input, Vector128.Create(0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 1, 3, 6, 8, 10, 12, 14, 0xff));
                            upper = Sse2.Or(upper, upperhalf);
                            lower = Sse2.Or(lower, lowerhalf);

                            upper = Sse41.Insert(upper, Unsafe.As<byte, byte>(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(val), 16)), 7);
                            lower = Sse41.Insert(lower, Unsafe.As<byte, byte>(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(val), 17)), 7);
                            lower = Sse41.Insert(lower, Unsafe.As<byte, byte>(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(val), 35)), 15);

                            Vector128<byte> charf = Vector128.Create((byte)'f');
                            Vector128<byte> tmpcmp = Sse2.Subtract(charf, lower);
                            int cmp = Sse2.MoveMask(tmpcmp);
                            if (cmp != 0)
                                throw new Exception("bad");

                            tmpcmp = Sse2.Subtract(charf, upper);
                            cmp = Sse2.MoveMask(tmpcmp);
                            if (cmp != 0)
                                throw new Exception("bad");

                            Vector128<byte> charTolower = Vector128.Create((byte)0x20);
                            Vector128<byte> lowerLetters = Sse2.Or(lower, charTolower);
                            Vector128<byte> upperLetters = Sse2.Or(upper, charTolower);

                            Vector128<byte> letterTohex = Vector128.Create((byte)('a' - '0' - 10));
                            lowerLetters = Sse2.Subtract(lowerLetters, letterTohex);
                            upperLetters = Sse2.Subtract(upperLetters, letterTohex);

                            Vector128<byte> char9 = Vector128.Create((byte)'9');
                            Vector128<byte> above9lower = (Sse2.CompareGreaterThan(lower.AsSByte(), char9.AsSByte())).AsByte();

                            Vector128<byte> ten = Vector128.Create((byte)('0' + 10));
                            tmpcmp = Sse2.Subtract(lowerLetters, ten);
                            tmpcmp = Sse2.And(tmpcmp, above9lower);
                            cmp = Sse2.MoveMask(tmpcmp);
                            if (cmp != 0)
                                throw new Exception("bad");
                            Vector128<byte> above9upper = (Sse2.CompareGreaterThan(upper.AsSByte(), char9.AsSByte())).AsByte();

                            tmpcmp = Sse2.Subtract(upperLetters, ten);
                            tmpcmp = Sse2.And(tmpcmp, above9upper);
                            cmp = Sse2.MoveMask(tmpcmp);
                            if (cmp != 0)
                                throw new Exception("bad");

                            lower = Sse41.BlendVariable(lower, lowerLetters, above9lower);
                            upper = Sse41.BlendVariable(upper, upperLetters, above9upper);
                            Vector128<byte> charzero = Vector128.Create((byte)'0');
                            lower = Sse2.Subtract(lower, charzero);
                            cmp = Sse2.MoveMask(lower);
                            if (cmp != 0)
                                throw new Exception("bad");
                            upper = Sse2.Subtract(upper, charzero);
                            cmp = Sse2.MoveMask(upper);
                            if (cmp != 0)
                                throw new Exception("bad");
                            upper = Sse2.ShiftLeftLogical(upper.AsUInt16(), 4).AsByte();
                            lower = Sse2.Or(lower, upper);
                            if (BitConverter.IsLittleEndian)
                                lower = Ssse3.Shuffle(lower, Vector128.Create((byte)0x03, 0x02, 0x01, 0x00, 0x05, 0x04, 0x07, 0x06, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F));
                            Unsafe.As<UUID, Vector128<byte>>(ref Unsafe.AsRef(in result)) = lower;
                            return true;
                        }

                        if (BitConverter.IsLittleEndian)
                        {
                            result.bytea3 = Utils.HexToByte(val, 0);
                            result.bytea2 = Utils.HexToByte(val, 2);
                            result.bytea1 = Utils.HexToByte(val, 4);
                            result.bytea0 = Utils.HexToByte(val, 6);

                            result.byteb1 = Utils.HexToByte(val, 9);
                            result.byteb0 = Utils.HexToByte(val, 11);

                            result.bytec1 = Utils.HexToByte(val, 14);
                            result.bytec0 = Utils.HexToByte(val, 16);
                        }
                        else
                        {
                            result.bytea0 = Utils.HexToByte(val, 0);
                            result.bytea1 = Utils.HexToByte(val, 2);
                            result.bytea2 = Utils.HexToByte(val, 4);
                            result.bytea3 = Utils.HexToByte(val, 6);

                            result.byteb0 = Utils.HexToByte(val, 9);
                            result.byteb1 = Utils.HexToByte(val, 11);

                            result.bytec0 = Utils.HexToByte(val, 14);
                            result.bytec1 = Utils.HexToByte(val, 16);
                        }

                        result.d = Utils.HexToByte(val, 19);
                        result.e = Utils.HexToByte(val, 21);


                        result.f = Utils.HexToByte(val, 24);
                        result.g = Utils.HexToByte(val, 26);
                        result.h = Utils.HexToByte(val, 28);
                        result.i = Utils.HexToByte(val, 30);
                        result.j = Utils.HexToByte(val, 32);
                        result.k = Utils.HexToByte(val, 34);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AppendCharBytes(char c, ref string src, ref int indx)
        {
            if (c <= 0x7f)
            {
                AppendASCII(c);
                return;
            }

            if (c < 0x800)
            {
                Append((byte)(0xC0 | (c >> 6)));
                Append((byte)(0x80 | (c & 0x3F)));
                return;
            }

            if (c >= 0xD800 && c < 0xE000)
            {
                if (c >= 0xDC00)
                    return;

                if (indx + 1 >= src.Length)
                    return;

                int a = c;

                ++indx;
                c = src[indx];
                if (c < 0xDC00 || c > 0xDFFF)
                    return; // ignore invalid

                a = (a << 10) + c - 0x35fdc00;

                Append((byte)(0xF0 | (a >> 18)));
                Append((byte)(0x80 | ((a >> 12) & 0x3f)));
                Append((byte)(0x80 | ((a >> 6) & 0x3f)));
                Append((byte)(0x80 | (a & 0x3f)));
                return;
            }

            Append((byte)(0xE0 | (c >> 12)));
            Append((byte)(0x80 | ((c >> 6) & 0x3f)));
            Append((byte)(0x80 | (c & 0x3f)));
        }

        public static byte[] GetASCIIBytes(string s)
        {
            byte[] b = new byte[s.Length];
            for(int i = 0; i < s.Length; ++i)
                b[i] = (byte)s[i];
            return b;
        }

        public static bool IsNullOrEmpty(osUTF8 u)
        {
            return (u is null || u.m_len == 0);
        }

        public static bool IsEmpty(osUTF8 u)
        {
            return (u is null || u.m_len == 0);
        }

        public static unsafe bool IsNullOrWhitespace(osUTF8 u)
        {
            if(u is null || u.m_len == 0)
                return true;
            byte[] data = u.m_data;
            for (int i = 0; i < u.m_len; ++i)
                {
                    if (data[i] != 0x20)
                        return false;
                }
            return true;
        }

        public void RemoveBytesSelf(int start, int len)
        {
            if (start < 0)
                throw new ArgumentOutOfRangeException("startIndex", "ArgumentOutOfRange_StartIndex");

            if(start >= m_len)
                return;

            if (len < 0)
            {
                m_len = start;
                return;
            }

            int end = start + len;
            if (end >= m_len)
            {
                m_len = start;
                return;
            }
            Array.Copy(m_data, end, m_data, start, m_len - end);
            m_len -=  len;
        }

        public osUTF8 RemoveBytes(int start, int len)
        {
            if (start < 0)
                throw new ArgumentOutOfRangeException("startIndex", "ArgumentOutOfRange_StartIndex");

            if (start >= m_len)
                return Clone();

            osUTF8 o = new osUTF8(m_len);
            Array.Copy(m_data, 0, o.m_data, 0, start);
            o.m_len = start;

            if (len < 0)
                return o;

            int end = start + len;
            if (end >= m_len)
                return o;

            Array.Copy(m_data, end, o.m_data, start, m_len - end);
            o.m_len = m_len - len;
            return o;
        }
    }
}
