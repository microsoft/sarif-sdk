// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching
{
    /// <summary>
    /// This class associates a Result object with additional metadata that
    /// makes it possible for a matching algorithm to determine if it matches
    /// a result from another run.
    /// </summary>
    /// <remarks>
    /// If a matching algorithm determines that two such results _do_ match, it
    /// will construct a <see cref="MatchedResults"/> object to capture the association.
    /// </remarks>
    public class ExtractedResult
    {
        public Result Result { get; set; }
        public Run OriginalRun { get; set; }
        public string RuleId { get; set; }

        private readonly ReportingDescriptor _rule;
        private readonly bool _hasDeprecatedIds;

        public ExtractedResult(Result result, Run run)
        {
            Result = result;
            OriginalRun = run;

            // Ensure Result.Run is set (if not previously set)
            result.Run = result.Run ?? run;

            // Look up and cache RuleId, Rule
            RuleId = result.ResolvedRuleId(result.Run);

            if (result.Run != null)
            {
                _rule = result.GetRule(result.Run);
                _hasDeprecatedIds = _rule?.DeprecatedIds?.Count > 0;
            }
        }

        /// <summary>
        ///  Match the 'Category' of two ExtractedResults (Tool and RuleId).
        /// </summary>
        /// <param name="other">ExtractedResult to match</param>
        /// <returns>True if Category is identical, False otherwise</returns>
        public bool MatchesCategory(ExtractedResult other)
        {
            if (!this._hasDeprecatedIds && !other._hasDeprecatedIds)
            {
                return (this.RuleId == other.RuleId);
            }

            // Handle ReportingDescriptor.DeprecatedIds (rare)
            if (other._hasDeprecatedIds && other._rule.DeprecatedIds.Contains(this.RuleId))
            {
                return true;
            }

            if (this._hasDeprecatedIds && this._rule.DeprecatedIds.Contains(other.RuleId))
            {
                return true;
            }

            return false;

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
        ///  Match enough of the 'What' properties of two ExtractedResults (Guid, Fingerprints, PartialFingerprints, Snippets, Message, Properties).
        ///  A match in high-confidence identity properties is a match (Guid, Fingerprint, >= 50% of PartialFingerprint).
        ///  A non-match in high-confidence identity properties is a non-match (Fingerprint, 0% of PartialFingerprints, Properties).
        ///  Otherwise, Results match if Message and first Snippet match.
        /// </summary>
        /// <param name="other">ExtractedResult to match</param>
        /// <param name="trustMap">TrustMap for either Run being compared, to weight attributes selectively</param>
        /// <returns>True if *any* 'What' property matches, False otherwise</returns>
        internal bool MatchesAnyWhat(ExtractedResult other, TrustMap trustMap)
        {
            return WhatComparer.MatchesWhat(this, other, trustMap);
        }

        /// <summary>
        ///  Match all of the 'Where' properties of two ExtractedResults (FileUri, StartLine/Column, EndLine/Column).
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

        /// <summary>
        ///  Determine whether this ExtractedResult is 'sufficiently similar' to another.
        ///  An ExtractedResult must have the same category and either match a 'What' property or all 'Where' properties.
        /// </summary>
        /// <param name="other">ExtractedResult to match</param>
        /// <param name="trustMap">TrustMap for either Run being compared, to weight attributes selectively</param>
        /// <returns>True if ExtractedResults are 'sufficiently similar', otherwise False.</returns>
        internal bool IsSufficientlySimilarTo(ExtractedResult other, TrustMap trustMap)
        {
            return this.MatchesCategory(other) && (this.MatchesAnyWhat(other, trustMap) || this.MatchesAllWhere(other));
        }

        public override string ToString()
        {
            return $"{Result.FormatForVisualStudio(Result.GetRule(OriginalRun))}";
        }
    }
}
