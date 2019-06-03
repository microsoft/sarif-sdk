namespace Microsoft.CodeAnalysis.Sarif.Query
{
    // TODO: Query Command outside.

    /// <summary>
    ///  IExpressions are parts of a logical expression (State != Closed AND Priority > 1).
    ///  Expression text is parsed into an IExpression, converted to a form which can be
    ///  evaluated on a given set of items, and then executed.
    /// </summary>
    public interface IExpression
    { }
}
