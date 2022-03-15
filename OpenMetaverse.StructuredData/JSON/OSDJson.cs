using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Text;
using LitJson;
using OpenMetaverse;

namespace OpenMetaverse.StructuredData
{
    public static partial class OSDParser
    {
        public static OSD DeserializeJson(Stream json)
        {
            using (StreamReader streamReader = new StreamReader(json))
            {
                JsonReader reader = new JsonReader(streamReader);
                return DeserializeJson(JsonMapper.ToObject(reader));
            }
        }

        public static OSD DeserializeJson(byte[] data)
        {
            return DeserializeJson(JsonMapper.ToObject(Encoding.UTF8.GetString(data)));
        }

        public static OSD DeserializeJson(string json)
        {
            return DeserializeJson(JsonMapper.ToObject(json));
        }

        public static OSD DeserializeJson(JsonData json)
        {
            if (json == null) return new OSD();

            switch (json.GetJsonType())
            {
                case JsonType.Boolean:
                    return OSD.FromBoolean((bool)json);
                case JsonType.Int:
                    return OSD.FromInteger((int)json);
                case JsonType.Long:
                    return OSD.FromLong((long)json);
                case JsonType.Double:
                    return OSD.FromReal((double)json);
                case JsonType.String:
                    string str = (string)json;
                    if (String.IsNullOrEmpty(str))
                        return new OSD();
                    else
                        return OSD.FromString(str);
                case JsonType.Array:
                    OSDArray array = new OSDArray(json.Count);
                    for (int i = 0; i < json.Count; i++)
                        array.Add(DeserializeJson(json[i]));
                    return array;
                case JsonType.Object:
                    OSDMap map = new OSDMap(json.Count);
                    IDictionaryEnumerator e = ((IOrderedDictionary)json).GetEnumerator();
                    while (e.MoveNext())
                        map.Add((string)e.Key, DeserializeJson((JsonData)e.Value));
                    return map;
                case JsonType.None:
                default:
                    return new OSD();
            }
        }

        /*
        public static string SerializeJsonString(OSD osd)
        {
            return SerializeJson(osd, false).ToJson();
        }

        public static byte[] SerializeJsonToBytes(OSD osd)
        {
            return Encoding.UTF8.GetBytes(SerializeJson(osd, false).ToJson());
        }

        public static string SerializeJsonString(OSD osd, bool preserveDefaults)
        {
            return SerializeJson(osd, preserveDefaults).ToJson();
        }
        */
        public static void SerializeJsonString(OSD osd, bool preserveDefaults, ref JsonWriter writer)
        {
            SerializeJson(osd, preserveDefaults).ToJson(writer);
        }

        public static JsonData SerializeJson(OSD osd, bool preserveDefaults)
        {
            switch (osd.Type)
            {
                case OSDType.Boolean:
                    return new JsonData(osd.AsBoolean());
                case OSDType.Integer:
                    return new JsonData(osd.AsInteger());
                case OSDType.Real:
                    return new JsonData(osd.AsReal());
                case OSDType.String:
                case OSDType.Date:
                case OSDType.URI:
                case OSDType.UUID:
                case OSDType.OSDUTF8:
                case OSDType.LLSDxml:
                    return new JsonData(osd.AsString());
                case OSDType.Binary:
                    byte[] binary = osd.AsBinary();
                    JsonData jsonbinarray = new JsonData();
                    jsonbinarray.SetJsonType(JsonType.Array);
                    for (int i = 0; i < binary.Length; i++)
                        jsonbinarray.Add(new JsonData(binary[i]));
                    return jsonbinarray;
                case OSDType.Array:
                    JsonData jsonarray = new JsonData();
                    jsonarray.SetJsonType(JsonType.Array);
                    OSDArray array = (OSDArray)osd;
                    for (int i = 0; i < array.Count; i++)
                        jsonarray.Add(SerializeJson(array[i], preserveDefaults));
                    return jsonarray;
                case OSDType.Map:
                    JsonData jsonmap = new JsonData();
                    jsonmap.SetJsonType(JsonType.Object);
                    OSDMap map = (OSDMap)osd;
                    foreach (KeyValuePair<string, OSD> kvp in map)
                    {
                        JsonData data = preserveDefaults ? SerializeJson(kvp.Value, true) : SerializeJsonNoDefaults(kvp.Value);
                        if (data != null)
                            jsonmap[kvp.Key] = data;
                    }
                    return jsonmap;
                case OSDType.Unknown:
                default:
                    return new JsonData(null);
            }
        }

        private static JsonData SerializeJsonNoDefaults(OSD osd)
        {
            switch (osd.Type)
            {
                case OSDType.Boolean:
                    bool b = osd.AsBoolean();
                    if (!b)
                        return null;

                    return new JsonData(b);
                case OSDType.Integer:
                    int v = osd.AsInteger();
                    if (v == 0)
                        return null;

                    return new JsonData(v);
                case OSDType.Real:
                    double d = osd.AsReal();
                    if (d == 0.0d)
                        return null;

                    return new JsonData(d);
                case OSDType.String:
                case OSDType.Date:
                case OSDType.URI:
                case OSDType.OSDUTF8:
                case OSDType.LLSDxml:
                    string str = osd.AsString();
                    if (String.IsNullOrEmpty(str))
                        return null;

                    return new JsonData(str);
                case OSDType.UUID:
                    UUID uuid = osd.AsUUID();
                    if (uuid.IsZero())
                        return null;

                    return new JsonData(uuid.ToString());
                case OSDType.Binary:
                    byte[] binary = osd.AsBinary();
                    if (binary.Length == 0)
                        return null;

                    JsonData jsonbinarray = new JsonData();
                    jsonbinarray.SetJsonType(JsonType.Array);
                    for (int i = 0; i < binary.Length; i++)
                        jsonbinarray.Add(new JsonData(binary[i]));
                    return jsonbinarray;
                case OSDType.Array:
                    JsonData jsonarray = new JsonData();
                    jsonarray.SetJsonType(JsonType.Array);
                    OSDArray array = (OSDArray)osd;
                    for (int i = 0; i < array.Count; i++)
                        jsonarray.Add(SerializeJson(array[i], false));
                    return jsonarray;
                case OSDType.Map:
                    JsonData jsonmap = new JsonData();
                    jsonmap.SetJsonType(JsonType.Object);
                    OSDMap map = (OSDMap)osd;
                    foreach (KeyValuePair<string, OSD> kvp in map)
                    {
                        JsonData data = SerializeJsonNoDefaults(kvp.Value);
                        if (data != null)
                            jsonmap[kvp.Key] = data;
                    }
                    return jsonmap;
                case OSDType.Unknown:
                default:
                    return null;
            }
        }

        public static string SerializeJsonString(OSD osd)
        {
            osUTF8 sb = OSUTF8Cached.Acquire();
            SerializeJson(osd, sb, false);
            return OSUTF8Cached.GetStringAndRelease(sb);
        }

        public static byte[] SerializeJsonToBytes(OSD osd)
        {
            osUTF8 sb = OSUTF8Cached.Acquire();
            SerializeJson(osd, sb, false);
            return OSUTF8Cached.GetArrayAndRelease(sb);
        }

        public static string SerializeJsonString(OSD osd, bool preserveDefaults)
        {
            osUTF8 sb = OSUTF8Cached.Acquire();
            SerializeJson(osd, sb, preserveDefaults);
            return OSUTF8Cached.GetStringAndRelease(sb);
        }

        public static void SerializeJson(OSD osd, osUTF8 sb, bool preserveDefaults)
        {
            int i;
            switch (osd.Type)
            {
                case OSDType.Boolean:
                    sb.Append(osd.AsBoolean() ? osUTF8Const.OSUTF8true : osUTF8Const.OSUTF8false);
                    break;
                case OSDType.Integer:
                    sb.AppendInt(osd.AsInteger());
                    break;
                case OSDType.Real:
                    string str = Convert.ToString(osd.AsReal(), NumberFormatInfo.InvariantInfo);
                    sb.AppendASCII(str);

                    if (str.IndexOfAny(new char[]{'.','E' }) == -1)
                        sb.AppendASCII(".0");
                    break;
                case OSDType.String:
                case OSDType.URI:
                case OSDType.LLSDxml:
                    appendJsonString((OSDString)osd, sb);
                    break;
                case OSDType.OSDUTF8:
                    osUTF8 ou8 = ((OSDUTF8)osd).value;
                    appendJsonOSUTF8(ou8, sb);
                    break;
                case OSDType.UUID:
                    sb.AppendASCII('"');
                    sb.AppendUUID(osd.AsUUID());
                    sb.AppendASCII('"');
                    break;
                case OSDType.Date:
                    appendJsonString(osd.AsString(), sb);
                    break;
                case OSDType.Binary:
                    byte[] binary = osd.AsBinary();
                    i = 0;
                    sb.AppendASCII('[');
                    while (i < binary.Length - 1)
                    {
                        sb.AppendInt(binary[i++]);
                        sb.AppendASCII(',');
                    }
                    if (i < binary.Length)
                        sb.AppendInt(binary[i]);
                    sb.AppendASCII(']');
                    break;
                case OSDType.Array:
                    sb.AppendASCII('[');
                    OSDArray array = (OSDArray)osd;
                    i = 0;
                    while (i < array.Count - 1)
                    {
                        SerializeJson(array[i++], sb, preserveDefaults);
                        sb.AppendASCII(',');
                    }
                    if (i < array.Count)
                        SerializeJson(array[i], sb, preserveDefaults);
                    sb.AppendASCII(']');
                    break;
                case OSDType.Map:
                    sb.AppendASCII('{');
                    OSDMap map = (OSDMap)osd;
                    i = 0;
                    foreach(KeyValuePair<string, OSD> kvp in map)
                    {
                        if(preserveDefaults)
                        {
                            if (i++ > 0)
                                sb.AppendASCII(',');
                            appendJsonString(kvp.Key, sb);
                            sb.AppendASCII(':');
                            SerializeJson(kvp.Value, sb, true);
                        }
                        else
                            SerializeJsonMapNoDefaults(kvp.Key, kvp.Value, ref i, sb);
                    }
                    sb.AppendASCII('}');
                    break;
                case OSDType.Unknown:
                default:
                    sb.Append(osUTF8Const.OSUTF8null);
                    break;
            }
        }

        public static void SerializeJsonMapNoDefaults(string name, OSD osd, ref int mapcont, osUTF8 sb)
        {
            int i;
            switch (osd.Type)
            {
                case OSDType.Boolean:
                    bool ob = osd.AsBoolean();
                    if (ob)
                    {
                        if (mapcont++ > 0)
                            sb.AppendASCII(',');
                        appendJsonString(name, sb);
                        sb.AppendASCII(':');
                        sb.Append(ob ? osUTF8Const.OSUTF8true : osUTF8Const.OSUTF8false);
                    }
                    break;
                case OSDType.Integer:
                    int oi = osd.AsInteger();
                    if(oi != 0)
                    {
                        if (mapcont++ > 0)
                            sb.AppendASCII(',');
                        appendJsonString(name, sb);
                        sb.AppendASCII(':');
                        sb.AppendInt(oi);
                    }
                    break;
                case OSDType.Real:
                    double od = osd.AsReal();
                    if(od != 0)
                    {
                        if (mapcont++ > 0)
                            sb.AppendASCII(',');
                        appendJsonString(name, sb);
                        sb.AppendASCII(':');
                        string str = Convert.ToString(od, NumberFormatInfo.InvariantInfo);
                        sb.AppendASCII(str);

                        if (str.IndexOfAny(new char[] { '.', 'E' }) == -1)
                            sb.AppendASCII(".0");
                    }
                    break;
                case OSDType.String:
                case OSDType.URI:
                case OSDType.LLSDxml:
                    OSDString ostr = (OSDString)osd;
                    if (!string.IsNullOrEmpty(ostr.value))
                    {
                        if (mapcont++ > 0)
                            sb.AppendASCII(',');
                        appendJsonString(name, sb);
                        sb.AppendASCII(':');
                        appendJsonString(ostr.value, sb);
                    }
                    break;
                case OSDType.OSDUTF8:
                    osUTF8 ou8 = ((OSDUTF8)osd).value;
                    if (ou8 != null && ou8.Length > 0)
                    {
                        if (mapcont++ > 0)
                            sb.AppendASCII(',');
                        appendJsonString(name, sb);
                        sb.AppendASCII(':');
                        appendJsonOSUTF8(ou8, sb);
                    }
                    break;
                case OSDType.UUID:
                    UUID ou = osd.AsUUID();
                    if(ou.IsNotZero())
                    {
                        if (mapcont++ > 0)
                            sb.AppendASCII(',');
                        appendJsonString(name, sb);
                        sb.AppendASCII(":\"");
                        sb.AppendUUID(ou);
                        sb.AppendASCII('"');
                    }
                    break;
                case OSDType.Date:
                    if (mapcont++ > 0)
                        sb.AppendASCII(',');
                    appendJsonString(name, sb);
                    sb.AppendASCII(':');
                    appendJsonString(osd.AsString(), sb);
                    break;
                case OSDType.Binary:
                    byte[] binary = osd.AsBinary();
                    if (mapcont++ > 0)
                        sb.AppendASCII(',');
                    appendJsonString(name, sb);
                    sb.AppendASCII(":[");
                    if (binary != null && binary.Length > 0)
                    {
                        i = 0;
                        while (i < binary.Length - 1)
                        {
                            sb.AppendInt(binary[i++]);
                            sb.AppendASCII(',');
                        }
                        if (i < binary.Length)
                            sb.AppendInt(binary[i]);
                    }
                    sb.AppendASCII(']');
                    break;
                case OSDType.Array:
                    if (mapcont++ > 0)
                        sb.AppendASCII(",");
                    appendJsonString(name, sb);
                    sb.AppendASCII(":[");

                    OSDArray array = (OSDArray)osd;
                    if (array != null && array.Count > 0)
                    {
                        i = 0;
                        while (i < array.Count - 1)
                        {
                            SerializeJson(array[i++], sb, false);
                            sb.AppendASCII(',');
                        }
                        if (i < array.Count)
                            SerializeJson(array[i], sb, false);
                    }
                    sb.AppendASCII(']');
                    break;
                case OSDType.Map:
                    OSDMap map = (OSDMap)osd;
                    if (map != null && map.Count > 0)
                    {
                        if (mapcont++ > 0)
                            sb.AppendASCII(',');
                        appendJsonString(name, sb);
                        sb.AppendASCII(":{");

                        i = 0;
                        foreach (KeyValuePair<string, OSD> kvp in map)
                            SerializeJsonMapNoDefaults(kvp.Key, kvp.Value, ref i, sb);
                        sb.AppendASCII('}');
                    }
                    break;
                case OSDType.Unknown:
                default:
                    break;
            }
        }

        public static void appendJsonString(string str, osUTF8 sb)
        {
            sb.AppendASCII('"');
            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];
                switch (c)
                {
                    case '\n':
                        sb.AppendASCII("\\n");
                        break;

                    case '\r':
                        sb.AppendASCII("\\r");
                        break;

                    case '\t':
                        sb.AppendASCII("\\t");
                        break;

                    case '"':
                    //case '/':
                    case '\\':
                        sb.AppendASCII('\\');
                        sb.AppendASCII(c);
                        break;

                    case '\f':
                        sb.AppendASCII("\\f");
                        break;

                    case '\b':
                        sb.AppendASCII("\\b");
                        break;

                    default:
                        // Default, turn into a \uXXXX sequence
                        if (c >= 32 && c <= 126)
                            sb.AppendASCII(c);
                        else
                        {
                            sb.AppendASCII("\\u");
                            sb.Append(Utils.NibbleToHexUpper((byte)(c >> 12)));
                            sb.Append(Utils.NibbleToHexUpper((byte)(c >> 8)));
                            sb.Append(Utils.NibbleToHexUpper((byte)(c >> 4)));
                            sb.Append(Utils.NibbleToHexUpper((byte)c));
                        }
                        break;
                }
            }
            sb.AppendASCII('"');
        }

        public static void appendJsonOSUTF8(osUTF8 str, osUTF8 sb)
        {
            int code;
            sb.AppendASCII('"');
            for (int i = 0; i < str.Length; i++)
            {
                byte c = str[i];
                if (c < 0x80)
                {
                    switch (c)
                    {
                        case (byte)'\n':
                            sb.AppendASCII("\\n");
                            break;

                        case (byte)'\r':
                            sb.AppendASCII("\\r");
                            break;

                        case (byte)'\t':
                            sb.AppendASCII("\\t");
                            break;

                        case (byte)'"':
                        case (byte)'/':
                        case (byte)'\\':
                            sb.AppendASCII('\\');
                            sb.Append(c);
                            break;

                        case (byte)'\f':
                            sb.AppendASCII("\\f");
                            break;

                        case (byte)'\b':
                            sb.AppendASCII("\\b");
                            break;

                        default:
                            // Default, turn into a \uXXXX sequence
                            if (c >= 32 && c <= 126)
                                sb.Append(c);
                            else
                            {
                                sb.AppendASCII("\\u00");
                                sb.Append(Utils.NibbleToHexUpper((byte)(c >> 4)));
                                sb.Append(Utils.NibbleToHexUpper((byte)c));
                            }
                            break;
                    }
                }
                if(c < 0xc0)
                    continue; // invalid
                if (c < 0xe0)
                {
                    // 2 bytes
                    if (i + 1 >= str.Length)
                        return;

                    code = (c & 0x1f) << 6;
                    code |= str[++i] & 0x3f;

                    sb.AppendASCII("\\u0");
                    sb.Append(Utils.NibbleToHexUpper((byte)(code >> 8)));
                    sb.Append(Utils.NibbleToHexUpper((byte)(code >> 4)));
                    sb.Append(Utils.NibbleToHexUpper((byte)code));
                }
                else if (c < 0xF0)
                {
                    // 3 bytes
                    if (i + 2 >= str.Length)
                        return;

                    // 1110aaaa 10bbbbcc 10ccdddd
                    sb.AppendASCII("\\u");
                    sb.Append(Utils.NibbleToHexUpper(c));
                    c = str[++i];
                    sb.Append(Utils.NibbleToHexUpper((byte)(c >> 2)));
                    code = (c & 3) << 2;
                    c = str[++i];
                    code |= (c & 0x30) >> 4;
                    sb.Append(Utils.NibbleToHexUpper((byte)code));
                    sb.Append(Utils.NibbleToHexUpper(c));
                }
                else if (c < 0xf8 )
                {
                    if (i + 3 >= str.Length)
                        return;

                    code = (c & 0x07) << 18;
                    code |= (str[++i] & 0x3f) << 6;
                    code |= (str[++i] & 0x3f) << 6;
                    code |= str[++i] & 0x3f;
                    int a = (code >> 10) + 0xd7c0;
                    code &= (code & 0x3ff) + 0xdc00;

                    sb.AppendASCII("\\u");
                    sb.Append(Utils.NibbleToHexUpper((byte)(a >> 12)));
                    sb.Append(Utils.NibbleToHexUpper((byte)(a >> 8)));
                    sb.Append(Utils.NibbleToHexUpper((byte)(a >> 4)));
                    sb.Append(Utils.NibbleToHexUpper((byte)a));
                    sb.AppendASCII("\\u");
                    sb.Append(Utils.NibbleToHexUpper((byte)(code >> 12)));
                    sb.Append(Utils.NibbleToHexUpper((byte)(code >> 8)));
                    sb.Append(Utils.NibbleToHexUpper((byte)(code >> 4)));
                    sb.Append(Utils.NibbleToHexUpper((byte)code));
                }
            }
            sb.AppendASCII('"');
        }
    }
}
