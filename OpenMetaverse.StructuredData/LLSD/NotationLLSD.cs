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
using System.Text;
using System.IO;

namespace OpenMetaverse.StructuredData
{
    /// <summary>
    /// 
    /// </summary>
    public static partial class OSDParser
    {
        private const string baseIndent = "  ";

        private const char undefNotationValue = '!';

        private const char trueNotationValueOne = '1';
        private const char trueNotationValueTwo = 't';
        private static readonly char[] trueNotationValueTwoFull = { 't', 'r', 'u', 'e' };
        private const char trueNotationValueThree = 'T';
        private static readonly char[] trueNotationValueThreeFull = { 'T', 'R', 'U', 'E' };

        private const char falseNotationValueOne = '0';
        private const char falseNotationValueTwo = 'f';
        private static readonly char[] falseNotationValueTwoFull = { 'f', 'a', 'l', 's', 'e' };
        private const char falseNotationValueThree = 'F';
        private static readonly char[] falseNotationValueThreeFull = { 'F', 'A', 'L', 'S', 'E' };

        private const char integerNotationMarker = 'i';
        private const char realNotationMarker = 'r';
        private const char uuidNotationMarker = 'u';
        private const char binaryNotationMarker = 'b';
        private const char stringNotationMarker = 's';
        private const char uriNotationMarker = 'l';
        private const char dateNotationMarker = 'd';

        private const char arrayBeginNotationMarker = '[';
        private const char arrayEndNotationMarker = ']';

        private const char mapBeginNotationMarker = '{';
        private const char mapEndNotationMarker = '}';
        private const char kommaNotationDelimiter = ',';
        private const char keyNotationDelimiter = ':';

        private const char sizeBeginNotationMarker = '(';
        private const char sizeEndNotationMarker = ')';
        private const char doubleQuotesNotationMarker = '"';
        private const char singleQuotesNotationMarker = '\'';

        public static OSD DeserializeLLSDNotation(string notationData)
        {
            using (StringReader reader = new StringReader(notationData))
            {
                OSD osd = DeserializeLLSDNotationStart(reader);
                return osd;
            }
        }

        public static OSD DeserializeLLSDNotation(byte[] xmlData)
        {
            using(MemoryStream ms = new MemoryStream(xmlData))
            using (StreamReader xrd = new StreamReader(ms))
            {
                return DeserializeLLSDNotationStart(xrd);
            }
        }

        public static OSD DeserializeLLSDNotation(Stream sreader)
        {
            using (StreamReader reader = new StreamReader(sreader))
            {
                OSD osd = DeserializeLLSDNotationStart(reader);
                return osd;
            }
        }

        public static OSD DeserializeLLSDNotation(StringReader reader)
        {
            OSD osd = DeserializeLLSDNotationStart(reader);
            return osd;
        }

        public static string SerializeLLSDNotation(OSD osd)
        {
            using (StringWriter writer = SerializeLLSDNotationStream(osd))
            {
                string s = writer.ToString();
                return s;
            }
        }

        public static byte[] SerializeLLSDNotationToBytes(OSD osd, bool header = false)
        {
            using(MemoryStream ms = new MemoryStream())
            using (StreamWriter writer = new StreamWriter(ms))
            {
                if(header)
                    writer.Write("<? llsd/notation ?>");
                SerializeLLSDNotationElement(writer, osd);
                writer.Flush();
                return ms.ToArray();
            }
        }

        public static string SerializeLLSDNotationFull(OSD osd)
        {
            using(StringWriter writer = new StringWriter())
            {
                writer.Write("<? llsd/notation ?>");
                SerializeLLSDNotationElement(writer, osd);
                return writer.ToString();
            }
        }

        public static StringWriter SerializeLLSDNotationStream(OSD osd)
        {
            StringWriter writer = new StringWriter();
            SerializeLLSDNotationElement(writer, osd);
            return writer;
        }

        public static string SerializeLLSDNotationFormatted(OSD osd)
        {
            StringWriter writer = SerializeLLSDNotationStreamFormatted(osd);
            string s = writer.ToString();
            writer.Close();

            return s;
        }

        public static StringWriter SerializeLLSDNotationStreamFormatted(OSD osd)
        {
            StringWriter writer = new StringWriter();

            string indent = String.Empty;
            SerializeLLSDNotationElementFormatted(writer, indent, osd);
            return writer;
        }


        private static OSD DeserializeLLSDNotationStart(TextReader reader)
        {
            int character = PeekAndSkipWhitespace(reader);
            if (character < 0)
                return new OSD();
            if(character == '<')
                reader.ReadBlock(new char[19], 0, 19);
            return DeserializeLLSDNotationElement(reader);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private static OSD DeserializeLLSDNotationElement(TextReader reader)
        {
            int character = ReadAndSkipWhitespace(reader);
            if (character < 0)
                return new OSD(); // server returned an empty file, so we're going to pass along a null LLSD object

            OSD osd;
            int matching;
            switch ((char)character)
            {
                case undefNotationValue:
                    osd = new OSD();
                    break;
                case trueNotationValueOne:
                    osd = OSD.FromBoolean(true);
                    break;
                case trueNotationValueTwo:
                    matching = BufferCharactersEqual(reader, trueNotationValueTwoFull, 1);
                    if (matching > 1 && matching < trueNotationValueTwoFull.Length)
                        throw new OSDException("Notation LLSD parsing: True value parsing error:");
                    osd = OSD.FromBoolean(true);
                    break;
                case trueNotationValueThree:
                    matching = BufferCharactersEqual(reader, trueNotationValueThreeFull, 1);
                    if (matching > 1 && matching < trueNotationValueThreeFull.Length)
                        throw new OSDException("Notation LLSD parsing: True value parsing error:");
                    osd = OSD.FromBoolean(true);
                    break;
                case falseNotationValueOne:
                    osd = OSD.FromBoolean(false);
                    break;
                case falseNotationValueTwo:
                    matching = BufferCharactersEqual(reader, falseNotationValueTwoFull, 1);
                    if (matching > 1 && matching < falseNotationValueTwoFull.Length)
                        throw new OSDException("Notation LLSD parsing: True value parsing error:");
                    osd = OSD.FromBoolean(false);
                    break;
                case falseNotationValueThree:
                    matching = BufferCharactersEqual(reader, falseNotationValueThreeFull, 1);
                    if (matching > 1 && matching < falseNotationValueThreeFull.Length)
                        throw new OSDException("Notation LLSD parsing: True value parsing error:");
                    osd = OSD.FromBoolean(false);
                    break;
                case integerNotationMarker:
                    osd = DeserializeLLSDNotationInteger(reader);
                    break;
                case realNotationMarker:
                    osd = DeserializeLLSDNotationReal(reader);
                    break;
                case uuidNotationMarker:
                    char[] uuidBuf = new char[36];
                    if (reader.Read(uuidBuf, 0, 36) < 36)
                        throw new OSDException("Notation LLSD parsing: Unexpected end of stream in UUID.");
                    UUID lluuid;
                    if (!UUID.TryParse(new String(uuidBuf), out lluuid))
                        throw new OSDException("Notation LLSD parsing: Invalid UUID discovered.");
                    osd = OSD.FromUUID(lluuid);
                    break;
                case binaryNotationMarker:
                    byte[] bytes = Utils.EmptyBytes;
                    int bChar = reader.Peek();
                    if (bChar < 0)
                        throw new OSDException("Notation LLSD parsing: Unexpected end of stream in binary.");
                    if ((char)bChar == sizeBeginNotationMarker)
                    {
                        throw new OSDException("Notation LLSD parsing: Raw binary encoding not supported.");
                    }
                    else if (Char.IsDigit((char)bChar))
                    {
                        char[] charsBaseEncoding = new char[2];
                        if (reader.Read(charsBaseEncoding, 0, 2) < 2)
                            throw new OSDException("Notation LLSD parsing: Unexpected end of stream in binary.");
                        int baseEncoding;
                        if (!Int32.TryParse(new String(charsBaseEncoding), out baseEncoding))
                            throw new OSDException("Notation LLSD parsing: Invalid binary encoding base.");
                        if (baseEncoding == 64)
                        {
                            if (reader.Read() < 0)
                                throw new OSDException("Notation LLSD parsing: Unexpected end of stream in binary.");
                            string bytes64 = GetStringDelimitedBy(reader, doubleQuotesNotationMarker);
                            bytes = Convert.FromBase64String(bytes64);
                        }
                        else
                        {
                            throw new OSDException("Notation LLSD parsing: Encoding base" + baseEncoding + " + not supported.");
                        }
                    }
                    osd = OSD.FromBinary(bytes);
                    break;
                case stringNotationMarker:
                    int numChars = GetLengthInBrackets(reader);
                    if (reader.Read() < 0)
                        throw new OSDException("Notation LLSD parsing: Unexpected end of stream in string.");
                    char[] chars = new char[numChars];
                    if (reader.Read(chars, 0, numChars) < numChars)
                        throw new OSDException("Notation LLSD parsing: Unexpected end of stream in string.");
                    if (reader.Read() < 0)
                        throw new OSDException("Notation LLSD parsing: Unexpected end of stream in string.");
                    osd = OSD.FromString(new String(chars));
                    break;
                case singleQuotesNotationMarker:
                    string sOne = GetStringDelimitedBy(reader, singleQuotesNotationMarker);
                    osd = OSD.FromString(sOne);
                    break;
                case doubleQuotesNotationMarker:
                    string sTwo = GetStringDelimitedBy(reader, doubleQuotesNotationMarker);
                    osd = OSD.FromString(sTwo);
                    break;
                case uriNotationMarker:
                    if (reader.Read() < 0)
                        throw new OSDException("Notation LLSD parsing: Unexpected end of stream in string.");
                    string sUri = GetStringDelimitedBy(reader, doubleQuotesNotationMarker);

                    Uri uri;
                    try
                    {
                        uri = new Uri(sUri, UriKind.RelativeOrAbsolute);
                    }
                    catch
                    {
                        throw new OSDException("Notation LLSD parsing: Invalid Uri format detected.");
                    }
                    osd = OSD.FromUri(uri);
                    break;
                case dateNotationMarker:
                    if (reader.Read() < 0)
                        throw new OSDException("Notation LLSD parsing: Unexpected end of stream in date.");
                    string date = GetStringDelimitedBy(reader, doubleQuotesNotationMarker);
                    DateTime dt;
                    if (!DateTime.TryParse(date, out dt))
                        throw new OSDException("Notation LLSD parsing: Invalid date discovered.");
                    osd = OSD.FromDate(dt);
                    break;
                case arrayBeginNotationMarker:
                    osd = DeserializeLLSDNotationArray(reader);
                    break;
                case mapBeginNotationMarker:
                    osd = DeserializeLLSDNotationMap(reader);
                    break;
                default:
                    throw new OSDException("Notation LLSD parsing: Unknown type marker '" + (char)character + "'.");
            }
            return osd;
        }

        private static OSD DeserializeLLSDNotationInteger(TextReader reader)
        {
            char character;
            bool neg = false;
            if (((character = (char)reader.Peek()) > 0) && (character == '-'))
            {
                neg = true;
                reader.Read();
            }
            int integer = 0;
            while ((character = (char)reader.Peek()) > 0 &&  Char.IsDigit(character))
            {
                integer *= 10;
                integer += character - '0';
                reader.Read();
            }
            if(neg)
                return OSD.FromInteger(-integer);
            return OSD.FromInteger(integer);
        }

        private static OSD DeserializeLLSDNotationReal(TextReader reader)
        {
            int character;
            StringBuilder s = new StringBuilder();
            if (((character = reader.Peek()) > 0) &&
                ((char)character == '-' || (char)character == '+'))
            {
                s.Append((char)character);
                reader.Read();
            }
            while (((character = reader.Peek()) > 0) &&
                   (Char.IsDigit((char)character) || (char)character == '.' ||
                    (char)character == 'e' || (char)character == 'E' ||
                    (char)character == '+' || (char)character == '-'))
            {
                s.Append((char)character);
                reader.Read();
            }
            double dbl;
            if (!Utils.TryParseDouble(s.ToString(), out dbl))
                throw new OSDException("Notation LLSD parsing: Can't parse real value: " + s.ToString());

            return OSD.FromReal(dbl);
        }

        private static OSD DeserializeLLSDNotationArray(TextReader reader)
        {
            int character;
            OSDArray osdArray = new OSDArray();
            while (((character = PeekAndSkipWhitespace(reader)) > 0))
            {
                 if((char)character == arrayEndNotationMarker)
                 {
                     reader.Read();
                     break;
                 }
                 else
                 {
                    osdArray.Add(DeserializeLLSDNotationElement(reader));

                    character = ReadAndSkipWhitespace(reader);
                    if (character < 0)
                        throw new OSDException("Notation LLSD parsing: Unexpected end of array discovered.");
                    else if ((char)character == kommaNotationDelimiter)
                        continue;
                    else if ((char)character == arrayEndNotationMarker)
                        break;
                 }
            }
            if (character < 0)
                throw new OSDException("Notation LLSD parsing: Unexpected end of array discovered.");

            return (OSD)osdArray;
        }

        private static OSD DeserializeLLSDNotationMap(TextReader reader)
        {
            int character;
            OSDMap osdMap = new OSDMap();
            while (((character = PeekAndSkipWhitespace(reader)) > 0))
            {
                if ((char)character == mapEndNotationMarker)
                {
                    reader.Read();
                    break;
                }
                else
                {
                    OSD osdKey = DeserializeLLSDNotationElement(reader);
                    if (osdKey.Type != OSDType.String)
                        throw new OSDException("Notation LLSD parsing: Invalid key in map");
                    string key = osdKey.AsString();

                    character = ReadAndSkipWhitespace(reader);
                    if ((char)character != keyNotationDelimiter)
                        throw new OSDException("Notation LLSD parsing: Unexpected end of stream in map.");
                    if ((char)character != keyNotationDelimiter)
                        throw new OSDException("Notation LLSD parsing: Invalid delimiter in map.");

                    osdMap[key] = DeserializeLLSDNotationElement(reader);
                    character = ReadAndSkipWhitespace(reader);
                    if (character < 0)
                        throw new OSDException("Notation LLSD parsing: Unexpected end of map discovered.");
                    else if ((char)character == kommaNotationDelimiter)
                        continue;
                    else if ((char)character == mapEndNotationMarker)
                        break;
                }
            }
            if (character < 0)
                throw new OSDException("Notation LLSD parsing: Unexpected end of map discovered.");

            return (OSD)osdMap;
        }

        private static void SerializeLLSDNotationElement(TextWriter writer, OSD osd)
        {

            switch (osd.Type)
            {
                case OSDType.Unknown:
                    writer.Write(undefNotationValue);
                    break;
                case OSDType.Boolean:
                    if (osd.AsBoolean())
                        writer.Write(trueNotationValueTwo);
                    else
                        writer.Write(falseNotationValueTwo);
                    break;
                case OSDType.Integer:
                    writer.Write(integerNotationMarker);
                    writer.Write(osd.AsString());
                    break;
                case OSDType.Real:
                    writer.Write(realNotationMarker);
                    writer.Write(osd.AsString());
                    break;
                case OSDType.UUID:
                    writer.Write(uuidNotationMarker);
                    writer.Write(osd.AsString());
                    break;
                case OSDType.String:
                    writer.Write(singleQuotesNotationMarker);
                    EscapeCharacter(osd.AsString(), singleQuotesNotationMarker, writer);
                    writer.Write(singleQuotesNotationMarker);
                    break;
                case OSDType.Binary:
                    writer.Write(binaryNotationMarker);
                    writer.Write("64");
                    writer.Write(doubleQuotesNotationMarker);
                    base64Encode(osd.AsBinary(), writer);
                    writer.Write(doubleQuotesNotationMarker);
                    break;
                case OSDType.Date:
                    writer.Write(dateNotationMarker);
                    writer.Write(doubleQuotesNotationMarker);
                    writer.Write(osd.AsString());
                    writer.Write(doubleQuotesNotationMarker);
                    break;
                case OSDType.URI:
                    writer.Write(uriNotationMarker);
                    writer.Write(doubleQuotesNotationMarker);
                    writer.Write(osd.AsString());
                    writer.Write(doubleQuotesNotationMarker);
                    break;
                case OSDType.Array:
                    SerializeLLSDNotationArray(writer, (OSDArray)osd);
                    break;
                case OSDType.Map:
                    SerializeLLSDNotationMap(writer, (OSDMap)osd);
                    break;
                default:
                    throw new OSDException("Notation serialization: Not existing element discovered.");
            }
        }

        public static unsafe void base64Encode(byte[] data, TextWriter tw)
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
                        tw.Write(b64[d[i] >> 2]);
                        tw.Write(b64[((d[i] & 0x03) << 4) | ((d[i + 1] & 0xf0) >> 4)]);
                        tw.Write(b64[((d[i + 1] & 0x0f) << 2) | ((d[i + 2] & 0xc0) >> 6)]);
                        tw.Write(b64[d[i + 2] & 0x3f]);
                        i += 3;
                    }

                    switch (lenMod3)
                    {
                        case 2:
                        {
                            i = len;
                            tw.Write(b64[d[i] >> 2]);
                            tw.Write(b64[((d[i] & 0x03) << 4) | ((d[i + 1] & 0xf0) >> 4)]);
                            tw.Write(b64[((d[i + 1] & 0x0f) << 2)]);
                            tw.Write('=');
                            break;
                        }
                        case 1:
                        {
                            i = len;
                            tw.Write(b64[d[i] >> 2]);
                            tw.Write(b64[(d[i] & 0x03) << 4]);
                            tw.Write("==");
                            break;
                        }
                    }
                }
            }
        }

        private static void SerializeLLSDNotationArray(TextWriter writer, OSDArray osdArray)
        {
            writer.Write(arrayBeginNotationMarker);
            int lastIndex = osdArray.Count - 1;

            for (int idx = 0; idx <= lastIndex; idx++)
            {
                SerializeLLSDNotationElement(writer, osdArray[idx]);
                if (idx < lastIndex)
                    writer.Write(kommaNotationDelimiter);
            }
            writer.Write(arrayEndNotationMarker);
        }

        private static void SerializeLLSDNotationMap(TextWriter writer, OSDMap osdMap)
        {
            writer.Write(mapBeginNotationMarker);
            int lastIndex = osdMap.Count - 1;
            int idx = 0;

            foreach (KeyValuePair<string, OSD> kvp in osdMap)
            {
                writer.Write(singleQuotesNotationMarker);
                writer.Write(EscapeCharacter(kvp.Key, singleQuotesNotationMarker));
                writer.Write(singleQuotesNotationMarker);
                writer.Write(keyNotationDelimiter);
                SerializeLLSDNotationElement(writer, kvp.Value);
                if (idx < lastIndex)
                    writer.Write(kommaNotationDelimiter);

                idx++;
            }
            writer.Write(mapEndNotationMarker);
        }

        private static void SerializeLLSDNotationElementFormatted(TextWriter writer, string indent, OSD osd)
        {
            switch (osd.Type)
            {
                case OSDType.Unknown:
                    writer.Write(undefNotationValue);
                    break;
                case OSDType.Boolean:
                    if (osd.AsBoolean())
                        writer.Write(trueNotationValueTwo);
                    else
                        writer.Write(falseNotationValueTwo);
                    break;
                case OSDType.Integer:
                    writer.Write(integerNotationMarker);
                    writer.Write(osd.AsString());
                    break;
                case OSDType.Real:
                    writer.Write(realNotationMarker);
                    writer.Write(osd.AsString());
                    break;
                case OSDType.UUID:
                    writer.Write(uuidNotationMarker);
                    writer.Write(osd.AsString());
                    break;
                case OSDType.String:
                    writer.Write(singleQuotesNotationMarker);
                    EscapeCharacter(osd.AsString(), singleQuotesNotationMarker, writer);
                    writer.Write(singleQuotesNotationMarker);
                    break;
                case OSDType.Binary:
                    writer.Write(binaryNotationMarker);
                    writer.Write("64");
                    writer.Write(doubleQuotesNotationMarker);
                    writer.Write(osd.AsString());
                    writer.Write(doubleQuotesNotationMarker);
                    break;
                case OSDType.Date:
                    writer.Write(dateNotationMarker);
                    writer.Write(doubleQuotesNotationMarker);
                    writer.Write(osd.AsString());
                    writer.Write(doubleQuotesNotationMarker);
                    break;
                case OSDType.URI:
                    writer.Write(uriNotationMarker);
                    writer.Write(doubleQuotesNotationMarker);
                    writer.Write(osd.AsString());
                    writer.Write(doubleQuotesNotationMarker);
                    break;
                case OSDType.Array:
                    SerializeLLSDNotationArrayFormatted(writer, indent + baseIndent, (OSDArray)osd);
                    break;
                case OSDType.Map:
                    SerializeLLSDNotationMapFormatted(writer, indent + baseIndent, (OSDMap)osd);
                    break;
                default:
                    throw new OSDException("Notation serialization: Not existing element discovered.");

            }
        }

        private static void SerializeLLSDNotationArrayFormatted(TextWriter writer, string intend, OSDArray osdArray)
        {
            writer.WriteLine();
            writer.Write(intend);
            writer.Write(arrayBeginNotationMarker);
            int lastIndex = osdArray.Count - 1;

            for (int idx = 0; idx <= lastIndex; idx++)
            {
                if (osdArray[idx].Type != OSDType.Array && osdArray[idx].Type != OSDType.Map)
                    writer.WriteLine();
                writer.Write(intend + baseIndent);
                SerializeLLSDNotationElementFormatted(writer, intend, osdArray[idx]);
                if (idx < lastIndex)
                {
                    writer.Write(kommaNotationDelimiter);
                }
            }
            writer.WriteLine();
            writer.Write(intend);
            writer.Write(arrayEndNotationMarker);
        }

        private static void SerializeLLSDNotationMapFormatted(TextWriter writer, string intend, OSDMap osdMap)
        {
            writer.WriteLine();
            writer.Write(intend);
            writer.WriteLine(mapBeginNotationMarker);
            int lastIndex = osdMap.Count - 1;
            int idx = 0;

            foreach (KeyValuePair<string, OSD> kvp in osdMap)
            {
                writer.Write(intend + baseIndent);
                writer.Write(singleQuotesNotationMarker);
                //writer.Write(EscapeCharacter(kvp.Key, singleQuotesNotationMarker));
                writer.Write(kvp.Key);
                writer.Write(singleQuotesNotationMarker);
                writer.Write(keyNotationDelimiter);
                SerializeLLSDNotationElementFormatted(writer, intend, kvp.Value);
                if (idx < lastIndex)
                {
                    writer.WriteLine();
                    writer.Write(intend + baseIndent);
                    writer.WriteLine(kommaNotationDelimiter);
                }

                idx++;
            }
            writer.WriteLine();
            writer.Write(intend);
            writer.Write(mapEndNotationMarker);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static int PeekAndSkipWhitespace(TextReader reader)
        {
            int character;
            while ((character = reader.Peek()) > 0)
            {
                char c = (char)character;
                if (c == ' ' || c == '\t' || c == '\n' || c == '\r')
                {
                    reader.Read();
                    continue;
                }
                else
                    break;
            }
            return character;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static int ReadAndSkipWhitespace(TextReader reader)
        {
            int character = PeekAndSkipWhitespace(reader);
            reader.Read();
            return character;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static int GetLengthInBrackets(TextReader reader)
        {
            int character;
            StringBuilder s = new StringBuilder();
            if (((character = PeekAndSkipWhitespace(reader)) > 0) &&
                     ((char)character == sizeBeginNotationMarker))
            {
                reader.Read();
            }
            while (((character = reader.Read()) > 0) &&
                    Char.IsDigit((char)character) &&
                  ((char)character != sizeEndNotationMarker))
            {
                s.Append((char)character);
            }
            if (character < 0)
                throw new OSDException("Notation LLSD parsing: Can't parse length value cause unexpected end of stream.");
            int length;
            if (!Int32.TryParse(s.ToString(), out length))
                throw new OSDException("Notation LLSD parsing: Can't parse length value.");

            return length;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="delimiter"></param>
        /// <returns></returns>
        public static string GetStringDelimitedBy(TextReader reader, char delimiter)
        {
            bool gotescape = false;
            bool gothex = false;
            bool gotnibble = false;

            int b = 0;
            char c;

            StringBuilder sb = new StringBuilder();
            while ((c = (char)reader.Read()) > 0) 
            {
                if (gotescape)
                {
                    if (gothex)
                    {
                        if (gotnibble)
                        {
                            b <<= 4;
                            b |= Utils.HexNibble(c);
                            sb.Append((char)b);
                            b = 0;
                            gothex = false;
                            gotnibble = false;
                            gotescape = false;
                            continue;
                        }
                        b = Utils.HexNibble(c);
                        gotnibble = true;
                        continue;
                    }
                    if(c == 'x')
                    {
                        gothex = true;
                        continue;
                    }
                    switch (c)
                    {
                        case 'a':
                            sb.Append('\a');
                            break;
                        case 'b':
                            sb.Append('\b');
                            break;
                        case 'f':
                            sb.Append('\f');
                            break;
                        case 'n':
                            sb.Append('\n');
                            break;
                        case 'r':
                            sb.Append('\r');
                            break;
                        case 't':
                            sb.Append('\t');
                            break;
                        case 'v':
                            sb.Append('\v');
                            break;
                        default:
                            sb.Append(c);
                            break;
                    }
                    gotescape = false;
                    continue;
                }

                if(c == delimiter)
                    break;

                if (c == '\\')
                {
                    gotescape = true;
                    continue;
                }
                sb.Append(c);
            }
            if (c < 0)
                throw new OSDException("Notation LLSD parsing: Can't parse text because unexpected end of stream while expecting a '"
                                            + delimiter + "' character.");

            return sb.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static int BufferCharactersEqual(TextReader reader, char[] buffer, int offset)
        {
            int character;
            int lastIndex = buffer.Length - 1;
            int crrIndex = offset;
            bool charactersEqual = true;

            while ((character = reader.Peek()) > 0 &&
                    crrIndex <= lastIndex &&
                    charactersEqual)
            {
                if (((char)character) != buffer[crrIndex])
                {
                    charactersEqual = false;
                    break;
                }
                crrIndex++;
                reader.Read();
            }

            return crrIndex;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static string UnescapeCharacter(String s, char d)
        {
            bool gotescape = false;
            bool gothex = false;
            bool gothexnibble = false;
            int b = 0;

            StringBuilder sb = new StringBuilder(s.Length);
            for (int i = 0; i < s.Length; ++i)
            {
                char c = s[i];
                if(gotescape)
                {
                    if(gothex)
                    {
                        if(gothexnibble)
                        {
                            gothex = false;
                            gothexnibble = false;
                            gotescape = false;
                            b <<= 4;
                            b |= Utils.HexNibble(c);
                            sb.Append((char)b);
                            b = 0;
                            continue;
                        }
                        b = Utils.HexNibble(c);
                        gothexnibble = true;
                        continue;
                    }
                    switch (c)
                    {
                        case 'x':
                            gothex = true;
                            continue;
                        case 'a':
                            sb.Append('\a');
                            break;
                        case 'b':
                            sb.Append('\b');
                            break;
                        case 'f':
                            sb.Append('\f');
                            break;
                        case 'n':
                            sb.Append('\n');
                            break;
                        case 'r':
                            sb.Append('\r');
                            break;
                        case 't':
                            sb.Append('\t');
                            break;
                        case 'v':
                            sb.Append('\v');
                            break;
                        default:
                            sb.Append(c);
                            break;
                    }
                    gotescape = false;
                    continue;
                }
                if (c == '\\')
                {
                    gotescape = true;
                    continue;
                }
                sb.Append(c);
            }
            return sb.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static string EscapeCharacter(String s, char d)
        {
            StringBuilder sb = new StringBuilder(s.Length);
            for(int i = 0; i < s.Length;++i)
            {
                char c = s[i];
                if (c == d)
                {
                    sb.Append("\\");
                    sb.Append(d);
                }
                else
                {
                    switch (c)
                    {
                        case '\\':
                            sb.Append("\\\\");
                            break;
                        default:
                            sb.Append(c);
                            break;
                    }
                }
            }
            return sb.ToString();
        }

        public static void EscapeCharacter(String s, char d, TextWriter tw)
        {
            for (int i = 0; i < s.Length; ++i)
            {
                char c = s[i];
                if (c == d)
                {
                    tw.Write("\\");
                    tw.Write(d);
                }
                else
                {
                    switch (c)
                    {
                        case '\\':
                            tw.Write("\\\\");
                            break;
                        default:
                            tw.Write(c);
                            break;
                    }
                }
            }
            return;
        }

    }
}
