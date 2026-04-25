// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class ProvideALASSignalArtifact : SarifValidationSkimmerBase
    {
        public ProvideALASSignalArtifact()
        {
            this.DefaultConfiguration.Level = FailureLevel.Note;
        }

        /// <summary>
        /// AI3004
        /// </summary>
        public override string Id => RuleId.AIProvideALASSignalArtifact;

        public override HashSet<RuleKind> RuleKinds => new HashSet<RuleKind>(new[] { RuleKind.AI });

        public override MultiformatMessageString FullDescription => new MultiformatMessageString
        {
            Text = RuleResources.AI3004_ProvideALASSignalArtifact_FullDescription_Text
        };

        protected override ICollection<string> MessageResourceNames => new List<string>
        {
            nameof(RuleResources.AI3004_ProvideALASSignalArtifact_Note_MissingLocation_Text),
            nameof(RuleResources.AI3004_ProvideALASSignalArtifact_Note_UnresolvableArtifact_Text)
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

                if (invocation.ToolExecutionNotifications == null)
                {
                    continue;
                }

                string notificationsPointer = invocationPointer.AtProperty(SarifPropertyName.ToolExecutionNotifications);

                for (int iNotification = 0; iNotification < invocation.ToolExecutionNotifications.Count; iNotification++)
                {
                    Notification notification = invocation.ToolExecutionNotifications[iNotification];

                    if (notification.Descriptor?.Id != "AI/EXEC/ALAS-SIGNAL")
                    {
                        continue;
                    }

                    string notificationPointer = notificationsPointer.AtIndex(iNotification);

                    if (notification.Locations == null || notification.Locations.Count == 0)
                    {
                        // {0}: This ALAS-SIGNAL notification does not specify a location
                        // referencing the signal artifact.
                        LogResult(
                            notificationPointer,
                            nameof(RuleResources.AI3004_ProvideALASSignalArtifact_Note_MissingLocation_Text));
                        continue;
                    }

                    int artifactIndex = notification.Locations[0]?.PhysicalLocation?.ArtifactLocation?.Index ?? -1;

                    if (artifactIndex < 0 ||
                        run.Artifacts == null ||
                        artifactIndex >= run.Artifacts.Count ||
                        !run.Artifacts[artifactIndex].Roles.HasFlag(ArtifactRoles.Attachment))
                    {
                        // {0}: This ALAS-SIGNAL notification references an artifact that cannot
                        // be resolved or does not have the 'attachment' role.
                        LogResult(
                            notificationPointer,
                            nameof(RuleResources.AI3004_ProvideALASSignalArtifact_Note_UnresolvableArtifact_Text),
                            artifactIndex.ToString());
                    }
                }
            }
        }
    }
}
