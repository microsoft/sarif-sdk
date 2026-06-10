// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

using Microsoft.CodeAnalysis.Sarif.Visitors;
using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class DoNotPersistPartialFingerprints : SarifValidationSkimmerBase
    {
        public DoNotPersistPartialFingerprints()
        {
            this.DefaultConfiguration.Level = FailureLevel.Warning;
        }

        /// <summary>
        /// AI2011
        /// </summary>
        public override string Id => RuleId.AIDoNotPersistPartialFingerprints;

        public override HashSet<RuleKind> RuleKinds => new HashSet<RuleKind>(new[] { RuleKind.AI });

        public override MultiformatMessageString FullDescription => new MultiformatMessageString
        {
            Text = RuleResources.AI2011_DoNotPersistPartialFingerprints_FullDescription_Text
        };

        protected override ICollection<string> MessageResourceNames => new List<string>
        {
            nameof(RuleResources.AI2011_DoNotPersistPartialFingerprints_Warning_Default_Text)
        };

        protected override void Analyze(Result result, string resultPointer)
        {
            if (result.PartialFingerprints == null || result.PartialFingerprints.Count == 0)
            {
                return;
            }

            // A GitHub-hosted run may persist the single rolling-hash primaryLocationLineHash:
            // GitHub's raw code-scanning SARIF upload API does not backfill partialFingerprints, so
            // emitting it is what keeps API-upload pipelines from raising duplicate alerts. Any other
            // partial fingerprint, and any partial fingerprint on a non-GitHub run, is still flagged.
            Run run = Context?.CurrentRun;
            if (run != null
                && VcpPortableRoot.IsGitHubHostedRun(run)
                && result.PartialFingerprints.Count == 1
                && result.PartialFingerprints.TryGetValue(InsertOptionalDataVisitor.PrimaryLocationLineHash, out string lineHash)
                && !string.IsNullOrWhiteSpace(lineHash))
            {
                return;
            }

            LogResult(
                resultPointer.AtProperty(SarifPropertyName.PartialFingerprints),
                nameof(RuleResources.AI2011_DoNotPersistPartialFingerprints_Warning_Default_Text),
                SarifPropertyName.PartialFingerprints);
        }
    }
}
