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


namespace OpenMetaverse
{
    public static class OSUTF8Cached
    {
        const int MAXSIZE = 128; // 16MB
        const int PREALLOC = 128;
        public const int MAXDATASIZE = 128 * 1024;

        private static readonly osUTF8[] m_pool = new osUTF8[MAXSIZE];
        private static readonly object m_poollock = new object();
        private static int m_poolPtr;

        static OSUTF8Cached()
        {
            for (int i = 0; i < PREALLOC; ++i)
                m_pool[i] = new osUTF8(MAXDATASIZE);
            m_poolPtr = PREALLOC - 1;
        }

        public static osUTF8 Acquire()
        {
            lock (m_poollock)
            {
                if (m_poolPtr >= 0)
                {
                    osUTF8 os = m_pool[m_poolPtr];
                    m_pool[m_poolPtr] = null;
                    m_poolPtr--;
                    os.Clear();
                    return os;
                }
            }
            return new osUTF8(MAXDATASIZE);
        }

        public static osUTF8 Acquire(int capacity)
        {
            if(capacity <= MAXDATASIZE)
            {
                lock (m_poollock)
                {
                    if (m_poolPtr >= 0)
                    {
                        osUTF8 os = m_pool[m_poolPtr];
                        m_pool[m_poolPtr] = null;
                        m_poolPtr--;
                        os.Clear();
                        return os;
                    }
                    capacity = MAXDATASIZE;
                }
            }
            return new osUTF8(capacity);
        }

        public static void Release(osUTF8 os)
        {
            if (os.m_data.Length == MAXDATASIZE)
            {
                lock (m_poollock)
                {
                    if (m_poolPtr < MAXSIZE - 1)
                    {
                        os.Clear();
                        m_poolPtr++;
                        m_pool[m_poolPtr] = os;
                    }
                }
            }
        }

        public static byte[] GetArrayAndRelease(osUTF8 os)
        {
            byte[] result = os.ToArray();
            if (os.m_data.Length == MAXDATASIZE)
            {
                lock (m_poollock)
                {
                    if (m_poolPtr < MAXSIZE - 1)
                    {
                        os.Clear();
                        m_poolPtr++;
                        m_pool[m_poolPtr] = os;
                    }
                }
            }
            return result;
        }

        public static string GetStringAndRelease(osUTF8 os)
        {
            string ret = os.ToString();
            if (os.m_data.Length == MAXDATASIZE)
            {
                lock (m_poollock)
                {
                    if (m_poolPtr < MAXSIZE - 1)
                    {
                        os.Clear();
                        m_poolPtr++;
                        m_pool[m_poolPtr] = os;
                    }
                }
            }
            return ret;
        }
    }
}
