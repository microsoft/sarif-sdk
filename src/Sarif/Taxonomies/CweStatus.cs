// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.CodeAnalysis.Sarif.Taxonomies
{
    /// <summary>
    /// Maturity status of a MITRE CWE entry, as declared in the upstream
    /// <see href="https://cwe.mitre.org/data/xml/cwec_latest.xml.zip">CWE XML feed</see>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Every entry in <see cref="CweTaxonomy"/>'s consolidated SARIF and brief artifacts
    /// carries one of these statuses as the <c>cwe/status</c> property. Callers select a
    /// subset with a bitwise combination of these flags; the default
    /// (<see cref="CweTaxonomy.DefaultStatuses"/>) is <c>Stable | Draft | Incomplete</c>,
    /// matching the practical floor of what real-world scanners actually emit.
    /// </para>
    /// <para>
    /// In cwec_v4.20 the distribution is wildly skewed: 26 Stable, 432 Draft, 486 Incomplete,
    /// 25 Deprecated, 0 Obsolete. See <see cref="CweTaxonomy.DefaultStatuses"/> for why the
    /// default deliberately includes Incomplete (to cover OWASP-tier CWEs like SSRF that
    /// MITRE has not yet promoted to Draft) and excludes Deprecated (so the enricher leaves
    /// a migration signal on stale rule descriptors).
    /// </para>
    /// </remarks>
    [Flags]
    public enum CweStatus
    {
        None = 0,
        Stable = 1,
        Draft = 1 << 1,
        Incomplete = 1 << 2,
        Deprecated = 1 << 3,
        Obsolete = 1 << 4,
        All = Stable | Draft | Incomplete | Deprecated | Obsolete,
    }
}
