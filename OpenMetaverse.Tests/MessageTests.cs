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

using Xunit;
using MessagePack;
using MessagePack.Resolvers;
using OpenMetaverse.Messages.Linden;
using OpenMetaverse.StructuredData;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Xml.Serialization;

namespace OpenMetaverse.Tests
{
    /// <summary>
    /// These unit tests specifically test the Message class can serialize and deserialize its own data properly
    /// a passed test does not necessarily indicate the formatting is correct in the resulting OSD to be handled
    /// by the simulator.
    /// </summary>
    
    public class MessageTests
    {
        private static readonly MessagePackSerializerOptions BenchmarkMessagePackOptions =
            MessagePackSerializerOptions.Standard.WithResolver(TypelessContractlessStandardResolver.Instance);

        private Uri testURI = new Uri("https://sim3187.agni.lindenlab.com:12043/cap/6028fc44-c1e5-80a1-f902-19bde114458b");
        private IPAddress testIP = IPAddress.Parse("127.0.0.1");
        private ulong testHandle = 1106108697797888;

        [Fact]
        public void AgentGroupDataUpdateMessage()
        {
            AgentGroupDataUpdateMessage s = new AgentGroupDataUpdateMessage();
            s.AgentID = UUID.Random();



            AgentGroupDataUpdateMessage.GroupData[] blocks = new AgentGroupDataUpdateMessage.GroupData[2];
            AgentGroupDataUpdateMessage.GroupData g1 = new AgentGroupDataUpdateMessage.GroupData();

            g1.AcceptNotices = false;
            g1.Contribution = 1024;
            g1.GroupID = UUID.Random();
            g1.GroupInsigniaID = UUID.Random();
            g1.GroupName = "Group Name Test 1";
            g1.GroupPowers = GroupPowers.Accountable | GroupPowers.AllowLandmark | GroupPowers.AllowSetHome;
            blocks[0] = g1;

            AgentGroupDataUpdateMessage.GroupData g2 = new AgentGroupDataUpdateMessage.GroupData();
            g2.AcceptNotices = false;
            g2.Contribution = 16;
            g2.GroupID = UUID.Random();
            g2.GroupInsigniaID = UUID.Random();
            g2.GroupName = "Group Name Test 2";
            g2.GroupPowers = GroupPowers.ChangeActions | GroupPowers.DeedObject;
            blocks[1] = g2;

            s.GroupDataBlock = blocks;

            AgentGroupDataUpdateMessage.NewGroupData[] nblocks = new AgentGroupDataUpdateMessage.NewGroupData[2];

            AgentGroupDataUpdateMessage.NewGroupData ng1 = new AgentGroupDataUpdateMessage.NewGroupData();
            ng1.ListInProfile = false;
            nblocks[0] = ng1;

            AgentGroupDataUpdateMessage.NewGroupData ng2 = new AgentGroupDataUpdateMessage.NewGroupData();
            ng2.ListInProfile = true;
            nblocks[1] = ng2;

            s.NewGroupDataBlock = nblocks;

            OSDMap map = s.Serialize();

            AgentGroupDataUpdateMessage t = new AgentGroupDataUpdateMessage();
            t.Deserialize(map);

            Assert.Equal(s.AgentID, t.AgentID);

            for (int i = 0; i < t.GroupDataBlock.Length; i++)
            {
                Assert.Equal(s.GroupDataBlock[i].AcceptNotices, t.GroupDataBlock[i].AcceptNotices);
                Assert.Equal(s.GroupDataBlock[i].Contribution, t.GroupDataBlock[i].Contribution);
                Assert.Equal(s.GroupDataBlock[i].GroupID, t.GroupDataBlock[i].GroupID);
                Assert.Equal(s.GroupDataBlock[i].GroupInsigniaID, t.GroupDataBlock[i].GroupInsigniaID);
                Assert.Equal(s.GroupDataBlock[i].GroupName, t.GroupDataBlock[i].GroupName);
                Assert.Equal(s.GroupDataBlock[i].GroupPowers, t.GroupDataBlock[i].GroupPowers);
            }

            for (int i = 0; i < t.NewGroupDataBlock.Length; i++)
            {
                Assert.Equal(s.NewGroupDataBlock[i].ListInProfile, t.NewGroupDataBlock[i].ListInProfile);
            }
        }

        [Fact]
        public void TeleportFinishMessage()
        {
            TeleportFinishMessage s = new TeleportFinishMessage();
            s.AgentID = UUID.Random();
            s.Flags = TeleportFlags.ViaLocation | TeleportFlags.IsFlying;
            s.IP = testIP;
            s.LocationID = 32767;
            s.Port = 3000;
            s.RegionHandle = testHandle;
            s.SeedCapability = testURI;
            s.SimAccess = SimAccess.Mature;

            OSDMap map = s.Serialize();

            TeleportFinishMessage t = new TeleportFinishMessage();
            t.Deserialize(map);

            Assert.Equal(s.AgentID, t.AgentID);
            Assert.Equal(s.Flags, t.Flags);
            Assert.Equal(s.IP, t.IP);
            Assert.Equal(s.LocationID, t.LocationID);
            Assert.Equal(s.Port, t.Port);
            Assert.Equal(s.RegionHandle, t.RegionHandle);
            Assert.Equal(s.SeedCapability, t.SeedCapability);
            Assert.Equal(s.SimAccess, t.SimAccess);
        }

        [Fact]
        public void EstablishAgentCommunicationMessage()
        {
            EstablishAgentCommunicationMessage s = new EstablishAgentCommunicationMessage();
            s.Address = testIP;
            s.AgentID = UUID.Random();
            s.Port = 3000;
            s.SeedCapability = testURI;

            OSDMap map = s.Serialize();

            EstablishAgentCommunicationMessage t = new EstablishAgentCommunicationMessage();
            t.Deserialize(map);

            Assert.Equal(s.Address, t.Address);
            Assert.Equal(s.AgentID, t.AgentID);
            Assert.Equal(s.Port, t.Port);
            Assert.Equal(s.SeedCapability, t.SeedCapability);
        }

        [Fact]
        public void ParcelObjectOwnersMessage()
        {
            ParcelObjectOwnersReplyMessage s = new ParcelObjectOwnersReplyMessage();
            s.PrimOwnersBlock = new ParcelObjectOwnersReplyMessage.PrimOwner[2];

            ParcelObjectOwnersReplyMessage.PrimOwner obj = new ParcelObjectOwnersReplyMessage.PrimOwner();
            obj.OwnerID = UUID.Random();
            obj.Count = 10;
            obj.IsGroupOwned = true;
            obj.OnlineStatus = false;
            obj.TimeStamp = new DateTime(2010, 4, 13, 7, 19, 43);
            s.PrimOwnersBlock[0] = obj;

            ParcelObjectOwnersReplyMessage.PrimOwner obj1 = new ParcelObjectOwnersReplyMessage.PrimOwner();
            obj1.OwnerID = UUID.Random();
            obj1.Count = 0;
            obj1.IsGroupOwned = false;
            obj1.OnlineStatus = false;
            obj1.TimeStamp = new DateTime(1991, 1, 31, 3, 13, 31);
            s.PrimOwnersBlock[1] = obj1;

            OSDMap map = s.Serialize();

            ParcelObjectOwnersReplyMessage t = new ParcelObjectOwnersReplyMessage();
            t.Deserialize(map);

            for (int i = 0; i < t.PrimOwnersBlock.Length; i++)
            {
                Assert.Equal(s.PrimOwnersBlock[i].Count, t.PrimOwnersBlock[i].Count);
                Assert.Equal(s.PrimOwnersBlock[i].IsGroupOwned, t.PrimOwnersBlock[i].IsGroupOwned);
                Assert.Equal(s.PrimOwnersBlock[i].OnlineStatus, t.PrimOwnersBlock[i].OnlineStatus);
                Assert.Equal(s.PrimOwnersBlock[i].OwnerID, t.PrimOwnersBlock[i].OwnerID);
                Assert.Equal(s.PrimOwnersBlock[i].TimeStamp, t.PrimOwnersBlock[i].TimeStamp);
            }
        }

        [Fact]
        public void ChatterBoxInvitationMessage()
        {
            ChatterBoxInvitationMessage s = new ChatterBoxInvitationMessage();
            s.BinaryBucket = Utils.EmptyBytes;
            s.Dialog = InstantMessageDialog.InventoryOffered;
            s.FromAgentID = UUID.Random();
            s.FromAgentName = "Prokofy Neva";
            s.GroupIM = false;
            s.IMSessionID = s.FromAgentID ^ UUID.Random();
            s.Message = "Test Test Test";
            s.Offline = InstantMessageOnline.Online;
            s.ParentEstateID = 1;
            s.Position = Vector3.One;
            s.RegionID = UUID.Random();
            s.Timestamp = DateTime.UtcNow;
            s.ToAgentID = UUID.Random();

            OSDMap map = s.Serialize();

            ChatterBoxInvitationMessage t = new ChatterBoxInvitationMessage();
            t.Deserialize(map);

            Assert.Equal(s.BinaryBucket, t.BinaryBucket);
            Assert.Equal(s.Dialog, t.Dialog);
            Assert.Equal(s.FromAgentID, t.FromAgentID);
            Assert.Equal(s.FromAgentName, t.FromAgentName);
            Assert.Equal(s.GroupIM, t.GroupIM);
            Assert.Equal(s.IMSessionID, t.IMSessionID);
            Assert.Equal(s.Message, t.Message);
            Assert.Equal(s.Offline, t.Offline);
            Assert.Equal(s.ParentEstateID, t.ParentEstateID);
            Assert.Equal(s.Position, t.Position);
            Assert.Equal(s.RegionID, t.RegionID);
            Assert.Equal(s.Timestamp, t.Timestamp);
            Assert.Equal(s.ToAgentID, t.ToAgentID);
        }

        [Fact]
        public void ChatterboxSessionEventReplyMessage()
        {
            ChatterboxSessionEventReplyMessage s = new ChatterboxSessionEventReplyMessage();
            s.SessionID = UUID.Random();
            s.Success = true;

            OSDMap map = s.Serialize();

            ChatterboxSessionEventReplyMessage t = new ChatterboxSessionEventReplyMessage();
            t.Deserialize(map);

            Assert.Equal(s.SessionID, t.SessionID);
            Assert.Equal(s.Success, t.Success);
        }

        [Fact]
        public void ChatterBoxSessionStartReplyMessage()
        {
            ChatterBoxSessionStartReplyMessage s = new ChatterBoxSessionStartReplyMessage();
            s.ModeratedVoice = true;
            s.SessionID = UUID.Random();
            s.SessionName = "Test Session";
            s.Success = true;
            s.TempSessionID = UUID.Random();
            s.Type = 1;
            s.VoiceEnabled = true;

            OSDMap map = s.Serialize();

            ChatterBoxSessionStartReplyMessage t = new ChatterBoxSessionStartReplyMessage();
            t.Deserialize(map);

            Assert.Equal(s.ModeratedVoice, t.ModeratedVoice);
            Assert.Equal(s.SessionID, t.SessionID);
            Assert.Equal(s.SessionName, t.SessionName);
            Assert.Equal(s.Success, t.Success);
            Assert.Equal(s.TempSessionID, t.TempSessionID);
            Assert.Equal(s.Type, t.Type);
            Assert.Equal(s.VoiceEnabled, t.VoiceEnabled);
        }

        [Fact]
        public void ChatterBoxSessionAgentListUpdatesMessage()
        {
            ChatterBoxSessionAgentListUpdatesMessage s = new ChatterBoxSessionAgentListUpdatesMessage();
            s.SessionID = UUID.Random();
            s.Updates = new ChatterBoxSessionAgentListUpdatesMessage.AgentUpdatesBlock[1];

            ChatterBoxSessionAgentListUpdatesMessage.AgentUpdatesBlock block1 = new ChatterBoxSessionAgentListUpdatesMessage.AgentUpdatesBlock();
            block1.AgentID = UUID.Random();
            block1.CanVoiceChat = true;
            block1.IsModerator = true;
            block1.MuteText = true;
            block1.MuteVoice = true;
            block1.Transition = "ENTER";

            ChatterBoxSessionAgentListUpdatesMessage.AgentUpdatesBlock block2 = new ChatterBoxSessionAgentListUpdatesMessage.AgentUpdatesBlock();
            block2.AgentID = UUID.Random();
            block2.CanVoiceChat = true;
            block2.IsModerator = true;
            block2.MuteText = true;
            block2.MuteVoice = true;
            block2.Transition = "LEAVE";

            s.Updates[0] = block1;
            // s.Updates[1] = block2;

            OSDMap map = s.Serialize();

            ChatterBoxSessionAgentListUpdatesMessage t = new ChatterBoxSessionAgentListUpdatesMessage();
            t.Deserialize(map);

            Assert.Equal(s.SessionID, t.SessionID);
            for (int i = 0; i < t.Updates.Length; i++)
            {
                Assert.Equal(s.Updates[i].AgentID, t.Updates[i].AgentID);
                Assert.Equal(s.Updates[i].CanVoiceChat, t.Updates[i].CanVoiceChat);
                Assert.Equal(s.Updates[i].IsModerator, t.Updates[i].IsModerator);
                Assert.Equal(s.Updates[i].MuteText, t.Updates[i].MuteText);
                Assert.Equal(s.Updates[i].MuteVoice, t.Updates[i].MuteVoice);
                Assert.Equal(s.Updates[i].Transition, t.Updates[i].Transition);
            }
        }

        [Fact]
        public void ViewerStatsMessage()
        {
            ViewerStatsMessage s = new ViewerStatsMessage();

            s.AgentFPS = 45.5f;
            s.AgentsInView = 1;
            s.SystemCPU = "Intel 80286";
            s.StatsDropped = 2;
            s.StatsFailedResends = 3;
            s.SystemGPU = "Vesa VGA+";
            s.SystemGPUClass = 4;
            s.SystemGPUVendor = "China";
            s.SystemGPUVersion = String.Empty;
            s.InCompressedPackets = 5000;
            s.InKbytes = 6000;
            s.InPackets = 22000;
            s.InSavings = 19;
            s.MiscInt1 = 5;
            s.MiscInt2 = 6;
            s.FailuresInvalid = 20;
            s.AgentLanguage = "en";
            s.AgentMemoryUsed = 12878728;
            s.MetersTraveled = 9999123;
            s.object_kbytes = 70001;
            s.FailuresOffCircuit = 201;
            s.SystemOS = "Palm OS 3.1";
            s.OutCompressedPackets = 8000;
            s.OutKbytes = 9000999;
            s.OutPackets = 21000210;
            s.OutSavings = 181;
            s.AgentPing = 135579;
            s.SystemInstalledRam = 4000000;
            s.RegionsVisited = 4579;
            s.FailuresResent = 9;
            s.AgentRuntime = 360023;
            s.FailuresSendPacket = 565;
            s.SessionID = UUID.Random();
            s.SimulatorFPS = 454;
            s.AgentStartTime = new DateTime(1973, 1, 16, 5, 23, 33);
            s.MiscString1 = "Unused String";
            s.texture_kbytes = 9367498382;
            s.AgentVersion = "1";
            s.MiscVersion = 1;
            s.VertexBuffersEnabled = true;
            s.world_kbytes = 232344439;

            OSDMap map = s.Serialize();
            ViewerStatsMessage t = new ViewerStatsMessage();
            t.Deserialize(map);

            Assert.Equal(s.AgentFPS, t.AgentFPS);
            Assert.Equal(s.AgentsInView, t.AgentsInView);
            Assert.Equal(s.SystemCPU, t.SystemCPU);
            Assert.Equal(s.StatsDropped, t.StatsDropped);
            Assert.Equal(s.StatsFailedResends, t.StatsFailedResends);
            Assert.Equal(s.SystemGPU, t.SystemGPU);
            Assert.Equal(s.SystemGPUClass, t.SystemGPUClass);
            Assert.Equal(s.SystemGPUVendor, t.SystemGPUVendor);
            Assert.Equal(s.SystemGPUVersion, t.SystemGPUVersion);
            Assert.Equal(s.InCompressedPackets, t.InCompressedPackets);
            Assert.Equal(s.InKbytes, t.InKbytes);
            Assert.Equal(s.InPackets, t.InPackets);
            Assert.Equal(s.InSavings, t.InSavings);
            Assert.Equal(s.MiscInt1, t.MiscInt1);
            Assert.Equal(s.MiscInt2, t.MiscInt2);
            Assert.Equal(s.FailuresInvalid, t.FailuresInvalid);
            Assert.Equal(s.AgentLanguage, t.AgentLanguage);
            Assert.Equal(s.AgentMemoryUsed, t.AgentMemoryUsed);
            Assert.Equal(s.MetersTraveled, t.MetersTraveled);
            Assert.Equal(s.object_kbytes, t.object_kbytes);
            Assert.Equal(s.FailuresOffCircuit, t.FailuresOffCircuit);
            Assert.Equal(s.SystemOS, t.SystemOS);
            Assert.Equal(s.OutCompressedPackets, t.OutCompressedPackets);
            Assert.Equal(s.OutKbytes, t.OutKbytes);
            Assert.Equal(s.OutPackets, t.OutPackets);
            Assert.Equal(s.OutSavings, t.OutSavings);
            Assert.Equal(s.AgentPing, t.AgentPing);
            Assert.Equal(s.SystemInstalledRam, t.SystemInstalledRam);
            Assert.Equal(s.RegionsVisited, t.RegionsVisited);
            Assert.Equal(s.FailuresResent, t.FailuresResent);
            Assert.Equal(s.AgentRuntime, t.AgentRuntime);
            Assert.Equal(s.FailuresSendPacket, t.FailuresSendPacket);
            Assert.Equal(s.SessionID, t.SessionID);
            Assert.Equal(s.SimulatorFPS, t.SimulatorFPS);
            Assert.Equal(s.AgentStartTime, t.AgentStartTime);
            Assert.Equal(s.MiscString1, t.MiscString1);
            Assert.Equal(s.texture_kbytes, t.texture_kbytes);
            Assert.Equal(s.AgentVersion, t.AgentVersion);
            Assert.Equal(s.MiscVersion, t.MiscVersion);
            Assert.Equal(s.VertexBuffersEnabled, t.VertexBuffersEnabled);
            Assert.Equal(s.world_kbytes, t.world_kbytes);


        }

        [Fact]
        public void ParcelVoiceInfoRequestMessage()
        {
            ParcelVoiceInfoRequestMessage s = new ParcelVoiceInfoRequestMessage();
            s.SipChannelUri = testURI;
            s.ParcelID = 1;
            s.RegionName = "Hooper";

            OSDMap map = s.Serialize();

            ParcelVoiceInfoRequestMessage t = new ParcelVoiceInfoRequestMessage();
            t.Deserialize(map);

            Assert.Equal(s.SipChannelUri, t.SipChannelUri);
            Assert.Equal(s.ParcelID, t.ParcelID);
            Assert.Equal(s.RegionName, t.RegionName);
        }

        [Fact]
        public void ScriptRunningReplyMessage()
        {
            ScriptRunningReplyMessage s = new ScriptRunningReplyMessage();
            s.ItemID = UUID.Random();
            s.Mono = true;
            s.Running = true;
            s.ObjectID = UUID.Random();

            OSDMap map = s.Serialize();

            ScriptRunningReplyMessage t = new ScriptRunningReplyMessage();
            t.Deserialize(map);

            Assert.Equal(s.ItemID, t.ItemID);
            Assert.Equal(s.Mono, t.Mono);
            Assert.Equal(s.ObjectID, t.ObjectID);
            Assert.Equal(s.Running, t.Running);

        }

        [Fact]
        public void MapLayerMessage()
        {

            MapLayerReplyVariant s = new MapLayerReplyVariant();
            s.Flags = 1;

            MapLayerReplyVariant.LayerData[] blocks = new MapLayerReplyVariant.LayerData[2];

            MapLayerReplyVariant.LayerData block = new MapLayerReplyVariant.LayerData();
            block.ImageID = UUID.Random();
            block.Bottom = 1;
            block.Top = 2;
            block.Left = 3;
            block.Right = 4;



            blocks[0] = block;

            block.ImageID = UUID.Random();
            block.Bottom = 5;
            block.Top = 6;
            block.Left = 7;
            block.Right = 9;

            blocks[1] = block;

            s.LayerDataBlocks = blocks;

            OSDMap map = s.Serialize();

            MapLayerReplyVariant t = new MapLayerReplyVariant();

            t.Deserialize(map);

            Assert.Equal(s.Flags, t.Flags);


            for (int i = 0; i < s.LayerDataBlocks.Length; i++)
            {
                Assert.Equal(s.LayerDataBlocks[i].ImageID, t.LayerDataBlocks[i].ImageID);
                Assert.Equal(s.LayerDataBlocks[i].Top, t.LayerDataBlocks[i].Top);
                Assert.Equal(s.LayerDataBlocks[i].Left, t.LayerDataBlocks[i].Left);
                Assert.Equal(s.LayerDataBlocks[i].Right, t.LayerDataBlocks[i].Right);
                Assert.Equal(s.LayerDataBlocks[i].Bottom, t.LayerDataBlocks[i].Bottom);
            }
        }

        [Fact] // VARIANT A
        public void ChatSessionRequestStartConference()
        {
            ChatSessionRequestStartConference s = new ChatSessionRequestStartConference();
            s.SessionID = UUID.Random();
            s.AgentsBlock = new UUID[2];
            s.AgentsBlock[0] = UUID.Random();
            s.AgentsBlock[0] = UUID.Random();

            OSDMap map = s.Serialize();

            ChatSessionRequestStartConference t = new ChatSessionRequestStartConference();
            t.Deserialize(map);

            Assert.Equal(s.SessionID, t.SessionID);
            Assert.Equal(s.Method, t.Method);
            for (int i = 0; i < t.AgentsBlock.Length; i++)
            {
                Assert.Equal(s.AgentsBlock[i], t.AgentsBlock[i]);
            }
        }

        [Fact]
        public void ChatSessionRequestMuteUpdate()
        {
            ChatSessionRequestMuteUpdate s = new ChatSessionRequestMuteUpdate();
            s.AgentID = UUID.Random();
            s.RequestKey = "text";
            s.RequestValue = true;
            s.SessionID = UUID.Random();

            OSDMap map = s.Serialize();

            ChatSessionRequestMuteUpdate t = new ChatSessionRequestMuteUpdate();
            t.Deserialize(map);

            Assert.Equal(s.AgentID, t.AgentID);
            Assert.Equal(s.Method, t.Method);
            Assert.Equal(s.RequestKey, t.RequestKey);
            Assert.Equal(s.RequestValue, t.RequestValue);
            Assert.Equal(s.SessionID, t.SessionID);
        }

        [Fact]
        public void ChatSessionAcceptInvitation()
        {
            ChatSessionAcceptInvitation s = new ChatSessionAcceptInvitation();
            s.SessionID = UUID.Random();

            OSDMap map = s.Serialize();

            ChatSessionAcceptInvitation t = new ChatSessionAcceptInvitation();
            t.Deserialize(map);

            Assert.Equal(s.Method, t.Method);
            Assert.Equal(s.SessionID, t.SessionID);
        }

        [Fact]
        public void RequiredVoiceVersionMessage()
        {
            RequiredVoiceVersionMessage s = new RequiredVoiceVersionMessage();
            s.MajorVersion = 1;
            s.MinorVersion = 0;
            s.RegionName = "Hooper";

            OSDMap map = s.Serialize();

            RequiredVoiceVersionMessage t = new RequiredVoiceVersionMessage();
            t.Deserialize(map);

            Assert.Equal(s.MajorVersion, t.MajorVersion);
            Assert.Equal(s.MinorVersion, t.MinorVersion);
            Assert.Equal(s.RegionName, t.RegionName);
        }

        [Fact]
        public void CopyInventoryFromNotecardMessage()
        {
            CopyInventoryFromNotecardMessage s = new CopyInventoryFromNotecardMessage();
            s.CallbackID = 1;
            s.FolderID = UUID.Random();
            s.ItemID = UUID.Random();
            s.NotecardID = UUID.Random();
            s.ObjectID = UUID.Random();

            OSDMap map = s.Serialize();

            CopyInventoryFromNotecardMessage t = new CopyInventoryFromNotecardMessage();
            t.Deserialize(map);

            Assert.Equal(s.CallbackID, t.CallbackID);
            Assert.Equal(s.FolderID, t.FolderID);
            Assert.Equal(s.ItemID, t.ItemID);
            Assert.Equal(s.NotecardID, t.NotecardID);
            Assert.Equal(s.ObjectID, t.ObjectID);
        }

        [Fact]
        public void ProvisionVoiceAccountRequestMessage()
        {
            ProvisionVoiceAccountRequestMessage s = new ProvisionVoiceAccountRequestMessage();
            s.Username = "username";
            s.Password = "password";

            OSDMap map = s.Serialize();

            ProvisionVoiceAccountRequestMessage t = new ProvisionVoiceAccountRequestMessage();
            t.Deserialize(map);

            Assert.Equal(s.Password, t.Password);
            Assert.Equal(s.Username, t.Username);
        }

        [Fact]
        public void UpdateAgentLanguageMessage()
        {
            UpdateAgentLanguageMessage s = new UpdateAgentLanguageMessage();
            s.Language = "en";
            s.LanguagePublic = false;

            OSDMap map = s.Serialize();

            UpdateAgentLanguageMessage t = new UpdateAgentLanguageMessage();
            t.Deserialize(map);

            Assert.Equal(s.Language, t.Language);
            Assert.Equal(s.LanguagePublic, t.LanguagePublic);

        }

        [Fact]
        public void ParcelPropertiesMessage()
        {
            ParcelPropertiesMessage s = new ParcelPropertiesMessage();
            s.AABBMax = Vector3.Parse("<1,2,3>");
            s.AABBMin = Vector3.Parse("<2,3,1>");
            s.Area = 1024;
            s.AuctionID = uint.MaxValue;
            s.AuthBuyerID = UUID.Random();
            s.Bitmap = Utils.EmptyBytes;
            s.Category = ParcelCategory.Educational;
            s.ClaimDate = new DateTime(2008, 12, 25, 3, 15, 22);
            s.ClaimPrice = 1000;
            s.Desc = "Test Description";
            s.GroupID = UUID.Random();
            s.GroupPrims = 50;
            s.IsGroupOwned = false;
            s.LandingType = LandingType.None;
            s.LocalID = 1;
            s.MaxPrims = 234;
            s.MediaAutoScale = false;
            s.MediaDesc = "Example Media Description";
            s.MediaHeight = 480;
            s.MediaID = UUID.Random();
            s.MediaLoop = false;
            s.MediaType = "text/html";
            s.MediaURL = "http://www.openmetaverse.co";
            s.MediaWidth = 640;
            s.MusicURL = "http://scfire-ntc-aa04.stream.aol.com:80/stream/1075"; // Yee Haw
            s.Name = "Test Name";
            s.ObscureMedia = false;
            s.ObscureMusic = false;
            s.OtherCleanTime = 5;
            s.OtherCount = 200;
            s.OtherPrims = 300;
            s.OwnerID = UUID.Random();
            s.OwnerPrims = 0;
            s.ParcelFlags = ParcelFlags.AllowDamage | ParcelFlags.AllowGroupScripts | ParcelFlags.AllowVoiceChat;
            s.ParcelPrimBonus = 0f;
            s.PassHours = 1.5f;
            s.PassPrice = 10;
            s.PublicCount = 20;
            s.RegionDenyAgeUnverified = false;
            s.RegionDenyAnonymous = false;
            s.RegionPushOverride = true;
            s.RentPrice = 0;
            s.RequestResult = ParcelResult.Single;
            s.SalePrice = 9999;
            s.SelectedPrims = 1;
            s.SelfCount = 2;
            s.SequenceID = -4000;
            s.SimWideMaxPrims = 937;
            s.SimWideTotalPrims = 117;
            s.SnapSelection = false;
            s.SnapshotID = UUID.Random();
            s.Status = ParcelStatus.Leased;
            s.TotalPrims = 219;
            s.UserLocation = Vector3.Parse("<3,4,5>");
            s.UserLookAt = Vector3.Parse("<5,4,3>");

            OSDMap map = s.Serialize();
            ParcelPropertiesMessage t = new ParcelPropertiesMessage();

            t.Deserialize(map);

            Assert.Equal(s.AABBMax, t.AABBMax);
            Assert.Equal(s.AABBMin, t.AABBMin);
            Assert.Equal(s.Area, t.Area);
            Assert.Equal(s.AuctionID, t.AuctionID);
            Assert.Equal(s.AuthBuyerID, t.AuthBuyerID);
            Assert.Equal(s.Bitmap, t.Bitmap);
            Assert.Equal(s.Category, t.Category);
            Assert.Equal(s.ClaimDate, t.ClaimDate);
            Assert.Equal(s.ClaimPrice, t.ClaimPrice);
            Assert.Equal(s.Desc, t.Desc);
            Assert.Equal(s.GroupID, t.GroupID);
            Assert.Equal(s.GroupPrims, t.GroupPrims);
            Assert.Equal(s.IsGroupOwned, t.IsGroupOwned);
            Assert.Equal(s.LandingType, t.LandingType);
            Assert.Equal(s.LocalID, t.LocalID);
            Assert.Equal(s.MaxPrims, t.MaxPrims);
            Assert.Equal(s.MediaAutoScale, t.MediaAutoScale);
            Assert.Equal(s.MediaDesc, t.MediaDesc);
            Assert.Equal(s.MediaHeight, t.MediaHeight);
            Assert.Equal(s.MediaID, t.MediaID);
            Assert.Equal(s.MediaLoop, t.MediaLoop);
            Assert.Equal(s.MediaType, t.MediaType);
            Assert.Equal(s.MediaURL, t.MediaURL);
            Assert.Equal(s.MediaWidth, t.MediaWidth);
            Assert.Equal(s.MusicURL, t.MusicURL);
            Assert.Equal(s.Name, t.Name);
            Assert.Equal(s.ObscureMedia, t.ObscureMedia);
            Assert.Equal(s.ObscureMusic, t.ObscureMusic);
            Assert.Equal(s.OtherCleanTime, t.OtherCleanTime);
            Assert.Equal(s.OtherCount, t.OtherCount);
            Assert.Equal(s.OtherPrims, t.OtherPrims);
            Assert.Equal(s.OwnerID, t.OwnerID);
            Assert.Equal(s.OwnerPrims, t.OwnerPrims);
            Assert.Equal(s.ParcelFlags, t.ParcelFlags);
            Assert.Equal(s.ParcelPrimBonus, t.ParcelPrimBonus);
            Assert.Equal(s.PassHours, t.PassHours);
            Assert.Equal(s.PassPrice, t.PassPrice);
            Assert.Equal(s.PublicCount, t.PublicCount);
            Assert.Equal(s.RegionDenyAgeUnverified, t.RegionDenyAgeUnverified);
            Assert.Equal(s.RegionDenyAnonymous, t.RegionDenyAnonymous);
            Assert.Equal(s.RegionPushOverride, t.RegionPushOverride);
            Assert.Equal(s.RentPrice, t.RentPrice);
            Assert.Equal(s.RequestResult, t.RequestResult);
            Assert.Equal(s.SalePrice, t.SalePrice);
            Assert.Equal(s.SelectedPrims, t.SelectedPrims);
            Assert.Equal(s.SelfCount, t.SelfCount);
            Assert.Equal(s.SequenceID, t.SequenceID);
            Assert.Equal(s.SimWideMaxPrims, t.SimWideMaxPrims);
            Assert.Equal(s.SimWideTotalPrims, t.SimWideTotalPrims);
            Assert.Equal(s.SnapSelection, t.SnapSelection);
            Assert.Equal(s.SnapshotID, t.SnapshotID);
            Assert.Equal(s.Status, t.Status);
            Assert.Equal(s.TotalPrims, t.TotalPrims);
            Assert.Equal(s.UserLocation, t.UserLocation);
            Assert.Equal(s.UserLookAt, t.UserLookAt);
        }

        [Fact]
        public void ParcelPropertiesUpdateMessage()
        {
            ParcelPropertiesUpdateMessage s = new ParcelPropertiesUpdateMessage();
            s.AuthBuyerID = UUID.Random();
            s.Category = ParcelCategory.Gaming;
            s.Desc = "Example Description";
            s.GroupID = UUID.Random();
            s.Landing = LandingType.LandingPoint;
            s.LocalID = 160;
            s.MediaAutoScale = true;
            s.MediaDesc = "Example Media Description";
            s.MediaHeight = 600;
            s.MediaID = UUID.Random();
            s.MediaLoop = false;
            s.MediaType = "image/jpeg";
            s.MediaURL = "http://www.openmetaverse.co/test.jpeg";
            s.MediaWidth = 800;
            s.MusicURL = "http://scfire-ntc-aa04.stream.aol.com:80/stream/1075";
            s.Name = "Example Parcel Description";
            s.ObscureMedia = true;
            s.ObscureMusic = true;
            s.ParcelFlags = ParcelFlags.AllowVoiceChat | ParcelFlags.ContributeWithDeed;
            s.PassHours = 5.5f;
            s.PassPrice = 100;
            s.SalePrice = 99;
            s.SnapshotID = UUID.Random();
            s.UserLocation = Vector3.Parse("<128,128,128>");
            s.UserLookAt = Vector3.Parse("<256,256,256>");

            OSDMap map = s.Serialize();

            ParcelPropertiesUpdateMessage t = new ParcelPropertiesUpdateMessage();

            t.Deserialize(map);

            Assert.Equal(s.AuthBuyerID, t.AuthBuyerID);
            Assert.Equal(s.Category, t.Category);
            Assert.Equal(s.Desc, t.Desc);
            Assert.Equal(s.GroupID, t.GroupID);
            Assert.Equal(s.Landing, t.Landing);
            Assert.Equal(s.LocalID, t.LocalID);
            Assert.Equal(s.MediaAutoScale, t.MediaAutoScale);
            Assert.Equal(s.MediaDesc, t.MediaDesc);
            Assert.Equal(s.MediaHeight, t.MediaHeight);
            Assert.Equal(s.MediaID, t.MediaID);
            Assert.Equal(s.MediaLoop, t.MediaLoop);
            Assert.Equal(s.MediaType, t.MediaType);
            Assert.Equal(s.MediaURL, t.MediaURL);
            Assert.Equal(s.MediaWidth, t.MediaWidth);
            Assert.Equal(s.MusicURL, t.MusicURL);
            Assert.Equal(s.Name, t.Name);
            Assert.Equal(s.ObscureMedia, t.ObscureMedia);
            Assert.Equal(s.ObscureMusic, t.ObscureMusic);
            Assert.Equal(s.ParcelFlags, t.ParcelFlags);
            Assert.Equal(s.PassHours, t.PassHours);
            Assert.Equal(s.PassPrice, t.PassPrice);
            Assert.Equal(s.SalePrice, t.SalePrice);
            Assert.Equal(s.SnapshotID, t.SnapshotID);
            Assert.Equal(s.UserLocation, t.UserLocation);
            Assert.Equal(s.UserLookAt, t.UserLookAt);
        }
        [Fact]
        public void EnableSimulatorMessage()
        {
            EnableSimulatorMessage s = new EnableSimulatorMessage();
            s.Simulators = new EnableSimulatorMessage.SimulatorInfoBlock[2];

            EnableSimulatorMessage.SimulatorInfoBlock block1 = new EnableSimulatorMessage.SimulatorInfoBlock();
            block1.IP = testIP;
            block1.Port = 3000;
            block1.RegionHandle = testHandle;
            s.Simulators[0] = block1;

            EnableSimulatorMessage.SimulatorInfoBlock block2 = new EnableSimulatorMessage.SimulatorInfoBlock();
            block2.IP = testIP;
            block2.Port = 3001;
            block2.RegionHandle = testHandle;
            s.Simulators[1] = block2;

            OSDMap map = s.Serialize();

            EnableSimulatorMessage t = new EnableSimulatorMessage();
            t.Deserialize(map);

            for (int i = 0; i < t.Simulators.Length; i++)
            {
                Assert.Equal(s.Simulators[i].IP, t.Simulators[i].IP);
                Assert.Equal(s.Simulators[i].Port, t.Simulators[i].Port);
                Assert.Equal(s.Simulators[i].RegionHandle, t.Simulators[i].RegionHandle);
            }
        }

        [Fact]
        public void RemoteParcelRequestReply()
        {
            RemoteParcelRequestReply s = new RemoteParcelRequestReply();
            s.ParcelID = UUID.Random();
            OSDMap map = s.Serialize();

            RemoteParcelRequestReply t = new RemoteParcelRequestReply();
            t.Deserialize(map);

            Assert.Equal(s.ParcelID, t.ParcelID);
        }

        [Fact]
        public void UpdateScriptTaskMessage()
        {
            UpdateScriptTaskUpdateMessage s = new UpdateScriptTaskUpdateMessage();
            s.TaskID = UUID.Random();
            s.Target = "mono";
            s.ScriptRunning = true;
            s.ItemID = UUID.Random();

            OSDMap map = s.Serialize();
            UpdateScriptTaskUpdateMessage t = new UpdateScriptTaskUpdateMessage();
            t.Deserialize(map);

            Assert.Equal(s.ItemID, t.ItemID);
            Assert.Equal(s.ScriptRunning, t.ScriptRunning);
            Assert.Equal(s.Target, t.Target);
            Assert.Equal(s.TaskID, t.TaskID);
        }

        [Fact]
        public void UpdateScriptAgentMessage()
        {
            UpdateScriptAgentRequestMessage s = new UpdateScriptAgentRequestMessage();
            s.ItemID = UUID.Random();
            s.Target = "lsl2";

            OSDMap map = s.Serialize();

            UpdateScriptAgentRequestMessage t = new UpdateScriptAgentRequestMessage();
            t.Deserialize(map);

            Assert.Equal(s.ItemID, t.ItemID);
            Assert.Equal(s.Target, t.Target);
        }

        [Fact]
        public void SendPostcardMessage()
        {
            SendPostcardMessage s = new SendPostcardMessage();
            s.FromEmail = "contact@openmetaverse.co";
            s.FromName = "Jim Radford";
            s.GlobalPosition = Vector3.One;
            s.Message = "Hello, How are you today?";
            s.Subject = "Postcard from the edge";
            s.ToEmail = "test1@example.com";

            OSDMap map = s.Serialize();

            SendPostcardMessage t = new SendPostcardMessage();
            t.Deserialize(map);

            Assert.Equal(s.FromEmail, t.FromEmail);
            Assert.Equal(s.FromName, t.FromName);
            Assert.Equal(s.GlobalPosition, t.GlobalPosition);
            Assert.Equal(s.Message, t.Message);
            Assert.Equal(s.Subject, t.Subject);
            Assert.Equal(s.ToEmail, t.ToEmail);
        }

        [Fact]
        public void UpdateNotecardAgentInventoryMessage()
        {
            UpdateAgentInventoryRequestMessage s = new UpdateAgentInventoryRequestMessage();
            s.ItemID = UUID.Random();

            OSDMap map = s.Serialize();

            UpdateAgentInventoryRequestMessage t = new UpdateAgentInventoryRequestMessage();
            t.Deserialize(map);

            Assert.Equal(s.ItemID, t.ItemID);
        }

        [Fact]
        public void LandStatReplyMessage()
        {
            LandStatReplyMessage s = new LandStatReplyMessage();
            s.ReportType = 22;
            s.RequestFlags = 44;
            s.TotalObjectCount = 2;
            s.ReportDataBlocks = new LandStatReplyMessage.ReportDataBlock[2];

            LandStatReplyMessage.ReportDataBlock block1 = new LandStatReplyMessage.ReportDataBlock();
            block1.Location = Vector3.One;
            block1.MonoScore = 99;
            block1.OwnerName = "Profoky Neva";
            block1.Score = 10;
            block1.TaskID = UUID.Random();
            block1.TaskLocalID = 987341;
            block1.TaskName = "Verbal Flogging";
            block1.TimeStamp = new DateTime(2009, 5, 23, 4, 30, 0);
            s.ReportDataBlocks[0] = block1;

            LandStatReplyMessage.ReportDataBlock block2 = new LandStatReplyMessage.ReportDataBlock();
            block2.Location = Vector3.One;
            block2.MonoScore = 1;
            block2.OwnerName = "Philip Linden";
            block2.Score = 5;
            block2.TaskID = UUID.Random();
            block2.TaskLocalID = 987342;
            block2.TaskName = "Happy Ant";
            block2.TimeStamp = new DateTime(2008, 4, 22, 3, 29, 55);
            s.ReportDataBlocks[1] = block2;

            OSDMap map = s.Serialize();

            LandStatReplyMessage t = new LandStatReplyMessage();
            t.Deserialize(map);

            Assert.Equal(s.ReportType, t.ReportType);
            Assert.Equal(s.RequestFlags, t.RequestFlags);
            Assert.Equal(s.TotalObjectCount, t.TotalObjectCount);

            for (int i = 0; i < t.ReportDataBlocks.Length; i++)
            {
                Assert.Equal(s.ReportDataBlocks[i].Location, t.ReportDataBlocks[i].Location);
                Assert.Equal(s.ReportDataBlocks[i].MonoScore, t.ReportDataBlocks[i].MonoScore);
                Assert.Equal(s.ReportDataBlocks[i].OwnerName, t.ReportDataBlocks[i].OwnerName);
                Assert.Equal(s.ReportDataBlocks[i].Score, t.ReportDataBlocks[i].Score);
                Assert.Equal(s.ReportDataBlocks[i].TaskID, t.ReportDataBlocks[i].TaskID);
                Assert.Equal(s.ReportDataBlocks[i].TaskLocalID, t.ReportDataBlocks[i].TaskLocalID);
                Assert.Equal(s.ReportDataBlocks[i].TaskName, t.ReportDataBlocks[i].TaskName);
                Assert.Equal(s.ReportDataBlocks[i].TimeStamp, t.ReportDataBlocks[i].TimeStamp);
            }
        }

        [Fact]
        public void TelportFailedMessage()
        {
            TeleportFailedMessage s = new TeleportFailedMessage();
            s.AgentID = UUID.Random();
            s.MessageKey = "Key";
            s.Reason = "Unable To Teleport for some unspecified reason";
            s.ExtraParams = String.Empty;

            OSDMap map = s.Serialize();

            TeleportFailedMessage t = new TeleportFailedMessage();
            t.Deserialize(map);

            Assert.Equal(s.AgentID, t.AgentID);
            Assert.Equal(s.ExtraParams, t.ExtraParams);
            Assert.Equal(s.MessageKey, t.MessageKey);
            Assert.Equal(s.Reason, t.Reason);

        }

        [Fact]
        public void UpdateAgentInformationMessage()
        {
            UpdateAgentInformationMessage s = new UpdateAgentInformationMessage();
            s.MaxAccess = "PG";
            OSDMap map = s.Serialize();

            UpdateAgentInformationMessage t = new UpdateAgentInformationMessage();
            t.Deserialize(map);

            Assert.Equal(s.MaxAccess, t.MaxAccess);
        }

        [Fact]
        public void PlacesReplyMessage()
        {
            PlacesReplyMessage s = new PlacesReplyMessage();
            s.TransactionID = UUID.Random();
            s.AgentID = UUID.Random();
            s.QueryID = UUID.Random();
            s.QueryDataBlocks = new PlacesReplyMessage.QueryData[2];

            PlacesReplyMessage.QueryData q1 = new PlacesReplyMessage.QueryData();
            q1.ActualArea = 1024;
            q1.BillableArea = 768;
            q1.Description = "Test Description Q1";
            q1.Dwell = 1435.4f;
            q1.Flags = 1 << 6;
            q1.GlobalX = 1;
            q1.GlobalY = 2;
            q1.GlobalZ = 3;
            q1.Name = "Test Name Q1";
            q1.OwnerID = UUID.Random();
            q1.Price = 1;
            q1.ProductSku = "021";
            q1.SimName = "Hooper";
            q1.SnapShotID = UUID.Random();

            s.QueryDataBlocks[0] = q1;

            PlacesReplyMessage.QueryData q2 = new PlacesReplyMessage.QueryData();
            q2.ActualArea = 512;
            q2.BillableArea = 384;
            q2.Description = "Test Description Q2";
            q2.Dwell = 1;
            q2.Flags = 1 << 4;
            q2.GlobalX = 4;
            q2.GlobalY = 5;
            q2.GlobalZ = 6;
            q2.Name = "Test Name Q2";
            q2.OwnerID = UUID.Random();
            q2.Price = 2;
            q2.ProductSku = "022";
            q2.SimName = "Tethys";
            q2.SnapShotID = UUID.Random();

            s.QueryDataBlocks[1] = q2;

            OSDMap map = s.Serialize();

            PlacesReplyMessage t = new PlacesReplyMessage();
            t.Deserialize(map);

            Assert.Equal(s.AgentID, t.AgentID);
            Assert.Equal(s.TransactionID, t.TransactionID);
            Assert.Equal(s.QueryID, t.QueryID);

            for (int i = 0; i < s.QueryDataBlocks.Length; i++)
            {
                Assert.Equal(s.QueryDataBlocks[i].ActualArea, t.QueryDataBlocks[i].ActualArea);
                Assert.Equal(s.QueryDataBlocks[i].BillableArea, t.QueryDataBlocks[i].BillableArea);
                Assert.Equal(s.QueryDataBlocks[i].Description, t.QueryDataBlocks[i].Description);
                Assert.Equal(s.QueryDataBlocks[i].Dwell, t.QueryDataBlocks[i].Dwell);
                Assert.Equal(s.QueryDataBlocks[i].Flags, t.QueryDataBlocks[i].Flags);
                Assert.Equal(s.QueryDataBlocks[i].GlobalX, t.QueryDataBlocks[i].GlobalX);
                Assert.Equal(s.QueryDataBlocks[i].GlobalY, t.QueryDataBlocks[i].GlobalY);
                Assert.Equal(s.QueryDataBlocks[i].GlobalZ, t.QueryDataBlocks[i].GlobalZ);
                Assert.Equal(s.QueryDataBlocks[i].Name, t.QueryDataBlocks[i].Name);
                Assert.Equal(s.QueryDataBlocks[i].OwnerID, t.QueryDataBlocks[i].OwnerID);
                Assert.Equal(s.QueryDataBlocks[i].Price, t.QueryDataBlocks[i].Price);
                Assert.Equal(s.QueryDataBlocks[i].ProductSku, t.QueryDataBlocks[i].ProductSku);
                Assert.Equal(s.QueryDataBlocks[i].SimName, t.QueryDataBlocks[i].SimName);
                Assert.Equal(s.QueryDataBlocks[i].SnapShotID, t.QueryDataBlocks[i].SnapShotID);
            }
        }

        [Fact]
        public void DirLandReplyMessage()
        {
            DirLandReplyMessage s = new DirLandReplyMessage();
            s.AgentID = UUID.Random();
            s.QueryID = UUID.Random();
            s.QueryReplies = new DirLandReplyMessage.QueryReply[2];

            DirLandReplyMessage.QueryReply q1 = new DirLandReplyMessage.QueryReply();
            q1.ActualArea = 1024;
            q1.Auction = true;
            q1.ForSale = true;
            q1.Name = "For Sale Parcel Q1";
            q1.ProductSku = "023";
            q1.SalePrice = 2193;
            q1.ParcelID = UUID.Random();

            s.QueryReplies[0] = q1;

            DirLandReplyMessage.QueryReply q2 = new DirLandReplyMessage.QueryReply();
            q2.ActualArea = 512;
            q2.Auction = true;
            q2.ForSale = true;
            q2.Name = "For Sale Parcel Q2";
            q2.ProductSku = "023";
            q2.SalePrice = 22193;
            q2.ParcelID = UUID.Random();

            s.QueryReplies[1] = q2;

            OSDMap map = s.Serialize();

            DirLandReplyMessage t = new DirLandReplyMessage();
            t.Deserialize(map);

            Assert.Equal(s.AgentID, t.AgentID);
            Assert.Equal(s.QueryID, t.QueryID);

            for (int i = 0; i < s.QueryReplies.Length; i++)
            {
                Assert.Equal(s.QueryReplies[i].ActualArea, t.QueryReplies[i].ActualArea);
                Assert.Equal(s.QueryReplies[i].Auction, t.QueryReplies[i].Auction);
                Assert.Equal(s.QueryReplies[i].ForSale, t.QueryReplies[i].ForSale);
                Assert.Equal(s.QueryReplies[i].Name, t.QueryReplies[i].Name);
                Assert.Equal(s.QueryReplies[i].ProductSku, t.QueryReplies[i].ProductSku);
                Assert.Equal(s.QueryReplies[i].ParcelID, t.QueryReplies[i].ParcelID);
                Assert.Equal(s.QueryReplies[i].SalePrice, t.QueryReplies[i].SalePrice);
            }
        }
        #region Performance Testing

        private const int TEST_ITER = 100000;

        [Fact]
        [Trait("Category", "Benchmark")]
        public void ReflectionPerformanceRemoteParcelResponse()
        {
            DateTime messageTestTime = DateTime.UtcNow;
            for (int x = 0; x < TEST_ITER; x++)
            {
                RemoteParcelRequestReply s = new RemoteParcelRequestReply();
                s.ParcelID = UUID.Random();
                OSDMap map = s.Serialize();

                RemoteParcelRequestReply t = new RemoteParcelRequestReply();
                t.Deserialize(map);

                Assert.Equal(s.ParcelID, t.ParcelID);
            }
            TimeSpan duration = DateTime.UtcNow - messageTestTime;
            Console.WriteLine("RemoteParcelRequestReply: OMV Message System Serialization/Deserialization Passes: {0} Total time: {1}", TEST_ITER, duration);

            //BinaryFormatter formatter = new BinaryFormatter();
            DateTime xmlTestTime = DateTime.UtcNow;
            for (int x = 0; x < TEST_ITER; x++)
            {
                RemoteParcelRequestReply s = new RemoteParcelRequestReply();
                s.ParcelID = UUID.Random();

                MemoryStream stream = new MemoryStream();
                byte[] llsdPayload = OSDParser.SerializeLLSDBinary(s.Serialize(), false);

                MessagePackSerializer.Serialize(stream, llsdPayload, BenchmarkMessagePackOptions);
                //formatter.Serialize(stream, s);

                stream.Seek(0, SeekOrigin.Begin);
                //RemoteParcelRequestReply t = (RemoteParcelRequestReply)formatter.Deserialize(stream);
                byte[] unpackedLlsd = MessagePackSerializer.Deserialize<byte[]>(stream, BenchmarkMessagePackOptions);
                RemoteParcelRequestReply t = new RemoteParcelRequestReply();
                t.Deserialize((OSDMap)OSDParser.DeserializeLLSDBinary(unpackedLlsd));

                Assert.Equal(s.ParcelID, t.ParcelID);
            }
            TimeSpan durationxml = DateTime.UtcNow - xmlTestTime;
            Console.WriteLine("RemoteParcelRequestReply: MessagePack(LLSD binary payload) Serialization/Deserialization Passes: {0} Total time: {1}", TEST_ITER, durationxml);
        }


        [Fact]
        [Trait("Category", "Benchmark")]
        public void ReflectionPerformanceDirLandReply()
        {

            DateTime messageTestTime = DateTime.UtcNow;
            for (int x = 0; x < TEST_ITER; x++)
            {
                DirLandReplyMessage s = new DirLandReplyMessage();
                s.AgentID = UUID.Random();
                s.QueryID = UUID.Random();
                s.QueryReplies = new DirLandReplyMessage.QueryReply[2];

                DirLandReplyMessage.QueryReply q1 = new DirLandReplyMessage.QueryReply();
                q1.ActualArea = 1024;
                q1.Auction = true;
                q1.ForSale = true;
                q1.Name = "For Sale Parcel Q1";
                q1.ProductSku = "023";
                q1.SalePrice = 2193;
                q1.ParcelID = UUID.Random();

                s.QueryReplies[0] = q1;

                DirLandReplyMessage.QueryReply q2 = new DirLandReplyMessage.QueryReply();
                q2.ActualArea = 512;
                q2.Auction = true;
                q2.ForSale = true;
                q2.Name = "For Sale Parcel Q2";
                q2.ProductSku = "023";
                q2.SalePrice = 22193;
                q2.ParcelID = UUID.Random();

                s.QueryReplies[1] = q2;

                OSDMap map = s.Serialize();
                DirLandReplyMessage t = new DirLandReplyMessage();

                t.Deserialize(map);
                Assert.Equal(s.AgentID, t.AgentID);
                Assert.Equal(s.QueryID, t.QueryID);

                for (int i = 0; i < s.QueryReplies.Length; i++)
                {
                    Assert.Equal(s.QueryReplies[i].ActualArea, t.QueryReplies[i].ActualArea);
                    Assert.Equal(s.QueryReplies[i].Auction, t.QueryReplies[i].Auction);
                    Assert.Equal(s.QueryReplies[i].ForSale, t.QueryReplies[i].ForSale);
                    Assert.Equal(s.QueryReplies[i].Name, t.QueryReplies[i].Name);
                    Assert.Equal(s.QueryReplies[i].ProductSku, t.QueryReplies[i].ProductSku);
                    Assert.Equal(s.QueryReplies[i].ParcelID, t.QueryReplies[i].ParcelID);
                    Assert.Equal(s.QueryReplies[i].SalePrice, t.QueryReplies[i].SalePrice);
                }
            }
            TimeSpan duration = DateTime.UtcNow - messageTestTime;
            Console.WriteLine("DirLandReplyMessage: OMV Message System Serialization/Deserialization Passes: {0} Total time: {1}", TEST_ITER, duration);

            DateTime xmlTestTime = DateTime.UtcNow;
            for (int x = 0; x < TEST_ITER; x++)
            {
                DirLandReplyMessage s = new DirLandReplyMessage();
                s.AgentID = UUID.Random();
                s.QueryID = UUID.Random();
                s.QueryReplies = new DirLandReplyMessage.QueryReply[2];

                DirLandReplyMessage.QueryReply q1 = new DirLandReplyMessage.QueryReply();
                q1.ActualArea = 1024;
                q1.Auction = true;
                q1.ForSale = true;
                q1.Name = "For Sale Parcel Q1";
                q1.ProductSku = "023";
                q1.SalePrice = 2193;
                q1.ParcelID = UUID.Random();

                s.QueryReplies[0] = q1;

                DirLandReplyMessage.QueryReply q2 = new DirLandReplyMessage.QueryReply();
                q2.ActualArea = 512;
                q2.Auction = true;
                q2.ForSale = true;
                q2.Name = "For Sale Parcel Q2";
                q2.ProductSku = "023";
                q2.SalePrice = 22193;
                q2.ParcelID = UUID.Random();

                s.QueryReplies[1] = q2;

                MemoryStream stream = new MemoryStream();
                byte[] llsdPayload = OSDParser.SerializeLLSDBinary(s.Serialize(), false);

                //formatter.Serialize(stream, s);
                MessagePackSerializer.Serialize(stream, llsdPayload, BenchmarkMessagePackOptions);

                stream.Seek(0, SeekOrigin.Begin);
                //DirLandReplyMessage t = (DirLandReplyMessage)formatter.Deserialize(stream);
                byte[] unpackedLlsd = MessagePackSerializer.Deserialize<byte[]>(stream, BenchmarkMessagePackOptions);
                DirLandReplyMessage t = new DirLandReplyMessage();
                t.Deserialize((OSDMap)OSDParser.DeserializeLLSDBinary(unpackedLlsd));

                Assert.Equal(s.AgentID, t.AgentID);
                Assert.Equal(s.QueryID, t.QueryID);

                for (int i = 0; i < s.QueryReplies.Length; i++)
                {
                    Assert.Equal(s.QueryReplies[i].ActualArea, t.QueryReplies[i].ActualArea);
                    Assert.Equal(s.QueryReplies[i].Auction, t.QueryReplies[i].Auction);
                    Assert.Equal(s.QueryReplies[i].ForSale, t.QueryReplies[i].ForSale);
                    Assert.Equal(s.QueryReplies[i].Name, t.QueryReplies[i].Name);
                    Assert.Equal(s.QueryReplies[i].ProductSku, t.QueryReplies[i].ProductSku);
                    Assert.Equal(s.QueryReplies[i].ParcelID, t.QueryReplies[i].ParcelID);
                    Assert.Equal(s.QueryReplies[i].SalePrice, t.QueryReplies[i].SalePrice);
                }
            }
            TimeSpan durationxml = DateTime.UtcNow - xmlTestTime;
            Console.WriteLine("DirLandReplyMessage: MessagePack(LLSD binary payload) Serialization/Deserialization Passes: {0} Total time: {1}", TEST_ITER, durationxml);
        }

        [Fact]
        [Trait("Category", "Benchmark")]
        public void ReflectionPerformanceDirLandReply2()
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(DirLandReplyMessage));

            DirLandReplyMessage s = new DirLandReplyMessage();
            s.AgentID = UUID.Random();
            s.QueryID = UUID.Random();
            s.QueryReplies = new DirLandReplyMessage.QueryReply[2];

            DirLandReplyMessage.QueryReply q1 = new DirLandReplyMessage.QueryReply();
            q1.ActualArea = 1024;
            q1.Auction = true;
            q1.ForSale = true;
            q1.Name = "For Sale Parcel Q1";
            q1.ProductSku = "023";
            q1.SalePrice = 2193;
            q1.ParcelID = UUID.Random();

            s.QueryReplies[0] = q1;

            DirLandReplyMessage.QueryReply q2 = new DirLandReplyMessage.QueryReply();
            q2.ActualArea = 512;
            q2.Auction = true;
            q2.ForSale = true;
            q2.Name = "For Sale Parcel Q2";
            q2.ProductSku = "023";
            q2.SalePrice = 22193;
            q2.ParcelID = UUID.Random();

            s.QueryReplies[1] = q2;

            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
            timer.Start();
            for (int i = 0; i < TEST_ITER; ++i)
            {
                MemoryStream stream = new MemoryStream();
                OSDMap map = s.Serialize();
                byte[] jsonData = Encoding.UTF8.GetBytes(OSDParser.SerializeJsonString(map));
                stream.Write(jsonData, 0, jsonData.Length);
                stream.Flush();
                stream.Close();
            }
            timer.Stop();
            Console.WriteLine("OMV Message System Serialization/Deserialization Passes: {0} Total time: {1}", TEST_ITER, timer.Elapsed.TotalSeconds);

            timer.Reset();
            timer.Start();
            for (int i = 0; i < TEST_ITER; ++i)
            {
                MemoryStream stream = new MemoryStream();
                xmlSerializer.Serialize(stream, s);
                stream.Flush();
                stream.Close();
            }
            timer.Stop();
            Console.WriteLine(".NET BinarySerialization/Deserialization Passes: {0} Total time: {1}", TEST_ITER, timer.Elapsed.TotalSeconds);
        }

        [Fact]
        [Trait("Category", "Benchmark")]
        public void ReflectionPerformanceParcelProperties()
        {
            DateTime messageTestTime = DateTime.UtcNow;
            for (int x = 0; x < TEST_ITER; x++)
            {
                ParcelPropertiesMessage s = new ParcelPropertiesMessage();
                s.AABBMax = Vector3.Parse("<1,2,3>");
                s.AABBMin = Vector3.Parse("<2,3,1>");
                s.Area = 1024;
                s.AuctionID = uint.MaxValue;
                s.AuthBuyerID = UUID.Random();
                s.Bitmap = Utils.EmptyBytes;
                s.Category = ParcelCategory.Educational;
                s.ClaimDate = new DateTime(2008, 12, 25, 3, 15, 22);
                s.ClaimPrice = 1000;
                s.Desc = "Test Description";
                s.GroupID = UUID.Random();
                s.GroupPrims = 50;
                s.IsGroupOwned = false;
                s.LandingType = LandingType.None;
                s.LocalID = 1;
                s.MaxPrims = 234;
                s.MediaAutoScale = false;
                s.MediaDesc = "Example Media Description";
                s.MediaHeight = 480;
                s.MediaID = UUID.Random();
                s.MediaLoop = false;
                s.MediaType = "text/html";
                s.MediaURL = "http://www.openmetaverse.co";
                s.MediaWidth = 640;
                s.MusicURL = "http://scfire-ntc-aa04.stream.aol.com:80/stream/1075"; // Yee Haw
                s.Name = "Test Name";
                s.ObscureMedia = false;
                s.ObscureMusic = false;
                s.OtherCleanTime = 5;
                s.OtherCount = 200;
                s.OtherPrims = 300;
                s.OwnerID = UUID.Random();
                s.OwnerPrims = 0;
                s.ParcelFlags = ParcelFlags.AllowDamage | ParcelFlags.AllowGroupScripts | ParcelFlags.AllowVoiceChat;
                s.ParcelPrimBonus = 0f;
                s.PassHours = 1.5f;
                s.PassPrice = 10;
                s.PublicCount = 20;
                s.RegionDenyAgeUnverified = false;
                s.RegionDenyAnonymous = false;
                s.RegionPushOverride = true;
                s.RentPrice = 0;
                s.RequestResult = ParcelResult.Single;
                s.SalePrice = 9999;
                s.SelectedPrims = 1;
                s.SelfCount = 2;
                s.SequenceID = -4000;
                s.SimWideMaxPrims = 937;
                s.SimWideTotalPrims = 117;
                s.SnapSelection = false;
                s.SnapshotID = UUID.Random();
                s.Status = ParcelStatus.Leased;
                s.TotalPrims = 219;
                s.UserLocation = Vector3.Parse("<3,4,5>");
                s.UserLookAt = Vector3.Parse("<5,4,3>");

                OSDMap map = s.Serialize();
                ParcelPropertiesMessage t = new ParcelPropertiesMessage();

                t.Deserialize(map);

                Assert.Equal(s.AABBMax, t.AABBMax);
                Assert.Equal(s.AABBMin, t.AABBMin);
                Assert.Equal(s.Area, t.Area);
                Assert.Equal(s.AuctionID, t.AuctionID);
                Assert.Equal(s.AuthBuyerID, t.AuthBuyerID);
                Assert.Equal(s.Bitmap, t.Bitmap);
                Assert.Equal(s.Category, t.Category);
                Assert.Equal(s.ClaimDate, t.ClaimDate);
                Assert.Equal(s.ClaimPrice, t.ClaimPrice);
                Assert.Equal(s.Desc, t.Desc);
                Assert.Equal(s.GroupID, t.GroupID);
                Assert.Equal(s.GroupPrims, t.GroupPrims);
                Assert.Equal(s.IsGroupOwned, t.IsGroupOwned);
                Assert.Equal(s.LandingType, t.LandingType);
                Assert.Equal(s.LocalID, t.LocalID);
                Assert.Equal(s.MaxPrims, t.MaxPrims);
                Assert.Equal(s.MediaAutoScale, t.MediaAutoScale);
                Assert.Equal(s.MediaDesc, t.MediaDesc);
                Assert.Equal(s.MediaHeight, t.MediaHeight);
                Assert.Equal(s.MediaID, t.MediaID);
                Assert.Equal(s.MediaLoop, t.MediaLoop);
                Assert.Equal(s.MediaType, t.MediaType);
                Assert.Equal(s.MediaURL, t.MediaURL);
                Assert.Equal(s.MediaWidth, t.MediaWidth);
                Assert.Equal(s.MusicURL, t.MusicURL);
                Assert.Equal(s.Name, t.Name);
                Assert.Equal(s.ObscureMedia, t.ObscureMedia);
                Assert.Equal(s.ObscureMusic, t.ObscureMusic);
                Assert.Equal(s.OtherCleanTime, t.OtherCleanTime);
                Assert.Equal(s.OtherCount, t.OtherCount);
                Assert.Equal(s.OtherPrims, t.OtherPrims);
                Assert.Equal(s.OwnerID, t.OwnerID);
                Assert.Equal(s.OwnerPrims, t.OwnerPrims);
                Assert.Equal(s.ParcelFlags, t.ParcelFlags);
                Assert.Equal(s.ParcelPrimBonus, t.ParcelPrimBonus);
                Assert.Equal(s.PassHours, t.PassHours);
                Assert.Equal(s.PassPrice, t.PassPrice);
                Assert.Equal(s.PublicCount, t.PublicCount);
                Assert.Equal(s.RegionDenyAgeUnverified, t.RegionDenyAgeUnverified);
                Assert.Equal(s.RegionDenyAnonymous, t.RegionDenyAnonymous);
                Assert.Equal(s.RegionPushOverride, t.RegionPushOverride);
                Assert.Equal(s.RentPrice, t.RentPrice);
                Assert.Equal(s.RequestResult, t.RequestResult);
                Assert.Equal(s.SalePrice, t.SalePrice);
                Assert.Equal(s.SelectedPrims, t.SelectedPrims);
                Assert.Equal(s.SelfCount, t.SelfCount);
                Assert.Equal(s.SequenceID, t.SequenceID);
                Assert.Equal(s.SimWideMaxPrims, t.SimWideMaxPrims);
                Assert.Equal(s.SimWideTotalPrims, t.SimWideTotalPrims);
                Assert.Equal(s.SnapSelection, t.SnapSelection);
                Assert.Equal(s.SnapshotID, t.SnapshotID);
                Assert.Equal(s.Status, t.Status);
                Assert.Equal(s.TotalPrims, t.TotalPrims);
                Assert.Equal(s.UserLocation, t.UserLocation);
                Assert.Equal(s.UserLookAt, t.UserLookAt);
            }
            TimeSpan duration = DateTime.UtcNow - messageTestTime;
            Console.WriteLine("ParcelPropertiesMessage: OMV Message System Serialization/Deserialization Passes: {0} Total time: {1}", TEST_ITER, duration);

            DateTime xmlTestTime = DateTime.UtcNow;
            for (int x = 0; x < TEST_ITER; x++)
            {

                ParcelPropertiesMessage s = new ParcelPropertiesMessage();
                s.AABBMax = Vector3.Parse("<1,2,3>");
                s.AABBMin = Vector3.Parse("<2,3,1>");
                s.Area = 1024;
                s.AuctionID = uint.MaxValue;
                s.AuthBuyerID = UUID.Random();
                s.Bitmap = Utils.EmptyBytes;
                s.Category = ParcelCategory.Educational;
                s.ClaimDate = new DateTime(2008, 12, 25, 3, 15, 22);
                s.ClaimPrice = 1000;
                s.Desc = "Test Description";
                s.GroupID = UUID.Random();
                s.GroupPrims = 50;
                s.IsGroupOwned = false;
                s.LandingType = LandingType.None;
                s.LocalID = 1;
                s.MaxPrims = 234;
                s.MediaAutoScale = false;
                s.MediaDesc = "Example Media Description";
                s.MediaHeight = 480;
                s.MediaID = UUID.Random();
                s.MediaLoop = false;
                s.MediaType = "text/html";
                s.MediaURL = "http://www.openmetaverse.co";
                s.MediaWidth = 640;
                s.MusicURL = "http://scfire-ntc-aa04.stream.aol.com:80/stream/1075"; // Yee Haw
                s.Name = "Test Name";
                s.ObscureMedia = false;
                s.ObscureMusic = false;
                s.OtherCleanTime = 5;
                s.OtherCount = 200;
                s.OtherPrims = 300;
                s.OwnerID = UUID.Random();
                s.OwnerPrims = 0;
                s.ParcelFlags = ParcelFlags.AllowDamage | ParcelFlags.AllowGroupScripts | ParcelFlags.AllowVoiceChat;
                s.ParcelPrimBonus = 0f;
                s.PassHours = 1.5f;
                s.PassPrice = 10;
                s.PublicCount = 20;
                s.RegionDenyAgeUnverified = false;
                s.RegionDenyAnonymous = false;
                s.RegionPushOverride = true;
                s.RentPrice = 0;
                s.RequestResult = ParcelResult.Single;
                s.SalePrice = 9999;
                s.SelectedPrims = 1;
                s.SelfCount = 2;
                s.SequenceID = -4000;
                s.SimWideMaxPrims = 937;
                s.SimWideTotalPrims = 117;
                s.SnapSelection = false;
                s.SnapshotID = UUID.Random();
                s.Status = ParcelStatus.Leased;
                s.TotalPrims = 219;
                s.UserLocation = Vector3.Parse("<3,4,5>");
                s.UserLookAt = Vector3.Parse("<5,4,3>");

                MemoryStream stream = new MemoryStream();
                byte[] llsdPayload = OSDParser.SerializeLLSDBinary(s.Serialize(), false);

                //formatter.Serialize(stream, s);
                MessagePackSerializer.Serialize(stream, llsdPayload, BenchmarkMessagePackOptions);

                stream.Seek(0, SeekOrigin.Begin);

                //ParcelPropertiesMessage t = (ParcelPropertiesMessage)formatter.Deserialize(stream);
                byte[] unpackedLlsd = MessagePackSerializer.Deserialize<byte[]>(stream, BenchmarkMessagePackOptions);
                ParcelPropertiesMessage t = new ParcelPropertiesMessage();
                t.Deserialize((OSDMap)OSDParser.DeserializeLLSDBinary(unpackedLlsd));

                Assert.Equal(s.AABBMax, t.AABBMax);
                Assert.Equal(s.AABBMin, t.AABBMin);
                Assert.Equal(s.Area, t.Area);
                Assert.Equal(s.AuctionID, t.AuctionID);
                Assert.Equal(s.AuthBuyerID, t.AuthBuyerID);
                Assert.Equal(s.Bitmap, t.Bitmap);
                Assert.Equal(s.Category, t.Category);
                Assert.Equal(s.ClaimDate, t.ClaimDate);
                Assert.Equal(s.ClaimPrice, t.ClaimPrice);
                Assert.Equal(s.Desc, t.Desc);
                Assert.Equal(s.GroupID, t.GroupID);
                Assert.Equal(s.GroupPrims, t.GroupPrims);
                Assert.Equal(s.IsGroupOwned, t.IsGroupOwned);
                Assert.Equal(s.LandingType, t.LandingType);
                Assert.Equal(s.LocalID, t.LocalID);
                Assert.Equal(s.MaxPrims, t.MaxPrims);
                Assert.Equal(s.MediaAutoScale, t.MediaAutoScale);
                Assert.Equal(s.MediaDesc, t.MediaDesc);
                Assert.Equal(s.MediaHeight, t.MediaHeight);
                Assert.Equal(s.MediaID, t.MediaID);
                Assert.Equal(s.MediaLoop, t.MediaLoop);
                Assert.Equal(s.MediaType, t.MediaType);
                Assert.Equal(s.MediaURL, t.MediaURL);
                Assert.Equal(s.MediaWidth, t.MediaWidth);
                Assert.Equal(s.MusicURL, t.MusicURL);
                Assert.Equal(s.Name, t.Name);
                Assert.Equal(s.ObscureMedia, t.ObscureMedia);
                Assert.Equal(s.ObscureMusic, t.ObscureMusic);
                Assert.Equal(s.OtherCleanTime, t.OtherCleanTime);
                Assert.Equal(s.OtherCount, t.OtherCount);
                Assert.Equal(s.OtherPrims, t.OtherPrims);
                Assert.Equal(s.OwnerID, t.OwnerID);
                Assert.Equal(s.OwnerPrims, t.OwnerPrims);
                Assert.Equal(s.ParcelFlags, t.ParcelFlags);
                Assert.Equal(s.ParcelPrimBonus, t.ParcelPrimBonus);
                Assert.Equal(s.PassHours, t.PassHours);
                Assert.Equal(s.PassPrice, t.PassPrice);
                Assert.Equal(s.PublicCount, t.PublicCount);
                Assert.Equal(s.RegionDenyAgeUnverified, t.RegionDenyAgeUnverified);
                Assert.Equal(s.RegionDenyAnonymous, t.RegionDenyAnonymous);
                Assert.Equal(s.RegionPushOverride, t.RegionPushOverride);
                Assert.Equal(s.RentPrice, t.RentPrice);
                Assert.Equal(s.RequestResult, t.RequestResult);
                Assert.Equal(s.SalePrice, t.SalePrice);
                Assert.Equal(s.SelectedPrims, t.SelectedPrims);
                Assert.Equal(s.SelfCount, t.SelfCount);
                Assert.Equal(s.SequenceID, t.SequenceID);
                Assert.Equal(s.SimWideMaxPrims, t.SimWideMaxPrims);
                Assert.Equal(s.SimWideTotalPrims, t.SimWideTotalPrims);
                Assert.Equal(s.SnapSelection, t.SnapSelection);
                Assert.Equal(s.SnapshotID, t.SnapshotID);
                Assert.Equal(s.Status, t.Status);
                Assert.Equal(s.TotalPrims, t.TotalPrims);
                Assert.Equal(s.UserLocation, t.UserLocation);
                Assert.Equal(s.UserLookAt, t.UserLookAt);
            }
            TimeSpan durationxml = DateTime.UtcNow - xmlTestTime;
            Console.WriteLine("ParcelPropertiesMessage: MessagePack(LLSD binary payload) Serialization/Deserialization Passes: {0} Total time: {1}", TEST_ITER, durationxml);
        }

        #endregion
    }
}

