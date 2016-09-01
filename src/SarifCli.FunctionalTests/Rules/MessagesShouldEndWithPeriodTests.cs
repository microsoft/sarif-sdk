// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.Sarif.Cli.Rules;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Cli.FunctionalTests.Rules
{
    public class MessagesShouldEndWithPeriodTests : SkimmerTestsBase
    {
        [Fact(DisplayName = nameof(MessagesShouldEndWithPeriod_AnnotatedCodeLocationDoesNotHaveEssential))]
        public void MessagesShouldEndWithPeriod_AnnotatedCodeLocationDoesNotHaveEssential()
        {
            Verify(new MessagesShouldEndWithPeriod(), "PeriodsAfterAllMessages.sarif");
        }

        [Fact(DisplayName = nameof(MessagesShouldEndWithPeriod_AnnotatedCodeLocationMessageWithoutPeriod))]
        public void MessagesShouldEndWithPeriod_AnnotatedCodeLocationMessageWithoutPeriod()
        {
            Verify(new MessagesShouldEndWithPeriod(), "AnnotatedCodeLocationMessageWithoutPeriod.sarif");
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

        [Fact(DisplayName = nameof(MessagesShouldEndWithPeriod_RuleMessageFormatWithoutPeriod))]
        public void MessagesShouldEndWithPeriod_RuleMessageFormatWithoutPeriod()
        {
            Verify(new MessagesShouldEndWithPeriod(), "RuleMessageFormatWithoutPeriod.sarif");
        }

        [Fact(DisplayName = nameof(MessagesShouldEndWithPeriod_StackMessageWithoutPeriod))]
        public void MessagesShouldEndWithPeriod_StackMessageWithoutPeriod()
        {
            Verify(new MessagesShouldEndWithPeriod(), "StackMessageWithoutPeriod.sarif");
        }

        [Fact(DisplayName = nameof(MessagesShouldEndWithPeriod_StackFrameMessageWithoutPeriod))]
        public void MessagesShouldEndWithPeriod_StackFrameMessageWithoutPeriod()
        {
            Verify(new MessagesShouldEndWithPeriod(), "StackFrameMessageWithoutPeriod.sarif");
        }
    }
}
