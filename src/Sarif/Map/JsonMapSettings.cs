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

        public double CurrentSizeRatio { get; set; }
        public int MinimumSizeForNode { get; set; }

        public double MapDesiredSizeRatio { get; set; }
        public double MapMaximumSizeBytes { get; set; }

        /// <summary>
        ///  Default Settings: Map is 1% of size of file mapped; up to a limit of 10 MB.
        /// </summary>
        public static JsonMapSettings DefaultSettings => new JsonMapSettings(0.01, 10 * 1024 * 1024);

        /// <summary>
        ///  Construct JsonMapSettings for a given target size ratio and size limit.
        /// </summary>
        /// <param name="mapSizeRatio">Target Map size (0.01 means map should be 1% of file size or JSON being mapped)</param>
        /// <param name="mapMaximumSizeBytes">Map size limit, in bytes (10 * 1024 * 1024 means 10 MB), 0 for no limit</param>
        public JsonMapSettings(double mapSizeRatio, double mapMaximumSizeBytes = 0)
        {
            if (mapSizeRatio <= 0 || mapSizeRatio >= 100) { throw new ArgumentOutOfRangeException("maxFileSizePercentage must be between 0 and 100."); }
            MapDesiredSizeRatio = mapSizeRatio;
            MapMaximumSizeBytes = mapMaximumSizeBytes;

            CurrentSizeRatio = mapSizeRatio;
            Recalculate();
        }

        internal void AdjustForFileSize(double fileSizeBytes)
        {
            double expectedSize = fileSizeBytes * MapDesiredSizeRatio;
            if (MapMaximumSizeBytes > 0 && expectedSize > MapMaximumSizeBytes)
            {
                CurrentSizeRatio = MapMaximumSizeBytes / fileSizeBytes;
            }
            else
            {
                CurrentSizeRatio = MapDesiredSizeRatio;
            }

            Recalculate();
        }

        private void Recalculate()
        {
            // Recalculate node size limit (cache; used constantly)
            MinimumSizeForNode = (int)(NodeSizeEstimateBytes / CurrentSizeRatio);
        }
    }
}
