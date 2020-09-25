// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    /// <summary>
    /// Represents one row of a FlawFinder CSV file.
    /// </summary>
    internal class FlawFinderCsvResult
    {
        public string File { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }
        public int Level { get; set; }
        public string Category { get; set; }
        public string Name { get; set; }
        public string Warning { get; set; }
        public string Suggestion { get; set; }
        public string Note { get; set; }
        public string CWEs { get; set; }
        public string Context { get; set; }
        public string Fingerprint { get; set; }
    }
}
