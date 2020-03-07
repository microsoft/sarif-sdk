// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching.HeuristicMatchers
{
    internal class HeuristicMatcher : IResultMatcher
    {
        public HeuristicMatcher(IResultMatchingComparer comparer, IRemappingCalculator remapper = null)
        {
            Comparer = comparer;
            Remapper = remapper;
        }

        public IResultMatchingComparer Comparer { get; }
        public IRemappingCalculator Remapper { get; }

        public IList<MatchedResults> Match(IList<ExtractedResult> previousResults, IList<ExtractedResult> currentResults)
        {
            IEnumerable<SarifLogRemapping> possibleRemappings = null;
            var baselineResults = new Dictionary<int, List<ExtractedResult>>();
            var matchedResults = new List<MatchedResults>();

            if (Remapper != null)
            {
                possibleRemappings = Remapper.CalculatePossibleRemappings(previousResults, currentResults);
            }

            foreach (ExtractedResult previousResult in previousResults)
            {
                if (Comparer.ResultMatcherApplies(previousResult))
                {
                    int key = Comparer.GetHashCode(previousResult);
                    if (baselineResults.ContainsKey(key))
                    {
                        baselineResults[key].Add(previousResult);
                    }
                    else
                    {
                        baselineResults[key] = new List<ExtractedResult>() { previousResult };
                    }
                }
            }

            foreach (ExtractedResult currentResult in currentResults)
            {
                if (Comparer.ResultMatcherApplies(currentResult))
                {
                    MatchedResults result;
                    if (TryMatchResult(baselineResults, currentResult, out result))
                    {
                        matchedResults.Add(result);
                    }
                    else if (possibleRemappings != null)
                    {
                        IEnumerable<SarifLogRemapping> applicableRemappings = possibleRemappings.Where(rm => rm.Applies(currentResult));
                        foreach (SarifLogRemapping remap in applicableRemappings)
                        {
                            if (TryMatchResult(baselineResults, remap.RemapResult(currentResult), out result))
                            {
                                matchedResults.Add(result);
                                continue;
                            }
                        }
                    }
                }
            }

            return matchedResults;
        }

        public bool TryMatchResult(Dictionary<int, List<ExtractedResult>> resultDictionary, ExtractedResult currentResult, out MatchedResults result)
        {
            int unmodifiedKey = Comparer.GetHashCode(currentResult);
            if (resultDictionary.ContainsKey(unmodifiedKey))
            {
                List<ExtractedResult> matchingBaselineResult = resultDictionary[unmodifiedKey].Where(b => Comparer.Equals(b, currentResult)).ToList();
                if (matchingBaselineResult.Count == 1)
                {
                    result = new MatchedResults(matchingBaselineResult[0], currentResult);
                    return true;
                }
                else if (matchingBaselineResult.Count > 1)
                {
                    // TODO--what if multiple results match here?  
                    // Right now we grab the first?  It might be better to group them up? ...?  
                    // We do want this to be completely deterministic, which is part of the problem--grabbing the first assumes
                    // the input logs have been made deterministic.
                    // This will remain unsolved in the early implementation.  
                    // One thought--A discrete difference metric probably makes sense here.
                    result = new MatchedResults(matchingBaselineResult[0], currentResult);
                    return true;
                }
            }
            result = null;
            return false;
        }

    }
}
