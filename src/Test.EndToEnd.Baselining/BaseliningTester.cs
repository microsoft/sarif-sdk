using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching;
using SarifBaseline.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Test.EndToEnd.Baselining
{
    public class BaseliningTester
    {
        public const string InputFolderName = "Input";
        public const string FirstBaselineFileName = "Baseline.sarif";
        public const string SummaryFileName = "Summary.log";
        public const string OutputFolderName = "Output";
        public const string ExpectedFolderName = "Expected";
        public const string OutputDebugFolderName = "Output_Debug";
        public const string ExpectedDebugFolderName = "Expected_Debug";

        public BaseliningSummary RunAll(string rootPath)
        {
            BaseliningSummary overallSummary = RunUnder(Path.Combine(rootPath, InputFolderName));

            Directory.CreateDirectory(Path.Combine(rootPath, OutputFolderName));
            using (StreamWriter writer = File.CreateText(Path.Combine(rootPath, OutputFolderName, SummaryFileName)))
            {
                overallSummary.Write(writer);
            }

            return overallSummary;
        }

        public BaseliningSummary RunUnder(string folderPath)
        {
            BaseliningSummary folderSummary = new BaseliningSummary(Path.GetFileName(folderPath));

            // Recurse on subfolders, if found
            foreach (string subfolder in Directory.EnumerateDirectories(folderPath, "*", SearchOption.TopDirectoryOnly))
            {
                BaseliningSummary part = RunUnder(subfolder);
                folderSummary.AddComponent(part);
            }

            // Look for logs and sort in Ordinal order
            if (Directory.EnumerateFiles(folderPath).Any())
            {
                BaseliningSummary seriesSummary = RunSeries(folderPath);
                folderSummary.AddCounts(seriesSummary);
            }

            return folderSummary;
        }

        public BaseliningSummary RunSeries(string seriesPath, int debugLogIndex = -1, int debugResultIndex = -1)
        {
            string outputLogPath = Path.ChangeExtension(seriesPath.Replace($"\\{InputFolderName}\\", $"\\{OutputFolderName}\\"), ".log");
            BaseliningSummary seriesSummary = new BaseliningSummary(Path.GetFileName(seriesPath));

            Directory.CreateDirectory(Path.GetDirectoryName(outputLogPath));
            using (Stream outputStream = File.Create(outputLogPath))
            using (BaseliningDetailLogger logger = new BaseliningDetailLogger(seriesPath, outputStream))
            {
                // Load the original baseline
                SarifLog baseline = LoadBaseline(seriesPath);

                // Baseline each log in order
                foreach (LogInSeries current in LoadSeriesLogs(seriesPath))
                {
                    if (debugLogIndex == current.LogIndex && Debugger.IsAttached)
                    {
                        Debugger.Break();
                        DebugResultComparison(baseline, current.Log, $"{debugLogIndex:d3} {debugResultIndex:d3}");
                    }

                    SarifLog newBaseline = Baseline(baseline, current.Log);

                    BaseliningSummary fileSummary = new BaseliningSummary(Path.GetFileNameWithoutExtension(current.FilePath));
                    fileSummary.Add(newBaseline, baseline, current.Log);
                    seriesSummary.AddCounts(fileSummary);
                    logger.Write(newBaseline, baseline, current.Log, fileSummary);

                    baseline = newBaseline;
                }
            }

            EnrichSeries(seriesPath);

            string comparisonResult = CompareToExpected(outputLogPath);
            Console.WriteLine($"{comparisonResult}{seriesSummary.ToString(seriesPath)}");
            return seriesSummary;
        }

        public void EnrichUnder(string folderPath)
        {
            // Recurse on subfolders, if found
            foreach (string subfolder in Directory.EnumerateDirectories(folderPath, "*", SearchOption.TopDirectoryOnly))
            {
                EnrichUnder(subfolder);
            }

            // Look for logs and sort in Ordinal order
            if (Directory.EnumerateFiles(folderPath).Any())
            {
                EnrichSeries(folderPath);
            }
        }

        public void EnrichSeries(string seriesPath)
        {
            string outputLogPath = Path.ChangeExtension(seriesPath.Replace($"\\{InputFolderName}\\", $"\\{OutputFolderName}\\"), ".log");
            BaseliningDetailEnricher enricher = new BaseliningDetailEnricher();

            // Load Baseline details
            SarifLog baseline = LoadBaseline(seriesPath);
            enricher.AddLog(baseline);

            // Load series log details
            foreach (LogInSeries current in LoadSeriesLogs(seriesPath))
            {
                enricher.AddLog(current.Log);
            }

            // Write the enriched log for the current run
            string enrichedLogPath = Path.ChangeExtension(seriesPath.Replace($"\\{InputFolderName}\\", $"\\{OutputDebugFolderName}\\"), ".log");
            enricher.Convert(outputLogPath, enrichedLogPath);

            // Write the enriched log for the baseline run (if present)
            enricher.Convert(
                Path.ChangeExtension(seriesPath.Replace($"\\{InputFolderName}\\", $"\\{ExpectedFolderName}\\"), ".log"),
                Path.ChangeExtension(seriesPath.Replace($"\\{InputFolderName}\\", $"\\{ExpectedDebugFolderName}\\"), ".log"));
        }

        private SarifLog LoadBaseline(string seriesPath)
        {
            // Load the original baseline
            SarifLog baseline = null;
            string baselinePath = Path.Combine(seriesPath, FirstBaselineFileName);

            if (File.Exists(baselinePath))
            {
                using (Stream stream = File.OpenRead(Path.Combine(seriesPath, FirstBaselineFileName)))
                {
                    baseline = SarifLog.Load(stream);
                }
            }


            if (baseline == null) { baseline = new SarifLog(); }
            AssignResultRIDs(baseline, 0);
            return baseline;
        }

        private class LogInSeries
        {
            public SarifLog Log { get; set; }
            public int LogIndex { get; set; }
            public string FilePath { get; set; }

            public LogInSeries(SarifLog log, int logIndex, string filePath)
            {
                Log = log;
                LogIndex = logIndex;
                FilePath = filePath;
            }
        }

        private IEnumerable<LogInSeries> LoadSeriesLogs(string seriesPath)
        {
            // Baseline each log in order
            List<string> logs = new List<string>(Directory.EnumerateFiles(seriesPath, "*.sarif", SearchOption.TopDirectoryOnly).Where(filePath => !filePath.EndsWith(FirstBaselineFileName)).OrderBy(path => path));
            int logIndex = 1;

            foreach (string filePath in logs)
            {
                SarifLog current = null;
                using (Stream stream = File.OpenRead(filePath))
                {
                    current = SarifLog.Load(stream);
                }

                AssignResultRIDs(current, logIndex);
                yield return new LogInSeries(current, logIndex, filePath);

                logIndex++;
            }
        }

        public static void DebugResultComparison(SarifLog baseline, SarifLog current, string currentResultGuid)
        {
            string baselineResultGuid = "";
            bool similar = AreSufficientlySimiliar(baseline, baselineResultGuid, current, currentResultGuid);
        }

        public static bool AreSufficientlySimiliar(SarifLog baseline, string baselineResultGuid, SarifLog current, string currentResultGuid)
        {
            Result bResult = baseline.FindByGuid(baselineResultGuid);
            Result cResult = current.FindByGuid(currentResultGuid);

            if (bResult == null || cResult == null)
            {
                return false;
            }

            ExtractedResult bExtractedResult = new ExtractedResult(bResult, bResult.Run);
            ExtractedResult cExtractedResult = new ExtractedResult(cResult, cResult.Run);

            bool outcome = bExtractedResult.IsSufficientlySimilarTo(cExtractedResult);
            return outcome;
        }

        public static void AssignResultRIDs(SarifLog log, int logIndex)
        {
            // RIDs are ascending integers in sorted-for-baselining order
            SortForBaselining(log);

            // Set as the GUID and clear the CorrelationGuid so that
            // the RID for both matched Results can be found after baselining
            int resultIndex = 0;
            foreach (Result result in log.EnumerateResults())
            {
                result.Guid = $"{logIndex:d3} {resultIndex:d3}";
                result.CorrelationGuid = null;
                resultIndex++;
            }
        }

        public string CompareToExpected(string outputLogPath)
        {
            string expectedLogPath = outputLogPath.Replace($"\\{OutputFolderName}\\", $"\\{ExpectedFolderName}\\");

            if (!File.Exists(expectedLogPath))
            {
                return "";
            }

            string expectedLog = File.ReadAllText(expectedLogPath);
            string actualLog = File.ReadAllText(outputLogPath);

            if (expectedLog == actualLog)
            {
                return "IDENTICAL ";
            }
            else
            {
                return "DIFFERENT ";
            }
        }

        // --- FROM SarifBaseliner; find better integration story

        private static SarifLog Baseline(SarifLog baselineLog, SarifLog currentLog)
        {
            // Baseline the complete log
            ISarifLogMatcher matcher = ResultMatchingBaselinerFactory.GetDefaultResultMatchingBaseliner();
            SarifLog outputLog = matcher.Match(new[] { baselineLog }, new[] { currentLog }).First();

            // Is this an incremental scan?
            BaselineFilteringMode filteringMode = currentLog.GetBaselineFilteringMode();

            // Mark all Results which are NOT in the new run as 'Unchanged'
            if (filteringMode == BaselineFilteringMode.ToIncludedArtifacts)
            {
                HashSet<string> includedArtifacts = new HashSet<string>(currentLog.AllArtifactUris().Select(uri => uri.OriginalString));

                foreach (Result result in outputLog.EnumerateResults())
                {
                    if (!ContainsUriInSet(result, includedArtifacts))
                    {
                        result.BaselineState = BaselineState.Unchanged;
                    }
                }
            }

            // Ensure the Baseline is sorted for fast future baselining
            SortForBaselining(outputLog);

            return outputLog;
        }

        private static bool ContainsUriInSet(Result result, HashSet<string> set)
        {
            foreach (Uri uri in result.AllArtifactUris())
            {
                if (set.Contains(uri.OriginalString))
                {
                    return true;
                }
            }

            return false;
        }

        private static void SortForBaselining(SarifLog log)
        {
            foreach (Run run in log.EnumerateRuns())
            {
                SortForBaselining(run);
            }
        }

        private static void SortForBaselining(Run run)
        {
            run.SetRunOnResults();
            List<Result> results = (List<Result>)run.Results;
            results.Sort(DirectResultMatchingComparer.Instance);
        }
    }
}
