// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif;
using Xunit;

namespace Microsoft.CodeAnalysis.Driver
{
    public class RuleUtilitiesTests
    {
        [Fact]
        public void BuildResult_BuildsExpectedResult()
        {
            // Arrange
            const string RuleMessageId = "Default";
            const string RuleId = "TST0001";
            string[] Arguments = new string[] { "42", "54" };

            var context = new TestAnalysisContext
            {
                TargetUri = new System.Uri("file:///c:/src/file.c"),
                Rule = new Rule
                {
                    Id = RuleId,
                    MessageStrings = new Dictionary<string, string>
                    {
                        [RuleMessageId] = "Expected {0} but got {1}."
                    }
                }
            };

            var region = new Region
            {
                StartLine = 42
            };

            (context.RuntimeErrors & RuntimeConditions.OneOrMoreWarningsFired).Should().Be(RuntimeConditions.None);
            (context.RuntimeErrors & RuntimeConditions.OneOrMoreErrorsFired).Should().Be(RuntimeConditions.None);

            // Act.
            Result result = RuleUtilities.BuildResult(
                ResultLevel.Error,
                context,
                region,
                RuleMessageId,
                Arguments);

            // Assert.
            result.RuleId.Should().Be(RuleId);

            result.Message.MessageId.Should().Be(RuleMessageId);

            result.Message.Arguments.Count.Should().Be(Arguments.Length);
            result.Message.Arguments[0].Should().Be(Arguments[0]);
            result.Message.Arguments[1].Should().Be(Arguments[1]);

            result.Locations.Count.Should().Be(1);
            result.Locations[0].PhysicalLocation.Region.ValueEquals(region).Should().BeTrue();

            (context.RuntimeErrors & RuntimeConditions.OneOrMoreWarningsFired).Should().Be(RuntimeConditions.None);
            (context.RuntimeErrors & RuntimeConditions.OneOrMoreErrorsFired).Should().Be(RuntimeConditions.OneOrMoreErrorsFired);

            result = RuleUtilities.BuildResult(
                ResultLevel.Warning,
                context,
                region,
                RuleMessageId,
                Arguments);

            (context.RuntimeErrors & RuntimeConditions.OneOrMoreWarningsFired).Should().Be(RuntimeConditions.OneOrMoreWarningsFired);
        }
    }
}
