﻿// Copyright (c) Microsoft. All rights reserved.
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
                        "--project-uri",
                        "https://github.com/my-org/my-project",
                        "--filtering-strategy",
                        "new",
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
                        "new",
                        "--grouping-strategy",
                        "perResult",
                        "test.sarif",
                    }
                },

                // All valid filtering strategies ("new" was covered above).
                new object[] {
                    new[] {
                        "file-work-items",
                        "--project-uri",
                        "https://github.com/my-org/my-project",
                        "--filtering-strategy",
                        "all",
                        "--grouping-strategy",
                        "perResult",
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
                        // Missing project URI.
                        "--filtering-strategy",
                        "new",
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
                        // Missing filtering strategy.
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
                        "new",
                        // Missing grouping strategy.
                        "test.sarif"
                    },
                },

                new object[] {
                    new[] {
                        "file-work-items",
                        "--project-uri",
                        "https://dev.azure.com/my-org/my-project",
                        "--filtering-strategy",
                        "new",
                        "--grouping-strategy",
                        "perResult",
                        // Missing input file path.
                    }
                },

                new object[] {
                    new[] {
                        "file-work-items",
                        "--project-uri",
                        "dev.azure.com/my-org/my-project",     // Relative project URI.
                        "--filtering-strategy",
                        "new",
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
                        "old",                          // Unknown filtering strategy.
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
                        "new",
                        "--grouping-strategy",
                        "perCentury",                   // Unknown grouping strategy.
                        "test.sarif",
                    }
                }
            };
    }
}
