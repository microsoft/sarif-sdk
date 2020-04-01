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
        public void V2ResultMatcher_MessageContainsLineNumbers()
        {
            // Messages have line and column numbers removed before comparison, so results will still be "sufficiently similar"
            // if the message references them and the results move to a different line.

            Run firstRun = SampleRun.DeepClone();

            Result result = firstRun.Results[2];
            result.PartialFingerprints = null;
            result.Message = new Message() { Text = $"Found issue on line {result.Locations[0].PhysicalLocation.Region.StartLine}" };

            Run secondRun = firstRun.DeepClone();
            result = secondRun.Results[2];
            result.Locations[0].PhysicalLocation.Region.StartLine += 5;
            result.Message = new Message() { Text = $"Found issue on line {result.Locations[0].PhysicalLocation.Region.StartLine}" };

            IEnumerable<MatchedResults> matches = CreateMatchedResults(firstRun, secondRun);
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
        public void V2ResultMatcher_TwoRulesSameLine()
        {
            // Verify that two Results with identical locations but different RuleIDs sort in a consistent order
            // so that they will be matched with each other.

            // COMPLEX: Getting to the "sufficiently similar" check requires that they don't match beforehand due to:
            //  - Identical where, so the results must move
            //  - Unique identical what, so there must be other unmatched results which share every trait with the ones we're checking

            Run firstRun = SampleRun.DeepClone();
            int countBeforeAdd = firstRun.Results.Count;

            // Copy the first Result and change the Rule only (they'll have same Message, Fingerprints, Location)
            firstRun.Results[1] = firstRun.Results[0].DeepClone();
            firstRun.Results[1].RuleId = "NewRuleId";

            // Make another copy of each result and move them so that the results won't have any per-rule unique traits
            firstRun.Results.Add(firstRun.Results[0].DeepClone());
            firstRun.Results.Add(firstRun.Results[1].DeepClone());

            firstRun.Results[countBeforeAdd].Locations[0].PhysicalLocation.Region.StartLine += 1;
            firstRun.Results[countBeforeAdd + 1].Locations[0].PhysicalLocation.Region.StartLine += 1;

            Run secondRun = firstRun.DeepClone();

            // Move all Results down
            secondRun.Results[0].Locations[0].PhysicalLocation.Region.StartLine += 4;
            secondRun.Results[1].Locations[0].PhysicalLocation.Region.StartLine += 4;

            secondRun.Results[countBeforeAdd].Locations[0].PhysicalLocation.Region.StartLine += 1;
            secondRun.Results[countBeforeAdd + 1].Locations[0].PhysicalLocation.Region.StartLine += 1;

            // Swap the order of them to reduce the chance they'll sort the same
            Result swap = secondRun.Results[0];
            secondRun.Results[0] = secondRun.Results[1];
            secondRun.Results[1] = swap;

            // Verify all Results match
            IEnumerable<MatchedResults> matches = CreateMatchedResults(firstRun, secondRun);
            matches.Where(m => m.PreviousResult == null || m.CurrentResult == null).Should().BeEmpty();
        }

        [Fact]
        public void V2ResultMatcher_MatchesFirstAndLastInFile()
        {
            // We want to ensure that the first and last Results in a file are compared.
            // To cover this, we need:
            //  - No unique properties in any results in the file.
            //  - The Result file Uri stays the same. (No file rename)
            //  - The Result moves within the file, but is still first/last.
            //  - The Results just before and after (in the file just before and after) change order

            // To do this, we'll change the URIs of /public-encrypt/test/test.pem and test_rsa_privkey_encrypted.pem to /zzz/test
            // and will move the result in /public-encrypt/test/test_rsa_privkey.pem

            Run firstRun = SampleRun.DeepClone();
            Run secondRun = SampleRun.DeepClone();

            // Change Uris for Result[1] and Result[3]
            string changeFrom = "/public-encrypt/";
            string changeTo = "/zzz/";
            ReplaceInUri(secondRun.Artifacts[1].Location, changeFrom, changeTo);
            ReplaceInUri(secondRun.Artifacts[3].Location, changeFrom, changeTo);
            ReplaceInUri(secondRun.Results[1].Locations[0].PhysicalLocation.ArtifactLocation, changeFrom, changeTo);
            ReplaceInUri(secondRun.Results[3].Locations[0].PhysicalLocation.ArtifactLocation, changeFrom, changeTo);

            // Move Result[2]
            Result firstInFile = secondRun.Results[2];
            firstInFile.Locations[0].PhysicalLocation.Region.StartLine += 1;
            firstInFile.Locations[0].PhysicalLocation.Region.CharOffset += 5;

            // Verify the result matched, and matched the copy from the same file
            IEnumerable<MatchedResults> matches = CreateMatchedResults(firstRun, secondRun);
            MatchedResults match = matches.Where(m => Location.ValueComparer.Equals(m.CurrentResult?.Result?.Locations?.FirstOrDefault(), firstInFile.Locations[0])).FirstOrDefault();
            match.PreviousResult.Should().NotBeNull();
            match.PreviousResult.Result.Locations[0].PhysicalLocation.ArtifactLocation.Uri
                .Should().Be(match.CurrentResult.Result.Locations[0].PhysicalLocation.ArtifactLocation.Uri);
        }

        [Fact]
        public void V2ResultMatcher_IgnoresConstantPartialFingerprint()
        {
            // If there's a PartialFingerprint which is useless (the same for every result),
            // the baseliner needs to recognize the situation and disregard that partialFingerprint. 

            // Spam had this issue with CanonicalLogicalLocation in JSON files where
            // all user content is in one place (WorkItemHyperLink -> "url")

            Run firstRun = SampleRun.DeepClone();
            Run secondRun = SampleRun.DeepClone();

            foreach (Result result in firstRun.Results)
            {
                // Give each result a unique (non-matching) and constant fingerprint
                result.PartialFingerprints.Clear();
                result.PartialFingerprints["SecretHash/v1"] = Guid.NewGuid().ToString(SarifConstants.GuidFormat);
                result.PartialFingerprints["Useless/v1"] = "Constant";

                // Give each result a unique (non-matching) snippet (to ensure fallback doesn't match)
                result.Locations[0].PhysicalLocation.Region.Snippet.Text = Guid.NewGuid().ToString(SarifConstants.GuidFormat);
            }

            foreach (Result result in secondRun.Results)
            {
                // Give each result a unique (non-matching) and constant fingerprint
                result.PartialFingerprints.Clear();
                result.PartialFingerprints["SecretHash/v1"] = Guid.NewGuid().ToString(SarifConstants.GuidFormat);
                result.PartialFingerprints["Useless/v1"] = "Constant";

                Region region = result.Locations[0].PhysicalLocation.Region;

                // Give each result a unique (non-matching) snippet (to ensure fallback doesn't match)
                region.Snippet.Text = Guid.NewGuid().ToString(SarifConstants.GuidFormat);

                // Ensure each results moves, so none match by where
                region.StartLine += 100;
            }

            // Match the Runs, and confirm *nothing* matches; this requires the constant partialFingerprint not to be trusted
            IEnumerable<MatchedResults> matches = CreateMatchedResults(firstRun, secondRun);
            matches.Where(m => m.PreviousResult != null && m.CurrentResult != null).Should().BeEmpty();
        }

        private static void ReplaceInUri(ArtifactLocation artifactLocation, string replaceThis, string withThis)
        {
            string original = artifactLocation.Uri.OriginalString;
            string updated = original.Replace(replaceThis, withThis);
            artifactLocation.Uri = new Uri(updated);
        }

        [Fact]
        public void V2ResultMatcher_WhenNewIssueIsSuppressed_SupressesTheResultInTheOutput()
        {
            Run matchedRun = CreateMatchedRun(SuppressionTestPreviousLog, SuppressionTestCurrentLog);

            Result newSuppressedResult = matchedRun.Results.Single(r => r.Message.Text == "New suppressed result.");

            newSuppressedResult.Suppressions.Count.Should().Be(1);
            AssertMatchedRunInvariants(matchedRun);
        }

        [Fact]
        public void V2ResultMatcher_WhenExistingUnsuppressedIssueIsNewlySuppressed_SupressesTheResultInTheOutput()
        {
            Run matchedRun = CreateMatchedRun(SuppressionTestPreviousLog, SuppressionTestCurrentLog);

            Result existingResultNewlySuppressed = matchedRun.Results.Single(r => r.Message.Text == "Existing, originally unsuppressed result.");

            existingResultNewlySuppressed.Suppressions.Count.Should().Be(1);
            AssertMatchedRunInvariants(matchedRun);
        }

        [Fact]
        public void V2ResultMatcher_WhenIssueIsSuppressedInBothRuns_SupressesTheResultInTheOutput()
        {
            Run matchedRun = CreateMatchedRun(SuppressionTestPreviousLog, SuppressionTestCurrentLog);

            Result existingResultNewlySuppressed = matchedRun.Results.Single(r => r.Message.Text == "Result suppressed in both runs.");

            existingResultNewlySuppressed.Suppressions.Count.Should().Be(1);
            AssertMatchedRunInvariants(matchedRun);
        }

        [Fact]
        public void V2ResultMatcher_WhenExistingSuppressedIssueIsNewlyUnsuppressed_DoesNotSupressTheResultInTheOutput()
        {
            Run matchedRun = CreateMatchedRun(SuppressionTestPreviousLog, SuppressionTestCurrentLog);

            Result existingResultNewlyUnsuppressed = matchedRun.Results.Single(r => r.Message.Text == "Existing, originally suppressed result.");

            existingResultNewlyUnsuppressed.Suppressions.Should().BeNull();
            AssertMatchedRunInvariants(matchedRun);
        }

        [Fact]
        public void V2ResultMatcher_RuleLookupAndPorting()
        {
            // Verify all Results have RuleId and RuleIndex set after baselining (this log has RuleId only)
            Run matchedRun = CreateMatchedRun(SuppressionTestPreviousLog, SuppressionTestCurrentLog);
            VerifyResultRules(matchedRun);

            // Change Results to correct RuleIndex only
            SarifLog indexOnlyRun = SuppressionTestCurrentLog.DeepClone();
            SetResultRuleIndexOnly(indexOnlyRun.Runs[0]);

            // Baseline with RuleIndex only and verify RuleId and RuleIndex set after baselining
            matchedRun = CreateMatchedRun(indexOnlyRun, SuppressionTestCurrentLog);
            VerifyResultRules(matchedRun);
        }

        private void VerifyResultRules(Run run)
        {
            Assert.NotNull(run?.Tool?.Driver?.Rules);

            foreach (Result result in run.Results)
            {
                // Verify all Results have had RuleId and RuleIndex set
                Assert.NotNull(result.RuleId);
                Assert.True(result.RuleIndex >= 0);

                // Verify the correct Rule (with matching Id) is at the specified index
                Assert.Equal(result.RuleId, run.Tool.Driver.Rules[result.RuleIndex].Id);
            }
        }

        private void SetResultRuleIndexOnly(Run run)
        {
            Dictionary<string, int> ruleIndexByRuleId = new Dictionary<string, int>();

            for (int i = 0; i < run.Tool.Driver.Rules.Count; ++i)
            {
                ReportingDescriptor rule = run.Tool.Driver.Rules[i];
                ruleIndexByRuleId[rule.Id] = i;
            }

            foreach (Result result in run.Results)
            {
                result.RuleIndex = ruleIndexByRuleId[result.ResolvedRuleId(run)];
                result.RuleId = null;
                result.Rule = null;
            }
        }

        /// <summary>
        /// Regression test for https://github.com/microsoft/sarif-sdk/issues/1684.
        /// </summary>
        [Fact]
        public void ResultMatchingBaseliner_WhenThereIsOnlyOneCurrentRun_CopiesSelectedRunData()
        {
            DateTime firstDetectionTime = new DateTime(2019, 10, 7, 12, 13, 14);

            Run originalRun = new Run
            {
                Tool = new Tool
                {
                    Driver = new ToolComponent
                    {
                        Name = TestData.ToolName,
                        Rules = new ReportingDescriptor[]
                        {
                            new ReportingDescriptor { Id = TestData.RuleIds.Rule1 },
                            new ReportingDescriptor { Id = TestData.RuleIds.Rule2 },
                        }
                    }
                },
                AutomationDetails = new RunAutomationDetails
                {
                    Guid = TestData.AutomationDetailsGuid
                },
                Results = new Result[]
                {
                    new Result
                    {
                        RuleId = TestData.RuleIds.Rule1,
                        RuleIndex = 0,
                        Provenance = new ResultProvenance
                        {
                            FirstDetectionTimeUtc = firstDetectionTime
                        }
                    },
                    new Result
                    {
                        RuleId = TestData.RuleIds.Rule2,
                        RuleIndex = 1,
                        Provenance = new ResultProvenance
                        {
                            FirstDetectionTimeUtc = firstDetectionTime
                        }
                    }
                },
                Conversion = new Conversion
                {
                    Tool = new Tool
                    {
                        Driver = new ToolComponent
                        {
                            Name = TestData.ConverterName
                        }
                    }
                },
                Taxonomies = new ToolComponent[]
                {
                    new ToolComponent
                    {
                        Name = TestData.TaxonomyName,
                        Taxa = new ReportingDescriptor[]
                        {
                            new ReportingDescriptor { Id = TestData.TaxonIds.Taxon1 },
                            new ReportingDescriptor { Id = TestData.TaxonIds.Taxon2 },
                        }
                    }
                },
                Policies = new ToolComponent[]
                {
                    new ToolComponent
                    {
                        Name = TestData.PolicyName,
                        Rules = new ReportingDescriptor[]
                        {
                            new ReportingDescriptor
                            {
                                Id = TestData.RuleIds.Rule2,
                                DefaultConfiguration = new ReportingConfiguration { Level = FailureLevel.Error }
                            }
                        }
                    }
                },
                Translations = new ToolComponent[]
                {
                    new ToolComponent
                    {
                        Name = TestData.TranslationName,
                        TranslationMetadata = new TranslationMetadata
                        {
                            Name = TestData.TranslationMetadataName
                        }
                    }
                },
                Language = TestData.LanguageIdentifier,
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
                // bag property, and Guid and CorrelationGuid set
                matchedRun.Results[i].BaselineState.Should().Be(BaselineState.Unchanged);
                matchedRun.Results[i].Properties.Should().ContainKey("ResultMatching");

                // But aside from that they should be the same:
                matchedRun.Results[i].Guid = originalRun.Results[i].Guid;
                matchedRun.Results[i].CorrelationGuid = originalRun.Results[i].CorrelationGuid;
                matchedRun.Results[i].BaselineState = originalRun.Results[i].BaselineState;
                matchedRun.Results[i].Properties = originalRun.Results[i].Properties;

                matchedRun.Results[i].ValueEquals(originalRun.Results[i]).Should().BeTrue();
            }
        }

        private void AssertMatchedRunInvariants(Run baselinedRun)
        {
            // Ensure all results always have a BaselineState, CorrelationGuid, and Guid assigned after baselining.
            // The CorrelationGuid provides a stable identity to identify a Result matching across Runs.
            // The Guid should be the default CorrelationGuid, so that matched Results have an identity mappable to the first occurrence of the Result (even before it was baselined)
            // The Guid also needs to be defined to provided an identity to attach other data (like Annotations) to the Result consistently.
            foreach (Result result in baselinedRun.Results ?? Enumerable.Empty<Result>())
            {
                result.Guid.Should().NotBeNullOrEmpty();
                result.CorrelationGuid.Should().NotBeNullOrEmpty();
                result.BaselineState.Should().NotBe(BaselineState.None);
            }
        }
    }
}
