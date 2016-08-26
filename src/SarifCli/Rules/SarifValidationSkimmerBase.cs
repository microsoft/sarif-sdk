// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;
using Microsoft.CodeAnalysis.Sarif.Driver;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Microsoft.Json.Pointer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Cli.Rules
{
    public abstract class SarifValidationSkimmerBase : SkimmerBase<SarifValidationContext>
    {
        private const string SarifSpecUri =
            "https://rawgit.com/sarif-standard/sarif-spec/master/Static%20Analysis%20Results%20Interchange%20Format%20(SARIF).html";

        private readonly Uri _defaultHelpUri = new Uri(SarifSpecUri);
        private JToken _rootToken;

        public override Uri HelpUri => _defaultHelpUri;

        protected SarifValidationContext Context { get; private set; }

        protected override sealed ResourceManager ResourceManager => RuleResources.ResourceManager;

        public override sealed void Analyze(SarifValidationContext context)
        {
            Context = context;

            string logContents = File.ReadAllText(context.TargetUri.LocalPath);
            _rootToken = JToken.Parse(logContents);

            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                ContractResolver = SarifContractResolver.Instance
            };

            SarifLog log = JsonConvert.DeserializeObject<SarifLog>(logContents, settings);
            string logPointer = string.Empty;

            Visit(log, logPointer);
        }

        private void Visit(SarifLog log, string logPointer)
        {
            if (log.Runs != null)
            {
                Run[] runs = log.Runs.ToArray();
                string runsPointer = logPointer.AtProperty(SarifPropertyName.Runs);

                for (int iRun = 0; iRun < runs.Length; ++iRun)
                {
                    Run run = runs[iRun];
                    string runPointer = runsPointer.AtIndex(iRun);

                    Visit(run, runPointer);
                }
            }
        }

        private void Visit(AnnotatedCodeLocation annotatedCodeLocation, string annotatedCodeLocationPointer)
        {
            if (annotatedCodeLocation.PhysicalLocation != null)
            {
                string physicalLocationPointer = annotatedCodeLocationPointer.AtProperty(SarifPropertyName.PhysicalLocation);
                Visit(annotatedCodeLocation.PhysicalLocation, physicalLocationPointer);
            }
        }

        private void Visit(CodeFlow codeFlow, string codeFlowPointer)
        {
            if (codeFlow.Locations != null)
            {
                AnnotatedCodeLocation[] annotatedCodeLocations = codeFlow.Locations.ToArray();
                string annotatedCodeLocationsPointer = codeFlowPointer.AtProperty(SarifPropertyName.Locations);

                for (int iAnnotatedCodeLocation = 0; iAnnotatedCodeLocation < annotatedCodeLocations.Length; ++iAnnotatedCodeLocation)
                {
                    AnnotatedCodeLocation annotatedCodeLocation = annotatedCodeLocations[iAnnotatedCodeLocation];
                    string annotatedCodeLocationPointer = annotatedCodeLocationsPointer.AtIndex(iAnnotatedCodeLocation);

                    Visit(annotatedCodeLocation, annotatedCodeLocationPointer);
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

                for (int iFileChange = 0; iFileChange < fileChanges.Length; ++iFileChange)
                {
                    FileChange fileChange = fileChanges[iFileChange];
                    string fileChangePointer = fileChangesPointer.AtIndex(iFileChange);

                    Visit(fileChange, fileChangePointer);
                }
            }
        }

        private void Visit(FileChange fileChange, string fileChangePointer)
        {
            Analyze(fileChange, fileChangePointer);
        }

        private void Visit(Location location, string locationPointer)
        {
            if (location.AnalysisTarget != null)
            {
                string analysisTargetPointer = locationPointer.AtProperty(SarifPropertyName.AnalysisTarget);
                Visit(location.AnalysisTarget, analysisTargetPointer);
            }

            if (location.ResultFile != null)
            {
                string resultFilePointer = locationPointer.AtProperty(SarifPropertyName.ResultFile);
                Visit(location.ResultFile, resultFilePointer);
            }
        }

        private void Visit(Notification notification, string notificationPointer)
        {
            if (notification.PhysicalLocation != null)
            {
                string physicalLocationPointer = notificationPointer.AtProperty(SarifPropertyName.PhysicalLocation);

                Visit(notification.PhysicalLocation, physicalLocationPointer);
            }
        }

        private void Visit(PhysicalLocation physicalLocation, string physicalLocationPointer)
        {
            Analyze(physicalLocation, physicalLocationPointer);
        }

        private void Visit(Result result, string resultPointer)
        {
            if (result.Locations != null)
            {
                Location[] locations = result.Locations.ToArray();
                string locationsPointer = resultPointer.AtProperty(SarifPropertyName.Locations);

                for (int iLocation = 0; iLocation < locations.Length; ++iLocation)
                {
                    Location location = locations[iLocation];
                    string locationPointer = locationsPointer.AtIndex(iLocation);

                    Visit(location, locationPointer);
                }
            }

            if (result.CodeFlows != null)
            {
                CodeFlow[] codeFlows = result.CodeFlows.ToArray();
                string codeFlowsPointer = resultPointer.AtProperty(SarifPropertyName.CodeFlows);

                for (int iCodeFlow = 0; iCodeFlow < codeFlows.Length; ++iCodeFlow)
                {
                    CodeFlow codeFlow = codeFlows[iCodeFlow];
                    string codeFlowPointer = codeFlowsPointer.AtIndex(iCodeFlow);

                    Visit(codeFlow, codeFlowPointer);
                }
            }

            if (result.Stacks != null)
            {
                Stack[] stacks = result.Stacks.ToArray();
                string stacksPointer = resultPointer.AtProperty(SarifPropertyName.Stacks);

                for (int iStack = 0; iStack < stacks.Length; ++iStack)
                {
                    Stack stack = stacks[iStack];
                    string stackPointer = stacksPointer.AtIndex(iStack);

                    Visit(stack, stackPointer);
                }
            }

            if (result.RelatedLocations != null)
            {
                AnnotatedCodeLocation[] relatedLocations = result.RelatedLocations.ToArray();
                string relatedLocationsPointer = resultPointer.AtProperty(SarifPropertyName.RelatedLocations);

                for (int iRelatedLocation = 0; iRelatedLocation < relatedLocations.Length; ++iRelatedLocation)
                {
                    AnnotatedCodeLocation relatedLocation = relatedLocations[iRelatedLocation];
                    string relatedLocationPointer = relatedLocationsPointer.AtIndex(iRelatedLocation);

                    Visit(relatedLocation, relatedLocationPointer);
                }
            }

            if (result.Fixes != null)
            {
                Fix[] fixes = result.Fixes.ToArray();
                string fixesPointer = resultPointer.AtProperty(SarifPropertyName.Fixes);

                for (int iFix = 0; iFix < fixes.Length; ++iFix)
                {
                    Fix fix = fixes[iFix];
                    string fixPointer = fixesPointer.AtIndex(iFix);

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

                for (int iResult = 0; iResult < results.Length; ++iResult)
                {
                    Result result = results[iResult];
                    string resultPointer = resultsPointer.AtIndex(iResult);

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

            if (run.Rules != null)
            {
                Rule[] rules = run.Rules.Values.ToArray();
                string rulesPointer = runPointer.AtProperty(SarifPropertyName.Rules);

                for (int iRule = 0; iRule < rules.Length; ++iRule)
                {
                    Rule rule = rules[iRule];
                    if (rule.Id != null)
                    {
                        string rulePointer = rulesPointer.AtProperty(rule.Id);
                        Analyze(rule, rulePointer);
                    }
                }
            }

            if (run.ToolNotifications != null)
            {
                Visit(run.ToolNotifications, runPointer, SarifPropertyName.ToolNotifications);
            }

            if (run.ConfigurationNotifications != null)
            {
                Visit(run.ConfigurationNotifications, runPointer, SarifPropertyName.ConfigurationNotifications);
            }
        }

        private void Visit(IList<Notification> notifications, string parentPointer, string propertyName)
        {
            Notification[] notificationsArray = notifications.ToArray();
            string notificationsPointer = parentPointer.AtProperty(propertyName);

            for (int iNotification = 0; iNotification < notificationsArray.Length; ++iNotification)
            {
                Notification notification = notificationsArray[iNotification];
                string notificationPointer = notificationsPointer.AtIndex(iNotification);

                Visit(notification, notificationPointer);
            }
        }

        private void Visit(Stack stack, string stackPointer)
        {
            if (stack.Frames != null)
            {
                StackFrame[] frames = stack.Frames.ToArray();
                string framesPointer = stackPointer.AtProperty(SarifPropertyName.Frames);

                for (int iFrame = 0; iFrame < frames.Length; ++iFrame)
                {
                    StackFrame frame = frames[iFrame];
                    string framePointer = framesPointer.AtIndex(iFrame);

                    Visit(frame, framePointer);
                }
            }
        }

        private void Visit(StackFrame frame, string framePointer)
        {
            Analyze(frame, framePointer);
        }

        protected virtual void Analyze(FileChange fileChange, string fileChangePointer)
        {
        }

        protected virtual void Analyze(FileData fileData, string fileKey, string filePointer)
        {
        }

        protected virtual void Analyze(PhysicalLocation physicalLocation, string physicalLocationPointer)
        {
        }

        protected virtual void Analyze(Rule rule, string rulePointer)
        {
        }

        protected virtual void Analyze(StackFrame frame, string framePointer)
        {
        }

        protected void LogResult(ResultLevel level, string jPointer, string formatId, params string[] args)
        {
            Region region = GetRegionFromJPointer(jPointer);

            // All messages start with "In {file}, at {jPointer}, ...". Prepend the jPointer to the args.
            // The Sarif.Driver framework will take care of prepending the file name.
            string[] argsWithPointer = new string[args.Length + 1];
            Array.Copy(args, 0, argsWithPointer, 1, args.Length);
            argsWithPointer[0] = jPointer;

            Context.Logger.Log(this,
                RuleUtilities.BuildResult(ResultLevel.Warning, Context, region, formatId, argsWithPointer));
        }

        private Region GetRegionFromJPointer(string jPointer)
        {
            JsonPointer jsonPointer = new JsonPointer(jPointer);
            JToken jToken = jsonPointer.Evaluate(_rootToken);
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
