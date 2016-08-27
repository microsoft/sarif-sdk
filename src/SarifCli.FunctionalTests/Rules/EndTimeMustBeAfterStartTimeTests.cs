// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.Sarif.Cli.Rules;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Cli.FunctionalTests.Rules
{
    public class EndTimeMustBeAfterStartTimeTests : SkimmerTestsBase
    {
        [Fact]
        public void EndTimeMustBeAfterStartTime_NoDiagnostic_EndTimeIsAfterStartTime()
        {
            Verify(new EndTimeMustBeAfterStartTime(), "EndTimeIsAfterStartTime.sarif");
        }

        [Fact]
        public void EndTimeMustBeAfterStartTime_Diagnostic_EndTimeEqualsStartTime()
        {
            Verify(new EndTimeMustBeAfterStartTime(), "EndTimeEqualsStartTime.sarif");
        }

        [Fact]
        public void EndTimeMustBeAfterStartTime_Diagnostic_EndTimeIsBeforeStartTime()
        {
            Verify(new EndTimeMustBeAfterStartTime(), "EndTimeIsBeforeStartTime.sarif");
        }
    }
}
