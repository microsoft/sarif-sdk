// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Query.Evaluators
{
    public static class ExpressionEvaluatorConverter
    {
        /// <summary>
        ///  Convert an IExpression to an IExpressionEvaluator, given a method which knows how
        ///  to evaluate single terms. The method should throw QueryParseException for terms
        ///  which are invalid in any way.
        /// </summary>
        /// <remarks>
        ///  Use the helper classes in Query.Evaluators to handle matching on simple types.
        /// </remarks>
        /// <typeparam name="T">Type of Item Evaluator will match</typeparam>
        /// <param name="expression">Parsed IExpression for overall query</param>
        /// <param name="termEvaluatorBuilder">Method which builds an evaluator for single terms</param>
        /// <returns>IExpressionEvaluator to run query against any ICollection or the item type</returns>
        public static IExpressionEvaluator<T> ToEvaluator<T>(this IExpression expression, Func<TermExpression, IExpressionEvaluator<T>> termEvaluatorBuilder)
        {
            if (expression is TermExpression termExpression)
            {
                return termEvaluatorBuilder(termExpression);
            }
            else if (expression is AndExpression andExpression)
            {
                return new AndEvaluator<T>(andExpression.Terms.Select(term => ToEvaluator<T>(term, termEvaluatorBuilder)));
            }
            else if (expression is OrExpression orExpression)
            {
                return new OrEvaluator<T>(orExpression.Terms.Select(term => ToEvaluator<T>(term, termEvaluatorBuilder)));
            }
            else if (expression is NotExpression notExpression)
            {
                return new NotEvaluator<T>(ToEvaluator<T>(notExpression.Inner, termEvaluatorBuilder));
            }
            else if (expression is AllExpression)
            {
                return new AllEvaluator<T>();
            }
            else if (expression is NoneExpression)
            {
                return new NoneEvaluator<T>();
            }
            else
            {
                throw new NotImplementedException($"ToEvaluator not implemented for {expression.GetType().Name}");
            }
        }
    }

    public class TermEvaluator<T> : IExpressionEvaluator<T>
    {
        private readonly Action<ICollection<T>, BitArray> _action;

        public TermEvaluator(Action<ICollection<T>, BitArray> action)
        {
            _action = action;
        }

        public void Evaluate(ICollection<T> list, BitArray matches)
        {
            _action(list, matches);
        }
    }

    public class AndEvaluator<T> : IExpressionEvaluator<T>
    {
        private readonly IReadOnlyList<IExpressionEvaluator<T>> _terms;

        public AndEvaluator(IEnumerable<IExpressionEvaluator<T>> terms)
        {
            _terms = terms.ToList();
        }

        public void Evaluate(ICollection<T> list, BitArray matches)
        {
            BitArray termMatches = new BitArray(list.Count);

            for (int i = 0; i < _terms.Count; ++i)
            {
                IExpressionEvaluator<T> term = _terms[i];

                // Get matches for the term
                termMatches.SetAll(false);
                term.Evaluate(list, termMatches);

                // Intersect with matches so far
                if (i == 0)
                {
                    matches.Or(termMatches);
                }
                else
                {
                    matches.And(termMatches);
                }

                // Stop if no matches remain
                if (matches.TrueCount() == 0) { break; }
            }
        }
    }

    public class OrEvaluator<T> : IExpressionEvaluator<T>
    {
        private readonly IReadOnlyList<IExpressionEvaluator<T>> _terms;

        public OrEvaluator(IEnumerable<IExpressionEvaluator<T>> terms)
        {
            _terms = terms.ToList();
        }

        public void Evaluate(ICollection<T> list, BitArray matches)
        {
            foreach (IExpressionEvaluator<T> term in _terms)
            {
                // Add each term's matches to the same set
                term.Evaluate(list, matches);

                // Stop if everything has already matched
                if (matches.TrueCount() == list.Count) { break; }
            }
        }
    }

    public class NotEvaluator<T> : IExpressionEvaluator<T>
    {
        private readonly IExpressionEvaluator<T> _inner;

        public NotEvaluator(IExpressionEvaluator<T> inner)
        {
            _inner = inner;
        }

        public void Evaluate(ICollection<T> list, BitArray matches)
        {
            _inner.Evaluate(list, matches);
            matches.Not();
        }
    }

    public class AllEvaluator<T> : IExpressionEvaluator<T>
    {
        public void Evaluate(ICollection<T> list, BitArray matches)
        {
            matches.SetAll(true);
        }
    }

    public class NoneEvaluator<T> : IExpressionEvaluator<T>
    {
        public void Evaluate(ICollection<T> list, BitArray matches)
        {
            // matches is empty by default
        }
    }
}
