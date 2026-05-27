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
    public class GHAzDOValidationRuleTests
    {
        private const string GoodAutomationId =
            "azuredevops/pipeline/build/myorg/myproj/1/1be018db-a340-454c-8c30-e6880b238463/refs/heads/main/1";

        private const string GoodPhaseGuid = "1be018db-a340-454c-8c30-e6880b238463";

        private static SarifLog CreateValidGHAzDOSarifLog()
        {
            var log = new SarifLog
            {
                Runs = new[]
                {
                    new Run
                    {
                        Tool = new Tool
                        {
                            Driver = new ToolComponent
                            {
                                Name = "TestGHAzDOScanner",
                                Version = "1.0.0",
                                SemanticVersion = "1.0.0",
                                InformationUri = new System.Uri("https://example.com/"),
                                Rules = new[]
                                {
                                    new ReportingDescriptor
                                    {
                                        Id = "RULE001",
                                        Name = "TestRule",
                                        ShortDescription = new MultiformatMessageString { Text = "Test rule." }
                                    }
                                }
                            }
                        },
                        AutomationDetails = new RunAutomationDetails
                        {
                            Guid = System.Guid.Parse("c8cc1757-7f24-48aa-ace6-625dc6f1024c"),
                            CorrelationGuid = System.Guid.Parse("af58d715-688f-4706-8016-18986354feb2"),
                            Id = GoodAutomationId
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
                        Results = new List<Result>()
                    }
                }
            };

            RunAutomationDetails ad = log.Runs[0].AutomationDetails;
            ad.SetProperty(GHAzDOProvidePipelineProperties.BuildDefinitionIdKey, "1");
            ad.SetProperty(GHAzDOProvidePipelineProperties.BuildDefinitionNameKey, "TestBuildDef");
            ad.SetProperty(GHAzDOProvidePipelineProperties.PhaseIdKey, GoodPhaseGuid);
            ad.SetProperty(GHAzDOProvidePipelineProperties.PhaseNameKey, "scan");

            return log;
        }

        private static SarifLog RunGHAzDOValidation(SarifLog inputLog)
        {
            string inputPath = Path.GetTempFileName() + ".sarif";
            string outputPath = Path.GetTempFileName() + ".sarif";

            try
            {
                inputLog.Save(inputPath);

                var options = new ValidateOptions
                {
                    TargetFileSpecifiers = new[] { inputPath },
                    OutputFilePath = outputPath,
                    OutputFileOptions = new[] { FilePersistenceOptions.ForceOverwrite },
                    RuleKindOption = new List<RuleKind> { RuleKind.GHAzDO },
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

        private static List<Result> ResultsFor(SarifLog output, string ruleId)
            => output.Runs[0].Results.Where(r => r.RuleId == ruleId).ToList();

        [Fact]
        public void GHAzDO1019_WhenAllPipelinePropertiesValid_NoResult()
        {
            SarifLog log = CreateValidGHAzDOSarifLog();

            SarifLog output = RunGHAzDOValidation(log);

            ResultsFor(output, "GHAzDO1019").Should().BeEmpty();
        }

        [Fact]
        public void GHAzDO1019_WhenAutomationDetailsNull_NoResult()
        {
            SarifLog log = CreateValidGHAzDOSarifLog();
            log.Runs[0].AutomationDetails = null;

            SarifLog output = RunGHAzDOValidation(log);

            // GHAzDO1014 owns the missing-AutomationDetails case; 1019 must not double-fire.
            ResultsFor(output, "GHAzDO1019").Should().BeEmpty();
        }

        [Fact]
        public void GHAzDO1019_WhenAllPipelinePropertiesMissing_ReportsFourErrors()
        {
            SarifLog log = CreateValidGHAzDOSarifLog();
            // Wipe the property bag — none of the four pipeline keys are set.
            log.Runs[0].AutomationDetails = new RunAutomationDetails
            {
                Guid = log.Runs[0].AutomationDetails.Guid,
                Id = log.Runs[0].AutomationDetails.Id
            };

            SarifLog output = RunGHAzDOValidation(log);
            List<Result> results = ResultsFor(output, "GHAzDO1019");

            results.Should().HaveCount(4);
            results.Should().OnlyContain(r => r.Level == FailureLevel.Error);
        }

        [Fact]
        public void GHAzDO1019_WhenBuildDefinitionIdNotPositiveInteger_ReportsError()
        {
            SarifLog log = CreateValidGHAzDOSarifLog();
            log.Runs[0].AutomationDetails.SetProperty(
                GHAzDOProvidePipelineProperties.BuildDefinitionIdKey, "0");

            SarifLog output = RunGHAzDOValidation(log);

            ResultsFor(output, "GHAzDO1019").Should().ContainSingle()
                .Which.Level.Should().Be(FailureLevel.Error);
        }

        [Fact]
        public void GHAzDO1019_WhenPhaseIdIsEmptyGuid_ReportsError()
        {
            SarifLog log = CreateValidGHAzDOSarifLog();
            log.Runs[0].AutomationDetails.SetProperty(
                GHAzDOProvidePipelineProperties.PhaseIdKey,
                "00000000-0000-0000-0000-000000000000");

            SarifLog output = RunGHAzDOValidation(log);

            ResultsFor(output, "GHAzDO1019").Should().ContainSingle()
                .Which.Level.Should().Be(FailureLevel.Error);
        }

        [Fact]
        public void GHAzDO1020_WhenIdHasCanonicalPrefix_NoResult()
        {
            SarifLog log = CreateValidGHAzDOSarifLog();

            SarifLog output = RunGHAzDOValidation(log);

            ResultsFor(output, "GHAzDO1020").Should().BeEmpty();
        }

        [Fact]
        public void GHAzDO1020_WhenIdHasWrongPrefix_ReportsError()
        {
            SarifLog log = CreateValidGHAzDOSarifLog();
            log.Runs[0].AutomationDetails.Id = "github/actions/run/12345";

            SarifLog output = RunGHAzDOValidation(log);

            ResultsFor(output, "GHAzDO1020").Should().ContainSingle()
                .Which.Level.Should().Be(FailureLevel.Error);
        }
    }
}
