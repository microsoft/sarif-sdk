// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// An enumeration of a subset of SARIF data that may be optionally inserted into
    /// or removed from SARIF log files.
    /// </summary>
    [Flags]
    public enum OptionallyEmittedData : int
    {
        None = 0,

        // Hashes of any file within the 'files' table
        Hashes = 0x1,

        // File that are deemed to be textual via examination of their extension.
        // Text files are useful to embed in SARIF log files to support investigation
        // of individual results.
        TextFiles = 0x2,

        // File that aren't verifiably text files are regarded as binary files. It isn't
        // typical to embed binary files into SARIF log files, as they aren't generally
        // useful in order to undersand or validate an analysis result.
        BinaryFiles = 0x4,

        // Some region properties for text regions do not need to be explicitly expressed. A 
        // region.StartLine value on its own, for example, includes all the remaining text on
        // that line, excluding new line characters, if no other properties are present. In
        // addition to the stard/end property pairs, the CharOffset and CharLength properties
        // can be used to specify a text region. This enum value either comprehensively 
        // populates all possible region properties or reduces all regions to a minimal form.
        ComprehensiveRegionProperties = 0x08,

        // The text snippet, if one exists, that is associated with a static analysis result.
        // This snippet matches the result region precisely. In practice, this means that
        // this snippet will typically be a partial line fragment, of limited utility
        // in viewer and fingerprinting scenarios. This limitation is being tracked
        // in as SARIF v2 design issue: https://github.com/oasis-tcs/sarif-spec/issues/197
        RegionSnippets = 0x10,

        // A code snippet, if applicable, that includes the text associated with a static
        // analysis result as well as a small amount of the code that surrounds it. A 
        // surrounding code snippet is useful to provide additional context on an issue as 
        // well as to serve as a partial fingerprint in result matching scenarios.
        ContextRegionSnippets = 0x20,

        // Some SARIF data, such as timestamps that denote the start and end of analysis,
        // are not deterministic run-over-run. In order to support caching mechanisms of 
        // modern build systems, it is sometimes helpful to be able to reliably remove this
        // non-deterministic data.
        NondeterministicProperties = 0x40,

        // Environment variables can exfiltrate sensitive information from environments
        // that produce static analysis results. It is useful in some contexts, therefore,
        // to strip this information. 
        EnvironmentVariables = 0x80,

        // SARIF messages can be rendered as combinations of format string singletons,
        // persisted to the resources.rules property plus result-specific arguments, a
        // fully constructed text message on the result, or both. This value can be 
        // used to flatten or remove fully contructed messages, in cases where
        // both versions exist
        FlattenedMessages = 0x100,

        // SARIF Results may each have a GUID assigned to uniquely identify them.
        Guids = 0x200,

        // A special enum value that indicates that insertion should overwrite any existing
        // information in the SARIF log file. In the absence of this setting, any existing
        // data that would otherwise have been overwritten by the insert operation will
        // be preserved.
        OverwriteExistingData = 0x40000000,

        // Insert Everything - should include every flag except the overwrite one
        All = ~OverwriteExistingData
    }
}
