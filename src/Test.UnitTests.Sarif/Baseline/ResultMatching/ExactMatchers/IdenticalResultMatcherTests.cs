// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching.ExactMatchers
{
    public class IdenticalResultMatcherTests
    {
        private static readonly IdenticalResultMatcher matcher = new IdenticalResultMatcher(considerPropertyBagsWhenComparing: true);

        [Fact]
        public void IdenticalResultMatcher_MatchesIdenticalResults_Single()
        {
            ExtractedResult resultA = new ExtractedResult(ResultMatchingTestHelpers.CreateMatchingResult("file://test", "file://test2", "test context"), null);
            ExtractedResult resultB = new ExtractedResult(resultA.Result.DeepClone(), null);

            IEnumerable<MatchedResults> matchedResults = matcher.Match(new ExtractedResult[] { resultA }, new ExtractedResult[] { resultB });

            matchedResults.Should().HaveCount(1);
            matchedResults.First().PreviousResult.Should().BeEquivalentTo(resultA);
            matchedResults.First().CurrentResult.Should().BeEquivalentTo(resultB);
        }

        [Fact]
        public void IdenticalResultMatcher_MatchesIdenticalResults_Multiple()
        {
            ExtractedResult resultAA = new ExtractedResult(ResultMatchingTestHelpers.CreateMatchingResult("file://test", "file://test2", "test context"), null);
            ExtractedResult resultBA = new ExtractedResult(resultAA.Result.DeepClone(), null);
            ExtractedResult resultAB = new ExtractedResult(ResultMatchingTestHelpers.CreateMatchingResult("file://test", "file://test2", "test context2"), null);
            ExtractedResult resultBB = new ExtractedResult(resultAB.Result.DeepClone(), null);

            IEnumerable<MatchedResults> matchedResults = matcher.Match(new ExtractedResult[] { resultAA, resultAB }, new ExtractedResult[] { resultBA, resultBB });

            matchedResults.Should().HaveCount(2);
            matchedResults.Where(f => f.PreviousResult == resultAA && f.CurrentResult == resultBA).Should().HaveCount(1);
            matchedResults.Where(f => f.PreviousResult == resultAB && f.CurrentResult == resultBB).Should().HaveCount(1);
        }

        [Fact]
        public void IdenticalResultMatcher_DoesNotMatchDifferentResults_Single()
        {
            ExtractedResult resultA = new ExtractedResult(ResultMatchingTestHelpers.CreateMatchingResult("file://test", "file://test2", "test context"), null);
            ExtractedResult resultB = new ExtractedResult(ResultMatchingTestHelpers.CreateMatchingResult("file://test", "file://test2", "test context2"), null);

            IEnumerable<MatchedResults> matchedResults = matcher.Match(new ExtractedResult[] { resultA }, new ExtractedResult[] { resultB });

            matchedResults.Should().BeEmpty();
        }

        [Fact]
        public void IdenticalResultMatcher_DoesNotMatchDifferentResults_Multiple()
        {
            ExtractedResult resultAA = new ExtractedResult(ResultMatchingTestHelpers.CreateMatchingResult("file://test", "file://test2", "test context1"), null);
            ExtractedResult resultBA = new ExtractedResult(ResultMatchingTestHelpers.CreateMatchingResult("file://test", "file://test2", "test context2"), null);
            ExtractedResult resultAB = new ExtractedResult(ResultMatchingTestHelpers.CreateMatchingResult("file://test", "file://test2", "test context3"), null);
            ExtractedResult resultBB = new ExtractedResult(ResultMatchingTestHelpers.CreateMatchingResult("file://test", "file://test2", "test context4"), null);

            IEnumerable<MatchedResults> matchedResults = matcher.Match(new ExtractedResult[] { resultAA, resultAB }, new ExtractedResult[] { resultBA, resultBB });

            matchedResults.Should().BeEmpty();
        }

        [Fact]
        public void IdenticalResultMatcher_MatchesResults_DifferingOnIdOrStatus_Single()
        {
            ExtractedResult resultA = new ExtractedResult(ResultMatchingTestHelpers.CreateMatchingResult("file://test", "file://test2", "test context"), null);

            Result changedResultA = resultA.Result.DeepClone();
            changedResultA.CorrelationGuid = Guid.NewGuid().ToString();
            changedResultA.BaselineState = BaselineState.Unchanged;

            ExtractedResult resultB = new ExtractedResult(changedResultA, null);

            IEnumerable<MatchedResults> matchedResults = matcher.Match(new ExtractedResult[] { resultA }, new ExtractedResult[] { resultB });

            matchedResults.Should().HaveCount(1);
            matchedResults.First().PreviousResult.Should().BeEquivalentTo(resultA);
            matchedResults.First().CurrentResult.Should().BeEquivalentTo(resultB);
        }

        [Fact]
        public void IdenticalResultMatcher_MatchesResults_DifferingOnIdOrStatus_Multiple()
        {
            ExtractedResult resultAA = new ExtractedResult(ResultMatchingTestHelpers.CreateMatchingResult("file://test", "file://test2", "test context"), null);

            Result changedResultA = resultAA.Result.DeepClone();
            changedResultA.CorrelationGuid = Guid.NewGuid().ToString();
            changedResultA.BaselineState = BaselineState.Unchanged;

            ExtractedResult resultBA = new ExtractedResult(changedResultA, null);
            ExtractedResult resultAB = new ExtractedResult(ResultMatchingTestHelpers.CreateMatchingResult("file://test", "file://test2", "test context2"), null);

            Result changedResultB = resultAB.Result.DeepClone();
            changedResultA.CorrelationGuid = Guid.NewGuid().ToString();
            changedResultA.BaselineState = BaselineState.New;

            ExtractedResult resultBB = new ExtractedResult(changedResultB, null);

            IEnumerable<MatchedResults> matchedResults = matcher.Match(new ExtractedResult[] { resultAA, resultAB }, new ExtractedResult[] { resultBA, resultBB });

            matchedResults.Should().HaveCount(2);
            matchedResults.Where(f => f.PreviousResult == resultAA && f.CurrentResult == resultBA).Should().HaveCount(1);
            matchedResults.Where(f => f.PreviousResult == resultAB && f.CurrentResult == resultBB).Should().HaveCount(1);
        }

        [Fact]
        public void IdenticalResultMatcher_MatchesResults_DifferingOnResultMatchingProperties_Multiple()
        {
            ExtractedResult resultAA = new ExtractedResult(ResultMatchingTestHelpers.CreateMatchingResult("file://test", "file://test2", "test context"), null);

            Result changedResultA = resultAA.Result.DeepClone();
            changedResultA.CorrelationGuid = Guid.NewGuid().ToString();
            changedResultA.BaselineState = BaselineState.Unchanged;
            changedResultA.SetProperty(SarifLogResultMatcher.ResultMatchingResultPropertyName, new Dictionary<string, string> { { "property", "value" } });

            ExtractedResult resultBA = new ExtractedResult(changedResultA, null);
            ExtractedResult resultAB = new ExtractedResult(ResultMatchingTestHelpers.CreateMatchingResult("file://test", "file://test2", "test context2"), null);

            Result changedResultB = resultAB.Result.DeepClone();
            changedResultA.CorrelationGuid = Guid.NewGuid().ToString();
            changedResultA.BaselineState = BaselineState.New;

            changedResultB.SetProperty(SarifLogResultMatcher.ResultMatchingResultPropertyName, new Dictionary<string, string> { { "property1", "value1" } });
            ExtractedResult resultBB = new ExtractedResult(changedResultB, null);

            IEnumerable<MatchedResults> matchedResults = matcher.Match(new ExtractedResult[] { resultAA, resultAB }, new ExtractedResult[] { resultBA, resultBB });

            matchedResults.Should().HaveCount(2);
            matchedResults.Where(f => f.PreviousResult == resultAA && f.CurrentResult == resultBA).Should().HaveCount(1);
            matchedResults.Where(f => f.PreviousResult == resultAB && f.CurrentResult == resultBB).Should().HaveCount(1);
        }
    }
}
