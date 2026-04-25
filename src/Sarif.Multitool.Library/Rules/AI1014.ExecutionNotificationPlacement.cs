// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class ExecutionNotificationPlacement : SarifValidationSkimmerBase
    {
        public ExecutionNotificationPlacement()
        {
            this.DefaultConfiguration.Level = FailureLevel.Error;
        }

        /// <summary>
        /// AI1014
        /// </summary>
        public override string Id => RuleId.AIExecutionNotificationPlacement;

        public override HashSet<RuleKind> RuleKinds => new HashSet<RuleKind>(new[] { RuleKind.AI });

        public override MultiformatMessageString FullDescription => new MultiformatMessageString
        {
            Text = RuleResources.AI1014_ExecutionNotificationPlacement_FullDescription_Text
        };

        protected override ICollection<string> MessageResourceNames => new List<string>
        {
            nameof(RuleResources.AI1014_ExecutionNotificationPlacement_Error_ExecInConfig_Text),
            nameof(RuleResources.AI1014_ExecutionNotificationPlacement_Error_CfgInExec_Text)
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
                        Notification notification = invocation.ToolExecutionNotifications[iNotification];

                        if (notification.Descriptor?.Id != null &&
                            notification.Descriptor.Id.StartsWith("AI/CFG/"))
                        {
                            // {0}: Notification descriptor '{1}' uses the 'AI/CFG/' prefix but
                            // appears in 'toolExecutionNotifications'. It should be placed in
                            // 'toolConfigurationNotifications'.
                            LogResult(
                                notificationsPointer.AtIndex(iNotification),
                                nameof(RuleResources.AI1014_ExecutionNotificationPlacement_Error_CfgInExec_Text),
                                notification.Descriptor.Id);
                        }
                    }
                }

                if (invocation.ToolConfigurationNotifications != null)
                {
                    string notificationsPointer = invocationPointer.AtProperty(SarifPropertyName.ToolConfigurationNotifications);

                    for (int iNotification = 0; iNotification < invocation.ToolConfigurationNotifications.Count; iNotification++)
                    {
                        Notification notification = invocation.ToolConfigurationNotifications[iNotification];

                        if (notification.Descriptor?.Id != null &&
                            notification.Descriptor.Id.StartsWith("AI/EXEC/"))
                        {
                            // {0}: Notification descriptor '{1}' uses the 'AI/EXEC/' prefix but
                            // appears in 'toolConfigurationNotifications'. It should be placed in
                            // 'toolExecutionNotifications'.
                            LogResult(
                                notificationsPointer.AtIndex(iNotification),
                                nameof(RuleResources.AI1014_ExecutionNotificationPlacement_Error_ExecInConfig_Text),
                                notification.Descriptor.Id);
                        }
                    }
                }
            }
        }
    }
}
