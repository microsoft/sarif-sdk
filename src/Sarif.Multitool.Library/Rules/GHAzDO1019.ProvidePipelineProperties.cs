// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    /// <summary>
    /// GHAzDO1019 — when run.automationDetails is present, require the four
    /// `azuredevops/pipeline/build/*` properties that GHAzDO ingestion reads to
    /// identify the build definition + phase. Missing or unparseable values cause
    /// ingestion to drop the run with "SarifValidation_MissingAdoPipelineProperties".
    ///
    /// Required keys (all under run.automationDetails.properties):
    ///   azuredevops/pipeline/build/buildDefinitionId    (int; positive or the -1 sentinel)
    ///   azuredevops/pipeline/build/buildDefinitionName  (non-empty string)
    ///   azuredevops/pipeline/build/phaseId              (GUID, != Guid.Empty)
    ///   azuredevops/pipeline/build/phaseName            (non-empty string)
    ///
    /// Source of truth: AdvancedSecurity.Service ./SarifUtils/SarifExtensions.cs
    /// `GetPipeline(Run)` and CodeScanningResultPluginBase.ValidateRun.
    /// </summary>
    public class GHAzDOProvidePipelineProperties
        : SarifValidationSkimmerBase
    {
        public const string BuildDefinitionIdKey = "azuredevops/pipeline/build/buildDefinitionId";
        public const string BuildDefinitionNameKey = "azuredevops/pipeline/build/buildDefinitionName";
        public const string PhaseIdKey = "azuredevops/pipeline/build/phaseId";
        public const string PhaseNameKey = "azuredevops/pipeline/build/phaseName";

        public override string Id => RuleId.GHAzDOProvidePipelineProperties;

        public override MultiformatMessageString FullDescription => new MultiformatMessageString
        {
            Text = RuleResources.GHAzDO1019_ProvidePipelineProperties_FullDescription_Text
        };

        protected override ICollection<string> MessageResourceNames => new List<string>
        {
            nameof(RuleResources.GHAzDO1019_ProvidePipelineProperties_Error_MissingBuildDefinitionId_Text),
            nameof(RuleResources.GHAzDO1019_ProvidePipelineProperties_Error_InvalidBuildDefinitionId_Text),
            nameof(RuleResources.GHAzDO1019_ProvidePipelineProperties_Error_MissingBuildDefinitionName_Text),
            nameof(RuleResources.GHAzDO1019_ProvidePipelineProperties_Error_MissingPhaseId_Text),
            nameof(RuleResources.GHAzDO1019_ProvidePipelineProperties_Error_InvalidPhaseId_Text),
            nameof(RuleResources.GHAzDO1019_ProvidePipelineProperties_Error_MissingPhaseName_Text)
        };

        public override HashSet<RuleKind> RuleKinds => new HashSet<RuleKind>(new[] { RuleKind.GHAzDO });

        protected override string ServiceName => RuleResources.ServiceName_GHAzDO;

        public GHAzDOProvidePipelineProperties()
        {
            this.DefaultConfiguration.Level = FailureLevel.Error;
        }

        protected override void Analyze(Run run, string runPointer)
        {
            // The presence of automationDetails itself is GHAzDO1014's responsibility.
            // We only fire when automationDetails exists — pipeline properties are
            // attached *to* automationDetails.
            if (run?.AutomationDetails == null)
            {
                return;
            }

            string automationDetailsPointer = runPointer.AtProperty(SarifPropertyName.AutomationDetails);

            // buildDefinitionId: required, must parse as int != 0.
            if (!run.AutomationDetails.TryGetProperty(BuildDefinitionIdKey, out string buildDefinitionIdValue) ||
                string.IsNullOrWhiteSpace(buildDefinitionIdValue))
            {
                LogResult(
                    automationDetailsPointer,
                    nameof(RuleResources.GHAzDO1019_ProvidePipelineProperties_Error_MissingBuildDefinitionId_Text),
                    BuildDefinitionIdKey);
            }
            else if (!int.TryParse(buildDefinitionIdValue, out int buildDefinitionId)
                  || (buildDefinitionId <= 0 && buildDefinitionId != -1))
            {
                LogResult(
                    automationDetailsPointer,
                    nameof(RuleResources.GHAzDO1019_ProvidePipelineProperties_Error_InvalidBuildDefinitionId_Text),
                    BuildDefinitionIdKey,
                    buildDefinitionIdValue);
            }

            // buildDefinitionName: required, non-empty.
            if (!run.AutomationDetails.TryGetProperty(BuildDefinitionNameKey, out string buildDefinitionName) ||
                string.IsNullOrWhiteSpace(buildDefinitionName))
            {
                LogResult(
                    automationDetailsPointer,
                    nameof(RuleResources.GHAzDO1019_ProvidePipelineProperties_Error_MissingBuildDefinitionName_Text),
                    BuildDefinitionNameKey);
            }

            // phaseId: required, must parse as Guid != Guid.Empty.
            if (!run.AutomationDetails.TryGetProperty(PhaseIdKey, out string phaseIdValue) ||
                string.IsNullOrWhiteSpace(phaseIdValue))
            {
                LogResult(
                    automationDetailsPointer,
                    nameof(RuleResources.GHAzDO1019_ProvidePipelineProperties_Error_MissingPhaseId_Text),
                    PhaseIdKey);
            }
            else if (!System.Guid.TryParse(phaseIdValue, out System.Guid phaseId) || phaseId == System.Guid.Empty)
            {
                LogResult(
                    automationDetailsPointer,
                    nameof(RuleResources.GHAzDO1019_ProvidePipelineProperties_Error_InvalidPhaseId_Text),
                    PhaseIdKey,
                    phaseIdValue);
            }

            // phaseName: required, non-empty.
            if (!run.AutomationDetails.TryGetProperty(PhaseNameKey, out string phaseName) ||
                string.IsNullOrWhiteSpace(phaseName))
            {
                LogResult(
                    automationDetailsPointer,
                    nameof(RuleResources.GHAzDO1019_ProvidePipelineProperties_Error_MissingPhaseName_Text),
                    PhaseNameKey);
            }
        }
    }
}
