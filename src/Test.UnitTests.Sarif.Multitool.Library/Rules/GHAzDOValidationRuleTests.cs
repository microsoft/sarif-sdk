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

        private static SarifLog RunGHAzDOValidation(SarifLog inputLog, List<RuleKind> ruleKinds = null)
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
                    RuleKindOption = ruleKinds ?? new List<RuleKind> { RuleKind.GHAzDO },
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

        private static SarifLog RunLocationValidation(
            Location location,
            Artifact[] artifacts = null,
            List<RuleKind> ruleKinds = null)
        {
            SarifLog log = CreateValidGHAzDOSarifLog();
            log.Runs[0].Artifacts = artifacts;
            log.Runs[0].Results.Add(new Result
            {
                RuleId = "RULE001",
                Message = new Message { Text = "Example finding." },
                Locations = new[] { location }
            });

            SarifLog output = RunGHAzDOValidation(log, ruleKinds);
            output.Runs[0].Results.Should().NotContain(
                r => r.RuleId.StartsWith("JSON", System.StringComparison.Ordinal),
                "the location regression fixtures must be schema-valid");
            return output;
        }

        [Fact]
        public void GHAzDO1016_WhenPhysicalLocationMissing_ReportsError()
            => AssertMissingPhysicalLocation(
                new List<RuleKind> { RuleKind.GHAzDO },
                "GHAzDO1016",
                "GHAzDO (GitHub Advanced Security for Azure DevOps)");

        [Fact]
        public void GHAzDO1016_WhenSelectedWithSarifAndAIAndPhysicalLocationMissing_ReportsError()
            => AssertMissingPhysicalLocation(
                new List<RuleKind> { RuleKind.Sarif, RuleKind.AI, RuleKind.GHAzDO },
                "GHAzDO1016",
                "GHAzDO (GitHub Advanced Security for Azure DevOps)");

        [Fact]
        public void GH1016_WhenPhysicalLocationMissing_ReportsError()
            => AssertMissingPhysicalLocation(
                new List<RuleKind> { RuleKind.Ghas },
                "GH1016",
                "GitHub Advanced Security");

        private static void AssertMissingPhysicalLocation(List<RuleKind> ruleKinds, string ruleId, string serviceName)
        {
            SarifLog output = RunLocationValidation(new Location(), ruleKinds: ruleKinds);

            Result result = ResultsFor(output, ruleId).Should().ContainSingle().Which;
            result.Level.Should().Be(FailureLevel.Error);
            result.Message.Id.Should().Be("Error_MissingPhysicalLocation");
            result.Message.Arguments.Should().Equal("runs[0].results[0].locations[0]", serviceName);
            result.GetMessageText(result.GetRule(output.Runs[0])).Should().Be(
                $"runs[0].results[0].locations[0]: This 'location' object does not provide a 'physicalLocation' object. This property is required by the {serviceName} service.");
        }

        [Fact]
        public void GHAzDO1016_WhenPhysicalLocationHasUri_NoResult()
            => AssertValidPhysicalLocation("GHAzDO1016", useArtifactIndex: false);

        [Fact]
        public void GHAzDO1016_WhenPhysicalLocationUsesArtifactIndex_NoResult()
            => AssertValidPhysicalLocation("GHAzDO1016", useArtifactIndex: true);

        private static void AssertValidPhysicalLocation(string ruleId, bool useArtifactIndex)
        {
            var artifactLocation = new ArtifactLocation
            {
                Uri = new System.Uri("src/example.cs", System.UriKind.Relative)
            };
            SarifLog output = RunLocationValidation(
                new Location
                {
                    PhysicalLocation = new PhysicalLocation
                    {
                        ArtifactLocation = useArtifactIndex ? new ArtifactLocation { Index = 0 } : artifactLocation,
                        Region = new Region { StartLine = 1 }
                    }
                },
                new[] { new Artifact { Location = artifactLocation } });

            ResultsFor(output, ruleId).Should().BeEmpty();
        }

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
        public void GHAzDO1019_WhenBuildDefinitionIdIsZero_ReportsError()
            => AssertBuildDefinitionId("0", expectError: true);

        [Fact]
        public void GHAzDO1019_WhenBuildDefinitionIdIsNegativeOneSentinel_NoResult()
            // -1 is ADO's sentinel for a run not associated with a saved definition.
            => AssertBuildDefinitionId("-1", expectError: false);

        [Fact]
        public void GHAzDO1019_WhenBuildDefinitionIdIsNegativeNonSentinel_ReportsError()
            => AssertBuildDefinitionId("-2", expectError: true);

        [Fact]
        public void GHAzDO1019_WhenBuildDefinitionIdNotInteger_ReportsError()
            => AssertBuildDefinitionId("not-an-int", expectError: true);

        private static void AssertBuildDefinitionId(string buildDefinitionId, bool expectError)
        {
            SarifLog log = CreateValidGHAzDOSarifLog();
            log.Runs[0].AutomationDetails.SetProperty(
                GHAzDOProvidePipelineProperties.BuildDefinitionIdKey, buildDefinitionId);

            SarifLog output = RunGHAzDOValidation(log);
            List<Result> results = ResultsFor(output, "GHAzDO1019");

            if (expectError)
            {
                results.Should().ContainSingle()
                    .Which.Level.Should().Be(FailureLevel.Error);
            }
            else
            {
                results.Should().BeEmpty();
            }
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

        [Fact]
        public void GHAzDO1021_WhenProvenanceIsValid_NoResult()
        {
            SarifLog log = CreateValidGHAzDOSarifLog();

            SarifLog output = RunGHAzDOValidation(log);

            ResultsFor(output, "GHAzDO1021").Should().BeEmpty();
        }

        [Fact]
        public void GHAzDO1021_WhenBranchIsBlank_ReportsError()
        {
            SarifLog log = CreateValidGHAzDOSarifLog();
            log.Runs[0].VersionControlProvenance[0].Branch = " ";

            SarifLog output = RunGHAzDOValidation(log);

            ResultsFor(output, "GHAzDO1021").Should().ContainSingle()
                .Which.Level.Should().Be(FailureLevel.Error);
        }

        [Fact]
        public void GHAzDO1021_WhenRevisionIdIsBlank_ReportsError()
        {
            SarifLog log = CreateValidGHAzDOSarifLog();
            log.Runs[0].VersionControlProvenance[0].RevisionId = " ";

            SarifLog output = RunGHAzDOValidation(log);

            ResultsFor(output, "GHAzDO1021").Should().ContainSingle()
                .Which.Level.Should().Be(FailureLevel.Error);
        }

        [Fact]
        public void GHAzDO1021_WhenRevisionIdIsAllZeroes_ReportsError()
        {
            SarifLog log = CreateValidGHAzDOSarifLog();
            log.Runs[0].VersionControlProvenance[0].Branch = "refs/heads/unknown";
            log.Runs[0].VersionControlProvenance[0].RevisionId =
                GHAzDOProvideValidVersionControlProvenance.ZeroGitRevision;

            SarifLog output = RunGHAzDOValidation(log);

            ResultsFor(output, "GHAzDO1021").Should().ContainSingle()
                .Which.Level.Should().Be(FailureLevel.Error);
        }

        [Fact]
        public void GHAzDO1021_WhenBranchIsNamedUnknownWithRealRevision_NoResult()
        {
            SarifLog log = CreateValidGHAzDOSarifLog();
            log.Runs[0].VersionControlProvenance[0].Branch = "refs/heads/unknown";

            SarifLog output = RunGHAzDOValidation(log);

            ResultsFor(output, "GHAzDO1021").Should().BeEmpty();
        }
    }
}
