// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.Sarif.Cli.Rules;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Cli.FunctionalTests.Rules
{
    public class DoNotUseFriendlyNameAsRuleIdTests : SkimmerTestsBase
    {
        [Fact(DisplayName = nameof(DoNotUseFriendlyNameAsRuleId_RuleIdDoesNotMatchFriendlyName))]
        public void DoNotUseFriendlyNameAsRuleId_RuleIdDoesNotMatchFriendlyName()
        {
            Verify(new DoNotUseFriendlyNameAsRuleId(), "RuleIdDoesNotMatchFriendlyName.sarif");
        }

        [Fact(DisplayName = nameof(DoNotUseFriendlyNameAsRuleId_RuleIdMatchesFriendlyName))]
        public void DoNotUseFriendlyNameAsRuleId_RuleIdMatchesFriendlyName()
        {
            Verify(new DoNotUseFriendlyNameAsRuleId(), "RuleIdMatchesFriendlyName.sarif");
        }
    }
}
