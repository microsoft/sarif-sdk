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
                if (File.Exists(inputPath)) File.Delete(inputPath);
                if (File.Exists(outputPath)) File.Delete(outputPath);
            }
        }

        private static List<Result> GetResultsForRule(SarifLog output, string ruleId)
        {
            return output.Runs[0].Results
                .Where(r => r.RuleId == ruleId)
                .ToList();
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

        #region AI1007 — ProvideExploitability

        [Fact]
        public void AI1007_WhenExploitabilityMissing_ReportsWarning()
        {
            SarifLog log = CreateValidAISarifLog();
            SetAIOrigin(log, "generated");
            // Don't set ai/exploitability

            SarifLog output = RunAIValidation(log);
            List<Result> results = GetResultsForRule(output, "AI1007");

            results.Should().NotBeEmpty();
            results[0].Level.Should().Be(FailureLevel.Warning);
        }

        [Theory]
        [InlineData("demonstrated")]
        [InlineData("poc")]
        [InlineData("theoretical")]
        public void AI1007_WhenExploitabilityValid_NoResult(string value)
        {
            SarifLog log = CreateValidAISarifLog();
            SetAIOrigin(log, "generated");
            SetExploitability(log, value);

            SarifLog output = RunAIValidation(log);
            List<Result> results = GetResultsForRule(output, "AI1007");

            results.Should().BeEmpty();
        }

        [Theory]
        [InlineData("Demonstrated")]  // wrong casing (PascalCase)
        [InlineData("POC")]           // wrong casing
        [InlineData("confirmed")]     // invalid value
        [InlineData("")]              // empty string
        public void AI1007_WhenExploitabilityInvalid_ReportsWarning(string value)
        {
            SarifLog log = CreateValidAISarifLog();
            SetAIOrigin(log, "generated");
            SetExploitability(log, value);

            SarifLog output = RunAIValidation(log);
            List<Result> results = GetResultsForRule(output, "AI1007");

            results.Should().NotBeEmpty();
            results[0].Level.Should().Be(FailureLevel.Warning);
        }

        [Fact]
        public void AI1007_WhenMixedPresence_ReportsInconsistency()
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
            List<Result> results = GetResultsForRule(output, "AI1007");

            // Should get: 1 inconsistency warning (run-level) + 1 missing warning (per-result on secondResult)
            results.Should().HaveCountGreaterOrEqualTo(2,
                "AI1007 should fire both run-level inconsistency and per-result missing warnings");

            // Verify the run-level result points at the results array, not an individual result
            results.Should().Contain(r =>
                r.Locations != null && r.Locations.Count > 0 &&
                r.Locations[0].PhysicalLocation.ArtifactLocation != null &&
                r.Message.Arguments != null && r.Message.Arguments.Count >= 2,
                "run-level inconsistency warning should include with/without counts as arguments");
        }

        #endregion

        #region AI2006 — ProvideMessageMarkdown (promoted to error)

        [Fact]
        public void AI2006_WhenMarkdownMissing_ReportsError()
        {
            SarifLog log = CreateValidAISarifLog();
            SetAIOrigin(log, "generated");
            SetExploitability(log, "demonstrated");
            log.Runs[0].Results[0].Message = new Message { Text = "No markdown provided." };

            SarifLog output = RunAIValidation(log);
            List<Result> results = GetResultsForRule(output, "AI2006");

            results.Should().NotBeEmpty();
            results[0].Level.Should().Be(FailureLevel.Error);
        }

        [Fact]
        public void AI2006_WhenMarkdownPresent_NoResult()
        {
            SarifLog log = CreateValidAISarifLog();
            SetAIOrigin(log, "generated");
            SetExploitability(log, "demonstrated");

            SarifLog output = RunAIValidation(log);
            List<Result> results = GetResultsForRule(output, "AI2006");

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

        #region AI2011 — DoNotPersistFingerprints

        [Fact]
        public void AI2011_WhenPartialFingerprintsPresent_ReportsNote()
        {
            SarifLog log = CreateValidAISarifLog();
            SetAIOrigin(log, "generated");
            SetExploitability(log, "demonstrated");
            SetAiHandoff(log, "tier 2 reached");
            log.Runs[0].Results[0].PartialFingerprints = new Dictionary<string, string>
            {
                { "primaryLocationLineHash", "abc123" }
            };

            SarifLog output = RunAIValidation(log);
            List<Result> results = GetResultsForRule(output, "AI2011");

            results.Should().NotBeEmpty();
            results[0].Level.Should().Be(FailureLevel.Note);
        }

        [Fact]
        public void AI2011_WhenFingerprintsPresent_ReportsNote()
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
            List<Result> results = GetResultsForRule(output, "AI2011");

            results.Should().NotBeEmpty();
            results[0].Level.Should().Be(FailureLevel.Note);
        }

        [Fact]
        public void AI2011_WhenNoFingerprints_NoResult()
        {
            SarifLog log = CreateValidAISarifLog();
            SetAIOrigin(log, "generated");
            SetExploitability(log, "demonstrated");
            SetAiHandoff(log, "tier 2 reached");

            SarifLog output = RunAIValidation(log);
            List<Result> results = GetResultsForRule(output, "AI2011");

            results.Should().BeEmpty();
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
            List<Result> aiResults = output.Runs[0].Results
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
            var ruleIds = new[]
            {
                RuleId.AIProvideRequiredRegionProperties, // AI1003
                RuleId.AIProvideVersionControlProvenance, // AI1004
                RuleId.AIProvideAIOrigin,         // AI1006
                RuleId.AIProvideExploitability,    // AI1007
                RuleId.AIProvideAttackerPosition,  // AI1008
                RuleId.AIProvideEvidenceBacking,   // AI1009
                RuleId.AIProvideEvidenceBackingUri, // AI1010
                RuleId.AIProvideRuleSubId,         // AI1012
                RuleId.AIProvideSemanticVersion,   // AI2003
                RuleId.AIProvideAutomationDetails, // AI2005
                RuleId.AIProvideMessageMarkdown,   // AI2006
                RuleId.AIProvideResultRank,         // AI2010
                RuleId.AIDoNotPersistFingerprints,  // AI2011
                RuleId.AIProvideAiHandoff,          // AI2012
                RuleId.AIRedactedRunMarker,         // AI2013
                RuleId.AIProvideNotificationDescriptor,    // AI3001
                RuleId.AIProvideNotificationAssociatedRule, // AI3002
                RuleId.AIExecutionNotificationPlacement,   // AI3003
                RuleId.AIProvideALASSignalArtifact,        // AI3004
                RuleId.AIProvideNotificationTimestamp,      // AI3005
            };

            ruleIds.Should().HaveCount(20);
            ruleIds.Should().Contain("AI1003");
            ruleIds.Should().Contain("AI1004");
            ruleIds.Should().Contain("AI1007");
            ruleIds.Should().Contain("AI1008");
            ruleIds.Should().Contain("AI1009");
            ruleIds.Should().Contain("AI1010");
            ruleIds.Should().Contain("AI2011");
            ruleIds.Should().Contain("AI2012");
            ruleIds.Should().Contain("AI2013");
            ruleIds.Should().Contain("AI3001");
            ruleIds.Should().Contain("AI3003");
            ruleIds.Should().Contain("AI3005");
            ruleIds.Should().NotContain("AI2009");
        }

        #endregion

        #region AI1008 — ProvideAttackerPosition

        [Fact]
        public void AI1008_WhenAttackerPositionMissing_ReportsWarning()
        {
            SarifLog log = CreateValidAISarifLog();
            SetAIOrigin(log, "generated");
            SetExploitability(log, "demonstrated");

            SarifLog output = RunAIValidation(log);
            List<Result> results = GetResultsForRule(output, "AI1008");

            results.Should().NotBeEmpty();
            results[0].Level.Should().Be(FailureLevel.Warning);
        }

        [Fact]
        public void AI1008_WhenAttackerPositionPresent_NoResult()
        {
            SarifLog log = CreateValidAISarifLog();
            SetAIOrigin(log, "generated");
            SetExploitability(log, "demonstrated");
            SetAttackerPosition(log, "network");

            SarifLog output = RunAIValidation(log);
            List<Result> results = GetResultsForRule(output, "AI1008");

            results.Should().BeEmpty();
        }

        #endregion

        #region AI2013 — RedactedRunMarker

        [Fact]
        public void AI2013_WhenRedactedIsFalse_ReportsWarning()
        {
            SarifLog log = CreateValidAISarifLog();
            SetAIOrigin(log, "generated");
            SetExploitability(log, "demonstrated");
            SetAttackerPosition(log, "network");
            log.Runs[0].SetProperty("ai/redacted", "false");

            SarifLog output = RunAIValidation(log);
            List<Result> results = GetResultsForRule(output, "AI2013");

            results.Should().NotBeEmpty();
        }

        [Fact]
        public void AI2013_WhenRedactedAbsent_NoResult()
        {
            SarifLog log = CreateValidAISarifLog();
            SetAIOrigin(log, "generated");
            SetExploitability(log, "demonstrated");
            SetAttackerPosition(log, "network");

            SarifLog output = RunAIValidation(log);
            List<Result> results = GetResultsForRule(output, "AI2013");

            results.Should().BeEmpty();
        }

        #endregion

        #region AI3003 — ExecutionNotificationPlacement

        [Fact]
        public void AI3003_WhenCfgDescriptorInExecNotifications_ReportsWarning()
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
                            Descriptor = new ReportingDescriptorReference { Id = "AI/CFG/TOOL-UNAVAILABLE" },
                            Message = new Message { Text = "CodeQL not installed." },
                            Level = FailureLevel.Warning
                        }
                    }
                }
            };

            SarifLog output = RunAIValidation(log);
            List<Result> results = GetResultsForRule(output, "AI3003");

            results.Should().NotBeEmpty();
        }

        [Fact]
        public void AI3003_WhenExecDescriptorInExecNotifications_NoResult()
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
                            Descriptor = new ReportingDescriptorReference { Id = "AI/EXEC/DECISION" },
                            Message = new Message { Text = "Pivoted to deep triage." },
                            Level = FailureLevel.Note
                        }
                    }
                }
            };

            SarifLog output = RunAIValidation(log);
            List<Result> results = GetResultsForRule(output, "AI3003");

            results.Should().BeEmpty();
        }

        #endregion

        #region AI3005 — ProvideNotificationTimestamp

        [Fact]
        public void AI3005_WhenTimestampMissing_ReportsNote()
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
                            Descriptor = new ReportingDescriptorReference { Id = "AI/EXEC/DECISION" },
                            Message = new Message { Text = "Analysis started." },
                            Level = FailureLevel.Note
                        }
                    }
                }
            };

            SarifLog output = RunAIValidation(log);
            List<Result> results = GetResultsForRule(output, "AI3005");

            results.Should().NotBeEmpty();
            results[0].Level.Should().Be(FailureLevel.Note);
        }

        [Fact]
        public void AI3005_WhenTimestampPresent_NoResult()
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
                            Descriptor = new ReportingDescriptorReference { Id = "AI/EXEC/DECISION" },
                            Message = new Message { Text = "Analysis started." },
                            Level = FailureLevel.Note,
                            TimeUtc = System.DateTime.UtcNow
                        }
                    }
                }
            };

            SarifLog output = RunAIValidation(log);
            List<Result> results = GetResultsForRule(output, "AI3005");

            results.Should().BeEmpty();
        }

        #endregion
    }
}
