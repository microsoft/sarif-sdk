﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public static class RuleId
    {
        public const string DoNotUseFriendlyNameAsRuleId = "SARIF1001";
        public const string UseAbsolutePathsForNestedFileUriFragments = "SARIF1002";
        public const string UrisMustBeValid = "SARIF1003";
        public const string HashAlgorithmsMustBeUnique = "SARIF1006";
        public const string EndTimeMustBeAfterStartTime = "SARIF1007";
        public const string MessagesShouldEndWithPeriod = "SARIF1008";
        public const string StepValuesMustFormOneBasedSequence = "SARIF1009";
        public const string StepMustAppearOnlyInCodeFlowLocations = "SARIF1010";
        public const string ImportanceMustAppearOnlyInCodeFlowLocations = "SARIF1011";
        public const string EndLineMustNotBeLessThanStartLine = "SARIF1012";
        public const string EndColumnMustNotBeLessThanStartColumn = "SARIF1013";
        public const string UriBaseIdRequiresRelativeUri = "SARIF1014";
    }
}
