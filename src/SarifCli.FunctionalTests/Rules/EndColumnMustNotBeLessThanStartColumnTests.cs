// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.Sarif.Cli.Rules;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Cli.FunctionalTests.Rules
{
    public class EndColumnMustNotBeLessThanStartColumnTests : SkimmerTestsBase
    {
        [Fact(DisplayName = nameof(EndColumnMustNotBeLessThanStartColumn_EndColumnEqualsStartColumn))]
        public void EndColumnMustNotBeLessThanStartColumn_EndColumnEqualsStartColumn()
        {
            Verify(new EndColumnMustNotBeLessThanStartColumn(), "EndColumnEqualsStartColumn.sarif");
        }

        [Fact(DisplayName = nameof(EndColumnMustNotBeLessThanStartColumn_EndColumnEqualsStartColumnNoEndLine))]
        public void EndColumnMustNotBeLessThanStartColumn_EndColumnEqualsStartColumnNoEndLine()
        {
            Verify(new EndColumnMustNotBeLessThanStartColumn(), "EndColumnEqualsStartColumnNoEndLine.sarif");
        }

        [Fact(DisplayName = nameof(EndColumnMustNotBeLessThanStartColumn_EndColumnGreaterThanStartColumn))]
        public void EndColumnMustNotBeLessThanStartColumn_EndColumnGreaterThanStartColumn()
        {
            Verify(new EndColumnMustNotBeLessThanStartColumn(), "EndColumnGreaterThanStartColumn.sarif");
        }

        [Fact(DisplayName = nameof(EndColumnMustNotBeLessThanStartColumn_EndColumnLessThanStartColumnInAnalysisTarget))]
        public void EndColumnMustNotBeLessThanStartColumn_EndColumnLessThanStartColumnInAnalysisTarget()
        {
            Verify(new EndColumnMustNotBeLessThanStartColumn(), "EndColumnLessThanStartColumnInAnalysisTarget.sarif");
        }

        [Fact(DisplayName = nameof(EndColumnMustNotBeLessThanStartColumn_EndColumnLessThanStartColumnInAnalysisTargetNoEndLine))]
        public void EndColumnMustNotBeLessThanStartColumn_EndColumnLessThanStartColumnInAnalysisTargetNoEndLine()
        {
            Verify(new EndColumnMustNotBeLessThanStartColumn(), "EndColumnLessThanStartColumnInAnalysisTargetNoEndLine.sarif");
        }

        [Fact(DisplayName = nameof(EndColumnMustNotBeLessThanStartColumn_EndColumnLessThanStartColumnInCodeFlow))]
        public void EndColumnMustNotBeLessThanStartColumn_EndColumnLessThanStartColumnInCodeFlow()
        {
            Verify(new EndColumnMustNotBeLessThanStartColumn(), "EndColumnLessThanStartColumnInCodeFlow.sarif");
        }

        [Fact(DisplayName = nameof(EndColumnMustNotBeLessThanStartColumn_EndColumnLessThanStartColumnInRelatedLocation))]
        public void EndColumnMustNotBeLessThanStartColumn_EndColumnLessThanStartColumnInRelatedLocation()
        {
            Verify(new EndColumnMustNotBeLessThanStartColumn(), "EndColumnLessThanStartColumnInRelatedLocation.sarif");
        }

        [Fact(DisplayName = nameof(EndColumnMustNotBeLessThanStartColumn_EndColumnLessThanStartColumnInResultFile))]
        public void EndColumnMustNotBeLessThanStartColumn_EndColumnLessThanStartColumnInResultFile()
        {
            Verify(new EndColumnMustNotBeLessThanStartColumn(), "EndColumnLessThanStartColumnInResultFile.sarif");
        }

        [Fact(DisplayName = nameof(EndColumnMustNotBeLessThanStartColumn_EndColumnNotSpecified))]
        public void EndColumnMustNotBeLessThanStartColumn_EndColumnNotSpecified()
        {
            Verify(new EndColumnMustNotBeLessThanStartColumn(), "EndColumnNotSpecified.sarif");
        }
    }
}
