using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.Sarif.Viewer.ErrorList;
using Xunit;

namespace Microsoft.Sarif.Viewer.VisualStudio.UnitTests
{
    public class SarifFileWithContentsTests
    {
        private const string Key = "/item.cpp";
        private const string Contents = "This is a test file.";

        public SarifFileWithContentsTests()
        {
            var testLog = new SarifLog
            {
                Runs = new List<Run>
                {
                    new Run
                    {
                        Tool = new Tool
                        {
                            Name = "Test",
                            SemanticVersion = "1.0"
                        },
                        Files = new Dictionary<string, FileData>
                        {
                            {
                                "file:///item.cpp",
                                new FileData
                                {
                                    MimeType = "text/x-c",
                                    Contents = "VGhpcyBpcyBhIHRlc3QgZmlsZS4=",
                                    Hashes = new List<Hash>
                                    {
                                        new Hash
                                        {
                                            Algorithm = AlgorithmKind.Sha256,
                                            Value = "HashValue"
                                        }
                                    }
                                }
                            }
                        },
                        Results = new List<Result>
                        {
                            new Result
                            {
                                RuleId = "C0001",
                                Message = "Error 1",
                                Locations = new List<Location>
                                {
                                    new Location
                                    {
                                        AnalysisTarget = new PhysicalLocation
                                        {
                                            Uri = new Uri("file:///item.cpp")
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            TestUtilities.InitializeTestEnvironment(testLog);
        }

        [Fact]
        public void SarifFileWithContents_SavesContents()
        {
            var fileDetails = CodeAnalysisResultManager.Instance.FileDetails;

            fileDetails.Should().ContainKey(Key);
        }

        [Fact]
        public void SarifFileWithContents_DecodesContents()
        {
            var fileDetail = CodeAnalysisResultManager.Instance.FileDetails[Key];
            var contents = fileDetail.Contents;

            contents.Should().Be(Contents);
        }
    }
}