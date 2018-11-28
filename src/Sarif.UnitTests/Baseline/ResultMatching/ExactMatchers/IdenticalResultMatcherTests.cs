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
        private static IdenticalResultMatcher matcher = new IdenticalResultMatcher(considerPropertyBagsWhenComparing: true);
        
        [Fact]
        public void IdenticalResultMatcher_MatchesIdenticalResults_Single()
        {
            ExtractedResult resultA = new ExtractedResult()
            {
                Result = ResultMatchingTestHelpers.CreateMatchingResult("file://test", "file://test2", "test context")
            };

            ExtractedResult resultB = new ExtractedResult()
            {
                Result = resultA.Result.DeepClone()
            };

            IEnumerable<MatchedResults> matchedResults = matcher.Match(new ExtractedResult[] {resultA }, new ExtractedResult[] { resultB });

            matchedResults.Should().HaveCount(1);
            matchedResults.First().PreviousResult.Should().BeEquivalentTo(resultA);
            matchedResults.First().CurrentResult.Should().BeEquivalentTo(resultB);
        }

        [Fact]
        public void IdenticalResultMatcher_MatchesIdenticalResults_Multiple()
        {
            ExtractedResult resultAA = new ExtractedResult()
            {
                Result = ResultMatchingTestHelpers.CreateMatchingResult("file://test", "file://test2", "test context")
            };

            ExtractedResult resultBA = new ExtractedResult()
            {
                Result = resultAA.Result.DeepClone()
            };

            ExtractedResult resultAB = new ExtractedResult()
            {
                Result = ResultMatchingTestHelpers.CreateMatchingResult("file://test", "file://test2", "test context2")
            };

            ExtractedResult resultBB = new ExtractedResult()
            {
                Result = resultAB.Result.DeepClone()
            };


            IEnumerable<MatchedResults> matchedResults = matcher.Match(new ExtractedResult[] { resultAA, resultAB }, new ExtractedResult[] { resultBA, resultBB });

            matchedResults.Should().HaveCount(2);
            matchedResults.Where(f => f.PreviousResult == resultAA && f.CurrentResult == resultBA).Should().HaveCount(1);
            matchedResults.Where(f => f.PreviousResult == resultAB && f.CurrentResult == resultBB).Should().HaveCount(1);
        }

        [Fact]
        public void IdenticalResultMatcher_DoesNotMatchDifferentResults_Single()
        {
            ExtractedResult resultA = new ExtractedResult()
            {
                Result = ResultMatchingTestHelpers.CreateMatchingResult("file://test", "file://test2", "test context")
            };

            ExtractedResult resultB = new ExtractedResult()
            {
                Result = ResultMatchingTestHelpers.CreateMatchingResult("file://test", "file://test2", "test context2")
            };

            IEnumerable<MatchedResults> matchedResults = matcher.Match(new ExtractedResult[] { resultA }, new ExtractedResult[] { resultB });

            matchedResults.Should().BeEmpty();
        }

        [Fact]
        public void IdenticalResultMatcher_DoesNotMatchDifferentResults_Multiple()
        {
            ExtractedResult resultAA = new ExtractedResult()
            {
                Result = ResultMatchingTestHelpers.CreateMatchingResult("file://test", "file://test2", "test context1")
            };

            ExtractedResult resultBA = new ExtractedResult()
            {
                Result = ResultMatchingTestHelpers.CreateMatchingResult("file://test", "file://test2", "test context2")
            };

            ExtractedResult resultAB = new ExtractedResult()
            {
                Result = ResultMatchingTestHelpers.CreateMatchingResult("file://test", "file://test2", "test context3")
            };

            ExtractedResult resultBB = new ExtractedResult()
            {
                Result = ResultMatchingTestHelpers.CreateMatchingResult("file://test", "file://test2", "test context4")
            };


            IEnumerable<MatchedResults> matchedResults = matcher.Match(new ExtractedResult[] { resultAA, resultAB }, new ExtractedResult[] { resultBA, resultBB });

            matchedResults.Should().BeEmpty();
        }

        [Fact]
        public void IdenticalResultMatcher_MatchesResults_DifferingOnIdOrStatus_Single()
        {
            ExtractedResult resultA = new ExtractedResult()
            {
                Result = ResultMatchingTestHelpers.CreateMatchingResult("file://test", "file://test2", "test context")
            };

            Result changedResultA = resultA.Result.DeepClone();
            changedResultA.CorrelationGuid = Guid.NewGuid().ToString();
            changedResultA.BaselineState = BaselineState.Existing;

            ExtractedResult resultB = new ExtractedResult()
            {
                Result = changedResultA
            };

            IEnumerable<MatchedResults> matchedResults = matcher.Match(new ExtractedResult[] { resultA }, new ExtractedResult[] { resultB });

            matchedResults.Should().HaveCount(1);
            matchedResults.First().PreviousResult.Should().BeEquivalentTo(resultA);
            matchedResults.First().CurrentResult.Should().BeEquivalentTo(resultB);
        }

        [Fact]
        public void IdenticalResultMatcher_MatchesResults_DifferingOnIdOrStatus_Multiple()
        {
            ExtractedResult resultAA = new ExtractedResult()
            {
                Result = ResultMatchingTestHelpers.CreateMatchingResult("file://test", "file://test2", "test context")
            };

            Result changedResultA = resultAA.Result.DeepClone();
            changedResultA.CorrelationGuid = Guid.NewGuid().ToString();
            changedResultA.BaselineState = BaselineState.Existing;

            ExtractedResult resultBA = new ExtractedResult()
            {
                Result = changedResultA
            };

            ExtractedResult resultAB = new ExtractedResult()
            {
                Result = ResultMatchingTestHelpers.CreateMatchingResult("file://test", "file://test2", "test context2")
            };

            Result changedResultB = resultAB.Result.DeepClone();
            changedResultA.CorrelationGuid = Guid.NewGuid().ToString();
            changedResultA.BaselineState = BaselineState.New;

            ExtractedResult resultBB = new ExtractedResult()
            {
                Result = changedResultB
            };


            IEnumerable<MatchedResults> matchedResults = matcher.Match(new ExtractedResult[] { resultAA, resultAB }, new ExtractedResult[] { resultBA, resultBB });

            matchedResults.Should().HaveCount(2);
            matchedResults.Where(f => f.PreviousResult == resultAA && f.CurrentResult == resultBA).Should().HaveCount(1);
            matchedResults.Where(f => f.PreviousResult == resultAB && f.CurrentResult == resultBB).Should().HaveCount(1);
        }

        [Fact]
        public void IdenticalResultMatcher_MatchesResults_DifferingOnResultMatchingProperties_Multiple()
        {
            ExtractedResult resultAA = new ExtractedResult()
            {
                Result = ResultMatchingTestHelpers.CreateMatchingResult("file://test", "file://test2", "test context")
            };

            Result changedResultA = resultAA.Result.DeepClone();
            changedResultA.CorrelationGuid = Guid.NewGuid().ToString();
            changedResultA.BaselineState = BaselineState.Existing;
            changedResultA.SetProperty(SarifLogResultMatcher.ResultMatchingResultPropertyName, new Dictionary<string, string> { { "property", "value" } });

            ExtractedResult resultBA = new ExtractedResult()
            {
                Result = changedResultA
            };

            ExtractedResult resultAB = new ExtractedResult()
            {
                Result = ResultMatchingTestHelpers.CreateMatchingResult("file://test", "file://test2", "test context2")
            };

            Result changedResultB = resultAB.Result.DeepClone();
            changedResultA.CorrelationGuid = Guid.NewGuid().ToString();
            changedResultA.BaselineState = BaselineState.New;

            changedResultB.SetProperty(SarifLogResultMatcher.ResultMatchingResultPropertyName, new Dictionary<string, string> { { "property1", "value1" } });
            ExtractedResult resultBB = new ExtractedResult()
            {
                Result = changedResultB
            };


            IEnumerable<MatchedResults> matchedResults = matcher.Match(new ExtractedResult[] { resultAA, resultAB }, new ExtractedResult[] { resultBA, resultBB });

            matchedResults.Should().HaveCount(2);
            matchedResults.Where(f => f.PreviousResult == resultAA && f.CurrentResult == resultBA).Should().HaveCount(1);
            matchedResults.Where(f => f.PreviousResult == resultAB && f.CurrentResult == resultBB).Should().HaveCount(1);
        }
    }
}
