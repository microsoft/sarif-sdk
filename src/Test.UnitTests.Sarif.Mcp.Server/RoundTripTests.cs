// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Linq;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif;

using Test.UnitTests.Sarif.Mcp.Server.Fixtures;

using Xunit;

namespace Test.UnitTests.Sarif.Mcp.Server
{
    /// <summary>
    /// One rich scenario driven through the full MCP emit surface, then a
    /// disk round-trip via the SDK loader. A single test surfaces a wide
    /// class of wire-marshaling bugs (enums-as-int, lost properties, broken
    /// converters) without needing dozens of atomic per-field tests.
    /// </summary>
    public sealed class RoundTripTests : McpScratchTestBase
    {
        [Fact]
        public void Roundtrip_RichScenario_PreservesEveryEmitField()
        {
            string outputPath = ScratchPath("rich.sarif");
            McpToolFlow.ProduceRichScenario(this.ScratchDir, outputPath);

            File.Exists(outputPath).Should().BeTrue();

            SarifLog log = SarifLog.Load(outputPath);

            // Run shape
            log.Runs.Should().HaveCount(1);
            Run run = log.Runs[0];

            // Tool driver + extensions surface
            run.Tool.Driver.Name.Should().Be("AI Security Analyzer");
            run.Tool.Driver.SemanticVersion.Should().Be("1.2.0");
            run.Tool.Driver.Organization.Should().Be("Contoso");
            run.Tool.Driver.InformationUri.Should().NotBeNull();
            run.Tool.Driver.InformationUri.AbsoluteUri.Should().Be("https://example.com/ai-security-analyzer");

            // Rule registration: single CWE-78 descriptor for both results
            run.Tool.Driver.Rules.Should().HaveCount(1);
            ReportingDescriptor cwe78 = run.Tool.Driver.Rules.Single();
            cwe78.Id.Should().Be("CWE-78");
            cwe78.Name.Should().Be("CommandInjectionApiHandler");
            cwe78.HelpUri.Should().NotBeNull();
            cwe78.HelpUri.AbsoluteUri.Should().Be("https://cwe.mitre.org/data/definitions/78.html");
            cwe78.DefaultConfiguration.Level.Should().Be(FailureLevel.Error);

            // Notification descriptors registered (one execution, one configuration kind \u2014 both
            // land in the driver's notifications array; kind is a placement decision at the
            // invocation level, not on the descriptor).
            run.Tool.Driver.Notifications.Should().HaveCount(2);
            run.Tool.Driver.Notifications.Select(n => n.Id)
                .Should().BeEquivalentTo(new[] { "SCAN-STARTED", "MODEL-SELECTED" });

            // Automation details: GUID is a real Guid?, automation id carries scenario suffix
            run.AutomationDetails.Should().NotBeNull();
            run.AutomationDetails.Guid.Should().NotBeNull();
            run.AutomationDetails.Id.Should().Contain("example/webapp");

            // Run-level wire surface
            run.ColumnKind.Should().Be(ColumnKind.Utf16CodeUnits);
            run.TryGetProperty("ai/origin", out string aiOrigin).Should().BeTrue();
            aiOrigin.Should().Be("generated");
            run.TryGetProperty("ai/handoff", out string handoff).Should().BeTrue();
            handoff.Should().Be("Two findings pending remediation review.");

            // VersionControlProvenance
            run.VersionControlProvenance.Should().HaveCount(1);
            VersionControlDetails vc = run.VersionControlProvenance[0];
            vc.RepositoryUri.AbsoluteUri.Should().Be("https://example.com/example/webapp");
            vc.RevisionId.Should().Be("a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2");
            vc.Branch.Should().Be("main");
            vc.MappedTo.UriBaseId.Should().Be("SRCROOT");

            // OriginalUriBaseIds
            run.OriginalUriBaseIds.Should().ContainKey("SRCROOT");

            // Invocations
            run.Invocations.Should().HaveCount(1);
            Invocation inv = run.Invocations.Single();
            inv.ExecutionSuccessful.Should().BeTrue();
            inv.ExitCode.Should().Be(0);
            inv.Machine.Should().Be("ci-host-01");
            inv.Account.Should().Be("ci-runner");
            inv.StartTimeUtc.Should().NotBe(default);
            inv.EndTimeUtc.Should().NotBe(default);
            inv.TryGetProperty("ai/agentName", out string agentName).Should().BeTrue();
            agentName.Should().Be("detect-agent");

            // Both notification kinds present, sourced from the same invocation
            inv.ToolExecutionNotifications.Should().HaveCount(1);
            inv.ToolExecutionNotifications[0].Descriptor.Id.Should().Be("SCAN-STARTED");
            inv.ToolConfigurationNotifications.Should().HaveCount(1);
            inv.ToolConfigurationNotifications[0].Descriptor.Id.Should().Be("MODEL-SELECTED");

            // Results: two of them, both pointing at the CWE-78 descriptor; first with
            // location, snippet, context region, AI properties; second with no location.
            run.Results.Should().HaveCount(2);

            Result first = run.Results[0];
            first.Rule.Should().NotBeNull();
            first.Rule.Id.Should().Be("CWE-78/api-handler");
            first.Rule.Index.Should().Be(0);
            first.Level.Should().Be(FailureLevel.Error);
            first.Kind.Should().Be(ResultKind.Fail);
            first.Rank.Should().Be(92.5);
            first.Guid.Should().NotBeNull();
            first.Message.Text.Should().StartWith("Command injection");
            first.Message.Markdown.Should().StartWith("## Command Injection");
            first.TryGetProperty("ai/exploitability", out string exploit).Should().BeTrue();
            exploit.Should().Be("demonstrated");
            first.TryGetProperty("ai/attackerPosition", out string ap).Should().BeTrue();
            ap.Should().Be("unauthenticated-network");
            first.TryGetProperty("ai/handoff", out string resultHandoff).Should().BeTrue();
            resultHandoff.Should().Be("Allowlist commands and remove shell=True.");

            first.Locations.Should().HaveCount(1);
            PhysicalLocation phys = first.Locations[0].PhysicalLocation;
            phys.ArtifactLocation.Uri.OriginalString.Should().Be("src/handler.py");
            phys.ArtifactLocation.UriBaseId.Should().Be("SRCROOT");
            phys.ArtifactLocation.Index.Should().Be(0);
            phys.Region.StartLine.Should().Be(4);
            phys.Region.Snippet.Should().NotBeNull();
            phys.Region.Snippet.Text.Should().Contain("subprocess.run");
            phys.ContextRegion.Should().NotBeNull();
            phys.ContextRegion.StartLine.Should().BeLessThan(phys.Region.StartLine,
                "context region must be a proper superset per SARIF \u00a73.30 / SARIF1008");

            Result second = run.Results[1];
            second.Rule.Id.Should().Be("CWE-78/timeout-not-validated");
            second.Rule.Index.Should().Be(0, "the second result reuses the registered CWE-78 descriptor");
            second.Locations.Should().BeNullOrEmpty();

            // Artifacts table: one entry for handler.py, with hashes + length
            run.Artifacts.Should().HaveCount(1);
            Artifact artifact = run.Artifacts[0];
            artifact.Location.Uri.OriginalString.Should().Be("src/handler.py");
            artifact.Hashes.Should().ContainKey("sha-256");
            artifact.Hashes["sha-256"].Should().MatchRegex("^[0-9A-F]{64}$",
                "SDK HashUtilities emits uppercase hex (named wire-output delta from the AI plug-in's lowercase \u2014 see PR-A)");
            artifact.Length.Should().BeGreaterThan(0);
        }
    }
}
