// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    public class SortingVisitor : SarifRewritingVisitor
    {
        private readonly IDictionary<int, int> ruleIndexMap;
        private readonly IDictionary<int, int> artifactIndexMap;

        public SortingVisitor()
        {
            this.ruleIndexMap = new Dictionary<int, int>();
            this.artifactIndexMap = new Dictionary<int, int>();
        }

        public override SarifLog VisitSarifLog(SarifLog node)
        {
            SarifLog current = base.VisitSarifLog(node);

            if (current?.Runs != null)
            {
                current.Runs = current.Runs.OrderBy(r => r, RunComparer.Instance).ToList();
            }

            return current;
        }

        public override Run VisitRun(Run node)
        {
            this.ruleIndexMap.Clear();
            this.artifactIndexMap.Clear();

            if (node?.Artifacts != null)
            {
                node.Artifacts = this.SortAndBuildIndexMap(node?.Artifacts, ArtifactComparer.Instance, this.artifactIndexMap);
            }

            Run current = base.VisitRun(node);

            if (current?.Results != null)
            {
                current.Results = current.Results.OrderBy(r => r, ResultComparer.Instance).ToList();
            }

            return current;
        }

        public override ToolComponent VisitToolComponent(ToolComponent node)
        {
            if (node?.Rules != null)
            {
                node.Rules = this.SortAndBuildIndexMap(node?.Rules, ReportingDescriptorComparer.Instance, this.ruleIndexMap);
            }

            return base.VisitToolComponent(node);
        }

        public override Result VisitResult(Result node)
        {
            Result current = base.VisitResult(node);

            if (current != null)
            {
                if (current.RuleIndex != -1 && this.ruleIndexMap.TryGetValue(current.RuleIndex, out int newIndex))
                {
                    current.RuleIndex = newIndex;
                }

                if (current.Locations != null)
                {
                    current.Locations = current.Locations.OrderBy(r => r, LocationComparer.Instance).ToList();
                }

                if (current.CodeFlows != null)
                {
                    current.CodeFlows = current.CodeFlows.OrderBy(r => r, CodeFlowComparer.Instance).ToList();
                }
            }

            return current;
        }

        public override CodeFlow VisitCodeFlow(CodeFlow node)
        {
            CodeFlow current = base.VisitCodeFlow(node);

            if (current?.ThreadFlows != null)
            {
                current.ThreadFlows = current.ThreadFlows.OrderBy(t => t, ThreadFlowComparer.Instance).ToList();
            }

            return current;
        }

        public override ThreadFlow VisitThreadFlow(ThreadFlow node)
        {
            ThreadFlow current = base.VisitThreadFlow(node);

            if (current?.Locations != null)
            {
                current.Locations = current.Locations.OrderBy(t => t, ThreadFlowLocationComparer.Instance).ToList();
            }

            return current;
        }

        public override Location VisitLocation(Location node)
        {
            Location current = base.VisitLocation(node);

            if (current?.LogicalLocations != null)
            {
                current.LogicalLocations = current.LogicalLocations.OrderBy(t => t, LogicalLocationComparer.Instance).ToList();
            }

            return current;
        }

        public override ArtifactLocation VisitArtifactLocation(ArtifactLocation node)
        {
            ArtifactLocation current = base.VisitArtifactLocation(node);

            if (current.Index != -1 && this.artifactIndexMap.TryGetValue(current.Index, out int newIndex))
            {
                current.Index = newIndex;
            }

            return current;
        }

        private IList<T> SortAndBuildIndexMap<T>(IList<T> list, IComparer<T> comparer, IDictionary<int, int> indexMapping)
        {
            if (list != null)
            {
                IDictionary<T, int> unsortedIndices = this.CacheListIndices(list);

                list = list.OrderBy(r => r, comparer).ToList();

                this.MapNewIndices(list, unsortedIndices, indexMapping);

                unsortedIndices.Clear();
            }

            return list;
        }

        private IDictionary<T, int> CacheListIndices<T>(IList<T> list)
        {
            // Assume each item in the list is unique (has different reference).
            // According to sarif-2.1.0-rtm.5.dotnet.json, artifacts array of runs and rules array of toolComponent
            // are defined as "uniqueItems".
            var dict = new Dictionary<T, int>(capacity: list.Count);

            for (int i = 0; i < list.Count; i++)
            {
                dict[list[i]] = i;
            }

            return dict;
        }

        private void MapNewIndices<T>(IList<T> newList, IDictionary<T, int> oldIndices, IDictionary<int, int> indexMapping)
        {
            for (int newIndex = 0; newIndex < newList.Count; newIndex++)
            {
                if (oldIndices.TryGetValue(newList[newIndex], out int oldIndex))
                {
                    indexMapping[oldIndex] = newIndex;
                }
            }
        }
    }
}
