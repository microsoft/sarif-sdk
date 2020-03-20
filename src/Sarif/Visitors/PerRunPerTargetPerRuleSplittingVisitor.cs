// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    public class PerRunPerTargetPerRuleSplittingVisitor : SplittingVisitor
    {
        private static readonly ArtifactLocation s_emptyArtifactLocation = new ArtifactLocation();

        private Dictionary<string, Dictionary<string, SarifLog>> _targetToRuleMap;

        public PerRunPerTargetPerRuleSplittingVisitor(Func<Result, bool> filteringStrategy = null) : base(filteringStrategy)
        {
        }

        public override Run VisitRun(Run node)
        {
            _targetToRuleMap = new Dictionary<string, Dictionary<string, SarifLog>>();
            return base.VisitRun(node);
        }

        public override Result VisitResult(Result node)
        {
            if (!FilteringStrategy(node)) { return node; }

            string ruleId;
            if (node.RuleIndex > -1 && CurrentRun.Tool?.Driver?.Rules?.Count > node.RuleIndex)
            {
                ruleId = CurrentRun.Tool.Driver.Rules?[node.RuleIndex].Id;
            }
            else
            {
                // Remove the rule pattern index from the rule id. e.g. CSCAN0230/5
                int lastIndexOf = node.RuleId.LastIndexOf('/');
                ruleId = node.RuleId.Substring(0, lastIndexOf >= 0 ? lastIndexOf : node.RuleId.Length);
            }

            ArtifactLocation artifactLocation = node.Locations?[0]?.PhysicalLocation?.ArtifactLocation ?? s_emptyArtifactLocation;

            if (!_targetToRuleMap.TryGetValue(artifactLocation.Uri.ToString(), out Dictionary<string, SarifLog> ruleToSarifLogMap))
            {
                ruleToSarifLogMap = _targetToRuleMap[artifactLocation.Uri.ToString()] = new Dictionary<string, SarifLog>();
            }

            if (!ruleToSarifLogMap.TryGetValue(ruleId, out SarifLog sarifLog))
            {
                ruleToSarifLogMap[ruleId] = sarifLog = new SarifLog()
                {
                    Runs = new[]
                    {
                        new Run
                        {
                            Tool = CurrentRun.Tool,
                            Invocations = CurrentRun.Invocations,
                            Results = new List<Result>()
                        },
                    }
                };
                SplitSarifLogs.Add(sarifLog);
            }

            if (artifactLocation != null && artifactLocation.Index > -1)
            {
                int originalIndex = CurrentRun.GetFileIndex(artifactLocation);
                artifactLocation = artifactLocation.DeepClone();
                artifactLocation.Index = sarifLog.Runs[0].GetFileIndex(artifactLocation);
                node.Locations[0].PhysicalLocation.ArtifactLocation = artifactLocation;
                sarifLog.Runs[0].Artifacts[artifactLocation.Index] = CurrentRun.Artifacts[originalIndex];
            }

            sarifLog.Runs[0].Results.Add(node);

            return node;
        }
    }
}
