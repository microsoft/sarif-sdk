// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching.ExactMatchers;
using Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching.HeuristicMatchers;

namespace Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching
{
    public class ResultMatchingBaselinerFactory
    {
        /// <summary>
        /// Get a Result Matching Baseliner that matches results between two groups of Sarif Logs using a sensible default set of rules for matching results.
        /// </summary>
        /// <returns>A result matching baseliner instance with the default set of strategies.</returns>
        public static ISarifLogMatcher GetDefaultResultMatchingBaseliner(bool oldAlgorithm = false)
        {
            if (oldAlgorithm)
            {
                return new SarifLogResultMatcher
                    (
                        // Exact matchers run first, in order.  These should do *no* remapping and offer fast comparisons to filter out
                        // common cases (e.x. identical results).
                        new List<IResultMatcher>()
                        {
                            ExactResultMatcherFactory.GetIdenticalResultMatcher(considerPropertyBagsWhenComparing: true)
                        },
                        // Heuristic matchers run in order after the exact matchers.
                        // These can do remapping, and catch the long tail of "changed" results.
                        new List<IResultMatcher>()
                        {
                            HeuristicResultMatcherFactory.GetPartialFingerprintResultMatcher()
                        }
                    );
            }
            else
            {
                return new SarifLogResultMatcher(
                    new List<IResultMatcher>() { new V2ResultMatcher() },
                    new List<IResultMatcher>()
                );
            }
        }

        /// <summary>
        /// Get a result matching baseliner that matches results between two groups of sarif logs using a 
        /// </summary>
        /// <param name="exactMatchers"></param>
        /// <param name="heuristicMatchers"></param>
        /// <returns></returns>
        public static ISarifLogMatcher GetResultMatchingBaseliner(List<IResultMatcher> exactMatchers, List<IResultMatcher> heuristicMatchers)
        {
            return new SarifLogResultMatcher(exactMatchers, heuristicMatchers);
        }
    }
}
