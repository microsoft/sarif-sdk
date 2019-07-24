// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif.Multitool;
using Xunit;

namespace Microsoft.CodeAnalysis.Test.UnitTests.Sarif.Multitool
{
    public class FileWorkItemsCommandTests
    {
        [Theory]
        [MemberData(nameof(ValidTestCases))]
        public void FileWorkItemsCommand_AcceptsValidCommandLines(string[] args)
        {
            int exitCode = Program.Main(args);

            exitCode.Should().Be(0);
        }

        [Theory]
        [MemberData(nameof(InvalidTestCases))]
        public void FileWorkItemsCommand_RejectsInvalidCommandLines(string[] args)
        {
            int exitCode = Program.Main(args);

            exitCode.Should().Be(1);
        }

        public static IEnumerable<object[]> ValidTestCases =>
            new List<object[]> {
                new object[] {
                    new[] {
                        "file-work-items",
                        "--test-option-validation",
                        "--project-uri",
                        "https://github.com/my-org/my-project",
                        "--filtering-strategy",
                        "NewResults",
                        "--grouping-strategy",
                        "PerResult",
                        "test.sarif",
                    }
                },

                new object[] {
                    new[] {
                        "file-work-items",
                        "--test-option-validation",
                        "--project-uri",
                        "https://dev.azure.com/my-org/my-project",
                        "--filtering-strategy",
                        "NewResults",
                        "--grouping-strategy",
                        "PerResult",
                        "test.sarif",
                    }
                },

                // All valid filtering strategies ("new" was covered above).
                new object[] {
                    new[] {
                        "file-work-items",
                        "--test-option-validation",
                        "--project-uri",
                        "https://github.com/my-org/my-project",
                        "--filtering-strategy",
                        "AllResults",
                        "--grouping-strategy",
                        "PerResult",
                        "test.sarif",
                    }
                }

                // There is only one grouping strategy so we don't need any more tests.
            };

        public static IEnumerable<object[]> InvalidTestCases =>
            new List<object[]> {
                new object[] {
                    new[] {
                        "file-work-items",
                        "--test-option-validation",
                        // Missing project URI.
                        "--filtering-strategy",
                        "NewResults",
                        "--grouping-strategy",
                        "PerResult",
                        "test.sarif",
                    }
                },

                new object[] {
                    new[] {
                        "file-work-items",
                        "--test-option-validation",
                        "--project-uri",
                        "https://dev.azure.com/my-org/my-project",
                        // Missing filtering strategy.
                        "--grouping-strategy",
                        "PerResult",
                        "test.sarif",
                    }
                },

                new object[] {
                    new[] {
                        "file-work-items",
                        "--test-option-validation",
                        "--project-uri",
                        "https://dev.azure.com/my-org/my-project",
                        "--filtering-strategy",
                        "NewResults",
                        // Missing grouping strategy.
                        "test.sarif"
                    },
                },

                new object[] {
                    new[] {
                        "file-work-items",
                        "--test-option-validation",
                        "--project-uri",
                        "https://dev.azure.com/my-org/my-project",
                        "--filtering-strategy",
                        "NewResults",
                        "--grouping-strategy",
                        "PerResult",
                        // Missing input file path.
                    }
                },

                new object[] {
                    new[] {
                        "file-work-items",
                        "--test-option-validation",
                       "--project-uri",
                        "dev.azure.com/my-org/my-project",     // Relative project URI.
                        "--filtering-strategy",
                        "NewResults",
                        "--grouping-strategy",
                        "PerResult",
                        "test.sarif",
                    }
                },

                new object[] {
                    new[] {
                        "file-work-items",
                        "--test-option-validation",
                        "--project-uri",
                        "https://dev.azure.com/my-org/my-project",
                        "--filtering-strategy",
                        "OldResults",                          // Unknown filtering strategy.
                        "--grouping-strategy",
                        "perResult",
                        "test.sarif",
                    }
                },

                new object[] {
                    new[] {
                        "file-work-items",
                        "--project-uri",
                        "https://dev.azure.com/my-org/my-project",
                        "--filtering-strategy",
                        "NewResults",
                        "--grouping-strategy",
                        "PerCentury",                   // Unknown grouping strategy.
                        "test.sarif",
                    }
                }
            };
    }
}
