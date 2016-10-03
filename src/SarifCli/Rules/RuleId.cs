// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif.Cli.Rules
{
    public static class RuleId
    {
        public const string DoNotUseFriendlyNameAsRuleId = "SARIF001";
        public const string UseAbsolutePathsForNestedFileUriFragments = "SARIF002";
        public const string UrisMustBeValid = "SARIF003";
        public const string AnnotatedCodeLocationIdIsObsolete = "SARIF004";
        public const string AnnotatedCodeLocationEssentialIsObsolete = "SARIF005";
        public const string HashAlgorithmsMustBeUnique = "SARIF006";
        public const string EndTimeMustBeAfterStartTime = "SARIF007";
        public const string MessagesShouldEndWithPeriod = "SARIF008";
        public const string StepValuesMustFormOneBasedSequence = "SARIF009";
        public const string StepMustAppearOnlyInCodeFlowLocations = "SARIF010";
        public const string ImportanceMustAppearOnlyInCodeFlowLocations = "SARIF011";
        public const string EndLineMustNotBeLessThanStartLine = "SARIF012";
        public const string EndColumnMustNotBeLessThanStartColumn = "SARIF013";
        public const string UriBaseIdRequiresRelativeUri = "SARIF014";
    }
}
