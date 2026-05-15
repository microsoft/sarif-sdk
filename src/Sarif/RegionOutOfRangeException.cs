// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Runtime.Serialization;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Thrown when a SARIF <see cref="Region"/> references content outside the bounds of the
    /// artifact it points at (e.g., a <c>startLine</c> beyond the file's last line, or a
    /// <c>charOffset</c>/<c>charLength</c> pair extending past end-of-file).
    /// </summary>
    /// <remarks>
    /// <para>
    /// This typically indicates drift between a SARIF log and its source artifacts — for instance,
    /// when a log produced against one commit is post-processed against a partial clone, a
    /// submodule snapshot, or a checkout at a different revision. Historically the SDK propagated
    /// the underlying <see cref="ArgumentOutOfRangeException"/> from string/line-index math, which
    /// leaked framework internals to callers and made it hard to distinguish "the SARIF log is
    /// invalid" from "your indexing arithmetic crashed."
    /// </para>
    /// <para>
    /// Catch this exception when invoking <c>InsertOptionalDataVisitor</c> against potentially
    /// drifted inputs to convert the failure into a SARIF-level diagnostic (for example, a
    /// <c>toolExecutionNotifications</c> entry) instead of an unhandled crash.
    /// </para>
    /// </remarks>
    [Serializable]
    public class RegionOutOfRangeException : Exception
    {
        public RegionOutOfRangeException()
        {
        }

        public RegionOutOfRangeException(string message)
            : base(message)
        {
        }

        public RegionOutOfRangeException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Creates an exception describing a region that exceeds the bounds of the artifact at
        /// <paramref name="artifactUri"/>.
        /// </summary>
        public RegionOutOfRangeException(Region region, Uri artifactUri, Exception innerException = null)
            : base(FormatMessage(region, artifactUri), innerException)
        {
            this.Region = region;
            this.ArtifactUri = artifactUri;
        }

        protected RegionOutOfRangeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public Region Region { get; }

        public Uri ArtifactUri { get; }

        private static string FormatMessage(Region region, Uri artifactUri)
        {
            string uriText = artifactUri?.OriginalString ?? "<unknown artifact>";

            if (region == null)
            {
                return $"A SARIF region references content outside the bounds of artifact '{uriText}'.";
            }

            // Choose whichever set of region properties is most likely to be informative.
            if (region.StartLine > 0)
            {
                return $"A SARIF region (startLine={region.StartLine}, endLine={region.EndLine}) " +
                       $"references content outside the bounds of artifact '{uriText}'. " +
                       "This typically indicates the SARIF log was produced against a different revision of the artifact.";
            }

            return $"A SARIF region (charOffset={region.CharOffset}, charLength={region.CharLength}) " +
                   $"references content outside the bounds of artifact '{uriText}'. " +
                   "This typically indicates the SARIF log was produced against a different revision of the artifact.";
        }
    }
}
