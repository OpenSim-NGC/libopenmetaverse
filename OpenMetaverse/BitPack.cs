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

namespace OpenMetaverse
{
    /// <summary>
    /// Wrapper around a byte array that allows bit to be packed and unpacked
    /// one at a time or by a variable amount. Useful for very tightly packed
    /// data like LayerData packets
    /// </summary>
    public class BitPack
    {
        private const int MAX_BITS = 8;
        private readonly bool weAreBigEndian = !BitConverter.IsLittleEndian;

        /// <summary></summary>
        public byte[] Data;

        /// <summary></summary>
        private int bytePos;
        public int BytePos
        {
            get
            {
                if (bytePos != 0 && bitPos == 0)
                    return bytePos - 1;
                else
                    return bytePos;
            }
        }

        /// <summary></summary>
        private int bitPos;
        public int BitPos { get { return bitPos; } }

        /// <summary>
        /// Default constructor, initialize the bit packer / bit unpacker
        /// with a byte array and starting position
        /// </summary>
        /// <param name="data">Byte array to pack bits in to or unpack from</param>
        /// <param name="pos">Starting position in the byte array</param>
        /// <param name="bitp">Optional bit position to start apendig more bits</param>
        public BitPack(byte[] data, int pos, int? bitp = null)
        {
            Data = data;
            bytePos = pos;
            if(bitp.HasValue)
            {
                bitPos = bitp.Value;
                if(bitPos < 0)
                    bitPos = 0;
                else if(bitPos > 7)
                    bitPos = 7; // this is wrong anyway
                if (bitPos == 0)
                    Data[pos] = 0;
                else
                  Data[pos] &= (byte)~(0xff >> bitPos);
            }
            else
                bitPos = 0;
        }

        /// <summary>
        /// Pack a floating point value in to the data
        /// </summary>
        /// <param name="data">Floating point value to pack</param>
        public unsafe void PackFloat(float data)
        {
            int d = (*(int*)&data);
            PackBitsFromByte((byte)d);
            PackBitsFromByte((byte)(d >> 8));
            PackBitsFromByte((byte)(d >> 16));
            PackBitsFromByte((byte)(d >> 24));
        }

        /// <summary>
        /// Pack part or all of an integer in to the data
        /// </summary>
        /// <param name="data">Integer containing the data to pack</param>
        /// <param name="totalCount">Number of bits of the integer to pack</param>
        public void PackBits(int data, int totalCount)
        {
            while (totalCount > 8)
            {
                PackBitsFromByte((byte)data);
                data >>= 8;
                totalCount -= 8;
            }
            if (totalCount > 0)
                PackBitsFromByte((byte)data, totalCount);
        }

        /// <summary>
        /// Pack part or all of an unsigned integer in to the data
        /// </summary>
        /// <param name="data">Unsigned integer containing the data to pack</param>
        /// <param name="totalCount">Number of bits of the integer to pack</param>
        public void PackBits(uint data, int totalCount)
        {
            while (totalCount > 8)
            {
                PackBitsFromByte((byte)data);
                data >>= 8;
                totalCount -= 8;
            }
            if (totalCount > 0)
                PackBitsFromByte((byte)data, totalCount);
        }

        public void PackBitsFromUInt(uint data)
        {
            PackBitsFromByte((byte)data);
            PackBitsFromByte((byte)(data >> 8));
            PackBitsFromByte((byte)(data >> 16));
            PackBitsFromByte((byte)(data >> 24));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="isSigned"></param>
        /// <param name="intBits"></param>
        /// <param name="fracBits"></param>
        public void PackFixed(float data, bool isSigned, int intBits, int fracBits)
        {
            int totalBits = intBits + fracBits;
            int max = 1 << intBits;

            if (isSigned)
            {
                totalBits++;
                data += max;
                max += max;
            }

            if (totalBits > 32)
                throw new Exception("Can't use fixed point packing for " + totalBits);

            int v;
            if(data <= 1e-6f)
                v = 0;
            else
            {
                if(data > max)
                    data = max;
                data *= 1 << fracBits;
                v = (int)data;
            }

            PackBitsFromByte((byte)v);
            if(totalBits > 8)
            {
                PackBitsFromByte((byte)(v >> 8));
                if (totalBits > 16)
                {
                    PackBitsFromByte((byte)(v >> 16));
                    PackBitsFromByte((byte)(v >> 24));
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public void PackUUID(UUID data)
        {
            byte[] bytes = data.GetBytes();
            for (int i = 0; i < 16; i++)
                PackBitsFromByte(bytes[i]);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public void PackColor(Color4 data)
        {
            PackBitsFromByte(Utils.FloatToByte(data.R, 0f, 1f));
            PackBitsFromByte(Utils.FloatToByte(data.G, 0f, 1f));
            PackBitsFromByte(Utils.FloatToByte(data.B, 0f, 1f));
            PackBitsFromByte(Utils.FloatToByte(data.A, 0f, 1f));
        }

        /// <summary>
        /// Unpacking a floating point value from the data
        /// </summary>
        /// <returns>Unpacked floating point value</returns>
        public float UnpackFloat()
        {
            byte[] output = UnpackBitsArray(32);

            if (weAreBigEndian) Array.Reverse(output);
            return BitConverter.ToSingle(output, 0);
        }

        /// <summary>
        /// Unpack a variable number of bits from the data in to integer format
        /// </summary>
        /// <param name="totalCount">Number of bits to unpack</param>
        /// <returns>An integer containing the unpacked bits</returns>
        /// <remarks>This function is only useful up to 32 bits</remarks>
        public int UnpackBits(int totalCount)
        {
            byte[] output = UnpackBitsArray(totalCount);

            if (weAreBigEndian) Array.Reverse(output);
            return BitConverter.ToInt32(output, 0);
        }

        /// <summary>
        /// Unpack a variable number of bits from the data in to unsigned 
        /// integer format
        /// </summary>
        /// <param name="totalCount">Number of bits to unpack</param>
        /// <returns>An unsigned integer containing the unpacked bits</returns>
        /// <remarks>This function is only useful up to 32 bits</remarks>
        public uint UnpackUBits(int totalCount)
        {
            byte[] output = UnpackBitsArray(totalCount);

            if (weAreBigEndian) Array.Reverse(output);
            return BitConverter.ToUInt32(output, 0);
        }

        /// <summary>
        /// Unpack a 16-bit signed integer
        /// </summary>
        /// <returns>16-bit signed integer</returns>
        public short UnpackShort()
        {
            return (short)UnpackBits(16);
        }

        /// <summary>
        /// Unpack a 16-bit unsigned integer
        /// </summary>
        /// <returns>16-bit unsigned integer</returns>
        public ushort UnpackUShort()
        {
            return (ushort)UnpackUBits(16);
        }

        /// <summary>
        /// Unpack a 32-bit signed integer
        /// </summary>
        /// <returns>32-bit signed integer</returns>
        public int UnpackInt()
        {
            return (int)UnpackUInt();
        }

        /// <summary>
        /// Unpack a 32-bit unsigned integer
        /// </summary>
        /// <returns>32-bit unsigned integer</returns>
        public uint UnpackUInt()
        {
            uint tmp = UnpackByte();
            tmp |= (byte)(UnpackByte() << 8);
            tmp |= (byte)(UnpackByte() << 16);
            tmp |= (byte)(UnpackByte() << 24);
            return tmp;
        }

        public byte UnpackByte()
        {
            byte o = Data[bytePos];
            if (bitPos == 0 || o == 0)
            {
                ++bytePos;
                return o;
            }

            o <<= bitPos;
            ++bytePos;
            o |= (byte)(Data[bytePos] >> (8 - bitPos));
            return o;
        }

        public float UnpackFixed(bool signed, int intBits, int fracBits)
        {
            int totalBits = intBits + fracBits;
            if (signed)
                totalBits++;

            int intVal = UnpackByte();
            if (totalBits > 8)
            {
                intVal |= (UnpackByte() << 8);
                if (totalBits > 16)
                {
                    intVal |= (UnpackByte() << 16);
                    intVal |= (UnpackByte() << 24);
                }
            }

            if(intVal == 0)
                return 0f;

            float fixedVal = intVal;
            fixedVal /= (1 << fracBits);

            if (signed) fixedVal -= (1 << intBits);

            return fixedVal;
        }

        public string UnpackString(int size)
        {
            if (bitPos != 0 || bytePos + size > Data.Length) throw new IndexOutOfRangeException();

            string str = System.Text.UTF8Encoding.UTF8.GetString(Data, bytePos, size);
            bytePos += size;
            return str;
        }

        public UUID UnpackUUID()
        {
            if (bitPos != 0) throw new IndexOutOfRangeException();

            UUID val = new UUID(Data, bytePos);
            bytePos += 16;
            return val;
        }

        private void PackBitArray(byte[] data, int totalCount)
        {
            int count = 0;
            while(totalCount > 8)
            {
                PackBitsFromByte(data[count]);
                ++count;
                totalCount -= 8;
            }
            if(totalCount > 0)
                PackBitsFromByte(data[count], totalCount);
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void PackBitsFromByte(byte inbyte)
        {
            if(bitPos == 0)
            {
                Data[bytePos++] = inbyte;
                if (bytePos < Data.Length)
                    Data[bytePos] = 0;
                return;
            }
            if (inbyte == 0)
            {
                ++bytePos;
                Data[bytePos] = 0;
                return;
            }
            Data[bytePos++] |= (byte)(inbyte >> bitPos);
            Data[bytePos] = (byte)(inbyte << (8 - bitPos));
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void PackBitsFromByte(byte inbyte, int count)
        {
            if (count > 8) //should not happen
                count = 7;
            else
                count = count - 1;

            while (count >= 0)
            {
                if ((inbyte & (0x01 << count)) != 0)
                    Data[bytePos] |= (byte)(0x80 >> bitPos);

                --count;
                ++bitPos;

                if (bitPos >= MAX_BITS)
                {
                    bitPos = 0;
                    ++bytePos;
                    if (bytePos < Data.Length)
                        Data[bytePos] = 0;
                }
            }
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void PackBitFromBit(byte inbyte)
        {
            if ((inbyte & 0x01) != 0)
                Data[bytePos] |= (byte)(0x80 >> bitPos);

            ++bitPos;

            if (bitPos >= MAX_BITS)
            {
                bitPos = 0;
                ++bytePos;
                if (bytePos < Data.Length)
                    Data[bytePos] = 0;
            }
        }

        /// <summary>
        /// Pack a single bit in to the data
        /// </summary>
        /// <param name="bit">Bit to pack</param>

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void PackBit(bool bit)
        {
            if (bit)
                Data[bytePos] |= (byte)(0x80 >> bitPos);

            ++bitPos;

            if (bitPos >= MAX_BITS)
            {
                bitPos = 0;
                ++bytePos;
                if (bytePos < Data.Length)
                    Data[bytePos] = 0;
            }
        }

        private byte[] UnpackBitsArray(int totalCount)
        {
            int count = 0;
            byte[] output = new byte[4];
            int curBytePos = 0;
            int curBitPos = 0;

            while (totalCount > 0)
            {
                if (totalCount > MAX_BITS)
                {
                    count = MAX_BITS;
                    totalCount -= MAX_BITS;
                }
                else
                {
                    count = totalCount;
                    totalCount = 0;
                }

                while (count > 0)
                {
                    // Shift the previous bits
                    output[curBytePos] <<= 1;

                    // Grab one bit
                    if ((Data[bytePos] & (0x80 >> bitPos++)) != 0)
                        ++output[curBytePos];

                    --count;
                    ++curBitPos;

                    if (bitPos >= MAX_BITS)
                    {
                        bitPos = 0;
                        ++bytePos;
                    }
                    if (curBitPos >= MAX_BITS)
                    {
                        curBitPos = 0;
                        ++curBytePos;
                    }
                }
            }

            return output;
        }
    }
}
