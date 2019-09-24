// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.CodeAnalysis.Sarif.Query
{
    public enum CompareOperator
    {
        Equals,
        NotEquals,
        GreaterThan,
        GreaterThanOrEquals,
        LessThan,
        LessThanOrEquals,

        StartsWith,
        Contains,
        EndsWith
    }
}
