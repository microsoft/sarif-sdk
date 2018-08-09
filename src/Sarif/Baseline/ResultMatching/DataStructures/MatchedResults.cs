// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching
{
    /// <summary>
    /// A set of two results that have been matched by a matching algorithm.
    /// </summary>
    public class MatchedResults
    {
        private const string MatchResultMetadata_RunKeyName = "Run";
        private const string MatchResultMetadata_FoundDateName = "FoundDate";

        public MatchingResult BaselineResult;

        public MatchingResult CurrentResult;

        public IResultMatcher MatchingAlgorithm;
        
        public Result CalculateNewBaselineResult()
        {
            Result result = null;

            Dictionary<string, string> ResultMatchingProperties = new Dictionary<string, string>();
            Dictionary<string, string> OriginalResultMatchingProperties = null;
            if (BaselineResult != null && CurrentResult != null)
            {
                // Result exists.
                result = CurrentResult.Result.DeepClone();
                result.CorrelationGuid = BaselineResult.Result.CorrelationGuid;
                result.SuppressionStates = BaselineResult.Result.SuppressionStates;
                result.BaselineState = BaselineState.Existing;

                if (!BaselineResult.Result.TryGetProperty(ResultMatchingBaseliner.ResultMatchingResultPropertyName, out OriginalResultMatchingProperties))
                {
                    OriginalResultMatchingProperties = new Dictionary<string, string>();
                }

                if (CurrentResult.OriginalRun.InstanceGuid != null)
                {
                    ResultMatchingProperties.Add(MatchedResults.MatchResultMetadata_RunKeyName, CurrentResult.OriginalRun.InstanceGuid);
                }
            }
            else if (BaselineResult == null && CurrentResult != null)
            {
                // Result is New.
                result = CurrentResult.Result.DeepClone();
                result.CorrelationGuid = Guid.NewGuid().ToString();
                result.BaselineState = BaselineState.New;

                if (!CurrentResult.Result.TryGetProperty(ResultMatchingBaseliner.ResultMatchingResultPropertyName, out OriginalResultMatchingProperties))
                {
                    OriginalResultMatchingProperties = new Dictionary<string, string>();
                }

                if (CurrentResult.OriginalRun.InstanceGuid != null)
                {
                    ResultMatchingProperties.Add(MatchedResults.MatchResultMetadata_RunKeyName, CurrentResult.OriginalRun.InstanceGuid);
                }
                
                // Potentially temporary.
                if (CurrentResult.OriginalRun.Invocations != null && CurrentResult.OriginalRun.Invocations.Any() && CurrentResult.OriginalRun.Invocations[0].StartTime != null)
                {
                    ResultMatchingProperties.Add(MatchedResults.MatchResultMetadata_FoundDateName, CurrentResult.OriginalRun.Invocations[0].StartTime.ToString());
                }
                
            }
            else if (BaselineResult != null && CurrentResult == null)
            {
                // Result is absent.
                result = BaselineResult.Result.DeepClone();
                result.BaselineState = BaselineState.Absent;

                if (!BaselineResult.Result.TryGetProperty(ResultMatchingBaseliner.ResultMatchingResultPropertyName, out OriginalResultMatchingProperties))
                {
                    OriginalResultMatchingProperties = new Dictionary<string, string>();
                }

                if (BaselineResult.OriginalRun.InstanceGuid != null)
                {
                    ResultMatchingProperties.Add(MatchedResults.MatchResultMetadata_RunKeyName, BaselineResult.OriginalRun.InstanceGuid);
                }
            }
            else
            {
                throw new InvalidOperationException("Cannot generate a Result for a new baseline where both results are null.");
            }

            ResultMatchingProperties = MergeDictionaryPreferFirst(ResultMatchingProperties, OriginalResultMatchingProperties);

            result.SetProperty(ResultMatchingBaseliner.ResultMatchingResultPropertyName, ResultMatchingProperties);

            return result;
        }

        private Dictionary<string, string> MergeDictionaryPreferFirst(Dictionary<string, string> resultMatchingProperties, Dictionary<string, string> originalResultMatchingProperties)
        {
            Dictionary<string, string> result = resultMatchingProperties;

            foreach (var key in originalResultMatchingProperties.Keys)
            {
                if (!result.ContainsKey(key))
                {
                    result[key] = originalResultMatchingProperties[key];
                }
            }
            return result;
        }
    }
}