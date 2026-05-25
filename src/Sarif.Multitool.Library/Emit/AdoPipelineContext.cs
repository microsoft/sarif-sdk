// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

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

        internal const string PropBuildDefinitionId = "azuredevops/pipeline/build/buildDefinitionId";
        internal const string PropBuildDefinitionName = "azuredevops/pipeline/build/buildDefinitionName";
        internal const string PropPhaseId = "azuredevops/pipeline/build/phaseId";
        internal const string PropPhaseName = "azuredevops/pipeline/build/phaseName";
        internal const string AutomationIdPrefix = "azuredevops/pipeline/build/";

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
                branchRefValue = NormalizeBranchRef(parsedBranch);
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
            };
            return DetectionState.Complete;
        }

        /// <summary>
        /// Stamps the detected pipeline identity onto <paramref name="run"/>.
        /// Creates <see cref="Run.AutomationDetails"/> if absent; does not overwrite
        /// <c>Guid</c> or <c>CorrelationGuid</c> fields populated from other sources.
        /// </summary>
        public void ApplyTo(Run run)
        {
            if (run == null) { throw new ArgumentNullException(nameof(run)); }

            run.AutomationDetails ??= new RunAutomationDetails();
            run.AutomationDetails.Id = string.Format(
                CultureInfo.InvariantCulture,
                "{0}{1}/{2}/{3}/{4}/{5}/{6}",
                AutomationIdPrefix,
                OrganizationName,
                ProjectId,
                BuildDefinitionId,
                PhaseId,
                BranchRef,
                BuildId);
            run.AutomationDetails.SetProperty(PropBuildDefinitionId, BuildDefinitionId.ToString(CultureInfo.InvariantCulture));
            run.AutomationDetails.SetProperty(PropBuildDefinitionName, BuildDefinitionName);
            run.AutomationDetails.SetProperty(PropPhaseId, PhaseId.ToString("D", CultureInfo.InvariantCulture));
            run.AutomationDetails.SetProperty(PropPhaseName, PhaseName);
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

        // AdvSec's helper prepends "refs/heads/" only if absent. Preserve PR refs
        // (refs/pull/N/merge) and tag refs (refs/tags/...) unchanged.
        private static string NormalizeBranchRef(string raw)
        {
            return raw.StartsWith("refs/", StringComparison.Ordinal)
                ? raw
                : "refs/heads/" + raw;
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
