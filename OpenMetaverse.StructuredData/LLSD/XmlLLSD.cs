/*
 * Copyright (c) 2006-2016, openmetaverse.co
 * Copyright (c) OpenSimulator Contributors, http://opensimulator.org/
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
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Text;
using System.Runtime.InteropServices;
using OpenMetaverse;

namespace OpenMetaverse.StructuredData
{
    /// <summary>
    /// 
    /// </summary>
    public static partial class OSDParser
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlData"></param>
        /// <returns></returns>
        public static OSD DeserializeLLSDXml(byte[] xmlData)
        {
            using(MemoryStream ms =  new MemoryStream(xmlData))
            using(XmlTextReader xrd =  new XmlTextReader(ms))
                return DeserializeLLSDXml(xrd);
        }

        public static OSD DeserializeLLSDXml(Stream xmlStream)
        {
            using(XmlTextReader xrd = new XmlTextReader(xmlStream))
                return DeserializeLLSDXml(xrd);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlData"></param>
        /// <returns></returns>
        public static OSD DeserializeLLSDXml(string xmlData)
        {
            using (StringReader sr = new StringReader(xmlData))
            using(XmlTextReader xrd = new XmlTextReader(sr))
                return DeserializeLLSDXml(xrd);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlData"></param>
        /// <returns></returns>
        public static OSD DeserializeLLSDXml(XmlTextReader xmlData)
        {
            xmlData.DtdProcessing = DtdProcessing.Ignore;
 
            try
            {
                xmlData.Read();
                SkipWhitespace(xmlData);

                xmlData.Read();
                OSD ret = ParseLLSDXmlElement(xmlData);

                return ret;
            }
            catch
            {
                return new OSD();
            }
        }

        public static byte[] SerializeLLSDXmlToBytes(OSD data, bool formal = false)
        {
            osUTF8 tmp = OSUTF8Cached.Acquire();
            if (formal)
                tmp.Append(osUTF8Const.XMLformalHeaderllsdstart);
            else
                tmp.Append(osUTF8Const.XMLllsdStart);
            SerializeLLSDXmlElement(tmp, data, formal);
            tmp.Append(osUTF8Const.XMLllsdEnd);

            return OSUTF8Cached.GetArrayAndRelease(tmp);
        }

        public static byte[] SerializeLLSDXmlToBytes(OSD data)
        {
            osUTF8 tmp = OSUTF8Cached.Acquire();

            tmp.Append(osUTF8Const.XMLllsdStart);
            SerializeLLSDXmlElement(tmp, data, false);
            tmp.Append(osUTF8Const.XMLllsdEnd);

            return OSUTF8Cached.GetArrayAndRelease(tmp);
        }

        public static byte[] SerializeInnerLLSDXmlToBytes(OSD data)
        {
            osUTF8 tmp = OSUTF8Cached.Acquire();
            SerializeLLSDXmlElement(tmp, data, false);

            return OSUTF8Cached.GetArrayAndRelease(tmp);
        }

        public static byte[] SerializeLLSDXmlBytes(OSD data, bool formal = false)
        {
            return SerializeLLSDXmlToBytes(data, formal);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string SerializeLLSDXmlString(OSD data, bool formal = false)
        {
            StringBuilder sb = osStringBuilderCache.Acquire();
            if(formal)
                sb.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");

            sb.Append("<llsd>");
            SerializeLLSDXmlElement(sb, data, formal);
            sb.Append("</llsd>");

            return osStringBuilderCache.GetStringAndRelease(sb);
        }

        public static string SerializeLLSDInnerXmlString(OSD data, bool formal = false)
        {
            StringBuilder sb = osStringBuilderCache.Acquire();
            SerializeLLSDXmlElement(sb, data, formal);

            return osStringBuilderCache.GetStringAndRelease(sb);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="data"></param>
        public static void SerializeLLSDXmlElement(StringBuilder sb, OSD data, bool formal)
        {
            switch (data.Type)
            {
                case OSDType.Unknown:
                    sb.Append("<undef />");
                    break;
                case OSDType.Boolean:
                    if(data.AsBoolean())
                        sb.Append("<boolean>1</boolean>");
                    else
                        sb.Append("<boolean>0</boolean>");
                    break;
                case OSDType.Integer:
                    sb.Append("<integer>");
                    sb.Append(data.AsString());
                    sb.Append("</integer>");
                    break;
                case OSDType.Real:
                    sb.Append("<real>");
                    sb.Append(data.AsString());
                    sb.Append("</real>");
                    break;
                case OSDType.String:
                    sb.Append("<string>");
                    EscapeToXML(data.AsString(), sb);
                    sb.Append("</string>");
                    break;
                case OSDType.UUID:
                    sb.Append("<uuid>");
                    sb.Append(data.AsString());
                    sb.Append("</uuid>");
                    break;
                case OSDType.Date:
                    sb.Append("<date>");
                    sb.Append(data.AsString());
                    sb.Append("</date>");
                    break;
                case OSDType.URI:
                    sb.Append("<uri>");
                    sb.Append(data.AsString());
                    sb.Append("</uri>");
                    break;
                case OSDType.Binary:
                    if(formal)
                        sb.Append("<binary encoding=\"base64\">");
                    else
                        sb.Append("<binary>");
                    base64Encode(data.AsBinary(), sb);
                    sb.Append("</binary>");
                    break;
                case OSDType.Map:
                    OSDMap map = (OSDMap)data;
                    sb.Append("<map>");
                    foreach (KeyValuePair<string, OSD> kvp in map)
                    {
                        sb.Append("<key>");
                        sb.Append(kvp.Key);
                        sb.Append("</key>");

                        SerializeLLSDXmlElement(sb, kvp.Value, formal);
                    }
                    sb.Append("</map>");
                    break;
                case OSDType.Array:
                    OSDArray array = (OSDArray)data;
                    sb.Append("<array>");
                    for (int i = 0; i < array.Count; i++)
                    {
                        SerializeLLSDXmlElement(sb, array[i], formal);
                    }
                    sb.Append("</array>");
                    break;
                case OSDType.LLSDxml:
                    sb.Append(data.AsString());
                    break;
                default:
                    break;
            }
        }

        public static void EscapeToXML(string s, StringBuilder sb)
        {
            char c;
            for (int i = 0; i < s.Length; ++i)
            {
                c = s[i];
                switch (c)
                {
                    case '<':
                        sb.Append("&lt;");
                        break;
                    case '>':
                        sb.Append("&gt;");
                        break;
                    case '&':
                        sb.Append("&amp;");
                        break;
                    case '"':
                        sb.Append("&quot;");
                        break;
                    case '\\':
                        sb.Append("&apos;");
                        break;
                    default:
                        sb.Append(c);
                        break;
                }
            }
        }

        public static void EscapeASCIIToXML(osUTF8 ms, string s)
        {
            char c;
            for (int i = 0; i < s.Length; i++)
            {
                c = s[i];
                switch (c)
                {
                    case '<':
                        ms.Append(osUTF8Const.XMLamp_lt);
                        break;
                    case '>':
                        ms.Append(osUTF8Const.XMLamp_gt);
                        break;
                    case '&':
                        ms.Append(osUTF8Const.XMLamp);
                        break;
                    case '"':
                        ms.Append(osUTF8Const.XMLamp_quot);
                        break;
                    case '\\':
                        ms.Append(osUTF8Const.XMLamp_apos);
                        break;
                    default:
                        ms.AppendASCII(c);
                        break;
                }
            }
        }

        public static void EscapeToXML(osUTF8 ms, string s)
        {
            char c;
            for (int i = 0; i <  s.Length; i++)
            {
                c = s[i];
                switch (c)
                {
                    case '<':
                        ms.Append(osUTF8Const.XMLamp_lt);
                        break;
                    case '>':
                        ms.Append(osUTF8Const.XMLamp_gt);
                        break;
                    case '&':
                        ms.Append(osUTF8Const.XMLamp);
                        break;
                    case '"':
                        ms.Append(osUTF8Const.XMLamp_quot);
                        break;
                    case '\\':
                        ms.Append(osUTF8Const.XMLamp_apos);
                        break;
                    default:
                        ms.AppendCharBytes(c, ref s, ref i);
                        break;
                }
            }
        }

        public static void EscapeToXML(osUTF8 ms, osUTF8 s)
        {
            byte c;
            for (int i = 0; i < s.Length; i++)
            {
                c = s[i];
                switch (c)
                {
                    case (byte)'<':
                        ms.Append(osUTF8Const.XMLamp_lt);
                        break;
                    case (byte)'>':
                        ms.Append(osUTF8Const.XMLamp_gt);
                        break;
                    case (byte)'&':
                        ms.Append(osUTF8Const.XMLamp);
                        break;
                    case (byte)'"':
                        ms.Append(osUTF8Const.XMLamp_quot);
                        break;
                    case (byte)'\\':
                        ms.Append(osUTF8Const.XMLamp_apos);
                        break;
                    default:
                        ms.Append(c);
                        break;
                }
            }
        }

        public static void SerializeLLSDXmlElement(osUTF8 mb, OSD data, bool formal)
        {
            switch (data.Type)
            {
                case OSDType.Unknown:
                    mb.Append(osUTF8Const.XMLundef);
                    break;
                case OSDType.Boolean:
                    if(data.AsBoolean())
                        mb.Append(osUTF8Const.XMLfullbooleanOne);
                    else
                        mb.Append(osUTF8Const.XMLfullbooleanZero);
                    break;
                case OSDType.Integer:
                    mb.Append(osUTF8Const.XMLintegerStart);
                    mb.AppendInt(data.AsInteger());
                    mb.Append(osUTF8Const.XMLintegerEnd);
                    break;
                case OSDType.Real:
                    mb.Append(osUTF8Const.XMLrealStart);
                    mb.AppendASCII(data.ToString());
                    mb.Append(osUTF8Const.XMLrealEnd);
                    break;
                case OSDType.String:
                    mb.Append(osUTF8Const.XMLstringStart);
                    EscapeToXML(mb, data);
                    mb.Append(osUTF8Const.XMLstringEnd);
                    break;
                case OSDType.UUID:
                    mb.Append(osUTF8Const.XMLuuidStart);
                    mb.AppendUUID(data.AsUUID());
                    mb.Append(osUTF8Const.XMLuuidEnd);
                    break;
                case OSDType.Date:
                    mb.Append(osUTF8Const.XMLdateStart);
                    mb.AppendASCII(data.ToString());
                    mb.Append(osUTF8Const.XMLdateEnd);
                    break;
                case OSDType.URI:
                    mb.Append(osUTF8Const.XMLuriStart);
                    EscapeToXML(mb, data.ToString());
                    mb.Append(osUTF8Const.XMLuriEnd);
                    break;
                case OSDType.Binary:
                    if (formal)
                        mb.Append(osUTF8Const.XMLformalBinaryStart);
                    else
                        mb.Append(osUTF8Const.XMLbinaryStart);
                    base64Encode(data.AsBinary(), mb);
                    mb.Append(osUTF8Const.XMLbinaryEnd);
                    break;
                case OSDType.Map:
                    mb.Append(osUTF8Const.XMLmapStart);
                    foreach (KeyValuePair<string, OSD> kvp in (OSDMap)data)
                    {
                        mb.Append(osUTF8Const.XMLkeyStart);
                        mb.Append(kvp.Key.ToString());
                        mb.Append(osUTF8Const.XMLkeyEnd);

                        SerializeLLSDXmlElement(mb, kvp.Value, formal);
                    }
                    mb.Append(osUTF8Const.XMLmapEnd);
                    break;
                case OSDType.Array:
                    OSDArray array = (OSDArray)data;
                    mb.Append(osUTF8Const.XMLarrayStart);
                    for (int i = 0; i < array.Count; i++)
                    {
                        SerializeLLSDXmlElement(mb, array[i], formal);
                    }
                    mb.Append(osUTF8Const.XMLarrayEnd);
                    break;
                case OSDType.LLSDxml:
                    mb.Append(data.AsString());
                    break;
                default:
                    break;
            }
        }

        public static unsafe void base64Encode(byte[] data, osUTF8 mb)
        {
            int lenMod3 = data.Length % 3;
            int len = data.Length - lenMod3;

            mb.CheckCapacity(4 * data.Length / 3);

            fixed (byte* d = data, b64 = osUTF8Const.base64Bytes)
            {
                int i = 0;
                while (i < len)
                {
                    mb.Append(b64[d[i] >> 2]);
                    mb.Append(b64[((d[i] & 0x03) << 4) | ((d[i + 1] & 0xf0) >> 4)]);
                    mb.Append(b64[((d[i + 1] & 0x0f) << 2) | ((d[i + 2] & 0xc0) >> 6)]);
                    mb.Append(b64[d[i + 2] & 0x3f]);
                    i += 3;
                }

                switch (lenMod3)
                {
                    case 2:
                    {
                        i = len;
                        mb.Append(b64[d[i] >> 2]);
                        mb.Append(b64[((d[i] & 0x03) << 4) | ((d[i + 1] & 0xf0) >> 4)]);
                        mb.Append(b64[((d[i + 1] & 0x0f) << 2)]);
                        mb.Append((byte)'=');
                        break;
                    }
                    case 1:
                    {
                        i = len;
                        mb.Append(b64[d[i] >> 2]);
                        mb.Append(b64[(d[i] & 0x03) << 4]);
                        mb.Append((byte)'=');
                        mb.Append((byte)'=');
                        break;
                    }
                }
            }
        }

        public static unsafe void base64Encode(byte[] data, int start, int lenght, osUTF8 mb)
        {
            int lenMod3 = lenght % 3;
            int len = lenght - lenMod3;

            fixed (byte* d = &data[start], b64 = osUTF8Const.base64Bytes)
            {
                int i = 0;
                while (i < len)
                {
                    mb.Append(b64[d[i] >> 2]);
                    mb.Append(b64[((d[i] & 0x03) << 4) | ((d[i + 1] & 0xf0) >> 4)]);
                    mb.Append(b64[((d[i + 1] & 0x0f) << 2) | ((d[i + 2] & 0xc0) >> 6)]);
                    mb.Append(b64[d[i + 2] & 0x3f]);
                    i += 3;
                }

                switch (lenMod3)
                {
                    case 2:
                    {
                        i = len;
                        mb.Append(b64[d[i] >> 2]);
                        mb.Append(b64[((d[i] & 0x03) << 4) | ((d[i + 1] & 0xf0) >> 4)]);
                        mb.Append(b64[((d[i + 1] & 0x0f) << 2)]);
                        mb.Append((byte)'=');
                        break;
                    }
                    case 1:
                    {
                        i = len;
                        mb.Append(b64[d[i] >> 2]);
                        mb.Append(b64[(d[i] & 0x03) << 4]);
                        mb.Append((byte)'=');
                        mb.Append((byte)'=');
                        break;
                    }
                }
            }
        }

        static readonly char[] base64Chars = {'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O',
                                              'P','Q','R','S','T','U','V','W','X','Y','Z','a','b','c','d',
                                              'e','f','g','h','i','j','k','l','m','n','o','p','q','r','s',
                                              't','u','v','w','x','y','z','0','1','2','3','4','5','6','7',
                                              '8','9','+','/'};

        public static unsafe void base64Encode(byte[] data, StringBuilder sb)
        {
            int lenMod3 = data.Length % 3;
            int len = data.Length - lenMod3;

            fixed (byte* d = data)
            {
                fixed (char* b64 = base64Chars)
                {
                    int i = 0;
                    while (i < len)
                    {
                        sb.Append(b64[d[i] >> 2]);
                        sb.Append(b64[((d[i] & 0x03) << 4) | ((d[i + 1] & 0xf0) >> 4)]);
                        sb.Append(b64[((d[i + 1] & 0x0f) << 2) | ((d[i + 2] & 0xc0) >> 6)]);
                        sb.Append(b64[d[i + 2] & 0x3f]);
                        i += 3;
                    }

                    switch (lenMod3)
                    {
                        case 2:
                        {
                            i = len;
                            sb.Append(b64[d[i] >> 2]);
                            sb.Append(b64[((d[i] & 0x03) << 4) | ((d[i + 1] & 0xf0) >> 4)]);
                            sb.Append(b64[((d[i + 1] & 0x0f) << 2)]);
                            sb.Append('=');
                            break;
                        }
                        case 1:
                        {
                            i = len;
                            sb.Append(b64[d[i] >> 2]);
                            sb.Append(b64[(d[i] & 0x03) << 4]);
                            sb.Append("==");
                            break;
                        }
                    }
                }
            }
        }


   /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private static OSD ParseLLSDXmlElement(XmlTextReader reader)
        {
            SkipWhitespace(reader);

            if (reader.NodeType != XmlNodeType.Element)
                throw new OSDException("Expected an element");

            string type = reader.LocalName;
            OSD ret;

            switch (type)
            {
                case "undef":
                    if (reader.IsEmptyElement)
                    {
                        reader.Read();
                        return new OSD();
                    }

                    reader.Read();
                    SkipWhitespace(reader);
                    ret = new OSD();
                    break;
                case "boolean":
                    if (reader.IsEmptyElement)
                    {
                        reader.Read();
                        return OSD.FromBoolean(false);
                    }

                    if (reader.Read())
                    {
                        string s = reader.ReadString().Trim();

                        if (!String.IsNullOrEmpty(s) && (s == "true" || s == "1"))
                        {
                            ret = OSD.FromBoolean(true);
                            break;
                        }
                    }

                    ret = OSD.FromBoolean(false);
                    break;
                case "integer":
                    if (reader.IsEmptyElement)
                    {
                        reader.Read();
                        return OSD.FromInteger(0);
                    }

                    if (reader.Read())
                    {
                        int value = 0;
                        Int32.TryParse(reader.ReadString().Trim(), out value);
                        ret = OSD.FromInteger(value);
                        break;
                    }

                    ret = OSD.FromInteger(0);
                    break;
                case "real":
                    if (reader.IsEmptyElement)
                    {
                        reader.Read();
                        return OSD.FromReal(0d);
                    }

                    if (reader.Read())
                    {
                        double value = 0d;
                        string str = reader.ReadString().Trim().ToLower();

                        if (str == "nan")
                            value = Double.NaN;
                        else
                            Utils.TryParseDouble(str, out value);

                        ret = OSD.FromReal(value);
                        break;
                    }

                    ret = OSD.FromReal(0d);
                    break;
                case "uuid":
                    if (reader.IsEmptyElement)
                    {
                        reader.Read();
                        return OSD.FromUUID(UUID.Zero);
                    }

                    if (reader.Read())
                    {
                        UUID value = UUID.Zero;
                        UUID.TryParse(reader.ReadString().Trim(), out value);
                        ret = OSD.FromUUID(value);
                        break;
                    }

                    ret = OSD.FromUUID(UUID.Zero);
                    break;
                case "date":
                    if (reader.IsEmptyElement)
                    {
                        reader.Read();
                        return OSD.FromDate(Utils.Epoch);
                    }

                    if (reader.Read())
                    {
                        DateTime value = Utils.Epoch;
                        DateTime.TryParse(reader.ReadString().Trim(), out value);
                        ret = OSD.FromDate(value);
                        break;
                    }

                    ret = OSD.FromDate(Utils.Epoch);
                    break;
                case "string":
                    if (reader.IsEmptyElement)
                    {
                        reader.Read();
                        return OSD.FromString(String.Empty);
                    }

                    if (reader.Read())
                    {
                        ret = OSD.FromString(reader.ReadString());
                        break;
                    }

                    ret = OSD.FromString(String.Empty);
                    break;
                case "binary":
                    if (reader.IsEmptyElement)
                    {
                        reader.Read();
                        return OSD.FromBinary(Array.Empty<byte>());
                    }

                    if (reader.GetAttribute("encoding") != null && reader.GetAttribute("encoding") != "base64")
                        throw new OSDException("Unsupported binary encoding: " + reader.GetAttribute("encoding"));

                    if (reader.Read())
                    {
                        try
                        {
                            ret = OSD.FromBinary(Convert.FromBase64String(reader.ReadString().Trim()));
                            break;
                        }
                        catch (FormatException ex)
                        {
                            throw new OSDException("Binary decoding exception: " + ex.Message);
                        }
                    }

                    ret = OSD.FromBinary(Array.Empty<byte>());
                    break;
                case "uri":
                    if (reader.IsEmptyElement)
                    {
                        reader.Read();
                        return OSD.FromUri(new Uri(String.Empty, UriKind.RelativeOrAbsolute));
                    }

                    if (reader.Read())
                    {
                        ret = OSD.FromUri(new Uri(reader.ReadString(), UriKind.RelativeOrAbsolute));
                        break;
                    }

                    ret = OSD.FromUri(new Uri(String.Empty, UriKind.RelativeOrAbsolute));
                    break;
                case "map":
                    return ParseLLSDXmlMap(reader);
                case "array":
                    return ParseLLSDXmlArray(reader);
                default:
                    reader.Read();
                    ret = null;
                    break;
            }

            if (reader.NodeType != XmlNodeType.EndElement || reader.LocalName != type)
            {
                throw new OSDException("Expected </" + type + ">");
            }
            else
            {
                reader.Read();
                return ret;
            }
        }

        private static OSDMap ParseLLSDXmlMap(XmlTextReader reader)
        {
            if (reader.NodeType != XmlNodeType.Element || reader.LocalName != "map")
                throw new NotImplementedException("Expected <map>");

            OSDMap map = new OSDMap();

            if (reader.IsEmptyElement)
            {
                reader.Read();
                return map;
            }

            if (reader.Read())
            {
                while (true)
                {
                    SkipWhitespace(reader);

                    if (reader.NodeType == XmlNodeType.EndElement && reader.LocalName == "map")
                    {
                        reader.Read();
                        break;
                    }

                    if (reader.NodeType != XmlNodeType.Element || reader.LocalName != "key")
                        throw new OSDException("Expected <key>");

                    string key = reader.ReadString();

                    if (reader.NodeType != XmlNodeType.EndElement || reader.LocalName != "key")
                        throw new OSDException("Expected </key>");

                    if (reader.Read())
                        map[key] = ParseLLSDXmlElement(reader);
                    else
                        throw new OSDException("Failed to parse a value for key " + key);
                }
            }

            return map;
        }

        private static OSDArray ParseLLSDXmlArray(XmlTextReader reader)
        {
            if (reader.NodeType != XmlNodeType.Element || reader.LocalName != "array")
                throw new OSDException("Expected <array>");

            OSDArray array = new OSDArray();

            if (reader.IsEmptyElement)
            {
                reader.Read();
                return array;
            }

            if (reader.Read())
            {
                while (true)
                {
                    SkipWhitespace(reader);

                    if (reader.NodeType == XmlNodeType.EndElement && reader.LocalName == "array")
                    {
                        reader.Read();
                        break;
                    }

                    array.Add(ParseLLSDXmlElement(reader));
                }
            }

            return array;
        }        

        private static void SkipWhitespace(XmlTextReader reader)
        {
            while (
                reader.NodeType == XmlNodeType.Comment ||
                reader.NodeType == XmlNodeType.Whitespace ||
                reader.NodeType == XmlNodeType.SignificantWhitespace ||
                reader.NodeType == XmlNodeType.XmlDeclaration)
            {
                reader.Read();
            }
        }
    }
}
 
