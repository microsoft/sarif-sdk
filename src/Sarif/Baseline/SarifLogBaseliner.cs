// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Baseline
{
    internal class SarifLogBaseliner : ISarifLogBaseliner
    {
        IEqualityComparer<Result> ResultComparator;

        public SarifLogBaseliner(IEqualityComparer<Result> comparator)
        {
            ResultComparator = comparator;
        }

        public Run CreateBaselinedRun(Run baseline, Run nextLog)
        {
            Run differencedRun = nextLog.DeepClone();
            differencedRun.Results = new List<Result>();

            DateTime now = DateTime.Now;
            DateTime baselineRunTime = GetRunTime(baseline, now);
            DateTime newRunTime = GetRunTime(nextLog, now);

            foreach (var result in nextLog.Results)
            {
                Result newResult = result.DeepClone();

                Result baselineResult = baseline.Results.FirstOrDefault(r => ResultComparator.Equals(r, newResult));

                SetResultBaselineInformation(newResult, baselineResult, newRunTime, baselineRunTime);

                differencedRun.Results.Add(newResult);
            }

            foreach (var result in baseline.Results)
            {
                if (!nextLog.Results.Contains(result, ResultComparator))
                {
                    Result newResult = result.DeepClone();
                    newResult.BaselineState = BaselineState.Absent;
                    differencedRun.Results.Add(newResult);
                }
            }

            return differencedRun;
        }

        // Get the execution time from the specified run. This time will be used
        // as the "first detection time" for results that don't already have that
        // information. If the run doesn't have execution time information, it
        // falls back to "now".
        internal static DateTime GetRunTime(Run run, DateTime now)
        {
            DateTime runTime;

            IList<Invocation> invocations = run.Invocations;
            if (invocations?.Count > 0)
            {
                Invocation invocation = invocations[0];
                runTime = invocation.EndTimeUtc;
                if (runTime == DateTime.MinValue)
                {
                    runTime = invocation.StartTimeUtc;
                }
                if (runTime == DateTime.MinValue)
                {
                    runTime = now;
                }
            }
            else
            {
                runTime = now;
            }

            return runTime;
        }

        // Set the baseline state and first detection time for the current result. If the
        // matching result from the baseline has a first detection time, roll it forward.
        // Otherwise, fall back to the time that the run was performed (for existing
        // results, look at the baseline run; for new results, look at the current run).
        // If the run doesn't have execution time information, it falls back to "now"
        // (see GetRunTime).
        internal static void SetResultBaselineInformation(
            Result currentResult,
            Result baselineResult, DateTime newRunTime, DateTime baselineRunTime)
        {
            if (baselineResult == null)
            {
                currentResult.BaselineState = BaselineState.New;
                SetFirstDetectionTime(currentResult, newRunTime);
            }
            else
            {
                currentResult.BaselineState = BaselineState.Unchanged;

                DateTime firstDetectionTime = baselineResult.Provenance != null && baselineResult.Provenance.FirstDetectionTimeUtc != DateTime.MinValue
                    ? baselineResult.Provenance.FirstDetectionTimeUtc
                    : baselineRunTime;

                SetFirstDetectionTime(currentResult, firstDetectionTime);
            }
        }

        private static void SetFirstDetectionTime(Result result, DateTime firstDetectionTime)
        {
            result.Provenance = result.Provenance ?? new ResultProvenance();
            result.Provenance.FirstDetectionTimeUtc = firstDetectionTime;
        }
    }
}
