// Copyright (c) Microsoft. All rights reserved.
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
        /// 
        /// In addition to the avoiding unnecessary arrays, there are other ways to optimize the
        /// size of SARIF log files.
        /// 
        /// Prefer the result object properties 'ruleId' and 'ruleIndex' to the nested object-valued
        /// property 'result.rule', unless the rule comes from a tool component other than the driver
        /// (in which case only 'result.rule' can accurately point to the metadata for the rule).
        /// The 'ruleId' and 'ruleIndex' properties are shorter and just as clear.
        /// 
        /// Do not specify the result object's 'analysisTarget' property unless it differs from the
        /// result location. The canonical scenario for using 'result.analysisTarget' is a C/C++ language
        /// analyzer that is instructed to analyze example.c, and detects a result in the included file
        /// example.h. In this case, 'analysisTarget' is example.c, and the result location is in example.h.
        /// </summary>
        public override MultiformatMessageString FullDescription => new MultiformatMessageString { Text = RuleResources.SARIF2004_OptimizeFileSize_FullDescription_Text };

        protected override IEnumerable<string> MessageResourceNames => new string[] {
            nameof(RuleResources.SARIF2004_OptimizeFileSize_Warning_AvoidDuplicativeAnalysisTarget_Text),
            nameof(RuleResources.SARIF2004_OptimizeFileSize_Warning_AvoidDuplicativeResultRuleInformation_Text),
            nameof(RuleResources.SARIF2004_OptimizeFileSize_Warning_EliminateLocationOnlyArtifacts_Text),
            nameof(RuleResources.SARIF2004_OptimizeFileSize_Warning_EliminateIdOnlyRules_Text),
            nameof(RuleResources.SARIF2004_OptimizeFileSize_Warning_PreferRuleId_Text)
        };

        public override FailureLevel DefaultLevel => FailureLevel.Warning;

        private string driverGuid;

        protected override void Analyze(Run run, string runPointer)
        {
            AnalyzeLocationOnlyArtifacts(run, runPointer);
            AnalyzeIdOnlyRules(run, runPointer);

            this.driverGuid = run.Tool.Driver?.Guid;
        }

        protected override void Analyze(Result result, string resultPointer)
        {
            ReportUnnecessaryAnalysisTarget(result, resultPointer);
            ReportRuleDuplication(result, resultPointer);
        }

        private void ReportRuleDuplication(Result result, string resultPointer)
        {
            if (result.Rule != null)
            {
                if (result.Rule.ToolComponent != null)
                {
                    if (result.Rule.ToolComponent.RefersToDriver(this.driverGuid))
                    {
                        // The result at '{0}' uses the 'rule' property to specify
                        // the violated rule, but this is not necessary because the rule
                        // is defined by 'tool.driver'. Use the 'ruleId' and 'ruleIndex'
                        // instead, because they are shorter and just as clear.
                        LogResult(
                            resultPointer,
                            nameof(RuleResources.SARIF2004_OptimizeFileSize_Warning_PreferRuleId_Text));
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(result.RuleId) || result.RuleIndex >= 0)
                        {
                            // '{0}' uses the 'rule' property to specify the violated rule, so it
                            // is not necessary also to specify 'ruleId' or 'ruleIndex'. This
                            // unnecessarily increases log file size. Remove the 'ruleId' and
                            // 'ruleIndex' properties.
                            LogResult(
                                resultPointer,
                                nameof(RuleResources.SARIF2004_OptimizeFileSize_Warning_AvoidDuplicativeResultRuleInformation_Text));
                        }
                    }                    
                }
                else
                {
                    // The result at '{0}' uses the 'rule' property to specify
                    // the violated rule, but this is not necessary because the rule
                    // is defined by 'tool.driver'. Use the 'ruleId' and 'ruleIndex'
                    // instead, because they are shorter and just as clear.
                    LogResult(
                        resultPointer,
                        nameof(RuleResources.SARIF2004_OptimizeFileSize_Warning_PreferRuleId_Text));
                }
            }
        }

        private void ReportUnnecessaryAnalysisTarget(Result result, string resultPointer)
        {
            if (result.Locations != null && result.AnalysisTarget != null)
            {
                foreach (Location location in result.Locations)
                {
                    if (location?.PhysicalLocation?.ArtifactLocation?.Uri == result.AnalysisTarget.Uri)
                    {
                        // The 'analysisTarget' property '{1}' at '{0}' can be removed because it is the same
                        // as the result location. This unnecessarily increases log file size. The
                        // 'analysisTarget' property is used to distinguish cases when a tool detects a result
                        // in a file (such as an included header) that is different than the file that was
                        // scanned (such as a .cpp file that included the header).
                        LogResult(
                            resultPointer.AtProperty(SarifPropertyName.AnalysisTarget),
                            nameof(RuleResources.SARIF2004_OptimizeFileSize_Warning_AvoidDuplicativeAnalysisTarget_Text),
                            result.AnalysisTarget.Uri.OriginalString);
                        break;
                    }
                }
            }
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

            string artifactPointer = runPointer.AtProperty(SarifPropertyName.Artifacts);

            string firstArtifactPointer = artifactPointer.AtIndex(0);

            string firstResultLocationPointer = runPointer
                .AtProperty(SarifPropertyName.Results).AtIndex(0)
                .AtProperty(SarifPropertyName.Locations).AtIndex(0)
                .AtProperty(SarifPropertyName.PhysicalLocation)
                .AtProperty(SarifPropertyName.ArtifactLocation);

            if (HasResultLocationsWithUriAndIndex(firstResultLocationPointer) && HasLocationOnlyArtifacts(firstArtifactPointer))
            {
                // The 'artifacts' array at '{0}' contains no information beyond the locations of the
                // artifacts. Removing this array might  reduce the log file size without losing
                // information. In some scenarios (for example, when assessing compliance with policy),
                // the 'artifacts' array might be used to record the full set of artifacts that were
                // analyzed. In such a scenario, the 'artifacts' array should be retained even if it
                // contains only location information.
                LogResult(
                    artifactPointer,
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
            // null check is not required.
            ReportingDescriptor firstRule = run.Tool.Driver.Rules?.FirstOrDefault();

            if (firstRule == null)
            {
                return;
            }

            string rulesPointer = runPointer
                .AtProperty(SarifPropertyName.Tool)
                .AtProperty(SarifPropertyName.Driver)
                .AtProperty(SarifPropertyName.Rules);

            string firstRulePointer = rulesPointer.AtIndex(0);
            if (HasIdOnlyRules(firstRulePointer))
            {
                // The 'rules' array at '{0}' contains no information beyond the ids of the rules.
                // Removing this array might reduce the log file size without losing information.
                // In some scenarios (for example, when assessing compliance with policy), the
                // 'rules' array might be used to record the full set of rules that were evaluated.
                // In such a scenario, the 'rules' array should be retained even if it contains
                // only id information.
                LogResult(
                    rulesPointer,
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
