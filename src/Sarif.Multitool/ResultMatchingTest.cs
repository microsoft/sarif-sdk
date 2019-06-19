// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    internal class ResultMatchingTestCommand : CommandBase
    {
        private readonly IFileSystem _fileSystem;

        public ResultMatchingTestCommand(IFileSystem fileSystem = null)
        {
            _fileSystem = fileSystem ?? new FileSystem();
        }

        public int Run(ResultMatchingTestOptions options)
        {
            options.OutputFolderPath = options.OutputFolderPath ?? Path.Combine(options.FolderPath, "Out");

            ISarifLogMatcher matcher = ResultMatchingBaselinerFactory.GetDefaultResultMatchingBaseliner();
            Formatting formatting = (options.PrettyPrint ? Formatting.Indented : Formatting.None);

            // Remove previous results
            if (Directory.Exists(options.OutputFolderPath))
            {
                Directory.Delete(options.OutputFolderPath, true);
            }

            // Create output folder
            Directory.CreateDirectory(options.OutputFolderPath);

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

                    // Compare each log with the previous one in the same group
                    if (currentGroup.Equals(previousGroup))
                    {
                        Console.WriteLine();
                        Console.WriteLine($"{previousFileName} -> {fileName}:");
                        SarifLog result = matcher.Match(new[] { previousLog }, new[] { currentLog }).First();

                        // Write the same and different count and different IDs
                        WriteDifferences(result);

                        // Write the log, if there were any changed results
                        if (result.Runs[0].Results.Any(r => r.BaselineState != BaselineState.Unchanged))
                        {
                            string outputFilePath = Path.Combine(options.OutputFolderPath, fileName);
                            WriteSarifFile(_fileSystem, result, outputFilePath, formatting);
                        }
                    }
                }
                catch (Exception ex) when (!Debugger.IsAttached)
                {
                    Console.WriteLine(ex.ToString());
                }
                
                previousFileName = fileName;
                previousGroup = currentGroup;
                previousLog = currentLog;
            }

            return 0;
        }

        private static string GetGroupName(string fileName)
        {
            int dashIndex = fileName.IndexOf('-');
            if (dashIndex < 1)
            {
                throw new InvalidOperationException($"Error: No group part (before dash) found in {fileName}. All files should be [Group]-[RunID] and each adjacent pair in the same group is compared. Stopping.");
            }

            return fileName.Substring(0, dashIndex);
        }

        private static void WriteDifferences(SarifLog mergedLog)
        {
            List<Result> differentResults = mergedLog.Runs[0].Results.Where(r => r.BaselineState != BaselineState.Unchanged).ToList();
            Console.WriteLine($"{mergedLog.Runs[0].Results.Count - differentResults.Count:n0} identical, {differentResults.Count:n0} changed");

            foreach (Result result in differentResults)
            {
                Console.WriteLine($"  {ChangedSymbol(result.BaselineState)} {Identifier(result)}");
            }
        }

        private static string ChangedSymbol(BaselineState state)
        {
            switch(state)
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
