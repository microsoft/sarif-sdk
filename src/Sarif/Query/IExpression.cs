// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif.Query
{
    /// <summary>
    ///  IExpressions are parts of a logical expression (State != Closed AND Priority > 1).
    ///  Expression text is parsed into an IExpression, converted to a form which can be
    ///  evaluated on a given set of items, and then executed.
    /// </summary>
    public interface IExpression
    {
        // No functionality; IExpression is a type constraint all Expression parts share.
    }
}
