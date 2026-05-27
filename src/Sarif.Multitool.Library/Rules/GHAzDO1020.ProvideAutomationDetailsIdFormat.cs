// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    /// <summary>
    /// GHAzDO1020 — when run.automationDetails.id is present, require it to start
    /// with the canonical `azuredevops/pipeline/build/` prefix. GHAzDO ingestion
    /// parses the slash-delimited remainder as
    /// `&lt;org&gt;/&lt;project&gt;/&lt;buildDefId&gt;/&lt;phaseId&gt;/&lt;branch&gt;/&lt;buildId&gt;`;
    /// IDs that don't carry the prefix fail downstream parsing.
    ///
    /// Source of truth: AdvancedSecurity.Service runAutomationDetails.Id consumers.
    /// We deliberately validate only the prefix here — the slash content is derived
    /// from pipeline state and not authored by hand.
    /// </summary>
    public class GHAzDOProvideAutomationDetailsIdFormat
        : SarifValidationSkimmerBase
    {
        public const string ExpectedIdPrefix = "azuredevops/pipeline/build/";

        public override string Id => RuleId.GHAzDOProvideAutomationDetailsIdFormat;

        public override MultiformatMessageString FullDescription => new MultiformatMessageString
        {
            Text = RuleResources.GHAzDO1020_ProvideAutomationDetailsIdFormat_FullDescription_Text
        };

        protected override ICollection<string> MessageResourceNames => new List<string>
        {
            nameof(RuleResources.GHAzDO1020_ProvideAutomationDetailsIdFormat_Error_BadPrefix_Text)
        };

        public override HashSet<RuleKind> RuleKinds => new HashSet<RuleKind>(new[] { RuleKind.GHAzDO });

        protected override string ServiceName => RuleResources.ServiceName_GHAzDO;

        public GHAzDOProvideAutomationDetailsIdFormat()
        {
            this.DefaultConfiguration.Level = FailureLevel.Error;
        }

        protected override void Analyze(Run run, string runPointer)
        {
            // GHAzDO1014 already enforces presence of automationDetails and a non-empty id.
            // We just check shape when both are present.
            string id = run?.AutomationDetails?.Id;
            if (string.IsNullOrWhiteSpace(id))
            {
                return;
            }

            if (!id.StartsWith(ExpectedIdPrefix, System.StringComparison.Ordinal))
            {
                LogResult(
                    runPointer.AtProperty(SarifPropertyName.AutomationDetails).AtProperty(SarifPropertyName.Id),
                    nameof(RuleResources.GHAzDO1020_ProvideAutomationDetailsIdFormat_Error_BadPrefix_Text),
                    ExpectedIdPrefix,
                    id);
            }
        }
    }
}
