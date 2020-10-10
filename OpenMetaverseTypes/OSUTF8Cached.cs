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

namespace OpenMetaverse
{
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
}
