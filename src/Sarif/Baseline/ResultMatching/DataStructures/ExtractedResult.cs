// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.


namespace Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching
{
    /// <summary>
    /// Result metadata for matching a group of results from a 
    /// set of runs (such as the current set of results) to a group of results 
    /// from a different set of runs (such as the prior set of results).
    /// </summary>
    public class ExtractedResult
    {
        public Result Result { get; set; }

        public Run OriginalRun { get; set; }

        /// <summary>
        ///  Match the 'Category' of two ExtractedResults (Tool and RuleId)
        /// </summary>
        /// <param name="other">ExtractedResult to match</param>
        /// <returns>True if Category is identical, False otherwise</returns>
        public bool MatchesCategory(ExtractedResult other)
        {
            return this.Result.RuleId == other.Result.RuleId;
        }

        /// <summary>
        ///  Match the 'What' properties of two ExtractedResults (Fingerprint and Snippet)
        /// </summary>
        /// <param name="other">ExtractedResult to match</param>
        /// <returns>True if *any* 'What' property matches, False otherwise</returns>
        public bool MatchesWhat(ExtractedResult other)
        {
            if (this?.Result?.Fingerprints != null && other?.Result?.Fingerprints != null)
            {
                foreach (string fingerprintName in this.Result.Fingerprints.Keys)
                {
                    if (other.Result.Fingerprints.TryGetValue(fingerprintName, out string otherValue))
                    {
                        if (this.Result.Fingerprints[fingerprintName] == otherValue) { return true; }
                    }
                }
            }

            if (this?.Result?.Message?.Text == other?.Result?.Message?.Text)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        ///  Match the 'Where' properties of two ExtractedResults (FileUri, StartLine/Column, EndLine/Column)
        /// </summary>
        /// <param name="other">ExtractedResult to match</param>
        /// <returns>True if *all* 'Where' properties match, False otherwise</returns>
        public bool MatchesWhere(ExtractedResult other)
        {
            return WhereComparer.CompareWhere(this, other) == 0;
        }

        /// <summary>
        ///  Determine whether this ExtractedResult is 'sufficiently similar' to another.
        ///  An ExtractedResult must have the same category and either match a 'What' property or all 'Where' properties.
        /// </summary>
        /// <param name="other">ExtractedResult to match</param>
        /// <returns>True if ExtractedResults are 'sufficiently similar', otherwise False.</returns>
        public bool IsSufficientlySimilarTo(ExtractedResult other)
        {
            return this.MatchesCategory(other) && (this.MatchesWhat(other) || this.MatchesWhere(other));
        }
    }
}
