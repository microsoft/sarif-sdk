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

        [Fact]
        public void UseAbsolutePathsForNestedFileUriFragments_Diagnostic_NestedFileUriFragmentIsRelativeInResultFileUri()
        {
            Verify(new UseAbsolutePathsForNestedFileUriFragments(), "NestedFileUriFragmentIsRelativeInResultFileUri.sarif");
        }

        [Fact]
        public void UseAbsolutePathsForNestedFileUriFragments_Diagnostic_NestedFileUriFragmentInCodeFlowLocationUri()
        {
            Verify(new UseAbsolutePathsForNestedFileUriFragments(), "NestedFileUriFragmentIsRelativeInCodeFlowLocationUri.sarif");
        }

        [Fact]
        public void UseAbsolutePathsForNestedFileUriFragments_Diagnostic_NestedFileUriFragmentInStackFrameUri()
        {
            Verify(new UseAbsolutePathsForNestedFileUriFragments(), "NestedFileUriFragmentIsRelativeInStackFrameUri.sarif");
        }

        [Fact]
        public void UseAbsolutePathsForNestedFileUriFragments_Diagnostic_NestedFileUriFragmentIsRelatedRelativeInLocationUri()
        {
            Verify(new UseAbsolutePathsForNestedFileUriFragments(), "NestedFileUriFragmentIsRelativeInRelatedLocationUri.sarif");
        }

        [Fact]
        public void UseAbsolutePathsForNestedFileUriFragments_Diagnostic_NestedFileUriFragmentIsRelativeInFilePropertyName()
        {
            Verify(new UseAbsolutePathsForNestedFileUriFragments(), "NestedFileUriFragmentIsRelativeInFilePropertyName.sarif");
        }
    }
}
