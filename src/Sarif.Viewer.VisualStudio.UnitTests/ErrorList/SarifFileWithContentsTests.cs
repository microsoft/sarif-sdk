// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif;
using Xunit;

namespace Microsoft.Sarif.Viewer.VisualStudio.UnitTests
{
    // Added tests to Collection because otherwise the other tests
    // will load in parallel, which causes issues with static collections.
    // Production code will only load one SARIF file at a time.
    [Collection("SarifObjectTests")]
    public class SarifFileWithContentsTests
    {
        private const string key1 = "/item.cpp#fragment";
        private const string key2 = "/binary.cpp";
        private const string key3 = "/text.cpp";
        private const string key4 = "/both.cpp";
        private const string key5 = "/emptybinary.cpp";
        private const string key6 = "/emptytext.cpp";
        private const string key7 = "/existinghash.cpp";
        private const string expectedContents1 = "This is a test file.";
        private const string expectedContents2 = "The quick brown fox jumps over the lazy dog.";

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
                                "file:///item.cpp#fragment",
                                new FileData
                                {
                                    MimeType = "text/x-c",
                                    Contents = new FileContent()
                                    {
                                        Binary = "VGhpcyBpcyBhIHRlc3QgZmlsZS4="
                                    },
                                    Hashes = new List<Hash>
                                    {
                                        new Hash
                                        {
                                            Algorithm = AlgorithmKind.Sha256,
                                            Value = "HashValue"
                                        }
                                    }
                                }
                            },
                            {
                                "file:///binary.cpp",
                                new FileData
                                {
                                    MimeType = "text/x-c",
                                    Contents = new FileContent()
                                    {
                                        Binary = "VGhlIHF1aWNrIGJyb3duIGZveCBqdW1wcyBvdmVyIHRoZSBsYXp5IGRvZy4="
                                    },
                                    Hashes = new List<Hash>
                                    {
                                        new Hash
                                        {
                                            Algorithm = AlgorithmKind.Sha256,
                                            Value = "ef537f25c895bfa782526529a9b63d97aa631564d5d789c2b765448c8635fb6c"
                                        }
                                    }
                                }
                            },
                            {
                                "file:///text.cpp",
                                new FileData
                                {
                                    MimeType = "text/x-c",
                                    Contents = new FileContent()
                                    {
                                        Text = expectedContents1
                                    }
                                }
                            },
                            {
                                "file:///both.cpp",
                                new FileData
                                {
                                    MimeType = "text/x-c",
                                    Contents = new FileContent()
                                    {
                                        Binary = "VGhlIHF1aWNrIGJyb3duIGZveCBqdW1wcyBvdmVyIHRoZSBsYXp5IGRvZy4=",
                                        Text = expectedContents2
                                    },
                                    Hashes = new List<Hash>
                                    {
                                        new Hash
                                        {
                                            Algorithm = AlgorithmKind.Sha256,
                                            Value = "ef537f25c895bfa782526529a9b63d97aa631564d5d789c2b765448c8635fb6c"
                                        }
                                    }
                                }
                            },
                            {
                                "file:///emptybinary.cpp",
                                new FileData
                                {
                                    MimeType = "text/x-c",
                                    Contents = new FileContent()
                                    {
                                        Binary = ""
                                    }
                                }
                            },
                            {
                                "file:///emptytext.cpp",
                                new FileData
                                {
                                    MimeType = "text/x-c",
                                    Contents = new FileContent()
                                    {
                                        Text = ""
                                    }
                                }
                            },
                            {
                                "file:///existinghash.cpp",
                                new FileData
                                {
                                    MimeType = "text/x-c",
                                    Contents = new FileContent()
                                    {
                                        Text = expectedContents2
                                    },
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
                                AnalysisTarget = new
                                FileLocation
                                {
                                    Uri = new Uri(@"file:///item.cpp")
                                },
                                RuleId = "C0001",
                                Message = new Message { Text = "Error 1" },
                                Locations = new List<Location>
                                {
                                    new Location() { }
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

            fileDetails.Should().ContainKey(key1);
        }

        [Fact]
        public void SarifFileWithContents_DecodesBinaryContents()
        {
            var fileDetail = CodeAnalysisResultManager.Instance.FileDetails[key2];
            var contents = fileDetail.GetContents();

            contents.Should().Be(expectedContents2);
        }

        [Fact]
        public void SarifFileWithContents_OpensEmbeddedBinaryFile()
        {
            var rebaselinedFile = CodeAnalysisResultManager.Instance.CreateFileFromContents(key2);
            var fileText = File.ReadAllText(rebaselinedFile);

            fileText.Should().Be(expectedContents2);
        }

        [Fact]
        public void SarifFileWithContents_DecodesTextContents()
        {
            var fileDetail = CodeAnalysisResultManager.Instance.FileDetails[key3];
            var contents = fileDetail.GetContents();

            contents.Should().Be(expectedContents1);
        }

        [Fact]
        public void SarifFileWithContents_DecodesBinaryContentsWithText()
        {
            var fileDetail = CodeAnalysisResultManager.Instance.FileDetails[key4];
            var contents = fileDetail.GetContents();

            contents.Should().Be(expectedContents2);
        }

        [Fact]
        public void SarifFileWithContents_HandlesEmptyBinaryContents()
        {
            var fileDetail = CodeAnalysisResultManager.Instance.FileDetails[key5];
            var contents = fileDetail.GetContents();

            contents.Should().Be(string.Empty);
        }

        [Fact]
        public void SarifFileWithContents_HandlesEmptyTextContents()
        {
            var fileDetail = CodeAnalysisResultManager.Instance.FileDetails[key6];
            var contents = fileDetail.GetContents();

            contents.Should().Be(String.Empty);
        }

        [Fact]
        public void SarifFileWithContents_HandlesExistingHash()
        {
            var fileDetail = CodeAnalysisResultManager.Instance.FileDetails[key7];
            var contents = fileDetail.GetContents();

            contents.Should().Be(expectedContents2);
        }

        [Fact]
        public void SarifFileWithContents_GeneratesHash()
        {
            var fileDetail = CodeAnalysisResultManager.Instance.FileDetails[key1];
            var contents = fileDetail.GetContents();

            contents.Should().Be(expectedContents1);
        }
    }
}