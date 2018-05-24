// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching.ExactMatchers;
using Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching.HeuristicMatchers;

namespace Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching
{
    class ResultMatchingBaseliner
    {

        public static ResultMatchingBaseliner DefaultResultMatchingBaseliner()
        {
            return new ResultMatchingBaseliner
                (
                    // Exact matchers run first, in order.  These do *no* remapping.
                    new List<IResultMatcher>()
                    {
                        new IdenticalResultMatcher(),
                        new FullFingerprintResultMatcher()
                    },
                    // Heuristic matchers run in order after the exact matchers.
                    new List<IResultMatcher>()
                    {
                        new ContextRegionHeuristicMatcher(),
                        new PartialFingerprintResultMatcher()
                    }
                );
        }

        public ResultMatchingBaseliner(IEnumerable<IResultMatcher> exactResultMatchers,
            IEnumerable<IResultMatcher> heuristicMatchers)
        {
            ExactResultMatchers = exactResultMatchers;
            HeuristicMatchers = heuristicMatchers;
        }

        public IEnumerable<IResultMatcher> ExactResultMatchers { get; }
        public IEnumerable<IResultMatcher> HeuristicMatchers { get; }

        public SarifLog BaselineSarifLogs(SarifLog baseline, SarifLog current)
        {
            Dictionary<string, List<Run>> runsByToolBaseline = GetRunsByTool(baseline);
            Dictionary<string, List<Run>> runsByToolCurrent = GetRunsByTool(current);
            
            List<string> tools = runsByToolBaseline.Keys.ToList();
            tools.AddRange(runsByToolCurrent.Keys);

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

            // TODO--merge logs.
            throw new NotImplementedException();
        }

        private static Dictionary<string, List<Run>> GetRunsByTool(SarifLog sarifLog)
        {
            Dictionary<string, List<Run>> runsByTool = new Dictionary<string, List<Run>>();
            foreach (var run in sarifLog.Runs)
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
            return ConstructSarifLogFromMatchedResults(matchedResults);
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

        public SarifLog ConstructSarifLogFromMatchedResults(IEnumerable<MatchedResults> results)
        {
            throw new NotImplementedException();
        }
    }
}
