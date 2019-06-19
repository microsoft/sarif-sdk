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
        public string RuleId { get; set; }

        public ExtractedResult(Result result, Run run)
        {
            Result = result;
            OriginalRun = run;

            // Look up and cache the RuleId
            RuleId = result.ResolvedRuleId(run);
        }

        /// <summary>
        ///  Match the 'Category' of two ExtractedResults (Tool and RuleId)
        /// </summary>
        /// <param name="other">ExtractedResult to match</param>
        /// <returns>True if Category is identical, False otherwise</returns>
        public bool MatchesCategory(ExtractedResult other)
        {
            return this.RuleId == other.RuleId;

            // Tool (checked on Runs beforehand)
            // Consider: CorrelationGuid, Level, Rank?
        }

        /// <summary>
        ///  Match the 'What' properties of two ExtractedResults (Fingerprint and Snippet)
        /// </summary>
        /// <param name="other">ExtractedResult to match</param>
        /// <returns>True if *any* 'What' property matches, False otherwise</returns>
        public bool MatchesWhat(ExtractedResult other)
        {
            return WhatComparer.MatchesWhat(this, other);
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

        public override string ToString()
        {
            return $"{Result.FormatForVisualStudio(Result.GetRule(OriginalRun))}";
        }
    }
}
