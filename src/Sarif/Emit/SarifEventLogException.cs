// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Runtime.Serialization;

namespace Microsoft.CodeAnalysis.Sarif.Emit
{
    /// <summary>
    /// Thrown when the event log is malformed, corrupt, locked, or carries an unsupported
    /// schema version for a known kind.
    /// </summary>
    [Serializable]
    public class SarifEventLogException : Exception
    {
        public SarifEventLogException() { }

        public SarifEventLogException(string message) : base(message) { }

        public SarifEventLogException(string message, Exception innerException)
            : base(message, innerException) { }

        protected SarifEventLogException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
