// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Xml;
using FluentAssertions;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    public class CppCheckLocationTests
    {
        private const string ExampleFileName = "example.cpp";

        [Fact]
        public void CppCheckLocation_CanBeConstructedFromFileAndLine()
        {
            var uut = new CppCheckLocation("1234", 42);
            Assert.Equal("1234", uut.File);
            Assert.Equal(42, uut.Line);
        }

        [Fact]
        public void CppCheckLocation_RejectsNullFile()
        {
            Assert.Throws<ArgumentException>(() => new CppCheckLocation("   ", 42));
        }

        [Fact]
        public void CppCheckLocation_RejectsNegativeLineNumbers()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new CppCheckLocation("file.cpp", -1));
        }

        [Fact]
        public void CppCheckLocation_SatisfiesEqualityInvariants()
        {
            var a = new CppCheckLocation("a.cpp", 1);
            var b = new CppCheckLocation("a.cpp", 2);
            var c = new CppCheckLocation("b.cpp", 1);

            Assert.NotEqual(a, b);
            Assert.NotEqual(a.GetHashCode(), b.GetHashCode());
            Assert.NotEqual(a, c);
            Assert.NotEqual(a.GetHashCode(), c.GetHashCode());
            Assert.NotEqual(b, c);
            Assert.NotEqual(b.GetHashCode(), c.GetHashCode());

            var anotherA = new CppCheckLocation("a.cpp", 1);
            Assert.Equal(a, anotherA);
            Assert.Equal(a.GetHashCode(), anotherA.GetHashCode());
            Assert.True(a == anotherA);
            Assert.False(a != anotherA);
            Assert.False(a.Equals(null));
            Assert.False(a.Equals("a string value"));
        }

        [Fact]
        public void CppCheckLocation_CanBeDebugPrinted()
        {
            string result = new CppCheckLocation("cute_fluffy_kittens.c", 1234).ToString();
            result.Should().Contain("cute_fluffy_kittens.c");
            result.Should().Contain("1234");
        }

        [Fact]
        public void CppCheckLocation_CanBeConvertedToSarifIssue()
        {
            PhysicalLocation result = new CppCheckLocation(ExampleFileName, 42).ToSarifPhysicalLocation();
            Assert.True(
                result.ValueEquals(
                    new PhysicalLocation
                    {
                        ArtifactLocation = new ArtifactLocation
                        {
                            Uri = new Uri(ExampleFileName, UriKind.RelativeOrAbsolute)
                        },
                        Region = new Region { StartLine = 42 }
                    }));
        }

        [Fact]
        public void CppCheckLocation_CanParseSelfClosingXmlNode()
        {
            using (XmlReader xml = Utilities.CreateXmlReaderFromString(testLocationXml))
            {
                AssertParsesAsTestLocation(xml);
            }
        }

        [Fact]
        public void CppCheckLocation_SkipsToNextXmlNode()
        {
            using (XmlReader xml = Utilities.CreateXmlReaderFromString("<error> " + testLocationXml + "   <followingNode /> </error>"))
            {
                xml.ReadToDescendant("location");
                AssertParsesAsTestLocation(xml);
                xml.Read();
                Assert.Equal("followingNode", xml.LocalName);
            }
        }

        [Fact]
        public void CppCheckLocation_SkipsToEndElement()
        {
            using (XmlReader xml = Utilities.CreateXmlReaderFromString("<error>  " + testLocationXml + " </error>"))
            {
                xml.ReadToDescendant("location");
                AssertParsesAsTestLocation(xml);
                xml.Read();
                Assert.Equal("error", xml.LocalName);
                Assert.Equal(XmlNodeType.EndElement, xml.NodeType);
            }
        }

        [Fact]
        public void CppCheckLocation_SkipsSubNodesOfLocation()
        {
            using (XmlReader xml = Utilities.CreateXmlReaderFromString("<root><location file=\"" + ExampleFileName + "\" line=\"1234\"> <child /> <nodes /> </location><followingNode /></root>"))
            {
                xml.ReadStartElement("root");
                AssertParsesAsTestLocation(xml);
                Assert.Equal("followingNode", xml.LocalName);
            }
        }

        private const string testLocationXml = "<location file=\"" + ExampleFileName + "\" line=\"1234\" />";

        private static CppCheckLocation AssertLocationIsTestLocation(CppCheckLocation result)
        {
            Assert.Equal(ExampleFileName, result.File);
            Assert.Equal(1234, result.Line);
            return result;
        }

        private static void AssertParsesAsTestLocation(XmlReader xml)
        {
            AssertLocationIsTestLocation(Parse(xml));
        }

        [Fact]
        public void CppCheckLocation_Invalid_ThrowsXmlExceptionForNonLocationNode()
        {
            using (XmlReader xml = Utilities.CreateXmlReaderFromString("<exclaim><thatsNotALocationNode /></exclaim>"))
            {
                Assert.Throws<XmlException>(() => Parse(xml));
            }
        }

        [Fact]
        public void CppCheckLocation_Invalid_ThrowsXmlExceptionForMissingFile()
        {
            using (XmlReader xml = Utilities.CreateXmlReaderFromString("<location line=\"42\" />"))
            {
                Assert.Throws<XmlException>(() => Parse(xml));
            }
        }

        [Fact]
        public void CppCheckLocation_Invalid_ThrowsXmlExceptionForMissingLine()
        {
            using (XmlReader xml = Utilities.CreateXmlReaderFromString("<location file=\"" + ExampleFileName + "\" />"))
            {
                Assert.Throws<XmlException>(() => Parse(xml));
            }
        }

        private static CppCheckLocation Parse(XmlReader xml)
        {
            return CppCheckLocation.Parse(xml, new CppCheckStrings(xml.NameTable));
        }
    }
}
