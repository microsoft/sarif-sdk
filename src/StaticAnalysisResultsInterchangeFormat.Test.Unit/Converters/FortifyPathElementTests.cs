// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Xml;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.CodeAnalysis.StaticAnalysisResultsInterchangeFormat.Converters
{
    [TestClass]
    public class FortifyPathElementTests
    {
        [TestMethod]
        public void FortifyPathElement_CanBeConstructed()
        {
            var uut = new FortifyPathElement("file", 42, "targetFunction");
            Assert.AreEqual("file", uut.FilePath);
            Assert.AreEqual(42, uut.LineStart);
            Assert.AreEqual("targetFunction", uut.TargetFunction);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FortifyPathElement_RequiresFile()
        {
            new FortifyPathElement(null, 42, "target");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void FortifyPathElement_RequiresPositiveLine_NonZero()
        {
            new FortifyPathElement("file", 0, "target");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void FortifyPathElement_RequiresPositiveLine_NonNegative()
        {
            new FortifyPathElement("file", -1, "target");
        }

        [TestMethod]
        public void FortifyPathElement_Parse_MinimalValidElement()
        {
            const string xml = @"<does_not_matter><FileName>FileName</FileName><FilePath>FilePath</FilePath><LineStart>42</LineStart></does_not_matter><following />";
            FortifyPathElement result = Parse(xml);
            Assert.AreEqual("FilePath", result.FilePath);
            Assert.AreEqual(42, result.LineStart);
            Assert.IsNull(result.TargetFunction);
        }

        [TestMethod]
        public void FortifyPathElement_Parse_WithSnippet()
        {
            const string xml = @"<does_not_matter><FileName>FileName</FileName><FilePath>FilePath</FilePath><LineStart>42</LineStart><Snippet>unused</Snippet></does_not_matter><following />";
            FortifyPathElement result = Parse(xml);
            Assert.AreEqual("FilePath", result.FilePath);
            Assert.AreEqual(42, result.LineStart);
            Assert.IsNull(result.TargetFunction);
        }

        [TestMethod]
        public void FortifyPathElement_Parse_WithSnippetLine()
        {
            const string xml = @"<does_not_matter><FileName>FileName</FileName><FilePath>FilePath</FilePath><LineStart>42</LineStart><SnippetLine>unused</SnippetLine></does_not_matter><following />";
            FortifyPathElement result = Parse(xml);
            Assert.AreEqual("FilePath", result.FilePath);
            Assert.AreEqual(42, result.LineStart);
            Assert.IsNull(result.TargetFunction);
        }

        [TestMethod]
        public void FortifyPathElement_Parse_WithTargetFunction()
        {
            const string xml = @"<does_not_matter><FileName>FileName</FileName><FilePath>FilePath</FilePath><LineStart>42</LineStart><TargetFunction>vector&lt;string&gt; example::member(int, int) const&amp;&amp;</TargetFunction></does_not_matter><following />";
            FortifyPathElement result = Parse(xml);
            Assert.AreEqual("FilePath", result.FilePath);
            Assert.AreEqual(42, result.LineStart);
            Assert.AreEqual("vector<string> example::member(int, int) const&&", result.TargetFunction);
        }

        [TestMethod]
        public void FortifyPathElement_Parse_WithAllProperties()
        {
            const string xml = @"<does_not_matter><FileName>FileName</FileName><FilePath>FilePath</FilePath><LineStart>42</LineStart><Snippet></Snippet><SnippetLine></SnippetLine><TargetFunction>vector&lt;string&gt; example::member(int, int) const&amp;&amp;</TargetFunction></does_not_matter><following />";
            FortifyPathElement result = Parse(xml);
            Assert.AreEqual("FilePath", result.FilePath);
            Assert.AreEqual(42, result.LineStart);
            Assert.AreEqual("vector<string> example::member(int, int) const&&", result.TargetFunction);
        }

        [TestMethod]
        [ExpectedException(typeof(XmlException))]
        public void FortifyPathElement_Parse_MustBeOnElement()
        {
            using (XmlReader reader = Utilities.CreateXmlReaderFromString("<xml />"))
            {
                Assert.AreNotEqual(XmlNodeType.Element, reader.NodeType);
                Parse(reader);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(XmlException))]
        public void FortifyPathElement_Parse_MustNotBeEmptyElement()
        {
            Parse("<xml />");
        }

        [TestMethod]
        [ExpectedException(typeof(XmlException))]
        public void FortifyPathElement_Parse_MustBeInOrder()
        {
            // FileName and FilePath have order switched
            const string xml = @"<does_not_matter><FilePath>FilePath</FilePath><FileName>FileName</FileName><LineStart>42</LineStart></does_not_matter><following />";
            Parse(xml);
        }

        [TestMethod]
        [ExpectedException(typeof(XmlException))]
        public void FortifyPathElement_Parse_MustNotContainExtraElements()
        {
            const string xml = @"<does_not_matter><FileName>FileName</FileName><FilePath>FilePath</FilePath><LineStart>42</LineStart><Snippet></Snippet><SnippetLine></SnippetLine><TargetFunction>vector&lt;string&gt; example::member(int, int) const&amp;&amp;</TargetFunction><ExtraBadElement /></does_not_matter><following />";
            Parse(xml);
        }

        [TestMethod]
        [ExpectedException(typeof(XmlException))]
        public void FortifyPathElement_Parse_MustMeetPathElementInvariants()
        {
            // Negative line number
            const string xml = @"<does_not_matter><FileName>FileName</FileName><FilePath>FilePath</FilePath><LineStart>-42</LineStart><Snippet></Snippet><SnippetLine></SnippetLine><TargetFunction>vector&lt;string&gt; example::member(int, int) const&amp;&amp;</TargetFunction></does_not_matter><following />";
            Parse(xml);
        }

        [TestMethod]
        [ExpectedException(typeof(XmlException))]
        public void FortifyPathElement_Parse_MustHaveFileName()
        {
            const string xml = @"<does_not_matter><FilePath>FilePath</FilePath><LineStart>42</LineStart></does_not_matter><following />";
            Parse(xml);
        }

        [TestMethod]
        [ExpectedException(typeof(XmlException))]
        public void FortifyPathElement_Parse_MustHaveFilePath()
        {
            const string xml = @"<does_not_matter><FileName>FileName</FileName><LineStart>42</LineStart></does_not_matter><following />";
            Parse(xml);
        }

        [TestMethod]
        [ExpectedException(typeof(XmlException))]
        public void FortifyPathElement_Parse_MustHaveLineStart()
        {
            const string xml = @"<does_not_matter><FileName>FileName</FileName><FilePath>FilePath</FilePath></does_not_matter><following />";
            Parse(xml);
        }

        private static FortifyPathElement Parse(string xml)
        {
            FortifyPathElement element;
            using (XmlReader reader = Utilities.CreateXmlReaderFromString(xml))
            {
                reader.Read(); // initial
                element = Parse(reader);
                Assert.AreEqual(XmlNodeType.Element, reader.NodeType);
                Assert.AreEqual("following", reader.LocalName);
            }

            return element;
        }

        private static FortifyPathElement Parse(XmlReader reader)
        {
            return FortifyPathElement.Parse(reader, new FortifyStrings(reader.NameTable));
        }
    }
}
