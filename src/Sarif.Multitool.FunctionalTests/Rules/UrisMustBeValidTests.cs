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

    public class UrisMustBeValidTests : SkimmerTestsBase
    {
        [Fact(DisplayName = nameof(UrisMustBeValid_InvalidFileLocationUri))]
        public void UrisMustBeValid_InvalidFileLocationUri()
        {
            Verify(new UrisMustBeValid(), "InvalidFileLocationUri.sarif");
        }

        [Fact(DisplayName = nameof(UrisMustBeValid_InvalidResultWorkItemUri))]
        public void UrisMustBeValid_InvalidResultWorkItemUri()
        {
            Verify(new UrisMustBeValid(), "InvalidResultWorkItemUri.sarif");
        }

        [Fact(DisplayName = nameof(UrisMustBeValid_InvalidRuleHelpUri))]
        public void UrisMustBeValid_InvalidRuleHelpUri()
        {
            Verify(new UrisMustBeValid(), "InvalidRuleHelpUri.sarif");
        }

        [Fact(DisplayName = nameof(UrisMustBeValid_InvalidSarifLogSchemaUri))]
        public void UrisMustBeValid_InvalidSarifLogSchemaUri()
        {
            Verify(new UrisMustBeValid(), "InvalidSarifLogSchemaUri.sarif");
        }

        [Fact(DisplayName = nameof(UrisMustBeValid_InvalidToolDownloadUri))]
        public void UrisMustBeValid_InvalidToolDownloadUri()
        {
            Verify(new UrisMustBeValid(), "InvalidToolDownloadUri.sarif");
        }

        [Fact(DisplayName = nameof(UrisMustBeValid_InvalidUriInFilePropertyName))]
        public void UrisMustBeValid_InvalidUriInFilePropertyName()
        {
            Verify(new UrisMustBeValid(), "InvalidUriInFilePropertyName.sarif");
        }

        [Fact(DisplayName = nameof(UrisMustBeValid_InvalidUriInOriginalUriBaseIds))]
        public void UrisMustBeValid_InvalidUriInOriginalUriBaseIds()
        {
            Verify(new UrisMustBeValid(), "InvalidUriInOriginalUriBaseIds.sarif");
        }

        [Fact(DisplayName = nameof(UrisMustBeValid_InvalidVersionControlDetailsUri))]
        public void UrisMustBeValid_InvalidVersionControlDetailsUri()
        {
            Verify(new UrisMustBeValid(), "InvalidVersionControlDetailsUri.sarif");
        }

        [Fact(DisplayName = nameof(UrisMustBeValid_ValidUris))]

        public void UrisMustBeValid_ValidUris()
        {
            Verify(new UrisMustBeValid(), "ValidUris.sarif");
        }
    }
}
