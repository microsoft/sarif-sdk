// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif;
using Xunit;

namespace Microsoft.Sarif.Viewer.VisualStudio.UnitTests
{
    public class SarifErrorListItemTests
    {
        public SarifErrorListItemTests()
        {
            TestUtilities.InitializeTestEnvironment();
        }

        [Fact]
        public void SarifErrorListItem_WhenRegionHasStartLine_HasLineMarker()
        {
            var item = new SarifErrorListItem
            {
                FileName = "file.ext",
                Region = new Region
                {
                    StartLine = 5
                }
            };

            var lineMarker = item.LineMarker;

            lineMarker.Should().NotBe(null);
        }

        [Fact]
        public void SarifErrorListItem_WhenRegionHasNoStartLine_HasNoLineMarker()
        {
            var item = new SarifErrorListItem
            {
                FileName = "file.ext",
                Region = new Region
                {
                    Offset = 20
                }
            };

            var lineMarker = item.LineMarker;

            lineMarker.Should().Be(null);
        }

        [Fact]
        public void SarifErrorListItem_WhenRegionIsAbsent_HasNoLineMarker()
        {
            var item = new SarifErrorListItem
            {
                FileName = "file.ext"
            };

            var lineMarker = item.LineMarker;

            lineMarker.Should().Be(null);
        }

        [Fact]
        public void SarifErrorListItem_WhenMessageIsAbsent_ContainsBlankMessage()
        {
            var result = new Result
            {
            };

            var item = MakeErrorListItem(result);

            item.Message.Should().Be(string.Empty);
        }

        [Fact]
        public void SarifErrorListItem_WhenResultRefersToNonExistentRule_ContainsBlankMessage()
        {
            var result = new Result
            {
                RuleId = "TST0001",
                RuleMessageId = "nonExistentMessageId"
            };

            var run = new Run
            {
                Resources = new CodeAnalysis.Sarif.Resources()
                {
                    Rules = new Dictionary<string, Rule>
                    {
                    }
                }
            };

            var item = MakeErrorListItem(run, result);

            item.Message.Should().Be(string.Empty);
        }

        [Fact]
        public void SarifErrorListItem_WhenResultRefersToRuleWithNoMessageFormats_ContainsBlankMessage()
        {
            var result = new Result
            {
                RuleId = "TST0001",
                RuleMessageId = "nonExistentMessageId"
            };

            var run = new Run
            {
                Resources = new CodeAnalysis.Sarif.Resources()
                {
                    Rules = new Dictionary<string, Rule>
                    {
                        {
                            "TST0001",
                            new Rule
                            {
                                Id = "TST0001"
                            }
                        }
                    }
                }
            };

            var item = MakeErrorListItem(run, result);

            item.Message.Should().Be(string.Empty);
        }

        [Fact]
        public void SarifErrorListItem_WhenResultRefersToNonExistentMessageFormat_ContainsBlankMessage()
        {
            var result = new Result
            {
                RuleId = "TST0001",
                RuleMessageId = "nonExistentFormatId"
            };

            var run = new Run
            {
                Resources = new CodeAnalysis.Sarif.Resources()
                {
                    Rules = new Dictionary<string, Rule>
                    {
                        {
                            "TST0001",
                            new Rule
                            {
                                Id = "TST0001",
                                MessageStrings = new Dictionary<string, string>
                                {
                                    { "realFormatId", "The message" }
                                }
                            }
                        }
                    }
                }
            };

            var item = MakeErrorListItem(run, result);

            item.Message.Should().Be(string.Empty);
        }

        [Fact]
        public void SarifErrorListItem_WhenResultRefersToExistingMessageFormat_ContainsExpectedMessage()
        {
            var result = new Result
            {
                RuleId = "TST0001",
                RuleMessageId = "greeting", 
                Message = new Message()
                {
                    Arguments = new string[]
                    {
                        "Mary"
                    }
                }
            };

            var run = new Run
            {
                Resources = new CodeAnalysis.Sarif.Resources()
                {
                    Rules = new Dictionary<string, Rule>
                    {
                        {
                            "TST0001",
                            new Rule
                            {
                                Id = "TST0001",
                                MessageStrings = new Dictionary<string, string>
                                {
                                    { "greeting", "Hello, {0}!" }
                                }
                            }
                        }
                    }
                }
            };

            var item = MakeErrorListItem(run, result);

            item.Message.Should().Be("Hello, Mary!");
        }

        [Fact]
        public void SarifErrorListItem_WhenFixHasRelativePath_UsesThatPath()
        {
            var result = new Result
            {
                Fixes = new[]
                {
                    new Fix
                    {
                        FileChanges = new[]
                        {
                            new FileChange
                            {
                                FileLocation = new FileLocation
                                {
                                    Uri = new Uri("path/to/file.html", UriKind.Relative)
                                },
                                Replacements = new[]
                                {
                                    new Replacement(0, 0, string.Empty)
                                }
                            }
                        }
                    }
                }
            };

            var item = MakeErrorListItem(result);

            item.Fixes[0].FileChanges[0].FilePath.Should().Be("path/to/file.html");
        }

        [Fact]
        public void SarifErrorListItem_WhenRuleMetadataIsPresent_PopulatesRuleModelFromSarifRule()
        {
            var result = new Result
            {
                RuleId = "TST0001-1"
            };

            var run = new Run
            {
                Resources = new CodeAnalysis.Sarif.Resources()
                {
                    Rules = new Dictionary<string, Rule>
                    {
                        {
                            "TST0001-1",
                            new Rule
                            {
                                Id = "TST0001"
                            }
                        }
                    }
                }
            };

            var item = MakeErrorListItem(run, result);

            item.Rule.Id.Should().Be("TST0001");
        }

        [Fact]
        public void SarifErrorListItem_WhenRuleMetadataIsAbsent_SynthesizesRuleModelFromResultRuleId()
        {
            var result = new Result
            {
                RuleId = "TST0001"
            };

            var run = new Run
            {
                Resources = new CodeAnalysis.Sarif.Resources()
                {
                    Rules = new Dictionary<string, Rule>
                    {
                        // No metadata for rule TST0001.
                    }
                }
            };

            var item = MakeErrorListItem(run, result);

            item.Rule.Id.Should().Be("TST0001");
        }

        [Fact]
        public void SarifErrorListItem_WhenMessageAndFormattedRuleMessageAreAbsentButRuleMetadataIsPresent_ContainsBlankMessage()
        {
            // This test prevents regression of #647,
            // "Viewer NRE when result lacks message/formattedRuleMessage but rule metadata is present"
            var result = new Result
            {
                RuleId = "TST0001"
            };

            var run = new Run
            {
                Resources = new CodeAnalysis.Sarif.Resources()
                {
                    Rules = new Dictionary<string, Rule>
                    {
                        {
                            "TST0001",
                            new Rule
                            {
                                Id = "TST0001"
                            }
                        }
                    }
                }
            };

            var item = MakeErrorListItem(run, result);

            item.Message.Should().Be(string.Empty);
        }

        private static SarifErrorListItem MakeErrorListItem(Result result)
        {
            return MakeErrorListItem(new Run(), result);
        }

        private static SarifErrorListItem MakeErrorListItem(Run run, Result result)
        {
            return new SarifErrorListItem(
                run,
                result,
                "log.sarif",
                new ProjectNameCache(solution: null));
        }
    }
}