// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using EnvDTE;

using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.Converters;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Microsoft.CodeAnalysis.Sarif.Writers;
using Microsoft.Sarif.Viewer.Models;
using Newtonsoft.Json;
using Microsoft.Sarif.Viewer.Sarif;

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

        internal static void ProcessSarifLog(SarifLog sarifLog, string logFilePath, Solution solution)
        {
            // Clear previous data
            SarifTableDataSource.Instance.CleanAllErrors();
            CodeAnalysisResultManager.Instance.SarifErrors.Clear();
            CodeAnalysisResultManager.Instance.FileDetails.Clear();

            foreach (Run run in sarifLog.Runs)
            {
                Instance.WriteRunToErrorList(run, logFilePath, solution);
            }
        }

        private ErrorListService()
        {
        }

        private void WriteRunToErrorList(Run run, string logFilePath, Solution solution)
        {
            List<SarifErrorListItem> sarifErrors = new List<SarifErrorListItem>();
            var projectNameCache = new ProjectNameCache(solution);

            StoreFileDetails(run.Files);

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

        private void EnsureHashExists(FileData file)
        {
            if (file.Hashes == null)
            {
                file.Hashes = new List<Hash>();
            }
            
            var hasSha256Hash = file.Hashes.Any(x => x.Algorithm == AlgorithmKind.Sha256);
            if (!hasSha256Hash)
            {
                string hashString = GenerateHash(file.Contents);

                file.Hashes.Add(new Hash(hashString, AlgorithmKind.Sha256));
            }
        }

        internal string GenerateHash(string content)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(content);
            SHA256Managed hashstring = new SHA256Managed();
            byte[] hash = hashstring.ComputeHash(bytes);
            return hash.Aggregate(string.Empty, (current, x) => current + $"{x:x2}");
        }
      
        private void StoreFileDetails(IDictionary<string, FileData> files)
        {
            if (files == null)
            {
                return;
            }

            foreach (var file in files)
            {
                Uri key;
                var isValid = Uri.TryCreate(file.Key, UriKind.RelativeOrAbsolute, out key);

                if (!isValid)
                {
                    continue;
                }

                var contents = file.Value.Contents;
                if (contents != null)
                {
                    EnsureHashExists(file.Value);
                    var fileDetails = new FileDetailsModel(file.Value);
                    CodeAnalysisResultManager.Instance.FileDetails.Add(key.ToPath(), fileDetails);
                }
            }
        }
    }
}