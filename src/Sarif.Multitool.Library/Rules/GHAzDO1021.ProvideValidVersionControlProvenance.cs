// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    /// <summary>
    /// GHAzDO1021 — require the primary version-control provenance entry to identify
    /// a branch and commit that GHAzDO can use for downstream processing.
    /// </summary>
    public class GHAzDOProvideValidVersionControlProvenance
        : SarifValidationSkimmerBase
    {
        internal const string ZeroGitRevision = "0000000000000000000000000000000000000000";

        public override string Id => RuleId.GHAzDOProvideValidVersionControlProvenance;

        public override MultiformatMessageString FullDescription => new MultiformatMessageString
        {
            Text = RuleResources.GHAzDO1021_ProvideValidVersionControlProvenance_FullDescription_Text
        };

        protected override ICollection<string> MessageResourceNames => new List<string>
        {
            nameof(RuleResources.GHAzDO1021_ProvideValidVersionControlProvenance_Error_MissingBranch_Text),
            nameof(RuleResources.GHAzDO1021_ProvideValidVersionControlProvenance_Error_MissingRevisionId_Text),
            nameof(RuleResources.GHAzDO1021_ProvideValidVersionControlProvenance_Error_ZeroRevisionId_Text)
        };

        public override HashSet<RuleKind> RuleKinds => new HashSet<RuleKind>(new[] { RuleKind.GHAzDO });

        protected override string ServiceName => RuleResources.ServiceName_GHAzDO;

        public GHAzDOProvideValidVersionControlProvenance()
        {
            this.DefaultConfiguration.Level = FailureLevel.Error;
        }

        protected override void Analyze(Run run, string runPointer)
        {
            VersionControlDetails provenance = run?.VersionControlProvenance?.FirstOrDefault();
            if (provenance == null)
            {
                return;
            }

            string provenancePointer = runPointer
                .AtProperty(SarifPropertyName.VersionControlProvenance)
                .AtIndex(0);

            if (string.IsNullOrWhiteSpace(provenance.Branch))
            {
                LogResult(
                    provenancePointer.AtProperty(VcpFieldNames.Branch),
                    nameof(RuleResources.GHAzDO1021_ProvideValidVersionControlProvenance_Error_MissingBranch_Text));
            }

            if (string.IsNullOrWhiteSpace(provenance.RevisionId))
            {
                LogResult(
                    provenancePointer.AtProperty(VcpFieldNames.RevisionId),
                    nameof(RuleResources.GHAzDO1021_ProvideValidVersionControlProvenance_Error_MissingRevisionId_Text));
            }
            else if (string.Equals(provenance.RevisionId, ZeroGitRevision, StringComparison.Ordinal))
            {
                LogResult(
                    provenancePointer.AtProperty(VcpFieldNames.RevisionId),
                    nameof(RuleResources.GHAzDO1021_ProvideValidVersionControlProvenance_Error_ZeroRevisionId_Text));
            }
        }
    }
}
