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
    public struct osUTF8
    {
        internal byte[] m_data;
        internal int m_len;

        public static readonly osUTF8 Empty = new osUTF8(new byte[0], 0, 0);

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

        public osUTF8(byte[] source, int offset, int len)
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
            m_data = source.m_data;
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
                m_len = source.Length;
                m_data = new byte[m_len];
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
                    i = m_len;
                if (i < 0)
                    i = 0;
                else if (i >= m_data.Length)
                    i = m_data.Length - 1;
                return m_data[i];
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

        public bool IsNullOrEmpty { get { return m_len == 0; } }
        public bool IsEmpty { get { return m_len == 0; } }
        public unsafe bool IsNullOrWhitespace
        {
            get
            {
                if (m_len == 0)
                    return true;
                for (int i = 0; i < m_data.Length; ++i)
                {
                    if (m_data[i] != 0x20)
                        return false;
                }
                return true;
            }
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

            if (obj is string)
                return Equals((string)obj);

            return false;
        }

        public unsafe bool Equals(osUTF8 o)
        {
            if (m_len != o.m_len)
                return false;

            byte[] otherdata = o.m_data;

            if (m_len < 8)
            {
                for (int i = 0; i < m_data.Length; ++i)
                {
                    if (m_data[i] != otherdata[i])
                        return false;
                }
                return true;
            }

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
            osUTF8 o = new osUTF8(s);
            return Equals(o);
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
            return new osUTF8(b, 0, m_len + other.m_len);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void CheckCapacity(int needed)
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
                    while (src <= scrend)
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
            while (!Utils.osUTF8TryGetbytesNoNullTerm(s, ref srcindx, m_data, ref indx))
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
                            return -1;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryParseUUID(osUTF8 inp, out UUID res, bool dashs = true)
        {
            return osUTF8Slice.TryParseUUID(new osUTF8Slice(inp), out res, dashs);
        }
    }

    public struct osUTF8Slice
    {
        internal byte[] m_data;
        internal int m_offset;
        internal int m_len;

        public static readonly osUTF8Slice Empty = new osUTF8Slice(new byte[0], 0, 0);

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

        public byte this[int i]
        {
            get
            {
                if (i >= m_len)
                    i = m_len;
                i += m_offset;
                if (i < 0)
                    i = 0;
                else if (i >= m_data.Length)
                    i = m_data.Length - 1;
                return m_data[i];
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

        public bool IsNullOrEmpty { get { return m_len == 0; } }
        public bool IsEmpty { get { return m_len == 0; } }
        public unsafe bool IsNullOrWhitespace
        {
            get
            {
                if (m_len == 0)
                    return true;
                if (m_len < 8)
                {
                    for (int i = m_offset; i < m_offset + m_len; ++i)
                    {
                        if (m_data[i] != 0x20)
                            return false;
                    }
                    return true;
                }

                fixed (byte* a = &m_data[m_offset])
                {
                    for (int i = 0; i < m_len; ++i)
                    {
                        if (a[i] != 0x20)
                            return false;
                    }
                    return true;
                }
            }
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

            return false;
        }

        public unsafe bool Equals(osUTF8Slice o)
        {
            if (m_len != o.m_len)
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
            if (m_len != o.m_len)
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

        public bool Equals(string s)
        {
            osUTF8 o = new osUTF8(s);
            return Equals(o);
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
            return new osUTF8(b, 0, m_len);
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
                    while (src <= scrend)
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
            while (!Utils.osUTF8TryGetbytesNoNullTerm(s, ref srcindx, m_data, ref indx))
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
            if (m_len == 0)
                return;
            int last = m_offset + m_len - 1;
            while (m_data[m_offset] == 0x20)
            {
                ++m_offset;
                --m_len;
                if (m_offset == last)
                    break;
            }
        }

        public void SelfTrimStart(byte b)
        {
            if (m_len == 0)
                return;
            int last = m_offset + m_len - 1;
            while (m_data[m_offset] == b)
            {
                ++m_offset;
                --m_len;
                if (m_offset == last)
                    break;
            }
        }

        public void SelfTrimStart(byte[] b)
        {
            if (m_len == 0)
                return;
            int last = m_offset + m_len - 1;
            while (checkAny(m_data[m_offset], b))
            {
                ++m_offset;
                --m_len;
                if (m_offset == last)
                    break;
            }
        }

        public void SelfTrimStart(char[] b)
        {
            if (m_len == 0)
                return;
            int last = m_offset + m_len - 1;
            while (checkAny(m_data[m_offset], b))
            {
                ++m_offset;
                --m_len;
                if (m_offset == last)
                    break;
            }
        }

        public void SelfTrimEnd()
        {
            if (m_len == 0)
                return;
            int last = m_offset + m_len - 1;
            while (m_data[last] == 0x20)
            {
                --last;
                --m_len;
                if (last == m_offset)
                    break;
            }
        }

        public void SelfTrimEnd(byte b)
        {
            if (m_len == 0)
                return;
            int last = m_offset + m_len - 1;
            while (m_data[last] == b)
            {
                --last;
                --m_len;
                if (last == m_offset)
                    break;
            }
        }

        public void SelfTrimEnd(byte[] b)
        {
            if (m_len == 0)
                return;
            int last = m_offset + m_len - 1;
            while (checkAny(m_data[last], b))
            {
                --last;
                --m_len;
                if (last == m_offset)
                    break;
            }
        }

        public void SelfTrimEnd(char[] b)
        {
            if (m_len == 0)
                return;
            int last = m_offset + m_len - 1;
            while (checkAny(m_data[last], b))
            {
                --last;
                --m_len;
                if (last == m_offset)
                    break;
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
                            return -1;
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
                            return -1;
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

        public static bool TryParseUUID(osUTF8Slice inp, out UUID res, bool dashs = true)
        {
            res = UUID.Zero;
            osUTF8Slice t = new osUTF8Slice(inp);

            t.SelfTrim();
            int len = t.m_len;
            if (len == 0)
                return false;

            if (dashs)
            {
                if (len < 36)
                    return false;
            }
            else
            {
                if (len < 32)
                    return false;
            }

            byte[] data = t.m_data;
            int dataoffset = t.m_offset;

            int _a = 0;
            if (!Utils.TryHexToInt(data, dataoffset, 8, out _a))
                return false;
            dataoffset += 8;

            if (dashs)
            {
                if (data[dataoffset] != (byte)'-')
                    return false;
                ++dataoffset;
            }

            int n;
            if (!Utils.TryHexToInt(data, dataoffset, 4, out n))
                return false;
            short _b = (short)n;
            dataoffset += 4;

            if (dashs)
            {
                if (data[dataoffset] != (byte)'-')
                    return false;
                ++dataoffset;
            }

            if (!Utils.TryHexToInt(data, dataoffset, 4, out n))
                return false;
            short _c = (short)n;
            dataoffset += 4;

            if (dashs)
            {
                if (data[dataoffset] != (byte)'-')
                    return false;
                ++dataoffset;
            }

            if (!Utils.TryHexToInt(data, dataoffset, 4, out n))
                return false;

            byte _d = (byte)(n >> 8);
            byte _e = (byte)n;
            dataoffset += 4;

            if (dashs)
            {
                if (data[dataoffset] != (byte)'-')
                    return false;
                ++dataoffset;
            }

            if (!Utils.TryHexToInt(data, dataoffset, 8, out n))
                return false;
            byte _f = (byte)(n >> 24);
            byte _g = (byte)(n >> 16);
            byte _h = (byte)(n >> 8);
            byte _i = (byte)n;
            dataoffset += 8;

            if (!Utils.TryHexToInt(data, dataoffset, 4, out n))
                return false;
            byte _j = (byte)(n >> 8);
            byte _k = (byte)n;

            Guid g = new Guid(_a, _b, _c, _d, _e, _f, _g, _h, _i, _j, _k);
            res = new UUID(g);
            return true;
        }
    }

    public static class OSUTF8Cached
    {
        [ThreadStatic]
        private static byte[] m_cached;

        public static osUTF8 Acquire()
        {
            byte[] sb = m_cached;
            if (sb != null)
            {
                m_cached = null;
                return new osUTF8(sb);
            }
            return new osUTF8(4096);
        }

        public static osUTF8 Acquire(int capacity)
        {
            if (capacity <= 4096)
            {
                byte[] sb = m_cached;
                if (sb != null)
                {
                    m_cached = null;
                    return new osUTF8(sb);
                }
                capacity = 4096;
            }
            return new osUTF8(capacity);
        }

        public static void Release(osUTF8 sb)
        {
            if (sb.Capacity == 4096)
                m_cached = sb.m_data;
        }

        public static byte[] GetArrayAndRelease(osUTF8 sb)
        {
            byte[] result = sb.ToArray();
            if (sb.Capacity == 4096)
                m_cached = sb.m_data;
            return result;
        }
    }


    /*
    public static class OSUTF8Cached
    {
        [ThreadStatic]
        private static osUTF8 m_cached;

        public static osUTF8 Acquire()
        {
            osUTF8 sb = m_cached;
            if (sb != null)
            {
                m_cached = null;
                sb.Clear();
                return sb;
            }
            return new osUTF8(4096);
        }

        public static osUTF8 Acquire(int capacity)
        {
            if (capacity <= 4096)
            {
                osUTF8 sb = m_cached;
                if (sb != null)
                {
                    m_cached = null;
                    sb.Clear();
                    return sb;
                }
                capacity = 4096;
            }
            return new osUTF8(capacity);
        }

        public static void Release(osUTF8 sb)
        {
            if (sb.Capacity == 4096)
                m_cached = sb;
        }

        public static byte[] GetArrayAndRelease(osUTF8 sb)
        {
            byte[] result = sb.ToArray();
            if (sb.Capacity == 4096)
                m_cached = sb;
            return result;
        }
    }
    */
}
