﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Microsoft.CodeAnalysis.Sarif.Processors;
using Microsoft.CodeAnalysis.Sarif.Visitors;

namespace Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching
{
    /// <summary>
    /// Default Result Matching Baseliner.
    /// </summary>
    internal class SarifLogResultMatcher : ISarifLogMatcher
    {
        public const string ResultMatchingResultPropertyName = "ResultMatching";

        public SarifLogResultMatcher(
            IEnumerable<IResultMatcher> exactResultMatchers,
            IEnumerable<IResultMatcher> heuristicMatchers,
            DictionaryMergeBehavior propertyBagMergeBehaviors = DictionaryMergeBehavior.None)
        {
            ExactResultMatchers = exactResultMatchers;
            HeuristicMatchers = heuristicMatchers;
            PropertyBagMergeBehavior = propertyBagMergeBehaviors;
        }

        public IEnumerable<IResultMatcher> ExactResultMatchers { get; }
        public IEnumerable<IResultMatcher> HeuristicMatchers { get; }
        public DictionaryMergeBehavior PropertyBagMergeBehavior { get; }

        /// <summary>
        /// Helper function that accepts a single baseline and current SARIF log and matches them.
        /// </summary>
        /// <param name="previousLog">Array of SARIF logs representing the baseline run</param>
        /// <param name="currentLog">Array of SARIF logs representing the current run</param>
        /// <returns>A SARIF log with the merged set of results.</returns>
        public SarifLog Match(SarifLog previousLog, SarifLog currentLog)
        {
            return Match(previousLogs: new[] { previousLog }, currentLogs: new[] { currentLog }).FirstOrDefault();
        }

        /// <summary>
        /// Take two groups of SARIF logs, and compute a SARIF log containing the complete set of results,
        /// with status (compared to baseline) and various baseline-related fields persisted (e.x. work item links,
        /// ID, etc.
        /// </summary>
        /// <param name="previousLogs">Array of SARIF logs representing the baseline run</param>
        /// <param name="currentLogs">Array of SARIF logs representing the current run</param>
        /// <returns>A SARIF log with the merged set of results.</returns>
        public IEnumerable<SarifLog> Match(IEnumerable<SarifLog> previousLogs, IEnumerable<SarifLog> currentLogs)
        {
            var resultToolLogs = new List<SarifLog>();
            Dictionary<string, List<Run>> runsByToolCurrent = GetRunsByTool(currentLogs);
            Dictionary<string, List<Run>> runsByToolPrevious = GetRunsByTool(previousLogs);

            foreach (KeyValuePair<string, List<Run>> runByToolCurrent in runsByToolCurrent)
            {
                string key = runByToolCurrent.Key;

                IEnumerable<Run> baselineRuns = new Run[0];
                if (runsByToolPrevious.TryGetValue(key, out List<Run> runs))
                {
                    baselineRuns = runs;
                }

                IEnumerable<Run> currentRuns = runByToolCurrent.Value;
                resultToolLogs.Add(BaselineSarifLogs(baselineRuns, currentRuns));
            }

            return new List<SarifLog> { resultToolLogs.Merge(mergeEmptyLogs: true) };
        }

        private static Dictionary<string, List<Run>> GetRunsByTool(IEnumerable<SarifLog> sarifLogs)
        {
            var runsByTool = new Dictionary<string, List<Run>>(StringComparer.OrdinalIgnoreCase);
            if (sarifLogs == null)
            {
                return runsByTool;
            }

            foreach (SarifLog sarifLog in sarifLogs)
            {
                if (sarifLog?.Runs == null)
                {
                    continue;
                }

                foreach (Run run in sarifLog.Runs)
                {
                    string toolName = run.Tool.Driver.Name;
                    if (runsByTool.TryGetValue(toolName, out List<Run> runs))
                    {
                        runs.Add(run);
                    }
                    else
                    {
                        runsByTool[toolName] = new List<Run>() { run };
                    }
                }
            }

            return runsByTool;
        }

        private SarifLog BaselineSarifLogs(IEnumerable<Run> previous, IEnumerable<Run> current)
        {
            // Spin out SARIF logs into MatchingResult objects.
            List<ExtractedResult> baselineResults =
                previous == null ? new List<ExtractedResult>() : ExtractResultsFromRuns(previous);

            List<ExtractedResult> currentResults =
                current == null ? new List<ExtractedResult>() : ExtractResultsFromRuns(current);

            var matchedResults = new List<MatchedResults>();

            // Calculate exact mappings using exactResultMatchers.
            CalculateMatches(ExactResultMatchers, baselineResults, currentResults, matchedResults);

            // Use the heuristic matchers to match remaining results.
            CalculateMatches(HeuristicMatchers, baselineResults, currentResults, matchedResults);

            // Add unmatched results here.
            AddUnmatchedResults(baselineResults, currentResults, matchedResults);

            // Create a combined SARIF log with the total results.
            return ConstructSarifLogFromMatchedResults(matchedResults, previous, current);
        }

        private static void AddUnmatchedResults(List<ExtractedResult> baselineResults, List<ExtractedResult> currentResults, List<MatchedResults> matchedResults)
        {
            // Add unmatched results from Baseline log which weren't already Absent in previous run
            foreach (ExtractedResult result in baselineResults)
            {
                matchedResults.Add(new MatchedResults(result, null));
            }

            foreach (ExtractedResult result in currentResults)
            {
                matchedResults.Add(new MatchedResults(null, result));
            }
        }

        private void CalculateMatches(IEnumerable<IResultMatcher> matchers, List<ExtractedResult> baselineResults, List<ExtractedResult> currentResults, List<MatchedResults> matchedResults)
        {
            if (matchers != null)
            {
                foreach (IResultMatcher matcher in matchers)
                {
                    IEnumerable<MatchedResults> results = matcher.Match(baselineResults, currentResults);
                    foreach (MatchedResults result in results)
                    {
                        baselineResults.Remove(result.PreviousResult);
                        currentResults.Remove(result.CurrentResult);
                    }
                    matchedResults.AddRange(results);
                }
            }
        }

        private List<ExtractedResult> ExtractResultsFromRuns(IEnumerable<Run> sarifRuns)
        {
            var results = new List<ExtractedResult>();
            foreach (Run run in sarifRuns)
            {
                if (run.Results != null)
                {
                    foreach (Result result in run.Results)
                    {
                        // Include all Results except Absent results
                        if (result.BaselineState != BaselineState.Absent)
                        {
                            results.Add(new ExtractedResult(result, run));
                        }
                    }
                }
            }

            return results;
        }

        private SarifLog ConstructSarifLogFromMatchedResults(
            IEnumerable<MatchedResults> results,
            IEnumerable<Run> previousRuns,
            IEnumerable<Run> currentRuns)
        {
            if (currentRuns == null || !currentRuns.Any())
            {
                throw new ArgumentNullException(nameof(currentRuns));
            }

            // Results should all be from the same tool, so we'll pull the log from the first run.
            Run firstRun = currentRuns.First();
            Tool tool = firstRun.Tool.DeepClone();

            // Only include the rules corresponding to matched results.
            tool.Driver.Rules = null;

            var run = new Run
            {
                Tool = tool
            };

            // If there was only one run, we can fill in more information because we don't need to
            // worry about it being different from run to run.
            if (currentRuns.Count() == 1)
            {
                run.AutomationDetails = firstRun.AutomationDetails;
                run.Conversion = firstRun.Conversion;
                run.Taxonomies = firstRun.Taxonomies;
                run.Translations = firstRun.Translations;
                run.Policies = firstRun.Policies;
                run.RedactionTokens = firstRun.RedactionTokens;
                run.Language = firstRun.Language;
            }

            if (previousRuns != null && previousRuns.Any())
            {
                // We flow the baseline instance id forward (which becomes the
                // baseline guid for the merged log).
                run.BaselineGuid = previousRuns.First().AutomationDetails?.Guid;
            }

            var visitor = new RunMergingVisitor();

            foreach (MatchedResults resultPair in results)
            {
                Result result = resultPair.CalculateBasedlinedResult(PropertyBagMergeBehavior);

                visitor.CurrentRun = result.Run;
                visitor.VisitResult(result);
            }

            visitor.PopulateWithMerged(run);

            IDictionary<string, SerializedPropertyInfo> properties = null;
            if (PropertyBagMergeBehavior.HasFlag(DictionaryMergeBehavior.InitializeFromOldest))
            {
                // Find the 'oldest' log file and initialize properties from that log property bag.
                properties = previousRuns.FirstOrDefault()?.Properties ?? currentRuns.First().Properties;
            }
            else
            {
                // Find the most recent log file instance and retain its property bag.
                // Find the 'oldest' log file and initialize properties from that log property bag.
                properties = currentRuns.Last().Properties;
            }

            properties ??= new Dictionary<string, SerializedPropertyInfo>();

            var graphs = new List<Graph>();
            var invocations = new List<Invocation>();
            var versionControls = new List<VersionControlDetails>();

            // TODO tool message strings are not currently handled
            // https://github.com/Microsoft/sarif-sdk/issues/1286

            foreach (Run currentRun in currentRuns)
            {
                if (currentRun.Graphs != null)
                {
                    graphs.AddRange(currentRun.Graphs);
                }

                if (currentRun.Invocations != null)
                {
                    invocations.AddRange(currentRun.Invocations);
                }

                if (PropertyBagMergeBehavior == DictionaryMergeBehavior.InitializeFromMostRecent)
                {
                    properties = currentRun.Properties;
                }

                if (currentRun?.VersionControlProvenance != null)
                {
                    versionControls = versionControls.DistinctMerge(
                        currentRun.VersionControlProvenance,
                        VersionControlDetails.ValueComparer).ToList();
                }
            }

            foreach (Run previousRun in previousRuns ?? Enumerable.Empty<Run>())
            {
                if (previousRun?.VersionControlProvenance != null)
                {
                    versionControls = versionControls.DistinctMerge(
                        previousRun.VersionControlProvenance,
                        VersionControlDetails.ValueComparer).ToList();
                }
            }

            run.Graphs = graphs;
            run.Invocations = invocations;
            run.Properties = properties;
            run.VersionControlProvenance = versionControls.Any() ? versionControls : null;

            return new SarifLog()
            {
                Version = SarifVersion.Current,
                SchemaUri = new Uri(SarifUtilities.SarifSchemaUri),
                Runs = new Run[] { run }
            };
        }

        internal static void MergeDictionaryInto<T, S>(
            IDictionary<T, S> baseDictionary,
            IDictionary<T, S> dictionaryToAdd,
            IEqualityComparer<S> duplicateCatch,
            DictionaryMergeBehavior propertyBagMergeBehavior)
        {
            foreach (KeyValuePair<T, S> pair in dictionaryToAdd)
            {
                if (!baseDictionary.TryGetValue(pair.Key, out S value))
                {
                    // The baseline does not contain the current dictionary value. This means
                    // that we can transport all properties associated with the new value.

                    baseDictionary.Add(pair.Key, pair.Value);
                    continue;
                }

                // We have a collision between a current and previously existing dictionary value. We need to strip
                // the properties, if any, from each value in order to perform a comparison of the core SARIF
                // data (which must be equivalent between the two instances.

                PropertyBagHolder basePropertyBagHolder, propertyBagHolderToMerge;
                IDictionary<string, SerializedPropertyInfo> baseProperties = null, propertiesToMerge = null;

                basePropertyBagHolder = value as PropertyBagHolder;

                if (basePropertyBagHolder != null)
                {
                    propertyBagHolderToMerge = pair.Value as PropertyBagHolder;
                    Debug.Assert(propertyBagHolderToMerge != null);

                    baseProperties = basePropertyBagHolder.Properties;
                    basePropertyBagHolder.Properties = null;
                    propertiesToMerge = propertyBagHolderToMerge.Properties;
                    propertyBagHolderToMerge.Properties = null;
                }

                // Now that we've emptied any properties, we can ensure that the base value and the value to
                // merge are equivalent. If they aren't we throw: there is no good way to understand which
                // construct to prefer.
                S baseValue = baseDictionary[pair.Key];
                if (!duplicateCatch.Equals(baseValue, pair.Value))
                {
                    throw new InvalidOperationException(
                        "We do not, at this moment, support merging dictionary " +
                        "value that share a key but have different content.");
                }

                if (basePropertyBagHolder != null)
                {
                    basePropertyBagHolder.Properties = propertyBagMergeBehavior == DictionaryMergeBehavior.InitializeFromMostRecent
                        ? propertiesToMerge
                        : baseProperties;
                }
            }
        }
    }
}
