// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        
// license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Microsoft.CodeAnalysis.Sarif.VersionOne;
using Microsoft.CodeAnalysis.Sarif.Visitors;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class SarifTransformerTests
    {
        private static SarifLogVersionOne GetSarifLogVersionOne(string logText)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                ContractResolver = SarifContractResolverVersionOne.Instance,
            };

            return JsonConvert.DeserializeObject<SarifLogVersionOne>(logText, settings);
        }

        private static SarifLog TransformVersionOneToCurrent(string v1LogText)
        {
            SarifLogVersionOne v1Log = GetSarifLogVersionOne(v1LogText);
            var transformer = new SarifVersionOneToCurrentVisitor();
            transformer.VisitSarifLogVersionOne(v1Log);

            return transformer.SarifLog;
        }

        [Fact]
        public void SarifTransformerTests_ToCurrent_Minimum()
        {
            string v1LogText =
              @"{
                  ""version"": ""1.0.0"",
                  ""runs"": [
                    {
                      ""tool"": {
                        ""name"": ""CodeScanner"",
                        ""semanticVersion"": ""2.1.0""
                      },
                      ""results"": [
                      ]
                    }
                  ]
                }";
            
            SarifLog v2Log = TransformVersionOneToCurrent(v1LogText);

            v2Log.Runs.Should().NotBeNull();
            v2Log.Runs.Count.Should().Be(1);

            v2Log.Runs[0].Results.Should().NotBeNull();
            v2Log.Runs[0].Results.Count.Should().Be(0);

            v2Log.Runs[0].Tool.Should().NotBeNull();
            v2Log.Runs[0].Tool.Name.Should().Be("CodeScanner");
            v2Log.Runs[0].Tool.SemanticVersion.Should().Be("2.1.0");
        }

        [Fact]
        public void SarifTransformerTests_ToCurrent_MinimumWithTwoRuns()
        {
            string v1LogText =
              @"{
                  ""version"": ""1.0.0"",
                  ""runs"": [
                    {
                      ""tool"": {
                        ""name"": ""CodeScanner"",
                        ""semanticVersion"": ""2.1.0""
                      },
                      ""results"": [
                      ]
                    },
                    {
                      ""tool"": {
                        ""name"": ""AssetScanner"",
                        ""semanticVersion"": ""1.7.2""
                      },
                      ""results"": [
                      ]
                    }
                  ]
                }";

            SarifLog v2Log = TransformVersionOneToCurrent(v1LogText);

            v2Log.Runs.Should().NotBeNull();
            v2Log.Runs.Count.Should().Be(2);

            v2Log.Runs[0].Results.Should().NotBeNull();
            v2Log.Runs[0].Results.Count.Should().Be(0);

            v2Log.Runs[0].Tool.Should().NotBeNull();
            v2Log.Runs[0].Tool.Name.Should().Be("CodeScanner");
            v2Log.Runs[0].Tool.SemanticVersion.Should().Be("2.1.0");

            v2Log.Runs[1].Results.Should().NotBeNull();
            v2Log.Runs[1].Results.Count.Should().Be(0);

            v2Log.Runs[1].Tool.Should().NotBeNull();
            v2Log.Runs[1].Tool.Name.Should().Be("AssetScanner");
            v2Log.Runs[1].Tool.SemanticVersion.Should().Be("1.7.2");
        }

        [Fact]
        public void SarifTransformerTests_ToCurrent_OneRunWithLogicalLocations()
        {
            string v1LogText =
              @"{
                  ""version"": ""1.0.0"",
                  ""runs"": [
                    {
                      ""logicalLocations"": {
                        ""collections::list::add"": {
                          ""name"": ""add"",
                          ""kind"": ""function"",
                          ""parentKey"": ""collections::list""
                        },
                        ""collections::list"": {
                          ""name"": ""list"",
                          ""kind"": ""type"",
                          ""parentKey"": ""collections""
                        },
                        ""collections"": {
                          ""name"": ""collections"",
                          ""kind"": ""namespace""
                        }
                      },
                      ""tool"": {
                        ""name"": ""CodeScanner"",
                        ""semanticVersion"": ""2.1.0""
                      },
                      ""results"": [
                      ]
                    }
                  ]
                }";

            SarifLog v2Log = TransformVersionOneToCurrent(v1LogText);

            v2Log.Runs.Should().NotBeNull();
            v2Log.Runs.Count.Should().Be(1);

            IDictionary<string, LogicalLocation> logicalLocations = v2Log.Runs[0].LogicalLocations;

            logicalLocations.Should().NotBeNull();
            logicalLocations.Count.Should().Be(3);

            logicalLocations["collections::list::add"].Name.Should().Be("add");
            logicalLocations["collections::list::add"].Kind.Should().Be("function");
            logicalLocations["collections::list::add"].ParentKey.Should().Be("collections::list");

            logicalLocations["collections::list"].Name.Should().Be("list");
            logicalLocations["collections::list"].Kind.Should().Be("type");
            logicalLocations["collections::list"].ParentKey.Should().Be("collections");

            logicalLocations["collections"].Name.Should().Be("collections");
            logicalLocations["collections"].Kind.Should().Be("namespace");
            logicalLocations["collections"].ParentKey.Should().BeNull();
        }

        [Fact]
        public void SarifTransformerTests_ToCurrent_OneRunWithFiles()
        {
            string v1LogText =
              @"{
                  ""version"": ""1.0.0"",
                  ""runs"": [
                    {
                      ""files"": {
                        ""file:///home/list.txt"": {
                          ""mimeType"": ""text/plain"",
                          ""length"": 43,
                          ""contents"": ""VGhlIHF1aWNrIGJyb3duIGZveCBqdW1wcyBvdmVyIHRoZSBsYXp5IGRvZw=="",
                          ""hashes"": [
                            {
                              ""algorithm"": ""sha256"",
                              ""value"": ""d7a8fbb307d7809469ca9abcb0082e4f8d5651e46d3cdb762d02d0bf37c9e592""
                            }
                          ]
                        },
                        ""file:///home/buildAgent/bin/app.zip"": {
                          ""mimeType"": ""application/zip"",
                          ""properties"": {
                            ""my_key"": ""some value""
                          }
                        },
                        ""file:///home/buildAgent/bin/app.zip#/docs/intro.docx"": {
                          ""uri"": ""file:///docs/intro.docx"",
                          ""mimeType"": ""application/vnd.openxmlformats-officedocument.wordprocessingml.document"",
                          ""parentKey"": ""file:///home/buildAgent/bin/app.zip"",
                          ""offset"": 17522,
                          ""length"": 4050
                        }
                      },
                      ""tool"": {
                        ""name"": ""CodeScanner"",
                        ""semanticVersion"": ""2.1.0""
                      },
                      ""results"": [
                      ]
                    }
                  ]
                }";

            SarifLog v2Log = TransformVersionOneToCurrent(v1LogText);

            v2Log.Runs.Should().NotBeNull();
            v2Log.Runs.Count.Should().Be(1);

            IDictionary<string, FileData> files = v2Log.Runs[0].Files;

            files.Should().NotBeNull();
            files.Count.Should().Be(3);

            FileData fileData = null;

            fileData = files["file:///home/list.txt"];
            fileData.MimeType.Should().Be("text/plain");
            fileData.Length.Should().Be(43);
            fileData.FileLocation.Should().BeNull();
            fileData.Contents.Should().NotBeNull();
            fileData.Contents.Binary.Should().Be("VGhlIHF1aWNrIGJyb3duIGZveCBqdW1wcyBvdmVyIHRoZSBsYXp5IGRvZw==");
            fileData.Hashes.Should().NotBeNull();
            fileData.Hashes[0].Algorithm.Should().Be("sha256");
            fileData.Hashes[0].Value.Should().Be("d7a8fbb307d7809469ca9abcb0082e4f8d5651e46d3cdb762d02d0bf37c9e592");

            fileData = files["file:///home/buildAgent/bin/app.zip"];
            fileData.MimeType.Should().Be("application/zip");
            fileData.Length.Should().Be(0);
            fileData.FileLocation.Should().BeNull();
            fileData.Contents.Should().BeNull();
            fileData.Hashes.Should().BeNull();
            fileData.Properties.Should().NotBeNull();
            fileData.Properties["my_key"].SerializedValue.Should().Be("\"some value\"");

            fileData = files["file:///home/buildAgent/bin/app.zip#/docs/intro.docx"];
            fileData.MimeType.Should().Be("application/vnd.openxmlformats-officedocument.wordprocessingml.document");
            fileData.ParentKey.Should().Be("file:///home/buildAgent/bin/app.zip");
            fileData.Offset.Should().Be(17522);
            fileData.Length.Should().Be(4050);
            fileData.FileLocation.Should().NotBeNull();
            fileData.FileLocation.Uri.Should().NotBeNull();
            fileData.FileLocation.Uri.OriginalString.Should().Be("file:///docs/intro.docx");
            fileData.Contents.Should().BeNull();
        }
    }
}
