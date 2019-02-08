// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    // There are far fewer tests of this rule (UrisMustBeValid) than there are of the
    // rule UriBaseIdRequiresRelativeUri, despite the fact that both rules evaluate
    // URI-valued strings wherever they occur in the file format. The reason is that
    // most URI-valued strings occur in the "uri" property of a fileLocation object.
    // The tests for UriBaseIdRequiresRelativeUri exhaustively cover all occurrences
    // of the fileLocation object in the format, so we know that all those "uri"
    // properties will be visited in the course of the analysis. So for this rule,
    // we only need verify that the analysis works for one fileLocation object -- we
    // choose the one in the result.analysisTarget property -- and for any "loose URIs"
    // that occur in the format, such as "rule.helpUri".

    public class UrisMustBeValidTests : ValidationSkimmerTestsBase<UrisMustBeValid>
    {
        [Fact(DisplayName = nameof(UrisMustBeValid_ReportsInvalidSarif))]
        public void UrisMustBeValid_ReportsInvalidSarif() =>
            // We need to disable compatibility transformations for any files that require
            // a malformed schema or malformed JSON, as this code fixes those things up
            Verify("Invalid.sarif", disablePrereleaseCompatibilityTransform: true);

        [Fact(DisplayName = nameof(UrisMustBeValid_AcceptsValidSarif))]

        public void UrisMustBeValid_AcceptsValidSarif() => Verify("Valid.sarif");
    }
}
