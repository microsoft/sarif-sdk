// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class DoNotUseFriendlyNameAsRuleIdTests : ValidationSkimmerTestsBase<DoNotUseFriendlyNameAsRuleId>
    {
        [Fact(DisplayName = nameof(DoNotUseFriendlyNameAsRuleId_ReportsInvalidSarif))]
        public void DoNotUseFriendlyNameAsRuleId_ReportsInvalidSarif()
        {
            Verify("Invalid.sarif");
        }

        [Fact(DisplayName = nameof(DoNotUseFriendlyNameAsRuleId_AcceptsValidSarif))]
        public void DoNotUseFriendlyNameAsRuleId_AcceptsValidSarif()
        {
            Verify("Valid.sarif");
        }
    }
}
