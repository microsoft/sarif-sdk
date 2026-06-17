// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
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
        // dev.azure.com, so the GitHub-only rolling-hash primaryLocationLineHash enrichment must
        // NOT be applied (security-severity is host-agnostic and IS applied to ADO runs).
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

        // A repo-less scan: no versionControlProvenance, but a transient local SRCROOT base the
        // producer injected so finalize can read source. --no-repo must finalize it.
        private static Run RepoLessRunHeader()
            => new Run
            {
                Tool = new Tool { Driver = new ToolComponent { Name = "demo" } },
                OriginalUriBaseIds = new System.Collections.Generic.Dictionary<string, ArtifactLocation>
                {
                    ["SRCROOT"] = new ArtifactLocation { Uri = new Uri("file:///d:/scan/root/", UriKind.Absolute) },
                },
            };

        [Fact]
        public void Run_WithNoRepo_FinalizesRepolessScan_ElidesLocalRootAndStampsUnpublishable()
        {
            SeedWip(
                (SarifEventKinds.RunHeader, RepoLessRunHeader()),
                (SarifEventKinds.Result, new Result { RuleId = "NOVEL-x", Message = new Message { Text = "x" } }));

            int exit = new EmitFinalizeCommand().Run(new EmitFinalizeOptions
            {
                OutputFilePath = OutPath,
                NoRepo = true,
            });

            exit.Should().Be(CommandBase.SUCCESS);

            SarifLog log = LoadSarif();
            Run run = log.Runs[0];
            run.OriginalUriBaseIds.Should().ContainKey("SRCROOT");
            run.OriginalUriBaseIds["SRCROOT"].Uri.Should().BeNull("the transient local root is elided");
            run.TryGetProperty(EmitFinalizeCommand.UnpublishablePropertyName, out bool unpublishable).Should().BeTrue();
            unpublishable.Should().BeTrue();

            string raw = File.ReadAllText(OutPath);
            raw.Should().NotContain("file:///", "the finalized log must carry no machine-specific path");
            raw.Should().NotContain("d:/scan/root");
        }

        [Fact]
        public void Run_WithoutNoRepo_AndNoVersionControl_FailsWithNudgeToNoRepo()
        {
            SeedWip(
                (SarifEventKinds.RunHeader, RepoLessRunHeader()),
                (SarifEventKinds.Result, new Result { RuleId = "NOVEL-x", Message = new Message { Text = "x" } }));

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
            capturedStderr.Should().Contain("--no-repo");
        }

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
        public void Run_StampsCweSecuritySeverityFromCuratedTable()
        {
            // A finding on a CWE rule (sub-id form collapses to descriptor "CWE-79"); finalize
            // stamps the curated per-CWE prior (CWE-79 -> 7.8), not anything derived from rank.
            SeedWip(
                (SarifEventKinds.RunHeader, RunHeader()),
                (SarifEventKinds.Result, new Result { RuleId = "CWE-79/template-xss", Rank = 60, Message = new Message { Text = "xss" } }),
                (SarifEventKinds.Result, new Result { RuleId = "CWE-79/dom-xss", Rank = 85, Message = new Message { Text = "xss" } }));

            int exit = new EmitFinalizeCommand().Run(new EmitFinalizeOptions { OutputFilePath = OutPath });

            exit.Should().Be(CommandBase.SUCCESS);
            ReportingDescriptor rule = LoadSarif().Runs[0].Tool.Driver.Rules.Single(r => r.Id == "CWE-79");
            SecuritySeverityOf(rule).Should().Be("7.8");
        }

        [Fact]
        public void Run_StampsCweSecuritySeverityForAzureDevOpsHostedRun()
        {
            // security-severity is host-agnostic: Azure DevOps Advanced Security reads it off the
            // rule descriptor on the same 0-10 scale as GitHub, so an ADO-hosted run is stamped too.
            SeedWip(
                (SarifEventKinds.RunHeader, AdoRunHeader()),
                (SarifEventKinds.Result, new Result { RuleId = "CWE-79/template-xss", Rank = 60, Message = new Message { Text = "xss" } }),
                (SarifEventKinds.Result, new Result { RuleId = "CWE-79/dom-xss", Rank = 85, Message = new Message { Text = "xss" } }));

            int exit = new EmitFinalizeCommand().Run(new EmitFinalizeOptions { OutputFilePath = OutPath });

            exit.Should().Be(CommandBase.SUCCESS);
            ReportingDescriptor rule = LoadSarif().Runs[0].Tool.Driver.Rules.Single(r => r.Id == "CWE-79");
            SecuritySeverityOf(rule).Should().Be("7.8");
        }

        [Fact]
        public void Run_PreservesProducerAuthoredSecuritySeverityOnAzureDevOpsHostedRun()
        {
            // A producer-authored value wins over the curated table prior, regardless of host.
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
            // even when the curated table carries a different prior for that CWE.
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
        public void ApplyAISecuritySeverity_StampsCuratedTableValue()
        {
            Run run = BuildRun("CWE-89");

            int stamped = EmitFinalizeCommand.ApplyAISecuritySeverity(run);

            stamped.Should().Be(1);
            SecuritySeverityOf(run.Tool.Driver.Rules[0]).Should().Be("8.8");
        }

        [Fact]
        public void ApplyAISecuritySeverity_StampsEachKnownCweRuleIndependently()
        {
            Run run = BuildRun("CWE-89", "CWE-79");

            int stamped = EmitFinalizeCommand.ApplyAISecuritySeverity(run);

            stamped.Should().Be(2);
            SecuritySeverityOf(run.Tool.Driver.Rules[0]).Should().Be("8.8");
            SecuritySeverityOf(run.Tool.Driver.Rules[1]).Should().Be("7.8");
        }

        [Fact]
        public void ApplyAISecuritySeverity_StampsMediumDefaultForUncuratedCweAndNovelRules()
        {
            // A CWE with no curated prior, and the NOVEL- escape hatch (which carries no CWE), are
            // both uncurated content: rather than ship with no severity, they get the neutral medium
            // emit-time default (5.0) so they bucket as security findings on GitHub/Azure DevOps.
            Run run = BuildRun("CWE-999999", "NOVEL-prompt-injection");

            int stamped = EmitFinalizeCommand.ApplyAISecuritySeverity(run);

            stamped.Should().Be(2);
            SecuritySeverityOf(run.Tool.Driver.Rules[0]).Should().Be("5.0");
            SecuritySeverityOf(run.Tool.Driver.Rules[1]).Should().Be("5.0");
        }

        [Fact]
        public void ApplyAISecuritySeverity_LeavesNonAiRuleBare()
        {
            // A rule id that is neither a CWE nor a NOVEL- id is not an AI security rule; it receives
            // no severity (and no default).
            Run run = BuildRun("MY-CUSTOM-RULE");

            int stamped = EmitFinalizeCommand.ApplyAISecuritySeverity(run);

            stamped.Should().Be(0);
            HasSecuritySeverity(run.Tool.Driver.Rules[0]).Should().BeFalse();
        }

        [Fact]
        public void ApplyAISecuritySeverity_PreservesProducerAuthoredValue()
        {
            Run run = BuildRun("CWE-89");
            run.Tool.Driver.Rules[0].SetProperty("security-severity", "2.0");

            int stamped = EmitFinalizeCommand.ApplyAISecuritySeverity(run);

            stamped.Should().Be(0);
            SecuritySeverityOf(run.Tool.Driver.Rules[0]).Should().Be("2.0");
        }

        [Fact]
        public void ApplyGitHubCweTags_StampsSecurityAndCweTags()
        {
            Run run = BuildRun("CWE-129");

            int stamped = EmitFinalizeCommand.ApplyGitHubCweTags(run);

            stamped.Should().Be(1);
            TagsOf(run.Tool.Driver.Rules[0]).Should().Equal("security", "external/cwe/cwe-129");
        }

        [Fact]
        public void ApplyGitHubCweTags_StampsEachKnownCweRuleIndependently()
        {
            Run run = BuildRun("CWE-89", "CWE-79");

            int stamped = EmitFinalizeCommand.ApplyGitHubCweTags(run);

            stamped.Should().Be(2);
            TagsOf(run.Tool.Driver.Rules[0]).Should().Equal("security", "external/cwe/cwe-89");
            TagsOf(run.Tool.Driver.Rules[1]).Should().Equal("security", "external/cwe/cwe-79");
        }

        [Fact]
        public void ApplyGitHubCweTags_DerivesCweNumberFromCweDescriptorId()
        {
            // A CWE-as-rule descriptor carries a base CWE id; the external/cwe tag tracks its number.
            Run run = BuildRun("CWE-89");

            EmitFinalizeCommand.ApplyGitHubCweTags(run);

            TagsOf(run.Tool.Driver.Rules[0]).Should().Equal("security", "external/cwe/cwe-89");
        }

        [Fact]
        public void ApplyGitHubCweTags_StampsBareSecurityTagForNovelRule()
        {
            // The NOVEL- escape hatch is a real security finding with no fitting CWE: it gets the
            // bare "security" tag (so GitHub classifies it as a security alert) but no external/cwe tag.
            Run run = BuildRun("NOVEL-prompt-injection");

            int stamped = EmitFinalizeCommand.ApplyGitHubCweTags(run);

            stamped.Should().Be(1);
            TagsOf(run.Tool.Driver.Rules[0]).Should().Equal("security");
        }

        [Fact]
        public void ApplyGitHubCweTags_ElidesNonAiRules()
        {
            // A rule id that is neither a CWE nor a NOVEL- id is not an AI security rule; it is not tagged.
            Run run = BuildRun("MY-CUSTOM-RULE");

            int stamped = EmitFinalizeCommand.ApplyGitHubCweTags(run);

            stamped.Should().Be(0);
            HasTags(run.Tool.Driver.Rules[0]).Should().BeFalse();
        }

        [Fact]
        public void ApplyGitHubCweTags_PreservesProducerAuthoredTags()
        {
            // A producer-authored tag survives; the two GitHub tags are merged in without dropping it.
            Run run = BuildRun("CWE-89");
            run.Tool.Driver.Rules[0].SetProperty("tags", new List<string> { "custom-taxonomy/foo" });

            int stamped = EmitFinalizeCommand.ApplyGitHubCweTags(run);

            stamped.Should().Be(1);
            TagsOf(run.Tool.Driver.Rules[0]).Should().Equal("custom-taxonomy/foo", "security", "external/cwe/cwe-89");
        }

        [Fact]
        public void ApplyGitHubCweTags_DoesNotDuplicateExistingTags()
        {
            // Re-running over an already-stamped rule adds nothing and reports no modification.
            Run run = BuildRun("CWE-89");
            EmitFinalizeCommand.ApplyGitHubCweTags(run);

            int stamped = EmitFinalizeCommand.ApplyGitHubCweTags(run);

            stamped.Should().Be(0);
            TagsOf(run.Tool.Driver.Rules[0]).Should().Equal("security", "external/cwe/cwe-89");
        }

        [Fact]
        public void Run_StampsGitHubCweTagsForGitHubHostedRun()
        {
            SeedWip(
                (SarifEventKinds.RunHeader, RunHeader()),
                (SarifEventKinds.Result, new Result { RuleId = "CWE-79/template-xss", Message = new Message { Text = "xss" } }));

            int exit = new EmitFinalizeCommand().Run(new EmitFinalizeOptions { OutputFilePath = OutPath });

            exit.Should().Be(CommandBase.SUCCESS);
            ReportingDescriptor rule = LoadSarif().Runs[0].Tool.Driver.Rules.Single(r => r.Id == "CWE-79");
            TagsOf(rule).Should().Equal("security", "external/cwe/cwe-79");
            SecuritySeverityOf(rule).Should().Be("7.8");
        }

        [Fact]
        public void Run_DoesNotStampGitHubCweTagsForAzureDevOpsHostedRun()
        {
            // The CWE tags are GitHub-only — GHAzDO does not require them. An ADO-hosted run still
            // gets the host-agnostic security-severity, but no tags.
            SeedWip(
                (SarifEventKinds.RunHeader, AdoRunHeader()),
                (SarifEventKinds.Result, new Result { RuleId = "CWE-79/template-xss", Message = new Message { Text = "xss" } }));

            int exit = new EmitFinalizeCommand().Run(new EmitFinalizeOptions { OutputFilePath = OutPath });

            exit.Should().Be(CommandBase.SUCCESS);
            ReportingDescriptor rule = LoadSarif().Runs[0].Tool.Driver.Rules.Single(r => r.Id == "CWE-79");
            HasTags(rule).Should().BeFalse();
            SecuritySeverityOf(rule).Should().Be("7.8");
        }

        [Fact]
        public void CollapseResultRuleSubIds_CollapsesSubIdToDescriptorId()
        {
            Run run = BuildRunWithResults(("CWE-79", "CWE-79/template-xss"));
            run.Results[0].RuleId.Should().Be("CWE-79/template-xss");

            int collapsed = EmitFinalizeCommand.CollapseResultRuleSubIds(run);

            collapsed.Should().Be(1);
            run.Results[0].RuleId.Should().Be("CWE-79");
        }

        [Fact]
        public void CollapseResultRuleSubIds_CollapsesMultipleSubRulesOfSameRule()
        {
            // Two distinct sub-ids of the same CWE both bind to the single "CWE-79" descriptor and
            // both collapse to it.
            Run run = BuildRunWithResults(
                ("CWE-79", "CWE-79/template-xss"),
                ("CWE-79", "CWE-79/dom-xss-via-sanitizer-bypass"));
            run.Results[0].RuleId.Should().Be("CWE-79/template-xss");
            run.Results[1].RuleId.Should().Be("CWE-79/dom-xss-via-sanitizer-bypass");

            int collapsed = EmitFinalizeCommand.CollapseResultRuleSubIds(run);

            collapsed.Should().Be(2);
            run.Results[0].RuleId.Should().Be("CWE-79");
            run.Results[1].RuleId.Should().Be("CWE-79");
        }

        [Fact]
        public void CollapseResultRuleSubIds_LeavesFlatRuleIdUnchanged()
        {
            // A result whose ruleId already equals its descriptor id (no sub-id) is left as-is and
            // is not counted as collapsed.
            Run run = BuildRunWithResults(("CWE-79", "CWE-79"));

            int collapsed = EmitFinalizeCommand.CollapseResultRuleSubIds(run);

            collapsed.Should().Be(0);
            run.Results[0].RuleId.Should().Be("CWE-79");
        }

        [Fact]
        public void CollapseResultRuleSubIds_LeavesNovelRuleIdUnchanged()
        {
            // NOVEL- ids are flat (no slash); there is no sub-id to collapse.
            Run run = BuildRunWithResults(("NOVEL-prompt-injection", "NOVEL-prompt-injection"));

            int collapsed = EmitFinalizeCommand.CollapseResultRuleSubIds(run);

            collapsed.Should().Be(0);
            run.Results[0].RuleId.Should().Be("NOVEL-prompt-injection");
        }

        [Fact]
        public void CollapseResultRuleSubIds_KeepsRuleIdAndRuleDotIdConsistent()
        {
            // When a result also carries a rule reference whose id is the sub-id form, both the
            // ruleId and the rule.id collapse together so they remain equal (valid SARIF §3.27.7).
            Run run = BuildRunWithResults(("CWE-79", "CWE-79/template-xss"));
            run.Results[0].Rule = new ReportingDescriptorReference { Id = "CWE-79/template-xss", Index = 0 };
            run.Results[0].RuleId.Should().Be("CWE-79/template-xss");
            run.Results[0].Rule.Id.Should().Be("CWE-79/template-xss");

            int collapsed = EmitFinalizeCommand.CollapseResultRuleSubIds(run);

            collapsed.Should().Be(1);
            run.Results[0].RuleId.Should().Be("CWE-79");
            run.Results[0].Rule.Id.Should().Be("CWE-79");
        }

        [Fact]
        public void Run_CollapsesResultRuleSubIdForGitHubHostedRun()
        {
            SeedWip(
                (SarifEventKinds.RunHeader, RunHeader()),
                (SarifEventKinds.Result, new Result { RuleId = "CWE-79/template-xss", Message = new Message { Text = "xss" } }));

            int exit = new EmitFinalizeCommand().Run(new EmitFinalizeOptions { OutputFilePath = OutPath });

            exit.Should().Be(CommandBase.SUCCESS);
            LoadSarif().Runs[0].Results[0].RuleId.Should().Be("CWE-79");
        }

        [Fact]
        public void Run_KeepsResultRuleSubIdForAzureDevOpsHostedRun()
        {
            // The collapse is a GitHub code-scanning compatibility shim; Azure DevOps resolves the
            // rule through ruleIndex correctly, so its results keep the legal sub-id form.
            SeedWip(
                (SarifEventKinds.RunHeader, AdoRunHeader()),
                (SarifEventKinds.Result, new Result { RuleId = "CWE-79/template-xss", Message = new Message { Text = "xss" } }));

            int exit = new EmitFinalizeCommand().Run(new EmitFinalizeOptions { OutputFilePath = OutPath });

            exit.Should().Be(CommandBase.SUCCESS);
            LoadSarif().Runs[0].Results[0].RuleId.Should().Be("CWE-79/template-xss");
        }

        private static Run BuildRun(params string[] ruleIds)
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
            };

        // Builds a run whose descriptor table is the distinct set of descriptor ids across the
        // supplied (descriptorId, ruleId) pairs, with each result's RuleIndex pointing at its
        // descriptor — mirroring the binding SarifEventReplayer produces from result ruleIds.
        private static Run BuildRunWithResults(params (string descriptorId, string ruleId)[] pairs)
        {
            var rules = new List<ReportingDescriptor>();
            var idToIndex = new Dictionary<string, int>(StringComparer.Ordinal);
            var results = new List<Result>();

            foreach ((string descriptorId, string ruleId) in pairs)
            {
                if (!idToIndex.TryGetValue(descriptorId, out int index))
                {
                    index = rules.Count;
                    rules.Add(new ReportingDescriptor { Id = descriptorId });
                    idToIndex[descriptorId] = index;
                }

                results.Add(new Result
                {
                    RuleId = ruleId,
                    RuleIndex = index,
                    Message = new Message { Text = "finding" },
                });
            }

            return new Run
            {
                Tool = new Tool { Driver = new ToolComponent { Name = "demo", Rules = rules } },
                Results = results,
            };
        }

        private static string SecuritySeverityOf(ReportingDescriptor descriptor)
            => descriptor.TryGetProperty("security-severity", out string value) ? value : null;

        private static bool HasSecuritySeverity(ReportingDescriptor descriptor)
            => descriptor.PropertyNames.Contains("security-severity");

        private static List<string> TagsOf(ReportingDescriptor descriptor)
            => descriptor.TryGetProperty("tags", out List<string> value) ? value : null;

        private static bool HasTags(ReportingDescriptor descriptor)
            => descriptor.PropertyNames.Contains("tags");
    }
}
