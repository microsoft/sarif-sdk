// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif.Cli.Rules
{
    public static class RuleId
    {
        public const string DoNotUseFriendlyNameAsRuleId = "SV0001";
        public const string UseAbsolutePathsForNestedFileUriFragments = "SV0002";
        public const string UrisMustBeValid = "SV0003";
        public const string AnnotatedCodeLocationIdIsObsolete = "SV0004";
        public const string HashAlgorithmsMustBeUnique = "SV0005";
    }
}
