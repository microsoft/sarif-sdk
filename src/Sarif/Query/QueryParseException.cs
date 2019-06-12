// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Runtime.Serialization;

namespace Microsoft.CodeAnalysis.Sarif.Query
{
    [Serializable]
    public class QueryParseException : Exception
    {
        internal QueryParseException(string categoryExpected, StringSlice text)
            : this($"Expected {categoryExpected} but found {(text.Length == 0 ? "<End>" : text)}")
        { }

        public QueryParseException() : base() { }
        public QueryParseException(string message) : base(message) { }
        public QueryParseException(string message, Exception inner) : base(message, inner) { }
        protected QueryParseException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
