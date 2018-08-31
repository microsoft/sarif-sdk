// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.CodeAnalysis.Sarif.Processors;

namespace Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching
{
    /// <summary>
    /// Default Result Matching Baseliner.
    /// </summary>
    internal class SarifLogResultMatcher : ISarifLogMatcher
    {
        public const string ResultMatchingResultPropertyName = "ResultMatching";


        public SarifLogResultMatcher(IEnumerable<IResultMatcher> exactResultMatchers,
            IEnumerable<IResultMatcher> heuristicMatchers)
        {
            ExactResultMatchers = exactResultMatchers;
            HeuristicMatchers = heuristicMatchers;
        }

        public IEnumerable<IResultMatcher> ExactResultMatchers { get; }
        public IEnumerable<IResultMatcher> HeuristicMatchers { get; }

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
            
            List<string> tools = runsByToolPrevious.Keys.ToList();
            tools.AddRange(runsByToolCurrent.Keys);
            tools = tools.Distinct().ToList();

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

            return new List<SarifLog> { resultToolLogs.Merge() }.AsEnumerable();
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
                    string toolName = run.Tool.Name;
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
                        Rule rule = GetRuleFromResources(result, run.Resources.Rules);
                        results.Add(new ExtractedResult() { Result = result, OriginalRun = run });
                    }
                }
            }

            return results;
        }
        
        private Rule GetRuleFromResources(Result result, IDictionary<string, Rule> rules)
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

        private SarifLog ConstructSarifLogFromMatchedResults(IEnumerable<MatchedResults> results, IEnumerable<Run> previous, IEnumerable<Run> currentRuns)
        {
            if(currentRuns == null || !currentRuns.Any())
            {
                throw new ArgumentException(nameof(currentRuns));
            }
            
            // Results should all be from the same tool, so we'll pull the log from the first run.
            Tool tool = currentRuns.First().Tool.DeepClone();

            Run run = new Run()
            {
                Tool = tool,
                AutomationLogicalId = currentRuns.First().AutomationLogicalId,
                InstanceGuid = currentRuns.First().InstanceGuid,
            };

            if (previous != null && previous.Count() != 0)

            {
                run.BaselineInstanceGuid = previous.First().InstanceGuid;
            }
            
            List<Result> newRunResults = new List<Result>();
            foreach (MatchedResults resultPair in results)
            {
                newRunResults.Add(resultPair.CalculateNewBaselineResult());
            }
            run.Results = newRunResults;
            
            // Merge run File data, resources, etc...
            Dictionary<string, FileData> fileData = new Dictionary<string, FileData>();
            Dictionary<string, Rule> ruleData = new Dictionary<string, Rule>();
            Dictionary<string, string> messageData = new Dictionary<string, string>();
            List<Graph> graphs = new List<Graph>();
            Dictionary<string, LogicalLocation> logicalLocations = new Dictionary<string, LogicalLocation>();
            List<Invocation> invocations = new List<Invocation>();

            foreach (Run currentRun in currentRuns)
            {
                if (currentRun.Files != null)
                {
                    MergeDictionaryInto(fileData, currentRun.Files, FileDataEqualityComparer.Instance);
                }
                if (currentRun.Resources != null)
                {
                    if (currentRun.Resources.Rules != null)
                    {
                        MergeDictionaryInto(ruleData, currentRun.Resources.Rules, RuleEqualityComparer.Instance);
                    }
                    if (currentRun.Resources.MessageStrings != null)
                    {
                        // Autogenerated code does not currently mark this properly as a string, string dictionary.
                        IDictionary<string, string> converted = currentRun.Resources.MessageStrings as Dictionary<string, string>;
                        if (converted == null)
                        {
                            throw new ArgumentException("Message Strings did not deserialize properly into a dictionary mapping strings to strings.");
                        }
                        MergeDictionaryInto(messageData, converted, StringComparer.InvariantCulture);
                    }
                }
                if (currentRun.LogicalLocations != null)
                {
                    MergeDictionaryInto(logicalLocations, currentRun.LogicalLocations, LogicalLocationEqualityComparer.Instance);
                }
                if (currentRun.Graphs != null)
                {
                    graphs.AddRange(currentRun.Graphs);
                }
                if (currentRun.Invocations != null)
                {
                    invocations.AddRange(currentRun.Invocations);
                }
            }

            run.Files = fileData;
            run.Graphs = graphs;
            run.LogicalLocations = logicalLocations;
            run.Resources = new Resources() { MessageStrings = messageData, Rules = ruleData };
            run.Invocations = invocations;

            return new SarifLog()
            {
                Runs = new Run[] { run },
            };
        }
        
        private void MergeDictionaryInto<T, S>(Dictionary<T, S> baseDictionary, IDictionary<T, S> dictionaryToAdd, IEqualityComparer<S> duplicateCatch)
        {
            foreach (KeyValuePair<T, S> pair in dictionaryToAdd)
            {
                if (!baseDictionary.ContainsKey(pair.Key))
                {
                    baseDictionary.Add(pair.Key, pair.Value);
                }
                else if (!duplicateCatch.Equals(baseDictionary[pair.Key], pair.Value))
                {
                    throw new InvalidOperationException("We do not, at this moment, support two different pieces of supporting metadata going to the same key in the same scan.");
                }
            }
        }
    }
}
