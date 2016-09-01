// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.Sarif.Cli.Rules;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Cli.FunctionalTests.Rules
{
    public class EndTimeMustBeAfterStartTimeTests : SkimmerTestsBase
    {
        [Fact(DisplayName = nameof(EndTimeMustBeAfterStartTime_EndTimeIsAfterStartTime))]
        public void EndTimeMustBeAfterStartTime_EndTimeIsAfterStartTime()
        {
            Verify(new EndTimeMustBeAfterStartTime(), "EndTimeIsAfterStartTime.sarif");
        }

        [Fact(DisplayName = nameof(EndTimeMustBeAfterStartTime_EndTimeEqualsStartTime))]
        public void EndTimeMustBeAfterStartTime_EndTimeEqualsStartTime()
        {
            Verify(new EndTimeMustBeAfterStartTime(), "EndTimeEqualsStartTime.sarif");
        }

        [Fact(DisplayName = nameof(EndTimeMustBeAfterStartTime_EndTimeIsBeforeStartTime))]
        public void EndTimeMustBeAfterStartTime_EndTimeIsBeforeStartTime()
        {
            Verify(new EndTimeMustBeAfterStartTime(), "EndTimeIsBeforeStartTime.sarif");
        }
    }
}
