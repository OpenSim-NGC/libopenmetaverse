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
using OpenMetaverse.StructuredData;

namespace OpenMetaverse
{
    #region Enumerations

    /// <summary>
    /// The type of bump-mapping applied to a face
    /// </summary>
    public enum Bumpiness : byte
    {
        /// <summary></summary>
        None = 0,
        /// <summary></summary>
        Brightness = 1,
        /// <summary></summary>
        Darkness = 2,
        /// <summary></summary>
        Woodgrain = 3,
        /// <summary></summary>
        Bark = 4,
        /// <summary></summary>
        Bricks = 5,
        /// <summary></summary>
        Checker = 6,
        /// <summary></summary>
        Concrete = 7,
        /// <summary></summary>
        Crustytile = 8,
        /// <summary></summary>
        Cutstone = 9,
        /// <summary></summary>
        Discs = 10,
        /// <summary></summary>
        Gravel = 11,
        /// <summary></summary>
        Petridish = 12,
        /// <summary></summary>
        Siding = 13,
        /// <summary></summary>
        Stonetile = 14,
        /// <summary></summary>
        Stucco = 15,
        /// <summary></summary>
        Suction = 16,
        /// <summary></summary>
        Weave = 17
    }

    /// <summary>
    /// The level of shininess applied to a face
    /// </summary>
    public enum Shininess : byte
    {
        /// <summary></summary>
        None = 0,
        /// <summary></summary>
        Low = 0x40,
        /// <summary></summary>
        Medium = 0x80,
        /// <summary></summary>
        High = 0xC0
    }

    /// <summary>
    /// The texture mapping style used for a face
    /// </summary>
    public enum MappingType : byte
    {
        /// <summary></summary>
        Default = 0,
        /// <summary></summary>
        Planar = 2,
        /// <summary></summary>
        Spherical = 4,
        /// <summary></summary>
        Cylindrical = 6
    }

    /// <summary>
    /// Flags in the TextureEntry block that describe which properties are 
    /// set
    /// </summary>
    [Flags]
    public enum TextureAttributes : uint
    {
        /// <summary></summary>
        None = 0,
        /// <summary></summary>
        TextureID = 1 << 0,
        /// <summary></summary>
        RGBA = 1 << 1,
        /// <summary></summary>
        RepeatU = 1 << 2,
        /// <summary></summary>
        RepeatV = 1 << 3,
        /// <summary></summary>
        OffsetU = 1 << 4,
        /// <summary></summary>
        OffsetV = 1 << 5,
        /// <summary></summary>
        Rotation = 1 << 6,
        /// <summary></summary>
        Material = 1 << 7,
        /// <summary></summary>
        Media = 1 << 8,
        /// <summary></summary>
        Glow = 1 << 9,
        /// <summary></summary>
        MaterialID = 1 << 10,
        /// <summary></summary>
        All = 0xFFFFFFFF
    }

    #endregion Enumerations

    public partial class Primitive
    {
        #region Enums

        /// <summary>
        /// Texture animation mode
        /// </summary>
        [Flags]
        public enum TextureAnimMode : uint
        {
            /// <summary>Disable texture animation</summary>
            ANIM_OFF = 0x00,
            /// <summary>Enable texture animation</summary>
            ANIM_ON = 0x01,
            /// <summary>Loop when animating textures</summary>
            LOOP = 0x02,
            /// <summary>Animate in reverse direction</summary>
            REVERSE = 0x04,
            /// <summary>Animate forward then reverse</summary>
            PING_PONG = 0x08,
            /// <summary>Slide texture smoothly instead of frame-stepping</summary>
            SMOOTH = 0x10,
            /// <summary>Rotate texture instead of using frames</summary>
            ROTATE = 0x20,
            /// <summary>Scale texture instead of using frames</summary>
            SCALE = 0x40,
        }

        #endregion Enums

        #region Subclasses

        /// <summary>
        /// A single textured face. Don't instantiate this class yourself, use the
        /// methods in TextureEntry
        /// </summary>
        public class TextureEntryFace : ICloneable
        {
            // +----------+ S = Shiny
            // | SSFBBBBB | F = Fullbright
            // | 76543210 | B = Bumpmap
            // +----------+
            private const byte BUMP_MASK = 0x1F;
            private const byte FULLBRIGHT_MASK = 0x20;
            private const byte SHINY_MASK = 0xC0;
            // +----------+ M = Media Flags (web page)
            // | .....TTM | T = Texture Mapping
            // | 76543210 | . = Unused
            // +----------+
            private const byte MEDIA_MASK = 0x01;
            private const byte TEX_MAP_MASK = 0x06;

            internal UUID m_textureID;
            internal UUID m_materialID;
            private TextureEntryFace DefaultTexture;
            internal Color4 m_rgba;
            internal float m_repeatU;
            internal float m_repeatV;
            internal short m_offsetU;
            internal short m_offsetV;
            internal short m_rotation;
            internal byte m_glow;
            private TextureAttributes m_dirtyFlags;
            internal TextureAttributes m_attributes;
            internal byte m_material;
            internal byte m_media;

            #region Properties
 
            /// <summary></summary>
            public Color4 RGBA
            {
                get
                {
                    if((m_attributes & TextureAttributes.RGBA) == 0)
                        return DefaultTexture.m_rgba;
                    return m_rgba;
                }
                set
                {
                    Color4 c = m_rgba;
                    if ((m_attributes & TextureAttributes.RGBA) == 0)
                        c = DefaultTexture.m_rgba;

                    if (c != value)
                    {
                        m_rgba = value;
                        m_dirtyFlags |= TextureAttributes.RGBA;
                        m_attributes |= TextureAttributes.RGBA;
                    }
                }
            }

            /// <summary></summary>
            public float RepeatU
            {
                get
                {
                    if ((m_attributes & TextureAttributes.RepeatU) == 0)
                        return DefaultTexture.m_repeatU;
                    return m_repeatU;
                }
                set
                {
                    float t = m_repeatU;
                    if ((m_attributes & TextureAttributes.RepeatU) == 0)
                        t = DefaultTexture.m_repeatU;

                    if (t != value)
                    { 
                        m_repeatU = value;
                        m_dirtyFlags |= TextureAttributes.RepeatU;
                        m_attributes |= TextureAttributes.RepeatU;
                    }
                }
            }

            /// <summary></summary>
            public float RepeatV
            {
                get
                {
                    if ((m_attributes & TextureAttributes.RepeatV) == 0)
                        return DefaultTexture.m_repeatV;
                    return m_repeatV;
                }
                set
                {
                    float t = m_repeatV;
                    if ((m_attributes & TextureAttributes.RepeatV) == 0)
                        t = DefaultTexture.m_repeatV;

                    if (t != value)
                    {
                        m_repeatV = value;
                        m_dirtyFlags |= TextureAttributes.RepeatV;
                        m_attributes |= TextureAttributes.RepeatV;
                    }
                }
            }

            /// <summary></summary>
            public float OffsetU
            {
                get
                {
                    if ((m_attributes & TextureAttributes.OffsetU) == 0)
                        return Helpers.TEOffsetFloat(DefaultTexture.m_offsetU);
                    return Helpers.TEOffsetFloat(m_offsetU);
                }
                set
                {
                    short o = m_offsetU;
                    if ((m_attributes & TextureAttributes.OffsetU) == 0)
                        o = DefaultTexture.m_offsetU;

                    short ts = Helpers.TEOffsetShort(value);
                    if(o != ts)
                    {
                        m_offsetU = ts;
                        m_dirtyFlags |= TextureAttributes.OffsetU;
                        m_attributes |= TextureAttributes.OffsetU;
                    }
                }
            }

            /// <summary></summary>
            public float OffsetV
            {
                get
                {
                    if ((m_attributes & TextureAttributes.OffsetV) == 0)
                        return Helpers.TEOffsetFloat(DefaultTexture.m_offsetV);
                    return Helpers.TEOffsetFloat(m_offsetV);
                }
                set
                {
                    short o = m_offsetV;
                    if ((m_attributes & TextureAttributes.OffsetV) == 0)
                        o = DefaultTexture.m_offsetV;

                    short ts = Helpers.TEOffsetShort(value);
                    if (o != ts)
                    {
                        m_offsetV = ts;
                        m_dirtyFlags |= TextureAttributes.OffsetV;
                        m_attributes |= TextureAttributes.OffsetV;
                    }
                }
            }

            /// <summary></summary>
            public float Rotation
            {
                get
                {
                    if ((m_attributes & TextureAttributes.Rotation) == 0)
                        return Helpers.TERotationFloat(DefaultTexture.m_rotation);
                    return Helpers.TERotationFloat(m_rotation);
                }
                set
                {
                    short o = m_rotation;
                    if ((m_attributes & TextureAttributes.Rotation) == 0)
                        o = DefaultTexture.m_rotation;

                    short ts = Helpers.TERotationShort(value);
                    if(o != ts)
                    {
                        m_rotation = ts;
                        m_dirtyFlags |= TextureAttributes.Rotation;
                        m_attributes |= TextureAttributes.Rotation;
                    }
                }
            }

            /// <summary></summary>
            public float Glow
            {
                get
                {
                    if ((m_attributes & TextureAttributes.Glow) == 0)
                        return Helpers.TEGlowFloat(DefaultTexture.m_glow);
                    return Helpers.TEGlowFloat(m_glow);
                }
                set
                {
                    byte o = m_glow;
                    if ((m_attributes & TextureAttributes.Glow) == 0)
                        o = DefaultTexture.m_glow;

                    byte tb = Helpers.TEGlowByte(value);
                    if(o != tb)
                    {
                        m_glow = tb;
                        m_dirtyFlags |= TextureAttributes.Glow;
                        m_attributes |= TextureAttributes.Glow;
                    }
                }
            }

            /// <summary></summary>
            public Bumpiness Bump
            {
                get
                {
                    if ((m_attributes & TextureAttributes.Material) == 0)
                        return (Bumpiness)(DefaultTexture.m_material & BUMP_MASK);
                    return (Bumpiness)(m_material & BUMP_MASK);
                }
                set
                {
                    byte o = m_material;
                    if ((m_attributes & TextureAttributes.Material) == 0)
                        m_material = DefaultTexture.m_material;

                    byte tb = (byte)(m_material & 0xE0);
                    tb |= (byte)value;

                    if(o != tb)
                    {
                        m_material = tb;
                        m_dirtyFlags |= TextureAttributes.Material;
                        m_attributes |= TextureAttributes.Material;
                    }
                }
            }

            public Shininess Shiny
            {
                get
                {
                    if ((m_attributes & TextureAttributes.Material) == 0)
                        return (Shininess)(DefaultTexture.m_material & SHINY_MASK);
                    return (Shininess)(m_material & SHINY_MASK);
                }
                set
                {
                    byte o = m_material;
                    if ((m_attributes & TextureAttributes.Material) == 0)
                        m_material = DefaultTexture.m_material;

                    // Clear out the old shiny value
                    byte tb = (byte)(m_material & 0x3F);
                    // Put the new shiny value in the material byte
                    tb |= (byte)value;
                    if (o != tb)
                    {
                        m_material = tb;
                        m_dirtyFlags |= TextureAttributes.Material;
                        m_attributes |= TextureAttributes.Material;
                    }
                }
            }

            public bool Fullbright
            {
                get
                {
                    if ((m_attributes & TextureAttributes.Material) == 0)
                        return (DefaultTexture.m_material & FULLBRIGHT_MASK) != 0;
                    return (m_material & FULLBRIGHT_MASK) != 0;
                }
                set
                {
                    byte o = m_material;
                    if ((m_attributes & TextureAttributes.Material) == 0)
                        m_material = DefaultTexture.m_material;

                    // Clear out the old fullbright value
                    byte tb = (byte)(m_material & 0xDF);
                    if (value)
                        tb |= 0x20;

                    if(o != tb)
                    {
                        m_material = tb;
                        m_dirtyFlags |= TextureAttributes.Material;
                        m_attributes |= TextureAttributes.Material;
                    }
                }
            }

            /// <summary>In the future this will specify whether a webpage is
            /// attached to this face</summary>
            public bool MediaFlags
            {
                get
                {
                    if ((m_attributes & TextureAttributes.Media) == 0)
                        return (DefaultTexture.m_media & MEDIA_MASK) != 0;
                    return (m_media & MEDIA_MASK) != 0;
                }
                set
                {
                    byte o = m_media;
                    if ((m_attributes & TextureAttributes.Media) == 0)
                        m_media = DefaultTexture.m_media;

                    // Clear out the old mediaflags value
                    byte tb = (byte)(m_media & 0xFE);
                    if (value)
                        tb |= 0x01;
                    if(o != tb)
                    {
                        m_media = tb;
                        m_dirtyFlags |= TextureAttributes.Media;
                        m_attributes |= TextureAttributes.Media;
                    }
                }
            }

            public MappingType TexMapType
            {
                get
                {
                    if ((m_attributes & TextureAttributes.Media) == 0)
                        return (MappingType)(DefaultTexture.m_media & TEX_MAP_MASK);
                    return (MappingType)(m_media & TEX_MAP_MASK);
                }
                set
                {
                    byte o = m_media;
                    if ((m_attributes & TextureAttributes.Media) == 0)
                        m_media = DefaultTexture.m_media;

                    byte tb = (byte)(m_media & 0xF9);
                    tb |= (byte)value;

                    if(tb != o)
                    {
                        m_media = tb;
                        m_dirtyFlags |= TextureAttributes.Media;
                        m_attributes |= TextureAttributes.Media;
                    }
                }
            }

            /// <summary></summary>
            public UUID TextureID
            {
                get
                {
                    if ((m_attributes & TextureAttributes.TextureID) == 0)
                        return (DefaultTexture.m_textureID);
                    return m_textureID;
                }
                set
                {
                    UUID od = m_textureID;
                    if ((m_attributes & TextureAttributes.TextureID) == 0)
                        od = DefaultTexture.m_textureID;

                    if (od != value)
                    {
                        m_textureID = value;
                        m_dirtyFlags |= TextureAttributes.TextureID;
                        m_attributes |= TextureAttributes.TextureID;
                    }
                }
            }

            /// <summary></summary>
            public UUID MaterialID
            {
                get
                {
                    if ((m_attributes & TextureAttributes.MaterialID) == 0)
                        return (DefaultTexture.m_materialID);
                    return m_materialID;
                }
                set
                {
                    UUID od = m_materialID;
                    if ((m_attributes & TextureAttributes.MaterialID) == 0)
                        od = DefaultTexture.MaterialID;

                    if (od != value)
                    {
                        m_materialID = value;
                        m_dirtyFlags |= TextureAttributes.MaterialID;
                        m_attributes |= TextureAttributes.MaterialID;
                    }
                }
            }

            public TextureAttributes DirtyFlags
            {
                get
                {
                    return m_dirtyFlags;
                }
                set
                {
                    m_dirtyFlags = value;
                }
            }
            #endregion Properties

            /// <summary>
            /// Contains the definition for individual faces
            /// </summary>
            /// <param name="defaultTexture"></param>
            public TextureEntryFace(TextureEntryFace defaultTexture)
            {
                m_rgba = Color4.White;
                m_repeatU = 1.0f;
                m_repeatV = 1.0f;

                DefaultTexture = defaultTexture;
                if (DefaultTexture == null)
                    m_attributes = TextureAttributes.All;
                else
                    m_attributes = TextureAttributes.None;
                m_dirtyFlags = TextureAttributes.None;
            }

            public OSD GetOSD(int faceNumber)
            {
                OSDMap tex = new OSDMap(10);
                if (faceNumber >= 0) tex["face_number"] = OSD.FromInteger(faceNumber);
                tex["colors"] = OSD.FromColor4(RGBA);
                tex["scales"] = OSD.FromReal(RepeatU);
                tex["scalet"] = OSD.FromReal(RepeatV);
                tex["offsets"] = OSD.FromReal(OffsetU);
                tex["offsett"] = OSD.FromReal(OffsetV);
                tex["imagerot"] = OSD.FromReal(Rotation);
                tex["bump"] = OSD.FromInteger((int)Bump);
                tex["shiny"] = OSD.FromInteger((int)Shiny);
                tex["fullbright"] = OSD.FromBoolean(Fullbright);
                tex["media_flags"] = OSD.FromInteger(Convert.ToInt32(MediaFlags));
                tex["mapping"] = OSD.FromInteger((int)TexMapType);
                tex["glow"] = OSD.FromReal(Glow);

                if (TextureID != Primitive.TextureEntry.WHITE_TEXTURE)
                    tex["imageid"] = OSD.FromUUID(TextureID);
                else
                    tex["imageid"] = OSD.FromUUID(UUID.Zero);

                tex["materialid"] = OSD.FromUUID(m_materialID);

                return tex;
            }

            public static TextureEntryFace FromOSD(OSD osd, TextureEntryFace defaultFace, out int faceNumber)
            {
                OSDMap map = (OSDMap)osd;

                TextureEntryFace face = new TextureEntryFace(defaultFace);
                faceNumber = (map.ContainsKey("face_number")) ? map["face_number"].AsInteger() : -1;
                Color4 rgba = face.RGBA;
                rgba = ((OSDArray)map["colors"]).AsColor4();
                face.RGBA = rgba;
                face.RepeatU = (float)map["scales"].AsReal();
                face.RepeatV = (float)map["scalet"].AsReal();
                face.OffsetU = (float)map["offsets"].AsReal();
                face.OffsetV = (float)map["offsett"].AsReal();
                face.Rotation = (float)map["imagerot"].AsReal();
                face.Bump = (Bumpiness)map["bump"].AsInteger();
                face.Shiny = (Shininess)map["shiny"].AsInteger();
                face.Fullbright = map["fullbright"].AsBoolean();
                face.MediaFlags = map["media_flags"].AsBoolean();
                face.TexMapType = (MappingType)map["mapping"].AsInteger();
                face.Glow = (float)map["glow"].AsReal();
                face.TextureID = map["imageid"].AsUUID();
                face.MaterialID = map["materialid"];
                return face;
            }

            public object Clone()
            {
                TextureEntryFace ret = new TextureEntryFace(this.DefaultTexture == null ? null : (TextureEntryFace)this.DefaultTexture.Clone());
                ret.m_textureID = m_textureID;
                ret.m_materialID = m_materialID;
                ret.m_rgba = m_rgba;
                ret.m_repeatU = m_repeatU;
                ret.m_repeatV = m_repeatV;
                ret.m_offsetU = m_offsetU;
                ret.m_offsetV = m_offsetV;
                ret.m_rotation = m_rotation;
                ret.m_glow = m_glow;
                ret.m_attributes = m_attributes;
                ret.m_dirtyFlags = m_dirtyFlags;
                ret.m_material = m_material;
                ret.m_media = m_media;
                return ret;
            }

            public override int GetHashCode()
            {
                return
                    m_textureID.GetHashCode() ^
                    m_materialID.GetHashCode() ^
                    m_rgba.GetHashCode() ^
                    m_repeatU.GetHashCode() ^
                    m_repeatV.GetHashCode() ^
                    m_offsetU.GetHashCode() ^
                    m_offsetV.GetHashCode() ^
                    m_glow.GetHashCode() ^
                    m_rotation.GetHashCode() ^
                    m_material.GetHashCode() ^
                    m_media.GetHashCode();
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return String.Format("Color: {0} RepeatU: {1} RepeatV: {2} OffsetU: {3} OffsetV: {4} " +
                    "Rotation: {5} Bump: {6} Shiny: {7} Fullbright: {8} Mapping: {9} Media: {10} Glow: {11} ID: {12} MaterialID: {13}",
                    RGBA, RepeatU, RepeatV, OffsetU, OffsetV, Rotation, Bump, Shiny, Fullbright, TexMapType,
                    MediaFlags, Glow, TextureID, MaterialID);
            }
        }

        /// <summary>
        /// Represents all of the texturable faces for an object
        /// </summary>
        /// <remarks>Grid objects have infinite faces, with each face
        /// using the properties of the default face unless set otherwise. So if
        /// you have a TextureEntry with a default texture uuid of X, and face 18
        /// has a texture UUID of Y, every face would be textured with X except for
        /// face 18 that uses Y. In practice however, primitives utilize a maximum
        /// of nine faces</remarks>
        public class TextureEntry
        {
            public const int MAX_FACES = 45;
            public static readonly UUID WHITE_TEXTURE = new UUID("5748decc-f629-461c-9a36-a35a221fe21f");

            /// <summary></summary>
            public TextureEntryFace DefaultTexture;
            /// <summary></summary>
            public TextureEntryFace[] FaceTextures = new TextureEntryFace[MAX_FACES];

            /// <summary>
            /// Constructor that takes a default texture UUID
            /// </summary>
            /// <param name="defaultTextureID">Texture UUID to use as the default texture</param>
            public TextureEntry(UUID defaultTextureID)
            {
                DefaultTexture = new TextureEntryFace(null);
                DefaultTexture.TextureID = defaultTextureID;
            }

            /// <summary>
            /// Constructor that takes a <code>TextureEntryFace</code> for the
            /// default face
            /// </summary>
            /// <param name="defaultFace">Face to use as the default face</param>
            public TextureEntry(TextureEntryFace defaultFace)
            {
                DefaultTexture = new TextureEntryFace(null);
                DefaultTexture.Bump = defaultFace.Bump;
                DefaultTexture.Fullbright = defaultFace.Fullbright;
                DefaultTexture.MediaFlags = defaultFace.MediaFlags;
                DefaultTexture.OffsetU = defaultFace.OffsetU;
                DefaultTexture.OffsetV = defaultFace.OffsetV;
                DefaultTexture.RepeatU = defaultFace.RepeatU;
                DefaultTexture.RepeatV = defaultFace.RepeatV;
                DefaultTexture.RGBA = defaultFace.RGBA;
                DefaultTexture.Rotation = defaultFace.Rotation;
                DefaultTexture.Glow = defaultFace.Glow;
                DefaultTexture.Shiny = defaultFace.Shiny;
                DefaultTexture.TexMapType = defaultFace.TexMapType;
                DefaultTexture.TextureID = defaultFace.TextureID;
                DefaultTexture.MaterialID = defaultFace.MaterialID;
            }

            /// <summary>
            /// Constructor that creates the TextureEntry class from a byte array
            /// </summary>
            /// <param name="data">Byte array containing the TextureEntry field</param>
            /// <param name="pos">Starting position of the TextureEntry field in 
            /// the byte array</param>
            /// <param name="length">Length of the TextureEntry field, in bytes</param>
            public TextureEntry(byte[] data, int pos, int length)
            {
                FromBytes(data, pos, length);
            }

            /// <summary>
            /// This will either create a new face if a custom face for the given
            /// index is not defined, or return the custom face for that index if
            /// it already exists
            /// </summary>
            /// <param name="index">The index number of the face to create or 
            /// retrieve</param>
            /// <returns>A TextureEntryFace containing all the properties for that
            /// face</returns>
            public TextureEntryFace CreateFace(uint index)
            {
                if (index >= MAX_FACES) throw new Exception(index + " is outside the range of MAX_FACES");

                if (FaceTextures[index] == null)
                    FaceTextures[index] = new TextureEntryFace(DefaultTexture);

                return FaceTextures[index];
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="index"></param>
            /// <returns></returns>
            public TextureEntryFace GetFace(uint index)
            {
                if (index >= MAX_FACES) throw new Exception(index + " is outside the range of MAX_FACES");
                if (FaceTextures[index] == null)
                    FaceTextures[index] = new TextureEntryFace(DefaultTexture);

                return FaceTextures[index];
            }

            public TextureAttributes GetDirtyFlags(int lenght, bool clear)
            {
                if (lenght > MAX_FACES)
                    lenght = MAX_FACES;

                TextureAttributes flags = TextureAttributes.None;
                if (DefaultTexture != null)
                {
                    flags = DefaultTexture.DirtyFlags;
                    if (clear)
                        DefaultTexture.DirtyFlags = TextureAttributes.None;
                }
                for (int i = lenght - 1; i >= 0; --i)
                {
                    if (FaceTextures[i] != null)
                    {
                        flags |= FaceTextures[i].DirtyFlags;
                        if (clear)
                            FaceTextures[i].DirtyFlags = TextureAttributes.None;
                     }
                }
                return flags;
            }

            public void SetDirtyFlags(int lenght, TextureAttributes flags)
            {
                if (lenght > MAX_FACES)
                    lenght = MAX_FACES;

                if (DefaultTexture != null)
                     DefaultTexture.DirtyFlags = flags;
                for (int i = lenght - 1; i >= 0; --i)
                {
                    if (FaceTextures[i] != null)
                         FaceTextures[i].DirtyFlags = flags;

                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public OSD GetOSD()
            {
                OSDArray array = new OSDArray();

                // If DefaultTexture is null, assume the whole TextureEntry is empty
                if (DefaultTexture == null)
                    return array;

                // Otherwise, always add default texture
                array.Add(DefaultTexture.GetOSD(-1));

                for (int i = 0; i < MAX_FACES; i++)
                {
                    if (FaceTextures[i] != null)
                        array.Add(FaceTextures[i].GetOSD(i));
                }

                return array;
            }

            public static TextureEntry FromOSD(OSD osd)
            {
                if (osd.Type == OSDType.Array)
                {
                    OSDArray array = (OSDArray)osd;
                    OSDMap faceSD;

                    if (array.Count > 0)
                    {
                        int faceNumber;
                        faceSD = (OSDMap)array[0];
                        TextureEntryFace defaultFace = TextureEntryFace.FromOSD(faceSD, null, out faceNumber);
                        TextureEntry te = new TextureEntry(defaultFace);

                        for (int i = 1; i < array.Count; i++)
                        {
                            TextureEntryFace tex = TextureEntryFace.FromOSD(array[i], defaultFace, out faceNumber);
                            if (faceNumber >= 0 && faceNumber < te.FaceTextures.Length)
                                te.FaceTextures[faceNumber] = tex;
                        }

                        return te;
                    }
                }

                return new TextureEntry(UUID.Zero);
            }

            private void FromBytes(byte[] data, int pos, int length)
            {
                if (length < 16)
                {
                    // No TextureEntry to process
                    DefaultTexture = null;
                    return;
                }
                else
                {
                    DefaultTexture = new TextureEntryFace(null);
                }

                ulong faceBits = 0;
                ulong bit;
                uint bitfieldSize = 0;
                uint face;
                int i = pos;

                #region Texture
                DefaultTexture.m_textureID = new UUID(data, i);
                i += 16;

                while (ReadFaceBitfield(data, ref i, ref faceBits, ref bitfieldSize))
                {
                    if (bitfieldSize > MAX_FACES)
                        bitfieldSize = MAX_FACES;
                    UUID tmpUUID = new UUID(data, i);
                    i += 16;
                    for (face = 0, bit = 1; face < bitfieldSize; face++, bit <<= 1)
                        if ((faceBits & bit) != 0)
                        { 
                            CreateFace(face).m_textureID = tmpUUID;
                            FaceTextures[face].m_attributes |= TextureAttributes.TextureID;
                        }
                }
                #endregion Texture

                #region Color
                DefaultTexture.m_rgba = new Color4(data, i, true);
                i += 4;

                while (ReadFaceBitfield(data, ref i, ref faceBits, ref bitfieldSize))
                {
                    if (bitfieldSize > MAX_FACES)
                        bitfieldSize = MAX_FACES;
                    Color4 tmpColor = new Color4(data, i, true);
                    i += 4;

                    for (face = 0, bit = 1; face < bitfieldSize; face++, bit <<= 1)
                        if ((faceBits & bit) != 0)
                        {
                            CreateFace(face).m_rgba = tmpColor;
                            FaceTextures[face].m_attributes |= TextureAttributes.RGBA;
                        }
                }
                #endregion Color

                #region RepeatU
                DefaultTexture.m_repeatU = Utils.BytesToFloat(data, i);
                i += 4;

                while (ReadFaceBitfield(data, ref i, ref faceBits, ref bitfieldSize))
                {
                    if (bitfieldSize > MAX_FACES)
                        bitfieldSize = MAX_FACES;
                    float tmpFloat = Utils.BytesToFloat(data, i);
                    i += 4;

                    for (face = 0, bit = 1; face < bitfieldSize; face++, bit <<= 1)
                        if ((faceBits & bit) != 0)
                        {
                            CreateFace(face).m_repeatU = tmpFloat;
                            FaceTextures[face].m_attributes |= TextureAttributes.RepeatU;
                        }
                }
                #endregion RepeatU

                #region RepeatV
                DefaultTexture.m_repeatV = Utils.BytesToFloat(data, i);
                i += 4;

                while (ReadFaceBitfield(data, ref i, ref faceBits, ref bitfieldSize))
                {
                    float tmpFloat = Utils.BytesToFloat(data, i);
                    i += 4;

                    for (face = 0, bit = 1; face < bitfieldSize; face++, bit <<= 1)
                        if ((faceBits & bit) != 0)
                        {
                            CreateFace(face).m_repeatV = tmpFloat;
                            FaceTextures[face].m_attributes |= TextureAttributes.RepeatV;
                        }
                }
                #endregion RepeatV

                #region OffsetU
                DefaultTexture.m_offsetU = Utils.BytesToInt16(data, i);
                i += 2;

                while (ReadFaceBitfield(data, ref i, ref faceBits, ref bitfieldSize))
                {
                    if (bitfieldSize > MAX_FACES)
                        bitfieldSize = MAX_FACES;

                    short tmpshort = Utils.BytesToInt16(data, i);
                    i += 2;

                    for (face = 0, bit = 1; face < bitfieldSize; face++, bit <<= 1)
                        if ((faceBits & bit) != 0)
                        {
                            CreateFace(face).m_offsetU = tmpshort;
                            FaceTextures[face].m_attributes |= TextureAttributes.OffsetU;
                        }
                }
                #endregion OffsetU

                #region OffsetV
                DefaultTexture.m_offsetV = Utils.BytesToInt16(data, i);
                i += 2;

                while (ReadFaceBitfield(data, ref i, ref faceBits, ref bitfieldSize))
                {
                    short tmpshort = Utils.BytesToInt16(data, i);
                    i += 2;

                    for (face = 0, bit = 1; face < bitfieldSize; face++, bit <<= 1)
                        if ((faceBits & bit) != 0)
                        {
                            CreateFace(face).m_offsetV = tmpshort;
                            FaceTextures[face].m_attributes |= TextureAttributes.OffsetV;
                        }
                }
                #endregion OffsetV

                #region Rotation
                DefaultTexture.m_rotation = Utils.BytesToInt16(data, i);
                i += 2;

                while (ReadFaceBitfield(data, ref i, ref faceBits, ref bitfieldSize))
                {
                    if (bitfieldSize > MAX_FACES)
                        bitfieldSize = MAX_FACES;

                    short tmpshort = Utils.BytesToInt16(data, i);
                    i += 2;

                    for (face = 0, bit = 1; face < bitfieldSize; face++, bit <<= 1)
                        if ((faceBits & bit) != 0)
                        {
                            CreateFace(face).m_rotation = tmpshort;
                            FaceTextures[face].m_attributes |= TextureAttributes.Rotation;
                        }
                }
            #endregion Rotation

            #region Material
                DefaultTexture.m_material = data[i];
                i++;

                while (ReadFaceBitfield(data, ref i, ref faceBits, ref bitfieldSize))
                {
                    if (bitfieldSize > MAX_FACES)
                        bitfieldSize = MAX_FACES;

                    byte tmpByte = data[i];
                    i++;

                    for (face = 0, bit = 1; face < bitfieldSize; face++, bit <<= 1)
                        if ((faceBits & bit) != 0)
                        {
                            CreateFace(face).m_material = tmpByte;
                            FaceTextures[face].m_attributes |= TextureAttributes.Material;
                        }
                }
                #endregion Material

                #region Media
                DefaultTexture.m_media = data[i];
                i++;

                while (i - pos < length && ReadFaceBitfield(data, ref i, ref faceBits, ref bitfieldSize))
                {
                    if (bitfieldSize > MAX_FACES)
                        bitfieldSize = MAX_FACES;

                    byte tmpByte = data[i];
                    i++;

                    for (face = 0, bit = 1; face < bitfieldSize; face++, bit <<= 1)
                        if ((faceBits & bit) != 0)
                        {
                            CreateFace(face).m_media = tmpByte;
                            FaceTextures[face].m_attributes |= TextureAttributes.Media;
                        }
                }
                    #endregion Media

                #region Glow
                DefaultTexture.m_glow = data[i++];

                while (ReadFaceBitfield(data, ref i, ref faceBits, ref bitfieldSize))
                {
                    if (bitfieldSize > MAX_FACES)
                        bitfieldSize = MAX_FACES;

                    byte tmpByte = data[i++];

                    for (face = 0, bit = 1; face < bitfieldSize; face++, bit <<= 1)
                        if ((faceBits & bit) != 0)
                        {
                            CreateFace(face).m_glow = tmpByte;
                            FaceTextures[face].m_attributes |= TextureAttributes.Glow;
                        }
                }
                #endregion Glow

                #region MaterialID
                if (i - pos + 16 <= length)
                {
                    DefaultTexture.m_materialID = new UUID(data, i);
                    i += 16;

                    while (i - pos + 16 <= length && ReadFaceBitfield(data, ref i, ref faceBits, ref bitfieldSize))
                    {
                        if (bitfieldSize > MAX_FACES)
                            bitfieldSize = MAX_FACES;

                        UUID tmpUUID = new UUID(data, i);
                        i += 16;

                        for (face = 0, bit = 1; face < bitfieldSize; face++, bit <<= 1)
                            if ((faceBits & bit) != 0)
                            {
                                CreateFace(face).m_materialID = tmpUUID;
                                FaceTextures[face].m_attributes |= TextureAttributes.MaterialID;
                            }
                    }
                }
                #endregion MaterialID
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public byte[] GetBytes(int maxfaces = MAX_FACES)
            {
                if (DefaultTexture == null)
                    return Array.Empty<byte>();

                using (MemoryStream ms = new MemoryStream(4096))
                {
                    ulong done = 0;
                    ulong cur = 0;
                    ulong next = 0;
                    ulong nulls = 0;

                    TextureEntryFace curFace;
                    int last = FaceTextures.Length - 1;
                    if(last > maxfaces - 1)
                        last = maxfaces - 1;

                    bool onLastastNulls = true;

                    #region Texture
                    ms.Write(DefaultTexture.m_textureID.GetBytes(), 0, 16);
                    for (int i = last; i >= 0; --i)
                    {
                        cur = (1UL << i);
                        curFace = FaceTextures[i];
                        if (curFace == null)
                        {
                            nulls |= cur;
                            continue;
                        }

                        if (onLastastNulls)
                        {
                            last = i;
                            onLastastNulls = false;
                        }

                        if ((done & cur) != 0)
                            continue;

                        if ((curFace.m_attributes & TextureAttributes.TextureID) == 0)
                            continue;

                        UUID id = curFace.TextureID;
                        if (id == DefaultTexture.m_textureID)
                            continue;

                        for (int j = i - 1; j >= 0; --j)
                        {
                            next = (1UL << j);
                            if ((done & next) != 0)
                                continue;

                            curFace = FaceTextures[j];
                            if (curFace == null)
                            {
                                nulls |= next;
                                done |= next;
                                continue;
                            }

                            if ((curFace.m_attributes & TextureAttributes.TextureID) == 0)
                                continue;

                            if (curFace.m_textureID != id)
                                continue;

                            done |= next;
                            cur |= next;
                        }
                        WriteFaceBitfieldBytes(ms, cur);
                        ms.Write(id.GetBytes(), 0, 16);
                    }
                    ms.WriteByte(0);
                    #endregion Texture

                    if (onLastastNulls)
                        last = -1;

                    #region Color
                    // Serialize the color bytes inverted to optimize for zerocoding
                    ms.Write(DefaultTexture.m_rgba.GetBytes(true), 0, 4);
                    done = nulls;
                    for (int i = last; i >= 0; --i)
                    {
                        cur = (1UL << i);
                        if ((done & cur) != 0)
                            continue;

                        curFace = FaceTextures[i];
                        if ((curFace.m_attributes & TextureAttributes.RGBA) == 0)
                            continue;

                        Color4 c = curFace.m_rgba;
                        if (c == DefaultTexture.m_rgba)
                            continue;

                        for (int j = i - 1; j >= 0; --j)
                        {
                            next = (1UL << j);
                            if ((done & next) != 0)
                                continue;

                            curFace = FaceTextures[j];
                            if ((curFace.m_attributes & TextureAttributes.RGBA) == 0)
                                continue;

                            if (curFace.m_rgba != c)
                                continue;

                            done |= next;
                            cur |= next;
                        }
                        WriteFaceBitfieldBytes(ms, cur);
                        ms.Write(c.GetBytes(true), 0, 4);
                    }
                    ms.WriteByte(0);
                    #endregion Color

                    #region RepeatU
                    float deff = DefaultTexture.m_repeatU;
                    Utils.FloatToBytes(ms, deff);
                    done = nulls;
                    for (int i = last; i >= 0; --i)
                    {
                        cur = (1UL << i);
                        if ((done & cur) != 0)
                            continue;

                        curFace = FaceTextures[i];
                        if ((curFace.m_attributes & TextureAttributes.RepeatU) == 0)
                            continue;

                        float repeat = curFace.m_repeatU;
                        if (repeat == deff)
                            continue;

                        for (int j = i - 1; j >= 0; --j)
                        {
                            next = (1UL << j);
                            if ((done & next) != 0)
                                continue;

                            curFace = FaceTextures[j];
                            if ((curFace.m_attributes & TextureAttributes.RepeatU) == 0)
                                continue;

                            if (curFace.m_repeatU != repeat)
                                continue;

                            done |= next;
                            cur |= next;
                        }
                        WriteFaceBitfieldBytes(ms, cur);
                        Utils.FloatToBytes(ms, repeat);
                    }
                    ms.WriteByte(0);
                    #endregion RepeatU

                    #region RepeatV
                    deff = DefaultTexture.m_repeatV;
                    Utils.FloatToBytes(ms, deff);
                    done = nulls;
                    for (int i = last; i >= 0; --i)
                    {
                        cur = (1UL << i);
                        if ((done & cur) != 0)
                            continue;

                        curFace = FaceTextures[i];
                        if ((curFace.m_attributes & TextureAttributes.RepeatV) == 0)
                            continue;

                        float repeat = curFace.m_repeatV;
                        if (repeat == deff)
                            continue;

                        for (int j = i - 1; j >= 0; --j)
                        {
                            next = (1UL << j);
                            if ((done & next) != 0)
                                continue;

                            curFace = FaceTextures[j];
                            if ((curFace.m_attributes & TextureAttributes.RepeatV) == 0)
                                continue;

                            if (curFace.m_repeatV != repeat)
                                continue;

                            done |= next;
                            cur |= next;
                        }
                        WriteFaceBitfieldBytes(ms, cur);
                        Utils.FloatToBytes(ms, repeat);
                    }
                    ms.WriteByte(0);

                    #endregion RepeatV

                    #region OffsetU
                    short def = DefaultTexture.m_offsetU;
                    Utils.Int16ToBytes(ms, def);
                    done = nulls;
                    for (int i = last; i >= 0; --i)
                    {
                        cur = (1UL << i);
                        if ((done & cur) != 0)
                            continue;

                        curFace = FaceTextures[i];
                        if ((curFace.m_attributes & TextureAttributes.OffsetU) == 0)
                            continue;

                        short offset = curFace.m_offsetU;
                        if (offset == def)
                            continue;

                        for (int j = i - 1; j >= 0; --j)
                        {
                            next = (1UL << j);
                            if ((done & next) != 0)
                                continue;

                            curFace = FaceTextures[j];
                            if ((curFace.m_attributes & TextureAttributes.OffsetU) == 0)
                                continue;

                            if (curFace.m_offsetU != offset)
                                continue;

                            done |= next;
                            cur |= next;
                        }
                        WriteFaceBitfieldBytes(ms, cur);
                        Utils.Int16ToBytes(ms, offset);
                    }
                    ms.WriteByte(0);
                    #endregion OffsetU

                    #region OffsetV
                    def = DefaultTexture.m_offsetV;
                    Utils.Int16ToBytes(ms, def);
                    done = nulls;
                    for (int i = last; i >= 0; --i)
                    {
                        cur = (1UL << i);
                        if ((done & cur) != 0)
                            continue;

                        curFace = FaceTextures[i];
                        if ((curFace.m_attributes & TextureAttributes.OffsetV) == 0)
                            continue;

                        short offset = curFace.m_offsetV;
                        if (offset == def)
                            continue;

                        for (int j = i - 1; j >= 0; --j)
                        {
                            next = (1UL << j);
                            if ((done & next) != 0)
                                continue;

                            curFace = FaceTextures[j];
                            if ((curFace.m_attributes & TextureAttributes.OffsetV) == 0)
                                continue;

                            if (curFace.m_offsetV != offset)
                                continue;

                            done |= next;
                            cur |= next;
                        }
                        WriteFaceBitfieldBytes(ms, cur);
                        Utils.Int16ToBytes(ms, offset);
                    }
                    ms.WriteByte(0);
                    #endregion OffsetV

                    #region Rotation
                    def = DefaultTexture.m_rotation;
                    Utils.Int16ToBytes(ms, def);
                    done = nulls;
                    for (int i = last; i >= 0; --i)
                    {
                        cur = (1UL << i);
                        if ((done & cur) != 0)
                            continue;

                        curFace = FaceTextures[i];
                        if ((curFace.m_attributes & TextureAttributes.Rotation) == 0)
                            continue;

                        short rotation = curFace.m_rotation;
                        if (rotation == def)
                            continue;

                        for (int j = i - 1; j >= 0; --j)
                        {
                            next = (1UL << j);
                            if ((done & next) != 0)
                                continue;

                            curFace = FaceTextures[j];
                            if ((curFace.m_attributes & TextureAttributes.Rotation) == 0)
                                continue;

                            if (curFace.m_rotation != rotation)
                                continue;

                            done |= next;
                            cur |= next;
                        }
                        WriteFaceBitfieldBytes(ms, cur);
                        Utils.Int16ToBytes(ms, rotation);
                    }
                    ms.WriteByte(0);
                    #endregion Rotation

                    #region Material
                    ms.WriteByte(DefaultTexture.m_material);
                    done = nulls;
                    for (int i = last; i >= 0; --i)
                    {
                        cur = (1UL << i);
                        if ((done & cur) != 0)
                            continue;

                        curFace = FaceTextures[i];
                        if ((curFace.m_attributes & TextureAttributes.Material) == 0)
                            continue;

                        byte material = curFace.m_material;
                        if (material == DefaultTexture.m_material)
                            continue;

                        for (int j = i - 1; j >= 0; --j)
                        {
                            next = (1UL << j);
                            if ((done & next) != 0)
                                continue;

                            curFace = FaceTextures[j];
                            if ((curFace.m_attributes & TextureAttributes.Material) == 0)
                                continue;

                            if (curFace.m_material != material)
                                continue;

                            done |= next;
                            cur |= next;
                        }
                        WriteFaceBitfieldBytes(ms, cur);
                        ms.WriteByte(material);
                    }
                    ms.WriteByte(0);
                    #endregion Material

                    #region Media
                    ms.WriteByte(DefaultTexture.m_media);
                    done = nulls;
                    for (int i = last; i >= 0; --i)
                    {
                        cur = (1UL << i);
                        if ((done & cur) != 0)
                            continue;

                        curFace = FaceTextures[i];
                        if ((curFace.m_attributes & TextureAttributes.Media) == 0)
                            continue;

                        byte media = curFace.m_media;
                        if (media == DefaultTexture.m_media)
                            continue;

                        for (int j = i - 1; j >= 0; --j)
                        {
                            next = (1UL << j);
                            if ((done & next) != 0)
                                continue;

                            curFace = FaceTextures[j];
                            if ((curFace.m_attributes & TextureAttributes.Media) == 0)
                                continue;

                            if (curFace.m_media != media)
                                continue;

                            done |= next;
                            cur |= next;
                        }
                        WriteFaceBitfieldBytes(ms, cur);
                        ms.WriteByte(media);
                    }
                    ms.WriteByte(0);
                    #endregion Media

                    #region Glow
                    byte defg = DefaultTexture.m_glow;
                    ms.WriteByte(defg);
                    done = nulls;
                    for (int i = last; i >= 0; --i)
                    {
                        cur = (1UL << i);
                        if ((done & cur) != 0)
                            continue;

                        curFace = FaceTextures[i];
                        if ((curFace.m_attributes & TextureAttributes.Glow) == 0)
                            continue;

                        byte glow = curFace.m_glow;
                        if (glow == defg)
                            continue;

                        for (int j = i - 1; j >= 0; --j)
                        {
                            next = (1UL << j);
                            if ((done & next) != 0)
                                continue;

                            curFace = FaceTextures[j];
                            if ((curFace.m_attributes & TextureAttributes.Glow) == 0)
                                continue;

                            if (curFace.m_glow != glow)
                                continue;

                            done |= next;
                            cur |= next;
                        }
                        WriteFaceBitfieldBytes(ms, cur);
                        ms.WriteByte(glow);
                    }
                    ms.WriteByte(0);
                    #endregion Glow

                    #region MaterialID
                    ms.Write(DefaultTexture.m_materialID.GetBytes(), 0, 16);
                    done = nulls;
                    for (int i = last; i >= 0; --i)
                    {
                        cur = (1UL << i);
                        if ((done & cur) != 0)
                            continue;

                        curFace = FaceTextures[i];
                        if ((curFace.m_attributes & TextureAttributes.MaterialID) == 0)
                            continue;

                        UUID materialID = curFace.m_materialID;
                        if (materialID == DefaultTexture.m_materialID)
                            continue;

                        for (int j = i - 1; j >= 0; --j)
                        {
                            next = (1UL << j);
                            if ((done & next) != 0)
                                continue;

                            curFace = FaceTextures[j];
                            if ((curFace.m_attributes & TextureAttributes.MaterialID) == 0)
                                continue;

                            if (curFace.m_materialID != materialID)
                                continue;

                            done |= next;
                            cur |= next;
                        }
                        WriteFaceBitfieldBytes(ms, cur);
                        ms.Write(materialID.GetBytes(), 0, 16);
                    }
                    ms.WriteByte(0);
                    #endregion MaterialID

                    return ms.ToArray();
                }
            }


            private static int[] AllBakedIndexes =  { 44, 43, 42, 41, 40, 20, 19, 11, 10, 9, 8 };
            private static int[] LegacyBakedIndexes = {20, 19, 11, 10, 9, 8 };

            public byte[] GetBakesBytes(int maxfaces = MAX_FACES)
            {
                if (DefaultTexture == null)
                    return Array.Empty<byte>();

                if (maxfaces > FaceTextures.Length)
                    maxfaces = FaceTextures.Length;

                int[] bakedIndexes = (maxfaces > 21) ? AllBakedIndexes : LegacyBakedIndexes;

                TextureEntryFace curFace;
                using (MemoryStream ms = new MemoryStream(4096))
                {
                    #region Texture
                    UUID defText = DefaultTexture.TextureID;
                    ms.Write(defText.GetBytes(), 0, 16);
                    foreach (int i in bakedIndexes)
                    {
                        curFace = FaceTextures[i];
                        if (curFace == null)
                            continue;

                        UUID id = curFace.TextureID;
                        if (id == defText)
                            continue;

                        WriteFaceBitfieldBytes(ms, (1UL << i));
                        ms.Write(id.GetBytes(), 0, 16);
                    }
                    ms.WriteByte(0);
                    #endregion Texture

                    #region Color
                    // Serialize the color bytes inverted to optimize for zerocoding
                    ms.Write(DefaultTexture.RGBA.GetBytes(true), 0, 4);
                    foreach (int i in bakedIndexes)
                    {
                        curFace = FaceTextures[i];
                        if (curFace == null)
                            continue;

                        Color4 c = curFace.RGBA;
                        if (c == DefaultTexture.RGBA)
                            continue;

                        WriteFaceBitfieldBytes(ms, (1UL << i));
                        ms.Write(c.GetBytes(true), 0, 4);
                    }
                    ms.WriteByte(0);
                    #endregion Color

                    #region RepeatU
                    float deff = DefaultTexture.RepeatU;
                    Utils.FloatToBytes(ms, deff);
                    foreach (int i in bakedIndexes)
                    {
                        curFace = FaceTextures[i];
                        if (curFace == null)
                            continue;

                        float repeat = curFace.RepeatU;
                        if (repeat == deff)
                            continue;

                        WriteFaceBitfieldBytes(ms, (1UL << i));
                        Utils.FloatToBytes(ms, repeat);
                    }
                    ms.WriteByte(0);
                    #endregion RepeatU

                    #region RepeatV
                    deff = DefaultTexture.RepeatV;
                    Utils.FloatToBytes(ms, deff);
                    foreach (int i in bakedIndexes)
                    {
                        curFace = FaceTextures[i];
                        if (curFace == null)
                            continue;

                        float repeat = curFace.RepeatV;
                        if (repeat == deff)
                            continue;

                        WriteFaceBitfieldBytes(ms, (1UL << i));
                        Utils.FloatToBytes(ms, repeat);
                    }
                    ms.WriteByte(0);

                    #endregion RepeatV

                    #region OffsetU
                    short def = DefaultTexture.m_offsetU;
                    Utils.Int16ToBytes(ms, def);
                    foreach (int i in bakedIndexes)
                    {
                        curFace = FaceTextures[i];
                        if (curFace == null)
                            continue;

                        short offset = curFace.m_offsetU;
                        if (offset == def)
                            continue;

                        WriteFaceBitfieldBytes(ms, (1UL << i));
                        Utils.Int16ToBytes(ms, offset);
                    }
                    ms.WriteByte(0);
                    #endregion OffsetU

                    #region OffsetV
                    def = DefaultTexture.m_offsetV;
                    Utils.Int16ToBytes(ms, def);
                    foreach (int i in bakedIndexes)
                    {
                        curFace = FaceTextures[i];
                        if (curFace == null)
                            continue;

                        short offset = curFace.m_offsetV;
                        if (offset == def)
                            continue;

                        WriteFaceBitfieldBytes(ms, (1UL << i));
                        Utils.Int16ToBytes(ms, offset);
                    }
                    ms.WriteByte(0);
                    #endregion OffsetV

                    #region Rotation
                    def = DefaultTexture.m_rotation;
                    Utils.Int16ToBytes(ms, def);
                    foreach (int i in bakedIndexes)
                    {
                        curFace = FaceTextures[i];
                        if (curFace == null)
                            continue;

                        short rotation = curFace.m_rotation;
                        if (rotation == def)
                            continue;

                        WriteFaceBitfieldBytes(ms, (1UL << i));
                        Utils.Int16ToBytes(ms, rotation);
                    }
                    ms.WriteByte(0);
                    #endregion Rotation

                    #region Material
                    ms.WriteByte(DefaultTexture.m_material);
                    foreach (int i in bakedIndexes)
                    {
                        curFace = FaceTextures[i];
                        if (curFace == null)
                            continue;

                        byte material = curFace.m_material;
                        if (material == DefaultTexture.m_material)
                            continue;

                        WriteFaceBitfieldBytes(ms, (1UL << i));
                        ms.WriteByte(material);
                    }
                    ms.WriteByte(0);
                    #endregion Material

                    #region Media
                    ms.WriteByte(DefaultTexture.m_media);
                    foreach (int i in bakedIndexes)
                    {
                        curFace = FaceTextures[i];
                        if (curFace == null)
                            continue;

                        byte media = curFace.m_media;
                        if (media == DefaultTexture.m_media)
                            continue;

                        WriteFaceBitfieldBytes(ms, (1UL << i));
                        ms.WriteByte(media);
                    }
                    ms.WriteByte(0);
                    #endregion Media

                    #region Glow
                    byte defg = DefaultTexture.m_glow;
                    ms.WriteByte(defg);
                    foreach (int i in bakedIndexes)
                    {
                        curFace = FaceTextures[i];
                        if (curFace == null)
                            continue;

                        byte glow = curFace.m_glow;
                        if (glow == defg)
                            continue;

                        WriteFaceBitfieldBytes(ms, (1UL << i));
                        ms.WriteByte(glow);
                    }
                    ms.WriteByte(0);
                    #endregion Glow

                    #region MaterialID
                    ms.Write(DefaultTexture.m_materialID.GetBytes(), 0, 16);
                    foreach (int i in bakedIndexes)
                    {
                        curFace = FaceTextures[i];
                        if (curFace == null)
                            continue;

                        UUID materialID = curFace.m_materialID;
                        if (materialID == DefaultTexture.m_materialID)
                            continue;

                        WriteFaceBitfieldBytes(ms, (1UL << i));
                        ms.Write(materialID.GetBytes(), 0, 16);
                    }
                    ms.WriteByte(0);
                    #endregion MaterialID

                    return ms.ToArray();
                }
            }

            public override int GetHashCode()
            {
                int hashCode = DefaultTexture != null ? DefaultTexture.GetHashCode() : 0;
                for (int i = 0; i < FaceTextures.Length; i++)
                {
                    if (FaceTextures[i] != null)
                        hashCode ^= FaceTextures[i].GetHashCode();
                }
                return hashCode;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                string output = String.Empty;

                output += "Default Face: " + DefaultTexture.ToString() + Environment.NewLine;

                for (int i = 0; i < FaceTextures.Length; i++)
                {
                    if (FaceTextures[i] != null)
                        output += "Face " + i + ": " + FaceTextures[i].ToString() + Environment.NewLine;
                }

                return output;
            }

            #region Helpers

            private bool ReadFaceBitfield(byte[] data, ref int pos, ref ulong faceBits, ref uint bitfieldSize)
            {
                if (pos >= data.Length)
                    return false;

                byte b = data[pos++];
                if(b == 0)
                    return false;

                faceBits = (uint)(b & 0x7F);
                bitfieldSize = 7;
                if((b & 0x80) == 0)
                    return true;

                do
                {
                    b = data[pos++];
                    faceBits = (faceBits << 7) | (uint)(b & 0x7F);
                    bitfieldSize += 7;
                }
                while ((b & 0x80) != 0);

                return true;
            }

            private void WriteFaceBitfieldBytes(MemoryStream ms, ulong bitfield)
            {
                if (bitfield == 0)
                    return;

                byte b;

                if (bitfield >= 0x80)
                {
                    if (bitfield >= 0x4000)
                    {
                        if (bitfield >= 0x200000)
                        {
                            if (bitfield >= 0x10000000)
                            {
                                if (bitfield >= 0x800000000)
                                {
                                    if (bitfield >= (1UL << 42))
                                    {
                                        if (bitfield >= (1UL << 49))
                                        {
                                            b = (byte)(bitfield >> 49);
                                            b |= 0x80;
                                            ms.WriteByte(b);
                                        }
                                        b = (byte)(bitfield >> 42);
                                        b |= 0x80;
                                        ms.WriteByte(b);
                                    }

                                    b = (byte)(bitfield >> 35);
                                    b |= 0x80;
                                    ms.WriteByte(b);
                                }
                                b = (byte)(bitfield >> 28);
                                b |= 0x80;
                                ms.WriteByte(b);
                            }
                            b = (byte)(bitfield >> 21);
                            b |= 0x80;
                            ms.WriteByte(b);
                        }
                        b = (byte)(bitfield >> 14);
                        b |= 0x80;
                        ms.WriteByte(b);
                    }
                    b = (byte)(bitfield >> 7);
                    b |= 0x80;
                    ms.WriteByte(b);
                }
                b = (byte)(bitfield & 0x7F);
                ms.WriteByte(b);
            }
        }
        #endregion Helpers

        /// <summary>
        /// Controls the texture animation of a particular prim
        /// </summary>
        public struct TextureAnimation
        {
            /// <summary></summary>
            public TextureAnimMode Flags;
            /// <summary></summary>
            public uint Face;
            /// <summary></summary>
            public uint SizeX;
            /// <summary></summary>
            public uint SizeY;
            /// <summary></summary>
            public float Start;
            /// <summary></summary>
            public float Length;
            /// <summary></summary>
            public float Rate;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="data"></param>
            /// <param name="pos"></param>
            public TextureAnimation(byte[] data, int pos)
            {
                if (data.Length >= 16)
                {
                    Flags = (TextureAnimMode)(data[pos++]);
                    Face = data[pos++];
                    SizeX = data[pos++];
                    SizeY = data[pos++];

                    Start = Utils.BytesToFloatSafepos(data, pos);
                    Length = Utils.BytesToFloatSafepos(data, pos + 4);
                    Rate = Utils.BytesToFloatSafepos(data, pos + 8);
                }
                else
                {
                    Flags = 0;
                    Face = 0;
                    SizeX = 0;
                    SizeY = 0;

                    Start = 0.0f;
                    Length = 0.0f;
                    Rate = 0.0f;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public byte[] GetBytes()
            {
                byte[] data = new byte[16];

                data[0] = (byte)Flags;
                data[1] = (byte)Face;
                data[2] = (byte)SizeX;
                data[3] = (byte)SizeY;

                Utils.FloatToBytesSafepos(Start, data, 4);
                Utils.FloatToBytesSafepos(Length, data, 8);
                Utils.FloatToBytesSafepos(Rate, data, 12);

                return data;
            }

            public OSD GetOSD()
            {
                OSDMap map = new OSDMap();

                map["face"] = OSD.FromInteger(Face);
                map["flags"] = OSD.FromInteger((uint)Flags);
                map["length"] = OSD.FromReal(Length);
                map["rate"] = OSD.FromReal(Rate);
                map["size_x"] = OSD.FromInteger(SizeX);
                map["size_y"] = OSD.FromInteger(SizeY);
                map["start"] = OSD.FromReal(Start);

                return map;
            }

            public static TextureAnimation FromOSD(OSD osd)
            {
                TextureAnimation anim = new TextureAnimation();
                OSDMap map = osd as OSDMap;

                if (map != null)
                {
                    anim.Face = map["face"].AsUInteger();
                    anim.Flags = (TextureAnimMode)map["flags"].AsUInteger();
                    anim.Length = (float)map["length"].AsReal();
                    anim.Rate = (float)map["rate"].AsReal();
                    anim.SizeX = map["size_x"].AsUInteger();
                    anim.SizeY = map["size_y"].AsUInteger();
                    anim.Start = (float)map["start"].AsReal();
                }

                return anim;
            }
        }

        #endregion Subclasses

        #region Public Members

        /// <summary></summary>
        public TextureEntry Textures;
        /// <summary></summary>
        public TextureAnimation TextureAnim;

        #endregion Public Members
    }
}
