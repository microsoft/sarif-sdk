// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.Sarif.Cli.Rules;
using Xunit;

namespace SarifCli.FunctionalTests.Rules
{
    public class HashAlgorithmsMustBeUniqueTests : SkimmerTestsBase
    {
        [Fact(DisplayName = nameof(HashAlgorithmsMustBeUnique_UniqueHashAlgorithms))]
        public void HashAlgorithmsMustBeUnique_UniqueHashAlgorithms()
        {
            Verify(new HashAlgorithmsMustBeUnique(), "UniqueHashAlgorithms.sarif");
        }

        [Fact(DisplayName = nameof(HashAlgorithmsMustBeUnique_NonUniqueHashAlgorithms))]
        public void HashAlgorithmsMustBeUnique_NonUniqueHashAlgorithms()
        {
            Verify(new HashAlgorithmsMustBeUnique(), "NonUniqueHashAlgorithms.sarif");
        }
    }
}
