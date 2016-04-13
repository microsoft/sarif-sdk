// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.Driver;
using Microsoft.CodeAnalysis.Sarif.Driver.Sdk;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Microsoft.CodeAnalysis.Sarif.Sdk;
using Microsoft.CodeAnalysis.Sarif.Writers;

using Newtonsoft.Json;

namespace Microsoft.Sarif.Viewer
{
    public class ErrorListService
    {
        public static readonly ErrorListService Instance = new ErrorListService();

        public static void ProcessLogFile(string filePath, ToolFormat toolFormat = ToolFormat.None)
        {
            ResultLog log;

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

            log = JsonConvert.DeserializeObject<ResultLog>(logText, settings);
            ProcessSarifLog(log);
        }

        private static void ProcessSarifLog(ResultLog resultLog)
        {
            foreach (RunLog runLog in resultLog.RunLogs)
            {
                Instance.WriteRunLogToErrorList(runLog);
            }

            SarifTableDataSource.Instance.BringToFront();
        }

        private ErrorListService()
        {
            this.documentToLineIndexMap = new Dictionary<string, NewLineIndex>();
        }

        private Dictionary<string, NewLineIndex> documentToLineIndexMap;

        private IRuleDescriptor GetRule(RunLog runLog, string ruleId)
        {
            if (runLog.RuleInfo == null)
            {
                return null;
            }

            foreach (IRuleDescriptor ruleDescriptor in runLog.RuleInfo)
            {
                if (ruleDescriptor.Id == ruleId) { return ruleDescriptor; }
            }

            throw new InvalidOperationException();
        }

        private void WriteRunLogToErrorList(RunLog runLog)
        {
            List<SarifError> sarifErrors = new List<SarifError>();

            // Prefer optional fullName,  fall back to required Name property
            string toolName = runLog.ToolInfo.FullName ?? runLog.ToolInfo.Name;

            foreach (Result result in runLog.Results)
            {
                string category, document, shortMessage, fullMessage;
                Region region;
                NewLineIndex newLineIndex;
                IRuleDescriptor rule = null;

                category = null;

                if (result.Properties != null)
                {
                    result.Properties.TryGetValue("category", out category);
                }

                foreach (Location location in result.Locations)
                {
                    region = null;

                    PhysicalLocationComponent physicalLocation = null;
                    if (location.ResultFile != null)
                    {
                        physicalLocation = location.ResultFile[0];
                        document = physicalLocation.Uri.LocalPath;
                        region = physicalLocation.Region;
                    }
                    else if (location.AnalysisTarget != null)
                    {
                        physicalLocation = location.AnalysisTarget[0];
                        document = physicalLocation.Uri.LocalPath;
                        region = physicalLocation.Region;
                    }
                    else
                    {
                        document = location.FullyQualifiedLogicalName;
                    }

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
                        Kind = result.Kind,
                        Category = category,
                        ShortMessage = shortMessage,
                        FullMessage = fullMessage,
                        MimeType = physicalLocation?.MimeType,
                        Tool = toolName,
                        HelpLink = rule?.HelpUri?.ToString()                        
                    };

                    CaptureAnnotatedCodeLocations(result.ExecutionFlows, AnnotatedCodeLocationKind.ExecutionFlow, sarifError);
                    CaptureAnnotatedCodeLocations(result.Stacks, AnnotatedCodeLocationKind.Stack, sarifError);

                    // TODO need new code location kind
                    //CaptureAnnotatedCodeLocations(result.RelatedLocations, AnnotatedCodeLocationKind.Stack, sarifError);

                    if (region != null)
                    {
                        sarifError.ColumnNumber = region.StartColumn - 1;
                        sarifError.LineNumber = region.StartLine - 1;
                    }

                    sarifErrors.Add(sarifError);
                }

                // TODO zap this on implementing todo above
                if (result.RelatedLocations != null)
                {
                    foreach (AnnotatedCodeLocation annotation in result.RelatedLocations)
                    {
                        PhysicalLocationComponent physicalLocation = annotation.PhysicalLocation[0];
                        region = physicalLocation.Region;
                        shortMessage = annotation.Message;
                        document = annotation.PhysicalLocation[0].Uri.LocalPath;

                        if (!this.documentToLineIndexMap.TryGetValue(document, out newLineIndex))
                        {
                            this.documentToLineIndexMap[document] = newLineIndex = new NewLineIndex(File.ReadAllText(document));
                        }

                        if (region != null)
                        {
                            region.Populate(newLineIndex);
                        }

                        SarifError sarifError = new SarifError(document)
                        {
                            Region = region,
                            RuleId = result.RuleId,
                            RuleName = rule?.Name,
                            Kind = ResultKind.Note,
                            Category = "Related Location", // or should we prefer original result category?
                            ShortMessage = shortMessage,
                            FullMessage = null,
                            MimeType = physicalLocation?.MimeType,
                            Tool = toolName
                        };

                        if (region != null)
                        {
                            sarifError.ColumnNumber = region.StartColumn - 1;
                            sarifError.LineNumber = region.StartLine - 1;
                        }

                        sarifErrors.Add(sarifError);
                    }
                }

                CodeAnalysisResultManager.Instance.SarifErrors = sarifErrors;
                SarifTableDataSource.Instance.AddErrors(sarifErrors);
            }
        }

        private static void CaptureAnnotatedCodeLocations(
            IEnumerable<IEnumerable<AnnotatedCodeLocation>> codeLocationCollections, 
            AnnotatedCodeLocationKind annotatedCodeLocationKind,
            SarifError sarifError)
        {
            if (codeLocationCollections == null) { return; }

            int annotationCollectionCount = 0;

            foreach (IEnumerable<AnnotatedCodeLocation> codeLocations in codeLocationCollections)
            {
                foreach (AnnotatedCodeLocation codeLocation in codeLocations)
                {
                    PhysicalLocationComponent plc = codeLocation.PhysicalLocation.Last();
                    sarifError.Annotations.Add(new AnnotatedCodeLocationModel()
                    {
                        Index = annotationCollectionCount,
                        Kind = annotatedCodeLocationKind,
                        Region = plc.Region,
                        FilePath = plc.Uri.LocalPath,
                        Message = codeLocation.Message
                    });
                }
                annotationCollectionCount++;
            }
        }

        private static bool IsError(ResultKind kind)
        {
            return 
                kind == ResultKind.ConfigurationError ||
                kind == ResultKind.Error ||
                kind == ResultKind.InternalError;
        }
    }
}