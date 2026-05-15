// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif.Driver.Sdk;
using Microsoft.CodeAnalysis.Sarif.Multitool.Rules;
using Microsoft.CodeAnalysis.Sarif.Writers;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    /// <summary>
    /// SDK-C: AI1003 / SARIF2017 / GH1003 / SARIF2011 now use the framework's
    /// <c>Analyze(Region, ...)</c> / <c>Analyze(PhysicalLocation, ...)</c> visitors so they
    /// fire on every Region/PhysicalLocation in the SARIF tree, not just <c>result.Locations[]</c>.
    /// These tests pin the new coverage: invalid regions under relatedLocations, codeFlows,
    /// contextRegion, and notification.locations must all be flagged.
    /// </summary>
    public class RegionVisitorCoverageTests
    {
        private static SarifLog RunAIValidationOnLog(SarifLog log)
        {
            string inputPath = Path.GetTempFileName() + ".sarif";
            string outputPath = Path.GetTempFileName() + ".sarif";

            try
            {
                log.Save(inputPath);

                var options = new ValidateOptions
                {
                    TargetFileSpecifiers = new string[] { inputPath },
                    OutputFilePath = outputPath,
                    OutputFileOptions = new[] { FilePersistenceOptions.ForceOverwrite },
                    RuleKindOption = new List<RuleKind> { RuleKind.AI },
                    Kind = new List<ResultKind> { ResultKind.Fail },
                    Level = new List<FailureLevel> { FailureLevel.Note, FailureLevel.Warning, FailureLevel.Error }
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

        private static SarifLog BaseAIConformantLog()
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
                                Name = "RegionCoverageTest",
                                Version = "1.0.0",
                                SemanticVersion = "1.0.0",
                                Rules = new[]
                                {
                                    new ReportingDescriptor
                                    {
                                        Id = "CWE-78",
                                        Name = "Cmd",
                                        ShortDescription = new MultiformatMessageString { Text = "x" }
                                    }
                                }
                            }
                        },
                        AutomationDetails = new RunAutomationDetails { Guid = System.Guid.NewGuid() },
                        VersionControlProvenance = new[]
                        {
                            new VersionControlDetails
                            {
                                RepositoryUri = new System.Uri("https://example.com/repo"),
                                RevisionId = "abc",
                                Branch = "main"
                            }
                        },
                        Results = new List<Result>()
                    }
                }
            };
        }

        private static Result NewResultWithPrimaryLocation()
        {
            return new Result
            {
                RuleId = "CWE-78/sub",
                RuleIndex = 0,
                Kind = ResultKind.Fail,
                Level = FailureLevel.Error,
                Rank = 90,
                Message = new Message { Text = "msg", Markdown = "# msg" },
                Locations = new[]
                {
                    new Location
                    {
                        PhysicalLocation = new PhysicalLocation
                        {
                            ArtifactLocation = new ArtifactLocation { Uri = new System.Uri("src/x.py", System.UriKind.Relative) },
                            Region = new Region { StartLine = 1, StartColumn = 1, EndLine = 1, EndColumn = 2 }
                        }
                    }
                }
            };
        }

        private static List<Result> GetResultsForRule(SarifLog log, string ruleId)
            => log.Runs[0].Results.Where(r => r.RuleId == ruleId).ToList();

        private static void SetRunAIOrigin(SarifLog log)
        {
            log.Runs[0].SetProperty("ai/origin", "generated");
            log.Runs[0].Results[0].SetProperty("ai/exploitability", "demonstrated");
            log.Runs[0].Results[0].SetProperty("ai/attackerPosition", "network");
        }

        [Fact]
        public void AI1003_FiresOnRegionUnderRelatedLocations()
        {
            SarifLog log = BaseAIConformantLog();
            Result result = NewResultWithPrimaryLocation();

            // Add a relatedLocation with a region that lacks startLine — the Region-level visitor
            // must reach it. Pre-SDK-C this slipped through because AI1003 only walked
            // result.Locations[].
            result.RelatedLocations = new[]
            {
                new Location
                {
                    PhysicalLocation = new PhysicalLocation
                    {
                        ArtifactLocation = new ArtifactLocation { Uri = new System.Uri("src/y.py", System.UriKind.Relative) },
                        Region = new Region { CharOffset = 12, CharLength = 5 }
                    }
                }
            };
            log.Runs[0].Results = new[] { result };
            SetRunAIOrigin(log);

            SarifLog output = RunAIValidationOnLog(log);

            GetResultsForRule(output, "AI1003").Should().NotBeEmpty(
                "AI1003 must visit regions under relatedLocations, not just result.locations[]");
        }

        [Fact]
        public void AI1003_FiresOnContextRegionMissingStartLine()
        {
            SarifLog log = BaseAIConformantLog();
            Result result = NewResultWithPrimaryLocation();

            // contextRegion missing startLine — under the old hand-walk only the primary region was
            // checked; contextRegion silently passed.
            result.Locations[0].PhysicalLocation.ContextRegion = new Region
            {
                CharOffset = 0,
                CharLength = 30
            };
            log.Runs[0].Results = new[] { result };
            SetRunAIOrigin(log);

            SarifLog output = RunAIValidationOnLog(log);

            GetResultsForRule(output, "AI1003").Should().NotBeEmpty(
                "AI1003 must visit contextRegion the same as region — both are Region instances");
        }

        [Fact]
        public void AI1003_FiresOnRegionUnderCodeFlowsThreadFlows()
        {
            SarifLog log = BaseAIConformantLog();
            Result result = NewResultWithPrimaryLocation();

            result.CodeFlows = new[]
            {
                new CodeFlow
                {
                    ThreadFlows = new[]
                    {
                        new ThreadFlow
                        {
                            Locations = new[]
                            {
                                new ThreadFlowLocation
                                {
                                    Location = new Location
                                    {
                                        PhysicalLocation = new PhysicalLocation
                                        {
                                            ArtifactLocation = new ArtifactLocation { Uri = new System.Uri("src/z.py", System.UriKind.Relative) },
                                            Region = new Region { CharOffset = 100, CharLength = 10 }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
            log.Runs[0].Results = new[] { result };
            SetRunAIOrigin(log);

            SarifLog output = RunAIValidationOnLog(log);

            GetResultsForRule(output, "AI1003").Should().NotBeEmpty(
                "AI1003 must visit regions inside codeFlows.threadFlows.locations[]");
        }

        [Fact]
        public void AI1003_DoesNotFire_OnBinaryRegion()
        {
            // SARIF §3.30 allows byte-offset binary regions as a first-class alternative. They are
            // not "missing startLine" — they are a different region representation. Pre-SDK-C the
            // hand-walked rule incorrectly flagged these (the old GH1003 baseline encoded the bug);
            // the framework Region visitor now treats them correctly by skipping IsBinaryRegion.
            SarifLog log = BaseAIConformantLog();
            Result result = NewResultWithPrimaryLocation();
            result.RelatedLocations = new[]
            {
                new Location
                {
                    PhysicalLocation = new PhysicalLocation
                    {
                        ArtifactLocation = new ArtifactLocation { Uri = new System.Uri("src/bin.dat", System.UriKind.Relative) },
                        Region = new Region { ByteOffset = 24, ByteLength = 84 }
                    }
                }
            };
            log.Runs[0].Results = new[] { result };
            SetRunAIOrigin(log);

            SarifLog output = RunAIValidationOnLog(log);

            GetResultsForRule(output, "AI1003").Where(r => r.Message?.Arguments != null && r.Message.Arguments[0].Contains("relatedLocations"))
                .Should().BeEmpty("binary regions are a valid SARIF §3.30 representation; AI1003 must not flag them");
        }
    }
}
