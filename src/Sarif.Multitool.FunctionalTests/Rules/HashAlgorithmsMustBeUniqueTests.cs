// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class HashAlgorithmsMustBeUniqueTests : SkimmerTestsBase<HashAlgorithmsMustBeUnique>
    {
        [Fact(DisplayName = nameof(HashAlgorithmsMustBeUnique_AcceptsValidSarif))]
        public void HashAlgorithmsMustBeUnique_AcceptsValidSarif()
        {
            Verify("Valid.sarif");
        }

        [Fact(DisplayName = nameof(HashAlgorithmsMustBeUnique_NonUniqueHashAlgorithms))]
        public void HashAlgorithmsMustBeUnique_NonUniqueHashAlgorithms()
        {
            Verify("NonUniqueHashAlgorithms.sarif");
        }
    }
}
