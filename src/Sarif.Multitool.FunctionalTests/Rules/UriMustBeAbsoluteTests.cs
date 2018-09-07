// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class UriMustBeAbsoluteTests : SkimmerTestsBase<UriMustBeAbsolute>
    {
        [Fact(DisplayName = nameof(UriMustBeAbsolute_ReportsInvalidSarif))]
        public void UriMustBeAbsolute_ReportsInvalidSarif()
        {
            Verify("Invalid.sarif");
        }

        [Fact(DisplayName = nameof(UriMustBeAbsolute_AcceptsValidSarif))]
        public void UriMustBeAbsolute_AcceptsValidSarif()
        {
            Verify("Valid.sarif");
        }
    }
}
