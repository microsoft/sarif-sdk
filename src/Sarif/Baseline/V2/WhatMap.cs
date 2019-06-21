// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching;

namespace Microsoft.CodeAnalysis.Sarif.Baseline
{
    /// <summary>
    ///  WhatMap is used to look for unique 'What' properties across batches of Results.
    ///  Each 'What' property value is added to a Dictionary, specific to the Category (RuleId)
    ///  and Property Name where that value was found.
    /// </summary>
    internal class WhatMap
    {
        // Dictionary of (Category | PropertyName | Value) => (Result Index)
        // If the same combination occurs multiple times, it will be in the map with an index of -1
        private readonly Dictionary<WhatComponent, int> _map;

        public WhatMap(IList<ExtractedResult> results, int[] linksFromResults)
        {
            _map = new Dictionary<WhatComponent, int>();

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
            foreach(WhatComponent component in result.WhatProperties())
            {
                Add(component, index);
            }
        }

        public void Add(WhatComponent component, int index)
        {
            if (component?.PropertyValue == null) { return; }

            if (_map.TryGetValue(component, out int existingIndex) && existingIndex != index)
            {
                // If the map has another of this value, set index -1 to indicate non-unique
                _map[component] = -1;
            }
            else
            {
                // Otherwise, point to the result
                _map[component] = index;
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
            foreach (var entry in _map.Where(entry => entry.Value != -1))
            {
                if (other._map.TryGetValue(entry.Key, out int otherIndex) && otherIndex != -1)
                {
                    yield return new Tuple<int, int>(entry.Value, otherIndex);
                }
            }
        }
    }
}
