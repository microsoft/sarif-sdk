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
    internal static class AbsoluteUriCommand
    {
        public static int Run(AbsoluteUriOptions absoluteUriOptions)
        {
            var sarifFiles = GetSarifFiles(absoluteUriOptions);

            foreach (var sarifLog in sarifFiles)
            {
                sarifLog.Log = sarifLog.Log.MakeUrisAbsolute();

                // Write output to file.
                string outputName = sarifLog.GetOutputFileName(absoluteUriOptions);
                var formatting = absoluteUriOptions.PrettyPrint
                    ? Newtonsoft.Json.Formatting.Indented
                    : Newtonsoft.Json.Formatting.None;

                MultitoolFileHelpers.WriteSarifFile(sarifLog.Log, outputName, formatting);
            }

            return 0;
        }

        private static IEnumerable<AbsoluteUriFile> GetSarifFiles(AbsoluteUriOptions absoluteUriOptions)
        {
            SearchOption searchOption = absoluteUriOptions.Recurse
                ? SearchOption.AllDirectories
                : SearchOption.TopDirectoryOnly;
            // Get files names first, as we may write more sarif logs to the same directory as we rebase them.
            List<string> fileNames = new List<string>();
            foreach (string path in absoluteUriOptions.Files)
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
            foreach (var file in fileNames)
            {
                yield return new AbsoluteUriFile() { FileName = file, Log = MultitoolFileHelpers.ReadSarifFile(file) };
            }
        }

        private class AbsoluteUriFile
        {
            public string FileName;

            public SarifLog Log;

            internal string GetOutputFileName(AbsoluteUriOptions mergeOptions)
            {
                return !string.IsNullOrEmpty(mergeOptions.OutputFilePath)
                    ? Path.GetFullPath(mergeOptions.OutputFilePath) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(FileName) + "-rebased.sarif"
                    : Path.GetDirectoryName(FileName) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(FileName) + "-rebased.sarif";
            }
        }
    }
}
