// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using Microsoft.CodeAnalysis.Test.Utilities.Sarif;
using System;
using System.Text.RegularExpressions;

namespace Microsoft.CodeAnalysis.Sarif
{
    public static class AssertionExtensions
    {
        /// <summary>
        /// Asserts that two strings are identical except for their end of line conventions.
        /// </summary>
        /// <remarks>
        /// This is useful for comparing logs that might have been generated on platforms with different
        /// end of line conventions.
        /// </remarks>
        public static AndConstraint<StringAssertions> BeCrossPlatformEquivalentStrings(
            this StringAssertions assertion,
            string expected, string because = "", params object[] becauseArgs)
        {
            Execute.Assertion
                .ForCondition(
                    string.Equals(RemoveLineEndings(expected), RemoveLineEndings(assertion.Subject), StringComparison.Ordinal)
                )
                .BecauseOf(because, becauseArgs)
                .FailWith(TestUtilitiesResources.BeCrossPlatformEquivalentError);

            return new AndConstraint<StringAssertions>(assertion);
        }

        private static string RemoveLineEndings(string input)
        {
            return Regex.Replace(input, @"\s+", "");
        }

        public static AndConstraint<StringAssertions> BeCrossPlatformEquivalent<T>(
            this StringAssertions assertion,
            string expected, string because = "", params object[] becauseArgs)
        {
            Execute.Assertion
                .ForCondition(
                    FileDiffingFunctionalTests.AreEquivalent<T>(actualSarif: assertion.Subject, expected)
                )
                .BecauseOf(because, becauseArgs)
                .FailWith(TestUtilitiesResources.BeCrossPlatformEquivalentError);

            return new AndConstraint<StringAssertions>(assertion);
        }
    }
}
