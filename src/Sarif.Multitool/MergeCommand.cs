﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis.Sarif.Processors;
using Microsoft.CodeAnalysis.Sarif.Visitors;
using Newtonsoft.Json;

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
                combinedLog.Version = SarifVersion.TwoZeroZero;
                combinedLog.SchemaUri = combinedLog.Version.ConvertToSchemaUri();

                OptionallyEmittedData dataToInsert = mergeOptions.DataToInsert.ToFlags();

                if (dataToInsert != OptionallyEmittedData.None)
                {
                    combinedLog = new InsertOptionalDataVisitor(dataToInsert).VisitSarifLog(combinedLog);
                }

                // Write output to file.
                string outputName = Path.Combine(mergeOptions.OutputFolderPath, GetOutputFileName(mergeOptions));
                
                var formatting = mergeOptions.PrettyPrint
                    ? Formatting.Indented
                    : Formatting.None;

                Directory.CreateDirectory(mergeOptions.OutputFolderPath);
                MultitoolFileHelpers.WriteSarifFile(combinedLog, outputName, formatting);
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
                yield return MultitoolFileHelpers.ReadSarifFile<SarifLog>(file);
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
