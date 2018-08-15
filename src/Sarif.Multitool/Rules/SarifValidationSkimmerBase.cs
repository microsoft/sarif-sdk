// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using Microsoft.CodeAnalysis.Sarif.Driver;
using Microsoft.Json.Pointer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public abstract class SarifValidationSkimmerBase : SkimmerBase<SarifValidationContext>
    {
        private const string SarifSpecUri =
            "http://docs.oasis-open.org/sarif/sarif/v2.0/csprd01/sarif-v2.0-csprd01.html";

        private readonly Uri _defaultHelpUri = new Uri(SarifSpecUri);

        public override Uri HelpUri => _defaultHelpUri;

        private readonly Message _emptyHelpMessage = new Message
        {
            Text = string.Empty
        };

        public override Message Help => _emptyHelpMessage;

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
            argsWithPointer[0] = jPointer;

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

        protected virtual void Analyze(FileChange fileChange, string fileChangePointer)
        {
        }

        protected virtual void Analyze(FileLocation fileLocation, string fileLocationPointer)
        {
        }

        protected virtual void Analyze(FileData fileData, string fileKey, string filePointer)
        {
        }

        protected virtual void Analyze(Graph graph, string graphPointer)
        {
        }

        protected virtual void Analyze(Invocation invocation, string invocationPointer)
        {
        }

        protected virtual void Analyze(Message message, string messagePointer)
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

        protected virtual void Analyze(Region region, string regionPointer)
        {
        }

        protected virtual void Analyze(Result result, string resultPointer)
        {
        }

        protected virtual void Analyze(Rule rule, string rulePointer)
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

        private void Visit(SarifLog log, string logPointer)
        {
            if (log.Runs != null)
            {
                Run[] runs = log.Runs.ToArray();
                string runsPointer = logPointer.AtProperty(SarifPropertyName.Runs);

                for (int i = 0; i < runs.Length; ++i)
                {
                    Visit(runs[i], runsPointer.AtIndex(i));
                }
            }
        }

        private void Visit(Attachment attachment, string attachmentPointer)
        {
            Analyze(attachment, attachmentPointer);

            if (attachment.FileLocation != null)
            {
                Visit(attachment.FileLocation, attachmentPointer.AtProperty(SarifPropertyName.FileLocation));
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
                ThreadFlow[] threadFlows = codeFlow.ThreadFlows.ToArray();
                string threadFlowsPointer = codeFlowPointer.AtProperty(SarifPropertyName.ThreadFlows);

                for (int i = 0; i < threadFlows.Length; ++i)
                {
                    Visit(threadFlows[i], threadFlowsPointer.AtIndex(i));
                }
            }
        }

        private void Visit(Conversion conversion, string conversionPointer)
        {
            Analyze(conversion, conversionPointer);

            if (conversion.AnalysisToolLogFiles != null)
            {
                FileLocation[] analysisToolLogFiles = conversion.AnalysisToolLogFiles.ToArray();
                string analysisToolLogFilesPointer = conversionPointer.AtProperty(SarifPropertyName.AnalysisToolLogFiles);

                for (int i = 0; i < analysisToolLogFiles.Length; ++i)
                {
                    Visit(analysisToolLogFiles[i], analysisToolLogFilesPointer.AtIndex(i));
                }
            }
        }

        private void Visit(Edge edge, string edgePointer)
        {
            Analyze(edge, edgePointer);
        }

        private void Visit(FileData fileData, string fileKey, string filePointer)
        {
            Analyze(fileData, fileKey, filePointer);

            if (fileData.FileLocation != null)
            {
                Visit(fileData.FileLocation, filePointer.AtProperty(SarifPropertyName.FileLocation));
            }
        }

        private void Visit(FileLocation fileLocation, string fileLocationPointer)
        {
            Analyze(fileLocation, fileLocationPointer);
        }

        private void Visit(Fix fix, string fixPointer)
        {
            if (fix.FileChanges != null)
            {
                FileChange[] fileChanges = fix.FileChanges.ToArray();
                string fileChangesPointer = fixPointer.AtProperty(SarifPropertyName.FileChanges);

                for (int i = 0; i < fileChanges.Length; ++i)
                {
                    Visit(fileChanges[i], fileChangesPointer.AtIndex(i));
                }
            }
        }

        private void Visit(FileChange fileChange, string fileChangePointer)
        {
            Analyze(fileChange, fileChangePointer);

            if (fileChange.FileLocation != null)
            {
                Visit(fileChange.FileLocation, fileChangePointer.AtProperty(SarifPropertyName.FileLocation));
            }
        }

        private void Visit(Graph graph, string graphPointer)
        {
            Analyze(graph, graphPointer);

            if (graph.Edges != null)
            {
                Edge[] edges = graph.Edges.ToArray();
                string edgesPointer = graphPointer.AtProperty(SarifPropertyName.Edges);

                for (int i = 0; i < edges.Length; ++i)
                {
                    Visit(edges[i], edgesPointer.AtIndex(i));
                }
            }

            if (graph.Nodes != null)
            {
                Node[] nodes = graph.Nodes.ToArray();
                string nodesPointer = graphPointer.AtProperty(SarifPropertyName.Nodes);

                for (int i = 0; i < nodes.Length; ++i)
                {
                    Visit(nodes[i], nodesPointer.AtIndex(i));
                }
            }
        }

        private void Visit(Invocation invocation, string invocationPointer)
        {
            Analyze(invocation, invocationPointer);

            if (invocation.Attachments != null)
            {
                Attachment[] attachments = invocation.Attachments.ToArray();
                string attachmentsPointer = invocationPointer.AtProperty(SarifPropertyName.Attachments);

                for (int i = 0; i < attachments.Length; ++i)
                {
                    Visit(attachments[i], attachmentsPointer.AtIndex(i));
                }
            }

            if (invocation.ExecutableLocation != null)
            {
                Visit(invocation.ExecutableLocation, invocationPointer.AtProperty(SarifPropertyName.ExecutableLocation));
            }

            if (invocation.ResponseFiles != null)
            {
                FileLocation[] responseFiles = invocation.ResponseFiles.ToArray();
                string responseFilesPointer = invocationPointer.AtProperty(SarifPropertyName.ResponseFiles);

                for (int i = 0; i < responseFiles.Length; ++i)
                {
                    Visit(responseFiles[i], responseFilesPointer.AtIndex(i));
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

            if (invocation.ToolNotifications != null)
            {
                Visit(invocation.ToolNotifications, invocationPointer, SarifPropertyName.ToolNotifications);
            }

            if (invocation.ConfigurationNotifications != null)
            {
                Visit(invocation.ConfigurationNotifications, invocationPointer, SarifPropertyName.ConfigurationNotifications);
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

        private void Visit(Message message, string messagePointer)
        {
            Analyze(message, messagePointer);
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

            if (physicalLocation.FileLocation != null)
            {
                Visit(physicalLocation.FileLocation, physicalLocationPointer.AtProperty(SarifPropertyName.FileLocation));
            }

            if (physicalLocation.Region != null)
            {
                Visit(physicalLocation.Region, physicalLocationPointer.AtProperty(SarifPropertyName.Region));
            }
        }

        private void Visit(Region region, string regionPointer)
        {
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
                Attachment[] attachments = result.Attachments.ToArray();
                string attachmentsPointer = resultPointer.AtProperty(SarifPropertyName.Attachments);

                for (int i = 0; i < attachments.Length; ++i)
                {
                    Visit(attachments[i], attachmentsPointer.AtIndex(i));
                }
            }

            if (result.Locations != null)
            {
                Location[] locations = result.Locations.ToArray();
                string locationsPointer = resultPointer.AtProperty(SarifPropertyName.Locations);

                for (int i = 0; i < locations.Length; ++i)
                {
                    Visit(locations[i], locationsPointer.AtIndex(i));
                }
            }

            if (result.CodeFlows != null)
            {
                CodeFlow[] codeFlows = result.CodeFlows.ToArray();
                string codeFlowsPointer = resultPointer.AtProperty(SarifPropertyName.CodeFlows);

                for (int i = 0; i < codeFlows.Length; ++i)
                {
                    Visit(codeFlows[i], codeFlowsPointer.AtIndex(i));
                }
            }

            if (result.ConversionProvenance != null)
            {
                PhysicalLocation[] physicalLocations = result.ConversionProvenance.ToArray();
                string conversionProvenancePointer = resultPointer.AtProperty(SarifPropertyName.ConversionProvenance);

                for (int i = 0; i < physicalLocations.Length; ++i)
                {
                    Visit(physicalLocations[i], conversionProvenancePointer.AtIndex(i));
                }
            }

            if (result.Graphs != null)
            {
                Graph[] graphs = result.Graphs.ToArray();
                string graphsPointer = resultPointer.AtProperty(SarifPropertyName.Graphs);

                for (int i = 0; i < graphs.Length; ++i)
                {
                    Visit(graphs[i], graphsPointer.AtIndex(i));
                }
            }

            if (result.Message != null)
            {
                Visit(result.Message, resultPointer.AtProperty(SarifPropertyName.Message));
            }

            if (result.Stacks != null)
            {
                Stack[] stacks = result.Stacks.ToArray();
                string stacksPointer = resultPointer.AtProperty(SarifPropertyName.Stacks);

                for (int i = 0; i < stacks.Length; ++i)
                {
                    Visit(stacks[i], stacksPointer.AtIndex(i));
                }
            }

            if (result.RelatedLocations != null)
            {
                Location[] relatedLocations = result.RelatedLocations.ToArray();
                string relatedLocationsPointer = resultPointer.AtProperty(SarifPropertyName.RelatedLocations);

                for (int i = 0; i < relatedLocations.Length; ++i)
                {
                    Visit(relatedLocations[i], relatedLocationsPointer.AtIndex(i));
                }
            }

            if (result.Fixes != null)
            {
                Fix[] fixes = result.Fixes.ToArray();
                string fixesPointer = resultPointer.AtProperty(SarifPropertyName.Fixes);

                for (int i = 0; i < fixes.Length; ++i)
                {
                    Visit(fixes[i], fixesPointer.AtIndex(i));
                }
            }
        }

        private void Visit(Run run, string runPointer)
        {
            if (run.Conversion != null)
            {
                Visit(run.Conversion, runPointer.AtProperty(SarifPropertyName.Conversion));
            }

            if (run.Results != null)
            {
                Result[] results = run.Results.ToArray();
                string resultsPointer = runPointer.AtProperty(SarifPropertyName.Results);

                for (int i = 0; i < results.Length; ++i)
                {
                    Visit(results[i], resultsPointer.AtIndex(i));
                }
            }

            if (run.Files != null)
            {
                IDictionary<string, FileData> files = run.Files;
                string filesPointer = runPointer.AtProperty(SarifPropertyName.Files);

                foreach (string fileKey in files.Keys)
                {
                    Visit(files[fileKey], fileKey, filesPointer.AtProperty(fileKey));
                }
            }

            if (run.Graphs != null)
            {
                Graph[] graphs = run.Graphs.ToArray();
                string graphsPointer = runPointer.AtProperty(SarifPropertyName.Graphs);

                for (int i = 0; i < graphs.Length; ++i)
                {
                    Visit(graphs[i], graphsPointer.AtIndex(i));
                }
            }

            if (run.Resources != null)
            {
                Visit(run.Resources, runPointer.AtProperty(SarifPropertyName.Resources));
            }

            if (run.Invocations != null)
            {
                Invocation[] invocations = run.Invocations.ToArray();
                string invocationsPointer = runPointer.AtProperty(SarifPropertyName.Invocations);

                for (int i = 0; i < invocations.Length; ++i)
                {
                    Visit(invocations[i], invocationsPointer.AtIndex(i));
                }
            }
        }

        private void Visit(IList<Notification> notifications, string parentPointer, string propertyName)
        {
            Notification[] notificationsArray = notifications.ToArray();
            string notificationsPointer = parentPointer.AtProperty(propertyName);

            for (int i = 0; i < notificationsArray.Length; ++i)
            {
                Visit(notificationsArray[i], notificationsPointer.AtIndex(i));
            }
        }

        private void Visit(Resources resources, string resourcesPointer)
        {
            Rule[] rules = resources.Rules.Values.ToArray();
            string rulesPointer = resourcesPointer.AtProperty(SarifPropertyName.Rules);

            for (int i = 0; i < rules.Length; ++i)
            {
                Rule rule = rules[i];
                if (rule.Id != null)
                {
                    Analyze(rule, rulesPointer.AtProperty(rule.Id));
                }
            }
        }

        private void Visit(Stack stack, string stackPointer)
        {
            Analyze(stack, stackPointer);

            if (stack.Frames != null)
            {
                StackFrame[] frames = stack.Frames.ToArray();
                string framesPointer = stackPointer.AtProperty(SarifPropertyName.Frames);

                for (int i = 0; i < frames.Length; ++i)
                {
                    Visit(frames[i], framesPointer.AtIndex(i));
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

            if (threadFlow.Locations != null)
            {
                ThreadFlowLocation[] threadFlowLocations = threadFlow.Locations.ToArray();
                string threadFlowLocationsPointer = threadFlowPointer.AtProperty(SarifPropertyName.Locations);

                for (int i = 0; i < threadFlowLocations.Length; ++i)
                {
                    Visit(threadFlowLocations[i], threadFlowLocationsPointer.AtIndex(i));
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
