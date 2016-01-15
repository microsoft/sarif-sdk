﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.


namespace Microsoft.CodeAnalysis.Sarif.Readers
{
    public class ResultDiffingVisitor : SarifRewritingVisitor
    {
        public ResultDiffingVisitor(ResultLog resultLog)
        {
            this.AbsentResults = new HashSet<Result>();
            this.SharedResults = new HashSet<Result>();
            this.NewResults = new HashSet<Result>();

            VisitResultLog(resultLog);
        }

        public HashSet<Result> NewResults { get; set; }
        public HashSet<Result> AbsentResults { get; set; }
        public HashSet<Result> SharedResults { get; set; }

        public override Result VisitResult(Result node)
        {
            this.NewResults.Add(node);
            return node;
        }

        public bool Diff(IEnumerable<Result> actual)
        {
            this.AbsentResults = this.SharedResults;

            this.SharedResults = new HashSet<Result>();

            foreach (Result result in this.NewResults)
            {
                this.AbsentResults.Add(result);
            }

            this.NewResults.Clear();

            foreach (Result result in actual)
            {
                if (this.AbsentResults.Contains(result))
                {
                    this.SharedResults.Add(result);
                    this.AbsentResults.Remove(result);
                }
                else
                {
                    this.NewResults.Add(result);
                }
            }

            return
                this.AbsentResults.Count == 0 &&
                this.NewResults.Count == 0;
        }
    }
}
