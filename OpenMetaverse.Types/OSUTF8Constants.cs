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

namespace OpenMetaverse
{
    public static class osUTF8Const
    {
        // direct utf8 string byte arrays to use with osUTF8 and osUTF8Slice
        // waste time calling osUTF8.GetASCIIBytes just because it is more readable then {(byte)char,... } form
        public static readonly byte[] XMLundef = osUTF8.GetASCIIBytes("<undef/>");
        public static readonly byte[] XMLfullbooleanOne = osUTF8.GetASCIIBytes("<boolean>1</boolean>");
        public static readonly byte[] XMLfullbooleanZero = osUTF8.GetASCIIBytes("<boolean>0</boolean>");
        public static readonly byte[] XMLintegerStart = osUTF8.GetASCIIBytes("<integer>");
        public static readonly byte[] XMLintegerEmpty = osUTF8.GetASCIIBytes("<integer />");
        public static readonly byte[] XMLintegerEnd = osUTF8.GetASCIIBytes("</integer>");
        public static readonly byte[] XMLrealStart = osUTF8.GetASCIIBytes("<real>");
        public static readonly byte[] XMLrealZero = osUTF8.GetASCIIBytes("<real>0</real>");
        public static readonly byte[] XMLrealZeroarrayEnd = osUTF8.GetASCIIBytes("<real>0</real></array>");
        public static readonly byte[] XMLrealEnd = osUTF8.GetASCIIBytes("</real>");
        public static readonly byte[] XMLrealEndarrayEnd = osUTF8.GetASCIIBytes("</real></array>");
        public static readonly byte[] XMLstringStart = osUTF8.GetASCIIBytes("<string>");
        public static readonly byte[] XMLstringEmpty = osUTF8.GetASCIIBytes("<string />");
        public static readonly byte[] XMLstringEnd = osUTF8.GetASCIIBytes("</string>");
        public static readonly byte[] XMLuuidStart = osUTF8.GetASCIIBytes("<uuid>");
        public static readonly byte[] XMLuuidEmpty = osUTF8.GetASCIIBytes("<uuid />");
        public static readonly byte[] XMLuuidEnd = osUTF8.GetASCIIBytes("</uuid>");
        public static readonly byte[] XMLdateStart = osUTF8.GetASCIIBytes("<date>");
        public static readonly byte[] XMLdateEmpty = osUTF8.GetASCIIBytes("<date />");
        public static readonly byte[] XMLdateEnd = osUTF8.GetASCIIBytes("</date>");
        public static readonly byte[] XMLuriStart = osUTF8.GetASCIIBytes("<uri>");
        public static readonly byte[] XMLuriEmpty = osUTF8.GetASCIIBytes("<uri />");
        public static readonly byte[] XMLuriEnd = osUTF8.GetASCIIBytes("</uri>");
        public static readonly byte[] XMLformalBinaryStart = osUTF8.GetASCIIBytes("<binary encoding=\"base64\">");
        public static readonly byte[] XMLbinaryStart = osUTF8.GetASCIIBytes("<binary>");
        public static readonly byte[] XMLbinaryEmpty = osUTF8.GetASCIIBytes("<binary />");
        public static readonly byte[] XMLbinaryEnd = osUTF8.GetASCIIBytes("</binary>");
        public static readonly byte[] XMLmapStart = osUTF8.GetASCIIBytes("<map>");
        public static readonly byte[] XMLmapEmpty = osUTF8.GetASCIIBytes("<map />");
        public static readonly byte[] XMLmapEnd = osUTF8.GetASCIIBytes("</map>");
        public static readonly byte[] XMLkeyStart = osUTF8.GetASCIIBytes("<key>");
        public static readonly byte[] XMLkeyEmpty = osUTF8.GetASCIIBytes("<key />");
        public static readonly byte[] XMLkeyEnd = osUTF8.GetASCIIBytes("</key>");
        public static readonly byte[] XMLkeyEndundef = osUTF8.GetASCIIBytes("</key><undef />");
        public static readonly byte[] XMLkeyEndmapStart = osUTF8.GetASCIIBytes("</key><map>");
        public static readonly byte[] XMLkeyEndmapEmpty = osUTF8.GetASCIIBytes("</key><map />");
        public static readonly byte[] XMLkeyEndarrayStart = osUTF8.GetASCIIBytes("</key><array>");
        public static readonly byte[] XMLkeyEndarrayEmpty = osUTF8.GetASCIIBytes("</key><array />");
        public static readonly byte[] XMLkeyEndarrayStartmapStart = osUTF8.GetASCIIBytes("</key><array><map>");
        public static readonly byte[] XMLarrayStart = osUTF8.GetASCIIBytes("<array>");
        public static readonly byte[] XMLarrayStartrealZero = osUTF8.GetASCIIBytes("<array><real>0</real>");
        public static readonly byte[] XMLarrayStartrealStart = osUTF8.GetASCIIBytes("<array><real>");
        public static readonly byte[] XMLkeyEndarrayStartrealZero = osUTF8.GetASCIIBytes("</key><array><real>0</real>");
        public static readonly byte[] XMLkeyEndarrayStartrealStart = osUTF8.GetASCIIBytes("</key><array><real>");
        public static readonly byte[] XMLarrayEmpty = osUTF8.GetASCIIBytes("<array />");
        public static readonly byte[] XMLarrayEnd = osUTF8.GetASCIIBytes("</array>");
        public static readonly byte[] XMLamp_lt = osUTF8.GetASCIIBytes("&lt;");
        public static readonly byte[] XMLamp_gt = osUTF8.GetASCIIBytes("&gt;");
        public static readonly byte[] XMLamp = osUTF8.GetASCIIBytes("&amp;");
        public static readonly byte[] XMLamp_quot = osUTF8.GetASCIIBytes("&quot;");
        public static readonly byte[] XMLamp_apos = osUTF8.GetASCIIBytes("&apos;");
        public static readonly byte[] XMLformalHeader = osUTF8.GetASCIIBytes("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        public static readonly byte[] XMLformalHeaderllsdstart = osUTF8.GetASCIIBytes("<?xml version=\"1.0\" encoding=\"utf-8\"?><llsd>");
        public static readonly byte[] XMLllsdStart = osUTF8.GetASCIIBytes("<llsd>");
        public static readonly byte[] XMLllsdEnd = osUTF8.GetASCIIBytes("</llsd>");
        public static readonly byte[] XMLllsdEmpty = osUTF8.GetASCIIBytes("<llsd><map /></llsd>");
        public static readonly byte[] XMLmapEndarrayEnd = osUTF8.GetASCIIBytes("</map></array>");

        public static readonly byte[] XMLarrayEndmapEnd = osUTF8.GetASCIIBytes("</array></map>");

        public static readonly byte[] XMLelement_name_Empty = osUTF8.GetASCIIBytes("<key>name</key><string />");
        public static readonly byte[] XMLelement_name_Start = osUTF8.GetASCIIBytes("<key>name</key><string>");

        public static readonly byte[] XMLelement_agent_id_Empty = osUTF8.GetASCIIBytes("<key>agent_id</key><uuid />");
        public static readonly byte[] XMLelement_agent_id_Start = osUTF8.GetASCIIBytes("<key>agent_id</key><uuid>");

        public static readonly byte[] XMLelement_owner_id_Empty = osUTF8.GetASCIIBytes("<key>owner_id</key><uuid />");
        public static readonly byte[] XMLelement_owner_id_Start = osUTF8.GetASCIIBytes("<key>owner_id</key><uuid>");

        public static readonly byte[] XMLelement_creator_id_Empty = osUTF8.GetASCIIBytes("<key>creator_id</key><uuid />");
        public static readonly byte[] XMLelement_creator_id_Start = osUTF8.GetASCIIBytes("<key>creator_id</key><uuid>");

        public static readonly byte[] XMLelement_group_id_Empty = osUTF8.GetASCIIBytes("<key>group_id</key><uuid />");
        public static readonly byte[] XMLelement_group_id_Start = osUTF8.GetASCIIBytes("<key>cgroup_id</key><uuid>");

        public static readonly byte[] XMLelement_parent_id_Empty = osUTF8.GetASCIIBytes("<key>parent_id</key><uuid />");
        public static readonly byte[] XMLelement_parent_id_Start = osUTF8.GetASCIIBytes("<key>parent_id</key><uuid>");

        public static readonly byte[] XMLelement_folder_id_Empty = osUTF8.GetASCIIBytes("<key>folder_id</key><uuid />");
        public static readonly byte[] XMLelement_folder_id_Start = osUTF8.GetASCIIBytes("<key>folder_id</key><uuid>");

        public static readonly byte[] XMLelement_asset_id_Empty = osUTF8.GetASCIIBytes("<key>asset_id</key><uuid />");
        public static readonly byte[] XMLelement_asset_id_Start = osUTF8.GetASCIIBytes("<key>asset_id</key><uuid>");

        public static readonly byte[] XMLelement_item_id_Empty = osUTF8.GetASCIIBytes("<key>item_id</key><uuid />");
        public static readonly byte[] XMLelement_item_id_Start = osUTF8.GetASCIIBytes("<key>item_id</key><uuid>");

        public static readonly byte[] XMLelement_category_id_Empty = osUTF8.GetASCIIBytes("<key>category_id</key><uuid />");
        public static readonly byte[] XMLelement_category_id_Start = osUTF8.GetASCIIBytes("<key>category_id</key><uuid>");

        public static readonly byte[] XMLelement_version_Empty = osUTF8.GetASCIIBytes("<key>version</key><integer />");
        public static readonly byte[] XMLelement_version_Start = osUTF8.GetASCIIBytes("<key>version</key><integer>");

        public static readonly byte[] XMLelement_sale_info_Empty = osUTF8.GetASCIIBytes("<key>sale_info</key><map><key>sale_price</key><integer /><key>sale_type</key><integer /></map>");
        public static readonly byte[] XMLelement_sale_info_Start = osUTF8.GetASCIIBytes("<key>sale_info</key><map><key>sale_price</key><integer>");
        public static readonly byte[] XMLelement_sale_info_Mid = osUTF8.GetASCIIBytes("</integer><key>sale_type</key><integer>");
        public static readonly byte[] XMLelement_sale_info_End = osUTF8.GetASCIIBytes("</integer></map>");

        public static readonly byte[] OSUTF8null = osUTF8.GetASCIIBytes("null");
        public static readonly byte[] OSUTF8true = osUTF8.GetASCIIBytes("true");
        public static readonly byte[] OSUTF8false = osUTF8.GetASCIIBytes("false");

        public static readonly byte[] base64Bytes = {(byte)'A',(byte)'B',(byte)'C',(byte)'D',(byte)'E',(byte)'F',(byte)'G',(byte)'H',(byte)'I',(byte)'J',(byte)'K',(byte)'L',(byte)'M',(byte)'N',(byte)'O',
                                              (byte)'P',(byte)'Q',(byte)'R',(byte)'S',(byte)'T',(byte)'U',(byte)'V',(byte)'W',(byte)'X',(byte)'Y',(byte)'Z',(byte)'a',(byte)'b',(byte)'c',(byte)'d',
                                              (byte)'e',(byte)'f',(byte)'g',(byte)'h',(byte)'i',(byte)'j',(byte)'k',(byte)'l',(byte)'m',(byte)'n',(byte)'o',(byte)'p',(byte)'q',(byte)'r',(byte)'s',
                                              (byte)'t',(byte)'u',(byte)'v',(byte)'w',(byte)'x',(byte)'y',(byte)'z',(byte)'0',(byte)'1',(byte)'2',(byte)'3',(byte)'4',(byte)'5',(byte)'6',(byte)'7',
                                              (byte)'8',(byte)'9',(byte)'+',(byte)'/'};

    }
}
