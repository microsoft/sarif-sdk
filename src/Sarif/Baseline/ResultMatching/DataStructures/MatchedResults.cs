// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching
{
    /// <summary>
    /// 
    /// </summary>
    public class MatchedResults
    {
        public MatchingResult BaselineResult;

        public MatchingResult CurrentResult;

        public IResultMatcher MatchingAlgorithm;


        public Result CalculateNewBaselineResult()
        {
            Result result = null;

            Dictionary<string, string> ResultMatchingProperties = new Dictionary<string, string>();
            if (BaselineResult != null && CurrentResult != null)
            {
                result = CurrentResult.Result.DeepClone();
                result.Id = BaselineResult.Result.Id;
                result.SuppressionStates = BaselineResult.Result.SuppressionStates;
                result.BaselineState = BaselineState.Existing;

                if (CurrentResult.OriginalRun.Id != null)
                {
                    ResultMatchingProperties.Add("Run", CurrentResult.OriginalRun.Id);
                }
            }
            else if (BaselineResult == null && CurrentResult != null)
            {
                result = CurrentResult.Result.DeepClone();
                result.Id = Guid.NewGuid().ToString();
                result.BaselineState = BaselineState.New;
                
                if (CurrentResult.OriginalRun.Id != null)
                {
                    ResultMatchingProperties.Add("Run", CurrentResult.OriginalRun.Id);
                }
            }
            else if (BaselineResult != null && CurrentResult == null)
            {
                result = BaselineResult.Result.DeepClone();
                result.BaselineState = BaselineState.Absent;
                
                if (BaselineResult.OriginalRun.Id != null)
                {
                    ResultMatchingProperties.Add("Run", BaselineResult.OriginalRun.Id);
                }

            }
            else
            {
                throw new InvalidOperationException("Cannot generate a Result for a new baseline where both results are null.");
            }

            result.SetProperty(ResultMatchingBaseliner.ResultMatchingResultPropertyName, ResultMatchingProperties);

            return result;
        }
    }
}