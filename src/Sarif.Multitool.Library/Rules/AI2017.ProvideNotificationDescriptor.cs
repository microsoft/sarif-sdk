// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class ProvideNotificationDescriptor : SarifValidationSkimmerBase
    {
        public ProvideNotificationDescriptor()
        {
            this.DefaultConfiguration.Level = FailureLevel.Warning;
        }

        /// <summary>
        /// AI2017
        /// </summary>
        public override string Id => RuleId.AIProvideNotificationDescriptor;

        public override HashSet<RuleKind> RuleKinds => new HashSet<RuleKind>(new[] { RuleKind.AI });

        public override MultiformatMessageString FullDescription => new MultiformatMessageString
        {
            Text = RuleResources.AI2017_ProvideNotificationDescriptor_FullDescription_Text
        };

        protected override ICollection<string> MessageResourceNames => new List<string>
        {
            nameof(RuleResources.AI2017_ProvideNotificationDescriptor_Warning_Missing_Text),
            nameof(RuleResources.AI2017_ProvideNotificationDescriptor_Warning_Unresolvable_Text)
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

                if (notification.Descriptor == null)
                {
                    LogResult(
                        notificationPointer,
                        nameof(RuleResources.AI2017_ProvideNotificationDescriptor_Warning_Missing_Text));
                    continue;
                }

                string descriptorId = notification.Descriptor.Id;
                if (descriptorId != null && !DescriptorExists(descriptorId, run))
                {
                    LogResult(
                        notificationPointer.AtProperty(SarifPropertyName.Descriptor),
                        nameof(RuleResources.AI2017_ProvideNotificationDescriptor_Warning_Unresolvable_Text),
                        descriptorId);
                }
            }
        }

        private static bool DescriptorExists(string id, Run run)
        {
            if (run.Tool?.Driver?.Notifications != null &&
                run.Tool.Driver.Notifications.Any(d => d.Id == id))
            {
                return true;
            }

            if (run.Tool?.Extensions != null)
            {
                foreach (ToolComponent extension in run.Tool.Extensions)
                {
                    if (extension.Notifications != null &&
                        extension.Notifications.Any(d => d.Id == id))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
