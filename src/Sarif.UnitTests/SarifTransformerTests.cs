// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        
// license. See LICENSE file in the project root for full license information.

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
        [Fact]
        public void SarifTransformerTests_ToCurrent_Minimum()
        {
            string v1LogText = "{\"version\":\"1.0.0\",\"runs\":[{\"tool\":{\"name\":\"CodeScanner\",\"semanticVersion\":\"2.1.0\"},\"results\":[]}]}";

            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                ContractResolver = SarifContractResolverVersionOne.Instance,
            };

            SarifLogVersionOne v1Log = JsonConvert.DeserializeObject<SarifLogVersionOne>(v1LogText, settings);
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
            string v1LogText = "{\"version\":\"1.0.0\",\"runs\":[{\"tool\":{\"name\":\"CodeScanner\",\"semanticVersion\":\"2.1.0\"},\"results\":[]},{\"tool\":{\"name\":\"AssetScanner\",\"semanticVersion\":\"1.7.2\"},\"results\":[]}]}";

            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                ContractResolver = SarifContractResolverVersionOne.Instance,
            };

            SarifLogVersionOne v1Log = JsonConvert.DeserializeObject<SarifLogVersionOne>(v1LogText, settings);
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
    }
}
