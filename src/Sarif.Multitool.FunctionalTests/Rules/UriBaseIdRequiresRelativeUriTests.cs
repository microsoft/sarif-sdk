// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class UriBaseIdRequiresRelativeUriTests : ValidationSkimmerTestsBase<UriBaseIdRequiresRelativeUri>
    {
        [Fact(DisplayName = nameof(UriBaseIdRequiresRelativeUri_ReportsInvalidSarif))]
        public void UriBaseIdRequiresRelativeUri_ReportsInvalidSarif() => Verify("Invalid.sarif");

        [Fact(DisplayName = nameof(UriBaseIdRequiresRelativeUri_AcceptsValidSarif))]
        public void UriBaseIdRequiresRelativeUri_AcceptsValidSarif() => Verify("Valid.sarif");
    }
}
