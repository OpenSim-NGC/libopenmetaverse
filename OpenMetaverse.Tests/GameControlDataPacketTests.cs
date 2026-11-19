/*
 * Copyright (c) 2026 Legion Builds
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Neither the name of the copyright holder nor the names
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
using Xunit;
using OpenMetaverse.Packets;

namespace OpenMetaverse.Tests
{
    /// <summary>
    /// GameControlData: High 33, NotTrusted, Zerocoded (Linden Lab message template, viewer release/26.5.1).
    /// AgentData{AgentID, SessionID, Packet U8, ActionMode U8}, then four Variable blocks:
    /// Axes{Value S16}, Buttons{Flags U32}, ModeAxes{Value S16}, ModeButtons{Flags U32}.
    /// </summary>
    public class GameControlDataPacketTests
    {
        private const int GameControlDataPacketType = 0x30000 + 33; // PacketFrequency.High marker + message number

        private static readonly UUID AgentId = new UUID("11111111-2222-3333-4444-555555555555");
        private static readonly UUID SessionId = new UUID("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");

        private static GameControlDataPacket Sample()
        {
            var p = new GameControlDataPacket();
            p.Header.Zerocoded = false;
            p.AgentData.AgentID = AgentId;
            p.AgentData.SessionID = SessionId;
            p.AgentData.Packet = 200;
            p.AgentData.ActionMode = 2;
            p.Axes = new[]
            {
                new GameControlDataPacket.AxesBlock { Value = 32767 },
                new GameControlDataPacket.AxesBlock { Value = -32768 },
                new GameControlDataPacket.AxesBlock { Value = 1 },
            };
            p.Buttons = new[] { new GameControlDataPacket.ButtonsBlock { Flags = 0x80000801u } };
            p.ModeAxes = new[]
            {
                new GameControlDataPacket.ModeAxesBlock { Value = -2 },
                new GameControlDataPacket.ModeAxesBlock { Value = 300 },
            };
            p.ModeButtons = new[] { new GameControlDataPacket.ModeButtonsBlock { Flags = 0x00000004u } };
            return p;
        }

        [Fact]
        public void Identity_IsHighFrequency33AndZerocoded()
        {
            var p = new GameControlDataPacket();

            Assert.Equal("GameControlData", p.Type.ToString());
            Assert.Equal(GameControlDataPacketType, (int)p.Type);
            Assert.Equal(PacketFrequency.High, p.Header.Frequency);
            Assert.Equal(33, p.Header.ID);
            Assert.True(p.Header.Zerocoded);
        }

        [Fact]
        public void BuildPacketByType_ReturnsGameControlDataPacket()
        {
            Packet built = Packet.BuildPacket((PacketType)GameControlDataPacketType);

            Assert.IsType<GameControlDataPacket>(built);
        }

        [Fact]
        public void Wire_MatchesTemplateLayout()
        {
            byte[] bytes = Sample().ToBytes();

            // header (flags, 4-byte sequence, extra-header length) = 6 bytes, then the one-byte High id
            Assert.Equal(6 + 1 + 34 + (1 + 6) + (1 + 4) + (1 + 4) + (1 + 4), bytes.Length);
            Assert.Equal(33, bytes[6]);

            int i = 7;
            Assert.Equal(AgentId, new UUID(bytes, i)); i += 16;
            Assert.Equal(SessionId, new UUID(bytes, i)); i += 16;
            Assert.Equal(200, bytes[i++]);                       // Packet
            Assert.Equal(2, bytes[i++]);                         // ActionMode

            Assert.Equal(3, bytes[i++]);                         // Axes count
            Assert.Equal(new byte[] { 0xFF, 0x7F, 0x00, 0x80, 0x01, 0x00 }, Slice(bytes, i, 6)); i += 6;

            Assert.Equal(1, bytes[i++]);                         // Buttons count
            Assert.Equal(new byte[] { 0x01, 0x08, 0x00, 0x80 }, Slice(bytes, i, 4)); i += 4;

            Assert.Equal(2, bytes[i++]);                         // ModeAxes count
            Assert.Equal(new byte[] { 0xFE, 0xFF, 0x2C, 0x01 }, Slice(bytes, i, 4)); i += 4;

            Assert.Equal(1, bytes[i++]);                         // ModeButtons count
            Assert.Equal(new byte[] { 0x04, 0x00, 0x00, 0x00 }, Slice(bytes, i, 4)); i += 4;

            Assert.Equal(bytes.Length, i);
        }

        [Fact]
        public void RoundTrip_PreservesEveryField()
        {
            byte[] bytes = Sample().ToBytes();
            Assert.True(bytes.Length > 40, "encoded packet is empty");

            int len = bytes.Length;
            var back = Assert.IsType<GameControlDataPacket>(Packet.BuildPacket(bytes, ref len, new byte[4096]));

            Assert.Equal(AgentId, back.AgentData.AgentID);
            Assert.Equal(SessionId, back.AgentData.SessionID);
            Assert.Equal(200, back.AgentData.Packet);
            Assert.Equal(2, back.AgentData.ActionMode);
            Assert.Equal(new short[] { 32767, -32768, 1 }, Array.ConvertAll(back.Axes, a => a.Value));
            Assert.Equal(new[] { 0x80000801u }, Array.ConvertAll(back.Buttons, b => b.Flags));
            Assert.Equal(new short[] { -2, 300 }, Array.ConvertAll(back.ModeAxes, a => a.Value));
            Assert.Equal(new[] { 4u }, Array.ConvertAll(back.ModeButtons, b => b.Flags));
        }

        [Fact]
        public void RoundTrip_ZeroEncoded_PreservesEveryField()
        {
            GameControlDataPacket p = Sample();
            p.Header.Zerocoded = true;
            p.AgentData.SessionID = UUID.Zero;       // long zero runs so zero coding actually shortens it
            p.Axes = new[] { new GameControlDataPacket.AxesBlock { Value = 0 }, new GameControlDataPacket.AxesBlock { Value = 0 } };

            byte[] raw = p.ToBytes();
            byte[] wire = new byte[4096];
            int len = Helpers.ZeroEncode(raw, raw.Length, wire);

            Assert.True(len > 0 && len < raw.Length, "zero coding did not shorten the packet");

            var back = Assert.IsType<GameControlDataPacket>(Packet.BuildPacket(wire, ref len, new byte[4096]));
            Assert.Equal(AgentId, back.AgentData.AgentID);
            Assert.Equal(UUID.Zero, back.AgentData.SessionID);
            Assert.Equal(new short[] { 0, 0 }, Array.ConvertAll(back.Axes, a => a.Value));
            Assert.Equal(new short[] { -2, 300 }, Array.ConvertAll(back.ModeAxes, a => a.Value));
        }

        [Fact]
        public void EmptyVariableBlocks_EncodeAsCountZeroAndDecodeAsEmpty()
        {
            var p = new GameControlDataPacket();
            p.Header.Zerocoded = false;
            p.AgentData.AgentID = AgentId;
            p.AgentData.SessionID = SessionId;
            p.Axes = new GameControlDataPacket.AxesBlock[0];
            p.Buttons = new GameControlDataPacket.ButtonsBlock[0];
            p.ModeAxes = new GameControlDataPacket.ModeAxesBlock[0];
            p.ModeButtons = new GameControlDataPacket.ModeButtonsBlock[0];

            byte[] bytes = p.ToBytes();
            Assert.Equal(6 + 1 + 34 + 4, bytes.Length);

            int len = bytes.Length;
            var back = Assert.IsType<GameControlDataPacket>(Packet.BuildPacket(bytes, ref len, new byte[4096]));
            Assert.Empty(back.Axes);
            Assert.Empty(back.Buttons);
            Assert.Empty(back.ModeAxes);
            Assert.Empty(back.ModeButtons);
        }

        private static byte[] Slice(byte[] source, int start, int count)
        {
            byte[] result = new byte[count];
            Buffer.BlockCopy(source, start, result, 0, count);
            return result;
        }
    }
}
