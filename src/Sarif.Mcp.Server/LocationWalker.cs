// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Mcp.Server
{
    /// <summary>
    /// Categorizes where a <see cref="WalkedLocation"/> was discovered within a Result.
    /// </summary>
    internal enum WalkedLocationKind
    {
        ResultLocation,
        RelatedLocation,
        CodeFlowStep,
        StackFrame,
        FixArtifactChange,
        AnalysisTarget,
        Attachment,
        GraphNode
    }

    internal readonly struct WalkedLocation
    {
        public PhysicalLocation? PhysicalLocation { get; init; }

        public ArtifactLocation? ArtifactLocation { get; init; }

        public WalkedLocationKind Kind { get; init; }

        /// <summary>
        /// True for locations that carry a Region (results, related, code flow steps, stack frames).
        /// False for fix artifact changes, analysis targets, and attachments which only have an ArtifactLocation.
        /// </summary>
        public bool CanHaveRegion => this.Kind
            is not (WalkedLocationKind.FixArtifactChange
                or WalkedLocationKind.AnalysisTarget
                or WalkedLocationKind.Attachment);
    }

    /// <summary>
    /// Recursively finds every location in a SARIF Result that needs artifact
    /// table caching and optional region enrichment. Modeled after
    /// <c>SarifRewritingVisitor.VisitPhysicalLocation</c> from the SDK.
    /// </summary>
    internal static class LocationWalker
    {
        /// <summary>
        /// Yields every location in a Result that may need artifact index resolution
        /// and/or region enrichment (snippet, context region, charOffset, hashes).
        /// </summary>
        public static IEnumerable<WalkedLocation> Walk(Result result)
        {
            // 1. result.Locations[]
            if (result.Locations != null)
            {
                foreach (Location loc in result.Locations)
                {
                    if (loc.PhysicalLocation != null)
                    {
                        yield return new WalkedLocation
                        {
                            PhysicalLocation = loc.PhysicalLocation,
                            ArtifactLocation = loc.PhysicalLocation.ArtifactLocation,
                            Kind = WalkedLocationKind.ResultLocation
                        };
                    }
                }
            }

            // 1b. result.AnalysisTarget (standalone ArtifactLocation)
            if (result.AnalysisTarget != null)
            {
                yield return new WalkedLocation
                {
                    PhysicalLocation = null,
                    ArtifactLocation = result.AnalysisTarget,
                    Kind = WalkedLocationKind.AnalysisTarget
                };
            }

            // 2. result.RelatedLocations[]
            if (result.RelatedLocations != null)
            {
                foreach (Location loc in result.RelatedLocations)
                {
                    if (loc.PhysicalLocation != null)
                    {
                        yield return new WalkedLocation
                        {
                            PhysicalLocation = loc.PhysicalLocation,
                            ArtifactLocation = loc.PhysicalLocation.ArtifactLocation,
                            Kind = WalkedLocationKind.RelatedLocation
                        };
                    }
                }
            }

            // 3. result.CodeFlows[].ThreadFlows[].Locations[].Location
            if (result.CodeFlows != null)
            {
                foreach (CodeFlow codeFlow in result.CodeFlows)
                {
                    if (codeFlow.ThreadFlows == null) { continue; }

                    foreach (ThreadFlow threadFlow in codeFlow.ThreadFlows)
                    {
                        if (threadFlow.Locations == null) { continue; }

                        foreach (ThreadFlowLocation tfl in threadFlow.Locations)
                        {
                            if (tfl.Location?.PhysicalLocation != null)
                            {
                                yield return new WalkedLocation
                                {
                                    PhysicalLocation = tfl.Location.PhysicalLocation,
                                    ArtifactLocation = tfl.Location.PhysicalLocation.ArtifactLocation,
                                    Kind = WalkedLocationKind.CodeFlowStep
                                };
                            }
                        }
                    }
                }
            }

            // 4. result.Stacks[].Frames[].Location
            if (result.Stacks != null)
            {
                foreach (Stack stack in result.Stacks)
                {
                    if (stack.Frames == null) { continue; }

                    foreach (StackFrame frame in stack.Frames)
                    {
                        if (frame.Location?.PhysicalLocation != null)
                        {
                            yield return new WalkedLocation
                            {
                                PhysicalLocation = frame.Location.PhysicalLocation,
                                ArtifactLocation = frame.Location.PhysicalLocation.ArtifactLocation,
                                Kind = WalkedLocationKind.StackFrame
                            };
                        }
                    }
                }
            }

            // 5. result.Fixes[].ArtifactChanges[].ArtifactLocation
            if (result.Fixes != null)
            {
                foreach (Fix fix in result.Fixes)
                {
                    if (fix.ArtifactChanges == null) { continue; }

                    foreach (ArtifactChange change in fix.ArtifactChanges)
                    {
                        if (change.ArtifactLocation != null)
                        {
                            yield return new WalkedLocation
                            {
                                PhysicalLocation = null,
                                ArtifactLocation = change.ArtifactLocation,
                                Kind = WalkedLocationKind.FixArtifactChange
                            };
                        }
                    }
                }
            }

            // 6. result.Attachments[].Location
            if (result.Attachments != null)
            {
                foreach (Attachment attachment in result.Attachments)
                {
                    yield return new WalkedLocation
                    {
                        PhysicalLocation = null,
                        ArtifactLocation = attachment.ArtifactLocation,
                        Kind = WalkedLocationKind.Attachment
                    };
                }
            }

            // 7. result.Graphs[].Nodes[].Location (recursive for children)
            if (result.Graphs != null)
            {
                foreach (Graph graph in result.Graphs)
                {
                    if (graph.Nodes == null) { continue; }

                    foreach (WalkedLocation walked in WalkNodes(graph.Nodes))
                    {
                        yield return walked;
                    }
                }
            }
        }

        private static IEnumerable<WalkedLocation> WalkNodes(IList<Node> nodes)
        {
            foreach (Node node in nodes)
            {
                if (node.Location?.PhysicalLocation != null)
                {
                    yield return new WalkedLocation
                    {
                        PhysicalLocation = node.Location.PhysicalLocation,
                        ArtifactLocation = node.Location.PhysicalLocation.ArtifactLocation,
                        Kind = WalkedLocationKind.GraphNode
                    };
                }

                if (node.Children != null)
                {
                    foreach (WalkedLocation child in WalkNodes(node.Children))
                    {
                        yield return child;
                    }
                }
            }
        }
    }
}
