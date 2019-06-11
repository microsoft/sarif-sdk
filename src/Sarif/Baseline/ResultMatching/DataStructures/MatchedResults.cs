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

        public ExtractedResult PreviousResult { get; set; }

        public ExtractedResult CurrentResult { get; set; }

        public Run Run { get; set; }

        public MatchedResults(ExtractedResult previous, ExtractedResult current)
        {
            this.PreviousResult = previous;
            this.CurrentResult = current;
            this.Run = current?.OriginalRun ?? previous?.OriginalRun;
        }

        /// <summary>
        /// Creates a new SARIF Result object with contents from the
        /// most recent result of the matched pair, the appropriate state,
        /// and some metadata in the property bag about the matching algorithm used.
        /// </summary>
        /// <returns>The new SARIF result.</returns>
        public Result CalculateBasedlinedResult(DictionaryMergeBehavior propertyBagMergeBehavior)
        {
            Result result;

            Dictionary<string, object> ResultMatchingProperties = new Dictionary<string, object>();
            Dictionary<string, object> OriginalResultMatchingProperties = null;
            if (PreviousResult != null && CurrentResult != null)
            {
                // Baseline result and current result have been matched => existing.
                result = ConstructExistingResult(ResultMatchingProperties, out OriginalResultMatchingProperties);
            }
            else if (PreviousResult == null && CurrentResult != null)
            {
                // No baseline result, present current result => new.
                result = ConstructNewResult(ResultMatchingProperties, out OriginalResultMatchingProperties);
            }
            else if (PreviousResult != null && CurrentResult == null)
            {
                // Baseline result is present, current result is missing => absent.
                result = ConstructAbsentResult(ResultMatchingProperties, out OriginalResultMatchingProperties);
            }
            else
            {
                throw new InvalidOperationException("Cannot generate a result for a new baseline where both results are null.");
            }

            ResultMatchingProperties = MergeDictionaryPreferFirst(ResultMatchingProperties, OriginalResultMatchingProperties);

            if (PreviousResult != null &&
                propertyBagMergeBehavior == DictionaryMergeBehavior.InitializeFromOldest)
            {
                result.Properties = PreviousResult.Result.Properties;
            }

            result.SetProperty(SarifLogResultMatcher.ResultMatchingResultPropertyName, ResultMatchingProperties);

            return result;
        }

        private Result ConstructAbsentResult(
            Dictionary<string, object> ResultMatchingProperties, 
            out Dictionary<string, object> OriginalResultMatchingProperties)
        {
            Result result = PreviousResult.Result.DeepClone();
            result.BaselineState = BaselineState.Absent;

            if (!PreviousResult.Result.TryGetProperty(SarifLogResultMatcher.ResultMatchingResultPropertyName, out OriginalResultMatchingProperties))
            {
                OriginalResultMatchingProperties = new Dictionary<string, object>();
            }

            if (PreviousResult.OriginalRun.AutomationDetails?.Guid != null)
            {
                ResultMatchingProperties.Add(MatchedResults.MatchResultMetadata_RunKeyName, PreviousResult.OriginalRun.AutomationDetails.Guid);
            }

            Run = PreviousResult.OriginalRun;

            return result;
        }

        private Result ConstructNewResult(
            Dictionary<string, object> ResultMatchingProperties, 
            out Dictionary<string, object> OriginalResultMatchingProperties)
        {
            // Result is New.
            Result result = CurrentResult.Result.DeepClone();
            result.CorrelationGuid = Guid.NewGuid().ToString();
            result.BaselineState = BaselineState.New;

            if (!CurrentResult.Result.TryGetProperty(SarifLogResultMatcher.ResultMatchingResultPropertyName, out OriginalResultMatchingProperties))
            {
                OriginalResultMatchingProperties = new Dictionary<string, object>();
            }

            if (CurrentResult.OriginalRun.AutomationDetails?.Guid != null)
            {
                ResultMatchingProperties.Add(MatchedResults.MatchResultMetadata_RunKeyName, CurrentResult.OriginalRun.AutomationDetails.Guid);
            }

            // Potentially temporary -- we persist the "originally found date" forward, and this sets it.
            if (CurrentResult.OriginalRun?.Invocations?.Any() == true && CurrentResult.OriginalRun.Invocations[0].StartTimeUtc != null)
            {
                ResultMatchingProperties.Add(MatchedResults.MatchResultMetadata_FoundDateName, CurrentResult.OriginalRun.Invocations[0].StartTimeUtc);
            }

            Run = CurrentResult.OriginalRun;

            return result;
        }

        private Result ConstructExistingResult(
            Dictionary<string, object> ResultMatchingProperties, 
            out Dictionary<string, object> OriginalResultMatchingProperties)
        {
            // Result exists.
            Result  result = CurrentResult.Result.DeepClone();
            result.CorrelationGuid = PreviousResult.Result.CorrelationGuid;
            result.Suppressions = PreviousResult.Result.Suppressions;
            result.BaselineState = BaselineState.Unchanged;

            if (!PreviousResult.Result.TryGetProperty(SarifLogResultMatcher.ResultMatchingResultPropertyName, out OriginalResultMatchingProperties))
            {
                OriginalResultMatchingProperties = new Dictionary<string, object>();
            }

            if (CurrentResult.OriginalRun.AutomationDetails?.Guid != null)
            {
                ResultMatchingProperties.Add(MatchedResults.MatchResultMetadata_RunKeyName, CurrentResult.OriginalRun.AutomationDetails.Guid);
            }

            Run = CurrentResult.OriginalRun;

            return result;
        }

        private Dictionary<string, object> MergeDictionaryPreferFirst(
            Dictionary<string, object> firstPropertyBag, 
            Dictionary<string, object> secondPropertyBag)
        {
            Dictionary<string, object> result = firstPropertyBag;

            foreach (string key in secondPropertyBag.Keys)
            {
                if (!result.ContainsKey(key))
                {
                    result[key] = secondPropertyBag[key];
                }
            }
            return result;
        }

        public override string ToString()
        {
            if (PreviousResult != null && CurrentResult != null)
            {
                return $"= {PreviousResult}";
            }
            else if (PreviousResult == null)
            {
                return $"+ {CurrentResult}";
            }
            else
            {
                return $"- {PreviousResult}";
            }
        }
    }
}