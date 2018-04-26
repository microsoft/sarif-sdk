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

            SarifLogVersionOne v1Log = GetSarifLogVersionOne(v1LogText);
            var transformer = new SarifVersionOneToCurrentVisitor();
            transformer.VisitSarifLogVersionOne(v1Log);

            SarifLog v2Log = transformer.SarifLog;

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

            SarifLogVersionOne v1Log = GetSarifLogVersionOne(v1LogText);
            var transformer = new SarifVersionOneToCurrentVisitor();
            transformer.VisitSarifLogVersionOne(v1Log);

            SarifLog v2Log = transformer.SarifLog;

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

            SarifLogVersionOne v1Log = GetSarifLogVersionOne(v1LogText);
            var transformer = new SarifVersionOneToCurrentVisitor();
            transformer.VisitSarifLogVersionOne(v1Log);

            SarifLog v2Log = transformer.SarifLog;

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
    }
}
