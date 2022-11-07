// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.CodeAnalysis.Sarif.Query.Evaluators
{
    /// <summary>
    ///  EvaluatorFactory contains methods to dynamically construct IExpressionEvaluators in
    ///  various contexts.
    /// </summary>
    public static class EvaluatorFactory
    {
        /// <summary>
        ///  BuildPrimitiveEvaluator will build an IExpressionEvaluator for supported primitive
        ///  types, set up to work directly on an array of primitives. you can use BuildPrimitiveEvaluator(typeof(int),
        /// </summary>
        /// <example>
        ///  TermExpression term = ExpressionParser.Parse("value > 5");
        ///  IExpressionEvaluator&lt;int&gt; evaluator = BuildPrimitiveEvaluator(typeof(int), term);
        ///
        ///  int[] set;
        /// BitArray matches = new BitArray(set.Length);
        ///  evaluator.Evaluate(set, matches);
        /// </example>
        /// <param name="fieldType">Primitive type of array to build evaluator for</param>
        /// <param name="term">Query term with comparison and constant to evaluate against array</param>
        /// <returns>IExpressionEvaluator for appropriate type</returns>
        public static object BuildPrimitiveEvaluator(Type fieldType, TermExpression term)
        {
            if (fieldType == typeof(bool))
            {
                return new BoolEvaluator<bool>(value => value, term);
            }
            else if (fieldType == typeof(double))
            {
                return new DoubleEvaluator<double>(value => value, term);
            }
            else if (fieldType == typeof(float))
            {
                return new DoubleEvaluator<float>(value => (double)value, term);
            }
            else if (fieldType == typeof(long))
            {
                return new LongEvaluator<long>(value => value, term);
            }
            else if (fieldType == typeof(int))
            {
                return new LongEvaluator<int>(value => value, term);
            }
            else if (fieldType == typeof(uint))
            {
                return new LongEvaluator<uint>(value => value, term);
            }
            else if (fieldType == typeof(short))
            {
                return new LongEvaluator<short>(value => value, term);
            }
            else if (fieldType == typeof(ushort))
            {
                return new LongEvaluator<ushort>(value => value, term);
            }
            else if (fieldType == typeof(byte))
            {
                return new LongEvaluator<byte>(value => value, term);
            }
            else if (fieldType == typeof(sbyte))
            {
                return new LongEvaluator<sbyte>(value => value, term);
            }
            else if (fieldType == typeof(DateTime))
            {
                return new DateTimeEvaluator<DateTime>(value => value, term);
            }
            else if (fieldType == typeof(string))
            {
                // Default StringComparison only
                return new StringEvaluator<string>(value => value, term, StringComparison.OrdinalIgnoreCase);
            }

            throw new NotImplementedException($"BuildPrimitiveEvaluator not implemented for type {fieldType.FullName}.");
        }
    }
}
