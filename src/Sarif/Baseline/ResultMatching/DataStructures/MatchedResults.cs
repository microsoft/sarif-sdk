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
        
        /// <summary>
        /// Creates a new SARIF Result object with contents from the
        /// most recent result of the matched pair, the appropriate state,
        /// and some metadata in the property bag about the matching algorithm used.
        /// </summary>
        /// <returns>The new SARIF result.</returns>
        public Result CalculateNewBaselineResult()
        {
            Result result = null;

            Dictionary<string, object> ResultMatchingProperties = new Dictionary<string, object>();
            Dictionary<string, object> OriginalResultMatchingProperties = null;
            if (BaselineResult != null && CurrentResult != null)
            {
                // Baseline result and current result have been matched => existing.
                result = ExistingResult(ResultMatchingProperties, out OriginalResultMatchingProperties);
            }
            else if (BaselineResult == null && CurrentResult != null)
            {
                // No baseline result, present current result => new.
                result = NewResult(ResultMatchingProperties, out OriginalResultMatchingProperties);
            }
            else if (BaselineResult != null && CurrentResult == null)
            {
                // Baseline result is present, current result is missing => absent.
                result = AbsentResult(ResultMatchingProperties, out OriginalResultMatchingProperties);
            }
            else
            {
                throw new InvalidOperationException("Cannot generate a Result for a new baseline where both results are null.");
            }

            ResultMatchingProperties = MergeDictionaryPreferFirst(ResultMatchingProperties, OriginalResultMatchingProperties);

            result.SetProperty(ResultMatchingBaseliner.ResultMatchingResultPropertyName, ResultMatchingProperties);

            return result;
        }

        private Result AbsentResult(
            Dictionary<string, object> ResultMatchingProperties, 
            out Dictionary<string, object> OriginalResultMatchingProperties)
        {
            Result result = BaselineResult.Result.DeepClone();
            result.BaselineState = BaselineState.Absent;

            if (!BaselineResult.Result.TryGetProperty(ResultMatchingBaseliner.ResultMatchingResultPropertyName, out OriginalResultMatchingProperties))
            {
                OriginalResultMatchingProperties = new Dictionary<string, object>();
            }

            if (BaselineResult.OriginalRun.InstanceGuid != null)
            {
                ResultMatchingProperties.Add(MatchedResults.MatchResultMetadata_RunKeyName, BaselineResult.OriginalRun.InstanceGuid);
            }
            return result;
        }

        private Result NewResult(
            Dictionary<string, object> ResultMatchingProperties, 
            out Dictionary<string, object> OriginalResultMatchingProperties)
        {
            // Result is New.
            Result result = CurrentResult.Result.DeepClone();
            result.CorrelationGuid = Guid.NewGuid().ToString();
            result.BaselineState = BaselineState.New;

            if (!CurrentResult.Result.TryGetProperty(ResultMatchingBaseliner.ResultMatchingResultPropertyName, out OriginalResultMatchingProperties))
            {
                OriginalResultMatchingProperties = new Dictionary<string, object>();
            }

            if (CurrentResult.OriginalRun.InstanceGuid != null)
            {
                ResultMatchingProperties.Add(MatchedResults.MatchResultMetadata_RunKeyName, CurrentResult.OriginalRun.InstanceGuid);
            }

            // Potentially temporary -- we persist the "originally found date" forward, and this sets it.
            if (CurrentResult.OriginalRun.Invocations != null && CurrentResult.OriginalRun.Invocations.Any() && CurrentResult.OriginalRun.Invocations[0].StartTime != null)
            {
                ResultMatchingProperties.Add(MatchedResults.MatchResultMetadata_FoundDateName, CurrentResult.OriginalRun.Invocations[0].StartTime);
            }
            return result;
        }

        private Result ExistingResult(
            Dictionary<string, object> ResultMatchingProperties, 
            out Dictionary<string, object> OriginalResultMatchingProperties)
        {
            // Result exists.
            Result  result = CurrentResult.Result.DeepClone();
            result.CorrelationGuid = BaselineResult.Result.CorrelationGuid;
            result.SuppressionStates = BaselineResult.Result.SuppressionStates;
            result.BaselineState = BaselineState.Existing;

            if (!BaselineResult.Result.TryGetProperty(ResultMatchingBaseliner.ResultMatchingResultPropertyName, out OriginalResultMatchingProperties))
            {
                OriginalResultMatchingProperties = new Dictionary<string, object>();
            }

            if (CurrentResult.OriginalRun.InstanceGuid != null)
            {
                ResultMatchingProperties.Add(MatchedResults.MatchResultMetadata_RunKeyName, CurrentResult.OriginalRun.InstanceGuid);
            }

            return result;
        }

        private Dictionary<string, object> MergeDictionaryPreferFirst(
            Dictionary<string, object> firstPropertyBag, 
            Dictionary<string, object> secondPropertyBag)
        {
            Dictionary<string, object> result = firstPropertyBag;

            foreach (var key in secondPropertyBag.Keys)
            {
                if (!result.ContainsKey(key))
                {
                    result[key] = secondPropertyBag[key];
                }
            }
            return result;
        }
    }
}