// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class EndTimeMustNotBeBeforeStartTimeTests : ValidationSkimmerTestsBase<EndTimeMustNotBeBeforeStartTime>
    {
        [Fact(DisplayName = nameof(EndTimeMustNotBeBeforeStartTime_ReportsInvalidSarif))]
        public void EndTimeMustNotBeBeforeStartTime_ReportsInvalidSarif() => Verify("Invalid.sarif");

        [Fact(DisplayName = nameof(EndTimeMustNotBeBeforeStartTime_AcceptsValidSarif))]
        public void EndTimeMustNotBeBeforeStartTime_AcceptsValidSarif() => Verify("Valid.sarif");
    }
}
