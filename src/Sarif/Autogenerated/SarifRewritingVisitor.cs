// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.CodeDom.Compiler;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Rewriting visitor for the Sarif object model.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.11.0.0")]
    public abstract class SarifRewritingVisitor
    {
        /// <summary>
        /// Starts a rewriting visit of a node in the Sarif object model.
        /// </summary>
        /// <param name="node">
        /// The node to rewrite.
        /// </param>
        /// <returns>
        /// A rewritten instance of the node.
        /// </returns>
        public virtual object Visit(ISarifNode node)
        {
            return this.VisitActual(node);
        }

        /// <summary>
        /// Visits and rewrites a node in the Sarif object model.
        /// </summary>
        /// <param name="node">
        /// The node to rewrite.
        /// </param>
        /// <returns>
        /// A rewritten instance of the node.
        /// </returns>
        public virtual object VisitActual(ISarifNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }

            switch (node.SarifNodeKind)
            {
                case SarifNodeKind.AnnotatedCodeLocation:
                    return VisitAnnotatedCodeLocation((AnnotatedCodeLocation)node);
                case SarifNodeKind.FileChange:
                    return VisitFileChange((FileChange)node);
                case SarifNodeKind.FileData:
                    return VisitFileData((FileData)node);
                case SarifNodeKind.Fix:
                    return VisitFix((Fix)node);
                case SarifNodeKind.FormattedMessage:
                    return VisitFormattedMessage((FormattedMessage)node);
                case SarifNodeKind.Hash:
                    return VisitHash((Hash)node);
                case SarifNodeKind.Location:
                    return VisitLocation((Location)node);
                case SarifNodeKind.LogicalLocationComponent:
                    return VisitLogicalLocationComponent((LogicalLocationComponent)node);
                case SarifNodeKind.PhysicalLocation:
                    return VisitPhysicalLocation((PhysicalLocation)node);
                case SarifNodeKind.Region:
                    return VisitRegion((Region)node);
                case SarifNodeKind.Replacement:
                    return VisitReplacement((Replacement)node);
                case SarifNodeKind.Result:
                    return VisitResult((Result)node);
                case SarifNodeKind.Rule:
                    return VisitRule((Rule)node);
                case SarifNodeKind.Run:
                    return VisitRun((Run)node);
                case SarifNodeKind.SarifLog:
                    return VisitSarifLog((SarifLog)node);
                case SarifNodeKind.Stack:
                    return VisitStack((Stack)node);
                case SarifNodeKind.StackFrame:
                    return VisitStackFrame((StackFrame)node);
                case SarifNodeKind.Tool:
                    return VisitTool((Tool)node);
                default:
                    return node;
            }
        }

        private T VisitNullChecked<T>(T node) where T : class, ISarifNode
        {
            if (node == null)
            {
                return null;
            }

            return (T)Visit(node);
        }

        public virtual AnnotatedCodeLocation VisitAnnotatedCodeLocation(AnnotatedCodeLocation node)
        {
            if (node != null)
            {
                node.PhysicalLocation = VisitNullChecked(node.PhysicalLocation);
            }

            return node;
        }

        public virtual FileChange VisitFileChange(FileChange node)
        {
            if (node != null)
            {
                if (node.Replacements != null)
                {
                    for (int index_0 = 0; index_0 < node.Replacements.Count; ++index_0)
                    {
                        node.Replacements[index_0] = VisitNullChecked(node.Replacements[index_0]);
                    }
                }
            }

            return node;
        }

        public virtual FileData VisitFileData(FileData node)
        {
            if (node != null)
            {
                if (node.Hashes != null)
                {
                    for (int index_0 = 0; index_0 < node.Hashes.Count; ++index_0)
                    {
                        node.Hashes[index_0] = VisitNullChecked(node.Hashes[index_0]);
                    }
                }
            }

            return node;
        }

        public virtual Fix VisitFix(Fix node)
        {
            if (node != null)
            {
                if (node.FileChanges != null)
                {
                    for (int index_0 = 0; index_0 < node.FileChanges.Count; ++index_0)
                    {
                        node.FileChanges[index_0] = VisitNullChecked(node.FileChanges[index_0]);
                    }
                }
            }

            return node;
        }

        public virtual FormattedMessage VisitFormattedMessage(FormattedMessage node)
        {
            if (node != null)
            {
            }

            return node;
        }

        public virtual Hash VisitHash(Hash node)
        {
            if (node != null)
            {
            }

            return node;
        }

        public virtual Location VisitLocation(Location node)
        {
            if (node != null)
            {
                node.AnalysisTarget = VisitNullChecked(node.AnalysisTarget);
                node.ResultFile = VisitNullChecked(node.ResultFile);
            }

            return node;
        }

        public virtual LogicalLocationComponent VisitLogicalLocationComponent(LogicalLocationComponent node)
        {
            if (node != null)
            {
            }

            return node;
        }

        public virtual PhysicalLocation VisitPhysicalLocation(PhysicalLocation node)
        {
            if (node != null)
            {
                node.Region = VisitNullChecked(node.Region);
            }

            return node;
        }

        public virtual Region VisitRegion(Region node)
        {
            if (node != null)
            {
            }

            return node;
        }

        public virtual Replacement VisitReplacement(Replacement node)
        {
            if (node != null)
            {
            }

            return node;
        }

        public virtual Result VisitResult(Result node)
        {
            if (node != null)
            {
                node.FormattedMessage = VisitNullChecked(node.FormattedMessage);
                if (node.Locations != null)
                {
                    for (int index_0 = 0; index_0 < node.Locations.Count; ++index_0)
                    {
                        node.Locations[index_0] = VisitNullChecked(node.Locations[index_0]);
                    }
                }

                if (node.Stacks != null)
                {
                    for (int index_0 = 0; index_0 < node.Stacks.Count; ++index_0)
                    {
                        node.Stacks[index_0] = VisitNullChecked(node.Stacks[index_0]);
                    }
                }

                if (node.CodeFlows != null)
                {
                    for (int index_0 = 0; index_0 < node.CodeFlows.Count; ++index_0)
                    {
                        var value_0 = node.CodeFlows[index_0];
                        if (value_0 != null)
                        {
                            for (int index_1 = 0; index_1 < value_0.Count; ++index_1)
                            {
                                value_0[index_1] = VisitNullChecked(value_0[index_1]);
                            }
                        }
                    }
                }

                if (node.RelatedLocations != null)
                {
                    for (int index_0 = 0; index_0 < node.RelatedLocations.Count; ++index_0)
                    {
                        node.RelatedLocations[index_0] = VisitNullChecked(node.RelatedLocations[index_0]);
                    }
                }

                if (node.Fixes != null)
                {
                    for (int index_0 = 0; index_0 < node.Fixes.Count; ++index_0)
                    {
                        node.Fixes[index_0] = VisitNullChecked(node.Fixes[index_0]);
                    }
                }
            }

            return node;
        }

        public virtual Rule VisitRule(Rule node)
        {
            if (node != null)
            {
            }

            return node;
        }

        public virtual Run VisitRun(Run node)
        {
            if (node != null)
            {
                node.Tool = VisitNullChecked(node.Tool);
                if (node.Files != null)
                {
                    foreach (var key in node.Files.Keys)
                    {
                        var value = node.Files[key];
                        if (value != null)
                        {
                            for (int index_0 = 0; index_0 < node.Files[key].Count; ++index_0)
                            {
                                node.Files[key][index_0] = VisitNullChecked(node.Files[key][index_0]);
                            }
                        }
                    }
                }

                if (node.LogicalLocations != null)
                {
                    foreach (var key in node.LogicalLocations.Keys)
                    {
                        var value = node.LogicalLocations[key];
                        if (value != null)
                        {
                            for (int index_0 = 0; index_0 < node.LogicalLocations[key].Count; ++index_0)
                            {
                                node.LogicalLocations[key][index_0] = VisitNullChecked(node.LogicalLocations[key][index_0]);
                            }
                        }
                    }
                }

                if (node.Results != null)
                {
                    for (int index_0 = 0; index_0 < node.Results.Count; ++index_0)
                    {
                        node.Results[index_0] = VisitNullChecked(node.Results[index_0]);
                    }
                }

                if (node.Rules != null)
                {
                    foreach (var key in node.Rules.Keys)
                    {
                        var value = node.Rules[key];
                        if (value != null)
                        {
                            node.Rules[key] = VisitNullChecked(value);
                        }
                    }
                }
            }

            return node;
        }

        public virtual SarifLog VisitSarifLog(SarifLog node)
        {
            if (node != null)
            {
                if (node.Runs != null)
                {
                    for (int index_0 = 0; index_0 < node.Runs.Count; ++index_0)
                    {
                        node.Runs[index_0] = VisitNullChecked(node.Runs[index_0]);
                    }
                }
            }

            return node;
        }

        public virtual Stack VisitStack(Stack node)
        {
            if (node != null)
            {
                if (node.Frames != null)
                {
                    for (int index_0 = 0; index_0 < node.Frames.Count; ++index_0)
                    {
                        node.Frames[index_0] = VisitNullChecked(node.Frames[index_0]);
                    }
                }
            }

            return node;
        }

        public virtual StackFrame VisitStackFrame(StackFrame node)
        {
            if (node != null)
            {
                node.Location = VisitNullChecked(node.Location);
            }

            return node;
        }

        public virtual Tool VisitTool(Tool node)
        {
            if (node != null)
            {
            }

            return node;
        }
    }
}