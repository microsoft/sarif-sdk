// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching
{
    /// <summary>
    /// A pair of results that have been matched by a matching algorithm.
    /// </summary>
    public class MatchedResults
    {
        private const string MatchResultMetadata_RunKeyName = "Run";
        private const string MatchResultMetadata_FoundDateName = "FoundDate";
        private const string MatchResultMetadata_PreviousGuid = "PreviousGuid";

        public ExtractedResult PreviousResult { get; }

        public ExtractedResult CurrentResult { get; }

        public Run Run { get; set; }

        public MatchedResults(ExtractedResult previous, ExtractedResult current)
        {
            if (previous == null && current == null) { throw new ArgumentException($"MatchedResults requires at least one non-null Result."); }
            this.PreviousResult = previous;
            this.CurrentResult = current;
            this.Run = current?.OriginalRun ?? previous?.OriginalRun;
        }

        /// <summary>
        /// Creates a new SARIF Result object with contents from the most recent result of the
        /// matched pair, the appropriate state and time of first detection, and some metadata
        /// in the property bag about the matching algorithm used.
        /// </summary>
        /// <returns>The new SARIF result.</returns>
        public Result CalculateBasedlinedResult(DictionaryMergeBehavior propertyBagMergeBehavior)
        {
            Result result = null;

            Dictionary<string, object> resultMatchingProperties = new Dictionary<string, object>();
            Dictionary<string, object> originalResultMatchingProperties = null;
            if (PreviousResult != null && CurrentResult != null)
            {
                // Baseline result and current result have been matched => existing.
                result = ConstructExistingResult(resultMatchingProperties, out originalResultMatchingProperties);
            }
            else if (PreviousResult == null && CurrentResult != null)
            {
                // No baseline result, present current result => new.
                result = ConstructNewResult(resultMatchingProperties, out originalResultMatchingProperties);
            }
            else if (PreviousResult != null && CurrentResult == null)
            {
                // Baseline result is present, current result is missing => absent.
                result = ConstructAbsentResult(resultMatchingProperties, out originalResultMatchingProperties);
            }

            SetFirstDetectionTime(result);

            resultMatchingProperties = MergeDictionaryPreferFirst(resultMatchingProperties, originalResultMatchingProperties);

            if (PreviousResult != null &&
                propertyBagMergeBehavior == DictionaryMergeBehavior.InitializeFromOldest)
            {
                result.Properties = PreviousResult.Result.Properties;
            }

            result.SetProperty(SarifLogResultMatcher.ResultMatchingResultPropertyName, resultMatchingProperties);

            return result;
        }

        private Result ConstructAbsentResult(
            Dictionary<string, object> resultMatchingProperties,
            out Dictionary<string, object> originalResultMatchingProperties)
        {
            Result result = CreateBaselinedResult(BaselineState.Absent);

            if (!PreviousResult.Result.TryGetProperty(SarifLogResultMatcher.ResultMatchingResultPropertyName, out originalResultMatchingProperties))
            {
                originalResultMatchingProperties = new Dictionary<string, object>();
            }

            if (PreviousResult.OriginalRun?.AutomationDetails?.Guid != null)
            {
                resultMatchingProperties.Add(MatchedResults.MatchResultMetadata_RunKeyName, PreviousResult.OriginalRun.AutomationDetails.Guid);
            }

            Run = PreviousResult.OriginalRun;

            return result;
        }

        private Result ConstructNewResult(
            Dictionary<string, object> resultMatchingProperties,
            out Dictionary<string, object> originalResultMatchingProperties)
        {
            // Result is New.
            Result result = CreateBaselinedResult(BaselineState.New);

            if (!CurrentResult.Result.TryGetProperty(SarifLogResultMatcher.ResultMatchingResultPropertyName, out originalResultMatchingProperties))
            {
                originalResultMatchingProperties = new Dictionary<string, object>();
            }

            if (CurrentResult.OriginalRun?.AutomationDetails?.Guid != null)
            {
                resultMatchingProperties.Add(MatchedResults.MatchResultMetadata_RunKeyName, CurrentResult.OriginalRun.AutomationDetails.Guid);
            }

            // Potentially temporary -- we persist the "originally found date" forward, and this sets it.
            if (CurrentResult.OriginalRun?.Invocations?[0]?.StartTimeUtc != null)
            {
                resultMatchingProperties.Add(MatchedResults.MatchResultMetadata_FoundDateName, CurrentResult.OriginalRun.Invocations[0].StartTimeUtc);
            }

            Run = CurrentResult.OriginalRun;

            return result;
        }

        private Result ConstructExistingResult(
            Dictionary<string, object> resultMatchingProperties,
            out Dictionary<string, object> originalResultMatchingProperties)
        {
            // Result exists.
            Result result = CreateBaselinedResult(BaselineState.Unchanged);

            if (!PreviousResult.Result.TryGetProperty(SarifLogResultMatcher.ResultMatchingResultPropertyName, out originalResultMatchingProperties))
            {
                originalResultMatchingProperties = new Dictionary<string, object>();
            }

            if (CurrentResult.OriginalRun.AutomationDetails?.Guid != null)
            {
                resultMatchingProperties.Add(MatchedResults.MatchResultMetadata_RunKeyName, CurrentResult.OriginalRun.AutomationDetails.Guid);
            }

            if (PreviousResult.Result.Guid != null)
            {
                resultMatchingProperties.Add(MatchedResults.MatchResultMetadata_PreviousGuid, PreviousResult.Result.Guid);
            }

            Run = CurrentResult.OriginalRun;

            return result;
        }

        private Result CreateBaselinedResult(BaselineState newBaselineState)
        {
            // Clone the most recent copy of the Result
            Result result = (CurrentResult ?? PreviousResult).Result.DeepClone();

            // Assign a Guid, if not assigned by the Result producer
            result.Guid = result.Guid ?? Guid.NewGuid().ToString(SarifConstants.GuidFormat);

            // Assign a CorrelationGuid to map copies of this logical Result across baselined logs to each other
            if (PreviousResult?.Result == null)
            {
                // If this Result is new, use the Guid as the Correlation Guid for this 'series'
                result.CorrelationGuid = result.CorrelationGuid ?? result.Guid;
            }
            else
            {
                // If not, persist forward the CorrelationGuid already assigned
                //  or the Guid if the previous log was never baselined
                //  or the new Guid if the previous Result never had a Guid assigned at all
                result.CorrelationGuid = PreviousResult.Result.CorrelationGuid ?? PreviousResult.Result.Guid ?? result.Guid;
            }

            result.BaselineState = newBaselineState;

            return result;
        }

        internal void SetFirstDetectionTime(Result targetResult)
        {
            DateTime firstDetectionTime = default;

            // For existing results (or results that have disappeared), get the first detection
            // time from the previous result and its associated run. For new results, get the first
            // detection time from the current result and its associated run.
            ExtractedResult extractedResult;
            switch (targetResult.BaselineState)
            {
                case BaselineState.Absent:
                case BaselineState.Updated:
                case BaselineState.Unchanged:
                    extractedResult = PreviousResult;
                    break;

                case BaselineState.New:
                    extractedResult = CurrentResult;
                    break;

                default:
                    throw new ArgumentException("Baseline state was not set.", nameof(targetResult));
            }

            Result sourceResult = extractedResult.Result;
            if (sourceResult.Provenance != null && sourceResult.Provenance.FirstDetectionTimeUtc != default)
            {
                firstDetectionTime = sourceResult.Provenance.FirstDetectionTimeUtc;
            }
            else
            {
                Invocation invocation = extractedResult.OriginalRun?.Invocations?[0];
                if (invocation != null)
                {
                    firstDetectionTime = invocation.EndTimeUtc != default
                        ? invocation.EndTimeUtc
                        : invocation.StartTimeUtc;
                }
            }

            if (firstDetectionTime == default)
            {
                firstDetectionTime = DateTime.UtcNow;
            }

            if (targetResult.Provenance == null)
            {
                targetResult.Provenance = new ResultProvenance();
            }

            targetResult.Provenance.FirstDetectionTimeUtc = firstDetectionTime;
        }

        private Dictionary<string, object> MergeDictionaryPreferFirst(
            Dictionary<string, object> firstPropertyBag,
            Dictionary<string, object> secondPropertyBag)
        {
            Dictionary<string, object> mergedPropertyBag = firstPropertyBag;

            foreach (string key in secondPropertyBag.Keys)
            {
                if (!mergedPropertyBag.ContainsKey(key))
                {
                    mergedPropertyBag[key] = secondPropertyBag[key];
                }
            }
            return mergedPropertyBag;
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