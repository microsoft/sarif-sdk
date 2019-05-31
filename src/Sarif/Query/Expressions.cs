using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.CodeAnalysis.Sarif.Query
{
    public class TermExpression : IExpression
    {
        public string PropertyName { get; private set; }
        public Operator Operator { get; private set; }
        public string Value { get; private set; }

        public TermExpression(string propertyName, Operator op, string value)
        {
            PropertyName = propertyName;
            Operator = op;
            Value = value;
        }

        public SarifLog Evaluate(SarifLog source)
        {
            throw new NotImplementedException();
        }

        public string ToQueryString()
        {
            throw new NotImplementedException();
        }
    }

    public class AndExpression : IExpression
    {

    }

    public class OrExpression : IExpression
    {

    }

    public class AllExpression : IExpression
    { }

    public class EmptyExpression : IExpression
    {

    }
}
