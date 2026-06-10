// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Linq;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif.Driver;
using Microsoft.CodeAnalysis.Sarif.Emit;

using Newtonsoft.Json;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class EmitFinalizeCommandTests : IDisposable
    {
        private readonly string _dir;

        public EmitFinalizeCommandTests()
        {
            _dir = Path.Combine(Path.GetTempPath(), $"emit-finalize-{Guid.NewGuid():N}");
            Directory.CreateDirectory(_dir);
        }

        public void Dispose()
        {
            if (Directory.Exists(_dir)) { Directory.Delete(_dir, recursive: true); }
        }

        private string OutPath => Path.Combine(_dir, "scan.sarif");
        private string WipPath => OutPath + ".wip.jsonl";

        private void SeedWip(params (string kind, object payload)[] events)
        {
            using var w = new SarifEventLogWriter(WipPath);
            foreach ((string kind, object payload) in events) { w.Append(kind, payload); }
        }

        private SarifLog LoadSarif()
        {
            using var sr = new StreamReader(OutPath);
            using var jr = new JsonTextReader(sr);
            return JsonSerializer.CreateDefault().Deserialize<SarifLog>(jr);
        }

        private const string FrozenSha = "1234567890abcdef1234567890abcdef12345678";

        // emit-finalize now requires every run to declare versionControlProvenance with a
        // mappedTo-bound local root so it can deconstruct local paths into portable permalinks.
        private static Run RunHeader()
            => new Run
            {
                Tool = new Tool { Driver = new ToolComponent { Name = "demo" } },
                VersionControlProvenance = new[]
                {
                    new VersionControlDetails
                    {
                        RepositoryUri = new Uri("https://github.com/microsoft/sarif-sdk", UriKind.Absolute),
                        RevisionId = FrozenSha,
                        Branch = "refs/heads/main",
                        MappedTo = new ArtifactLocation { UriBaseId = "SRCROOT" },
                    },
                },
                OriginalUriBaseIds = new System.Collections.Generic.Dictionary<string, ArtifactLocation>
                {
                    ["SRCROOT"] = new ArtifactLocation { Uri = new Uri("file:///d:/repo/", UriKind.Absolute) },
                },
            };

        private const string FrozenAdoRevisionId = "cafebabecafebabecafebabecafebabecafebabe";

        // An Azure DevOps-hosted counterpart to RunHeader(): the run's repositoryUri host is
        // dev.azure.com, so the GitHub-only finalize enrichments (security-severity, rolling-hash
        // primaryLocationLineHash) must NOT be applied.
        private static Run AdoRunHeader()
            => new Run
            {
                Tool = new Tool { Driver = new ToolComponent { Name = "demo" } },
                VersionControlProvenance = new[]
                {
                    new VersionControlDetails
                    {
                        RepositoryUri = new Uri("https://dev.azure.com/example-org/example-project/_git/sarif-sdk", UriKind.Absolute),
                        RevisionId = FrozenAdoRevisionId,
                        Branch = "refs/heads/main",
                        MappedTo = new ArtifactLocation { UriBaseId = "SRCROOT" },
                    },
                },
                OriginalUriBaseIds = new System.Collections.Generic.Dictionary<string, ArtifactLocation>
                {
                    ["SRCROOT"] = new ArtifactLocation { Uri = new Uri("file:///d:/repo/", UriKind.Absolute) },
                },
            };

        [Fact]
        public void Run_HappyPath_WritesSarifWithEnrichedCweDescriptorsAndRemovesWip()
        {
            SeedWip(
                (SarifEventKinds.RunHeader, RunHeader()),
                (SarifEventKinds.Result, new Result { RuleId = "CWE-79/template-xss", Message = new Message { Text = "xss" } }),
                (SarifEventKinds.Result, new Result { RuleId = "NOVEL-custom", Message = new Message { Text = "n/a" } }));

            int exit = new EmitFinalizeCommand().Run(new EmitFinalizeOptions { OutputFilePath = OutPath });

            exit.Should().Be(CommandBase.SUCCESS);
            File.Exists(OutPath).Should().BeTrue();
            File.Exists(WipPath).Should().BeFalse();

            SarifLog log = LoadSarif();
            log.Runs[0].Tool.Driver.Rules.Should().HaveCount(2);
            log.Runs[0].Tool.Driver.Rules[0].Id.Should().Be("CWE-79");
            log.Runs[0].Tool.Driver.Rules[0].HelpUri.Should().NotBeNull();
            log.Runs[0].Tool.Driver.Rules[0].Name.Should().NotBeNullOrEmpty();
            log.Runs[0].Tool.Driver.Rules[1].Id.Should().Be("NOVEL-custom");
            log.Runs[0].Tool.Driver.Rules[1].HelpUri.Should().BeNull();
        }

        [Fact]
        public void Run_FailsIfWipDoesNotExist()
        {
            int exit = new EmitFinalizeCommand().Run(new EmitFinalizeOptions { OutputFilePath = OutPath });
            exit.Should().Be(CommandBase.FAILURE);
            File.Exists(OutPath).Should().BeFalse();
        }

        [Fact]
        public void Run_WithNoCweEnrichment_LeavesCweDescriptorBare()
        {
            SeedWip(
                (SarifEventKinds.RunHeader, RunHeader()),
                (SarifEventKinds.Result, new Result { RuleId = "CWE-79/template-xss", Message = new Message { Text = "xss" } }));

            int exit = new EmitFinalizeCommand().Run(new EmitFinalizeOptions
            {
                OutputFilePath = OutPath,
                NoCweEnrichment = true,
            });

            exit.Should().Be(CommandBase.SUCCESS);
            SarifLog log = LoadSarif();
            ReportingDescriptor descriptor = log.Runs[0].Tool.Driver.Rules[0];
            descriptor.Id.Should().Be("CWE-79");
            descriptor.HelpUri.Should().BeNull();
            descriptor.Name.Should().BeNull();
        }

        [Fact]
        public void Run_WithKeepWip_RetainsWipAfterSuccess()
        {
            SeedWip(
                (SarifEventKinds.RunHeader, RunHeader()));

            int exit = new EmitFinalizeCommand().Run(new EmitFinalizeOptions
            {
                OutputFilePath = OutPath,
                KeepWip = true,
            });

            exit.Should().Be(CommandBase.SUCCESS);
            File.Exists(WipPath).Should().BeTrue();
            File.Exists(OutPath).Should().BeTrue();
        }

        [Fact]
        public void Run_AtomicallyReplacesExistingSarif()
        {
            // Pre-existing SARIF on disk should be replaced wholesale, not appended to.
            File.WriteAllText(OutPath, "{ \"stale\": true }");

            SeedWip(
                (SarifEventKinds.RunHeader, RunHeader()),
                (SarifEventKinds.Result, new Result { RuleId = "NOVEL-rule-1", Message = new Message { Text = "x" } }));

            int exit = new EmitFinalizeCommand().Run(new EmitFinalizeOptions { OutputFilePath = OutPath });

            exit.Should().Be(CommandBase.SUCCESS);
            string contents = File.ReadAllText(OutPath);
            contents.Should().NotContain("stale");
            contents.Should().Contain("NOVEL-rule-1");
        }
        [Fact]
        public void Run_RejectsNonCompliantRuleId_WritesAIRuleIdEnvelopeToStderr()
        {
            SeedWip(
                (SarifEventKinds.RunHeader, new Run { Tool = new Tool { Driver = new ToolComponent { Name = "demo" } } }),
                (SarifEventKinds.Result, new Result { RuleId = "CWE-79", Message = new Message { Text = "xss" } }));

            string capturedStderr;
            int exit;
            using (var writer = new StringWriter())
            {
                TextWriter original = Console.Error;
                Console.SetError(writer);
                try
                {
                    exit = new EmitFinalizeCommand().Run(new EmitFinalizeOptions { OutputFilePath = OutPath });
                }
                finally
                {
                    Console.SetError(original);
                }
                capturedStderr = writer.ToString();
            }

            exit.Should().Be(CommandBase.FAILURE);
            File.Exists(OutPath).Should().BeFalse();
            capturedStderr.Should().Contain(AIRuleIdConventionException.ErrorCode);
            capturedStderr.Should().Contain("'CWE-79'");
            capturedStderr.Should().NotContain("at Microsoft.CodeAnalysis.Sarif", "the catch block should write the envelope, not a stack trace");
        }

        [Fact]
        public void Run_RebasesSrcRootToPortableGitHubPermalink()
        {
            // Producer emits with a local file:// SRCROOT so InsertOptionalDataVisitor can
            // resolve sources; finalize deconstructs that local anchor into a portable GitHub
            // blob permalink derived from versionControlProvenance. The shipped artifact must
            // carry the post-rebase value and keep result URIs relative.
            SeedWip(
                (SarifEventKinds.RunHeader, RunHeader()),
                (SarifEventKinds.Result, new Result
                {
                    RuleId = "NOVEL-test",
                    Message = new Message { Text = "x" },
                    Locations = new[]
                    {
                        new Location
                        {
                            PhysicalLocation = new PhysicalLocation
                            {
                                ArtifactLocation = new ArtifactLocation
                                {
                                    Uri = new Uri("src/a.cs", UriKind.Relative),
                                    UriBaseId = "SRCROOT",
                                },
                            },
                        },
                    },
                }));

            int exit = new EmitFinalizeCommand().Run(new EmitFinalizeOptions { OutputFilePath = OutPath });

            exit.Should().Be(CommandBase.SUCCESS);
            SarifLog log = LoadSarif();
            log.Runs[0].OriginalUriBaseIds.Should().ContainKey("SRCROOT");
            log.Runs[0].OriginalUriBaseIds["SRCROOT"].Uri
                .Should().Be(new Uri($"https://github.com/microsoft/sarif-sdk/blob/{FrozenSha}/", UriKind.Absolute));

            ArtifactLocation shipped = log.Runs[0].Results[0].Locations[0].PhysicalLocation.ArtifactLocation;
            shipped.Uri.OriginalString.Should().Be("src/a.cs");
            shipped.UriBaseId.Should().Be("SRCROOT");
        }

        [Fact]
        public void Run_FailsWhenRunHasNoVersionControlProvenance()
        {
            // The finalize contract requires versionControlProvenance so local paths can be
            // rebased to portable permalinks; a run without it is refused before any file ships.
            SeedWip(
                (SarifEventKinds.RunHeader, new Run { Tool = new Tool { Driver = new ToolComponent { Name = "demo" } } }),
                (SarifEventKinds.Result, new Result { RuleId = "NOVEL-test", Message = new Message { Text = "x" } }));

            string capturedStderr;
            int exit;
            using (var writer = new StringWriter())
            {
                TextWriter original = Console.Error;
                Console.SetError(writer);
                try
                {
                    exit = new EmitFinalizeCommand().Run(new EmitFinalizeOptions { OutputFilePath = OutPath });
                }
                finally
                {
                    Console.SetError(original);
                }
                capturedStderr = writer.ToString();
            }

            exit.Should().Be(CommandBase.FAILURE);
            File.Exists(OutPath).Should().BeFalse();
            capturedStderr.Should().Contain("versionControlProvenance");
        }

        [Fact]
        public void Run_WithValidateFlag_ReturnsFailureWhenErrorFindingsPresent()
        {
            // A run that carries versionControlProvenance (so finalize can rebase) but is
            // otherwise bare of AI-profile metadata fires several AI* error-level findings
            // (AI1006 missing ai/origin, automationDetails, etc.). The --validate gate should
            // propagate this as a FAILURE exit code and leave the report file on disk for
            // forensics. The clean-input success path is covered with higher fidelity by the
            // CweGenerateSample.ps1 + CweGhasSample.sarif integration fixture.
            SeedWip(
                (SarifEventKinds.RunHeader, RunHeader()));

            int exit = new EmitFinalizeCommand().Run(new EmitFinalizeOptions
            {
                OutputFilePath = OutPath,
                Validate = true,
            });

            exit.Should().Be(CommandBase.FAILURE);
            File.Exists(OutPath).Should().BeTrue();
            File.Exists(Path.Combine(_dir, "scan.validate-report.sarif")).Should().BeTrue();
        }

        [Fact]
        public void Run_StampsSecuritySeverityFromObservedMaxRank()
        {
            // Two findings on the same CWE rule (sub-id forms both collapse to descriptor
            // "CWE-79"); the higher rank (85) wins and maps to security-severity 8.5.
            SeedWip(
                (SarifEventKinds.RunHeader, RunHeader()),
                (SarifEventKinds.Result, new Result { RuleId = "CWE-79/template-xss", Rank = 60, Message = new Message { Text = "xss" } }),
                (SarifEventKinds.Result, new Result { RuleId = "CWE-79/dom-xss", Rank = 85, Message = new Message { Text = "xss" } }));

            int exit = new EmitFinalizeCommand().Run(new EmitFinalizeOptions { OutputFilePath = OutPath });

            exit.Should().Be(CommandBase.SUCCESS);
            ReportingDescriptor rule = LoadSarif().Runs[0].Tool.Driver.Rules.Single(r => r.Id == "CWE-79");
            SecuritySeverityOf(rule).Should().Be("8.5");
        }

        [Fact]
        public void Run_DoesNotStampSecuritySeverityForAzureDevOpsHostedRun()
        {
            // Same ranked findings as the github-hosted case, but the run is dev.azure.com-hosted.
            // security-severity is a GitHub property with no Azure DevOps analog, so finalize must
            // not add it.
            SeedWip(
                (SarifEventKinds.RunHeader, AdoRunHeader()),
                (SarifEventKinds.Result, new Result { RuleId = "CWE-79/template-xss", Rank = 60, Message = new Message { Text = "xss" } }),
                (SarifEventKinds.Result, new Result { RuleId = "CWE-79/dom-xss", Rank = 85, Message = new Message { Text = "xss" } }));

            int exit = new EmitFinalizeCommand().Run(new EmitFinalizeOptions { OutputFilePath = OutPath });

            exit.Should().Be(CommandBase.SUCCESS);
            ReportingDescriptor rule = LoadSarif().Runs[0].Tool.Driver.Rules.Single(r => r.Id == "CWE-79");
            HasSecuritySeverity(rule).Should().BeFalse();
        }

        [Fact]
        public void Run_PreservesProducerAuthoredSecuritySeverityOnAzureDevOpsHostedRun()
        {
            // Finalize does not ADD security-severity to an Azure DevOps run, but a value the
            // producer authored is left untouched (the gate only governs the enrichment, not removal).
            var seededRule = new ReportingDescriptor { Id = "CWE-79" };
            seededRule.SetProperty("security-severity", "2.0");

            SeedWip(
                (SarifEventKinds.RunHeader, AdoRunHeader()),
                (SarifEventKinds.RuleDescriptor, seededRule),
                (SarifEventKinds.Result, new Result { RuleId = "CWE-79/dom-xss", Rank = 85, Message = new Message { Text = "xss" } }));

            int exit = new EmitFinalizeCommand().Run(new EmitFinalizeOptions { OutputFilePath = OutPath });

            exit.Should().Be(CommandBase.SUCCESS);
            ReportingDescriptor rule = LoadSarif().Runs[0].Tool.Driver.Rules.Single(r => r.Id == "CWE-79");
            SecuritySeverityOf(rule).Should().Be("2.0");
        }

        [Fact]
        public void IsGitHubHostedRun_TrueForGitHubHostedRun()
        {
            VcpPortableRoot.IsGitHubHostedRun(RunHeader()).Should().BeTrue();
        }

        [Fact]
        public void IsGitHubHostedRun_FalseForAzureDevOpsHostedRun()
        {
            VcpPortableRoot.IsGitHubHostedRun(AdoRunHeader()).Should().BeFalse();
        }

        [Fact]
        public void IsGitHubHostedRun_FalseForMixedGitHubAndAzureDevOpsProvenance()
        {
            // Default-deny: a run is GitHub-hosted only when EVERY provenance entry is GitHub.
            // A single Azure DevOps entry forfeits the enrichments for the whole run.
            var run = new Run
            {
                VersionControlProvenance = new System.Collections.Generic.List<VersionControlDetails>
                {
                    new VersionControlDetails
                    {
                        RepositoryUri = new Uri("https://github.com/microsoft/sarif-sdk", UriKind.Absolute),
                        RevisionId = FrozenSha,
                        Branch = "refs/heads/main",
                    },
                    new VersionControlDetails
                    {
                        RepositoryUri = new Uri("https://dev.azure.com/example-org/example-project/_git/sarif-sdk", UriKind.Absolute),
                        RevisionId = FrozenAdoRevisionId,
                        Branch = "refs/heads/main",
                    },
                },
            };

            VcpPortableRoot.IsGitHubHostedRun(run).Should().BeFalse();
        }

        [Fact]
        public void IsGitHubHostedRun_FalseWhenRunHasNoVersionControlProvenance()
        {
            VcpPortableRoot.IsGitHubHostedRun(new Run()).Should().BeFalse();
        }

        [Fact]
        public void Run_PreservesProducerAuthoredSecuritySeverityThroughReplay()
        {
            // A producer that authored a rule descriptor with its own security-severity keeps it,
            // even when a ranked result referencing that rule would otherwise derive a higher value.
            var seededRule = new ReportingDescriptor { Id = "CWE-79" };
            seededRule.SetProperty("security-severity", "2.0");

            SeedWip(
                (SarifEventKinds.RunHeader, RunHeader()),
                (SarifEventKinds.RuleDescriptor, seededRule),
                (SarifEventKinds.Result, new Result { RuleId = "CWE-79/dom-xss", Rank = 95, Message = new Message { Text = "xss" } }));

            int exit = new EmitFinalizeCommand().Run(new EmitFinalizeOptions { OutputFilePath = OutPath });

            exit.Should().Be(CommandBase.SUCCESS);
            ReportingDescriptor rule = LoadSarif().Runs[0].Tool.Driver.Rules.Single(r => r.Id == "CWE-79");
            SecuritySeverityOf(rule).Should().Be("2.0");
        }

        [Fact]
        public void ApplyRankDerivedSecuritySeverity_StampsMaxRankDividedByTen()
        {
            Run run = BuildRun(
                new[] { "CWE-89" },
                (ruleIndex: 0, rank: 40.0),
                (ruleIndex: 0, rank: 95.0));

            int stamped = EmitFinalizeCommand.ApplyRankDerivedSecuritySeverity(run);

            stamped.Should().Be(1);
            SecuritySeverityOf(run.Tool.Driver.Rules[0]).Should().Be("9.5");
        }

        [Fact]
        public void ApplyRankDerivedSecuritySeverity_TreatsEachRuleIndependently()
        {
            Run run = BuildRun(
                new[] { "CWE-89", "CWE-79" },
                (ruleIndex: 0, rank: 95.0),
                (ruleIndex: 1, rank: 30.0));

            int stamped = EmitFinalizeCommand.ApplyRankDerivedSecuritySeverity(run);

            stamped.Should().Be(2);
            SecuritySeverityOf(run.Tool.Driver.Rules[0]).Should().Be("9.5");
            SecuritySeverityOf(run.Tool.Driver.Rules[1]).Should().Be("3.0");
        }

        [Fact]
        public void ApplyRankDerivedSecuritySeverity_SkipsRuleWithNoRankedResults()
        {
            // Rank defaults to the -1.0 "unset" sentinel; a rule whose results carry no rank
            // is left bare rather than fabricated a 0.0.
            Run run = BuildRun(
                new[] { "CWE-89" },
                (ruleIndex: 0, rank: -1.0));

            int stamped = EmitFinalizeCommand.ApplyRankDerivedSecuritySeverity(run);

            stamped.Should().Be(0);
            HasSecuritySeverity(run.Tool.Driver.Rules[0]).Should().BeFalse();
        }

        [Fact]
        public void ApplyRankDerivedSecuritySeverity_StampsExplicitZeroRank()
        {
            // Rank 0 is an authored value (lowest severity), distinct from the -1.0 unset
            // sentinel, so it is honored and maps to 0.0.
            Run run = BuildRun(
                new[] { "CWE-89" },
                (ruleIndex: 0, rank: 0.0));

            int stamped = EmitFinalizeCommand.ApplyRankDerivedSecuritySeverity(run);

            stamped.Should().Be(1);
            SecuritySeverityOf(run.Tool.Driver.Rules[0]).Should().Be("0.0");
        }

        [Fact]
        public void ApplyRankDerivedSecuritySeverity_ClampsRankAboveOneHundred()
        {
            Run run = BuildRun(
                new[] { "CWE-89" },
                (ruleIndex: 0, rank: 150.0));

            EmitFinalizeCommand.ApplyRankDerivedSecuritySeverity(run);

            SecuritySeverityOf(run.Tool.Driver.Rules[0]).Should().Be("10.0");
        }

        [Fact]
        public void ApplyRankDerivedSecuritySeverity_MapsRankOneHundredToTen()
        {
            Run run = BuildRun(
                new[] { "CWE-89" },
                (ruleIndex: 0, rank: 100.0));

            EmitFinalizeCommand.ApplyRankDerivedSecuritySeverity(run);

            SecuritySeverityOf(run.Tool.Driver.Rules[0]).Should().Be("10.0");
        }

        [Fact]
        public void ApplyRankDerivedSecuritySeverity_RoundsFractionalRankToOneDecimal()
        {
            // rank 84.9 / 10 = 8.49, formatted to one decimal as 8.5.
            Run run = BuildRun(
                new[] { "CWE-89" },
                (ruleIndex: 0, rank: 84.9));

            EmitFinalizeCommand.ApplyRankDerivedSecuritySeverity(run);

            SecuritySeverityOf(run.Tool.Driver.Rules[0]).Should().Be("8.5");
        }

        [Fact]
        public void ApplyRankDerivedSecuritySeverity_PreservesProducerAuthoredValue()
        {
            Run run = BuildRun(
                new[] { "CWE-89" },
                (ruleIndex: 0, rank: 95.0));
            run.Tool.Driver.Rules[0].SetProperty("security-severity", "2.0");

            int stamped = EmitFinalizeCommand.ApplyRankDerivedSecuritySeverity(run);

            stamped.Should().Be(0);
            SecuritySeverityOf(run.Tool.Driver.Rules[0]).Should().Be("2.0");
        }

        [Fact]
        public void ApplyRankDerivedSecuritySeverity_IgnoresResultWithOutOfRangeRuleIndex()
        {
            Run run = BuildRun(
                new[] { "CWE-89" },
                (ruleIndex: 7, rank: 95.0));

            int stamped = EmitFinalizeCommand.ApplyRankDerivedSecuritySeverity(run);

            stamped.Should().Be(0);
            HasSecuritySeverity(run.Tool.Driver.Rules[0]).Should().BeFalse();
        }

        private static Run BuildRun(string[] ruleIds, params (int ruleIndex, double rank)[] results)
            => new Run
            {
                Tool = new Tool
                {
                    Driver = new ToolComponent
                    {
                        Name = "demo",
                        Rules = ruleIds.Select(id => new ReportingDescriptor { Id = id }).ToList(),
                    },
                },
                Results = results
                    .Select(r => new Result { RuleIndex = r.ruleIndex, Rank = r.rank })
                    .ToList(),
            };

        private static string SecuritySeverityOf(ReportingDescriptor descriptor)
            => descriptor.TryGetProperty("security-severity", out string value) ? value : null;

        private static bool HasSecuritySeverity(ReportingDescriptor descriptor)
            => descriptor.PropertyNames.Contains("security-severity");
    }
}
