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
    /// Detects an Azure DevOps pipeline execution context from environment variables and stamps
    /// the corresponding <c>automationDetails</c> shape onto a <see cref="Run"/>, matching the
    /// canonical write surface used by the Azure DevOps Advanced Security SARIF upload SDK
    /// (<c>AlertHttpClientExtensions.AddAutomationDetails</c>).
    /// </summary>
    /// <remarks>
    /// <para>Detection is gated on the standard ADO sentinel <c>TF_BUILD=True</c>. When not
    /// running inside an ADO pipeline, <see cref="DetectionState.None"/> is returned and no
    /// stamping occurs. This avoids surprising failures on non-ADO CI systems that happen to
    /// populate a subset of <c>BUILD_*</c> variables.</para>
    /// <para>Inside an ADO pipeline three states are possible:</para>
    /// <list type="bullet">
    /// <item><see cref="DetectionState.Complete"/> — every required logical variable is present
    /// and well-formed; <see cref="ApplyTo(Run)"/> writes <c>automationDetails.id</c> plus the
    /// four <c>azuredevops/pipeline/build/*</c> property keys that ADO ingestion validates.</item>
    /// <item><see cref="DetectionState.None"/> — no required variables are populated; nothing is
    /// stamped (e.g. a manual local invocation that happens to have <c>TF_BUILD</c> set without
    /// the rest).</item>
    /// <item><see cref="DetectionState.Partial"/> — one or more required variables are present
    /// but others are missing or malformed; a partial pipeline identity is a misconfiguration
    /// signal, not a soft skip, so callers should fail loudly rather than emit half-stamped
    /// SARIF that will fail GHAzDO1019/1020 downstream.</item>
    /// </list>
    /// </remarks>
    public sealed class AdoPipelineContext
    {
        internal const string TfBuildEnvVar = "TF_BUILD";
        internal const string CollectionUriEnvVar = "SYSTEM_COLLECTIONURI";
        internal const string TeamProjectIdEnvVar = "SYSTEM_TEAMPROJECTID";
        internal const string BuildDefinitionIdPrimaryEnvVar = "BUILD_DEFINITIONID";
        internal const string BuildDefinitionIdFallbackEnvVar = "SYSTEM_DEFINITIONID";
        internal const string BuildDefinitionNameEnvVar = "BUILD_DEFINITIONNAME";
        internal const string BuildIdEnvVar = "BUILD_BUILDID";
        internal const string PhaseIdPrimaryEnvVar = "SYSTEM_PHASEID";
        internal const string PhaseIdFallbackEnvVar = "SYSTEM_JOBID";
        internal const string PhaseNamePrimaryEnvVar = "SYSTEM_PHASENAME";
        internal const string PhaseNameFallbackEnvVar = "SYSTEM_JOBNAME";
        internal const string SourceBranchEnvVar = "BUILD_SOURCEBRANCH";
        internal const string RepositoryUriEnvVar = "BUILD_REPOSITORY_URI";
        internal const string SourceVersionEnvVar = "BUILD_SOURCEVERSION";

        internal const string PropBuildDefinitionId = "azuredevops/pipeline/build/buildDefinitionId";
        internal const string PropBuildDefinitionName = "azuredevops/pipeline/build/buildDefinitionName";
        internal const string PropPhaseId = "azuredevops/pipeline/build/phaseId";
        internal const string PropPhaseName = "azuredevops/pipeline/build/phaseName";
        internal const string AutomationIdPrefix = "azuredevops/pipeline/build/";

        // BUILD_SOURCEVERSION is always a full 40-char SHA-1 in ADO, but we accept any
        // 7-40 hex window so callers can hand-set the var to an abbreviated SHA in tests
        // or local invocations without a special carve-out.
        private static readonly Regex s_revisionIdRegex =
            new Regex(@"^[0-9a-fA-F]{7,40}$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);

        public enum DetectionState
        {
            None,
            Partial,
            Complete,
        }

        public string OrganizationName { get; private set; }
        public Guid ProjectId { get; private set; }
        public int BuildDefinitionId { get; private set; }
        public string BuildDefinitionName { get; private set; }
        public int BuildId { get; private set; }
        public Guid PhaseId { get; private set; }
        public string PhaseName { get; private set; }
        public string BranchRef { get; private set; }

        /// <summary>
        /// Absolute URL identifier of the source repository. Lifted from
        /// <c>BUILD_REPOSITORY_URI</c> when present and well-formed; otherwise null.
        /// </summary>
        public Uri RepositoryUri { get; private set; }

        /// <summary>
        /// The commit identifier (typically a 40-character SHA-1) the pipeline is building.
        /// Lifted from <c>BUILD_SOURCEVERSION</c> when present and well-formed; otherwise null.
        /// </summary>
        public string RevisionId { get; private set; }

        /// <summary>
        /// Reads ADO predefined environment variables via <paramref name="environment"/> and
        /// returns one of <see cref="DetectionState"/>.
        /// </summary>
        /// <param name="environment">Env getter (test seam).</param>
        /// <param name="context">Populated context when state is <see cref="DetectionState.Complete"/>; otherwise <c>null</c>.</param>
        /// <param name="errorMessage">Human-readable description of present/missing/malformed variables when state is <see cref="DetectionState.Partial"/>; otherwise <c>null</c>.</param>
        public static DetectionState TryDetect(
            IEnvironmentVariableGetter environment,
            out AdoPipelineContext context,
            out string errorMessage)
        {
            if (environment == null) { throw new ArgumentNullException(nameof(environment)); }

            context = null;
            errorMessage = null;

            string tfBuild = environment.GetEnvironmentVariable(TfBuildEnvVar);
            if (!IsTrueLike(tfBuild))
            {
                return DetectionState.None;
            }

            string collectionUri = environment.GetEnvironmentVariable(CollectionUriEnvVar);
            string teamProjectId = environment.GetEnvironmentVariable(TeamProjectIdEnvVar);
            string buildDefIdPrimary = environment.GetEnvironmentVariable(BuildDefinitionIdPrimaryEnvVar);
            string buildDefIdFallback = environment.GetEnvironmentVariable(BuildDefinitionIdFallbackEnvVar);
            string buildDefName = environment.GetEnvironmentVariable(BuildDefinitionNameEnvVar);
            string buildId = environment.GetEnvironmentVariable(BuildIdEnvVar);
            string phaseIdPrimary = environment.GetEnvironmentVariable(PhaseIdPrimaryEnvVar);
            string phaseIdFallback = environment.GetEnvironmentVariable(PhaseIdFallbackEnvVar);
            string phaseNamePrimary = environment.GetEnvironmentVariable(PhaseNamePrimaryEnvVar);
            string phaseNameFallback = environment.GetEnvironmentVariable(PhaseNameFallbackEnvVar);
            string sourceBranch = environment.GetEnvironmentVariable(SourceBranchEnvVar);
            string repositoryUri = environment.GetEnvironmentVariable(RepositoryUriEnvVar);
            string sourceVersion = environment.GetEnvironmentVariable(SourceVersionEnvVar);

            string buildDefIdRaw = FirstNonBlank(buildDefIdPrimary, buildDefIdFallback);
            string phaseIdRaw = FirstNonBlank(phaseIdPrimary, phaseIdFallback);
            string phaseNameRaw = FirstNonBlank(phaseNamePrimary, phaseNameFallback);

            // If every logical var is unset, treat as None (TF_BUILD set without other vars is
            // not a typical ADO agent state, but we don't fail it — the test-clear pattern is
            // simpler if "all unset" is silent).
            if (string.IsNullOrWhiteSpace(collectionUri)
                && string.IsNullOrWhiteSpace(teamProjectId)
                && string.IsNullOrWhiteSpace(buildDefIdRaw)
                && string.IsNullOrWhiteSpace(buildDefName)
                && string.IsNullOrWhiteSpace(buildId)
                && string.IsNullOrWhiteSpace(phaseIdRaw)
                && string.IsNullOrWhiteSpace(phaseNameRaw)
                && string.IsNullOrWhiteSpace(sourceBranch))
            {
                return DetectionState.None;
            }

            var present = new List<string>();
            var problems = new List<string>();

            string organizationName = null;
            if (TryParseOrganizationName(collectionUri, CollectionUriEnvVar, present, problems, out string parsedOrg))
            {
                organizationName = parsedOrg;
            }

            Guid projectIdValue = default;
            if (TryParseRequiredGuid(teamProjectId, TeamProjectIdEnvVar, present, problems, out Guid parsedProjectId))
            {
                projectIdValue = parsedProjectId;
            }

            int buildDefinitionIdValue = 0;
            if (TryParseRequiredPositiveInt(buildDefIdPrimary, buildDefIdFallback, BuildDefinitionIdPrimaryEnvVar, BuildDefinitionIdFallbackEnvVar, present, problems, out int parsedBuildDefId))
            {
                buildDefinitionIdValue = parsedBuildDefId;
            }

            string buildDefinitionNameValue = null;
            if (TryReadRequiredString(buildDefName, BuildDefinitionNameEnvVar, present, problems, out string parsedBuildDefName))
            {
                buildDefinitionNameValue = parsedBuildDefName;
            }

            int buildIdValue = 0;
            if (TryParseRequiredPositiveInt(buildId, null, BuildIdEnvVar, null, present, problems, out int parsedBuildId))
            {
                buildIdValue = parsedBuildId;
            }

            Guid phaseIdValue = default;
            if (TryParseRequiredGuid(phaseIdRaw, !string.IsNullOrWhiteSpace(phaseIdPrimary) ? PhaseIdPrimaryEnvVar : PhaseIdFallbackEnvVar, present, problems, out Guid parsedPhaseId))
            {
                phaseIdValue = parsedPhaseId;
            }

            string phaseNameValue = null;
            if (TryReadRequiredString(phaseNameRaw, !string.IsNullOrWhiteSpace(phaseNamePrimary) ? PhaseNamePrimaryEnvVar : PhaseNameFallbackEnvVar, present, problems, out string parsedPhaseName))
            {
                phaseNameValue = parsedPhaseName;
            }

            string branchRefValue = null;
            if (TryReadRequiredString(sourceBranch, SourceBranchEnvVar, present, problems, out string parsedBranch))
            {
                branchRefValue = parsedBranch;
            }

            // BUILD_REPOSITORY_URI and BUILD_SOURCEVERSION are optional arguments used to
            // populate versionControlProvenance. They do NOT contribute to the partial-state
            // gate when absent — automationDetails stamping is independent of VCP enrichment.
            // When present but malformed, they DO add to problems so a misconfigured pipeline
            // doesn't ship a half-enriched run.
            Uri repositoryUriValue = null;
            if (TryReadOptionalAbsoluteHttpUri(repositoryUri, RepositoryUriEnvVar, present, problems, out Uri parsedRepoUri))
            {
                repositoryUriValue = parsedRepoUri;
            }

            string revisionIdValue = null;
            if (TryReadOptionalRevisionId(sourceVersion, SourceVersionEnvVar, present, problems, out string parsedRevision))
            {
                revisionIdValue = parsedRevision;
            }

            if (problems.Count > 0)
            {
                errorMessage = FormatPartialError(present, problems);
                return DetectionState.Partial;
            }

            context = new AdoPipelineContext
            {
                OrganizationName = organizationName,
                ProjectId = projectIdValue,
                BuildDefinitionId = buildDefinitionIdValue,
                BuildDefinitionName = buildDefinitionNameValue,
                BuildId = buildIdValue,
                PhaseId = phaseIdValue,
                PhaseName = phaseNameValue,
                BranchRef = branchRefValue,
                RepositoryUri = repositoryUriValue,
                RevisionId = revisionIdValue,
            };
            return DetectionState.Complete;
        }

        /// <summary>
        /// Stamps the detected pipeline identity onto <paramref name="run"/>, returning
        /// <c>true</c> when no conflict was detected. When the run already carries a
        /// non-conflicting <c>automationDetails.id</c> or any of the four
        /// <c>azuredevops/pipeline/build/*</c> property values, the existing values are
        /// preserved. When the run carries a conflicting value, this method returns
        /// <c>false</c> with a diagnostic on <paramref name="conflictError"/> and leaves
        /// the run unchanged.
        /// </summary>
        /// <remarks>
        /// <para>The "stamp only when absent, fail on conflict" contract is required because
        /// callers (notably <c>emit-init-run</c>'s JSON-payload contract) may supply these
        /// fields directly. An unconditional overwrite would silently clobber a producer's
        /// declared identity; a conflict is a misconfiguration signal that we want to surface
        /// at the verb rather than ship in the run.</para>
        /// <para>Producer-supplied <see cref="RunAutomationDetails.Guid"/> and
        /// <see cref="RunAutomationDetails.CorrelationGuid"/> fields are never touched —
        /// they name a different scope (run / run-equivalence-class identity) than the
        /// pipeline identity stamped here.</para>
        /// </remarks>
        public bool TryApplyTo(Run run, out string conflictError)
        {
            if (run == null) { throw new ArgumentNullException(nameof(run)); }

            conflictError = null;

            string canonicalId = BuildCanonicalAutomationId();
            string buildDefinitionIdValue = BuildDefinitionId.ToString(CultureInfo.InvariantCulture);
            string phaseIdValue = PhaseId.ToString("D", CultureInfo.InvariantCulture);

            // Probe-before-write so a conflict on any field leaves the run unchanged.
            RunAutomationDetails existing = run.AutomationDetails;
            if (existing != null)
            {
                if (!IsAbsentOrEqual(existing.Id, canonicalId, "automationDetails.id", out string idError))
                {
                    conflictError = idError;
                    return false;
                }

                if (!IsPropertyAbsentOrEqual(existing, PropBuildDefinitionId, buildDefinitionIdValue, out string p1Error))
                {
                    conflictError = p1Error;
                    return false;
                }

                if (!IsPropertyAbsentOrEqual(existing, PropBuildDefinitionName, BuildDefinitionName, out string p2Error))
                {
                    conflictError = p2Error;
                    return false;
                }

                if (!IsPropertyAbsentOrEqual(existing, PropPhaseId, phaseIdValue, out string p3Error))
                {
                    conflictError = p3Error;
                    return false;
                }

                if (!IsPropertyAbsentOrEqual(existing, PropPhaseName, PhaseName, out string p4Error))
                {
                    conflictError = p4Error;
                    return false;
                }
            }

            run.AutomationDetails ??= new RunAutomationDetails();
            if (string.IsNullOrEmpty(run.AutomationDetails.Id))
            {
                run.AutomationDetails.Id = canonicalId;
            }

            StampPropertyIfAbsent(run.AutomationDetails, PropBuildDefinitionId, buildDefinitionIdValue);
            StampPropertyIfAbsent(run.AutomationDetails, PropBuildDefinitionName, BuildDefinitionName);
            StampPropertyIfAbsent(run.AutomationDetails, PropPhaseId, phaseIdValue);
            StampPropertyIfAbsent(run.AutomationDetails, PropPhaseName, PhaseName);

            return true;
        }

        /// <summary>
        /// Computes the canonical <c>automationDetails.id</c>
        /// (<c>azuredevops/pipeline/build/&lt;org&gt;/&lt;projectId&gt;/&lt;buildDefId&gt;/&lt;phaseId&gt;/&lt;branch&gt;/&lt;buildId&gt;</c>)
        /// for this pipeline context. Exposed so JSON-direct callers can stamp the id without
        /// constructing a typed <see cref="Run"/>.
        /// </summary>
        internal string BuildCanonicalAutomationId()
            => string.Format(
                CultureInfo.InvariantCulture,
                "{0}{1}/{2}/{3}/{4}/{5}/{6}",
                AutomationIdPrefix,
                OrganizationName,
                ProjectId,
                BuildDefinitionId,
                PhaseId,
                BranchRef,
                BuildId);

        /// <summary>
        /// Returns the four <c>azuredevops/pipeline/build/*</c> property name/value pairs
        /// validated by <c>GHAzDO1019</c>. Exposed so JSON-direct callers can stamp them
        /// without constructing a typed <see cref="Run"/>.
        /// </summary>
        internal IReadOnlyList<KeyValuePair<string, string>> GetPipelinePropertyValues()
            => new[]
            {
                new KeyValuePair<string, string>(PropBuildDefinitionId, BuildDefinitionId.ToString(CultureInfo.InvariantCulture)),
                new KeyValuePair<string, string>(PropBuildDefinitionName, BuildDefinitionName),
                new KeyValuePair<string, string>(PropPhaseId, PhaseId.ToString("D", CultureInfo.InvariantCulture)),
                new KeyValuePair<string, string>(PropPhaseName, PhaseName),
            };

        /// <summary>
        /// True when this context carries at least one <c>versionControlProvenance</c>
        /// field (repository URI, revision id, or branch ref) lifted from the pipeline
        /// environment. False indicates the VCP enrichment path is a no-op for this
        /// context and callers should leave any caller-supplied VCP untouched.
        /// </summary>
        internal bool HasVcpFields => RepositoryUri != null
            || !string.IsNullOrEmpty(RevisionId)
            || !string.IsNullOrEmpty(BranchRef);

        /// <summary>
        /// Returns the non-null <c>versionControlProvenance</c> field name/value pairs
        /// for this pipeline context. Pairs are ordered <c>repositoryUri</c>,
        /// <c>revisionId</c>, <c>branch</c>; absent fields are omitted (the caller
        /// should treat the list as the set we know about). Exposed so JSON-direct
        /// callers can enrich without constructing a typed
        /// <see cref="VersionControlDetails"/>.
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

        private static bool IsAbsentOrEqual(string existingValue, string detectedValue, string label, out string conflictError)
        {
            conflictError = null;
            if (string.IsNullOrEmpty(existingValue) || string.Equals(existingValue, detectedValue, StringComparison.Ordinal))
            {
                return true;
            }

            conflictError = string.Format(
                CultureInfo.InvariantCulture,
                "Supplied {0} '{1}' conflicts with detected ADO pipeline value '{2}'.",
                label,
                existingValue,
                detectedValue);
            return false;
        }

        private static bool IsPropertyAbsentOrEqual(RunAutomationDetails details, string propertyName, string detectedValue, out string conflictError)
        {
            conflictError = null;
            if (!details.TryGetProperty(propertyName, out string existingValue))
            {
                return true;
            }

            if (string.Equals(existingValue, detectedValue, StringComparison.Ordinal))
            {
                return true;
            }

            conflictError = string.Format(
                CultureInfo.InvariantCulture,
                "Supplied automationDetails.properties[\"{0}\"]='{1}' conflicts with detected ADO pipeline value '{2}'.",
                propertyName,
                existingValue,
                detectedValue);
            return false;
        }

        private static void StampPropertyIfAbsent(RunAutomationDetails details, string propertyName, string value)
        {
            if (!details.TryGetProperty(propertyName, out _))
            {
                details.SetProperty(propertyName, value);
            }
        }

        private static bool IsTrueLike(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) { return false; }
            return string.Equals(raw.Trim(), "True", StringComparison.OrdinalIgnoreCase)
                || string.Equals(raw.Trim(), "1", StringComparison.Ordinal);
        }

        private static string FirstNonBlank(string a, string b)
            => !string.IsNullOrWhiteSpace(a) ? a : (!string.IsNullOrWhiteSpace(b) ? b : null);

        // SYSTEM_COLLECTIONURI accepted forms:
        //   https://dev.azure.com/<org>/
        //   https://<org>.visualstudio.com/
        //   https://vsrm.dev.azure.com/<org>/
        //   https://<org>.vsrm.visualstudio.com/
        private static bool TryParseOrganizationName(string raw, string envName, List<string> present, List<string> problems, out string organizationName)
        {
            organizationName = null;
            if (string.IsNullOrWhiteSpace(raw))
            {
                problems.Add(string.Format(CultureInfo.InvariantCulture, "{0} is required but not set", envName));
                return false;
            }

            present.Add(envName);

            if (!Uri.TryCreate(raw.Trim(), UriKind.Absolute, out Uri parsed))
            {
                problems.Add(string.Format(CultureInfo.InvariantCulture, "{0}='{1}' is not a valid absolute URI", envName, raw));
                return false;
            }

            string host = parsed.Host;
            string firstSegment = parsed.AbsolutePath.Trim('/').Split('/')[0];

            if (string.Equals(host, "dev.azure.com", StringComparison.OrdinalIgnoreCase)
                || string.Equals(host, "vsrm.dev.azure.com", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrEmpty(firstSegment))
                {
                    problems.Add(string.Format(CultureInfo.InvariantCulture, "{0}='{1}' is missing the organization path segment", envName, raw));
                    return false;
                }
                organizationName = firstSegment;
                return true;
            }

            if (host.EndsWith(".visualstudio.com", StringComparison.OrdinalIgnoreCase))
            {
                string subdomain = host.Substring(0, host.Length - ".visualstudio.com".Length);
                if (subdomain.EndsWith(".vsrm", StringComparison.OrdinalIgnoreCase))
                {
                    subdomain = subdomain.Substring(0, subdomain.Length - ".vsrm".Length);
                }
                if (string.IsNullOrEmpty(subdomain))
                {
                    problems.Add(string.Format(CultureInfo.InvariantCulture, "{0}='{1}' is missing the organization subdomain", envName, raw));
                    return false;
                }
                organizationName = subdomain;
                return true;
            }

            problems.Add(string.Format(CultureInfo.InvariantCulture, "{0}='{1}' uses an unrecognized host (expected dev.azure.com/<org>, <org>.visualstudio.com, vsrm.dev.azure.com/<org>, or <org>.vsrm.visualstudio.com)", envName, raw));
            return false;
        }

        private static bool TryParseRequiredGuid(string raw, string envName, List<string> present, List<string> problems, out Guid value)
        {
            value = default;
            if (string.IsNullOrWhiteSpace(raw))
            {
                problems.Add(string.Format(CultureInfo.InvariantCulture, "{0} is required but not set", envName));
                return false;
            }

            present.Add(envName);

            if (!Guid.TryParseExact(raw.Trim(), "D", out Guid parsed))
            {
                problems.Add(string.Format(CultureInfo.InvariantCulture, "{0}='{1}' is not a valid GUID (expected canonical 8-4-4-4-12 hex form)", envName, raw));
                return false;
            }

            if (parsed == Guid.Empty)
            {
                problems.Add(string.Format(CultureInfo.InvariantCulture, "{0}='{1}' must not be Guid.Empty", envName, raw));
                return false;
            }

            value = parsed;
            return true;
        }

        private static bool TryParseRequiredPositiveInt(string primaryRaw, string fallbackRaw, string primaryEnvName, string fallbackEnvName, List<string> present, List<string> problems, out int value)
        {
            value = 0;

            string raw = FirstNonBlank(primaryRaw, fallbackRaw);
            string sourceEnv = !string.IsNullOrWhiteSpace(primaryRaw) ? primaryEnvName : fallbackEnvName;

            if (string.IsNullOrWhiteSpace(raw))
            {
                // If neither env var was set, report the primary as missing.
                problems.Add(string.Format(CultureInfo.InvariantCulture, "{0} is required but not set", primaryEnvName));
                return false;
            }

            present.Add(sourceEnv);

            if (!int.TryParse(raw.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed))
            {
                problems.Add(string.Format(CultureInfo.InvariantCulture, "{0}='{1}' is not a valid integer", sourceEnv, raw));
                return false;
            }

            if (parsed <= 0)
            {
                problems.Add(string.Format(CultureInfo.InvariantCulture, "{0}='{1}' must be a positive integer", sourceEnv, raw));
                return false;
            }

            // If both primary and fallback are populated, they must agree (these vars name the
            // same pipeline-level identifier; disagreement is a configuration bug). Phase/Job
            // is exempted from this check — those vars are semantically distinct in YAML
            // pipelines and routinely differ.
            if (fallbackEnvName != null
                && !string.IsNullOrWhiteSpace(primaryRaw)
                && !string.IsNullOrWhiteSpace(fallbackRaw)
                && int.TryParse(fallbackRaw.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int fallbackParsed)
                && fallbackParsed != parsed)
            {
                problems.Add(string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}='{1}' disagrees with {2}='{3}' (both name the same pipeline identifier and must match)",
                    primaryEnvName,
                    primaryRaw,
                    fallbackEnvName,
                    fallbackRaw));
                return false;
            }

            value = parsed;
            return true;
        }

        private static bool TryReadRequiredString(string raw, string envName, List<string> present, List<string> problems, out string value)
        {
            value = null;
            if (string.IsNullOrWhiteSpace(raw))
            {
                problems.Add(string.Format(CultureInfo.InvariantCulture, "{0} is required but not set", envName));
                return false;
            }

            present.Add(envName);
            value = raw.Trim();
            return true;
        }

        // The two parsers below treat absence as "no signal" (return false silently). Presence
        // adds to `present` (so the diagnostic surface lists the var alongside the required
        // ones), and malformed presence adds to `problems` so a misconfigured BUILD_REPOSITORY_URI
        // or BUILD_SOURCEVERSION turns into a clean Partial rather than a half-enriched VCP.
        private static bool TryReadOptionalAbsoluteHttpUri(string raw, string envName, List<string> present, List<string> problems, out Uri value)
        {
            value = null;
            if (string.IsNullOrWhiteSpace(raw))
            {
                return false;
            }

            present.Add(envName);

            if (!Uri.TryCreate(raw.Trim(), UriKind.Absolute, out Uri parsed)
                || (parsed.Scheme != Uri.UriSchemeHttp && parsed.Scheme != Uri.UriSchemeHttps))
            {
                problems.Add(string.Format(CultureInfo.InvariantCulture, "{0}='{1}' is not a valid absolute http(s) URI", envName, raw));
                return false;
            }

            value = parsed;
            return true;
        }

        private static bool TryReadOptionalRevisionId(string raw, string envName, List<string> present, List<string> problems, out string value)
        {
            value = null;
            if (string.IsNullOrWhiteSpace(raw))
            {
                return false;
            }

            present.Add(envName);

            string trimmed = raw.Trim();
            if (!s_revisionIdRegex.IsMatch(trimmed))
            {
                problems.Add(string.Format(CultureInfo.InvariantCulture, "{0}='{1}' is not a valid revision id (expected 7-40 hex chars)", envName, raw));
                return false;
            }

            value = trimmed;
            return true;
        }

        private static string FormatPartialError(List<string> present, List<string> problems)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ADO pipeline context is partially configured. Either populate every required variable or clear them all.");
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
