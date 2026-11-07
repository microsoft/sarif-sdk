// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.CodeAnalysis.Sarif.Serialization
{
    /// <summary>
    /// The base exception for JSON operations performed through <see cref="SarifJson"/>.
    /// </summary>
    public class SarifJsonException : Exception
    {
        public SarifJsonException() { }

        public SarifJsonException(string message)
            : base(message)
        {
        }

        public SarifJsonException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Gets the JSON path at which the error occurred, when available.
        /// </summary>
        public string Path { get; internal set; }
    }

    /// <summary>
    /// An exception caused by malformed JSON.
    /// </summary>
    public sealed class SarifJsonReaderException : SarifJsonException
    {
        public SarifJsonReaderException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Gets the line number at which the error occurred, when available.
        /// </summary>
        public int LineNumber { get; internal set; }

        /// <summary>
        /// Gets the character position within the line at which the error occurred, when available.
        /// </summary>
        public int LinePosition { get; internal set; }
    }

    /// <summary>
    /// An exception caused by JSON that cannot be mapped to or from the requested object model.
    /// </summary>
    public sealed class SarifJsonSerializationException : SarifJsonException
    {
        public SarifJsonSerializationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
