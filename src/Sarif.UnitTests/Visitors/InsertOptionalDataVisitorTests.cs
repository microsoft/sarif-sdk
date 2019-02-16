// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        
// license. See LICENSE file in the project root for full license information. 

using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif.TestUtilities;
using Microsoft.CodeAnalysis.Sarif.Writers;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{

    public class InsertOptionalDataVisitorTests : FileDiffingTests, IClassFixture<InsertOptionalDataVisitorTests.InsertOptionalDataVisitorTestsFixture>
    {
        public class InsertOptionalDataVisitorTestsFixture : DeletesOutputsDirectoryOnClassInitializationFixture { }

        private OptionallyEmittedData _currentOptionallyEmittedData;

        public InsertOptionalDataVisitorTests(ITestOutputHelper outputHelper, InsertOptionalDataVisitorTestsFixture fixture) : base (outputHelper)
        {
        }

        protected override bool RebaselineExpectedResults => false;

        protected override string ConstructTestOutputFromInputResource(string inputResourceName)
        {
            PrereleaseCompatibilityTransformer.UpdateToCurrentVersion(
                GetResourceText(inputResourceName),
                formatting: Formatting.Indented, 
                out string transformedLog);

            SarifLog actualLog = PrereleaseCompatibilityTransformer.UpdateToCurrentVersion(transformedLog, formatting: Formatting.None, out transformedLog);

            Uri originalUri = actualLog.Runs[0].OriginalUriBaseIds["TESTROOT"].Uri;
            string uriString = originalUri.ToString();

            // This code rewrites the log persisted URI to match the test environment
            string currentDirectory = Environment.CurrentDirectory;
            currentDirectory = currentDirectory.Substring(0, currentDirectory.IndexOf(@"\bld\"));
            uriString = uriString.Replace("REPLACED_AT_TEST_RUNTIME", currentDirectory);

            actualLog.Runs[0].OriginalUriBaseIds["TESTROOT"] = new ArtifactLocation { Uri = new Uri(uriString, UriKind.Absolute) };

            var visitor = new InsertOptionalDataVisitor(_currentOptionallyEmittedData);
            visitor.Visit(actualLog.Runs[0]);

            // Restore the remanufactured URI so that file diffing matches
            actualLog.Runs[0].OriginalUriBaseIds["TESTROOT"] = new ArtifactLocation { Uri = originalUri };

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
            RunTest("CoreTests.sarif", OptionallyEmittedData.Hashes);
        }

        [Fact]
        public void InsertOptionalDataVisitor_PersistsTextFiles()
        {
            RunTest("CoreTests.sarif", OptionallyEmittedData.TextFiles);
        }

        [Fact]
        public void InsertOptionalDataVisitor_PersistsRegionSnippets()
        {
            RunTest("CoreTests.sarif", OptionallyEmittedData.RegionSnippets);
        }

        [Fact]
        public void InsertOptionalDataVisitor_PersistsFlattenedMessages()
        {
            RunTest("CoreTests.sarif", OptionallyEmittedData.FlattenedMessages);
        }

        [Fact]
        public void InsertOptionalDataVisitor_PersistsContextRegionSnippets()
        {
            RunTest("CoreTests.sarif", OptionallyEmittedData.ContextRegionSnippets);
        }

        [Fact]
        public void InsertOptionalDataVisitor_PersistsComprehensiveRegionProperties()
        {
            RunTest("CoreTests.sarif", OptionallyEmittedData.ComprehensiveRegionProperties);
        }

        [Fact]
        public void InsertOptionalDataVisitor_PersistsAll()
        {
            RunTest("CoreTests.sarif", 
                OptionallyEmittedData.ComprehensiveRegionProperties | 
                OptionallyEmittedData.RegionSnippets | 
                OptionallyEmittedData.TextFiles | 
                OptionallyEmittedData.Hashes | 
                OptionallyEmittedData.ContextRegionSnippets | 
                OptionallyEmittedData.FlattenedMessages);
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
                        ToolNotifications = new List<Notification>{ },
                        ConfigurationNotifications = new List<Notification>{ }
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
                        RuleDescriptors = new List<ReportingDescriptor>
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
        public void InsertOptionalDataVisitorTests_FlattensMessageStringsInResult()
        {
            Run run = CreateBasicRunForMessageStringLookupTesting();

            run.Results.Add(
                new Result
                {
                    RuleId = RuleId,
                    RuleIndex = RuleIndex,
                    Message = new Message
                    {
                        MessageId = UniqueGlobalMessageId
                    }
                });

            run.Results.Add(
                new Result
                {
                    RuleId = RuleId,
                    RuleIndex = RuleIndex,
                    Message = new Message
                    {
                        MessageId = UniqueRuleMessageId
                    }
                });


            run.Results.Add(
                new Result
                {
                    RuleId = RuleId,
                    RuleIndex = RuleIndex,
                    Message = new Message
                    {
                        MessageId = SharedMessageId
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
        public void InsertOptionalDataVisitorTests_FlattensMessageStringsInNotification()
        {
            Run run = CreateBasicRunForMessageStringLookupTesting();

            IList<Notification> toolNotifications = run.Invocations[0].ToolNotifications;
            IList<Notification> configurationNotifications = run.Invocations[0].ConfigurationNotifications;

            // Shared message id with no overriding rule id
            toolNotifications.Add(
                new Notification
                {
                    Id = NotificationId,
                    Message = new Message {  MessageId = SharedMessageId}
                });
            configurationNotifications.Add(toolNotifications[0]);


            // Notification that refers to a rule that does not contain a message with 
            // the same id as the specified notification id.In this case it is no surprise
            // that the message comes from the global string table.
            toolNotifications.Add(
                new Notification
                {
                    Id = NotificationId,
                    RuleIndex = RuleIndex,
                    Message = new Message { MessageId = UniqueGlobalMessageId }
                });
            configurationNotifications.Add(toolNotifications[1]);


            // Notification that refers to a rule that contains a message with the same
            // id as the specified notification message id. The message should still be
            // retrieved from the global strings table.
            toolNotifications.Add(
                new Notification
                {
                    Id = NotificationId,
                    RuleIndex = RuleIndex,
                    Message = new Message { MessageId = SharedMessageId }
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
        public void InsertOptionalDataVisitorTests_FlattensMessageStringsInFix()
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
                                MessageId = UniqueGlobalMessageId
                            }
                        },
                        new Fix
                        {
                            Description = new Message
                            {
                                MessageId = UniqueRuleMessageId
                            }
                        },
                        new Fix
                        {
                            Description = new Message
                            {
                                MessageId = SharedMessageId
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
                                MessageId = SharedMessageId
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
        public void InsertOptionalDataVisitorTests_ResolvesOriginalUriBaseIds()
        {
            string inputFileName = "InsertOptionalDataVisitor.txt";
            string testDirectory = GetTestDirectory("InsertOptionalDataVisitor") + @"\";
            string uriBaseId = "TEST_DIR";
            string fileKey = "#" + uriBaseId + "#" + inputFileName;

            IDictionary<string, ArtifactLocation> originalUriBaseIds = new Dictionary<string, ArtifactLocation> { { uriBaseId, new ArtifactLocation { Uri = new Uri(testDirectory, UriKind.Absolute) } } };

            Run run = new Run()
            {
                DefaultFileEncoding = "UTF-8",
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

        private static string FormatFailureReason(string failureOutput)
        {
            string message = "the rewritten file should matched the supplied SARIF. ";
            message += failureOutput + Environment.NewLine;

            message = "If the actual output is expected, generate new baselines by setting s_rebaseline == true in the test code and rerunning.";
            return message;
        }
    
        private string NormalizeOptionallyEmittedDataToString(OptionallyEmittedData optionallyEmittedData)
        {
            string result = optionallyEmittedData.ToString();
            return result.Replace(", ", "+");
        }
    }
}