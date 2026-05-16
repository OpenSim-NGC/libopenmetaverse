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
using System.Text;

namespace OpenMetaverse.Tests
{
    /// <summary>
    /// XmlSDTests is a suite of tests for libsl implementation of the SD XML format.
    /// 
    /// </summary>
    
    public class XmlSDTests
    {
        /// <summary>
        /// Test that the sample LLSD supplied by Linden Lab is properly deserialized.
        /// The LLSD string in the test is a pared down version of the sample on the blog.
        /// http://wiki.secondlife.com/wiki/LLSD
        /// </summary>
        [Fact]
        public void DeserializeLLSDSample()
        {
            OSD theSD = null;
            OSDMap map = null;
            OSD tempSD = null;
            OSDUUID tempUUID = null;
            OSDString tempStr = null;
            OSDReal tempReal = null;

            String testSD = @"<?xml version='1.0' encoding='UTF-8'?>
            <llsd>
                <map>
	                <key>region_id</key>
	                <uuid>67153d5b-3659-afb4-8510-adda2c034649</uuid>
	                <key>scale</key>
	                <string>one minute</string>
	                <key>simulator statistics</key>
	                <map>
		                <key>time dilation</key>
		                <real>0.9878624</real>
		                <key>sim fps</key>
		                <real>44.38898</real>
		                <key>agent updates per second</key>
		                <real>nan</real>
		                <key>total task count</key>
		                <real>4</real>
		                <key>active task count</key>
		                <real>0</real>
		                <key>pending uploads</key>
		                <real>0.0001096525</real>
	                </map>
                </map>
            </llsd>";

            //Deserialize the string
            byte[] bytes = Encoding.UTF8.GetBytes(testSD);
            theSD = OSDParser.DeserializeLLSDXml(bytes);

            //Confirm the contents
            Assert.NotNull(theSD);
            Assert.True(theSD is OSDMap);
            Assert.True(theSD.Type == OSDType.Map);
            map = (OSDMap)theSD;

            tempSD = map["region_id"];
            Assert.NotNull(tempSD);
            Assert.True(tempSD is OSDUUID);
            Assert.True(tempSD.Type == OSDType.UUID);
            tempUUID = (OSDUUID)tempSD;
            Assert.Equal(new UUID("67153d5b-3659-afb4-8510-adda2c034649"), tempUUID.AsUUID());

            tempSD = map["scale"];
            Assert.NotNull(tempSD);
            Assert.True(tempSD is OSDString);
            Assert.True(tempSD.Type == OSDType.String);
            tempStr = (OSDString)tempSD;
            Assert.Equal("one minute", tempStr.AsString());

            tempSD = map["simulator statistics"];
            Assert.NotNull(tempSD);
            Assert.True(tempSD is OSDMap);
            Assert.True(tempSD.Type == OSDType.Map);
            map = (OSDMap)tempSD;

            tempSD = map["time dilation"];
            Assert.NotNull(tempSD);
            Assert.True(tempSD is OSDReal);
            Assert.True(tempSD.Type == OSDType.Real);
            tempReal = (OSDReal)tempSD;

            Assert.Equal(0.9878624d, tempReal.AsReal());
            //TODO - figure out any relevant rounding variability for 64 bit reals
            tempSD = map["sim fps"];
            Assert.NotNull(tempSD);
            Assert.True(tempSD is OSDReal);
            Assert.True(tempSD.Type == OSDType.Real);
            tempReal = (OSDReal)tempSD;
            Assert.Equal(44.38898d, tempReal.AsReal());

            tempSD = map["agent updates per second"];
            Assert.NotNull(tempSD);
            Assert.True(tempSD is OSDReal);
            Assert.True(tempSD.Type == OSDType.Real);
            tempReal = (OSDReal)tempSD;
            Assert.Equal(Double.NaN, tempSD.AsReal());

            tempSD = map["total task count"];
            Assert.NotNull(tempSD);
            Assert.True(tempSD is OSDReal);
            Assert.True(tempSD.Type == OSDType.Real);
            tempReal = (OSDReal)tempSD;
            Assert.Equal(4.0d, tempReal.AsReal());

            tempSD = map["active task count"];
            Assert.NotNull(tempSD);
            Assert.True(tempSD is OSDReal);
            Assert.True(tempSD.Type == OSDType.Real);
            tempReal = (OSDReal)tempSD;
            Assert.Equal(0.0d, tempReal.AsReal());

            tempSD = map["pending uploads"];
            Assert.NotNull(tempSD);
            Assert.True(tempSD is OSDReal);
            Assert.True(tempSD.Type == OSDType.Real);
            tempReal = (OSDReal)tempSD;
            Assert.Equal(0.0001096525d, tempReal.AsReal());

        }

        /// <summary>
        /// Test that various Real representations are parsed correctly.
        /// </summary>
        [Fact]
        public void DeserializeReals()
        {
            OSD theSD = null;
            OSDArray array = null;
            OSDReal tempReal = null;

            String testSD = @"<?xml version='1.0' encoding='UTF-8'?>
            <llsd>
                <array>
                    <real>44.38898</real>
		            <real>nan</real>
		            <real>4</real>
                    <real>-13.333</real>
                    <real/>
                </array>
            </llsd>";
            //Deserialize the string
            byte[] bytes = Encoding.UTF8.GetBytes(testSD);
            theSD = OSDParser.DeserializeLLSDXml(bytes);

            Assert.True(theSD is OSDArray);
            array = (OSDArray)theSD;

            Assert.Equal(OSDType.Real, array[0].Type);
            tempReal = (OSDReal)array[0];
            Assert.Equal(44.38898d, tempReal.AsReal());

            Assert.Equal(OSDType.Real, array[1].Type);
            tempReal = (OSDReal)array[1];
            Assert.Equal(Double.NaN, tempReal.AsReal());

            Assert.Equal(OSDType.Real, array[2].Type);
            tempReal = (OSDReal)array[2];
            Assert.Equal(4.0d, tempReal.AsReal());

            Assert.Equal(OSDType.Real, array[3].Type);
            tempReal = (OSDReal)array[3];
            Assert.Equal(-13.333d, tempReal.AsReal());

            Assert.Equal(OSDType.Real, array[4].Type);
            tempReal = (OSDReal)array[4];
            Assert.Equal(0d, tempReal.AsReal());
        }

        /// <summary>
        /// Test that various String representations are parsed correctly.
        /// </summary>
        [Fact]
        public void DeserializeStrings()
        {
            OSD theSD = null;
            OSDArray array = null;
            OSDString tempStr = null;

            String testSD = @"<?xml version='1.0' encoding='UTF-8'?>
            <llsd>
                <array>
                    <string>Kissling</string>
                    <string>Attack ships on fire off the shoulder of Orion</string>
                    <string>&lt; &gt; &amp; &apos; &quot;</string>
                    <string/>
                </array>
            </llsd>";
            //Deserialize the string
            byte[] bytes = Encoding.UTF8.GetBytes(testSD);
            theSD = OSDParser.DeserializeLLSDXml(bytes);

            Assert.True(theSD is OSDArray);
            array = (OSDArray)theSD;

            Assert.Equal(OSDType.String, array[0].Type);
            tempStr = (OSDString)array[0];
            Assert.Equal("Kissling", tempStr.AsString());

            Assert.Equal(OSDType.String, array[1].Type);
            tempStr = (OSDString)array[1];
            Assert.Equal("Attack ships on fire off the shoulder of Orion", tempStr.AsString());

            Assert.Equal(OSDType.String, array[2].Type);
            tempStr = (OSDString)array[2];
            Assert.Equal("< > & \' \"", tempStr.AsString());

            Assert.Equal(OSDType.String, array[3].Type);
            tempStr = (OSDString)array[3];
            Assert.Equal("", tempStr.AsString());

        }

        /// <summary>
        /// Test that various Integer representations are parsed correctly.
        /// These tests currently only test for values within the range of a
        /// 32 bit signed integer, even though the SD specification says
        /// the type is a 64 bit signed integer, because LLSInteger is currently
        /// implemented using int, a.k.a. Int32.  Not testing Int64 range until
        /// it's understood if there was a design reason for the Int32.
        /// </summary>
        [Fact]
        public void DeserializeIntegers()
        {
            OSD theSD = null;
            OSDArray array = null;
            OSDInteger tempInt = null;

            String testSD = @"<?xml version='1.0' encoding='UTF-8'?>
            <llsd>
                <array>
                    <integer>2147483647</integer>
		            <integer>-2147483648</integer>
		            <integer>0</integer>
                    <integer>013</integer>
                    <integer/>
                </array>
            </llsd>";
            //Deserialize the string
            byte[] bytes = Encoding.UTF8.GetBytes(testSD);
            theSD = OSDParser.DeserializeLLSDXml(bytes);

            Assert.True(theSD is OSDArray);
            array = (OSDArray)theSD;

            Assert.Equal(OSDType.Integer, array[0].Type);
            tempInt = (OSDInteger)array[0];
            Assert.Equal(2147483647, tempInt.AsInteger());

            Assert.Equal(OSDType.Integer, array[1].Type);
            tempInt = (OSDInteger)array[1];
            Assert.Equal(-2147483648, tempInt.AsInteger());

            Assert.Equal(OSDType.Integer, array[2].Type);
            tempInt = (OSDInteger)array[2];
            Assert.Equal(0, tempInt.AsInteger());

            Assert.Equal(OSDType.Integer, array[3].Type);
            tempInt = (OSDInteger)array[3];
            Assert.Equal(13, tempInt.AsInteger());

            Assert.Equal(OSDType.Integer, array[4].Type);
            tempInt = (OSDInteger)array[4];
            Assert.Equal(0, tempInt.AsInteger());
        }

        /// <summary>
        /// Test that various UUID representations are parsed correctly.
        /// </summary>
        [Fact]
        public void DeserializeUUID()
        {
            OSD theSD = null;
            OSDArray array = null;
            OSDUUID tempUUID = null;

            String testSD = @"<?xml version='1.0' encoding='UTF-8'?>
            <llsd>
                <array>
                    <uuid>d7f4aeca-88f1-42a1-b385-b9db18abb255</uuid>
                    <uuid/>
                </array>
            </llsd>";
            //Deserialize the string
            byte[] bytes = Encoding.UTF8.GetBytes(testSD);
            theSD = OSDParser.DeserializeLLSDXml(bytes);

            Assert.True(theSD is OSDArray);
            array = (OSDArray)theSD;

            Assert.Equal(OSDType.UUID, array[0].Type);
            tempUUID = (OSDUUID)array[0];
            Assert.Equal(new UUID("d7f4aeca-88f1-42a1-b385-b9db18abb255"), tempUUID.AsUUID());

            Assert.Equal(OSDType.UUID, array[1].Type);
            tempUUID = (OSDUUID)array[1];
            Assert.Equal(UUID.Zero, tempUUID.AsUUID());
        }

        /// <summary>
        /// Test that various date representations are parsed correctly.
        /// </summary>
        [Fact]
        public void DeserializeDates()
        {
            OSD theSD = null;
            OSDArray array = null;
            OSDDate tempDate = null;
            DateTime testDate;

            String testSD = @"<?xml version='1.0' encoding='UTF-8'?>
            <llsd>
                <array>
                    <date>2006-02-01T14:29:53Z</date>
                    <date>1999-01-01T00:00:00Z</date>
                    <date/>
                </array>
            </llsd>";
            //Deserialize the string
            byte[] bytes = Encoding.UTF8.GetBytes(testSD);
            theSD = OSDParser.DeserializeLLSDXml(bytes);

            Assert.True(theSD is OSDArray);
            array = (OSDArray)theSD;

            Assert.Equal(OSDType.Date, array[0].Type);
            tempDate = (OSDDate)array[0];
            DateTime.TryParse("2006-02-01T14:29:53Z", out testDate);
            Assert.Equal(testDate, tempDate.AsDate());

            Assert.Equal(OSDType.Date, array[1].Type);
            tempDate = (OSDDate)array[1];
            DateTime.TryParse("1999-01-01T00:00:00Z", out testDate);
            Assert.Equal(testDate, tempDate.AsDate());

            Assert.Equal(OSDType.Date, array[2].Type);
            tempDate = (OSDDate)array[2];
            Assert.Equal(Utils.Epoch, tempDate.AsDate());
        }

        /// <summary>
        /// Test that various Boolean representations are parsed correctly.
        /// </summary>
        [Fact]
        public void DeserializeBoolean()
        {
            OSD theSD = null;
            OSDArray array = null;
            OSDBoolean tempBool = null;

            String testSD = @"<?xml version='1.0' encoding='UTF-8'?>
            <llsd>
                <array>
                    <boolean>1</boolean>
                    <boolean>true</boolean>
                    <boolean>0</boolean>
                    <boolean>false</boolean>
                    <boolean/>
                </array>
            </llsd>";
            //Deserialize the string
            byte[] bytes = Encoding.UTF8.GetBytes(testSD);
            theSD = OSDParser.DeserializeLLSDXml(bytes);

            Assert.True(theSD is OSDArray);
            array = (OSDArray)theSD;

            Assert.Equal(OSDType.Boolean, array[0].Type);
            tempBool = (OSDBoolean)array[0];
            Assert.Equal(true, tempBool.AsBoolean());

            Assert.Equal(OSDType.Boolean, array[1].Type);
            tempBool = (OSDBoolean)array[1];
            Assert.Equal(true, tempBool.AsBoolean());

            Assert.Equal(OSDType.Boolean, array[2].Type);
            tempBool = (OSDBoolean)array[2];
            Assert.Equal(false, tempBool.AsBoolean());

            Assert.Equal(OSDType.Boolean, array[3].Type);
            tempBool = (OSDBoolean)array[3];
            Assert.Equal(false, tempBool.AsBoolean());

            Assert.Equal(OSDType.Boolean, array[4].Type);
            tempBool = (OSDBoolean)array[4];
            Assert.Equal(false, tempBool.AsBoolean());
        }

        /// <summary>
        /// Test that binary elements are parsed correctly.
        /// </summary>
        [Fact]
        public void DeserializeBinary()
        {
            OSD theSD = null;
            OSDArray array = null;
            OSDBinary tempBinary = null;

            String testSD = @"<?xml version='1.0' encoding='UTF-8'?>
            <llsd>
                <array>
                    <binary encoding='base64'>cmFuZG9t</binary>
                    <binary>dGhlIHF1aWNrIGJyb3duIGZveA==</binary>
                    <binary/>
                </array>
            </llsd>";

            //Deserialize the string
            byte[] bytes = Encoding.UTF8.GetBytes(testSD);
            theSD = OSDParser.DeserializeLLSDXml(bytes);

            Assert.True(theSD is OSDArray);
            array = (OSDArray)theSD;

            Assert.Equal(OSDType.Binary, array[0].Type);
            tempBinary = (OSDBinary)array[0];
            byte[] testData1 = { 114, 97, 110, 100, 111, 109 };
            TestHelper.TestBinary(tempBinary, testData1);

            Assert.Equal(OSDType.Binary, array[1].Type);
            tempBinary = (OSDBinary)array[1];
            byte[] testData2 = {116, 104, 101, 32, 113, 117, 105, 99, 107, 32, 98,
                                114, 111, 119, 110, 32, 102, 111, 120};
            TestHelper.TestBinary(tempBinary, testData2);

            Assert.Equal(OSDType.Binary, array[1].Type);
            tempBinary = (OSDBinary)array[2];
            Assert.Equal(0, tempBinary.AsBinary().Length);
        }

        /// <summary>
        /// Test that undefened elements are parsed correctly.
        /// Currently this just checks that there is no error since undefined has no
        /// value and there is no SD child class for Undefined elements - the
        /// current implementation generates an instance of SD
        /// </summary>
        [Fact]
        public void DeserializeUndef()
        {
            OSD theSD = null;

            String testSD = @"<?xml version='1.0' encoding='UTF-8'?>
            <llsd>
                <undef/>
            </llsd>";
            //Deserialize the string
            byte[] bytes = Encoding.UTF8.GetBytes(testSD);
            theSD = OSDParser.DeserializeLLSDXml(bytes);

            Assert.True(theSD is OSD);
        }

        /// <summary>
        /// Test that various URI representations are parsed correctly.
        /// </summary>
        [Fact]
        public void DeserializeURI()
        {
            OSD theSD = null;
            OSDArray array = null;
            OSDUri tempURI = null;

            String testSD = @"<?xml version='1.0' encoding='UTF-8'?>
            <llsd>
                <array>
                    <uri>http://sim956.agni.lindenlab.com:12035/runtime/agents</uri>
                    <uri/>
                </array>
            </llsd>";
            //Deserialize the string
            byte[] bytes = Encoding.UTF8.GetBytes(testSD);
            theSD = OSDParser.DeserializeLLSDXml(bytes);

            Assert.True(theSD is OSDArray);
            array = (OSDArray)theSD;

            Assert.Equal(OSDType.URI, array[0].Type);
            tempURI = (OSDUri)array[0];
            Uri testURI = new Uri(@"http://sim956.agni.lindenlab.com:12035/runtime/agents");
            Assert.Equal(testURI, tempURI.AsUri());

            Assert.Equal(OSDType.URI, array[1].Type);
            tempURI = (OSDUri)array[1];
            Assert.Equal("", tempURI.AsUri().ToString());
        }

        /// <summary>
        /// Test some nested containers.  This is not a very deep or complicated SD graph
        /// but it should reveal basic nesting issues.
        /// </summary>
        [Fact]
        public void DeserializeNestedContainers()
        {
            OSD theSD = null;
            OSDArray array = null;
            OSDMap map = null;
            OSD tempSD = null;

            String testSD = @"<?xml version='1.0' encoding='UTF-8'?>
            <llsd>
                <array>
                    <map>
                        <key>Map One</key>
                        <map>
                            <key>Array One</key>
                            <array>
                                <integer>1</integer>
                                <integer>2</integer>
                            </array>
                        </map>
                    </map>
                    <array>
                        <string>A</string>
                        <string>B</string>
                        <array>
                            <integer>1</integer>
                            <integer>4</integer>
                            <integer>9</integer>
                        </array>
                    </array>
                </array>
            </llsd>";
            //Deserialize the string
            byte[] bytes = Encoding.UTF8.GetBytes(testSD);
            theSD = OSDParser.DeserializeLLSDXml(bytes);

            Assert.True(theSD is OSDArray);
            array = (OSDArray)theSD;
            Assert.Equal(2, array.Count);

            //The first element of top level array, a map
            Assert.Equal(OSDType.Map, array[0].Type);
            map = (OSDMap)array[0];
            //First nested map
            tempSD = map["Map One"];
            Assert.NotNull(tempSD);
            Assert.Equal(OSDType.Map, tempSD.Type);
            map = (OSDMap)tempSD;
            //First nested array
            tempSD = map["Array One"];
            Assert.NotNull(tempSD);
            Assert.Equal(OSDType.Array, tempSD.Type);
            array = (OSDArray)tempSD;
            Assert.Equal(2, array.Count);

            array = (OSDArray)theSD;
            //Second element of top level array, an array
            tempSD = array[1];
            Assert.Equal(OSDType.Array, tempSD.Type);
            array = (OSDArray)tempSD;
            Assert.Equal(3, array.Count);
            //Nested array
            tempSD = array[2];
            Assert.Equal(OSDType.Array, tempSD.Type);
            array = (OSDArray)tempSD;
            Assert.Equal(3, array.Count);
        }
    }

    internal static class TestHelper
    {
        /// <summary>
        /// Asserts that the contents of the SDBinary match the values and length
        /// of the supplied byte array
        /// </summary>
        /// <param name="inBinary"></param>
        /// <param name="inExpected"></param>
        internal static void TestBinary(OSDBinary inBinary, byte[] inExpected)
        {
            byte[] binary = inBinary.AsBinary();
            Assert.Equal(inExpected.Length, binary.Length);
            for (int i = 0; i < inExpected.Length; i++)
            {
                if (inExpected[i] != binary[i])
                {
                    throw new Xunit.Sdk.XunitException("Expected " + inExpected[i].ToString() + " at position " + i.ToString() +
                        " but saw " + binary[i].ToString());
                }
            }
        }
    }
}
