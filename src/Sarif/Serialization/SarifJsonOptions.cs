// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;

namespace Microsoft.CodeAnalysis.Sarif.Serialization
{
    /// <summary>
    /// Serializer-neutral options for JSON operations performed through <see cref="SarifJson"/>.
    /// </summary>
    public sealed class SarifJsonOptions
    {
        /// <summary>
        /// Gets or sets whether JSON output is indented.
        /// </summary>
        public bool WriteIndented { get; set; }

        /// <summary>
        /// Gets or sets the culture used to convert JSON values.
        /// </summary>
        public CultureInfo Culture { get; set; } = CultureInfo.InvariantCulture;

        /// <summary>
        /// Gets or sets whether HTML-sensitive characters are escaped.
        /// </summary>
        public bool EscapeHtml { get; set; }

        /// <summary>
        /// Gets or sets whether null-valued properties are omitted.
        /// </summary>
        public bool IgnoreNullValues { get; set; }

        /// <summary>
        /// Gets or sets a callback that can inspect and handle serialization errors.
        /// </summary>
        public Action<SarifJsonErrorEventArgs> Error { get; set; }
    }

    /// <summary>
    /// Provides information about an error encountered while serializing or deserializing JSON.
    /// </summary>
    public sealed class SarifJsonErrorEventArgs : EventArgs
    {
        internal SarifJsonErrorEventArgs(
            object currentObject,
            object originalObject,
            object member,
            string path,
            Exception error)
        {
            CurrentObject = currentObject;
            OriginalObject = originalObject;
            Member = member;
            Path = path;
            Error = error;
        }

        public object CurrentObject { get; }

        public object OriginalObject { get; }

        public object Member { get; }

        public string Path { get; }

        public Exception Error { get; }

        /// <summary>
        /// Gets or sets whether the serializer should continue after this error.
        /// </summary>
        public bool Handled { get; set; }
    }
}
