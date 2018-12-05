// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class EndTimeMustBeAfterStartTimeTests : ValidationSkimmerTestsBase<EndTimeMustBeAfterStartTime>
    {
        [Fact(DisplayName = nameof(EndTimeMustBeAfterStartTime_ReportsInvalidSarif))]
        public void EndTimeMustBeAfterStartTime_ReportsInvalidSarif() => Verify("Invalid.sarif");

        [Fact(DisplayName = nameof(EndTimeMustBeAfterStartTime_AcceptsValidSarif))]
        public void EndTimeMustBeAfterStartTime_AcceptsValidSarif() => Verify("Valid.sarif");
    }
}
