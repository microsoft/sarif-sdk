// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    /// <summary>
    /// Detects a GitHub Actions execution context from environment variables and surfaces the
    /// <c>versionControlProvenance</c> fields the workflow runner publishes
    /// (<c>GITHUB_SERVER_URL</c>/<c>GITHUB_REPOSITORY</c> compose the repository URI;
    /// <c>GITHUB_SHA</c> supplies the revision; <c>GITHUB_REF</c> supplies the branch
    /// ref).
    /// </summary>
    /// <remarks>
    /// <para>This context is VCP-scoped: it does not stamp <c>automationDetails</c> for GitHub
    /// Actions. The runner exposes <c>GITHUB_RUN_ID</c> / <c>GITHUB_WORKFLOW</c> / etc., but
    /// downstream ingestion conventions for the GitHub-side automationDetails shape are out of
    /// scope for this verb today.</para>
    /// <para>Detection is gated on the standard runner sentinel <c>GITHUB_ACTIONS=true</c>. When
    /// not inside a GitHub Actions workflow, <see cref="DetectionState.None"/> is returned and no
    /// stamping occurs. Inside a workflow three states are possible:</para>
    /// <list type="bullet">
    /// <item><see cref="DetectionState.Complete"/> — the runner is active and every populated
    /// VCP variable parses cleanly. Absent VCP variables are tolerated: in that case the context
    /// is Complete but <see cref="HasVcpFields"/> returns <c>false</c> and the verb's VCP
    /// stamping is a no-op for this source.</item>
    /// <item><see cref="DetectionState.Partial"/> — one or more present VCP variables are
    /// malformed (e.g. a non-hex <c>GITHUB_SHA</c>, an unparseable
    /// <c>GITHUB_SERVER_URL</c>); the verb should fail loudly rather than stamp a half-derived
    /// VCP entry.</item>
    /// <item><see cref="DetectionState.None"/> — <c>GITHUB_ACTIONS</c> is unset or not
    /// truthy.</item>
    /// </list>
    /// </remarks>
    public sealed class GitHubActionsContext
    {
        internal const string GitHubActionsEnvVar = "GITHUB_ACTIONS";
        internal const string ServerUrlEnvVar = "GITHUB_SERVER_URL";
        internal const string RepositoryEnvVar = "GITHUB_REPOSITORY";
        internal const string ShaEnvVar = "GITHUB_SHA";
        internal const string RefEnvVar = "GITHUB_REF";

        // Same regex contract as AdoPipelineContext: 7-40 hex window. The runner always sets
        // a full 40-char SHA, but the window admits hand-set abbreviated SHAs for tests and
        // local invocations.
        private static readonly Regex s_revisionIdRegex =
            new Regex(@"^[0-9a-fA-F]{7,40}$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);

        public enum DetectionState
        {
            None,
            Partial,
            Complete,
        }

        /// <summary>
        /// Absolute URL of the source repository, composed from <c>GITHUB_SERVER_URL</c> and
        /// <c>GITHUB_REPOSITORY</c> when both are present and well-formed; otherwise null.
        /// </summary>
        public Uri RepositoryUri { get; private set; }

        /// <summary>
        /// The commit identifier (typically a 40-character SHA-1) the workflow run is building.
        /// Lifted from <c>GITHUB_SHA</c> when present and well-formed; otherwise null.
        /// </summary>
        public string RevisionId { get; private set; }

        /// <summary>
        /// The branch ref (e.g. <c>refs/heads/main</c>, <c>refs/pull/42/merge</c>) that
        /// triggered the workflow. Lifted from <c>GITHUB_REF</c> when present; null when
        /// absent. Pass-through with no normalization — the value is whatever the runner
        /// (or hand-built env) published.
        /// </summary>
        public string BranchRef { get; private set; }

        /// <summary>
        /// Reads GitHub Actions predefined environment variables via
        /// <paramref name="environment"/> and returns one of <see cref="DetectionState"/>.
        /// </summary>
        /// <param name="environment">Env getter (test seam).</param>
        /// <param name="context">Populated context when state is <see cref="DetectionState.Complete"/>; otherwise <c>null</c>.</param>
        /// <param name="errorMessage">Human-readable description of present/malformed variables when state is <see cref="DetectionState.Partial"/>; otherwise <c>null</c>.</param>
        public static DetectionState TryDetect(
            IEnvironmentVariableGetter environment,
            out GitHubActionsContext context,
            out string errorMessage)
        {
            if (environment == null) { throw new ArgumentNullException(nameof(environment)); }

            context = null;
            errorMessage = null;

            string gate = environment.GetEnvironmentVariable(GitHubActionsEnvVar);
            if (!IsTrueLike(gate))
            {
                return DetectionState.None;
            }

            string serverUrl = environment.GetEnvironmentVariable(ServerUrlEnvVar);
            string repository = environment.GetEnvironmentVariable(RepositoryEnvVar);
            string sha = environment.GetEnvironmentVariable(ShaEnvVar);
            string refValue = environment.GetEnvironmentVariable(RefEnvVar);

            var present = new List<string>();
            var problems = new List<string>();

            Uri repositoryUriValue = TryComposeRepositoryUri(serverUrl, repository, present, problems);
            string revisionIdValue = TryReadOptionalRevisionId(sha, ShaEnvVar, present, problems);
            string branchRefValue = TryReadOptionalBranchRef(refValue, present);

            if (problems.Count > 0)
            {
                errorMessage = FormatPartialError(present, problems);
                return DetectionState.Partial;
            }

            context = new GitHubActionsContext
            {
                RepositoryUri = repositoryUriValue,
                RevisionId = revisionIdValue,
                BranchRef = branchRefValue,
            };
            return DetectionState.Complete;
        }

        /// <summary>
        /// True when this context carries at least one <c>versionControlProvenance</c> field
        /// (repository URI, revision id, or branch ref) lifted from the workflow
        /// environment. False indicates the VCP enrichment path is a no-op for this context.
        /// </summary>
        internal bool HasVcpFields => RepositoryUri != null
            || !string.IsNullOrEmpty(RevisionId)
            || !string.IsNullOrEmpty(BranchRef);

        /// <summary>
        /// Returns the non-null <c>versionControlProvenance</c> field name/value pairs for this
        /// workflow context. Pairs are ordered <c>repositoryUri</c>, <c>revisionId</c>,
        /// <c>branch</c>; absent fields are omitted.
        /// </summary>
        internal IReadOnlyList<KeyValuePair<string, string>> GetVcpFieldValues()
        {
            var pairs = new List<KeyValuePair<string, string>>(3);
            if (RepositoryUri != null)
            {
                pairs.Add(new KeyValuePair<string, string>(VcpFieldNames.RepositoryUri, RepositoryUri.AbsoluteUri));
            }
            if (!string.IsNullOrEmpty(RevisionId))
            {
                pairs.Add(new KeyValuePair<string, string>(VcpFieldNames.RevisionId, RevisionId));
            }
            if (!string.IsNullOrEmpty(BranchRef))
            {
                pairs.Add(new KeyValuePair<string, string>(VcpFieldNames.Branch, BranchRef));
            }
            return pairs;
        }

        // Both server and repository must be present to derive a URI. Either absent silently
        // yields null (no signal); either present-but-malformed adds to problems.
        private static Uri TryComposeRepositoryUri(string rawServer, string rawRepository, List<string> present, List<string> problems)
        {
            bool serverPresent = !string.IsNullOrWhiteSpace(rawServer);
            bool repoPresent = !string.IsNullOrWhiteSpace(rawRepository);
            if (!serverPresent && !repoPresent) { return null; }

            if (serverPresent) { present.Add(ServerUrlEnvVar); }
            if (repoPresent) { present.Add(RepositoryEnvVar); }

            if (!serverPresent || !repoPresent)
            {
                problems.Add(string.Format(
                    CultureInfo.InvariantCulture,
                    "{0} and {1} must both be set to derive a repository URI; got {0}='{2}', {1}='{3}'",
                    ServerUrlEnvVar,
                    RepositoryEnvVar,
                    rawServer ?? string.Empty,
                    rawRepository ?? string.Empty));
                return null;
            }

            string server = rawServer.Trim().TrimEnd('/');
            string repository = rawRepository.Trim().TrimStart('/');

            string composed = server + "/" + repository;
            if (!Uri.TryCreate(composed, UriKind.Absolute, out Uri parsed)
                || (parsed.Scheme != Uri.UriSchemeHttp && parsed.Scheme != Uri.UriSchemeHttps))
            {
                problems.Add(string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}='{1}' and {2}='{3}' do not compose a valid absolute http(s) URI",
                    ServerUrlEnvVar,
                    rawServer,
                    RepositoryEnvVar,
                    rawRepository));
                return null;
            }

            return parsed;
        }

        private static string TryReadOptionalRevisionId(string raw, string envName, List<string> present, List<string> problems)
        {
            if (string.IsNullOrWhiteSpace(raw)) { return null; }

            present.Add(envName);

            string trimmed = raw.Trim();
            if (!s_revisionIdRegex.IsMatch(trimmed))
            {
                problems.Add(string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}='{1}' is not a valid revision id (expected 7-40 hex chars)",
                    envName,
                    raw));
                return null;
            }

            return trimmed;
        }

        // GITHUB_REF is the full ref (e.g. 'refs/heads/main', 'refs/pull/42/merge',
        // 'refs/tags/v1'). The workflow runner always sets it. We pass it through
        // as-is — no stripping, no normalization. GHAzDO ingestion accepts both short
        // and long forms; AdoPipelineContext also passes BUILD_SOURCEBRANCH through
        // as-is, so the two sources produce comparable shapes when both env sets are
        // populated.
        private static string TryReadOptionalBranchRef(string rawRef, List<string> present)
        {
            if (string.IsNullOrWhiteSpace(rawRef)) { return null; }

            present.Add(RefEnvVar);
            return rawRef.Trim();
        }

        private static bool IsTrueLike(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) { return false; }
            return string.Equals(raw.Trim(), "True", StringComparison.OrdinalIgnoreCase)
                || string.Equals(raw.Trim(), "1", StringComparison.Ordinal);
        }

        private static string FormatPartialError(List<string> present, List<string> problems)
        {
            var sb = new StringBuilder();
            sb.AppendLine("GitHub Actions context is partially configured. Either populate every required variable or clear them all.");
            if (present.Count > 0)
            {
                sb.AppendLine("Present:");
                foreach (string p in present)
                {
                    sb.Append("  ").AppendLine(p);
                }
            }
            sb.AppendLine("Problems:");
            foreach (string p in problems)
            {
                sb.Append("  ").AppendLine(p);
            }
            return sb.ToString().TrimEnd('\r', '\n');
        }
    }
}
