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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace OpenMetaverse.StructuredData
{
    /// <summary>
    /// 
    /// </summary>
    public enum OSDType:byte
    {
        /// <summary></summary>
        Unknown,
        /// <summary></summary>
        Boolean,
        /// <summary></summary>
        Integer,
        /// <summary></summary>
        Real,
        /// <summary></summary>
        String,
        /// <summary></summary>
        UUID,
        /// <summary></summary>
        Date,
        /// <summary></summary>
        URI,
        /// <summary></summary>
        Binary,
        /// <summary></summary>
        Map,
        /// <summary></summary>
        Array,
        LLSDxml,
        OSDUTF8
    }

    public enum OSDFormat
    {
        Xml = 0,
        Json,
        Binary
    }

    /// <summary>
    /// 
    /// </summary>
    public class OSDException : Exception
    {
        public OSDException(string message) : base(message) { }
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class OSD
    {
        protected static readonly byte[] trueBinary = { 0x31 };
        protected static readonly byte[] falseBinary = { 0x30 };

        public OSDType Type = OSDType.Unknown;

        // .net4.8 64Bit JIT fails polimorphism
        public virtual bool AsBoolean()
        {
            switch (Type)
            {
                case OSDType.Boolean:
                    return ((OSDBoolean)this).value;
                case OSDType.Integer:
                    return ((OSDInteger)this).value != 0;
                case OSDType.Real:
                    double d = ((OSDReal)this).value;
                    return (!Double.IsNaN(d) && d != 0);
                case OSDType.String:
                    string s = ((OSDString)this).value;
                    if (String.IsNullOrEmpty(s))
                        return false;
                    if (s == "0" || s.ToLower() == "false")
                        return false;
                    return true;
                case OSDType.UUID:
                    return (((OSDUUID)this).value.IsZero()) ? false : true;
                case OSDType.Map:
                    return ((OSDMap)this).dicvalue.Count > 0;
                case OSDType.Array:
                    return ((OSDArray)this).value.Count > 0;
                case OSDType.OSDUTF8:
                    osUTF8 u = ((OSDUTF8)this).value;
                    if (osUTF8.IsNullOrEmpty(u))
                        return false;
                    if (u.Equals('0') || u.ACSIILowerEquals("false"))
                        return false;
                    return true;

                default:
                    return false;
            }
        }

        public virtual int AsInteger()
        {
            switch (Type)
            {
                case OSDType.Boolean:
                    return ((OSDBoolean)this).value ? 1 : 0;
                case OSDType.Integer:
                    return ((OSDInteger)this).value;
                case OSDType.Real:
                    double v = ((OSDReal)this).value;
                    if (Double.IsNaN(v))
                        return 0;
                    if (v >= Int32.MaxValue)
                        return Int32.MaxValue;
                    if (v <= Int32.MinValue)
                        return Int32.MinValue;
                    return (int)Math.Round(v);
                case OSDType.String:
                    string s = ((OSDString)this).value;
                    double dbl;
                    if (Double.TryParse(s, out dbl))
                        return (int)Math.Floor(dbl);
                    else
                        return 0;
                case OSDType.OSDUTF8:
                    string us = ((OSDUTF8)this).value.ToString();
                    double udbl;
                    if (Double.TryParse(us, out udbl))
                        return (int)Math.Floor(udbl);
                    else
                        return 0;
                case OSDType.Binary:
                    byte[] b = ((OSDBinary)this).value;
                    if (b.Length < 4)
                        return 0;
                    return (b[0] << 24) | (b[1] << 16) | (b[2] << 8) | b[3];
                case OSDType.Array:
                    List<OSD> l = ((OSDArray)this).value;
                    if (l.Count < 4)
                        return 0;
                    byte[] by = new byte[4];
                    for (int i = 0; i < 4; i++)
                        by[i] = (byte)l[i].AsInteger();
                    return (by[0] << 24) | (by[1] << 16) | (by[2] << 8) | by[3];
                case OSDType.Date:
                    return (int)Utils.DateTimeToUnixTime(((OSDDate)this).value);
                default:
                    return 0;
            }
        }

        public virtual uint AsUInteger()
        {
            switch (Type)
            {
                case OSDType.Boolean:
                    return ((OSDBoolean)this).value ? 1U : 0;
                case OSDType.Integer:
                    return (uint)((OSDInteger)this).value;
                case OSDType.Real:
                    double v = ((OSDReal)this).value;
                    if (Double.IsNaN(v))
                        return 0;
                    if (v > UInt32.MaxValue)
                        return UInt32.MaxValue;
                    if (v < UInt32.MinValue)
                        return UInt32.MinValue;
                    return (uint)Math.Round(v);
                case OSDType.String:
                    string s = ((OSDString)this).value;
                    double dbl;
                    if (Double.TryParse(s, out dbl))
                        return (uint)Math.Floor(dbl);
                    else
                        return 0;
                case OSDType.OSDUTF8:
                    string us = ((OSDUTF8)this).value.ToString();
                    double udbl;
                    if (Double.TryParse(us, out udbl))
                        return (uint)Math.Floor(udbl);
                    else
                        return 0;
                case OSDType.Date:
                    return Utils.DateTimeToUnixTime(((OSDDate)this).value);
                case OSDType.Binary:
                    byte[] b = ((OSDBinary)this).value;
                    if(b.Length < 4)
                        return 0;
                    return (uint)(
                        (b[0] << 24) | (b[1] << 16) | (b[2] << 8) | b[3]);
                case OSDType.Array:
                    List<OSD> l = ((OSDArray)this).value;
                    if (l.Count < 4)
                        return 0;
                    byte[] by = new byte[4];
                    for (int i = 0; i < 4; i++)
                        by[i] = (byte)l[i].AsInteger();
                    return (uint)((by[0] << 24) | (by[1] << 16) | (by[2] << 8) | by[3]);
                default:
                    return 0;
            }
        }

        public virtual long AsLong()
        {
            switch (Type)
            {
                case OSDType.Boolean:
                    return ((OSDBoolean)this).value ? 1 : 0;
                case OSDType.Integer:
                    return ((OSDInteger)this).value;
                case OSDType.Real:
                    double v = ((OSDReal)this).value;
                    if (Double.IsNaN(v))
                        return 0;
                    if (v > Int64.MaxValue)
                        return Int64.MaxValue;
                    if (v < Int64.MinValue)
                        return Int64.MinValue;
                    return (long)Math.Round(v);
                case OSDType.String:
                    string s = ((OSDString)this).value;
                    double dbl;
                    if (Double.TryParse(s, out dbl))
                        return (long)Math.Floor(dbl);
                    else
                        return 0;
                case OSDType.OSDUTF8:
                    string us = ((OSDUTF8)this).value.ToString();
                    double udbl;
                    if (Double.TryParse(us, out udbl))
                        return (long)Math.Floor(udbl);
                    else
                        return 0;
                case OSDType.Date:
                    return Utils.DateTimeToUnixTime(((OSDDate)this).value);
                case OSDType.Binary:
                {
                    byte[] b = ((OSDBinary)this).value;
                    if(b.Length < 8)
                        return 0;
                    return (
                        ((long)b[0] << 56) |
                        ((long)b[1] << 48) |
                        ((long)b[2] << 40) |
                        ((long)b[3] << 32) |
                        ((long)b[4] << 24) |
                        ((long)b[5] << 16) |
                        ((long)b[6] << 8) |
                        b[7]);
                }
                case OSDType.Array:
                {
                    List<OSD> l = ((OSDArray)this).value;
                    if (l.Count < 8)
                        return 0;
                    byte[] b = new byte[8];
                    for (int i = 0; i < 8; i++)
                        b[i] = (byte)l[i].AsInteger();
                    return (
                        ((long)b[0] << 56) |
                        ((long)b[1] << 48) |
                        ((long)b[2] << 40) |
                        ((long)b[3] << 32) |
                        ((long)b[4] << 24) |
                        ((long)b[5] << 16) |
                        ((long)b[6] << 8) |
                        b[7]);
                }
                default:
                    return 0;
            }
        }

        public virtual ulong AsULong()
        {
            switch (Type)
            {
                case OSDType.Boolean:
                    return ((OSDBoolean)this).value ? 1UL : 0;
                case OSDType.Integer:
                    return (ulong)((OSDInteger)this).value;
                case OSDType.Real:
                    double v = ((OSDReal)this).value;
                    if (Double.IsNaN(v))
                        return 0;
                    if (v > UInt64.MaxValue)
                        return UInt64.MaxValue;
                    if (v < UInt64.MinValue)
                        return UInt64.MinValue;
                    return (ulong)Math.Round(v);
                case OSDType.String:
                    string s = ((OSDString)this).value;
                    double dbl;
                    if (Double.TryParse(s, out dbl))
                        return (ulong)Math.Floor(dbl);
                    else
                        return 0;
                case OSDType.OSDUTF8:
                    string us = ((OSDUTF8)this).value.ToString();
                    double udbl;
                    if (Double.TryParse(us, out udbl))
                        return (ulong)Math.Floor(udbl);
                    else
                        return 0;
                case OSDType.Date:
                    return Utils.DateTimeToUnixTime(((OSDDate)this).value);
                case OSDType.Binary:
                {
                    byte[] b = ((OSDBinary)this).value;
                    if (b.Length < 8)
                        return 0;
                    return (
                        ((ulong)b[0] << 56) |
                        ((ulong)b[1] << 48) |
                        ((ulong)b[2] << 40) |
                        ((ulong)b[3] << 32) |
                        ((ulong)b[4] << 24) |
                        ((ulong)b[5] << 16) |
                        ((ulong)b[6] << 8) |
                        b[7]);
                }
                case OSDType.Array:
                {
                    List<OSD> l = ((OSDArray)this).value;
                    if (l.Count < 8)
                        return 0;
                    byte[] b = new byte[8];
                    for (int i = 0; i < 8; i++)
                        b[i] = (byte)l[i].AsInteger();
                    return (
                        ((ulong)b[0] << 56) |
                        ((ulong)b[1] << 48) |
                        ((ulong)b[2] << 40) |
                        ((ulong)b[3] << 32) |
                        ((ulong)b[4] << 24) |
                        ((ulong)b[5] << 16) |
                        ((ulong)b[6] << 8) |
                        b[7]);
                }
                default:
                    return 0;
            }
        }

        public virtual double AsReal()
        {
            switch (Type)
            {
                case OSDType.Boolean:
                    return ((OSDBoolean)this).value ? 1.0 : 0;
                case OSDType.Integer:
                    return ((OSDInteger)this).value;
                case OSDType.Real:
                    return ((OSDReal)this).value;
                case OSDType.String:
                    string s = ((OSDString)this).value;
                    double dbl;
                    if (Double.TryParse(s, out dbl))
                        return dbl;
                    else
                        return 0;
                case OSDType.OSDUTF8:
                    string us = ((OSDUTF8)this).value.ToString();
                    double udbl;
                    if (Double.TryParse(us, out udbl))
                        return udbl;
                    else
                        return 0;
                default:
                    return 0;
            }
        }

        public virtual string AsString()
        {
            switch (Type)
            {
                case OSDType.Boolean:
                    return ((OSDBoolean)this).value ? "1" : "0";
                case OSDType.Integer:
                    return ((OSDInteger)this).value.ToString();
                case OSDType.Real:
                    return ((OSDReal)this).value.ToString("r", Utils.EnUsCulture);
                case OSDType.String:
                    return ((OSDString)this).value;
                case OSDType.OSDUTF8:
                    return ((OSDUTF8)this).value.ToString();
                case OSDType.UUID:
                    return ((OSDUUID)this).value.ToString();
                case OSDType.Date:
                    string format;
                    DateTime dt = ((OSDDate)this).value;
                    if (dt.Millisecond > 0)
                        format = "yyyy-MM-ddTHH:mm:ss.ffZ";
                    else
                        format = "yyyy-MM-ddTHH:mm:ssZ";
                    return dt.ToUniversalTime().ToString(format);
                case OSDType.URI:
                    Uri ur = ((OSDUri)this).value;
                    if (ur == null)
                        return string.Empty;
                    if (ur.IsAbsoluteUri)
                        return ur.AbsoluteUri;
                    else
                        return ur.ToString();

                case OSDType.Binary:
                    byte[] b = ((OSDBinary)this).value;
                    return Convert.ToBase64String(b);
                case OSDType.LLSDxml:
                    return ((OSDllsdxml)this).value;
                default:
                    return String.Empty;
            }
        }

        public virtual UUID AsUUID()
        {
            switch (Type)
            {
                case OSDType.String:
                    UUID uuid;
                    if (UUID.TryParse(((OSDString)this).value, out uuid))
                        return uuid;
                    else
                        return UUID.Zero;
                case OSDType.OSDUTF8:
                    UUID ouuid;
                    if (UUID.TryParse(((OSDUTF8)this).value.ToString(), out ouuid))
                        return ouuid;
                    else
                        return UUID.Zero;
                case OSDType.UUID:
                    return ((OSDUUID)this).value;
                default:
                    return UUID.Zero;
            }
        }

        public virtual DateTime AsDate()
        {
            switch (Type)
            {
                case OSDType.String:
                    DateTime dt;
                    if (DateTime.TryParse(((OSDString)this).value, out dt))
                        return dt;
                    else
                        return Utils.Epoch;
                case OSDType.OSDUTF8:
                    DateTime odt;
                    if (DateTime.TryParse(((OSDUTF8)this).value.ToString(), out odt))
                        return odt;
                    else
                        return Utils.Epoch;
                case OSDType.UUID:
                case OSDType.Date:
                    return ((OSDDate)this).value;
                default:
                    return Utils.Epoch;
            }
        }
        public virtual Uri AsUri()
        {
            switch (Type)
            {
                case OSDType.String:
                    Uri uri;
                    if (Uri.TryCreate(((OSDString)this).value, UriKind.RelativeOrAbsolute, out uri))
                        return uri;
                    else
                        return null;
                case OSDType.OSDUTF8:
                    Uri ouri;
                    if (Uri.TryCreate(((OSDUTF8)this).value.ToString(), UriKind.RelativeOrAbsolute, out ouri))
                        return ouri;
                    else
                        return null;
                case OSDType.URI:
                    return ((OSDUri)this).value;
                default:
                    return null;
            }
        }

        public virtual byte[] AsBinary()
        {
            switch (Type)
            {
                case OSDType.Boolean:
                    return ((OSDBoolean)this).value ? trueBinary : falseBinary;
                case OSDType.Integer:
                    return Utils.IntToBytesBig(((OSDInteger)this).value);
                case OSDType.Real:
                    return Utils.DoubleToBytesBig(((OSDReal)this).value);
                case OSDType.String:
                    return Encoding.UTF8.GetBytes(((OSDString)this).value);
                case OSDType.OSDUTF8:
                    return ((OSDUTF8)this).value.ToArray();
                case OSDType.UUID:
                    return (((OSDUUID)this).value).GetBytes();
                case OSDType.Date:
                    TimeSpan ts = (((OSDDate)this).value).ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    return Utils.DoubleToBytes(ts.TotalSeconds);
                case OSDType.URI:
                    return Encoding.UTF8.GetBytes(((OSDUri)this).AsString());
                case OSDType.Binary:
                    return ((OSDBinary)this).value;
                case OSDType.Map:
                case OSDType.Array:
                    List<OSD> l = ((OSDArray)this).value;
                    byte[] binary = new byte[l.Count];
                    for (int i = 0; i < l.Count; i++)
                        binary[i] = (byte)l[i].AsInteger();
                    return binary;
                case OSDType.LLSDxml:
                    return Encoding.UTF8.GetBytes(((OSDllsdxml)this).value);
                default:
                    return Array.Empty<byte>();
            }
        }

        public Vector2 AsVector2()
        {
            switch (Type)
            {
                case OSDType.String:
                    return Vector2.Parse(((OSDString)this).value);
                case OSDType.OSDUTF8:
                    return Vector2.Parse(((OSDUTF8)this).value.ToString());
                case OSDType.Array:
                    List<OSD> l = ((OSDArray)this).value;
                    Vector2 vector = Vector2.Zero;
                    if (l.Count == 2)
                    {
                        vector.X = (float)l[0].AsReal();
                        vector.Y = (float)l[1].AsReal();
                    }
                    return vector;
                default:
                    return Vector2.Zero;
            }
        }

        public Vector3 AsVector3()
        {
            switch (Type)
            {
                case OSDType.String:
                    return Vector3.Parse(((OSDString)this).value);
                case OSDType.OSDUTF8:
                    return Vector3.Parse(((OSDUTF8)this).value.ToString());
                case OSDType.Array:
                    List<OSD> l = ((OSDArray)this).value;
                    Vector3 vector = Vector3.Zero;
                    if (l.Count == 3)
                    {
                        vector.X = (float)l[0].AsReal();
                        vector.Y = (float)l[1].AsReal();
                        vector.Z = (float)l[2].AsReal();
                    }
                    return vector;
                default:
                    return Vector3.Zero;
            }
        }

        public Vector3d AsVector3d()
        {
            switch (Type)
            {
                case OSDType.String:
                    return Vector3d.Parse(((OSDString)this).value);
                case OSDType.OSDUTF8:
                    return Vector3d.Parse(((OSDUTF8)this).value.ToString());
                case OSDType.Array:
                    List<OSD> l = ((OSDArray)this).value;
                    Vector3d vector = Vector3d.Zero;
                    if (l.Count == 3)
                    {
                        vector.X = (float)l[0].AsReal();
                        vector.Y = (float)l[1].AsReal();
                        vector.Z = (float)l[2].AsReal();
                    }
                    return vector;
                default:
                    return Vector3d.Zero;
            }
        }

        public Vector4 AsVector4()
        {
            switch (Type)
            {
                case OSDType.String:
                    return Vector4.Parse(((OSDString)this).value);
                case OSDType.OSDUTF8:
                    return Vector4.Parse(((OSDUTF8)this).value.ToString());
                case OSDType.Array:
                    List<OSD> l = ((OSDArray)this).value;
                    Vector4 vector = Vector4.Zero;
                    if (l.Count == 4)
                    {
                        vector.X = (float)l[0].AsReal();
                        vector.Y = (float)l[1].AsReal();
                        vector.Z = (float)l[2].AsReal();
                        vector.W = (float)l[3].AsReal();
                    }
                    return vector;
                default:
                    return Vector4.Zero;
            }
        }

        public Quaternion AsQuaternion()
        {
            switch (Type)
            {
                case OSDType.String:
                    return Quaternion.Parse(((OSDString)this).value);
                case OSDType.OSDUTF8:
                    return Quaternion.Parse(((OSDString)this).value.ToString());
                case OSDType.Array:
                    List<OSD> l = ((OSDArray)this).value;
                    Quaternion q = Quaternion.Identity;
                    if (l.Count == 4)
                    {
                        q.X = (float)l[0].AsReal();
                        q.Y = (float)l[1].AsReal();
                        q.Z = (float)l[2].AsReal();
                        q.W = (float)l[3].AsReal();
                    }
                    return q;
                default:
                    return Quaternion.Identity;
            }
        }

        public virtual Color4 AsColor4()
        {
            switch (Type)
            {
                case OSDType.Array:
                    List<OSD> l = ((OSDArray)this).value;
                    Color4 color = Color4.Black;
                    if (l.Count == 4)
                    {
                        color.R = (float)l[0].AsReal();
                        color.G = (float)l[1].AsReal();
                        color.B = (float)l[2].AsReal();
                        color.A = (float)l[3].AsReal();
                    }
                    return color;
                default:
                    return Color4.Black;
            }
        }

        public virtual void Clear() { }

        public virtual OSD Copy()
        {
            switch (Type)
            {
                case OSDType.Boolean:
                    return new OSDBoolean(((OSDBoolean)this).value);
                case OSDType.Integer:
                    return new OSDInteger(((OSDInteger)this).value);
                case OSDType.Real:
                    return new OSDReal(((OSDReal)this).value);
                case OSDType.String:
                    return new OSDString(((OSDString)this).value);
                case OSDType.OSDUTF8:
                    return new OSDUTF8(((OSDUTF8)this).value);
                case OSDType.UUID:
                    return new OSDUUID(((OSDUUID)this).value);
                case OSDType.Date:
                    return new OSDDate(((OSDDate)this).value);
                case OSDType.URI:
                    return new OSDUri(((OSDUri)this).value);
                case OSDType.Binary:
                    return new OSDBinary(((OSDBinary)this).value);
                case OSDType.Map:
                    return new OSDMap(((OSDMap)this).dicvalue);
                case OSDType.Array:
                    return new OSDArray(((OSDArray)this).value);
                case OSDType.LLSDxml:
                    return new OSDBoolean(((OSDBoolean)this).value);
                default:
                    return new OSD();
            }
        }

        public override string ToString()
        {
            switch (Type)
            {
                case OSDType.Boolean:
                    return ((OSDBoolean)this).value ? "1" : "0";
                case OSDType.Integer:
                    return ((OSDInteger)this).value.ToString();
                case OSDType.Real:
                    return ((OSDReal)this).value.ToString("r", Utils.EnUsCulture);
                case OSDType.String:
                    return ((OSDString)this).value;
                case OSDType.OSDUTF8:
                    return ((OSDUTF8)this).value.ToString();
                case OSDType.UUID:
                    return ((OSDUUID)this).value.ToString();
                case OSDType.Date:
                    string format;
                    DateTime dt = ((OSDDate)this).value;
                    if (dt.Millisecond > 0)
                        format = "yyyy-MM-ddTHH:mm:ss.ffZ";
                    else
                        format = "yyyy-MM-ddTHH:mm:ssZ";
                    return dt.ToUniversalTime().ToString(format);
                case OSDType.URI:
                    Uri ur = ((OSDUri)this).value;
                    if (ur == null)
                        return string.Empty;
                    if (ur.IsAbsoluteUri)
                        return ur.AbsoluteUri;
                    else
                        return ur.ToString();
                case OSDType.Binary:
                    return Utils.BytesToHexString(((OSDBinary)this).value, null);
                case OSDType.LLSDxml:
                    return ((OSDllsdxml)this).value;
                case OSDType.Map:
                    return OSDParser.SerializeJsonString((OSDMap)this, true);
                case OSDType.Array:
                    return OSDParser.SerializeJsonString((OSDArray)this, true);
                default:
                    return "undef";
            }
        }

        public static OSD FromBoolean(bool value) { return new OSDBoolean(value); }
        public static OSD FromInteger(int value) { return new OSDInteger(value); }
        public static OSD FromInteger(uint value) { return new OSDInteger((int)value); }
        public static OSD FromInteger(short value) { return new OSDInteger((int)value); }
        public static OSD FromInteger(ushort value) { return new OSDInteger((int)value); }
        public static OSD FromInteger(sbyte value) { return new OSDInteger((int)value); }
        public static OSD FromInteger(byte value) { return new OSDInteger((int)value); }
        public static OSD FromUInteger(uint value) { return new OSDBinary(value); }
        public static OSD FromLong(long value) { return new OSDBinary(value); }
        public static OSD FromULong(ulong value) { return new OSDBinary(value); }
        public static OSD FromReal(double value) { return new OSDReal(value); }
        public static OSD FromReal(float value) { return new OSDReal((double)value); }
        public static OSD FromString(string value) { return new OSDString(value); }
        public static OSD FromUUID(UUID value) { return new OSDUUID(value); }
        public static OSD FromDate(DateTime value) { return new OSDDate(value); }
        public static OSD FromUri(Uri value) { return new OSDUri(value); }
        public static OSD FromBinary(byte[] value) { return new OSDBinary(value); }

        public static OSD FromVector2(Vector2 value)
        {
            OSDArray array = new OSDArray();
            array.Add(OSD.FromReal(value.X));
            array.Add(OSD.FromReal(value.Y));
            return array;
        }

        public static OSD FromVector3(Vector3 value)
        {
            OSDArray array = new OSDArray();
            array.Add(OSD.FromReal(value.X));
            array.Add(OSD.FromReal(value.Y));
            array.Add(OSD.FromReal(value.Z));
            return array;
        }

        public static OSD FromVector3d(Vector3d value)
        {
            OSDArray array = new OSDArray();
            array.Add(OSD.FromReal(value.X));
            array.Add(OSD.FromReal(value.Y));
            array.Add(OSD.FromReal(value.Z));
            return array;
        }

        public static OSD FromVector4(Vector4 value)
        {
            OSDArray array = new OSDArray();
            array.Add(OSD.FromReal(value.X));
            array.Add(OSD.FromReal(value.Y));
            array.Add(OSD.FromReal(value.Z));
            array.Add(OSD.FromReal(value.W));
            return array;
        }

        public static OSD FromQuaternion(Quaternion value)
        {
            OSDArray array = new OSDArray();
            array.Add(OSD.FromReal(value.X));
            array.Add(OSD.FromReal(value.Y));
            array.Add(OSD.FromReal(value.Z));
            array.Add(OSD.FromReal(value.W));
            return array;
        }

        public static OSD FromColor4(Color4 value)
        {
            OSDArray array = new OSDArray();
            array.Add(OSD.FromReal(value.R));
            array.Add(OSD.FromReal(value.G));
            array.Add(OSD.FromReal(value.B));
            array.Add(OSD.FromReal(value.A));
            return array;
        }

        public static OSD FromObject(object value)
        {
            if (value == null) { return new OSD(); }
            else if (value is bool) { return new OSDBoolean((bool)value); }
            else if (value is int) { return new OSDInteger((int)value); }
            else if (value is uint) { return new OSDBinary((uint)value); }
            else if (value is short) { return new OSDInteger((int)(short)value); }
            else if (value is ushort) { return new OSDInteger((int)(ushort)value); }
            else if (value is sbyte) { return new OSDInteger((int)(sbyte)value); }
            else if (value is byte) { return new OSDInteger((int)(byte)value); }
            else if (value is double) { return new OSDReal((double)value); }
            else if (value is float) { return new OSDReal((double)(float)value); }
            else if (value is string) { return new OSDString((string)value); }
            else if (value is UUID) { return new OSDUUID((UUID)value); }
            else if (value is DateTime) { return new OSDDate((DateTime)value); }
            else if (value is Uri) { return new OSDUri((Uri)value); }
            else if (value is byte[]) { return new OSDBinary((byte[])value); }
            else if (value is long) { return new OSDBinary((long)value); }
            else if (value is ulong) { return new OSDBinary((ulong)value); }
            else if (value is Vector2) { return FromVector2((Vector2)value); }
            else if (value is Vector3) { return FromVector3((Vector3)value); }
            else if (value is Vector3d) { return FromVector3d((Vector3d)value); }
            else if (value is Vector4) { return FromVector4((Vector4)value); }
            else if (value is Quaternion) { return FromQuaternion((Quaternion)value); }
            else if (value is Color4) { return FromColor4((Color4)value); }
            else return new OSD();
        }

        public static object ToObject(Type type, OSD value)
        {
            if (type == typeof(ulong))
            {
                if (value.Type == OSDType.Binary)
                {
                    byte[] bytes = value.AsBinary();
                    return Utils.BytesToUInt64(bytes);
                }
                else
                {
                    return (ulong)value.AsInteger();
                }
            }
            else if (type == typeof(uint))
            {
                if (value.Type == OSDType.Binary)
                {
                    byte[] bytes = value.AsBinary();
                    return Utils.BytesToUInt(bytes);
                }
                else
                {
                    return (uint)value.AsInteger();
                }
            }
            else if (type == typeof(ushort))
            {
                return (ushort)value.AsInteger();
            }
            else if (type == typeof(byte))
            {
                return (byte)value.AsInteger();
            }
            else if (type == typeof(short))
            {
                return (short)value.AsInteger();
            }
            else if (type == typeof(string))
            {
                return value.AsString();
            }
            else if (type == typeof(bool))
            {
                return value.AsBoolean();
            }
            else if (type == typeof(float))
            {
                return (float)value.AsReal();
            }
            else if (type == typeof(double))
            {
                return value.AsReal();
            }
            else if (type == typeof(int))
            {
                return value.AsInteger();
            }
            else if (type == typeof(UUID))
            {
                return value.AsUUID();
            }
            else if (type == typeof(Vector3))
            {
                if (value.Type == OSDType.Array)
                    return ((OSDArray)value).AsVector3();
                else
                    return Vector3.Zero;
            }
            else if (type == typeof(Vector4))
            {
                if (value.Type == OSDType.Array)
                    return ((OSDArray)value).AsVector4();
                else
                    return Vector4.Zero;
            }
            else if (type == typeof(Quaternion))
            {
                if (value.Type == OSDType.Array)
                    return ((OSDArray)value).AsQuaternion();
                else
                    return Quaternion.Identity;
            }
            else if (type == typeof(OSDArray))
            {
                OSDArray newArray = new OSDArray();
                foreach (OSD o in (OSDArray)value)
                    newArray.Add(o);
                return newArray;
            }
            else if (type == typeof(OSDMap))
            {
                OSDMap newMap = new OSDMap();
                foreach (KeyValuePair<string, OSD> o in (OSDMap)value)
                    newMap.Add(o);
                return newMap;
            }
            else
            {
                return null;
            }
        }

        #region Implicit Conversions

        public static implicit operator OSD(bool value) { return new OSDBoolean(value); }
        public static implicit operator OSD(int value) { return new OSDInteger(value); }
        public static implicit operator OSD(uint value) { return new OSDInteger((int)value); }
        public static implicit operator OSD(short value) { return new OSDInteger((int)value); }
        public static implicit operator OSD(ushort value) { return new OSDInteger((int)value); }
        public static implicit operator OSD(sbyte value) { return new OSDInteger((int)value); }
        public static implicit operator OSD(byte value) { return new OSDInteger((int)value); }
        public static implicit operator OSD(long value) { return new OSDBinary(value); }
        public static implicit operator OSD(ulong value) { return new OSDBinary(value); }
        public static implicit operator OSD(double value) { return new OSDReal(value); }
        public static implicit operator OSD(float value) { return new OSDReal(value); }
        public static implicit operator OSD(string value) { return new OSDString(value); }
        public static implicit operator OSD(UUID value) { return new OSDUUID(value); }
        public static implicit operator OSD(DateTime value) { return new OSDDate(value); }
        public static implicit operator OSD(Uri value) { return new OSDUri(value); }
        public static implicit operator OSD(byte[] value) { return new OSDBinary(value); }
        public static implicit operator OSD(Vector2 value) { return OSD.FromVector2(value); }
        public static implicit operator OSD(Vector3 value) { return OSD.FromVector3(value); }
        public static implicit operator OSD(Vector3d value) { return OSD.FromVector3d(value); }
        public static implicit operator OSD(Vector4 value) { return OSD.FromVector4(value); }
        public static implicit operator OSD(Quaternion value) { return OSD.FromQuaternion(value); }
        public static implicit operator OSD(Color4 value) { return OSD.FromColor4(value); }

        public static implicit operator bool(OSD value) { return value.AsBoolean(); }
        public static implicit operator int(OSD value) { return value.AsInteger(); }
        public static implicit operator uint(OSD value) { return value.AsUInteger(); }
        public static implicit operator long(OSD value) { return value.AsLong(); }
        public static implicit operator ulong(OSD value) { return value.AsULong(); }
        public static implicit operator double(OSD value) { return value.AsReal(); }
        public static implicit operator float(OSD value) { return (float)value.AsReal(); }
        public static implicit operator string(OSD value) { return value.AsString(); }
        public static implicit operator UUID(OSD value) { return value.AsUUID(); }
        public static implicit operator DateTime(OSD value) { return value.AsDate(); }
        public static implicit operator Uri(OSD value) { return value.AsUri(); }
        public static implicit operator byte[](OSD value) { return value.AsBinary(); }
        public static implicit operator Vector2(OSD value) { return value.AsVector2(); }
        public static implicit operator Vector3(OSD value) { return value.AsVector3(); }
        public static implicit operator Vector3d(OSD value) { return value.AsVector3d(); }
        public static implicit operator Vector4(OSD value) { return value.AsVector4(); }
        public static implicit operator Quaternion(OSD value) { return value.AsQuaternion(); }
        public static implicit operator Color4(OSD value) { return value.AsColor4(); }

        #endregion Implicit Conversions

        /// <summary>
        /// Uses reflection to create an SDMap from all of the SD
        /// serializable types in an object
        /// </summary>
        /// <param name="obj">Class or struct containing serializable types</param>
        /// <returns>An SDMap holding the serialized values from the
        /// container object</returns>
        public static OSDMap SerializeMembers(object obj)
        {
            Type t = obj.GetType();
            FieldInfo[] fields = t.GetFields();

            OSDMap map = new OSDMap(fields.Length);

            for (int i = 0; i < fields.Length; i++)
            {
                FieldInfo field = fields[i];
                if (!Attribute.IsDefined(field, typeof(NonSerializedAttribute)))
                {
                    OSD serializedField = OSD.FromObject(field.GetValue(obj));

                    if (serializedField.Type != OSDType.Unknown || field.FieldType == typeof(string) || field.FieldType == typeof(byte[]))
                        map.Add(field.Name, serializedField);
                }
            }

            return map;
        }

        /// <summary>
        /// Uses reflection to deserialize member variables in an object from
        /// an SDMap
        /// </summary>
        /// <param name="obj">Reference to an object to fill with deserialized
        /// values</param>
        /// <param name="serialized">Serialized values to put in the target
        /// object</param>
        public static void DeserializeMembers(ref object obj, OSDMap serialized)
        {
            Type t = obj.GetType();
            FieldInfo[] fields = t.GetFields();

            for (int i = 0; i < fields.Length; i++)
            {
                FieldInfo field = fields[i];
                if (!Attribute.IsDefined(field, typeof(NonSerializedAttribute)))
                {
                    OSD serializedField;
                    if (serialized.TryGetValue(field.Name, out serializedField))
                        field.SetValue(obj, ToObject(field.FieldType, serializedField));
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class OSDBoolean : OSD
    {
        public readonly bool value;

        public OSDBoolean(bool value)
        {
            Type = OSDType.Boolean;
            this.value = value;
        }

        public override bool AsBoolean() { return value; }
        public override int AsInteger() { return value ? 1 : 0; }
        public override double AsReal() { return value ? 1d : 0d; }
        public override string AsString() { return value ? "1" : "0"; }
        public override byte[] AsBinary() { return value ? trueBinary : falseBinary; }
        public override OSD Copy() { return new OSDBoolean(value); }
        public override string ToString() { return AsString(); }
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class OSDInteger : OSD
    {
        public readonly int value;

        public OSDInteger(int value)
        {
            Type = OSDType.Integer;
            this.value = value;
        }

        public override bool AsBoolean() { return value != 0; }
        public override int AsInteger() { return value; }
        public override uint AsUInteger() { return (uint)value; }
        public override long AsLong() { return value; }
        public override ulong AsULong() { return (ulong)value; }
        public override double AsReal() { return (double)value; }
        public override string AsString() { return value.ToString(); }
        public override byte[] AsBinary() { return Utils.IntToBytesBig(value); }
        public override OSD Copy() { return new OSDInteger(value); }
        public override string ToString() { return AsString(); }
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class OSDReal : OSD
    {
        public readonly double value;

        public OSDReal(double value)
        {
            Type = OSDType.Real;
            this.value = value;
        }

        public override bool AsBoolean() { return (!Double.IsNaN(value) && value != 0d); }
        public override OSD Copy() { return new OSDReal(value); }
        public override int AsInteger()
        {
            if (Double.IsNaN(value))
                return 0;
            if (value > (double)Int32.MaxValue)
                return Int32.MaxValue;
            if (value < (double)Int32.MinValue)
                return Int32.MinValue;
            return (int)Math.Round(value);
        }

        public override uint AsUInteger()
        {
            if (Double.IsNaN(value))
                return 0;
            if (value > (double)UInt32.MaxValue)
                return UInt32.MaxValue;
            if (value < (double)UInt32.MinValue)
                return UInt32.MinValue;
            return (uint)Math.Round(value);
        }

        public override long AsLong()
        {
            if (Double.IsNaN(value))
                return 0;
            if (value > (double)Int64.MaxValue)
                return Int64.MaxValue;
            if (value < (double)Int64.MinValue)
                return Int64.MinValue;
            return (long)Math.Round(value);
        }

        public override ulong AsULong()
        {
            if (Double.IsNaN(value))
                return 0;
            if (value > (double)UInt64.MaxValue)
                return Int32.MaxValue;
            if (value < (double)UInt64.MinValue)
                return UInt64.MinValue;
            return (ulong)Math.Round(value);
        }

        public override double AsReal() { return value; }
        // "r" ensures the value will correctly round-trip back through Double.TryParse
        public override string AsString() { return value.ToString("r", Utils.EnUsCulture); }
        public override byte[] AsBinary() { return Utils.DoubleToBytesBig(value); }
        public override string ToString() { return AsString(); }
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class OSDllsdxml : OSD
    {
        public readonly string value;

        public override OSD Copy() { return new OSDllsdxml(value); }

        public OSDllsdxml(string value)
        {
            Type = OSDType.LLSDxml;
            // Refuse to hold null pointers
            if (value != null)
                this.value = value;
            else
                this.value = String.Empty;
        }

        public override string AsString() { return value; }
        public override byte[] AsBinary() { return Encoding.UTF8.GetBytes(value); }
        public override string ToString() { return AsString(); }
    }

    public sealed class OSDUTF8 : OSD
    {
        public readonly osUTF8 value;

        public override OSD Copy() { return new OSDUTF8(value.Clone()); }

        public OSDUTF8(osUTF8 value)
        {
            Type = OSDType.OSDUTF8;
            // Refuse to hold null pointers
            if (value != null)
                this.value = value;
            else
                this.value = new osUTF8();
        }

        public OSDUTF8(byte[] value)
        {
            Type = OSDType.OSDUTF8;
            // Refuse to hold null pointers
            if (value != null)
                this.value = new osUTF8(value);
            else
                this.value = new osUTF8();
        }

        public OSDUTF8(string value)
        {
            Type = OSDType.OSDUTF8;
            // Refuse to hold null pointers
            if (value != null)
                this.value = new osUTF8(value);
            else
                this.value = new osUTF8();
        }

        public override bool AsBoolean()
        {
            if (osUTF8.IsNullOrEmpty(value))
                return false;

            if (value.Equals('0') || value.ACSIILowerEquals("false"))
                return false;

            return true;
        }

        public override int AsInteger()
        {
            double dbl;
            if (Double.TryParse(value.ToString(), out dbl))
                return (int)Math.Floor(dbl);
            else
                return 0;
        }

        public override uint AsUInteger()
        {
            double dbl;
            if (Double.TryParse(value.ToString(), out dbl))
                return (uint)Math.Floor(dbl);
            else
                return 0;
        }

        public override long AsLong()
        {
            double dbl;
            if (Double.TryParse(value.ToString(), out dbl))
                return (long)Math.Floor(dbl);
            else
                return 0;
        }

        public override ulong AsULong()
        {
            double dbl;
            if (Double.TryParse(value.ToString(), out dbl))
                return (ulong)Math.Floor(dbl);
            else
                return 0;
        }

        public override double AsReal()
        {
            double dbl;
            if (Double.TryParse(value.ToString(), out dbl))
                return dbl;
            else
                return 0d;
        }

        public override string AsString() { return value.ToString(); }
        public override byte[] AsBinary() { return value.ToArray(); }

        public override UUID AsUUID()
        {
            UUID uuid;
            if (UUID.TryParse(value.ToString(), out uuid))
                return uuid;
            else
                return UUID.Zero;
        }

        public override DateTime AsDate()
        {
            DateTime dt;
            if (DateTime.TryParse(value.ToString(), out dt))
                return dt;
            else
                return Utils.Epoch;
        }

        public override Uri AsUri()
        {
            Uri uri;
            if (Uri.TryCreate(value.ToString(), UriKind.RelativeOrAbsolute, out uri))
                return uri;
            else
                return null;
        }

        public override string ToString() { return AsString(); }
    }

    public sealed class OSDString : OSD
    {
        public readonly string value;

        public override OSD Copy() { return new OSDString(value); }

        public OSDString(string value)
        {
            Type = OSDType.String;
            // Refuse to hold null pointers
            if (value != null)
                this.value = value;
            else
                this.value = String.Empty;
        }

        public override bool AsBoolean()
        {
            if (String.IsNullOrEmpty(value))
                return false;

            if (value == "0" || value.ToLower() == "false")
                return false;

            return true;
        }

        public override int AsInteger()
        {
            double dbl;
            if (Double.TryParse(value, out dbl))
                return (int)Math.Floor(dbl);
            else
                return 0;
        }

        public override uint AsUInteger()
        {
            double dbl;
            if (Double.TryParse(value, out dbl))
                return (uint)Math.Floor(dbl);
            else
                return 0;
        }

        public override long AsLong()
        {
            double dbl;
            if (Double.TryParse(value, out dbl))
                return (long)Math.Floor(dbl);
            else
                return 0;
        }

        public override ulong AsULong()
        {
            double dbl;
            if (Double.TryParse(value, out dbl))
                return (ulong)Math.Floor(dbl);
            else
                return 0;
        }

        public override double AsReal()
        {
            double dbl;
            if (Double.TryParse(value, out dbl))
                return dbl;
            else
                return 0d;
        }

        public override string AsString() { return value; }
        public override byte[] AsBinary() { return Encoding.UTF8.GetBytes(value); }

        public override UUID AsUUID()
        {
            UUID uuid;
            if (UUID.TryParse(value, out uuid))
                return uuid;
            else
                return UUID.Zero;
        }

        public override DateTime AsDate()
        {
            DateTime dt;
            if (DateTime.TryParse(value, out dt))
                return dt;
            else
                return Utils.Epoch;
        }

        public override Uri AsUri()
        {
            Uri uri;
            if (Uri.TryCreate(value, UriKind.RelativeOrAbsolute, out uri))
                return uri;
            else
                return null;
        }

        public override string ToString() { return AsString(); }
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class OSDUUID : OSD
    {
        public readonly UUID value;

        public OSDUUID(UUID value)
        {
            Type = OSDType.UUID;
            this.value = value;
        }

        public override OSD Copy() { return new OSDUUID(value); }
        public override bool AsBoolean() { return (value.IsZero()) ? false : true; }
        public override string AsString() { return value.ToString(); }
        public override UUID AsUUID() { return value; }
        public override byte[] AsBinary() { return value.GetBytes(); }
        public override string ToString() { return AsString(); }
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class OSDDate : OSD
    {
        public readonly DateTime value;

        public OSDDate(DateTime value)
        {
            Type = OSDType.Date;
            this.value = value;
        }

        public override string AsString()
        {
            string format;
            if (value.Millisecond > 0)
                format = "yyyy-MM-ddTHH:mm:ss.ffZ";
            else
                format = "yyyy-MM-ddTHH:mm:ssZ";
            return value.ToUniversalTime().ToString(format);
        }

        public override int AsInteger()
        {
            return (int)Utils.DateTimeToUnixTime(value);
        }

        public override uint AsUInteger()
        {
            return Utils.DateTimeToUnixTime(value);
        }

        public override long AsLong()
        {
            return (long)Utils.DateTimeToUnixTime(value);
        }

        public override ulong AsULong()
        {
            return Utils.DateTimeToUnixTime(value);
        }

        public override byte[] AsBinary()
        {
            TimeSpan ts = value.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return Utils.DoubleToBytes(ts.TotalSeconds);
        }

        public override OSD Copy() { return new OSDDate(value); }
        public override DateTime AsDate() { return value; }
        public override string ToString() { return AsString(); }
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class OSDUri : OSD
    {
        public readonly Uri value;

        public OSDUri(Uri value)
        {
            Type = OSDType.URI;
            this.value = value;
        }

        public override string AsString()
        {
            if (value != null)
            {
                if (value.IsAbsoluteUri)
                    return value.AbsoluteUri;
                else
                    return value.ToString();
            }
            return string.Empty;
        }

        public override OSD Copy() { return new OSDUri(value); }
        public override Uri AsUri() { return value; }
        public override byte[] AsBinary() { return Encoding.UTF8.GetBytes(AsString()); }
        public override string ToString() { return AsString(); }
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class OSDBinary : OSD
    {
        public readonly byte[] value;

        public OSDBinary(byte[] value)
        {
            Type = OSDType.Binary;
            if (value != null)
                this.value = value;
            else
                this.value = Array.Empty<byte>();
        }

        public OSDBinary(uint value)
        {
            Type = OSDType.Binary;
            this.value = new byte[]
            {
                (byte)((value >> 24) % 256),
                (byte)((value >> 16) % 256),
                (byte)((value >> 8) % 256),
                (byte)(value % 256)
            };
        }

        public OSDBinary(long value)
        {
            Type = OSDType.Binary;
            this.value = new byte[]
            {
                (byte)((value >> 56) % 256),
                (byte)((value >> 48) % 256),
                (byte)((value >> 40) % 256),
                (byte)((value >> 32) % 256),
                (byte)((value >> 24) % 256),
                (byte)((value >> 16) % 256),
                (byte)((value >> 8) % 256),
                (byte)(value % 256)
            };
        }

        public OSDBinary(ulong value)
        {
            Type = OSDType.Binary;
            this.value = new byte[]
            {
                (byte)((value >> 56) % 256),
                (byte)((value >> 48) % 256),
                (byte)((value >> 40) % 256),
                (byte)((value >> 32) % 256),
                (byte)((value >> 24) % 256),
                (byte)((value >> 16) % 256),
                (byte)((value >> 8) % 256),
                (byte)(value % 256)
            };
        }

        public override OSD Copy() { return new OSDBinary(value); }
        public override string AsString() { return Convert.ToBase64String(value); }
        public override byte[] AsBinary() { return value; }

        public override int AsInteger()
        {
            return (
                (value[0] << 24) +
                (value[1] << 16) +
                (value[2] << 8) +
                (value[3] << 0));
        }

        public override uint AsUInteger()
        {
            return (uint)(
                (value[0] << 24) +
                (value[1] << 16) +
                (value[2] << 8) +
                (value[3] << 0));
        }

        public override long AsLong()
        {
            return (long)(
                ((long)value[0] << 56) +
                ((long)value[1] << 48) +
                ((long)value[2] << 40) +
                ((long)value[3] << 32) +
                ((long)value[4] << 24) +
                ((long)value[5] << 16) +
                ((long)value[6] << 8) +
                ((long)value[7] << 0));
        }

        public override ulong AsULong()
        {
            return (ulong)(
                ((ulong)value[0] << 56) +
                ((ulong)value[1] << 48) +
                ((ulong)value[2] << 40) +
                ((ulong)value[3] << 32) +
                ((ulong)value[4] << 24) +
                ((ulong)value[5] << 16) +
                ((ulong)value[6] << 8) +
                ((ulong)value[7] << 0));
        }

        public override string ToString()
        {
            return Utils.BytesToHexString(value, null);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class OSDMap : OSD, IDictionary<string, OSD>
    {
        public readonly Dictionary<string, OSD> dicvalue;

        public OSDMap()
        {
            Type = OSDType.Map;
            dicvalue = new Dictionary<string, OSD>();
        }

        public OSDMap(int capacity)
        {
            Type = OSDType.Map;
            dicvalue = new Dictionary<string, OSD>(capacity);
        }

        public OSDMap(Dictionary<string, OSD> value)
        {
            Type = OSDType.Map;
            if (value != null)
                this.dicvalue = value;
            else
                this.dicvalue = new Dictionary<string, OSD>();
        }

        public override string ToString()
        {
            return OSDParser.SerializeJsonString(this, true);
        }

        public override OSD Copy()
        {
            return new OSDMap(new Dictionary<string, OSD>(dicvalue));
        }

        #region IDictionary Implementation

        public int Count { get { return dicvalue.Count; } }
        public bool IsReadOnly { get { return false; } }
        public ICollection<string> Keys { get { return dicvalue.Keys; } }
        public ICollection<OSD> Values { get { return dicvalue.Values; } }

        public OSD this[string key]
        {
            get
            {
                OSD llsd;
                if (dicvalue.TryGetValue(key, out llsd))
                    return llsd;
                else
                    return new OSD();
            }
            set { dicvalue[key] = value; }
        }

        public bool ContainsKey(string key)
        {
            return dicvalue.ContainsKey(key);
        }
        
        public void Add(string key, OSD llsd)
        {
            dicvalue.Add(key, llsd);
        }

        public void Add(KeyValuePair<string, OSD> kvp)
        {
            dicvalue.Add(kvp.Key, kvp.Value);
        }

        public bool Remove(string key)
        {
            return dicvalue.Remove(key);
        }

        public bool TryGetValue(string key, out OSD llsd)
        {
            return dicvalue.TryGetValue(key, out llsd);
        }

        public override void Clear()
        {
            dicvalue.Clear();
        }

        public bool Contains(KeyValuePair<string, OSD> kvp)
        {
            // This is a bizarre function... we don't really implement it
            // properly, hopefully no one wants to use it
            return dicvalue.ContainsKey(kvp.Key);
        }

        public void CopyTo(KeyValuePair<string, OSD>[] array, int index)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<string, OSD> kvp)
        {
            return dicvalue.Remove(kvp.Key);
        }

        public System.Collections.IDictionaryEnumerator GetEnumerator()
        {
            return dicvalue.GetEnumerator();
        }

        IEnumerator<KeyValuePair<string, OSD>> IEnumerable<KeyValuePair<string, OSD>>.GetEnumerator()
        {
            return null;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return dicvalue.GetEnumerator();
        }

        #endregion IDictionary Implementation
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class OSDArray : OSD, IList<OSD>
    {
        public readonly List<OSD> value;

        public OSDArray()
        {
            Type = OSDType.Array;
            value = new List<OSD>();
        }

        public OSDArray(int capacity)
        {
            Type = OSDType.Array;
            value = new List<OSD>(capacity);
        }

        public OSDArray(List<OSD> value)
        {
            Type = OSDType.Array;
            if (value != null)
                this.value = value;
            else
                this.value = new List<OSD>();
        }

        public override byte[] AsBinary()
        {
            byte[] binary = new byte[value.Count];

            for (int i = 0; i < value.Count; i++)
                binary[i] = (byte)value[i].AsInteger();

            return binary;
        }

        public override long AsLong()
        {
            if (value.Count < 8)
                return 0;
            byte[] b = new byte[8];
            for (int i = 0; i < 8; i++)
                b[i] = (byte)value[i].AsInteger();
            return (
                ((long)b[0] << 56) |
                ((long)b[1] << 48) |
                ((long)b[2] << 40) |
                ((long)b[3] << 32) |
                ((long)b[4] << 24) |
                ((long)b[5] << 16) |
                ((long)b[6] << 8) |
                b[7]);
        }

        public override ulong AsULong()
        {
            if (value.Count < 8)
                return 0;
            byte[] b = new byte[8];
            for (int i = 0; i < 8; i++)
                b[i] = (byte)value[i].AsInteger();
            return (
                ((ulong)b[0] << 56) |
                ((ulong)b[1] << 48) |
                ((ulong)b[2] << 40) |
                ((ulong)b[3] << 32) |
                ((ulong)b[4] << 24) |
                ((ulong)b[5] << 16) |
                ((ulong)b[6] << 8) |
                b[7]);
        }

        public override int AsInteger()
        {
            if (value.Count < 4)
                return 0;
            byte[] by = new byte[4];
            for (int i = 0; i < 4; i++)
                by[i] = (byte)value[i].AsInteger();
            return (by[0] << 24) | (by[1] << 16) | (by[2] << 8) | by[3];
        }

        public override uint AsUInteger()
        {
            if (value.Count < 4)
                return 0;
            byte[] by = new byte[4];
            for (int i = 0; i < 4; i++)
                by[i] = (byte)value[i].AsInteger();
            return (uint)((by[0] << 24) | (by[1] << 16) | (by[2] << 8) | by[3]);
        }
        /*
        public override Vector2 AsVector2()
        {
            Vector2 vector = Vector2.Zero;

            if (this.Count == 2)
            {
                vector.X = (float)this[0].AsReal();
                vector.Y = (float)this[1].AsReal();
            }

            return vector;
        }

        public override Vector3 AsVector3()
        {
            Vector3 vector = Vector3.Zero;

            if (this.Count == 3)
            {
                vector.X = this[0].AsReal();
                vector.Y = this[1].AsReal();
                vector.Z = this[2].AsReal();
            }

            return vector;
        }

        public override Vector3d AsVector3d()
        {
            Vector3d vector = Vector3d.Zero;

            if (this.Count == 3)
            {
                vector.X = this[0].AsReal();
                vector.Y = this[1].AsReal();
                vector.Z = this[2].AsReal();
            }

            return vector;
        }

        public override Vector4 AsVector4()
        {
            Vector4 vector = Vector4.Zero;

            if (this.Count == 4)
            {
                vector.X = (float)this[0].AsReal();
                vector.Y = (float)this[1].AsReal();
                vector.Z = (float)this[2].AsReal();
                vector.W = (float)this[3].AsReal();
            }

            return vector;
        }

        public override Quaternion AsQuaternion()
        {
            Quaternion quaternion = Quaternion.Identity;

            if (this.Count == 4)
            {
                quaternion.X = (float)this[0].AsReal();
                quaternion.Y = (float)this[1].AsReal();
                quaternion.Z = (float)this[2].AsReal();
                quaternion.W = (float)this[3].AsReal();
            }

            return quaternion;
        }
        */
        public override Color4 AsColor4()
        {
            Color4 color = Color4.Black;

            if (this.Count == 4)
            {
                color.R = (float)this[0].AsReal();
                color.G = (float)this[1].AsReal();
                color.B = (float)this[2].AsReal();
                color.A = (float)this[3].AsReal();
            }

            return color;
        }

        public override OSD Copy()
        {
            return new OSDArray(new List<OSD>(value));
        }

        public override string ToString()
        {
            return OSDParser.SerializeJsonString(this, true);
        }

        #region IList Implementation

        public int Count { get { return value.Count; } }
        public bool IsReadOnly { get { return false; } }
        public OSD this[int index]
        {
            get { return value[index]; }
            set { this.value[index] = value; }
        }

        public int IndexOf(OSD llsd)
        {
            return value.IndexOf(llsd);
        }

        public void Insert(int index, OSD llsd)
        {
            value.Insert(index, llsd);
        }

        public void RemoveAt(int index)
        {
            value.RemoveAt(index);
        }

        public void Add(OSD llsd)
        {
            value.Add(llsd);
        }

        public override void Clear()
        {
            value.Clear();
        }

        public bool Contains(OSD llsd)
        {
            return value.Contains(llsd);
        }

        public bool Contains(string element)
        {
            for (int i = 0; i < value.Count; i++)
            {
                if (value[i].Type == OSDType.String && value[i].AsString() == element)
                    return true;
            }

            return false;
        }

        public void CopyTo(OSD[] array, int index)
        {
            throw new NotImplementedException();
        }

        public bool Remove(OSD llsd)
        {
            return value.Remove(llsd);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return value.GetEnumerator();
        }

        IEnumerator<OSD> IEnumerable<OSD>.GetEnumerator()
        {
            return value.GetEnumerator();
        }

        #endregion IList Implementation
    }

    public partial class OSDParser
    {
        const string LLSD_BINARY_HEADER   = "<? llsd/binary";
        const string LLSD_NOTATION_HEADER = "<? llsd/notatio";
        const string LLSD_XML_HEADER      = "<llsd>";
        const string LLSD_XML_ALT_HEADER  = "<?xml";
        const string LLSD_XML_ALT2_HEADER = "<? llsd/xml";

        public static OSD Deserialize(byte[] data)
        {
            string header = Encoding.ASCII.GetString(data, 0, data.Length >= 15 ? 15 : data.Length);

            if (header.StartsWith(LLSD_XML_HEADER, StringComparison.InvariantCultureIgnoreCase) ||
                    header.StartsWith(LLSD_XML_ALT_HEADER, StringComparison.InvariantCultureIgnoreCase) ||
                    header.StartsWith(LLSD_XML_ALT2_HEADER, StringComparison.InvariantCultureIgnoreCase))
                return DeserializeLLSDXml(data);

            if (header.StartsWith(LLSD_NOTATION_HEADER, StringComparison.InvariantCultureIgnoreCase))
                return DeserializeLLSDNotation(data);

            if (header.StartsWith(LLSD_BINARY_HEADER, StringComparison.InvariantCultureIgnoreCase))
                return DeserializeLLSDBinary(data);

            return DeserializeJson(data);
        }

        public static OSD Deserialize(string data)
        {
            if (data.StartsWith(LLSD_XML_HEADER, StringComparison.InvariantCultureIgnoreCase) ||
                    data.StartsWith(LLSD_XML_ALT_HEADER, StringComparison.InvariantCultureIgnoreCase) ||
                    data.StartsWith(LLSD_XML_ALT2_HEADER, StringComparison.InvariantCultureIgnoreCase))
                return DeserializeLLSDXml(data);

            if (data.StartsWith(LLSD_NOTATION_HEADER, StringComparison.InvariantCultureIgnoreCase))
                return DeserializeLLSDNotation(data);

            if (data.StartsWith(LLSD_BINARY_HEADER, StringComparison.InvariantCultureIgnoreCase))
                return DeserializeLLSDBinary(Encoding.UTF8.GetBytes(data));

            return DeserializeJson(data);
        }

        public static OSD Deserialize(Stream stream)
        {
            if (stream.CanSeek)
            {
                byte[] headerData = new byte[15];
                stream.Read(headerData, 0, 15);
                stream.Seek(0, SeekOrigin.Begin);
                string header = Encoding.ASCII.GetString(headerData);

                if (header.StartsWith(LLSD_XML_HEADER) || header.StartsWith(LLSD_XML_ALT_HEADER) || header.StartsWith(LLSD_XML_ALT2_HEADER))
                    return DeserializeLLSDXml(stream);

                if (header.StartsWith(LLSD_NOTATION_HEADER))
                    return DeserializeLLSDNotation(stream);

                if (header.StartsWith(LLSD_BINARY_HEADER))
                    return DeserializeLLSDBinary(stream);

                return DeserializeJson(stream);
            }
            else
            {
                throw new OSDException("Cannot deserialize structured data from unseekable streams");
            }
        }
    }
}
