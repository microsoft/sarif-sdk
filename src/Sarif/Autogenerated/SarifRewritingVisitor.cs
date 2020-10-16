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
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.0.0")]
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
                case SarifNodeKind.Address:
                    return VisitAddress((Address)node);
                case SarifNodeKind.Artifact:
                    return VisitArtifact((Artifact)node);
                case SarifNodeKind.ArtifactChange:
                    return VisitArtifactChange((ArtifactChange)node);
                case SarifNodeKind.ArtifactContent:
                    return VisitArtifactContent((ArtifactContent)node);
                case SarifNodeKind.ArtifactLocation:
                    return VisitArtifactLocation((ArtifactLocation)node);
                case SarifNodeKind.Attachment:
                    return VisitAttachment((Attachment)node);
                case SarifNodeKind.CodeFlow:
                    return VisitCodeFlow((CodeFlow)node);
                case SarifNodeKind.ConfigurationOverride:
                    return VisitConfigurationOverride((ConfigurationOverride)node);
                case SarifNodeKind.Conversion:
                    return VisitConversion((Conversion)node);
                case SarifNodeKind.Edge:
                    return VisitEdge((Edge)node);
                case SarifNodeKind.EdgeTraversal:
                    return VisitEdgeTraversal((EdgeTraversal)node);
                case SarifNodeKind.ExceptionData:
                    return VisitExceptionData((ExceptionData)node);
                case SarifNodeKind.ExternalProperties:
                    return VisitExternalProperties((ExternalProperties)node);
                case SarifNodeKind.ExternalPropertyFileReference:
                    return VisitExternalPropertyFileReference((ExternalPropertyFileReference)node);
                case SarifNodeKind.ExternalPropertyFileReferences:
                    return VisitExternalPropertyFileReferences((ExternalPropertyFileReferences)node);
                case SarifNodeKind.Fix:
                    return VisitFix((Fix)node);
                case SarifNodeKind.Graph:
                    return VisitGraph((Graph)node);
                case SarifNodeKind.GraphTraversal:
                    return VisitGraphTraversal((GraphTraversal)node);
                case SarifNodeKind.Invocation:
                    return VisitInvocation((Invocation)node);
                case SarifNodeKind.Location:
                    return VisitLocation((Location)node);
                case SarifNodeKind.LocationRelationship:
                    return VisitLocationRelationship((LocationRelationship)node);
                case SarifNodeKind.LogicalLocation:
                    return VisitLogicalLocation((LogicalLocation)node);
                case SarifNodeKind.Message:
                    return VisitMessage((Message)node);
                case SarifNodeKind.MultiformatMessageString:
                    return VisitMultiformatMessageString((MultiformatMessageString)node);
                case SarifNodeKind.Node:
                    return VisitNode((Node)node);
                case SarifNodeKind.Notification:
                    return VisitNotification((Notification)node);
                case SarifNodeKind.PhysicalLocation:
                    return VisitPhysicalLocation((PhysicalLocation)node);
                case SarifNodeKind.PropertyBag:
                    return VisitPropertyBag((PropertyBag)node);
                case SarifNodeKind.Rectangle:
                    return VisitRectangle((Rectangle)node);
                case SarifNodeKind.Region:
                    return VisitRegion((Region)node);
                case SarifNodeKind.Replacement:
                    return VisitReplacement((Replacement)node);
                case SarifNodeKind.ReportingConfiguration:
                    return VisitReportingConfiguration((ReportingConfiguration)node);
                case SarifNodeKind.ReportingDescriptor:
                    return VisitReportingDescriptor((ReportingDescriptor)node);
                case SarifNodeKind.ReportingDescriptorReference:
                    return VisitReportingDescriptorReference((ReportingDescriptorReference)node);
                case SarifNodeKind.ReportingDescriptorRelationship:
                    return VisitReportingDescriptorRelationship((ReportingDescriptorRelationship)node);
                case SarifNodeKind.Result:
                    return VisitResult((Result)node);
                case SarifNodeKind.ResultProvenance:
                    return VisitResultProvenance((ResultProvenance)node);
                case SarifNodeKind.Run:
                    return VisitRun((Run)node);
                case SarifNodeKind.RunAutomationDetails:
                    return VisitRunAutomationDetails((RunAutomationDetails)node);
                case SarifNodeKind.SarifLog:
                    return VisitSarifLog((SarifLog)node);
                case SarifNodeKind.SpecialLocations:
                    return VisitSpecialLocations((SpecialLocations)node);
                case SarifNodeKind.Stack:
                    return VisitStack((Stack)node);
                case SarifNodeKind.StackFrame:
                    return VisitStackFrame((StackFrame)node);
                case SarifNodeKind.Suppression:
                    return VisitSuppression((Suppression)node);
                case SarifNodeKind.ThreadFlow:
                    return VisitThreadFlow((ThreadFlow)node);
                case SarifNodeKind.ThreadFlowLocation:
                    return VisitThreadFlowLocation((ThreadFlowLocation)node);
                case SarifNodeKind.Tool:
                    return VisitTool((Tool)node);
                case SarifNodeKind.ToolComponent:
                    return VisitToolComponent((ToolComponent)node);
                case SarifNodeKind.ToolComponentReference:
                    return VisitToolComponentReference((ToolComponentReference)node);
                case SarifNodeKind.TranslationMetadata:
                    return VisitTranslationMetadata((TranslationMetadata)node);
                case SarifNodeKind.VersionControlDetails:
                    return VisitVersionControlDetails((VersionControlDetails)node);
                case SarifNodeKind.WebRequest:
                    return VisitWebRequest((WebRequest)node);
                case SarifNodeKind.WebResponse:
                    return VisitWebResponse((WebResponse)node);
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

        public virtual Address VisitAddress(Address node)
        {
            if (node != null)
            {
            }

            return node;
        }

        public virtual Artifact VisitArtifact(Artifact node)
        {
            if (node != null)
            {
                node.Description = VisitNullChecked(node.Description);
                node.Location = VisitNullChecked(node.Location);
                node.Contents = VisitNullChecked(node.Contents);
            }

            return node;
        }

        public virtual ArtifactChange VisitArtifactChange(ArtifactChange node)
        {
            if (node != null)
            {
                node.ArtifactLocation = VisitNullChecked(node.ArtifactLocation);
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

        public virtual ArtifactContent VisitArtifactContent(ArtifactContent node)
        {
            if (node != null)
            {
                node.Rendered = VisitNullChecked(node.Rendered);
            }

            return node;
        }

        public virtual ArtifactLocation VisitArtifactLocation(ArtifactLocation node)
        {
            if (node != null)
            {
                node.Description = VisitNullChecked(node.Description);
            }

            return node;
        }

        public virtual Attachment VisitAttachment(Attachment node)
        {
            if (node != null)
            {
                node.Description = VisitNullChecked(node.Description);
                node.ArtifactLocation = VisitNullChecked(node.ArtifactLocation);
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

        public virtual ConfigurationOverride VisitConfigurationOverride(ConfigurationOverride node)
        {
            if (node != null)
            {
                node.Configuration = VisitNullChecked(node.Configuration);
                node.Descriptor = VisitNullChecked(node.Descriptor);
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
                if (node.FinalState != null)
                {
                    var keys = node.FinalState.Keys.ToArray();
                    foreach (var key in keys)
                    {
                        var value = node.FinalState[key];
                        if (value != null)
                        {
                            node.FinalState[key] = VisitNullChecked(value);
                        }
                    }
                }
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

        public virtual ExternalProperties VisitExternalProperties(ExternalProperties node)
        {
            if (node != null)
            {
                node.Conversion = VisitNullChecked(node.Conversion);
                if (node.Graphs != null)
                {
                    for (int index_0 = 0; index_0 < node.Graphs.Count; ++index_0)
                    {
                        node.Graphs[index_0] = VisitNullChecked(node.Graphs[index_0]);
                    }
                }

                node.ExternalizedProperties = VisitNullChecked(node.ExternalizedProperties);
                if (node.Artifacts != null)
                {
                    for (int index_0 = 0; index_0 < node.Artifacts.Count; ++index_0)
                    {
                        node.Artifacts[index_0] = VisitNullChecked(node.Artifacts[index_0]);
                    }
                }

                if (node.Invocations != null)
                {
                    for (int index_0 = 0; index_0 < node.Invocations.Count; ++index_0)
                    {
                        node.Invocations[index_0] = VisitNullChecked(node.Invocations[index_0]);
                    }
                }

                if (node.LogicalLocations != null)
                {
                    for (int index_0 = 0; index_0 < node.LogicalLocations.Count; ++index_0)
                    {
                        node.LogicalLocations[index_0] = VisitNullChecked(node.LogicalLocations[index_0]);
                    }
                }

                if (node.ThreadFlowLocations != null)
                {
                    for (int index_0 = 0; index_0 < node.ThreadFlowLocations.Count; ++index_0)
                    {
                        node.ThreadFlowLocations[index_0] = VisitNullChecked(node.ThreadFlowLocations[index_0]);
                    }
                }

                if (node.Results != null)
                {
                    for (int index_0 = 0; index_0 < node.Results.Count; ++index_0)
                    {
                        node.Results[index_0] = VisitNullChecked(node.Results[index_0]);
                    }
                }

                if (node.Taxonomies != null)
                {
                    for (int index_0 = 0; index_0 < node.Taxonomies.Count; ++index_0)
                    {
                        node.Taxonomies[index_0] = VisitNullChecked(node.Taxonomies[index_0]);
                    }
                }

                node.Driver = VisitNullChecked(node.Driver);
                if (node.Extensions != null)
                {
                    for (int index_0 = 0; index_0 < node.Extensions.Count; ++index_0)
                    {
                        node.Extensions[index_0] = VisitNullChecked(node.Extensions[index_0]);
                    }
                }

                if (node.Policies != null)
                {
                    for (int index_0 = 0; index_0 < node.Policies.Count; ++index_0)
                    {
                        node.Policies[index_0] = VisitNullChecked(node.Policies[index_0]);
                    }
                }

                if (node.Translations != null)
                {
                    for (int index_0 = 0; index_0 < node.Translations.Count; ++index_0)
                    {
                        node.Translations[index_0] = VisitNullChecked(node.Translations[index_0]);
                    }
                }

                if (node.Addresses != null)
                {
                    for (int index_0 = 0; index_0 < node.Addresses.Count; ++index_0)
                    {
                        node.Addresses[index_0] = VisitNullChecked(node.Addresses[index_0]);
                    }
                }

                if (node.WebRequests != null)
                {
                    for (int index_0 = 0; index_0 < node.WebRequests.Count; ++index_0)
                    {
                        node.WebRequests[index_0] = VisitNullChecked(node.WebRequests[index_0]);
                    }
                }

                if (node.WebResponses != null)
                {
                    for (int index_0 = 0; index_0 < node.WebResponses.Count; ++index_0)
                    {
                        node.WebResponses[index_0] = VisitNullChecked(node.WebResponses[index_0]);
                    }
                }
            }

            return node;
        }

        public virtual ExternalPropertyFileReference VisitExternalPropertyFileReference(ExternalPropertyFileReference node)
        {
            if (node != null)
            {
                node.Location = VisitNullChecked(node.Location);
            }

            return node;
        }

        public virtual ExternalPropertyFileReferences VisitExternalPropertyFileReferences(ExternalPropertyFileReferences node)
        {
            if (node != null)
            {
                node.Conversion = VisitNullChecked(node.Conversion);
                if (node.Graphs != null)
                {
                    for (int index_0 = 0; index_0 < node.Graphs.Count; ++index_0)
                    {
                        node.Graphs[index_0] = VisitNullChecked(node.Graphs[index_0]);
                    }
                }

                node.ExternalizedProperties = VisitNullChecked(node.ExternalizedProperties);
                if (node.Artifacts != null)
                {
                    for (int index_0 = 0; index_0 < node.Artifacts.Count; ++index_0)
                    {
                        node.Artifacts[index_0] = VisitNullChecked(node.Artifacts[index_0]);
                    }
                }

                if (node.Invocations != null)
                {
                    for (int index_0 = 0; index_0 < node.Invocations.Count; ++index_0)
                    {
                        node.Invocations[index_0] = VisitNullChecked(node.Invocations[index_0]);
                    }
                }

                if (node.LogicalLocations != null)
                {
                    for (int index_0 = 0; index_0 < node.LogicalLocations.Count; ++index_0)
                    {
                        node.LogicalLocations[index_0] = VisitNullChecked(node.LogicalLocations[index_0]);
                    }
                }

                if (node.ThreadFlowLocations != null)
                {
                    for (int index_0 = 0; index_0 < node.ThreadFlowLocations.Count; ++index_0)
                    {
                        node.ThreadFlowLocations[index_0] = VisitNullChecked(node.ThreadFlowLocations[index_0]);
                    }
                }

                if (node.Results != null)
                {
                    for (int index_0 = 0; index_0 < node.Results.Count; ++index_0)
                    {
                        node.Results[index_0] = VisitNullChecked(node.Results[index_0]);
                    }
                }

                if (node.Taxonomies != null)
                {
                    for (int index_0 = 0; index_0 < node.Taxonomies.Count; ++index_0)
                    {
                        node.Taxonomies[index_0] = VisitNullChecked(node.Taxonomies[index_0]);
                    }
                }

                if (node.Addresses != null)
                {
                    for (int index_0 = 0; index_0 < node.Addresses.Count; ++index_0)
                    {
                        node.Addresses[index_0] = VisitNullChecked(node.Addresses[index_0]);
                    }
                }

                node.Driver = VisitNullChecked(node.Driver);
                if (node.Extensions != null)
                {
                    for (int index_0 = 0; index_0 < node.Extensions.Count; ++index_0)
                    {
                        node.Extensions[index_0] = VisitNullChecked(node.Extensions[index_0]);
                    }
                }

                if (node.Policies != null)
                {
                    for (int index_0 = 0; index_0 < node.Policies.Count; ++index_0)
                    {
                        node.Policies[index_0] = VisitNullChecked(node.Policies[index_0]);
                    }
                }

                if (node.Translations != null)
                {
                    for (int index_0 = 0; index_0 < node.Translations.Count; ++index_0)
                    {
                        node.Translations[index_0] = VisitNullChecked(node.Translations[index_0]);
                    }
                }

                if (node.WebRequests != null)
                {
                    for (int index_0 = 0; index_0 < node.WebRequests.Count; ++index_0)
                    {
                        node.WebRequests[index_0] = VisitNullChecked(node.WebRequests[index_0]);
                    }
                }

                if (node.WebResponses != null)
                {
                    for (int index_0 = 0; index_0 < node.WebResponses.Count; ++index_0)
                    {
                        node.WebResponses[index_0] = VisitNullChecked(node.WebResponses[index_0]);
                    }
                }
            }

            return node;
        }

        public virtual Fix VisitFix(Fix node)
        {
            if (node != null)
            {
                node.Description = VisitNullChecked(node.Description);
                if (node.ArtifactChanges != null)
                {
                    for (int index_0 = 0; index_0 < node.ArtifactChanges.Count; ++index_0)
                    {
                        node.ArtifactChanges[index_0] = VisitNullChecked(node.ArtifactChanges[index_0]);
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
                if (node.InitialState != null)
                {
                    var keys = node.InitialState.Keys.ToArray();
                    foreach (var key in keys)
                    {
                        var value = node.InitialState[key];
                        if (value != null)
                        {
                            node.InitialState[key] = VisitNullChecked(value);
                        }
                    }
                }

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

                if (node.RuleConfigurationOverrides != null)
                {
                    for (int index_0 = 0; index_0 < node.RuleConfigurationOverrides.Count; ++index_0)
                    {
                        node.RuleConfigurationOverrides[index_0] = VisitNullChecked(node.RuleConfigurationOverrides[index_0]);
                    }
                }

                if (node.NotificationConfigurationOverrides != null)
                {
                    for (int index_0 = 0; index_0 < node.NotificationConfigurationOverrides.Count; ++index_0)
                    {
                        node.NotificationConfigurationOverrides[index_0] = VisitNullChecked(node.NotificationConfigurationOverrides[index_0]);
                    }
                }

                if (node.ToolExecutionNotifications != null)
                {
                    for (int index_0 = 0; index_0 < node.ToolExecutionNotifications.Count; ++index_0)
                    {
                        node.ToolExecutionNotifications[index_0] = VisitNullChecked(node.ToolExecutionNotifications[index_0]);
                    }
                }

                if (node.ToolConfigurationNotifications != null)
                {
                    for (int index_0 = 0; index_0 < node.ToolConfigurationNotifications.Count; ++index_0)
                    {
                        node.ToolConfigurationNotifications[index_0] = VisitNullChecked(node.ToolConfigurationNotifications[index_0]);
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
                if (node.LogicalLocations != null)
                {
                    for (int index_0 = 0; index_0 < node.LogicalLocations.Count; ++index_0)
                    {
                        node.LogicalLocations[index_0] = VisitNullChecked(node.LogicalLocations[index_0]);
                    }
                }

                node.Message = VisitNullChecked(node.Message);
                if (node.Annotations != null)
                {
                    for (int index_0 = 0; index_0 < node.Annotations.Count; ++index_0)
                    {
                        node.Annotations[index_0] = VisitNullChecked(node.Annotations[index_0]);
                    }
                }

                if (node.Relationships != null)
                {
                    for (int index_0 = 0; index_0 < node.Relationships.Count; ++index_0)
                    {
                        node.Relationships[index_0] = VisitNullChecked(node.Relationships[index_0]);
                    }
                }
            }

            return node;
        }

        public virtual LocationRelationship VisitLocationRelationship(LocationRelationship node)
        {
            if (node != null)
            {
                node.Description = VisitNullChecked(node.Description);
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

        public virtual MultiformatMessageString VisitMultiformatMessageString(MultiformatMessageString node)
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
                if (node.Locations != null)
                {
                    for (int index_0 = 0; index_0 < node.Locations.Count; ++index_0)
                    {
                        node.Locations[index_0] = VisitNullChecked(node.Locations[index_0]);
                    }
                }

                node.Message = VisitNullChecked(node.Message);
                node.Exception = VisitNullChecked(node.Exception);
                node.Descriptor = VisitNullChecked(node.Descriptor);
                node.AssociatedRule = VisitNullChecked(node.AssociatedRule);
            }

            return node;
        }

        public virtual PhysicalLocation VisitPhysicalLocation(PhysicalLocation node)
        {
            if (node != null)
            {
                node.Address = VisitNullChecked(node.Address);
                node.ArtifactLocation = VisitNullChecked(node.ArtifactLocation);
                node.Region = VisitNullChecked(node.Region);
                node.ContextRegion = VisitNullChecked(node.ContextRegion);
            }

            return node;
        }

        public virtual PropertyBag VisitPropertyBag(PropertyBag node)
        {
            if (node != null)
            {
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

        public virtual ReportingConfiguration VisitReportingConfiguration(ReportingConfiguration node)
        {
            if (node != null)
            {
            }

            return node;
        }

        public virtual ReportingDescriptor VisitReportingDescriptor(ReportingDescriptor node)
        {
            if (node != null)
            {
                node.ShortDescription = VisitNullChecked(node.ShortDescription);
                node.FullDescription = VisitNullChecked(node.FullDescription);
                if (node.MessageStrings != null)
                {
                    var keys = node.MessageStrings.Keys.ToArray();
                    foreach (var key in keys)
                    {
                        var value = node.MessageStrings[key];
                        if (value != null)
                        {
                            node.MessageStrings[key] = VisitNullChecked(value);
                        }
                    }
                }

                node.DefaultConfiguration = VisitNullChecked(node.DefaultConfiguration);
                node.Help = VisitNullChecked(node.Help);
                if (node.Relationships != null)
                {
                    for (int index_0 = 0; index_0 < node.Relationships.Count; ++index_0)
                    {
                        node.Relationships[index_0] = VisitNullChecked(node.Relationships[index_0]);
                    }
                }
            }

            return node;
        }

        public virtual ReportingDescriptorReference VisitReportingDescriptorReference(ReportingDescriptorReference node)
        {
            if (node != null)
            {
                node.ToolComponent = VisitNullChecked(node.ToolComponent);
            }

            return node;
        }

        public virtual ReportingDescriptorRelationship VisitReportingDescriptorRelationship(ReportingDescriptorRelationship node)
        {
            if (node != null)
            {
                node.Target = VisitNullChecked(node.Target);
                node.Description = VisitNullChecked(node.Description);
            }

            return node;
        }

        public virtual Result VisitResult(Result node)
        {
            if (node != null)
            {
                node.Rule = VisitNullChecked(node.Rule);
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
                    for (int index_0 = 0; index_0 < node.Graphs.Count; ++index_0)
                    {
                        node.Graphs[index_0] = VisitNullChecked(node.Graphs[index_0]);
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

                if (node.Suppressions != null)
                {
                    for (int index_0 = 0; index_0 < node.Suppressions.Count; ++index_0)
                    {
                        node.Suppressions[index_0] = VisitNullChecked(node.Suppressions[index_0]);
                    }
                }

                if (node.Attachments != null)
                {
                    for (int index_0 = 0; index_0 < node.Attachments.Count; ++index_0)
                    {
                        node.Attachments[index_0] = VisitNullChecked(node.Attachments[index_0]);
                    }
                }

                node.Provenance = VisitNullChecked(node.Provenance);
                if (node.Fixes != null)
                {
                    for (int index_0 = 0; index_0 < node.Fixes.Count; ++index_0)
                    {
                        node.Fixes[index_0] = VisitNullChecked(node.Fixes[index_0]);
                    }
                }

                if (node.Taxa != null)
                {
                    for (int index_0 = 0; index_0 < node.Taxa.Count; ++index_0)
                    {
                        node.Taxa[index_0] = VisitNullChecked(node.Taxa[index_0]);
                    }
                }

                node.WebRequest = VisitNullChecked(node.WebRequest);
                node.WebResponse = VisitNullChecked(node.WebResponse);
            }

            return node;
        }

        public virtual ResultProvenance VisitResultProvenance(ResultProvenance node)
        {
            if (node != null)
            {
                if (node.ConversionSources != null)
                {
                    for (int index_0 = 0; index_0 < node.ConversionSources.Count; ++index_0)
                    {
                        node.ConversionSources[index_0] = VisitNullChecked(node.ConversionSources[index_0]);
                    }
                }
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

                if (node.OriginalUriBaseIds != null)
                {
                    var keys = node.OriginalUriBaseIds.Keys.ToArray();
                    foreach (var key in keys)
                    {
                        var value = node.OriginalUriBaseIds[key];
                        if (value != null)
                        {
                            node.OriginalUriBaseIds[key] = VisitNullChecked(value);
                        }
                    }
                }

                if (node.Artifacts != null)
                {
                    for (int index_0 = 0; index_0 < node.Artifacts.Count; ++index_0)
                    {
                        node.Artifacts[index_0] = VisitNullChecked(node.Artifacts[index_0]);
                    }
                }

                if (node.LogicalLocations != null)
                {
                    for (int index_0 = 0; index_0 < node.LogicalLocations.Count; ++index_0)
                    {
                        node.LogicalLocations[index_0] = VisitNullChecked(node.LogicalLocations[index_0]);
                    }
                }

                if (node.Graphs != null)
                {
                    for (int index_0 = 0; index_0 < node.Graphs.Count; ++index_0)
                    {
                        node.Graphs[index_0] = VisitNullChecked(node.Graphs[index_0]);
                    }
                }

                if (node.Results != null)
                {
                    for (int index_0 = 0; index_0 < node.Results.Count; ++index_0)
                    {
                        node.Results[index_0] = VisitNullChecked(node.Results[index_0]);
                    }
                }

                node.AutomationDetails = VisitNullChecked(node.AutomationDetails);
                if (node.RunAggregates != null)
                {
                    for (int index_0 = 0; index_0 < node.RunAggregates.Count; ++index_0)
                    {
                        node.RunAggregates[index_0] = VisitNullChecked(node.RunAggregates[index_0]);
                    }
                }

                node.ExternalPropertyFileReferences = VisitNullChecked(node.ExternalPropertyFileReferences);
                if (node.ThreadFlowLocations != null)
                {
                    for (int index_0 = 0; index_0 < node.ThreadFlowLocations.Count; ++index_0)
                    {
                        node.ThreadFlowLocations[index_0] = VisitNullChecked(node.ThreadFlowLocations[index_0]);
                    }
                }

                if (node.Taxonomies != null)
                {
                    for (int index_0 = 0; index_0 < node.Taxonomies.Count; ++index_0)
                    {
                        node.Taxonomies[index_0] = VisitNullChecked(node.Taxonomies[index_0]);
                    }
                }

                if (node.Addresses != null)
                {
                    for (int index_0 = 0; index_0 < node.Addresses.Count; ++index_0)
                    {
                        node.Addresses[index_0] = VisitNullChecked(node.Addresses[index_0]);
                    }
                }

                if (node.Translations != null)
                {
                    for (int index_0 = 0; index_0 < node.Translations.Count; ++index_0)
                    {
                        node.Translations[index_0] = VisitNullChecked(node.Translations[index_0]);
                    }
                }

                if (node.Policies != null)
                {
                    for (int index_0 = 0; index_0 < node.Policies.Count; ++index_0)
                    {
                        node.Policies[index_0] = VisitNullChecked(node.Policies[index_0]);
                    }
                }

                if (node.WebRequests != null)
                {
                    for (int index_0 = 0; index_0 < node.WebRequests.Count; ++index_0)
                    {
                        node.WebRequests[index_0] = VisitNullChecked(node.WebRequests[index_0]);
                    }
                }

                if (node.WebResponses != null)
                {
                    for (int index_0 = 0; index_0 < node.WebResponses.Count; ++index_0)
                    {
                        node.WebResponses[index_0] = VisitNullChecked(node.WebResponses[index_0]);
                    }
                }

                node.SpecialLocations = VisitNullChecked(node.SpecialLocations);
            }

            return node;
        }

        public virtual RunAutomationDetails VisitRunAutomationDetails(RunAutomationDetails node)
        {
            if (node != null)
            {
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

                if (node.InlineExternalProperties != null)
                {
                    for (int index_0 = 0; index_0 < node.InlineExternalProperties.Count; ++index_0)
                    {
                        node.InlineExternalProperties[index_0] = VisitNullChecked(node.InlineExternalProperties[index_0]);
                    }
                }
            }

            return node;
        }

        public virtual SpecialLocations VisitSpecialLocations(SpecialLocations node)
        {
            if (node != null)
            {
                node.DisplayBase = VisitNullChecked(node.DisplayBase);
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

        public virtual Suppression VisitSuppression(Suppression node)
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
                if (node.Taxa != null)
                {
                    for (int index_0 = 0; index_0 < node.Taxa.Count; ++index_0)
                    {
                        node.Taxa[index_0] = VisitNullChecked(node.Taxa[index_0]);
                    }
                }

                if (node.State != null)
                {
                    var keys = node.State.Keys.ToArray();
                    foreach (var key in keys)
                    {
                        var value = node.State[key];
                        if (value != null)
                        {
                            node.State[key] = VisitNullChecked(value);
                        }
                    }
                }

                node.WebRequest = VisitNullChecked(node.WebRequest);
                node.WebResponse = VisitNullChecked(node.WebResponse);
            }

            return node;
        }

        public virtual Tool VisitTool(Tool node)
        {
            if (node != null)
            {
                node.Driver = VisitNullChecked(node.Driver);
                if (node.Extensions != null)
                {
                    for (int index_0 = 0; index_0 < node.Extensions.Count; ++index_0)
                    {
                        node.Extensions[index_0] = VisitNullChecked(node.Extensions[index_0]);
                    }
                }
            }

            return node;
        }

        public virtual ToolComponent VisitToolComponent(ToolComponent node)
        {
            if (node != null)
            {
                node.ShortDescription = VisitNullChecked(node.ShortDescription);
                node.FullDescription = VisitNullChecked(node.FullDescription);
                if (node.GlobalMessageStrings != null)
                {
                    var keys = node.GlobalMessageStrings.Keys.ToArray();
                    foreach (var key in keys)
                    {
                        var value = node.GlobalMessageStrings[key];
                        if (value != null)
                        {
                            node.GlobalMessageStrings[key] = VisitNullChecked(value);
                        }
                    }
                }

                if (node.Notifications != null)
                {
                    for (int index_0 = 0; index_0 < node.Notifications.Count; ++index_0)
                    {
                        node.Notifications[index_0] = VisitNullChecked(node.Notifications[index_0]);
                    }
                }

                if (node.Rules != null)
                {
                    for (int index_0 = 0; index_0 < node.Rules.Count; ++index_0)
                    {
                        node.Rules[index_0] = VisitNullChecked(node.Rules[index_0]);
                    }
                }

                if (node.Taxa != null)
                {
                    for (int index_0 = 0; index_0 < node.Taxa.Count; ++index_0)
                    {
                        node.Taxa[index_0] = VisitNullChecked(node.Taxa[index_0]);
                    }
                }

                if (node.Locations != null)
                {
                    for (int index_0 = 0; index_0 < node.Locations.Count; ++index_0)
                    {
                        node.Locations[index_0] = VisitNullChecked(node.Locations[index_0]);
                    }
                }

                node.AssociatedComponent = VisitNullChecked(node.AssociatedComponent);
                node.TranslationMetadata = VisitNullChecked(node.TranslationMetadata);
                if (node.SupportedTaxonomies != null)
                {
                    for (int index_0 = 0; index_0 < node.SupportedTaxonomies.Count; ++index_0)
                    {
                        node.SupportedTaxonomies[index_0] = VisitNullChecked(node.SupportedTaxonomies[index_0]);
                    }
                }
            }

            return node;
        }

        public virtual ToolComponentReference VisitToolComponentReference(ToolComponentReference node)
        {
            if (node != null)
            {
            }

            return node;
        }

        public virtual TranslationMetadata VisitTranslationMetadata(TranslationMetadata node)
        {
            if (node != null)
            {
                node.ShortDescription = VisitNullChecked(node.ShortDescription);
                node.FullDescription = VisitNullChecked(node.FullDescription);
            }

            return node;
        }

        public virtual VersionControlDetails VisitVersionControlDetails(VersionControlDetails node)
        {
            if (node != null)
            {
                node.MappedTo = VisitNullChecked(node.MappedTo);
            }

            return node;
        }

        public virtual WebRequest VisitWebRequest(WebRequest node)
        {
            if (node != null)
            {
                node.Body = VisitNullChecked(node.Body);
            }

            return node;
        }

        public virtual WebResponse VisitWebResponse(WebResponse node)
        {
            if (node != null)
            {
                node.Body = VisitNullChecked(node.Body);
            }

            return node;
        }
    }
}