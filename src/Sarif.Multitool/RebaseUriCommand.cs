// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis.Sarif.Processors;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    internal static class RebaseUriCommand
    {
        public static int Run(RebaseUriOptions rebaseOptions)
        {
            try
            {
                Uri baseUri;
                if (!Uri.TryCreate(rebaseOptions.BasePath, UriKind.Absolute, out baseUri))
                {
                    throw new ArgumentException($"BasePath {rebaseOptions.BasePath} was not an absolute URI.  It must be.");
                }

                // In case someone accidentally passes C:\bld\src and meant C:\bld\src\--the base path should always be a folder, not something that points to a file.
                if (!string.IsNullOrEmpty(baseUri.GetFileName()))
                {
                    baseUri = new Uri(baseUri.ToString() + "/");
                }

                var sarifFiles = GetSarifFiles(rebaseOptions);

                Directory.CreateDirectory(rebaseOptions.OutputFolderPath);
                foreach (var sarifLog in sarifFiles)
                {
                    sarifLog.Log = sarifLog.Log.RebaseUri(rebaseOptions.BasePathToken, rebaseOptions.RebaseRelativeUris, baseUri);

                    // Write output to file.
                    string outputName = sarifLog.GetOutputFileName(rebaseOptions);
                    var formatting = rebaseOptions.PrettyPrint
                        ? Formatting.Indented
                        : Formatting.None;
                    
                    MultitoolFileHelpers.WriteSarifFile(sarifLog.Log, outputName, formatting);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return 1;
            }

            return 0;
        }
        
        private static IEnumerable<RebaseUriFile> GetSarifFiles(RebaseUriOptions mergeOptions)
        {
            // Get files names first, as we may write more sarif logs to the same directory as we rebase them.
            HashSet<string> fileNames = MultitoolFileHelpers.CreateTargetsSet(mergeOptions.TargetFileSpecifiers, mergeOptions.Recurse);
            foreach(var file in fileNames)
            {
                yield return new RebaseUriFile() { FileName = file, Log = MultitoolFileHelpers.ReadSarifFile<SarifLog>(file) };
            }
        }
        
        private class RebaseUriFile
        {
            public string FileName;

            public SarifLog Log;
            
            internal string GetOutputFileName(RebaseUriOptions mergeOptions)
            {
                return !string.IsNullOrEmpty(mergeOptions.OutputFolderPath)
                    ? Path.GetFullPath(mergeOptions.OutputFolderPath) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(FileName) + "-rebased.sarif"
                    : Path.GetDirectoryName(FileName) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(FileName) + "-rebased.sarif";
            }
        }
    }
}
