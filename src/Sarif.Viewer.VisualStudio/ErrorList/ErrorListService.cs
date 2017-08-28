// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using System.Collections.Generic;
using System.IO;
using System.Text;

using EnvDTE;

using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.Converters;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Microsoft.CodeAnalysis.Sarif.Writers;

using Newtonsoft.Json;

namespace Microsoft.Sarif.Viewer.ErrorList
{
    public class ErrorListService
    {
        public static readonly ErrorListService Instance = new ErrorListService();

        public static void ProcessLogFile(string filePath, Solution solution, string toolFormat = ToolFormat.None)
        {
            SarifLog log;

            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                ContractResolver = SarifContractResolver.Instance,
            };

            string logText;

            if (toolFormat.MatchesToolFormat(ToolFormat.None))
            {
                logText = File.ReadAllText(filePath);
            }
            else if (toolFormat.MatchesToolFormat(ToolFormat.PREfast))
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
            ProcessSarifLog(log, filePath, solution);
        }

        private static void ProcessSarifLog(SarifLog sarifLog, string logFilePath, Solution solution)
        {
            // Clear previous data
            SarifTableDataSource.Instance.CleanAllErrors();
            CodeAnalysisResultManager.Instance.SarifErrors.Clear();

            foreach (Run run in sarifLog.Runs)
            {
                Instance.WriteRunToErrorList(run, logFilePath, solution);
            }

            SarifTableDataSource.Instance.BringToFront();
        }

        private ErrorListService()
        {
        }

        private void WriteRunToErrorList(Run run, string logFilePath, Solution solution)
        {
            List<SarifErrorListItem> sarifErrors = new List<SarifErrorListItem>();

            var projectNameCache = new ProjectNameCache(solution);

            if (run.Results != null)
            {
                foreach (Result result in run.Results)
                {
                    var sarifError = new SarifErrorListItem(run, result, logFilePath, projectNameCache);
                    sarifErrors.Add(sarifError);
                }
            }

            foreach (var error in sarifErrors)
            {
                CodeAnalysisResultManager.Instance.SarifErrors.Add(error);
            }
            SarifTableDataSource.Instance.AddErrors(sarifErrors);
        }
    }
}