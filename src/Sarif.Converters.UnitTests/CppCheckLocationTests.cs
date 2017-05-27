// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Xml;
using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    [TestClass]
    public class CppCheckLocationTests
    {
        [TestMethod]
        public void CppCheckLocation_CanBeConstructedFromFileAndLine()
        {
            var uut = new CppCheckLocation("1234", 42);
            Assert.AreEqual("1234", uut.File);
            Assert.AreEqual(42, uut.Line);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CppCheckLocation_RejectsNullFile()
        {
            new CppCheckLocation("   ", 42);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void CppCheckLocation_RejectsNegativeLineNumbers()
        {
            new CppCheckLocation("file.cpp", -1);
        }

        [TestMethod]
        public void CppCheckLocation_SatisfiesEqualityInvariants()
        {
            var a = new CppCheckLocation("a.cpp", 1);
            var b = new CppCheckLocation("a.cpp", 2);
            var c = new CppCheckLocation("b.cpp", 1);

            Assert.AreNotEqual(a, b);
            Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());
            Assert.AreNotEqual(a, c);
            Assert.AreNotEqual(a.GetHashCode(), c.GetHashCode());
            Assert.AreNotEqual(b, c);
            Assert.AreNotEqual(b.GetHashCode(), c.GetHashCode());

            var anotherA = new CppCheckLocation("a.cpp", 1);
            Assert.AreEqual(a, anotherA);
            Assert.AreEqual(a.GetHashCode(), anotherA.GetHashCode());
            Assert.IsTrue(a == anotherA);
            Assert.IsFalse(a != anotherA);
            Assert.IsFalse(a.Equals(null));
            Assert.IsFalse(a.Equals("a string value"));
        }

        [TestMethod]
        public void CppCheckLocation_CanBeDebugPrinted()
        {
            string result = new CppCheckLocation("cute_fluffy_kittens.c", 1234).ToString();
            result.Should().Contain("cute_fluffy_kittens.c");
            result.Should().Contain("1234");
        }

        [TestMethod]
        public void CppCheckLocation_CanBeConvertedToSarifIssue()
        {
            PhysicalLocation result = new CppCheckLocation("foo.cpp", 42).ToSarifPhysicalLocation();
            Assert.IsTrue(
                result.ValueEquals(
                    new PhysicalLocation
                    {
                        Uri = new Uri("foo.cpp", UriKind.RelativeOrAbsolute),
                        Region = new Region { StartLine = 42 }
                    }));
        }

        [TestMethod]
        public void CppCheckLocation_CanParseSelfClosingXmlNode()
        {
            using (XmlReader xml = Utilities.CreateXmlReaderFromString(testLocationXml))
            {
                AssertParsesAsTestLocation(xml);
            }
        }

        [TestMethod]
        public void CppCheckLocation_SkipsToNextXmlNode()
        {
            using (XmlReader xml = Utilities.CreateXmlReaderFromString("<error> " + testLocationXml + "   <followingNode /> </error>"))
            {
                xml.ReadToDescendant("location");
                AssertParsesAsTestLocation(xml);
                xml.Read();
                Assert.AreEqual("followingNode", xml.LocalName);
            }
        }

        [TestMethod]
        public void CppCheckLocation_SkipsToEndElement()
        {
            using (XmlReader xml = Utilities.CreateXmlReaderFromString("<error>  " + testLocationXml + " </error>"))
            {
                xml.ReadToDescendant("location");
                AssertParsesAsTestLocation(xml);
                xml.Read();
                Assert.AreEqual("error", xml.LocalName);
                Assert.AreEqual(XmlNodeType.EndElement, xml.NodeType);
            }
        }

        [TestMethod]
        public void CppCheckLocation_SkipsSubNodesOfLocation()
        {
            using (XmlReader xml = Utilities.CreateXmlReaderFromString("<root><location file=\"foo.cpp\" line=\"1234\"> <child /> <nodes /> </location><followingNode /></root>"))
            {
                xml.ReadStartElement("root");
                AssertParsesAsTestLocation(xml);
                Assert.AreEqual("followingNode", xml.LocalName);
            }
        }

        private const string testLocationXml = "<location file=\"foo.cpp\" line=\"1234\" />";

        private static CppCheckLocation AssertLocationIsTestLocation(CppCheckLocation result)
        {
            Assert.AreEqual("foo.cpp", result.File);
            Assert.AreEqual(1234, result.Line);
            return result;
        }

        private static void AssertParsesAsTestLocation(XmlReader xml)
        {
            AssertLocationIsTestLocation(Parse(xml));
        }

        [TestMethod]
        [ExpectedException(typeof(XmlException))]
        public void CppCheckLocation_Invalid_ThrowsXmlExceptionForNonLocationNode()
        {
            using (XmlReader xml = Utilities.CreateXmlReaderFromString("<exclaim><thatsNotALocationNode /></exclaim>"))
            {
                Parse(xml);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(XmlException))]
        public void CppCheckLocation_Invalid_ThrowsXmlExceptionForMissingFile()
        {
            using (XmlReader xml = Utilities.CreateXmlReaderFromString("<location line=\"42\" />"))
            {
                Parse(xml);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(XmlException))]
        public void CppCheckLocation_Invalid_ThrowsXmlExceptionForMissingLine()
        {
            using (XmlReader xml = Utilities.CreateXmlReaderFromString("<location file=\"foo.cpp\" />"))
            {
                Parse(xml);
            }
        }

        private static CppCheckLocation Parse(XmlReader xml)
        {
            return CppCheckLocation.Parse(xml, new CppCheckStrings(xml.NameTable));
        }
    }
}
