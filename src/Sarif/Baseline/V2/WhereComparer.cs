// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching;

namespace Microsoft.CodeAnalysis.Sarif.Baseline
{
    /// <summary>
    ///  Compare the 'Where' properties of two ExtractedResults.
    ///  They will compare equal if every PhysicalLocation Uri and Region position matches
    ///  and all LogicalLocation FullyQualifiedName matches.
    /// </summary>
    public class WhereComparer : IComparer<ExtractedResult>
    {
        public static WhereComparer Instance = new WhereComparer();

        public int Compare(ExtractedResult left, ExtractedResult right)
        {
            return CompareWhere(left, right);
        }

        /// <summary>
        ///  Compare the first Physical Location Uri of each Result
        /// </summary>
        public static int CompareFirstArtifactUri(ExtractedResult left, ExtractedResult right)
        {
            Uri leftUri = FirstUri(left);
            Uri rightUri = FirstUri(right);

            return CompareTo(leftUri, rightUri);
        }

        public static int CompareWhere(ExtractedResult left, ExtractedResult right)
        {
            return CompareTo(left?.Result?.Locations, left?.OriginalRun, right?.Result?.Locations, right?.OriginalRun);
        }

        public static int CompareTo(IList<Location> left, Run leftRun, IList<Location> right, Run rightRun)
        {
            if (left == null && right == null) { return 0; }
            if (left == null) { return -1; }
            if (right == null) { return 1; }

            int commonLength = Math.Min(left.Count, right.Count);
            for (int i = 0; i < commonLength; ++i)
            {
                int cmp = CompareTo(left[i], leftRun, right[i], rightRun);
                if (cmp != 0) { return cmp; }
            }

            return left.Count.CompareTo(right.Count);
        }

        public static int CompareTo(Location left, Run leftRun, Location right, Run rightRun)
        {
            if (left == null && right == null) { return 0; }
            if (left == null) { return -1; }
            if (right == null) { return 1; }

            // Compare by Physical Location, if present.
            int cmp = CompareTo(left.PhysicalLocation, leftRun, right.PhysicalLocation, rightRun);
            if (cmp != 0) { return cmp; }

            // Compare by all Logical Locations, if present.
            cmp = CompareTo(left.LogicalLocations, leftRun, right.LogicalLocations, rightRun);
            if (cmp != 0) { return cmp; }

            return cmp;
        }

        public static int CompareTo(IList<LogicalLocation> left, Run leftRun, IList<LogicalLocation> right, Run rightRun)
        {
            if (left == null && right == null) { return 0; }
            if (left == null) { return -1; }
            if (right == null) { return 1; }

            int commonLength = Math.Min(left.Count, right.Count);
            for (int i = 0; i < commonLength; ++i)
            {
                int cmp = CompareTo(left[i], leftRun, right[i], rightRun);
                if (cmp != 0) { return cmp; }
            }

            return left.Count.CompareTo(right.Count);
        }

        public static int CompareTo(LogicalLocation left, Run leftRun, LogicalLocation right, Run rightRun)
        {
            // Look up LogicalLocations if these are indices only.
            left = left?.Resolve(leftRun);
            right = right?.Resolve(rightRun);

            return string.Compare(left?.FullyQualifiedName, right?.FullyQualifiedName);
        }

        public static int CompareTo(PhysicalLocation left, Run leftRun, PhysicalLocation right, Run rightRun)
        {
            if (left == null && right == null) { return 0; }
            if (left == null) { return -1; }
            if (right == null) { return 1; }

            // Compare Uris first
            int cmp = CompareTo(left.ArtifactLocation, leftRun, right.ArtifactLocation, rightRun);
            if (cmp != 0) { return cmp; }

            // Compare Region if Uris match
            cmp = CompareTo(left.Region, right.Region);
            if (cmp != 0) { return cmp; }

            return 0;
        }

        public static int CompareTo(ArtifactLocation left, Run leftRun, ArtifactLocation right, Run rightRun)
        {
            return CompareTo(ArtifactUri(left, leftRun), ArtifactUri(right, rightRun));
        }

        /// <summary>
        ///  Compare Regions for 'where' sorting. Does not compare snippets or messages.
        /// </summary>
        public static int CompareTo(Region left, Region right)
        {
            if (left == null && right == null) { return 0; }
            if (left == null) { return -1; }
            if (right == null) { return 1; }

            int cmp = left.ByteOffset.CompareTo(right.ByteOffset);
            if (cmp != 0) { return cmp; }

            cmp = left.ByteLength.CompareTo(right.ByteLength);
            if (cmp != 0) { return cmp; }

            cmp = left.CharOffset.CompareTo(right.CharOffset);
            if (cmp != 0) { return cmp; }

            cmp = left.CharLength.CompareTo(right.CharLength);
            if (cmp != 0) { return cmp; }

            cmp = left.StartLine.CompareTo(right.StartLine);
            if (cmp != 0) { return cmp; }

            cmp = ResolvedStartColumn(left).CompareTo(ResolvedStartColumn(right));
            if (cmp != 0) { return cmp; }

            cmp = ResolvedEndLine(left).CompareTo(ResolvedEndLine(right));
            if (cmp != 0) { return cmp; }

            cmp = left.EndColumn.CompareTo(right.EndColumn);
            if (cmp != 0) { return cmp; }

            return cmp;
        }

        private static int ResolvedStartColumn(Region region)
        {
            return (region.StartColumn == -1 ? 1 : region.StartColumn);
        }

        private static int ResolvedEndLine(Region region)
        {
            return (region.EndLine == -1 ? region.StartLine : region.EndLine);
        }

        public static int CompareTo(Uri left, Uri right)
        {
            if (left == null && right == null) { return 0; }
            if (left == null) { return -1; }
            if (right == null) { return 1; }

            return left.OriginalString.CompareTo(right.OriginalString);
        }

        /// <summary>
        ///  Choose a location (Uri or FullyQualifiedName) to represent this Result.
        ///  It's the first Uri or FullyQualifiedName which was also seen in the other Run.
        /// </summary>
        public static string LocationSpecifier(ExtractedResult result, HashSet<string> otherRunLocations)
        {
            // Return the first Uri also in the other run, if any
            foreach (Location location in result.Result.Locations ?? Enumerable.Empty<Location>())
            {
                string uri = LocationUri(location, result.OriginalRun)?.OriginalString;
                if (uri != null && otherRunLocations.Contains(uri))
                {
                    return uri;
                }
            }

            // Otherwise, return the first FullyQualifiedName also in the other run, if any
            foreach (Location location in result.Result.Locations ?? Enumerable.Empty<Location>())
            {
                string fullyQualifiedName = LocationFullyQualifiedName(location);
                if (fullyQualifiedName != null && otherRunLocations.Contains(fullyQualifiedName))
                {
                    return fullyQualifiedName;
                }
            }

            // Otherwise, return empty, so that all Results without matching locations
            // are put in the same "bucket" for matching. (File Rename handling)
            return String.Empty;
        }

        // Add all Uris and FullyQualifiedLocations in the Result to a set
        // Used to identify Uris or FQLs which 
        public static void AddLocationIdentifiers(ExtractedResult result, HashSet<string> toSet)
        {
            foreach (Location location in result.Result.Locations ?? Enumerable.Empty<Location>())
            {
                string uri = LocationUri(location, result.OriginalRun)?.OriginalString;
                if (uri != null)
                {
                    toSet.Add(uri);
                }

                string fullyQualifiedName = LocationFullyQualifiedName(location);
                if(fullyQualifiedName != null)
                {
                    toSet.Add(fullyQualifiedName);
                }
            }
        }

        public static string LocationFullyQualifiedName(Location loc)
        {
            return loc?.LogicalLocation?.FullyQualifiedName;
        }

        public static Uri FirstUri(ExtractedResult result)
        {
            return LocationUri(result?.Result?.Locations?.FirstOrDefault(), result?.OriginalRun);
        }

        public static Uri LocationUri(Location loc, Run run)
        {
            return ArtifactUri(loc?.PhysicalLocation?.ArtifactLocation, run);
        }

        private static Uri ArtifactUri(ArtifactLocation loc, Run run)
        {
            return loc?.Uri ?? loc?.Resolve(run)?.Uri;
        }
    }
}
