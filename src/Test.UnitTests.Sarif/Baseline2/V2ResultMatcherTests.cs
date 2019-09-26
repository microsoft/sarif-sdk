// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.Baseline;
using Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching;
using Microsoft.CodeAnalysis.Test.Utilities.Sarif;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.CodeAnalysis.Test.UnitTests.Sarif.Baseline
{
    public class V2ResultMatcherTests
    {
        private const string SampleFilePath = "elfie-arriba.sarif";
        private Run SampleRun { get; }

        private const string SuppressionTestPreviousFilePath = "SuppressionTestPrevious.sarif";
        private SarifLog SuppressionTestPreviousLog { get; }

        private const string SuppressionTestCurrentFilePath = "SuppressionTestCurrent.sarif";
        private SarifLog SuppressionTestCurrentLog { get; }

        private static readonly IResultMatcher matcher = new V2ResultMatcher();
        private static readonly ResourceExtractor extractor = new ResourceExtractor(typeof(V2ResultMatcherTests));

        public V2ResultMatcherTests()
        {
            SampleRun = GetLogFromResource(SampleFilePath).Runs[0];

            SuppressionTestPreviousLog = GetLogFromResource(SuppressionTestPreviousFilePath);
            SuppressionTestCurrentLog = GetLogFromResource(SuppressionTestCurrentFilePath);
        }

        private static SarifLog GetLogFromResource(string filePath)
        {
            string fileContents = extractor.GetResourceText(filePath);
            return JsonConvert.DeserializeObject<SarifLog>(fileContents);
        }

        private static IEnumerable<MatchedResults> CreateMatchedResults(Run previous, Run current)
        {
            return matcher.Match(
                previous.Results.Select(r => new ExtractedResult(r, previous)).ToList(),
                current.Results.Select(r => new ExtractedResult(r, current)).ToList()
            );
        }

        private static Run CreateMatchedRun(SarifLog previousLog, SarifLog currentLog)
        {
            ISarifLogMatcher baseliner = ResultMatchingBaselinerFactory.GetDefaultResultMatchingBaseliner();
            SarifLog matchedLog = baseliner.Match(new[] { previousLog }, new[] { currentLog }).Single();
            return matchedLog.Runs[0];
        }

        [Fact]
        public void V2ResultMatcher_Identical()
        {
            IEnumerable<MatchedResults> matches = CreateMatchedResults(SampleRun, SampleRun);
            matches.Where(m => m.PreviousResult == null || m.CurrentResult == null).Should().BeEmpty();
        }

        [Fact]
        public void V2ResultMatcher_MoveWithinFile()
        {
            Run newRun = SampleRun.DeepClone();
            newRun.Results[2].Locations[0].PhysicalLocation.Region.StartLine += 1;

            IEnumerable<MatchedResults> matches = CreateMatchedResults(SampleRun, newRun);
            matches.Where(m => m.PreviousResult == null || m.CurrentResult == null).Should().BeEmpty();
        }

        [Fact]
        public void V2ResultMatcher_RenameFile()
        {
            Run newRun = SampleRun.DeepClone();
            newRun.Results[2].Locations[0].PhysicalLocation.ArtifactLocation.Index = -1;
            newRun.Results[2].Locations[0].PhysicalLocation.ArtifactLocation.Uri = new Uri("file:///C:/Code/elfie-arriba/XForm/XForm.Web/node_modules/public-encrypt/test/test_rsa_privkey_NEW.pem");

            IEnumerable<MatchedResults> matches = CreateMatchedResults(SampleRun, newRun);
            matches.Where(m => m.PreviousResult == null || m.CurrentResult == null).Should().BeEmpty();
        }

        [Fact]
        public void V2ResultMatcher_AddResult()
        {
            Run newRun = SampleRun.DeepClone();
            Result newResult = newRun.Results[0].DeepClone();
            newResult.Locations[0].PhysicalLocation.ArtifactLocation.Uri = new Uri("file:///C:/Code/elfie-arriba/XForm/XForm.Web/node_modules/NEW.pem");
            newResult.PartialFingerprints = null;
            newResult.Properties = null;

            newRun.Results.Add(newResult);

            IEnumerable<MatchedResults> matches = CreateMatchedResults(SampleRun, newRun);
            matches.Where(m => m.PreviousResult != null && m.CurrentResult != null).Should().HaveCount(5);
            matches.Where(m => m.PreviousResult == null || m.CurrentResult == null).Should().HaveCount(1);

            MatchedResults nonMatch = matches.Where(m => m.PreviousResult == null || m.CurrentResult == null).First();
            newResult.Should().BeSameAs(nonMatch.CurrentResult.Result);
        }

        [Fact]
        public void V2ResultMatcher_RemoveResult()
        {
            Run newRun = SampleRun.DeepClone();
            newRun.Results.RemoveAt(2);

            IEnumerable<MatchedResults> matches = CreateMatchedResults(SampleRun, newRun);
            matches.Where(m => m.PreviousResult != null && m.CurrentResult != null).Should().HaveCount(4);
            matches.Where(m => m.PreviousResult == null || m.CurrentResult == null).Should().HaveCount(1);

            MatchedResults nonMatch = matches.Where(m => m.PreviousResult == null || m.CurrentResult == null).First();
            SampleRun.Results[2].Should().BeSameAs(nonMatch.PreviousResult.Result);
        }

        [Fact]
        public void V2ResultMatcher_RemovedAndAdded()
        {
            Run newRun = SampleRun.DeepClone();
            newRun.Results.RemoveAt(2);

            Result newResult = newRun.Results[0].DeepClone();
            newResult.Locations[0].PhysicalLocation.ArtifactLocation.Uri = new Uri("file:///C:/Code/elfie-arriba/XForm/XForm.Web/node_modules/NEW.pem");
            newResult.PartialFingerprints = null;
            newResult.Message.Text = "Different Message";
            newRun.Results.Add(newResult);

            IEnumerable<MatchedResults> matches = CreateMatchedResults(SampleRun, newRun);
            matches.Where(m => m.PreviousResult != null && m.CurrentResult != null).Should().HaveCount(4);
            matches.Where(m => m.PreviousResult == null || m.CurrentResult == null).Should().HaveCount(2);

            MatchedResults removed = matches.Where(m => m.CurrentResult == null).First();
            SampleRun.Results[2].Should().BeSameAs(removed.PreviousResult.Result);

            MatchedResults added = matches.Where(m => m.PreviousResult == null).First();
            newResult.Should().BeSameAs(added.CurrentResult.Result);
        }

        [Fact]
        public void V2ResultMatcher_TwoAdded()
        {
            Run newRun = SampleRun.DeepClone();

            Result newResult = newRun.Results[0].DeepClone();
            newResult.Locations[0].PhysicalLocation.ArtifactLocation.Uri = new Uri("file:///C:/Code/elfie-arriba/XForm/XForm.Web/node_modules/NEW.pem");
            newResult.Message.Text = "Different Message";
            newRun.Results.Add(newResult);

            Result newResult2 = newRun.Results[2].DeepClone();
            newResult.Locations[0].PhysicalLocation.ArtifactLocation.Uri = new Uri("file:///C:/Code/elfie-arriba/XForm/XForm.Web/node_modules/NEW_2.pem");
            newResult.Message.Text = "Different Message";
            newRun.Results.Add(newResult);

            IEnumerable<MatchedResults> matches = CreateMatchedResults(SampleRun, newRun);
            matches.Where(m => m.PreviousResult != null && m.CurrentResult != null).Should().HaveCount(5);
            matches.Where(m => m.PreviousResult == null || m.CurrentResult == null).Should().HaveCount(2);
        }

        [Fact]
        public void V2ResultMatcher_WhenNewIssueIsSuppressed_SupressesTheResultInTheOutput()
        {
            Run matchedRun = CreateMatchedRun(SuppressionTestPreviousLog, SuppressionTestCurrentLog);

            Result newSuppressedResult = matchedRun.Results.Single(r => r.Message.Text == "New suppressed result.");

            newSuppressedResult.Suppressions.Count.Should().Be(1);
        }

        [Fact]
        public void V2ResultMatcher_WhenExistingUnsuppressedIssueIsNewlySuppressed_SupressesTheResultInTheOutput()
        {
            Run matchedRun = CreateMatchedRun(SuppressionTestPreviousLog, SuppressionTestCurrentLog);

            Result existingResultNewlySuppressed = matchedRun.Results.Single(r => r.Message.Text == "Existing, originally unsuppressed result.");

            existingResultNewlySuppressed.Suppressions.Count.Should().Be(1);
        }

        [Fact]
        public void V2ResultMatcher_WhenIssueIsSuppressedInBothRuns_SupressesTheResultInTheOutput()
        {
            Run matchedRun = CreateMatchedRun(SuppressionTestPreviousLog, SuppressionTestCurrentLog);

            Result existingResultNewlySuppressed = matchedRun.Results.Single(r => r.Message.Text == "Result suppressed in both runs.");

            existingResultNewlySuppressed.Suppressions.Count.Should().Be(1);
        }

        [Fact]
        public void V2ResultMatcher_WhenExistingSuppressedIssueIsNewlyUnsuppressed_DoesNotSupressTheResultInTheOutput()
        {
            Run matchedRun = CreateMatchedRun(SuppressionTestPreviousLog, SuppressionTestCurrentLog);

            Result existingResultNewlyUnsuppressed = matchedRun.Results.Single(r => r.Message.Text == "Existing, originally suppressed result.");

            existingResultNewlyUnsuppressed.Suppressions.Should().BeNull();
        }

        /// <summary>
        /// Regression test for https://github.com/microsoft/sarif-sdk/issues/1684.
        /// </summary>
        [Fact]
        public void ResultMatchingBaseliner_WhenThereIsOnlyOneCurrentRun_CopiesSelectedRunData()
        {
            Run originalRun = new Run
            {
                Tool = new Tool
                {
                    Driver = new ToolComponent
                    {
                        Name = TestConstants.ToolName,
                        Rules = new ReportingDescriptor[]
                        {
                            new ReportingDescriptor { Id = TestConstants.RuleIds.Rule1 },
                            new ReportingDescriptor { Id = TestConstants.RuleIds.Rule2 },
                        }
                    }
                },
                AutomationDetails = new RunAutomationDetails
                {
                    Guid = TestConstants.AutomationDetailsGuid
                },
                Results = new Result[]
                {
                    new Result
                    {
                        RuleId = TestConstants.RuleIds.Rule1,
                        RuleIndex = 0
                    },
                    new Result
                    {
                        RuleId = TestConstants.RuleIds.Rule2,
                        RuleIndex = 1
                    }
                },
                Conversion = new Conversion
                {
                    Tool = new Tool
                    {
                        Driver = new ToolComponent
                        {
                            Name = TestConstants.ConverterName
                        }
                    }
                },
                Taxonomies = new ToolComponent[]
                {
                    new ToolComponent
                    {
                        Name = TestConstants.TaxonomyName,
                        Taxa = new ReportingDescriptor[]
                        {
                            new ReportingDescriptor { Id = TestConstants.TaxonIds.Taxon1 },
                            new ReportingDescriptor { Id = TestConstants.TaxonIds.Taxon2 },
                        }
                    }
                },
                Policies = new ToolComponent[]
                {
                    new ToolComponent
                    {
                        Name = TestConstants.PolicyName,
                        Rules = new ReportingDescriptor[]
                        {
                            new ReportingDescriptor
                            {
                                Id = TestConstants.RuleIds.Rule2,
                                DefaultConfiguration = new ReportingConfiguration { Level = FailureLevel.Error }
                            }
                        }
                    }
                },
                Translations = new ToolComponent[]
                {
                    new ToolComponent
                    {
                        Name = TestConstants.TranslationName,
                        TranslationMetadata = new TranslationMetadata
                        {
                            Name = TestConstants.TranslationMetadataName
                        }
                    }
                },
                Language = TestConstants.LanguageIdentifier,
                RedactionTokens = new string[]
                {
                    SarifConstants.RedactedMarker
                }
            };

            var sarifLog = new SarifLog
            {
                Version = SarifVersion.Current,
                Runs = new Run[] { originalRun }
            };

            Run matchedRun = CreateMatchedRun(previousLog: sarifLog, currentLog: sarifLog);

            matchedRun.AutomationDetails.ValueEquals(originalRun.AutomationDetails).Should().BeTrue();

            matchedRun.Conversion.ValueEquals(originalRun.Conversion).Should().BeTrue();

            matchedRun.Taxonomies.Count.Should().Be(originalRun.Taxonomies.Count);
            for (int i = 0; i < originalRun.Taxonomies.Count; ++i)
            {
                matchedRun.Taxonomies[i].ValueEquals(originalRun.Taxonomies[i]).Should().BeTrue();
            }

            matchedRun.Translations.Count.Should().Be(originalRun.Translations.Count);
            for (int i = 0; i < originalRun.Translations.Count; ++i)
            {
                matchedRun.Translations[i].ValueEquals(originalRun.Translations[i]).Should().BeTrue();
            }

            matchedRun.Policies.Count.Should().Be(originalRun.Policies.Count);
            for (int i = 0; i < originalRun.Policies.Count; ++i)
            {
                matchedRun.Policies[i].ValueEquals(originalRun.Policies[i]).Should().BeTrue();
            }

            matchedRun.Language.Should().Be(originalRun.Language);
            matchedRun.RedactionTokens.Count.Should().Be(originalRun.RedactionTokens.Count);
            for (int i = 0; i < originalRun.RedactionTokens.Count; ++i)
            {
                matchedRun.RedactionTokens[i].Should().Be(originalRun.RedactionTokens[i]);
            }

            matchedRun.Results.Count.Should().Be(originalRun.Results.Count);
            for (int i = 0; i < originalRun.Results.Count; ++i)
            {
                // The Result objects won't be identical because the results in the matched run
                // will have their baseline state set, and they have a "ResultMatching" property
                // bag property.
                matchedRun.Results[i].BaselineState.Should().Be(BaselineState.Unchanged);
                matchedRun.Results[i].Properties.Should().ContainKey("ResultMatching");

                // But aside from that they should be the same:
                matchedRun.Results[i].BaselineState = originalRun.Results[i].BaselineState;
                matchedRun.Results[i].Properties = originalRun.Results[i].Properties;

                matchedRun.Results[i].ValueEquals(originalRun.Results[i]).Should().BeTrue();
            }
        }
    }
}
