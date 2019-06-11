using Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching;

namespace Microsoft.CodeAnalysis.Sarif.Baseline
{
    public static class ExtractedResultMatchingExtensions
    {
        #region ExtractedResult Matching and Sorting        
        /// <summary>
        ///  Match the 'Category' of two ExtractedResults (Tool and RuleId)
        /// </summary>
        /// <param name="other">ExtractedResult to match</param>
        /// <returns>True if Category is identical, False otherwise</returns>
        public static bool MatchesCategory(this ExtractedResult me, ExtractedResult other)
        {
            return me.Result.RuleId == other.Result.RuleId;
        }

        /// <summary>
        ///  Match the 'What' properties of two ExtractedResults (Fingerprint and Snippet)
        /// </summary>
        /// <param name="other">ExtractedResult to match</param>
        /// <returns>True if *any* 'What' property matches, False otherwise</returns>
        public static bool MatchesWhat(this ExtractedResult me, ExtractedResult other)
        {
            if (me?.Result?.Fingerprints != null && other?.Result?.Fingerprints != null)
            {
                foreach (string fingerprintName in me.Result.Fingerprints.Keys)
                {
                    if (other.Result.Fingerprints.TryGetValue(fingerprintName, out string otherValue))
                    {
                        if (me.Result.Fingerprints[fingerprintName] == otherValue) { return true; }
                    }
                }
            }

            if (me?.Result?.Message?.Text == other?.Result?.Message?.Text)
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
        public static bool MatchesWhere(this ExtractedResult me, ExtractedResult other)
        {
            return CompareToWhere(me, other) == 0;
        }

        /// <summary>
        ///  Determine whether this ExtractedResult is 'sufficiently similar' to another.
        ///  An ExtractedResult must have the same category and either match a 'What' property or all 'Where' properties.
        /// </summary>
        /// <param name="other">ExtractedResult to match</param>
        /// <returns>True if ExtractedResults are 'sufficiently similar', otherwise False.</returns>
        public static bool IsSufficientlySimilarTo(this ExtractedResult me, ExtractedResult other)
        {
            return me.MatchesCategory(other) && (me.MatchesWhat(other) || me.MatchesWhere(other));
        }

        /// <summary>
        ///  Compare ExtractedResults based on their 'Where' properties only.
        /// </summary>
        /// <param name="left">ExtractedResult to compare</param>
        /// <param name="right">ExtractedResult to compare</param>
        /// <returns>0 if ExtractedResults have identical where properties, negative if 'left' sorts first, positive if 'right' sorts first</returns>
        public static int CompareToWhere(this ExtractedResult left, ExtractedResult right)
        {
            //int cmp = 0;

            //cmp = left.FileUri.CompareTo(right.FileUri);
            //if (cmp != 0) { return cmp; }

            //cmp = left.StartLine.CompareTo(right.StartLine);
            //if (cmp != 0) { return cmp; }

            //cmp = left.StartColumn.CompareTo(right.StartColumn);
            //if (cmp != 0) { return cmp; }

            //cmp = left.EndLine.CompareTo(right.EndLine);
            //if (cmp != 0) { return cmp; }

            //cmp = left.EndColumn.CompareTo(right.EndColumn);
            //if (cmp != 0) { return cmp; }

            return 0;
        }
        #endregion
    }
}
