﻿// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        
// license. See LICENSE file in the project root for full license information. 

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    public class InsertOptionalDataVisitorTests : FileDiffingTests
    {
        // To rebaseline all test files set this value to true and rerun the testa
        private static bool s_rebaseline = false;

        private ITestOutputHelper _outputHelper;

        private static string GetTestDirectory(string subdirectory = "")
        {
            return Path.GetFullPath(Path.Combine(@".\TestData", subdirectory));
        }

        // Retrieving the source path of the tests is only used in developer ad hoc
        // rebaselining scenarios. i.e., this path won't be consumed by AppVeyor.
        private static string GetProductTestDataDirectory(string subdirectory = "")
        {
            return Path.GetFullPath(Path.Combine(@"..\..\..\..\..\src\Sarif.UnitTests\TestData", subdirectory));
        }

        public InsertOptionalDataVisitorTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        [Theory]
        [InlineData(OptionallyEmittedData.Hashes)]
        [InlineData(OptionallyEmittedData.TextFiles)]
        [InlineData(OptionallyEmittedData.RegionSnippets)]
        [InlineData(OptionallyEmittedData.FlattenedMessages)]
        [InlineData(OptionallyEmittedData.ContextRegionSnippets)]
        [InlineData(OptionallyEmittedData.ComprehensiveRegionProperties)]
        [InlineData(OptionallyEmittedData.ComprehensiveRegionProperties | OptionallyEmittedData.RegionSnippets | OptionallyEmittedData.TextFiles | OptionallyEmittedData.Hashes | OptionallyEmittedData.ContextRegionSnippets | OptionallyEmittedData.FlattenedMessages)]
        public void InsertOptionalDataVisitorTests_InsertsOptionalDataForCommonConditions(OptionallyEmittedData optionallyEmittedData)
        {
            string testDirectory = GetTestDirectory("InsertOptionalDataVisitor");

            string inputFileName = "CoreTests";
            RunTest(testDirectory, inputFileName, optionallyEmittedData);
        }

        private void RunTest(string testDirectory, string inputFileName, OptionallyEmittedData optionallyEmittedData)
        {
            var sb = new StringBuilder();

            string optionsNameSuffix = "_" + NormalizeOptionallyEmittedDataToString(optionallyEmittedData);

            string expectedFileName = inputFileName + optionsNameSuffix + ".sarif";
            string actualFileName = @"Actual\" + inputFileName + optionsNameSuffix + ".sarif";
            inputFileName = inputFileName + ".sarif";

            expectedFileName = Path.Combine(testDirectory, expectedFileName);
            actualFileName = Path.Combine(testDirectory, actualFileName);
            inputFileName = Path.Combine(testDirectory, inputFileName);

            string actualDirectory = Path.GetDirectoryName(actualFileName);
            if (!Directory.Exists(actualDirectory)) { Directory.CreateDirectory(actualDirectory); }

            File.Exists(inputFileName).Should().BeTrue();

            SarifLog actualLog;

            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                ContractResolver = SarifContractResolver.Instance,
                Formatting = Formatting.Indented
            };

            try
            {
                actualLog = JsonConvert.DeserializeObject<SarifLog>(File.ReadAllText(inputFileName), settings);

                Uri originalUri = actualLog.Runs[0].OriginalUriBaseIds["TESTROOT"];
                string uriString = originalUri.ToString();

                // This code rewrites the log persisted URI to match the test environment
                string currentDirectory = Environment.CurrentDirectory;
                currentDirectory = currentDirectory.Substring(0, currentDirectory.IndexOf(@"\bld\"));
                uriString = uriString.Replace("REPLACED_AT_TEST_RUNTIME", currentDirectory);

                actualLog.Runs[0].OriginalUriBaseIds["TESTROOT"] = new Uri(uriString, UriKind.Absolute);

                var visitor = new InsertOptionalDataVisitor(optionallyEmittedData);
                visitor.Visit(actualLog.Runs[0]);

                // Restore the remanufactured URI so that file diffing matches
                actualLog.Runs[0].OriginalUriBaseIds["TESTROOT"] = originalUri;
            }
            catch (Exception ex)
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, "Unhandled exception processing input '{0}' with the following options: '{1}'.\r\n", inputFileName, optionallyEmittedData);
                sb.AppendLine(ex.ToString());
                ValidateResults(sb.ToString());
                return;
            }

            string expectedSarif = File.Exists(expectedFileName) ? File.ReadAllText(expectedFileName) : null;
            string actualSarif = JsonConvert.SerializeObject(actualLog, settings);

            if (!AreEquivalentSarifLogs(actualSarif, expectedSarif))
            {
                if (s_rebaseline)
                {
                    // We rewrite to test output directory. This allows subsequent tests to 
                    // pass without requiring a rebuild that recopies SARIF test files
                    File.WriteAllText(expectedFileName, actualSarif);

                    string subdirectory = Path.GetFileName(testDirectory);
                    string productTestDirectory = GetProductTestDataDirectory(subdirectory);
                    expectedFileName = Path.GetFileName(expectedFileName);
                    expectedFileName = Path.Combine(productTestDirectory, expectedFileName);

                    // We also rewrite the checked in test baselines
                    File.WriteAllText(expectedFileName, actualSarif);
                }
                else
                {
                    File.WriteAllText(actualFileName, actualSarif);

                    string errorMessage = "Expanding optional data for input '{0}' produced unexpected results for the following options: '{1}'.";
                    sb.AppendLine(string.Format(CultureInfo.CurrentCulture, errorMessage, inputFileName, optionallyEmittedData));
                    sb.AppendLine("Check individual differences with:");
                    sb.AppendLine(GenerateDiffCommand(expectedFileName, actualFileName) + Environment.NewLine);
                    sb.AppendLine("To compare all difference for this test suite:");
                    sb.AppendLine(GenerateDiffCommand(Path.GetDirectoryName(expectedFileName), Path.GetDirectoryName(actualFileName)) + Environment.NewLine);
                }
            }

            // Add this check to prevent us from unexpectedly checking in this static with the wrong value
            s_rebaseline.Should().BeFalse();

            ValidateResults(sb.ToString());
        }

        private void ValidateResults(string output)
        {
            if (!string.IsNullOrEmpty(output))
            {
                _outputHelper.WriteLine(output);
            }

            // We can't use the 'because' argument here because someone along
            // the line is stripping \n from output strings. This compromises
            // our file paths. e.g., 'c:\build\netcore2.0\etc' is rendered
            // as 'c:\build\etcore2.0'. 
            output.Length.Should().Be(0);
        }

        [Fact]
        public void InsertOptionalDataVisitorTests_VisitDictionaryValueNullChecked_ValidEncoding()
        {
            var visitor = new InsertOptionalDataVisitor(OptionallyEmittedData.OverwriteExistingData | OptionallyEmittedData.TextFiles);
            visitor.VisitRun(new Run()); // VisitDictionaryValueNullChecked requires a non-null run

            string uriString = "file:///C:/src/foo.cs";

            FileData fileData = FileData.Create(new Uri(uriString), mimeType: "text/x-csharp", encoding: Encoding.UTF8);
            fileData.Length = 12345;

            FileData outputFileData = visitor.VisitDictionaryValueNullChecked(uriString, fileData);
            outputFileData.MimeType.Should().Be(fileData.MimeType);
            outputFileData.Encoding.Should().Be(fileData.Encoding);
            outputFileData.Length.Should().Be(fileData.Length);
        }

        [Fact]
        public void InsertOptionalDataVisitorTests_VisitDictionaryValueNullChecked_InvalidEncoding()
        {
            var visitor = new InsertOptionalDataVisitor(OptionallyEmittedData.OverwriteExistingData | OptionallyEmittedData.TextFiles);
            visitor.VisitRun(new Run()); // VisitDictionaryValueNullChecked requires a non-null run

            string uriString = "file:///C:/src/foo.cs";

            FileData fileData = FileData.Create(new Uri(uriString), mimeType: "text/x-csharp");
            fileData.Encoding = "invalid";
            fileData.Length = 54321;

            FileData outputFileData = visitor.VisitDictionaryValueNullChecked(uriString, fileData);
            outputFileData.MimeType.Should().Be(fileData.MimeType);
            outputFileData.Encoding.Should().BeNull();
            outputFileData.Length.Should().Be(fileData.Length);
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