// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class MessagesShouldEndWithPeriodTests : SkimmerTestsBase<MessagesShouldEndWithPeriod>
    {
        [Fact(DisplayName = nameof(MessagesShouldEndWithPeriod_PeriodsAfterAllMessages))]
        public void MessagesShouldEndWithPeriod_PeriodsAfterAllMessages()
        {
            Verify("PeriodsAfterAllMessages.sarif");
        }

        [Fact(DisplayName = nameof(MessagesShouldEndWithPeriod_CodeFlowMessageWithoutPeriod))]
        public void MessagesShouldEndWithPeriod_CodeFlowMessageWithoutPeriod()
        {
            Verify("CodeFlowMessageWithoutPeriod.sarif");
        }

        [Fact(DisplayName = nameof(MessagesShouldEndWithPeriod_NotificationMessageWithoutPeriod))]
        public void MessagesShouldEndWithPeriod_NotificationMessageWithoutPeriod()
        {
            Verify("NotificationMessageWithoutPeriod.sarif");
        }

        [Fact(DisplayName = nameof(MessagesShouldEndWithPeriod_ResultMessageWithoutPeriod))]
        public void MessagesShouldEndWithPeriod_ResultMessageWithoutPeriod()
        {
            Verify("ResultMessageWithoutPeriod.sarif");
        }

        [Fact(DisplayName = nameof(MessagesShouldEndWithPeriod_RuleMessageWithoutPeriod))]
        public void MessagesShouldEndWithPeriod_RuleMessageWithoutPeriod()
        {
            Verify("RuleMessageWithoutPeriod.sarif");
        }

        [Fact(DisplayName = nameof(MessagesShouldEndWithPeriod_StackFrameMessageWithoutPeriod))]
        public void MessagesShouldEndWithPeriod_StackFrameMessageWithoutPeriod()
        {
            Verify("StackFrameMessageWithoutPeriod.sarif");
        }

        [Fact(DisplayName = nameof(MessagesShouldEndWithPeriod_StackMessageWithoutPeriod))]
        public void MessagesShouldEndWithPeriod_StackMessageWithoutPeriod()
        {
            Verify("StackMessageWithoutPeriod.sarif");
        }

        [Fact(DisplayName = nameof(MessagesShouldEndWithPeriod_ThreadFlowLocationMessageWithoutPeriod))]
        public void MessagesShouldEndWithPeriod_ThreadFlowLocationMessageWithoutPeriod()
        {
            Verify("ThreadFlowLocationMessageWithoutPeriod.sarif");
        }
    }
}
