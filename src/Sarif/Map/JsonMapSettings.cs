// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.CodeAnalysis.Sarif.Map
{
    /// <summary>
    ///  JsonMapSettings contains settings to control how JsonMaps are constructed.
    ///  For now, this is only the Map size target, to determine how much detail the map will include.
    /// </summary>
    public class JsonMapSettings
    {
        public const long NodeSizeEstimateBytes = 90;         // Bytes for one Node with containing property but not including children
        public const long ArrayStartSizeEstimateBytes = 5;    // Bytes for one ArrayStart location, if under 10KB.
        public const long Megabyte = 1024 * 1024;

        public double MapDesiredSizeRatio { get; private set; }
        public double MapMaximumSizeBytes { get; private set; }

        /// <summary>
        ///  Default Settings: Map is 1% of size of file mapped; up to a limit of 10 MB.
        /// </summary>
        public static JsonMapSettings DefaultSettings => new JsonMapSettings(0.01, 10 * Megabyte);

        /// <summary>
        ///  Construct JsonMapSettings for a given target size ratio and size limit.
        /// </summary>
        /// <param name="mapSizeRatio">Target Map size (0.01 means map should be 1% of file size of JSON being mapped)</param>
        /// <param name="mapMaximumSizeBytes">Map size limit, in bytes (ex: 10 * JsonMapSettings.Megabyte), 0 for no limit</param>
        public JsonMapSettings(double mapSizeRatio, double mapMaximumSizeBytes = 0)
        {
            if (mapSizeRatio <= 0 || mapSizeRatio > 100) { throw new ArgumentOutOfRangeException("maxFileSizePercentage must be > 0 and <= 100."); }
            MapDesiredSizeRatio = mapSizeRatio;
            MapMaximumSizeBytes = mapMaximumSizeBytes;
        }
    }

    /// <summary>
    ///  JsonMapRunSettings combines user-provided JsonMapSettings with calculated values
    ///  based on the specific file being analyzed.
    /// </summary>
    internal class JsonMapRunSettings : JsonMapSettings
    {
        public double CurrentSizeRatio { get; private set; }
        public int MinimumSizeForNode { get; private set; }

        public JsonMapRunSettings(double fileSizeBytes, JsonMapSettings userSettings)
            : base(userSettings.MapDesiredSizeRatio, userSettings.MapMaximumSizeBytes)
        {
            double expectedSize = fileSizeBytes * MapDesiredSizeRatio;

            // Calculate real ratio to use for this file (desired if file size permits)
            if (MapMaximumSizeBytes > 0 && expectedSize > MapMaximumSizeBytes)
            {
                CurrentSizeRatio = MapMaximumSizeBytes / fileSizeBytes;
            }
            else
            {
                CurrentSizeRatio = MapDesiredSizeRatio;
            }

            // Calculate the minimum node size which can fit into the map given the specific ratio
            MinimumSizeForNode = (int)(NodeSizeEstimateBytes / CurrentSizeRatio);
        }
    }
}
