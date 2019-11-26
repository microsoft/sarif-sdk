// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching;
using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Baseline
{
    public class V2ResultMatcher : IResultMatcher
    {
        public IList<MatchedResults> Match(IList<ExtractedResult> before, IList<ExtractedResult> after)
        {
            StatefulResultMatcher state = new StatefulResultMatcher(before, after);
            return state.Match();
        }
    }

    /// <summary>
    ///  StatefulResultMatcher contains all of the state needed to compute matches
    ///  between two batches of Results.
    /// </summary>
    /// <remarks>
    /// This class exists so that with a V2ResultMatcher in hand, you can call Match on it
    /// repeatedly without having to worry about resetting its internal state each time.
    /// All the state is encapsulated by the StatefulResultMatcher, which is discarded
    /// after each call to V2ResultMatcher.Match.
    /// </remarks>
    internal class StatefulResultMatcher
    {
        // Results in the first set.
        private List<ExtractedResult> Before { get; }

        // Results in the second set.
        private List<ExtractedResult> After { get; }

        // The index of the Result in After matching this Result in Before, or -1.
        private int[] MatchingIndexFromBefore { get; }

        // The index of the Result in Before matching this Result in After, or -1.
        private int[] MatchingIndexFromAfter { get; }

        public StatefulResultMatcher(IList<ExtractedResult> before, IList<ExtractedResult> after)
        {
            // Sort results by 'Where', then 'RuleId' for matching
            Before = new List<ExtractedResult>(before);
            Before.Sort(ResultMatchingComparer.Instance);

            After = new List<ExtractedResult>(after);
            After.Sort(ResultMatchingComparer.Instance);

            // Set all match indices to -1 initially (no Results matched).
            MatchingIndexFromBefore = new int[Before.Count];
            Fill(MatchingIndexFromBefore, -1);

            MatchingIndexFromAfter = new int[After.Count];
            Fill(MatchingIndexFromAfter, -1);
        }

        public IList<MatchedResults> Match()
        {
            // If there's only one result, the "match before" and "match after" logic doesn't fire for them,
            // so the only way they'll actually get compared is if we add this special case:
            if (Before.Count == 1 && After.Count == 1)
            {
                // If there's only one issue, it matches if 'Sufficiently Similar'.
                if (Before[0].IsSufficientlySimilarTo(After[0]))
                {
                    LinkIfSimilar(0, 0);
                }
            }
            else
            {
                LinkResultsWithIdenticalWhere();
                LinkFirstAndLastFromSameArtifact();
                LinkResultsWithUniqueIdenticalWhat();
                LinkAdjacentSimilarResults();
            }

            return BuildMatchList();
        }

        private void LinkResultsWithIdenticalWhere()
        {
            // Walk Results sorted by where, linking those with identical positions and a matching category.
            int beforeIndex = 0, afterIndex = 0;
            while (beforeIndex < Before.Count && afterIndex < After.Count)
            {
                ExtractedResult left = Before[beforeIndex];
                ExtractedResult right = After[afterIndex];

                int whereCmp = WhereComparer.CompareWhere(left, right);
                if (whereCmp < 0)
                {
                    // Left is in a 'Where' before Right - look at the next Result in 'Before'.
                    beforeIndex++;
                }
                else if (whereCmp > 0)
                {
                    // Right is in a 'Where' before Left - look at the next Result in 'After'.
                    afterIndex++;
                }
                else
                {
                    // The Results have a matching where. If the category matches, link them.
                    if (left.MatchesCategory(right))
                    {
                        LinkIfSimilar(beforeIndex, afterIndex);
                    }

                    // Look at the next pair of Results.
                    beforeIndex++;
                    afterIndex++;
                }
            }
        }

        private void LinkFirstAndLastFromSameArtifact()
        {
            int afterIndex = 0;
            int beforeIndex = 0;

            while (beforeIndex < Before.Count && afterIndex < After.Count)
            {
                // Get the next After Result (the first for a given Uri)
                ExtractedResult afterFirstForUri = After[afterIndex];

                // Look for the first Before Result with the same Uri, if any
                ExtractedResult beforeFirstForUri = FirstWithUri(afterFirstForUri, Before, ref beforeIndex);

                // If there was one...
                if (beforeFirstForUri != null)
                {
                    // ... Try to link the first Results together
                    LinkIfSimilar(beforeIndex, afterIndex);

                    // ... Find the last Before and After result with the same Uri
                    ExtractedResult beforeLastForUri = LastWithUri(afterFirstForUri, Before, ref beforeIndex);
                    ExtractedResult afterLastForUri = LastWithUri(afterFirstForUri, After, ref afterIndex);

                    // ... Try to link those as well (either may be the first Result if there was only one for that Uri)
                    LinkIfSimilar(beforeIndex, afterIndex);
                }
                else
                {
                    // ... If no Before results for this Uri, skip to the After Result with the next Uri
                    LastWithUri(afterFirstForUri, After, ref afterIndex);
                }

                // Move to the first Result with the next Uri
                afterIndex++;
            }
        }

        private void LinkResultsWithUniqueIdenticalWhat()
        {
            WhatMap beforeMap = new WhatMap(Before, MatchingIndexFromBefore);
            WhatMap afterMap = new WhatMap(After, MatchingIndexFromAfter);

            foreach (Tuple<int, int> link in beforeMap.UniqueLinks(afterMap))
            {
                LinkIfSimilar(link.Item1, link.Item2);
            }
        }

        private void LinkAdjacentSimilarResults()
        {
            // Walk up, matching similar, previously unlinked Results after already linked pairs.
            for (int beforeIndex = 0; beforeIndex < Before.Count - 1; ++beforeIndex)
            {
                int afterIndex = MatchingIndexFromBefore[beforeIndex];
                if (afterIndex == -1 || afterIndex + 1 >= After.Count) { continue; }

                // This is very subtle. At first glance it seems that we only give the result pairs
                // _immediately_ after previously linked pairs a chance to match. But when we link
                // this pair, we'll add its indices to the MatchingIndexFromBefore and MatchingIndexFromAfter
                // maps. So the next time through the loop, afterIndex will once again _not_ be -1,
                // and we'll give the next pair a chance to match as well.
                LinkIfSimilar(beforeIndex + 1, afterIndex + 1);
            }

            // Walk down, matching similar, previously unlinked Results before already linked pairs.
            for (int beforeIndex = Before.Count - 1; beforeIndex > 0; --beforeIndex)
            {
                int afterIndex = MatchingIndexFromBefore[beforeIndex];
                if (afterIndex <= 0 || afterIndex - 1 >= After.Count) { continue; }
                LinkIfSimilar(beforeIndex - 1, afterIndex - 1);
            }
        }

        private IList<MatchedResults> BuildMatchList()
        {
            List<MatchedResults> matches = new List<MatchedResults>();

            // 1. Add all Removed Results.
            for (int beforeIndex = 0; beforeIndex < Before.Count; ++beforeIndex)
            {
                if (MatchingIndexFromBefore[beforeIndex] == -1)
                {
                    matches.Add(new MatchedResults(Before[beforeIndex], null));
                }
            }

            // 2. Add all linked pairs and Added Results (in 'After' order).
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
            // Link Results *if* they weren't matched earlier and they are 'Sufficiently Similar'.
            if (MatchingIndexFromBefore[beforeIndex] == -1 && MatchingIndexFromAfter[afterIndex] == -1)
            {
                if (Before[beforeIndex].IsSufficientlySimilarTo(After[afterIndex]))
                {
                    MatchingIndexFromBefore[beforeIndex] = afterIndex;
                    MatchingIndexFromAfter[afterIndex] = beforeIndex;
                }
            }
        }

        private ExtractedResult FirstWithUri(ExtractedResult desiredUri, IList<ExtractedResult> set, ref int fromIndex)
        {
            // Find the first Result with a Uri *matching* the desired one, or null if there aren't any
            for (; fromIndex < set.Count; ++fromIndex)
            {
                int whereCmp = WhereComparer.CompareFirstArtifactUri(set[fromIndex], desiredUri);

                if (whereCmp == 0)
                {
                    return set[fromIndex];
                }
                else if (whereCmp > 0)
                {
                    break;
                }
            }

            return null;
        }

        private ExtractedResult LastWithUri(ExtractedResult desiredUri, IList<ExtractedResult> set, ref int fromIndex)
        {
            ExtractedResult lastMatch = null;

            // Find the first Result with a Uri *after* the desired one, saving the last Result that matched as we go
            for (; fromIndex < set.Count; ++fromIndex)
            {
                int whereCmp = WhereComparer.CompareFirstArtifactUri(set[fromIndex], desiredUri);

                if (whereCmp == 0)
                {
                    lastMatch = set[fromIndex];
                }
                else if (whereCmp > 0)
                {
                    break;
                }
            }

            // Ensure the index ends at the last match
            if (fromIndex > 0)
            {
                fromIndex--;
            }

            return lastMatch;
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
