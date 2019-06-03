using System;
using System.Collections;
using System.Linq;
using Microsoft.CodeAnalysis.Sarif.Query.Evaluators;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Query
{
    public class EvaluatorTests
    {
        [Fact]
        public void LongEvaluator_Basics()
        {
            long[] values = Enumerable.Range(0, 100).Select(i => (long)i).ToArray();

            Run(1, values, "Value == 10");
            Run(0, values, "Value == -99");
            Run(99, values, "Value != 10");
            Run(100, values, "Value != -5");

            Run(10, values, "Value < 10");
            Run(100, values, "Value < 100");
            Run(0, values, "Value < 0");

            Run(11, values, "Value <= 10");
            Run(100, values, "Value <= 100");
            Run(1, values, "Value <= 0");

            Run(89, values, "Value > 10");
            Run(0, values, "Value > 100");
            Run(99, values, "Value > 0");

            Run(90, values, "Value >= 10");
            Run(0, values, "Value >= 100");
            Run(100, values, "Value >= 0");
        }

        private static void Run(int expectedCount, long[] values, string query)
        {
            // Parse the Query
            IExpression expression = ExpressionParser.ParseExpression(query);

            // Build an Evaluator against the int array
            IExpressionEvaluator<long> evaluator = expression.ToEvaluator<long>(ConvertTerm);

            // Ask for matches from the array
            BitArray matches = new BitArray(values.Length);
            evaluator.Evaluate(values, matches);

            // Verify the match count is correct
            Assert.Equal(expectedCount, matches.CountTrue());
        }

        private static IExpressionEvaluator<long> ConvertTerm(TermExpression term)
        {
            if (!String.Equals(term.PropertyName, "Value", StringComparison.OrdinalIgnoreCase)) { throw new QueryParseException($"Name {term.PropertyName} unknown in term {term}. 'Value' is the only valid name."); }
            return new LongEvaluator<long>(l => l, term);
        }
    }
}
