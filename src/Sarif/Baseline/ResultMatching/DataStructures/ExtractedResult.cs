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

            // Tool contributes to category, but SarifLogMatcher ensures only Runs with matching Tools are compared,
            // so we don't check here.
        }

        /// <summary>
        ///  Match enough of the 'What' properties of two ExtractedResults (Guid, Fingerprints, PartialFingerprints, Snippets, Message, Properties).
        ///  A match in high-confidence identity properties is a match (Guid, Fingerprint, >= 50% of PartialFingerprint).
        ///  A non-match in high-confidence identity properties is a non-match (Fingerprint, 0% of PartialFingerprints, Properties).
        ///  Otherwise, Results match if Message and first Snippet match.
        /// </summary>
        /// <param name="other">ExtractedResult to match</param>
        /// <returns>True if *any* 'What' property matches, False otherwise</returns>
        public bool MatchesAnyWhat(ExtractedResult other)
        {
            return WhatComparer.MatchesWhat(this, other);
        }

        /// <summary>
        ///  Match all of the 'Where' properties of two ExtractedResults (FileUri, StartLine/Column, EndLine/Column)
        /// </summary>
        /// <param name="other">ExtractedResult to match</param>
        /// <returns>True if *all* 'Where' properties match, False otherwise</returns>
        public bool MatchesAllWhere(ExtractedResult other)
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
            return this.MatchesCategory(other) && (this.MatchesAnyWhat(other) || this.MatchesAllWhere(other));
        }

        public override string ToString()
        {
            return $"{Result.FormatForVisualStudio(Result.GetRule(OriginalRun))}";
        }
    }
}
