using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.CodeAnalysis.Sarif
{
    public static class AssertionExtensions
    {
        public static AndConstraint<StringAssertions> BeCrossPlatformEquivalent(
            this StringAssertions assertion,
            string expected, string because = "", params object[] becauseArgs)
        {
            Execute.Assertion
                .ForCondition(string.Equals(expected, assertion.Subject, StringComparison.OrdinalIgnoreCase))
                .BecauseOf(because, becauseArgs)
                .FailWith(TestUtilityResources.BeCrossPlatformEquivalentError);

            return new AndConstraint<StringAssertions>(assertion);
        }
    }
}
