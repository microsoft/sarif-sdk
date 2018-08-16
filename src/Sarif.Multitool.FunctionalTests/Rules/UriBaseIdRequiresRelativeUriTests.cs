// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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

        [Fact(DisplayName = nameof(UriBaseIdRequiresRelativeUri_AbsoluteUriInConversionAnalysisToolLogFiles))]
        public void UriBaseIdRequiresRelativeUri_AbsoluteUriInConversionAnalysisToolLogFiles()
        {
            Verify(new UriBaseIdRequiresRelativeUri(), "AbsoluteUriInConversionAnalysisToolLogFiles.sarif");
        }

        [Fact(DisplayName = nameof(UriBaseIdRequiresRelativeUri_AbsoluteUriInFileChange))]
        public void UriBaseIdRequiresRelativeUri_AbsoluteUriInFileChange()
        {
            Verify(new UriBaseIdRequiresRelativeUri(), "AbsoluteUriInFileChange.sarif");
        }

        [Fact(DisplayName = nameof(UriBaseIdRequiresRelativeUri_AbsoluteUriInFilesDictionary))]
        public void UriBaseIdRequiresRelativeUri_AbsoluteUriInFilesDictionary()
        {
            Verify(new UriBaseIdRequiresRelativeUri(), "AbsoluteUriInFilesDictionary.sarif");
        }

        [Fact(DisplayName = nameof(UriBaseIdRequiresRelativeUri_AbsoluteUriInInvocationAttachment))]
        public void UriBaseIdRequiresRelativeUri_AbsoluteUriInInvocationAttachment()
        {
            Verify(new UriBaseIdRequiresRelativeUri(), "AbsoluteUriInInvocationAttachment.sarif");
        }

        [Fact(DisplayName = nameof(UriBaseIdRequiresRelativeUri_AbsoluteUriInInvocationExecutableLocation))]
        public void UriBaseIdRequiresRelativeUri_AbsoluteUriInInvocationExecutableLocation()
        {
            Verify(new UriBaseIdRequiresRelativeUri(), "AbsoluteUriInInvocationExecutableLocation.sarif");
        }

        [Fact(DisplayName = nameof(UriBaseIdRequiresRelativeUri_AbsoluteUriInInvocationResponseFile))]
        public void UriBaseIdRequiresRelativeUri_AbsoluteUriInInvocationResponseFile()
        {
            Verify(new UriBaseIdRequiresRelativeUri(), "AbsoluteUriInInvocationResponseFile.sarif");
        }

        [Fact(DisplayName = nameof(UriBaseIdRequiresRelativeUri_AbsoluteUriInInvocationStandardStreams))]
        public void UriBaseIdRequiresRelativeUri_AbsoluteUriInInvocationStandardStreams()
        {
            Verify(new UriBaseIdRequiresRelativeUri(), "AbsoluteUriInInvocationStandardStreams.sarif");
        }

        [Fact(DisplayName = nameof(UriBaseIdRequiresRelativeUri_AbsoluteUriInRelatedLocation))]
        public void UriBaseIdRequiresRelativeUri_AbsoluteUriInRelatedLocation()
        {
            Verify(new UriBaseIdRequiresRelativeUri(), "AbsoluteUriInRelatedLocation.sarif");
        }

        [Fact(DisplayName = nameof(UriBaseIdRequiresRelativeUri_AbsoluteUriInResultAttachment))]
        public void UriBaseIdRequiresRelativeUri_AbsoluteUriInResultAttachment()
        {
            Verify(new UriBaseIdRequiresRelativeUri(), "AbsoluteUriInResultAttachment.sarif");
        }

        [Fact(DisplayName = nameof(UriBaseIdRequiresRelativeUri_AbsoluteUriInResultConversionProvenance))]
        public void UriBaseIdRequiresRelativeUri_AbsoluteUriInResultConversionProvenance()
        {
            Verify(new UriBaseIdRequiresRelativeUri(), "AbsoluteUriInResultConversionProvenance.sarif");
        }

        [Fact(DisplayName = nameof(UriBaseIdRequiresRelativeUri_AbsoluteUriInResultGraphNode))]
        public void UriBaseIdRequiresRelativeUri_AbsoluteUriInResultGraphNode()
        {
            Verify(new UriBaseIdRequiresRelativeUri(), "AbsoluteUriInResultGraphNode.sarif");
        }

        [Fact(DisplayName = nameof(UriBaseIdRequiresRelativeUri_AbsoluteUriInResultLocation))]
        public void UriBaseIdRequiresRelativeUri_AbsoluteUriInResultLocation()
        {
            Verify(new UriBaseIdRequiresRelativeUri(), "AbsoluteUriInResultLocation.sarif");
        }

        [Fact(DisplayName = nameof(UriBaseIdRequiresRelativeUri_AbsoluteUriInRunGraphNode))]
        public void UriBaseIdRequiresRelativeUri_AbsoluteUriInRunGraphNode()
        {
            Verify(new UriBaseIdRequiresRelativeUri(), "AbsoluteUriInRunGraphNode.sarif");
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
