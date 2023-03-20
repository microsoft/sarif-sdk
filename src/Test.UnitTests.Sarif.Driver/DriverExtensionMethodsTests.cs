// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif.Writers;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    public class DriverExtensionMethodsTests
    {
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
                        OutputFilePath = null,
                        OutputFileOptions = new[] { FilePersistenceOptions.Inline },

                    },
                    ExpectedResult = true,
                    ExpectedForce = true
                },

                new ValidateSingleFileOutputOptionsTestCase
                {
                    Title = "--inline and superfluous --force",
                    Options = new SingleFileOptionsBase
                    {
                        OutputFileOptions = new[] { FilePersistenceOptions.ForceOverwrite, FilePersistenceOptions.Inline },
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
                        OutputFileOptions = new[] { FilePersistenceOptions.ForceOverwrite },
                        OutputFilePath = "output.sarif"
                    },
                    ExpectedResult = true,
                    ExpectedForce = true
                },

                new ValidateSingleFileOutputOptionsTestCase
                {
                    Title = "Output path without --force",
                    Options = new SingleFileOptionsBase
                    {
                        OutputFileOptions = new[] { FilePersistenceOptions.None },
                        OutputFilePath = "output.sarif"
                    },
                    ExpectedResult = true,
                    ExpectedForce = false
                },

                new ValidateSingleFileOutputOptionsTestCase
                {
                    Title = "Neither --inline nor output path",
                    Options = new SingleFileOptionsBase
                    {

                        OutputFileOptions = new[] { FilePersistenceOptions.None },
                        OutputFilePath = null
                    },
                    ExpectedResult = true
                },

                new ValidateSingleFileOutputOptionsTestCase
                {
                    Title = "Both --inline and output path",
                    Options = new SingleFileOptionsBase
                    {
                        OutputFileOptions = new[] { FilePersistenceOptions.Inline },
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
                        OutputFileOptions = new[] { FilePersistenceOptions.ForceOverwrite },
                    },
                    ExpectedResult = true,
                    ExpectedForce = true
                },

                new ValidateMultipleFilesOutputOptionsTestCase
                {
                    Title = "--inline and not --force",
                    Options = new MultipleFilesOptionsBase
                    {
                        OutputFileOptions = new[] { FilePersistenceOptions.Inline },
                    },
                    ExpectedResult = true,
                    ExpectedForce = true
                },

                new ValidateMultipleFilesOutputOptionsTestCase
                {
                    Title = "--inline and superfluous --force",
                    Options = new MultipleFilesOptionsBase
                    {
                        OutputFileOptions = new[] { FilePersistenceOptions.ForceOverwrite, FilePersistenceOptions.Inline },
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

        [Fact]
        public void ValidateAnalyzeOutputOptions_ProducesExpectedResults()
        {
            var context = new TestAnalysisContext();
            var analyzeOptionsBase = new TestAnalyzeOptions();

            analyzeOptionsBase.Quiet = null;
            analyzeOptionsBase.OutputFilePath = null;
            Assert.True(analyzeOptionsBase.ValidateOutputOptions(context));

            analyzeOptionsBase.Quiet = null;
            analyzeOptionsBase.OutputFilePath = "SomeFile.txt";
            Assert.True(analyzeOptionsBase.ValidateOutputOptions(context));

            analyzeOptionsBase.Quiet = BoolType.False;
            analyzeOptionsBase.OutputFilePath = null;
            Assert.True(analyzeOptionsBase.ValidateOutputOptions(context));

            analyzeOptionsBase.Quiet = BoolType.False;
            analyzeOptionsBase.OutputFilePath = "SomeFile.txt";
            Assert.True(analyzeOptionsBase.ValidateOutputOptions(context));

            analyzeOptionsBase.Quiet = BoolType.True;
            analyzeOptionsBase.OutputFilePath = null;
            Assert.False(analyzeOptionsBase.ValidateOutputOptions(context));

            analyzeOptionsBase.Quiet = BoolType.True;
            analyzeOptionsBase.OutputFilePath = "SomeFile.txt";
            Assert.True(analyzeOptionsBase.ValidateOutputOptions(context));

            analyzeOptionsBase.PostUri = "https://NotNull.example.com";
            analyzeOptionsBase.OutputFilePath = null;
            Assert.False(analyzeOptionsBase.ValidateOutputOptions(context));

            analyzeOptionsBase.PostUri = "https://NotNull.example.com";
            analyzeOptionsBase.OutputFilePath = null;
            Assert.False(analyzeOptionsBase.ValidateOutputOptions(context));

            analyzeOptionsBase.PostUri = "InvalidUrlText";
            analyzeOptionsBase.OutputFilePath = "SomeFile.txt";
            Assert.False(analyzeOptionsBase.ValidateOutputOptions(context));

            analyzeOptionsBase.PostUri = null;
            analyzeOptionsBase.OutputFilePath = "SomeFile.txt";
            Assert.True(analyzeOptionsBase.ValidateOutputOptions(context));

            analyzeOptionsBase.PostUri = "https://NotNull.example.com";
            analyzeOptionsBase.OutputFilePath = "SomeFile.txt";
            Assert.True(analyzeOptionsBase.ValidateOutputOptions(context));
        }

        [Fact]
        public void ValidateAnalyzeOutputOptions_ProducesExpectedResultsWhenUsingPostUriParameter()
        {
            var context = new TestAnalysisContext();
            var analyzeOptionsBase = new TestAnalyzeOptions();

            var testCases = new[]
            {
                new
                {
                    Title = "Invalid OutputFilePath",
                    PostUri = "https://NotNull.example.com",
                    OutputFilePath = string.Empty,
                    ExpectedValue = false
                },
                new
                {
                    Title = "Invalid PostUri",
                    PostUri = "InvalidUrlText",
                    OutputFilePath = "SomeFile.txt",
                    ExpectedValue = false
                },
                new
                {
                    Title = "Invalid PostUri",
                    PostUri = string.Empty,
                    OutputFilePath = "SomeFile.txt",
                    ExpectedValue = true
                },
                new
                {
                    Title = "Invalid PostUri",
                    PostUri = "https://NotNull.example.com",
                    OutputFilePath = "SomeFile.txt",
                    ExpectedValue = true
                },
            };

            var sb = new StringBuilder();

            foreach (var testCase in testCases)
            {
                analyzeOptionsBase.PostUri = testCase.PostUri;
                analyzeOptionsBase.OutputFilePath = testCase.OutputFilePath;

                bool validationResult = analyzeOptionsBase.ValidateOutputOptions(context);
                if (validationResult != testCase.ExpectedValue)
                {
                    sb.AppendLine($"The test '{testCase.Title}' was expecting '{testCase.ExpectedValue}' but found '{validationResult}'.");
                }
            }

            sb.Length.Should().Be(0, sb.ToString());
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
                        OutputFileOptions = new[] { FilePersistenceOptions.PrettyPrint }
                    },
                    ExpectedResult = true,
                    ExpectedPrettyPrint = true
                },

                new ValidateOutputFormatOptionsTestCase
                {
                    Title = "--minify and not --pretty-print",
                    Options = new SingleFileOptionsBase
                    {
                        OutputFileOptions = new[] { FilePersistenceOptions.Minify }
                    },
                    ExpectedResult = true,
                    ExpectedPrettyPrint = false
                },

                new ValidateOutputFormatOptionsTestCase
                {
                    Title = "Neither --pretty-print nor --minify",
                    Options = new SingleFileOptionsBase
                    {
                        OutputFileOptions = new[] { FilePersistenceOptions.None }
                    },
                    ExpectedResult = true,
                    ExpectedPrettyPrint = true
                },

                new ValidateOutputFormatOptionsTestCase
                {
                    Title = "Both --pretty-print and --minify (succeeds and prefers pretty print)",
                    Options = new SingleFileOptionsBase
                    {
                        OutputFileOptions = new[] { FilePersistenceOptions.Minify, FilePersistenceOptions.PrettyPrint }
                    },
                    ExpectedResult = true,
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
