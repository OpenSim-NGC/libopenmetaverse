/*
 * Copyright (c) 2006-2016, openmetaverse.co
 * All rights reserved.
 *
 * Copyright (c) Contributors, http://opensimulator.org/
 * See CONTRIBUTORS.TXT for a full list of copyright holders.
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
    More compact Storage and manipulation of utf8 byte chars strings, that .net core may have one day
    Not this is basicly a wrapper around a byte array that is shared
    Will get more things, in time.
    based on some ideas like FastStrings https://github.com/dhasenan/FastString
    Ubit Umarov 2020
*/

using System;
using System.Text;
using System.Collections.Generic;

namespace OpenMetaverse
{
    public struct osUTF8
    {
        internal readonly byte[] m_data;
        internal int m_offset;
        internal int m_len;

        public static readonly osUTF8 Empty = new osUTF8(new byte[0],0,0);

        public osUTF8(byte[] source)
        {
            m_data = source;
            m_offset = 0;
            m_len = source.Length;
        }

        public osUTF8(byte[] source, int offset, int len)
        {
            m_data = source;
            m_offset = offset;
            m_len = len;
        }

        public osUTF8(osUTF8 source)
        {
            m_data = source.m_data;
            m_offset = source.m_offset;
            m_len = source.m_len;
        }

        public osUTF8(string source)
        {
            m_data = Utils.StringToBytesNoTerm(source);
            m_offset = 0;
            m_len = m_data.Length;
        }

        public byte this[int i]
        {
            get
            {
                if(i >= m_len)
                    i = m_len;
                i += m_offset;
                if( i < 0)
                    i= 0;
                else if(i >= m_data.Length)
                    i= m_data.Length -1;
                return m_data[i + m_offset];
            }
        }

        public int Lenght
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

            if(m_offset < 0)
            {
                m_len -= m_offset;
                m_offset = 0;
            }
            else if(m_offset >= m_data.Length)
            {
                m_offset = m_data.Length - 1;
                m_len = 0;
            }
            else if(m_offset + m_len > m_data.Length)
                m_len = m_data.Length - m_offset;
        }

        public void ResetToFull()
        {
            m_offset = 0;
            m_len = m_data.Length;
        }

        public bool IsNullOrEmpty { get { return m_len == 0; } }
        public bool IsEmpty { get { return m_len == 0; } }
        public bool IsNullOrWhitespace
        {
            get
            {
                if(m_len == 0)
                    return true;
                for(int i = m_offset; i< m_offset + m_len; ++i)
                {
                    if(m_data[i] != (byte)' ')
                        return false;
                }
                return true;
            }
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public override string ToString()
        {
            return Encoding.UTF8.GetString(m_data, m_offset, m_len);
        }

        public osUTF8 Clone()
        {
            byte[] b = new byte[m_data.Length];
            Array.Copy(m_data, 0, b, 0, m_data.Length);
            return new osUTF8(b, m_offset, m_len);
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public osUTF8 osUTF8SubString(int start)
        {
            return osUTF8SubString(start, m_len - start);
        }

        //returns a segment view of main buffer
        public osUTF8 osUTF8SubString(int start, int len)
        {
            if (start < 0)
                start = 0;
            if (len > m_len)
                len = m_len;

            start += m_offset; // things are relative to current

            if (start >= m_data.Length)
                return new osUTF8(m_data, m_data.Length - 1, 0);

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

            return new osUTF8(m_data, start, last - start + 1);
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void osUTF8SubStringSelf(int start)
        {
            osUTF8SubStringSelf(start, m_len - start);
        }

        //returns a segment view of main buffer
        public void osUTF8SubStringSelf(int start, int len)
        {
            if (start < 0)
                start = 0;
            if (len > m_len)
                len = m_len;

            start += m_offset; // things are relative to current

            if (start >= m_data.Length)
            {
                m_offset += m_data.Length - 1;
                m_len = 0;
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

        public osUTF8 Extract()
        {
            byte[] b = new byte[m_len];
            Array.Copy(m_data, m_offset, b, 0, m_len);
            return new osUTF8(b, 0, m_len);
        }

        public osUTF8 Concat(osUTF8 other)
        {
            byte[] b = new byte[m_len + other.m_len];
            Array.Copy(m_data, m_offset, b, 0, m_len);
            Array.Copy(other.m_data, other.m_offset, b, m_len, other.m_len);
            return new osUTF8(b, 0, m_len + other.m_len);
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public string SubString(int start)
        {
            return SubString(start, m_len - start);
        }

        //returns a segment view of main buffer
        public string SubString(int start, int len)
        {
             osUTF8 res = osUTF8SubString(start, len);
             return res.ToString();
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private bool checkAny(byte b, byte[] bytes)
        {
            for (int i = 0; i < bytes.Length; ++i)
            {
                if (b == bytes[i])
                    return true;
            }
            return false;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
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
            while (m_data[m_offset] == (byte)' ')
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

        // inplace remove white spaces at start
        public void SelfTrimEnd()
        {
            if (m_len == 0)
                return;
            int last = m_offset + m_len - 1;
            while (m_data[last] == (byte)' ')
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
            while (checkAny(m_data[m_offset], b))
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
            while (checkAny(m_data[m_offset], b))
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

        public osUTF8 TrimStart()
        {
            osUTF8 ret = new osUTF8(this);
            ret.SelfTrimStart();
            return ret;
        }

        public osUTF8 TrimEnd()
        {
            osUTF8 ret = new osUTF8(this);
            ret.SelfTrimEnd();
            return ret;
        }

        public osUTF8 Trim()
        {
            osUTF8 ret = new osUTF8(this);
            ret.SelfTrimStart();
            ret.SelfTrimEnd();
            return ret;
        }

        public osUTF8 TrimStart(byte[] v)
        {
            osUTF8 ret = new osUTF8(this);
            ret.SelfTrimStart(v);
            return ret;
        }

        public osUTF8 TrimEnd(byte[] v)
        {
            osUTF8 ret = new osUTF8(this);
            ret.SelfTrimEnd(v);
            return ret;
        }

        public osUTF8 Trim(byte[] v)
        {
            osUTF8 ret = new osUTF8(this);
            ret.SelfTrimStart(v);
            ret.SelfTrimEnd(v);
            return ret;
        }

        public osUTF8 TrimStart(char[] v)
        {
            osUTF8 ret = new osUTF8(this);
            ret.SelfTrimStart(v);
            return ret;
        }

        public osUTF8 TrimEnd(char[] v)
        {
            osUTF8 ret = new osUTF8(this);
            ret.SelfTrimEnd(v);
            return ret;
        }

        public osUTF8 Trim(char[] v)
        {
            osUTF8 ret = new osUTF8(this);
            ret.SelfTrimStart(v);
            ret.SelfTrimEnd(v);
            return ret;
        }

        public bool StartsWith(osUTF8 other)
        {
            if (other.m_len > m_len)
                return false;
            byte[] otherdata = other.m_data;
            for (int i = other.m_offset, j = m_offset; i < other.m_offset + other.m_len; ++i, ++j)
            {
                if (m_data[j] != otherdata[i])
                    return false;
            }
            return true;
        }

        public bool StartsWith(string s)
        {
            osUTF8 other = new osUTF8(s); // yeack
            return StartsWith(other);
        }

        public bool EndsWith(osUTF8 other)
        {
            if (other.m_len > m_len)
                return false;
            byte[] otherdata = other.m_data;
            for (int i = other.m_len - 1, j = m_len - 1; i <= 0; --i, --j)
            {
                if (m_data[j] != otherdata[i])
                    return false;
            }
            return true;
        }

        public bool EndsWith(string s)
        {
            osUTF8 other = new osUTF8(s); // yeack
            return EndsWith(other);
        }

        public int IndexOf(byte b)
        {
            for (int i = m_offset; i < m_offset + m_len; ++i)
            {
                if(m_data[i] == b)
                    return i - m_offset;
            }
            return -1;
        }

        public int IndexOf(char b)
        {
            if (b < 0x80)
                return IndexOf((byte)b);
            string s = new string(new char[]{ b});
            return IndexOf(s);
        }

        public int IndexOf(osUTF8 other)
        {
            int otherlen = other.m_len;
            if (otherlen > m_len || otherlen == 0)
                return -1;

            byte[] otherdata = other.m_data;
            int otherend = otherlen + other.m_offset;
            for (int i = 0; i < m_len - otherlen; ++i)
            {
                int k = other.m_offset;
                for (int j = m_offset + i; k < otherend; ++k, ++j)
                {
                    if (m_data[j] != otherdata[k])
                        return -1;
                }
                if (k == otherend)
                    return i;
            }
            return -1;
        }

        public int IndexOf(string s)
        {
            if(string.IsNullOrEmpty(s))
                return -1;
            osUTF8 o = new osUTF8(s);
            return IndexOf(o);
        }

        public int IndexOfAny(byte[] b)
        {
            for (int i = m_offset; i < m_offset + m_len; ++i)
            {
                byte c = m_data[i];
                for (int k = 0; k < b.Length; ++k)
                    if (c == b[k])
                        return i - m_offset;
            }
            return -1;
        }

        public int IndexOfAny(char[] b)
        {
            for (int i = m_offset; i < m_offset + m_len; ++i)
            {
                byte c = m_data[i];
                for (int k = 0; k < b.Length; ++k)
                    if (c == (byte)b[k])
                        return i - m_offset;
            }
            return -1;
        }

        public bool Contains(osUTF8 other)
        {
            return IndexOf(other) > 0;
        }

        public bool Contains(string s)
        {
            return IndexOf(s) > 0;
        }

        public osUTF8[] Split(byte b, bool ignoreEmpty = true)
        {
            if (m_len == 0)
            {
                return new osUTF8[]{ this };
            }

            bool incEmpty = !ignoreEmpty;
            osUTF8 tmp = new osUTF8(this);
            List<osUTF8> lst = new List<osUTF8>();

            int indx;
            while ((indx = tmp.IndexOf(b)) >= 0)
            {
                osUTF8 o = tmp.osUTF8SubString(0, indx);
                if(incEmpty)
                    lst.Add(o);
                else if (o.m_len > 0)
                    lst.Add(o);
                tmp.MoveStart(indx + 1);
            }

            if (tmp.m_len > 0)
                lst.Add(tmp);
            return lst.ToArray();
        }

        public osUTF8[] Split(byte[] b, bool ignoreEmpty = true)
        {
            if (m_len == 0)
            {
                return new osUTF8[] { this };
            }

            bool incEmpty = !ignoreEmpty;
            osUTF8 tmp = new osUTF8(this);
            List<osUTF8> lst = new List<osUTF8>();

            int indx;
            while ((indx = tmp.IndexOfAny(b)) >= 0)
            {
                osUTF8 o = tmp.osUTF8SubString(0, indx);
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

        public osUTF8[] Split(char[] b, bool ignoreEmpty = true)
        {
            if (m_len == 0)
            {
                return new osUTF8[] { this };
            }

            bool incEmpty = !ignoreEmpty;
            osUTF8 tmp = new osUTF8(this);
            List<osUTF8> lst = new List<osUTF8>();

            int indx;
            while ((indx = tmp.IndexOfAny(b)) >= 0)
            {
                osUTF8 o = tmp.osUTF8SubString(0, indx);
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

        public osUTF8[] Split(char b, bool ignoreEmpty = true)
        {
            if(b < 0x80)
                return Split((byte)b, ignoreEmpty);

            return new osUTF8[0];
        }

        public bool ReadLine(out osUTF8 line)
        {
            if (m_len == 0)
            {
                line = new osUTF8(new byte[0], 0, 0);
                return false;
            }

            int lineend = -1;
            byte b;
            for (int i = m_offset; i < m_offset + m_len; ++i)
            {
                b = m_data[i];
                if (b == (byte)'\r' || b == (byte)'\n')
                {
                    if (i > 0 && m_data[i - 1] == '\\')
                        continue;
                    lineend = i;
                    break;
                }
            }

            line = new osUTF8(m_data, m_offset, m_len);
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

            b = m_data[m_offset];
            if (b == (byte)'\r' || b == (byte)'\n')
            {
                ++m_offset;
                --m_len;
                if (linelen >= m_len)
                {
                    m_offset = m_offset + m_len - 1;
                    m_len = 0;
                    return true;
                }
            }

            return true;
        }

        public bool SkipLine()
        {
            if (m_len == 0)
                return false;

            int lineend = -1;
            byte b;
            for (int i = m_offset; i < m_offset + m_len; ++i)
            {
                b = m_data[i];
                if (b == (byte)'\r' || b == (byte)'\n')
                {
                    if (i > 0 && m_data[i - 1] == '\\')
                        continue;
                    lineend = i;
                    break;
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

            b = m_data[m_offset];
            if (b == (byte)'\r' || b == (byte)'\n')
            {
                ++m_offset;
                --m_len;
                if (linelen >= m_len)
                {
                    m_offset = m_offset + m_len - 1;
                    m_len = 0;
                    return true;
                }
            }

            return true;
        }

    }
}
