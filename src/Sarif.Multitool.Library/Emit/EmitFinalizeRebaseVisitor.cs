// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    /// <summary>
    /// Rewrites absolute local file paths in a run into relative URIs plus portable, per-repository
    /// <c>uriBaseId</c>s derived from <c>versionControlProvenance</c>. Each artifact location is
    /// resolved against the run's input <c>originalUriBaseIds</c>, attributed to the owning
    /// repository by longest-prefix match on the mapped local root, and re-expressed relative to
    /// that repository's minted output base. The rebuilt <c>originalUriBaseIds</c> anchor each base
    /// at a portable root — a GitHub-compatible blob permalink (commit-pinned in the URL) or an Azure
    /// DevOps repository root (commit pinning carried by <c>versionControlProvenance.revisionId</c>),
    /// derived from the repositoryUri by <see cref="VcpPortableRoot"/> — so the finalized SARIF
    /// carries no machine-specific path. Each minted base also carries a <c>description</c> whose
    /// <c>text</c> is a SARIF embedded link (§3.11.6) linking the short repository name to a
    /// browsable root-at-revision URL and naming the pinned commit, unless the input base already
    /// supplied a description.
    /// </summary>
    /// <remarks>
    /// One repository collapses to the bare <c>SRCROOT</c> base. Multiple repositories each receive
    /// <c>SRCROOT_&lt;REPO-LEAF&gt;</c>, disambiguated by an ordinal suffix on collision. A result URI
    /// that resolves to a local file path under no declared repository root fails finalize (it would
    /// leak); an unmatched URI under a portable scheme is inlined as an absolute reference.
    /// </remarks>
    internal sealed class EmitFinalizeRebaseVisitor : SarifRewritingVisitor
    {
        private const int MaxBaseChainDepth = 64;

        private readonly List<string> _errors = new List<string>();
        private readonly HashSet<string> _referencedBaseIds = new HashSet<string>(StringComparer.Ordinal);
        private readonly List<string> _leakUris = new List<string>();

        private List<RepoRoot> _plan;
        private IDictionary<string, ArtifactLocation> _inputBases;
        private Dictionary<string, string> _sourceToOutputBaseId;

        internal IReadOnlyList<string> Errors => _errors;

        internal bool Success => _errors.Count == 0;

        public override Run VisitRun(Run node)
        {
            if (node == null) { return null; }

            _plan = null;
            _inputBases = null;
            _sourceToOutputBaseId = null;
            _referencedBaseIds.Clear();
            _leakUris.Clear();

            if (!TryBuildPlan(node, out _plan))
            {
                // Planning recorded the failure(s); the run is left untouched so finalize fails
                // before any partial rewrite can ship a half-rebased payload.
                return node;
            }

            _sourceToOutputBaseId = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (RepoRoot root in _plan)
            {
                _sourceToOutputBaseId[root.SourceBaseId] = root.OutputBaseId;
            }

            // Resolve every artifact location against a private copy of the input bases so that
            // mutating live locations during the pass cannot corrupt resolution, then detach the
            // dictionary so the base traversal in the rewriting visitor does not touch the base
            // definitions (they are rebuilt from the plan below).
            _inputBases = SnapshotBases(node.OriginalUriBaseIds);
            node.OriginalUriBaseIds = null;

            Run rewritten = base.VisitRun(node);

            rewritten.OriginalUriBaseIds = BuildOutputBases(_plan);

            VerifyNoLeak(rewritten);

            return rewritten;
        }

        public override VersionControlDetails VisitVersionControlDetails(VersionControlDetails node)
        {
            // mappedTo records the local root, which the minted output base now represents; bind it
            // to that base directly rather than letting the generic rebase touch it.
            RepoRoot root = _plan?.FirstOrDefault(r => ReferenceEquals(r.Vcd, node));
            if (root != null)
            {
                node.MappedTo = new ArtifactLocation { UriBaseId = root.OutputBaseId };
                _referencedBaseIds.Add(root.OutputBaseId);
            }

            return node;
        }

        public override ArtifactLocation VisitArtifactLocation(ArtifactLocation node)
        {
            if (node == null)
            {
                return node;
            }

            if (node.Uri == null)
            {
                // A base-only reference (the artifact is a repository root / directory). Its input
                // uriBaseId is being retired; remap it to the minted output base so it does not
                // dangle. An unmapped base id is recorded so the final assertion rejects it.
                if (!string.IsNullOrEmpty(node.UriBaseId))
                {
                    if (_sourceToOutputBaseId != null
                        && _sourceToOutputBaseId.TryGetValue(node.UriBaseId, out string outputBaseId))
                    {
                        node.UriBaseId = outputBaseId;
                        _referencedBaseIds.Add(outputBaseId);
                    }
                    else
                    {
                        _referencedBaseIds.Add(node.UriBaseId);
                    }
                }

                return node;
            }

            if (!node.TryReconstructAbsoluteUri(_inputBases, out Uri abs))
            {
                // A relative reference we cannot resolve through a declared base. A bare relative URI
                // ships as-is; a rooted local-path shape is a leak the final assertion will reject.
                if (!string.IsNullOrEmpty(node.UriBaseId)) { _referencedBaseIds.Add(node.UriBaseId); }
                if (IsRootedLocalShape(node.Uri)) { _leakUris.Add(node.Uri.OriginalString); }
                return node;
            }

            RepoRoot owner = LongestPrefixOwner(abs);
            if (owner != null)
            {
                node.Uri = owner.LocalRoot.MakeRelativeUri(abs);
                node.UriBaseId = owner.OutputBaseId;
                _referencedBaseIds.Add(owner.OutputBaseId);
                return node;
            }

            if (abs.IsAbsoluteUri && abs.Scheme == Uri.UriSchemeFile)
            {
                _leakUris.Add(abs.OriginalString);
                return node;
            }

            // Already portable (https and friends): inline the absolute reference with no base so the
            // rebuilt originalUriBaseIds cannot leave it dangling.
            node.Uri = abs;
            node.UriBaseId = null;
            return node;
        }

        private bool TryBuildPlan(Run run, out List<RepoRoot> plan)
        {
            plan = null;

            IList<VersionControlDetails> vcp = run.VersionControlProvenance;
            if (vcp == null || vcp.Count == 0)
            {
                _errors.Add("emit-finalize requires run.versionControlProvenance to declare at least one repository, each carrying mappedTo, repositoryUri, and revisionId.");
                return false;
            }

            if (!TryValidateNoBaseCycles(run.OriginalUriBaseIds, out string cycleError))
            {
                _errors.Add(cycleError);
                return false;
            }

            var roots = new List<RepoRoot>(vcp.Count);
            var seenLocalRoots = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < vcp.Count; i++)
            {
                VersionControlDetails vcd = vcp[i];
                string where = string.Format(CultureInfo.InvariantCulture, "versionControlProvenance[{0}]", i);

                if (vcd == null)
                {
                    _errors.Add(string.Format(CultureInfo.InvariantCulture, "{0} must be a version-control-details object.", where));
                    return false;
                }

                if (vcd.MappedTo == null || string.IsNullOrEmpty(vcd.MappedTo.UriBaseId))
                {
                    _errors.Add(string.Format(CultureInfo.InvariantCulture, "{0}.mappedTo must declare a uriBaseId that binds the repository root to an originalUriBaseIds entry.", where));
                    return false;
                }

                if (vcd.MappedTo.Uri != null)
                {
                    _errors.Add(string.Format(CultureInfo.InvariantCulture, "{0}.mappedTo must carry only a uriBaseId; the repository root's local path is declared on the named originalUriBaseIds entry, not inline on mappedTo.", where));
                    return false;
                }

                if (vcd.RepositoryUri == null)
                {
                    _errors.Add(string.Format(CultureInfo.InvariantCulture, "{0}.repositoryUri is required so a portable root can be derived.", where));
                    return false;
                }

                if (string.IsNullOrEmpty(vcd.RevisionId))
                {
                    _errors.Add(string.Format(CultureInfo.InvariantCulture, "{0}.revisionId must be a non-empty commit identifier so a portable root can be derived.", where));
                    return false;
                }

                if (!TryResolveLocalRoot(run.OriginalUriBaseIds, vcd.MappedTo, out Uri localRoot)
                    || !localRoot.IsAbsoluteUri
                    || localRoot.Scheme != Uri.UriSchemeFile)
                {
                    _errors.Add(string.Format(CultureInfo.InvariantCulture, "{0}.mappedTo (uriBaseId '{1}') must resolve to an absolute local file:// path through originalUriBaseIds.", where, vcd.MappedTo.UriBaseId));
                    return false;
                }

                localRoot = EnsureTrailingSlash(localRoot);

                string localKey = localRoot.AbsoluteUri;
                if (seenLocalRoots.TryGetValue(localKey, out int previous))
                {
                    _errors.Add(string.Format(CultureInfo.InvariantCulture, "{0}.mappedTo resolves to the same local root as versionControlProvenance[{1}]; each repository must map to a distinct root.", where, previous));
                    return false;
                }

                seenLocalRoots[localKey] = i;

                if (!VcpPortableRoot.TryDerivePortableRoot(vcd.RepositoryUri, vcd.RevisionId, out Uri portableRoot, out Uri canonicalRepositoryUri, out string leaf, out Uri revisionWebUrl, out string deriveError))
                {
                    _errors.Add(string.Format(CultureInfo.InvariantCulture, "{0}: {1}", where, deriveError));
                    return false;
                }

                // Ship the canonical https identity so a normalized ssh/scp clone URL surfaces as its
                // https form in the finalized run. A credential-bearing repositoryUri is already
                // rejected upstream, so this only reshapes ssh/scp (or relative) inputs; a clean https
                // repositoryUri compares equal and is left byte-identical. Uri.Equals is not used
                // because it ignores userinfo and fragment.
                bool alreadyCanonical = vcd.RepositoryUri.IsAbsoluteUri
                    && string.Equals(vcd.RepositoryUri.AbsoluteUri, canonicalRepositoryUri.AbsoluteUri, StringComparison.Ordinal);

                if (!alreadyCanonical)
                {
                    vcd.RepositoryUri = canonicalRepositoryUri;
                }

                roots.Add(new RepoRoot
                {
                    Vcd = vcd,
                    SourceBaseId = vcd.MappedTo.UriBaseId,
                    LocalRoot = localRoot,
                    PortableRoot = portableRoot,
                    RevisionWebUrl = revisionWebUrl,
                    Leaf = leaf,
                });
            }

            AssignOutputBaseIds(roots);
            plan = roots;
            return true;
        }

        private static void AssignOutputBaseIds(List<RepoRoot> roots)
        {
            if (roots.Count == 1)
            {
                roots[0].OutputBaseId = EmitRunCommand.SourceRootBaseId;
                return;
            }

            var used = new HashSet<string>(StringComparer.Ordinal);
            foreach (RepoRoot root in roots)
            {
                string baseId = EmitRunCommand.SourceRootBaseId + "_" + UpperSnake(root.Leaf);
                string candidate = baseId;
                int n = 2;
                while (!used.Add(candidate))
                {
                    candidate = baseId + "_" + n.ToString(CultureInfo.InvariantCulture);
                    n++;
                }

                root.OutputBaseId = candidate;
            }
        }

        private RepoRoot LongestPrefixOwner(Uri abs)
        {
            RepoRoot best = null;
            int bestLength = -1;

            foreach (RepoRoot root in _plan)
            {
                if (root.LocalRoot.IsBaseOf(abs))
                {
                    int length = root.LocalRoot.AbsoluteUri.Length;
                    if (length > bestLength)
                    {
                        best = root;
                        bestLength = length;
                    }
                }
            }

            return best;
        }

        private IDictionary<string, ArtifactLocation> BuildOutputBases(List<RepoRoot> roots)
        {
            var bases = new Dictionary<string, ArtifactLocation>(StringComparer.Ordinal);
            foreach (RepoRoot root in roots)
            {
                ArtifactLocation baseEntry =
                    _inputBases != null
                    && _inputBases.TryGetValue(root.SourceBaseId, out ArtifactLocation source)
                    && source != null
                        ? source
                        : new ArtifactLocation();

                baseEntry.Uri = root.PortableRoot;
                baseEntry.UriBaseId = null;
                baseEntry.Description ??= new Message
                {
                    Text = string.Format(
                        CultureInfo.InvariantCulture,
                        "Source root mapped to [{0}]({1}) at commit {2}.",
                        root.Leaf,
                        root.RevisionWebUrl.AbsoluteUri,
                        root.Vcd.RevisionId),
                };
                bases[root.OutputBaseId] = baseEntry;
            }

            return bases;
        }

        private void VerifyNoLeak(Run run)
        {
            foreach (string baseId in _referencedBaseIds)
            {
                if (run.OriginalUriBaseIds == null || !run.OriginalUriBaseIds.ContainsKey(baseId))
                {
                    _errors.Add(string.Format(CultureInfo.InvariantCulture, "After rebasing, a location still references undefined uriBaseId '{0}'.", baseId));
                }
            }

            foreach (string leak in _leakUris)
            {
                _errors.Add(string.Format(CultureInfo.InvariantCulture, "Local file path '{0}' could not be attributed to a declared repository root; finalize will not ship a machine-specific path.", leak));
            }

            if (run.OriginalUriBaseIds != null)
            {
                foreach (KeyValuePair<string, ArtifactLocation> kv in run.OriginalUriBaseIds)
                {
                    if (kv.Value?.Uri != null && kv.Value.Uri.IsAbsoluteUri && kv.Value.Uri.Scheme == Uri.UriSchemeFile)
                    {
                        _errors.Add(string.Format(CultureInfo.InvariantCulture, "originalUriBaseIds['{0}'] still anchors at a local path '{1}'.", kv.Key, kv.Value.Uri.OriginalString));
                    }
                }
            }
        }

        private static IDictionary<string, ArtifactLocation> SnapshotBases(IDictionary<string, ArtifactLocation> bases)
        {
            var copy = new Dictionary<string, ArtifactLocation>(StringComparer.Ordinal);
            if (bases != null)
            {
                foreach (KeyValuePair<string, ArtifactLocation> kv in bases)
                {
                    copy[kv.Key] = kv.Value?.DeepClone();
                }
            }

            return copy;
        }

        private static bool TryValidateNoBaseCycles(IDictionary<string, ArtifactLocation> bases, out string error)
        {
            error = null;
            if (bases == null) { return true; }

            foreach (string startKey in bases.Keys)
            {
                var seen = new HashSet<string>(StringComparer.Ordinal);
                string key = startKey;
                int depth = 0;

                while (key != null)
                {
                    if (!seen.Add(key))
                    {
                        error = string.Format(CultureInfo.InvariantCulture, "originalUriBaseIds contains a cyclic uriBaseId chain involving '{0}'.", key);
                        return false;
                    }

                    if (++depth > MaxBaseChainDepth)
                    {
                        error = "originalUriBaseIds uriBaseId chain exceeds the maximum supported depth.";
                        return false;
                    }

                    if (!bases.TryGetValue(key, out ArtifactLocation al) || al == null) { break; }

                    key = al.UriBaseId;
                }
            }

            return true;
        }

        // mappedTo carries only a uriBaseId (enforced in TryBuildPlan); the repository's absolute
        // local root is the resolution of the matching originalUriBaseIds entry.
        private static bool TryResolveLocalRoot(IDictionary<string, ArtifactLocation> bases, ArtifactLocation mappedTo, out Uri localRoot)
        {
            localRoot = null;

            return bases != null
                && !string.IsNullOrEmpty(mappedTo.UriBaseId)
                && bases.TryGetValue(mappedTo.UriBaseId, out ArtifactLocation baseEntry)
                && baseEntry != null
                && baseEntry.TryReconstructAbsoluteUri(bases, out localRoot);
        }

        private static Uri EnsureTrailingSlash(Uri uri)
        {
            string text = uri.AbsoluteUri;
            return text.EndsWith("/", StringComparison.Ordinal)
                ? uri
                : new Uri(text + "/", UriKind.Absolute);
        }

        private static bool IsRootedLocalShape(Uri uri)
        {
            if (uri.IsAbsoluteUri)
            {
                return uri.Scheme == Uri.UriSchemeFile;
            }

            string text = uri.OriginalString ?? string.Empty;
            if (text.Length == 0) { return false; }

            if (text[0] == '/' || text[0] == '\\') { return true; }

            return text.Length >= 2 && text[1] == ':' && char.IsLetter(text[0]);
        }

        private static string UpperSnake(string value)
        {
            var sb = new StringBuilder(value.Length);
            bool lastUnderscore = false;

            foreach (char c in value)
            {
                if (char.IsLetterOrDigit(c))
                {
                    sb.Append(char.ToUpperInvariant(c));
                    lastUnderscore = false;
                }
                else if (!lastUnderscore && sb.Length > 0)
                {
                    sb.Append('_');
                    lastUnderscore = true;
                }
            }

            string result = sb.ToString().TrimEnd('_');
            return result.Length == 0 ? "REPO" : result;
        }

        private sealed class RepoRoot
        {
            public VersionControlDetails Vcd { get; set; }

            public string SourceBaseId { get; set; }

            public Uri LocalRoot { get; set; }

            public Uri PortableRoot { get; set; }

            public Uri RevisionWebUrl { get; set; }

            public string Leaf { get; set; }

            public string OutputBaseId { get; set; }
        }
    }
}
