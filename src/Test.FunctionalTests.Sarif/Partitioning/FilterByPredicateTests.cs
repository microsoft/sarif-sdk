// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;
using Microsoft.CodeAnalysis.Sarif.Writers;
using Xunit.Abstractions;
using Newtonsoft.Json;
using Microsoft.CodeAnalysis.Test.Utilities.Sarif;
using System;
using Microsoft.CodeAnalysis.Sarif.Visitors;

namespace Microsoft.CodeAnalysis.Sarif.FunctionalTests.Partitioning
{
    public class FilterByPredicateTests : FileDiffingFunctionalTests
    {
        public FilterByPredicateTests(ITestOutputHelper outputHelper, bool testProducesSarifCurrentVersion = true) :
            base(outputHelper, testProducesSarifCurrentVersion)
        { }

        protected override string IntermediateTestFolder => @"Partitioning";

        [Fact]
        public void Filter_WithAlwaysTruePredicate_ReturnsIdenticalLog()
        {
            FilteringVisitor.FilteringPredicate predicate = (Result result) => true;

            RunTest("FilterByPredicate.sarif", "AlwaysTruePredicate.sarif", predicate);
        }

        [Fact]
        public void Filter_WithAlwaysFalsePredicate_ReturnsLogWithNoResults()
        {
            FilteringVisitor.FilteringPredicate predicate = (Result result) => false;

            RunTest("FilterByPredicate.sarif", "AlwaysFalsePredicate.sarif", predicate);
        }

        [Fact]
        public void Filter_WithRuleIdPredicate_ReturnsLogWithResultsForOnlyThatRuleId()
        {
            FilteringVisitor.FilteringPredicate predicate =
                (Result result) => result.RuleId.Equals(TestConstants.RuleIds.Rule2, StringComparison.InvariantCulture);

            RunTest("FilterByPredicate.sarif", "RuleIdPredicate.sarif", predicate);
        }

        [Fact]
        public void Filter_FiltersArtifacts()
        {
            FilteringVisitor.FilteringPredicate predicate =
                (Result result) => result.RuleId.Equals(TestConstants.RuleIds.Rule1, StringComparison.InvariantCulture);

            RunTest("FilterByPredicateWithArtifacts.sarif", "FilterByPredicateWithArtifacts.sarif", predicate);
        }

        protected override string ConstructTestOutputFromInputResource(string inputResourceName, object parameter)
        {
            var predicate = (FilteringVisitor.FilteringPredicate)parameter;

            string inputText = GetResourceText(inputResourceName);
            SarifLog inputLog = JsonConvert.DeserializeObject<SarifLog>(inputText);

            SarifLog outputLog = SarifPartitioner.Filter(inputLog, predicate);

            return JsonConvert.SerializeObject(outputLog, Formatting.Indented);
        }
    }
}
