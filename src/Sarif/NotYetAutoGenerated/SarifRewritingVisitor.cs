// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>Rewriting visitor for a Sarif tree.</summary>
    public abstract class SarifRewritingVisitor
    {
        /// <summary>Starts a rewriting visit of a Sarif tree.</summary>
        /// <param name="node">The node to rewrite.</param>
        public virtual object Visit(ISyntax node)
        {
            return this.VisitActual(node);
        }

        /// <summary>Executes a visit of a Sarif tree.</summary>
        /// <param name="node">The node to visit.</param>
        public virtual object VisitActual(ISyntax node)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }

            switch (node.SyntaxKind)
            {
                case SarifKind.AnnotatedCodeLocation:
                    return this.VisitAnnotatedCodeLocation((AnnotatedCodeLocation)node);
                case SarifKind.FileChange:
                    return this.VisitFileChange((FileChange)node);
                case SarifKind.FileReference:
                    return this.VisitFileReference((FileReference)node);
                case SarifKind.Fix:
                    return this.VisitFix((Fix)node);
                case SarifKind.FormattedMessage:
                    return this.VisitFormattedMessage((FormattedMessage)node);
                case SarifKind.Hash:
                    return this.VisitHash((Hash)node);
                case SarifKind.Location:
                    return this.VisitLocation((Location)node);
                case SarifKind.LogicalLocationComponent:
                    return this.VisitLogicalLocationComponent((LogicalLocationComponent)node);
                case SarifKind.PhysicalLocation:
                    return this.VisitPhysicalLocation((PhysicalLocation)node);
                case SarifKind.Region:
                    return this.VisitRegion((Region)node);
                case SarifKind.Replacement:
                    return this.VisitReplacement((Replacement)node);
                case SarifKind.Result:
                    return this.VisitResult((Result)node);
                case SarifKind.SarifLog:
                    return this.VisitSarifLog((SarifLog)node);
                case SarifKind.RuleDescriptor:
                    return this.VisitRuleDescriptor((RuleDescriptor)node);
                case SarifKind.RunInfo:
                    return this.VisitRunInfo((RunInfo)node);
                case SarifKind.RunLog:
                    return this.VisitRunLog((RunLog)node);
                case SarifKind.ToolInfo:
                    return this.VisitToolInfo((ToolInfo)node);
                default:
                    return node;
            }
        }

        private T VisitNullChecked<T>(T node)
            where T : class, ISyntax
        {
            if (node == null)
            {
                return null;
            }

            return (T)this.Visit(node);
        }

        /// <summary>Rewrites a AnnotatedCodeLocation node in a Sarif tree.</summary>
        /// <param name="node">A AnnotatedCodeLocation node to visit.</param>
        public virtual AnnotatedCodeLocation VisitAnnotatedCodeLocation(AnnotatedCodeLocation node)
        {
            if (node != null)
            {
                node.PhysicalLocation = this.VisitNullChecked(node.PhysicalLocation);
            }

            return node;
        }

        /// <summary>Rewrites a FileChange node in a Sarif tree.</summary>
        /// <param name="node">A FileChange node to visit.</param>
        public virtual FileChange VisitFileChange(FileChange node)
        {
            if (node != null)
            {
                if (node.Replacements != null)
                {
                    for (int index_0 = 0; index_0 < node.Replacements.Count; ++index_0)
                    {
                        node.Replacements[index_0] = this.VisitNullChecked(node.Replacements[index_0]);
                    }
                }

            }

            return node;
        }

        /// <summary>Rewrites a FileReference node in a Sarif tree.</summary>
        /// <param name="node">A FileReference node to visit.</param>
        public virtual FileReference VisitFileReference(FileReference node)
        {
            if (node != null)
            {
                if (node.Hashes != null)
                {
                    for (int index_0 = 0; index_0 < node.Hashes.Count; ++index_0)
                    {
                        node.Hashes[index_0] = this.VisitNullChecked(node.Hashes[index_0]);
                    }
                }

            }

            return node;
        }

        /// <summary>Rewrites a Fix node in a Sarif tree.</summary>
        /// <param name="node">A Fix node to visit.</param>
        public virtual Fix VisitFix(Fix node)
        {
            if (node != null)
            {
                if (node.FileChanges != null)
                {
                    for (int index_0 = 0; index_0 < node.FileChanges.Count; ++index_0)
                    {
                        node.FileChanges[index_0] = this.VisitNullChecked(node.FileChanges[index_0]);
                    }
                }

            }

            return node;
        }

        /// <summary>Rewrites a FormattedMessage node in a Sarif tree.</summary>
        /// <param name="node">A FormattedMessage node to visit.</param>
        public virtual FormattedMessage VisitFormattedMessage(FormattedMessage node)
        {
            if (node != null)
            {
            }

            return node;
        }

        /// <summary>Rewrites a Hash node in a Sarif tree.</summary>
        /// <param name="node">A Hash node to visit.</param>
        public virtual Hash VisitHash(Hash node)
        {
            if (node != null)
            {
            }

            return node;
        }

        /// <summary>Rewrites a Location node in a Sarif tree.</summary>
        /// <param name="node">A Location node to visit.</param>
        public virtual Location VisitLocation(Location node)
        {
            if (node != null)
            {
                node.AnalysisTarget = this.VisitNullChecked(node.AnalysisTarget);
                node.ResultFile = this.VisitNullChecked(node.ResultFile);
                if (node.LogicalLocation != null)
                {
                    for (int index_0 = 0; index_0 < node.LogicalLocation.Count; ++index_0)
                    {
                        node.LogicalLocation[index_0] = this.VisitNullChecked(node.LogicalLocation[index_0]);
                    }
                }

            }

            return node;
        }

        /// <summary>Rewrites a LogicalLocationComponent node in a Sarif tree.</summary>
        /// <param name="node">A LogicalLocationComponent node to visit.</param>
        public virtual LogicalLocationComponent VisitLogicalLocationComponent(LogicalLocationComponent node)
        {
            if (node != null)
            {
            }

            return node;
        }

        /// <summary>Rewrites a PhysicalLocation node in a Sarif tree.</summary>
        /// <param name="node">A PhysicalLocation node to visit.</param>
        public virtual PhysicalLocation VisitPhysicalLocation(PhysicalLocation node)
        {
            if (node != null)
            {
                node.Region = this.VisitNullChecked(node.Region);
            }

            return node;
        }

        /// <summary>Rewrites a Region node in a Sarif tree.</summary>
        /// <param name="node">A Region node to visit.</param>
        public virtual Region VisitRegion(Region node)
        {
            if (node != null)
            {
            }

            return node;
        }

        /// <summary>Rewrites a Replacement node in a Sarif tree.</summary>
        /// <param name="node">A Replacement node to visit.</param>
        public virtual Replacement VisitReplacement(Replacement node)
        {
            if (node != null)
            {
            }

            return node;
        }

        /// <summary>Rewrites a Result node in a Sarif tree.</summary>
        /// <param name="node">A Result node to visit.</param>
        public virtual Result VisitResult(Result node)
        {
            if (node != null)
            {
                node.FormattedMessage = this.VisitNullChecked(node.FormattedMessage);
                if (node.Locations != null)
                {
                    for (int index_0 = 0; index_0 < node.Locations.Count; ++index_0)
                    {
                        node.Locations[index_0] = this.VisitNullChecked(node.Locations[index_0]);
                    }
                }

                if (node.Stacks != null)
                {
                    for (int index_0 = 0; index_0 < node.Stacks.Count; ++index_0)
                    {
                        var value_0 = node.Stacks[index_0];
                        if (value_0 != null)
                        {
                            for (int index_1 = 0; index_1 < value_0.Count; ++index_1)
                            {
                                value_0[index_1] = this.VisitNullChecked(value_0[index_1]);
                            }
                        }
                    }
                }

                if (node.ExecutionFlows != null)
                {
                    for (int index_0 = 0; index_0 < node.ExecutionFlows.Count; ++index_0)
                    {
                        var value_0 = node.ExecutionFlows[index_0];
                        if (value_0 != null)
                        {
                            for (int index_1 = 0; index_1 < value_0.Count; ++index_1)
                            {
                                value_0[index_1] = this.VisitNullChecked(value_0[index_1]);
                            }
                        }
                    }
                }

                if (node.RelatedLocations != null)
                {
                    for (int index_0 = 0; index_0 < node.RelatedLocations.Count; ++index_0)
                    {
                        node.RelatedLocations[index_0] = this.VisitNullChecked(node.RelatedLocations[index_0]);
                    }
                }

                if (node.Fixes != null)
                {
                    for (int index_0 = 0; index_0 < node.Fixes.Count; ++index_0)
                    {
                        node.Fixes[index_0] = this.VisitNullChecked(node.Fixes[index_0]);
                    }
                }

            }

            return node;
        }

        /// <summary>Rewrites a SarifLog node in a Sarif tree.</summary>
        /// <param name="node">A SarifLog node to visit.</param>
        public virtual SarifLog VisitSarifLog(SarifLog node)
        {
            if (node != null)
            {
                if (node.RunLogs != null)
                {
                    for (int index_0 = 0; index_0 < node.RunLogs.Count; ++index_0)
                    {
                        node.RunLogs[index_0] = this.VisitNullChecked(node.RunLogs[index_0]);
                    }
                }

            }

            return node;
        }

        /// <summary>Rewrites a RuleDescriptor node in a Sarif tree.</summary>
        /// <param name="node">A RuleDescriptor node to visit.</param>
        public virtual RuleDescriptor VisitRuleDescriptor(RuleDescriptor node)
        {
            if (node != null)
            {
            }

            return node;
        }

        /// <summary>Rewrites a RunInfo node in a Sarif tree.</summary>
        /// <param name="node">A RunInfo node to visit.</param>
        public virtual RunInfo VisitRunInfo(RunInfo node)
        {
            if (node != null)
            {
            }

            return node;
        }

        /// <summary>Rewrites a RunLog node in a Sarif tree.</summary>
        /// <param name="node">A RunLog node to visit.</param>
        public virtual RunLog VisitRunLog(RunLog node)
        {
            if (node != null)
            {
                node.ToolInfo = this.VisitNullChecked(node.ToolInfo);
                node.RunInfo = this.VisitNullChecked(node.RunInfo);
                if (node.RuleInfo != null)
                {
                    for (int index_0 = 0; index_0 < node.RuleInfo.Count; ++index_0)
                    {
                        node.RuleInfo[index_0] = this.VisitNullChecked(node.RuleInfo[index_0]);
                    }
                }

                if (node.Results != null)
                {
                    for (int index_0 = 0; index_0 < node.Results.Count; ++index_0)
                    {
                        node.Results[index_0] = this.VisitNullChecked(node.Results[index_0]);
                    }
                }

            }

            return node;
        }

        /// <summary>Rewrites a ToolInfo node in a Sarif tree.</summary>
        /// <param name="node">A ToolInfo node to visit.</param>
        public virtual ToolInfo VisitToolInfo(ToolInfo node)
        {
            if (node != null)
            {
            }

            return node;
        }
    }
}