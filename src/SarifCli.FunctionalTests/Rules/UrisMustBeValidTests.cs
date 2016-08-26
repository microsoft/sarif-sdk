// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.Sarif.Cli.Rules;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Cli.FunctionalTests.Rules
{
    public class UrisMustBeValidTests : SkimmerTestsBase
    {
        [Fact]
        public void UrisMustBeValid_NoDiagnostic_ValidUris()
        {
            Verify(new UrisMustBeValid(), "ValidUris.sarif");
        }

        [Fact]
        public void UrisMustBeValid_Diagnostic_InvalidHelpUriInRule()
        {
            Verify(new UrisMustBeValid(), "InvalidHelpUriInRule.sarif");
        }

        [Fact]
        public void UrisMustBeValid_Diagnostic_InvalidUriInAnalysisTarget()
        {
            Verify(new UrisMustBeValid(), "InvalidUriInAnalysisTarget.sarif");
        }

        [Fact]
        public void UrisMustBeValid_Diagnostic_InvalidUriInCodeFlowLocation()
        {
            Verify(new UrisMustBeValid(), "InvalidUriInCodeFlowLocation.sarif");
        }

        [Fact]
        public void UrisMustBeValid_Diagnostic_InvalidUriInConfigurationNotification()
        {
            Verify(new UrisMustBeValid(), "InvalidUriInConfigurationNotification.sarif");
        }

        [Fact]
        public void UrisMustBeValid_Diagnostic_InvalidUriInFileChange()
        {
            Verify(new UrisMustBeValid(), "InvalidUriInFileChange.sarif");
        }

        [Fact]
        public void UrisMustBeValid_Diagnostic_InvalidUriInFilePropertyName()
        {
            Verify(new UrisMustBeValid(), "InvalidUriInFilePropertyName.sarif");
        }

        [Fact]
        public void UrisMustBeValid_Diagnostic_InvalidUriInRelatedLocation()
        {
            Verify(new UrisMustBeValid(), "InvalidUriInRelatedLocation.sarif");
        }

        [Fact]
        public void UrisMustBeValid_Diagnostic_InvalidUriInResultFile()
        {
            Verify(new UrisMustBeValid(), "InvalidUriInResultFile.sarif");
        }

        [Fact]
        public void UrisMustBeValid_Diagnostic_InvalidUriInStackFrame()
        {
            Verify(new UrisMustBeValid(), "InvalidUriInStackFrame.sarif");
        }

        [Fact]
        public void UrisMustBeValid_Diagnostic_InvalidUriInToolNotification()
        {
            Verify(new UrisMustBeValid(), "InvalidUriInToolNotification.sarif");
        }
    }
}
