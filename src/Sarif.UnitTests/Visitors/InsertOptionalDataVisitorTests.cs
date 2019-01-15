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
                forceUpdate: false,
                formatting: Formatting.Indented, out string transformedLog);

            SarifLog actualLog = PrereleaseCompatibilityTransformer.UpdateToCurrentVersion(transformedLog, forceUpdate: false, formatting: Formatting.None, out transformedLog);

            Uri originalUri = actualLog.Runs[0].OriginalUriBaseIds["TESTROOT"].Uri;
            string uriString = originalUri.ToString();

            // This code rewrites the log persisted URI to match the test environment
            string currentDirectory = Environment.CurrentDirectory;
            currentDirectory = currentDirectory.Substring(0, currentDirectory.IndexOf(@"\bld\"));
            uriString = uriString.Replace("REPLACED_AT_TEST_RUNTIME", currentDirectory);

            actualLog.Runs[0].OriginalUriBaseIds["TESTROOT"] = new FileLocation { Uri = new Uri(uriString, UriKind.Absolute) };

            var visitor = new InsertOptionalDataVisitor(_currentOptionallyEmittedData);
            visitor.Visit(actualLog.Runs[0]);

            // Restore the remanufactured URI so that file diffing matches
            actualLog.Runs[0].OriginalUriBaseIds["TESTROOT"] = new FileLocation { Uri = originalUri };

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

        [Fact]
        public void InsertOptionalDataVisitorTests_ResolvesOriginalUriBaseIds()
        {
            string inputFileName = "InsertOptionalDataVisitor.txt";
            string testDirectory = GetTestDirectory("InsertOptionalDataVisitor") + @"\";
            string uriBaseId = "TEST_DIR";
            string fileKey = "#" + uriBaseId + "#" + inputFileName;

            IDictionary<string, FileLocation> originalUriBaseIds = new Dictionary<string, FileLocation> { { uriBaseId, new FileLocation { Uri = new Uri(testDirectory, UriKind.Absolute) } } };

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
                                     FileLocation = new FileLocation
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
            run.Files.Count.Should().Be(1);
            run.Files[0].Contents.Should().BeNull();

            visitor = new InsertOptionalDataVisitor(OptionallyEmittedData.TextFiles, originalUriBaseIds);
            visitor.VisitRun(run);

            run.OriginalUriBaseIds.Should().Equal(originalUriBaseIds);
            run.Files[0].Contents.Text.Should().Be(File.ReadAllText(Path.Combine(testDirectory, inputFileName)));
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