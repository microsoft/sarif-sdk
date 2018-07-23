// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using Microsoft.CodeAnalysis.Sarif;
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

        protected virtual void Analyze(ThreadFlowLocation threadFlowLocation, string codeFlowLocationPointer)
        {
        }

        protected virtual void Analyze(CodeFlow codeFlow, string codeFlowPointer)
        {
        }

        protected virtual void Analyze(FileChange fileChange, string fileChangePointer)
        {
        }

        protected virtual void Analyze(FileData fileData, string fileKey, string filePointer)
        {
        }

        protected virtual void Analyze(Invocation invocation, string invocationPointer)
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

        private void Visit(SarifLog log, string logPointer)
        {
            if (log.Runs != null)
            {
                Run[] runs = log.Runs.ToArray();
                string runsPointer = logPointer.AtProperty(SarifPropertyName.Runs);

                for (int i = 0; i < runs.Length; ++i)
                {
                    Run run = runs[i];
                    string runPointer = runsPointer.AtIndex(i);

                    Visit(run, runPointer);
                }
            }
        }

        private void Visit(ThreadFlowLocation threadFlowLocation, string codeFlowLocationPointer)
        {
            Analyze(threadFlowLocation, codeFlowLocationPointer);

            if (threadFlowLocation.Location != null)
            {
                string locationPointer = codeFlowLocationPointer.AtProperty(SarifPropertyName.Location);
                Visit(threadFlowLocation.Location, locationPointer);
            }
        }

        private void Visit(CodeFlow codeFlow, string codeFlowPointer)
        {
            Analyze(codeFlow, codeFlowPointer);

            if (codeFlow.Message != null)
            {
                string messagePointer = codeFlowPointer.AtProperty(SarifPropertyName.Message);
                Visit(codeFlow.Message, messagePointer);
            }

            if (codeFlow.ThreadFlows != null)
            {
                ThreadFlow[] threadFlows = codeFlow.ThreadFlows.ToArray();
                string threadFlowsPointer = codeFlowPointer.AtProperty(SarifPropertyName.ThreadFlows);

                for (int i = 0; i < threadFlows.Length; ++i)
                {
                    ThreadFlow threadFlow = threadFlows[i];
                    string threadFlowPointer = threadFlowsPointer.AtIndex(i);

                    Visit(threadFlow, threadFlowPointer);
                }
            }
        }

        private void Visit(FileData fileData, string fileKey, string filePointer)
        {
            Analyze(fileData, fileKey, filePointer);
        }

        private void Visit(Fix fix, string fixPointer)
        {
            if (fix.FileChanges != null)
            {
                FileChange[] fileChanges = fix.FileChanges.ToArray();
                string fileChangesPointer = fixPointer.AtProperty(SarifPropertyName.FileChanges);

                for (int i = 0; i < fileChanges.Length; ++i)
                {
                    FileChange fileChange = fileChanges[i];
                    string fileChangePointer = fileChangesPointer.AtIndex(i);

                    Visit(fileChange, fileChangePointer);
                }
            }
        }

        private void Visit(FileChange fileChange, string fileChangePointer)
        {
            Analyze(fileChange, fileChangePointer);
        }

        private void Visit(Invocation invocation, string invocationPointer)
        {
            Analyze(invocation, invocationPointer);

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
            if (location.PhysicalLocation != null)
            {
                string physicalLocationPointer = locationPointer.AtProperty(SarifPropertyName.PhysicalLocation);
                Visit(location.PhysicalLocation, physicalLocationPointer);
            }
        }

        private void Visit(Message message, string messagePointer)
        {
        }

        private void Visit(Notification notification, string notificationPointer)
        {
            Analyze(notification, notificationPointer);

            if (notification.PhysicalLocation != null)
            {
                string physicalLocationPointer = notificationPointer.AtProperty(SarifPropertyName.PhysicalLocation);

                Visit(notification.PhysicalLocation, physicalLocationPointer);
            }
        }

        private void Visit(PhysicalLocation physicalLocation, string physicalLocationPointer)
        {
            Analyze(physicalLocation, physicalLocationPointer);

            if (physicalLocation.Region != null)
            {
                string regionPointer = physicalLocationPointer.AtProperty(SarifPropertyName.Region);

                Visit(physicalLocation.Region, regionPointer);
            }
        }

        private void Visit(Region region, string regionPointer)
        {
            Analyze(region, regionPointer);
        }

        private void Visit(Result result, string resultPointer)
        {
            Analyze(result, resultPointer);

            if (result.Locations != null)
            {
                Location[] locations = result.Locations.ToArray();
                string locationsPointer = resultPointer.AtProperty(SarifPropertyName.Locations);

                for (int i = 0; i < locations.Length; ++i)
                {
                    Location location = locations[i];
                    string locationPointer = locationsPointer.AtIndex(i);

                    Visit(location, locationPointer);
                }
            }

            if (result.CodeFlows != null)
            {
                CodeFlow[] codeFlows = result.CodeFlows.ToArray();
                string codeFlowsPointer = resultPointer.AtProperty(SarifPropertyName.CodeFlows);

                for (int i = 0; i < codeFlows.Length; ++i)
                {
                    CodeFlow codeFlow = codeFlows[i];
                    string codeFlowPointer = codeFlowsPointer.AtIndex(i);

                    Visit(codeFlow, codeFlowPointer);
                }
            }

            if (result.Stacks != null)
            {
                Stack[] stacks = result.Stacks.ToArray();
                string stacksPointer = resultPointer.AtProperty(SarifPropertyName.Stacks);

                for (int i = 0; i < stacks.Length; ++i)
                {
                    Stack stack = stacks[i];
                    string stackPointer = stacksPointer.AtIndex(i);

                    Visit(stack, stackPointer);
                }
            }

            if (result.RelatedLocations != null)
            {
                Location[] relatedLocations = result.RelatedLocations.ToArray();
                string relatedLocationsPointer = resultPointer.AtProperty(SarifPropertyName.RelatedLocations);

                for (int i = 0; i < relatedLocations.Length; ++i)
                {
                    Location relatedLocation = relatedLocations[i];
                    string relatedLocationPointer = relatedLocationsPointer.AtIndex(i);

                    Visit(relatedLocation, relatedLocationPointer);
                }
            }

            if (result.Fixes != null)
            {
                Fix[] fixes = result.Fixes.ToArray();
                string fixesPointer = resultPointer.AtProperty(SarifPropertyName.Fixes);

                for (int i = 0; i < fixes.Length; ++i)
                {
                    Fix fix = fixes[i];
                    string fixPointer = fixesPointer.AtIndex(i);

                    Visit(fix, fixPointer);
                }
            }
        }

        private void Visit(Run run, string runPointer)
        {
            if (run.Results != null)
            {
                Result[] results = run.Results.ToArray();
                string resultsPointer = runPointer.AtProperty(SarifPropertyName.Results);

                for (int i = 0; i < results.Length; ++i)
                {
                    Result result = results[i];
                    string resultPointer = resultsPointer.AtIndex(i);

                    Visit(result, resultPointer);
                }
            }

            if (run.Files != null)
            {
                IDictionary<string, FileData> files = run.Files;
                string filesPointer = runPointer.AtProperty(SarifPropertyName.Files);

                foreach (string fileKey in files.Keys)
                {
                    string filePointer = filesPointer.AtProperty(fileKey);

                    Visit(files[fileKey], fileKey, filePointer);
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

                    string invocationPointer = invocationsPointer.AtIndex(i);

                    Visit(invocations[i], invocationPointer);
                }
            }
        }

        private void Visit(IList<Notification> notifications, string parentPointer, string propertyName)
        {
            Notification[] notificationsArray = notifications.ToArray();
            string notificationsPointer = parentPointer.AtProperty(propertyName);

            for (int i = 0; i < notificationsArray.Length; ++i)
            {
                Notification notification = notificationsArray[i];
                string notificationPointer = notificationsPointer.AtIndex(i);

                Visit(notification, notificationPointer);
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
                    string rulePointer = rulesPointer.AtProperty(rule.Id);
                    Analyze(rule, rulePointer);
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
                    StackFrame frame = frames[i];
                    string framePointer = framesPointer.AtIndex(i);

                    Visit(frame, framePointer);
                }
            }
        }

        private void Visit(StackFrame frame, string framePointer)
        {
            Analyze(frame, framePointer);
        }

        private void Visit(ThreadFlow threadFlow, string threadFlowPointer)
        {
            Analyze(threadFlow, threadFlowPointer);
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
