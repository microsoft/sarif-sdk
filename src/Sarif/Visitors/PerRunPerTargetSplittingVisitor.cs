// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    public class PerRunPerTargetSplittingVisitor : SplittingVisitor
    {
        private static ArtifactLocation s_emptyArtifactLocation = new ArtifactLocation();

        private Dictionary<string, SarifLog> _targetMap;

        public PerRunPerTargetSplittingVisitor(Func<Result, bool> filteringStrategy = null) : base(filteringStrategy)
        {
        }

        public override Run VisitRun(Run node)
        {
            _targetMap = new Dictionary<string, SarifLog>();
            return base.VisitRun(node);
        }

        public override Result VisitResult(Result node)
        {
            if (!FilteringStrategy(node)) { return node; }

            ArtifactLocation artifactLocation = s_emptyArtifactLocation;
            if (node.Locations[0].PhysicalLocation?.ArtifactLocation != null)
            {
                artifactLocation = node.Locations[0].PhysicalLocation?.ArtifactLocation;
            }

            if (artifactLocation == null)
            {
                throw new InvalidOperationException("Result.Locations.PhysicalLocation.ArtifactLocation is null.");
            }

            if (!_targetMap.TryGetValue(artifactLocation.Uri.ToString(), out SarifLog sarifLog))
            {
                sarifLog = _targetMap[artifactLocation.Uri.ToString()] = new SarifLog()
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
