/*
 * Copyright (c) Contributors, http://opensimulator.org/
 * See CONTRIBUTORS.TXT for a full list of copyright holders.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of the OpenSimulator Project nor the
 *       names of its contributors may be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE DEVELOPERS ``AS IS'' AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE CONTRIBUTORS BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

// a class for low level LLSD encoding into a provided osUTF8
// for cases where we already need to know the low level detail
// and so using something like OSD or even protbuf is just a pure waste

using System;
using System.Globalization;
using System.Text;
using OpenMetaverse;


namespace OpenMetaverse.StructuredData
{
    public static class LLSDxmlEncode2
    {
        static readonly  DateTime depoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static void AddStart(osUTF8 sb, bool addxmlversion = false)
        {
            if(addxmlversion)
                sb.Append(osUTF8Const.XMLformalHeaderllsdstart); // legacy llsd xml name still valid
            else
                sb.Append(osUTF8Const.XMLllsdStart);
        }

        public static osUTF8 Start(int size = OSUTF8Cached.MAXDATASIZE, bool addxmlversion = false)
        {
            osUTF8 sb = OSUTF8Cached.Acquire(size);
            if(addxmlversion)
                sb.Append(osUTF8Const.XMLformalHeaderllsdstart); // legacy llsd xml name still valid
            else
                sb.Append(osUTF8Const.XMLllsdStart);
            return sb;
        }

        public static void AddEnd(osUTF8 sb)
        {
            sb.Append(osUTF8Const.XMLllsdEnd);
        }

        public static string End(osUTF8 sb)
        {
            sb.Append(osUTF8Const.XMLllsdEnd);
            string s = sb.ToString();
            OSUTF8Cached.Release(sb);
            return s;
        }

        public static byte[] EndToNBBytes(osUTF8 sb)
        {
            sb.Append(osUTF8Const.XMLllsdEnd);
            return OSUTF8Cached.GetArrayAndRelease(sb);
        }

        public static byte[] EndToBytes(osUTF8 sb)
        {
            sb.Append(osUTF8Const.XMLllsdEnd);
            return OSUTF8Cached.GetArrayAndRelease(sb);
        }

        // map == a list of key value pairs
        public static void AddMap(osUTF8 sb)
        {
            sb.Append(osUTF8Const.XMLmapStart);
        }

        public static void AddEndMap(osUTF8 sb)
        {
            sb.Append(osUTF8Const.XMLmapEnd);
        }

        public static void AddEmptyMap(osUTF8 sb)
        {
            sb.Append(osUTF8Const.XMLmapEmpty);
        }

        public static void AddArray(osUTF8 sb)
        {
            sb.Append(osUTF8Const.XMLarrayStart);
        }

        public static void AddEndArray(osUTF8 sb)
        {
            sb.Append(osUTF8Const.XMLarrayEnd);
        }

        public static void AddEndMapAndArray(osUTF8 sb)
        {
            sb.Append(osUTF8Const.XMLmapEndarrayEnd);
        }

        public static void AddEmptyArray(osUTF8 sb)
        {
            sb.Append(osUTF8Const.XMLarrayEmpty);
        }

        public static void AddEndArrayAndMap(osUTF8 sb)
        {
            sb.Append(osUTF8Const.XMLarrayEndmapEnd);
        }

        // undefined or null
        public static void AddUnknownElem(osUTF8 sb)
        {
            sb.Append(osUTF8Const.XMLundef);
        }

        public static void AddElem(bool e, osUTF8 sb)
        {
            if(e)
                sb.Append(osUTF8Const.XMLfullbooleanOne);
            else
                sb.Append(osUTF8Const.XMLfullbooleanZero);
        }

        public static void AddElem(byte e, osUTF8 sb)
        {
            if(e == 0)
                sb.Append(osUTF8Const.XMLintegerEmpty);
            else
            {
                sb.Append(osUTF8Const.XMLintegerStart);
                sb.AppendInt(e);
                sb.Append(osUTF8Const.XMLintegerEnd);
            }
        }

        public static void AddElem(byte[] e, osUTF8 sb)
        {
            if(e == null || e.Length == 0)
                sb.Append(osUTF8Const.XMLbinaryEmpty);
            else
            {
                sb.Append(osUTF8Const.XMLbinaryStart); // encode64 is default
                OSDParser.base64Encode(e, sb);
                sb.Append(osUTF8Const.XMLbinaryEnd);
            }
        }

        public static void AddElem(byte[] e, int start, int length, osUTF8 sb)
        {
            if (start + length >= e.Length)
                length = e.Length - start;

            if (e == null || e.Length == 0 || length <= 0)
                sb.Append(osUTF8Const.XMLbinaryEmpty);
            else
            {
                sb.Append(osUTF8Const.XMLbinaryStart); // encode64 is default
                OSDParser.base64Encode(e, start, length, sb);
                sb.Append(osUTF8Const.XMLbinaryEnd);
            }
        }

        public static void AddElem(int e, osUTF8 sb)
        {
            if(e == 0)
                sb.Append(osUTF8Const.XMLintegerEmpty);
            else
            {
                sb.Append(osUTF8Const.XMLintegerStart);
                sb.AppendInt(e);
                sb.Append(osUTF8Const.XMLintegerEnd);
            }
        }

        public static void AddElem(uint e, osUTF8 sb)
        {
            AddElem(uintToByteArray(e), sb);
        }

        public static void AddElem(ulong e, osUTF8 sb)
        {
            AddElem(ulongToByteArray(e), sb);
        }

        public static void AddElem(float e, osUTF8 sb)
        {
            if(e == 0)
                sb.Append(osUTF8Const.XMLrealZero);
            else
            {
                sb.Append(osUTF8Const.XMLrealStart);
                sb.AppendASCII(e.ToString(CultureInfo.InvariantCulture));
                sb.Append(osUTF8Const.XMLrealEnd);
            }
        }

        public static void AddElem(Vector2 e, osUTF8 sb)
        {
            if(e.X == 0)
                sb.Append(osUTF8Const.XMLarrayStartrealZero);
            else
            {
                sb.Append(osUTF8Const.XMLarrayStartrealStart);
                sb.AppendASCII(e.X.ToString(CultureInfo.InvariantCulture));
                sb.Append(osUTF8Const.XMLrealEnd);
            }

            if(e.Y == 0)
                sb.Append(osUTF8Const.XMLrealZeroarrayEnd);
            else
            {
                sb.Append(osUTF8Const.XMLrealStart);
                sb.AppendASCII(e.Y.ToString(CultureInfo.InvariantCulture));
                sb.Append(osUTF8Const.XMLrealEndarrayEnd);
            }
        }

        public static void AddElem(Vector3 e, osUTF8 sb)
        {
            if(e.X == 0)
                sb.Append(osUTF8Const.XMLarrayStartrealZero);
            else
            {
                sb.Append(osUTF8Const.XMLarrayStartrealStart);
                sb.AppendASCII(e.X.ToString(CultureInfo.InvariantCulture));
                sb.Append(osUTF8Const.XMLrealEnd);
            }

            if(e.Y == 0)
                sb.Append(osUTF8Const.XMLrealZero);
            else
            {
                sb.Append(osUTF8Const.XMLrealStart);
                sb.AppendASCII(e.Y.ToString(CultureInfo.InvariantCulture));
                sb.Append(osUTF8Const.XMLrealEnd);
            }

            if(e.Z == 0)
                sb.Append(osUTF8Const.XMLrealZeroarrayEnd);
            else
            {
                sb.Append(osUTF8Const.XMLrealStart);
                sb.AppendASCII(e.Z.ToString(CultureInfo.InvariantCulture));
                sb.Append(osUTF8Const.XMLrealEndarrayEnd);
            }
        }

        public static void AddElem(Quaternion e, osUTF8 sb)
        {
            if(e.X == 0)
                sb.Append(osUTF8Const.XMLarrayStartrealZero);
            else
            {
                sb.Append(osUTF8Const.XMLarrayStartrealStart);
                sb.AppendASCII(e.X.ToString(CultureInfo.InvariantCulture));
                sb.Append(osUTF8Const.XMLrealEnd);
            }

            if(e.Y == 0)
                sb.Append(osUTF8Const.XMLrealZero);
            else
            {
                sb.Append(osUTF8Const.XMLrealStart);
                sb.AppendASCII(e.Y.ToString(CultureInfo.InvariantCulture));
                sb.Append(osUTF8Const.XMLrealEnd);
            }
            if(e.Z == 0)
                sb.Append(osUTF8Const.XMLrealZero);
            else
            {
                sb.Append(osUTF8Const.XMLrealStart);
                sb.AppendASCII(e.Z.ToString(CultureInfo.InvariantCulture));
                sb.Append(osUTF8Const.XMLrealEnd);
            }

            if(e.W == 0)
                sb.Append(osUTF8Const.XMLrealZeroarrayEnd);
            else
            {
                sb.Append(osUTF8Const.XMLrealStart);
                sb.AppendASCII(e.W.ToString(CultureInfo.InvariantCulture));
                sb.Append(osUTF8Const.XMLrealEndarrayEnd);
            }
        }

        public static void AddElem(double e, osUTF8 sb)
        {
            if(e == 0)
                sb.Append(osUTF8Const.XMLrealZero);
            else
            {
                sb.Append(osUTF8Const.XMLrealStart);
                sb.AppendASCII(e.ToString(CultureInfo.InvariantCulture));
                sb.Append(osUTF8Const.XMLrealEnd);
            }
        }

        public static void AddElem(UUID e, osUTF8 sb)
        {
            if(e.IsZero())
                sb.Append(osUTF8Const.XMLuuidEmpty);
            else
            {
                sb.Append(osUTF8Const.XMLuuidStart);
                sb.AppendUUID(e);
                sb.Append(osUTF8Const.XMLuuidEnd);
            }
        }

        public static void AddElem(string e, osUTF8 sb)
        {
            if(string.IsNullOrEmpty(e))
                sb.Append(osUTF8Const.XMLstringEmpty);
            else
            {
                sb.Append(osUTF8Const.XMLstringStart);
                OSDParser.EscapeToXML(sb, e);
                sb.Append(osUTF8Const.XMLstringEnd);
            }
        }

        public static void AddElem(osUTF8 e, osUTF8 sb)
        {
            if (osUTF8.IsNullOrEmpty(e))
                sb.Append(osUTF8Const.XMLstringEmpty);
            else
            {
                sb.Append(osUTF8Const.XMLstringStart);
                OSDParser.EscapeToXML(sb, e);
                sb.Append(osUTF8Const.XMLstringEnd);
            }
        }

        public static void AddRawElem(string e, osUTF8 sb)
        {
            if (!string.IsNullOrEmpty(e))
                sb.Append(e);
        }

        public static void AddRawElem(byte[] e, osUTF8 sb)
        {
            if (e != null && e.Length >= 0)
                sb.Append(e);
        }

        public static void AddElem(Uri e, osUTF8 sb)
        {
            if(e == null)
            {
                sb.Append(osUTF8Const.XMLuriEmpty);
                return;
            }

            string s;
            if (e.IsAbsoluteUri)
                s = e.AbsoluteUri;
            else
                s = e.ToString();

            if(string.IsNullOrEmpty(s))
                sb.Append(osUTF8Const.XMLuriEmpty);
            else
            {
                sb.Append(osUTF8Const.XMLuriStart);
                sb.Append(s);     
                sb.Append(osUTF8Const.XMLuriEnd);
            }
        }

        public static void AddElem(DateTime e, osUTF8 sb)
        {
            DateTime u = e.ToUniversalTime();
            if(u == depoch)
            {
                sb.Append(osUTF8Const.XMLdateEmpty);
                return;
            }    
            string format;
            if(u.Hour == 0 && u.Minute == 0 && u.Second == 0)
                format = "yyyy-MM-dd";
            else if (u.Millisecond > 0)
                format = "yyyy-MM-ddTHH:mm:ss.ffZ";
            else
                format = "yyyy-MM-ddTHH:mm:ssZ";
            sb.Append(osUTF8Const.XMLdateStart);
            sb.AppendASCII(u.ToString(format,CultureInfo.InvariantCulture));
            sb.Append(osUTF8Const.XMLdateEnd);
        }

//************ key value *******************
// assumes name is a valid llsd key

        public static void AddMap(string name, osUTF8 sb)
        {
            sb.Append(osUTF8Const.XMLkeyStart);
            sb.AppendASCII(name);
            sb.Append(osUTF8Const.XMLkeyEndmapStart);
        }

        public static void AddEmptyMap(string name, osUTF8 sb)
        {
            sb.Append(osUTF8Const.XMLkeyStart);
            sb.AppendASCII(name);
            sb.Append(osUTF8Const.XMLkeyEndmapEmpty);
        }

        // array == a list values
        public static void AddArray(string name, osUTF8 sb)
        {
            sb.Append(osUTF8Const.XMLkeyStart);
            sb.AppendASCII(name);
            sb.Append(osUTF8Const.XMLkeyEndarrayStart);
        }

        public static void AddEmptyArray(string name, osUTF8 sb)
        {
            sb.Append(osUTF8Const.XMLkeyStart);
            sb.AppendASCII(name);
            sb.Append(osUTF8Const.XMLkeyEndarrayEmpty);
        }

        public static void AddArrayAndMap(string name, osUTF8 sb)
        {
            sb.Append(osUTF8Const.XMLkeyStart);
            sb.AppendASCII(name);
            sb.Append(osUTF8Const.XMLkeyEndarrayStartmapStart);
        }


        // undefined or null
        public static void AddUnknownElem(string name, osUTF8 sb)
        {
            sb.Append(osUTF8Const.XMLkeyStart);
            sb.AppendASCII(name);
            sb.Append(osUTF8Const.XMLkeyEndundef);
        }

        public static void AddElem(string name, bool e, osUTF8 sb)
        {
            sb.Append(osUTF8Const.XMLkeyStart);
            sb.AppendASCII(name);
            sb.Append(osUTF8Const.XMLkeyEnd);

            if(e)
                sb.Append(osUTF8Const.XMLfullbooleanOne);
            else
                sb.Append(osUTF8Const.XMLfullbooleanZero);
        }

        public static void AddElem(string name, byte e, osUTF8 sb)
        {
            sb.Append(osUTF8Const.XMLkeyStart);
            sb.AppendASCII(name);
            sb.Append(osUTF8Const.XMLkeyEnd);

            if(e == 0)
                sb.Append(osUTF8Const.XMLintegerEmpty);
            else
            {
                sb.Append(osUTF8Const.XMLintegerStart);
                sb.AppendInt(e);
                sb.Append(osUTF8Const.XMLintegerEnd);
            }
        }

        public static void AddElem(string name, byte[] e, osUTF8 sb)
        {
            sb.Append(osUTF8Const.XMLkeyStart);
            sb.AppendASCII(name);
            sb.Append(osUTF8Const.XMLkeyEnd);

            if (e == null || e.Length == 0)
                sb.Append(osUTF8Const.XMLbinaryEmpty);
            else
            {
                sb.Append(osUTF8Const.XMLbinaryStart); // encode64 is default
                OSDParser.base64Encode(e, sb);
                sb.Append(osUTF8Const.XMLbinaryEnd);
            }
        }

        public static void AddElem(string name, byte[] e, int start, int length, osUTF8 sb)
        {
            sb.Append(osUTF8Const.XMLkeyStart);
            sb.AppendASCII(name);
            sb.Append(osUTF8Const.XMLkeyEnd);

            if (start + length >= e.Length)
                length = e.Length - start;

            if (e == null || e.Length == 0 || length <= 0)
                sb.Append(osUTF8Const.XMLbinaryEmpty);
            else
            {
                sb.Append(osUTF8Const.XMLbinaryStart); // encode64 is default
                OSDParser.base64Encode(e, start, length, sb);
                sb.Append(osUTF8Const.XMLbinaryEnd);
            }
        }

        public static void AddElem(string name, int e, osUTF8 sb)
        {
            sb.Append(osUTF8Const.XMLkeyStart);
            sb.AppendASCII(name);
            sb.Append(osUTF8Const.XMLkeyEnd);

            if(e == 0)
                sb.Append(osUTF8Const.XMLintegerEmpty);
            else
            {
                sb.Append(osUTF8Const.XMLintegerStart);
                sb.AppendInt(e);
                sb.Append(osUTF8Const.XMLintegerEnd);
            }
        }

        public static void AddElem(string name, uint e, osUTF8 sb)
        {
            AddElem(name, uintToByteArray(e), sb);
        }

        public static void AddElem(string name, ulong e, osUTF8 sb)
        {
            AddElem(name, ulongToByteArray(e), sb);
        }

        public static void AddElem(string name, float e, osUTF8 sb)
        {
            sb.Append(osUTF8Const.XMLkeyStart);
            sb.AppendASCII(name);
            sb.Append(osUTF8Const.XMLkeyEnd);

            if(e == 0)
                sb.Append(osUTF8Const.XMLrealZero);
            else
            {
                sb.Append(osUTF8Const.XMLrealStart);
                sb.AppendASCII(e.ToString(CultureInfo.InvariantCulture));     
                sb.Append(osUTF8Const.XMLrealEnd);
            }
        }

        public static void AddElem(string name, Vector2 e, osUTF8 sb)
        {
            sb.Append(osUTF8Const.XMLkeyStart);
            sb.AppendASCII(name);

            if(e.X == 0)
                sb.Append(osUTF8Const.XMLkeyEndarrayStartrealZero);
            else
            {
                sb.Append(osUTF8Const.XMLkeyEndarrayStartrealZero);
                sb.AppendASCII(e.X.ToString(CultureInfo.InvariantCulture));
                sb.Append(osUTF8Const.XMLrealEnd);
            }

            if(e.Y == 0)
                sb.Append(osUTF8Const.XMLrealZeroarrayEnd);
            else
            {
                sb.Append(osUTF8Const.XMLrealStart);
                sb.AppendASCII(e.Y.ToString(CultureInfo.InvariantCulture));
                sb.Append(osUTF8Const.XMLrealEndarrayEnd);
            }
        }

        public static void AddElem(string name, Vector3 e, osUTF8 sb)
        {
            sb.Append(osUTF8Const.XMLkeyStart);
            sb.AppendASCII(name);

            if(e.X == 0)
                sb.Append(osUTF8Const.XMLkeyEndarrayStartrealZero);
            else
            {
                sb.Append(osUTF8Const.XMLkeyEndarrayStartrealStart);
                sb.AppendASCII(e.X.ToString(CultureInfo.InvariantCulture));
                sb.Append(osUTF8Const.XMLrealEnd);
            }

            if(e.Y == 0)
                sb.Append(osUTF8Const.XMLrealZero);
            else
            {
                sb.Append(osUTF8Const.XMLrealStart);
                sb.AppendASCII(e.Y.ToString(CultureInfo.InvariantCulture));
                sb.Append(osUTF8Const.XMLrealEnd);
            }

            if(e.Z == 0)
                sb.Append(osUTF8Const.XMLrealZeroarrayEnd);
            else
            {
                sb.Append(osUTF8Const.XMLrealStart);
                sb.AppendASCII(e.Z.ToString(CultureInfo.InvariantCulture));
                sb.Append(osUTF8Const.XMLrealEndarrayEnd);
            }
        }

        public static void AddElem(string name, Quaternion e, osUTF8 sb)
        {
            sb.Append(osUTF8Const.XMLkeyStart);
            sb.AppendASCII(name);

            if(e.X == 0)
                sb.Append(osUTF8Const.XMLkeyEndarrayStartrealZero);
            else
            {
                sb.Append(osUTF8Const.XMLkeyEndarrayStartrealStart);
                sb.AppendASCII(e.X.ToString(CultureInfo.InvariantCulture));
                sb.Append(osUTF8Const.XMLrealEnd);
            }

            if(e.Y == 0)
                sb.Append(osUTF8Const.XMLrealZero);
            else
            {
                sb.Append(osUTF8Const.XMLrealStart);
                sb.AppendASCII(e.Y.ToString(CultureInfo.InvariantCulture));
                sb.Append(osUTF8Const.XMLrealEnd);
            }
            if(e.Z == 0)
                sb.Append(osUTF8Const.XMLrealZero);
            else
            {
                sb.Append(osUTF8Const.XMLrealStart);
                sb.AppendASCII(e.Z.ToString(CultureInfo.InvariantCulture));
                sb.Append(osUTF8Const.XMLrealEnd);
            }

            if(e.W == 0)
                sb.Append(osUTF8Const.XMLrealZeroarrayEnd);
            else
            {
                sb.Append(osUTF8Const.XMLrealStart);
                sb.AppendASCII(e.W.ToString(CultureInfo.InvariantCulture));
                sb.Append(osUTF8Const.XMLrealEndarrayEnd);
            }
        }

        public static void AddElem(string name, double e, osUTF8 sb)
        {
            sb.Append(osUTF8Const.XMLkeyStart);
            sb.AppendASCII(name);
            sb.Append(osUTF8Const.XMLkeyEnd);

            if(e == 0)
                sb.Append(osUTF8Const.XMLrealZero);
            else
            {
                sb.Append(osUTF8Const.XMLrealStart);
                sb.AppendASCII(e.ToString(CultureInfo.InvariantCulture));
                sb.Append(osUTF8Const.XMLrealEnd);
            }
        }

        public static void AddElem(string name, UUID e, osUTF8 sb)
        {
            sb.Append(osUTF8Const.XMLkeyStart);
            sb.AppendASCII(name);
            sb.Append(osUTF8Const.XMLkeyEnd);

            if (e.IsZero())
                sb.Append(osUTF8Const.XMLuuidEmpty);
            else
            {
                sb.Append(osUTF8Const.XMLuuidStart);
                sb.AppendUUID(e);
                sb.Append(osUTF8Const.XMLuuidEnd);
            }
        }

        public static void AddElem(string name, string e, osUTF8 sb)
        {
            sb.Append(osUTF8Const.XMLkeyStart);
            sb.AppendASCII(name);
            sb.Append(osUTF8Const.XMLkeyEnd);

            if(string.IsNullOrEmpty(e))
                sb.Append(osUTF8Const.XMLstringEmpty);
            else
            {
                sb.Append(osUTF8Const.XMLstringStart);
                OSDParser.EscapeToXML(sb, e);
                sb.Append(osUTF8Const.XMLstringEnd);
            }
        }

        public static void AddElem(string name, osUTF8 e, osUTF8 sb)
        {
            sb.Append(osUTF8Const.XMLkeyStart);
            sb.AppendASCII(name);
            sb.Append(osUTF8Const.XMLkeyEnd);

            if (osUTF8.IsNullOrEmpty(e))
                sb.Append(osUTF8Const.XMLstringEmpty);
            else
            {
                sb.Append(osUTF8Const.XMLstringStart);
                OSDParser.EscapeToXML(sb, e);
                sb.Append(osUTF8Const.XMLstringEnd);
            }
        }

        public static void AddRawElem(string name, string e, osUTF8 sb)
        {
            if (string.IsNullOrEmpty(e))
                return;

            sb.Append(osUTF8Const.XMLkeyStart);
            sb.AppendASCII(name);
            sb.Append(osUTF8Const.XMLkeyEnd);
            sb.Append(e);
        }

        public static void AddElem(string name, Uri e, osUTF8 sb)
        {
            sb.Append(osUTF8Const.XMLkeyStart);
            sb.AppendASCII(name);
            sb.Append(osUTF8Const.XMLkeyEnd);

            if(e == null)
            {
                sb.Append(osUTF8Const.XMLuriEmpty);
                return;
            }

            string s;
            if (e.IsAbsoluteUri)
                s = e.AbsoluteUri;
            else
                s = e.ToString();

            if(string.IsNullOrEmpty(s))
                sb.Append(osUTF8Const.XMLuriEmpty);
            else
            {
                sb.Append(osUTF8Const.XMLuriStart);
                sb.Append(s);
                sb.Append(osUTF8Const.XMLuriEnd);
            }
        }

        public static void AddElem(string name, DateTime e, osUTF8 sb)
        {
            sb.Append(osUTF8Const.XMLkeyStart);
            sb.AppendASCII(name);
            sb.Append(osUTF8Const.XMLkeyEnd);

            DateTime u = e.ToUniversalTime();
            if(u == depoch)
            {
                sb.Append(osUTF8Const.XMLdateEmpty);
                return;
            }    
            string format;
            if(u.Hour == 0 && u.Minute == 0 && u.Second == 0)
                format = "yyyy-MM-dd";
            else if (u.Millisecond > 0)
                format = "yyyy-MM-ddTHH:mm:ss.ffZ";
            else
                format = "yyyy-MM-ddTHH:mm:ssZ";
            sb.Append(osUTF8Const.XMLdateStart);
            sb.AppendASCII(u.ToString(format,CultureInfo.InvariantCulture));
            sb.Append(osUTF8Const.XMLdateEnd);
        }

        public static void AddLLSD(string e, osUTF8 sb)
        {
            sb.Append(e);
        }

        public static void AddLLSD(string name, string e, osUTF8 sb)
        {
            sb.Append(osUTF8Const.XMLkeyStart);
            sb.AppendASCII(name);
            sb.Append(osUTF8Const.XMLkeyEnd);
            sb.Append(e);
        }

        public static void AddElem_name(string s, osUTF8 sb)
        {
            if (string.IsNullOrEmpty(s))
                sb.Append(osUTF8Const.XMLelement_name_Empty);
            else
            {
                sb.Append(osUTF8Const.XMLelement_name_Start);
                OSDParser.EscapeToXML(sb, s);
                sb.Append(osUTF8Const.XMLstringEnd);
            }
        }

        public static void AddElem_agent_id(UUID e, osUTF8 sb)
        {
            if (e.IsZero())
                sb.Append(osUTF8Const.XMLelement_agent_id_Empty);
            else
            {
                sb.Append(osUTF8Const.XMLelement_agent_id_Start);
                sb.AppendUUID(e);
                sb.Append(osUTF8Const.XMLuuidEnd);
            }
        }

        public static void AddElem_owner_id(UUID e, osUTF8 sb)
        {
            if (e.IsZero())
                sb.Append(osUTF8Const.XMLelement_owner_id_Empty);
            else
            {
                sb.Append(osUTF8Const.XMLelement_owner_id_Start);
                sb.AppendUUID(e);
                sb.Append(osUTF8Const.XMLuuidEnd);
            }
        }

        public static void AddElem_parent_id(UUID e, osUTF8 sb)
        {
            if (e.IsZero())
                sb.Append(osUTF8Const.XMLelement_parent_id_Empty);
            else
            {
                sb.Append(osUTF8Const.XMLelement_parent_id_Start);
                sb.AppendUUID(e);
                sb.Append(osUTF8Const.XMLuuidEnd);
            }
        }

        public static void AddElem_folder_id(UUID e, osUTF8 sb)
        {
            if (e.IsZero())
                sb.Append(osUTF8Const.XMLelement_folder_id_Empty);
            else
            {
                sb.Append(osUTF8Const.XMLelement_folder_id_Start);
                sb.AppendUUID(e);
                sb.Append(osUTF8Const.XMLuuidEnd);
            }
        }

        public static void AddElem_asset_id(UUID e, osUTF8 sb)
        {
            if (e.IsZero())
                sb.Append(osUTF8Const.XMLelement_asset_id_Empty);
            else
            {
                sb.Append(osUTF8Const.XMLelement_asset_id_Start);
                sb.AppendUUID(e);
                sb.Append(osUTF8Const.XMLuuidEnd);
            }
        }

        public static void AddElem_item_id(UUID e, osUTF8 sb)
        {
            if (e.IsZero())
                sb.Append(osUTF8Const.XMLelement_item_id_Empty);
            else
            {
                sb.Append(osUTF8Const.XMLelement_item_id_Start);
                sb.AppendUUID(e);
                sb.Append(osUTF8Const.XMLuuidEnd);
            }
        }

        public static void AddElem_category_id(UUID e, osUTF8 sb)
        {
            if (e.IsZero())
                sb.Append(osUTF8Const.XMLelement_category_id_Empty);
            else
            {
                sb.Append(osUTF8Const.XMLelement_category_id_Start);
                sb.AppendUUID(e);
                sb.Append(osUTF8Const.XMLuuidEnd);
            }
        }

        public static void AddElem_creator_id(UUID e, osUTF8 sb)
        {
            if (e.IsZero())
                sb.Append(osUTF8Const.XMLelement_creator_id_Empty);
            else
            {
                sb.Append(osUTF8Const.XMLelement_creator_id_Start);
                sb.AppendUUID(e);
                sb.Append(osUTF8Const.XMLuuidEnd);
            }
        }

        public static void AddElem_group_id(UUID e, osUTF8 sb)
        {
            if (e.IsZero())
                sb.Append(osUTF8Const.XMLelement_group_id_Empty);
            else
            {
                sb.Append(osUTF8Const.XMLelement_group_id_Start);
                sb.AppendUUID(e);
                sb.Append(osUTF8Const.XMLuuidEnd);
            }
        }

        public static void AddElem_version(int v, osUTF8 sb)
        {
            if (v == 0)
                sb.Append(osUTF8Const.XMLelement_version_Empty);
            else
            {
                sb.Append(osUTF8Const.XMLelement_version_Start);
                sb.AppendInt(v);
                sb.Append(osUTF8Const.XMLintegerEnd);
            }
        }

        public static void AddElem_sale_info(int price, byte type, osUTF8 sb)
        {
            if(price == 0  && type == 0)
                sb.Append(osUTF8Const.XMLelement_sale_info_Empty);
            else
            {
                sb.Append(osUTF8Const.XMLelement_sale_info_Start);
                sb.AppendInt(price);
                sb.Append(osUTF8Const.XMLelement_sale_info_Mid);
                sb.AppendInt(type);
                sb.Append(osUTF8Const.XMLelement_sale_info_End);
            }
        }     


        public static byte[] ulongToByteArray(ulong uLongValue)
        {
            return new byte[8]
            {
                (byte)(uLongValue >> 56),
                (byte)(uLongValue >> 48),
                (byte)(uLongValue >> 40),
                (byte)(uLongValue >> 32),
                (byte)(uLongValue >> 24),
                (byte)(uLongValue >> 16),
                (byte)(uLongValue >> 8),
                (byte)uLongValue
            };
        }

        public static byte[] uintToByteArray(uint value)
        {
            return new byte[4]
            {
                (byte)(value >> 24),
                (byte)(value >> 16),
                (byte)(value >> 8),
                (byte)value
            };
        }
    }
}
