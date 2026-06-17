// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    /// <summary>
    /// Single source of truth for turning a <c>versionControlProvenance.repositoryUri</c> into a
    /// portable artifact root. <see cref="EmitFinalizeRebaseVisitor"/> mints the root at finalize;
    /// <see cref="EmitRunCommand"/> validates the repositoryUri shape at receipt so a producer learns
    /// of a malformed value at authorship rather than after a full run is assembled.
    /// </summary>
    /// <remarks>
    /// Two repository families are recognized:
    /// <list type="bullet">
    /// <item><description>
    /// Azure DevOps: <c>dev.azure.com</c> only, in the exact form
    /// <c>https://dev.azure.com/&lt;org&gt;/&lt;project&gt;/_git/&lt;repo&gt;</c>. The portable root is
    /// the repository root; commit pinning rides on <c>versionControlProvenance.revisionId</c>
    /// because Azure DevOps per-file web URLs are query-based
    /// (<c>?path=&amp;version=GC&lt;sha&gt;</c>) and cannot serve as a uriBaseId prefix. The legacy
    /// <c>&lt;org&gt;.visualstudio.com</c> form is rejected; callers must supply the dev.azure.com
    /// URL, and the derived root is always emitted in that form.
    /// </description></item>
    /// <item><description>
    /// GitHub: <c>github.com</c> (public OSS and Enterprise Managed Users on dotcom) and the
    /// data-residency / EMU hosts <c>&lt;slug&gt;.ghe.com</c>, each with a two-segment
    /// <c>&lt;owner&gt;/&lt;repo&gt;</c> path. The portable root is a commit-pinned blob permalink
    /// (<c>https://&lt;host&gt;/&lt;owner&gt;/&lt;repo&gt;/blob/&lt;revisionId&gt;/</c>). The host set
    /// is an allow-list: any other host is rejected so a confidently-wrong link is never minted.
    /// Custom-hostname GitHub Enterprise Server deployments are out of scope.
    /// </description></item>
    /// </list>
    /// SSH and scp-style clone URLs for the GitHub family are normalized to https first. Azure DevOps
    /// SSH normalization is not supported; such a repositoryUri is rejected with a pointer to the
    /// https clone URL. The derivation also yields a canonical repositoryUri — the https identity with
    /// any userinfo stripped — so a credential-bearing or ssh clone URL never ships in the finalized
    /// run.
    /// </remarks>
    internal static class VcpPortableRoot
    {
        // The only supported Azure DevOps host. The legacy <org>.visualstudio.com form is rejected.
        private const string AzureDevOpsHost = "dev.azure.com";

        // Azure DevOps SSH endpoint. Its clone path is restructured relative to the https form
        // (v3/<org>/<project>/<repo> with no _git), so it cannot be normalized by the GitHub scheme
        // swap; reject with guidance instead of minting a bad root.
        private const string AzureDevOpsSshHost = "ssh.dev.azure.com";

        /// <summary>
        /// Validates that <paramref name="rawRepositoryUri"/> has a shape from which a portable root
        /// can be derived, without minting one (no revisionId required). Used at emit-run receipt.
        /// </summary>
        internal static bool TryValidateRepositoryUri(Uri rawRepositoryUri, out string leaf, out string error)
        {
            leaf = null;

            if (!TryClassify(rawRepositoryUri, out Classification classification, out error))
            {
                return false;
            }

            leaf = classification.Leaf;
            return true;
        }

        /// <summary>
        /// Reports whether <paramref name="run"/> is hosted on GitHub: it carries at least one
        /// <c>versionControlProvenance</c> entry and every entry's <c>repositoryUri</c> classifies as a
        /// supported GitHub host (<c>github.com</c> or a <c>&lt;slug&gt;.ghe.com</c> host). Default-deny:
        /// a run with no provenance, a null entry, or any entry that is Azure DevOps or unclassifiable
        /// yields <c>false</c>. This is the single discriminator for GitHub-only finalize enrichments —
        /// rule-level <c>security-severity</c> and the <c>primaryLocationLineHash</c> partial fingerprint
        /// — which have no Azure DevOps analog.
        /// </summary>
        internal static bool IsGitHubHostedRun(Run run)
        {
            IList<VersionControlDetails> provenance = run?.VersionControlProvenance;
            if (provenance == null || provenance.Count == 0) { return false; }

            foreach (VersionControlDetails details in provenance)
            {
                if (details == null
                    || !TryClassify(details.RepositoryUri, out Classification classification, out _)
                    || !classification.IsGitHub)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Resolves the Azure DevOps organization, project, and repository from
        /// <paramref name="rawRepositoryUri"/>, applying the same host and credential guards as
        /// portable-root derivation. Fails when the repository is not an Azure DevOps target. The
        /// coordinates are URL-path escaped, ready to compose into a REST endpoint path.
        /// </summary>
        internal static bool TryGetAzureDevOpsTarget(Uri rawRepositoryUri, out string organization, out string project, out string repository, out string error)
        {
            organization = null;
            project = null;
            repository = null;

            if (!TryClassify(rawRepositoryUri, out Classification classification, out error))
            {
                return false;
            }

            if (!classification.IsAzureDevOps)
            {
                error = string.Format(CultureInfo.InvariantCulture, "publish to GHAzDO requires an Azure DevOps repositoryUri of the form https://dev.azure.com/<org>/<project>/_git/<repo>; '{0}' is not one.", classification.Display);
                return false;
            }

            string[] prefix = classification.AdoPrefix.Split('/');
            organization = prefix[0];
            project = prefix[1];
            repository = classification.RepoForUrl;
            return true;
        }

        /// <summary>
        /// Resolves the GitHub upload target (<paramref name="owner"/>/<paramref name="repository"/>)
        /// and the REST API host (<paramref name="apiHost"/>) from a run's raw repositoryUri, reusing
        /// the shared classifier so credential-bearing, non-https, and non-GitHub URIs are rejected.
        /// Fails when the repository is not a GitHub target.
        /// </summary>
        internal static bool TryGetGitHubTarget(Uri rawRepositoryUri, out string owner, out string repository, out string apiHost, out string error)
        {
            owner = null;
            repository = null;
            apiHost = null;

            if (!TryClassify(rawRepositoryUri, out Classification classification, out error))
            {
                return false;
            }

            if (!classification.IsGitHub)
            {
                error = string.Format(CultureInfo.InvariantCulture, "publish to GHAS requires a GitHub repositoryUri of the form https://github.com/<owner>/<repo> (or a <slug>.ghe.com host); '{0}' is not one.", classification.Display);
                return false;
            }

            owner = classification.Owner;
            repository = classification.RepoForUrl;

            // Derive the API host from the *normalized* host (TryClassify rewrites an SSH/scp clone
            // URL to https), not the raw URI host — an scp-style URL has no Uri.Host and would yield
            // a broken "api." host.
            apiHost = GitHubApiHost(new Uri(classification.SchemeAndServer).Host);
            return true;
        }

        // github.com → api.github.com; a <slug>.ghe.com data-residency host → api.<slug>.ghe.com.
        // These are the only GitHub hosts the classifier admits, so no other shape reaches here.
        private static string GitHubApiHost(string host)
            => string.Equals(host, "github.com", StringComparison.OrdinalIgnoreCase)
                ? "api.github.com"
                : "api." + host.ToLowerInvariant();

        /// <summary>
        /// Mints the portable root for <paramref name="rawRepositoryUri"/>. Used at emit-finalize.
        /// <paramref name="canonicalRepositoryUri"/> is the clean https identity (userinfo stripped,
        /// ssh/scp normalized) that should be written back onto the run so the finalized SARIF never
        /// ships a credential-bearing or non-https repositoryUri.
        /// </summary>
        internal static bool TryDerivePortableRoot(Uri rawRepositoryUri, string revisionId, out Uri portableRoot, out Uri canonicalRepositoryUri, out string leaf, out Uri revisionWebUrl, out string error)
        {
            portableRoot = null;
            canonicalRepositoryUri = null;
            leaf = null;
            revisionWebUrl = null;

            if (!TryClassify(rawRepositoryUri, out Classification classification, out error))
            {
                return false;
            }

            leaf = classification.Leaf;

            if (!Uri.TryCreate(classification.Display, UriKind.Absolute, out canonicalRepositoryUri))
            {
                error = string.Format(CultureInfo.InvariantCulture, "could not compose a canonical repositoryUri from '{0}'.", classification.Display);
                return false;
            }

            string composed = classification.IsAzureDevOps
                ? string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}/{1}/_git/{2}/",
                    classification.SchemeAndServer,
                    classification.AdoPrefix,
                    classification.RepoForUrl)
                : string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}/{1}/{2}/blob/{3}/",
                    classification.SchemeAndServer,
                    classification.Owner,
                    classification.RepoForUrl,
                    Uri.EscapeDataString(revisionId));

            if (!Uri.TryCreate(composed, UriKind.Absolute, out portableRoot))
            {
                error = string.Format(CultureInfo.InvariantCulture, "could not compose a portable root from repositoryUri '{0}'.", classification.Display);
                return false;
            }

            // A browsable web URL anchored at the revision. The portable root above is a uriBaseId
            // prefix (a GitHub blob/<sha>/ path that expects a file beneath it), so it is not itself
            // clickable; this companion URL points at the repository root as it stood at the commit.
            // GitHub exposes that as a /tree/<sha> permalink; Azure DevOps as a ?version=GC<sha> query
            // on the repository root (its per-file web URLs are query-based for the same reason).
            string revisionWeb = classification.IsAzureDevOps
                ? string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}/{1}/_git/{2}?version=GC{3}",
                    classification.SchemeAndServer,
                    classification.AdoPrefix,
                    classification.RepoForUrl,
                    Uri.EscapeDataString(revisionId))
                : string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}/{1}/{2}/tree/{3}",
                    classification.SchemeAndServer,
                    classification.Owner,
                    classification.RepoForUrl,
                    Uri.EscapeDataString(revisionId));

            if (!Uri.TryCreate(revisionWeb, UriKind.Absolute, out revisionWebUrl))
            {
                error = string.Format(CultureInfo.InvariantCulture, "could not compose a revision web URL from repositoryUri '{0}'.", classification.Display);
                return false;
            }

            return true;
        }

        private static bool TryClassify(Uri rawRepositoryUri, out Classification classification, out string error)
        {
            classification = null;
            error = null;

            if (rawRepositoryUri == null)
            {
                error = "repositoryUri is required so a portable root can be derived.";
                return false;
            }

            if (!TryNormalizeToHttps(rawRepositoryUri, out Uri repositoryUri, out error))
            {
                return false;
            }

            // A shipped repositoryUri must be a clean repository identity. Embedded userinfo (a bare
            // account, an account:password, or a token) is rejected outright: a credential must never
            // ride in a repositoryUri, and even a benign account segment is not part of the repository
            // identity. A query/fragment means a browser URL rather than a repository root, and a
            // non-default port means a malformed authority; both fail closed. Error text excludes
            // userinfo so a credential can never surface in a diagnostic.
            string display = SanitizeForDisplay(repositoryUri);

            if (!repositoryUri.IsDefaultPort)
            {
                error = string.Format(CultureInfo.InvariantCulture, "repositoryUri '{0}' must use the host's default port.", display);
                return false;
            }

            if (!string.IsNullOrEmpty(repositoryUri.Query) || !string.IsNullOrEmpty(repositoryUri.Fragment))
            {
                error = string.Format(CultureInfo.InvariantCulture, "repositoryUri '{0}' must be a bare repository URL with no query or fragment.", display);
                return false;
            }

            if (!string.IsNullOrEmpty(repositoryUri.UserInfo))
            {
                error = "a repositoryUri must not carry embedded credentials (no account@ or account:password@); supply the clean https repository URL.";
                return false;
            }

            string[] segments = repositoryUri.AbsolutePath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            // Host-first dispatch. The GitHub allow-list is checked before the Azure DevOps host so a
            // GitHub path that happens to contain a '_git' segment is classified as GitHub (and then
            // rejected for shape) rather than misread as Azure DevOps.
            if (IsGitHubHost(repositoryUri.Host))
            {
                return TryClassifyGitHubCompatible(repositoryUri, segments, display, out classification, out error);
            }

            if (string.Equals(repositoryUri.Host, AzureDevOpsHost, StringComparison.OrdinalIgnoreCase))
            {
                return TryClassifyAzureDevOps(repositoryUri, segments, display, out classification, out error);
            }

            if (IsLegacyAzureDevOpsHost(repositoryUri.Host))
            {
                error = string.Format(CultureInfo.InvariantCulture, "the legacy Azure DevOps host form is not supported; supply the repository as https://dev.azure.com/<org>/<project>/_git/<repo>. '{0}' did not.", display);
                return false;
            }

            error = string.Format(CultureInfo.InvariantCulture, "portable root derivation supports GitHub (github.com or <slug>.ghe.com) and Azure DevOps (dev.azure.com); '{0}' is not a supported host.", display);
            return false;
        }

        private static bool TryClassifyAzureDevOps(Uri repositoryUri, string[] segments, string display, out Classification classification, out string error)
        {
            classification = null;
            error = null;

            // dev.azure.com repositories are exactly https://dev.azure.com/<org>/<project>/_git/<repo>:
            // four path segments with _git in the third position. Anything else (missing project,
            // trailing path after the repo, _git elsewhere) is rejected rather than guessed at.
            if (segments.Length != 4 || !string.Equals(segments[2], "_git", StringComparison.OrdinalIgnoreCase))
            {
                error = string.Format(CultureInfo.InvariantCulture, "azure devops repositoryUri must take the form https://dev.azure.com/<org>/<project>/_git/<repo>; '{0}' did not.", display);
                return false;
            }

            // Validate org and project as single decoded path segments so an encoded separator (e.g.
            // a project named "%2F") cannot compose into an ambiguous REST path downstream.
            if (!IsSingleSafeSegment(segments[0]) || !IsSingleSafeSegment(segments[1]))
            {
                error = string.Format(CultureInfo.InvariantCulture, "azure devops repositoryUri '{0}': the organization and project must each be a single valid path segment.", display);
                return false;
            }

            if (!TryNormalizeRepoSegment(segments[3], out string repoForUrl, out string leaf, out string leafError))
            {
                error = string.Format(CultureInfo.InvariantCulture, "azure devops repositoryUri '{0}': {1}", display, leafError);
                return false;
            }

            // Reproduce org/project in their original percent-encoded form so names containing spaces
            // (%20) round-trip unchanged.
            string adoPrefix = string.Join("/", segments, 0, 2);

            classification = new Classification
            {
                IsAzureDevOps = true,
                SchemeAndServer = repositoryUri.GetComponents(UriComponents.SchemeAndServer, UriFormat.UriEscaped),
                AdoPrefix = adoPrefix,
                RepoForUrl = repoForUrl,
                Leaf = leaf,
                Display = display,
            };

            return true;
        }

        private static bool TryClassifyGitHubCompatible(Uri repositoryUri, string[] segments, string display, out Classification classification, out string error)
        {
            classification = null;
            error = null;

            if (segments.Length != 2)
            {
                error = string.Format(CultureInfo.InvariantCulture, "github repositoryUri must take the form https://<host>/<owner>/<repo>; '{0}' did not.", display);
                return false;
            }

            if (!TryNormalizeRepoSegment(segments[1], out string repoForUrl, out string leaf, out string leafError))
            {
                error = string.Format(CultureInfo.InvariantCulture, "github repositoryUri '{0}': {1}", display, leafError);
                return false;
            }

            classification = new Classification
            {
                IsGitHub = true,
                IsAzureDevOps = false,
                SchemeAndServer = repositoryUri.GetComponents(UriComponents.SchemeAndServer, UriFormat.UriEscaped),
                Owner = segments[0],
                RepoForUrl = repoForUrl,
                Leaf = leaf,
                Display = display,
            };

            return true;
        }

        // The supported GitHub host set: github.com (public OSS and EMU on dotcom) and the
        // <slug>.ghe.com data-residency / Enterprise Managed Users hosts, which are GitHub cloud and
        // share github.com's /blob/<sha>/ permalink shape. Custom-hostname GitHub Enterprise Server
        // deployments are intentionally out of scope and are rejected rather than guessed at.
        private static bool IsGitHubHost(string host)
            => string.Equals(host, "github.com", StringComparison.OrdinalIgnoreCase)
            || host.EndsWith(".ghe.com", StringComparison.OrdinalIgnoreCase);

        // The legacy Azure DevOps host form (<org>.visualstudio.com) is recognized only so it can be
        // rejected with a targeted message: it predates dev.azure.com and is not accepted.
        private static bool IsLegacyAzureDevOpsHost(string host)
            => string.Equals(host, "visualstudio.com", StringComparison.OrdinalIgnoreCase)
            || host.EndsWith(".visualstudio.com", StringComparison.OrdinalIgnoreCase);

        // Accepts an https repositoryUri as-is, and normalizes a GitHub-compatible SSH or scp-style
        // clone URL (git@host:owner/repo.git or ssh://git@host/owner/repo.git) to https. http is not
        // accepted. Azure DevOps SSH endpoints are rejected with guidance because their path is
        // restructured relative to the https clone URL.
        private static bool TryNormalizeToHttps(Uri rawRepositoryUri, out Uri httpsRepositoryUri, out string error)
        {
            httpsRepositoryUri = null;
            error = null;

            if (rawRepositoryUri.IsAbsoluteUri && rawRepositoryUri.Scheme == Uri.UriSchemeHttps)
            {
                httpsRepositoryUri = rawRepositoryUri;
                return true;
            }

            string host;
            string userInfo;
            string path;

            if (rawRepositoryUri.IsAbsoluteUri
                && string.Equals(rawRepositoryUri.Scheme, "ssh", StringComparison.OrdinalIgnoreCase))
            {
                // An ssh clone URL carries only host + path identity. A non-default port, query, or
                // fragment cannot be represented in the normalized https form, so reject rather than
                // silently drop it (which would mint a root for a different endpoint). Messages do not
                // echo the raw URL because it may carry an ssh login.
                if (!rawRepositoryUri.IsDefaultPort)
                {
                    error = "an ssh repositoryUri must use the default port; supply the https clone URL instead.";
                    return false;
                }

                if (!string.IsNullOrEmpty(rawRepositoryUri.Query) || !string.IsNullOrEmpty(rawRepositoryUri.Fragment))
                {
                    error = "an ssh repositoryUri must be a bare clone URL with no query or fragment; supply the https clone URL instead.";
                    return false;
                }

                host = rawRepositoryUri.Host;
                userInfo = rawRepositoryUri.UserInfo;
                path = rawRepositoryUri.AbsolutePath;
            }
            else if (!TryParseScp(rawRepositoryUri.OriginalString, out host, out userInfo, out path))
            {
                error = "repositoryUri must be an absolute https URL, or a github-compatible ssh/scp clone URL (git@host:owner/repo).";
                return false;
            }

            if (userInfo.IndexOf(':') >= 0)
            {
                error = "a repositoryUri carrying credentials (user:password@) cannot be used to derive a portable root.";
                return false;
            }

            string normalizedPath = path.Replace('\\', '/').TrimStart('/');

            // Reject Azure DevOps ssh up front: the known endpoint by host, and any ssh URL whose path
            // begins with the v3 routing segment, since neither can be reshaped by a scheme swap.
            if (string.Equals(host, AzureDevOpsSshHost, StringComparison.OrdinalIgnoreCase)
                || normalizedPath.StartsWith("v3/", StringComparison.OrdinalIgnoreCase))
            {
                error = "azure devops ssh repositoryUri normalization is not supported; supply the https clone URL (https://dev.azure.com/<org>/<project>/_git/<repo>).";
                return false;
            }

            // The ssh login (git@) is not part of the repository identity, so it is dropped here: the
            // composed URL carries only host + path.
            string composed = string.Format(CultureInfo.InvariantCulture, "https://{0}/{1}", host, normalizedPath);
            if (!Uri.TryCreate(composed, UriKind.Absolute, out httpsRepositoryUri))
            {
                error = "repositoryUri must be an absolute https URL, or a github-compatible ssh/scp clone URL (git@host:owner/repo).";
                return false;
            }

            return true;
        }

        // scp syntax is [user@]host:path with no scheme and no slash before the colon (which would
        // make the colon a port separator on an authority instead). The authority is validated tightly
        // so a crafted login (e.g. git@evil@host:owner/repo) cannot smuggle a different host past the
        // single-'@' split.
        private static bool TryParseScp(string raw, out string host, out string userInfo, out string path)
        {
            host = null;
            userInfo = string.Empty;
            path = null;

            if (string.IsNullOrEmpty(raw) || raw.IndexOf("://", StringComparison.Ordinal) >= 0)
            {
                return false;
            }

            int colon = raw.IndexOf(':');
            if (colon < 0) { return false; }

            string authority = raw.Substring(0, colon);
            if (authority.Length == 0 || authority.IndexOf('/') >= 0) { return false; }

            int at = authority.IndexOf('@');
            if (at >= 0)
            {
                // A second '@' is ambiguous and could hide the real host; reject it.
                if (authority.IndexOf('@', at + 1) >= 0) { return false; }

                userInfo = authority.Substring(0, at);
                host = authority.Substring(at + 1);

                if (userInfo.Length == 0) { return false; }
            }
            else
            {
                host = authority;
            }

            // The host must be a bare hostname: no embedded authority delimiters or whitespace.
            if (host.Length == 0 || host.IndexOfAny(s_invalidScpHostChars) >= 0) { return false; }

            path = raw.Substring(colon + 1);

            // A '@' in the path means the chosen colon was not the host:path separator (it sat inside a
            // user:password authority, e.g. user:secret@host:owner/repo). Reject so a credential never
            // composes into the https URL or surfaces in a diagnostic.
            if (path.IndexOf('@') >= 0) { return false; }

            return path.Length != 0;
        }

        private static readonly char[] s_invalidScpHostChars =
            new[] { '@', '/', '\\', ':', '?', '#', ' ', '\t' };

        private static bool IsSingleSafeSegment(string rawSegment)
        {
            if (string.IsNullOrEmpty(rawSegment)) { return false; }

            string decoded = Uri.UnescapeDataString(rawSegment);
            return decoded.Length != 0
                && decoded != "."
                && decoded != ".."
                && decoded.IndexOf('/') < 0
                && decoded.IndexOf('\\') < 0;
        }

        private static bool TryNormalizeRepoSegment(string rawSegment, out string repoForUrl, out string leaf, out string error)
        {
            repoForUrl = null;
            leaf = null;
            error = null;

            string repo = rawSegment;
            if (repo.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
            {
                repo = repo.Substring(0, repo.Length - 4);
            }

            string decoded = Uri.UnescapeDataString(repo);
            if (decoded.Length == 0
                || decoded == "."
                || decoded == ".."
                || decoded.IndexOf('/') >= 0
                || decoded.IndexOf('\\') >= 0)
            {
                error = "the repository name is empty or is not a single valid path segment.";
                return false;
            }

            repoForUrl = repo;
            leaf = decoded;
            return true;
        }

        private static string SanitizeForDisplay(Uri uri)
        {
            // Never echo credentials (or browser query/fragment) in diagnostics: keep only
            // scheme, host, optional port, and path.
            return uri.GetComponents(UriComponents.SchemeAndServer | UriComponents.Path, UriFormat.UriEscaped);
        }

        private sealed class Classification
        {
            public bool IsGitHub { get; set; }

            public bool IsAzureDevOps { get; set; }

            public string SchemeAndServer { get; set; }

            public string AdoPrefix { get; set; }

            public string Owner { get; set; }

            public string RepoForUrl { get; set; }

            public string Leaf { get; set; }

            public string Display { get; set; }
        }
    }
}
