using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Query
{
    public class ExpressionParserTests
    {
        [Fact]
        public void ExpressionParser_Basics()
        {
            IExpression result;

            result = ExpressionParser.ParseExpression("baselineState = Unchanged");

        }
    }
}
