// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class EndLineMustNotBeLessThanStartLineTests : ValidationSkimmerTestsBase<EndLineMustNotBeLessThanStartLine>
    {
        [Fact(DisplayName = nameof(EndLineMustNotBeLessThanStartLine_ReportsInvalidSarif))]
        public void EndLineMustNotBeLessThanStartLine_ReportsInvalidSarif() => Verify("Invalid.sarif");

        [Fact(DisplayName = nameof(EndLineMustNotBeLessThanStartLine_AcceptsValidSarif))]
        public void EndLineMustNotBeLessThanStartLine_AcceptsValidSarif() => Verify("Valid.sarif");
    }
}
