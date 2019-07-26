// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif.Multitool;
using Xunit;

namespace Microsoft.CodeAnalysis.Test.UnitTests.Sarif.Multitool
{
    public class FileWorkItemsCommandTests
    {
        [Fact]
        public void FileWorkItemsCommand_AcceptsOrRejectsCommandLinesAsAppropriate()
        {
            var failedTestCases = new List<string>();

            foreach (TestCase testCase in s_testCases)
            {
                int exitCode = Program.Main(testCase.Args);

                if (exitCode != testCase.ExpectedExitCode)
                {
                    failedTestCases.Add(testCase.Title);
                }
            }

            failedTestCases.Should().BeEmpty();
        }

        private class TestCase
        {
            public string Title;
            public string[] Args;
            public int ExpectedExitCode;
        }

        private static readonly TestCase[] s_testCases =
            new TestCase[] {
                new TestCase {
                    Title = "AzureDevOps host",
                    Args = new string[] {
                        "file-work-items",
                        "--test-option-validation",
                        "--project-uri",
                        "https://github.com/my-org/my-project",
                        "--filtering-strategy",
                        "NewResults",
                        "--grouping-strategy",
                        "PerResult",
                        "--inline",
                        "test.sarif",
                    },
                    ExpectedExitCode = 0
                },

                new TestCase {
                    Title = "GitHub host",
                    Args = new string[] {
                        "file-work-items",
                        "--test-option-validation",
                        "--project-uri",
                        "https://dev.azure.com/my-org/my-project",
                        "--filtering-strategy",
                        "NewResults",
                        "--grouping-strategy",
                        "PerResult",
                        "--inline",
                        "test.sarif",
                    },
                    ExpectedExitCode = 0
                },

                new TestCase {
                    Title = "Output file",
                    Args = new string[] {
                        "file-work-items",
                        "--test-option-validation",
                        "--project-uri",
                        "https://dev.azure.com/my-org/my-project",
                        "--filtering-strategy",
                        "NewResults",
                        "--grouping-strategy",
                        "PerResult",
                        "--output",
                        "test-output.sarif",
                        "test.sarif",
                    },
                    ExpectedExitCode = 0
                },

                // All valid filtering strategies ("NewResults" was covered above).
                new TestCase {
                    Title = "AllResults strategy",
                    Args = new string[] {
                        "file-work-items",
                        "--test-option-validation",
                        "--project-uri",
                        "https://github.com/my-org/my-project",
                        "--filtering-strategy",
                        "AllResults",
                        "--grouping-strategy",
                        "PerResult",
                        "--inline",
                        "test.sarif",
                    },
                    ExpectedExitCode = 0
                },

                // There is only one grouping strategy so we don't need any more positive tests.

                new TestCase {
                    Title = "Missing projectUri",
                    Args = new string[] {
                        "file-work-items",
                        "--test-option-validation",
                        "--filtering-strategy",
                        "NewResults",
                        "--grouping-strategy",
                        "PerResult",
                        "--inline",
                        "test.sarif",
                    },
                    ExpectedExitCode = 1
                },

                new TestCase {
                    Title = "Missing filtering-strategy",
                    Args = new string[] {
                        "file-work-items",
                        "--test-option-validation",
                        "--project-uri",
                        "https://dev.azure.com/my-org/my-project",
                        "--grouping-strategy",
                        "PerResult",
                        "--inline",
                        "test.sarif",
                    },
                    ExpectedExitCode = 1
                },

                new TestCase {
                    Title = "Missing grouping-strategy",
                    Args = new string[] {
                        "file-work-items",
                        "--test-option-validation",
                        "--project-uri",
                        "https://dev.azure.com/my-org/my-project",
                        "--filtering-strategy",
                        "NewResults",
                        "--inline",
                        "test.sarif"
                    },
                    ExpectedExitCode = 1
                },

                new TestCase {
                    Title = "Missing inputFile",
                    Args = new string[] {
                        "file-work-items",
                        "--test-option-validation",
                        "--project-uri",
                        "https://dev.azure.com/my-org/my-project",
                        "--filtering-strategy",
                        "NewResults",
                        "--grouping-strategy",
                        "PerResult",
                        "--inline"
                    },
                    ExpectedExitCode = 1
                },

                new TestCase {
                    Title = "Relative projectUri",
                    Args = new string[] {
                        "file-work-items",
                        "--test-option-validation",
                       "--project-uri",
                        "dev.azure.com/my-org/my-project",
                        "--filtering-strategy",
                        "NewResults",
                        "--grouping-strategy",
                        "PerResult",
                        "--inline",
                        "test.sarif",
                    },
                    ExpectedExitCode = 1
                },

                new TestCase {
                    Title = "Unknown filtering-strategy",
                    Args = new string[] {
                        "file-work-items",
                        "--test-option-validation",
                        "--project-uri",
                        "https://dev.azure.com/my-org/my-project",
                        "--filtering-strategy",
                        "OldResults",
                        "--grouping-strategy",
                        "PerResult",
                        "--inline",
                        "test.sarif",
                    },
                    ExpectedExitCode = 1
                },

                new TestCase {
                    Title = "Unknown grouping-strategy",
                    Args = new string[] {
                        "file-work-items",
                        "--test-option-validation",
                        "--project-uri",
                        "https://dev.azure.com/my-org/my-project",
                        "--filtering-strategy",
                        "NewResults",
                        "--grouping-strategy",
                        "PerCentury",
                        "--inline",
                        "test.sarif",
                    },
                    ExpectedExitCode = 1
                },

                new TestCase {
                    Title = "Both output and inline",
                    Args = new string[] {
                        "file-work-items",
                        "--test-option-validation",
                        "--project-uri",
                        "https://dev.azure.com/my-org/my-project",
                        "--filtering-strategy",
                        "NewResults",
                        "--grouping-strategy",
                        "PerResult",
                        "--output",
                        "test-output.sarif",
                        "--inline",
                        "test.sarif"
                    },
                    ExpectedExitCode = 1
                },

                new TestCase {
                    Title = "Neither output nor inline",
                    Args = new string[] {
                        "file-work-items",
                        "--test-option-validation",
                        "--project-uri",
                        "https://dev.azure.com/my-org/my-project",
                        "--filtering-strategy",
                        "NewResults",
                        "--grouping-strategy",
                        "PerResult",
                        "test.sarif",
                    },
                    ExpectedExitCode = 1
                }
            };
    }
}
