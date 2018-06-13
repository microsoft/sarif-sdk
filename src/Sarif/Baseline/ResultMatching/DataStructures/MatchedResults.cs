﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

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
            if (BaselineResult != null && CurrentResult != null)
            {
                Result result = CurrentResult.Result.DeepClone();
                result.Id = BaselineResult.Result.Id;
                result.SuppressionStates = BaselineResult.Result.SuppressionStates;
                result.BaselineState = BaselineState.Existing;
                return result;
            }
            else if (BaselineResult == null && CurrentResult != null)
            {
                Result result = CurrentResult.Result.DeepClone();
                result.Id = Guid.NewGuid().ToString();
                result.BaselineState = BaselineState.New;
                return result;
            }
            else if (BaselineResult != null && CurrentResult == null)
            {
                Result result = BaselineResult.Result.DeepClone();
                result.BaselineState = BaselineState.Absent;
                return result;
            }
            else
            {
                throw new InvalidOperationException("Cannot generate a Result for a new baseline where both results are null.");
            }
        }
    }
}