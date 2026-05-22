// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// SampleCode.cs - a stable target for SARIF taxonomy samples in this directory.
// The contents are deliberately inert; the file exists to give sample findings
// a real on-disk file + region to highlight. When a viewer navigates to a
// SARIF result emitted by one of the *GenerateSample.ps1 scripts in this
// folder, the region selects the placeholder comment that describes which
// CWE (or other taxonomy entry) the sample was demonstrating. There are no
// actual defects in this file for scanners to flag.

namespace Microsoft.CodeAnalysis.Sarif.Taxonomies.SampleCode
{
    public static class Placeholders
    {
        // Placeholder region for CWE-79 (Improper Neutralization of Input
        // During Web Page Generation - 'Cross-site Scripting').
        public static readonly string Cwe79 = "placeholder";

        // Placeholder region for CWE-89 (Improper Neutralization of Special
        // Elements used in an SQL Command - 'SQL Injection').
        public static readonly string Cwe89 = "placeholder";

        // Placeholder region for CWE-22 (Improper Limitation of a Pathname
        // to a Restricted Directory - 'Path Traversal').
        public static readonly string Cwe22 = "placeholder";

        // Placeholder region for CWE-798 (Use of Hard-coded Credentials).
        public static readonly string Cwe798 = "placeholder";

        // Placeholder region for CWE-1220 (Insufficient Granularity of
        // Access Control).
        public static readonly string Cwe1220 = "placeholder";
    }
}
