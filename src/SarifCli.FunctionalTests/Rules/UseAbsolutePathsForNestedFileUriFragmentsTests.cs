// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.Sarif.Cli.Rules;
using Xunit;

namespace SarifCli.FunctionalTests.Rules
{
    public class UseAbsolutePathsForNestedFileUriFragmentsTests : SkimmerTestsBase
    {
        [Fact(DisplayName = nameof(UseAbsolutePathsForNestedFileUriFragments_NestedFileUriFragmentsAreAbsolute))]
        public void UseAbsolutePathsForNestedFileUriFragments_NestedFileUriFragmentsAreAbsolute()
        {
            Verify(new UseAbsolutePathsForNestedFileUriFragments(), "NestedFileUriFragmentsAreAbsolute.sarif");
        }

        [Fact(DisplayName = nameof(UseAbsolutePathsForNestedFileUriFragments_NestedFileUriFragmentIsRelativeInAnalysisTargetUri))]
        public void UseAbsolutePathsForNestedFileUriFragments_NestedFileUriFragmentIsRelativeInAnalysisTargetUri()
        {
            Verify(new UseAbsolutePathsForNestedFileUriFragments(), "NestedFileUriFragmentIsRelativeInAnalysisTargetUri.sarif");
        }

        [Fact(DisplayName = nameof(UseAbsolutePathsForNestedFileUriFragments_NestedFileUriFragmentIsRelativeInCodeFlowLocationUri))]
        public void UseAbsolutePathsForNestedFileUriFragments_NestedFileUriFragmentIsRelativeInCodeFlowLocationUri()
        {
            Verify(new UseAbsolutePathsForNestedFileUriFragments(), "NestedFileUriFragmentIsRelativeInCodeFlowLocationUri.sarif");
        }

        [Fact(DisplayName = nameof(UseAbsolutePathsForNestedFileUriFragments_NestedFileUriFragmentIsRelativeInConfigurationNotificationUri))]
        public void UseAbsolutePathsForNestedFileUriFragments_NestedFileUriFragmentIsRelativeInConfigurationNotificationUri()
        {
            Verify(new UseAbsolutePathsForNestedFileUriFragments(), "NestedFileUriFragmentIsRelativeInConfigurationNotificationUri.sarif");
        }

        [Fact(DisplayName = nameof(UseAbsolutePathsForNestedFileUriFragments_NestedFileUriFragmentIsRelativeInFileChangeUri))]
        public void UseAbsolutePathsForNestedFileUriFragments_NestedFileUriFragmentIsRelativeInFileChangeUri()
        {
            Verify(new UseAbsolutePathsForNestedFileUriFragments(), "NestedFileUriFragmentIsRelativeInFileChangeUri.sarif");
        }

        [Fact(DisplayName = nameof(UseAbsolutePathsForNestedFileUriFragments_NestedFileUriFragmentIsRelativeInFilePropertyName))]
        public void UseAbsolutePathsForNestedFileUriFragments_NestedFileUriFragmentIsRelativeInFilePropertyName()
        {
            Verify(new UseAbsolutePathsForNestedFileUriFragments(), "NestedFileUriFragmentIsRelativeInFilePropertyName.sarif");
        }

        [Fact(DisplayName = nameof(UseAbsolutePathsForNestedFileUriFragments_NestedFileUriFragmentIsRelativeInRelatedLocationUri))]
        public void UseAbsolutePathsForNestedFileUriFragments_NestedFileUriFragmentIsRelativeInRelatedLocationUri()
        {
            Verify(new UseAbsolutePathsForNestedFileUriFragments(), "NestedFileUriFragmentIsRelativeInRelatedLocationUri.sarif");
        }

        [Fact(DisplayName = nameof(UseAbsolutePathsForNestedFileUriFragments_NestedFileUriFragmentIsRelativeInResultFileUri))]
        public void UseAbsolutePathsForNestedFileUriFragments_NestedFileUriFragmentIsRelativeInResultFileUri()
        {
            Verify(new UseAbsolutePathsForNestedFileUriFragments(), "NestedFileUriFragmentIsRelativeInResultFileUri.sarif");
        }

        [Fact(DisplayName = nameof(UseAbsolutePathsForNestedFileUriFragments_NestedFileUriFragmentIsRelativeInStackFrameUri))]
        public void UseAbsolutePathsForNestedFileUriFragments_NestedFileUriFragmentIsRelativeInStackFrameUri()
        {
            Verify(new UseAbsolutePathsForNestedFileUriFragments(), "NestedFileUriFragmentIsRelativeInStackFrameUri.sarif");
        }

        [Fact(DisplayName = nameof(UseAbsolutePathsForNestedFileUriFragments_NestedFileUriFragmentIsRelativeInToolNotificationUri))]
        public void UseAbsolutePathsForNestedFileUriFragments_NestedFileUriFragmentIsRelativeInToolNotificationUri()
        {
            Verify(new UseAbsolutePathsForNestedFileUriFragments(), "NestedFileUriFragmentIsRelativeInToolNotificationUri.sarif");
        }
    }
}
