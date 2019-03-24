﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.Sarif.Driver;
using Microsoft.Json.Pointer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public abstract class SarifValidationSkimmerBase : Skimmer<SarifValidationContext>
    {
        private const string SarifSpecUri =
            "http://docs.oasis-open.org/sarif/sarif/v2.0/csprd01/sarif-v2.0-csprd01.html";

        private readonly Uri _defaultHelpUri = new Uri(SarifSpecUri);

        public override Uri HelpUri => _defaultHelpUri;

        private readonly MultiformatMessageString _emptyHelpMessage = new MultiformatMessageString
        {
            Text = string.Empty
        };

        public override MultiformatMessageString Help => _emptyHelpMessage;

        protected SarifValidationContext Context { get; private set; }

        protected override sealed ResourceManager ResourceManager => RuleResources.ResourceManager;

        private readonly string[] _emptyMessageResourceNames = new string[0];

        protected override IEnumerable<string> MessageResourceNames => _emptyMessageResourceNames;

        public override sealed void Analyze(SarifValidationContext context)
        {
            Context = context;

            Context.InputLogToken = JToken.Parse(Context.InputLogContents);

            Visit(Context.InputLog, logPointer: string.Empty);
        }

        protected void LogResult(string jPointer, string formatId, params string[] args)
        {
            Region region = GetRegionFromJPointer(jPointer);

            // All messages start with "In {file}, at {jPointer}, ...". Prepend the jPointer to the args.
            string[] argsWithPointer = new string[args.Length + 1];
            Array.Copy(args, 0, argsWithPointer, 1, args.Length);
            argsWithPointer[0] = JsonPointerToJavaScript(jPointer);

            Context.Logger.Log(this,
                RuleUtilities.BuildResult(DefaultLevel, Context, region, formatId, argsWithPointer));
        }

        protected virtual void Analyze(Attachment attachment, string attachmentPointer)
        {
        }

        protected virtual void Analyze(CodeFlow codeFlow, string codeFlowPointer)
        {
        }

        protected virtual void Analyze(Conversion conversion, string conversionPointer)
        {
        }

        protected virtual void Analyze(Edge edge, string edgePointer)
        {
        }

        protected virtual void Analyze(EdgeTraversal edgeTraversal, string edgeTraversalPointer)
        {
        }

        protected virtual void Analyze(ArtifactChange fileChange, string fileChangePointer)
        {
        }

        protected virtual void Analyze(ArtifactLocation fileLocation, string fileLocationPointer)
        {
        }

        protected virtual void Analyze(Artifact fileData, string filePointer)
        {
        }

        protected virtual void Analyze(Graph graph, string graphPointer)
        {
        }

        protected virtual void Analyze(Invocation invocation, string invocationPointer)
        {
        }
        protected virtual void Analyze(LogicalLocation logicalLocation, string logicalLocationPointer)
        {
        }

        protected virtual void Analyze(Message message, string messagePointer)
        {
        }

        protected virtual void Analyze(MultiformatMessageString multiformatMessageString, string multiformatMessageStringPointer)
        {
        }

        protected virtual void Analyze(ReportingDescriptor reportingDescriptor, string reportingDescriptorPointer)
        {
        }

        protected virtual void Analyze(Node node, string nodePointer)
        {
        }

        protected virtual void Analyze(Notification notification, string notificationPointer)
        {
        }

        protected virtual void Analyze(PhysicalLocation physicalLocation, string physicalLocationPointer)
        {
        }

        protected virtual void Analyze(Rectangle rectangle, string rectanglePointer)
        {
        }

        protected virtual void Analyze(Region region, string regionPointer)
        {
        }

        protected virtual void Analyze(Result result, string resultPointer)
        {
        }

        protected virtual void Analyze(ResultProvenance resultProvenance, string resultProvenancePointer)
        {
        }

        protected virtual void Analyze(Run run, string runPointer)
        {
        }

        protected virtual void Analyze(SarifLog log, string logPointer)
        {
        }

        protected virtual void Analyze(Stack stack, string stackPointer)
        {
        }

        protected virtual void Analyze(StackFrame frame, string framePointer)
        {
        }

        protected virtual void Analyze(ThreadFlow threadFlow, string threadFlowPointer)
        {
        }

        protected virtual void Analyze(ThreadFlowLocation threadFlowLocation, string threadFlowLocationPointer)
        {
        }

        protected virtual void Analyze(Tool tool, string toolPointer)
        {
        }

        protected virtual void Analyze(ToolComponent toolComponent, string toolComponentPointer)
        {
        }

        protected virtual void Analyze(VersionControlDetails versionControlDetails, string versionControlDetailsPointer)
        {
        }

        // Convert a string in JSON Pointer format to JavaScript syntax.
        // For example, "/runs/0/id/instanceId" => "runs[0].id.instanceId".
        internal static string JsonPointerToJavaScript(string pointerString)
        {
            var sb = new StringBuilder();
            var pointer = new JsonPointer(pointerString);
            foreach (string token in pointer.ReferenceTokens)
            {
                if (int.TryParse(token, out int index))
                {
                    sb.Append('[' + token + ']');
                }
                else
                {
                    if (TokenIsJavascriptIdentifier(token))
                    {
                        if (sb.Length > 0) { sb.Append('.'); }
                        sb.Append(token);
                    }
                    else
                    {
                        sb.Append("['" + token.UnescapeJsonPointer() + "']");
                    }
                }
            }

            return sb.ToString();
        }

        private static readonly string s_javaScriptIdentifierPattern = @"^[$_\p{L}][$_\p{L}0-9]*$";
        private static readonly Regex s_javaScriptIdentifierRegex = new Regex(s_javaScriptIdentifierPattern, RegexOptions.Compiled);

        private static bool TokenIsJavascriptIdentifier(string token)
        {
            return s_javaScriptIdentifierRegex.IsMatch(token);
        }

        private void Visit(SarifLog log, string logPointer)
        {
            Analyze(log, logPointer);

            if (log.Runs != null)
            {
                string runsPointer = logPointer.AtProperty(SarifPropertyName.Runs);

                for (int i = 0; i < log.Runs.Count; ++i)
                {
                    Visit(log.Runs[i], runsPointer.AtIndex(i));
                }
            }
        }

        private void Visit(Attachment attachment, string attachmentPointer)
        {
            Analyze(attachment, attachmentPointer);

            if (attachment.ArtifactLocation != null)
            {
                Visit(attachment.ArtifactLocation, attachmentPointer.AtProperty(SarifPropertyName.ArtifactLocation));
            }

            if (attachment.Description != null)
            {
                Visit(attachment.Description, attachmentPointer.AtProperty(SarifPropertyName.Description));
            }

            if (attachment.Regions != null)
            {
                string regionsPointer = attachmentPointer.AtProperty(SarifPropertyName.Regions);

                for (int i = 0; i < attachment.Regions.Count; ++i)
                {
                    Visit(attachment.Regions[i], regionsPointer.AtIndex(i));
                }
            }

            if (attachment.Rectangles != null)
            {
                string rectangesPointer = attachmentPointer.AtProperty(SarifPropertyName.Rectangles);

                for (int i = 0; i < attachment.Rectangles.Count; ++i)
                {
                    Visit(attachment.Rectangles[i], rectangesPointer.AtIndex(i));
                }
            }
        }

        private void Visit(CodeFlow codeFlow, string codeFlowPointer)
        {
            Analyze(codeFlow, codeFlowPointer);

            if (codeFlow.Message != null)
            {
                Visit(codeFlow.Message, codeFlowPointer.AtProperty(SarifPropertyName.Message));
            }

            if (codeFlow.ThreadFlows != null)
            {
                string threadFlowsPointer = codeFlowPointer.AtProperty(SarifPropertyName.ThreadFlows);

                for (int i = 0; i < codeFlow.ThreadFlows.Count; ++i)
                {
                    Visit(codeFlow.ThreadFlows[i], threadFlowsPointer.AtIndex(i));
                }
            }
        }

        private void Visit(Conversion conversion, string conversionPointer)
        {
            Analyze(conversion, conversionPointer);

            if (conversion.AnalysisToolLogFiles != null)
            {
                string analysisToolLogFilesPointer = conversionPointer.AtProperty(SarifPropertyName.AnalysisToolLogFiles);

                for (int i = 0; i < conversion.AnalysisToolLogFiles.Count; ++i)
                {
                    Visit(conversion.AnalysisToolLogFiles[i], analysisToolLogFilesPointer.AtIndex(i));
                }
            }
        }

        private void Visit(Edge edge, string edgePointer)
        {
            Analyze(edge, edgePointer);
        }

        private void Visit(EdgeTraversal edgeTraversal, string edgeTraversalPointer)
        {
            Analyze(edgeTraversal, edgeTraversalPointer);

            if (edgeTraversal.Message != null)
            {
                Visit(edgeTraversal.Message, edgeTraversalPointer.AtProperty(SarifPropertyName.Message));
            }
        }

        private void Visit(Artifact fileData, string filePointer)
        {
            Analyze(fileData, filePointer);

            if (fileData.Location != null)
            {
                Visit(fileData.Location, filePointer.AtProperty(SarifPropertyName.Location));
            }
        }

        private void Visit(ArtifactLocation fileLocation, string fileLocationPointer)
        {
            Analyze(fileLocation, fileLocationPointer);
        }

        private void Visit(Fix fix, string fixPointer)
        {
            if (fix.Description != null)
            {
                Visit(fix.Description, fixPointer.AtProperty(SarifPropertyName.Description));
            }

            if (fix.Changes != null)
            {
                string fileChangesPointer = fixPointer.AtProperty(SarifPropertyName.Changes);

                for (int i = 0; i < fix.Changes.Count; ++i)
                {
                    Visit(fix.Changes[i], fileChangesPointer.AtIndex(i));
                }
            }
        }

        private void Visit(ArtifactChange fileChange, string fileChangePointer)
        {
            Analyze(fileChange, fileChangePointer);

            if (fileChange.ArtifactLocation != null)
            {
                Visit(fileChange.ArtifactLocation, fileChangePointer.AtProperty(SarifPropertyName.ArtifactLocation));
            }
        }

        private void Visit(Graph graph, string graphPointer)
        {
            Analyze(graph, graphPointer);

            if (graph.Description != null)
            {
                Visit(graph.Description, graphPointer.AtProperty(SarifPropertyName.Description));
            }

            if (graph.Edges != null)
            {
                string edgesPointer = graphPointer.AtProperty(SarifPropertyName.Edges);

                for (int i = 0; i < graph.Edges.Count; ++i)
                {
                    Visit(graph.Edges[i], edgesPointer.AtIndex(i));
                }
            }

            if (graph.Nodes != null)
            {
                string nodesPointer = graphPointer.AtProperty(SarifPropertyName.Nodes);

                for (int i = 0; i < graph.Nodes.Count; ++i)
                {
                    Visit(graph.Nodes[i], nodesPointer.AtIndex(i));
                }
            }
        }

        private void Visit(GraphTraversal graphTraversal, string graphTraversalPointer)
        {
            if (graphTraversal.Description != null)
            {
                Visit(graphTraversal.Description, graphTraversalPointer.AtProperty(SarifPropertyName.Description));
            }

            if (graphTraversal.EdgeTraversals != null)
            {
                string edgeTraversalsPointer = graphTraversalPointer.AtProperty(SarifPropertyName.EdgeTraversals);

                for (int i = 0; i < graphTraversal.EdgeTraversals.Count; ++i)
                {
                    Visit(graphTraversal.EdgeTraversals[i], edgeTraversalsPointer.AtIndex(i));
                }
            }
        }

        private void Visit(Invocation invocation, string invocationPointer)
        {
            Analyze(invocation, invocationPointer);

            if (invocation.ExecutableLocation != null)
            {
                Visit(invocation.ExecutableLocation, invocationPointer.AtProperty(SarifPropertyName.ExecutableLocation));
            }

            if (invocation.ResponseFiles != null)
            {
                string responseFilesPointer = invocationPointer.AtProperty(SarifPropertyName.ResponseFiles);

                for (int i = 0; i < invocation.ResponseFiles.Count; ++i)
                {
                    Visit(invocation.ResponseFiles[i], responseFilesPointer.AtIndex(i));
                }
            }

            if (invocation.Stdin != null)
            {
                Visit(invocation.Stdin, invocationPointer.AtProperty(SarifPropertyName.Stdin));
            }

            if (invocation.Stdout != null)
            {
                Visit(invocation.Stdout, invocationPointer.AtProperty(SarifPropertyName.Stdout));
            }

            if (invocation.Stderr != null)
            {
                Visit(invocation.Stderr, invocationPointer.AtProperty(SarifPropertyName.Stderr));
            }

            if (invocation.StdoutStderr != null)
            {
                Visit(invocation.StdoutStderr, invocationPointer.AtProperty(SarifPropertyName.StdoutStderr));
            }

            if (invocation.ToolExecutionNotifications != null)
            {
                Visit(invocation.ToolExecutionNotifications, invocationPointer, SarifPropertyName.ToolExecutionNotifications);
            }

            if (invocation.ToolConfigurationNotifications != null)
            {
                Visit(invocation.ToolConfigurationNotifications, invocationPointer, SarifPropertyName.ToolConfigurationNotifications);
            }
        }

        private void Visit(Location location, string locationPointer)
        {
            if (location.Message != null)
            {
                Visit(location.Message, locationPointer.AtProperty(SarifPropertyName.Message));
            }

            if (location.PhysicalLocation != null)
            {
                Visit(location.PhysicalLocation, locationPointer.AtProperty(SarifPropertyName.PhysicalLocation));
            }
        }

        private void Visit(LogicalLocation logicalLocation, string logicalLocationPointer)
        {
            Analyze(logicalLocation, logicalLocationPointer);
        }

        private void Visit(Message message, string messagePointer)
        {
            Analyze(message, messagePointer);
        }

        private void Visit(MultiformatMessageString multiformatMessageString, string multiformatMessageStringPointer)
        {
            Analyze(multiformatMessageString, multiformatMessageStringPointer);
        }

        private void VisitReportingDescriptor(ReportingDescriptor reportingDescriptor, string reportingDescriptorPointer)
        {
            Analyze(reportingDescriptor, reportingDescriptorPointer);
        }

        private void Visit(Node node, string nodePointer)
        {
            Analyze(node, nodePointer);

            if (node.Location != null)
            {
                Visit(node.Location, nodePointer.AtProperty(SarifPropertyName.Location));
            }
        }

        private void Visit(Notification notification, string notificationPointer)
        {
            Analyze(notification, notificationPointer);

            if (notification.Message != null)
            {
                Visit(notification.Message, notificationPointer.AtProperty(SarifPropertyName.Message));
            }

            if (notification.PhysicalLocation != null)
            {
                Visit(notification.PhysicalLocation, notificationPointer.AtProperty(SarifPropertyName.PhysicalLocation));
            }
        }

        private void Visit(PhysicalLocation physicalLocation, string physicalLocationPointer)
        {
            Analyze(physicalLocation, physicalLocationPointer);

            if (physicalLocation.ArtifactLocation != null)
            {
                Visit(physicalLocation.ArtifactLocation, physicalLocationPointer.AtProperty(SarifPropertyName.ArtifactLocation));
            }

            if (physicalLocation.Region != null)
            {
                Visit(physicalLocation.Region, physicalLocationPointer.AtProperty(SarifPropertyName.Region));
            }
        }

        private void Visit(Rectangle rectangle, string rectanglePointer)
        {
            if (rectangle.Message != null)
            {
                Visit(rectangle.Message, rectanglePointer.AtProperty(SarifPropertyName.Message));
            }

            Analyze(rectangle, rectanglePointer);
        }

        private void Visit(Region region, string regionPointer)
        {
            if (region.Message != null)
            {
                Visit(region.Message, regionPointer.AtProperty(SarifPropertyName.Message));
            }

            Analyze(region, regionPointer);
        }

        private void Visit(Result result, string resultPointer)
        {
            Analyze(result, resultPointer);

            if (result.AnalysisTarget != null)
            {
                Visit(result.AnalysisTarget, resultPointer.AtProperty(SarifPropertyName.AnalysisTarget));
            }

            if (result.Attachments != null)
            {
                string attachmentsPointer = resultPointer.AtProperty(SarifPropertyName.Attachments);

                for (int i = 0; i < result.Attachments.Count; ++i)
                {
                    Visit(result.Attachments[i], attachmentsPointer.AtIndex(i));
                }
            }

            if (result.Locations != null)
            {
                string locationsPointer = resultPointer.AtProperty(SarifPropertyName.Locations);

                for (int i = 0; i < result.Locations.Count; ++i)
                {
                    Visit(result.Locations[i], locationsPointer.AtIndex(i));
                }
            }

            if (result.CodeFlows != null)
            {
                string codeFlowsPointer = resultPointer.AtProperty(SarifPropertyName.CodeFlows);

                for (int i = 0; i < result.CodeFlows.Count; ++i)
                {
                    Visit(result.CodeFlows[i], codeFlowsPointer.AtIndex(i));
                }
            }

            if (result.Provenance != null)
            {
                Visit(result.Provenance, resultPointer.AtProperty(SarifPropertyName.Provenance));
            }

            if (result.Graphs != null)
            {
                string graphsPointer = resultPointer.AtProperty(SarifPropertyName.Graphs);

                for (int i = 0; i < result.Graphs.Count; ++i)
                {
                    Visit(result.Graphs[i], graphsPointer.AtIndex(i));
                }
            }

            if (result.GraphTraversals != null)
            {
                string graphTraversalsPointer = resultPointer.AtProperty(SarifPropertyName.GraphTraversals);

                for (int i = 0; i < result.GraphTraversals.Count; ++i)
                {
                    Visit(result.GraphTraversals[i], graphTraversalsPointer.AtIndex(i));
                }
            }

            if (result.Message != null)
            {
                Visit(result.Message, resultPointer.AtProperty(SarifPropertyName.Message));
            }

            if (result.Stacks != null)
            {
                string stacksPointer = resultPointer.AtProperty(SarifPropertyName.Stacks);

                for (int i = 0; i < result.Stacks.Count; ++i)
                {
                    Visit(result.Stacks[i], stacksPointer.AtIndex(i));
                }
            }

            if (result.RelatedLocations != null)
            {
                string relatedLocationsPointer = resultPointer.AtProperty(SarifPropertyName.RelatedLocations);

                for (int i = 0; i < result.RelatedLocations.Count; ++i)
                {
                    Visit(result.RelatedLocations[i], relatedLocationsPointer.AtIndex(i));
                }
            }

            if (result.Fixes != null)
            {
                string fixesPointer = resultPointer.AtProperty(SarifPropertyName.Fixes);

                for (int i = 0; i < result.Fixes.Count; ++i)
                {
                    Visit(result.Fixes[i], fixesPointer.AtIndex(i));
                }
            }
        }

        private void Visit(IList<Notification> notifications, string parentPointer, string propertyName)
        {
            string notificationsPointer = parentPointer.AtProperty(propertyName);

            for (int i = 0; i < notifications.Count; ++i)
            {
                Visit(notifications[i], notificationsPointer.AtIndex(i));
            }
        }

        private void Visit(ResultProvenance resultProvenance, string resultProvenancePointer)
        {
            Analyze(resultProvenance, resultProvenancePointer);

            string conversionSourcesPointer = resultProvenancePointer.AtProperty(SarifPropertyName.ConversionSources);
            for (int i = 0; i < resultProvenance.ConversionSources.Count; ++i)
            {
                Visit(resultProvenance.ConversionSources[i], conversionSourcesPointer.AtIndex(i));
            }
        }

        private void Visit(ReportingDescriptor reportingDecriptor, string reportingDescriptorPointer)
        {
            Analyze(reportingDecriptor, reportingDescriptorPointer);

            if (reportingDecriptor.ShortDescription != null)
            {
                Visit(reportingDecriptor.ShortDescription, reportingDescriptorPointer.AtProperty(SarifPropertyName.ShortDescription));
            }

            if (reportingDecriptor.FullDescription != null)
            {
                Visit(reportingDecriptor.FullDescription, reportingDescriptorPointer.AtProperty(SarifPropertyName.FullDescription));
            }
        }

        private void Visit(Run run, string runPointer)
        {
            Analyze(run, runPointer);

            if (run.Conversion != null)
            {
                Visit(run.Conversion, runPointer.AtProperty(SarifPropertyName.Conversion));
            }

            if (run.Results != null)
            {
                string resultsPointer = runPointer.AtProperty(SarifPropertyName.Results);

                for (int i = 0; i < run.Results.Count; ++i)
                {
                    Visit(run.Results[i], resultsPointer.AtIndex(i));
                }
            }

            if (run.Artifacts != null)
            {
                string filesPointer = runPointer.AtProperty(SarifPropertyName.Artifacts);

                for (int i = 0; i < run.Artifacts.Count; ++i)
                {
                    Visit(run.Artifacts[i], filesPointer.AtIndex(i));
                }
            }

            if (run.LogicalLocations != null)
            {
                string logicalLocationsPointer = runPointer.AtProperty(SarifPropertyName.LogicalLocations);

                for (int i = 0; i < run.LogicalLocations.Count; ++i)
                {
                    Visit(run.LogicalLocations[i], logicalLocationsPointer.AtIndex(i));
                }
            }

            if (run.Graphs != null)
            {
                string graphsPointer = runPointer.AtProperty(SarifPropertyName.Graphs);

                for (int i = 0; i < run.Graphs.Count; ++i)
                {
                    Visit(run.Graphs[i], graphsPointer.AtIndex(i));
                }
            }

            if (run.Invocations != null)
            {
                string invocationsPointer = runPointer.AtProperty(SarifPropertyName.Invocations);

                for (int i = 0; i < run.Invocations.Count; ++i)
                {
                    Visit(run.Invocations[i], invocationsPointer.AtIndex(i));
                }
            }

            if (run.Tool != null)
            {
                Visit(run.Tool, runPointer.AtProperty(SarifPropertyName.Tool));
            }

            if (run.VersionControlProvenance != null)
            {
                string versionControlProvenancePointer = runPointer.AtProperty(SarifPropertyName.VersionControlProvenance);

                for (int i = 0; i < run.VersionControlProvenance.Count; ++i)
                {
                    Visit(run.VersionControlProvenance[i], versionControlProvenancePointer.AtIndex(i));
                }
            }
        }

        private void Visit(Stack stack, string stackPointer)
        {
            Analyze(stack, stackPointer);

            if (stack.Frames != null)
            {
                string framesPointer = stackPointer.AtProperty(SarifPropertyName.Frames);

                for (int i = 0; i < stack.Frames.Count; ++i)
                {
                    Visit(stack.Frames[i], framesPointer.AtIndex(i));
                }
            }

            if (stack.Message != null)
            {
                Visit(stack.Message, stackPointer.AtProperty(SarifPropertyName.Message));
            }
        }

        private void Visit(StackFrame frame, string framePointer)
        {
            Analyze(frame, framePointer);

            if (frame.Location != null)
            {
                Visit(frame.Location, framePointer.AtProperty(SarifPropertyName.Location));
            }
        }

        private void Visit(ThreadFlow threadFlow, string threadFlowPointer)
        {
            Analyze(threadFlow, threadFlowPointer);

            if (threadFlow.Message != null)
            {
                Visit(threadFlow.Message, threadFlowPointer.AtProperty(SarifPropertyName.Message));
            }

            if (threadFlow.Locations != null)
            {
                string threadFlowLocationsPointer = threadFlowPointer.AtProperty(SarifPropertyName.Locations);

                for (int i = 0; i < threadFlow.Locations.Count; ++i)
                {
                    Visit(threadFlow.Locations[i], threadFlowLocationsPointer.AtIndex(i));
                }
            }
        }

        private void Visit(ThreadFlowLocation threadFlowLocation, string threadFlowLocationPointer)
        {
            Analyze(threadFlowLocation, threadFlowLocationPointer);

            if (threadFlowLocation.Location != null)
            {
                Visit(threadFlowLocation.Location, threadFlowLocationPointer.AtProperty(SarifPropertyName.Location));
            }
        }

        private void Visit(Tool tool, string toolPointer)
        {
            Analyze(tool, toolPointer);

            if (tool.Driver != null)
            {
                Visit(tool.Driver, toolPointer.AtProperty(SarifPropertyName.Driver));
            }

            if (tool.Extensions != null)
            {
                string extensionsPointer = toolPointer.AtProperty(SarifPropertyName.Extensions);
                for (int i = 0; i < tool.Extensions.Count; ++i)
                {
                    Visit(tool.Extensions[i], extensionsPointer.AtIndex(i));
                }
            }
        }

        private void Visit(ToolComponent toolComponent, string toolComponentPointer)
        {
            Analyze(toolComponent, toolComponentPointer);

            if (toolComponent.NotificationDescriptors != null)
            {
                string notificationsPointer = toolComponentPointer.AtProperty(SarifPropertyName.NotificationDescriptors);
                for (int i = 0; i < toolComponent.NotificationDescriptors.Count; ++i)
                {
                    Visit(toolComponent.NotificationDescriptors[i], notificationsPointer.AtIndex(i));
                }
            }

            if (toolComponent.RuleDescriptors != null)
            {
                string rulesPointer = toolComponentPointer.AtProperty(SarifPropertyName.RuleDescriptors);
                for (int i = 0; i < toolComponent.RuleDescriptors.Count; ++i)
                {
                    Visit(toolComponent.RuleDescriptors[i], rulesPointer.AtIndex(i));
                }
            }
        }

        private void Visit(VersionControlDetails versionControlDetails, string versionControlDetailsPointer)
        {
            Analyze(versionControlDetails, versionControlDetailsPointer);

            if (versionControlDetails.MappedTo != null)
            {
                Visit(versionControlDetails.MappedTo, versionControlDetailsPointer.AtProperty(SarifPropertyName.MappedTo));
            }
        }

        private Region GetRegionFromJPointer(string jPointer)
        {
            JsonPointer jsonPointer = new JsonPointer(jPointer);
            JToken jToken = jsonPointer.Evaluate(Context.InputLogToken);
            IJsonLineInfo lineInfo = jToken;

            Region region = null;
            if (lineInfo.HasLineInfo())
            {
                region = new Region
                {
                    StartLine = lineInfo.LineNumber,
                    StartColumn = lineInfo.LinePosition
                };
            }

            return region;
        }
    }
}
