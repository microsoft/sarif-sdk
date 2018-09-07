// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class UriBaseIdRequiresRelativeUriTests : SkimmerTestsBase<UriBaseIdRequiresRelativeUri>
    {
        [Fact(DisplayName = nameof(UriBaseIdRequiresRelativeUri_UrisAreRelative))]
        public void UriBaseIdRequiresRelativeUri_UrisAreRelative()
        {
            Verify("UrisAreRelative.sarif");
        }

        [Fact(DisplayName = nameof(UriBaseIdRequiresRelativeUri_AbsoluteUriInAnalysisTarget))]
        public void UriBaseIdRequiresRelativeUri_AbsoluteUriInAnalysisTarget()
        {
            Verify("AbsoluteUriInAnalysisTarget.sarif");
        }

        [Fact(DisplayName = nameof(UriBaseIdRequiresRelativeUri_AbsoluteUriInCodeFlow))]
        public void UriBaseIdRequiresRelativeUri_AbsoluteUriInCodeFlow()
        {
            Verify("AbsoluteUriInCodeFlow.sarif");
        }

        [Fact(DisplayName = nameof(UriBaseIdRequiresRelativeUri_AbsoluteUriInConfigurationNotification))]
        public void UriBaseIdRequiresRelativeUri_AbsoluteUriInConfigurationNotification()
        {
            Verify("AbsoluteUriInConfigurationNotification.sarif");
        }

        [Fact(DisplayName = nameof(UriBaseIdRequiresRelativeUri_AbsoluteUriInConversionAnalysisToolLogFiles))]
        public void UriBaseIdRequiresRelativeUri_AbsoluteUriInConversionAnalysisToolLogFiles()
        {
            Verify("AbsoluteUriInConversionAnalysisToolLogFiles.sarif");
        }

        [Fact(DisplayName = nameof(UriBaseIdRequiresRelativeUri_AbsoluteUriInFileChange))]
        public void UriBaseIdRequiresRelativeUri_AbsoluteUriInFileChange()
        {
            Verify("AbsoluteUriInFileChange.sarif");
        }

        [Fact(DisplayName = nameof(UriBaseIdRequiresRelativeUri_AbsoluteUriInFilesDictionary))]
        public void UriBaseIdRequiresRelativeUri_AbsoluteUriInFilesDictionary()
        {
            Verify("AbsoluteUriInFilesDictionary.sarif");
        }

        [Fact(DisplayName = nameof(UriBaseIdRequiresRelativeUri_AbsoluteUriInInvocationAttachment))]
        public void UriBaseIdRequiresRelativeUri_AbsoluteUriInInvocationAttachment()
        {
            Verify("AbsoluteUriInInvocationAttachment.sarif");
        }

        [Fact(DisplayName = nameof(UriBaseIdRequiresRelativeUri_AbsoluteUriInInvocationExecutableLocation))]
        public void UriBaseIdRequiresRelativeUri_AbsoluteUriInInvocationExecutableLocation()
        {
            Verify("AbsoluteUriInInvocationExecutableLocation.sarif");
        }

        [Fact(DisplayName = nameof(UriBaseIdRequiresRelativeUri_AbsoluteUriInInvocationResponseFile))]
        public void UriBaseIdRequiresRelativeUri_AbsoluteUriInInvocationResponseFile()
        {
            Verify("AbsoluteUriInInvocationResponseFile.sarif");
        }

        [Fact(DisplayName = nameof(UriBaseIdRequiresRelativeUri_AbsoluteUriInInvocationStandardStreams))]
        public void UriBaseIdRequiresRelativeUri_AbsoluteUriInInvocationStandardStreams()
        {
            Verify("AbsoluteUriInInvocationStandardStreams.sarif");
        }

        [Fact(DisplayName = nameof(UriBaseIdRequiresRelativeUri_AbsoluteUriInRelatedLocation))]
        public void UriBaseIdRequiresRelativeUri_AbsoluteUriInRelatedLocation()
        {
            Verify("AbsoluteUriInRelatedLocation.sarif");
        }

        [Fact(DisplayName = nameof(UriBaseIdRequiresRelativeUri_AbsoluteUriInResultAttachment))]
        public void UriBaseIdRequiresRelativeUri_AbsoluteUriInResultAttachment()
        {
            Verify("AbsoluteUriInResultAttachment.sarif");
        }

        [Fact(DisplayName = nameof(UriBaseIdRequiresRelativeUri_AbsoluteUriInResultConversionProvenance))]
        public void UriBaseIdRequiresRelativeUri_AbsoluteUriInResultConversionProvenance()
        {
            Verify("AbsoluteUriInResultConversionProvenance.sarif");
        }

        [Fact(DisplayName = nameof(UriBaseIdRequiresRelativeUri_AbsoluteUriInResultGraphNode))]
        public void UriBaseIdRequiresRelativeUri_AbsoluteUriInResultGraphNode()
        {
            Verify("AbsoluteUriInResultGraphNode.sarif");
        }

        [Fact(DisplayName = nameof(UriBaseIdRequiresRelativeUri_AbsoluteUriInResultLocation))]
        public void UriBaseIdRequiresRelativeUri_AbsoluteUriInResultLocation()
        {
            Verify("AbsoluteUriInResultLocation.sarif");
        }

        [Fact(DisplayName = nameof(UriBaseIdRequiresRelativeUri_AbsoluteUriInRunGraphNode))]
        public void UriBaseIdRequiresRelativeUri_AbsoluteUriInRunGraphNode()
        {
            Verify("AbsoluteUriInRunGraphNode.sarif");
        }

        [Fact(DisplayName = nameof(UriBaseIdRequiresRelativeUri_AbsoluteUriInStackFrame))]
        public void UriBaseIdRequiresRelativeUri_AbsoluteUriInStackFrame()
        {
            Verify("AbsoluteUriInStackFrame.sarif");
        }

        [Fact(DisplayName = nameof(UriBaseIdRequiresRelativeUri_AbsoluteUriInToolNotification))]
        public void UriBaseIdRequiresRelativeUri_AbsoluteUriInToolNotification()
        {
            Verify("AbsoluteUriInToolNotification.sarif");
        }
    }
}
