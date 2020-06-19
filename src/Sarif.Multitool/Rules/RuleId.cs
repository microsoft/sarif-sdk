// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public static class RuleId
    {
        public const string RuleIdentifiersMustBeValid = "SARIF1001";
        public const string UrisMustBeValid = "SARIF1002";
        public const string InvocationPropertiesMustBeConsistent = "SARIF1006";
        public const string AuthorHighQualityMessages = "SARIF2001";
        public const string EndLineMustNotBeLessThanStartLine = "SARIF1012";
        public const string EndColumnMustNotBeLessThanStartColumn = "SARIF1013";
        public const string UriBaseIdRequiresRelativeUri = "SARIF1014";
        public const string UriMustBeAbsolute = "SARIF1005";
        public const string PhysicalLocationPropertiesMustBeConsistent = "SARIF1008";
        public const string IndexPropertiesMustBeConsistentWithArrays = "SARIF1009";
        public const string InvalidUriInOriginalUriBaseIds = "SARIF1018";
        public const string RuleIdMustBeConsistent = "SARIF1010";
        public const string ReferenceFinalSchema = "SARIF1011";
        public const string ProvideSchema = "SARIF2008";
    }
}
