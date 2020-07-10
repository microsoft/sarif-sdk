// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching;
using Microsoft.CodeAnalysis.Sarif.Driver;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class ResultMatchSetCommand : CommandBase
    {
        private readonly IFileSystem _fileSystem;

        public ResultMatchSetCommand(IFileSystem fileSystem = null)
        {
            _fileSystem = fileSystem ?? new FileSystem();
        }

        public int Run(ResultMatchSetOptions options)
        {
            int returnCode = SUCCESS;

            options.OutputFolderPath = options.OutputFolderPath ?? Path.Combine(options.FolderPath, "Out");

            ISarifLogMatcher matcher = ResultMatchingBaselinerFactory.GetDefaultResultMatchingBaseliner();
            Formatting formatting = options.PrettyPrint ? Formatting.Indented : Formatting.None;

            // Remove previous results.
            if (_fileSystem.DirectoryExists(options.OutputFolderPath) && options.Force)
            {
                _fileSystem.DeleteDirectory(options.OutputFolderPath, true);
            }

            // Create output folder.
            _fileSystem.CreateDirectory(options.OutputFolderPath);

            string previousFileName = "";
            string previousGroup = "";
            SarifLog previousLog = null, currentLog = null;

            foreach (string filePath in Directory.GetFiles(options.FolderPath, "*.sarif"))
            {
                string fileName = Path.GetFileName(filePath);
                string currentGroup = GetGroupName(fileName);

                try
                {
                    currentLog = ReadSarifFile<SarifLog>(_fileSystem, filePath);

                    // Compare each log with the previous one in the same group.
                    if (currentGroup.Equals(previousGroup) && currentLog?.Runs?[0]?.Results.Count != 0 && previousLog?.Runs?[0]?.Results.Count != 0)
                    {
                        Console.WriteLine();
                        Console.WriteLine($"{previousFileName} -> {fileName}:");
                        SarifLog mergedLog = matcher.Match(new[] { previousLog }, new[] { currentLog }).First();

                        // Write the same and different count and different IDs.
                        WriteDifferences(mergedLog);

                        // Write the log, if there were any changed results
                        if (mergedLog.Runs[0].Results.Any(r => r.BaselineState != BaselineState.Unchanged))
                        {
                            string outputFilePath = Path.Combine(options.OutputFolderPath, fileName);

                            if (DriverUtilities.ReportWhetherOutputFileCanBeCreated(outputFilePath, options.Force, _fileSystem))
                            {
                                WriteSarifFile(_fileSystem, mergedLog, outputFilePath, formatting);
                            }
                            else
                            {
                                returnCode = FAILURE;
                            }
                        }
                    }
                }
                catch (Exception ex) when (!Debugger.IsAttached)
                {
                    Console.WriteLine(ex.ToString());
                    returnCode = FAILURE;
                }

                previousFileName = fileName;
                previousGroup = currentGroup;
                previousLog = currentLog;
            }

            return returnCode;
        }

        private static string GetGroupName(string fileName)
        {
            int dashIndex = fileName.IndexOf('-');
            if (dashIndex < 1)
            {
                throw new InvalidOperationException(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        MultitoolResources.ErrorNoGroupInFileName,
                        fileName));
            }

            return fileName.Substring(0, dashIndex);
        }

        private static void WriteDifferences(SarifLog mergedLog)
        {
            List<Result> differentResults = mergedLog.Runs[0].Results.Where(r => r.BaselineState != BaselineState.Unchanged).ToList();
            Console.WriteLine(
                string.Format(
                    CultureInfo.CurrentCulture,
                    MultitoolResources.ResultDifferenceSummary,
                    mergedLog.Runs[0].Results.Count - differentResults.Count,
                    differentResults.Count));

            foreach (Result result in differentResults)
            {
                Console.WriteLine($"  {ChangedSymbol(result.BaselineState)} {Identifier(result)}");
            }
        }

        private static string ChangedSymbol(BaselineState state)
        {
            switch (state)
            {
                case BaselineState.Unchanged:
                    return "=";
                case BaselineState.New:
                    return "+";
                case BaselineState.Absent:
                    return "-";
                case BaselineState.Updated:
                    return "~";
                case BaselineState.None:
                    return " ";
                default:
                    throw new NotImplementedException($"ChangedSymbol not implemented for {state}.");
            }
        }

        private static string Identifier(Result result)
        {
            return result.Guid ?? result.Fingerprints?.FirstOrDefault().Value ?? result.PartialFingerprints?.FirstOrDefault().Value ?? "";
        }
    }
}
