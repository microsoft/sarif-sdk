// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.Driver;
using Microsoft.CodeAnalysis.Sarif.Driver.Sdk;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Microsoft.CodeAnalysis.Sarif.Writers;

using Newtonsoft.Json;

namespace SarifViewer
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
                File.WriteAllText(@"d:\repros\prefast.test.sarif", logText);
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

            foreach (IRule rule in runLog.Rules)
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
                string category, document, shortMessage, fullMessage;
                Region region;
                NewLineIndex newLineIndex;
                IRule rule = null;

                category = null;

                if (result.Properties != null)
                {
                    result.Properties.TryGetValue("category", out category);
                }

                foreach (Location location in result.Locations)
                {
                    region = null;

                    PhysicalLocation physicalLocation = null;
                    if (location.ResultFile != null)
                    {
                        physicalLocation = location.ResultFile;
                        document = physicalLocation.Uri.LocalPath;
                        region = physicalLocation.Region;
                    }
                    else if (location.AnalysisTarget != null)
                    {
                        physicalLocation = location.AnalysisTarget;
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
                        RuleName = rule.Name,
                        Kind = result.Kind,
                        Category = category,
                        ShortMessage = shortMessage,
                        FullMessage = fullMessage,
                        Tool = toolName,
                        HelpLink = rule?.HelpUri?.ToString()                        
                    };

                    if (region != null)
                    {
                        sarifError.ColumnNumber = region.StartColumn - 1;
                        sarifError.LineNumber = region.StartLine - 1;
                    }

                    sarifErrors.Add(sarifError);
                }

                if (result.RelatedLocations != null)
                {
                    foreach (AnnotatedCodeLocation annotation in result.RelatedLocations)
                    {
                        PhysicalLocation physicalLocation = annotation.PhysicalLocation;
                        region = physicalLocation.Region;
                        shortMessage = annotation.Message;
                        document = annotation.PhysicalLocation.Uri.LocalPath;

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

                SarifTableDataSource.Instance.AddErrors(sarifErrors);
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