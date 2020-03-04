// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.CodeAnalysis.Sarif.Query
{
    /// <summary>
    ///  Generic Expression parser supporting And|Or|Not, precedence, parenthesized subexpressions,
    ///  escaped strings, and terms with standard comparison operators.
    /// </summary>
    /// <remarks>
    ///  Grammar
    ///  =======
    ///   Expression      :=  AndExpression (Or AndExpression)*
    ///   AndExpression   :=  Term (And Term)*
    ///   Term            :=  '(' Expression ')' | Not? PropertyName Operator Value
    ///   PropertyName    :=  String
    ///   Value           :=  String  
    ///   And             := 'AND' | '&&'
    ///   Or              := 'OR' | '||'
    ///   Not             := 'NOT' | '!'
    ///   Operator        :=  '=' | '==' | '&lt;&gt;' | '!=' | '&lt;' | '&lt;=' | '&gt;' | '&gt;='
    ///   String          := [NotSpaceOrRightParen]+ SpaceOrRightParen | '[NotApostrophe | '']*' | "[NotQuote | ""]*"
    /// </remarks>
    public static class ExpressionParser
    {
        private static readonly List<Literal<CompareOperator>> CompareOperators;
        private static readonly List<Literal<ExpressionToken>> Tokens;

        static ExpressionParser()
        {
            // Build sets of operators and tokens.
            // WARNING: The longer forms must come first (== before =) to ensure longer forms are properly interpreted and consumed.

            CompareOperators = new List<Literal<CompareOperator>>()
            {
                new Literal<CompareOperator>("|>", CompareOperator.StartsWith),
                new Literal<CompareOperator>(">|", CompareOperator.EndsWith),
                new Literal<CompareOperator>(":", CompareOperator.Contains),
                new Literal<CompareOperator>("==", CompareOperator.Equals),
                new Literal<CompareOperator>("=", CompareOperator.Equals),
                new Literal<CompareOperator>("!=", CompareOperator.NotEquals),
                new Literal<CompareOperator>("<>", CompareOperator.NotEquals),
                new Literal<CompareOperator>("<=", CompareOperator.LessThanOrEquals),
                new Literal<CompareOperator>("<", CompareOperator.LessThan),
                new Literal<CompareOperator>(">=", CompareOperator.GreaterThanOrEquals),
                new Literal<CompareOperator>(">", CompareOperator.GreaterThan)
            };

            Tokens = new List<Literal<ExpressionToken>>()
            {
                new Literal<ExpressionToken>("AND", ExpressionToken.And),
                new Literal<ExpressionToken>("&&", ExpressionToken.And),
                new Literal<ExpressionToken>("OR", ExpressionToken.Or),
                new Literal<ExpressionToken>("||", ExpressionToken.Or),
                new Literal<ExpressionToken>("NOT", ExpressionToken.Not),
                new Literal<ExpressionToken>("!", ExpressionToken.Not),
                new Literal<ExpressionToken>("(", ExpressionToken.LeftParen),
                new Literal<ExpressionToken>(")", ExpressionToken.RightParen),
                new Literal<ExpressionToken>("*", ExpressionToken.All)
            };
        }

        public static string Escape(string value)
        {
            if (string.IsNullOrEmpty(value)) { return "''"; }

            // If we can avoid escaping, avoid escaping
            if (value.IndexOf(' ') == -1 && value[0] != '\'' && value[0] != '"') { return value; }
            if (value.IndexOf('\'') == -1) { return $"'{value}'"; }
            if (value.IndexOf('\"') == -1) { return $"\"{value}\""; }

            // Otherwise, build the string, doubling each character to escape
            int nextCopyFrom = 0;
            StringBuilder result = new StringBuilder();
            result.Append("'");

            for (int i = 0; i < value.Length; ++i)
            {
                if (value[i] == '\'')
                {
                    // Copy up to and including the character to escape
                    result.Append(value, nextCopyFrom, 1 + i - nextCopyFrom);

                    // Next time, copy starting with it (to get two copies)
                    nextCopyFrom = i;
                }
            }

            // Copy the rest of the string
            if (nextCopyFrom < value.Length)
            {
                result.Append(value, nextCopyFrom, value.Length - nextCopyFrom);
            }

            result.Append("'");
            return result.ToString();
        }

        public static string ToString(CompareOperator op)
        {
            switch (op)
            {
                case CompareOperator.Equals:
                    return "==";
                case CompareOperator.NotEquals:
                    return "!=";
                case CompareOperator.LessThan:
                    return "<";
                case CompareOperator.LessThanOrEquals:
                    return "<=";
                case CompareOperator.GreaterThan:
                    return ">";
                case CompareOperator.GreaterThanOrEquals:
                    return ">=";
                case CompareOperator.StartsWith:
                    return "|>";
                case CompareOperator.Contains:
                    return ":";
                case CompareOperator.EndsWith:
                    return ">|";
                default:
                    throw new NotImplementedException(op.ToString());
            }
        }

        public static IExpression ParseExpression(string expression)
        {
            StringSlice text = new StringSlice(expression);

            // Parse the expression
            IExpression result = ParseExpression(ref text);

            // Verify there was no remaining query text
            ConsumeWhitespace(ref text);
            if (text.Length > 0) { throw new QueryParseException("boolean operator", text); }

            return result;
        }

        private static IExpression ParseExpression(ref StringSlice text)
        {
            ConsumeWhitespace(ref text);
            if (text.Length == 0) { return new AllExpression(); }

            List<IExpression> terms = new List<IExpression>();
            terms.Add(ParseAndExpression(ref text));

            while (true)
            {
                Literal<ExpressionToken> t = StartingToken(ref text);
                if (t?.Value == ExpressionToken.Or)
                {
                    text = text.Substring(t.Text.Length);
                    terms.Add(ParseAndExpression(ref text));
                }
                else
                {
                    break;
                }
            }

            return (terms.Count == 1 ? terms[0] : new OrExpression(terms));
        }

        private static IExpression ParseAndExpression(ref StringSlice text)
        {
            List<IExpression> terms = new List<IExpression>();
            terms.Add(ParseTerm(ref text));

            while (true)
            {
                Literal<ExpressionToken> t = StartingToken(ref text);
                if (t?.Value == ExpressionToken.And)
                {
                    text = text.Substring(t.Text.Length);
                    terms.Add(ParseTerm(ref text));
                }
                else
                {
                    break;
                }
            }

            return (terms.Count == 1 ? terms[0] : new AndExpression(terms));
        }

        private static IExpression ParseTerm(ref StringSlice text)
        {
            Literal<ExpressionToken> t = StartingToken(ref text);

            // All?
            if (t?.Value == ExpressionToken.All)
            {
                text = text.Substring(t.Text.Length);
                return new AllExpression();
            }

            // Parenthesized subexpression?
            if (t?.Value == ExpressionToken.LeftParen)
            {
                text = text.Substring(t.Text.Length);
                IExpression subexpression = ParseExpression(ref text);

                t = StartingToken(ref text);
                if (t?.Value != ExpressionToken.RightParen) { throw new QueryParseException("nested expression end paren", text); }
                text = text.Substring(t.Text.Length);

                return subexpression;
            }

            // Not?
            if (t?.Value == ExpressionToken.Not)
            {
                text = text.Substring(t.Text.Length);
                return new NotExpression(ParseTerm(ref text));
            }

            // PropertyName CompareOperator Value
            StringSlice propertyName = ParseString(ref text);
            CompareOperator op = ParseCompareOperator(ref text);
            StringSlice value = ParseString(ref text);

            return new TermExpression(propertyName.ToString(), op, value.ToString());
        }

        private static CompareOperator ParseCompareOperator(ref StringSlice text)
        {
            ConsumeWhitespace(ref text);

            foreach (Literal<CompareOperator> op in CompareOperators)
            {
                if (text.StartsWith(op.Text, StringComparison.OrdinalIgnoreCase))
                {
                    text = text.Substring(op.Text.Length);
                    return op.Value;
                }
            }

            throw new QueryParseException("compare operator", text);
        }

        private static StringSlice ParseString(ref StringSlice text)
        {
            ConsumeWhitespace(ref text);
            if (text.Length == 0) { throw new QueryParseException("string", text); }

            char start = text[0];
            if (start == '\'' || start == '"')
            {
                char terminator = start;

                // Keep a StringBuilder for unescaping (only if needed) and track what we've copied to it already
                StringBuilder unescapedForm = null;
                int nextCopyFrom = 1;

                int terminatorIndex = 1;
                for (; terminatorIndex < text.Length - 1; ++terminatorIndex)
                {
                    if (text[terminatorIndex] == terminator)
                    {
                        // If this wasn't a doubled quote, the string is done
                        if (text[terminatorIndex + 1] != terminator) { break; }

                        // Copy to the StringBuilder without either quote
                        if (unescapedForm == null) { unescapedForm = new StringBuilder(); }
                        text.Substring(nextCopyFrom, terminatorIndex - nextCopyFrom).AppendTo(unescapedForm);

                        // Include the second quote next time
                        nextCopyFrom = terminatorIndex + 1;

                        // Skip to look after the second quote next iteration
                        terminatorIndex += 1;
                    }
                }

                // Ensure the string is terminated
                if (terminatorIndex == text.Length || text[terminatorIndex] != terminator) { throw new QueryParseException("string terminator", text); }

                if (unescapedForm != null)
                {
                    if (nextCopyFrom < terminatorIndex) { text.Substring(nextCopyFrom, terminatorIndex - nextCopyFrom).AppendTo(unescapedForm); }
                    text = text.Substring(terminatorIndex + 1);
                    return unescapedForm.ToString();
                }
                else
                {
                    StringSlice resultSlice = text.Substring(1, terminatorIndex - 1);
                    text = text.Substring(terminatorIndex + 1);
                    return resultSlice;
                }
            }
            else
            {
                // Read until whitespace or ')' (so closing a subexpression doesn't require a space)
                int terminatorIndex = 0;
                for (; terminatorIndex < text.Length; ++terminatorIndex)
                {
                    char c = text[terminatorIndex];
                    if (c == ')' || IsWhitespace(c)) { break; }
                }

                // Consume and return the string
                StringSlice result = text.Substring(0, terminatorIndex);
                text = text.Substring(terminatorIndex);
                return result;
            }
        }

        private static Literal<ExpressionToken> StartingToken(ref StringSlice text)
        {
            ConsumeWhitespace(ref text);

            foreach (Literal<ExpressionToken> token in Tokens)
            {
                if (text.StartsWith(token.Text, StringComparison.OrdinalIgnoreCase)) { return token; }
            }

            return null;
        }

        private static void ConsumeWhitespace(ref StringSlice text)
        {
            int whitespaceCount = 0;
            for (; whitespaceCount < text.Length; ++whitespaceCount)
            {
                if (!IsWhitespace(text[whitespaceCount])) { break; }
            }

            text = text.Substring(whitespaceCount);
        }

        private static bool IsWhitespace(char c)
        {
            return c == ' ';
        }

        private class Literal<T>
        {
            public StringSlice Text;
            public T Value;

            public Literal(StringSlice text, T value)
            {
                Text = text;
                Value = value;
            }
        }

        private enum ExpressionToken
        {
            LeftParen,
            RightParen,
            And,
            Or,
            Not,
            All
        }
    }
}
