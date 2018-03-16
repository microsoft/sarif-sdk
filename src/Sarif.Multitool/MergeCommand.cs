// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis.Sarif.Processors;
using Microsoft.CodeAnalysis.Sarif.Visitors;
using Microsoft.CodeAnalysis.Sarif.Writers;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    internal static class MergeCommand
    {
        public static int Run(MergeOptions mergeOptions)
        {
	        var sarifFiles = GetSarifFiles(mergeOptions);

	        var allRuns = ParseFiles(sarifFiles);

            // Build one SarifLog with all the Runs.
            SarifLog combinedLog = allRuns.Merge();
            
            // Reformat the SARIF log if we need to.
            LoggingOptions loggingOptions = mergeOptions.ConvertToLoggingOptions();
            SarifLog reformattedLog = new ReformattingVisitor(loggingOptions).VisitSarifLog(combinedLog);
            
            // Write output to file.
            string outputName = GetOutputFileName(mergeOptions);
            var formatting = mergeOptions.PrettyPrint
                ? Newtonsoft.Json.Formatting.Indented
                : Newtonsoft.Json.Formatting.None;

            MultitoolFileHelpers.WriteSarifFile(reformattedLog, outputName, formatting);

            return 0;
        }

	    private static IEnumerable<SarifLog> ParseFiles(IEnumerable<string> sarifFiles)
	    {
            foreach (var file in sarifFiles)
            {
                yield return MultitoolFileHelpers.ReadSarifFile(file);
            }
        }

        private static IEnumerable<string> GetSarifFiles(MergeOptions mergeOptions)
        {
            SearchOption searchOption = mergeOptions.Recurse
                ? SearchOption.AllDirectories
                : SearchOption.TopDirectoryOnly;

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
                        yield return file;
                    }
                }
            }
        }

        internal static string GetOutputFileName(MergeOptions mergeOptions)
        {
            return !string.IsNullOrEmpty(mergeOptions.OutputFilePath)
                ? mergeOptions.OutputFilePath 
                : "combined.sarif";
        }
    }
}
