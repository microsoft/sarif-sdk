// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CodeDom.Compiler;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// An interface for all types generated from the Sarif schema.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.0.0")]
    public interface ISarifNode
    {
        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        SarifNodeKind SarifNodeKind { get; }

        /// <summary>
        /// Makes a deep copy of this instance.
        /// </summary>
        ISarifNode DeepClone();
    }
}