// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using System;
using System.Text.RegularExpressions;

namespace Microsoft.CodeAnalysis.Sarif.TestUtilities
{
    public static class AssertionExtensions
    {
        public static AndConstraint<StringAssertions> BeCrossPlatformEquivalent(
            this StringAssertions assertion,
            string expected, string because = "", params object[] becauseArgs)
        {
            Execute.Assertion
                .ForCondition(
                    string.Equals(RemoveLineEndings(expected), RemoveLineEndings(assertion.Subject), StringComparison.OrdinalIgnoreCase)
                )
                .BecauseOf(because, becauseArgs)
                .FailWith(TestUtilityResources.BeCrossPlatformEquivalentError);

            return new AndConstraint<StringAssertions>(assertion);
        }

        private static string RemoveLineEndings(string input)
        {
            return Regex.Replace(input, @"\s+", "");
        }
    }
}
