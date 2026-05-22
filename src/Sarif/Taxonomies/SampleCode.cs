// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// SampleCode.cs - a stable target for SARIF taxonomy samples in this directory.
// The contents are deliberately inert; the file exists to give sample findings
// a real on-disk file + region to highlight. When a viewer navigates to a
// SARIF result emitted by one of the *GenerateSample.ps1 scripts in this
// folder, the region selects the placeholder comment that describes which
// taxonomy entry (or NOVEL escape hatch) the sample was demonstrating.
// There are no actual defects in this file for scanners to flag.

namespace Microsoft.CodeAnalysis.Sarif.Taxonomies.SampleCode
{
    public static class Placeholders
    {
        // Placeholder region for CWE-79 sub-id 'unescaped-view-input': bare
        // template interpolation of a user value.
        public static readonly string Cwe79UnescapedViewInput = "placeholder";

        // Placeholder region for CWE-89 sub-id 'string-concat-query': SQL
        // built by concatenating an untrusted value.
        public static readonly string Cwe89StringConcatQuery = "placeholder";

        // Placeholder region for CWE-22 sub-id 'untrusted-path-no-canon':
        // Path opened from untrusted input without canonicalization.
        public static readonly string Cwe22UntrustedPathNoCanon = "placeholder";

        // Placeholder region for CWE-798 sub-id 'embedded-credential':
        // Production secret baked into source code.
        public static readonly string Cwe798EmbeddedCredential = "placeholder";

        // Placeholder region for CWE-1220 sub-id 'missing-tenant-scope':
        // AuthZ check ignores the caller's tenant context.
        public static readonly string Cwe1220MissingTenantScope = "placeholder";

        // Placeholder region for CWE-79 sub-id 'dom-xss-via-sanitizer-bypass':
        // demonstrates that two sub-ids can share the same base descriptor
        // (CWE-79) after replay.
        public static readonly string Cwe79DomXssBypass = "placeholder";

        // Placeholder region for a NOVEL- escape-hatch finding:
        // 'NOVEL-prompt-injection-via-system-message'. Demonstrates the
        // shape used when no taxonomy entry fits — flat, no slash.
        public static readonly string NovelPromptInjection = "placeholder";
    }
}

