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

        private class ValidateSingleFileOutputOptionsTestCase
        {
            public string Title;
            public SingleFileOptionsBase Options;

            // The expected return value from ValidateOutputOptions.
            public bool ExpectedResult;

            // The expected (possibly adjusted) value of Force after the call to
            // ValidateOutputOptions. Not applicable if ExpectedResult is false.
            public bool ExpectedForce;
        }

        private static readonly ValidateSingleFileOutputOptionsTestCase[] s_validateSingleFileOutputOptionsTestCases =
            new[]
            {
                new ValidateSingleFileOutputOptionsTestCase
                {
                    Title = "--inline and not --force",
                    Options = new SingleFileOptionsBase
                    {
                        Inline = true,
                        Force = false,
                        OutputFilePath = null
                    },
                    ExpectedResult = true,
                    ExpectedForce = true
                },

                new ValidateSingleFileOutputOptionsTestCase
                {
                    Title = "--inline and superfluous --force",
                    Options = new SingleFileOptionsBase
                    {
                        Inline = true,
                        Force = true,
                        OutputFilePath = null
                    },
                    ExpectedResult = true,
                    ExpectedForce = true
                },

                new ValidateSingleFileOutputOptionsTestCase
                {
                    Title = "Output path with --force",
                    Options = new SingleFileOptionsBase
                    {
                        Inline = false,
                        Force = true,
                        OutputFilePath = "output.sarif",
                    },
                    ExpectedResult = true,
                    ExpectedForce = true
                },

                new ValidateSingleFileOutputOptionsTestCase
                {
                    Title = "Output path without --force",
                    Options = new SingleFileOptionsBase
                    {
                        Inline = false,
                        Force = false,
                        OutputFilePath = "output.sarif",
                    },
                    ExpectedResult = true,
                    ExpectedForce = false
                },

                new ValidateSingleFileOutputOptionsTestCase
                {
                    Title = "Neither --inline nor output path",
                    Options = new SingleFileOptionsBase
                    {
                        Inline = false,
                        Force = false,
                        OutputFilePath = null
                    },
                    ExpectedResult = true
                },

                new ValidateSingleFileOutputOptionsTestCase
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

        [Fact]
        [Trait(TestTraits.Bug, "https://github.com/microsoft/sarif-sdk/issues/1642")]
        public void ValidatingSingleFileOutputOptions_ProducesExpectedResults()
        {
            var failedTestCases = new List<string>();

            foreach (ValidateSingleFileOutputOptionsTestCase testCase in s_validateSingleFileOutputOptionsTestCases)
            {
                bool result = testCase.Options.ValidateOutputOptions();

                if (result != testCase.ExpectedResult)
                {
                    failedTestCases.Add(testCase.Title);
                }

                failedTestCases.Should().BeEmpty();
            }
        }

        private class ValidateMultipleFilesOutputOptionsTestCase
        {
            public string Title;
            public MultipleFilesOptionsBase Options;

            // The expected return value from ValidateOutputOptions.
            public bool ExpectedResult;

            // The expected (possibly adjusted) value of Force after the call to
            // ValidateOutputOptions. Not applicable if ExpectedResult is false.
            public bool ExpectedForce;
        }

        private static readonly ValidateMultipleFilesOutputOptionsTestCase[] s_validateMultipleFileOutputOptionsTestCases =
            new[]
            {
                new ValidateMultipleFilesOutputOptionsTestCase
                {
                    Title = "--force and not --inline",
                    Options = new MultipleFilesOptionsBase
                    {
                        Inline = false,
                        Force = true
                    },
                    ExpectedResult = true,
                    ExpectedForce = true
                },

                new ValidateMultipleFilesOutputOptionsTestCase
                {
                    Title = "--inline and not --force",
                    Options = new MultipleFilesOptionsBase
                    {
                        Inline = true,
                        Force = false
                    },
                    ExpectedResult = true,
                    ExpectedForce = true
                },

                new ValidateMultipleFilesOutputOptionsTestCase
                {
                    Title = "--inline and superfluous --force",
                    Options = new MultipleFilesOptionsBase
                    {
                        Inline = true,
                        Force = true
                    },
                    ExpectedResult = true,
                    ExpectedForce = true
                }
            };

        [Fact]
        [Trait(TestTraits.Bug, "https://github.com/microsoft/sarif-sdk/issues/1642")]
        public void ValidatingMultipleFilesOutputOptions_ProducesExpectedResults()
        {
            var failedTestCases = new List<string>();

            foreach (ValidateMultipleFilesOutputOptionsTestCase testCase in s_validateMultipleFileOutputOptionsTestCases)
            {
                bool result = testCase.Options.ValidateOutputOptions();

                if (result != testCase.ExpectedResult)
                {
                    failedTestCases.Add(testCase.Title);
                }

                failedTestCases.Should().BeEmpty();
            }
        }
    }
}
