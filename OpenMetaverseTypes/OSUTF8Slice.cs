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
    public class osUTF8Slice
    {
        internal byte[] m_data;
        internal int m_offset;
        internal int m_len;

        public static readonly osUTF8Slice Empty = new osUTF8Slice();

        public osUTF8Slice()
        {
            m_data = new byte[0];
            m_offset = 0;
            m_len = 0;
        }

        public osUTF8Slice(int capacity)
        {
            m_data = new byte[capacity];
            m_offset = 0;
            m_len = 0;
        }

        public osUTF8Slice(byte[] source)
        {
            m_data = source;
            m_offset = 0;
            m_len = source.Length;
        }

        public osUTF8Slice(byte[] source, int offset, int len)
        {
            m_data = source;
            m_offset = offset;
            m_len = len;
        }

        public osUTF8Slice(osUTF8Slice source)
        {
            m_data = source.m_data;
            m_offset = source.m_offset;
            m_len = source.m_len;
        }

        public osUTF8Slice(osUTF8 source)
        {
            m_data = source.m_data;
            m_offset = 0;
            m_len = source.Length;
        }

        public osUTF8Slice(string source)
        {
            m_data = Utils.StringToBytesNoTerm(source);
            m_offset = 0;
            m_len = m_data.Length;
        }

        public osUTF8Slice(string source, int maxlen)
        {
            m_data = Utils.StringToBytesNoTerm(source, maxlen);
            m_len = m_data.Length;
            m_offset = 0;
        }

        public osUTF8Slice(string source, bool isascii)
        {
            m_offset = 0;
            m_len = 0;
            if (isascii)
            {
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
                i += m_offset;
                if (i < 0)
                    i = 0;
                else if (i >= m_data.Length)
                    i = m_data.Length - 1;
                return m_data[i];
            }
            set
            {
                if (i > 0)
                {
                    i += m_offset;
                    if(i < m_len)
                        m_data[i] = value;
                }
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

        public int StartOffset
        {
            get { return m_offset; }
        }

        public void MoveStart(int of)
        {
            m_len -= of;
            m_offset += of;

            if (m_offset < 0)
            {
                m_len -= m_offset;
                m_offset = 0;
            }
            else if (m_offset >= m_data.Length)
            {
                m_offset = m_data.Length - 1;
                m_len = 0;
            }
            else if (m_offset + m_len > m_data.Length)
                m_len = m_data.Length - m_offset;
        }

        public void FromBytesSafe(byte[] source, int offset, int len)
        {
            m_data = source;
            m_offset = offset;
            m_len = len;
        }

        public void FromBytes(byte[] source, int offset, int len)
        {
            if (source == null || offset >= source.Length || offset + len >= source.Length)
            {
                m_offset = 0;
                m_len = 0;
            }
            if (len < 0)
                len = 0;
            m_data = source;
            m_offset = offset;
            m_len = len;
        }

        public static bool IsNullOrEmpty(osUTF8Slice u)
        {
            return (u == null || u.m_len == 0);
        }

        public static bool IsEmpty(osUTF8Slice u)
        {
            return (u == null || u.m_len == 0);
        }

        public static unsafe bool IsNullOrWhitespace(osUTF8Slice u)
        {
            if (u == null || u.m_len == 0)
                return true;
            byte[] data = u.m_data;
            for (int i = u.m_offset; i < u.m_offset + u.m_len; ++i)
            {
                if (data[i] != 0x20)
                    return false;
            }
            return true;
        }

        public unsafe override int GetHashCode()
        {
            int hash = m_len;
            if (m_len < 8)
            {
                for (int i = m_offset; i < m_offset + m_len; ++i)
                {
                    hash += m_data[i];
                    hash <<= 3;
                    hash += hash >> 26;
                }
            }
            else
            {
                fixed (byte* a = &m_data[m_offset])
                {
                    for (int i = 0; i < m_len; ++i)
                    {
                        hash += a[i];
                        hash <<= 5;
                        hash += hash >> 26;
                    }
                }
            }
            return hash & 0x7fffffff;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
        {
            if(m_len == 0)
                return string.Empty;
            return Encoding.UTF8.GetString(m_data, m_offset, m_len);
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
            if (o == null || m_len != o.m_len)
                return false;

            byte[] otherdata = o.m_data;

            if (m_len < 8)
            {
                for (int i = m_offset, j = o.m_offset; i < m_offset + m_len; ++i, ++j)
                {
                    if (m_data[i] != otherdata[j])
                        return false;
                }
                return true;
            }

            fixed (byte* a = &m_data[m_offset], b = &otherdata[o.m_offset])
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
            if (o == null || m_len != o.m_len)
                return false;

            byte[] otherdata = o.m_data;

            if (m_len < 8)
            {
                for (int i = m_offset, j = 0; i < m_offset + m_len; ++i, ++j)
                {
                    if (m_data[i] != otherdata[j])
                        return false;
                }
                return true;
            }

            fixed (byte* a = &m_data[m_offset], b = otherdata)
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
                for (int i = m_offset, j = 0; i < m_offset + m_len; ++i, ++j)
                {
                    if (m_data[i] != o[j])
                        return false;
                }
                return true;
            }

            fixed (byte* a = &m_data[m_offset], b = o)
            {
                for (int i = 0; i < m_len; ++i)
                {
                    if (a[i] != b[i])
                        return false;
                }
            }
            return true;
        }

        public bool Equals(string s)
        {
            if(string.IsNullOrEmpty(s))
                return m_len == 0;
            osUTF8 o = new osUTF8(s);
            return Equals(o);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(char c)
        {
            return m_len == 1 && m_data[m_offset] == (byte)c;
        }

        public unsafe bool ACSIILowerEquals(osUTF8 o)
        {
            if (o == null || m_len != o.m_len)
                return false;

            fixed (byte* a = &m_data[m_offset], b = o.m_data)
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

        public unsafe bool ACSIILowerEquals(byte[] o)
        {
            if (o == null || m_len != o.Length)
                return false;

            fixed (byte* a = &m_data[m_offset], b = o)
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
            if (string.IsNullOrEmpty(o))
                return false;

            int olen = o.Length;
            if (m_len != olen)
                return false;

            fixed (byte* a = &m_data[m_offset])
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
            m_offset = 0;
            m_len = 0;
        }

        public osUTF8Slice Clone()
        {
            byte[] b = new byte[m_data.Length];
            Array.Copy(m_data, 0, b, 0, m_data.Length);
            return new osUTF8Slice(b, m_offset, m_len);
        }

        public osUTF8 Extract()
        {
            byte[] b = new byte[m_len];
            Array.Copy(m_data, m_offset, b, 0, m_len);
            return new osUTF8(b);
        }

        public byte[] ToArray()
        {
            byte[] b = new byte[m_len];
            Array.Copy(m_data, m_offset, b, 0, m_len);
            return b;
        }

        public osUTF8Slice Concat(osUTF8Slice other)
        {
            byte[] b = new byte[m_len + other.m_len];
            Array.Copy(m_data, m_offset, b, 0, m_len);
            Array.Copy(other.m_data, other.m_offset, b, m_len, other.m_len);
            return new osUTF8Slice(b, 0, m_len + other.m_len);
        }

        public osUTF8Slice Concat(osUTF8 other)
        {
            byte[] b = new byte[m_len + other.m_len];
            Array.Copy(m_data, m_offset, b, 0, m_len);
            Array.Copy(other.m_data, 0, b, m_len, other.m_len);
            return new osUTF8Slice(b, 0, m_len + other.m_len);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void CheckCapacity(ref int curindx, int needed)
        {
            int needlimit = curindx + needed;
            int cur = m_data.Length;
            if (needlimit > cur)
            {
                cur *= 2;
                if (needlimit < cur)
                    needlimit = cur;

                if ((uint)needlimit > 0x7FFFFFC7)
                    needlimit = 0x7FFFFFC7;

                byte[] b = new byte[needlimit];
                Array.Copy(m_data, m_offset, b, 0, m_len);

                curindx -= m_offset;
                m_offset = 0;
                m_data = b;
            }
        }

        public void AppendASCII(char c)
        {
            int indx = m_offset + m_len;
            CheckCapacity(ref indx, 1);
            m_data[indx] = (byte)c;
            ++m_len;
        }

        public unsafe void AppendASCII(string asciiString)
        {
            int indx = m_offset + m_len;
            int nbytes = asciiString.Length;
            CheckCapacity(ref indx, nbytes);
            fixed (byte* bdst = &m_data[indx])
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
            int indx = m_offset + m_len;
            int srcindx = 0;

            CheckCapacity(ref indx, s.Length);
            while (!Utils.osUTF8TryGetbytes(s, ref srcindx, m_data, ref indx))
            {
                m_len = indx - m_offset;
                CheckCapacity(ref indx, s.Length - srcindx + 256);
            }

            m_len = indx - m_offset;
        }

        public void Append(byte[] b)
        {
            int indx = m_offset + m_len;
            int nbytes = b.Length;
            CheckCapacity(ref indx, nbytes);
            Array.Copy(b, 0, m_data, indx, nbytes);
            m_len += nbytes;
        }

        public void Append(osUTF8Slice b)
        {
            int indx = m_offset + m_len;
            int nbytes = b.m_len;
            CheckCapacity(ref indx, nbytes);
            Array.Copy(b.m_data, b.m_offset, m_data, indx, nbytes);
            m_len += nbytes;
        }

        public void Append(osUTF8 b)
        {
            int indx = m_offset + m_len;
            int nbytes = b.m_len;
            CheckCapacity(ref indx, nbytes);
            Array.Copy(b.m_data, 0, m_data, indx, nbytes);
            m_len += nbytes;
        }

        public unsafe void ToASCIILowerSelf()
        {
            if (m_len == 0)
                return;
            fixed (byte* baseptr = m_data)
            {
                byte* ptr = baseptr + m_offset;
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
                byte* ptr = baseptr + m_offset;
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
        public osUTF8Slice SubUTF8(int start, int len)
        {
            if (start < 0)
                start = 0;
            if (len > m_len)
                len = m_len;

            start += m_offset; // things are relative to current

            if (start >= m_data.Length)
                return new osUTF8Slice(m_data, m_data.Length - 1, 0);

            int last = start + len - 1;

            // cut at code points;
            if (start > 0 && (m_data[start] & 0x80) != 0)
            {
                do
                {
                    --last;
                }
                while (start > 0 && (m_data[start] & 0xc0) != 0xc0);
            }

            if (last > start && (m_data[last] & 0x80) != 0)
            {
                do
                {
                    --last;
                }
                while (last > start && (m_data[last] & 0xc0) != 0xc0);
            }

            return new osUTF8Slice(m_data, start, last - start + 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SubUTF8Self(int start)
        {
            SubUTF8Self(start, m_len - start);
        }

        //returns a segment view of main buffer
        public void SubUTF8Self(int start, int len)
        {
            if (m_len == 0)
                return;

            if (start < 0)
                start = 0;
            else if (start >= m_len)
            {
                m_offset += m_len - 1;
                m_len = 0;
                return;
            }

            if (len == 0)
            {
                m_len = 0;
                return;
            }

            if (len > m_len)
                len = m_len;

            start += m_offset; // things are relative to current

            if (start >= m_data.Length)
            {
                m_offset = m_data.Length - 1;
                m_len = 0;
                return;
            }

            int last = start + len - 1;
            // cut at code points;
            if (start > 0 && (m_data[start] & 0x80) != 0)
            {
                do
                {
                    --last;
                }
                while (start > 0 && (m_data[start] & 0xc0) != 0xc0);
            }

            if (last > start && (m_data[last] & 0x80) != 0)
            {
                do
                {
                    --last;
                }
                while (last > start && (m_data[last] & 0xc0) != 0xc0);
            }

            m_offset = start;
            m_len = last - start + 1;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string SubString(int start)
        {
            return SubString(start, m_len - start);
        }

        //returns a segment view of main buffer
        public string SubString(int start, int len)
        {
            osUTF8Slice res = SubUTF8(start, len);
            return res.ToString();
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

        // inplace remove white spaces at start
        public void SelfTrimStart()
        {
            while (m_len > 0 && m_data[m_offset] == 0x20)
            {
                ++m_offset;
                --m_len;
            }
        }

        public void SelfTrimStart(byte b)
        {
            while (m_len > 0 && m_data[m_offset] == b)
            {
                ++m_offset;
                --m_len;
            }
        }

        public void SelfTrimStart(byte[] b)
        {
            while (m_len > 0 && checkAny(m_data[m_offset], b))
            {
                ++m_offset;
                --m_len;
            }
        }

        public void SelfTrimStart(char[] b)
        {
            while (m_len > 0 && checkAny(m_data[m_offset], b))
            {
                ++m_offset;
                --m_len;
            }
        }

        public void SelfTrimEnd()
        {
            if (m_len == 0)
                return;
            int last = m_offset + m_len - 1;
            while (m_len > 0 && m_data[last] == 0x20)
            {
                --last;
                --m_len;
            }
        }

        public void SelfTrimEnd(byte b)
        {
            if (m_len == 0)
                return;
            int last = m_offset + m_len - 1;
            while (m_len > 0 && m_data[last] == b)
            {
                --last;
                --m_len;
            }
        }

        public void SelfTrimEnd(byte[] b)
        {
            if (m_len == 0)
                return;
            int last = m_offset + m_len - 1;
            while (m_len > 0 && checkAny(m_data[last], b))
            {
                --last;
                --m_len;
            }
        }

        public void SelfTrimEnd(char[] b)
        {
            if (m_len == 0)
                return;
            int last = m_offset + m_len - 1;
            while (m_len > 0 && checkAny(m_data[last], b))
            {
                --last;
                --m_len;
            }
        }

        public void SelfTrim()
        {
            SelfTrimStart();
            SelfTrimEnd();
        }

        public void SelfTrim(byte b)
        {
            SelfTrimStart(b);
            SelfTrimEnd(b);
        }

        public void SelfTrim(byte[] v)
        {
            SelfTrimStart(v);
            SelfTrimEnd(v);
        }

        public void SelfTrim(char[] v)
        {
            SelfTrimStart(v);
            SelfTrimEnd(v);
        }

        public osUTF8Slice TrimStart()
        {
            osUTF8Slice ret = new osUTF8Slice(this);
            ret.SelfTrimStart();
            return ret;
        }

        public osUTF8Slice TrimEnd()
        {
            osUTF8Slice ret = new osUTF8Slice(this);
            ret.SelfTrimEnd();
            return ret;
        }

        public osUTF8Slice Trim()
        {
            osUTF8Slice ret = new osUTF8Slice(this);
            ret.SelfTrimStart();
            ret.SelfTrimEnd();
            return ret;
        }

        public osUTF8Slice TrimStart(byte b)
        {
            osUTF8Slice ret = new osUTF8Slice(this);
            ret.SelfTrimStart(b);
            return ret;
        }

        public osUTF8Slice TrimEnd(byte b)
        {
            osUTF8Slice ret = new osUTF8Slice(this);
            ret.SelfTrimEnd(b);
            return ret;
        }

        public osUTF8Slice Trim(byte b)
        {
            osUTF8Slice ret = new osUTF8Slice(this);
            ret.SelfTrimStart(b);
            ret.SelfTrimEnd(b);
            return ret;
        }

        public osUTF8Slice TrimStart(byte[] v)
        {
            osUTF8Slice ret = new osUTF8Slice(this);
            ret.SelfTrimStart(v);
            return ret;
        }

        public osUTF8Slice TrimEnd(byte[] v)
        {
            osUTF8Slice ret = new osUTF8Slice(this);
            ret.SelfTrimEnd(v);
            return ret;
        }

        public osUTF8Slice Trim(byte[] v)
        {
            osUTF8Slice ret = new osUTF8Slice(this);
            ret.SelfTrimStart(v);
            ret.SelfTrimEnd(v);
            return ret;
        }

        public osUTF8Slice TrimStart(char[] v)
        {
            osUTF8Slice ret = new osUTF8Slice(this);
            ret.SelfTrimStart(v);
            return ret;
        }

        public osUTF8Slice TrimEnd(char[] v)
        {
            osUTF8Slice ret = new osUTF8Slice(this);
            ret.SelfTrimEnd(v);
            return ret;
        }

        public osUTF8Slice Trim(char[] v)
        {
            osUTF8Slice ret = new osUTF8Slice(this);
            ret.SelfTrimStart(v);
            ret.SelfTrimEnd(v);
            return ret;
        }

        public unsafe bool StartsWith(osUTF8Slice other)
        {
            int otherlen = other.m_len;
            if (otherlen > m_len)
                return false;

            fixed (byte* a = &m_data[m_offset], b = &other.m_data[other.m_offset])
            {
                for (int i = 0; i < otherlen; ++i)
                {
                    if (a[i] != b[i])
                        return false;
                }
            }
            return true;
        }

        public unsafe bool StartsWith(osUTF8 other)
        {
            int otherlen = other.m_len;
            if (otherlen > m_len)
                return false;

            fixed (byte* a = &m_data[m_offset], b = other.m_data)
            {
                for (int i = 0; i < otherlen; ++i)
                {
                    if (a[i] != b[i])
                        return false;
                }
            }
            return true;
        }

        public unsafe bool StartsWith(byte[] other)
        {
            int otherlen = other.Length;
            if (otherlen > m_len)
                return false;

            fixed (byte* a = &m_data[m_offset], b = other)
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
            osUTF8Slice other = new osUTF8Slice(s); // yeack
            return StartsWith(other);
        }

        public bool StartsWith(byte b)
        {
            return m_data[m_offset] == b;
        }

        public bool StartsWith(char b)
        {
            return m_data[m_offset] == (byte)b;
        }

        public bool EndsWith(byte b)
        {
            return m_data[m_offset + m_len - 1] == b;
        }

        public bool EndsWith(char b)
        {
            return m_data[m_offset + m_len - 1] == (byte)b;
        }

        public unsafe bool EndsWith(osUTF8Slice other)
        {
            int otherlen = other.m_len;
            if (otherlen > m_len)
                return false;

            fixed (byte* a = &m_data[m_offset], b = &other.m_data[other.m_offset])
            {
                for (int i = otherlen - 1, j = m_len - 1; i >= 0; --i, --j)
                {
                    if (a[j] != b[i])
                        return false;
                }
                return true;
            }
        }

        public unsafe bool EndsWith(osUTF8 other)
        {
            int otherlen = other.m_len;
            if (otherlen > m_len)
                return false;

            fixed (byte* a = &m_data[m_offset], b = other.m_data)
            {
                for (int i = otherlen - 1, j = m_len - 1; i >= 0; --i, --j)
                {
                    if (a[j] != b[i])
                        return false;
                }
                return true;
            }
        }

        public unsafe bool EndsWith(byte[] other)
        {
            int otherlen = other.Length;
            if (otherlen > m_len)
                return false;

            fixed (byte* a = &m_data[m_offset], b = other)
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
            osUTF8Slice other = new osUTF8Slice(s); // yeack
            return EndsWith(other);
        }

        public unsafe int IndexOf(byte b)
        {
            if (m_len > 8)
            {
                fixed (byte* a = &m_data[m_offset])
                {
                    for (int i = 0; i < m_len; ++i)
                    {
                        if (a[i] == b)
                            return i;
                    }
                    return -1;
                }
            }

            for (int i = m_offset; i < m_offset + m_len; ++i)
            {
                if (m_data[i] == b)
                    return i - m_offset;
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

        public unsafe int IndexOf(osUTF8Slice other)
        {
            int otherlen = other.m_len;
            if (otherlen > m_len || otherlen == 0)
                return -1;

            byte[] otherdata = other.m_data;
            fixed (byte* a = &m_data[m_offset], b = &otherdata[other.m_offset])
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

        public unsafe int IndexOf(osUTF8 other)
        {
            int otherlen = other.m_len;
            if (otherlen > m_len || otherlen == 0)
                return -1;

            byte[] otherdata = other.m_data;
            fixed (byte* a = &m_data[m_offset], b = otherdata)
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
            if(other == null)
                return -1;
            int otherlen = other.Length;
            if (otherlen > m_len || otherlen == 0)
                return -1;

            fixed (byte* a = &m_data[m_offset], b = other)
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

        public unsafe int IndexOfAny(byte[] b)
        {
            if (m_len < 8)
            {
                for (int i = m_offset; i < m_offset + m_len; ++i)
                {
                    if (checkAny(m_data[i], b))
                        return i - m_offset;
                }
                return -1;
            }
            fixed (byte* a = &m_data[m_offset])
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
                for (int i = m_offset; i < m_offset + m_len; ++i)
                {
                    if (checkAny(m_data[i], b))
                        return i - m_offset;
                }
                return -1;
            }
            fixed (byte* a = &m_data[m_offset])
            {
                for (int i = 0; i < m_len; ++i)
                {
                    if (checkAny(a[i], b))
                        return i;
                }
                return -1;
            }
        }

        public bool Contains(osUTF8Slice other)
        {
            return IndexOf(other) > 0;
        }

        public bool Contains(string s)
        {
            return IndexOf(s) > 0;
        }

        public osUTF8Slice[] Split(byte b, bool ignoreEmpty = true)
        {
            if (m_len == 0)
            {
                return new osUTF8Slice[] { this };
            }

            bool incEmpty = !ignoreEmpty;
            osUTF8Slice tmp = new osUTF8Slice(this);
            List<osUTF8Slice> lst = new List<osUTF8Slice>();

            int indx;
            while ((indx = tmp.IndexOf(b)) >= 0)
            {
                osUTF8Slice o = tmp.SubUTF8(0, indx);
                if (incEmpty)
                    lst.Add(o);
                else if (o.m_len > 0)
                    lst.Add(o);
                tmp.MoveStart(indx + 1);
            }

            if (tmp.m_len > 0)
                lst.Add(tmp);
            return lst.ToArray();
        }

        public osUTF8Slice[] Split(byte[] b, bool ignoreEmpty = true)
        {
            if (m_len == 0)
            {
                return new osUTF8Slice[] { this };
            }

            bool incEmpty = !ignoreEmpty;
            osUTF8Slice tmp = new osUTF8Slice(this);
            List<osUTF8Slice> lst = new List<osUTF8Slice>();

            int indx;
            while ((indx = tmp.IndexOfAny(b)) >= 0)
            {
                osUTF8Slice o = tmp.SubUTF8(0, indx);
                if (incEmpty)
                    lst.Add(o);
                else if (o.m_len > 0)
                    lst.Add(o);
                tmp.MoveStart(indx + 1);
            }

            if (tmp.m_len > 0)
                lst.Add(tmp);
            return lst.ToArray();
        }

        public osUTF8Slice[] Split(char[] b, bool ignoreEmpty = true)
        {
            if (m_len == 0)
            {
                return new osUTF8Slice[] { this };
            }

            bool incEmpty = !ignoreEmpty;
            osUTF8Slice tmp = new osUTF8Slice(this);
            List<osUTF8Slice> lst = new List<osUTF8Slice>();

            int indx;
            while ((indx = tmp.IndexOfAny(b)) >= 0)
            {
                osUTF8Slice o = tmp.SubUTF8(0, indx);
                if (incEmpty)
                    lst.Add(o);
                else if (o.m_len > 0)
                    lst.Add(o);
                tmp.MoveStart(indx + 1);
            }

            if (tmp.m_len > 0)
                lst.Add(tmp);
            return lst.ToArray();
        }

        public osUTF8Slice[] Split(char b, bool ignoreEmpty = true)
        {
            if (b < 0x80)
                return Split((byte)b, ignoreEmpty);

            return new osUTF8Slice[0];
        }

        public unsafe bool ReadLine(out osUTF8Slice line)
        {
            if (m_len == 0)
            {
                line = new osUTF8Slice(new byte[0], 0, 0);
                return false;
            }

            int lineend = -1;
            byte b = 0;
            if (m_len < 8)
            {
                for (int i = m_offset; i < m_offset + m_len; ++i)
                {
                    b = m_data[i];
                    if (b == (byte)'\r' || b == (byte)'\n')
                    {
                        if (i > 0 && m_data[i - 1] == (byte)'\\')
                            continue;
                        lineend = i;
                        break;
                    }
                }
            }
            else
            {
                fixed (byte* a = &m_data[m_offset])
                {
                    for (int i = 0; i < m_len; ++i)
                    {
                        b = a[i];
                        if (b == (byte)'\r' || b == (byte)'\n')
                        {
                            if (i > 0 && a[i - 1] == (byte)'\\')
                                continue;
                            lineend = i + m_offset;
                            break;
                        }
                    }
                }
            }

            line = new osUTF8Slice(m_data, m_offset, m_len);
            if (lineend < 0)
            {
                m_offset = m_offset + m_len - 1;
                m_len = 0;
                return false;
            }

            int linelen = lineend - m_offset;
            line.m_len = linelen;

            ++linelen;
            if (linelen >= m_len)
            {
                m_offset = m_offset + m_len - 1;
                m_len = 0;
                return true;
            }

            m_offset += linelen;
            m_len -= linelen;

            if (m_len <= 0)
            {
                m_len = 0;
                return true;
            }

            if (b == (byte)'\r')
            {
                if (m_data[m_offset] == (byte)'\n')
                {
                    ++m_offset;
                    --m_len;
                }
            }

            if (m_len <= 0)
                m_len = 0;

            return true;
        }

        public unsafe bool SkipLine()
        {
            if (m_len == 0)
                return false;

            int lineend = -1;
            byte b = 0;
            if (m_len < 8)
            {
                for (int i = m_offset; i < m_offset + m_len; ++i)
                {
                    b = m_data[i];
                    if (b == (byte)'\r' || b == (byte)'\n')
                    {
                        if (i > 0 && m_data[i - 1] == (byte)'\\')
                            continue;
                        lineend = i;
                        break;
                    }
                }
            }
            else
            {
                fixed (byte* a = &m_data[m_offset])
                {
                    for (int i = 0; i < m_len; ++i)
                    {
                        b = a[i];
                        if (b == (byte)'\r' || b == (byte)'\n')
                        {
                            if (i > 0 && a[i - 1] == (byte)'\\')
                                continue;
                            lineend = i + m_offset;
                            break;
                        }
                    }
                }
            }

            if (lineend < 0)
            {
                m_offset = m_offset + m_len - 1;
                m_len = 0;
                return true;
            }

            int linelen = lineend - m_offset;

            ++linelen;
            if (linelen >= m_len)
            {
                m_offset = m_offset + m_len - 1;
                m_len = 0;
                return true;
            }

            m_offset += linelen;
            m_len -= linelen;
            if (m_len <= 0)
            {
                m_len = 0;
                return true;
            }

            if (b == (byte)'\r')
            {
                if (m_data[m_offset] == (byte)'\n')
                {
                    ++m_offset;
                    --m_len;
                }
            }

            if (m_len <= 0)
                m_len = 0;

            return true;
        }

        public void RemoveBytesSelf(int start, int len)
        {
            if (start < 0)
                throw new ArgumentOutOfRangeException("startIndex", "ArgumentOutOfRange_StartIndex");

            if (start >= m_len)
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
            if(start == 0)
                m_offset += end;
            else
                Array.Copy(m_data, end + m_offset, m_data, start + m_offset, m_len - end);
            m_len -= len;
        }

        public osUTF8Slice RemoveBytes(int start, int len)
        {
            if (start < 0)
                throw new ArgumentOutOfRangeException("startIndex", "ArgumentOutOfRange_StartIndex");

            if (start >= m_len)
                return Clone();

            osUTF8Slice o = new osUTF8Slice(m_len);
            Array.Copy(m_data, m_offset, o.m_data, 0, start);
            o.m_len = start;

            if (len < 0)
                return o;

            int end = start + len;
            if (end >= m_len)
                return o;

            Array.Copy(m_data, end + m_offset, o.m_data, start, m_len - end);
            o.m_len = m_len - len;
            return o;
        }

        public bool TryParseInt(out int res)
        {
            res = 0;

            SelfTrim();
            if (m_len == 0)
                return false;

            int len = m_len;
            int start = m_offset;
            len += start;

            bool neg = false;
            if (m_data[start] == (byte)'-')
            {
                neg = true;
                ++start;
            }
            else if (m_data[start] == (byte)'+')
                ++start;

            int b;
            try
            {
                while (start < len)
                {
                    b = m_data[start];
                    b -= (byte)'0';
                    if (b < 0 || b > 9)
                        break;

                    res *= 10;
                    res += b;
                    ++start;
                }
                if (neg)
                    res = -res;
                return true;
            }
            catch { }
            return false;
        }
        public static bool TryParseInt(osUTF8Slice t, out int res)
        {
            res = 0;

            t.SelfTrim();
            int len = t.m_len;
            if (len == 0)
                return false;

            byte[] data = t.m_data;

            int start = t.m_offset;
            len += start;

            bool neg = false;
            if (data[start] == (byte)'-')
            {
                neg = true;
                ++start;
            }
            else if (data[start] == (byte)'+')
                ++start;

            int b;
            try
            {
                while (start < len)
                {
                    b = data[start];
                    b -= (byte)'0';
                    if (b < 0 || b > 9)
                        break;

                    res *= 10;
                    res += b;
                    ++start;
                }
                if (neg)
                    res = -res;
                return true;
            }
            catch { }
            return false;
        }

        public unsafe static bool TryParseUUID(osUTF8Slice inp, out UUID result)
        {
            result = new UUID();
            if (inp.m_len == 0)
                return false;

            try
            {
                fixed (byte* bval = &inp.m_data[inp.m_offset])
                {
                    byte* val = bval;

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
    }
}
