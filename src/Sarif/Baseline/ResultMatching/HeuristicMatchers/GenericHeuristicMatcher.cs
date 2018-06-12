// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching.HeuristicMatchers
{
    internal class GenericHeuristicMatcher : IResultMatcher
    {
        public GenericHeuristicMatcher(IResultMatchingComparer comparer, IRemappingCalculator remapper = null)
        {
            Comparer = comparer;
            Remapper = remapper;
        }

        public IResultMatchingComparer Comparer { get; }
        public IRemappingCalculator Remapper { get; }

        public IEnumerable<MatchedResults> MatchResults(IEnumerable<MatchingResult> baseline, IEnumerable<MatchingResult> current)
        {
            IEnumerable<SarifLogRemapping> possibleRemappings = null;
            Dictionary<int, List<MatchingResult>> baselineResults = new Dictionary<int, List<MatchingResult>>();
            List<MatchedResults> results = new List<MatchedResults>();

            if (Remapper != null)
            {
                possibleRemappings = Remapper.CalculatePossibleRemappings(baseline, current);
            }

            foreach (var baseResult in baseline)
            {
                if (Comparer.ResultMatcherApplies(baseResult))
                {
                    int key = Comparer.GetHashCode(baseResult);
                    if (baselineResults.ContainsKey(key))
                    {
                        baselineResults[key].Add(baseResult);
                    }
                    else
                    {
                        baselineResults[key] = new List<MatchingResult>() { baseResult };
                    }
                }
            }

            foreach (var currResult in current)
            {
                if (Comparer.ResultMatcherApplies(currResult))
                {
                    MatchedResults result;
                    if (TryMatchResult(baselineResults, currResult, out result))
                    {
                        results.Add(result);
                    }
                    else if (possibleRemappings != null)
                    {
                        var applicableRemappings = possibleRemappings.Where(rm => rm.Applies(currResult));
                        foreach (SarifLogRemapping remap in applicableRemappings)
                        {
                            if (TryMatchResult(baselineResults, remap.RemapResult(currResult), out result))
                            {
                                results.Add(result);
                                continue;
                            }
                        }
                    }
                }
            }
            
            return results;
        }
        
        public bool TryMatchResult(Dictionary<int, List<MatchingResult>> resultDictionary, MatchingResult currentResult, out MatchedResults result)
        {
            int unmodifiedKey = Comparer.GetHashCode(currentResult);
            if (resultDictionary.ContainsKey(unmodifiedKey))
            {
                List<MatchingResult> matchingBaselineResult = resultDictionary[unmodifiedKey].Where(b => Comparer.Equals(b, currentResult)).ToList();
                if (matchingBaselineResult.Count == 1)
                {
                    result = new MatchedResults() { BaselineResult = matchingBaselineResult[0], CurrentResult = currentResult, MatchingAlgorithm = this };
                    return true;
                }
                else if (matchingBaselineResult.Count > 1)
                {
                    // TODO--what if multiple results match here?  Grab the first?  Group them up? ...?  We do want this to be completely deterministic, which is part of the problem.
                    // This will remain unsolved in the early implementation.  A discrete difference metric probably makes sense here.
                    throw new NotImplementedException("Not implemented for early matchers.");
                }
            }
            result = null;
            return false;
        }

    }
}
