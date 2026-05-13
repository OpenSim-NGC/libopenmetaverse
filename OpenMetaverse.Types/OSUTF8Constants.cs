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
        public static readonly byte[] XMLundef = "<undef/>"u8.ToArray();
        public static readonly byte[] XMLfullbooleanOne = "<boolean>1</boolean>"u8.ToArray();
        public static readonly byte[] XMLfullbooleanZero = "<boolean>0</boolean>"u8.ToArray();
        public static readonly byte[] XMLintegerStart = "<integer>"u8.ToArray();
        public static readonly byte[] XMLintegerEmpty = "<integer />"u8.ToArray();
        public static readonly byte[] XMLintegerEnd = "</integer>"u8.ToArray();
        public static readonly byte[] XMLrealStart = "<real>"u8.ToArray();
        public static readonly byte[] XMLrealZero = "<real>0</real>"u8.ToArray();
        public static readonly byte[] XMLrealZeroarrayEnd = "<real>0</real></array>"u8.ToArray();
        public static readonly byte[] XMLrealEnd = "</real>"u8.ToArray();
        public static readonly byte[] XMLrealEndarrayEnd = "</real></array>"u8.ToArray();
        public static readonly byte[] XMLstringStart = "<string>"u8.ToArray();
        public static readonly byte[] XMLstringEmpty = "<string />"u8.ToArray();
        public static readonly byte[] XMLstringEnd = "</string>"u8.ToArray();
        public static readonly byte[] XMLuuidStart = "<uuid>"u8.ToArray();
        public static readonly byte[] XMLuuidEmpty = "<uuid />"u8.ToArray();
        public static readonly byte[] XMLuuidEnd = "</uuid>"u8.ToArray();
        public static readonly byte[] XMLdateStart = "<date>"u8.ToArray();
        public static readonly byte[] XMLdateEmpty = "<date />"u8.ToArray();
        public static readonly byte[] XMLdateEnd = "</date>"u8.ToArray();
        public static readonly byte[] XMLuriStart = "<uri>"u8.ToArray();
        public static readonly byte[] XMLuriEmpty = "<uri />"u8.ToArray();
        public static readonly byte[] XMLuriEnd = "</uri>"u8.ToArray();
        public static readonly byte[] XMLformalBinaryStart = "<binary encoding=\"base64\">"u8.ToArray();
        public static readonly byte[] XMLbinaryStart = "<binary>"u8.ToArray();
        public static readonly byte[] XMLbinaryEmpty = "<binary />"u8.ToArray();
        public static readonly byte[] XMLbinaryEnd = "</binary>"u8.ToArray();
        public static readonly byte[] XMLmapStart = "<map>"u8.ToArray();
        public static readonly byte[] XMLmapEmpty = "<map />"u8.ToArray();
        public static readonly byte[] XMLmapEnd = "</map>"u8.ToArray();
        public static readonly byte[] XMLkeyStart = "<key>"u8.ToArray();
        public static readonly byte[] XMLkeyEmpty = "<key />"u8.ToArray();
        public static readonly byte[] XMLkeyEnd = "</key>"u8.ToArray();
        public static readonly byte[] XMLkeyEndundef = "</key><undef />"u8.ToArray();
        public static readonly byte[] XMLkeyEndmapStart = "</key><map>"u8.ToArray();
        public static readonly byte[] XMLkeyEndmapEmpty = "</key><map />"u8.ToArray();
        public static readonly byte[] XMLkeyEndarrayStart = "</key><array>"u8.ToArray();
        public static readonly byte[] XMLkeyEndarrayEmpty = "</key><array />"u8.ToArray();
        public static readonly byte[] XMLkeyEndarrayStartmapStart = "</key><array><map>"u8.ToArray();
        public static readonly byte[] XMLarrayStart = "<array>"u8.ToArray();
        public static readonly byte[] XMLarrayStartrealZero = "<array><real>0</real>"u8.ToArray();
        public static readonly byte[] XMLarrayStartrealStart = "<array><real>"u8.ToArray();
        public static readonly byte[] XMLkeyEndarrayStartrealZero = "</key><array><real>0</real>"u8.ToArray();
        public static readonly byte[] XMLkeyEndarrayStartrealStart = "</key><array><real>"u8.ToArray();
        public static readonly byte[] XMLarrayEmpty = "<array />"u8.ToArray();
        public static readonly byte[] XMLarrayEnd = "</array>"u8.ToArray();
        public static readonly byte[] XMLamp_lt = "&lt;"u8.ToArray();
        public static readonly byte[] XMLamp_gt = "&gt;"u8.ToArray();
        public static readonly byte[] XMLamp = "&amp;"u8.ToArray();
        public static readonly byte[] XMLamp_quot = "&quot;"u8.ToArray();
        public static readonly byte[] XMLamp_apos = "&apos;"u8.ToArray();
        public static readonly byte[] XMLformalHeader = "<?xml version=\"1.0\" encoding=\"utf-8\"?>"u8.ToArray();
        public static readonly byte[] XMLformalHeaderllsdstart = "<?xml version=\"1.0\" encoding=\"utf-8\"?><llsd>"u8.ToArray();
        public static readonly byte[] XMLllsdStart = "<llsd>"u8.ToArray();
        public static readonly byte[] XMLllsdEnd = "</llsd>"u8.ToArray();
        public static readonly byte[] XMLllsdEmpty = "<llsd><map /></llsd>"u8.ToArray();
        public static readonly byte[] XMLmapEndarrayEnd = "</map></array>"u8.ToArray();

        public static readonly byte[] XMLarrayEndmapEnd = "</array></map>"u8.ToArray();

        public static readonly byte[] XMLelement_name_Empty = "<key>name</key><string />"u8.ToArray();
        public static readonly byte[] XMLelement_name_Start = "<key>name</key><string>"u8.ToArray();

        public static readonly byte[] XMLelement_agent_id_Empty = "<key>agent_id</key><uuid />"u8.ToArray();
        public static readonly byte[] XMLelement_agent_id_Start = "<key>agent_id</key><uuid>"u8.ToArray();

        public static readonly byte[] XMLelement_owner_id_Empty = "<key>owner_id</key><uuid />"u8.ToArray();
        public static readonly byte[] XMLelement_owner_id_Start = "<key>owner_id</key><uuid>"u8.ToArray();

        public static readonly byte[] XMLelement_creator_id_Empty = "<key>creator_id</key><uuid />"u8.ToArray();
        public static readonly byte[] XMLelement_creator_id_Start = "<key>creator_id</key><uuid>"u8.ToArray();

        public static readonly byte[] XMLelement_group_id_Empty = "<key>group_id</key><uuid />"u8.ToArray();
        public static readonly byte[] XMLelement_group_id_Start = "<key>cgroup_id</key><uuid>"u8.ToArray();

        public static readonly byte[] XMLelement_parent_id_Empty = "<key>parent_id</key><uuid />"u8.ToArray();
        public static readonly byte[] XMLelement_parent_id_Start = "<key>parent_id</key><uuid>"u8.ToArray();

        public static readonly byte[] XMLelement_folder_id_Empty = "<key>folder_id</key><uuid />"u8.ToArray();
        public static readonly byte[] XMLelement_folder_id_Start = "<key>folder_id</key><uuid>"u8.ToArray();

        public static readonly byte[] XMLelement_asset_id_Empty = "<key>asset_id</key><uuid />"u8.ToArray();
        public static readonly byte[] XMLelement_asset_id_Start = "<key>asset_id</key><uuid>"u8.ToArray();

        public static readonly byte[] XMLelement_item_id_Empty = "<key>item_id</key><uuid />"u8.ToArray();
        public static readonly byte[] XMLelement_item_id_Start = "<key>item_id</key><uuid>"u8.ToArray();

        public static readonly byte[] XMLelement_category_id_Empty = "<key>category_id</key><uuid />"u8.ToArray();
        public static readonly byte[] XMLelement_category_id_Start = "<key>category_id</key><uuid>"u8.ToArray();

        public static readonly byte[] XMLelement_version_Empty = "<key>version</key><integer />"u8.ToArray();
        public static readonly byte[] XMLelement_version_Start = "<key>version</key><integer>"u8.ToArray();

        public static readonly byte[] XMLelement_sale_info_Empty = "<key>sale_info</key><map><key>sale_price</key><integer /><key>sale_type</key><integer /></map>"u8.ToArray();
        public static readonly byte[] XMLelement_sale_info_Start = "<key>sale_info</key><map><key>sale_price</key><integer>"u8.ToArray();
        public static readonly byte[] XMLelement_sale_info_Mid = "</integer><key>sale_type</key><integer>"u8.ToArray();
        public static readonly byte[] XMLelement_sale_info_End = "</integer></map>"u8.ToArray();

        public static readonly byte[] OSUTF8null = "null"u8.ToArray();
        public static readonly byte[] OSUTF8true = "true"u8.ToArray();
        public static readonly byte[] OSUTF8false = "false"u8.ToArray();

        public static readonly byte[] base64Bytes = {(byte)'A',(byte)'B',(byte)'C',(byte)'D',(byte)'E',(byte)'F',(byte)'G',(byte)'H',(byte)'I',(byte)'J',(byte)'K',(byte)'L',(byte)'M',(byte)'N',(byte)'O',
                                              (byte)'P',(byte)'Q',(byte)'R',(byte)'S',(byte)'T',(byte)'U',(byte)'V',(byte)'W',(byte)'X',(byte)'Y',(byte)'Z',(byte)'a',(byte)'b',(byte)'c',(byte)'d',
                                              (byte)'e',(byte)'f',(byte)'g',(byte)'h',(byte)'i',(byte)'j',(byte)'k',(byte)'l',(byte)'m',(byte)'n',(byte)'o',(byte)'p',(byte)'q',(byte)'r',(byte)'s',
                                              (byte)'t',(byte)'u',(byte)'v',(byte)'w',(byte)'x',(byte)'y',(byte)'z',(byte)'0',(byte)'1',(byte)'2',(byte)'3',(byte)'4',(byte)'5',(byte)'6',(byte)'7',
                                              (byte)'8',(byte)'9',(byte)'+',(byte)'/'};

    }
}
