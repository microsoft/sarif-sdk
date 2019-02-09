// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class UriMustBeAbsoluteTests : ValidationSkimmerTestsBase<UriMustBeAbsolute>
    {
        [Fact(DisplayName = nameof(UriMustBeAbsolute_ReportsInvalidSarif))]
        public void UriMustBeAbsolute_ReportsInvalidSarif()
        {
            // We need to disable compatibility transformations for any files that require
            // a malformed schema or malformed JSON, as this code fixes those things up.
            //
            // As a result, when this test fails, you must open the test file below
            // and manually bring it into conformance with the current v2 spec.
            Verify("Invalid.sarif", disablePrereleaseCompatibilityTransform: true);
        }

        [Fact(DisplayName = nameof(UriMustBeAbsolute_AcceptsValidSarif))]
        public void UriMustBeAbsolute_AcceptsValidSarif()
        {
            Verify("Valid.sarif");
        }
    }
}
