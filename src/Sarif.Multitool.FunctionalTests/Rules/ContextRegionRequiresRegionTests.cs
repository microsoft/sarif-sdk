// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class ContextRegionRequiresRegionTests : ValidationSkimmerTestsBase<ContextRegionRequiresRegion>
    {
        [Fact(DisplayName = nameof(ContextRegionRequiresRegion_ReportsInvalidSarif))]
        public void ContextRegionRequiresRegion_ReportsInvalidSarif()
        {
            // We need to disable compatibility transformations for any files that require
            // a malformed schema or malformed JSON, as this code fixes those things up
            Verify("Invalid.sarif", disablePrereleaseCompatibilityTransform: true);
        }

        [Fact(DisplayName = nameof(ContextRegionRequiresRegion_AcceptsValidSarif))]
        public void ContextRegionRequiresRegion_AcceptsValidSarif()
        {
            Verify("Valid.sarif");
        }
    }
}
