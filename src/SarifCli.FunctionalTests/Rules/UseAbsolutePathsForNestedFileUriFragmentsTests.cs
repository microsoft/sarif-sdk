// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.Sarif.Cli.Rules;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Cli.FunctionalTests.Rules
{
    public class UseAbsolutePathsForNestedFileUriFragmentsTests : SkimmerTestsBase
    {
        [Fact]
        public void UseAbsolutePathsForNestedFileUriFragments_NoDiagnostic_NestedFileUriFragmentsAreAbsolute()
        {
            Verify(new UseAbsolutePathsForNestedFileUriFragments(), "NestedFileUriFragmentsAreAbsolute.sarif");
        }

        [Fact]
        public void UseAbsolutePathsForNestedFileUriFragments_Diagnostic_NestedFileUriFragmentIsRelativeInAnalysisTargetUri()
        {
            Verify(new UseAbsolutePathsForNestedFileUriFragments(), "NestedFileUriFragmentIsRelativeInAnalysisTargetUri.sarif");
        }

        // TODO Add a test for ResultFile.Uri
        // TODO Add tests for other locations, like annotatedCodeLocation.Uri.

        [Fact]
        public void UseAbsolutePathsForNestedFileUriFragments_Diagnostic_NestedFileUriFragmentIsRelativeInFilePropertyName()
        {
            Verify(new UseAbsolutePathsForNestedFileUriFragments(), "NestedFileUriFragmentIsRelativeInFilePropertyName.sarif");
        }
    }
}
