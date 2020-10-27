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

/* 
 * 
 * This tests are based upon the description at
 * 
 * http://wiki.secondlife.com/wiki/SD
 * 
 * and (partially) generated by the (supposed) reference implementation at
 * 
 * http://svn.secondlife.com/svn/linden/release/indra/lib/python/indra/base/llsd.py
 * 
 */

using NUnit.Framework;
using OpenMetaverse.StructuredData;
using System;
using System.IO;
using System.Text;
using System.Xml;

namespace OpenMetaverse.Tests
{

    [TestFixture()]
    public class NotationSDTests
    {
        [Test()]
        public void HelperFunctions()
        {
            StringReader reader = new StringReader("test1tast2test3");

            char[] charsOne = { 't', 'e', 's', 't' };
            int resultOne = OSDParser.BufferCharactersEqual(reader, charsOne, 0);
            Assert.AreEqual(charsOne.Length, resultOne);

            char[] charsTwo = { '1', 't', 'e' };
            int resultTwo = OSDParser.BufferCharactersEqual(reader, charsTwo, 0);
            Assert.AreEqual(2, resultTwo);

            char[] charsThree = { 'a', 's', 't', '2', 't', 'e', 's' };
            int resultThree = OSDParser.BufferCharactersEqual(reader, charsThree, 1);
            Assert.AreEqual(1, resultThree);

            int resultFour = OSDParser.BufferCharactersEqual(reader, charsThree, 0);
            Assert.AreEqual(charsThree.Length, resultFour);

            char[] charsFive = { 't', '3', 'a', 'a' };
            int resultFive = OSDParser.BufferCharactersEqual(reader, charsFive, 0);
            Assert.AreEqual(2, resultFive);


        }

        [Test()]
        public void DeserializeUndef()
        {
            String s = "!";
            OSD llsd = OSDParser.DeserializeLLSDNotation(s);
            Assert.AreEqual(OSDType.Unknown, llsd.Type);
        }

        [Test()]
        public void SerializeUndef()
        {
            OSD llsd = new OSD();
            string s = OSDParser.SerializeLLSDNotation(llsd);

            OSD llsdDS = OSDParser.DeserializeLLSDNotation(s);
            Assert.AreEqual(OSDType.Unknown, llsdDS.Type);
        }

        [Test()]
        public void DeserializeBoolean()
        {
            String t = "true";
            OSD llsdT = OSDParser.DeserializeLLSDNotation(t);
            Assert.AreEqual(OSDType.Boolean, llsdT.Type);
            Assert.AreEqual(true, llsdT.AsBoolean());

            String tTwo = "t";
            OSD llsdTTwo = OSDParser.DeserializeLLSDNotation(tTwo);
            Assert.AreEqual(OSDType.Boolean, llsdTTwo.Type);
            Assert.AreEqual(true, llsdTTwo.AsBoolean());

            String tThree = "TRUE";
            OSD llsdTThree = OSDParser.DeserializeLLSDNotation(tThree);
            Assert.AreEqual(OSDType.Boolean, llsdTThree.Type);
            Assert.AreEqual(true, llsdTThree.AsBoolean());

            String tFour = "T";
            OSD llsdTFour = OSDParser.DeserializeLLSDNotation(tFour);
            Assert.AreEqual(OSDType.Boolean, llsdTFour.Type);
            Assert.AreEqual(true, llsdTFour.AsBoolean());

            String tFive = "1";
            OSD llsdTFive = OSDParser.DeserializeLLSDNotation(tFive);
            Assert.AreEqual(OSDType.Boolean, llsdTFive.Type);
            Assert.AreEqual(true, llsdTFive.AsBoolean());

            String f = "false";
            OSD llsdF = OSDParser.DeserializeLLSDNotation(f);
            Assert.AreEqual(OSDType.Boolean, llsdF.Type);
            Assert.AreEqual(false, llsdF.AsBoolean());

            String fTwo = "f";
            OSD llsdFTwo = OSDParser.DeserializeLLSDNotation(fTwo);
            Assert.AreEqual(OSDType.Boolean, llsdFTwo.Type);
            Assert.AreEqual(false, llsdFTwo.AsBoolean());

            String fThree = "FALSE";
            OSD llsdFThree = OSDParser.DeserializeLLSDNotation(fThree);
            Assert.AreEqual(OSDType.Boolean, llsdFThree.Type);
            Assert.AreEqual(false, llsdFThree.AsBoolean());

            String fFour = "F";
            OSD llsdFFour = OSDParser.DeserializeLLSDNotation(fFour);
            Assert.AreEqual(OSDType.Boolean, llsdFFour.Type);
            Assert.AreEqual(false, llsdFFour.AsBoolean());

            String fFive = "0";
            OSD llsdFFive = OSDParser.DeserializeLLSDNotation(fFive);
            Assert.AreEqual(OSDType.Boolean, llsdFFive.Type);
            Assert.AreEqual(false, llsdFFive.AsBoolean());
        }

        [Test()]
        public void SerializeBoolean()
        {
            OSD llsdTrue = OSD.FromBoolean(true);
            string sTrue = OSDParser.SerializeLLSDNotation(llsdTrue);
            OSD llsdTrueDS = OSDParser.DeserializeLLSDNotation(sTrue);
            Assert.AreEqual(OSDType.Boolean, llsdTrueDS.Type);
            Assert.AreEqual(true, llsdTrueDS.AsBoolean());

            OSD llsdFalse = OSD.FromBoolean(false);
            string sFalse = OSDParser.SerializeLLSDNotation(llsdFalse);
            OSD llsdFalseDS = OSDParser.DeserializeLLSDNotation(sFalse);
            Assert.AreEqual(OSDType.Boolean, llsdFalseDS.Type);
            Assert.AreEqual(false, llsdFalseDS.AsBoolean());
        }

        [Test()]
        public void DeserializeInteger()
        {
            string integerOne = "i12319423";
            OSD llsdOne = OSDParser.DeserializeLLSDNotation(integerOne);
            Assert.AreEqual(OSDType.Integer, llsdOne.Type);
            Assert.AreEqual(12319423, llsdOne.AsInteger());

            string integerTwo = "i-489234";
            OSD llsdTwo = OSDParser.DeserializeLLSDNotation(integerTwo);
            Assert.AreEqual(OSDType.Integer, llsdTwo.Type);
            Assert.AreEqual(-489234, llsdTwo.AsInteger());
        }

        [Test()]
        public void SerializeInteger()
        {
            OSD llsdOne = OSD.FromInteger(12319423);
            string sOne = OSDParser.SerializeLLSDNotation(llsdOne);
            OSD llsdOneDS = OSDParser.DeserializeLLSDNotation(sOne);
            Assert.AreEqual(OSDType.Integer, llsdOneDS.Type);
            Assert.AreEqual(12319423, llsdOne.AsInteger());

            OSD llsdTwo = OSD.FromInteger(-71892034);
            string sTwo = OSDParser.SerializeLLSDNotation(llsdTwo);
            OSD llsdTwoDS = OSDParser.DeserializeLLSDNotation(sTwo);
            Assert.AreEqual(OSDType.Integer, llsdTwoDS.Type);
            Assert.AreEqual(-71892034, llsdTwoDS.AsInteger());
        }

        [Test()]
        public void DeserializeReal()
        {
            String realOne = "r1123412345.465711";
            OSD llsdOne = OSDParser.DeserializeLLSDNotation(realOne);
            Assert.AreEqual(OSDType.Real, llsdOne.Type);
            Assert.AreEqual(1123412345.465711d, llsdOne.AsReal());

            String realTwo = "r-11234684.923411";
            OSD llsdTwo = OSDParser.DeserializeLLSDNotation(realTwo);
            Assert.AreEqual(OSDType.Real, llsdTwo.Type);
            Assert.AreEqual(-11234684.923411d, llsdTwo.AsReal());

            String realThree = "r1";
            OSD llsdThree = OSDParser.DeserializeLLSDNotation(realThree);
            Assert.AreEqual(OSDType.Real, llsdThree.Type);
            Assert.AreEqual(1d, llsdThree.AsReal());

            String realFour = "r2.0193899999999998204e-06";
            OSD llsdFour = OSDParser.DeserializeLLSDNotation(realFour);
            Assert.AreEqual(OSDType.Real, llsdFour.Type);
            Assert.AreEqual(2.0193899999999998204e-06d, llsdFour.AsReal());

            String realFive = "r0";
            OSD llsdFive = OSDParser.DeserializeLLSDNotation(realFive);
            Assert.AreEqual(OSDType.Real, llsdFive.Type);
            Assert.AreEqual(0d, llsdFive.AsReal());
        }

        [Test()]
        public void SerializeReal()
        {
            OSD llsdOne = OSD.FromReal(12987234.723847d);
            string sOne = OSDParser.SerializeLLSDNotation(llsdOne);
            OSD llsdOneDS = OSDParser.DeserializeLLSDNotation(sOne);
            Assert.AreEqual(OSDType.Real, llsdOneDS.Type);
            Assert.AreEqual(12987234.723847d, llsdOneDS.AsReal());

            OSD llsdTwo = OSD.FromReal(-32347892.234234d);
            string sTwo = OSDParser.SerializeLLSDNotation(llsdTwo);
            OSD llsdTwoDS = OSDParser.DeserializeLLSDNotation(sTwo);
            Assert.AreEqual(OSDType.Real, llsdTwoDS.Type);
            Assert.AreEqual(-32347892.234234d, llsdTwoDS.AsReal());

            OSD llsdThree = OSD.FromReal(Double.MaxValue);
            string sThree = OSDParser.SerializeLLSDNotation(llsdThree);
            OSD llsdThreeDS = OSDParser.DeserializeLLSDNotation(sThree);
            Assert.AreEqual(OSDType.Real, llsdThreeDS.Type);
            Assert.AreEqual(Double.MaxValue, llsdThreeDS.AsReal());

            OSD llsdFour = OSD.FromReal(Double.MinValue);
            string sFour = OSDParser.SerializeLLSDNotation(llsdFour);
            OSD llsdFourDS = OSDParser.DeserializeLLSDNotation(sFour);
            Assert.AreEqual(OSDType.Real, llsdFourDS.Type);
            Assert.AreEqual(Double.MinValue, llsdFourDS.AsReal());

            OSD llsdFive = OSD.FromReal(-1.1123123E+50d);
            string sFive = OSDParser.SerializeLLSDNotation(llsdFive);
            OSD llsdFiveDS = OSDParser.DeserializeLLSDNotation(sFive);
            Assert.AreEqual(OSDType.Real, llsdFiveDS.Type);
            Assert.AreEqual(-1.1123123E+50d, llsdFiveDS.AsReal());

            OSD llsdSix = OSD.FromReal(2.0193899999999998204e-06);
            string sSix = OSDParser.SerializeLLSDNotation(llsdSix);
            OSD llsdSixDS = OSDParser.DeserializeLLSDNotation(sSix);
            Assert.AreEqual(OSDType.Real, llsdSixDS.Type);
            Assert.AreEqual(2.0193899999999998204e-06, llsdSixDS.AsReal());
        }

        [Test()]
        public void DeserializeUUID()
        {
            String uuidOne = "u97f4aeca-88a1-42a1-b385-b97b18abb255";
            OSD llsdOne = OSDParser.DeserializeLLSDNotation(uuidOne);
            Assert.AreEqual(OSDType.UUID, llsdOne.Type);
            Assert.AreEqual("97f4aeca-88a1-42a1-b385-b97b18abb255", llsdOne.AsString());

            String uuidTwo = "u00000000-0000-0000-0000-000000000000";
            OSD llsdTwo = OSDParser.DeserializeLLSDNotation(uuidTwo);
            Assert.AreEqual(OSDType.UUID, llsdTwo.Type);
            Assert.AreEqual("00000000-0000-0000-0000-000000000000", llsdTwo.AsString());
        }

        [Test()]
        public void SerializeUUID()
        {
            OSD llsdOne = OSD.FromUUID(new UUID("97f4aeca-88a1-42a1-b385-b97b18abb255"));
            string sOne = OSDParser.SerializeLLSDNotation(llsdOne);
            OSD llsdOneDS = OSDParser.DeserializeLLSDNotation(sOne);
            Assert.AreEqual(OSDType.UUID, llsdOneDS.Type);
            Assert.AreEqual("97f4aeca-88a1-42a1-b385-b97b18abb255", llsdOneDS.AsString());

            OSD llsdTwo = OSD.FromUUID(new UUID("00000000-0000-0000-0000-000000000000"));
            string sTwo = OSDParser.SerializeLLSDNotation(llsdTwo);
            OSD llsdTwoDS = OSDParser.DeserializeLLSDNotation(sTwo);
            Assert.AreEqual(OSDType.UUID, llsdTwoDS.Type);
            Assert.AreEqual("00000000-0000-0000-0000-000000000000", llsdTwoDS.AsString());
        }

        public void DeserializeString()
        {
            string sOne = "''";
            OSD llsdOne = OSDParser.DeserializeLLSDNotation(sOne);
            Assert.AreEqual(OSDType.String, llsdOne.Type);
            Assert.AreEqual("", llsdOne.AsString());

            // This is double escaping. Once for the encoding, and once for csharp.  
            string sTwo = "'test\\'\"test'";
            OSD llsdTwo = OSDParser.DeserializeLLSDNotation(sTwo);
            Assert.AreEqual(OSDType.String, llsdTwo.Type);
            Assert.AreEqual("test'\"test", llsdTwo.AsString());

            // "test \\lest"
            char[] cThree = { (char)0x27, (char)0x74, (char)0x65, (char)0x73, (char)0x74, (char)0x20, (char)0x5c,
                                (char)0x5c, (char)0x6c, (char)0x65, (char)0x73, (char)0x74, (char)0x27 };
            string sThree = new string(cThree);

            OSD llsdThree = OSDParser.DeserializeLLSDNotation(sThree);
            Assert.AreEqual(OSDType.String, llsdThree.Type);
            Assert.AreEqual("test \\lest", llsdThree.AsString());

            string sFour = "'aa\t la'";
            OSD llsdFour = OSDParser.DeserializeLLSDNotation(sFour);
            Assert.AreEqual(OSDType.String, llsdFour.Type);
            Assert.AreEqual("aa\t la", llsdFour.AsString());

            char[] cFive = { (char)0x27, (char)0x5c, (char)0x5c, (char)0x27 };
            string sFive = new String(cFive);
            OSD llsdFive = OSDParser.DeserializeLLSDNotation(sFive);
            Assert.AreEqual(OSDType.String, llsdFive.Type);
            Assert.AreEqual("\\", llsdFive.AsString());


            string sSix = "s(10)\"1234567890\"";
            OSD llsdSix = OSDParser.DeserializeLLSDNotation(sSix);
            Assert.AreEqual(OSDType.String, llsdSix.Type);
            Assert.AreEqual("1234567890", llsdSix.AsString());

            string sSeven = "s(5)\"\\\\\\\\\\\"";
            OSD llsdSeven = OSDParser.DeserializeLLSDNotation(sSeven);
            Assert.AreEqual(OSDType.String, llsdSeven.Type);
            Assert.AreEqual("\\\\\\\\\\", llsdSeven.AsString());

            string sEight = "\"aouAOUhsdjklfghskldjfghqeiurtzwieortzaslxfjkgh\"";
            OSD llsdEight = OSDParser.DeserializeLLSDNotation(sEight);
            Assert.AreEqual(OSDType.String, llsdEight.Type);
            Assert.AreEqual("aouAOUhsdjklfghskldjfghqeiurtzwieortzaslxfjkgh", llsdEight.AsString());



        }

        public void DoSomeStringSerializingActionsAndAsserts(string s)
        {
            OSD llsdOne = OSD.FromString(s);
            string sOne = OSDParser.SerializeLLSDNotation(llsdOne);
            OSD llsdOneDS = OSDParser.DeserializeLLSDNotation(sOne);
            Assert.AreEqual(OSDType.String, llsdOne.Type);
            Assert.AreEqual(s, llsdOneDS.AsString());
        }


        [Test()]
        public void SerializeString()
        {
            DoSomeStringSerializingActionsAndAsserts("");

            DoSomeStringSerializingActionsAndAsserts("\\");

            DoSomeStringSerializingActionsAndAsserts("\"\"");

            DoSomeStringSerializingActionsAndAsserts("������-these-should-be-some-german-umlauts");

            DoSomeStringSerializingActionsAndAsserts("\t\n\r");

            DoSomeStringSerializingActionsAndAsserts("asdkjfhaksldjfhalskdjfhaklsjdfhaklsjdhjgzqeuiowrtzserghsldfg" +
                                                      "asdlkfhqeiortzsdkfjghslkdrjtzsoidklghuisoehiguhsierughaishdl" +
                                                      "asdfkjhueiorthsgsdkfughaslkdfjshldkfjghsldkjghsldkfghsdklghs" +
                                                      "wopeighisdjfghklasdfjghsdklfgjhsdklfgjshdlfkgjshdlfkgjshdlfk");

            DoSomeStringSerializingActionsAndAsserts("all is N\"\\'othing and n'oting is all");

            DoSomeStringSerializingActionsAndAsserts("very\"british is this.");

            // We test here also for 4byte characters
            string xml = "<x>&#x10137;</x>";
            byte[] bytes = Encoding.UTF8.GetBytes(xml);
            XmlTextReader xtr = new XmlTextReader(new MemoryStream(bytes, false));
            xtr.Read();
            xtr.Read();
            string content = xtr.ReadString();

            DoSomeStringSerializingActionsAndAsserts(content);

        }

        [Test()]
        public void DeserializeURI()
        {
            string sUriOne = "l\"http://test.com/test test>\\\"/&yes\"";
            OSD llsdOne = OSDParser.DeserializeLLSDNotation(sUriOne);
            Assert.AreEqual(OSDType.URI, llsdOne.Type);
            Assert.AreEqual("http://test.com/test%20test%3E%22/&yes", llsdOne.AsString());

            string sUriTwo = "l\"test/test/test?test=1&toast=2\"";
            OSD llsdTwo = OSDParser.DeserializeLLSDNotation(sUriTwo);
            Assert.AreEqual(OSDType.URI, llsdTwo.Type);
            Assert.AreEqual("test/test/test?test=1&toast=2", llsdTwo.AsString());
        }

        [Test()]
        public void SerializeURI()
        {
            Uri uriOne = new Uri("http://test.org/test test>\\\"/&yes\"", UriKind.RelativeOrAbsolute);
            OSD llsdOne = OSD.FromUri(uriOne);
            string sUriOne = OSDParser.SerializeLLSDNotation(llsdOne);
            OSD llsdOneDS = OSDParser.DeserializeLLSDNotation(sUriOne);
            Assert.AreEqual(OSDType.URI, llsdOneDS.Type);
            Assert.AreEqual(uriOne, llsdOneDS.AsUri());

            Uri uriTwo = new Uri("test/test/near/the/end?test=1", UriKind.RelativeOrAbsolute);
            OSD llsdTwo = OSD.FromUri(uriTwo);
            string sUriTwo = OSDParser.SerializeLLSDNotation(llsdTwo);
            OSD llsdTwoDS = OSDParser.DeserializeLLSDNotation(sUriTwo);
            Assert.AreEqual(OSDType.URI, llsdTwoDS.Type);
            Assert.AreEqual(uriTwo, llsdTwoDS.AsUri());
        }

        [Test()]
        public void DeserializeDate()
        {
            string sDateOne = "d\"2007-12-31T20:49:10Z\"";
            OSD llsdOne = OSDParser.DeserializeLLSDNotation(sDateOne);
            Assert.AreEqual(OSDType.Date, llsdOne.Type);
            DateTime dt = new DateTime(2007, 12, 31, 20, 49, 10, 0, DateTimeKind.Utc);
            DateTime dtDS = llsdOne.AsDate();
            Assert.AreEqual(dt, dtDS.ToUniversalTime());
        }

        [Test()]
        public void SerializeDate()
        {
            DateTime dtOne = new DateTime(2005, 8, 10, 11, 23, 4, DateTimeKind.Utc);
            OSD llsdOne = OSD.FromDate(dtOne);
            string sDtOne = OSDParser.SerializeLLSDNotation(llsdOne);
            OSD llsdOneDS = OSDParser.DeserializeLLSDNotation(sDtOne);
            Assert.AreEqual(OSDType.Date, llsdOneDS.Type);
            DateTime dtOneDS = llsdOneDS.AsDate();
            Assert.AreEqual(dtOne, dtOneDS.ToUniversalTime());

            DateTime dtTwo = new DateTime(2010, 10, 11, 23, 00, 10, 100, DateTimeKind.Utc);
            OSD llsdTwo = OSD.FromDate(dtTwo);
            string sDtTwo = OSDParser.SerializeLLSDNotation(llsdTwo);
            OSD llsdTwoDS = OSDParser.DeserializeLLSDNotation(sDtTwo);
            Assert.AreEqual(OSDType.Date, llsdTwoDS.Type);
            DateTime dtTwoDS = llsdTwoDS.AsDate();
            Assert.AreEqual(dtTwo, dtTwoDS.ToUniversalTime());

            // check if a *local* time can be serialized and deserialized
            DateTime dtThree = new DateTime(2009, 12, 30, 8, 25, 10, DateTimeKind.Local);
            OSD llsdDateThree = OSD.FromDate(dtThree);
            string sDateThreeSerialized = OSDParser.SerializeLLSDNotation(llsdDateThree);
            OSD llsdDateThreeDS = OSDParser.DeserializeLLSDNotation(sDateThreeSerialized);
            Assert.AreEqual(OSDType.Date, llsdDateThreeDS.Type);
            Assert.AreEqual(dtThree, llsdDateThreeDS.AsDate());
        }

        [Test()]
        public void SerializeBinary()
        {
            byte[] binary = { 0x0, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0b,
                                0x0b, 0x0c, 0x0d, 0x0e, 0x0f };

            OSD llsdBinary = OSD.FromBinary(binary);
            string sBinarySerialized = OSDParser.SerializeLLSDNotation(llsdBinary);
            OSD llsdBinaryDS = OSDParser.DeserializeLLSDNotation(sBinarySerialized);
            Assert.AreEqual(OSDType.Binary, llsdBinaryDS.Type);
            Assert.AreEqual(binary, llsdBinaryDS.AsBinary());
        }

        [Test()]
        public void DeserializeArray()
        {
            string sArrayOne = "[]";
            OSDArray llsdArrayOne = (OSDArray)OSDParser.DeserializeLLSDNotation(sArrayOne);
            Assert.AreEqual(OSDType.Array, llsdArrayOne.Type);
            Assert.AreEqual(0, llsdArrayOne.Count);

            string sArrayTwo = "[ i0 ]";
            OSDArray llsdArrayTwo = (OSDArray)OSDParser.DeserializeLLSDNotation(sArrayTwo);
            Assert.AreEqual(OSDType.Array, llsdArrayTwo.Type);
            Assert.AreEqual(1, llsdArrayTwo.Count);
            OSDInteger llsdIntOne = (OSDInteger)llsdArrayTwo[0];
            Assert.AreEqual(OSDType.Integer, llsdIntOne.Type);
            Assert.AreEqual(0, llsdIntOne.AsInteger());

            string sArrayThree = "[ i0, i1 ]";
            OSDArray llsdArrayThree = (OSDArray)OSDParser.DeserializeLLSDNotation(sArrayThree);
            Assert.AreEqual(OSDType.Array, llsdArrayThree.Type);
            Assert.AreEqual(2, llsdArrayThree.Count);
            OSDInteger llsdIntTwo = (OSDInteger)llsdArrayThree[0];
            Assert.AreEqual(OSDType.Integer, llsdIntTwo.Type);
            Assert.AreEqual(0, llsdIntTwo.AsInteger());
            OSDInteger llsdIntThree = (OSDInteger)llsdArrayThree[1];
            Assert.AreEqual(OSDType.Integer, llsdIntThree.Type);
            Assert.AreEqual(1, llsdIntThree.AsInteger());

            string sArrayFour = " [ \"testtest\", \"aha\",t,f,i1, r1.2, [ i1] ] ";
            OSDArray llsdArrayFour = (OSDArray)OSDParser.DeserializeLLSDNotation(sArrayFour);
            Assert.AreEqual(OSDType.Array, llsdArrayFour.Type);
            Assert.AreEqual(7, llsdArrayFour.Count);
            Assert.AreEqual("testtest", llsdArrayFour[0].AsString());
            Assert.AreEqual("aha", llsdArrayFour[1].AsString());
            Assert.AreEqual(true, llsdArrayFour[2].AsBoolean());
            Assert.AreEqual(false, llsdArrayFour[3].AsBoolean());
            Assert.AreEqual(1, llsdArrayFour[4].AsInteger());
            Assert.AreEqual(1.2d, llsdArrayFour[5].AsReal());
            Assert.AreEqual(OSDType.Array, llsdArrayFour[6].Type);
            OSDArray llsdArrayFive = (OSDArray)llsdArrayFour[6];
            Assert.AreEqual(1, llsdArrayFive[0].AsInteger());

        }

        [Test()]
        public void SerializeArray()
        {
            OSDArray llsdOne = new OSDArray();
            string sOne = OSDParser.SerializeLLSDNotation(llsdOne);
            OSDArray llsdOneDS = (OSDArray)OSDParser.DeserializeLLSDNotation(sOne);
            Assert.AreEqual(OSDType.Array, llsdOneDS.Type);
            Assert.AreEqual(0, llsdOneDS.Count);

            OSD llsdTwo = OSD.FromInteger(123234);
            OSD llsdThree = OSD.FromString("asedkfjhaqweiurohzasdf");
            OSDArray llsdFour = new OSDArray();
            llsdFour.Add(llsdTwo);
            llsdFour.Add(llsdThree);

            llsdOne.Add(llsdTwo);
            llsdOne.Add(llsdThree);
            llsdOne.Add(llsdFour);

            string sFive = OSDParser.SerializeLLSDNotation(llsdOne);
            OSDArray llsdFive = (OSDArray)OSDParser.DeserializeLLSDNotation(sFive);
            Assert.AreEqual(OSDType.Array, llsdFive.Type);
            Assert.AreEqual(3, llsdFive.Count);
            Assert.AreEqual(OSDType.Integer, llsdFive[0].Type);
            Assert.AreEqual(123234, llsdFive[0].AsInteger());
            Assert.AreEqual(OSDType.String, llsdFive[1].Type);
            Assert.AreEqual("asedkfjhaqweiurohzasdf", llsdFive[1].AsString());

            OSDArray llsdSix = (OSDArray)llsdFive[2];
            Assert.AreEqual(OSDType.Array, llsdSix.Type);
            Assert.AreEqual(2, llsdSix.Count);
            Assert.AreEqual(OSDType.Integer, llsdSix[0].Type);
            Assert.AreEqual(123234, llsdSix[0].AsInteger());
            Assert.AreEqual(OSDType.String, llsdSix[1].Type);
            Assert.AreEqual("asedkfjhaqweiurohzasdf", llsdSix[1].AsString());
        }

        [Test()]
        public void DeserializeMap()
        {
            string sMapOne = " { } ";
            OSDMap llsdMapOne = (OSDMap)OSDParser.DeserializeLLSDNotation(sMapOne);
            Assert.AreEqual(OSDType.Map, llsdMapOne.Type);
            Assert.AreEqual(0, llsdMapOne.Count);

            string sMapTwo = " { \"test\":i2 } ";
            OSDMap llsdMapTwo = (OSDMap)OSDParser.DeserializeLLSDNotation(sMapTwo);
            Assert.AreEqual(OSDType.Map, llsdMapTwo.Type);
            Assert.AreEqual(1, llsdMapTwo.Count);
            Assert.AreEqual(OSDType.Integer, llsdMapTwo["test"].Type);
            Assert.AreEqual(2, llsdMapTwo["test"].AsInteger());

            string sMapThree = " { 'test':\"testtesttest\", 'aha':\"muahahaha\" , \"anywhere\":! } ";
            OSDMap llsdMapThree = (OSDMap)OSDParser.DeserializeLLSDNotation(sMapThree);
            Assert.AreEqual(OSDType.Map, llsdMapThree.Type);
            Assert.AreEqual(3, llsdMapThree.Count);
            Assert.AreEqual(OSDType.String, llsdMapThree["test"].Type);
            Assert.AreEqual("testtesttest", llsdMapThree["test"].AsString());
            Assert.AreEqual(OSDType.String, llsdMapThree["test"].Type);
            Assert.AreEqual("muahahaha", llsdMapThree["aha"].AsString());
            Assert.AreEqual(OSDType.Unknown, llsdMapThree["self"].Type);

            string sMapFour = " { 'test' : { 'test' : i1, 't0st' : r2.5 }, 'tist' : \"hello world!\", 'tast' : \"last\" } ";
            OSDMap llsdMapFour = (OSDMap)OSDParser.DeserializeLLSDNotation(sMapFour);
            Assert.AreEqual(OSDType.Map, llsdMapFour.Type);
            Assert.AreEqual(3, llsdMapFour.Count);
            Assert.AreEqual("hello world!", llsdMapFour["tist"].AsString());
            Assert.AreEqual("last", llsdMapFour["tast"].AsString());
            OSDMap llsdMapFive = (OSDMap)llsdMapFour["test"];
            Assert.AreEqual(OSDType.Map, llsdMapFive.Type);
            Assert.AreEqual(2, llsdMapFive.Count);
            Assert.AreEqual(OSDType.Integer, llsdMapFive["test"].Type);
            Assert.AreEqual(1, llsdMapFive["test"].AsInteger());
            Assert.AreEqual(OSDType.Real, llsdMapFive["t0st"].Type);
            Assert.AreEqual(2.5d, llsdMapFive["t0st"].AsReal());

        }

        [Test()]
        public void SerializeMap()
        {
            OSDMap llsdOne = new OSDMap();
            string sOne = OSDParser.SerializeLLSDNotation(llsdOne);
            OSDMap llsdOneDS = (OSDMap)OSDParser.DeserializeLLSDNotation(sOne);
            Assert.AreEqual(OSDType.Map, llsdOneDS.Type);
            Assert.AreEqual(0, llsdOneDS.Count);

            OSD llsdTwo = OSD.FromInteger(123234);
            OSD llsdThree = OSD.FromString("asedkfjhaqweiurohzasdf");
            OSDMap llsdFour = new OSDMap();
            llsdFour["test0"] = llsdTwo;
            llsdFour["test1"] = llsdThree;

            llsdOne["test0"] = llsdTwo;
            llsdOne["test1"] = llsdThree;
            llsdOne["test2"] = llsdFour;

            string sFive = OSDParser.SerializeLLSDNotation(llsdOne);
            OSDMap llsdFive = (OSDMap)OSDParser.DeserializeLLSDNotation(sFive);
            Assert.AreEqual(OSDType.Map, llsdFive.Type);
            Assert.AreEqual(3, llsdFive.Count);
            Assert.AreEqual(OSDType.Integer, llsdFive["test0"].Type);
            Assert.AreEqual(123234, llsdFive["test0"].AsInteger());
            Assert.AreEqual(OSDType.String, llsdFive["test1"].Type);
            Assert.AreEqual("asedkfjhaqweiurohzasdf", llsdFive["test1"].AsString());

            OSDMap llsdSix = (OSDMap)llsdFive["test2"];
            Assert.AreEqual(OSDType.Map, llsdSix.Type);
            Assert.AreEqual(2, llsdSix.Count);
            Assert.AreEqual(OSDType.Integer, llsdSix["test0"].Type);
            Assert.AreEqual(123234, llsdSix["test0"].AsInteger());
            Assert.AreEqual(OSDType.String, llsdSix["test1"].Type);
            Assert.AreEqual("asedkfjhaqweiurohzasdf", llsdSix["test1"].AsString());

            // We test here also for 4byte characters as map keys
            string xml = "<x>&#x10137;</x>";
            byte[] bytes = Encoding.UTF8.GetBytes(xml);
            XmlTextReader xtr = new XmlTextReader(new MemoryStream(bytes, false));
            xtr.Read();
            xtr.Read();
            string content = xtr.ReadString();

            OSDMap llsdSeven = new OSDMap();
            llsdSeven[content] = OSD.FromString(content);
            string sSeven = OSDParser.SerializeLLSDNotation(llsdSeven);
            OSDMap llsdSevenDS = (OSDMap)OSDParser.DeserializeLLSDNotation(sSeven);
            Assert.AreEqual(OSDType.Map, llsdSevenDS.Type);
            Assert.AreEqual(1, llsdSevenDS.Count);
            Assert.AreEqual(content, llsdSevenDS[content].AsString());
        }

        [Test()]
        public void DeserializeRealWorldExamples()
        {
            string realWorldExample = @"
[
  {'destination':'http://secondlife.com'}, 
  {'version':i1}, 
  {
    'agent_id':u3c115e51-04f4-523c-9fa6-98aff1034730, 
    'session_id':u2c585cec-038c-40b0-b42e-a25ebab4d132, 
    'circuit_code':i1075, 
    'first_name':'Phoenix', 
    'last_name':'Linden',
    'position':[r70.9247,r254.378,r38.7304], 
    'look_at':[r-0.043753,r-0.999042,r0], 
    'granters':[ua2e76fcd-9360-4f6d-a924-000000000003],
    'attachment_data':
    [
      {
        'attachment_point':i2,
        'item_id':ud6852c11-a74e-309a-0462-50533f1ef9b3,
        'asset_id':uc69b29b1-8944-58ae-a7c5-2ca7b23e22fb
      },
      {
        'attachment_point':i10, 
        'item_id':uff852c22-a74e-309a-0462-50533f1ef900,
        'asset_id':u5868dd20-c25a-47bd-8b4c-dedc99ef9479
      }
    ]
  }
]";
            // We dont do full testing here. We are fine if a few values are right
            // and the parser doesnt throw an exception
            OSDArray llsdArray = (OSDArray)OSDParser.DeserializeLLSDNotation(realWorldExample);
            Assert.AreEqual(OSDType.Array, llsdArray.Type);
            Assert.AreEqual(3, llsdArray.Count);

            OSDMap llsdMapOne = (OSDMap)llsdArray[0];
            Assert.AreEqual(OSDType.Map, llsdMapOne.Type);
            Assert.AreEqual("http://secondlife.com", llsdMapOne["destination"].AsString());

            OSDMap llsdMapTwo = (OSDMap)llsdArray[1];
            Assert.AreEqual(OSDType.Map, llsdMapTwo.Type);
            Assert.AreEqual(OSDType.Integer, llsdMapTwo["version"].Type);
            Assert.AreEqual(1, llsdMapTwo["version"].AsInteger());

            OSDMap llsdMapThree = (OSDMap)llsdArray[2];
            Assert.AreEqual(OSDType.UUID, llsdMapThree["session_id"].Type);
            Assert.AreEqual("2c585cec-038c-40b0-b42e-a25ebab4d132", llsdMapThree["session_id"].AsString());
            Assert.AreEqual(OSDType.UUID, llsdMapThree["agent_id"].Type);
            Assert.AreEqual("3c115e51-04f4-523c-9fa6-98aff1034730", llsdMapThree["agent_id"].AsString());

        }

        [Test()]
        public void SerializeFormattedTest()
        {
            // This is not a real test. Instead look at the console.out tab for how formatted notation looks like.
            OSDArray llsdArray = new OSDArray();
            OSD llsdOne = OSD.FromInteger(1);
            OSD llsdTwo = OSD.FromInteger(1);
            llsdArray.Add(llsdOne);
            llsdArray.Add(llsdTwo);

            OSDMap llsdMap = new OSDMap();
            OSD llsdThree = OSD.FromInteger(2);
            llsdMap["test1"] = llsdThree;
            OSD llsdFour = OSD.FromInteger(2);
            llsdMap["test2"] = llsdFour;

            llsdArray.Add(llsdMap);

            OSDArray llsdArrayTwo = new OSDArray();
            OSD llsdFive = OSD.FromString("asdflkhjasdhj");
            OSD llsdSix = OSD.FromString("asdkfhasjkldfghsd");
            llsdArrayTwo.Add(llsdFive);
            llsdArrayTwo.Add(llsdSix);

            llsdMap["test3"] = llsdArrayTwo;

            string sThree = OSDParser.SerializeLLSDNotationFormatted(llsdArray);

            // we also try to parse this... and look a little at the results 
            OSDArray llsdSeven = (OSDArray)OSDParser.DeserializeLLSDNotation(sThree);
            Assert.AreEqual(OSDType.Array, llsdSeven.Type);
            Assert.AreEqual(3, llsdSeven.Count);
            Assert.AreEqual(OSDType.Integer, llsdSeven[0].Type);
            Assert.AreEqual(1, llsdSeven[0].AsInteger());
            Assert.AreEqual(OSDType.Integer, llsdSeven[1].Type);
            Assert.AreEqual(1, llsdSeven[1].AsInteger());

            Assert.AreEqual(OSDType.Map, llsdSeven[2].Type);
            // thats enough for now.            
        }
    }
}
