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
            try
            {
                FileWorkItemsCommand.s_validateOptionsOnly = true;

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
            finally
            {
                FileWorkItemsCommand.s_validateOptionsOnly = false;
            }
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
                        "--project-uri",
                        "https://github.com/my-org/my-project",
                        "--inline",
                        "test.sarif",
                    },
                    ExpectedExitCode = 0
                },

                new TestCase {
                    Title = "GitHub host",
                    Args = new string[] {
                        "file-work-items",
                        "--project-uri",
                        "https://dev.azure.com/my-org/my-project",
                        "--inline",
                        "test.sarif",
                    },
                    ExpectedExitCode = 0
                },

                new TestCase {
                    Title = "Output file",
                    Args = new string[] {
                        "file-work-items",
                        "--project-uri",
                        "https://dev.azure.com/my-org/my-project",
                        "--output",
                        "test-output.sarif",
                        "test.sarif",
                    },
                    ExpectedExitCode = 0
                },

                new TestCase {
                    Title = "Implicit grouping strategy",
                    Args = new string[] {
                        "file-work-items",
                        "--project-uri",
                        "https://github.com/my-org/my-project",
                        "--inline",
                        "test.sarif",
                    },
                    ExpectedExitCode = 0
                },

                new TestCase {
                    Title = "Implicit grouping strategy",
                    Args = new string[] {
                        "file-work-items",
                        "--project-uri",
                        "https://github.com/my-org/my-project",
                        "--inline",
                        "test.sarif",
                    },
                    ExpectedExitCode = 0
                },

                new TestCase {
                    Title = "Explicit 'All' grouping strategy",
                    Args = new string[] {
                        "file-work-items",
                        "--project-uri",
                        "https://github.com/my-org/my-project",
                        "--inline",
                        "test.sarif",
                        "--group",
                        "All"
                    },
                    ExpectedExitCode = 0
                },

                new TestCase {
                    Title = "Explicit 'None' grouping strategy (equivalent to 'All')",
                    Args = new string[] {
                        "file-work-items",
                        "--project-uri",
                        "https://github.com/my-org/my-project",
                        "--inline",
                        "test.sarif",
                        "--group",
                        "None"
                    },
                    ExpectedExitCode = 0
                },

                new TestCase {
                    Title = "PerRunPerRule grouping strategy",
                    Args = new string[] {
                        "file-work-items",
                        "--project-uri",
                        "https://github.com/my-org/my-project",
                        "--inline",
                        "test.sarif",
                        "--group",
                        "PerRunPerRule"
                    },
                    ExpectedExitCode = 0
                },

                new TestCase {
                    Title = "PerRunPerTargetPerRun grouping strategy",
                    Args = new string[] {
                        "file-work-items",
                        "--project-uri",
                        "https://github.com/my-org/my-project",
                        "--inline",
                        "test.sarif",
                        "--group",
                        "PerRunPerTargetPerRule"
                    },
                    ExpectedExitCode = 0
                },

                new TestCase {
                    Title = "Non-existent grouping strategy",
                    Args = new string[] {
                        "file-work-items",
                        "--project-uri",
                        "https://github.com/my-org/my-project",
                        "--inline",
                        "test.sarif",
                        "--group",
                        "PerRunPerRun"
                    },
                    ExpectedExitCode = 1
                },

                new TestCase {
                    Title = "Missing projectUri",
                    Args = new string[] {
                        "file-work-items",
                        "--inline",
                        "test.sarif",
                    },
                    ExpectedExitCode = 1
                },

                new TestCase {
                    Title = "Missing inputFile",
                    Args = new string[] {
                        "file-work-items",
                        "--project-uri",
                        "https://dev.azure.com/my-org/my-project",
                        "--inline"
                    },
                    ExpectedExitCode = 1
                },

                new TestCase {
                    Title = "Relative projectUri",
                    Args = new string[] {
                        "file-work-items",
                       "--project-uri",
                        "dev.azure.com/my-org/my-project",
                        "--inline",
                        "test.sarif",
                    },
                    ExpectedExitCode = 1
                },

                new TestCase {
                    Title = "Both output and inline",
                    Args = new string[] {
                        "file-work-items",
                        "--project-uri",
                        "https://dev.azure.com/my-org/my-project",
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
                        "--project-uri",
                        "https://dev.azure.com/my-org/my-project",
                        "test.sarif",
                    },
                    ExpectedExitCode = 1
                }
            };
    }
}
