// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class ProvideNotificationTimestamp : SarifValidationSkimmerBase
    {
        public ProvideNotificationTimestamp()
        {
            this.DefaultConfiguration.Level = FailureLevel.Note;
        }

        /// <summary>
        /// AI3005
        /// </summary>
        public override string Id => RuleId.AIProvideNotificationTimestamp;

        public override HashSet<RuleKind> RuleKinds => new HashSet<RuleKind>(new[] { RuleKind.AI });

        public override MultiformatMessageString FullDescription => new MultiformatMessageString
        {
            Text = RuleResources.AI3005_ProvideNotificationTimestamp_FullDescription_Text
        };

        protected override ICollection<string> MessageResourceNames => new List<string>
        {
            nameof(RuleResources.AI3005_ProvideNotificationTimestamp_Note_Default_Text)
        };

        protected override void Analyze(Run run, string runPointer)
        {
            if (run.Invocations == null)
            {
                return;
            }

            string invocationsPointer = runPointer.AtProperty(SarifPropertyName.Invocations);

            for (int iInvocation = 0; iInvocation < run.Invocations.Count; iInvocation++)
            {
                Invocation invocation = run.Invocations[iInvocation];
                string invocationPointer = invocationsPointer.AtIndex(iInvocation);

                if (invocation.ToolExecutionNotifications != null)
                {
                    string notificationsPointer = invocationPointer.AtProperty(SarifPropertyName.ToolExecutionNotifications);

                    for (int iNotification = 0; iNotification < invocation.ToolExecutionNotifications.Count; iNotification++)
                    {
                        if (invocation.ToolExecutionNotifications[iNotification].TimeUtc == default(DateTime))
                        {
                            LogResult(
                                notificationsPointer.AtIndex(iNotification),
                                nameof(RuleResources.AI3005_ProvideNotificationTimestamp_Note_Default_Text));
                        }
                    }
                }

                if (invocation.ToolConfigurationNotifications != null)
                {
                    string notificationsPointer = invocationPointer.AtProperty(SarifPropertyName.ToolConfigurationNotifications);

                    for (int iNotification = 0; iNotification < invocation.ToolConfigurationNotifications.Count; iNotification++)
                    {
                        if (invocation.ToolConfigurationNotifications[iNotification].TimeUtc == default(DateTime))
                        {
                            LogResult(
                                notificationsPointer.AtIndex(iNotification),
                                nameof(RuleResources.AI3005_ProvideNotificationTimestamp_Note_Default_Text));
                        }
                    }
                }
            }
        }
    }
}
