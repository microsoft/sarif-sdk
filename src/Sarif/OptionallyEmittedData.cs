﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// An enumeration of a subset of SARIF data that may be optionally inserted into
    /// or removed from SARIF log files.
    /// </summary>
    [Flags]
    public enum OptionallyEmittedData
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
        Regions = 0x08,

        // The text snippet, if one exists, that is associated with a static analysis result.
        CodeSnippets = 0x10,

        // A code snippet, if applicable, that includes the text associated with a static
        // analysis result as well as a small amount of the code that surrounds it. A 
        // surrounding code snippet is useful to provide additional context on an issue as 
        // well as to serve as a partial fingerprint in result matching scenarios.
        ContextCodeSnippets = 0x20,

        // Some SARIF data, such as timestamps that denote the start and end of analysis,
        // are not determinitic run-over-run. In order to support caching mechanisms of 
        // modern build systems, it is sometimes helpful to be able to reliably remove this
        // non-deterministic data.
        NondeterministicProperties = 0x40,

        // Environment variables can exfiltrate sensitive information from environments
        // that produce static analysis results. It is useful in some contexts, therefore,
        // to strip this information. 
        EnvironmentVariables = 0x80
    }
}
