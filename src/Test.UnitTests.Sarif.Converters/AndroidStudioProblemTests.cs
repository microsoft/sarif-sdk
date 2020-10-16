// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif.Driver;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    public class AndroidStudioProblemTests
    {
        [Fact]
        public void AndroidStudioProblem_RejectsNullProblemClass()
        {
            AndroidStudioProblem.Builder builder = GetDefaultProblemBuilder();
            builder.ProblemClass = null;
            Assert.Throws<ArgumentException>(() => new AndroidStudioProblem(builder));
        }

        [Fact]
        public void AndroidStudioProblem_RejectsLinePresentButNotFile()
        {
            AndroidStudioProblem.Builder builder = GetDefaultProblemBuilder();
            builder.File = null;
            builder.Line = 42;
            Assert.Throws<ArgumentException>(() => new AndroidStudioProblem(builder));
        }

        [Fact]
        public void AndroidStudioProblem_AcceptsFileAsLocationInformation()
        {
            AndroidStudioProblem.Builder builder = GetDefaultProblemBuilder();
            builder.File = "File";
            builder.Module = null;
            builder.Package = null;
            builder.EntryPointName = null;
            // Assert does not throw:
            new AndroidStudioProblem(builder);
        }

        [Fact]
        public void AndroidStudioProblem_AcceptsModuleAsLocationInformation()
        {
            AndroidStudioProblem.Builder builder = GetDefaultProblemBuilder();
            builder.File = null;
            builder.Module = "Mod";
            builder.Package = null;
            builder.EntryPointName = null;
            // Assert does not throw:
            new AndroidStudioProblem(builder);
        }

        [Fact]
        public void AndroidStudioProblem_AcceptsPackageAsLocationInformation()
        {
            AndroidStudioProblem.Builder builder = GetDefaultProblemBuilder();
            builder.File = null;
            builder.Module = null;
            builder.Package = "Pack";
            builder.EntryPointName = null;
            // Assert does not throw:
            new AndroidStudioProblem(builder);
        }

        [Fact]
        public void AndroidStudioProblem_AcceptsEntryPointAsLocationInformation()
        {
            AndroidStudioProblem.Builder builder = GetDefaultProblemBuilder();
            builder.File = null;
            builder.Module = null;
            builder.Package = null;
            builder.EntryPointName = "Entry";
            builder.EntryPointType = "class";

            // Assert does not throw:
            new AndroidStudioProblem(builder);
        }

        [Fact]
        public void AndroidStudioProblem_RejectsLackOfLocationInformation()
        {
            AndroidStudioProblem.Builder builder = GetDefaultProblemBuilder();
            builder.File = null;
            builder.Module = null;
            builder.Package = null;
            builder.EntryPointName = null;

            // Error: No location info, should throw
            Assert.Throws<ArgumentException>(() => new AndroidStudioProblem(builder));
        }

        [Fact]
        public void AndroidStudioProblem_RequiresEntryPointTypeIfEntryPointNameSet()
        {
            AndroidStudioProblem.Builder builder = GetDefaultProblemBuilder();
            builder.EntryPointType = "Class";

            // Error: No location info, should throw
            Assert.Throws<ArgumentException>(() => new AndroidStudioProblem(builder));
        }

        [Fact]
        public void AndroidStudioProblem_RequiresEntryPointNameIfEntryPointTypeSet()
        {
            AndroidStudioProblem.Builder builder = GetDefaultProblemBuilder();
            builder.EntryPointName = "Frobinate.java";

            // Error: No location info, should throw
            Assert.Throws<ArgumentException>(() => new AndroidStudioProblem(builder));
        }

        [Fact]
        public void AndroidStudioProblem_DefaultConstructedBuilderIsEmpty()
        {
            var uut = new AndroidStudioProblem.Builder();
            Assert.True(uut.IsEmpty);
        }

        [Fact]
        public void AndroidStudioProblem_BuilderWithFileIsNotEmpty()
        {
            var uut = new AndroidStudioProblem.Builder
            {
                File = "file"
            };

            Assert.False(uut.IsEmpty);
        }

        [Fact]
        public void AndroidStudioProblem_BuilderWithLineIsNotEmpty()
        {
            var uut = new AndroidStudioProblem.Builder
            {
                Line = 42
            };

            Assert.False(uut.IsEmpty);
        }

        [Fact]
        public void AndroidStudioProblem_BuilderWithModuleIsNotEmpty()
        {
            var uut = new AndroidStudioProblem.Builder
            {
                Module = "mod"
            };

            Assert.False(uut.IsEmpty);
        }

        [Fact]
        public void AndroidStudioProblem_BuilderWithPackageIsNotEmpty()
        {
            var uut = new AndroidStudioProblem.Builder
            {
                Package = "package"
            };

            Assert.False(uut.IsEmpty);
        }

        [Fact]
        public void AndroidStudioProblem_BuilderWithEntryPointTypeIsNotEmpty()
        {
            var uut = new AndroidStudioProblem.Builder
            {
                EntryPointType = "TYPE"
            };

            Assert.False(uut.IsEmpty);
        }

        [Fact]
        public void AndroidStudioProblem_BuilderWithEntryPointNameIsNotEmpty()
        {
            var uut = new AndroidStudioProblem.Builder
            {
                EntryPointName = "Some name"
            };

            Assert.False(uut.IsEmpty);
        }

        [Fact]
        public void AndroidStudioProblem_BuilderWithSeverityIsNotEmpty()
        {
            var uut = new AndroidStudioProblem.Builder
            {
                Severity = "Sev"
            };

            Assert.False(uut.IsEmpty);
        }

        [Fact]
        public void AndroidStudioProblem_BuilderWithAttributeKeyIsNotEmpty()
        {
            var uut = new AndroidStudioProblem.Builder
            {
                AttributeKey = "Key"
            };
            Assert.False(uut.IsEmpty);
        }

        [Fact]
        public void AndroidStudioProblem_BuilderWithProblemClassIsNotEmpty()
        {
            var uut = new AndroidStudioProblem.Builder
            {
                ProblemClass = "Foo"
            };

            Assert.False(uut.IsEmpty);
        }

        [Fact]
        public void AndroidStudioProblem_BuilderWithHintsIsNotEmpty()
        {
            var uut = new AndroidStudioProblem.Builder
            {
                Hints = ImmutableArray<string>.Empty
            };

            Assert.False(uut.IsEmpty);
            uut.Hints = ImmutableArray.Create("a", "b", "c");
            Assert.False(uut.IsEmpty);
            uut.Hints = default(ImmutableArray<string>);
            Assert.True(uut.IsEmpty);
        }

        [Fact]
        public void AndroidStudioProblem_BuilderWithDescritionIsNotEmpty()
        {
            var uut = new AndroidStudioProblem.Builder
            {
                Description = "You broke things"
            };

            Assert.False(uut.IsEmpty);
        }

        [Fact]
        public void AndroidStudioProblem_ParseSelfClosingNodeConsumesNode()
        {
            string xmlStr = "<root><problem /><following /></root>";
            using (XmlReader xml = Utilities.CreateXmlReaderFromString(xmlStr))
            {
                xml.ReadStartElement("root");
                Assert.Null(Parse(xml));
                Assert.Equal("following", xml.LocalName);
            }
        }

        [Fact]
        public void AndroidStudioProblem_ParseEmptyNodeConsumesNode()
        {
            string xmlStr = "<root><problem></problem><following /></root>";
            using (XmlReader xml = Utilities.CreateXmlReaderFromString(xmlStr))
            {
                xml.ReadStartElement("root");
                Assert.Null(Parse(xml));
                Assert.Equal("following", xml.LocalName);
            }
        }

        [Fact]
        public void AndroidStudioProblem_ParseValidProblem()
        {
            XElement xml = CreateDefaultProblem();
            using (XmlReader xmlReader = xml.CreateNameTableReader())
            {
                AndroidStudioProblem uut = Parse(xmlReader);
                AssertThatProblemHasDefaultProblemProperties(uut);
            }
        }

        [Fact]
        public void AndroidStudioProblem_ParseDoesNotDependOnElementOrder()
        {
            XElement xml = CreateDefaultProblem();
            IList<XElement> elements = xml.Elements().Shuffle();
            xml.Elements().Remove();
            xml.Add(elements);

            using (XmlReader xmlReader = xml.CreateNameTableReader())
            {
                AndroidStudioProblem uut = Parse(xmlReader);
                AssertThatProblemHasDefaultProblemProperties(uut);
            }
        }

        [Fact]
        public void AndroidStudioProblem_ParseDoesNotRequireFile()
        {
            XElement xml = CreateDefaultProblem();
            xml.Elements("file").Remove();
            xml.Elements("line").Remove();
            using (XmlReader xmlReader = xml.CreateNameTableReader())
            {
                AndroidStudioProblem uut = Parse(xmlReader);
                Assert.Null(uut.File);
            }
        }

        [Fact]
        public void AndroidStudioProblem_ParseDoesNotRequireLine()
        {
            XElement xml = CreateDefaultProblem();
            xml.Elements("line").Remove();
            using (XmlReader xmlReader = xml.CreateNameTableReader())
            {
                AndroidStudioProblem uut = Parse(xmlReader);
                Assert.Equal(0, uut.Line);
            }
        }

        [Fact]
        public void AndroidStudioProblem_ParseClampsInvalidLineToOne_Zero()
        {
            XElement xml = CreateDefaultProblem();
            xml.Elements("line").First().Value = XmlConvert.ToString(0);
            using (XmlReader xmlReader = xml.CreateNameTableReader())
            {
                Parse(xmlReader).Line.Should().Be(1);
            }
        }

        [Fact]
        public void AndroidStudioProblem_ParseClampsInvalidLineToOne_Negative()
        {
            XElement xml = CreateDefaultProblem();
            xml.Elements("line").First().Value = XmlConvert.ToString(-1);
            using (XmlReader xmlReader = xml.CreateNameTableReader())
            {
                Parse(xmlReader).Line.Should().Be(1);
            }
        }

        [Fact]
        public void AndroidStudioProblem_ParseDoesNotRequireModule()
        {
            XElement xml = CreateDefaultProblem();
            xml.Elements("module").Remove();
            using (XmlReader xmlReader = xml.CreateNameTableReader())
            {
                AndroidStudioProblem uut = Parse(xmlReader);
                Assert.Null(uut.Module);
            }
        }

        [Fact]
        public void AndroidStudioProblem_ParseDoesNotRequirePackage()
        {
            XElement xml = CreateDefaultProblem();
            xml.Elements("package").Remove();
            using (XmlReader xmlReader = xml.CreateNameTableReader())
            {
                AndroidStudioProblem uut = Parse(xmlReader);
                Assert.Null(uut.Package);
            }
        }

        [Fact]
        public void AndroidStudioProblem_ParseDoesNotRequireEntryPoint()
        {
            XElement xml = CreateDefaultProblem();
            xml.Elements("entry_point").Remove();
            using (XmlReader xmlReader = xml.CreateNameTableReader())
            {
                AndroidStudioProblem uut = Parse(xmlReader);
                Assert.Null(uut.EntryPointType);
                Assert.Null(uut.EntryPointName);
            }
        }

        [Fact]
        public void AndroidStudioProblem_ParseRequiresEntryPoint_Type()
        {
            XElement xml = CreateDefaultProblem();
            xml.Elements("entry_point").Attributes("TYPE").Remove();
            using (XmlReader xmlReader = xml.CreateNameTableReader())
            {
                Assert.Throws<XmlException>(() => Parse(xmlReader));
            }
        }

        [Fact]
        public void AndroidStudioProblem_ParseRequiresEntryPoint_Name()
        {
            XElement xml = CreateDefaultProblem();
            xml.Elements("entry_point").Attributes("FQNAME").Remove();
            using (XmlReader xmlReader = xml.CreateNameTableReader())
            {
                Assert.Throws<XmlException>(() => Parse(xmlReader));
            }
        }

        [Fact]
        public void AndroidStudioProblem_ParseRequiresProblemClass()
        {
            XElement xml = CreateDefaultProblem();
            xml.Elements("problem_class").Remove();
            using (XmlReader xmlReader = xml.CreateNameTableReader())
            {
                Assert.Throws<XmlException>(() => Parse(xmlReader));
            }
        }

        [Fact]
        public void AndroidStudioProblem_ParseDoesNotRequireSeverity()
        {
            XElement xml = CreateDefaultProblem();
            xml.Elements("problem_class").Attributes("severity").Remove();
            using (XmlReader xmlReader = xml.CreateNameTableReader())
            {
                AndroidStudioProblem uut = Parse(xmlReader);
                Assert.Null(uut.Severity);
            }
        }

        [Fact]
        public void AndroidStudioProblem_ParseDoesNotRequireAttributeKey()
        {
            XElement xml = CreateDefaultProblem();
            xml.Elements("problem_class").Attributes("attribute_key").Remove();
            using (XmlReader xmlReader = xml.CreateNameTableReader())
            {
                AndroidStudioProblem uut = Parse(xmlReader);
                Assert.Null(uut.AttributeKey);
            }
        }

        [Fact]
        public void AndroidStudioProblem_ParseDoesNotRequireHints()
        {
            XElement xml = CreateDefaultProblem();
            xml.Elements("hints").Remove();
            using (XmlReader xmlReader = xml.CreateNameTableReader())
            {
                AndroidStudioProblem uut = Parse(xmlReader);
                Assert.False(uut.Hints.IsDefault);
                Assert.True(uut.Hints.IsEmpty);
            }
        }

        [Fact]
        public void AndroidStudioProblem_ParseDoesNotRequireDescription()
        {
            XElement xml = CreateDefaultProblem();
            xml.Elements("description").Remove();
            using (XmlReader xmlReader = xml.CreateNameTableReader())
            {
                AndroidStudioProblem uut = Parse(xmlReader);
                Assert.Null(uut.Description);
            }
        }

        [Fact]
        public void AndroidStudioProblem_ParseIgnoresUnknownData()
        {
            XElement xml = CreateDefaultProblem();
            xml.Add(new XElement("some_unknown_element"));
            xml.Elements().Skip(2).First().AddAfterSelf(new XElement("some_other_unknown_element"));
            xml.Element("file").Add(new XAttribute("some_ignored_attribute", "and data"));
            using (XmlReader xmlReader = xml.CreateNameTableReader())
            {
                AndroidStudioProblem uut = Parse(xmlReader);
                AssertThatProblemHasDefaultProblemProperties(uut);
            }
        }

        [Fact]
        public void AndroidStudioProblem_ParseRejectsNonProblem()
        {
            XElement xml = CreateDefaultProblem();
            xml.Name = "not_a_problem";
            using (XmlReader xmlReader = xml.CreateNameTableReader())
            {
                Assert.Throws<XmlException>(() => Parse(xmlReader));
            }
        }

        [Fact]
        public void AndroidStudioProblem_ReadHintsSelfClosingNodeConsumesNode()
        {
            string xmlStr = "<root><hints /><following /></root>";
            using (XmlReader xml = Utilities.CreateXmlReaderFromString(xmlStr))
            {
                xml.ReadStartElement("root");
                ImmutableArray<string> result = ReadHints(xml);
                Assert.Equal("following", xml.LocalName);
                Assert.True(result.IsEmpty);
                Assert.False(result.IsDefault);
            }
        }

        [Fact]
        public void AndroidStudioProblem_ReadHintsEmptyNodeConsumesNode()
        {
            string xmlStr = "<root><hints></hints><following /></root>";
            using (XmlReader xml = Utilities.CreateXmlReaderFromString(xmlStr))
            {
                xml.ReadStartElement("root");
                ImmutableArray<string> result = ReadHints(xml);
                Assert.Equal("following", xml.LocalName);
                Assert.True(result.IsEmpty);
                Assert.False(result.IsDefault);
            }
        }

        [Fact]
        public void AndroidStudioProblem_ReadHintsRejectsNonHintChildren()
        {
            string xmlStr = "<root><hints><hint value=\"val\" /><not_a_hint /></hints><following /></root>";
            using (XmlReader xml = Utilities.CreateXmlReaderFromString(xmlStr))
            {
                xml.ReadStartElement("root");
                Assert.Throws<XmlException>(() => ReadHints(xml));
            }
        }

        [Fact]
        public void AndroidStudioProblem_ReadHintsRejectsMissingValueHints()
        {
            string xmlStr = "<root><hints><hint /></hints><following /></root>";
            using (XmlReader xml = Utilities.CreateXmlReaderFromString(xmlStr))
            {
                xml.ReadStartElement("root");
                Assert.Throws<XmlException>(() => ReadHints(xml));
            }
        }

        [Fact]
        public void AndroidStudioProblem_ReadHintsAcceptsEmptyStringHints()
        {
            string xmlStr = "<root><hints><hint value=\"\" /></hints><following /></root>";
            using (XmlReader xml = Utilities.CreateXmlReaderFromString(xmlStr))
            {
                xml.ReadStartElement("root");
                ImmutableArray<string> results = ReadHints(xml);
                results.Should().BeEmpty();
            }
        }

        [Fact]
        public void AndroidStudioProblem_ReadHintsAcceptsValidHints()
        {
            string xmlStr = "<root><hints><hint value=\"content_first\" /><hint value=\"content_second\" /></hints><following /></root>";
            using (XmlReader xml = Utilities.CreateXmlReaderFromString(xmlStr))
            {
                xml.ReadStartElement("root");
                ImmutableArray<string> results = ReadHints(xml);
                results.Should().Equal(new[] { "content_first", "content_second" });
            }
        }

        private static XElement CreateDefaultProblem()
        {
            return new XElement("problem",
                new XElement("file", "file://$PROJECT_DIR$/file.java"),
                new XElement("line", 42),
                new XElement("module", "mod"),
                new XElement("package", "pack"),
                new XElement("entry_point", new XAttribute("TYPE", "file"), new XAttribute("FQNAME", "fqname")),
                new XElement("problem_class", new XAttribute("severity", "WARNING"), new XAttribute("attribute_key", "WARNING_ATTRIBUTES"),
                    "Assertions"),
                new XElement("hints",
                    new XElement("hint", new XAttribute("value", "some hint content"))
                    ),
                new XElement("description", "Method is never used.")
                );
        }

        private static void AssertThatProblemHasDefaultProblemProperties(AndroidStudioProblem uut)
        {
            Assert.Equal("file://$PROJECT_DIR$/file.java", uut.File);
            Assert.Equal(42, uut.Line);
            Assert.Equal("mod", uut.Module);
            Assert.Equal("pack", uut.Package);
            Assert.Equal("file", uut.EntryPointType);
            Assert.Equal("fqname", uut.EntryPointName);
            Assert.Equal("WARNING", uut.Severity);
            Assert.Equal("WARNING_ATTRIBUTES", uut.AttributeKey);
            Assert.Equal("Assertions", uut.ProblemClass);
            uut.Hints.Should().Equal(new[] { "some hint content" });
            Assert.Equal("Method is never used.", uut.Description);
        }

        private static AndroidStudioProblem Parse(XmlReader reader)
        {
            return AndroidStudioProblem.Parse(reader, new AndroidStudioStrings(reader.NameTable));
        }

        private static ImmutableArray<string> ReadHints(XmlReader reader)
        {
            return AndroidStudioProblem.ReadHints(reader, new AndroidStudioStrings(reader.NameTable));
        }

        internal static AndroidStudioProblem.Builder GetDefaultProblemBuilder()
        {
            return new AndroidStudioProblem.Builder
            {
                ProblemClass = "A Problematic Problem",
                File = "Unused"
            };
        }

        internal static AndroidStudioProblem GetDefaultProblem()
        {
            return new AndroidStudioProblem(GetDefaultProblemBuilder());
        }
    }
}
