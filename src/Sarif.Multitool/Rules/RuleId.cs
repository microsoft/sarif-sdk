// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public static class RuleId
    {
        public const string RuleIdentifiersMustBeValid = "SARIF1001";
        public const string UrisMustBeValid = "SARIF1002";
        public const string EndTimeMustNotBeBeforeStartTime = "SARIF1006";
        public const string MessagesShouldEndWithPeriod = "SARIF2001";
        public const string EndLineMustNotBeLessThanStartLine = "SARIF1012";
        public const string EndColumnMustNotBeLessThanStartColumn = "SARIF1013";
        public const string UriBaseIdRequiresRelativeUri = "SARIF1014";
        public const string UriMustBeAbsolute = "SARIF1015";
        public const string ContextRegionRequiresRegion = "SARIF1016";
        public const string InvalidIndex = "SARIF1017";
        public const string InvalidUriInOriginalUriBaseIds = "SARIF1018";
        public const string RuleIdMustBePresentAndConsistent = "SARIF1019";
        public const string ReferToFinalSchema = "SARIF1020";
    }
}
