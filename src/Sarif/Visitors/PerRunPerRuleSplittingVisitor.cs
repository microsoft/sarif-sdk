// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    public class PerRunPerRuleSplittingVisitor : SarifRewritingVisitor
    {
        public PerRunPerRuleSplittingVisitor(Func<Result, bool> filteringStrategy = null)
        {
            _filteringStrategy = filteringStrategy ?? FilteringStrategies.NewOrUpdatedResults;
        }

        private Run _currentRun;
        private Func<Result, bool> _filteringStrategy;
        private Dictionary<string, SarifLog> _ruleToSarifLogMap;

        public IList<SarifLog> SplitSarifLogs { get; private set; }

        // Each run will drive creation of a single SarifLog instance.
        public override Run VisitRun(Run node)
        {
            SplitSarifLogs = new List<SarifLog>();

            _currentRun = node;
            _ruleToSarifLogMap = new Dictionary<string, SarifLog>();

            return base.VisitRun(node);
        }

        public override Result VisitResult(Result node)
        {
            if (!_filteringStrategy(node)) { return node; }

            string ruleId = node.RuleId;

            if (string.IsNullOrEmpty(ruleId) && node.RuleIndex > -1)
            {
                ruleId = _currentRun.Tool.Driver.Rules?[node.RuleIndex].Id;
            }

            if (!_ruleToSarifLogMap.TryGetValue(ruleId, out SarifLog sarifLog))
            {
                _ruleToSarifLogMap[ruleId] = sarifLog = new SarifLog()
                { 
                    Runs = new[]
                    {
                        new Run
                        {
                            Tool = _currentRun.Tool,
                            Invocations = _currentRun.Invocations,
                            Results = new List<Result>()
                        },                        
                    }
                };
                SplitSarifLogs.Add(sarifLog);
            }

            ArtifactLocation artifactLocation = node.Locations?[0].PhysicalLocation?.ArtifactLocation;

            if (artifactLocation != null)
            {
                artifactLocation = artifactLocation.DeepClone();
                artifactLocation.Index = sarifLog.Runs[0].GetFileIndex(artifactLocation);
                node.Locations[0].PhysicalLocation.ArtifactLocation = artifactLocation;
            }

            sarifLog.Runs[0].Results.Add(node);

            return node;
        }
    }
}
