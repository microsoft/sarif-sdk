// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
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
            ruleIndexMap = new ConcurrentDictionary<int, int>();
            artifactIndexMap = new ConcurrentDictionary<int, int>();
        }

        public override Run VisitRun(Run node)
        {
            if (node?.Artifacts != null)
            {
                IDictionary<Artifact, int> oldIndexes = new Dictionary<Artifact, int>(capacity: node.Artifacts.Count);
                // save old indexes
                for (int i = 0; i < node.Artifacts.Count; i++)
                {
                    if (!oldIndexes.TryGetValue(node.Artifacts[i], out int index))
                    {
                        oldIndexes.Add(node.Artifacts[i], i);
                    }
                }

                // sort
                node.Artifacts = node.Artifacts.OrderBy(a => a, ArtifactSortingComparer.Instance).ToList();

                // udpate new indexes
                for (int newIndex = 0; newIndex < node.Artifacts.Count; newIndex++)
                {
                    if (oldIndexes.TryGetValue(node.Artifacts[newIndex], out int oldIndex)
                        && !artifactIndexMap.TryGetValue(oldIndex, out _))
                    {
                        artifactIndexMap.Add(oldIndex, newIndex);
                    }
                }

                oldIndexes.Clear();
            }

            // traverse child nodes first, so the child list properties should be sorted
            Run current = base.VisitRun(node);

            // then sort properties of current node
            if (current?.Results != null)
            {
                current.Results = current.Results.OrderBy(r => r, ResultSortingComparer.Instance).ToList();
            }

            return current;
        }

        public override ToolComponent VisitToolComponent(ToolComponent node)
        {
            ToolComponent current = base.VisitToolComponent(node);
            if (current?.Rules != null)
            {
                IDictionary<ReportingDescriptor, int> oldIndexes = new Dictionary<ReportingDescriptor, int>(capacity: current.Rules.Count);
                // before sort the rules, save old indexes
                for (int i = 0; i < current.Rules.Count; i++)
                {
                    if (!oldIndexes.TryGetValue(current.Rules[i], out int index))
                    {
                        oldIndexes.Add(current.Rules[i], i);
                    }
                }

                // sort rules
                current.Rules = current.Rules.OrderBy(r => r, ReportingDescriptorSortingComparer.Instance).ToList();

                // udpate new indexes
                for (int newIndex = 0; newIndex < current.Rules.Count; newIndex++)
                {
                    if (oldIndexes.TryGetValue(current.Rules[newIndex], out int oldIndex)
                        && !ruleIndexMap.TryGetValue(oldIndex, out _))
                    {
                        ruleIndexMap.Add(oldIndex, newIndex);
                    }
                }

                oldIndexes.Clear();
            }
            return current;
        }

        public override Result VisitResult(Result node)
        {
            Result current = base.VisitResult(node);
            if (current != null)
            {
                // update old index to new index
                if (current.RuleIndex != -1
                    && ruleIndexMap.TryGetValue(current.RuleIndex, out int newIndex))
                {
                    current.RuleIndex = newIndex;
                }

                if (current.Locations != null)
                {
                    current.Locations = current.Locations.OrderBy(r => r, LocationSortingComparer.Instance).ToList();
                }
                if (current.CodeFlows != null)
                {
                    current.CodeFlows = current.CodeFlows.OrderBy(r => r, CodeFlowSortingComparer.Instance).ToList();
                }
            }
            return current;
        }

        public override CodeFlow VisitCodeFlow(CodeFlow node)
        {
            CodeFlow current = base.VisitCodeFlow(node);
            if (current?.ThreadFlows != null)
            {
                current.ThreadFlows = current.ThreadFlows.OrderBy(t => t, ThreadFlowSortingComparer.Instance).ToList();
            }
            return current;
        }

        public override ThreadFlow VisitThreadFlow(ThreadFlow node)
        {
            ThreadFlow current = base.VisitThreadFlow(node);
            if (current?.Locations != null)
            {
                current.Locations = current.Locations.OrderBy(t => t, ThreadFlowLocationSortingComparer.Instance).ToList();
            }
            return current;
        }

        public override ThreadFlowLocation VisitThreadFlowLocation(ThreadFlowLocation node)
        {
            return base.VisitThreadFlowLocation(node);
        }

        public override Location VisitLocation(Location node)
        {
            Location current = base.VisitLocation(node);
            if (current?.LogicalLocations != null)
            {
                current.LogicalLocations = current.LogicalLocations.OrderBy(t => t, LogicalLocationSortingComparer.Instance).ToList();
            }
            return current;
        }

        public override ArtifactLocation VisitArtifactLocation(ArtifactLocation node)
        {
            ArtifactLocation current = base.VisitArtifactLocation(node);
            // update old index to new index
            if (current.Index != -1
                && artifactIndexMap.TryGetValue(current.Index, out int newIndex))
            {
                current.Index = newIndex;
            }

            return current;
        }
    }
}
