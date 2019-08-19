// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Runtime.Serialization;

namespace Microsoft.CodeAnalysis.Test.Utilities.Sarif
{
    /// <summary>
    /// An exception that can be thrown intentionally from test methods, to verify
    /// that the expected exception was thrown.
    /// </summary>
    public class TestException : Exception
    {
        public TestException() : base() { }
        public TestException(string message) : base(message) { }
        public TestException(string message, Exception innerException) : base(message, innerException) { }
        public TestException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
