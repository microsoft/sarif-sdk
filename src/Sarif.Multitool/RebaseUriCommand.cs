// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.Sarif.Processors;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Microsoft.CodeAnalysis.Sarif.Visitors;
using Microsoft.CodeAnalysis.Sarif.Writers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    internal static class RebaseUriCommand
    {
        public static int Run(RebaseUriOptions rebaseOptions)
        {
            Uri baseUri;
            if (!Uri.TryCreate(rebaseOptions.BasePath, UriKind.Absolute, out baseUri))
            {
                return -1;
            }

            if (baseUri.GetFileName() != "")
            {
                baseUri = new Uri(baseUri.ToString() + "/");
            }
            
            var sarifFiles = GetSarifFiles(rebaseOptions);
            
            foreach (var sarifLog in sarifFiles)
            {
                sarifLog.Log = sarifLog.Log.RebaseUri(rebaseOptions.BasePathName, baseUri);
                
                // Write output to file.
                string outputName = sarifLog.GetOutputFileName(rebaseOptions);
                var formatting = rebaseOptions.PrettyPrint
                    ? Newtonsoft.Json.Formatting.Indented
                    : Newtonsoft.Json.Formatting.None;
                var settings = new JsonSerializerSettings
                {
                    ContractResolver = SarifContractResolver.Instance,
                    Formatting = formatting
                };
                LoggingOptions loggingOptions = rebaseOptions.ConvertToLoggingOptions();
                File.WriteAllText(outputName, JsonConvert.SerializeObject(sarifLog.Log, settings));
            }

            return 0;
        }
        
        private static IEnumerable<RebaseUriFile> GetSarifFiles(RebaseUriOptions mergeOptions)
        {
            SearchOption searchOption = mergeOptions.Recurse
                ? SearchOption.AllDirectories
                : SearchOption.TopDirectoryOnly;

            List<RebaseUriFile> rebaseUris = new List<RebaseUriFile>();
            foreach (string path in mergeOptions.Files)
            {
                string directory, filename;
                if (Directory.Exists(path))
                {
                    directory = path;
                    filename = "*";
                }
                else
                {
                    directory = Path.GetDirectoryName(path) ?? path;
                    filename = Path.GetFileName(path) ?? "*";
                }
                foreach (string file in Directory.GetFiles(directory, filename, searchOption))
                {
                    if (file.EndsWith(".sarif", StringComparison.OrdinalIgnoreCase))
                    {
                        rebaseUris.Add(new RebaseUriFile() { FileName = file, Log = ReadFile(file) });
                    }
                }
            }
            return rebaseUris;
        }

        private static SarifLog ReadFile(string filePath)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                ContractResolver = SarifContractResolver.Instance
            };

            string logText = File.ReadAllText(filePath);

            return JsonConvert.DeserializeObject<SarifLog>(logText, settings);
        }


        private class RebaseUriFile
        {
            public string FileName;

            public SarifLog Log;
            
            internal string GetOutputFileName(RebaseUriOptions mergeOptions)
            {
                return !string.IsNullOrEmpty(mergeOptions.OutputFilePath)
                    ? Path.GetFullPath(mergeOptions.OutputFilePath) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(FileName) + "-rebased.sarif"
                    : Path.GetDirectoryName(FileName) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(FileName) + "-rebased.sarif";
            }

        }
    }
}
