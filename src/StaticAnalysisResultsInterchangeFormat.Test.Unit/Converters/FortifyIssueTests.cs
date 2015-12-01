// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Immutable;
using System.Xml;
using System.Xml.Linq;
using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.CodeAnalysis.StaticAnalysisResultsInterchangeFormat.Converters
{
    [TestClass]
    public class FortifyIssueTests
    {
        private static readonly FortifyPathElement s_pathElementA =
            new FortifyPathElement("cute_fluffy_kittens.cpp", 42, "void Kitten::meow(int) const");
        private static readonly FortifyPathElement s_pathElementB =
            new FortifyPathElement("momma_cats.cpp", 1729, "void MommaCat::purr(int) const");
        private static readonly string s_fullIssueXml =
@"<Result ruleID=""7DDEC64A-9142-4943-BB5C-57D6F09C94DC"" iid=""BDC8FC3C5AAE67B07F46EC48B928AA6E"">
  <Category>Format String</Category>
  <Folder>High</Folder>
  <Kingdom>Input Validation and Representation</Kingdom>
  <Abstract>An attacker can control the format string argument to vfwprintf() at bannedAPIs.m line 225, allowing an attack much like a buffer overflow.</Abstract>
  <AbstractCustom>A message Bill added for testing purposes.</AbstractCustom>
  <Friority>High</Friority>
  <Tag><Name>name</Name><Value>value</Value></Tag>
  <Tag><Name>name</Name><Value>value</Value></Tag>
  <Tag><Name>name</Name><Value>value</Value></Tag>
  <Tag><Name>name</Name><Value>value</Value></Tag>
  <Comment><UserInfo>userInfo</UserInfo><Comment>Comment</Comment></Comment>
  <Comment><UserInfo>userInfo</UserInfo><Comment>Comment</Comment></Comment>
  <Comment><UserInfo>userInfo</UserInfo><Comment>Comment</Comment></Comment>
  <Comment><UserInfo>userInfo</UserInfo><Comment>Comment</Comment></Comment>
  <Primary>
    <FileName>bannedAPIs.m</FileName>
    <FilePath>bannedAPIs.m</FilePath>
    <LineStart>225</LineStart>
    <Snippet> Does Not Matter</Snippet>
  </Primary>
  <Source>
    <FileName>bannedAPIs.m</FileName>
    <FilePath>bannedAPIs.m</FilePath>
    <LineStart>238</LineStart>
    <Snippet> Also does not matter </Snippet>
  </Source>
  <ExternalCategory type = ""CWE"" > CWE ID 134</ExternalCategory>
</Result>";
        private static readonly string s_minimalIssueXml =
@"<Result>
  <Category>Format String</Category>
  <Folder>High</Folder>
  <Kingdom>Input Validation and Representation</Kingdom>
  <Primary>
    <FileName>bannedAPIs.m</FileName>
    <FilePath>bannedAPIs.m</FilePath>
    <LineStart>225</LineStart>
    <Snippet> Does Not Matter</Snippet>
  </Primary>
</Result>";


        [TestMethod]
        public void FortifyIssue_CanBeConstructed_WithAllProperties()
        {
            var uut = new FortifyIssue(
                "ruleId",
                "iid",
                "category",
                "kingdom",
                "abstract",
                "abstractCustom",
                "priority",
                s_pathElementA,
                s_pathElementB,
                ImmutableArray.Create(1, 2, 3)
                );

            Assert.AreEqual("ruleId", uut.RuleId);
            Assert.AreEqual("iid", uut.InstanceId);
            Assert.AreEqual("category", uut.Category);
            Assert.AreEqual("kingdom", uut.Kingdom);
            Assert.AreEqual("abstract", uut.Abstract);
            Assert.AreEqual("abstractCustom", uut.AbstractCustom);
            Assert.AreEqual("priority", uut.Priority);
            Assert.AreSame(s_pathElementA, uut.PrimaryOrSink);
            Assert.AreSame(s_pathElementB, uut.Source);
            uut.CweIds.Should().Equal(new[] { 1, 2, 3 });
        }

        [TestMethod]
        public void FortifyIssue_CanBeConstructed_WithMinimalProperties()
        {
            var uut = new FortifyIssue(
                null,
                null,
                "category",
                "kingdom",
                null,
                null,
                null,
                s_pathElementA,
                null,
                ImmutableArray<int>.Empty
                );

            Assert.IsNull(uut.RuleId);
            Assert.IsNull(uut.InstanceId);
            Assert.AreEqual("category", uut.Category);
            Assert.AreEqual("kingdom", uut.Kingdom);
            Assert.IsNull(uut.Abstract);
            Assert.IsNull(uut.AbstractCustom);
            Assert.IsNull(uut.Priority);
            Assert.AreSame(s_pathElementA, uut.PrimaryOrSink);
            Assert.IsNull(uut.Source);
            uut.CweIds.Should().BeEmpty();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FortifyIssue_RequiresCategory()
        {
            var uut = new FortifyIssue(
                null,
                null,
                null,
                "kingdom",
                null,
                null,
                null,
                s_pathElementA,
                null,
                ImmutableArray<int>.Empty
                );
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FortifyIssue_RequiresKingdom()
        {
            var uut = new FortifyIssue(
                null,
                null,
                "category",
                null,
                null,
                null,
                null,
                s_pathElementA,
                null,
                ImmutableArray<int>.Empty
                );
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FortifyIssue_RequiresPrimary()
        {
            var uut = new FortifyIssue(
                null,
                null,
                "category",
                "kingdom",
                null,
                null,
                null,
                null,
                null,
                ImmutableArray<int>.Empty
                );
        }

        [TestMethod]
        public void FortifyIssue_ParseCweIds_NoIds()
        {
            FortifyIssue.ParseCweIds("1234").Should().BeEmpty();
        }

        [TestMethod]
        public void FortifyIssue_ParseCweIds_SingleCweId()
        {
            FortifyIssue.ParseCweIds("CWE ID 476").Should().Equal(new[] { 476 });
        }

        [TestMethod]
        public void FortifyIssue_ParseCweIds_MultipleCweIds()
        {
            FortifyIssue.ParseCweIds("CWE ID 134, CWE ID 787").Should().Equal(new[] { 134, 787 });
        }

        [TestMethod]
        public void FortifyIssue_ParseCweIds_SortsCweIds()
        {
            FortifyIssue.ParseCweIds("CWE ID 787, CWE ID 134").Should().Equal(new[] { 134, 787 });
        }

        [TestMethod]
        public void FortifyIssue_ParseCweIds_UniquesCweIds()
        {
            FortifyIssue.ParseCweIds("CWE ID 134, CWE ID 134").Should().Equal(new[] { 134 });
        }

        [TestMethod]
        public void FortifyIssue_Parse_CanParseFullIssue()
        {
            string xml = "<xml>" + s_fullIssueXml + "<following /></xml>";
            using (XmlReader reader = Utilities.CreateWhitespaceSkippingXmlReaderFromString(xml))
            {
                reader.Read(); //<xml>
                reader.Read(); //<Result>
                FortifyIssue result = Parse(reader);
                Assert.AreEqual("following", reader.LocalName);
                Assert.AreEqual("7DDEC64A-9142-4943-BB5C-57D6F09C94DC", result.RuleId);
                Assert.AreEqual("BDC8FC3C5AAE67B07F46EC48B928AA6E", result.InstanceId);
                Assert.AreEqual("Format String", result.Category);
                Assert.AreEqual("Input Validation and Representation", result.Kingdom);
                Assert.AreEqual("An attacker can control the format string argument to vfwprintf() at bannedAPIs.m line 225, allowing an attack much like a buffer overflow.", result.Abstract);
                Assert.AreEqual("A message Bill added for testing purposes.", result.AbstractCustom);
                Assert.AreEqual("High", result.Priority);
                Assert.AreEqual(225, result.PrimaryOrSink.LineStart);
                Assert.AreEqual(238, result.Source.LineStart);
                result.CweIds.Should().Equal(new[] { 134 });
            }
        }

        [TestMethod]
        public void FortifyIssue_Parse_CanParseMinimalIssue()
        {
            string xml = "<xml>" + s_minimalIssueXml + "<following /></xml>";
            using (XmlReader reader = Utilities.CreateWhitespaceSkippingXmlReaderFromString(xml))
            {
                reader.Read(); //<xml>
                reader.Read(); //<Result>
                FortifyIssue result = Parse(reader);
                Assert.AreEqual("following", reader.LocalName);
                Assert.IsNull(result.RuleId);
                Assert.IsNull(result.InstanceId);
                Assert.AreEqual("Format String", result.Category);
                Assert.AreEqual("Input Validation and Representation", result.Kingdom);
                Assert.IsNull(result.Abstract);
                Assert.IsNull(result.AbstractCustom);
                Assert.IsNull(result.Priority);
                Assert.AreEqual(225, result.PrimaryOrSink.LineStart);
                Assert.IsNull(result.Source);
                result.CweIds.Should().BeEmpty();
            }
        }

        [TestMethod]
        public void FortifyIssue_Parse_IgnoresNonCweTypeExternalCategories()
        {
            XElement xml = XElement.Parse(s_fullIssueXml);
            xml.Element("ExternalCategory").Attribute("type").Value = "a_differnt_type";
            FortifyIssue result = Parse(xml);
            result.CweIds.Should().BeEmpty();
        }

        [TestMethod]
        [ExpectedException(typeof(XmlException))]
        public void FortifyIssue_Parse_RequiresElementsInOrder()
        {
            XElement xml = XElement.Parse(s_fullIssueXml);
            // Move the primary node to the end
            XElement primary = xml.Element("Primary");
            primary.Remove();
            xml.Add(primary);
            Parse(xml);
        }

        [TestMethod]
        [ExpectedException(typeof(XmlException))]
        public void FortifyIssue_Parse_RequiresCategory()
        {
            XElement xml = XElement.Parse(s_minimalIssueXml);
            xml.Element("Category").Remove();
            Parse(xml);
        }

        [TestMethod]
        [ExpectedException(typeof(XmlException))]
        public void FortifyIssue_Parse_RequiresFolder()
        {
            XElement xml = XElement.Parse(s_minimalIssueXml);
            xml.Element("Folder").Remove();
            Parse(xml);
        }

        [TestMethod]
        [ExpectedException(typeof(XmlException))]
        public void FortifyIssue_Parse_RequiresKingdom()
        {
            XElement xml = XElement.Parse(s_minimalIssueXml);
            xml.Element("Kingdom").Remove();
            Parse(xml);
        }

        [TestMethod]
        [ExpectedException(typeof(XmlException))]
        public void FortifyIssue_Parse_RequiresPrimary()
        {
            XElement xml = XElement.Parse(s_minimalIssueXml);
            xml.Element("Primary").Remove();
            Parse(xml);
        }

        private static FortifyIssue Parse(XmlReader reader)
        {
            return FortifyIssue.Parse(reader, new FortifyStrings(reader.NameTable));
        }

        private static FortifyIssue Parse(XElement element)
        {
            using (XmlReader reader = Utilities.CreateXmlReaderFromString(element.ToString(SaveOptions.DisableFormatting)))
            {
                Assert.IsTrue(reader.Read());
                return Parse(reader);
            }
        }
    }
}
