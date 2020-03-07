// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching.HeuristicMatchers
{
    internal class ContextRegionHeuristicMatcher : HeuristicMatcher
    {
        public ContextRegionHeuristicMatcher() : base(ContextRegionResultComparer.Instance) { }

        public class ContextRegionResultComparer : IResultMatchingComparer
        {
            public static readonly ContextRegionResultComparer Instance = new ContextRegionResultComparer();

            public bool Equals(ExtractedResult x, ExtractedResult y)
            {
                IEnumerable<ArtifactContent> xContextRegions = x.Result.Locations.Select(loc => loc.PhysicalLocation.ContextRegion.Snippet);

                HashSet<ArtifactContent> yContextRegions = new HashSet<ArtifactContent>(ArtifactContentEqualityComparer.Instance);
                foreach (ArtifactContent content in y.Result.Locations.Select(loc => loc.PhysicalLocation.ContextRegion.Snippet))
                {
                    yContextRegions.Add(content);
                }

                if (xContextRegions.Count() != yContextRegions.Count())
                {
                    return false;
                }

                foreach (ArtifactContent content in xContextRegions)
                {
                    if (!yContextRegions.Contains(content))
                    {
                        return false;
                    }
                }

                return true;
            }

            public int GetHashCode(ExtractedResult obj)
            {
                int hashCode = -187510987;
                IEnumerable<ArtifactContent> contextRegions = obj.Result.Locations.Select(loc => loc.PhysicalLocation.ContextRegion.Snippet);
                foreach (ArtifactContent content in contextRegions)
                {
                    hashCode ^= ArtifactContentEqualityComparer.Instance.GetHashCode(content);
                }

                return hashCode;
            }

            public bool ResultMatcherApplies(ExtractedResult result)
            {
                bool? applies = result.Result.Locations?.Select(loc => loc.PhysicalLocation?.ContextRegion?.Snippet).Where(snippet => !string.IsNullOrEmpty(snippet?.Text)).Any();

                return applies.HasValue ? applies.Value : false;
            }
        }

    }
}
