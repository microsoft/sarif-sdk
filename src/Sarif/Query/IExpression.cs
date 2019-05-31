namespace Microsoft.CodeAnalysis.Sarif.Query
{
    public interface IExpression
    {
        SarifLog Evaluate(SarifLog source);
        string ToQueryString();
    }
}
