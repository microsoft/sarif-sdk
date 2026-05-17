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

using Xunit;

namespace Test.UnitTests.Sarif.Mcp.Server
{
    /// <summary>
    /// In-process end-to-end exercise of the MCP tool flow without spinning up
    /// the stdio transport. Validates that create-run + add-result + finalize
    /// produces a SARIF file the SDK can round-trip cleanly, and pins the
    /// redaction-removal contract at the output level (no <c>redacted_path</c>).
    /// </summary>
    public class EndToEndFlowTests : IDisposable
    {
        private readonly string _scratchDir;

        public EndToEndFlowTests()
        {
            this._scratchDir = Path.Combine(
                Path.GetTempPath(),
                "sarif-mcp-tests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(this._scratchDir);
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(this._scratchDir))
                {
                    Directory.Delete(this._scratchDir, recursive: true);
                }
            }
            catch
            {
                // Best-effort cleanup; tests should not fail on tear-down errors.
            }

            GC.SuppressFinalize(this);
        }

        [Fact]
        public void CreateAddFinalize_ProducesSarifThatRoundTripsThroughSdkLoader()
        {
            var store = new SarifRunStore();
            using var cweResolver = new CweNameResolver();
            IOptions<SarifMcpOptions> options = Options.Create(new SarifMcpOptions());

            var create = new CreateRunTool(store, cweResolver, options);
            var addResult = new AddResultTool(store);
            var finalize = new FinalizeRunTool(store);

            string outputPath = Path.Combine(this._scratchDir, "end-to-end.sarif");

            string createJson = create.CreateRun(
                toolName: "AI Security Analyzer",
                toolVersion: "1.0.0",
                repoUri: "https://example.com/example/webapp",
                revisionId: "a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2",
                sourceRoot: this._scratchDir,
                outputPath: outputPath,
                aiOrigin: "generated",
                toolOrganization: "Contoso",
                toolInformationUri: null,
                branch: "main",
                scenario: null,
                campaignGuid: null);

            using JsonDocument createDoc = JsonDocument.Parse(createJson);
            string runGuid = createDoc.RootElement.GetProperty("run_guid").GetString()!;
            runGuid.Should().NotBeNullOrEmpty();

            string addJson = addResult.AddResult(
                runGuid: runGuid,
                ruleId: "CWE-78/api-handler",
                messageText: "Command injection via 'command' parameter on the API handler.",
                messageMarkdown: "## Command Injection\nThe `command` parameter is passed to `subprocess.run(..., shell=True)`.",
                ruleName: "CommandInjectionApiHandler",
                ruleDescription: "OS command injection via unsanitized API parameter.",
                level: "error",
                rank: 92.5,
                uri: null,
                startLine: null,
                startColumn: null,
                endLine: null,
                endColumn: null,
                exploitability: "demonstrated",
                attackerPosition: "unauthenticated-network",
                handoff: null);

            using JsonDocument addDoc = JsonDocument.Parse(addJson);
            addDoc.RootElement.GetProperty("status").GetString().Should().Be("added");
            addDoc.RootElement.GetProperty("rule_registered").GetBoolean().Should().BeTrue();

            string finalizeJson = finalize.Finalize(runGuid, handoffNotes: "Remediation pending.");

            using JsonDocument finalizeDoc = JsonDocument.Parse(finalizeJson);
            finalizeDoc.RootElement.GetProperty("status").GetString().Should().Be("finalized");
            finalizeDoc.RootElement.GetProperty("result_count").GetInt32().Should().Be(1);
            finalizeDoc.RootElement.GetProperty("rule_count").GetInt32().Should().Be(1);

            // Contract pin: redaction was removed from this server. The finalize
            // result must not expose a redacted_path field.
            finalizeDoc.RootElement.TryGetProperty("redacted_path", out _).Should().BeFalse(
                "the MCP server does not produce redacted SARIF; redacted_path was removed during the port");

            // The finalize file must round-trip through the SDK loader cleanly.
            File.Exists(outputPath).Should().BeTrue();
            SarifLog roundTripped = SarifLog.Load(outputPath);
            roundTripped.Should().NotBeNull();
            roundTripped.Runs.Should().HaveCount(1);
            roundTripped.Runs[0].Results.Should().HaveCount(1);
            roundTripped.Runs[0].Tool.Driver.Rules.Should().HaveCount(1);
            roundTripped.Runs[0].Tool.Driver.Rules[0].Id.Should().Be("CWE-78");

            // Per SARIF \u00a73.52.4, the sub-id ("CWE-78/api-handler") rides on
            // result.rule.id rather than result.ruleId. The SDK serializer
            // (ShouldSerializeRuleId) omits Result.RuleId when Result.Rule.Id
            // is set, so the AI-plug-in convention of putting the sub-id only
            // on Result.RuleId would lose it through this codepath.
            roundTripped.Runs[0].Results[0].Rule.Should().NotBeNull();
            roundTripped.Runs[0].Results[0].Rule.Id.Should().Be("CWE-78/api-handler");
            roundTripped.Runs[0].Results[0].Rule.Index.Should().Be(0);
        }

        [Fact]
        public void SamplePortedFixture_LoadsThroughTheSdk()
        {
            string fixturePath = Path.Combine(
                AppContext.BaseDirectory,
                "sample-ai-generated-basic.sarif");

            File.Exists(fixturePath).Should().BeTrue(
                "the AI-generated sample SARIF must be deployed alongside the test binaries");

            SarifLog log = SarifLog.Load(fixturePath);
            log.Should().NotBeNull();
            log.Runs.Should().HaveCount(1);
            log.Runs[0].Results.Should().HaveCount(1);
            log.Runs[0].Tool.Driver.Name.Should().Be("AI Security Analyzer");
            log.Runs[0].Results[0].RuleId.Should().Be("CWE-78/api-handler");
        }
    }
}
