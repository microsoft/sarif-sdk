// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Linq;
using System.Text.Json;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.Mcp.Server;
using Microsoft.CodeAnalysis.Sarif.Mcp.Server.Tools;
using Microsoft.Extensions.Options;

using Test.UnitTests.Sarif.Mcp.Server.Fixtures;

using Xunit;

namespace Test.UnitTests.Sarif.Mcp.Server
{
    /// <summary>
    /// Pins enrichment-cascade invariants the round-trip test cannot cover:
    /// context regions are proper supersets (SARIF \u00a73.30 / SARIF1008), snippets
    /// extract the right text from the source artifact, and unreadable
    /// source files degrade to a warning instead of crashing.
    /// </summary>
    public sealed class EnrichmentTests : McpScratchTestBase
    {
        [Fact]
        public void ContextRegion_IsProperSupersetOfRegion()
        {
            string outputPath = ScratchPath("ctxregion.sarif");
            McpToolFlow.ProduceRichScenario(this.ScratchDir, outputPath);

            SarifLog log = SarifLog.Load(outputPath);
            PhysicalLocation phys = log.Runs[0].Results[0].Locations[0].PhysicalLocation;

            phys.Region.Should().NotBeNull();
            phys.ContextRegion.Should().NotBeNull("MCP enrichment must populate ContextRegion when the source file is readable");

            // SARIF \u00a73.30 / SARIF1008: contextRegion must be a *proper* superset of region.
            // Either the context starts on an earlier line, ends on a later line, or both \u2014
            // it cannot equal the original region.
            bool startsEarlier = phys.ContextRegion.StartLine < phys.Region.StartLine;
            bool endsLater = phys.ContextRegion.EndLine > phys.Region.EndLine;
            (startsEarlier || endsLater).Should().BeTrue(
                "context region must strictly enclose the target region per SARIF \u00a73.30");
        }

        [Fact]
        public void Snippet_ExtractsExpectedLineFromSourceFile()
        {
            string outputPath = ScratchPath("snippet.sarif");
            McpToolFlow.ProduceRichScenario(this.ScratchDir, outputPath);

            SarifLog log = SarifLog.Load(outputPath);
            ArtifactContent snippet = log.Runs[0].Results[0].Locations[0].PhysicalLocation.Region.Snippet;

            snippet.Should().NotBeNull();
            snippet.Text.Should().Contain("subprocess.run",
                "the result targets line 4 of handler.py which contains the subprocess.run call");
        }

        [Fact]
        public void MissingSourceFile_DegradesToWarning_DoesNotCrash()
        {
            // Drive the flow against a sourceRoot with no actual handler.py present.
            string outputPath = ScratchPath("missing-src.sarif");

            var store = new SarifRunStore();
            var cweResolver = new CweNameResolver();
            IOptions<SarifMcpOptions> options = Options.Create(new SarifMcpOptions());
            var create = new CreateRunTool(store, cweResolver, options);
            var addResult = new AddResultTool(store);
            var finalize = new FinalizeRunTool(store);

            string createJson = create.CreateRun(
                toolName: "Analyzer",
                toolVersion: "1.0.0",
                repoUri: "https://example.com/example/webapp",
                revisionId: "a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2",
                sourceRoot: this.ScratchDir,
                outputPath: outputPath,
                aiOrigin: "generated",
                toolOrganization: null,
                toolInformationUri: null,
                branch: null,
                scenario: null,
                campaignGuid: null);
            using JsonDocument createDoc = JsonDocument.Parse(createJson);
            string runGuid = createDoc.RootElement.GetProperty("run_guid").GetString()!;

            // Reference a file that does not exist on disk under SourceRoot.
            string addJson = addResult.AddResult(
                runGuid: runGuid,
                ruleId: "CWE-78/missing-file",
                messageText: "Result against a missing source file.",
                messageMarkdown: "Result against a missing source file.",
                ruleName: "CommandInjection",
                ruleDescription: null,
                level: "warning",
                rank: null,
                uri: "src/not-on-disk.py",
                startLine: 1,
                startColumn: null,
                endLine: null,
                endColumn: null,
                exploitability: null,
                attackerPosition: null,
                handoff: null);

            using JsonDocument addDoc = JsonDocument.Parse(addJson);
            addDoc.RootElement.GetProperty("status").GetString().Should().Be("added");

            // Warnings array should not be empty \u2014 we should have flagged the unreadable source.
            JsonElement warnings = addDoc.RootElement.GetProperty("warnings");
            warnings.GetArrayLength().Should().BeGreaterThan(0);
            warnings.EnumerateArray()
                .Select(e => e.GetString() ?? string.Empty)
                .Any(s => s.Contains("not-on-disk.py"))
                .Should().BeTrue("the warning should name the unreadable file");

            // Finalize must still write a valid SARIF file with the result.
            _ = finalize.Finalize(runGuid, handoffNotes: null);
            File.Exists(outputPath).Should().BeTrue();
            SarifLog log = SarifLog.Load(outputPath);
            log.Runs[0].Results.Should().HaveCount(1);
        }
    }
}
