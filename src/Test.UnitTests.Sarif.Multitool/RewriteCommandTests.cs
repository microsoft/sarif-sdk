// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

using CommandLine;

using FluentAssertions;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class RewriteCommandTests
    {
        [Fact]
        public void FileWorkItemsCommand_ArgumentsTests()
        {
            foreach (dynamic testCase in s_testCases)
            {
                this.VerifyCommandLineOptions(testCase.Args, testCase.Valid, testCase.ExpectedOptions);
            }
        }

        private void VerifyCommandLineOptions(IEnumerable<string> args, bool valid, RewriteOptions expected)
        {
            Parser parser = Parser.Default;
            ParserResult<RewriteOptions> result = parser.ParseArguments<RewriteOptions>(args);

            if (valid)
            {
                result.Should().BeOfType<Parsed<RewriteOptions>>();
                result.Should().NotBeNull();
                RewriteOptions parsedResult = ((Parsed<RewriteOptions>)result).Value;
                parsedResult.Should().NotBeNull();
                parsedResult.Should().BeOfType<RewriteOptions>();
                parsedResult.Should().BeEquivalentTo(expected);
            }
            else
            {
                result.Should().BeOfType<NotParsed<RewriteOptions>>();
                result.Should().NotBeNull();
                IEnumerable<CommandLine.Error> errors = ((NotParsed<RewriteOptions>)result).Errors;
                errors.Should().NotBeEmpty();
            }
        }

        private static RewriteOptions SetEmptyValue(RewriteOptions option)
        {
            option.DataToInsert ??= new List<OptionallyEmittedData>();
            option.DataToRemove ??= new List<OptionallyEmittedData>();
            option.UriBaseIds ??= new List<string>();
            option.InsertProperties ??= new List<string>();
            return option;
        }

        private static readonly dynamic[] s_testCases =
            new[] {
                new {
                    Title = "rewrite pass case",
                    Args = new string[] {
                        "test.sarif",
                        "--output",
                        "updated.sarif",
                        "--remove",
                        "VersionControlDetails;NondeterministicProperties",
                        "--sort-results",
                        "--force"
                    },
                    Valid = true,
                    ExpectedOptions = SetEmptyValue(new RewriteOptions
                    {
                        InputFilePath = "test.sarif",
                        OutputFilePath = "updated.sarif",
                        DataToRemove = new List<OptionallyEmittedData>
                        {
                            OptionallyEmittedData.VersionControlDetails,
                            OptionallyEmittedData.NondeterministicProperties
                        },
                        SortResults = true,
                        Force = true,
                    }),
                },
                new {
                    Title = "pass case: different ordering",
                    Args = new string[] {
                        "--output",
                        "updated.sarif",
                        "test.sarif",
                        "--force",
                        "--sort-results",
                    },
                    Valid = true,
                    ExpectedOptions = SetEmptyValue(new RewriteOptions
                    {
                        InputFilePath = "test.sarif",
                        OutputFilePath = "updated.sarif",
                        SortResults = true,
                        Force = true,
                    }),
                },
                new {
                    Title = "pass case: use short argument name",
                    Args = new string[] {
                        "-o",
                        "updated.sarif",
                        "test.sarif",
                        "-s",
                        "-f"
                    },
                    Valid = true,
                    ExpectedOptions = SetEmptyValue(new RewriteOptions
                    {
                        InputFilePath = "test.sarif",
                        OutputFilePath = "updated.sarif",
                        SortResults = true,
                        Force = true,
                    }),
                },
                new {
                    Title = "fail case: argument value not provided",
                    Args = new string[] {
                        "test.sarif",
                        "--output",
                        "updated.sarif",
                        "--remove",
                        "--sort-results",
                        "--force"
                    },
                    Valid = false,
                    ExpectedOptions = (RewriteOptions)null,
                },
                new {
                    Title = "fail case: wrong arg name --sort-result(s)",
                    Args = new string[] {
                        "test.sarif",
                        "--output",
                        "updated.sarif",
                        "--sort-result",
                        "--force"
                    },
                    Valid = false,
                    ExpectedOptions = (RewriteOptions)null,
                },
            };
    }
}
