﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Json.Pointer;

using Newtonsoft.Json.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class OptimizeFileSize : SarifValidationSkimmerBase
    {
        /// <summary>
        /// SARIF2004
        /// </summary>
        public override string Id => RuleId.OptimizeFileSize;

        /// <summary>
        /// Emit arrays only if they provide additional information.
        ///
        /// In several parts of a SARIF log file, a subset of information about an object appears
        /// in one place, and the full information describing all such objects appears in an array
        /// elsewhere in the log file. For example, each 'result' object has a 'ruleId' property
        /// that identifies the rule that was violated. Elsewhere in the log file, the array
        /// 'run.tool.driver.rules' contains additional information about the rules. But if the
        /// elements of the 'rules' array contained no information about the rules beyond their ids,
        /// then there might be no reason to include the 'rules' array at all, and the log file
        /// could be made smaller simply by omitting it. In some scenarios (for example, when
        /// assessing compliance with policy), the 'rules' array might be used to record the full
        /// set of rules that were evaluated. In such a scenario, the 'rules' array should be retained
        /// even if it contains only id information.
        ///
        /// Similarly, most 'result' objects contain at least one 'artifactLocation' object. Elsewhere
        /// in the log file, the array 'run.artifacts' contains additional information about the artifacts
        /// that were analyzed. But if the elements of the 'artifacts' array contained not information
        /// about the artifacts beyond their locations, then there might be no reason to include the
        /// 'artifacts' array at all, and again the log file could be made smaller by omitting it. In
        /// some scenarios (for example, when assessing compliance with policy), the 'artifacts' array
        /// might be used to record the full set of artifacts that were analyzed. In such a scenario,
        /// the 'artifacts' array should be retained even if it contains only location information.
        /// </summary>
        public override MultiformatMessageString FullDescription => new MultiformatMessageString { Text = RuleResources.SARIF2004_OptimizeFileSize_FullDescription_Text };

        protected override IEnumerable<string> MessageResourceNames => new string[] {
            nameof(RuleResources.SARIF2004_OptimizeFileSize_Warning_EliminateLocationOnlyArtifacts_Text),
            nameof(RuleResources.SARIF2004_OptimizeFileSize_Warning_EliminateIdOnlyRules_Text)
        };

        public override FailureLevel DefaultLevel => FailureLevel.Warning;

        protected override void Analyze(Run run, string runPointer)
        {
            AnalyzeLocationOnlyArtifacts(run, runPointer);
            AnalyzeIdOnlyRules(run, runPointer);
        }

        private void AnalyzeLocationOnlyArtifacts(Run run, string runPointer)
        {
            // We only verify first item in the results and artifacts array,
            // since tools will typically generate similar nodes.
            // This approach may cause occasional false negatives.
            ArtifactLocation firstLocationInArtifactsArray = run.Artifacts?.FirstOrDefault()?.Location;
            ArtifactLocation firstlocationInResults = run.Results?.FirstOrDefault()?.Locations?.FirstOrDefault()?.PhysicalLocation?.ArtifactLocation;

            if (firstLocationInArtifactsArray == null || firstlocationInResults == null)
            {
                return;
            }

            string firstArtifactPointer = runPointer
                .AtProperty(SarifPropertyName.Artifacts).AtIndex(0);

            string firstResultLocationPointer = runPointer
                .AtProperty(SarifPropertyName.Results).AtIndex(0)
                .AtProperty(SarifPropertyName.Locations).AtIndex(0)
                .AtProperty(SarifPropertyName.PhysicalLocation)
                .AtProperty(SarifPropertyName.ArtifactLocation);

            if (HasResultLocationsWithUriAndIndex(firstResultLocationPointer) && HasLocationOnlyArtifacts(firstArtifactPointer))
            {
                // {0}: The 'artifacts' array contains no information beyond the locations of the
                // artifacts. Removing this array might  reduce the log file size without losing
                // information. In some scenarios (for example, when assessing compliance with policy),
                // the 'artifacts' array might be used to record the full set of artifacts that were
                // analyzed. In such a scenario, the 'artifacts' array should be retained even if it
                // contains only location information.
                LogResult(
                    firstArtifactPointer,
                    nameof(RuleResources.SARIF2004_OptimizeFileSize_Warning_EliminateLocationOnlyArtifacts_Text));
            }
        }

        private bool HasResultLocationsWithUriAndIndex(string resultPointer)
        {
            JToken resultToken = resultPointer.ToJToken(Context.InputLogToken);
            return
                resultToken.HasProperty(SarifPropertyName.Uri) &&
                resultToken.HasProperty(SarifPropertyName.Index);
        }

        private bool HasLocationOnlyArtifacts(string artifactPointer)
        {
            JToken artifactToken = artifactPointer.ToJToken(Context.InputLogToken);
            return
                artifactToken.HasProperty(SarifPropertyName.Location) &&
                artifactToken.Children().Count() == 1;
        }

        private void AnalyzeIdOnlyRules(Run run, string runPointer)
        {
            // We only verify first item in the rules array,
            // since tools will typically generate similar nodes.
            // This approach may cause occasional false negatives.
            // Also, `tool` and `driver` are mandatory fields, hence
            // null check in not required.
            ReportingDescriptor firstRule = run.Tool.Driver.Rules?.FirstOrDefault();

            if (firstRule == null)
            {
                return;
            }

            string firstRulePointer = runPointer
                .AtProperty(SarifPropertyName.Tool)
                .AtProperty(SarifPropertyName.Driver)
                .AtProperty(SarifPropertyName.Rules)
                .AtIndex(0);

            if (HasIdOnlyRules(firstRulePointer))
            {
                // {0}: The 'rules' array contains no information beyond the ids of the rules.
                // Removing this array might reduce the log file size without losing information.
                // In some scenarios (for example, when assessing compliance with policy), the
                // 'rules' array might be used to record the full set of rules that were evaluated.
                // In such a scenario, the 'rules' array should be retained even if it contains
                // only id information.
                LogResult(
                    firstRulePointer,
                    nameof(RuleResources.SARIF2004_OptimizeFileSize_Warning_EliminateIdOnlyRules_Text));
            }
        }

        private bool HasIdOnlyRules(string rulePointer)
        {
            JToken ruleToken = rulePointer.ToJToken(Context.InputLogToken);
            return
                ruleToken.HasProperty(SarifPropertyName.Id) &&
                ruleToken.Children().Count() == 1;
        }
    }
}
