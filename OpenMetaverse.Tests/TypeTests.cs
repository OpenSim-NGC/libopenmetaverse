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
using OpenMetaverse.StructuredData;
using System;
using System.Collections.Generic;

namespace OpenMetaverse.Tests
{
    
    public class TypeTests
    {
        [Fact]
        public void UUIDs()
        {
            // Creation
            UUID a = new UUID();
            byte[] bytes = a.GetBytes();
            for (int i = 0; i < 16; i++)
                Assert.True(bytes[i] == 0x00);

            // Comparison
            a = new UUID(new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A,
                0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0xFF, 0xFF }, 0);
            UUID b = new UUID(new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A,
                0x0B, 0x0C, 0x0D, 0x0E, 0x0F }, 0);

            Assert.True(a == b, "UUID comparison operator failed, " + a.ToString() + " should equal " +
                b.ToString());

            // From string
            a = new UUID(new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A,
                0x0B, 0x0C, 0x0D, 0x0E, 0x0F }, 0);
            string zeroonetwo = "00010203-0405-0607-0809-0a0b0c0d0e0f";
            b = new UUID(zeroonetwo);

            Assert.True(a == b, "UUID hyphenated string constructor failed, should have " + a.ToString() +
                " but we got " + b.ToString());

            // ToString()            
            Assert.True(a == b);
            Assert.True(a == (UUID)zeroonetwo);

            // TODO: CRC test
        }

        [Fact]
        public void Vector3ApproxEquals()
        {
            Vector3 a = new Vector3(1f, 0f, 0f);
            Vector3 b = new Vector3(0f, 0f, 0f);

            Assert.False(a.ApproxEquals(b, 0.9f), "ApproxEquals failed (1)");
            Assert.True(a.ApproxEquals(b, 1.0f), "ApproxEquals failed (2)");

            a = new Vector3(-1f, 0f, 0f);
            b = new Vector3(1f, 0f, 0f);

            Assert.False(a.ApproxEquals(b, 1.9f), "ApproxEquals failed (3)");
            Assert.True(a.ApproxEquals(b, 2.0f), "ApproxEquals failed (4)");

            a = new Vector3(0f, -1f, 0f);
            b = new Vector3(0f, -1.1f, 0f);

            Assert.False(a.ApproxEquals(b, 0.09f), "ApproxEquals failed (5)");
            Assert.True(a.ApproxEquals(b, 0.11f), "ApproxEquals failed (6)");

            a = new Vector3(0f, 0f, 0.00001f);
            b = new Vector3(0f, 0f, 0f);

            Assert.False(b.ApproxEquals(a, Single.Epsilon), "ApproxEquals failed (6)");
            Assert.True(b.ApproxEquals(a, 0.0001f), "ApproxEquals failed (7)");
        }

        [Fact]
        public void VectorCasting()
        {
            Dictionary<string, double> testNumbers;
            testNumbers = new Dictionary<string, double>();
            testNumbers["1.0"] = 1.0;
            testNumbers["1.1"] = 1.1;
            testNumbers["1.01"] = 1.01;
            testNumbers["1.001"] = 1.001;
            testNumbers["1.0001"] = 1.0001;
            testNumbers["1.00001"] = 1.00001;
            testNumbers["1.000001"] = 1.000001;
            testNumbers["1.0000001"] = 1.0000001;
            testNumbers["1.00000001"] = 1.00000001;

            foreach (KeyValuePair<string, double> kvp in testNumbers)
            {
                double testNumber = kvp.Value;
                double testNumber2 = (double)((float)testNumber);
                bool noPrecisionLoss = testNumber == testNumber2;

                Vector3 a = new Vector3(
                        (float)testNumber,
                        (float)testNumber, (float)testNumber);
                Vector3d b = new Vector3d(testNumber, testNumber, testNumber);

                Vector3 c = (Vector3)b;
                Vector3d d = a;

                if (noPrecisionLoss)
                {
                    Console.Error.WriteLine("Unsuitable test value used-" +
                            " test number should have precision loss when" +
                            " cast to float ({0}).", kvp.Key);
                }
                else
                {
                    Assert.False(a == b, string.Format(
                            "Vector casting failed, precision loss should" +
                            " have occurred. " +
                            "{0}: {1}, {2}", kvp.Key, a.X, b.X));
                    Assert.False(b == d, string.Format(
                            "Vector casting failed, explicit cast of double" +
                            " to float should result in precision loss" +
                            " whichwas should not magically disappear when" +
                            " Vector3 is implicitly cast to Vector3d." +
                            " {0}: {1}, {2}", kvp.Key, b.X, d.X));
                }
                Assert.True(a == c, string.Format(
                        "Vector casting failed, Vector3 compared to" +
                        " explicit cast of Vector3d to Vector3 should" +
                        " result in identical precision loss." +
                        " {0}: {1}, {2}", kvp.Key, a.X, c.X));
                Assert.True(a == d, string.Format(
                        "Vector casting failed, implicit cast of Vector3" +
                        " to Vector3d should not result in precision loss." +
                        " {0}: {1}, {2}", kvp.Key, a.X, d.X));
            }
        }

        [Fact]
        public void Quaternions()
        {
            Quaternion a = new Quaternion(1, 0, 0, 0);
            Quaternion b = new Quaternion(1, 0, 0, 0);

            Assert.True(a == b, "Quaternion comparison operator failed");

            Quaternion expected = new Quaternion(0, 0, 0, -1);
            Quaternion result = a * b;

            Assert.True(result == expected, a.ToString() + " * " + b.ToString() + " produced " + result.ToString() +
                " instead of " + expected.ToString());

            a = new Quaternion(1, 0, 0, 0);
            b = new Quaternion(0, 1, 0, 0);
            expected = new Quaternion(0, 0, 1, 0);
            result = a * b;

            Assert.True(result == expected, a.ToString() + " * " + b.ToString() + " produced " + result.ToString() +
                " instead of " + expected.ToString());

            a = new Quaternion(0, 0, 1, 0);
            b = new Quaternion(0, 1, 0, 0);
            expected = new Quaternion(-1, 0, 0, 0);
            result = a * b;

            Assert.True(result == expected, a.ToString() + " * " + b.ToString() + " produced " + result.ToString() +
                " instead of " + expected.ToString());
        }

        [Fact]
        public void testMatrix()
        {
            Matrix4 matrix = new Matrix4(0, 0, 74, 1,
                                         0, 435, 0, 1,
                                         345, 0, 34, 1,
                                         0, 0, 0, 0);

            /* determinant of singular matrix returns zero */
            Assert.InRange((double)matrix.Determinant(), -0.001d, 0.001d);

            /* inverse of identity matrix is the identity matrix */
            Assert.True(Matrix4.Identity == Matrix4.Inverse(Matrix4.Identity));

            /* inverse of non-singular matrix returns True And InverseMatrix */
            matrix = new Matrix4(1, 1, 0, 0,
                                 1, 1, 1, 0,
                                 0, 1, 1, 0,
                                 0, 0, 0, 1);
            Matrix4 expectedInverse = new Matrix4(0, 1, -1, 0,
                                                  1, -1, 1, 0,
                                                 -1, 1, 0, 0,
                                                  0, 0, 0, 1);
            Assert.Equal(expectedInverse, Matrix4.Inverse(matrix));
        }

        //[Fact]
        //public void VectorQuaternionMath()
        //{
        //    // Convert a vector to a quaternion and back
        //    Vector3 a = new Vector3(1f, 0.5f, 0.75f);
        //    Quaternion b = a.ToQuaternion();
        //    Vector3 c;
        //    b.GetEulerAngles(out c.X, out c.Y, out c.Z);

        //    Assert.True(a == c, c.ToString() + " does not equal " + a.ToString());
        //}

        [Fact]
        public void FloatsToTerseStrings()
        {
            float f = 1.20f;
            string a = String.Empty;
            string b = "1.2";

            a = Helpers.FloatToTerseString(f);
            Assert.True(a == b, f.ToString() + " converted to " + a + ", expecting " + b);

            f = 24.00f;
            b = "24";

            a = Helpers.FloatToTerseString(f);
            Assert.True(a == b, f.ToString() + " converted to " + a + ", expecting " + b);

            f = -0.59f;
            b = "-.59";

            a = Helpers.FloatToTerseString(f);
            Assert.True(a == b, f.ToString() + " converted to " + a + ", expecting " + b);

            f = 0.59f;
            b = ".59";

            a = Helpers.FloatToTerseString(f);
            Assert.True(a == b, f.ToString() + " converted to " + a + ", expecting " + b);
        }

        [Fact]
        public void BitUnpacking()
        {
            byte[] data = new byte[] { 0x80, 0x00, 0x0F, 0x50, 0x83, 0x7D };
            BitPack bitpacker = new BitPack(data, 0);

            int b = bitpacker.UnpackBits(1);
            Assert.True(b == 1, "Unpacked " + b + " instead of 1");

            b = bitpacker.UnpackBits(1);
            Assert.True(b == 0, "Unpacked " + b + " instead of 0");

            bitpacker = new BitPack(data, 2);

            b = bitpacker.UnpackBits(4);
            Assert.True(b == 0, "Unpacked " + b + " instead of 0");

            b = bitpacker.UnpackBits(8);
            Assert.True(b == 0xF5, "Unpacked " + b + " instead of 0xF5");

            b = bitpacker.UnpackBits(4);
            Assert.True(b == 0, "Unpacked " + b + " instead of 0");

            b = bitpacker.UnpackBits(10);
            Assert.True(b == 0x0183, "Unpacked " + b + " instead of 0x0183");
        }

        [Fact]
        public void BitPacking()
        {
            byte[] packedBytes = new byte[12];
            BitPack bitpacker = new BitPack(packedBytes, 0);

            bitpacker.PackBits(0x0ABBCCDD, 32);
            bitpacker.PackBits(25, 5);
            bitpacker.PackFloat(123.321f);
            bitpacker.PackBits(1000, 16);

            bitpacker = new BitPack(packedBytes, 0);

            int b = bitpacker.UnpackBits(32);
            Assert.True(b == 0x0ABBCCDD, "Unpacked " + b + " instead of 2864434397");

            b = bitpacker.UnpackBits(5);
            Assert.True(b == 25, "Unpacked " + b + " instead of 25");

            float f = bitpacker.UnpackFloat();
            Assert.True(f == 123.321f, "Unpacked " + f + " instead of 123.321");

            b = bitpacker.UnpackBits(16);
            Assert.True(b == 1000, "Unpacked " + b + " instead of 1000");

            packedBytes = new byte[1];
            bitpacker = new BitPack(packedBytes, 0);
            bitpacker.PackBit(true);

            bitpacker = new BitPack(packedBytes, 0);
            b = bitpacker.UnpackBits(1);
            Assert.True(b == 1, "Unpacked " + b + " instead of 1");

            packedBytes = new byte[1]{255};
            bitpacker = new BitPack(packedBytes, 0, 0); // tell to append
            bitpacker.PackBit(false);

            bitpacker = new BitPack(packedBytes, 0);
            b = bitpacker.UnpackBits(1);
            Assert.True(b == 0, "Unpacked " + b + " instead of 0");
        }

        [Fact]
        public void LLSDTerseParsing()
        {
            string testOne = "[r0.99967899999999998428,r-0.025334599999999998787,r0]";
            string testTwo = "[[r1,r1,r1],r0]";
            string testThree = "{'region_handle':[r255232, r256512], 'position':[r33.6, r33.71, r43.13], 'look_at':[r34.6, r33.71, r43.13]}";

            OSD obj = OSDParser.DeserializeLLSDNotation(testOne);
            Assert.True(obj is OSDArray, "Expected SDArray, got " + obj.GetType().ToString());
            OSDArray array = (OSDArray)obj;
            Assert.True(array.Count == 3, "Expected three contained objects, got " + array.Count);
            Assert.True(array[0].AsReal() > 0.9d && array[0].AsReal() < 1.0d, "Unexpected value for first real " + array[0].AsReal());
            Assert.True(array[1].AsReal() < 0.0d && array[1].AsReal() > -0.03d, "Unexpected value for second real " + array[1].AsReal());
            Assert.True(array[2].AsReal() == 0.0d, "Unexpected value for third real " + array[2].AsReal());

            obj = OSDParser.DeserializeLLSDNotation(testTwo);
            Assert.True(obj is OSDArray, "Expected SDArray, got " + obj.GetType().ToString());
            array = (OSDArray)obj;
            Assert.True(array.Count == 2, "Expected two contained objects, got " + array.Count);
            Assert.True(array[1].AsReal() == 0.0d, "Unexpected value for real " + array[1].AsReal());
            obj = array[0];
            Assert.True(obj is OSDArray, "Expected ArrayList, got " + obj.GetType().ToString());
            array = (OSDArray)obj;
            Assert.True(array[0].AsReal() == 1.0d && array[1].AsReal() == 1.0d && array[2].AsReal() == 1.0d,
                "Unexpected value(s) for nested array: " + array[0].AsReal() + ", " + array[1].AsReal() + ", " +
                array[2].AsReal());

            obj = OSDParser.DeserializeLLSDNotation(testThree);
            Assert.True(obj is OSDMap, "Expected LLSDMap, got " + obj.GetType().ToString());
            OSDMap hashtable = (OSDMap)obj;
            Assert.True(hashtable.Count == 3, "Expected three contained objects, got " + hashtable.Count);
            Assert.IsType<OSDArray>(hashtable["region_handle"]);
            Assert.True(((OSDArray)hashtable["region_handle"]).Count == 2);
            Assert.IsType<OSDArray>(hashtable["position"]);
            Assert.True(((OSDArray)hashtable["position"]).Count == 3);
            Assert.IsType<OSDArray>(hashtable["look_at"]);
            Assert.True(((OSDArray)hashtable["look_at"]).Count == 3);
        }
    }
}
