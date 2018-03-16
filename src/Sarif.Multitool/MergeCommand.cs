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
            try
            {
                var sarifFiles = MultitoolFileHelpers.CreateTargetsSet(mergeOptions.TargetFileSpecifiers, mergeOptions.Recurse);

                var allRuns = ParseFiles(sarifFiles);

                // Build one SarifLog with all the Runs.
                SarifLog combinedLog = allRuns.Merge();

                // Reformat the SARIF log if we need to.
                LoggingOptions loggingOptions = mergeOptions.ConvertToLoggingOptions();
                SarifLog reformattedLog = new ReformattingVisitor(loggingOptions).VisitSarifLog(combinedLog);

                // Write output to file.
                string outputName = Path.Combine(mergeOptions.OutputFolderPath, GetOutputFileName(mergeOptions));
                
                var formatting = mergeOptions.PrettyPrint
                    ? Newtonsoft.Json.Formatting.Indented
                    : Newtonsoft.Json.Formatting.None;

                Directory.CreateDirectory(mergeOptions.OutputFolderPath);
                MultitoolFileHelpers.WriteSarifFile(reformattedLog, outputName, formatting);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return 1;
            }
            return 0;
        }

	    private static IEnumerable<SarifLog> ParseFiles(IEnumerable<string> sarifFiles)
	    {
            foreach (var file in sarifFiles)
            {
                yield return MultitoolFileHelpers.ReadSarifFile(file);
            }
        }
        
        internal static string GetOutputFileName(MergeOptions mergeOptions)
        {
            return !string.IsNullOrEmpty(mergeOptions.OutputFileName)
                ? mergeOptions.OutputFileName
                : "combined.sarif";
        }
    }
}
