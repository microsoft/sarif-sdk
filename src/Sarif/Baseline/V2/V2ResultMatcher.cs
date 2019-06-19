// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

using Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching;

namespace Microsoft.CodeAnalysis.Sarif.Baseline
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
                    LinkIfSimilar(0, 0);
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
                        LinkIfSimilar(beforeIndex, afterIndex);
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
                LinkIfSimilar(link.Item1, link.Item2);
            }
        }

        public void LinkAdjacentSimilarResults()
        {
            // Walk down, matching similar, previously unlinked Results after already linked pairs
            for (int beforeIndex = 0; beforeIndex < Before.Count - 1; ++beforeIndex)
            {
                int afterIndex = MatchingIndexFromBefore[beforeIndex];
                if (afterIndex == -1 || afterIndex + 1 >= After.Count) { continue; }
                LinkIfSimilar(beforeIndex + 1, afterIndex + 1);
            }

            // Walk up, matching similar, previously unlinked Results before already linked pairs
            for (int beforeIndex = Before.Count - 1; beforeIndex > 0; --beforeIndex)
            {
                int afterIndex = MatchingIndexFromBefore[beforeIndex];
                if (afterIndex <= 0 || afterIndex - 1 >= After.Count) { continue; }
                LinkIfSimilar(beforeIndex - 1, afterIndex - 1);
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

        private void LinkIfSimilar(int beforeIndex, int afterIndex)
        {
            // Link Results *if* they weren't matched earlier and they are 'Sufficiently Similar'
            if (MatchingIndexFromBefore[beforeIndex] == -1 && MatchingIndexFromAfter[afterIndex] == -1)
            {
                if (Before[beforeIndex].IsSufficientlySimilarTo(After[afterIndex]))
                {
                    MatchingIndexFromBefore[beforeIndex] = afterIndex;
                    MatchingIndexFromAfter[afterIndex] = beforeIndex;
                }
            }
        }

        private static void Fill(int[] array, int value)
        {
            for (int i = 0; i < array.Length; ++i)
            {
                array[i] = value;
            }
        }
    }
}
