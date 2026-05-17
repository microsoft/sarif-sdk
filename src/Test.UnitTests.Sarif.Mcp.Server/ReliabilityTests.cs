// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
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
    /// Reliability guards on the emit surface:
    /// <list type="bullet">
    ///   <item>create-run fails fast when the target file already exists,
    ///   unless the caller opts in via <c>allowOverwrite</c>;</item>
    ///   <item>finalize writes atomically: readers observe either the prior
    ///   contents or the new contents, never a partial write; temp files
    ///   are not left behind on success.</item>
    /// </list>
    /// </summary>
    public sealed class ReliabilityTests : McpScratchTestBase
    {
        [Fact]
        public void CreateRun_FailsFast_IfOutputExists_AndAllowOverwriteFalse()
        {
            string outputPath = ScratchPath("preexisting.sarif");
            File.WriteAllText(outputPath, "{}"); // pre-existing scan result

            (CreateRunTool create, _, _) = NewTools();

            Action act = () => CreateRunWith(create, outputPath, allowOverwrite: false);

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*already exists*allowOverwrite=true*",
                    "default behavior must protect a prior scan's results from accidental clobbering");

            // File contents are untouched.
            File.ReadAllText(outputPath).Should().Be("{}");
        }

        [Fact]
        public void CreateRun_Succeeds_IfOutputExists_AndAllowOverwriteTrue()
        {
            string outputPath = ScratchPath("overwritable.sarif");
            File.WriteAllText(outputPath, "{}"); // pre-existing

            (CreateRunTool create, _, _) = NewTools();

            string createJson = CreateRunWith(create, outputPath, allowOverwrite: true);
            using JsonDocument doc = JsonDocument.Parse(createJson);
            doc.RootElement.GetProperty("status").GetString().Should().Be("created");
        }

        [Fact]
        public void Finalize_LeavesNoTempFilesBehind()
        {
            string outputPath = ScratchPath("atomic.sarif");
            McpToolFlow.ProduceRichScenario(this.ScratchDir, outputPath);

            File.Exists(outputPath).Should().BeTrue();

            // The atomic-write helper uses ".tmp-<guid>" sidecar files; on
            // success it must move them onto the target and leave no debris.
            string[] residue = Directory.GetFiles(this.ScratchDir, "*.tmp-*");
            residue.Should().BeEmpty(
                "SarifLogWriter.Save must clean up its temp-rename sidecar on success");
        }

        [Fact]
        public void Finalize_AtomicallyOverwrites_PriorFile_WithAllowOverwrite()
        {
            string outputPath = ScratchPath("rewrite.sarif");
            File.WriteAllText(outputPath, "{ \"prior\": true }");

            (CreateRunTool create, AddResultTool addResult, FinalizeRunTool finalize) = NewTools();

            string createJson = CreateRunWith(create, outputPath, allowOverwrite: true);
            using JsonDocument createDoc = JsonDocument.Parse(createJson);
            string runGuid = createDoc.RootElement.GetProperty("run_guid").GetString()!;

            _ = addResult.AddResult(
                runGuid: runGuid,
                ruleId: "CWE-78/api-handler",
                messageText: "Replacement scan result.",
                messageMarkdown: "Replacement scan result.",
                ruleName: "OsCommandInjection",
                ruleDescription: null,
                level: "warning",
                rank: null,
                uri: null,
                startLine: null,
                startColumn: null,
                endLine: null,
                endColumn: null,
                exploitability: null,
                attackerPosition: null,
                handoff: null);

            _ = finalize.Finalize(runGuid, handoffNotes: null);

            // Prior contents are gone; the file is now valid SARIF with the new result.
            SarifLog log = SarifLog.Load(outputPath);
            log.Runs.Should().HaveCount(1);
            log.Runs[0].Results.Should().HaveCount(1);
            log.Runs[0].Results[0].Rule.Id.Should().Be("CWE-78/api-handler");

            // And the temp-rename sidecar from the atomic write was cleaned up.
            Directory.GetFiles(this.ScratchDir, "*.tmp-*").Should().BeEmpty();
        }

        private (CreateRunTool Create, AddResultTool AddResult, FinalizeRunTool Finalize) NewTools()
        {
            var store = new SarifRunStore();
            var cweResolver = new CweNameResolver();
            IOptions<SarifMcpOptions> options = Options.Create(new SarifMcpOptions());
            return (
                new CreateRunTool(store, cweResolver, options),
                new AddResultTool(store),
                new FinalizeRunTool(store));
        }

        private string CreateRunWith(CreateRunTool create, string outputPath, bool allowOverwrite)
        {
            return create.CreateRun(
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
                campaignGuid: null,
                allowOverwrite: allowOverwrite);
        }
    }
}
