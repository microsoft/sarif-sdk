// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis.Sarif.Driver;
using Microsoft.CodeAnalysis.Sarif.Processors;
using Microsoft.CodeAnalysis.Sarif.Visitors;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class MergeCommand : CommandBase
    {
        private readonly IFileSystem _fileSystem;

        public MergeCommand(IFileSystem fileSystem = null)
        {
            _fileSystem = fileSystem ?? new FileSystem();
        }

        public int Run(MergeOptions mergeOptions)
        {
            try
            {
                string outputDirectory = mergeOptions.OutputFolderPath ?? Environment.CurrentDirectory;
                string outputFilePath = Path.Combine(outputDirectory, GetOutputFileName(mergeOptions));

                if (!DriverUtilities.ReportWhetherOutputFileCanBeCreated(outputFilePath, mergeOptions.Force, _fileSystem))
                {
                    return FAILURE;
                }

                HashSet<string> sarifFiles = CreateTargetsSet(mergeOptions.TargetFileSpecifiers, mergeOptions.Recurse, _fileSystem);

                IEnumerable<SarifLog> allRuns = ParseFiles(sarifFiles);

                // Build one SarifLog with all the Runs.
                SarifLog combinedLog = allRuns.Merge();

                // If there were no input files, the Merge operation set combinedLog.Runs to null. Although
                // null is valid in certain error cases, it is not valid here. Here, the correct value is
                // an empty list. See the SARIF spec, §3.13.4, "runs property".
                combinedLog.Runs = combinedLog.Runs ?? new List<Run>();

                combinedLog.Version = SarifVersion.Current;
                combinedLog.SchemaUri = combinedLog.Version.ConvertToSchemaUri();

                OptionallyEmittedData dataToInsert = mergeOptions.DataToInsert.ToFlags();

                if (dataToInsert != OptionallyEmittedData.None)
                {
                    combinedLog = new InsertOptionalDataVisitor(dataToInsert).VisitSarifLog(combinedLog);
                }

                // Write output to file.
                Formatting formatting = mergeOptions.PrettyPrint
                    ? Formatting.Indented
                    : Formatting.None;

                _fileSystem.CreateDirectory(outputDirectory);

                WriteSarifFile(_fileSystem, combinedLog, outputFilePath, formatting);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return FAILURE;
            }
            return SUCCESS;
        }

        private IEnumerable<SarifLog> ParseFiles(IEnumerable<string> sarifFiles)
        {
            foreach (string file in sarifFiles)
            {
                yield return ReadSarifFile<SarifLog>(_fileSystem, file);
            }
        }

        internal static string GetOutputFileName(MergeOptions mergeOptions)
        {
            return string.IsNullOrEmpty(mergeOptions.OutputFileName) == false
                ? mergeOptions.OutputFileName
                : "combined.sarif";
        }
    }
}
