// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Xml;
using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    [TestClass]
    public class CppCheckErrorTests
    {
        private readonly ImmutableArray<CppCheckLocation> _dummyLocations = ImmutableArray.Create(new CppCheckLocation("file.cpp", 42));

        [TestMethod]
        public void CppCheckError_PassesThroughConstructorParameters()
        {
            var uut = new CppCheckError("id", "message", "verbose", "style", _dummyLocations);
            AssertOuterPropertiesAreExampleError(uut);
            Assert.AreEqual(_dummyLocations, uut.Locations);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CppCheckError_RequiresNonEmptyLocations()
        {
            new CppCheckError("id", "message", "verbose", "style", ImmutableArray<CppCheckLocation>.Empty);
        }

        [TestMethod]
        public void CppCheckError_MinimalErrorCanBeConvertedToSarifIssue()
        {
            Result result = new CppCheckError("id", "message", "verbose", "style", _dummyLocations)
                .ToSarifIssue();
            Assert.AreEqual("id", result.RuleId);
            Assert.AreEqual("verbose", result.Message);
            result.Properties.Should().Equal(new Dictionary<string, string> { { "Severity", "style" } });
            Assert.AreEqual("file.cpp", result.Locations.First().ResultFile.Uri.ToString());
        }

        [TestMethod]
        public void CppCheckError_ErrorWithSingleLocationIsConvertedToSarifIssue()
        {
            Result result = new CppCheckError("id", "message", "verbose", "my fancy severity", ImmutableArray.Create(
                new CppCheckLocation("foo.cpp", 1234)
                )).ToSarifIssue();
            Assert.AreEqual("id", result.RuleId);
            Assert.AreEqual("verbose", result.Message);
            result.Properties.Should().Equal(new Dictionary<string, string> { { "Severity", "my fancy severity" } });
            result.Locations.SequenceEqual(new[] { new Location {
                    ResultFile = new PhysicalLocation
                    {
                        Uri = new Uri("foo.cpp", UriKind.RelativeOrAbsolute),
                        Region = new Region { StartLine = 1234 }
                    }
                }
            }, Location.ValueComparer).Should().BeTrue();
            Assert.IsNull(result.CodeFlows);
        }

        [TestMethod]
        public void CppCheckError_ErrorWithMultipleLocationsFillsOutCodeFlow()
        {
            Result result = new CppCheckError("id", "message", "verbose", "my fancy severity", ImmutableArray.Create(
                new CppCheckLocation("foo.cpp", 1234),
                new CppCheckLocation("bar.cpp", 5678)
                )).ToSarifIssue();

            result.Locations.SequenceEqual(new[] { new Location {
                        ResultFile = new PhysicalLocation
                        {
                            Uri = new Uri("bar.cpp", UriKind.RelativeOrAbsolute),
                            Region = new Region { StartLine = 5678 }
                        }
                    }
                }, Location.ValueComparer).Should().BeTrue();

            Assert.AreEqual(1, result.CodeFlows.Count);
            result.CodeFlows.First().Locations.SequenceEqual(new[]
                {
                    new AnnotatedCodeLocation {
                        PhysicalLocation = new PhysicalLocation {
                            Uri = new Uri("foo.cpp", UriKind.RelativeOrAbsolute),
                            Region = new Region { StartLine = 1234 }
                        }
                    },
                    new AnnotatedCodeLocation {
                        PhysicalLocation = new PhysicalLocation {
                            Uri = new Uri("bar.cpp", UriKind.RelativeOrAbsolute),
                            Region = new Region { StartLine = 5678 }
                        }
                    }
                }, AnnotatedCodeLocation.ValueComparer).Should().BeTrue();
        }

        [TestMethod]
        public void CppCheckError_DoesNotEmitShortMessageWhenVerboseMessageIsTheSame()
        {
            Result result = new CppCheckError("id", "message", "message", "style", _dummyLocations)
                .ToSarifIssue();
            Assert.AreEqual("message", result.Message);
        }

        [TestMethod]
        [ExpectedException(typeof(XmlException))]
        public void CppCheckError_RejectsSelfClosingError()
        {
            using (XmlReader xml = Utilities.CreateXmlReaderFromString(exampleErrorXmlSelfClosed))
            {
                var uut = Parse(xml);
                AssertOuterPropertiesAreExampleError(uut);
                uut.Locations.Should().BeEmpty();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(XmlException))]
        public void CppCheckError_RejectsErrorWithNoLocations()
        {
            using (XmlReader xml = Utilities.CreateXmlReaderFromString(exampleErrorXmlOpen + exampleErrorClose))
            {
                var uut = Parse(xml);
                AssertOuterPropertiesAreExampleError(uut);
                uut.Locations.Should().BeEmpty();
            }
        }

        [TestMethod]
        public void CppCheckError_CanParseErrorWithSingleLocation()
        {
            string errorXml = exampleErrorXmlOpen + " <location file=\"foo.cpp\" line=\"42\" /> " + exampleErrorClose;
            using (XmlReader xml = Utilities.CreateXmlReaderFromString(errorXml))
            {
                var uut = Parse(xml);
                AssertOuterPropertiesAreExampleError(uut);
                uut.Locations.Should().Equal(new[] { new CppCheckLocation("foo.cpp", 42) });
            }
        }

        [TestMethod]
        public void CppCheckError_CanParseErrorWithMultipleLocations()
        {
            string errorXml = exampleErrorXmlOpen + " <location file=\"foo.cpp\" line=\"42\" />  <location file=\"bar.cpp\" line=\"1729\" /> " + exampleErrorClose;
            using (XmlReader xml = Utilities.CreateXmlReaderFromString(errorXml))
            {
                var uut = Parse(xml);
                AssertOuterPropertiesAreExampleError(uut);
                uut.Locations.Should().Equal(new[] {
                    new CppCheckLocation("foo.cpp", 42),
                    new CppCheckLocation("bar.cpp", 1729)
                });
            }
        }

        [TestMethod]
        [ExpectedException(typeof(XmlException))]
        public void CppCheckError_InvalidParse_BadRootNodeDetected()
        {
            using (XmlReader xml = Utilities.CreateXmlReaderFromString("<foobar />"))
            {
                Parse(xml);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(XmlException))]
        public void CppCheckError_InvalidParse_BadChildrenNodeDetected()
        {
            using (XmlReader xml = Utilities.CreateXmlReaderFromString(exampleErrorXmlOpen + "<foobar />" + exampleErrorClose))
            {
                Parse(xml);
            }
        }

        private const string exampleErrorXmlBase = "<error id=\"id\" msg=\"message\" verbose=\"verbose\" severity=\"style\"";
        private const string exampleErrorXmlOpen = exampleErrorXmlBase + ">";
        private const string exampleErrorClose = "</error>";
        private const string exampleErrorXmlSelfClosed = exampleErrorXmlBase + " />";
        private static void AssertOuterPropertiesAreExampleError(CppCheckError uut)
        {
            Assert.AreEqual("id", uut.Id);
            Assert.AreEqual("message", uut.Message);
            Assert.AreEqual("verbose", uut.VerboseMessage);
            Assert.AreEqual("style", uut.Severity);
        }

        private static CppCheckError Parse(XmlReader xml)
        {
            return CppCheckError.Parse(xml, new CppCheckStrings(xml.NameTable));
        }
    }
}
