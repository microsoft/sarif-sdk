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
                    FileDiffingTests.AreEquivalentSarifLogs<SarifLog>(actualSarif: assertion.Subject, expected)
                )
                .BecauseOf(because, becauseArgs)
                .FailWith(TestUtilityResources.BeCrossPlatformEquivalentError);

            return new AndConstraint<StringAssertions>(assertion);
        }
    }
}
