using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.Sarif.Query.Evaluators;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Query
{
    public enum State
    {
        NotStarted,
        Active,
        Blocked,
        Completed
    }

    public class SampleItem
    {
        public int ID { get; set; }
        public State State { get; set; }
        public string Uri { get; set; }
        public bool InProgress => (State == State.Active || State == State.Blocked);

        public static IExpressionEvaluator<SampleItem> Evaluator(TermExpression term)
        {
            switch (term.PropertyName.ToLowerInvariant())
            {
                case "id":
                    return new LongEvaluator<SampleItem>(i => i.ID, term);
                case "state":
                    return new EnumEvaluator<SampleItem, State>(i => i.State, term);
                case "uri":
                    return new StringEvaluator<SampleItem>(i => i.Uri, term, StringComparison.OrdinalIgnoreCase);
                case "inprogress":
                    return new BoolEvaluator<SampleItem>(i => i.InProgress, term);
                default:
                    throw new QueryParseException($"Property Name {term.PropertyName} unrecognized. Known Names: ID, State, Uri");
            }
        }
    }

    public class EvaluatorTests
    {
        [Fact]
        public void ComplexEvaluator_Basics()
        {
            List<SampleItem> set = new List<SampleItem>();
            for (int i = 0; i < 100; ++i)
            {
                SampleItem item = new SampleItem();
                item.ID = i;
                item.State = (State)(i % 4);
                item.Uri = i.ToString();

                set.Add(item);
            }

            Run(10, "ID < 10", set);
            Run(25, "State = Blocked", set);
            Run(10, "Uri > 9", set);
            Run(1, "ID < 10 AND Uri >= 9", set);
            Run(3, "ID < 10 && State == Active", set);  // 1, 5, 9
            Run(0, "ID > 50 && Uri > 'Z' && State != Completed", set);
            Run(25, "ID < 50 && InProgress = true", set);

            // StartsWith, Contains, EndsWith
            Run(11, "Uri |> 9", set);           // StartsWith 9: 9, 9x
            Run(10, "Uri >| 9", set);           // EndsWith 9: x9
            Run(19, "Uri : 9", set);            // Contains 9: x9, 9x

            // Unknown Property Name
            Assert.Throws<QueryParseException>(() => Run(0, "Unknown != true", set));

            // Value (long) didn't parse
            Assert.Throws<QueryParseException>(() => Run(0, "ID < Active", set));

            // Operator (>) not supported for enum
            Assert.Throws<QueryParseException>(() => Run(0, "State > Blocked", set));

            // Value (enum) didn't parse
            Assert.Throws<QueryParseException>(() => Run(0, "State != Locked", set));

            // Value (bool) didn't parse
            Assert.Throws<QueryParseException>(() => Run(0, "InProgress != 40", set));

        }

        private static void Run(int expectedCount, string query, IList<SampleItem> set)
        {
            Run(expectedCount, query, set, SampleItem.Evaluator);
        }

        [Fact]
        public void LongEvaluator_Basics()
        {
            long[] values = Enumerable.Range(0, 100).Select(i => (long)i).ToArray();

            Run(1, "Value == 10", values);
            Run(0, "Value == -99", values);
            Run(99, "Value != 10", values);
            Run(100, "Value != -5", values);

            Run(10, "Value < 10", values);
            Run(100, "Value < 100", values);
            Run(0, "Value < 0", values);

            Run(11, "Value <= 10", values);
            Run(100, "Value <= 100", values);
            Run(1, "Value <= 0", values);

            Run(89, "Value > 10", values);
            Run(0, "Value > 100", values);
            Run(99, "Value > 0", values);

            Run(90, "Value >= 10", values);
            Run(0, "Value >= 100", values);
            Run(100, "Value >= 0", values);

            Assert.Throws<QueryParseException>(() => Run(0, "Unknown > 5", values));
            Assert.Throws<QueryParseException>(() => Run(0, "Value > Bill", values));

        }

        private static void Run(int expectedCount, string query, long[] values)
        {
            Run(expectedCount, query, values, LongArrayEvaluator);
        }

        private static IExpressionEvaluator<long> LongArrayEvaluator(TermExpression term)
        {
            if (!string.Equals(term.PropertyName, "Value", StringComparison.OrdinalIgnoreCase)) { throw new QueryParseException($"Name {term.PropertyName} unknown in term {term}. 'Value' is the only valid name."); }
            return new LongEvaluator<long>(l => l, term);
        }

        private static void Run<T>(int expectedCount, string query, IList<T> values, Func<TermExpression, IExpressionEvaluator<T>> converter)
        {
            // Parse the Query
            IExpression expression = ExpressionParser.ParseExpression(query);

            // Build an Evaluator against the int array
            IExpressionEvaluator<T> evaluator = expression.ToEvaluator<T>(converter);

            // Ask for matches from the array
            BitArray matches = new BitArray(values.Count);
            evaluator.Evaluate(values, matches);

            // Verify the match count is correct
            Assert.Equal(expectedCount, matches.TrueCount());
        }
    }
}
