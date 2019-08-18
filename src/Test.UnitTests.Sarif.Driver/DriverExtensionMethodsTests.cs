// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif.Writers;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    public class DriverExtensionMethodsTests
    {
        [Fact]
        public void ConvertingAnalyzeOptionsToLoggingOptions_ProducesExpectedLoggingOptions()
        {
            LoggingOptions loggingOptions;

            TestAnalyzeOptions analyzeOptions = new TestAnalyzeOptions()
            {
                Verbose = true
            };

            loggingOptions = analyzeOptions.ConvertToLoggingOptions();
            loggingOptions.Should().Be(LoggingOptions.Verbose);

            analyzeOptions = new TestAnalyzeOptions()
            {
                PrettyPrint = true
            };

            loggingOptions = analyzeOptions.ConvertToLoggingOptions();
            loggingOptions.Should().Be(LoggingOptions.PrettyPrint);

            analyzeOptions = new TestAnalyzeOptions()
            {
                Force = true
            };

            loggingOptions = analyzeOptions.ConvertToLoggingOptions();
            loggingOptions.Should().Be(LoggingOptions.OverwriteExistingOutputFile);
        }

        [Fact]
        [Trait(TestTraits.Bug, "1642")]
        public void ValidatingOutputOptions_ProducesExpectedResults()
        {
            var failedTestCases = new List<string>();

            foreach (ValidateOutputOptionsTestCase testCase in s_validateOutputOptionsTestCases)
            {
                bool result = testCase.Options.ValidateOutputOptions();

                if (result != testCase.ExpectedResult)
                {
                    failedTestCases.Add(testCase.Title);
                }

                failedTestCases.Should().BeEmpty();
            }
        }

        private class ValidateOutputOptionsTestCase
        {
            public string Title;
            public SingleFileOptionsBase Options;
            public bool ExpectedResult;
        }

        private static readonly ValidateOutputOptionsTestCase[] s_validateOutputOptionsTestCases =
            new[]
            {
                new ValidateOutputOptionsTestCase
                {
                    Title = "--inline and not --force",
                    Options = new SingleFileOptionsBase
                    {
                        Inline = true,
                        Force = false,
                        OutputFilePath = null
                    },
                    ExpectedResult = true
                },

                new ValidateOutputOptionsTestCase
                {
                    Title = "--inline and superfluous --force",
                    Options = new SingleFileOptionsBase
                    {
                        Inline = true,
                        Force = true,
                        OutputFilePath = null
                    },
                    ExpectedResult = true
                },

                new ValidateOutputOptionsTestCase
                {
                    Title = "Neither --inline nor output path",
                    Options = new SingleFileOptionsBase
                    {
                        Inline = false,
                        Force = false,
                        OutputFilePath = null
                    },
                    ExpectedResult = false
                },

                new ValidateOutputOptionsTestCase
                {
                    Title = "Both --inline and output path",
                    Options = new SingleFileOptionsBase
                    {
                        Inline = true,
                        Force = false,
                        OutputFilePath = "output.sarif"
                    },
                    ExpectedResult = false
                }
            };
    }
}
