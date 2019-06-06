// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Text;

namespace Microsoft.CodeAnalysis.Sarif.Query
{
    public class TermExpression : IExpression
    {
        public string PropertyName { get; private set; }
        public CompareOperator Operator { get; private set; }
        public string Value { get; private set; }

        public TermExpression(string propertyName, CompareOperator op, string value)
        {
            PropertyName = propertyName;
            Operator = op;
            Value = value;
        }

        public override string ToString()
        {
            return $"{ExpressionParser.Escape(PropertyName)} {ExpressionParser.ToString(Operator)} {ExpressionParser.Escape(Value)}";
        }
    }

    public class AndExpression : IExpression
    {
        public IReadOnlyList<IExpression> Terms { get; private set; }

        public AndExpression(IReadOnlyList<IExpression> terms)
        {
            Terms = terms;
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();

            foreach (IExpression part in Terms)
            {
                if (result.Length > 0) { result.Append(" AND "); }

                if (part is OrExpression)
                {
                    // Explicit parenthesis are only required on an OR expression inside and AND expression.
                    // AND takes precedence over OR, so A OR B AND C OR D => (A OR (B AND C) OR D)
                    result.Append("(");
                    result.Append(part);
                    result.Append(")");
                }
                else
                {
                    result.Append(part);
                }
            }

            return result.ToString();
        }
    }

    public class OrExpression : IExpression
    {
        public IReadOnlyList<IExpression> Terms { get; private set; }

        public OrExpression(IReadOnlyList<IExpression> terms)
        {
            Terms = terms;
        }

        public override string ToString()
        {
            return string.Join(" OR ", Terms);
        }
    }

    public class NotExpression : IExpression
    {
        public IExpression Inner { get; private set; }

        public NotExpression(IExpression inner)
        {
            Inner = inner;
        }

        public override string ToString()
        {
            return $"NOT({Inner})";
        }
    }

    public class AllExpression : IExpression
    {
        public override string ToString()
        {
            return "*";
        }
    }

    public class NoneExpression : IExpression
    {
        public override string ToString()
        {
            return "NOT(*)";
        }
    }
}
