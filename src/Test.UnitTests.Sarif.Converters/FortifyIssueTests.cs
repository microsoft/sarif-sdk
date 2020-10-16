// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Immutable;
using System.Xml;
using System.Xml.Linq;
using FluentAssertions;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    public class FortifyIssueTests
    {
        private static readonly FortifyPathElement s_pathElementA =
            new FortifyPathElement("cute_fluffy_kittens.cpp", 42, "void Kitten::meow(int) const");
        private static readonly FortifyPathElement s_pathElementB =
            new FortifyPathElement("momma_cats.cpp", 1729, "void MommaCat::purr(int) const");
        private static readonly string s_fullIssueXml =
@"<Issue ruleID=""7DDEC64A-9142-4943-BB5C-57D6F09C94DC"" iid=""BDC8FC3C5AAE67B07F46EC48B928AA6E"">
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
</Issue>";
        private static readonly string s_minimalIssueXml =
@"<Issue>
  <Category>Format String</Category>
  <Folder>High</Folder>
  <Kingdom>Input Validation and Representation</Kingdom>
  <Primary>
    <FileName>bannedAPIs.m</FileName>
    <FilePath>bannedAPIs.m</FilePath>
    <LineStart>225</LineStart>
    <Snippet> Does Not Matter</Snippet>
  </Primary>
</Issue>";


        [Fact]
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

            Assert.Equal("ruleId", uut.RuleId);
            Assert.Equal("iid", uut.InstanceId);
            Assert.Equal("category", uut.Category);
            Assert.Equal("kingdom", uut.Kingdom);
            Assert.Equal("abstract", uut.Abstract);
            Assert.Equal("abstractCustom", uut.AbstractCustom);
            Assert.Equal("priority", uut.Priority);
            Assert.Same(s_pathElementA, uut.PrimaryOrSink);
            Assert.Same(s_pathElementB, uut.Source);
            uut.CweIds.Should().Equal(new[] { 1, 2, 3 });
        }

        [Fact]
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

            Assert.Null(uut.RuleId);
            Assert.Null(uut.InstanceId);
            Assert.Equal("category", uut.Category);
            Assert.Equal("kingdom", uut.Kingdom);
            Assert.Null(uut.Abstract);
            Assert.Null(uut.AbstractCustom);
            Assert.Null(uut.Priority);
            Assert.Same(s_pathElementA, uut.PrimaryOrSink);
            Assert.Null(uut.Source);
            uut.CweIds.Should().BeEmpty();
        }

        [Fact]
        public void FortifyIssue_RequiresCategory()
        {
            Assert.Throws<ArgumentNullException>(() => new FortifyIssue(
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
                ));
        }

        [Fact]
        public void FortifyIssue_RequiresKingdom()
        {
            Assert.Throws<ArgumentNullException>(() => new FortifyIssue(
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
                ));
        }

        [Fact]
        public void FortifyIssue_RequiresPrimary()
        {
            Assert.Throws<ArgumentNullException>(() => new FortifyIssue(
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
                ));
        }

        [Fact]
        public void FortifyIssue_ParseCweIds_NoIds()
        {
            FortifyIssue.ParseCweIds("1234").Should().BeEmpty();
        }

        [Fact]
        public void FortifyIssue_ParseCweIds_SingleCweId()
        {
            FortifyIssue.ParseCweIds("CWE ID 476").Should().Equal(new[] { 476 });
        }

        [Fact]
        public void FortifyIssue_ParseCweIds_MultipleCweIds()
        {
            FortifyIssue.ParseCweIds("CWE ID 134, CWE ID 787").Should().Equal(new[] { 134, 787 });
        }

        [Fact]
        public void FortifyIssue_ParseCweIds_SortsCweIds()
        {
            FortifyIssue.ParseCweIds("CWE ID 787, CWE ID 134").Should().Equal(new[] { 134, 787 });
        }

        [Fact]
        public void FortifyIssue_ParseCweIds_UniquesCweIds()
        {
            FortifyIssue.ParseCweIds("CWE ID 134, CWE ID 134").Should().Equal(new[] { 134 });
        }

        [Fact]
        public void FortifyIssue_Parse_CanParseFullIssue()
        {
            string xml = "<xml>" + s_fullIssueXml + "<following /></xml>";
            using (XmlReader reader = Utilities.CreateWhitespaceSkippingXmlReaderFromString(xml))
            {
                reader.Read(); //<xml>
                reader.Read(); //<Issue>
                FortifyIssue result = Parse(reader);
                Assert.Equal("following", reader.LocalName);
                Assert.Equal("7DDEC64A-9142-4943-BB5C-57D6F09C94DC", result.RuleId);
                Assert.Equal("BDC8FC3C5AAE67B07F46EC48B928AA6E", result.InstanceId);
                Assert.Equal("Format String", result.Category);
                Assert.Equal("Input Validation and Representation", result.Kingdom);
                Assert.Equal("An attacker can control the format string argument to vfwprintf() at bannedAPIs.m line 225, allowing an attack much like a buffer overflow.", result.Abstract);
                Assert.Equal("A message Bill added for testing purposes.", result.AbstractCustom);
                Assert.Equal("High", result.Priority);
                Assert.Equal(225, result.PrimaryOrSink.LineStart);
                Assert.Equal(238, result.Source.LineStart);
                result.CweIds.Should().Equal(new[] { 134 });
            }
        }

        [Fact]
        public void FortifyIssue_Parse_CanParseMinimalIssue()
        {
            string xml = "<xml>" + s_minimalIssueXml + "<following /></xml>";
            using (XmlReader reader = Utilities.CreateWhitespaceSkippingXmlReaderFromString(xml))
            {
                reader.Read(); //<xml>
                reader.Read(); //<Issue>
                FortifyIssue result = Parse(reader);
                Assert.Equal("following", reader.LocalName);
                Assert.Null(result.RuleId);
                Assert.Null(result.InstanceId);
                Assert.Equal("Format String", result.Category);
                Assert.Equal("Input Validation and Representation", result.Kingdom);
                Assert.Null(result.Abstract);
                Assert.Null(result.AbstractCustom);
                Assert.Null(result.Priority);
                Assert.Equal(225, result.PrimaryOrSink.LineStart);
                Assert.Null(result.Source);
                result.CweIds.Should().BeEmpty();
            }
        }

        [Fact]
        public void FortifyIssue_Parse_IgnoresNonCweTypeExternalCategories()
        {
            XElement xml = XElement.Parse(s_fullIssueXml);
            xml.Element("ExternalCategory").Attribute("type").Value = "a_differnt_type";
            FortifyIssue result = Parse(xml);
            result.CweIds.Should().BeEmpty();
        }

        [Fact]
        public void FortifyIssue_Parse_RequiresElementsInOrder()
        {
            XElement xml = XElement.Parse(s_fullIssueXml);
            // Move the primary node to the end
            XElement primary = xml.Element("Primary");
            primary.Remove();
            xml.Add(primary);
            Assert.Throws<XmlException>(() => Parse(xml));
        }

        [Fact]
        public void FortifyIssue_Parse_RequiresCategory()
        {
            XElement xml = XElement.Parse(s_minimalIssueXml);
            xml.Element("Category").Remove();
            Assert.Throws<XmlException>(() => Parse(xml));
        }

        [Fact]
        public void FortifyIssue_Parse_RequiresFolder()
        {
            XElement xml = XElement.Parse(s_minimalIssueXml);
            xml.Element("Folder").Remove();
            Assert.Throws<XmlException>(() => Parse(xml));
        }

        [Fact]
        public void FortifyIssue_Parse_RequiresKingdom()
        {
            XElement xml = XElement.Parse(s_minimalIssueXml);
            xml.Element("Kingdom").Remove();
            Assert.Throws<XmlException>(() => Parse(xml));
        }

        [Fact]
        public void FortifyIssue_Parse_RequiresPrimary()
        {
            XElement xml = XElement.Parse(s_minimalIssueXml);
            xml.Element("Primary").Remove();
            Assert.Throws<XmlException>(() => Parse(xml));
        }

        private static FortifyIssue Parse(XmlReader reader)
        {
            return FortifyIssue.Parse(reader, new FortifyStrings(reader.NameTable));
        }

        private static FortifyIssue Parse(XElement element)
        {
            using (XmlReader reader = Utilities.CreateXmlReaderFromString(element.ToString(SaveOptions.DisableFormatting)))
            {
                Assert.True(reader.Read());
                return Parse(reader);
            }
        }
    }
}
