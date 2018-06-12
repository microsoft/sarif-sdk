using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching.HeuristicMatchers
{
    public static class HeuristicResultMatcherFactory
    {
        public static IResultMatcher GetContextRegionHeuristicResultMatcher()
        {
            return new ContextRegionHeuristicMatcher();
        }
        
        public static IResultMatcher GetPartialFingerprintResultMatcher()
        {
            return new PartialFingerprintResultMatcher();
        }
    }
}
