// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.CodeAnalysis.Sarif.Driver.Sdk
{
    public class ExitApplicationException<T> : Exception where T : struct
    {
        public ExitApplicationException() : base() { }
        public ExitApplicationException(string message) : base(message) { }
        public ExitApplicationException(string message, Exception innerException) : base(message, innerException) { }

        public T ExitReason { get; set; }
    }
}
