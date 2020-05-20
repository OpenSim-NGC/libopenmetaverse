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
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Text;

namespace OpenMetaverse.StructuredData
{
    /// <summary>
    /// 
    /// </summary>
    public static partial class OSDParser
    {
        private static XmlSchema XmlSchema;
        private static XmlTextReader XmlTextReader;
        private static string LastXmlErrors = String.Empty;
        private static object XmlValidationLock = new object();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlData"></param>
        /// <returns></returns>
        public static OSD DeserializeLLSDXml(byte[] xmlData)
        {
            using(XmlTextReader xrd =  new XmlTextReader(new MemoryStream(xmlData, false)))
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
            byte[] bytes = Utils.StringToBytes(xmlData);
            using(XmlTextReader xrd = new XmlTextReader(new MemoryStream(bytes, false)))
                return DeserializeLLSDXml(xrd);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlData"></param>
        /// <returns></returns>
        public static OSD DeserializeLLSDXml(XmlTextReader xmlData)
        {
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

        public static byte[] SerializeLLSDXmlBytes(OSD data, bool formal = false)
        {
            return Encoding.UTF8.GetBytes(SerializeLLSDXmlString(data, formal));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string SerializeLLSDXmlString(OSD data, bool formal = false)
        {
            StringBuilder sb = new StringBuilder(16384);
            if(formal)
                sb.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");

            sb.Append("<llsd>");
            SerializeLLSDXmlElement(sb, data, formal);
            sb.Append("</llsd>");

            return sb.ToString();
        }

        public static string SerializeLLSDInnerXmlString(OSD data,bool formal = false)
        {
            StringBuilder sb = new StringBuilder(16384);
            SerializeLLSDXmlElement(sb, data, formal);

            return sb.ToString();
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

        /*
        // lazy keywords but more readable than direct bytes
        static readonly byte[] undefBytes = Encoding.UTF8.GetBytes("<undef/>");
        static readonly byte[] booleanOneB = Encoding.UTF8.GetBytes("<boolean>1</boolean>");
        static readonly byte[] booleanZeroB = Encoding.UTF8.GetBytes("<boolean>0</boolean>");
        static readonly byte[] integerB = Encoding.UTF8.GetBytes("<integer>");
        static readonly byte[] integerBend = Encoding.UTF8.GetBytes("</integer>");
        static readonly byte[] realB = Encoding.UTF8.GetBytes("<real>");
        static readonly byte[] realBend = Encoding.UTF8.GetBytes("</real>");
        static readonly byte[] stringB = Encoding.UTF8.GetBytes("<string>");
        static readonly byte[] stringBend = Encoding.UTF8.GetBytes("</string>");
        static readonly byte[] uuidB = Encoding.UTF8.GetBytes("<uuid>");
        static readonly byte[] uuidBend = Encoding.UTF8.GetBytes("</uuid>");
        static readonly byte[] dateB = Encoding.UTF8.GetBytes("<date>");
        static readonly byte[] dateBend = Encoding.UTF8.GetBytes("</date>");
        static readonly byte[] uriB = Encoding.UTF8.GetBytes("<uri>");
        static readonly byte[] uriBend = Encoding.UTF8.GetBytes("</uri>");
        static readonly byte[] formalBinaryB = Encoding.UTF8.GetBytes("<binary encoding=\"base64\">");
        static readonly byte[] binaryB = Encoding.UTF8.GetBytes("<binary>");
        static readonly byte[] binaryBend = Encoding.UTF8.GetBytes("</binary>");
        static readonly byte[] mapB = Encoding.UTF8.GetBytes("<map>");
        static readonly byte[] mapBend = Encoding.UTF8.GetBytes("</map>");
        static readonly byte[] keyB = Encoding.UTF8.GetBytes("<key>");
        static readonly byte[] keyBend = Encoding.UTF8.GetBytes("</key>");
        static readonly byte[] arrayB = Encoding.UTF8.GetBytes("<array>");
        static readonly byte[] arrayBend = Encoding.UTF8.GetBytes("</array>");
        static readonly byte[] ltB = Encoding.UTF8.GetBytes("&lt;");
        static readonly byte[] gtB = Encoding.UTF8.GetBytes("&gt;");
        static readonly byte[] ampB = Encoding.UTF8.GetBytes("&amp;");
        static readonly byte[] quotB = Encoding.UTF8.GetBytes("&quot;");
        static readonly byte[] aposB = Encoding.UTF8.GetBytes("&apos;");


        static readonly byte[] base64Bytes = {(byte)'A',(byte)'B',(byte)'C',(byte)'D',(byte)'E',(byte)'F',(byte)'G',(byte)'H',(byte)'I',(byte)'J',(byte)'K',(byte)'L',(byte)'M',(byte)'N',(byte)'O',
                                              (byte)'P',(byte)'Q',(byte)'R',(byte)'S',(byte)'T',(byte)'U',(byte)'V',(byte)'W',(byte)'X',(byte)'Y',(byte)'Z',(byte)'a',(byte)'b',(byte)'c',(byte)'d',
                                              (byte)'e',(byte)'f',(byte)'g',(byte)'h',(byte)'i',(byte)'j',(byte)'k',(byte)'l',(byte)'m',(byte)'n',(byte)'o',(byte)'p',(byte)'q',(byte)'r',(byte)'s',
                                              (byte)'t',(byte)'u',(byte)'v',(byte)'w',(byte)'x',(byte)'y',(byte)'z',(byte)'0',(byte)'1',(byte)'2',(byte)'3',(byte)'4',(byte)'5',(byte)'6',(byte)'7',
                                              (byte)'8',(byte)'9',(byte)'+',(byte)'/'};


        private static void writeOSDString(MemoryStream mb, OSD data)
        {
            byte[] tmp = Encoding.UTF8.GetBytes(data.AsString());
            mb.Write(tmp,0,tmp.Length);
        }

        public static void EscapeToXML(MemoryStream ms, string s)
        {
            int i;
            char c;
            int len = s.Length;

            for (i = 0; i < len; i++)
            {
                c = s[i];
                switch (c)
                {
                    case '<':
                        ms.Write(ltB, 0, ltB.Length);
                        break;
                    case '>':
                        ms.Write(gtB, 0, gtB.Length);
                        break;
                    case '&':
                        ms.Write(ampB, 0, ampB.Length);
                        break;
                    case '"':
                        ms.Write(quotB, 0, quotB.Length);
                        break;
                    case '\\':
                        ms.Write(aposB, 0, aposB.Length);
                        break;
                    default:
                        ms.WriteByte((byte)c);
                        break;
                }
            }
        }

        public static void SerializeLLSDXmlElement(MemoryStream mb, OSD data, bool formal)
        {
            switch (data.Type)
            {
                case OSDType.Unknown:
                    mb.Write(undefBytes, 0, undefBytes.Length);
                    break;
                case OSDType.Boolean:
                    if(data.AsBoolean())
                        mb.Write(booleanOneB, 0, booleanOneB.Length);
                    else
                        mb.Write(booleanZeroB, 0, booleanZeroB.Length);
                    break;
                case OSDType.Integer:
                    mb.Write(integerB, 0, integerB.Length);
                    writeOSDString(mb, data);
                    mb.Write(integerBend, 0, integerBend.Length);
                    break;
                case OSDType.Real:
                    mb.Write(realB, 0, realB.Length);
                    writeOSDString(mb, data);
                    mb.Write(realBend, 0, realBend.Length);
                    break;
                case OSDType.String:
                    mb.Write(stringB, 0, stringB.Length);
                    writeOSDString(mb, data);
                    mb.Write(stringBend, 0, stringBend.Length);
                    break;
                case OSDType.UUID:
                    mb.Write(uuidB, 0, uuidB.Length);
                    writeOSDString(mb, data);
                    mb.Write(uuidBend, 0, uuidBend.Length);
                    break;
                case OSDType.Date:
                    mb.Write(dateB, 0, dateB.Length);
                    writeOSDString(mb, data);
                    mb.Write(dateBend, 0, dateBend.Length);
                    break;
                case OSDType.URI:
                    mb.Write(uriB, 0, uriB.Length);
                    writeOSDString(mb, data);
                    mb.Write(uriBend, 0, uriBend.Length);
                    break;
                case OSDType.Binary:
                    if (formal)
                        mb.Write(formalBinaryB, 0, formalBinaryB.Length);
                    else
                        mb.Write(binaryB, 0, binaryB.Length);
                    base64Encode(data.AsBinary(), mb);
                    mb.Write(binaryBend, 0, binaryBend.Length);
                    break;
                case OSDType.Map:
                    mb.Write(mapB, 0, mapB.Length);
                    foreach (KeyValuePair<string, OSD> kvp in (OSDMap)data)
                    {
                        mb.Write(keyB, 0, keyB.Length);
                        writeOSDString(mb, kvp.Key);
                        mb.Write(keyBend, 0, keyBend.Length);

                        SerializeLLSDXmlElement(mb, kvp.Value, formal);
                    }
                    mb.Write(mapBend, 0, mapBend.Length);
                    break;
                case OSDType.Array:
                    OSDArray array = (OSDArray)data;
                    mb.Write(arrayB, 0, arrayB.Length);
                    for (int i = 0; i < array.Count; i++)
                    {
                        SerializeLLSDXmlElement(mb, array[i], formal);
                    }
                    mb.Write(arrayBend, 0, arrayBend.Length);
                    break;
                case OSDType.LLSDxml:
                    writeOSDString(mb, data.AsString());
                    break;
                default:
                    break;
            }
        }

         public static unsafe void base64Encode(byte[] data, MemoryStream mb)
        {
            int lenMod3 = data.Length % 3;
            int len = data.Length - lenMod3;

            fixed (byte* d = data, b64 = base64Bytes)
            {
                int i = 0;
                while (i < len)
                {
                    mb.WriteByte(b64[d[i] >> 2]);
                    mb.WriteByte(b64[((d[i] & 0x03) << 4) | ((d[i + 1] & 0xf0) >> 4)]);
                    mb.WriteByte(b64[((d[i + 1] & 0x0f) << 2) | ((d[i + 2] & 0xc0) >> 6)]);
                    mb.WriteByte(b64[d[i + 2] & 0x3f]);
                    i += 3;
                }

                switch (lenMod3)
                {
                    case 2:
                    {
                        i = len;
                        mb.WriteByte(b64[d[i] >> 2]);
                        mb.WriteByte(b64[((d[i] & 0x03) << 4) | ((d[i + 1] & 0xf0) >> 4)]);
                        mb.WriteByte(b64[((d[i + 1] & 0x0f) << 2)]);
                        mb.WriteByte((byte)'=');
                        break;
                    }
                    case 1:
                    {
                        i = len;
                        mb.WriteByte(b64[d[i] >> 2]);
                        mb.WriteByte(b64[(d[i] & 0x03) << 4]);
                        mb.WriteByte((byte)'=');
                        mb.WriteByte((byte)'=');
                        break;
                    }
                }
            }
        }
        */

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
        /// <param name="xmlData"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public static bool TryValidateLLSDXml(XmlTextReader xmlData, out string error)
        {
            lock (XmlValidationLock)
            {
                LastXmlErrors = String.Empty;
                XmlTextReader = xmlData;

                CreateLLSDXmlSchema();

                XmlReaderSettings readerSettings = new XmlReaderSettings();
                readerSettings.ValidationType = ValidationType.Schema;
                readerSettings.Schemas.Add(XmlSchema);
                readerSettings.ValidationEventHandler += new ValidationEventHandler(LLSDXmlSchemaValidationHandler);

                using(XmlReader reader = XmlReader.Create(xmlData, readerSettings))
                {

                    try
                    {
                        while (reader.Read()) { }
                    }
                    catch (XmlException)
                    {
                        error = LastXmlErrors;
                        return false;
                    }

                    if (LastXmlErrors == String.Empty)
                    {
                        error = null;
                        return true;
                    }
                    else
                    {
                        error = LastXmlErrors;
                        return false;
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
                        return OSD.FromBinary(Utils.EmptyBytes);
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

                    ret = OSD.FromBinary(Utils.EmptyBytes);
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

        private static void CreateLLSDXmlSchema()
        {
            if (XmlSchema == null)
            {
                #region XSD
                string schemaText = @"
<?xml version=""1.0"" encoding=""utf-8""?>
<xs:schema elementFormDefault=""qualified"" xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
  <xs:import schemaLocation=""xml.xsd"" namespace=""http://www.w3.org/XML/1998/namespace"" />
  <xs:element name=""uri"" type=""xs:string"" />
  <xs:element name=""uuid"" type=""xs:string"" />
  <xs:element name=""KEYDATA"">
    <xs:complexType>
      <xs:sequence>
        <xs:element ref=""key"" />
        <xs:element ref=""DATA"" />
      </xs:sequence>
    </xs:complexType>
  </xs:element>
  <xs:element name=""date"" type=""xs:string"" />
  <xs:element name=""key"" type=""xs:string"" />
  <xs:element name=""boolean"" type=""xs:string"" />
  <xs:element name=""undef"">
    <xs:complexType>
      <xs:sequence>
        <xs:element ref=""EMPTY"" />
      </xs:sequence>
    </xs:complexType>
  </xs:element>
  <xs:element name=""map"">
    <xs:complexType>
      <xs:sequence>
        <xs:element minOccurs=""0"" maxOccurs=""unbounded"" ref=""KEYDATA"" />
      </xs:sequence>
    </xs:complexType>
  </xs:element>
  <xs:element name=""real"" type=""xs:string"" />
  <xs:element name=""ATOMIC"">
    <xs:complexType>
      <xs:choice>
        <xs:element ref=""undef"" />
        <xs:element ref=""boolean"" />
        <xs:element ref=""integer"" />
        <xs:element ref=""real"" />
        <xs:element ref=""uuid"" />
        <xs:element ref=""string"" />
        <xs:element ref=""date"" />
        <xs:element ref=""uri"" />
        <xs:element ref=""binary"" />
      </xs:choice>
    </xs:complexType>
  </xs:element>
  <xs:element name=""DATA"">
    <xs:complexType>
      <xs:choice>
        <xs:element ref=""ATOMIC"" />
        <xs:element ref=""map"" />
        <xs:element ref=""array"" />
      </xs:choice>
    </xs:complexType>
  </xs:element>
  <xs:element name=""llsd"">
    <xs:complexType>
      <xs:sequence>
        <xs:element ref=""DATA"" />
      </xs:sequence>
    </xs:complexType>
  </xs:element>
  <xs:element name=""binary"">
    <xs:complexType>
      <xs:simpleContent>
        <xs:extension base=""xs:string"">
          <xs:attribute default=""base64"" name=""encoding"" type=""xs:string"" />
        </xs:extension>
      </xs:simpleContent>
    </xs:complexType>
  </xs:element>
  <xs:element name=""array"">
    <xs:complexType>
      <xs:sequence>
        <xs:element minOccurs=""0"" maxOccurs=""unbounded"" ref=""DATA"" />
      </xs:sequence>
    </xs:complexType>
  </xs:element>
  <xs:element name=""integer"" type=""xs:string"" />
  <xs:element name=""string"">
    <xs:complexType>
      <xs:simpleContent>
        <xs:extension base=""xs:string"">
          <xs:attribute ref=""xml:space"" />
        </xs:extension>
      </xs:simpleContent>
    </xs:complexType>
  </xs:element>
</xs:schema>
";
                #endregion XSD

                MemoryStream stream = new MemoryStream(Encoding.ASCII.GetBytes(schemaText));

                XmlSchema = new XmlSchema();
                XmlSchema = XmlSchema.Read(stream, new ValidationEventHandler(LLSDXmlSchemaValidationHandler));
            }
        }

        private static void LLSDXmlSchemaValidationHandler(object sender, ValidationEventArgs args)
        {
            string error = String.Format("Line: {0} - Position: {1} - {2}", XmlTextReader.LineNumber, XmlTextReader.LinePosition,
                args.Message);

            if (LastXmlErrors == String.Empty)
                LastXmlErrors = error;
            else
                LastXmlErrors += Environment.NewLine + error;
        }
    }
}
