// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.CodeAnalysis.Sarif
{
    [TestClass]
    public class ExtensionsTests
    {
        [TestMethod]
        public void Extensions_PropertValue_NullProperties()
        {
            Dictionary<string, string> uut = null;
            Assert.IsNull(uut.PropertyValue("anything"));
        }

        [TestMethod]
        public void Extensions_PropertValue_UnsetValue()
        {
            var uut = new Dictionary<string, string>();
            Assert.IsNull(uut.PropertyValue("anything"));
        }

        [TestMethod]
        public void Extensions_PropertValue_SetValue()
        {
            var uut = new Dictionary<string, string>();
            uut.Add("anything", "the value");
            Assert.AreEqual("the value", uut.PropertyValue("anything"));
        }

        [TestMethod]
        public void Extensions_IsNewline_CarriageReturn()
        {
            Assert.IsTrue(Extensions.IsNewline('\r'));
        }

        [TestMethod]
        public void Extensions_IsNewline_LineFeed()
        {
            Assert.IsTrue(Extensions.IsNewline('\n'));
        }

        [TestMethod]
        public void Extensions_IsNewline_UnicodeLine()
        {
            Assert.IsTrue(Extensions.IsNewline('\u2028'));
        }

        [TestMethod]
        public void Extensions_IsNewline_UnicodeParagraph()
        {
            Assert.IsTrue(Extensions.IsNewline('\u2029'));
        }

        [TestMethod]
        public void Extensions_IsNewline_Other()
        {
            Assert.IsFalse(Extensions.IsNewline('E'));
        }

        private static readonly char[] s_testArray = "  match   ".ToCharArray();
        // 0123456789

        [TestMethod]
        public void Extensions_ArrayMatches_NegativeStartIndex()
        {
            Assert.IsFalse(Extensions.ArrayMatches(s_testArray, -1, "match"));
        }

        [TestMethod]
        public void Extensions_ArrayMatches_TooLong()
        {
            Assert.IsFalse(Extensions.ArrayMatches(s_testArray, 6, "match"));
        }

        [TestMethod]
        public void Extensions_ArrayMatches_Match()
        {
            Assert.IsTrue(Extensions.ArrayMatches(s_testArray, 2, "match"));
        }

        [TestMethod]
        public void Extensions_ArrayMatches_Mismatch()
        {
            Assert.IsFalse(Extensions.ArrayMatches(s_testArray, 0, "match"));
        }

        [TestMethod]
        public void Extensions_XmlCreateException_WithLineInfo()
        {
            // 0000000001111111111222222
            // 1234567890123456789012345
            using (XmlReader unitUnderTest = Utilities.CreateXmlReaderFromString("<hello> <world/> </hello>"))
            {
                unitUnderTest.Read(); // <hello>
                unitUnderTest.Read(); // <world>
                XmlException result = unitUnderTest.CreateException("cute fluffy kittens");
                Assert.AreEqual(1, result.LineNumber);
                Assert.AreEqual(8, result.LinePosition);
            }
        }

        [TestMethod]
        public void Extensions_XmlCreateException_WithoutLineInfo()
        {
            var testData = new XElement("hello", new XElement("world"));
            using (XmlReader unitUnderTest = testData.CreateReader())
            {
                unitUnderTest.Read(); // <hello>
                unitUnderTest.Read(); // <world />
                XmlException result = unitUnderTest.CreateException("hungry EVIL zombies");
                Assert.AreEqual(0, result.LineNumber);
                Assert.AreEqual(0, result.LinePosition);
            }
        }

        [TestMethod]
        public void Extensions_XmlCreateException_WithFormat()
        {
            var testData = new XElement("hello", new XElement("world"));
            using (XmlReader unitUnderTest = testData.CreateReader())
            {
                XmlException result = unitUnderTest.CreateException("hungry {0} zombies", "evil");
                Assert.AreEqual("hungry evil zombies", result.Message);
            }
        }

        private const string simpleXmlDoc = "<xml><skip_this>expected child content</skip_this><following/></xml>";

        [TestMethod]
        public void Extensions_XmlIgnoreElementContent_Required_Success()
        {
            using (XmlReader xml = Utilities.CreateXmlReaderFromString(simpleXmlDoc))
            {
                xml.ReadStartElement("xml");
                xml.IgnoreElement(xml.NameTable.Add("skip_this"), IgnoreOptions.Required);
                Assert.AreEqual("following", xml.LocalName);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(XmlException))]
        public void Extensions_XmlIgnoreElement_Required_Failure_BadName()
        {
            using (XmlReader xml = Utilities.CreateXmlReaderFromString(simpleXmlDoc))
            {
                xml.Read(); // <xml>
                xml.IgnoreElement(xml.NameTable.Add("skip_this"), IgnoreOptions.Required);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(XmlException))]
        public void Extensions_XmlIgnoreElement_Required_Failure_BadNodeType()
        {
            using (XmlReader xml = Utilities.CreateXmlReaderFromString(simpleXmlDoc))
            {
                xml.Read(); // Position on <xml>
                xml.Read(); // Position on <skip_this>
                xml.Read(); // Position on simple content
                xml.Read(); // Position on </skip_this>
                xml.IgnoreElement(xml.NameTable.Add("skip_this"), IgnoreOptions.Required);
            }
        }

        [TestMethod]
        public void Extensions_XmlIgnoreElement_Optional_Success()
        {
            using (XmlReader xml = Utilities.CreateXmlReaderFromString(simpleXmlDoc))
            {
                xml.ReadStartElement("xml");
                xml.IgnoreElement(xml.NameTable.Add("skip_this"), IgnoreOptions.Optional);
                Assert.AreEqual("following", xml.LocalName);
            }
        }

        [TestMethod]
        public void Extensions_XmlIgnoreElement_Optional_Failure_BadName()
        {
            using (XmlReader xml = Utilities.CreateXmlReaderFromString(simpleXmlDoc))
            {
                xml.Read(); // Position on <xml>
                xml.IgnoreElement(xml.NameTable.Add("skip_this"), IgnoreOptions.Optional);
                Assert.AreEqual("xml", xml.LocalName);
            }
        }

        [TestMethod]
        public void Extensions_XmlIgnoreElement_Optional_Failure_BadNodeType()
        {
            using (XmlReader xml = Utilities.CreateXmlReaderFromString(simpleXmlDoc))
            {
                xml.Read(); // Position on <xml>
                xml.Read(); // Position on <skip_this>
                xml.Read(); // Position on simple content
                xml.Read(); // Position on </skip_this>
                xml.IgnoreElement(xml.NameTable.Add("skip_this"), IgnoreOptions.Optional);
                xml.Read();
                Assert.AreEqual("following", xml.LocalName);
            }
        }

        [TestMethod]
        public void Extensions_XmlIgnoreElement_OptionalMulti_Failure_BadNodeType()
        {
            using (XmlReader xml = Utilities.CreateXmlReaderFromString(simpleXmlDoc))
            {
                xml.Read(); // Position on <xml>
                xml.Read(); // Position on <skip_this>
                xml.Read(); // Position on simple content
                xml.Read(); // Position on </skip_this>
                xml.IgnoreElement(xml.NameTable.Add("skip_this"), IgnoreOptions.Optional | IgnoreOptions.Multiple);
                xml.Read();
                Assert.AreEqual("following", xml.LocalName);
            }
        }

        private const string simpleXmlMulitDoc = "<xml><xml>child <unused /> content</xml><xml>following</xml></xml>";

        [TestMethod]
        public void Extensions_XmlIgnoreElement_Singular_DoesNotOverread()
        {
            using (XmlReader xml = Utilities.CreateXmlReaderFromString(simpleXmlMulitDoc))
            {
                xml.Read(); // Position on <xml> at depth 0
                xml.Read(); // Position on <xml> at depth 1
                xml.IgnoreElement(xml.NameTable.Add("xml"), IgnoreOptions.Optional);
                Assert.AreEqual(XmlNodeType.Element, xml.NodeType);
                Assert.AreEqual("following", xml.ReadElementContentAsString());
            }
        }

        [TestMethod]
        public void Extensions_XmlIgnoreElement_Multiple_DoesNotOverread()
        {
            using (XmlReader xml = Utilities.CreateXmlReaderFromString(simpleXmlMulitDoc))
            {
                xml.Read(); // Position on <xml> at depth 0
                xml.Read(); // Position on <xml> at depth 1
                xml.IgnoreElement(xml.NameTable.Add("xml"), IgnoreOptions.Multiple);
                Assert.AreEqual(XmlNodeType.EndElement, xml.NodeType);
                Assert.AreEqual(0, xml.Depth);
            }
        }

        [TestMethod]
        public void Extensions_XmlIgnoreElement_Multiple_WhenRequiredReadsZeroElements()
        {
            using (XmlReader xml = Utilities.CreateXmlReaderFromString(simpleXmlMulitDoc))
            {
                xml.Read(); // Position on <xml> at depth 0
                xml.Read(); // Position on <xml> at depth 1
                try
                {
                    xml.IgnoreElement(xml.NameTable.Add("different_element"), IgnoreOptions.Multiple);
                    Assert.Fail();
                }
                catch (XmlException) { }
                Assert.AreEqual(XmlNodeType.Element, xml.NodeType);
                Assert.AreEqual("xml", xml.LocalName);
                Assert.AreEqual(1, xml.Depth);
            }
        }

        [TestMethod]
        public void Extensions_XmlReadOptionalElementContentAsString_Success()
        {
            using (XmlReader xml = Utilities.CreateXmlReaderFromString(simpleXmlDoc))
            {
                xml.ReadStartElement("xml");
                string elementName = xml.NameTable.Add("skip_this");
                Assert.AreEqual("expected child content", xml.ReadOptionalElementContentAsString(elementName));
            }
        }

        [TestMethod]
        public void Extensions_XmlReadOptionalElementContentAsString_Failure_BadNodeType()
        {
            using (XmlReader xml = Utilities.CreateXmlReaderFromString(simpleXmlDoc))
            {
                xml.ReadStartElement("xml");
                xml.ReadStartElement("skip_this"); // Position in the simple content inside <skip_this/>
                xml.Read(); // Now on end element
                Assert.AreEqual(XmlNodeType.EndElement, xml.NodeType);
                string elementName = xml.NameTable.Add("skip_this");
                Assert.IsNull(xml.ReadOptionalElementContentAsString(elementName));
            }
        }

        [TestMethod]
        public void Extensions_XmlReadOptionalElementContentAsString_Failure_BadName()
        {
            using (XmlReader xml = Utilities.CreateXmlReaderFromString(simpleXmlDoc))
            {
                xml.ReadStartElement("xml");
                string elementName = xml.NameTable.Add("bad_name");
                Assert.IsNull(xml.ReadOptionalElementContentAsString(elementName));
            }
        }

        private static readonly XElement s_consumeElementOfDepthTestDocument =
            new XElement("root",
                new XElement("empty_child"),
                new XElement("content_child", "content"),
                new XElement("nodes_child", new XElement("node", "node content")),
                new XElement("deep_child", new XElement("node", new XElement("node")))
                );

        [TestMethod]
        public void Extensions_ConsumeElementOfDepth_AtLesserDepthTakesNoAction()
        {
            using (var xml = s_consumeElementOfDepthTestDocument.CreateReader())
            {
                xml.ConsumeElementOfDepth(1); // Already at endElementDepth 0, should have no effect
                Assert.AreEqual(ReadState.Initial, xml.ReadState);
            }
        }

        [TestMethod]
        public void Extensions_ConsumeElementOfDepth_ConsumesEmptyElement()
        {
            using (var xml = s_consumeElementOfDepthTestDocument.CreateReader())
            {
                Assert.IsTrue(xml.ReadToDescendant("empty_child"));
                Assert.IsTrue(xml.IsEmptyElement);
                xml.ConsumeElementOfDepth(1);
                Assert.AreEqual("content_child", xml.LocalName);
            }
        }

        [TestMethod]
        public void Extensions_ConsumeElementOfDepth_ConsumesElementWithChildren()
        {
            using (var xml = s_consumeElementOfDepthTestDocument.CreateReader())
            {
                Assert.IsTrue(xml.ReadToDescendant("nodes_child"));
                xml.ConsumeElementOfDepth(1);
                Assert.AreEqual(XmlNodeType.Element, xml.NodeType);
                Assert.AreEqual("deep_child", xml.LocalName);
            }
        }

        [TestMethod]
        public void Extensions_ConsumeElementOfDepth_ConsumesWhenAlreadyInsideElement()
        {
            using (var xml = s_consumeElementOfDepthTestDocument.CreateReader())
            {
                Assert.IsTrue(xml.ReadToDescendant("nodes_child"));
                Assert.IsTrue(xml.ReadToDescendant("node"));
                xml.Read();
                Assert.AreEqual(XmlNodeType.Text, xml.NodeType); // node content
                xml.ConsumeElementOfDepth(1);
                Assert.AreEqual(XmlNodeType.Element, xml.NodeType);
                Assert.AreEqual("deep_child", xml.LocalName);
            }
        }

        [TestMethod]
        public void Extensions_ConsumeElementOfDepth_ConsumesEndElement()
        {
            using (var xml = s_consumeElementOfDepthTestDocument.CreateReader())
            {
                Assert.IsTrue(xml.ReadToDescendant("nodes_child"));
                Assert.IsTrue(xml.ReadToDescendant("node"));
                Assert.IsTrue(xml.Read()); // node content
                Assert.IsTrue(xml.Read()); // </node> -- This test is that we consume the end element when we're standing on it
                Assert.AreEqual(XmlNodeType.EndElement, xml.NodeType);
                xml.ConsumeElementOfDepth(2);
                Assert.AreEqual(XmlNodeType.Element, xml.NodeType);
                Assert.AreEqual("deep_child", xml.LocalName);
            }
        }
    }
}
