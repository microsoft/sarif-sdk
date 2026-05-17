// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif.Mcp.Server;
using Microsoft.CodeAnalysis.Sarif.Mcp.Server.Tools;
using Microsoft.Extensions.Options;

using Test.UnitTests.Sarif.Mcp.Server.Fixtures;

using Xunit;

namespace Test.UnitTests.Sarif.Mcp.Server
{
    /// <summary>
    /// Pins negative-path contracts of the MCP emit surface: malformed
    /// inputs are rejected with informative errors, and the run is
    /// immutable after finalize.
    /// </summary>
    public sealed class NegativeTests : McpScratchTestBase
    {
        [Theory]
        [InlineData("CWE-78", "ruleId without a sub-id slash is invalid for the AI profile")]
        [InlineData("CWE-78/", "trailing slash with empty sub-id is invalid")]
        [InlineData("/api-handler", "missing base id is invalid")]
        public void AddResult_WithMalformedRuleId_Throws(string ruleId, string scenario)
        {
            (string runGuid, AddResultTool addResult) = OpenRunAndGetAddResult();

            Action act = () => addResult.AddResult(
                runGuid: runGuid,
                ruleId: ruleId,
                messageText: "irrelevant",
                messageMarkdown: "irrelevant",
                ruleName: "Irrelevant",
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

            act.Should().Throw<ArgumentException>(scenario);
        }

        [Fact]
        public void PostFinalize_AddResult_Throws()
        {
            string outputPath = ScratchPath("post-finalize.sarif");

            var store = new SarifRunStore();
            using var cweResolver = new CweNameResolver();
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
            using System.Text.Json.JsonDocument doc = System.Text.Json.JsonDocument.Parse(createJson);
            string runGuid = doc.RootElement.GetProperty("run_guid").GetString()!;

            _ = finalize.Finalize(runGuid, handoffNotes: null);

            // After finalize, the run is removed from the store and no longer accepts mutations.
            // The contract surface: the next caller looking up the run gets a meaningful error.
            Action act = () => addResult.AddResult(
                runGuid: runGuid,
                ruleId: "CWE-78/api-handler",
                messageText: "should fail",
                messageMarkdown: "should fail",
                ruleName: "CommandInjection",
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

            act.Should().Throw<InvalidOperationException>(
                "finalized runs are removed from the store; subsequent mutations against the same run_guid must fail");
        }

        private (string RunGuid, AddResultTool AddResult) OpenRunAndGetAddResult()
        {
            var store = new SarifRunStore();
            using var cweResolver = new CweNameResolver();
            IOptions<SarifMcpOptions> options = Options.Create(new SarifMcpOptions());
            var create = new CreateRunTool(store, cweResolver, options);
            var addResult = new AddResultTool(store);

            string outputPath = Path.Combine(this.ScratchDir, "negative.sarif");
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
            using System.Text.Json.JsonDocument doc = System.Text.Json.JsonDocument.Parse(createJson);
            string runGuid = doc.RootElement.GetProperty("run_guid").GetString()!;
            return (runGuid, addResult);
        }
    }
}
