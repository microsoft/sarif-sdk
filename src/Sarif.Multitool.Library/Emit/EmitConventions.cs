// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    /// <summary>
    /// Conventions shared by the <c>emit-*</c> verbs.
    /// </summary>
    internal static class EmitConventions
    {
        /// <summary>
        /// Returns the wip event-log path for a given destination SARIF path
        /// (<c>foo.sarif</c> → <c>foo.sarif.wip.jsonl</c>).
        /// </summary>
        internal static string GetWipPath(string sarifOutputPath) => sarifOutputPath + ".wip.jsonl";

        /// <summary>The URI-base-id used for source-root.</summary>
        internal const string SourceRootBaseId = "SRCROOT";
    }
}
