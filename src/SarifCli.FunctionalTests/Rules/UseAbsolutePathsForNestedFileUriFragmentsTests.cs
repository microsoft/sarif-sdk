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
        public void UseAbsolutePathsForNestedFileUriFragments_Diagnostic_NestedFileUriFragmentIsRelativeInCodeFlowLocationUri()
        {
            Verify(new UseAbsolutePathsForNestedFileUriFragments(), "NestedFileUriFragmentIsRelativeInCodeFlowLocationUri.sarif");
        }

        [Fact]
        public void UseAbsolutePathsForNestedFileUriFragments_Diagnostic_NestedFileUriFragmentIsRelativeInConfigurationNotificationUri()
        {
            Verify(new UseAbsolutePathsForNestedFileUriFragments(), "NestedFileUriFragmentIsRelativeInConfigurationNotificationUri.sarif");
        }

        [Fact]
        public void UseAbsolutePathsForNestedFileUriFragments_Diagnostic_NestedFileUriFragmentIsRelativeInFileChangeUri()
        {
            Verify(new UseAbsolutePathsForNestedFileUriFragments(), "NestedFileUriFragmentIsRelativeInFileChangeUri.sarif");
        }

        [Fact]
        public void UseAbsolutePathsForNestedFileUriFragments_Diagnostic_NestedFileUriFragmentIsRelativeInFilePropertyName()
        {
            Verify(new UseAbsolutePathsForNestedFileUriFragments(), "NestedFileUriFragmentIsRelativeInFilePropertyName.sarif");
        }

        [Fact]
        public void UseAbsolutePathsForNestedFileUriFragments_Diagnostic_NestedFileUriFragmentIsRelativeInRelatedLocationUri()
        {
            Verify(new UseAbsolutePathsForNestedFileUriFragments(), "NestedFileUriFragmentIsRelativeInRelatedLocationUri.sarif");
        }

        [Fact]
        public void UseAbsolutePathsForNestedFileUriFragments_Diagnostic_NestedFileUriFragmentIsRelativeInResultFileUri()
        {
            Verify(new UseAbsolutePathsForNestedFileUriFragments(), "NestedFileUriFragmentIsRelativeInResultFileUri.sarif");
        }

        [Fact]
        public void UseAbsolutePathsForNestedFileUriFragments_Diagnostic_NestedFileUriFragmentIsRelativeInStackFrameUri()
        {
            Verify(new UseAbsolutePathsForNestedFileUriFragments(), "NestedFileUriFragmentIsRelativeInStackFrameUri.sarif");
        }

        [Fact]
        public void UseAbsolutePathsForNestedFileUriFragments_Diagnostic_NestedFileUriFragmentIsRelativeInToolNotificationUri()
        {
            Verify(new UseAbsolutePathsForNestedFileUriFragments(), "NestedFileUriFragmentIsRelativeInToolNotificationUri.sarif");
        }
    }
}
