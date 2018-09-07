// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class EndLineMustNotBeLessThanStartLineTests : SkimmerTestsBase<EndLineMustNotBeLessThanStartLine>
    {
        [Fact(DisplayName = nameof(EndLineMustNotBeLessThanStartLine_EndLineEqualsStartLine))]
        public void EndLineMustNotBeLessThanStartLine_EndLineEqualsStartLine()
        {
            Verify("EndLineEqualsStartLine.sarif");
        }

        [Fact(DisplayName = nameof(EndLineMustNotBeLessThanStartLine_EndLineGreaterThanStartLine))]
        public void EndLineMustNotBeLessThanStartLine_EndLineGreaterThanStartLine()
        {
            Verify("EndLineGreaterThanStartLine.sarif");
        }

        [Fact(DisplayName = nameof(EndLineMustNotBeLessThanStartLine_EndLineLessThanStartLineInCodeFlow))]
        public void EndLineMustNotBeLessThanStartLine_EndLineLessThanStartLineInCodeFlow()
        {
            Verify("EndLineLessThanStartLineInCodeFlow.sarif");
        }

        [Fact(DisplayName = nameof(EndLineMustNotBeLessThanStartLine_EndLineLessThanStartLineInRelatedLocation))]
        public void EndLineMustNotBeLessThanStartLine_EndLineLessThanStartLineInRelatedLocation()
        {
            Verify("EndLineLessThanStartLineInRelatedLocation.sarif");
        }

        [Fact(DisplayName = nameof(EndLineMustNotBeLessThanStartLine_EndLineNotSpecified))]
        public void EndLineMustNotBeLessThanStartLine_EndLineNotSpecified()
        {
            Verify("EndLineNotSpecified.sarif");
        }

        [Fact(DisplayName = nameof(EndLineMustNotBeLessThanStartLine_EndLineLessThanStartLineInResultLocation))]
        public void EndLineMustNotBeLessThanStartLine_EndLineLessThanStartLineInResultLocation()
        {
            Verify("EndLineLessThanStartLineInResultLocation.sarif");
        }
    }
}
