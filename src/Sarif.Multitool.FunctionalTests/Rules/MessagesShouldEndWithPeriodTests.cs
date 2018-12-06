// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class MessagesShouldEndWithPeriodTests : ValidationSkimmerTestsBase<MessagesShouldEndWithPeriod>
    {
        [Fact(DisplayName = nameof(MessagesShouldEndWithPeriod_ReportsInvalidSarif))]
        public void MessagesShouldEndWithPeriod_ReportsInvalidSarif() => Verify("Invalid.sarif");

        [Fact(DisplayName = nameof(MessagesShouldEndWithPeriod_AcceptsValidSarif))]
        public void MessagesShouldEndWithPeriod_AcceptsValidSarif() => Verify("Valid.sarif");
    }
}
