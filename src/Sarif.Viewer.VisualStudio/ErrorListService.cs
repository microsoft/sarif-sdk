// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Microsoft.CodeAnalysis.Sarif.Writers;

using Newtonsoft.Json;

namespace Microsoft.Sarif.Viewer
{
    public class ErrorListService
    {
        public static readonly ErrorListService Instance = new ErrorListService();

        public static void ProcessLogFile(string filePath, ToolFormat toolFormat = ToolFormat.None)
        {
            SarifLog log;

            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                ContractResolver = SarifContractResolver.Instance,
            };

            string logText;

            if (toolFormat == ToolFormat.None)
            {
                logText = File.ReadAllText(filePath);
            }
            else if (toolFormat == ToolFormat.PREfast)
            {
                logText = ToolFormatConverter.ConvertPREfastToStandardFormat(filePath);
            }
            else
            {
                // We have conversion to do
                var converter = new ToolFormatConverter();
                var sb = new StringBuilder();

                using (var input = new MemoryStream(File.ReadAllBytes(filePath)))
                {
                    var outputTextWriter = new StringWriter(sb);                
                    var outputJson = new JsonTextWriter(outputTextWriter);
                    var output = new ResultLogJsonWriter(outputJson);

                    input.Seek(0, SeekOrigin.Begin);
                    converter.ConvertToStandardFormat(toolFormat, input, output);

                    // This is serving as a flush mechanism
                    output.Dispose();

                    logText = sb.ToString();
                }
            }

            log = JsonConvert.DeserializeObject<SarifLog>(logText, settings);
            ProcessSarifLog(log);
        }

        private static void ProcessSarifLog(SarifLog sarifLog)
        {
            foreach (Run run in sarifLog.Runs)
            {
                Instance.WriteRunToErrorList(run);
            }

            SarifTableDataSource.Instance.BringToFront();
        }

        private ErrorListService()
        {
            this.documentToLineIndexMap = new Dictionary<string, NewLineIndex>();
        }

        private Dictionary<string, NewLineIndex> documentToLineIndexMap;

        private IRule GetRule(Run runLog, string ruleId)
        {
            if (runLog.Rules == null)
            {
                return null;
            }

            foreach (Rule rule in runLog.Rules.Values)
            {
                if (rule.Id == ruleId) { return rule; }
            }

            throw new InvalidOperationException();
        }

        private void WriteRunToErrorList(Run runLog)
        {
            List<SarifError> sarifErrors = new List<SarifError>();

            // Prefer optional fullName,  fall back to required Name property
            string toolName = runLog.Tool.FullName ?? runLog.Tool.Name;

            foreach (Result result in runLog.Results)
            {
                string category, document;
                Region region;

                category = null;

                if (result.Properties != null)
                {
                    result.Properties.TryGetValue("category", out category);
                }


                if (result.Locations != null)
                {

                    foreach (Location location in result?.Locations)
                    {
                        region = null;
                        Uri uri;

                        PhysicalLocation physicalLocation = null;
                        if (location.ResultFile != null)
                        {
                            physicalLocation = location.ResultFile;
                            uri = physicalLocation.Uri;
                            document = uri.IsAbsoluteUri ? uri.LocalPath : uri.ToString();
                            region = physicalLocation.Region;
                        }
                        else if (location.AnalysisTarget != null)
                        {
                            physicalLocation = location.AnalysisTarget;
                            uri = physicalLocation.Uri;
                            document = physicalLocation.Uri.LocalPath;
                            region = physicalLocation.Region;
                        }
                        else
                        {
                            document = location.FullyQualifiedLogicalName;
                        }

                        AddResult(runLog, sarifErrors, toolName, result, category, document, region);
                    }
                }
                else
                {
                    AddResult(runLog, sarifErrors, toolName, result, category, document: @"d:\repros\test.txt", region: null);
                }

                CodeAnalysisResultManager.Instance.SarifErrors = sarifErrors;
                SarifTableDataSource.Instance.AddErrors(sarifErrors);
            }
        }

        private void AddResult(Run runLog, List<SarifError> sarifErrors, string toolName, Result result, string category, string document, Region region)
        {
            IRule rule;
            string shortMessage, fullMessage;

            rule = GetRule(runLog, result.RuleId);
            shortMessage = result.GetMessageText(rule, concise: true);
            fullMessage = result.GetMessageText(rule, concise: false);

            if (shortMessage == fullMessage)
            {
                fullMessage = null;
            }

            SarifError sarifError = new SarifError(document)
            {
                Region = region,
                RuleId = result.RuleId,
                RuleName = rule?.Name,
                Level = result.Level,
                Category = category,
                ShortMessage = shortMessage,
                FullMessage = fullMessage,
                Tool = toolName,
                HelpLink = rule?.HelpUri?.ToString()
            };

            IEnumerable<IEnumerable<AnnotatedCodeLocation>> stackLocations = CreateAnnotatedCodeLocationsFromStacks(result.Stacks);
            IEnumerable<IEnumerable<AnnotatedCodeLocation>> codeFlowLocations = CreateAnnotatedCodeLocationsFromCodeFlows(result.CodeFlows);

            CreateAnnotatedCodeLocationCollections(stackLocations, AnnotatedCodeLocationKind.Stack, sarifError);
            CreateAnnotatedCodeLocationCollections(codeFlowLocations, AnnotatedCodeLocationKind.CodeFlow, sarifError);
            CaptureAnnotatedCodeLocations(result.RelatedLocations, AnnotatedCodeLocationKind.Stack, sarifError);

            if (region != null)
            {
                sarifError.ColumnNumber = region.StartColumn - 1;
                sarifError.LineNumber = region.StartLine - 1;
            }

            sarifErrors.Add(sarifError);
        }

        private IEnumerable<IEnumerable<AnnotatedCodeLocation>> CreateAnnotatedCodeLocationsFromStacks(IEnumerable<Stack> stacks)
        {
            if (stacks == null) { return null; }

            List<List<AnnotatedCodeLocation>> codeLocationCollections = new List<List<AnnotatedCodeLocation>>();

            foreach (Stack stack in stacks)
            {
                if (stack.Frames == null) { continue; }

                var codeLocations = new List<AnnotatedCodeLocation>();

                foreach (StackFrame stackFrame in stack.Frames)
                {
                    codeLocations.Add(new AnnotatedCodeLocation
                    {
                        Message = stackFrame.ToString(),
                        PhysicalLocation = new PhysicalLocation
                        {
                            Uri = stackFrame.Uri,
                            Region = new Region
                            {
                                StartLine = stackFrame.Line,
                                StartColumn = stackFrame.Column
                            }
                        }
                    });
                }
                codeLocationCollections.Add(codeLocations);
            }
            return codeLocationCollections;
        }

        private IEnumerable<IEnumerable<AnnotatedCodeLocation>> CreateAnnotatedCodeLocationsFromCodeFlows(IEnumerable<CodeFlow> codeFlows)
        {
            if (codeFlows == null) { return null; }

            List<List<AnnotatedCodeLocation>> codeLocationCollections = new List<List<AnnotatedCodeLocation>>();

            foreach (CodeFlow codeFlow in codeFlows)
            {
                if (codeFlow.Locations == null) { continue; }

                var codeLocations = new List<AnnotatedCodeLocation>(codeFlow.Locations);
                codeLocationCollections.Add(codeLocations);
            }
            return codeLocationCollections;
        }

        private static void CreateAnnotatedCodeLocationCollections(
            IEnumerable<IEnumerable<AnnotatedCodeLocation>> codeLocationCollections, 
            AnnotatedCodeLocationKind annotatedCodeLocationKind,
            SarifError sarifError)
        {
            if (codeLocationCollections == null) { return; }

            foreach (IEnumerable<AnnotatedCodeLocation> codeLocations in codeLocationCollections)
            {
                CaptureAnnotatedCodeLocations(codeLocations, annotatedCodeLocationKind, sarifError);
            }
        }

        private static void CaptureAnnotatedCodeLocations(IEnumerable<AnnotatedCodeLocation> codeLocations, AnnotatedCodeLocationKind annotatedCodeLocationKind, SarifError sarifError)
        {
            if (codeLocations == null)
            {
                return;
            }

            int annotationCollectionCount = 0;

            foreach (AnnotatedCodeLocation codeLocation in codeLocations)
            {
                PhysicalLocation plc = codeLocation.PhysicalLocation;
                sarifError.Annotations.Add(new AnnotatedCodeLocationModel()
                {
                    Index = annotationCollectionCount,
                    Kind = annotatedCodeLocationKind,
                    Region = plc.Region,
                    FilePath = plc.Uri.LocalPath,
                    Message = codeLocation.Message
                });

                annotationCollectionCount++;
            }
        }

        private static bool IsError(ResultLevel level)
        {
            return level == ResultLevel.Error;
        }
    }
}