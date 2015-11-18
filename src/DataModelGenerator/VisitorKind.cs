// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.DataModelGenerator
{
    /// <summary>Values that represent visitor kinds.</summary>
    internal enum VisitorKind
    {
        /// <summary>Generate a visitor suitable for observing a given grammar tree.</summary>
        Observing,

        /// <summary>Generate a visitor suitable for mutating a given grammar tree.</summary>
        Rewriting
    }
}
