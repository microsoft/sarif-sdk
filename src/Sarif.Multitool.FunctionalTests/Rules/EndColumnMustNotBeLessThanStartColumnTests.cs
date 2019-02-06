// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class EndColumnMustNotBeLessThanStartColumnTests : ValidationSkimmerTestsBase<EndColumnMustNotBeLessThanStartColumn>
    {
        [Fact(DisplayName = nameof(EndColumnMustNotBeLessThanStartColumn_ReportsInvalidSarif))]
        public void EndColumnMustNotBeLessThanStartColumn_ReportsInvalidSarif() => Verify("Invalid.sarif");

        [Fact(DisplayName = nameof(EndColumnMustNotBeLessThanStartColumn_AcceptsValidSarif))]
        public void EndColumnMustNotBeLessThanStartColumn_AcceptsValidSarif() => Verify("Valid.sarif");
    }
}
