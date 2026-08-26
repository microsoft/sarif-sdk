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
using Newtonsoft.Json.Linq;

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

        private static void SeedWipAt(string path, Run header, params Result[] results)
        {
            using var w = new SarifEventLogWriter(path);
            w.Append(SarifEventKinds.RunHeader, header);
            foreach (Result result in results) { w.Append(SarifEventKinds.Result, result); }
        }

        private static Run OriginRunHeader(string origin)
        {
            Run run = RunHeader();
            run.SetProperty("ai/origin", origin);
            return run;
        }

        private static string GetOrigin(Run run)
            => run.TryGetProperty("ai/origin", out string origin) ? origin : null;

        private static Result NovelResult()
            => new Result { RuleId = "NOVEL-x", Message = new Message { Text = "x" } };

        private static Location PrimaryWithRelationship(string kind)
            => new Location
            {
                Id = 0,
                Relationships = new[] { new LocationRelationship { Target = 1, Kinds = new[] { kind } } },
            };

        private static Location SarifPointerLocation(string sarifUri)
            => new Location
            {
                Id = 1,
                PhysicalLocation = new PhysicalLocation
                {
                    ArtifactLocation = new ArtifactLocation { Uri = new Uri(sarifUri, UriKind.Absolute) },
                },
            };

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
        public void Run_WithNoCweEnrichment_SkipsTaxonomyProseButStillNamesWeakness()
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
            // --no-cwe-enrichment suppresses the MITRE prose (public data AdvSec already holds)...
            descriptor.HelpUri.Should().BeNull();
            descriptor.ShortDescription.Should().BeNull();
            descriptor.FullDescription.Should().BeNull();
            descriptor.Help.Should().BeNull();
            // ...but CWE-79 is a genuine Weakness, so name — cheap, and required for a spec-valid,
            // GHAzDO-publishable descriptor — is still resolved from the taxonomy unconditionally.
            descriptor.Name.Should().Be("CrossSiteScripting");
        }

        [Fact]
        public void Run_WithNoCweEnrichment_LeavesProducerSuppliedNovelDescriptorFullyIntact()
        {
            // --no-cwe-enrichment must be scoped to CWE-as-rule-id descriptors only. A NOVEL- id is
            // never touched by CweTaxonomyEnricher (nothing public describes it, so it's not in the
            // embedded taxonomy) and EnsureCweRuleDescriptorNames skips it too (IsKnownWeakness is
            // false for a NOVEL- id). Assert every producer-authored field -- not just Name -- survives
            // byte-for-byte with the flag set, proving a non-CWE tool's output is completely unaffected.
            const string novelId = "NOVEL-prompt-injection-via-system-message";
            var novelDescriptor = new ReportingDescriptor
            {
                Id = novelId,
                Name = "PromptInjectionViaSystemMessage",
                HelpUri = new Uri("https://example.com/rules/novel-prompt-injection", UriKind.Absolute),
                ShortDescription = new MultiformatMessageString { Text = "Untrusted content reaches a system-role prompt at runtime." },
                FullDescription = new MultiformatMessageString { Text = "Untrusted content reaches a system-role prompt at runtime; full prose." },
                Help = new MultiformatMessageString { Text = "Help text.", Markdown = "## Help" },
            };

            SeedWip(
                (SarifEventKinds.RunHeader, RunHeader()),
                (SarifEventKinds.RuleDescriptor, novelDescriptor),
                (SarifEventKinds.Result, new Result { RuleId = novelId, Message = new Message { Text = "prompt injection" } }));

            int exit = new EmitFinalizeCommand().Run(new EmitFinalizeOptions
            {
                OutputFilePath = OutPath,
                NoCweEnrichment = true,
            });

            exit.Should().Be(CommandBase.SUCCESS);
            ReportingDescriptor descriptor = LoadSarif().Runs[0].Tool.Driver.Rules.Single(r => r.Id == novelId);
            descriptor.Name.Should().Be("PromptInjectionViaSystemMessage");
            descriptor.HelpUri.Should().Be(novelDescriptor.HelpUri);
            descriptor.ShortDescription.Text.Should().Be(novelDescriptor.ShortDescription.Text);
            descriptor.FullDescription.Text.Should().Be(novelDescriptor.FullDescription.Text);
            descriptor.Help.Text.Should().Be(novelDescriptor.Help.Text);
            descriptor.Help.Markdown.Should().Be(novelDescriptor.Help.Markdown);
        }

        [Fact]
        public void Run_WithNoCweEnrichment_ValidatorConfirmsWeaknessPassesGHAzDO2012ButCategoryFails()
        {
            // The point of the fix is validator-observable: a Weakness (CWE-79) must actually pass
            // GHAzDO2012 (name required) once finalize runs, while a Category (CWE-16) must actually
            // still fail it. Asserting the Name property directly checks our own arithmetic; running
            // the real validator over the finalized output checks the thing GHAzDO ingestion checks.
            SeedWip(
                (SarifEventKinds.RunHeader, RunHeader()),
                (SarifEventKinds.Result, new Result { RuleId = "CWE-79/template-xss", Message = new Message { Text = "xss" } }),
                (SarifEventKinds.Result, new Result { RuleId = "CWE-16/insecure-default-config", Message = new Message { Text = "config" } }));

            int exit = new EmitFinalizeCommand().Run(new EmitFinalizeOptions
            {
                OutputFilePath = OutPath,
                NoCweEnrichment = true,
            });
            exit.Should().Be(CommandBase.SUCCESS);

            IList<ReportingDescriptor> finalizedRules = LoadSarif().Runs[0].Tool.Driver.Rules;
            int xssRuleIndex = finalizedRules.ToList().FindIndex(r => r.Id == "CWE-79");
            int categoryRuleIndex = finalizedRules.ToList().FindIndex(r => r.Id == "CWE-16");

            SarifLog validationReport = RunGHAzDOValidator(OutPath);
            List<Result> ghazdo2012Results = validationReport.Runs[0].Results
                .Where(r => r.RuleId == "GHAzDO2012")
                .ToList();

            ghazdo2012Results.Should().Contain(
                r => TargetsRuleAtIndex(r, categoryRuleIndex),
                "the nameless Category descriptor must still fail GHAzDO2012");
            ghazdo2012Results.Should().NotContain(
                r => TargetsRuleAtIndex(r, xssRuleIndex),
                "the named Weakness descriptor must pass GHAzDO2012 now that name is resolved");
        }

        private static SarifLog RunGHAzDOValidator(string targetPath)
        {
            string reportPath = targetPath + ".ghazdo-validate-report.sarif";
            try
            {
                var options = new ValidateOptions
                {
                    TargetFileSpecifiers = new[] { targetPath },
                    OutputFilePath = reportPath,
                    OutputFileOptions = new[] { FilePersistenceOptions.ForceOverwrite },
                    RuleKindOption = new List<RuleKind> { RuleKind.GHAzDO },
                    Kind = new List<ResultKind> { ResultKind.Fail },
                    Level = new List<FailureLevel> { FailureLevel.Note, FailureLevel.Warning, FailureLevel.Error },
                };

                var context = new SarifValidationContext { FileSystem = FileSystem.Instance };
                new ValidateCommand().Run(options, ref context);

                return SarifLog.Load(reportPath);
            }
            finally
            {
                if (File.Exists(reportPath)) { File.Delete(reportPath); }
            }
        }

        [Fact]
        public void Run_WithNoCweEnrichment_RealValidateFlagConfirmsWeaknessClearsSarif1001IdentityCollision()
        {
            // SARIF1001 (id/name collision forbidden, spec 3.49.7) is the other rule the design doc
            // calls out alongside GHAzDO2012. Unlike GHAzDO2012, emit-finalize's own --validate flag
            // already runs Sarif+AI rule kinds in production (RunValidatorAndReport), so this test
            // drives that real flag/report instead of a hand-rolled validator invocation -- it proves
            // the shipped --validate path itself, not just a re-implementation of it.
            //
            // Note: --validate's default FailureLevels filter is Error+Warning (BaseLogger.ErrorWarning),
            // so Note-level Sarif rules are not emitted on this path.
            SeedWip(
                (SarifEventKinds.RunHeader, RunHeader()),
                (SarifEventKinds.Result, new Result { RuleId = "CWE-79/template-xss", Message = new Message { Text = "xss" } }),
                (SarifEventKinds.Result, new Result { RuleId = "CWE-16/insecure-default-config", Message = new Message { Text = "config" } }));

            string reportPath = Path.Combine(
                Path.GetDirectoryName(OutPath) ?? string.Empty,
                Path.GetFileNameWithoutExtension(OutPath) + ".validate-report.sarif");

            try
            {
                // Note: the exit code here reflects the run's overall AI1005/AI1006/AI1016 "no
                // security-severity" findings baked into this minimal fixture -- unrelated to the
                // name-resolution fix -- so it isn't asserted; the report contents are what matter.
                new EmitFinalizeCommand().Run(new EmitFinalizeOptions
                {
                    OutputFilePath = OutPath,
                    NoCweEnrichment = true,
                    Validate = true,
                });

                IList<ReportingDescriptor> finalizedRules = LoadSarif().Runs[0].Tool.Driver.Rules;
                int xssRuleIndex = finalizedRules.ToList().FindIndex(r => r.Id == "CWE-79");

                File.Exists(reportPath).Should().BeTrue("--validate must persist a validate-report.sarif");
                SarifLog validationReport = SarifLog.Load(reportPath);
                IList<Result> results = validationReport.Runs[0].Results ?? new List<Result>();

                results.Where(r => r.RuleId == "SARIF1001")
                    .Should().NotContain(r => TargetsRuleAtIndex(r, xssRuleIndex),
                        "CWE-79's resolved name ('CrossSiteScripting') differs from its id ('CWE-79'), so SARIF1001 does not fire");
            }
            finally
            {
                if (File.Exists(reportPath)) { File.Delete(reportPath); }
            }
        }

        // The validator's GHAzDO2012 result carries no ruleId/ruleIndex of its own (it's a
        // reportingDescriptor-level finding on the *target* log, not a result-level one). Its
        // message is built from a format string plus positional Arguments, the first of which is
        // the JSON-pointer-derived path to the offending descriptor, e.g.
        // "runs[0].tool.driver.rules[<index>]" — match on that argument, which is populated even
        // when Message.Text itself is left for lazy resource-string formatting.
        private static bool TargetsRuleAtIndex(Result result, int ruleIndex)
            => result.Message?.Arguments?.Any(a => a == $"runs[0].tool.driver.rules[{ruleIndex}]") == true;

        [Fact]
        public void Run_WithNoCweEnrichment_HandlesWeaknessCategoryAndNovelRulesTogether()
        {
            // End-to-end coverage of the three rule shapes emit-finalize --no-cwe-enrichment must
            // treat differently in a single run: a Weakness (named from the taxonomy), a Category
            // (left nameless on purpose so it fails loudly), and a NOVEL- id (producer-owned, untouched).
            SeedWip(
                (SarifEventKinds.RunHeader, RunHeader()),
                (SarifEventKinds.Result, new Result { RuleId = "CWE-79/template-xss", Message = new Message { Text = "xss" } }),
                (SarifEventKinds.Result, new Result { RuleId = "CWE-89/string-concat-query", Message = new Message { Text = "sqli" } }),
                (SarifEventKinds.Result, new Result { RuleId = "CWE-16/insecure-default-config", Message = new Message { Text = "config" } }),
                (SarifEventKinds.Result, new Result { RuleId = "NOVEL-prompt-injection", Message = new Message { Text = "prompt" } }));

            int exit = new EmitFinalizeCommand().Run(new EmitFinalizeOptions
            {
                OutputFilePath = OutPath,
                NoCweEnrichment = true,
            });

            exit.Should().Be(CommandBase.SUCCESS);
            IList<ReportingDescriptor> rules = LoadSarif().Runs[0].Tool.Driver.Rules;

            ReportingDescriptor xss = rules.Single(r => r.Id == "CWE-79");
            xss.Name.Should().Be("CrossSiteScripting");
            xss.HelpUri.Should().BeNull();
            xss.ShortDescription.Should().BeNull();

            ReportingDescriptor sqli = rules.Single(r => r.Id == "CWE-89");
            sqli.Name.Should().Be("SqlInjection");
            sqli.HelpUri.Should().BeNull();

            ReportingDescriptor category = rules.Single(r => r.Id == "CWE-16");
            category.Name.Should().BeNull();
            category.HelpUri.Should().BeNull();

            ReportingDescriptor novel = rules.Single(r => r.Id == "NOVEL-prompt-injection");
            novel.Name.Should().BeNull();
            novel.HelpUri.Should().BeNull();
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
        public void Run_WithInputs_AssemblesOrderedMultiRunLog_AndDeletesEveryInput()
        {
            // The generated tier (runs[0]) and synthesized tier (runs[1]) are staged separately,
            // each with a cross-run sarif: pointer at the other. --inputs replays them in order
            // into one multi-run log; the pointers are index-pinned, so the order must be exactly
            // as listed and the pointers must survive finalize unchanged.
            string genWip = Path.Combine(_dir, "0_generated.wip.jsonl");
            string synWip = Path.Combine(_dir, "1_synthesized.wip.jsonl");

            SeedWipAt(genWip, OriginRunHeader("generated"),
                new Result
                {
                    RuleId = "CWE-306/missing-auth-check",
                    Message = new Message { Text = "missing auth" },
                    Locations = new[] { PrimaryWithRelationship("isIncludedBy") },
                    RelatedLocations = new[] { SarifPointerLocation("sarif:/runs/1/results/0") },
                });

            SeedWipAt(synWip, OriginRunHeader("synthesized"),
                new Result
                {
                    RuleId = "CWE-918/ssrf-via-unvalidated-fetch",
                    Message = new Message { Text = "ssrf cluster" },
                    Locations = new[] { PrimaryWithRelationship("includes") },
                    RelatedLocations = new[] { SarifPointerLocation("sarif:/runs/0/results/0") },
                });

            int exit = new EmitFinalizeCommand().Run(new EmitFinalizeOptions
            {
                OutputFilePath = OutPath,
                Inputs = new[] { genWip, synWip },
            });

            exit.Should().Be(CommandBase.SUCCESS);
            File.Exists(genWip).Should().BeFalse("every consumed input wip is cleaned up");
            File.Exists(synWip).Should().BeFalse("every consumed input wip is cleaned up");

            SarifLog log = LoadSarif();
            log.Runs.Should().HaveCount(2);
            GetOrigin(log.Runs[0]).Should().Be("generated");
            GetOrigin(log.Runs[1]).Should().Be("synthesized");

            log.Runs[1].Results[0].RelatedLocations[0].PhysicalLocation.ArtifactLocation.Uri
                .OriginalString.Should().Be("sarif:/runs/0/results/0");
            log.Runs[0].Results[0].RelatedLocations[0].PhysicalLocation.ArtifactLocation.Uri
                .OriginalString.Should().Be("sarif:/runs/1/results/0");
        }

        [Fact]
        public void Run_WithInputs_PreservesCallerOrder_WhenReversed()
        {
            // Order is the property merge lacks: listing the synthesized tier first must place it
            // at runs[0]. This is what makes cross-run sarif: pointers safe under emit-finalize.
            string genWip = Path.Combine(_dir, "gen.wip.jsonl");
            string synWip = Path.Combine(_dir, "syn.wip.jsonl");

            SeedWipAt(genWip, OriginRunHeader("generated"), NovelResult());
            SeedWipAt(synWip, OriginRunHeader("synthesized"), NovelResult());

            int exit = new EmitFinalizeCommand().Run(new EmitFinalizeOptions
            {
                OutputFilePath = OutPath,
                Inputs = new[] { synWip, genWip },
            });

            exit.Should().Be(CommandBase.SUCCESS);

            SarifLog log = LoadSarif();
            log.Runs.Should().HaveCount(2);
            GetOrigin(log.Runs[0]).Should().Be("synthesized");
            GetOrigin(log.Runs[1]).Should().Be("generated");
        }

        [Fact]
        public void Run_WithInputs_MissingInput_FailsWithoutWritingOutput_AndLeavesExistingInput()
        {
            // Inputs are validated to exist before any is replayed, so a missing input fails clean:
            // no output ships and the present input is not consumed.
            string present = Path.Combine(_dir, "present.wip.jsonl");
            string missing = Path.Combine(_dir, "missing.wip.jsonl");

            SeedWipAt(present, OriginRunHeader("generated"), NovelResult());

            int exit = new EmitFinalizeCommand().Run(new EmitFinalizeOptions
            {
                OutputFilePath = OutPath,
                Inputs = new[] { present, missing },
            });

            exit.Should().Be(CommandBase.FAILURE);
            File.Exists(OutPath).Should().BeFalse();
            File.Exists(present).Should().BeTrue("no input is consumed when another input is missing");
        }

        [Fact]
        public void Run_WithSingleInput_BehavesLikePositionalForm()
        {
            // A single --inputs entry is the multi-run path with N=1: it must produce the same
            // single-run result the positional '<output>.wip.jsonl' form does.
            string wip = Path.Combine(_dir, "only.wip.jsonl");
            SeedWipAt(wip, OriginRunHeader("generated"),
                new Result { RuleId = "CWE-79/template-xss", Message = new Message { Text = "xss" } });

            int exit = new EmitFinalizeCommand().Run(new EmitFinalizeOptions
            {
                OutputFilePath = OutPath,
                Inputs = new[] { wip },
            });

            exit.Should().Be(CommandBase.SUCCESS);
            File.Exists(wip).Should().BeFalse();

            SarifLog log = LoadSarif();
            log.Runs.Should().HaveCount(1);
            log.Runs[0].Tool.Driver.Rules[0].Id.Should().Be("CWE-79");
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
        public void Run_WithValidateFlag_OnFailure_WritesStructuredReceiptToStdoutAndHumanSummaryToStderr()
        {
            // Converged channel discipline: stdout carries a structured JSON receipt (the verdict
            // and the full, uncapped error set) — the machine-readable twin of the emit batch verbs'
            // { appended, rejected }; stderr carries the concise human summary (count header, capped
            // per-error detail, report pointer) — the channel a CI log reliably captures; the full
            // structured report persists to disk.
            SeedWip(
                (SarifEventKinds.RunHeader, RunHeader()));

            (int exit, string stdout, string stderr) = RunCapturingBothStreams(new EmitFinalizeOptions
            {
                OutputFilePath = OutPath,
                Validate = true,
            });

            exit.Should().Be(CommandBase.FAILURE);
            File.Exists(Path.Combine(_dir, "scan.validate-report.sarif")).Should().BeTrue();

            // stderr: the human summary.
            stderr.Should().Contain("does not conform");
            stderr.Should().Contain("[Sarif+AI]");
            stderr.Should().Contain("Full report:");
            stderr.Should().Contain("\n  ", "each Error-level finding is rendered as an indented detail line");

            // stdout: the structured receipt — parseable, carrying the verdict and full error set.
            JObject receipt = JObject.Parse(stdout.Substring(stdout.IndexOf('{')));
            receipt.Value<bool>("conforms").Should().BeFalse();
            receipt.Value<string>("profile").Should().Be("Sarif;AI");
            receipt.Value<int>("errorCount").Should().BeGreaterThan(0);
            receipt.Value<string>("reportPath").Should().Contain("scan.validate-report.sarif");
            ((JArray)receipt["errors"]).Count.Should().Be(receipt.Value<int>("errorCount"),
                "the stdout receipt carries the full, uncapped error set");

            // The human prose never leaks onto stdout.
            stdout.Should().NotContain("does not conform");
            stdout.Should().NotContain("Full report:");
            stdout.Should().NotContain("[Sarif+AI]", "the bracketed human header stays on stderr");
        }

        private static (int exit, string stdout, string stderr) RunCapturingBothStreams(EmitFinalizeOptions options)
        {
            using var outWriter = new StringWriter();
            using var errWriter = new StringWriter();
            TextWriter originalOut = Console.Out;
            TextWriter originalError = Console.Error;
            Console.SetOut(outWriter);
            Console.SetError(errWriter);
            try
            {
                int exit = new EmitFinalizeCommand().Run(options);
                return (exit, outWriter.ToString(), errWriter.ToString());
            }
            finally
            {
                Console.SetOut(originalOut);
                Console.SetError(originalError);
            }
        }

        [Fact]
        public void Run_WithNoRepoAndValidate_DoesNotFaultRepolessRunForMissingVersionControl()
        {
            // Regression for #3100: --no-repo deliberately omits versionControlProvenance and stamps
            // the run unpublishable. The --validate gate must not fault that run with AI1004 (the rule
            // skips the unpublishable marker), even though other AI-profile findings may still fire.
            SeedWip(
                (SarifEventKinds.RunHeader, RepoLessRunHeader()),
                (SarifEventKinds.Result, NovelResult()));

            new EmitFinalizeCommand().Run(new EmitFinalizeOptions
            {
                OutputFilePath = OutPath,
                NoRepo = true,
                Validate = true,
            });

            string reportPath = Path.Combine(_dir, "scan.validate-report.sarif");
            File.Exists(reportPath).Should().BeTrue();

            using var sr = new StreamReader(reportPath);
            using var jr = new JsonTextReader(sr);
            SarifLog report = JsonSerializer.CreateDefault().Deserialize<SarifLog>(jr);

            report.Runs
                .SelectMany(r => r.Results ?? new List<Result>())
                .Where(res => res.RuleId == "AI1004")
                .Should().BeEmpty("--no-repo stamps the run unpublishable, exempting it from AI1004");
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
        public void EnsureCweRuleDescriptorNames_ResolvesNameFromTaxonomy()
        {
            // CWE-89 is a genuine Weakness; under --no-cwe-enrichment the replayer-created descriptor
            // reaches finalize nameless, so this resolves its real MITRE title from the embedded
            // taxonomy — cheap (~20 bytes) and spec-valid — rather than flooring to the bare id.
            Run run = BuildRun("CWE-89");

            int modified = EmitFinalizeCommand.EnsureCweRuleDescriptorNames(run);

            modified.Should().Be(1);
            run.Tool.Driver.Rules[0].Name.Should().Be("SqlInjection");
        }

        [Fact]
        public void EnsureCweRuleDescriptorNames_LeavesCategoryDescriptorNameless()
        {
            // CWE-16 is a MITRE Category, not a Weakness, so mapping a result to it is a producer
            // bug. This deliberately leaves it nameless so it fails loudly (AI1016 at validate,
            // GHAzDO2012 at ingestion) instead of being normalized into a publishable-looking descriptor.
            Run run = BuildRun("CWE-16");

            int modified = EmitFinalizeCommand.EnsureCweRuleDescriptorNames(run);

            modified.Should().Be(0);
            run.Tool.Driver.Rules[0].Name.Should().BeNull();
        }

        [Fact]
        public void EnsureCweRuleDescriptorNames_ResolvesEachUnnamedWeaknessIndependently()
        {
            Run run = BuildRun("CWE-79", "CWE-89");

            int modified = EmitFinalizeCommand.EnsureCweRuleDescriptorNames(run);

            modified.Should().Be(2);
            run.Tool.Driver.Rules[0].Name.Should().Be("CrossSiteScripting");
            run.Tool.Driver.Rules[1].Name.Should().Be("SqlInjection");
        }

        [Fact]
        public void EnsureCweRuleDescriptorNames_LeavesEnrichedNameUntouched()
        {
            Run run = BuildRun("CWE-79");
            run.Tool.Driver.Rules[0].Name = "Cross-site Scripting";

            int modified = EmitFinalizeCommand.EnsureCweRuleDescriptorNames(run);

            modified.Should().Be(0);
            run.Tool.Driver.Rules[0].Name.Should().Be("Cross-site Scripting");
        }

        [Fact]
        public void EnsureCweRuleDescriptorNames_LeavesNovelAndNonCweDescriptorsAlone()
        {
            // This is the GHAzDO publishability guarantee for the CWE Weakness descriptors the SDK
            // injects; a NOVEL- id and an arbitrary rule id are producer-owned and out of scope.
            Run run = BuildRun("NOVEL-prompt-injection", "MY-CUSTOM-RULE");

            int modified = EmitFinalizeCommand.EnsureCweRuleDescriptorNames(run);

            modified.Should().Be(0);
            run.Tool.Driver.Rules[0].Name.Should().BeNull();
            run.Tool.Driver.Rules[1].Name.Should().BeNull();
        }

        [Fact]
        public void Run_LeavesCategoryCweDescriptorNamelessSoItFailsLoudly()
        {
            // End-to-end: a result mapped to a Category CWE sub-id (CWE-16, not a Weakness) is a
            // producer mapping bug. emit-finalize leaves the descriptor nameless on purpose so it
            // fails GHAzDO2012 at ingestion rather than being normalized into publishable-looking output.
            SeedWip(
                (SarifEventKinds.RunHeader, RunHeader()),
                (SarifEventKinds.Result, new Result { RuleId = "CWE-16/insecure-default-config", Message = new Message { Text = "config" } }));

            int exit = new EmitFinalizeCommand().Run(new EmitFinalizeOptions { OutputFilePath = OutPath });

            exit.Should().Be(CommandBase.SUCCESS);
            ReportingDescriptor rule = LoadSarif().Runs[0].Tool.Driver.Rules.Single(r => r.Id == "CWE-16");
            rule.Name.Should().BeNull();
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
