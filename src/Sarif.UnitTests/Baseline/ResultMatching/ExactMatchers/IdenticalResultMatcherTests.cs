// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using FluentAssertions;
using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching.ExactMatchers
{
    public class IdenticalResultMatcherTests
    {
        private static IdenticalResultMatcher matcher = new IdenticalResultMatcher();
        
        [Fact]
        public void IdenticalResultMatcher_MatchesIdenticalResults_Single()
        {
            MatchingResult resultA = new MatchingResult()
            {
                Result = ResultMatchingTestHelpers.CreateMatchingResult("file://test", "file://test2", "test context")
            };

            MatchingResult resultB = new MatchingResult()
            {
                Result = resultA.Result.DeepClone()
            };

            IEnumerable<MatchedResults> matchedResults = matcher.MatchResults(new MatchingResult[] {resultA }, new MatchingResult[] { resultB });

            matchedResults.Should().HaveCount(1);
            matchedResults.First().BaselineResult.Should().BeEquivalentTo(resultA);
            matchedResults.First().CurrentResult.Should().BeEquivalentTo(resultB);
        }

        [Fact]
        public void IdenticalResultMatcher_MatchesIdenticalResults_Multiple()
        {
            MatchingResult resultAA = new MatchingResult()
            {
                Result = ResultMatchingTestHelpers.CreateMatchingResult("file://test", "file://test2", "test context")
            };

            MatchingResult resultBA = new MatchingResult()
            {
                Result = resultAA.Result.DeepClone()
            };

            MatchingResult resultAB = new MatchingResult()
            {
                Result = ResultMatchingTestHelpers.CreateMatchingResult("file://test", "file://test2", "test context2")
            };

            MatchingResult resultBB = new MatchingResult()
            {
                Result = resultAB.Result.DeepClone()
            };


            IEnumerable<MatchedResults> matchedResults = matcher.MatchResults(new MatchingResult[] { resultAA, resultAB }, new MatchingResult[] { resultBA, resultBB });

            matchedResults.Should().HaveCount(2);
            matchedResults.Where(f => f.BaselineResult == resultAA && f.CurrentResult == resultBA).Should().HaveCount(1);
            matchedResults.Where(f => f.BaselineResult == resultAB && f.CurrentResult == resultBB).Should().HaveCount(1);
        }

        [Fact]
        public void IdenticalResultMatcher_DoesNotMatchDifferentResults_Single()
        {
            MatchingResult resultA = new MatchingResult()
            {
                Result = ResultMatchingTestHelpers.CreateMatchingResult("file://test", "file://test2", "test context")
            };

            MatchingResult resultB = new MatchingResult()
            {
                Result = ResultMatchingTestHelpers.CreateMatchingResult("file://test", "file://test2", "test context2")
            };

            IEnumerable<MatchedResults> matchedResults = matcher.MatchResults(new MatchingResult[] { resultA }, new MatchingResult[] { resultB });

            matchedResults.Should().BeEmpty();
        }

        [Fact]
        public void IdenticalResultMatcher_DoesNotMatchDifferentResults_Multiple()
        {
            MatchingResult resultAA = new MatchingResult()
            {
                Result = ResultMatchingTestHelpers.CreateMatchingResult("file://test", "file://test2", "test context1")
            };

            MatchingResult resultBA = new MatchingResult()
            {
                Result = ResultMatchingTestHelpers.CreateMatchingResult("file://test", "file://test2", "test context2")
            };

            MatchingResult resultAB = new MatchingResult()
            {
                Result = ResultMatchingTestHelpers.CreateMatchingResult("file://test", "file://test2", "test context3")
            };

            MatchingResult resultBB = new MatchingResult()
            {
                Result = ResultMatchingTestHelpers.CreateMatchingResult("file://test", "file://test2", "test context4")
            };


            IEnumerable<MatchedResults> matchedResults = matcher.MatchResults(new MatchingResult[] { resultAA, resultAB }, new MatchingResult[] { resultBA, resultBB });

            matchedResults.Should().BeEmpty();
        }

        [Fact]
        public void IdenticalResultMatcher_MatchesResults_DifferingOnIdOrStatus_Single()
        {
            MatchingResult resultA = new MatchingResult()
            {
                Result = ResultMatchingTestHelpers.CreateMatchingResult("file://test", "file://test2", "test context")
            };

            Result changedResultA = resultA.Result.DeepClone();
            changedResultA.CorrelationGuid = Guid.NewGuid().ToString();
            changedResultA.BaselineState = BaselineState.Existing;

            MatchingResult resultB = new MatchingResult()
            {
                Result = changedResultA
            };

            IEnumerable<MatchedResults> matchedResults = matcher.MatchResults(new MatchingResult[] { resultA }, new MatchingResult[] { resultB });

            matchedResults.Should().HaveCount(1);
            matchedResults.First().BaselineResult.Should().BeEquivalentTo(resultA);
            matchedResults.First().CurrentResult.Should().BeEquivalentTo(resultB);
        }

        [Fact]
        public void IdenticalResultMatcher_MatchesResults_DifferingOnIdOrStatus_Multiple()
        {
            MatchingResult resultAA = new MatchingResult()
            {
                Result = ResultMatchingTestHelpers.CreateMatchingResult("file://test", "file://test2", "test context")
            };

            Result changedResultA = resultAA.Result.DeepClone();
            changedResultA.CorrelationGuid = Guid.NewGuid().ToString();
            changedResultA.BaselineState = BaselineState.Existing;

            MatchingResult resultBA = new MatchingResult()
            {
                Result = changedResultA
            };

            MatchingResult resultAB = new MatchingResult()
            {
                Result = ResultMatchingTestHelpers.CreateMatchingResult("file://test", "file://test2", "test context2")
            };

            Result changedResultB = resultAB.Result.DeepClone();
            changedResultA.CorrelationGuid = Guid.NewGuid().ToString();
            changedResultA.BaselineState = BaselineState.New;

            MatchingResult resultBB = new MatchingResult()
            {
                Result = changedResultB
            };


            IEnumerable<MatchedResults> matchedResults = matcher.MatchResults(new MatchingResult[] { resultAA, resultAB }, new MatchingResult[] { resultBA, resultBB });

            matchedResults.Should().HaveCount(2);
            matchedResults.Where(f => f.BaselineResult == resultAA && f.CurrentResult == resultBA).Should().HaveCount(1);
            matchedResults.Where(f => f.BaselineResult == resultAB && f.CurrentResult == resultBB).Should().HaveCount(1);
        }

        [Fact]
        public void IdenticalResultMatcher_MatchesResults_DifferingOnResultMatchingProperties_Multiple()
        {
            MatchingResult resultAA = new MatchingResult()
            {
                Result = ResultMatchingTestHelpers.CreateMatchingResult("file://test", "file://test2", "test context")
            };

            Result changedResultA = resultAA.Result.DeepClone();
            changedResultA.CorrelationGuid = Guid.NewGuid().ToString();
            changedResultA.BaselineState = BaselineState.Existing;
            changedResultA.SetProperty(ResultMatchingBaseliner.ResultMatchingResultPropertyName, new Dictionary<string, string> { { "property", "value" } });

            MatchingResult resultBA = new MatchingResult()
            {
                Result = changedResultA
            };

            MatchingResult resultAB = new MatchingResult()
            {
                Result = ResultMatchingTestHelpers.CreateMatchingResult("file://test", "file://test2", "test context2")
            };

            Result changedResultB = resultAB.Result.DeepClone();
            changedResultA.CorrelationGuid = Guid.NewGuid().ToString();
            changedResultA.BaselineState = BaselineState.New;

            changedResultB.SetProperty(ResultMatchingBaseliner.ResultMatchingResultPropertyName, new Dictionary<string, string> { { "property1", "value1" } });
            MatchingResult resultBB = new MatchingResult()
            {
                Result = changedResultB
            };


            IEnumerable<MatchedResults> matchedResults = matcher.MatchResults(new MatchingResult[] { resultAA, resultAB }, new MatchingResult[] { resultBA, resultBB });

            matchedResults.Should().HaveCount(2);
            matchedResults.Where(f => f.BaselineResult == resultAA && f.CurrentResult == resultBA).Should().HaveCount(1);
            matchedResults.Where(f => f.BaselineResult == resultAB && f.CurrentResult == resultBB).Should().HaveCount(1);
        }
    }
}
