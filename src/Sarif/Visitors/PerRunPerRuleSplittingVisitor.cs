// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    public class PerRunPerRuleSplittingVisitor : SplittingVisitor
    {
        private Dictionary<string, SarifLog> _ruleToSarifLogMap;

        public PerRunPerRuleSplittingVisitor(Func<Result, bool> filteringStrategy = null) : base(filteringStrategy)
        {
        }

        public override Run VisitRun(Run node)
        {
            _ruleToSarifLogMap = new Dictionary<string, SarifLog>();
            return base.VisitRun(node);
        }

        public override Result VisitResult(Result node)
        {
            if (!FilteringStrategy(node)) { return node; }

            string ruleId = node.RuleId;

            if (string.IsNullOrEmpty(ruleId) && node.RuleIndex > -1)
            {
                ruleId = CurrentRun.Tool.Driver.Rules?[node.RuleIndex].Id;
            }

            if (!_ruleToSarifLogMap.TryGetValue(ruleId, out SarifLog sarifLog))
            {
                _ruleToSarifLogMap[ruleId] = sarifLog = new SarifLog()
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

            ArtifactLocation artifactLocation = node.Locations?[0].PhysicalLocation?.ArtifactLocation;

            if (artifactLocation != null && artifactLocation.Index > -1)
            {
                int originalIndex = artifactLocation.Index;
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
