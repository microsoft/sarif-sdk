// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        
// license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Microsoft.CodeAnalysis.Sarif.VersionOne;
using Microsoft.CodeAnalysis.Sarif.Visitors;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class SarifCurrentToVersionOneTests
    {
        private static readonly JsonSerializerSettings s_v1JsonSettings = new JsonSerializerSettings
        {
            ContractResolver = SarifContractResolverVersionOne.Instance,
            Formatting = Formatting.Indented
        };

        private static readonly JsonSerializerSettings s_v2JsonSettings = new JsonSerializerSettings
        {
            ContractResolver = SarifContractResolver.Instance,
            Formatting = Formatting.Indented
        };

        private static SarifLog GetSarifLog(string logText)
        {
            return JsonConvert.DeserializeObject<SarifLog>(logText, s_v2JsonSettings);
        }

        private static SarifLogVersionOne TransformCurrentToVersion1(string v2LogText)
        {
            SarifLog v2Log = GetSarifLog(v2LogText);
            var transformer = new SarifCurrentToVersionOneVisitor();
            transformer.VisitSarifLog(v2Log);

            return SarifCurrentToVersionOneVisitor.SarifLogVersionOne;
        }

        [Fact]
        public void SarifTransformerTests_ToVersionOne_Minimum()
        {
            string v2LogText =
              @"{
                  ""$schema"": ""http://json.schemastore.org/sarif-2.0.0"",
                  ""version"": ""2.0.0"",
                  ""runs"": [
                    {
                      ""tool"": {
                        ""name"": ""CodeScanner"",
                        ""semanticVersion"": ""2.1.0""
                      },
                      ""results"": []
                    }
                  ]
                }";

            SarifLogVersionOne v1Log = TransformCurrentToVersion1(v2LogText);

            v1Log.Runs.Should().NotBeNull();
            v1Log.Runs.Count.Should().Be(1);

            RunVersionOne run = v1Log.Runs[0];

            run.Results.Should().NotBeNull();
            run.Results.Count.Should().Be(0);

            run.Tool.Should().NotBeNull();
            run.Tool.Name.Should().Be("CodeScanner");
            run.Tool.SemanticVersion.Should().Be("2.1.0");
        }

        [Fact]
        public void SarifTransformerTests_ToVersionOne_MinimumWithTwoRuns()
        {
            string v2LogText =
              @"{
                  ""$schema"": ""http://json.schemastore.org/sarif-2.0.0"",
                  ""version"": ""2.0.0"",
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
                      ""results"": []
                    }
                  ]
                }";

            SarifLogVersionOne v1Log = TransformCurrentToVersion1(v2LogText);

            v1Log.Runs.Should().NotBeNull();
            v1Log.Runs.Count.Should().Be(2);

            RunVersionOne run = v1Log.Runs[0];

            run.Results.Should().NotBeNull();
            run.Results.Count.Should().Be(0);

            run.Tool.Should().NotBeNull();
            run.Tool.Name.Should().Be("CodeScanner");
            run.Tool.SemanticVersion.Should().Be("2.1.0");

            run = v1Log.Runs[1];

            run.Results.Should().NotBeNull();
            run.Results.Count.Should().Be(0);

            run.Tool.Should().NotBeNull();
            run.Tool.SemanticVersion.Should().Be("1.7.2");
            run.Tool.Name.Should().Be("AssetScanner");
        }

        [Fact]
        public void SarifTransformerTests_ToVersionOne_OneRunWithLogicalLocations()
        {
            string v1LogText =
              @"{
                  ""version"": ""2.0.0"",
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

            SarifLogVersionOne v1Log = TransformCurrentToVersion1(v1LogText);

            v1Log.Runs.Should().NotBeNull();
            v1Log.Runs.Count.Should().Be(1);

            IDictionary<string, LogicalLocationVersionOne> logicalLocations = v1Log.Runs[0].LogicalLocations;

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
    }
}
