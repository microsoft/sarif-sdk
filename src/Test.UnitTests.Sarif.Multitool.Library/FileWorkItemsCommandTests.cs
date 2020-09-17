// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class FileWorkItemsCommandTests
    {
        [Fact]
        public void FileWorkItemsCommand_AcceptsOrRejectsCommandLinesAsAppropriate()
        {
            try
            {
                FileWorkItemsCommand.s_validateOptionsOnly = true;

                Environment.SetEnvironmentVariable("SarifWorkItemFilingPat", Guid.NewGuid().ToString());

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
                    Title = "GitHub host",
                    Args = new string[] {
                        "file-work-items",
                        "--host-uri",
                        "https://github.com/my-org/my-project",
                        "--inline",
                        "test.sarif"
                    },
                    ExpectedExitCode = 0
                },

                new TestCase {
                    Title = "AzureDevOps host",
                    Args = new string[] {
                        "file-work-items",
                        "--host-uri",
                        "https://dev.azure.com/my-org/my-project",
                        "--inline",
                        "test.sarif"
                    },
                    ExpectedExitCode = 0
                },

                new TestCase {
                    Title = "AzureDevOps host with legacy uri",
                    Args = new string[] {
                        "file-work-items",
                        "--host-uri",
                        "https://my-org.visualstudio.com/my-project",
                        "--inline",
                        "test.sarif"
                    },
                    ExpectedExitCode = 0
                },

                new TestCase {
                    Title = "Output file",
                    Args = new string[] {
                        "file-work-items",
                        "--host-uri",
                        "https://dev.azure.com/my-org/my-project",
                        "--output",
                        "test-output.sarif",
                        "test.sarif"
                    },
                    ExpectedExitCode = 0
                },


                new TestCase {
                    Title = "Implicit splitting strategy",
                    Args = new string[] {
                        "file-work-items",
                        "--host-uri",
                        "https://github.com/my-org/my-project",
                        "--inline",
                        "test.sarif"
                    },
                    ExpectedExitCode = 0
                },

                new TestCase {
                    Title = "Explicit 'PerResult' splitting strategy",
                    Args = new string[] {
                        "file-work-items",
                        "--host-uri",
                        "https://github.com/my-org/my-project",
                        "--inline",
                        "test.sarif",
                        "--split",
                        "PerResult"
                    },
                    ExpectedExitCode = 0
                },

                new TestCase {
                    Title = "Explicit 'PerRun' splitting strategy",
                    Args = new string[] {
                        "file-work-items",
                        "--host-uri",
                        "https://github.com/my-org/my-project",
                        "--inline",
                        "test.sarif",
                        "--split",
                        "PerRun"
                    },
                    ExpectedExitCode = 0
                },

                new TestCase {
                    Title = "Explicit 'None' splitting strategy",
                    Args = new string[] {
                        "file-work-items",
                        "--host-uri",
                        "https://github.com/my-org/my-project",
                        "--inline",
                        "test.sarif",
                        "--split",
                        "None"
                    },
                    ExpectedExitCode = 0
                },

                new TestCase {
                    Title = "PerRunPerRule splitting strategy",
                    Args = new string[] {
                        "file-work-items",
                        "--host-uri",
                        "https://github.com/my-org/my-project",
                        "--inline",
                        "test.sarif",
                        "--split",
                        "PerRunPerRule"
                    },
                    ExpectedExitCode = 0
                },

                new TestCase {
                    Title = "PerRunPerTargetPerRun splitting strategy",
                    Args = new string[] {
                        "file-work-items",
                        "--host-uri",
                        "https://github.com/my-org/my-project",
                        "--inline",
                        "test.sarif",
                        "--split",
                        "PerRunPerTargetPerRule"
                    },
                    ExpectedExitCode = 0
                },


                new TestCase {
                    Title = "Non-existent splitting strategy",
                    Args = new string[] {
                        "file-work-items",
                        "--host-uri",
                        "https://github.com/my-org/my-project",
                        "--inline",
                        "test.sarif",
                        "--split",
                        "PerRunPerRun"
                    },
                    ExpectedExitCode = 1
                },

                new TestCase {
                    Title = "Missing hostUri",
                    Args = new string[] {
                        "file-work-items",
                        "--inline",
                        "test.sarif"
                    },
                    ExpectedExitCode = 1
                },

                new TestCase {
                    Title = "Missing inputFile",
                    Args = new string[] {
                        "file-work-items",
                        "--host-uri",
                        "https://dev.azure.com/my-org/my-project",
                        "--inline"
                    },
                    ExpectedExitCode = 1
                },

                new TestCase {
                    Title = "Relative hostUri",
                    Args = new string[] {
                        "file-work-items",
                       "--host-uri",
                        "dev.azure.com/my-org/my-project",
                        "--inline",
                        "test.sarif"
                    },
                    ExpectedExitCode = 1
                },

                new TestCase {
                    Title = "Both output and inline",
                    Args = new string[] {
                        "file-work-items",
                        "--host-uri",
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
                        "--host-uri",
                        "https://dev.azure.com/my-org/my-project",
                        "test.sarif"
                    },
                    ExpectedExitCode = 0
                }
            };
    }
}
