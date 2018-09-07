// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class EndColumnMustNotBeLessThanStartColumnTests : SkimmerTestsBase<EndColumnMustNotBeLessThanStartColumn>
    {
        [Fact(DisplayName = nameof(EndColumnMustNotBeLessThanStartColumn_EndColumnEqualsStartColumn))]
        public void EndColumnMustNotBeLessThanStartColumn_EndColumnEqualsStartColumn()
        {
            Verify("EndColumnEqualsStartColumn.sarif");
        }

        [Fact(DisplayName = nameof(EndColumnMustNotBeLessThanStartColumn_EndColumnEqualsStartColumnNoEndLine))]
        public void EndColumnMustNotBeLessThanStartColumn_EndColumnEqualsStartColumnNoEndLine()
        {
            Verify("EndColumnEqualsStartColumnNoEndLine.sarif");
        }

        [Fact(DisplayName = nameof(EndColumnMustNotBeLessThanStartColumn_EndColumnGreaterThanStartColumn))]
        public void EndColumnMustNotBeLessThanStartColumn_EndColumnGreaterThanStartColumn()
        {
            Verify("EndColumnGreaterThanStartColumn.sarif");
        }

        [Fact(DisplayName = nameof(EndColumnMustNotBeLessThanStartColumn_EndColumnLessThanStartColumnInCodeFlow))]
        public void EndColumnMustNotBeLessThanStartColumn_EndColumnLessThanStartColumnInCodeFlow()
        {
            Verify("EndColumnLessThanStartColumnInCodeFlow.sarif");
        }

        [Fact(DisplayName = nameof(EndColumnMustNotBeLessThanStartColumn_EndColumnLessThanStartColumnInRelatedLocation))]
        public void EndColumnMustNotBeLessThanStartColumn_EndColumnLessThanStartColumnInRelatedLocation()
        {
            Verify("EndColumnLessThanStartColumnInRelatedLocation.sarif");
        }

        [Fact(DisplayName = nameof(EndColumnMustNotBeLessThanStartColumn_EndColumnLessThanStartColumnInResultLocation))]
        public void EndColumnMustNotBeLessThanStartColumn_EndColumnLessThanStartColumnInResultLocation()
        {
            Verify("EndColumnLessThanStartColumnInResultLocation.sarif");
        }

        [Fact(DisplayName = nameof(EndColumnMustNotBeLessThanStartColumn_EndColumnNotSpecified))]
        public void EndColumnMustNotBeLessThanStartColumn_EndColumnNotSpecified()
        {
            Verify("EndColumnNotSpecified.sarif");
        }
    }
}
