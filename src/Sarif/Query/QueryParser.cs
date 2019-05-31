using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Microsoft.CodeAnalysis.Sarif.Query
{
    public struct StringSlice
    {
        private string Value { get; set; }
        private int Index { get; set; }
        public int Length { get; private set; }
        public char this[int index] => Value[Index + index];

        public StringSlice(string value, int index, int length)
        {
            if (index < 0 || length < 0 || index + length > (value?.Length ?? 0)) { throw new ArgumentOutOfRangeException($"StringSlice for index {index}, length {length} out of range for string length {value?.Length ?? 0}"); }
            Value = value;
            Index = index;
            Length = length;
        }

        public static implicit operator StringSlice(string value)
        {
            return new StringSlice(value, 0, value?.Length ?? 0);
        }

        public StringSlice Substring(int index, int length = -1)
        {
            if (length == -1) { length = Length - index; }
            return new StringSlice(Value, Index + index, length);
        }

        public void AppendTo(StringBuilder builder)
        {
            builder.Append(Value, Index, Length);
        }

        public override string ToString()
        {
            return Value?.Substring(Index, Length) ?? "";
        }
    }

    [Serializable]
    public class QueryParseException : Exception
    {
        public QueryParseException(string categoryExpected, StringSlice text)
            : this($"Expected {categoryExpected} but found {(text.Length == 0 ? "<End>" : text)}")
        { }

        public QueryParseException() : base() { }
        public QueryParseException(string message) : base(message) { }
        public QueryParseException(string message, Exception inner) : base(message, inner) { }
        protected QueryParseException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    /// <summary>
    ///  Parses Sarif Log queries.
    /// </summary>
    /// <remarks>
    ///  Grammar
    ///  =======
    ///   Query           :=  AndExpression (Or AndExpression)*
    ///   AndExpression   :=  Term (And Term)*
    ///   And             := 'AND' | '&&'
    ///   Or              := 'OR' | '||'
    ///   Term            :=  PropertyName Operator Value
    ///   PropertyName    :=  String
    ///   Value           :=  String  
    ///   Operator        :=  '=' | '==' | '&lt;&gt;' | '!=' | '&lt;' | '&lt;=' | '&gt;' | '&gt;='
    ///   String          := [NotSpace]+ Space | '[NotApostrophe | '']*' | "[NotQuote | ""]*"
    /// </remarks>
    public static class QueryParser
    {
        private static Dictionary<string, Operator> Operators;

        static QueryParser()
        {
            Operators = new Dictionary<string, Operator>();
            Operators["="]  = Operator.Equals;
            Operators["=="] = Operator.Equals;
            Operators["<>"] = Operator.NotEquals;
            Operators["!="] = Operator.NotEquals;
            Operators["<"]  = Operator.LessThan;
            Operators["<="] = Operator.LessThanOrEquals;
            Operators[">"]  = Operator.GreaterThan;
            Operators[">="] = Operator.GreaterThanOrEquals;
        }

        public static IExpression Parse(ref StringSlice text)
        {
            if (text.Length == 0) { return new AllExpression(); }

            while (text.Length > 0)
            {

            }
        }

        public static IExpression ParseTerm(ref StringSlice text)
        {
            string propertyName = ParseString(ref text);
            Operator op = ParseOperator(ref text);
            string value = ParseString(ref text);

            return new TermExpression(propertyName, op, value);
        }

        public static Operator ParseOperator(ref StringSlice text)
        {
            ConsumeWhitespace(ref text);
            if (text.Length == 0) { throw new QueryParseException("operator", text); }

            char first = text[0];
            if (first == '=')
            {

            }
        }

        public static string ParseString(ref StringSlice text)
        {
            ConsumeWhitespace(ref text);

            char start = text[0];
            if (start == '\'' || start == '"')
            {
                StringBuilder result = new StringBuilder();

                int i = 1;
                int copiedFrom = i;

                for (; i < text.Length - 1; ++i)
                {
                    if (text[i] == start)
                    {
                        if (text[i + 1] != start) { break; }
                        text.Substring(copiedFrom, i - copiedFrom).AppendTo(result);
                        copiedFrom = i;
                    }
                }

                text.Substring(copiedFrom, i - copiedFrom).AppendTo(result);
                text = text.Substring(i + 1);
                return result.ToString();
            }
            else
            {
                int i = 0;
                for (; i < text.Length; ++i)
                {
                    if (text[i] == ' ') { break; }
                }

                return text.Substring(0, i).ToString();
            }
        }

        public static void ConsumeWhitespace(ref StringSlice text)
        {
            int i = 0;
            for (; i < text.Length; ++i)
            {
                if (text[i] != ' ') { break; }
            }

            text = text.Substring(i);
        }
    }
}
