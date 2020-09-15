// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif.Query;
using Microsoft.CodeAnalysis.Sarif.Query.Evaluators;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class QueryCommandPropertyBagTests : IClassFixture<QueryCommandPropertyBagTests.TestFixture>
    {
        private const string FilePath = "property-bag-queries.sarif";

        public class TestFixture
        {
            public TestFixture()
            {
                ResourceExtractor extractor = new ResourceExtractor(typeof(QueryCommandPropertyBagTests));
                File.WriteAllText(FilePath, extractor.GetResourceText($"QueryCommand.{FilePath}"));
            }
        }

        [Fact]
        public void QueryCommand_CanAccessResultAndRulePropertyBags()
        {
            RunAndVerifyCount(2, "properties.name == 'Terisa'");
            RunAndVerifyCount(2, "rule.properties.Category == 'security'");
        }

        [Fact]
        public void QueryCommand_ComparesPropertyBagPropertyNamesCaseInsensitively()
        {
            RunAndVerifyCount(2, "properties.nAmE == 'Terisa'");
            RunAndVerifyCount(2, "rule.properties.CaTegORy == 'security'");
        }

        [Fact]
        public void QueryCommand_ReadsIntegerProperty()
        {
            RunAndVerifyCount(1, "properties.count == 42");
        }

        [Fact]
        public void QueryCommand_ReadsFloatProperty()
        {
            RunAndVerifyCount(2, "properties.confidence >= 0.95");
        }

        [Fact]
        public void QueryCommand_PerformsStringSpecificComparisons()
        {
            RunAndVerifyCount(1, "properties.confidence : 95");     // contains
            RunAndVerifyCount(3, "properties.confidence |> 0.");    // startswith
            RunAndVerifyCount(1, "properties.confidence >| 9");     // endswith
        }

        [Fact]
        public void QueryCommand_TreatsUnparseableValueAsHavingTheDefaultValue()
        {
            // In this test, all the results will match, so we need to know how many there are.
            SarifLog sarifLog = JsonConvert.DeserializeObject<SarifLog>(File.ReadAllText(FilePath));
            int numResults = sarifLog.Runs[0].Results.Count;

            // 'name' is a string-valued property that doesn't parse to an integer. The
            // PropertyBagPropertyEvaluator sees a number on the right-hand side of the comparison
            // and decides that a numeric comparison is intended. When it encounters a property
            // value that can't be parsed as a number, it treats it as numeric 0 rather than
            // throwing.
            RunAndVerifyCount(numResults, "properties.name == 0");
        }

        // The above tests cover all but one code block in the underlying PropertyBagPropertyEvaluator.
        // It doesn't cover the case of an invalid "prefix" (that is, a property name that doesn't
        // start with "properties." or "rule.properties"), because QueryCommand never creates a
        // PropertyBagPropertyEvaluator for a term with such a property name. So we cover that
        // case here:
        [Fact]
        public void PropertyBagPropertyEvaluator_RejectsPropertyNameWithInvalidPrefix()
        {
            var expression = new TermExpression(propertyName: "invalid.prefix", op: CompareOperator.Equals, value: string.Empty);

            Action action = () => new PropertyBagPropertyEvaluator(expression);

            action.Should().Throw<ArgumentException>();
        }

        private void RunAndVerifyCount(int expectedCount, string expression)
        {
            var options = new QueryOptions
            {
                Expression = expression,
                InputFilePath = FilePath,
                ReturnCount = true
            };

            int exitCode = new QueryCommand().RunWithoutCatch(options);
            exitCode.Should().Be(expectedCount);
        }
    }
}
