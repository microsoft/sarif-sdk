﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;

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

        private static readonly ReadOnlyCollection<ValidateSingleFileOutputOptionsTestCase> s_validateSingleFileOutputOptionsTestCases =
            new List<ValidateSingleFileOutputOptionsTestCase>
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
            }.AsReadOnly();

        [Fact]
        [Trait(TestTraits.Bug, "https://github.com/microsoft/sarif-sdk/issues/1642")]
        public void ValidatingSingleFileOutputOptions_ProducesExpectedResults()
        {
            var failedTestCases = new List<string>();

            foreach (ValidateSingleFileOutputOptionsTestCase testCase in s_validateSingleFileOutputOptionsTestCases)
            {
                bool result = testCase.Options.Validate();

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

        private static readonly ReadOnlyCollection<ValidateMultipleFilesOutputOptionsTestCase> s_validateMultipleFileOutputOptionsTestCases =
            new List<ValidateMultipleFilesOutputOptionsTestCase>
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
            }.AsReadOnly();

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

        private class ValidateOutputFormatOptionsTestCase
        {
            public string Title;
            public SingleFileOptionsBase Options;

            // The expected return value from ValidateOutputFormatOptions.
            public bool ExpectedResult;

            // The expected (possibly adjusted) value of PrettyPrint after the call to
            // ValidateOutputFormatOptions. Not applicable if ExpectedResult is false.
            public bool ExpectedPrettyPrint;
        }

        private static readonly ReadOnlyCollection<ValidateOutputFormatOptionsTestCase> s_validateOutputFormatOptionsTestCases =
            new List<ValidateOutputFormatOptionsTestCase>
            {
                new ValidateOutputFormatOptionsTestCase
                {
                    Title = "--pretty-print and not --minify",
                    Options = new SingleFileOptionsBase
                    {
                        PrettyPrint = true,
                        Minify = false
                    },
                    ExpectedResult = true,
                    ExpectedPrettyPrint = true
                },

                new ValidateOutputFormatOptionsTestCase
                {
                    Title = "--minify and not --pretty-print",
                    Options = new SingleFileOptionsBase
                    {
                        PrettyPrint = false,
                        Minify = true
                    },
                    ExpectedResult = true,
                    ExpectedPrettyPrint = false
                },

                new ValidateOutputFormatOptionsTestCase
                {
                    Title = "Neither --pretty-print nor --minify",
                    Options = new SingleFileOptionsBase
                    {
                        PrettyPrint = false,
                        Minify = false
                    },
                    ExpectedResult = true,
                    ExpectedPrettyPrint = true
                },

                new ValidateOutputFormatOptionsTestCase
                {
                    Title = "Both --pretty-print and --minify",
                    Options = new SingleFileOptionsBase
                    {
                        PrettyPrint = true,
                        Minify = true
                    },
                    ExpectedResult = false,
                    ExpectedPrettyPrint = true
                }
            }.AsReadOnly();

        [Fact]
        [Trait(TestTraits.Bug, "https://github.com/microsoft/sarif-sdk/issues/2098")]
        public void ValidatingOutputFormatOptions_ProducesExpectedResults()
        {
            var failedTestCases = new List<string>();

            foreach (ValidateOutputFormatOptionsTestCase testCase in s_validateOutputFormatOptionsTestCases)
            {
                bool result = testCase.Options.Validate();

                if (result != testCase.ExpectedResult)
                {
                    failedTestCases.Add(testCase.Title);
                }

                failedTestCases.Should().BeEmpty();
            }
        }
    }
}
