using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.Sarif.Baseline;
using Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching;

namespace SecretScan.Telemetry.ResultMatching
{
    public class V2ResultMatcher : IResultMatcher
    {
        public IList<MatchedResults> Match(IList<ExtractedResult> before, IList<ExtractedResult> after)
        {
            ResultMatchState state = new ResultMatchState(before, after);
            return state.Match();
        }
    }

    /// <summary>
    ///  ResultMatchState contains all of the state needed to compute matches
    ///  between two batches of Results.
    /// </summary>
    internal class ResultMatchState
    {
        // Results in the first set
        public List<ExtractedResult> Before;

        // Results in the second set
        public List<ExtractedResult> After;

        // The index of the Result in After matching this Result in Before, or -1
        public int[] MatchingIndexFromBefore;

        // The index of the Result in Before matching this Result in After, or -1
        public int[] MatchingIndexFromAfter;

        public ResultMatchState(IList<ExtractedResult> before, IList<ExtractedResult> after)
        {
            // Sort results by 'Where' for matching
            Before = new List<ExtractedResult>(before);
            Before.Sort(WhereComparer.Instance);

            After = new List<ExtractedResult>(after);
            After.Sort(WhereComparer.Instance);

            // Set all match indices to -1 initially (no Results matched)
            MatchingIndexFromBefore = new int[Before.Count];
            Fill(MatchingIndexFromBefore, -1);

            MatchingIndexFromAfter = new int[After.Count];
            Fill(MatchingIndexFromAfter, -1);
        }

        public IList<MatchedResults> Match()
        {
            if (Before.Count == 1 && After.Count == 1)
            {
                // If there's only one issue, it matches if 'Sufficiently Similar'
                if (Before[0].IsSufficientlySimilarTo(After[0]))
                {
                    Link(0, 0);
                }
            }
            else
            {
                LinkResultsWithIdenticalWhere();
                LinkResultsWithUniqueIdenticalWhat();
                LinkAdjacentSimilarResults();
            }

            return BuildMatchList();
        }

        public void LinkResultsWithIdenticalWhere()
        {
            // Walk Results sorted by where, linking those with identical positions and a matching category
            int beforeIndex = 0, afterIndex = 0;
            while (beforeIndex < Before.Count && afterIndex < After.Count)
            {
                ExtractedResult left = Before[beforeIndex];
                ExtractedResult right = After[afterIndex];

                int whereCmp = WhereComparer.CompareWhere(left, right);
                if (whereCmp < 0)
                {
                    // Left is in a 'Where' before Right - look at the next Result in 'Before'
                    beforeIndex++;
                }
                else if (whereCmp > 0)
                {
                    // Right is in a 'Where' before Left - look at the next Result in 'After'
                    afterIndex++;
                }
                else
                {
                    // The Results have a matching where. If the category matches, link them
                    if (left.MatchesCategory(right))
                    {
                        Link(beforeIndex, afterIndex);
                    }

                    // Look at the next pair of Results
                    beforeIndex++;
                    afterIndex++;
                }
            }
        }

        public void LinkResultsWithUniqueIdenticalWhat()
        {
            WhatMap beforeMap = new WhatMap(Before, MatchingIndexFromBefore);
            WhatMap afterMap = new WhatMap(After, MatchingIndexFromAfter);

            foreach (Tuple<int, int> link in beforeMap.UniqueLinks(afterMap))
            {
                Link(link.Item1, link.Item2);
            }
        }

        public void LinkAdjacentSimilarResults()
        {
            // Walk down, matching similar, previously unlinked Results after already linked pairs
            for (int beforeIndex = 0; beforeIndex < Before.Count - 1; ++beforeIndex)
            {
                int afterIndex = MatchingIndexFromBefore[beforeIndex];
                if (afterIndex == -1 || afterIndex + 1 >= After.Count) { continue; }
                if (MatchingIndexFromBefore[beforeIndex + 1] != -1 || MatchingIndexFromAfter[afterIndex + 1] != -1) { continue; }

                if (Before[beforeIndex + 1].IsSufficientlySimilarTo(After[afterIndex + 1]))
                {
                    Link(beforeIndex + 1, afterIndex + 1);
                }
            }

            // Walk up, matching similar, previously unlinked Results before already linked pairs
            for (int beforeIndex = 1; beforeIndex < Before.Count; ++beforeIndex)
            {
                int afterIndex = MatchingIndexFromBefore[beforeIndex];
                if (afterIndex <= 0 || afterIndex - 1 >= After.Count) { continue; }
                if (MatchingIndexFromBefore[beforeIndex - 1] != -1 || MatchingIndexFromAfter[afterIndex - 1] != -1) { continue; }

                if (Before[beforeIndex - 1].IsSufficientlySimilarTo(After[afterIndex - 1]))
                {
                    Link(beforeIndex - 1, afterIndex - 1);
                }
            }
        }

        public IList<MatchedResults> BuildMatchList()
        {
            List<MatchedResults> matches = new List<MatchedResults>();

            // 1. Add all Removed Results
            for (int beforeIndex = 0; beforeIndex < Before.Count; ++beforeIndex)
            {
                if (MatchingIndexFromBefore[beforeIndex] == -1)
                {
                    matches.Add(new MatchedResults(Before[beforeIndex], null));
                }
            }

            // 2. Add all linked pairs and Added Results (in 'After' order)
            for (int afterIndex = 0; afterIndex < After.Count; ++afterIndex)
            {
                int beforeIndex = MatchingIndexFromAfter[afterIndex];
                if (beforeIndex == -1)
                {
                    matches.Add(new MatchedResults(null, After[afterIndex]));
                }
                else
                {
                    matches.Add(new MatchedResults(Before[beforeIndex], After[afterIndex]));
                }
            }

            return matches;
        }

        private void Link(int beforeIndex, int afterIndex)
        {
            MatchingIndexFromBefore[beforeIndex] = afterIndex;
            MatchingIndexFromAfter[afterIndex] = beforeIndex;
        }

        private static void Fill(int[] array, int value)
        {
            for (int i = 0; i < array.Length; ++i)
            {
                array[i] = value;
            }
        }

        /// <summary>
        ///  WhatMap is used to look for unique 'What' properties across batches of Results.
        ///  Each 'What' property value is added to a Dictionary, specific to the Category (RuleId)
        ///  and Property Name where that value was found.
        /// </summary>
        internal class WhatMap
        {
            // Dictionary of (Category | PropertyName | Value) => (Result Index)
            // If the same combination occurs multiple times, it will be in the map with an index of -1
            private Dictionary<Tuple<string, string, string>, int> Map;

            public WhatMap(IList<ExtractedResult> results, int[] linksFromResults)
            {
                Map = new Dictionary<Tuple<string, string, string>, int>();

                // Map *only* results which aren't already linked
                for (int i = 0; i < results.Count; ++i)
                {
                    if (linksFromResults[i] == -1)
                    {
                        Add(results[i], i);
                    }
                }
            }

            private void Add(ExtractedResult result, int index)
            {
                // Add all 'What' properties to the map, specific to the given Category and Property Name.
                foreach (var fingerprint in result.Result.Fingerprints)
                {
                    Add(result.Result.RuleId, fingerprint.Key, fingerprint.Value, index);
                }
            }

            private void Add(string category, string propertyName, string value, int index)
            {
                if (String.IsNullOrEmpty(value)) { return; }

                Tuple<string, string, string> key = new Tuple<string, string, string>(category, propertyName, value);
                if (Map.ContainsKey(key))
                {
                    // If the map has another of this value, set index -1 to indicate non-unique
                    Map[key] = -1;
                }
                else
                {
                    // Otherwise, point to the result
                    Map[key] = index;
                }
            }

            /// <summary>
            ///  Return the indices of results in this set and the other set
            ///  which have a common trait which is unique in each set.
            /// </summary>
            /// <param name="other">WhatMap for other Result set</param>
            /// <returns>Index of this and Index of other Result where the two have a unique trait in common.</returns>
            public IEnumerable<Tuple<int, int>> UniqueLinks(WhatMap other)
            {
                foreach (var entry in Map.Where(entry => entry.Value != -1))
                {
                    if (other.Map.TryGetValue(entry.Key, out int otherIndex) && otherIndex != -1)
                    {
                        yield return new Tuple<int, int>(entry.Value, otherIndex);
                    }
                }
            }
        }
    }
}
