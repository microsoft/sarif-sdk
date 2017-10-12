// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.


using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Readers
{
    public class ResultDiffingVisitorTests
    {
        [Fact]
        public void ResultDiffingVisitor_DetectsAbsentAndNewResults()
        {
            var result1 = new Result { RuleId = "TST0001" };
            var result2 = new Result { RuleId = "TST0002" };
            var result3 = new Result { RuleId = "TST0003" };

            var sarifLog = new SarifLog
            {
                Runs = new[]
                {
                    new Run
                    {
                        Results = new List<Result>
                        {
                            result1,
                            result2
                        }
                    }
                }
            };

            var visitor = new ResultDiffingVisitor(sarifLog);

            result2 = new Result { RuleId = "TST0002" };
            result3 = new Result { RuleId = "TST0003" };


            var newResults = new Result[]
            {
                result2,
                result3
            };

            visitor.Diff(newResults);

            Assert.Single(visitor.AbsentResults);
            Assert.Equal(visitor.AbsentResults.First().RuleId, result1.RuleId);

            Assert.Single(visitor.NewResults);
            Assert.Equal(visitor.NewResults.First().RuleId, result3.RuleId);
        }
    }
}
