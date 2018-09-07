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
            Verify(new EndLineMustNotBeLessThanStartLine(), "EndLineEqualsStartLine.sarif");
        }

        [Fact(DisplayName = nameof(EndLineMustNotBeLessThanStartLine_EndLineGreaterThanStartLine))]
        public void EndLineMustNotBeLessThanStartLine_EndLineGreaterThanStartLine()
        {
            Verify(new EndLineMustNotBeLessThanStartLine(), "EndLineGreaterThanStartLine.sarif");
        }

        [Fact(DisplayName = nameof(EndLineMustNotBeLessThanStartLine_EndLineLessThanStartLineInCodeFlow))]
        public void EndLineMustNotBeLessThanStartLine_EndLineLessThanStartLineInCodeFlow()
        {
            Verify(new EndLineMustNotBeLessThanStartLine(), "EndLineLessThanStartLineInCodeFlow.sarif");
        }

        [Fact(DisplayName = nameof(EndLineMustNotBeLessThanStartLine_EndLineLessThanStartLineInRelatedLocation))]
        public void EndLineMustNotBeLessThanStartLine_EndLineLessThanStartLineInRelatedLocation()
        {
            Verify(new EndLineMustNotBeLessThanStartLine(), "EndLineLessThanStartLineInRelatedLocation.sarif");
        }

        [Fact(DisplayName = nameof(EndLineMustNotBeLessThanStartLine_EndLineNotSpecified))]
        public void EndLineMustNotBeLessThanStartLine_EndLineNotSpecified()
        {
            Verify(new EndLineMustNotBeLessThanStartLine(), "EndLineNotSpecified.sarif");
        }

        [Fact(DisplayName = nameof(EndLineMustNotBeLessThanStartLine_EndLineLessThanStartLineInResultLocation))]
        public void EndLineMustNotBeLessThanStartLine_EndLineLessThanStartLineInResultLocation()
        {
            Verify(new EndLineMustNotBeLessThanStartLine(), "EndLineLessThanStartLineInResultLocation.sarif");
        }
    }
}
