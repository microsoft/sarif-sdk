// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Text.Json;

using Microsoft.CodeAnalysis.Sarif.Mcp.Server;
using Microsoft.CodeAnalysis.Sarif.Mcp.Server.Tools;
using Microsoft.Extensions.Options;

namespace Test.UnitTests.Sarif.Mcp.Server.Fixtures
{
    /// <summary>
    /// Drives the MCP tool surface (CreateRun + Start/End-Invocation +
    /// Add-Result + Add-Notification + Finalize) through a single, rich
    /// scenario whose output covers as much of the SARIF emit surface as
    /// the server produces. One produced SARIF feeds several high-information
    /// tests (round-trip, schema-validation, wire-format), so each of those
    /// tests is concise.
    /// </summary>
    public static class McpToolFlow
    {
        public sealed record FlowResult(string RunGuid, string OutputPath);

        public static FlowResult ProduceRichScenario(string sourceRoot, string outputPath)
        {
            var store = new SarifRunStore();
            var cweResolver = new CweNameResolver();
            IOptions<SarifMcpOptions> options = Options.Create(new SarifMcpOptions());

            var create = new CreateRunTool(store, cweResolver, options);
            var start = new StartInvocationTool(store);
            var end = new EndInvocationTool(store);
            var addResult = new AddResultTool(store);
            var addNotification = new AddNotificationTool(store);
            var finalize = new FinalizeRunTool(store);

            // Source file the enrichment cascade can read.
            string sourceFile = Path.Combine(sourceRoot, "src", "handler.py");
            Directory.CreateDirectory(Path.GetDirectoryName(sourceFile)!);
            File.Copy(
                Path.Combine(AppContext.BaseDirectory, "TestFiles", "handler.py"),
                sourceFile,
                overwrite: true);

            // 1. create run
            string runGuid = JsonAt(
                create.CreateRun(
                    toolName: "AI Security Analyzer",
                    toolVersion: "1.2.0",
                    repoUri: "https://example.com/example/webapp",
                    revisionId: "a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2",
                    sourceRoot: sourceRoot,
                    outputPath: outputPath,
                    aiOrigin: "generated",
                    toolOrganization: "Contoso",
                    toolInformationUri: "https://example.com/ai-security-analyzer",
                    branch: "main",
                    scenario: null,
                    campaignGuid: null),
                "run_guid");

            // 2. first invocation: detect agent
            int detectInvocation = JsonIntAt(
                start.StartInvocation(
                    runGuid: runGuid,
                    agentName: "detect-agent",
                    description: "Initial vulnerability sweep over the request handlers.",
                    machine: "ci-host-01",
                    account: "ci-runner"),
                "invocationIndex");

            // 3. results — share a rule (CWE-78), trigger CWE name resolution, populate enrichment
            //    Result 1: command injection in handler.py via subprocess.run(shell=True)
            _ = addResult.AddResult(
                runGuid: runGuid,
                ruleId: "CWE-78/api-handler",
                messageText: "Command injection via 'command' parameter on the API handler.",
                messageMarkdown: "## Command Injection\nUnsanitized `command` argument flows to `subprocess.run(..., shell=True)`.",
                ruleName: "CommandInjectionApiHandler",
                ruleDescription: "OS command injection through unsanitized API parameter.",
                level: "error",
                rank: 92.5,
                uri: "src/handler.py",
                startLine: 4,
                startColumn: 5,
                endLine: 4,
                endColumn: null,
                exploitability: "demonstrated",
                attackerPosition: "unauthenticated-network",
                handoff: "Allowlist commands and remove shell=True.");

            //    Result 2: same rule, no location (idempotent rule registration)
            _ = addResult.AddResult(
                runGuid: runGuid,
                ruleId: "CWE-78/timeout-not-validated",
                messageText: "The 'timeout' parameter is not validated before use.",
                messageMarkdown: "The `timeout` value is taken directly from the request body.",
                ruleName: "CommandInjectionApiHandler",
                ruleDescription: null,
                level: "warning",
                rank: 60.0,
                uri: null,
                startLine: null,
                startColumn: null,
                endLine: null,
                endColumn: null,
                exploitability: "theoretical",
                attackerPosition: "authenticated-user",
                handoff: null);

            // 4. notifications: one execution (scan started), one configuration (model)
            _ = addNotification.AddNotification(
                runGuid: runGuid,
                descriptorId: "SCAN-STARTED",
                message: "Scan started against the request handlers in src/.",
                messageMarkdown: "## Scan started\nTarget: `src/`",
                kind: "execution",
                level: "note",
                timeUtc: "2026-05-17T13:00:00Z",
                invocationIndex: detectInvocation);

            _ = addNotification.AddNotification(
                runGuid: runGuid,
                descriptorId: "MODEL-SELECTED",
                message: "Selected model gpt-4o for inference.",
                messageMarkdown: "Selected `gpt-4o`.",
                kind: "configuration",
                level: "note",
                timeUtc: null,
                invocationIndex: detectInvocation);

            // 5. end invocation
            _ = end.EndInvocation(
                runGuid: runGuid,
                invocationIndex: detectInvocation,
                success: true,
                exitCode: 0,
                summary: "2 results, 1 rule.");

            // 6. finalize with handoff
            _ = finalize.Finalize(runGuid, handoffNotes: "Two findings pending remediation review.");

            return new FlowResult(runGuid, outputPath);
        }

        private static string JsonAt(string json, string property)
        {
            using JsonDocument doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty(property).GetString()!;
        }

        private static int JsonIntAt(string json, string property)
        {
            using JsonDocument doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty(property).GetInt32();
        }
    }
}
