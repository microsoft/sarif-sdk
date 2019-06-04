using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Query
{
    public class ExpressionParserTests
    {
        private static void AssertParsesTo(string expected, string query)
        {
            // Verify Parse
            IExpression expression = ExpressionParser.ParseExpression(query);
            string actual = expression.ToString();
            Assert.Equal(expected, actual);

            // Verify Expression.ToString() value parses back to itself correctly
            expression = ExpressionParser.ParseExpression(actual);
            Assert.Equal(actual, expression.ToString());
        }

        [Fact]
        public void ExpressionParser_Basics()
        {
            // Simple Term
            AssertParsesTo("baselineState == Unchanged", "baselineState = Unchanged");

            // Whitespace handling
            AssertParsesTo("baselineState == Unchanged", "  baselineState = Unchanged ");

            // Compare Operators
            AssertParsesTo("baselineState == Unchanged", "baselineState == Unchanged");
            AssertParsesTo("baselineState != Unchanged", "baselineState != Unchanged");
            AssertParsesTo("baselineState != Unchanged", "baselineState <> Unchanged");
            AssertParsesTo("baselineState < Unchanged", "baselineState <  Unchanged");
            AssertParsesTo("baselineState <= Unchanged", "baselineState <= Unchanged");
            AssertParsesTo("baselineState > Unchanged", "baselineState >  Unchanged");
            AssertParsesTo("baselineState >= Unchanged", "baselineState >= Unchanged");

            // String Quoting forms
            AssertParsesTo("baselineState == Unchanged", "'baselineState'=\"Unchanged\"");

            // Decoding Multiple Escaped Values
            AssertParsesTo("baselineState == I'J'K", "baselineState = 'I''J''K'");
            AssertParsesTo("baselineState == \"''\"", "baselineState = ''''''");
            AssertParsesTo("baselineState == I\"J\"K", "baselineState = \"I\"\"J\"\"K\"");
            AssertParsesTo("baselineState == '\"\"'", "baselineState = \"\"\"\"\"\"");

            // Escaping and "don't need to escape" handling
            AssertParsesTo("Isn't == Name\"Scott\"", "Isn't = Name\"Scott\"");
            AssertParsesTo("Isn't == Name\"Scott\"", "'Isn''t' = \"Name\"\"Scott\"\"\"");
            AssertParsesTo("Isn't == Name\"Scott\"", "\"Isn't\" = 'Name\"Scott\"'");
            AssertParsesTo("'Created State' == 'New Item'", "'Created State' = \"New Item\"");

            // Escaping on output
            AssertParsesTo("baselineState == 'Isn''t this value \"bad\"?'", "'baselineState' = \"Isn't this value \"\"bad\"\"?\"");

            // Not
            AssertParsesTo("NOT(baselineState == Unchanged)", "!baselineState = Unchanged");
            AssertParsesTo("NOT(baselineState == Unchanged)", "NOT(baselineState == 'Unchanged')");
            AssertParsesTo("NOT(baselineState == Unchanged)", "nOt(baselineState == 'Unchanged')");

            // And
            AssertParsesTo("State == Active AND Priority > 1", "State == Active AND Priority > 1");
            AssertParsesTo("State == Active AND Priority > 1", "State == Active And Priority > 1");
            AssertParsesTo("State == Active AND Priority > 1", "State == Active && Priority > 1");
            AssertParsesTo("Priority > 1 AND Priority > 2 AND Priority > 3", "Priority > 1 AND Priority > 2 AND Priority > 3");

            // Or
            AssertParsesTo("State == Active OR Priority > 1", "State == Active OR Priority > 1");
            AssertParsesTo("State == Active OR Priority > 1", "State == Active oR Priority > 1");
            AssertParsesTo("State == Active OR Priority > 1", "State == Active || Priority > 1");
            AssertParsesTo("Priority > 1 OR Priority > 2 OR Priority > 3", "Priority > 1 OR Priority > 2 OR Priority > 3");

            // Precedence, parens only emitted when needed
            AssertParsesTo("State == Active AND Priority > 1 OR Priority == 1", "State == Active AND Priority > 1 OR Priority == 1");
            AssertParsesTo("State == Active AND Priority > 1 OR Priority == 1", "(State == Active AND Priority > 1) OR Priority == 1");
            AssertParsesTo("State == Active AND (Priority > 1 OR Priority == 1)", "State == Active AND (Priority > 1 OR Priority == 1)");

            // Empty string is All Query
            AssertParsesTo("*", "");
            AssertParsesTo("*", " ");

            // All/None
            AssertParsesTo("*", "*");
            AssertParsesTo("*", "(*)");
            AssertParsesTo("NOT(*)", "!*");
            AssertParsesTo("NOT(*)", "NOT(*)");
        }

        [Fact]
        public void ExpressionParser_Errors()
        {
            // No Operator
            AssertThrows("baselineState");

            // No Value
            AssertThrows("baselineState =");

            // Unknown Operator
            AssertThrows("baselineState *= 1");

            // No Column Name
            AssertThrows(">= 1");

            // Unknown Boolean Operator
            AssertThrows("State == Active Und Priority == 1");

            // Unclosed string
            AssertThrows("'State == Active");
            AssertThrows("\"State == Active");
            AssertThrows("'State' == 'Active");
            AssertThrows("\"State\" == \"Active");

            // Unclosed subexpression
            AssertThrows("NOT(State == Active");
        }

        private void AssertThrows(string query)
        {
            Assert.Throws<QueryParseException>(() => ExpressionParser.ParseExpression(query));
        }
    }
}
