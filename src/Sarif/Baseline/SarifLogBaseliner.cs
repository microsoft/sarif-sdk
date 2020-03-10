// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Baseline
{
    internal class SarifLogBaseliner : ISarifLogBaseliner
    {
        readonly IEqualityComparer<Result> ResultComparator;

        public SarifLogBaseliner(IEqualityComparer<Result> comparator)
        {
            ResultComparator = comparator;
        }

        public Run CreateBaselinedRun(Run baseLine, Run nextLog)
        {
            Run differencedRun = nextLog.DeepClone();
            differencedRun.Results = new List<Result>();

            foreach (Result result in nextLog.Results)
            {
                Result newResult = result.DeepClone();

                newResult.BaselineState =
                    baseLine.Results.Contains(result, ResultComparator) ? BaselineState.Unchanged : BaselineState.New;

                differencedRun.Results.Add(newResult);
            }

            foreach (Result result in baseLine.Results)
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
    }
}
