// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif.Multitool.Rules;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class AIValidationRuleTests
    {
        #region Test SARIF Generators

        private static SarifLog CreateValidAISarifLog()
        {
            return new SarifLog
            {
                Runs = new[]
                {
                    new Run
                    {
                        Tool = new Tool
                        {
                            Driver = new ToolComponent
                            {
                                Name = "TestAIScanner",
                                Version = "1.0.0",
                                SemanticVersion = "1.0.0",
                                Rules = new[]
                                {
                                    new ReportingDescriptor
                                    {
                                        Id = "CWE-78/api-handler",
                                        Name = "CommandInjection",
                                        ShortDescription = new MultiformatMessageString { Text = "Command injection via unsanitized parameter" }
                                    }
                                }
                            }
                        },
                        AutomationDetails = new RunAutomationDetails
                        {
                            Guid = System.Guid.Parse("d4e7f8a0-1234-5678-abcd-ef0123456789")
                        },
                        VersionControlProvenance = new[]
                        {
                            new VersionControlDetails
                            {
                                RepositoryUri = new System.Uri("https://github.com/example/project"),
                                RevisionId = "abc123def456",
                                Branch = "main"
                            }
                        },
                        Results = new[]
                        {
                            new Result
                            {
                                RuleId = "CWE-78/api-handler",
                                Kind = ResultKind.Fail,
                                Level = FailureLevel.Error,
                                Rank = 92.5,
                                Message = new Message
                                {
                                    Text = "The 'command' parameter flows unsanitized to subprocess.run().",
                                    Markdown = "## Command Injection\n\n### What's Wrong\nUnsanitized input."
                                },
                                Locations = new[]
                                {
                                    new Location
                                    {
                                        PhysicalLocation = new PhysicalLocation
                                        {
                                            ArtifactLocation = new ArtifactLocation { Uri = new System.Uri("src/handler.py", System.UriKind.Relative) },
                                            Region = new Region
                                            {
                                                StartLine = 42,
                                                StartColumn = 5,
                                                EndLine = 42,
                                                EndColumn = 55,
                                                Snippet = new ArtifactContent { Text = "    subprocess.run(command, shell=True)" }
                                            },
                                            ContextRegion = new Region
                                            {
                                                StartLine = 40,
                                                EndLine = 44,
                                                Snippet = new ArtifactContent { Text = "def execute_job(request):\n    command = request.json['command']\n    subprocess.run(command, shell=True)\n    return {'status': 'ok'}\n" }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        private static void SetAIOrigin(SarifLog log, string value)
        {
            log.Runs[0].SetProperty("ai/origin", value);
        }

        private static void SetAiHandoff(SarifLog log, string value)
        {
            log.Runs[0].SetProperty("ai/handoff", value);
        }

        private static void SetExploitability(SarifLog log, string value)
        {
            log.Runs[0].Results[0].SetProperty("ai/exploitability", value);
        }

        private static void SetAttackerPosition(SarifLog log, string value)
        {
            log.Runs[0].Results[0].SetProperty("ai/attackerPosition", value);
        }

        private static void SetRepositoryHostToAzureDevOps(SarifLog log)
        {
            log.Runs[0].VersionControlProvenance[0].RepositoryUri =
                new System.Uri("https://dev.azure.com/example-org/example-project/_git/sarif-sdk");
        }

        #endregion

        #region Helper

        private static SarifLog RunAIValidation(SarifLog inputLog, string configFilePath = null)
        {
            string inputPath = Path.GetTempFileName() + ".sarif";
            string outputPath = Path.GetTempFileName() + ".sarif";

            try
            {
                inputLog.Save(inputPath);

                var options = new ValidateOptions
                {
                    TargetFileSpecifiers = new string[] { inputPath },
                    OutputFilePath = outputPath,
                    OutputFileOptions = new[] { FilePersistenceOptions.ForceOverwrite },
                    RuleKindOption = new List<RuleKind> { RuleKind.AI },
                    Kind = new List<ResultKind> { ResultKind.Fail },
                    Level = new List<FailureLevel> { FailureLevel.Note, FailureLevel.Warning, FailureLevel.Error },
                    ConfigurationFilePath = configFilePath
                };

                var context = new SarifValidationContext { FileSystem = FileSystem.Instance };
                new ValidateCommand().Run(options, ref context);

                return SarifLog.Load(outputPath);
            }
            finally
            {
                if (File.Exists(inputPath))
                {
                    File.Delete(inputPath);
                }

                if (File.Exists(outputPath))
                {
                    File.Delete(outputPath);
                }
            }
        }

        private static List<Result> GetResultsForRule(SarifLog output, string ruleId)
        {
            return output.Runs[0].Results
                .Where(r => r.RuleId == ruleId)
                .ToList();
        }

        #endregion

        #region AI1012 — ProvideRuleSubId

        private static void SetRuleIdShape(SarifLog log, string descriptorId, string resultRuleId)
        {
            log.Runs[0].Tool.Driver.Rules[0].Id = descriptorId;
            log.Runs[0].Results[0].RuleId = resultRuleId;
        }

        [Fact]
        public void AI1012_WhenRuleIdIsBareTaxonomyBaseId_OnNonGitHubHostedRun_ReportsError()
        {
            SarifLog log = CreateValidAISarifLog();
            SetAIOrigin(log, "generated");
            SetExploitability(log, "demonstrated");
            // A non-GitHub host keeps the sub-id requirement in force.
            SetRepositoryHostToAzureDevOps(log);
            // Bare taxonomy base id on both descriptor and result — no sub-id.
            SetRuleIdShape(log, descriptorId: "CWE-78", resultRuleId: "CWE-78");

            SarifLog output = RunAIValidation(log);
            List<Result> results = GetResultsForRule(output, "AI1012");

            results.Should().NotBeEmpty("a bare taxonomy ruleId (e.g. 'CWE-78') has no sub-id classifier");
            results[0].Level.Should().Be(FailureLevel.Error);
            results[0].Message.Id.Should().Be("Error_Missing", "a bare CWE base id is repairable by appending a sub-id");
        }

        [Fact]
        public void AI1012_WhenRuleIdIsBareTaxonomyBaseId_OnGitHubHostedRun_NoResult()
        {
            SarifLog log = CreateValidAISarifLog();
            SetAIOrigin(log, "generated");
            SetExploitability(log, "demonstrated");
            // The seed log is GitHub-hosted. emit-finalize collapses hierarchical ruleIds to their
            // base descriptor id for GitHub (whose security classifier binds by ruleId-string
            // equality), so a bare 'CWE-78' is the expected, correct shape there — AI1012 stays silent.
            SetRuleIdShape(log, descriptorId: "CWE-78", resultRuleId: "CWE-78");

            SarifLog output = RunAIValidation(log);
            List<Result> results = GetResultsForRule(output, "AI1012");

            results.Should().BeEmpty("a bare CWE base id is the expected collapsed shape on a GitHub-hosted run");
        }

        private void RunMalformedRuleIdTest(string ruleId, string because)
        {
            SarifLog log = CreateValidAISarifLog();
            SetAIOrigin(log, "generated");
            SetExploitability(log, "demonstrated");
            SetRuleIdShape(log, descriptorId: ruleId, resultRuleId: ruleId);

            SarifLog output = RunAIValidation(log);
            List<Result> results = GetResultsForRule(output, "AI1012");

            results.Should().NotBeEmpty(because);
            results[0].Level.Should().Be(FailureLevel.Error);
            results[0].Message.Id.Should().Be("Error_Malformed", because);
        }

        [Fact]
        public void AI1012_WhenSubIdIsNotLowercaseKebab_ReportsMalformed()
            => RunMalformedRuleIdTest("CWE-89/KQL_Injection", "the sub-id must be lowercase kebab-case (no uppercase, no underscores)");

        [Fact]
        public void AI1012_WhenNovelEscapeHatchUsesSlash_ReportsMalformed()
            => RunMalformedRuleIdTest("NOVEL/prompt-injection", "the NOVEL- escape hatch is flat and takes no slash");

        [Fact]
        public void AI1012_WhenSubIdIsEmpty_ReportsMalformed()
            => RunMalformedRuleIdTest("CWE-89/", "a trailing slash with no sub-id is not a sub-classification");

        [Fact]
        public void AI1012_WhenBaseIsNeitherCweNorNovel_ReportsMalformed()
            => RunMalformedRuleIdTest("TST0002", "only 'CWE-<number>/<sub-id>' and 'NOVEL-<sub-id>' are accepted bases");

        [Fact]
        public void AI1012_WhenRuleIdCarriesSubId_NoResult()
        {
            SarifLog log = CreateValidAISarifLog();
            SetAIOrigin(log, "generated");
            SetExploitability(log, "demonstrated");
            // The seed log already uses this shape; assert it stays clean.

            SarifLog output = RunAIValidation(log);
            List<Result> results = GetResultsForRule(output, "AI1012");

            results.Should().BeEmpty("'CWE-78/api-handler' carries a sub-id classifier");
        }

        [Fact]
        public void AI1012_WhenRuleIdUsesNovelPrefix_NoResult()
        {
            SarifLog log = CreateValidAISarifLog();
            SetAIOrigin(log, "generated");
            SetExploitability(log, "demonstrated");
            // NOVEL- escape hatch: flat descriptor id, same value on result.
            SetRuleIdShape(
                log,
                descriptorId: "NOVEL-prompt-injection-via-system-message",
                resultRuleId: "NOVEL-prompt-injection-via-system-message");

            SarifLog output = RunAIValidation(log);
            List<Result> results = GetResultsForRule(output, "AI1012");

            results.Should().BeEmpty("the NOVEL- prefix is recognized as an inherent sub-classifier");
        }

        #endregion

        #region AI1006 — ProvideAIOrigin

        [Fact]
        public void AI1006_WhenOriginMissing_ReportsError()
        {
            SarifLog log = CreateValidAISarifLog();
            // Don't set ai/origin
            SetExploitability(log, "demonstrated");

            SarifLog output = RunAIValidation(log);
            List<Result> results = GetResultsForRule(output, "AI1006");

            results.Should().NotBeEmpty();
            results[0].Level.Should().Be(FailureLevel.Error);
        }

        [Fact]
        public void AI1006_WhenOriginValid_NoResult()
        {
            SarifLog log = CreateValidAISarifLog();
            SetAIOrigin(log, "generated");
            SetExploitability(log, "demonstrated");

            SarifLog output = RunAIValidation(log);
            List<Result> results = GetResultsForRule(output, "AI1006");

            results.Should().BeEmpty();
        }

        [Fact]
        public void AI1006_WhenOriginInvalid_ReportsError()
        {
            SarifLog log = CreateValidAISarifLog();
            SetAIOrigin(log, "invented");
            SetExploitability(log, "demonstrated");

            SarifLog output = RunAIValidation(log);
            List<Result> results = GetResultsForRule(output, "AI1006");

            results.Should().NotBeEmpty();
            results[0].Level.Should().Be(FailureLevel.Error);
        }

        #endregion

        #region AI2014 — ProvideExploitability

        [Fact]
        public void AI2014_WhenExploitabilityMissing_ReportsWarning()
        {
            SarifLog log = CreateValidAISarifLog();
            SetAIOrigin(log, "generated");
            // Don't set ai/exploitability

            SarifLog output = RunAIValidation(log);
            List<Result> results = GetResultsForRule(output, "AI2014");

            results.Should().NotBeEmpty();
            results[0].Level.Should().Be(FailureLevel.Warning);
        }

        [Theory]
        [InlineData("demonstrated")]
        [InlineData("poc")]
        [InlineData("theoretical")]
        public void AI2014_WhenExploitabilityValid_NoResult(string value)
        {
            SarifLog log = CreateValidAISarifLog();
            SetAIOrigin(log, "generated");
            SetExploitability(log, value);

            SarifLog output = RunAIValidation(log);
            List<Result> results = GetResultsForRule(output, "AI2014");

            results.Should().BeEmpty();
        }

        [Theory]
        [InlineData("Demonstrated")]  // wrong casing (PascalCase)
        [InlineData("POC")]           // wrong casing
        [InlineData("confirmed")]     // invalid value
        [InlineData("")]              // empty string
        public void AI2014_WhenExploitabilityInvalid_ReportsWarning(string value)
        {
            SarifLog log = CreateValidAISarifLog();
            SetAIOrigin(log, "generated");
            SetExploitability(log, value);

            SarifLog output = RunAIValidation(log);
            List<Result> results = GetResultsForRule(output, "AI2014");

            results.Should().NotBeEmpty();
            results[0].Level.Should().Be(FailureLevel.Warning);
        }

        [Fact]
        public void AI2014_WhenMixedPresence_ReportsInconsistency()
        {
            SarifLog log = CreateValidAISarifLog();
            SetAIOrigin(log, "generated");
            SetExploitability(log, "demonstrated");

            // Add a second result WITHOUT exploitability
            log.Runs[0].Results = new List<Result>(log.Runs[0].Results);
            var secondResult = new Result
            {
                RuleId = "TEST002",
                Level = FailureLevel.Warning,
                Message = new Message
                {
                    Text = "Second finding.",
                    Markdown = "**Second finding.**"
                },
                Locations = new List<Location>(log.Runs[0].Results[0].Locations),
                Rank = 75.0
            };
            log.Runs[0].Results.Add(secondResult);

            SarifLog output = RunAIValidation(log);
            List<Result> results = GetResultsForRule(output, "AI2014");

            // Should get: 1 inconsistency warning (run-level) + 1 missing warning (per-result on secondResult)
            results.Should().HaveCountGreaterOrEqualTo(2,
                "AI2014 should fire both run-level inconsistency and per-result missing warnings");

            // Verify the run-level result points at the results array, not an individual result
            results.Should().Contain(r =>
                r.Locations != null && r.Locations.Count > 0 &&
                r.Locations[0].PhysicalLocation.ArtifactLocation != null &&
                r.Message.Arguments != null && r.Message.Arguments.Count >= 2,
                "run-level inconsistency warning should include with/without counts as arguments");
        }

        #endregion

        #region AI1005 — ProvideMessageMarkdown (promoted to error)

        [Fact]
        public void AI1005_WhenMarkdownMissing_ReportsError()
        {
            SarifLog log = CreateValidAISarifLog();
            SetAIOrigin(log, "generated");
            SetExploitability(log, "demonstrated");
            log.Runs[0].Results[0].Message = new Message { Text = "No markdown provided." };

            SarifLog output = RunAIValidation(log);
            List<Result> results = GetResultsForRule(output, "AI1005");

            results.Should().NotBeEmpty();
            results[0].Level.Should().Be(FailureLevel.Error);
        }

        [Fact]
        public void AI1005_WhenMarkdownPresent_NoResult()
        {
            SarifLog log = CreateValidAISarifLog();
            SetAIOrigin(log, "generated");
            SetExploitability(log, "demonstrated");

            SarifLog output = RunAIValidation(log);
            List<Result> results = GetResultsForRule(output, "AI1005");

            results.Should().BeEmpty();
        }

        #endregion

        #region AI2009 — Removed (UsePartialFingerprints)

        [Fact]
        public void AI2009_RuleIsRemoved_NoResultsEvenWithFingerprints()
        {
            SarifLog log = CreateValidAISarifLog();
            SetAIOrigin(log, "generated");
            SetExploitability(log, "demonstrated");
            log.Runs[0].Results[0].Fingerprints = new Dictionary<string, string>
            {
                { "v1", "abc123" }
            };

            SarifLog output = RunAIValidation(log);
            List<Result> results = GetResultsForRule(output, "AI2009");

            results.Should().BeEmpty("AI2009 has been removed from the AI rule set");
        }

        #endregion

        #region AI2010 — ProvideResultRank

        [Fact]
        public void AI2010_WhenRankMissing_ReportsNote()
        {
            SarifLog log = CreateValidAISarifLog();
            SetAIOrigin(log, "generated");
            SetExploitability(log, "demonstrated");
            log.Runs[0].Results[0].Rank = -1.0; // default = unknown

            SarifLog output = RunAIValidation(log);
            List<Result> results = GetResultsForRule(output, "AI2010");

            results.Should().NotBeEmpty();
            results[0].Level.Should().Be(FailureLevel.Note);
        }

        #endregion

        #region Elevated rules — SARIF2010, SARIF2011 (via AI profile)

        [Fact]
        public void SARIF2010_FiresInAIProfile_WhenSnippetMissing()
        {
            SarifLog log = CreateValidAISarifLog();
            SetAIOrigin(log, "generated");
            SetExploitability(log, "demonstrated");

            // Remove snippet from region
            log.Runs[0].Results[0].Locations[0].PhysicalLocation.Region.Snippet = null;

            SarifLog output = RunAIValidation(log);
            List<Result> results = GetResultsForRule(output, "SARIF2010");

            results.Should().NotBeEmpty("SARIF2010 should fire in AI profile");
        }

        [Fact]
        public void SARIF2011_FiresInAIProfile_WhenContextRegionMissing()
        {
            SarifLog log = CreateValidAISarifLog();
            SetAIOrigin(log, "generated");
            SetExploitability(log, "demonstrated");

            // Remove context region
            log.Runs[0].Results[0].Locations[0].PhysicalLocation.ContextRegion = null;

            SarifLog output = RunAIValidation(log);
            List<Result> results = GetResultsForRule(output, "SARIF2011");

            results.Should().NotBeEmpty("SARIF2011 should fire in AI profile");
        }

        #endregion

        #region AI1003 — ProvideRequiredRegionProperties (error in AI profile)

        [Fact]
        public void AI1003_WhenRegionMissing_ReportsError()
        {
            SarifLog log = CreateValidAISarifLog();
            SetAIOrigin(log, "generated");
            SetExploitability(log, "demonstrated");

            // Remove region entirely
            log.Runs[0].Results[0].Locations[0].PhysicalLocation.Region = null;

            SarifLog output = RunAIValidation(log);
            List<Result> results = GetResultsForRule(output, "AI1003");

            results.Should().NotBeEmpty("AI1003 should fire when region is missing");
            results[0].Level.Should().Be(FailureLevel.Error);
        }

        [Fact]
        public void AI1003_WhenStartLineMissing_ReportsError()
        {
            SarifLog log = CreateValidAISarifLog();
            SetAIOrigin(log, "generated");
            SetExploitability(log, "demonstrated");

            // Set region without startLine (startLine defaults to 0)
            log.Runs[0].Results[0].Locations[0].PhysicalLocation.Region = new Region();

            SarifLog output = RunAIValidation(log);
            List<Result> results = GetResultsForRule(output, "AI1003");

            results.Should().NotBeEmpty("AI1003 should fire when startLine is absent");
            results[0].Level.Should().Be(FailureLevel.Error);
        }

        [Fact]
        public void AI1003_WhenRegionComplete_NoResult()
        {
            SarifLog log = CreateValidAISarifLog();
            SetAIOrigin(log, "generated");
            SetExploitability(log, "demonstrated");

            SarifLog output = RunAIValidation(log);
            List<Result> results = GetResultsForRule(output, "AI1003");

            results.Should().BeEmpty("fully populated region should not trigger AI1003");
        }

        #endregion

        #region AI1004 — ProvideVersionControlProvenance (error in AI profile)

        [Fact]
        public void AI1004_WhenVCPMissing_ReportsError()
        {
            SarifLog log = CreateValidAISarifLog();
            SetAIOrigin(log, "generated");
            SetExploitability(log, "demonstrated");
            log.Runs[0].VersionControlProvenance = null;

            SarifLog output = RunAIValidation(log);
            List<Result> results = GetResultsForRule(output, "AI1004");

            results.Should().NotBeEmpty("AI1004 should fire when versionControlProvenance is missing");
            results[0].Level.Should().Be(FailureLevel.Error);
        }

        [Fact]
        public void AI1004_WhenVCPPresent_NoResult()
        {
            SarifLog log = CreateValidAISarifLog();
            SetAIOrigin(log, "generated");
            SetExploitability(log, "demonstrated");

            SarifLog output = RunAIValidation(log);
            List<Result> results = GetResultsForRule(output, "AI1004");

            results.Should().BeEmpty("VCP is present in the valid log");
        }

        [Fact]
        public void AI1004_WhenRunMarkedUnpublishable_NoResult()
        {
            SarifLog log = CreateValidAISarifLog();
            SetAIOrigin(log, "generated");
            SetExploitability(log, "demonstrated");
            log.Runs[0].VersionControlProvenance = null;
            log.Runs[0].SetProperty(EmitFinalizeCommand.UnpublishablePropertyName, true);

            SarifLog output = RunAIValidation(log);
            List<Result> results = GetResultsForRule(output, "AI1004");

            results.Should().BeEmpty(
                "a run finalized --no-repo is stamped unpublishable and must be exempt from AI1004");
        }

        #endregion

        #region SARIF2017 — does NOT fire in AI-only profile

        [Fact]
        public void SARIF2017_DoesNotFireInAIProfile()
        {
            SarifLog log = CreateValidAISarifLog();
            SetAIOrigin(log, "generated");
            SetExploitability(log, "demonstrated");

            // Remove region — AI1003 should fire, but not SARIF2017
            log.Runs[0].Results[0].Locations[0].PhysicalLocation.Region = null;

            SarifLog output = RunAIValidation(log);
            List<Result> results = GetResultsForRule(output, "SARIF2017");

            results.Should().BeEmpty("SARIF2017 is Sarif-only, should not fire under --rule-kind AI");
        }

        #endregion

        #region AI1007 / AI2011 — DoNotPersist(Partial)Fingerprints

        [Fact]
        public void AI2011_WhenNonLineHashPartialFingerprintPresent_ReportsWarning()
        {
            SarifLog log = CreateValidAISarifLog();
            SetAIOrigin(log, "generated");
            SetExploitability(log, "demonstrated");
            SetAiHandoff(log, "tier 2 reached");
            log.Runs[0].Results[0].PartialFingerprints = new Dictionary<string, string>
            {
                { "contextRegionHash/v1", "abc123" }
            };

            SarifLog output = RunAIValidation(log);
            List<Result> results = GetResultsForRule(output, "AI2011");

            results.Should().NotBeEmpty();
            results[0].Level.Should().Be(FailureLevel.Warning);

            // The Error-level fingerprints rule must NOT fire on partial fingerprints.
            GetResultsForRule(output, "AI1007").Should().BeEmpty();
        }

        [Fact]
        public void AI2011_WhenLoneLineHashOnGitHubHostedRun_NoWarning()
        {
            // GitHub's raw code-scanning upload API does not backfill partialFingerprints,
            // so a github-hosted run may persist the single rolling-hash
            // primaryLocationLineHash without AI2011 objecting. CreateValidAISarifLog is
            // github.com-hosted.
            SarifLog log = CreateValidAISarifLog();
            SetAIOrigin(log, "generated");
            SetExploitability(log, "demonstrated");
            SetAiHandoff(log, "tier 2 reached");
            log.Runs[0].Results[0].PartialFingerprints = new Dictionary<string, string>
            {
                { "primaryLocationLineHash", "abc123" }
            };

            SarifLog output = RunAIValidation(log);

            GetResultsForRule(output, "AI2011").Should().BeEmpty();
        }

        [Fact]
        public void AI2011_WhenLoneLineHashOnAzureDevOpsHostedRun_ReportsWarning()
        {
            // The carve-out is github-only; a dev.azure.com-hosted run still gets flagged.
            SarifLog log = CreateValidAISarifLog();
            SetAIOrigin(log, "generated");
            SetExploitability(log, "demonstrated");
            SetAiHandoff(log, "tier 2 reached");
            SetRepositoryHostToAzureDevOps(log);
            log.Runs[0].Results[0].PartialFingerprints = new Dictionary<string, string>
            {
                { "primaryLocationLineHash", "abc123" }
            };

            SarifLog output = RunAIValidation(log);
            List<Result> results = GetResultsForRule(output, "AI2011");

            results.Should().NotBeEmpty();
            results[0].Level.Should().Be(FailureLevel.Warning);
        }

        [Fact]
        public void AI2011_WhenLineHashAccompaniedByOtherKeyOnGitHubRun_ReportsWarning()
        {
            // The carve-out covers only the LONE primaryLocationLineHash; a second
            // partial fingerprint forfeits it even on a github-hosted run.
            SarifLog log = CreateValidAISarifLog();
            SetAIOrigin(log, "generated");
            SetExploitability(log, "demonstrated");
            SetAiHandoff(log, "tier 2 reached");
            log.Runs[0].Results[0].PartialFingerprints = new Dictionary<string, string>
            {
                { "primaryLocationLineHash", "abc123" },
                { "contextRegionHash/v1", "def456" }
            };

            SarifLog output = RunAIValidation(log);
            List<Result> results = GetResultsForRule(output, "AI2011");

            results.Should().NotBeEmpty();
            results[0].Level.Should().Be(FailureLevel.Warning);
        }

        [Fact]
        public void AI1007_WhenFingerprintsPresent_ReportsError()
        {
            SarifLog log = CreateValidAISarifLog();
            SetAIOrigin(log, "generated");
            SetExploitability(log, "demonstrated");
            SetAiHandoff(log, "tier 2 reached");
            log.Runs[0].Results[0].Fingerprints = new Dictionary<string, string>
            {
                { "stable/v1", "deadbeef" }
            };

            SarifLog output = RunAIValidation(log);
            List<Result> results = GetResultsForRule(output, "AI1007");

            results.Should().NotBeEmpty();
            results[0].Level.Should().Be(FailureLevel.Error);

            // The Warning-level partial-fingerprints rule must NOT fire on fingerprints.
            GetResultsForRule(output, "AI2011").Should().BeEmpty();
        }

        [Fact]
        public void AI1007_AI2011_WhenNoFingerprints_NoResult()
        {
            SarifLog log = CreateValidAISarifLog();
            SetAIOrigin(log, "generated");
            SetExploitability(log, "demonstrated");
            SetAiHandoff(log, "tier 2 reached");

            SarifLog output = RunAIValidation(log);

            GetResultsForRule(output, "AI1007").Should().BeEmpty();
            GetResultsForRule(output, "AI2011").Should().BeEmpty();
        }

        #endregion

        #region AI2012 — ProvideAiHandoff

        [Fact]
        public void AI2012_WhenHandoffMissing_ReportsNote()
        {
            SarifLog log = CreateValidAISarifLog();
            SetAIOrigin(log, "generated");
            SetExploitability(log, "demonstrated");
            // no ai/handoff

            SarifLog output = RunAIValidation(log);
            List<Result> results = GetResultsForRule(output, "AI2012");

            results.Should().NotBeEmpty();
            results[0].Level.Should().Be(FailureLevel.Note);
        }

        [Fact]
        public void AI2012_WhenHandoffEmpty_ReportsNote()
        {
            SarifLog log = CreateValidAISarifLog();
            SetAIOrigin(log, "generated");
            SetExploitability(log, "demonstrated");
            SetAiHandoff(log, "   ");

            SarifLog output = RunAIValidation(log);
            List<Result> results = GetResultsForRule(output, "AI2012");

            results.Should().NotBeEmpty();
        }

        [Fact]
        public void AI2012_WhenHandoffPresent_NoResult()
        {
            SarifLog log = CreateValidAISarifLog();
            SetAIOrigin(log, "generated");
            SetExploitability(log, "demonstrated");
            SetAiHandoff(log, "Reached tier 3 (tests). Formatter: black --line-length 100.");

            SarifLog output = RunAIValidation(log);
            List<Result> results = GetResultsForRule(output, "AI2012");

            results.Should().BeEmpty();
        }

        #endregion

        #region Full valid AI SARIF — zero violations

        [Fact]
        public void FullyConformantAISarif_ProducesNoAIViolations()
        {
            SarifLog log = CreateValidAISarifLog();
            SetAIOrigin(log, "generated");
            SetExploitability(log, "demonstrated");
            SetAttackerPosition(log, "network");
            SetAiHandoff(log, "Reached tier 3 (tests). Formatter: black --line-length 100.");

            SarifLog output = RunAIValidation(log);

            // Filter to only AI-prefixed rules
            var aiResults = output.Runs[0].Results
                .Where(r => r.RuleId.StartsWith("AI"))
                .ToList();

            aiResults.Should().BeEmpty("a fully conformant AI SARIF should produce no AI rule violations");
        }

        #endregion

        #region Rule discovery

        [Fact]
        public void AIRuleSet_ContainsExpectedRules()
        {
            // Verify the expected rules exist and AI2009 is gone
            string[] ruleIds = new[]
            {
                RuleId.AIProvideRequiredRegionProperties, // AI1003
                RuleId.AIProvideVersionControlProvenance, // AI1004
                RuleId.AIProvideAIOrigin,         // AI1006
                RuleId.AIProvideExploitability,    // AI2014
                RuleId.AIProvideAttackerPosition,  // AI2015
                RuleId.AIProvideEvidenceBacking,   // AI2016
                RuleId.AIProvideEvidenceBackingUri, // AI1010
                RuleId.AIProvideRuleSubId,         // AI1012
                RuleId.AIProvideSemanticVersion,   // AI2003
                RuleId.AIProvideAutomationDetails, // AI2005
                RuleId.AIProvideMessageMarkdown,   // AI1005
                RuleId.AIProvideResultRank,         // AI2010
                RuleId.AIDoNotPersistFingerprints,  // AI1007
                RuleId.AIDoNotPersistPartialFingerprints,  // AI2011
                RuleId.AIProvideAiHandoff,          // AI2012
                RuleId.AIProvideNotificationDescriptor,    // AI2017
                RuleId.AIProvideNotificationAssociatedRule, // AI1013
                RuleId.AIProvideLearningSignalArtifact,         // AI2018
                RuleId.AIProvideNotificationTimestamp,      // AI2019
            };

            ruleIds.Should().HaveCount(19);
            ruleIds.Should().Contain("AI1003");
            ruleIds.Should().Contain("AI1004");
            ruleIds.Should().Contain("AI2014");
            ruleIds.Should().Contain("AI2015");
            ruleIds.Should().Contain("AI2016");
            ruleIds.Should().Contain("AI1010");
            ruleIds.Should().Contain("AI1007");
            ruleIds.Should().Contain("AI2011");
            ruleIds.Should().Contain("AI2012");
            ruleIds.Should().Contain("AI2017");
            ruleIds.Should().Contain("AI2019");
            ruleIds.Should().NotContain("AI2009");
            ruleIds.Should().NotContain("AI1014");
        }

        #endregion

        #region AI2015 — ProvideAttackerPosition

        [Fact]
        public void AI2015_WhenAttackerPositionMissing_ReportsWarning()
        {
            SarifLog log = CreateValidAISarifLog();
            SetAIOrigin(log, "generated");
            SetExploitability(log, "demonstrated");

            SarifLog output = RunAIValidation(log);
            List<Result> results = GetResultsForRule(output, "AI2015");

            results.Should().NotBeEmpty();
            results[0].Level.Should().Be(FailureLevel.Warning);
        }

        [Fact]
        public void AI2015_WhenAttackerPositionPresent_NoResult()
        {
            SarifLog log = CreateValidAISarifLog();
            SetAIOrigin(log, "generated");
            SetExploitability(log, "demonstrated");
            SetAttackerPosition(log, "network");

            SarifLog output = RunAIValidation(log);
            List<Result> results = GetResultsForRule(output, "AI2015");

            results.Should().BeEmpty();
        }

        #endregion

        #region AI2019 — ProvideNotificationTimestamp

        [Fact]
        public void AI2019_WhenTimestampMissing_ReportsNote()
        {
            SarifLog log = CreateValidAISarifLog();
            SetAIOrigin(log, "generated");
            SetExploitability(log, "demonstrated");
            SetAttackerPosition(log, "network");
            log.Runs[0].Invocations = new[]
            {
                new Invocation
                {
                    ExecutionSuccessful = true,
                    ToolExecutionNotifications = new[]
                    {
                        new Notification
                        {
                            Descriptor = new ReportingDescriptorReference { Id = "DECISION" },
                            Message = new Message { Text = "Analysis started." },
                            Level = FailureLevel.Note
                        }
                    }
                }
            };

            SarifLog output = RunAIValidation(log);
            List<Result> results = GetResultsForRule(output, "AI2019");

            results.Should().NotBeEmpty();
            results[0].Level.Should().Be(FailureLevel.Note);
        }

        [Fact]
        public void AI2019_WhenTimestampPresent_NoResult()
        {
            SarifLog log = CreateValidAISarifLog();
            SetAIOrigin(log, "generated");
            SetExploitability(log, "demonstrated");
            SetAttackerPosition(log, "network");
            log.Runs[0].Invocations = new[]
            {
                new Invocation
                {
                    ExecutionSuccessful = true,
                    ToolExecutionNotifications = new[]
                    {
                        new Notification
                        {
                            Descriptor = new ReportingDescriptorReference { Id = "DECISION" },
                            Message = new Message { Text = "Analysis started." },
                            Level = FailureLevel.Note,
                            TimeUtc = System.DateTime.UtcNow
                        }
                    }
                }
            };

            SarifLog output = RunAIValidation(log);
            List<Result> results = GetResultsForRule(output, "AI2019");

            results.Should().BeEmpty();
        }

        #endregion

        #region Evidence well-formedness — AI2016 owns the malformed-evidence report

        private static void SetEvidence(SarifLog log, object value)
        {
            log.Runs[0].Results[0].SetProperty("ai/evidence", value);
        }

        [Fact]
        public void AI2016_WhenEvidenceIsNotAnArray_ReportsMalformed()
        {
            SarifLog log = CreateValidAISarifLog();
            SetAIOrigin(log, "generated");
            SetExploitability(log, "demonstrated");
            // 'ai/evidence' is present but is a bare string, not a JSON array.
            SetEvidence(log, "this-is-not-a-json-array");

            SarifLog output = RunAIValidation(log);
            List<Result> results = GetResultsForRule(output, "AI2016");

            results.Should().ContainSingle("AI2016 owns the well-formedness report for a malformed 'ai/evidence'");
            results[0].Level.Should().Be(FailureLevel.Warning);
            results[0].Message.Id.Should().Be("Warning_MalformedEvidence");
        }

        [Fact]
        public void AI1010_WhenEvidenceIsNotAnArray_NoResult()
        {
            SarifLog log = CreateValidAISarifLog();
            SetAIOrigin(log, "generated");
            SetExploitability(log, "demonstrated");
            SetEvidence(log, "this-is-not-a-json-array");

            SarifLog output = RunAIValidation(log);
            List<Result> results = GetResultsForRule(output, "AI1010");

            results.Should().BeEmpty("AI1010 skips malformed 'ai/evidence' cleanly and defers the report to AI2016");
        }

        [Fact]
        public void AI2016_WhenEvidenceIsWellFormedArray_NoMalformedResult()
        {
            SarifLog log = CreateValidAISarifLog();
            SetAIOrigin(log, "generated");
            // A well-formed evidence array with a non-demonstrated entry: nothing to flag.
            SetEvidence(log, new object[] { new { summary = "Reviewed call site.", strength = "theoretical" } });

            SarifLog output = RunAIValidation(log);
            List<Result> results = GetResultsForRule(output, "AI2016");

            results.Should().BeEmpty("a well-formed 'ai/evidence' array with no demonstrated entry is conformant");
        }

        #endregion

        #region AI1015 — ProvideReciprocalGroupingRelationships

        private static Location SarifPointerRelatedLocation(int id, string pointer)
            => new Location
            {
                Id = id,
                PhysicalLocation = new PhysicalLocation
                {
                    ArtifactLocation = new ArtifactLocation
                    {
                        Uri = new System.Uri(pointer, System.UriKind.RelativeOrAbsolute)
                    }
                }
            };

        private static Result GroupingResult(string text, string kind, int relatedLocationId, string targetPointer, bool declareRelationship)
        {
            var primary = new Location
            {
                Id = 0,
                PhysicalLocation = new PhysicalLocation
                {
                    ArtifactLocation = new ArtifactLocation { Uri = new System.Uri("src/a.cs", System.UriKind.Relative) }
                }
            };

            if (declareRelationship)
            {
                primary.Relationships = new List<LocationRelationship>
                {
                    new LocationRelationship { Target = relatedLocationId, Kinds = new[] { kind } }
                };
            }

            var result = new Result
            {
                RuleId = "CWE-89/kql-injection",
                Message = new Message { Text = text },
                Locations = new List<Location> { primary }
            };

            if (declareRelationship)
            {
                result.RelatedLocations = new List<Location>
                {
                    SarifPointerRelatedLocation(relatedLocationId, targetPointer)
                };
            }

            return result;
        }

        private static SarifLog CreateGroupedLog(bool generatedDeclaresInverse, bool synthesizedDeclaresIncludes)
        {
            var generated = new Run
            {
                Tool = new Tool { Driver = new ToolComponent { Name = "GeneratedScanner" } },
                Results = new List<Result>
                {
                    GroupingResult("A raw generated finding.", "isIncludedBy", 1, "sarif:/runs/1/results/0", generatedDeclaresInverse)
                }
            };

            var synthesized = new Run
            {
                Tool = new Tool { Driver = new ToolComponent { Name = "SynthesizedSkill" } },
                Results = new List<Result>
                {
                    GroupingResult("A synthesized grouping finding.", "includes", 1, "sarif:/runs/0/results/0", synthesizedDeclaresIncludes)
                }
            };

            return new SarifLog { Runs = new[] { generated, synthesized } };
        }

        [Fact]
        public void AI1015_WhenGroupingRelationshipsAreReciprocal_NoResult()
        {
            SarifLog log = CreateGroupedLog(generatedDeclaresInverse: true, synthesizedDeclaresIncludes: true);

            SarifLog output = RunAIValidation(log);
            List<Result> results = GetResultsForRule(output, "AI1015");

            results.Should().BeEmpty("the 'includes' and 'isIncludedBy' edges point back at each other");
        }

        [Fact]
        public void AI1015_WhenIncludesHasNoReciprocal_ReportsError()
        {
            SarifLog log = CreateGroupedLog(generatedDeclaresInverse: false, synthesizedDeclaresIncludes: true);

            SarifLog output = RunAIValidation(log);
            List<Result> results = GetResultsForRule(output, "AI1015");

            results.Should().NotBeEmpty("the synthesized 'includes' edge has no matching 'isIncludedBy' back");
            results[0].Level.Should().Be(FailureLevel.Error);
        }

        [Fact]
        public void AI1015_WhenIsIncludedByHasNoReciprocal_ReportsError()
        {
            SarifLog log = CreateGroupedLog(generatedDeclaresInverse: true, synthesizedDeclaresIncludes: false);

            SarifLog output = RunAIValidation(log);
            List<Result> results = GetResultsForRule(output, "AI1015");

            results.Should().NotBeEmpty("the generated 'isIncludedBy' edge has no matching 'includes' back");
            results[0].Level.Should().Be(FailureLevel.Error);
        }

        #endregion

        #region AI2020 — ProvideConventionalGroupingDirection

        private static SarifLog CreateDirectedGroupingLog(string includingOrigin, string includedOrigin)
        {
            var included = new Run
            {
                Tool = new Tool { Driver = new ToolComponent { Name = "IncludedScanner" } },
                Results = new List<Result>
                {
                    GroupingResult("An included finding.", "isIncludedBy", 1, "sarif:/runs/1/results/0", declareRelationship: true)
                }
            };

            var including = new Run
            {
                Tool = new Tool { Driver = new ToolComponent { Name = "IncludingSkill" } },
                Results = new List<Result>
                {
                    GroupingResult("An including finding.", "includes", 1, "sarif:/runs/0/results/0", declareRelationship: true)
                }
            };

            if (includedOrigin != null)
            {
                included.SetProperty("ai/origin", includedOrigin);
            }

            if (includingOrigin != null)
            {
                including.SetProperty("ai/origin", includingOrigin);
            }

            return new SarifLog { Runs = new[] { included, including } };
        }

        [Fact]
        public void AI2020_WhenSynthesizedIncludesGenerated_NoResult()
        {
            SarifLog log = CreateDirectedGroupingLog(includingOrigin: "synthesized", includedOrigin: "generated");

            SarifLog output = RunAIValidation(log);
            List<Result> results = GetResultsForRule(output, "AI2020");

            results.Should().BeEmpty("an 'includes' edge from a synthesized run to a generated run is the conventional direction");
        }

        [Fact]
        public void AI2020_WhenGeneratedIncludesSynthesized_ReportsWarning()
        {
            SarifLog log = CreateDirectedGroupingLog(includingOrigin: "generated", includedOrigin: "synthesized");

            SarifLog output = RunAIValidation(log);
            List<Result> results = GetResultsForRule(output, "AI2020");

            results.Should().NotBeEmpty("the 'includes' edge inverts the generated/synthesized hierarchy");
            results[0].Level.Should().Be(FailureLevel.Warning);
        }

        [Fact]
        public void AI2020_WhenIncludesIsWithinSameTier_ReportsWarning()
        {
            SarifLog log = CreateDirectedGroupingLog(includingOrigin: "synthesized", includedOrigin: "synthesized");

            SarifLog output = RunAIValidation(log);
            List<Result> results = GetResultsForRule(output, "AI2020");

            results.Should().NotBeEmpty("an 'includes' edge should target a generated or annotated run, not another synthesized one");
            results[0].Level.Should().Be(FailureLevel.Warning);
        }

        [Fact]
        public void AI2020_WhenOriginsAbsent_NoResult()
        {
            SarifLog log = CreateDirectedGroupingLog(includingOrigin: null, includedOrigin: null);

            SarifLog output = RunAIValidation(log);
            List<Result> results = GetResultsForRule(output, "AI2020");

            results.Should().BeEmpty("direction is assessed only when both runs declare an 'ai/origin'");
        }

        #endregion

        #region AI1016 — ProvideValidRuleId

        // The rule is descriptor-level: it judges 'tool.driver.rules[].id'. The seed log carries a
        // single rule descriptor, so setting its id is enough; the matching result.ruleId is set too
        // to keep the log internally consistent (this rule never inspects the result).
        private void RunAI1016ErrorTest(string descriptorId, string expectedMessageId, string because)
        {
            SarifLog log = CreateValidAISarifLog();
            SetRuleIdShape(log, descriptorId: descriptorId, resultRuleId: descriptorId);

            SarifLog output = RunAIValidation(log);
            List<Result> results = GetResultsForRule(output, "AI1016");

            results.Should().NotBeEmpty(because);
            results[0].Level.Should().Be(FailureLevel.Error);
            results[0].Message.Id.Should().Be(expectedMessageId, because);
        }

        private void RunAI1016NoResultTest(string descriptorId, string because)
        {
            SarifLog log = CreateValidAISarifLog();
            SetRuleIdShape(log, descriptorId: descriptorId, resultRuleId: descriptorId);

            SarifLog output = RunAIValidation(log);
            GetResultsForRule(output, "AI1016").Should().BeEmpty(because);
        }

        [Fact]
        public void AI1016_WhenDescriptorIdIsCweCategory_ReportsCategoryError()
            => RunAI1016ErrorTest(
                "CWE-16",
                "Error_Category",
                "CWE-16 is the MITRE Category 'Configuration', not a Weakness, so it is never a valid rule id");

        [Fact]
        public void AI1016_WhenDescriptorIdIsCweView_ReportsNotAWeaknessError()
            => RunAI1016ErrorTest(
                "CWE-1000",
                "Error_NotAWeakness",
                "CWE-1000 is a MITRE View, not a Weakness");

        [Fact]
        public void AI1016_WhenDescriptorIdIsUnknownCwe_ReportsNotAWeaknessError()
            => RunAI1016ErrorTest(
                "CWE-99999",
                "Error_NotAWeakness",
                "CWE-99999 is not a known CWE Weakness (a typo or an id newer than the bundled catalog)");

        [Fact]
        public void AI1016_WhenDescriptorIdIsMitreAttackTechnique_ReportsNotCweOrNovelError()
            => RunAI1016ErrorTest(
                "MITRE-ATTACK-T1059",
                "Error_NotCweOrNovel",
                "a MITRE ATT&CK technique is neither a CWE Weakness nor a NOVEL- id");

        [Fact]
        public void AI1016_WhenDescriptorIdIsToolPrivateId_ReportsNotCweOrNovelError()
            => RunAI1016ErrorTest(
                "TST0002",
                "Error_NotCweOrNovel",
                "a tool-private rule id is neither a CWE Weakness nor a NOVEL- id");

        [Fact]
        public void AI1016_WhenDescriptorIdIsCweWeakness_NoResult()
            => RunAI1016NoResultTest(
                "CWE-89",
                "CWE-89 is a genuine Weakness and a valid native rule id");

        [Fact]
        public void AI1016_WhenDescriptorIdUsesNovelPrefix_NoResult()
            => RunAI1016NoResultTest(
                "NOVEL-prompt-injection",
                "the NOVEL- escape hatch is a valid rule id for a finding that maps to no CWE");

        #endregion
    }
}
