// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;

using Microsoft.CodeAnalysis.Sarif.Driver;
using Microsoft.CodeAnalysis.Sarif.Visitors;
using Microsoft.CodeAnalysis.Sarif.Writers;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class PartitionCommand : CommandBase
    {
        private readonly IFileSystem _fileSystem;

        public PartitionCommand(IFileSystem fileSystem = null)
        {
            _fileSystem = fileSystem ?? Sarif.FileSystem.Instance;
        }

        public int Run(PartitionOptions options)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                if (!ValidateOptions(options))
                {
                    return FAILURE;
                }

                string outputDirectory = string.IsNullOrEmpty(options.OutputDirectoryPath)
                    ? Environment.CurrentDirectory
                    : options.OutputDirectoryPath;

                Console.WriteLine($"Partitioning '{options.InputFilePath}' using strategy '{options.SplittingStrategy}'...");

                SarifLog inputLog = ReadSarifFile<SarifLog>(_fileSystem, options.InputFilePath);

                PartitionFunction<string> partitionFunction = options.SplittingStrategy == SplittingStrategy.PerIndexList
                    ? PartitionFunctions.ForIndexList(
                          inputLog,
                          options.Indices,
                          string.IsNullOrEmpty(options.SpilloverBucket) ? null : options.SpilloverBucket,
                          options.StrictCoverage)
                    : PartitionFunctions.ForStrategy(inputLog, options.SplittingStrategy);

                IDictionary<string, SarifLog> partitions = SarifPartitioner.Partition(
                    inputLog,
                    partitionFunction,
                    deepClone: true);

                _fileSystem.DirectoryCreateDirectory(outputDirectory);

                int written = 0;
                foreach (KeyValuePair<string, SarifLog> entry in partitions)
                {
                    string fileName = BuildOutputFileName(options.OutputFilePrefix, entry.Key);
                    string outputPath = Path.Combine(outputDirectory, fileName);

                    if (!options.ForceOverwrite && _fileSystem.FileExists(outputPath))
                    {
                        Console.Error.WriteLine(
                            $"Output file '{outputPath}' already exists. Pass --log ForceOverwrite to overwrite.");
                        return FAILURE;
                    }

                    SarifLog partitionLog = entry.Value;
                    partitionLog.Version = SarifVersion.Current;
                    partitionLog.SchemaUri = partitionLog.Version.ConvertToSchemaUri();

                    WriteSarifFile(_fileSystem, partitionLog, outputPath, options.Minify);
                    written++;
                }

                Console.WriteLine($"Wrote {written.ToString(CultureInfo.InvariantCulture)} partition file(s) to '{outputDirectory}'.");
                return SUCCESS;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return FAILURE;
            }
            finally
            {
                Console.WriteLine($"Partition completed in {stopwatch.Elapsed}.");
            }
        }

        internal static string BuildOutputFileName(string prefix, string partitionKey)
        {
            string safePrefix = string.IsNullOrEmpty(prefix) ? "partition" : prefix;
            string safeKey = string.IsNullOrEmpty(partitionKey) ? "default" : partitionKey;
            return ($"{safePrefix}_{safeKey}.sarif").ReplaceInvalidCharInFileName(".");
        }

        private static bool ValidateOptions(PartitionOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.InputFilePath))
            {
                Console.Error.WriteLine("An input SARIF file is required.");
                return false;
            }

            if (options.SplittingStrategy == SplittingStrategy.None)
            {
                Console.Error.WriteLine(
                    "Splitting strategy 'None' would produce a single output identical to the input. " +
                    "Pick PerRule, PerRunPerRule, PerRunPerTarget, PerRunPerTargetPerRule, PerRun, PerResult, or PerIndexList.");
                return false;
            }

            if (options.SplittingStrategy == SplittingStrategy.PerIndexList
                && string.IsNullOrWhiteSpace(options.Indices))
            {
                Console.Error.WriteLine("--indices is required when --strategy=PerIndexList.");
                return false;
            }

            if (options.SplittingStrategy != SplittingStrategy.PerIndexList
                && !string.IsNullOrWhiteSpace(options.Indices))
            {
                Console.Error.WriteLine("--indices is only valid when --strategy=PerIndexList.");
                return false;
            }

            if (options.SplittingStrategy != SplittingStrategy.PerIndexList && options.StrictCoverage)
            {
                Console.Error.WriteLine("--strict-coverage is only valid when --strategy=PerIndexList.");
                return false;
            }

            if (options.SplittingStrategy != SplittingStrategy.PerIndexList && !string.IsNullOrEmpty(options.SpilloverBucket))
            {
                Console.Error.WriteLine("--spillover-bucket is only valid when --strategy=PerIndexList.");
                return false;
            }

            return true;
        }
    }
}
