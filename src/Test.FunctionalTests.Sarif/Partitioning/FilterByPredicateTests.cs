// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Microsoft.CodeAnalysis.Sarif.Visitors;
using Microsoft.CodeAnalysis.Sarif.Writers;
using Microsoft.CodeAnalysis.Test.Utilities.Sarif;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.CodeAnalysis.Sarif.FunctionalTests.Partitioning
{
    public class FilterByPredicateTests : FileDiffingFunctionalTests
    {
        public FilterByPredicateTests(ITestOutputHelper outputHelper, bool testProducesSarifCurrentVersion = true) :
            base(outputHelper, testProducesSarifCurrentVersion)
        { }

        protected override string IntermediateTestFolder => @"Partitioning";

        [Fact]
        public void Filter_WithAlwaysTruePredicate_ReturnsLogWithAllResultsAndRunLevelArrayContentsFromAllResults()
        {
            FilteringVisitor.IncludeResultPredicate predicate = result => true;

            RunTest("FilterByPredicate.sarif", "AlwaysTruePredicate.sarif", predicate);
        }

        [Fact]
        public void Filter_WithAlwaysFalsePredicate_ReturnsLogWithNoResultsAndNoRunLevelArrayContents()
        {
            FilteringVisitor.IncludeResultPredicate predicate = (Result result) => false;

            RunTest("FilterByPredicate.sarif", "AlwaysFalsePredicate.sarif", predicate);
        }

        [Fact]
        public void Filter_WithRuleIdPredicate_ReturnsLogWithExpectedResultsAndRunLevelArrayContentsFromSelectedResults()
        {
            FilteringVisitor.IncludeResultPredicate predicate =
                (Result result) => result.RuleId.Equals(TestData.RuleIds.Rule1, StringComparison.InvariantCulture);

            RunTest("FilterByPredicate.sarif", "RuleIdPredicate.sarif", predicate);
        }

        protected override string ConstructTestOutputFromInputResource(string inputResourceName, object parameter)
        {
            var predicate = (FilteringVisitor.IncludeResultPredicate)parameter;

            string inputText = GetResourceText(inputResourceName);
            SarifLog inputLog = JsonConvert.DeserializeObject<SarifLog>(inputText);

            SarifLog outputLog = SarifPartitioner.Filter(inputLog, predicate);

            return JsonConvert.SerializeObject(outputLog, Formatting.Indented);
        }
    }
}
