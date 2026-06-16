// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif.Emit
{
    /// <summary>
    /// Describes a single emit element that was refused, identified by its position in the submitted
    /// payload. A lone object is index 0; an array element carries its zero-based position so a
    /// producer can correct the offender and retry the corrected payload idempotently.
    /// </summary>
    public sealed class EmitElementError
    {
        public EmitElementError(int index, string message, string errorCode = null)
        {
            Index = index;
            Message = message;
            ErrorCode = errorCode;
        }

        /// <summary>The element's zero-based position in the submitted payload (0 for a lone object).</summary>
        public int Index { get; }

        /// <summary>An optional machine-readable code (e.g. <c>AI1012</c>) for the rejection.</summary>
        public string ErrorCode { get; }

        /// <summary>A human/AI-consumable description of why the element was rejected.</summary>
        public string Message { get; }
    }
}
