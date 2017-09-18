// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.Sarif.Multitool.Rules;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class UriBaseIdRequiresRelativeUriTests : SkimmerTestsBase
    {
        [Fact(DisplayName = nameof(UriBaseIdRequiresRelativeUri_UrisAreRelative))]
        public void UriBaseIdRequiresRelativeUri_UrisAreRelative()
        {
            Verify(new UriBaseIdRequiresRelativeUri(), "UrisAreRelative.sarif");
        }

        [Fact(DisplayName = nameof(UriBaseIdRequiresRelativeUri_AbsoluteUriInAnalysisTarget))]
        public void UriBaseIdRequiresRelativeUri_AbsoluteUriInAnalysisTarget()
        {
            Verify(new UriBaseIdRequiresRelativeUri(), "AbsoluteUriInAnalysisTarget.sarif");
        }

        [Fact(DisplayName = nameof(UriBaseIdRequiresRelativeUri_AbsoluteUriInCodeFlow))]
        public void UriBaseIdRequiresRelativeUri_AbsoluteUriInCodeFlow()
        {
            Verify(new UriBaseIdRequiresRelativeUri(), "AbsoluteUriInCodeFlow.sarif");
        }

        [Fact(DisplayName = nameof(UriBaseIdRequiresRelativeUri_AbsoluteUriInConfigurationNotification))]
        public void UriBaseIdRequiresRelativeUri_AbsoluteUriInConfigurationNotification()
        {
            Verify(new UriBaseIdRequiresRelativeUri(), "AbsoluteUriInConfigurationNotification.sarif");
        }

        [Fact(DisplayName = nameof(UriBaseIdRequiresRelativeUri_AbsoluteUriInFileChange))]
        public void UriBaseIdRequiresRelativeUri_AbsoluteUriInFileChange()
        {
            Verify(new UriBaseIdRequiresRelativeUri(), "AbsoluteUriInFileChange.sarif");
        }

        [Fact(DisplayName = nameof(UriBaseIdRequiresRelativeUri_AbsoluteUriInRelatedLocation))]
        public void UriBaseIdRequiresRelativeUri_AbsoluteUriInRelatedLocation()
        {
            Verify(new UriBaseIdRequiresRelativeUri(), "AbsoluteUriInRelatedLocation.sarif");
        }

        [Fact(DisplayName = nameof(UriBaseIdRequiresRelativeUri_AbsoluteUriInResultFile))]
        public void UriBaseIdRequiresRelativeUri_AbsoluteUriInResultFile()
        {
            Verify(new UriBaseIdRequiresRelativeUri(), "AbsoluteUriInResultFile.sarif");
        }

        [Fact(DisplayName = nameof(UriBaseIdRequiresRelativeUri_AbsoluteUriInStackFrame))]
        public void UriBaseIdRequiresRelativeUri_AbsoluteUriInStackFrame()
        {
            Verify(new UriBaseIdRequiresRelativeUri(), "AbsoluteUriInStackFrame.sarif");
        }

        [Fact(DisplayName = nameof(UriBaseIdRequiresRelativeUri_AbsoluteUriInToolNotification))]
        public void UriBaseIdRequiresRelativeUri_AbsoluteUriInToolNotification()
        {
            Verify(new UriBaseIdRequiresRelativeUri(), "AbsoluteUriInToolNotification.sarif");
        }
    }
}
