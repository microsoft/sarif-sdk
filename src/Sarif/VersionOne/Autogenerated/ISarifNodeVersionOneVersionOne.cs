// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CodeDom.Compiler;

namespace Microsoft.CodeAnalysis.Sarif.VersionOne
{
    /// <summary>
    /// An interface for all types generated from the Sarif schema.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.52.0.0")]
    public interface ISarifNodeVersionOne
    {
        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNodeVersionOne" />.
        /// </summary>
        SarifNodeKindVersionOne SarifNodeKindVersionOne { get; }

        /// <summary>
        /// Makes a deep copy of this instance.
        /// </summary>
        ISarifNodeVersionOne DeepClone();
    }
}