// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.Sarif.Cli.Rules;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Cli.FunctionalTests.Rules
{
    public class HashAlgorithmsMustBeUniqueTests : SkimmerTestsBase
    {
        [Fact]
        public void HashAlgorithmsMustBeUnique_NoDiagnostic_UniqueHashAlgorithms()
        {
            Verify(new HashAlgorithmsMustBeUnique(), "UniqueHashAlgorithms.sarif");
        }

        [Fact]
        public void HashAlgorithmsMustBeUnique_Diagnostic_NonUniqueHashAlgorithms()
        {
            Verify(new HashAlgorithmsMustBeUnique(), "NonUniqueHashAlgorithms.sarif");
        }
    }
}
