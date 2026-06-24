// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class AIProvideVersionControlProvenance : SarifValidationSkimmerBase
    {
        public AIProvideVersionControlProvenance()
        {
            this.DefaultConfiguration.Level = FailureLevel.Error;
        }

        /// <summary>
        /// AI1004
        /// </summary>
        public override string Id => RuleId.AIProvideVersionControlProvenance;

        public override HashSet<RuleKind> RuleKinds => new HashSet<RuleKind>(new[] { RuleKind.AI });

        public override MultiformatMessageString FullDescription => new MultiformatMessageString
        {
            Text = RuleResources.AI1004_ProvideVersionControlProvenance_FullDescription_Text
        };

        protected override ICollection<string> MessageResourceNames => new List<string>
        {
            nameof(RuleResources.AI1004_ProvideVersionControlProvenance_Error_Default_Text)
        };

        protected override void Analyze(Run run, string runPointer)
        {
            // A run finalized with emit-finalize --no-repo declares it has no version control and
            // is stamped unpublishable. It must not be faulted for the version-control provenance
            // it has deliberately asserted is absent; every other AI-profile check still applies.
            if (run.TryGetProperty(EmitFinalizeCommand.UnpublishablePropertyName, out bool unpublishable)
                && unpublishable)
            {
                return;
            }

            if (run.VersionControlProvenance == null || run.VersionControlProvenance.Count == 0)
            {
                LogResult(
                    runPointer,
                    nameof(RuleResources.AI1004_ProvideVersionControlProvenance_Error_Default_Text));
            }
        }
    }
}
