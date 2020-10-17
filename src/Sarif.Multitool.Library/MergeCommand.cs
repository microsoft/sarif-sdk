// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.CodeAnalysis.Sarif.Driver;
using Microsoft.CodeAnalysis.Sarif.Processors;
using Microsoft.CodeAnalysis.Sarif.Writers;

using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class MergeCommand : CommandBase
    {
        private readonly IFileSystem _fileSystem;

        public MergeCommand(IFileSystem fileSystem = null)
        {
            _fileSystem = fileSystem ?? FileSystem.Instance;
        }

        public int Run(MergeOptions mergeOptions)
        {
            try
            {
                string outputDirectory = mergeOptions.OutputDirectoryPath ?? Environment.CurrentDirectory;
                string outputFilePath = Path.Combine(outputDirectory, GetOutputFileName(mergeOptions));

                if (mergeOptions.SplittingStrategy == 0)
                {
                    if (!DriverUtilities.ReportWhetherOutputFileCanBeCreated(outputFilePath, mergeOptions.Force, _fileSystem))
                    {
                        return FAILURE;
                    }
                }

                HashSet<string> sarifFiles = CreateTargetsSet(mergeOptions.TargetFileSpecifiers, mergeOptions.Recurse, _fileSystem);

                IEnumerable<SarifLog> allRuns = ParseFiles(sarifFiles);

                // Build one SarifLog with all the Runs.
                SarifLog mergedLog = allRuns
                    .Merge(mergeOptions.MergeEmptyLogs)
                    .InsertOptionalData(mergeOptions.DataToInsert.ToFlags())
                    .RemoveOptionalData(mergeOptions.DataToInsert.ToFlags());

                // If there were no input files, the Merge operation set combinedLog.Runs to null. Although
                // null is valid in certain error cases, it is not valid here. Here, the correct value is
                // an empty list. See the SARIF spec, §3.13.4, "runs property".
                mergedLog.Runs ??= new List<Run>();
                mergedLog.Version = SarifVersion.Current;
                mergedLog.SchemaUri = mergedLog.Version.ConvertToSchemaUri();

                if (mergeOptions.SplittingStrategy != SplittingStrategy.PerRule)
                {
                    _fileSystem.DirectoryCreate(outputDirectory);

                    // Write output to file.
                    WriteSarifFile(_fileSystem, mergedLog, outputFilePath, mergeOptions.Formatting);
                    return 0;
                }

                var ruleToRunsMap = new Dictionary<string, HashSet<Run>>();

                foreach (Run run in mergedLog.Runs)
                {
                    IList<Result> cachedResults = run.Results;

                    run.Results = null;

                    if (mergeOptions.MergeRuns)
                    {
                        run.Tool.Driver.Rules = null;
                        run.Artifacts = null;
                        run.Invocations = null;
                    }

                    Run emptyRun = run.DeepClone();
                    run.Results = cachedResults;

                    var idToRunMap = new Dictionary<string, Run>();

                    if (run.Results != null)
                    {
                        foreach (Result result in run.Results)
                        {
                            if (!idToRunMap.TryGetValue(result.RuleId, out Run splitRun))
                            {
                                splitRun = idToRunMap[result.RuleId] = emptyRun.DeepClone();
                            }
                            splitRun.Results ??= new List<Result>();

                            if (!ruleToRunsMap.TryGetValue(result.RuleId, out HashSet<Run> runs))
                            {
                                IEqualityComparer<Run> comparer = Microsoft.CodeAnalysis.Sarif.Run.ValueComparer;
                                runs = ruleToRunsMap[result.RuleId] = new HashSet<Run>(comparer);
                            }
                            runs.Add(splitRun);
                        }
                    }
                }

                foreach (string ruleId in ruleToRunsMap.Keys)
                {
                    HashSet<Run> runs = ruleToRunsMap[ruleId];
                    var perRuleLog = new SarifLog
                    {
                        Runs = new List<Run>(runs)
                    };

                    if (mergeOptions.MergeRuns)
                    {
                        new FixupVisitor().VisitSarifLog(perRuleLog);
                    }

                    _fileSystem.DirectoryCreate(outputDirectory);

                    outputFilePath = Path.Combine(outputDirectory, GetOutputFileName(mergeOptions, ruleId));

                    if (!DriverUtilities.ReportWhetherOutputFileCanBeCreated(outputFilePath, mergeOptions.Force, _fileSystem))
                    {
                        return FAILURE;
                    }

                    WriteSarifFile(_fileSystem, perRuleLog, outputFilePath, mergeOptions.Formatting);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return FAILURE;
            }
            return SUCCESS;
        }

        private IEnumerable<SarifLog> SplitLogs(SplittingStrategy perRule)
        {
            throw new NotImplementedException();
        }

        private IEnumerable<SarifLog> ParseFiles(IEnumerable<string> sarifFiles)
        {
            foreach (string file in sarifFiles)
            {
                yield return PrereleaseCompatibilityTransformer.UpdateToCurrentVersion(
                    File.ReadAllText(file),
                    formatting: Formatting.None,
                    out string sarifText);
            }
        }

        internal static string GetOutputFileName(MergeOptions mergeOptions, string prefix = null)
        {
            return string.IsNullOrEmpty(mergeOptions.OutputFileName) == false
                ? GetPrefix(prefix) + mergeOptions.OutputFileName
                : GetPrefix("merged.sarif");
        }

        private static string GetPrefix(string prefix)
        {
            if (prefix?.EndsWith("_") == false)
            {
                prefix += "_";
            }

            return prefix ?? "";
        }
    }

    internal class FixupVisitor : SarifRewritingVisitor
    {
        public override Result VisitResult(Result node)
        {
            node.RuleIndex = -1;
            return base.VisitResult(node);
        }

        public override ArtifactLocation VisitArtifactLocation(ArtifactLocation node)
        {
            node.Index = -1;
            return base.VisitArtifactLocation(node);
        }
    }
}
