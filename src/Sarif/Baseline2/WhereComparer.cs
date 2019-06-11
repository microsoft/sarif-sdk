using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching;

namespace Microsoft.CodeAnalysis.Sarif.Baseline
{
    public class WhereComparer : IComparer<ExtractedResult>
    {
        public static WhereComparer Instance = new WhereComparer();

        public int Compare(ExtractedResult left, ExtractedResult right)
        {
            return CompareWhere(left, right);
        }

        public static int CompareWhere(ExtractedResult left, ExtractedResult right)
        {
            return Compare(left?.Result?.Locations, left?.OriginalRun, right?.Result?.Locations, right?.OriginalRun);
        }

        public static int Compare(IList<Location> left, Run leftRun, IList<Location> right, Run rightRun)
        {
            if (left == null && right == null) { return 0; }
            if (left == null) { return -1; }
            if (right == null) { return 1; }

            int commonLength = Math.Min(left.Count, right.Count);
            for (int i = 0; i < commonLength; ++i)
            {
                int cmp = Compare(left[i], leftRun, right[i], rightRun);
                if (cmp != 0) { return cmp; }
            }

            return left.Count.CompareTo(right.Count);
        }

        public static int Compare(Location left, Run leftRun, Location right, Run rightRun)
        {
            int cmp = 0;

            if (left == null && right == null) { return 0; }
            if (left == null) { return -1; }
            if (right == null) { return 1; }

            // Compare by Physical Location, if present
            cmp = Compare(left.PhysicalLocation, leftRun, right.PhysicalLocation, rightRun);
            if (cmp != 0) { return cmp; }

            // Compare by 'primary' Logical Location, if present
            cmp = Compare(left.LogicalLocation, leftRun, right.LogicalLocation, rightRun);
            if (cmp != 0) { return cmp; }

            // Compare by all Logical Locations, if present
            cmp = Compare(left.LogicalLocations, leftRun, right.LogicalLocations, rightRun);
            if (cmp != 0) { return cmp; }

            return cmp;
        }

        public static int Compare(IList<LogicalLocation> left, Run leftRun, IList<LogicalLocation> right, Run rightRun)
        {
            if (left == null && right == null) { return 0; }
            if (left == null) { return -1; }
            if (right == null) { return 1; }

            int commonLength = Math.Min(left.Count, right.Count);
            for (int i = 0; i < commonLength; ++i)
            {
                int cmp = Compare(left[i], leftRun, right[i], rightRun);
                if (cmp != 0) { return cmp; }
            }

            return left.Count.CompareTo(right.Count);
        }

        public static int Compare(LogicalLocation left, Run leftRun, LogicalLocation right, Run rightRun)
        {
            // Look up LogicalLocations if these are indices only
            left = Resolve(left, leftRun);
            right = Resolve(right, rightRun);

            return String.Compare(left.FullyQualifiedName, right.FullyQualifiedName);
        }

        public static int Compare(PhysicalLocation left, Run leftRun, PhysicalLocation right, Run rightRun)
        {
            int cmp = 0;

            if (left == null && right == null) { return 0; }
            if (left == null) { return -1; }
            if (right == null) { return 1; }

            // Compare Uris first
            cmp = Compare(left.ArtifactLocation, leftRun, right.ArtifactLocation, rightRun);
            if (cmp != 0) { return cmp; }

            // Compare Region if Uris match
            cmp = Compare(left.Region, right.Region);
            if (cmp != 0) { return cmp; }

            return 0;
        }

        public static int Compare(ArtifactLocation left, Run leftRun, ArtifactLocation right, Run rightRun)
        {
            return Compare(ArtifactUri(left, leftRun), ArtifactUri(right, rightRun));
        }

        /// <summary>
        ///  Compare Regions for 'where' sorting. Does not compare snippets or messages.
        /// </summary>
        public static int Compare(Region left, Region right)
        {
            int cmp = 0;

            if (left == null && right == null) { return 0; }
            if (left == null) { return -1; }
            if (right == null) { return 1; }

            cmp = left.ByteOffset.CompareTo(right.ByteOffset);
            if (cmp != 0) { return cmp; }

            cmp = left.ByteLength.CompareTo(right.ByteLength);
            if (cmp != 0) { return cmp; }

            cmp = left.CharOffset.CompareTo(right.CharOffset);
            if (cmp != 0) { return cmp; }

            cmp = left.CharLength.CompareTo(right.CharLength);
            if (cmp != 0) { return cmp; }

            cmp = left.StartLine.CompareTo(right.StartLine);
            if (cmp != 0) { return cmp; }

            cmp = left.StartColumn.CompareTo(right.StartColumn);
            if (cmp != 0) { return cmp; }

            cmp = left.EndLine.CompareTo(right.EndLine);
            if (cmp != 0) { return cmp; }

            cmp = left.EndColumn.CompareTo(right.EndColumn);
            if (cmp != 0) { return cmp; }

            return cmp;
        }

        public static int Compare(Uri left, Uri right)
        {
            if (left == null && right == null) { return 0; }
            if (left == null) { return -1; }
            if (right == null) { return 1; }

            return left.AbsoluteUri.CompareTo(right.AbsoluteUri);
        }

        private static Uri ArtifactUri(ArtifactLocation loc, Run run)
        {
            return Resolve(loc, run)?.Uri;
        }

        private static LogicalLocation Resolve(LogicalLocation loc, Run run)
        {
            if (loc == null)
            {
                return null;
            }
            else if (loc.Index >= 0 && loc.Index < run?.LogicalLocations?.Count)
            {
                return run.LogicalLocations[loc.Index];
            }
            else
            {
                return loc;
            }
        }

        private static ArtifactLocation Resolve(ArtifactLocation loc, Run run)
        {
            if (loc == null)
            {
                return null;
            }
            else if (loc.Index >= 0 && loc.Index < run?.Artifacts?.Count)
            {
                return run.Artifacts[loc.Index].Location;
            }
            else
            {
                return loc;
            }
        }
    }
}
