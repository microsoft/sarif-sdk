// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Rewriting visitor for the Sarif object model.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.58.0.0")]
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
                case SarifNodeKind.Attachment:
                    return VisitAttachment((Attachment)node);
                case SarifNodeKind.CodeFlow:
                    return VisitCodeFlow((CodeFlow)node);
                case SarifNodeKind.Conversion:
                    return VisitConversion((Conversion)node);
                case SarifNodeKind.Edge:
                    return VisitEdge((Edge)node);
                case SarifNodeKind.EdgeTraversal:
                    return VisitEdgeTraversal((EdgeTraversal)node);
                case SarifNodeKind.ExceptionData:
                    return VisitExceptionData((ExceptionData)node);
                case SarifNodeKind.ExternalFiles:
                    return VisitExternalFiles((ExternalFiles)node);
                case SarifNodeKind.FileChange:
                    return VisitFileChange((FileChange)node);
                case SarifNodeKind.FileContent:
                    return VisitFileContent((FileContent)node);
                case SarifNodeKind.FileData:
                    return VisitFileData((FileData)node);
                case SarifNodeKind.FileLocation:
                    return VisitFileLocation((FileLocation)node);
                case SarifNodeKind.Fix:
                    return VisitFix((Fix)node);
                case SarifNodeKind.Graph:
                    return VisitGraph((Graph)node);
                case SarifNodeKind.GraphTraversal:
                    return VisitGraphTraversal((GraphTraversal)node);
                case SarifNodeKind.Hash:
                    return VisitHash((Hash)node);
                case SarifNodeKind.Invocation:
                    return VisitInvocation((Invocation)node);
                case SarifNodeKind.Location:
                    return VisitLocation((Location)node);
                case SarifNodeKind.LogicalLocation:
                    return VisitLogicalLocation((LogicalLocation)node);
                case SarifNodeKind.Message:
                    return VisitMessage((Message)node);
                case SarifNodeKind.Node:
                    return VisitNode((Node)node);
                case SarifNodeKind.Notification:
                    return VisitNotification((Notification)node);
                case SarifNodeKind.PhysicalLocation:
                    return VisitPhysicalLocation((PhysicalLocation)node);
                case SarifNodeKind.Rectangle:
                    return VisitRectangle((Rectangle)node);
                case SarifNodeKind.Region:
                    return VisitRegion((Region)node);
                case SarifNodeKind.Replacement:
                    return VisitReplacement((Replacement)node);
                case SarifNodeKind.Resources:
                    return VisitResources((Resources)node);
                case SarifNodeKind.Result:
                    return VisitResult((Result)node);
                case SarifNodeKind.Rule:
                    return VisitRule((Rule)node);
                case SarifNodeKind.RuleConfiguration:
                    return VisitRuleConfiguration((RuleConfiguration)node);
                case SarifNodeKind.Run:
                    return VisitRun((Run)node);
                case SarifNodeKind.SarifLog:
                    return VisitSarifLog((SarifLog)node);
                case SarifNodeKind.Stack:
                    return VisitStack((Stack)node);
                case SarifNodeKind.StackFrame:
                    return VisitStackFrame((StackFrame)node);
                case SarifNodeKind.ThreadFlow:
                    return VisitThreadFlow((ThreadFlow)node);
                case SarifNodeKind.ThreadFlowLocation:
                    return VisitThreadFlowLocation((ThreadFlowLocation)node);
                case SarifNodeKind.Tool:
                    return VisitTool((Tool)node);
                case SarifNodeKind.VersionControlDetails:
                    return VisitVersionControlDetails((VersionControlDetails)node);
                default:
                    return node;
            }
        }


        private T VisitNullChecked<T>(T node) where T : class, ISarifNode
        {
            string emptyKey = null;
            return VisitNullChecked<T>(node, ref emptyKey);
        }


        private T VisitNullChecked<T>(T node, ref string key) where T : class, ISarifNode
        {
            if (node == null)
            {
                return null;
            }

            if (key == null)
            {
                return (T)Visit(node);
            }

            return (T)VisitDictionaryEntry(node, ref key);
        }

        private ISarifNode VisitDictionaryEntry(ISarifNode node, ref string key)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            switch (node.SarifNodeKind)
            {
                case SarifNodeKind.FileData:
                    return VisitFileDataDictionaryEntry((FileData)node, ref key);

                // add other dictionary things

                default:
                    throw new InvalidOperationException(); // whoops! unknown type
            }
        }

        public virtual FileData VisitFileDataDictionaryEntry(FileData node, ref string key)
        {
            return (FileData)Visit(node);
        }

        public virtual Attachment VisitAttachment(Attachment node)
        {
            if (node != null)
            {
                node.Description = VisitNullChecked(node.Description);
                node.FileLocation = VisitNullChecked(node.FileLocation);
                if (node.Regions != null)
                {
                    for (int index_0 = 0; index_0 < node.Regions.Count; ++index_0)
                    {
                        node.Regions[index_0] = VisitNullChecked(node.Regions[index_0]);
                    }
                }

                if (node.Rectangles != null)
                {
                    for (int index_0 = 0; index_0 < node.Rectangles.Count; ++index_0)
                    {
                        node.Rectangles[index_0] = VisitNullChecked(node.Rectangles[index_0]);
                    }
                }
            }

            return node;
        }

        public virtual CodeFlow VisitCodeFlow(CodeFlow node)
        {
            if (node != null)
            {
                node.Message = VisitNullChecked(node.Message);
                if (node.ThreadFlows != null)
                {
                    for (int index_0 = 0; index_0 < node.ThreadFlows.Count; ++index_0)
                    {
                        node.ThreadFlows[index_0] = VisitNullChecked(node.ThreadFlows[index_0]);
                    }
                }
            }

            return node;
        }

        public virtual Conversion VisitConversion(Conversion node)
        {
            if (node != null)
            {
                node.Tool = VisitNullChecked(node.Tool);
                node.Invocation = VisitNullChecked(node.Invocation);
                if (node.AnalysisToolLogFiles != null)
                {
                    for (int index_0 = 0; index_0 < node.AnalysisToolLogFiles.Count; ++index_0)
                    {
                        node.AnalysisToolLogFiles[index_0] = VisitNullChecked(node.AnalysisToolLogFiles[index_0]);
                    }
                }
            }

            return node;
        }

        public virtual Edge VisitEdge(Edge node)
        {
            if (node != null)
            {
                node.Label = VisitNullChecked(node.Label);
            }

            return node;
        }

        public virtual EdgeTraversal VisitEdgeTraversal(EdgeTraversal node)
        {
            if (node != null)
            {
                node.Message = VisitNullChecked(node.Message);
            }

            return node;
        }

        public virtual ExceptionData VisitExceptionData(ExceptionData node)
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

        public virtual ExternalFiles VisitExternalFiles(ExternalFiles node)
        {
            if (node != null)
            {
                node.Conversion = VisitNullChecked(node.Conversion);
                node.Files = VisitNullChecked(node.Files);
                node.Graphs = VisitNullChecked(node.Graphs);
                if (node.Invocations != null)
                {
                    for (int index_0 = 0; index_0 < node.Invocations.Count; ++index_0)
                    {
                        node.Invocations[index_0] = VisitNullChecked(node.Invocations[index_0]);
                    }
                }

                node.LogicalLocations = VisitNullChecked(node.LogicalLocations);
                node.Resources = VisitNullChecked(node.Resources);
                if (node.Results != null)
                {
                    for (int index_0 = 0; index_0 < node.Results.Count; ++index_0)
                    {
                        node.Results[index_0] = VisitNullChecked(node.Results[index_0]);
                    }
                }
            }

            return node;
        }

        public virtual FileChange VisitFileChange(FileChange node)
        {
            if (node != null)
            {
                node.FileLocation = VisitNullChecked(node.FileLocation);
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

        public virtual FileContent VisitFileContent(FileContent node)
        {
            if (node != null)
            {
            }

            return node;
        }

        public virtual FileData VisitFileData(FileData node)
        {
            if (node != null)
            {
                node.FileLocation = VisitNullChecked(node.FileLocation);
                node.Contents = VisitNullChecked(node.Contents);
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

        public virtual FileLocation VisitFileLocation(FileLocation node)
        {
            if (node != null)
            {
            }

            return node;
        }

        public virtual Fix VisitFix(Fix node)
        {
            if (node != null)
            {
                node.Description = VisitNullChecked(node.Description);
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

        public virtual Graph VisitGraph(Graph node)
        {
            if (node != null)
            {
                node.Description = VisitNullChecked(node.Description);
                if (node.Nodes != null)
                {
                    for (int index_0 = 0; index_0 < node.Nodes.Count; ++index_0)
                    {
                        node.Nodes[index_0] = VisitNullChecked(node.Nodes[index_0]);
                    }
                }

                if (node.Edges != null)
                {
                    for (int index_0 = 0; index_0 < node.Edges.Count; ++index_0)
                    {
                        node.Edges[index_0] = VisitNullChecked(node.Edges[index_0]);
                    }
                }
            }

            return node;
        }

        public virtual GraphTraversal VisitGraphTraversal(GraphTraversal node)
        {
            if (node != null)
            {
                node.Description = VisitNullChecked(node.Description);
                if (node.EdgeTraversals != null)
                {
                    for (int index_0 = 0; index_0 < node.EdgeTraversals.Count; ++index_0)
                    {
                        node.EdgeTraversals[index_0] = VisitNullChecked(node.EdgeTraversals[index_0]);
                    }
                }
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

        public virtual Invocation VisitInvocation(Invocation node)
        {
            if (node != null)
            {
                if (node.ResponseFiles != null)
                {
                    for (int index_0 = 0; index_0 < node.ResponseFiles.Count; ++index_0)
                    {
                        node.ResponseFiles[index_0] = VisitNullChecked(node.ResponseFiles[index_0]);
                    }
                }

                if (node.Attachments != null)
                {
                    for (int index_0 = 0; index_0 < node.Attachments.Count; ++index_0)
                    {
                        node.Attachments[index_0] = VisitNullChecked(node.Attachments[index_0]);
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

                node.ExecutableLocation = VisitNullChecked(node.ExecutableLocation);
                node.WorkingDirectory = VisitNullChecked(node.WorkingDirectory);
                node.Stdin = VisitNullChecked(node.Stdin);
                node.Stdout = VisitNullChecked(node.Stdout);
                node.Stderr = VisitNullChecked(node.Stderr);
                node.StdoutStderr = VisitNullChecked(node.StdoutStderr);
            }

            return node;
        }

        public virtual Location VisitLocation(Location node)
        {
            if (node != null)
            {
                node.PhysicalLocation = VisitNullChecked(node.PhysicalLocation);
                node.Message = VisitNullChecked(node.Message);
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

        public virtual LogicalLocation VisitLogicalLocation(LogicalLocation node)
        {
            if (node != null)
            {
            }

            return node;
        }

        public virtual Message VisitMessage(Message node)
        {
            if (node != null)
            {
            }

            return node;
        }

        public virtual Node VisitNode(Node node)
        {
            if (node != null)
            {
                node.Label = VisitNullChecked(node.Label);
                node.Location = VisitNullChecked(node.Location);
                if (node.Children != null)
                {
                    for (int index_0 = 0; index_0 < node.Children.Count; ++index_0)
                    {
                        node.Children[index_0] = VisitNullChecked(node.Children[index_0]);
                    }
                }
            }

            return node;
        }

        public virtual Notification VisitNotification(Notification node)
        {
            if (node != null)
            {
                node.PhysicalLocation = VisitNullChecked(node.PhysicalLocation);
                node.Message = VisitNullChecked(node.Message);
                node.Exception = VisitNullChecked(node.Exception);
            }

            return node;
        }

        public virtual PhysicalLocation VisitPhysicalLocation(PhysicalLocation node)
        {
            if (node != null)
            {
                node.FileLocation = VisitNullChecked(node.FileLocation);
                node.Region = VisitNullChecked(node.Region);
                node.ContextRegion = VisitNullChecked(node.ContextRegion);
            }

            return node;
        }

        public virtual Rectangle VisitRectangle(Rectangle node)
        {
            if (node != null)
            {
                node.Message = VisitNullChecked(node.Message);
            }

            return node;
        }

        public virtual Region VisitRegion(Region node)
        {
            if (node != null)
            {
                node.Snippet = VisitNullChecked(node.Snippet);
                node.Message = VisitNullChecked(node.Message);
            }

            return node;
        }

        public virtual Replacement VisitReplacement(Replacement node)
        {
            if (node != null)
            {
                node.DeletedRegion = VisitNullChecked(node.DeletedRegion);
                node.InsertedContent = VisitNullChecked(node.InsertedContent);
            }

            return node;
        }

        public virtual Resources VisitResources(Resources node)
        {
            if (node != null)
            {
                if (node.Rules != null)
                {
                    var keys = node.Rules.Keys.ToArray();
                    foreach (var key in keys)
                    {
                        var value = node.Rules[key];
                        if (value != null)
                        {
                            ISarifNode sarifNode = value as ISarifNode;

                            if (sarifNode == null)
                            {
                                throw new InvalidOperationException("Attempted to visit an IRule instance that does not implement ISarifNode");
                            }

                            node.Rules[key] = (IRule)Visit(sarifNode);
                        }
                    }
                }
            }

            return node;
        }

        public virtual Result VisitResult(Result node)
        {
            if (node != null)
            {
                node.Message = VisitNullChecked(node.Message);
                node.AnalysisTarget = VisitNullChecked(node.AnalysisTarget);
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

                if (node.Graphs != null)
                {
                    var keys = node.Graphs.Keys.ToArray();
                    foreach (var key in keys)
                    {
                        var value = node.Graphs[key];
                        if (value != null)
                        {
                            node.Graphs[key] = VisitNullChecked(value);
                        }
                    }
                }

                if (node.GraphTraversals != null)
                {
                    for (int index_0 = 0; index_0 < node.GraphTraversals.Count; ++index_0)
                    {
                        node.GraphTraversals[index_0] = VisitNullChecked(node.GraphTraversals[index_0]);
                    }
                }

                if (node.RelatedLocations != null)
                {
                    for (int index_0 = 0; index_0 < node.RelatedLocations.Count; ++index_0)
                    {
                        node.RelatedLocations[index_0] = VisitNullChecked(node.RelatedLocations[index_0]);
                    }
                }

                if (node.Attachments != null)
                {
                    for (int index_0 = 0; index_0 < node.Attachments.Count; ++index_0)
                    {
                        node.Attachments[index_0] = VisitNullChecked(node.Attachments[index_0]);
                    }
                }

                if (node.ConversionProvenance != null)
                {
                    for (int index_0 = 0; index_0 < node.ConversionProvenance.Count; ++index_0)
                    {
                        node.ConversionProvenance[index_0] = VisitNullChecked(node.ConversionProvenance[index_0]);
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
                node.Name = VisitNullChecked(node.Name);
                node.ShortDescription = VisitNullChecked(node.ShortDescription);
                node.FullDescription = VisitNullChecked(node.FullDescription);
                node.Configuration = VisitNullChecked(node.Configuration);
                node.Help = VisitNullChecked(node.Help);
            }

            return node;
        }

        public virtual RuleConfiguration VisitRuleConfiguration(RuleConfiguration node)
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
                if (node.Invocations != null)
                {
                    for (int index_0 = 0; index_0 < node.Invocations.Count; ++index_0)
                    {
                        node.Invocations[index_0] = VisitNullChecked(node.Invocations[index_0]);
                    }
                }

                node.Conversion = VisitNullChecked(node.Conversion);
                if (node.VersionControlProvenance != null)
                {
                    for (int index_0 = 0; index_0 < node.VersionControlProvenance.Count; ++index_0)
                    {
                        node.VersionControlProvenance[index_0] = VisitNullChecked(node.VersionControlProvenance[index_0]);
                    }
                }

                if (node.Files != null)
                {
                    var keys = node.Files.Keys.ToArray();
                    foreach (var key in keys)
                    {
                        var value = node.Files[key];
                        if (value != null)
                        {
                            string newKey = key;
                            node.Files.Remove(key);
                            value = VisitNullChecked(value, ref newKey);

                            if (newKey != null)
                            {
                                node.Files[newKey] = value;
                            }
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

                if (node.Graphs != null)
                {
                    var keys = node.Graphs.Keys.ToArray();
                    foreach (var key in keys)
                    {
                        var value = node.Graphs[key];
                        if (value != null)
                        {
                            node.Graphs[key] = VisitNullChecked(value);
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

                node.Resources = VisitNullChecked(node.Resources);
                node.Description = VisitNullChecked(node.Description);
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
                node.Message = VisitNullChecked(node.Message);
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

        public virtual ThreadFlow VisitThreadFlow(ThreadFlow node)
        {
            if (node != null)
            {
                node.Message = VisitNullChecked(node.Message);
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

        public virtual ThreadFlowLocation VisitThreadFlowLocation(ThreadFlowLocation node)
        {
            if (node != null)
            {
                node.Location = VisitNullChecked(node.Location);
                node.Stack = VisitNullChecked(node.Stack);
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

        public virtual VersionControlDetails VisitVersionControlDetails(VersionControlDetails node)
        {
            if (node != null)
            {
            }

            return node;
        }
    }
}