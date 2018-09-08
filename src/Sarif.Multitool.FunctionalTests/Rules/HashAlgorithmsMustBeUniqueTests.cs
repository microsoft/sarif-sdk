// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class HashAlgorithmsMustBeUniqueTests : SkimmerTestsBase<HashAlgorithmsMustBeUnique>
    {
        [Fact(DisplayName = nameof(HashAlgorithmsMustBeUnique_ReportsInvalidSarif))]
        public void HashAlgorithmsMustBeUnique_ReportsInvalidSarif()
        {
            Verify("Invalid.sarif");
        }

        [Fact(DisplayName = nameof(HashAlgorithmsMustBeUnique_AcceptsValidSarif))]
        public void HashAlgorithmsMustBeUnique_AcceptsValidSarif()
        {
            Verify("Valid.sarif");
        }
    }
}
