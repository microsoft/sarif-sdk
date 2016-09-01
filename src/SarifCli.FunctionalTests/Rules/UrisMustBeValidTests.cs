// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.Sarif.Cli.Rules;
using Xunit;

namespace SarifCli.FunctionalTests.Rules
{
    public class UrisMustBeValidTests : SkimmerTestsBase
    {
        [Fact(DisplayName = nameof(UrisMustBeValid_ValidUris))]
        public void UrisMustBeValid_ValidUris()
        {
            Verify(new UrisMustBeValid(), "ValidUris.sarif");
        }

        [Fact(DisplayName = nameof(UrisMustBeValid_InvalidHelpUriInRule))]
        public void UrisMustBeValid_InvalidHelpUriInRule()
        {
            Verify(new UrisMustBeValid(), "InvalidHelpUriInRule.sarif");
        }

        [Fact(DisplayName = nameof(UrisMustBeValid_InvalidUriInAnalysisTarget))]
        public void UrisMustBeValid_InvalidUriInAnalysisTarget()
        {
            Verify(new UrisMustBeValid(), "InvalidUriInAnalysisTarget.sarif");
        }

        [Fact(DisplayName = nameof(UrisMustBeValid_InvalidUriInCodeFlowLocation))]
        public void UrisMustBeValid_InvalidUriInCodeFlowLocation()
        {
            Verify(new UrisMustBeValid(), "InvalidUriInCodeFlowLocation.sarif");
        }

        [Fact(DisplayName = nameof(UrisMustBeValid_InvalidUriInConfigurationNotification))]
        public void UrisMustBeValid_InvalidUriInConfigurationNotification()
        {
            Verify(new UrisMustBeValid(), "InvalidUriInConfigurationNotification.sarif");
        }

        [Fact(DisplayName = nameof(UrisMustBeValid_InvalidUriInFileChange))]
        public void UrisMustBeValid_InvalidUriInFileChange()
        {
            Verify(new UrisMustBeValid(), "InvalidUriInFileChange.sarif");
        }

        [Fact(DisplayName = nameof(UrisMustBeValid_InvalidUriInFilePropertyName))]
        public void UrisMustBeValid_InvalidUriInFilePropertyName()
        {
            Verify(new UrisMustBeValid(), "InvalidUriInFilePropertyName.sarif");
        }

        [Fact(DisplayName = nameof(UrisMustBeValid_InvalidUriInRelatedLocation))]
        public void UrisMustBeValid_InvalidUriInRelatedLocation()
        {
            Verify(new UrisMustBeValid(), "InvalidUriInRelatedLocation.sarif");
        }

        [Fact(DisplayName = nameof(UrisMustBeValid_InvalidUriInResultFile))]
        public void UrisMustBeValid_InvalidUriInResultFile()
        {
            Verify(new UrisMustBeValid(), "InvalidUriInResultFile.sarif");
        }

        [Fact(DisplayName = nameof(UrisMustBeValid_InvalidUriInStackFrame))]
        public void UrisMustBeValid_InvalidUriInStackFrame()
        {
            Verify(new UrisMustBeValid(), "InvalidUriInStackFrame.sarif");
        }

        [Fact(DisplayName = nameof(UrisMustBeValid_InvalidUriInToolNotification))]
        public void UrisMustBeValid_InvalidUriInToolNotification()
        {
            Verify(new UrisMustBeValid(), "InvalidUriInToolNotification.sarif");
        }
    }
}
