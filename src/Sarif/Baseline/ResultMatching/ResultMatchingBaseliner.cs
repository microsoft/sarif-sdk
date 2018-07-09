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
    class ResultMatchingBaseliner : IResultMatchingBaseliner
    {
        public const string ResultMatchingResultPropertyName = "ResultMatching";


        public ResultMatchingBaseliner(IEnumerable<IResultMatcher> exactResultMatchers,
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
        /// <param name="baseline">Array of sarif logs representing the baseline run</param>
        /// <param name="current">Array of sarif logs representing the current run</param>
        /// <returns>A SARIF log with the merged set of results.</returns>
        public SarifLog BaselineSarifLogs(SarifLog[] baseline, SarifLog[] current)
        {
            Dictionary<string, List<Run>> runsByToolBaseline = GetRunsByTool(baseline);
            Dictionary<string, List<Run>> runsByToolCurrent = GetRunsByTool(current);
            
            List<string> tools = runsByToolBaseline.Keys.ToList();
            tools.AddRange(runsByToolCurrent.Keys);
            tools = tools.Distinct().ToList();

            List<SarifLog> baselinedByToolLogs = new List<SarifLog>();

            foreach (var key in tools)
            {
                Run[] baselineRuns = new Run[0];
                if (runsByToolBaseline.ContainsKey(key))
                {
                     baselineRuns = runsByToolBaseline[key].ToArray();
                }
                Run[] currentRuns = new Run[0];
                if (runsByToolCurrent.ContainsKey(key))
                {
                    currentRuns = runsByToolCurrent[key].ToArray();
                }

                baselinedByToolLogs.Add(BaselineSarifLogs(baselineRuns, currentRuns));
            }

            return baselinedByToolLogs.Merge();
        }

        private static Dictionary<string, List<Run>> GetRunsByTool(SarifLog[] sarifLogs)
        {
            Dictionary<string, List<Run>> runsByTool = new Dictionary<string, List<Run>>();
            foreach (SarifLog sarifLog in sarifLogs)
            {
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

        public SarifLog BaselineSarifLogs(Run[] baseline, Run[] current)
        {
            // Spin out SARIF logs into MatchingResult objects.
            List<MatchingResult> baselineResults = GetMatchingResultsFromRuns(baseline);
            List<MatchingResult> currentResults = GetMatchingResultsFromRuns(current);
            List<MatchedResults> matchedResults = new List<MatchedResults>();

            // Calculate exact mappings using exactResultMatchers.
            CalculateMatches(ExactResultMatchers, baselineResults, currentResults, matchedResults);
            
            // Use the heuristic matchers to match remaining results.
            CalculateMatches(HeuristicMatchers, baselineResults, currentResults, matchedResults);

            // Add unmatched results here.
            AddUnmatchedResults(baselineResults, currentResults, matchedResults);

            // Create a combined SARIF log with the total results.
            return ConstructSarifLogFromMatchedResults(matchedResults, baseline[0].Id, current);
        }

        private static void AddUnmatchedResults(List<MatchingResult> baselineResults, List<MatchingResult> currentResults, List<MatchedResults> matchedResults)
        {
            foreach (MatchingResult result in baselineResults)
            {
                matchedResults.Add(new MatchedResults() { BaselineResult = result, CurrentResult = null });
            }

            foreach (MatchingResult result in currentResults)
            {
                matchedResults.Add(new MatchedResults() { BaselineResult = null, CurrentResult = result });
            }
        }

        private void CalculateMatches(IEnumerable<IResultMatcher> matchers, List<MatchingResult> baselineResults, List<MatchingResult> currentResults, List<MatchedResults> matchedResults)
        {
            if (matchers != null)
            {
                foreach (IResultMatcher matcher in matchers)
                {
                    IEnumerable<MatchedResults> results = matcher.MatchResults(baselineResults, currentResults);
                    foreach (var result in results)
                    {
                        baselineResults.Remove(result.BaselineResult);
                        currentResults.Remove(result.CurrentResult);
                    }
                    matchedResults.AddRange(results);
                }
            }
        }

        public List<MatchingResult> GetMatchingResultsFromRuns(Run[] sarifRuns)
        {
            List<MatchingResult> results = new List<MatchingResult>();

            foreach (Run run in sarifRuns)
            {
                foreach (Result result in run.Results)
                {
                    Rule rule = GetRuleFromResources(result, run.Resources.Rules);
                    results.Add(new MatchingResult() { RuleId = GetRuleIdFromResult(result, rule), Result = result, Tool = run.Tool, Rule = rule, OriginalRun = run });
                }
            }

            return results;
        }

        private string GetRuleIdFromResult(Result result, Rule rule)
        {
            if (rule == null)
            {
                return result.RuleId;
            }
            else
            {
                return rule.Id;
            }
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

        public SarifLog ConstructSarifLogFromMatchedResults(IEnumerable<MatchedResults> results, string baselineId, Run[] currentRuns)
        {
            if(currentRuns == null || !currentRuns.Any())
            {
                throw new ArgumentException(nameof(currentRuns));
            }

            // Results should all be from the same tool, so we'll pull the log from the first run.
            Tool tool = currentRuns[0].Tool.DeepClone();

            Run run = new Run()
            {
                Tool = tool,
                AutomationId = currentRuns[0].AutomationId,
                BaselineId = baselineId,
                Id = currentRuns[0].Id,
            };

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
                if (run.Files != null)
                {
                    MergeDictionaryInto(fileData, run.Files, FileDataEqualityComparer.Instance);
                }
                if (run.Resources != null)
                {
                    if (run.Resources.Rules != null)
                    {
                        MergeDictionaryInto(ruleData, run.Resources.Rules, RuleEqualityComparer.Instance);
                    }
                    if (run.Resources.MessageStrings != null)
                    {
                        // Autogenerated code does not currently mark this properly as a string, string dictionary.
                        IDictionary<string, string> converted = run.Resources.MessageStrings as Dictionary<string, string>;
                        if (converted == null)
                        {
                            throw new ArgumentException("Message Strings did not deserialize properly into a dictionary mapping strings to strings.");
                        }
                        MergeDictionaryInto(messageData, converted, StringComparer.InvariantCulture);
                    }
                }
                if (run.LogicalLocations != null)
                {
                    MergeDictionaryInto(logicalLocations, run.LogicalLocations, LogicalLocationEqualityComparer.Instance);
                }
                if (run.Graphs != null)
                {
                    graphs.AddRange(run.Graphs);
                }
                if (run.Invocations != null)
                {
                    invocations.AddRange(run.Invocations);
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
                else if (duplicateCatch.Equals(baseDictionary[pair.Key], pair.Value))
                {
                    throw new NotImplementedException("We do not, at this moment, support two different pieces of supporting metadata going to the same key in the same scan.");
                }
            }
        }
    }
}
