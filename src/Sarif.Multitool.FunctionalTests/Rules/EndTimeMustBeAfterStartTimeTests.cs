// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class EndTimeMustBeAfterStartTimeTests : SkimmerTestsBase<EndTimeMustBeAfterStartTime>
    {
        [Fact(DisplayName = nameof(EndTimeMustBeAfterStartTime_EndTimeIsAfterStartTime))]
        public void EndTimeMustBeAfterStartTime_EndTimeIsAfterStartTime()
        {
            Verify("EndTimeIsAfterStartTime.sarif");
        }

        [Fact(DisplayName = nameof(EndTimeMustBeAfterStartTime_EndTimeEqualsStartTime))]
        public void EndTimeMustBeAfterStartTime_EndTimeEqualsStartTime()
        {
            Verify("EndTimeEqualsStartTime.sarif");
        }

        [Fact(DisplayName = nameof(EndTimeMustBeAfterStartTime_EndTimeIsBeforeStartTime))]
        public void EndTimeMustBeAfterStartTime_EndTimeIsBeforeStartTime()
        {
            Verify("EndTimeIsBeforeStartTime.sarif");
        }
    }
}
