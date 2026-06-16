// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Emit
{
    /// <summary>
    /// The outcome of an atomic emit append: how many events were appended (all-or-none) and, on
    /// rejection, why. The append is atomic — if any element is rejected, <see cref="Appended"/> is
    /// zero and nothing reaches the sink — so a retry of the corrected payload never double-appends
    /// the elements that were already valid.
    /// </summary>
    /// <remarks>
    /// Two rejection shapes are distinguished. A per-element rejection populates <see cref="Rejected"/>
    /// (the payload was a well-formed object or array, but one or more elements failed validation). A
    /// <see cref="PayloadError"/> is a whole-payload rejection: the payload itself was neither a JSON
    /// object nor an array, so there were no elements to validate. A CLI shell routes
    /// <see cref="PayloadError"/> to stderr and <see cref="Rejected"/> to its structured stdout report.
    /// </remarks>
    public sealed class EmitReport
    {
        public EmitReport(int appended, IReadOnlyList<EmitElementError> rejected, string payloadError = null)
        {
            Appended = appended;
            Rejected = rejected ?? Array.Empty<EmitElementError>();
            PayloadError = payloadError;
        }

        /// <summary>The number of events appended to the sink (0 on any rejection).</summary>
        public int Appended { get; }

        /// <summary>The per-element rejections, ordered by submitted index. Empty on success.</summary>
        public IReadOnlyList<EmitElementError> Rejected { get; }

        /// <summary>
        /// Non-null when the whole payload was rejected before any element could be considered (the
        /// payload was neither a JSON object nor an array).
        /// </summary>
        public string PayloadError { get; }

        /// <summary>True when nothing was rejected and the append committed.</summary>
        public bool Succeeded => PayloadError == null && Rejected.Count == 0;
    }
}
