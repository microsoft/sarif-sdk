// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class ProvideNotificationAssociatedRule : SarifValidationSkimmerBase
    {
        public ProvideNotificationAssociatedRule()
        {
            this.DefaultConfiguration.Level = FailureLevel.Error;
        }

        /// <summary>
        /// AI1013
        /// </summary>
        public override string Id => RuleId.AIProvideNotificationAssociatedRule;

        public override HashSet<RuleKind> RuleKinds => new HashSet<RuleKind>(new[] { RuleKind.AI });

        public override MultiformatMessageString FullDescription => new MultiformatMessageString
        {
            Text = RuleResources.AI1013_ProvideNotificationAssociatedRule_FullDescription_Text
        };

        protected override ICollection<string> MessageResourceNames => new List<string>
        {
            nameof(RuleResources.AI1013_ProvideNotificationAssociatedRule_Error_Default_Text)
        };

        protected override void Analyze(Run run, string runPointer)
        {
            if (run.Invocations == null)
            {
                return;
            }

            for (int invIdx = 0; invIdx < run.Invocations.Count; invIdx++)
            {
                Invocation invocation = run.Invocations[invIdx];
                string invPointer = runPointer.AtProperty(SarifPropertyName.Invocations).AtIndex(invIdx);

                CheckNotifications(invocation.ToolExecutionNotifications, invPointer, SarifPropertyName.ToolExecutionNotifications, run);
                CheckNotifications(invocation.ToolConfigurationNotifications, invPointer, SarifPropertyName.ToolConfigurationNotifications, run);
            }
        }

        private void CheckNotifications(IList<Notification> notifications, string invPointer, string propertyName, Run run)
        {
            if (notifications == null)
            {
                return;
            }

            string notificationsPointer = invPointer.AtProperty(propertyName);

            for (int i = 0; i < notifications.Count; i++)
            {
                Notification notification = notifications[i];
                string notificationPointer = notificationsPointer.AtIndex(i);

                if (notification.AssociatedRule == null)
                {
                    continue;
                }

                string ruleId = notification.AssociatedRule.Id;
                if (ruleId != null && !RuleExists(ruleId, run))
                {
                    LogResult(
                        notificationPointer.AtProperty(SarifPropertyName.AssociatedRule),
                        nameof(RuleResources.AI1013_ProvideNotificationAssociatedRule_Error_Default_Text),
                        ruleId);
                }
            }
        }

        private static bool RuleExists(string id, Run run)
        {
            if (run.Tool?.Driver?.Rules != null &&
                run.Tool.Driver.Rules.Any(r => r.Id == id))
            {
                return true;
            }

            if (run.Tool?.Extensions != null)
            {
                foreach (ToolComponent extension in run.Tool.Extensions)
                {
                    if (extension.Rules != null &&
                        extension.Rules.Any(r => r.Id == id))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
