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
                Console.WriteLine($"BasePath {rebaseOptions.BasePath} was not an absolute URI.  It must be.");
                return -1;
            }

            // In case someone accidentally passes C:\bld\src and meant C:\bld\src\--the base path should always be a folder, not something that points to a file.
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

                MultitoolFileHelpers.WriteSarifFile(sarifLog.Log, outputName, formatting);
            }

            return 0;
        }
        
        private static IEnumerable<RebaseUriFile> GetSarifFiles(RebaseUriOptions mergeOptions)
        {
            SearchOption searchOption = mergeOptions.Recurse
                ? SearchOption.AllDirectories
                : SearchOption.TopDirectoryOnly;
            // Get files names first, as we may write more sarif logs to the same directory as we rebase them.
            List<string> fileNames = new List<string>();
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
                        fileNames.Add(file);
                    }
                }
            }
            foreach(var file in fileNames)
            {
                yield return new RebaseUriFile() { FileName = file, Log = MultitoolFileHelpers.ReadSarifFile(file) };
            }
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
