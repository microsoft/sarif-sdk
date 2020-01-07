using Microsoft.CodeAnalysis.Sarif;
using SarifBaseline.Extensions;
using System;
using System.IO;
using System.Linq;

namespace Test.EndToEnd.Baselining
{
    /// <summary>
    ///  Custom Result property formatting for BaselineE2E debugging.
    /// </summary>
    public static class ResultTextFormatter
    {
        public static string PartialFingerprint(this Result result, string fingerprintName, int lengthLimit = -1)
        {
            if (result == null) { return ""; }
            if (!result.PartialFingerprints.TryGetValue(fingerprintName, out string value)) { return ""; }

            if (lengthLimit > 0 && value.Length > lengthLimit) { value = value.Substring(0, lengthLimit); }
            return value;
        }

        public static string Snippet(this Result result, int lengthLimit = -1)
        {
            string value = result?.EnumeratePhysicalLocations().FirstOrDefault()?.Region?.Snippet?.Text ?? "";
            if (lengthLimit > 0 && value.Length > lengthLimit) { value = value.Substring(0, lengthLimit); }
            value = value.Replace("\n", "").Replace("\r", "");
            return value;
        }

        public static string FirstLocation(this Result result)
        {
            PhysicalLocation pLoc = result.EnumeratePhysicalLocations().FirstOrDefault();
            if (pLoc == null) { return ""; }

            Uri uri = pLoc.FileUri(result.Run);

            return $"{Path.GetFileName(uri?.OriginalString)}{RegionString(pLoc.Region)}";
        }

        public static string RegionString(this Region region)
        {
            // NOTE: Similar to but not the same as FormatForVisualStudio(Region); this has offset handling and some other tweaks
            if (region == null) { return ""; }

            if (region.EndLine != region.StartLine)
            {
                // "(15, 13 - 19, 26)"
                return $"({region.StartLine}, {region.StartColumn} - {region.EndLine}, {region.EndColumn})";
            }
            else if (region.EndColumn > region.StartColumn)
            {
                // "(15, 13-26)"
                return $"({region.StartLine}, {region.StartColumn}-{region.EndColumn})";
            }
            else if (region.StartColumn != 0)
            {
                // "(15, 13)"
                return $"({region.StartLine}, {region.StartColumn})";
            }
            else if (region.StartLine != 0)
            {
                // "(15)"
                return $"({region.StartLine})";
            }
            else if (region.CharOffset != 0)
            {
                // "(c 290-320)"
                return $"(c {region.CharOffset}-{region.CharOffset + region.CharLength})";
            }
            else if (region.ByteOffset != 0)
            {
                // "(b 120-140)
                return $"(b {region.ByteOffset}-{region.ByteOffset + region.ByteLength})";
            }
            else
            {
                return "";
            }
        }
    }
}
