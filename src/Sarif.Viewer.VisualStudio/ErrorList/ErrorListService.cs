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
using Microsoft.Sarif.Viewer.Sarif;

namespace Microsoft.Sarif.Viewer.ErrorList
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

        private void WriteRunToErrorList(Run run)
        {
            List<SarifErrorListItem> sarifErrors = new List<SarifErrorListItem>();

            foreach (Result result in run.Results)
            {
                SarifErrorListItem sarifError = GetResult(run, result);
                sarifErrors.Add(sarifError);
            }

            CodeAnalysisResultManager.Instance.SarifErrors = sarifErrors;
            SarifTableDataSource.Instance.AddErrors(sarifErrors);
        }

        private SarifErrorListItem GetResult(Run run, Result result)
        {
            SarifErrorListItem sarifError = new SarifErrorListItem(run, result);

            IEnumerable<IEnumerable<AnnotatedCodeLocation>> stackLocations = CreateAnnotatedCodeLocationsFromStacks(result.Stacks);
            IEnumerable<IEnumerable<AnnotatedCodeLocation>> codeFlowLocations = CreateAnnotatedCodeLocationsFromCodeFlows(result.CodeFlows);

            CreateAnnotatedCodeLocationCollections(stackLocations, AnnotatedCodeLocationKind.Stack, sarifError);
            CreateAnnotatedCodeLocationCollections(codeFlowLocations, AnnotatedCodeLocationKind.CodeFlow, sarifError);
            CaptureAnnotatedCodeLocations(result.RelatedLocations, AnnotatedCodeLocationKind.Stack, sarifError);

            return sarifError;
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
            SarifErrorListItem sarifError)
        {
            if (codeLocationCollections == null) { return; }

            foreach (IEnumerable<AnnotatedCodeLocation> codeLocations in codeLocationCollections)
            {
                CaptureAnnotatedCodeLocations(codeLocations, annotatedCodeLocationKind, sarifError);
            }
        }

        private static void CaptureAnnotatedCodeLocations(IEnumerable<AnnotatedCodeLocation> codeLocations, AnnotatedCodeLocationKind annotatedCodeLocationKind, SarifErrorListItem sarifError)
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