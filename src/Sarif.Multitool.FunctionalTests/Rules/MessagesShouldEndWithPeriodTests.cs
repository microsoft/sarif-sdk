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
            Verify(new MessagesShouldEndWithPeriod(), "PeriodsAfterAllMessages.sarif");
        }

        [Fact(DisplayName = nameof(MessagesShouldEndWithPeriod_CodeFlowMessageWithoutPeriod))]
        public void MessagesShouldEndWithPeriod_CodeFlowMessageWithoutPeriod()
        {
            Verify(new MessagesShouldEndWithPeriod(), "CodeFlowMessageWithoutPeriod.sarif");
        }

        [Fact(DisplayName = nameof(MessagesShouldEndWithPeriod_NotificationMessageWithoutPeriod))]
        public void MessagesShouldEndWithPeriod_NotificationMessageWithoutPeriod()
        {
            Verify(new MessagesShouldEndWithPeriod(), "NotificationMessageWithoutPeriod.sarif");
        }

        [Fact(DisplayName = nameof(MessagesShouldEndWithPeriod_ResultMessageWithoutPeriod))]
        public void MessagesShouldEndWithPeriod_ResultMessageWithoutPeriod()
        {
            Verify(new MessagesShouldEndWithPeriod(), "ResultMessageWithoutPeriod.sarif");
        }

        [Fact(DisplayName = nameof(MessagesShouldEndWithPeriod_RuleMessageWithoutPeriod))]
        public void MessagesShouldEndWithPeriod_RuleMessageWithoutPeriod()
        {
            Verify(new MessagesShouldEndWithPeriod(), "RuleMessageWithoutPeriod.sarif");
        }

        [Fact(DisplayName = nameof(MessagesShouldEndWithPeriod_StackFrameMessageWithoutPeriod))]
        public void MessagesShouldEndWithPeriod_StackFrameMessageWithoutPeriod()
        {
            Verify(new MessagesShouldEndWithPeriod(), "StackFrameMessageWithoutPeriod.sarif");
        }

        [Fact(DisplayName = nameof(MessagesShouldEndWithPeriod_StackMessageWithoutPeriod))]
        public void MessagesShouldEndWithPeriod_StackMessageWithoutPeriod()
        {
            Verify(new MessagesShouldEndWithPeriod(), "StackMessageWithoutPeriod.sarif");
        }

        [Fact(DisplayName = nameof(MessagesShouldEndWithPeriod_ThreadFlowLocationMessageWithoutPeriod))]
        public void MessagesShouldEndWithPeriod_ThreadFlowLocationMessageWithoutPeriod()
        {
            Verify(new MessagesShouldEndWithPeriod(), "ThreadFlowLocationMessageWithoutPeriod.sarif");
        }
    }
}
