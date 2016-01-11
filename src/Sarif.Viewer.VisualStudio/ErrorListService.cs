// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.CodeAnalysis.Sarif.Driver;
using Microsoft.CodeAnalysis.Sarif.Driver.Sdk;
using Microsoft.CodeAnalysis.Sarif.Sdk;

namespace SarifViewer
{
    public class ErrorListService
    {
        public static ErrorListService Instance = new ErrorListService();

        public static void ProcessSarifLog(ResultLog resultLog)
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
                string message;
                string document;
                Region region;
                NewLineIndex newLineIndex;

                foreach (Location location in result.Locations)
                {
                    PhysicalLocationComponent physicalLocation;
                    if (location.ResultFile != null)
                    {
                        physicalLocation = location.ResultFile[0];
                    }
                    else
                    {
                        physicalLocation = location.AnalysisTarget[0];
                    }

                    // TODO helper to provide best representation of URI
                    document = physicalLocation.Uri.LocalPath;

                    // TODO retrieve file from nested component
                    region = physicalLocation.Region;

                    IRuleDescriptor rule = GetRule(runLog, result.RuleId);
                    message = result.GetMessageText(rule, concise: true);

                    SarifError sarifError = new SarifError(document)
                    {
                        Region = region,
                        ErrorCode = result.RuleId,
                        IsError = IsError(result.Kind),
                        Message = message,
                        MimeType = physicalLocation.MimeType,
                        Tool = toolName
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
                        PhysicalLocationComponent physicalLocation = annotation.PhysicalLocation[0];
                        region = physicalLocation.Region;
                        message = annotation.Message;
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
                            ErrorCode = result.RuleId,
                            IsError = IsError(result.Kind),
                            Message = message,
                            MimeType = physicalLocation.MimeType,
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

        private bool IsError(ResultKind kind)
        {
            return 
                kind == ResultKind.ConfigurationError ||
                kind == ResultKind.Error ||
                kind == ResultKind.InternalError;
        }
    }
}