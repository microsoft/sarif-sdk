// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    /// <summary>
    /// Shared decoding of the two-tier finding-grouping cross-links (see
    /// docs/ai/grouping-findings.md). A grouping edge is a 'locationRelationship'
    /// whose kind is 'includes' or 'isIncludedBy' and whose target location carries
    /// a 'sarif:' pointer to another result. Used by AI1015 and AI2020.
    /// </summary>
    internal static class GroupingRelationshipHelper
    {
        internal const string Includes = "includes";
        internal const string IsIncludedBy = "isIncludedBy";

        internal struct GroupingEdge
        {
            public string Kind;
            public int RunIndex;
            public int ResultIndex;
            public string TargetUri;
        }

        private static readonly Regex s_resultPointer =
            new Regex(@"^sarif:/runs/(\d+)/results/(\d+)", RegexOptions.Compiled);

        internal static string Inverse(string kind)
            => kind == Includes ? IsIncludedBy
             : kind == IsIncludedBy ? Includes
             : null;

        internal static IEnumerable<GroupingEdge> GetGroupingEdges(Result result)
        {
            if (result == null)
            {
                yield break;
            }

            var locationsById = new Dictionary<int, Location>();
            IndexById(locationsById, result.Locations);
            IndexById(locationsById, result.RelatedLocations);

            foreach (Location location in EnumerateLocations(result))
            {
                if (location?.Relationships == null)
                {
                    continue;
                }

                foreach (LocationRelationship relationship in location.Relationships)
                {
                    string kind = GroupingKind(relationship?.Kinds);
                    if (kind == null)
                    {
                        continue;
                    }

                    if (!locationsById.TryGetValue(relationship.Target, out Location target))
                    {
                        continue;
                    }

                    string uri = target.PhysicalLocation?.ArtifactLocation?.Uri?.OriginalString;
                    if (string.IsNullOrEmpty(uri))
                    {
                        continue;
                    }

                    Match match = s_resultPointer.Match(uri);
                    if (!match.Success
                        || !int.TryParse(match.Groups[1].Value, out int runIndex)
                        || !int.TryParse(match.Groups[2].Value, out int resultIndex))
                    {
                        continue;
                    }

                    yield return new GroupingEdge
                    {
                        Kind = kind,
                        RunIndex = runIndex,
                        ResultIndex = resultIndex,
                        TargetUri = uri
                    };
                }
            }
        }

        private static IEnumerable<Location> EnumerateLocations(Result result)
        {
            if (result.Locations != null)
            {
                foreach (Location location in result.Locations)
                {
                    yield return location;
                }
            }

            if (result.RelatedLocations != null)
            {
                foreach (Location location in result.RelatedLocations)
                {
                    yield return location;
                }
            }
        }

        private static void IndexById(Dictionary<int, Location> map, IList<Location> locations)
        {
            if (locations == null)
            {
                return;
            }

            foreach (Location location in locations)
            {
                if (location != null)
                {
                    int id = (int)location.Id;
                    if (!map.ContainsKey(id))
                    {
                        map[id] = location;
                    }
                }
            }
        }

        private static string GroupingKind(IList<string> kinds)
        {
            if (kinds == null)
            {
                return null;
            }

            if (kinds.Contains(Includes))
            {
                return Includes;
            }

            if (kinds.Contains(IsIncludedBy))
            {
                return IsIncludedBy;
            }

            return null;
        }
    }
}
