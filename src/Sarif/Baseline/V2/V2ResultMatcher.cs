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
        // Threshold for how many 'nearby' Results are considered after other match phases
        private const int NearnessThreshold = 3;

        // Results in the first set.
        private readonly List<ExtractedResult> _before;
        private readonly TrustMap _beforeTrustMap;
        private readonly WhatMap _beforeWhatMap;
        private readonly int[] _matchingIndexFromBefore;

        // Results in the second set.
        private readonly List<ExtractedResult> _after;
        private readonly TrustMap _afterTrustMap;
        private readonly WhatMap _afterWhatMap;
        private readonly int[] _matchingIndexFromAfter;

        public StatefulResultMatcher(IList<ExtractedResult> before, IList<ExtractedResult> after)
        {
            // Sort results by 'Where', then 'RuleId' for matching
            _before = new List<ExtractedResult>(before);
            _before.Sort(ResultMatchingComparer.Instance);

            _beforeTrustMap = new TrustMap();
            _beforeWhatMap = new WhatMap();

            _matchingIndexFromBefore = new int[_before.Count];
            Fill(_matchingIndexFromBefore, -1);

            _after = new List<ExtractedResult>(after);
            _after.Sort(ResultMatchingComparer.Instance);

            _afterTrustMap = new TrustMap();
            _afterWhatMap = new WhatMap();

            _matchingIndexFromAfter = new int[_after.Count];
            Fill(_matchingIndexFromAfter, -1);
        }

        public IList<MatchedResults> Match()
        {
            BuildMaps();

            // If there's only one result, the "match before" and "match after" logic doesn't fire for them,
            // so the only way they'll actually get compared is if we add this special case:
            if (_before.Count == 1 && _after.Count == 1)
            {
                // If there's only one issue, it matches if 'Sufficiently Similar'.
                if (_before[0].IsSufficientlySimilarTo(_after[0], _afterTrustMap))
                {
                    LinkIfSimilar(0, 0);
                }
            }
            else
            {
                LinkResultsWithIdenticalWhere();
                LinkFirstAndLastFromSameArtifact();
                LinkResultsWithUniqueIdenticalWhat();
                LinkNearbySimilarResults();
            }

            return BuildMatchList();
        }

        private void BuildMaps()
        {
            // Identify all locations used in each log
            HashSet<string> beforeLocationIdentifiers = new HashSet<string>();
            _before.ForEach((result) => WhereComparer.AddLocationIdentifiers(result, beforeLocationIdentifiers));

            HashSet<string> afterLocationIdentifiers = new HashSet<string>();
            _after.ForEach((result) => WhereComparer.AddLocationIdentifiers(result, afterLocationIdentifiers));

            // Populate WhatMap and TrustMap to guide subsequent matching
            BuildMap(_before, _beforeWhatMap, _beforeTrustMap, otherRunLocations: afterLocationIdentifiers);
            BuildMap(_after, _afterWhatMap, _afterTrustMap, otherRunLocations: beforeLocationIdentifiers);

            // Match the TrustMaps to finish determining trust
            _afterTrustMap.CountMatchesWith(_beforeTrustMap);
        }

        private static void BuildMap(List<ExtractedResult> results, WhatMap whatMap, TrustMap trustMap, HashSet<string> otherRunLocations)
        {
            // Populate the WhatMap and TrustMap
            for (int i = 0; i < results.Count; ++i)
            {
                ExtractedResult result = results[i];

                // Find the LocationSpecifier for the Result (the first Uri or FQN also in the other Run)
                string locationSpecifier = WhereComparer.LocationSpecifier(result, otherRunLocations);

                foreach (WhatComponent component in WhatComparer.WhatProperties(result, locationSpecifier))
                {
                    // Add Result attributes used as matching hints in a "bucket" for the Rule x LocationSpecifier x AttributeName
                    whatMap.Add(component, i);

                    // Track attribute usage to determine per-attribute trust
                    trustMap.Add(component);
                }
            }
        }

        private void LinkResultsWithIdenticalWhere()
        {
            // Walk Results sorted by where, linking those with identical positions and a matching category.
            int beforeIndex = 0, afterIndex = 0;
            while (beforeIndex < _before.Count && afterIndex < _after.Count)
            {
                ExtractedResult left = _before[beforeIndex];
                ExtractedResult right = _after[afterIndex];

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

            // Walk Before and After once, looking for the first and last results per Uri
            // NOTE: 'beforeIndex' and 'afterIndex' are passed by ref to FirstWithUri and LastWithUri, which move them forward only.
            while (beforeIndex < _before.Count && afterIndex < _after.Count)
            {
                // Get the next After Result (the first for a given Uri)
                ExtractedResult afterFirstForUri = _after[afterIndex];

                // Look for the first Before Result with the same Uri, if any
                ExtractedResult beforeFirstForUri = FirstWithUri(afterFirstForUri, _before, ref beforeIndex);

                // If there was one...
                if (beforeFirstForUri != null)
                {
                    // ... Try to link the first Results together
                    LinkIfSimilar(beforeIndex, afterIndex);

                    // ... Find the last Before and After result with the same Uri
                    ExtractedResult beforeLastForUri = LastWithUri(afterFirstForUri, _before, ref beforeIndex);
                    ExtractedResult afterLastForUri = LastWithUri(afterFirstForUri, _after, ref afterIndex);

                    // ... Try to link those as well (either may be the first Result if there was only one for that Uri)
                    LinkIfSimilar(beforeIndex, afterIndex);
                }
                else
                {
                    // ... If no Before results for this Uri, skip to the After Result with the next Uri
                    LastWithUri(afterFirstForUri, _after, ref afterIndex);
                }

                // Move to the first Result with the next Uri
                afterIndex++;
            }
        }

        private void LinkResultsWithUniqueIdenticalWhat()
        {
            foreach (Tuple<int, int> link in _beforeWhatMap.UniqueLinks(_afterWhatMap))
            {
                LinkIfSimilar(link.Item1, link.Item2);
            }
        }

        private void LinkNearbySimilarResults()
        {
            // Walk up, matching similar, previously unlinked Results after already linked pairs.
            for (int beforeIndex = 0; beforeIndex < _before.Count - 1; ++beforeIndex)
            {
                int afterIndex = _matchingIndexFromBefore[beforeIndex];
                if (afterIndex == -1) { continue; }

                // This is very subtle. At first glance it seems that we only give the result pairs
                // _immediately_ after previously linked pairs a chance to match. But when we link
                // this pair, we'll add its indices to the MatchingIndexFromBefore and MatchingIndexFromAfter
                // maps. So the next time through the loop, afterIndex will once again _not_ be -1,
                // and we'll give the next pair a chance to match as well.
                for (int i = 1; i < NearnessThreshold; ++i)
                {
                    if (afterIndex + i >= _after.Count) { break; }
                    LinkIfSimilar(beforeIndex + 1, afterIndex + i);
                }
            }

            // Walk down, matching similar, previously unlinked Results before already linked pairs.
            for (int beforeIndex = _before.Count - 1; beforeIndex > 0; --beforeIndex)
            {
                int afterIndex = _matchingIndexFromBefore[beforeIndex];
                if (afterIndex == -1) { continue; }

                for (int i = 1; i < NearnessThreshold; ++i)
                {
                    if (afterIndex - i < 0) { break; }
                    LinkIfSimilar(beforeIndex - 1, afterIndex - i);
                }
            }
        }

        private IList<MatchedResults> BuildMatchList()
        {
            List<MatchedResults> matches = new List<MatchedResults>();

            // 1. Add all Removed Results.
            for (int beforeIndex = 0; beforeIndex < _before.Count; ++beforeIndex)
            {
                if (_matchingIndexFromBefore[beforeIndex] == -1)
                {
                    matches.Add(new MatchedResults(_before[beforeIndex], null));
                }
            }

            // 2. Add all linked pairs and Added Results (in 'After' order).
            for (int afterIndex = 0; afterIndex < _after.Count; ++afterIndex)
            {
                int beforeIndex = _matchingIndexFromAfter[afterIndex];
                if (beforeIndex == -1)
                {
                    matches.Add(new MatchedResults(null, _after[afterIndex]));
                }
                else
                {
                    matches.Add(new MatchedResults(_before[beforeIndex], _after[afterIndex]));
                }
            }

            return matches;
        }

        private void LinkIfSimilar(int beforeIndex, int afterIndex)
        {
            // Link Results *if* they weren't matched earlier and they are 'Sufficiently Similar'.
            if (_matchingIndexFromBefore[beforeIndex] == -1 && _matchingIndexFromAfter[afterIndex] == -1)
            {
                if (_before[beforeIndex].IsSufficientlySimilarTo(_after[afterIndex], _afterTrustMap))
                {
                    _matchingIndexFromBefore[beforeIndex] = afterIndex;
                    _matchingIndexFromAfter[afterIndex] = beforeIndex;
                }
            }
        }

        private ExtractedResult FirstWithUri(ExtractedResult desiredUri, IList<ExtractedResult> set, ref int fromIndex)
        {
            // Find the first Result at fromIndex or later with a Uri *matching* the desired one, or null if there aren't any
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

            // Find the first Result  at fromIndex or later with a Uri *after* the desired one, saving the last Result that matched as we go
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
