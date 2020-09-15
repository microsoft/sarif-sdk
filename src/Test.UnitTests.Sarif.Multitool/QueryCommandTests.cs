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
    public class QueryCommandTests
    {
        private static readonly ResourceExtractor Extractor = new ResourceExtractor(typeof(QueryCommandTests));

        [Fact]
        public void QueryCommand_Basics()
        {
            string filePath = "elfie-arriba.Q.sarif";
            File.WriteAllText(filePath, Extractor.GetResourceText(@"PageCommand.elfie-arriba.sarif"));

            // All Results: No filter
            RunAndVerifyCount(5, new QueryOptions() { Expression = "", InputFilePath = filePath });

            // Rule filtering
            RunAndVerifyCount(1, new QueryOptions() { Expression = "RuleId = 'CSCAN0020/0'", InputFilePath = filePath });
            RunAndVerifyCount(4, new QueryOptions() { Expression = "RuleId = 'CSCAN0060/0'", InputFilePath = filePath });

            // Level filtering
            RunAndVerifyCount(1, new QueryOptions() { Expression = "Level != Error", InputFilePath = filePath });
            RunAndVerifyCount(1, new QueryOptions() { Expression = "Level != Error && RuleId = CSCAN0060/0", InputFilePath = filePath });

            // Intersection w/no matches
            RunAndVerifyCount(0, new QueryOptions() { Expression = "Level != Error && RuleId != CSCAN0060/0", InputFilePath = filePath });

            // Verify parsing errors (unknown enum, operator)
            Assert.Throws<QueryParseException>(() => RunAndVerifyCount(0, new QueryOptions() { Expression = "Level != UnknownValue", InputFilePath = filePath }));
            Assert.Throws<QueryParseException>(() => RunAndVerifyCount(0, new QueryOptions() { Expression = "Level ** Error", InputFilePath = filePath }));
            Assert.Throws<QueryParseException>(() => RunAndVerifyCount(0, new QueryOptions() { Expression = "Leveler != Error", InputFilePath = filePath }));

            // Verify threshold logging
            Assert.Equal(0, new QueryCommand().RunWithoutCatch(new QueryOptions() { Expression = "RuleId = 'CSCAN0060/0'", NonZeroExitCodeIfCountOver = 4, InputFilePath = filePath }));
            Assert.Equal(2, new QueryCommand().RunWithoutCatch(new QueryOptions() { Expression = "RuleId = 'CSCAN0060/0'", NonZeroExitCodeIfCountOver = 3, InputFilePath = filePath }));

            // Verify output file
            string outputFilePath = "elfie-arriba.CSCAN0020.actual.sarif";
            RunAndVerifyCount(1, new QueryOptions() { Expression = "RuleId = 'CSCAN0020/0'", InputFilePath = filePath, OutputFilePath = outputFilePath, PrettyPrint = true, Force = true });

            string expected = Extractor.GetResourceText("QueryCommand.elfie-arriba.CSCAN0020.sarif");
            string actual = File.ReadAllText(outputFilePath);
            Assert.Equal(expected, actual);
        }

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

        // The QueryCommand tests cover all but one code block in the underlying
        // PropertyBagPropertyEvaluator. It doesn't cover the case of an invalid "prefix"
        // (that is, a property name that doesn't start with "properties." or
        // "rule.properties"), because QueryCommand never creates a PropertyBagPropertyEvaluator
        // with such a name. So we cover that case here:
        [Fact]
        public void PropertyBagPropertyEvaluator_RejectsPropertyNameWithInvalidPrefix()
        {
            var expression = new TermExpression(propertyName: "invalid.prefix", op: CompareOperator.Equals, value: string.Empty);

            Action action = () => new PropertyBagPropertyEvaluator(expression);

            action.Should().Throw<ArgumentException>();
        }

        private void RunAndVerifyCount(int expectedCount, QueryOptions options)
        {
            options.ReturnCount = true;
            int exitCode = new QueryCommand().RunWithoutCatch(options);
            exitCode.Should().Be(expectedCount);
        }
    }
}
