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
    public class QueryCommandPropertyBagTests : QueryCommandTestsBase
    {
        [Fact]
        public void QueryCommand_CanAccessResultAndRulePropertyBags()
        {
            const string FilePath = "property-bag-queries.sarif";
            File.WriteAllText(FilePath, Extractor.GetResourceText($"QueryCommand.{FilePath}"));

            RunAndVerifyCount(2, new QueryOptions() { Expression = "properties.name == 'Terisa'", InputFilePath = FilePath });
            RunAndVerifyCount(2, new QueryOptions() { Expression = "rule.properties.Category == 'security'", InputFilePath = FilePath });
        }

        [Fact]
        public void QueryCommand_ComparesPropertyBagPropertyNamesCaseInsensitively()
        {
            const string FilePath = "property-bag-queries.sarif";
            File.WriteAllText(FilePath, Extractor.GetResourceText($"QueryCommand.{FilePath}"));

            RunAndVerifyCount(2, new QueryOptions() { Expression = "properties.nAmE == 'Terisa'", InputFilePath = FilePath });
            RunAndVerifyCount(2, new QueryOptions() { Expression = "rule.properties.CaTegORy == 'security'", InputFilePath = FilePath });
        }

        [Fact]
        public void QueryCommand_AcceptsOptionalStringTypeSpecifier()
        {
            const string FilePath = "property-bag-queries.sarif";
            File.WriteAllText(FilePath, Extractor.GetResourceText($"QueryCommand.{FilePath}"));

            RunAndVerifyCount(2, new QueryOptions() { Expression = "properties.name:s == 'Terisa'", InputFilePath = FilePath });
            RunAndVerifyCount(2, new QueryOptions() { Expression = "rule.properties.Category:s == 'security'", InputFilePath = FilePath });
        }

        [Fact]
        public void QueryCommand_RejectsUnknownTypeSpecifier()
        {
            const string FilePath = "property-bag-queries.sarif";
            File.WriteAllText(FilePath, Extractor.GetResourceText($"QueryCommand.{FilePath}"));

            var options = new QueryOptions { Expression = "properties.name:x == 'Terisa'", InputFilePath = FilePath };
            Action action = () => new QueryCommand().RunWithoutCatch(options);

            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void QueryCommand_ReadsIntegerProperty()
        {
            const string FilePath = "property-bag-queries.sarif";
            File.WriteAllText(FilePath, Extractor.GetResourceText($"QueryCommand.{FilePath}"));

            RunAndVerifyCount(1, new QueryOptions { Expression = "properties.count:n == 42", InputFilePath = FilePath });
        }

        [Fact]
        public void QueryCommand_ReadsFloatProperty()
        {
            const string FilePath = "property-bag-queries.sarif";
            File.WriteAllText(FilePath, Extractor.GetResourceText($"QueryCommand.{FilePath}"));

            RunAndVerifyCount(2, new QueryOptions { Expression = "properties.confidence:f >= 0.95", InputFilePath = FilePath });
        }

        [Fact]
        public void QueryCommand_TreatsUnparseableValueAsHavingTheDefaultValue()
        {
            const string FilePath = "property-bag-queries.sarif";
            File.WriteAllText(FilePath, Extractor.GetResourceText($"QueryCommand.{FilePath}"));

            // In this test, all the results will match, so we need to know how many there are.
            SarifLog sarifLog = JsonConvert.DeserializeObject<SarifLog>(File.ReadAllText(FilePath));
            int numResults = sarifLog.Runs[0].Results.Count;

            // 'name' is a string-valued property that doesn't parse to an integer. The query evaluator
            // treats it as having the default value.
            RunAndVerifyCount(numResults, new QueryOptions { Expression = "properties.name:n == 0", InputFilePath = FilePath });
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
    }
}
