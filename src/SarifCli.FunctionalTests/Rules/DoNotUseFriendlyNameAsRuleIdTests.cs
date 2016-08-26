// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.Sarif.Cli.Rules;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Cli.FunctionalTests.Rules
{
    public class DoNotUseFriendlyNameAsRuleIdTests : SkimmerTestsBase
    {
        [Fact]
        public void DoNotUseFriendlyNameAsRuleId_NoDiagnostic_RuleIdDoesNotMatchFriendlyName()
        {
            Verify(new DoNotUseFriendlyNameAsRuleId(), "RuleIdDoesNotMatchFriendlyName.sarif");
        }

        [Fact]
        public void DoNotUseFriendlyNameAsRuleId_Diagnostic_RuleIdMatchesFriendlyName()
        {
            Verify(new DoNotUseFriendlyNameAsRuleId(), "RuleIdMatchesFriendlyName.sarif");
        }
    }
}
