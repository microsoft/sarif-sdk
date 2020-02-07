// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif.VersionOne
{
    /// <summary>
    /// Rewriting visitor for the Sarif object model.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.52.0.0")]
    public abstract class SarifRewritingVisitorVersionOne
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
        public virtual object Visit(ISarifNodeVersionOne node)
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
        public virtual object VisitActual(ISarifNodeVersionOne node)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }

            switch (node.SarifNodeKindVersionOne)
            {
                case SarifNodeKindVersionOne.AnnotatedCodeLocationVersionOne:
                    return VisitAnnotatedCodeLocationVersionOne((AnnotatedCodeLocationVersionOne)node);
                case SarifNodeKindVersionOne.AnnotationVersionOne:
                    return VisitAnnotationVersionOne((AnnotationVersionOne)node);
                case SarifNodeKindVersionOne.CodeFlowVersionOne:
                    return VisitCodeFlowVersionOne((CodeFlowVersionOne)node);
                case SarifNodeKindVersionOne.ExceptionDataVersionOne:
                    return VisitExceptionDataVersionOne((ExceptionDataVersionOne)node);
                case SarifNodeKindVersionOne.FileChangeVersionOne:
                    return VisitFileChangeVersionOne((FileChangeVersionOne)node);
                case SarifNodeKindVersionOne.FileDataVersionOne:
                    return VisitFileDataVersionOne((FileDataVersionOne)node);
                case SarifNodeKindVersionOne.FixVersionOne:
                    return VisitFixVersionOne((FixVersionOne)node);
                case SarifNodeKindVersionOne.FormattedRuleMessageVersionOne:
                    return VisitFormattedRuleMessageVersionOne((FormattedRuleMessageVersionOne)node);
                case SarifNodeKindVersionOne.HashVersionOne:
                    return VisitHashVersionOne((HashVersionOne)node);
                case SarifNodeKindVersionOne.InvocationVersionOne:
                    return VisitInvocationVersionOne((InvocationVersionOne)node);
                case SarifNodeKindVersionOne.LocationVersionOne:
                    return VisitLocationVersionOne((LocationVersionOne)node);
                case SarifNodeKindVersionOne.LogicalLocationVersionOne:
                    return VisitLogicalLocationVersionOne((LogicalLocationVersionOne)node);
                case SarifNodeKindVersionOne.NotificationVersionOne:
                    return VisitNotificationVersionOne((NotificationVersionOne)node);
                case SarifNodeKindVersionOne.PhysicalLocationVersionOne:
                    return VisitPhysicalLocationVersionOne((PhysicalLocationVersionOne)node);
                case SarifNodeKindVersionOne.RegionVersionOne:
                    return VisitRegionVersionOne((RegionVersionOne)node);
                case SarifNodeKindVersionOne.ReplacementVersionOne:
                    return VisitReplacementVersionOne((ReplacementVersionOne)node);
                case SarifNodeKindVersionOne.ResultVersionOne:
                    return VisitResultVersionOne((ResultVersionOne)node);
                case SarifNodeKindVersionOne.RuleVersionOne:
                    return VisitRuleVersionOne((RuleVersionOne)node);
                case SarifNodeKindVersionOne.RunVersionOne:
                    return VisitRunVersionOne((RunVersionOne)node);
                case SarifNodeKindVersionOne.SarifLogVersionOne:
                    return VisitSarifLogVersionOne((SarifLogVersionOne)node);
                case SarifNodeKindVersionOne.StackFrameVersionOne:
                    return VisitStackFrameVersionOne((StackFrameVersionOne)node);
                case SarifNodeKindVersionOne.StackVersionOne:
                    return VisitStackVersionOne((StackVersionOne)node);
                case SarifNodeKindVersionOne.ToolVersionOne:
                    return VisitToolVersionOne((ToolVersionOne)node);
                default:
                    return node;
            }
        }

        private T VisitNullChecked<T>(T node) where T : class, ISarifNodeVersionOne
        {
            if (node == null)
            {
                return null;
            }

            return (T)Visit(node);
        }

        public virtual AnnotatedCodeLocationVersionOne VisitAnnotatedCodeLocationVersionOne(AnnotatedCodeLocationVersionOne node)
        {
            if (node != null)
            {
                node.PhysicalLocation = VisitNullChecked(node.PhysicalLocation);
                if (node.Annotations != null)
                {
                    for (int index_0 = 0; index_0 < node.Annotations.Count; ++index_0)
                    {
                        node.Annotations[index_0] = VisitNullChecked(node.Annotations[index_0]);
                    }
                }
            }

            return node;
        }

        public virtual AnnotationVersionOne VisitAnnotationVersionOne(AnnotationVersionOne node)
        {
            if (node != null)
            {
                if (node.Locations != null)
                {
                    for (int index_0 = 0; index_0 < node.Locations.Count; ++index_0)
                    {
                        node.Locations[index_0] = VisitNullChecked(node.Locations[index_0]);
                    }
                }
            }

            return node;
        }

        public virtual CodeFlowVersionOne VisitCodeFlowVersionOne(CodeFlowVersionOne node)
        {
            if (node != null)
            {
                if (node.Locations != null)
                {
                    for (int index_0 = 0; index_0 < node.Locations.Count; ++index_0)
                    {
                        node.Locations[index_0] = VisitNullChecked(node.Locations[index_0]);
                    }
                }
            }

            return node;
        }

        public virtual ExceptionDataVersionOne VisitExceptionDataVersionOne(ExceptionDataVersionOne node)
        {
            if (node != null)
            {
                node.Stack = VisitNullChecked(node.Stack);
                if (node.InnerExceptions != null)
                {
                    for (int index_0 = 0; index_0 < node.InnerExceptions.Count; ++index_0)
                    {
                        node.InnerExceptions[index_0] = VisitNullChecked(node.InnerExceptions[index_0]);
                    }
                }
            }

            return node;
        }

        public virtual FileChangeVersionOne VisitFileChangeVersionOne(FileChangeVersionOne node)
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

        public virtual FileDataVersionOne VisitFileDataVersionOne(FileDataVersionOne node)
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

        public virtual FixVersionOne VisitFixVersionOne(FixVersionOne node)
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

        public virtual FormattedRuleMessageVersionOne VisitFormattedRuleMessageVersionOne(FormattedRuleMessageVersionOne node)
        {
            if (node != null)
            {
            }

            return node;
        }

        public virtual HashVersionOne VisitHashVersionOne(HashVersionOne node)
        {
            if (node != null)
            {
            }

            return node;
        }

        public virtual InvocationVersionOne VisitInvocationVersionOne(InvocationVersionOne node)
        {
            if (node != null)
            {
            }

            return node;
        }

        public virtual LocationVersionOne VisitLocationVersionOne(LocationVersionOne node)
        {
            if (node != null)
            {
                node.AnalysisTarget = VisitNullChecked(node.AnalysisTarget);
                node.ResultFile = VisitNullChecked(node.ResultFile);
            }

            return node;
        }

        public virtual LogicalLocationVersionOne VisitLogicalLocationVersionOne(LogicalLocationVersionOne node)
        {
            if (node != null)
            {
            }

            return node;
        }

        public virtual NotificationVersionOne VisitNotificationVersionOne(NotificationVersionOne node)
        {
            if (node != null)
            {
                node.PhysicalLocation = VisitNullChecked(node.PhysicalLocation);
                node.Exception = VisitNullChecked(node.Exception);
            }

            return node;
        }

        public virtual PhysicalLocationVersionOne VisitPhysicalLocationVersionOne(PhysicalLocationVersionOne node)
        {
            if (node != null)
            {
                node.Region = VisitNullChecked(node.Region);
            }

            return node;
        }

        public virtual RegionVersionOne VisitRegionVersionOne(RegionVersionOne node)
        {
            if (node != null)
            {
            }

            return node;
        }

        public virtual ReplacementVersionOne VisitReplacementVersionOne(ReplacementVersionOne node)
        {
            if (node != null)
            {
            }

            return node;
        }

        public virtual ResultVersionOne VisitResultVersionOne(ResultVersionOne node)
        {
            if (node != null)
            {
                node.FormattedRuleMessage = VisitNullChecked(node.FormattedRuleMessage);
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
                        node.CodeFlows[index_0] = VisitNullChecked(node.CodeFlows[index_0]);
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

        public virtual RuleVersionOne VisitRuleVersionOne(RuleVersionOne node)
        {
            if (node != null)
            {
            }

            return node;
        }

        public virtual RunVersionOne VisitRunVersionOne(RunVersionOne node)
        {
            if (node != null)
            {
                node.Tool = VisitNullChecked(node.Tool);
                node.Invocation = VisitNullChecked(node.Invocation);
                if (node.Files != null)
                {
                    var keys = node.Files.Keys.ToArray();
                    foreach (var key in keys)
                    {
                        var value = node.Files[key];
                        if (value != null)
                        {
                            node.Files[key] = VisitNullChecked(value);
                        }
                    }
                }

                if (node.LogicalLocations != null)
                {
                    var keys = node.LogicalLocations.Keys.ToArray();
                    foreach (var key in keys)
                    {
                        var value = node.LogicalLocations[key];
                        if (value != null)
                        {
                            node.LogicalLocations[key] = VisitNullChecked(value);
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

                if (node.ToolNotifications != null)
                {
                    for (int index_0 = 0; index_0 < node.ToolNotifications.Count; ++index_0)
                    {
                        node.ToolNotifications[index_0] = VisitNullChecked(node.ToolNotifications[index_0]);
                    }
                }

                if (node.ConfigurationNotifications != null)
                {
                    for (int index_0 = 0; index_0 < node.ConfigurationNotifications.Count; ++index_0)
                    {
                        node.ConfigurationNotifications[index_0] = VisitNullChecked(node.ConfigurationNotifications[index_0]);
                    }
                }

                if (node.Rules != null)
                {
                    var keys = node.Rules.Keys.ToArray();
                    foreach (var key in keys)
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

        public virtual SarifLogVersionOne VisitSarifLogVersionOne(SarifLogVersionOne node)
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

        public virtual StackFrameVersionOne VisitStackFrameVersionOne(StackFrameVersionOne node)
        {
            if (node != null)
            {
            }

            return node;
        }

        public virtual StackVersionOne VisitStackVersionOne(StackVersionOne node)
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

        public virtual ToolVersionOne VisitToolVersionOne(ToolVersionOne node)
        {
            if (node != null)
            {
            }

            return node;
        }
    }
}