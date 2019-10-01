using System;

namespace Microsoft.CodeAnalysis.Sarif.Query.Evaluators
{
    public static class PrimitiveEvaluatorFactory
    {
        public static object BuildPrimitiveEvaluator(Type fieldType, TermExpression term)
        {
            if (fieldType == typeof(bool))
            {
                return new BoolEvaluator<bool>(value => value, term);
            }
            else if(fieldType == typeof(double))
            {
                return new DoubleEvaluator<double>(value => value, term);
            }
            else if(fieldType == typeof(float))
            {
                return new DoubleEvaluator<float>(value => (double)value, term);
            }
            else if(fieldType == typeof(long))
            {
                return new LongEvaluator<long>(value => value, term);
            }
            else if(fieldType == typeof(int))
            {
                return new LongEvaluator<int>(value => (long)value, term);
            }
            else if (fieldType == typeof(uint))
            {
                return new LongEvaluator<uint>(value => (long)value, term);
            }
            else if (fieldType == typeof(short))
            {
                return new LongEvaluator<short>(value => (long)value, term);
            }
            else if (fieldType == typeof(ushort))
            {
                return new LongEvaluator<ushort>(value => (long)value, term);
            }
            else if (fieldType == typeof(byte))
            {
                return new LongEvaluator<byte>(value => (long)value, term);
            }
            else if (fieldType == typeof(sbyte))
            {
                return new LongEvaluator<sbyte>(value => (long)value, term);
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
