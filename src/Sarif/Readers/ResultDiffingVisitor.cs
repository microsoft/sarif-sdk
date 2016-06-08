// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Readers
{
    public class ResultDiffingVisitor : SarifRewritingVisitor
    {
        public ResultDiffingVisitor(SarifLog sarifLog)
        {
            this.AbsentResults = new HashSet<Result>(Result.ValueComparer);
            this.SharedResults = new HashSet<Result>(Result.ValueComparer);
            this.NewResults = new HashSet<Result>(Result.ValueComparer);

            VisitSarifLog(sarifLog);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public HashSet<Result> NewResults { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public HashSet<Result> AbsentResults { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public HashSet<Result> SharedResults { get; set; }

        public override Result VisitResult(Result node)
        {
            this.NewResults.Add(node);
            return node;
        }

        public bool Diff(IEnumerable<Result> actual)
        {

            this.AbsentResults = this.SharedResults;

            this.SharedResults = new HashSet<Result>(Result.ValueComparer);

            foreach (Result result in this.NewResults)
            {
                this.AbsentResults.Add(result);
            }

            this.NewResults.Clear();

            if (actual != null)
            {
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
            }

            return
                this.AbsentResults.Count == 0 &&
                this.NewResults.Count == 0;

        }
    }
}
