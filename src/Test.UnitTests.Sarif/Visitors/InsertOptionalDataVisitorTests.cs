// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        
// license. See LICENSE file in the project root for full license information. 

using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif.Driver;
using Microsoft.CodeAnalysis.Sarif.Writers;
using Microsoft.CodeAnalysis.Test.Utilities.Sarif;

using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    public class InsertOptionalDataVisitorTests : FileDiffingUnitTests, IClassFixture<DeletesOutputsDirectoryOnClassInitializationFixture>
    {
        private OptionallyEmittedData _currentOptionallyEmittedData;

        public InsertOptionalDataVisitorTests(ITestOutputHelper outputHelper, DeletesOutputsDirectoryOnClassInitializationFixture _) : base(outputHelper)
        {
        }

        protected override string ConstructTestOutputFromInputResource(string inputResourceName, object parameter)
        {
            PrereleaseCompatibilityTransformer.UpdateToCurrentVersion(
                GetResourceText(inputResourceName),
                formatting: Formatting.Indented,
                out string transformedLog);

            SarifLog actualLog = PrereleaseCompatibilityTransformer.UpdateToCurrentVersion(transformedLog, formatting: Formatting.None, out transformedLog);

            // For CoreTests only - this code rewrites the log persisted URI to match the test environment
            if (inputResourceName == "Inputs.CoreTests-Relative.sarif")
            {
                Uri originalUri = actualLog.Runs[0].OriginalUriBaseIds["TESTROOT"].Uri;
                string uriString = originalUri.ToString();

                string currentDirectory = Environment.CurrentDirectory;
                currentDirectory = currentDirectory.Substring(0, currentDirectory.IndexOf(@"\bld\"));
                uriString = uriString.Replace("REPLACED_AT_TEST_RUNTIME", currentDirectory);

                actualLog.Runs[0].OriginalUriBaseIds["TESTROOT"] = new ArtifactLocation { Uri = new Uri(uriString, UriKind.Absolute) };

                var visitor = new InsertOptionalDataVisitor(_currentOptionallyEmittedData);
                visitor.Visit(actualLog.Runs[0]);

                // Restore the remanufactured URI so that file diffing matches
                actualLog.Runs[0].OriginalUriBaseIds["TESTROOT"] = new ArtifactLocation { Uri = originalUri };
            }
            else if (inputResourceName == "Inputs.CoreTests-Absolute.sarif")
            {
                Uri originalUri = actualLog.Runs[0].Artifacts[0].Location.Uri;
                string uriString = originalUri.ToString();

                string currentDirectory = Environment.CurrentDirectory;
                currentDirectory = currentDirectory.Substring(0, currentDirectory.IndexOf(@"\bld\"));
                uriString = uriString.Replace("REPLACED_AT_TEST_RUNTIME", currentDirectory);

                actualLog.Runs[0].Artifacts[0].Location = new ArtifactLocation { Uri = new Uri(uriString, UriKind.Absolute) };

                var visitor = new InsertOptionalDataVisitor(_currentOptionallyEmittedData);
                visitor.Visit(actualLog.Runs[0]);

                // Restore the remanufactured URI so that file diffing matches
                actualLog.Runs[0].Artifacts[0].Location = new ArtifactLocation { Uri = originalUri };
            }
            else
            {
                var visitor = new InsertOptionalDataVisitor(_currentOptionallyEmittedData);
                visitor.Visit(actualLog.Runs[0]);
            }

            // Verify and remove Guids, because they'll vary with every run and can't be compared to a fixed expected output
            if (_currentOptionallyEmittedData.HasFlag(OptionallyEmittedData.Guids))
            {
                for (int i = 0; i < actualLog.Runs[0].Results.Count; ++i)
                {
                    Result result = actualLog.Runs[0].Results[i];
                    if (string.IsNullOrEmpty(result.Guid))
                    {
                        Assert.True(false, $"Results[{i}] had no Guid assigned, but OptionallyEmittedData.Guids flag was set.");
                    }

                    result.Guid = null;
                }
            }

            return JsonConvert.SerializeObject(actualLog, Formatting.Indented);
        }

        private void RunTest(string inputResourceName, OptionallyEmittedData optionallyEmittedData)
        {
            _currentOptionallyEmittedData = optionallyEmittedData;
            string expectedOutputResourceName = Path.GetFileNameWithoutExtension(inputResourceName);
            expectedOutputResourceName = expectedOutputResourceName + "_" + optionallyEmittedData.ToString().Replace(", ", "+");
            RunTest(inputResourceName, expectedOutputResourceName);
        }

        [Fact]
        public void InsertOptionalDataVisitor_PersistsHashes()
        {
            RunTest("CoreTests-Relative.sarif", OptionallyEmittedData.Hashes);
        }

        [Fact]
        public void InsertOptionalDataVisitor_PersistsTextFilesWithRelativeUris()
        {
            RunTest("CoreTests-Relative.sarif", OptionallyEmittedData.TextFiles);
        }

        [Fact]
        public void InsertOptionalDataVisitor_PersistsTextFilesWithAbsoluteUris()
        {
            RunTest("CoreTests-Absolute.sarif", OptionallyEmittedData.TextFiles);
        }

        [Fact]
        public void InsertOptionalDataVisitor_PersistsRegionSnippets()
        {
            RunTest("CoreTests-Relative.sarif", OptionallyEmittedData.RegionSnippets);
        }

        [Fact]
        public void InsertOptionalDataVisitor_PersistsFlattenedMessages()
        {
            RunTest("CoreTests-Relative.sarif", OptionallyEmittedData.FlattenedMessages);
        }

        [Fact]
        public void InsertOptionalDataVisitor_PersistsContextRegionSnippets()
        {
            RunTest("CoreTests-Relative.sarif", OptionallyEmittedData.ContextRegionSnippets);
        }

        [Fact]
        public void InsertOptionalDataVisitor_PersistsComprehensiveRegionProperties()
        {
            RunTest("CoreTests-Relative.sarif", OptionallyEmittedData.ComprehensiveRegionProperties);
        }

        [Fact]
        public void InsertOptionalDataVisitor_PersistsGuids()
        {
            // NOTE: Test adding Guids, but validation is in test code, not diff, as Guids vary with each run.
            RunTest("CoreTests-Relative.sarif", OptionallyEmittedData.Guids);
        }

        [Fact]
        public void InsertOptionalDataVisitor_PersistsNone()
        {
            RunTest("CoreTests-Relative.sarif");
        }

        [Fact]
        public void InsertOptionalDataVisitor_PersistsHashesAndTextFiles()
        {
            RunTest("CoreTests-Relative.sarif",
                OptionallyEmittedData.TextFiles |
                OptionallyEmittedData.Hashes);
        }

        [Fact]
        public void InsertOptionalDataVisitor_PersistsAll()
        {
            RunTest("CoreTests-Relative.sarif",
                OptionallyEmittedData.All);
        }

        [Fact]
        public void InsertOptionalDataVisitor_ContextRegionSnippets_DoesNotFail_TopLevelOriginalUriBaseIdUriMissing()
        {
            RunTest("TopLevelOriginalUriBaseIdUriMissing.sarif",
                OptionallyEmittedData.ContextRegionSnippets);
        }

        [Fact]
        public void InsertOptionalDataVisitor_CanVisitIndividualResultsInASuppliedRun()
        {
            const string TestFileContents =
@"One
Two
Three";
            const string ExpectedSnippet = "Two";

            using (var tempFile = new TempFile(".txt"))
            {
                string tempFilePath = tempFile.Name;
                string tempFileName = Path.GetFileName(tempFilePath);
                string tempFileDirectory = Path.GetDirectoryName(tempFilePath);

                File.WriteAllText(tempFilePath, TestFileContents);

                var run = new Run
                {
                    OriginalUriBaseIds = new Dictionary<string, ArtifactLocation>
                    {
                        [TestData.TestRootBaseId] = new ArtifactLocation
                        {
                            Uri = new Uri(tempFileDirectory, UriKind.Absolute)
                        }
                    },
                    Results = new List<Result>
                    {
                        new Result
                        {
                            Locations = new List<Location>
                            {
                                new Location
                                {
                                    PhysicalLocation = new PhysicalLocation
                                    {
                                        ArtifactLocation = new ArtifactLocation
                                        {
                                            Uri = new Uri(tempFileName, UriKind.Relative),
                                            UriBaseId = TestData.TestRootBaseId
                                        },
                                        Region = new Region
                                        {
                                            StartLine = 2
                                        }
                                    }
                                }
                            }
                        }
                    }
                };

                var visitor = new InsertOptionalDataVisitor(OptionallyEmittedData.RegionSnippets, run);

                visitor.VisitResult(run.Results[0]);

                run.Results[0].Locations[0].PhysicalLocation.Region.Snippet.Text.Should().Be(ExpectedSnippet);
            }
        }

        private const int RuleIndex = 0;
        private const string RuleId = nameof(RuleId);
        private const string NotificationId = nameof(NotificationId);

        private const string SharedMessageId = nameof(SharedMessageId);
        private const string SharedKeyRuleMessageValue = nameof(SharedKeyRuleMessageValue);
        private const string SharedKeyGlobalMessageValue = nameof(SharedKeyGlobalMessageValue);

        private const string UniqueRuleMessageId = nameof(UniqueRuleMessageId);
        private const string UniqueRuleMessageValue = nameof(UniqueRuleMessageValue);

        private const string UniqueGlobalMessageId = nameof(UniqueGlobalMessageId);
        private const string UniqueGlobalMessageValue = nameof(UniqueGlobalMessageValue);

        private static Run CreateBasicRunForMessageStringLookupTesting()
        {
            // Returns a run object that defines unique string instances both
            // for an individual rule and in the global strings object. Also
            // defines values for a key that is shared between the rule object
            // and the global table. Used for evaluating string look-up semantics.
            var run = new Run
            {
                Results = new List<Result> { }, // add non-null collections for convenience
                Invocations = new List<Invocation>
                {
                    new Invocation
                    {
                        ToolExecutionNotifications = new List<Notification>{ },
                        ToolConfigurationNotifications = new List<Notification>{ }
                    }
                },
                Tool = new Tool
                {
                    Driver = new ToolComponent
                    {
                        GlobalMessageStrings = new Dictionary<string, MultiformatMessageString>
                        {
                            [UniqueGlobalMessageId] = new MultiformatMessageString { Text = UniqueGlobalMessageValue },
                            [SharedMessageId] = new MultiformatMessageString { Text = SharedKeyGlobalMessageValue }
                        },
                        Rules = new List<ReportingDescriptor>
                        {
                            new ReportingDescriptor
                            {
                                Id = RuleId,
                                MessageStrings = new Dictionary<string, MultiformatMessageString>
                                {
                                    [UniqueRuleMessageId] = new MultiformatMessageString { Text = UniqueRuleMessageValue },
                                    [SharedMessageId] = new MultiformatMessageString { Text = SharedKeyRuleMessageValue }
                                }
                            }
                        }
                    }
                }
            };

            return run;
        }

        [Fact]
        public void InsertOptionalDataVisitor_FlattensMessageStringsInResult()
        {
            Run run = CreateBasicRunForMessageStringLookupTesting();

            run.Results.Add(
                new Result
                {
                    RuleId = RuleId,
                    RuleIndex = RuleIndex,
                    Message = new Message
                    {
                        Id = UniqueGlobalMessageId
                    }
                });

            run.Results.Add(
                new Result
                {
                    RuleId = RuleId,
                    RuleIndex = RuleIndex,
                    Message = new Message
                    {
                        Id = UniqueRuleMessageId
                    }
                });


            run.Results.Add(
                new Result
                {
                    RuleId = RuleId,
                    RuleIndex = RuleIndex,
                    Message = new Message
                    {
                        Id = SharedMessageId
                    }
                });


            var visitor = new InsertOptionalDataVisitor(OptionallyEmittedData.FlattenedMessages);
            visitor.Visit(run);

            run.Results[0].Message.Text.Should().Be(UniqueGlobalMessageValue);
            run.Results[1].Message.Text.Should().Be(UniqueRuleMessageValue);

            // Prefer rule-specific value in the event of a message id collision
            run.Results[2].Message.Text.Should().Be(SharedKeyRuleMessageValue);
        }

        [Fact]
        public void InsertOptionalDataVisitor_FlattensMessageStringsInNotification()
        {
            Run run = CreateBasicRunForMessageStringLookupTesting();

            IList<Notification> toolNotifications = run.Invocations[0].ToolExecutionNotifications;
            IList<Notification> configurationNotifications = run.Invocations[0].ToolConfigurationNotifications;

            // Shared message id with no overriding rule id
            toolNotifications.Add(
                new Notification
                {
                    Descriptor = new ReportingDescriptorReference
                    {
                        Id = NotificationId
                    },
                    Message = new Message { Id = SharedMessageId }
                });
            configurationNotifications.Add(toolNotifications[0]);


            // Notification that refers to a rule that does not contain a message with 
            // the same id as the specified notification id.In this case it is no surprise
            // that the message comes from the global string table.
            toolNotifications.Add(
                new Notification
                {
                    Descriptor = new ReportingDescriptorReference
                    {
                        Id = NotificationId
                    },
                    AssociatedRule = new ReportingDescriptorReference
                    {
                        Index = RuleIndex
                    },
                    Message = new Message { Id = UniqueGlobalMessageId }
                });
            configurationNotifications.Add(toolNotifications[1]);


            // Notification that refers to a rule that contains a message with the same
            // id as the specified notification message id. The message should still be
            // retrieved from the global strings table.
            toolNotifications.Add(
                new Notification
                {
                    Descriptor = new ReportingDescriptorReference
                    {
                        Id = NotificationId
                    },
                    AssociatedRule = new ReportingDescriptorReference
                    {
                        Index = RuleIndex
                    },
                    Message = new Message { Id = SharedMessageId }
                });
            configurationNotifications.Add(toolNotifications[2]);


            var visitor = new InsertOptionalDataVisitor(OptionallyEmittedData.FlattenedMessages);
            visitor.Visit(run);

            toolNotifications[0].Message.Text.Should().Be(SharedKeyGlobalMessageValue);
            configurationNotifications[0].Message.Text.Should().Be(SharedKeyGlobalMessageValue);

            toolNotifications[1].Message.Text.Should().Be(UniqueGlobalMessageValue);
            configurationNotifications[1].Message.Text.Should().Be(UniqueGlobalMessageValue);

            toolNotifications[2].Message.Text.Should().Be(SharedKeyGlobalMessageValue);
            configurationNotifications[2].Message.Text.Should().Be(SharedKeyGlobalMessageValue);
        }

        [Fact]
        public void InsertOptionalDataVisitor_FlattensMessageStringsInFix()
        {
            Run run = CreateBasicRunForMessageStringLookupTesting();

            run.Results.Add(
                new Result
                {
                    RuleId = RuleId,
                    RuleIndex = RuleIndex,
                    Message = new Message
                    {
                        Text = "Some testing occurred."
                    },
                    Fixes = new List<Fix>
                    {
                        new Fix
                        {
                            Description = new Message
                            {
                                Id = UniqueGlobalMessageId
                            }
                        },
                        new Fix
                        {
                            Description = new Message
                            {
                                Id = UniqueRuleMessageId
                            }
                        },
                        new Fix
                        {
                            Description = new Message
                            {
                                Id = SharedMessageId
                            }
                        }
                    }
                });
            run.Results.Add(
                new Result
                {
                    RuleId = "RuleWithNoRuleDescriptor",
                    Message = new Message
                    {
                        Text = "Some testing occurred."
                    },
                    Fixes = new List<Fix>
                    {
                        new Fix
                        {
                            Description = new Message
                            {
                                Id = SharedMessageId
                            }
                        }
                    }
                });

            var visitor = new InsertOptionalDataVisitor(OptionallyEmittedData.FlattenedMessages);
            visitor.Visit(run);

            run.Results[0].Fixes[0].Description.Text.Should().Be(UniqueGlobalMessageValue);
            run.Results[0].Fixes[1].Description.Text.Should().Be(UniqueRuleMessageValue);

            // Prefer rule-specific value in the event of a message id collision
            run.Results[0].Fixes[2].Description.Text.Should().Be(SharedKeyRuleMessageValue);

            // Prefer global value in the event of no rules metadata
            run.Results[1].Fixes[0].Description.Text.Should().Be(SharedKeyGlobalMessageValue);
        }

        [Fact]
        public void InsertOptionalDataVisitor_ResolvesOriginalUriBaseIds()
        {
            string inputFileName = "InsertOptionalDataVisitor.txt";
            string testDirectory = GetTestDirectory("InsertOptionalDataVisitor") + @"\";
            string uriBaseId = "TEST_DIR";
            string fileKey = "#" + uriBaseId + "#" + inputFileName;

            IDictionary<string, ArtifactLocation> originalUriBaseIds = new Dictionary<string, ArtifactLocation> { { uriBaseId, new ArtifactLocation { Uri = new Uri(testDirectory, UriKind.Absolute) } } };

            Run run = new Run()
            {
                DefaultEncoding = "UTF-8",
                OriginalUriBaseIds = null,
                Results = new[]
                {
                    new Result()
                    {
                        Locations = new []
                        {
                            new Location
                            {
                                PhysicalLocation = new PhysicalLocation
                                {
                                     ArtifactLocation = new ArtifactLocation
                                     {
                                        Uri = new Uri(inputFileName, UriKind.Relative),
                                        UriBaseId = uriBaseId
                                     }
                                }
                            }
                        }
                    }
                }
            };

            var visitor = new InsertOptionalDataVisitor(OptionallyEmittedData.TextFiles);
            visitor.VisitRun(run);

            run.OriginalUriBaseIds.Should().BeNull();
            run.Artifacts.Count.Should().Be(1);
            run.Artifacts[0].Contents.Should().BeNull();

            visitor = new InsertOptionalDataVisitor(OptionallyEmittedData.TextFiles, originalUriBaseIds);
            visitor.VisitRun(run);

            run.OriginalUriBaseIds.Should().Equal(originalUriBaseIds);
            run.Artifacts[0].Contents.Text.Should().Be(File.ReadAllText(Path.Combine(testDirectory, inputFileName)));
        }
    }
}