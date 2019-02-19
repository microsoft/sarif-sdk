// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.CodeAnalysis.Sarif.Processors;
using Microsoft.CodeAnalysis.Sarif.Readers;
using System.Diagnostics;
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
        /// <param name="previousLog">Array of sarif logs representing the baseline run</param>
        /// <param name="currentLogs">Array of sarif logs representing the current run</param>
        /// <returns>A SARIF log with the merged set of results.</returns>
        public SarifLog Match(SarifLog previousLog, SarifLog currentLog)
        {
            return Match(previousLogs: new[] { previousLog }, currentLogs: new[]{ currentLog }).FirstOrDefault();
        }


        /// <summary>
        /// Take two groups of sarif logs, and compute a sarif log containing the complete set of results,
        /// with status (compared to baseline) and various baseline-related fields persisted (e.x. work item links,
        /// ID, etc.
        /// </summary>
        /// <param name="previousLogs">Array of sarif logs representing the baseline run</param>
        /// <param name="currentLogs">Array of sarif logs representing the current run</param>
        /// <returns>A SARIF log with the merged set of results.</returns>
        public IEnumerable<SarifLog> Match(IEnumerable<SarifLog> previousLogs, IEnumerable<SarifLog> currentLogs)
        {
            Dictionary<string, List<Run>> runsByToolPrevious = GetRunsByTool(previousLogs);
            Dictionary<string, List<Run>> runsByToolCurrent = GetRunsByTool(currentLogs);
            
            List<string> tools = runsByToolPrevious.Keys.Union(runsByToolCurrent.Keys).ToList();

            List<SarifLog> resultToolLogs = new List<SarifLog>();

            foreach (var key in tools)
            {
                IEnumerable<Run> baselineRuns = new Run[0];
                if (runsByToolPrevious.ContainsKey(key))
                {
                     baselineRuns = runsByToolPrevious[key];
                }
                IEnumerable<Run> currentRuns = new Run[0];

                if (runsByToolCurrent.ContainsKey(key))
                {
                    currentRuns = runsByToolCurrent[key];
                }

                resultToolLogs.Add(BaselineSarifLogs(baselineRuns, currentRuns));
            }

            return new List<SarifLog> { resultToolLogs.Merge() };
        }

        private static Dictionary<string, List<Run>> GetRunsByTool(IEnumerable<SarifLog> sarifLogs)
        {
            Dictionary<string, List<Run>> runsByTool = new Dictionary<string, List<Run>>();
            if (sarifLogs == null)
            {
                return runsByTool;
            }

            foreach (SarifLog sarifLog in sarifLogs)
            {
                if (sarifLog == null)
                {
                    continue;
                }
                foreach (Run run in sarifLog.Runs)
                {
                    string toolName = run.Tool.Driver.Name;
                    if (runsByTool.ContainsKey(toolName))
                    {
                        runsByTool[toolName].Add(run);
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
                previous == null ? new List<ExtractedResult>() : GetMatchingResultsFromRuns(previous);

            List<ExtractedResult> currentResults =
                current == null ? new List<ExtractedResult>() : GetMatchingResultsFromRuns(current);

            List<MatchedResults> matchedResults = new List<MatchedResults>();

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
            foreach (ExtractedResult result in baselineResults)
            {
                matchedResults.Add(new MatchedResults() { PreviousResult = result, CurrentResult = null });
            }

            foreach (ExtractedResult result in currentResults)
            {
                matchedResults.Add(new MatchedResults() { PreviousResult = null, CurrentResult = result });
            }
        }

        private void CalculateMatches(IEnumerable<IResultMatcher> matchers, List<ExtractedResult> baselineResults, List<ExtractedResult> currentResults, List<MatchedResults> matchedResults)
        {
            if (matchers != null)
            {
                foreach (IResultMatcher matcher in matchers)
                {
                    IEnumerable<MatchedResults> results = matcher.Match(baselineResults, currentResults);
                    foreach (var result in results)
                    {
                        baselineResults.Remove(result.PreviousResult);
                        currentResults.Remove(result.CurrentResult);
                    }
                    matchedResults.AddRange(results);
                }
            }
        }

        private List<ExtractedResult> GetMatchingResultsFromRuns(IEnumerable<Run> sarifRuns)
        {
            List<ExtractedResult> results = new List<ExtractedResult>();          
            foreach (Run run in sarifRuns)
            {
                if (run.Results != null)
                {
                    foreach (Result result in run.Results)
                    {
                        results.Add(new ExtractedResult() { Result = result, OriginalRun = run });
                    }
                }
            }

            return results;
        }
        
        private ReportingDescriptor GetRuleFromResources(Result result, IDictionary<string, ReportingDescriptor> rules)
        {
            if (!string.IsNullOrEmpty(result.RuleId))
            {
                if (rules.ContainsKey(result.RuleId))
                {
                    return rules[result.RuleId];
                }
            }
            return null;
        }

        private SarifLog ConstructSarifLogFromMatchedResults(
            IEnumerable<MatchedResults> results, 
            IEnumerable<Run> previousRuns, 
            IEnumerable<Run> currentRuns)
        {
            if (currentRuns == null || !currentRuns.Any())
            {
                throw new ArgumentException(nameof(currentRuns));
            }
            
            // Results should all be from the same tool, so we'll pull the log from the first run.
            Tool tool = currentRuns.First().Tool.DeepClone();

            Run run = new Run()
            {
                Tool = tool,
                Id = currentRuns.First().Id,
            };

            IDictionary<string, SerializedPropertyInfo> properties = null;

            if (previousRuns != null && previousRuns.Count() != 0)
            {
                // We flow the baseline instance id forward (which becomes the 
                // baseline guid for the merged log)
                run.BaselineInstanceGuid = previousRuns.First().Id?.InstanceGuid;
            }

            bool initializeFromOldest = PropertyBagMergeBehavior.HasFlag(DictionaryMergeBehavior.InitializeFromOldest);
            if (initializeFromOldest)
            {
                // Find the 'oldest' log file and initialize properties from that log property bag
                properties = previousRuns.FirstOrDefault() != null
                    ? previousRuns.First().Properties
                    : currentRuns.First().Properties;
            }
            else
            {
                // Find the most recent log file instance and retain its property bag
                // Find the 'oldest' log file and initialize properties from that log property bag
                properties = currentRuns.Last().Properties;
            }

            var reportingDescriptors = new Dictionary<ReportingDescriptor, int>(ReportingDescriptor.ValueComparer);

            var indexRemappingVisitor = new RemapIndicesVisitor(currentFiles: null);

            properties = properties ?? new Dictionary<string, SerializedPropertyInfo>();

            List<Result> newRunResults = new List<Result>();
            foreach (MatchedResults resultPair in results)
            {
                Result result = resultPair.CalculateBasedlinedResult(PropertyBagMergeBehavior);

                IList<Artifact> files = 
                    (PropertyBagMergeBehavior.HasFlag(DictionaryMergeBehavior.InitializeFromOldest) &&
                    (result.BaselineState == BaselineState.Unchanged || result.BaselineState == BaselineState.Updated)) 
                    ? resultPair.PreviousResult.OriginalRun.Artifacts
                    : resultPair.Run.Artifacts;

                indexRemappingVisitor.HistoricalFiles = files;
                indexRemappingVisitor.HistoricalLogicalLocations = resultPair.Run.LogicalLocations;
                indexRemappingVisitor.VisitResult(result);

                if (result.RuleIndex != -1)
                {
                    ReportingDescriptor rule = resultPair.Run.Tool.Driver.RuleDescriptors[0];
                    if (!reportingDescriptors.TryGetValue(rule, out int ruleIndex))
                    {
                        reportingDescriptors[rule] = run.Tool.Driver.RuleDescriptors.Count;
                        run.Tool.Driver.RuleDescriptors.Add(rule);
                    }
                    result.RuleIndex = ruleIndex;
                }

                newRunResults.Add(result);
            }

            run.Results = newRunResults;
            run.Artifacts = indexRemappingVisitor.CurrentFiles;
            
            var graphs = new Dictionary<string, Graph>();
            var ruleData = new Dictionary<string, ReportingDescriptor>();
            var invocations = new List<Invocation>();

            // TODO tool message strings are not currently handled
            // https://github.com/Microsoft/sarif-sdk/issues/1286

            foreach (Run currentRun in currentRuns)
            {
                if (currentRun.Graphs != null)
                {
                    MergeDictionaryInto(graphs, currentRun.Graphs, GraphEqualityComparer.Instance);
                }

                if (currentRun.Invocations != null)
                {
                    invocations.AddRange(currentRun.Invocations);
                }

                if (PropertyBagMergeBehavior == DictionaryMergeBehavior.InitializeFromMostRecent)
                {
                    properties = currentRun.Properties;
                }
            }

            run.Graphs = graphs;
            run.LogicalLocations = new List<LogicalLocation>(indexRemappingVisitor.RemappedLogicalLocationIndices.Keys);
            //run.Resources = new Resources() { MessageStrings = messageData, Rules = ruleData }; TODO
            run.Invocations = invocations;

            if (properties != null && properties.Count > 0)
            {
                run.Properties = properties;
            }

            return new SarifLog()
            {
                Version = SarifVersion.Current,
                SchemaUri = new Uri(SarifUtilities.SarifSchemaUri),
                Runs = new Run[] { run }
            };
        }

        private void MergeDictionaryInto<T, S>(
            IDictionary<T, S> baseDictionary, 
            IDictionary<T, S> dictionaryToAdd, 
            IEqualityComparer<S> dictionaryValueComparer)
        {
            MergeDictionaryInto(baseDictionary, dictionaryToAdd, dictionaryValueComparer, PropertyBagMergeBehavior);
        }

        internal static void MergeDictionaryInto<T, S>(
            IDictionary<T, S> baseDictionary, 
            IDictionary<T, S> dictionaryToAdd, 
            IEqualityComparer<S> duplicateCatch, 
            DictionaryMergeBehavior propertyBagMergeBehavior)
        {
            foreach (KeyValuePair<T, S> pair in dictionaryToAdd)
            {

                if (!baseDictionary.ContainsKey(pair.Key))
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

                basePropertyBagHolder = baseDictionary[pair.Key] as PropertyBagHolder;

                if (basePropertyBagHolder != null)
                {
                    propertyBagHolderToMerge = pair.Value as PropertyBagHolder;
                    Debug.Assert(propertyBagHolderToMerge != null);

                    baseProperties = basePropertyBagHolder.Properties;
                    basePropertyBagHolder.Properties = null;
                    propertiesToMerge = propertyBagHolderToMerge.Properties;
                    propertyBagHolderToMerge.Properties = null;
                };

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
