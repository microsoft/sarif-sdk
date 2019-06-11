using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching;

namespace Microsoft.CodeAnalysis.Sarif.Baseline
{
    /// <summary>
    ///  Comparer for sorting Results by their 'Where' properties.
    /// </summary>
    public class ResultWhereComparer : IComparer<ExtractedResult>
    {
        public static ResultWhereComparer Instance = new ResultWhereComparer();

        public int Compare(ExtractedResult left, ExtractedResult right)
        {
            return ExtractedResultMatchingExtensions.CompareToWhere(left, right);
        }
    }
}