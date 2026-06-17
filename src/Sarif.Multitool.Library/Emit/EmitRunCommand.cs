// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;

using Microsoft.CodeAnalysis.Sarif.Driver;
using Microsoft.CodeAnalysis.Sarif.Emit;

using Newtonsoft.Json.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    /// <summary>
    /// Implements <c>emit-run</c>: creates an append-only SARIF event log
    /// (<c>&lt;output&gt;.wip.jsonl</c>) seeded with a <c>run-header</c> event built from a
    /// caller-supplied SARIF <c>Run</c> JSON document (file via <c>--input</c> or stdin).
    /// </summary>
    /// <remarks>
    /// <para>The JSON-payload contract matches the other emit verbs (<c>emit-results</c>,
    /// <c>emit-invocations</c>, <c>emit-notification-descriptors</c>,
    /// <c>emit-rule-descriptors</c>). The supplied <c>Run</c> may
    /// carry any subset of the partial-Run shape the replayer accepts (<c>tool</c>,
    /// <c>language</c>, <c>columnKind</c>, <c>defaultEncoding</c>, <c>defaultSourceLanguage</c>,
    /// <c>originalUriBaseIds</c>, <c>versionControlProvenance</c>, <c>automationDetails</c>,
    /// <c>baselineGuid</c>, <c>redactionTokens</c>, …). <c>results</c>, <c>invocations</c>, and
    /// notifications on the header are ignored at replay; those belong in their own events.</para>
    /// <para>State table:</para>
    /// <list type="table">
    /// <listheader>
    /// <term>State</term>
    /// <term>No <c>--force-overwrite</c></term>
    /// <term>With <c>--force-overwrite</c></term>
    /// </listheader>
    /// <item>
    /// <term>Neither .sarif nor .wip.jsonl exists</term>
    /// <term>Create new .wip.jsonl</term>
    /// <term>Create new .wip.jsonl</term>
    /// </item>
    /// <item>
    /// <term>.sarif exists, no .wip.jsonl</term>
    /// <term>Fail — would clobber a committed SARIF on finalize</term>
    /// <term>Create new .wip.jsonl (existing .sarif is left until finalize replaces it)</term>
    /// </item>
    /// <item>
    /// <term>No .sarif, .wip.jsonl exists</term>
    /// <term>Fail — another authoring session is in flight (or was crashed)</term>
    /// <term>Delete .wip.jsonl and recreate</term>
    /// </item>
    /// <item>
    /// <term>Both .sarif and .wip.jsonl exist</term>
    /// <term>Fail</term>
    /// <term>Delete .wip.jsonl and recreate</term>
    /// </item>
    /// </list>
    /// </remarks>
    public class EmitRunCommand : CommandBase
    {
        internal const string SourceRootBaseId = "SRCROOT";
        internal static readonly string[] AiOriginValues = new[] { "generated", "annotated", "synthesized" };

        private readonly IEnvironmentVariableGetter _environment;

        public EmitRunCommand() : this(new EnvironmentVariableGetter())
        {
        }

        public EmitRunCommand(IEnvironmentVariableGetter environment)
        {
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        public int Run(EmitRunOptions options, IFileSystem fileSystem = null)
        {
            fileSystem ??= Sarif.FileSystem.Instance;

            try
            {
                if (string.IsNullOrWhiteSpace(options?.OutputFilePath))
                {
                    Console.Error.WriteLine("Output SARIF path is required.");
                    return FAILURE;
                }

                // Detect ADO pipeline context BEFORE any file-system side effects so a partial
                // failure doesn't leave a half-written or freshly-deleted .wip.jsonl on disk.
                AdoPipelineContext.DetectionState adoState =
                    AdoPipelineContext.TryDetect(_environment, out AdoPipelineContext adoContext, out string adoError);
                if (adoState == AdoPipelineContext.DetectionState.Partial)
                {
                    Console.Error.WriteLine(adoError);
                    return FAILURE;
                }

                // Same up-front detection for GitHub Actions; GHA contributes to VCP stamping.
                GitHubActionsContext.DetectionState ghaState =
                    GitHubActionsContext.TryDetect(_environment, out GitHubActionsContext ghaContext, out string ghaError);
                if (ghaState == GitHubActionsContext.DetectionState.Partial)
                {
                    Console.Error.WriteLine(ghaError);
                    return FAILURE;
                }

                // Parse input before file-system side effects.
                int code = EmitEventLogHelpers.TryReadJsonPayload(
                    options.InputFilePath,
                    payloadKind: "run",
                    fileSystem,
                    out JToken payload);
                if (code != SUCCESS) { return code; }

                var runObject = (JObject)payload;

                if (!TryRejectSarifLogShape(runObject)) { return FAILURE; }

                if (!TryValidateRunHeader(runObject, fileSystem)) { return FAILURE; }

                if (adoState == AdoPipelineContext.DetectionState.Complete)
                {
                    if (!TryStampAdoContext(runObject, adoContext, out string stampError))
                    {
                        Console.Error.WriteLine(stampError);
                        return FAILURE;
                    }
                }

                // ADO and GHA VCP fields must agree when both sources publish the same field.
                if (adoState == AdoPipelineContext.DetectionState.Complete
                    || ghaState == GitHubActionsContext.DetectionState.Complete)
                {
                    if (!TryResolveVcpFields(
                            adoContext,
                            ghaContext,
                            out Uri vcpRepositoryUri,
                            out string vcpRevisionId,
                            out string vcpBranch,
                            out string mergeError))
                    {
                        Console.Error.WriteLine(mergeError);
                        return FAILURE;
                    }

                    if (!TryStampVcp(
                            runObject,
                            vcpRepositoryUri,
                            vcpRevisionId,
                            vcpBranch,
                            out string vcpError))
                    {
                        Console.Error.WriteLine(vcpError);
                        return FAILURE;
                    }
                }

                if (!TryValidateVcpRepositoryShapes(runObject)) { return FAILURE; }

                string outputPath = Path.GetFullPath(options.OutputFilePath);
                string wipPath = outputPath + EmitEventLogHelpers.WipSuffix;

                bool sarifExists = fileSystem.FileExists(outputPath);
                bool wipExists = fileSystem.FileExists(wipPath);

                if ((sarifExists || wipExists) && !options.ForceOverwrite)
                {
                    Console.Error.WriteLine(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            "'{0}'{1} already exists. Pass --force-overwrite to replace.",
                            outputPath,
                            wipExists && sarifExists ? " (and its .wip.jsonl)"
                            : wipExists ? " (.wip.jsonl)"
                            : string.Empty));
                    return FAILURE;
                }

                if (wipExists)
                {
                    fileSystem.FileDelete(wipPath);
                }

                string directory = Path.GetDirectoryName(wipPath);
                if (!string.IsNullOrEmpty(directory) && !fileSystem.DirectoryExists(directory))
                {
                    fileSystem.DirectoryCreateDirectory(directory);
                }

                WarnOnIgnoredHeaderData(runObject);

                using (var writer = new SarifEventLogWriter(wipPath))
                {
                    writer.Append(SarifEventKinds.RunHeader, runObject);
                }

                string toolName = (string)runObject["tool"]?["driver"]?["name"];
                Console.Out.WriteLine(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        "Opened '{0}' for '{1}'.",
                        wipPath,
                        toolName));
                return SUCCESS;
            }
            catch (Exception ex) when (!Debugger.IsAttached)
            {
                Console.Error.WriteLine(ex);
                return FAILURE;
            }
        }

        private static bool TryRejectSarifLogShape(JObject runObject)
        {
            // Catch full SARIF logs before they fall through to a misleading tool.driver.name error.
            JToken runs = runObject["runs"];
            JToken version = runObject["version"];
            if (runs != null && version != null && runs.Type == JTokenType.Array)
            {
                Console.Error.WriteLine(
                    "--input expects a SARIF Run object, not a SARIF log. Supply runs[0] or construct a Run JSON directly.");
                return false;
            }

            return true;
        }

        // results and invocations on the run header are dropped when the event log is replayed
        // (SarifEventReplayer); they belong in their own emit-results / emit-invocations events. Warn
        // rather than reject so a producer that ships a fuller Run object is told what is lost.
        private static void WarnOnIgnoredHeaderData(JObject runObject)
        {
            var dropped = new List<string>();
            if (runObject["results"] is JArray results && results.Count > 0)
            {
                dropped.Add(string.Format(CultureInfo.CurrentCulture, "results[{0}]", results.Count));
            }

            if (runObject["invocations"] is JArray invocations && invocations.Count > 0)
            {
                dropped.Add(string.Format(CultureInfo.CurrentCulture, "invocations[{0}]", invocations.Count));
            }

            if (dropped.Count > 0)
            {
                Console.Error.WriteLine(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        "warning: the run header carries {0}; this data is ignored at replay (results belong in emit-results, invocations in emit-invocations) and will not appear in the finalized log.",
                        string.Join(", ", dropped)));
            }
        }

        private static bool TryValidateRunHeader(JObject runObject, IFileSystem fileSystem)
        {
            // Validate parent shapes before child accessors can trip JValue indexers.
            if (!TryRequireOptionalObject(runObject, "tool", out JObject toolObject)) { return false; }
            JObject driverObject = null;
            if (toolObject != null
                && !TryRequireOptionalObject(toolObject, "tool.driver", out driverObject))
            {
                return false;
            }
            if (!TryRequireOptionalObject(runObject, "automationDetails", out JObject automationDetailsObject)) { return false; }
            if (automationDetailsObject != null
                && !TryRequireOptionalObject(automationDetailsObject, "automationDetails.properties", out _))
            {
                return false;
            }
            if (!TryRequireOptionalObject(runObject, "properties", out _)) { return false; }
            if (!TryRequireOptionalObject(runObject, "originalUriBaseIds", out JObject originalUriBaseIdsObject)) { return false; }
            if (originalUriBaseIdsObject != null
                && !TryRequireOptionalObject(originalUriBaseIdsObject, "originalUriBaseIds[\"SRCROOT\"]", out _))
            {
                return false;
            }

            JToken toolDriverNameToken = driverObject?["name"];
            if (toolDriverNameToken == null
                || toolDriverNameToken.Type != JTokenType.String
                || string.IsNullOrWhiteSpace((string)toolDriverNameToken))
            {
                Console.Error.WriteLine(
                    "tool.driver.name is required and must be a non-empty JSON string.");
                return false;
            }

            if (!TryValidateOptionalUriToken(
                driverObject?["informationUri"],
                "tool.driver.informationUri",
                EmitEventLogHelpers.DocumentationUriSchemes))
            {
                return false;
            }

            JToken vcpToken = runObject["versionControlProvenance"];
            if (vcpToken != null)
            {
                if (vcpToken.Type != JTokenType.Array)
                {
                    Console.Error.WriteLine(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            "versionControlProvenance must be a JSON array, but the payload supplied a {0}.",
                            vcpToken.Type.ToString().ToLowerInvariant()));
                    return false;
                }

                int index = 0;
                foreach (JToken entry in (JArray)vcpToken)
                {
                    if (entry.Type != JTokenType.Object)
                    {
                        Console.Error.WriteLine(
                            string.Format(
                                CultureInfo.CurrentCulture,
                                "versionControlProvenance[{0}] must be a JSON object, but the payload supplied a {1}.",
                                index,
                                entry.Type.ToString().ToLowerInvariant()));
                        return false;
                    }

                    if (!TryValidateOptionalUriToken(
                        entry["repositoryUri"],
                        string.Format(CultureInfo.InvariantCulture, "versionControlProvenance[{0}].repositoryUri", index),
                        EmitEventLogHelpers.DocumentationUriSchemes))
                    {
                        return false;
                    }

                    index++;
                }
            }

            if (!TryValidateOptionalUriToken(
                originalUriBaseIdsObject?[SourceRootBaseId]?["uri"],
                "originalUriBaseIds[\"SRCROOT\"].uri",
                EmitEventLogHelpers.BaseUriSchemes))
            {
                return false;
            }

            if (!TryValidateSourceRootResolvesOnDisk(
                originalUriBaseIdsObject?[SourceRootBaseId]?["uri"],
                fileSystem))
            {
                return false;
            }

            if (!TryValidateOptionalGuidToken(automationDetailsObject?["guid"], "automationDetails.guid"))
            {
                return false;
            }

            if (!TryValidateOptionalGuidToken(automationDetailsObject?["correlationGuid"], "automationDetails.correlationGuid"))
            {
                return false;
            }

            if (!TryValidateOptionalAiOriginToken(((JObject)runObject["properties"])?["ai/origin"]))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Confirms that every present <c>versionControlProvenance[].repositoryUri</c> has a shape
        /// from which <see cref="EmitFinalizeRebaseVisitor"/> can later derive a portable root. Runs
        /// after header validation (which proves each value is an absolute https URI) and after env
        /// stamping, so both caller-supplied and stamped entries are covered. Entries without a
        /// repositoryUri are left to the finalize-time contract.
        /// </summary>
        private static bool TryValidateVcpRepositoryShapes(JObject runObject)
        {
            if (runObject["versionControlProvenance"] is not JArray vcpArray) { return true; }

            int index = 0;
            foreach (JToken entry in vcpArray)
            {
                JToken repositoryUriToken = (entry as JObject)?["repositoryUri"];
                string raw = repositoryUriToken?.Type == JTokenType.String ? (string)repositoryUriToken : null;

                if (!string.IsNullOrEmpty(raw)
                    && Uri.TryCreate(raw, UriKind.Absolute, out Uri repositoryUri)
                    && !VcpPortableRoot.TryValidateRepositoryUri(repositoryUri, out _, out string error))
                {
                    Console.Error.WriteLine(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            "versionControlProvenance[{0}]: {1}",
                            index,
                            error));
                    return false;
                }

                index++;
            }

            return true;
        }

        /// <summary>
        /// Requires an optional token to be null/absent or a JSON object; returns the object via
        /// <paramref name="value"/>.
        /// </summary>
        private static bool TryRequireOptionalObject(JObject parent, string path, out JObject value)
        {
            value = null;
            JToken token = parent[ExtractLeafKey(path)];
            if (token == null || token.Type == JTokenType.Null) { return true; }
            if (token.Type != JTokenType.Object)
            {
                Console.Error.WriteLine(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        "{0} must be a JSON object, but the payload supplied a {1}.",
                        path,
                        token.Type.ToString().ToLowerInvariant()));
                return false;
            }
            value = (JObject)token;
            return true;
        }

        private static string ExtractLeafKey(string path)
        {
            // tool.driver -> driver; automationDetails.properties -> properties; originalUriBaseIds["SRCROOT"] -> SRCROOT
            int bracket = path.LastIndexOf('[');
            if (bracket >= 0)
            {
                int closing = path.LastIndexOf(']');
                if (closing > bracket)
                {
                    string inner = path.Substring(bracket + 1, closing - bracket - 1);
                    return inner.Trim('"');
                }
            }
            int dot = path.LastIndexOf('.');
            return dot < 0 ? path : path.Substring(dot + 1);
        }

        private static bool TryValidateOptionalUriToken(JToken token, string path, string[] allowedSchemes)
        {
            if (token == null || token.Type == JTokenType.Null) { return true; }

            if (token.Type != JTokenType.String)
            {
                Console.Error.WriteLine(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        "{0} must be a JSON string, but the payload supplied a {1}.",
                        path,
                        token.Type.ToString().ToLowerInvariant()));
                return false;
            }

            string value = (string)token;
            if (string.IsNullOrWhiteSpace(value)) { return true; }

            if (!EmitEventLogHelpers.TryValidateUri(value, path, allowedSchemes, out string error))
            {
                Console.Error.WriteLine(error);
                return false;
            }

            return true;
        }

        // A file: SRCROOT must resolve to a directory that exists on disk when the run header is
        // received, so finalize can enrich result locations against an observable checkout.
        private static bool TryValidateSourceRootResolvesOnDisk(JToken token, IFileSystem fileSystem)
        {
            if (token == null || token.Type != JTokenType.String) { return true; }

            string value = (string)token;
            if (string.IsNullOrWhiteSpace(value)) { return true; }

            if (!Uri.TryCreate(value, UriKind.Absolute, out Uri uri) || !uri.IsFile) { return true; }

            if (!fileSystem.DirectoryExists(uri.LocalPath))
            {
                Console.Error.WriteLine(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        "originalUriBaseIds[\"SRCROOT\"].uri: '{0}' does not resolve to an existing directory ('{1}'). A file: source root must point at an observable checkout when the run header is received.",
                        value,
                        uri.LocalPath));
                return false;
            }

            return true;
        }

        private static bool TryValidateOptionalGuidToken(JToken token, string path)
        {
            if (token == null || token.Type == JTokenType.Null) { return true; }

            if (token.Type != JTokenType.String)
            {
                Console.Error.WriteLine(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        "{0} must be a JSON string, but the payload supplied a {1}.",
                        path,
                        token.Type.ToString().ToLowerInvariant()));
                return false;
            }

            string raw = (string)token;
            if (string.IsNullOrWhiteSpace(raw) || !Guid.TryParseExact(raw, "D", out _))
            {
                Console.Error.WriteLine(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        "{0}: '{1}' is not a valid GUID (expected canonical 8-4-4-4-12 hex form, e.g. a7ad9ab8-1234-5678-9abc-def012345678). Omit the field if no value is available.",
                        path,
                        raw));
                return false;
            }

            return true;
        }

        private static bool TryValidateOptionalAiOriginToken(JToken token)
        {
            if (token == null || token.Type == JTokenType.Null) { return true; }

            if (token.Type != JTokenType.String)
            {
                Console.Error.WriteLine(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        "properties[\"ai/origin\"] must be a JSON string, but the payload supplied a {0}.",
                        token.Type.ToString().ToLowerInvariant()));
                return false;
            }

            string raw = (string)token;
            foreach (string v in AiOriginValues)
            {
                if (string.Equals(v, raw, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            Console.Error.WriteLine(
                string.Format(
                    CultureInfo.CurrentCulture,
                    "properties[\"ai/origin\"]: '{0}' is not valid. Expected exactly one of: {1}.",
                    raw,
                    string.Join(", ", AiOriginValues)));
            return false;
        }

        /// <summary>
        /// Stamps ADO pipeline identity directly onto the JSON payload, preserving fields not
        /// surfaced by the typed <see cref="Run"/> model.
        /// </summary>
        private static bool TryStampAdoContext(JObject runObject, AdoPipelineContext context, out string conflictError)
        {
            conflictError = null;

            string canonicalId = context.BuildCanonicalAutomationId();
            IReadOnlyList<KeyValuePair<string, string>> pipelineProps = context.GetPipelinePropertyValues();

            // Probe-before-write so a conflict on any field leaves the JObject unchanged.
            var automationDetails = (JObject)runObject["automationDetails"];
            if (automationDetails != null)
            {
                JToken existingIdToken = automationDetails["id"];
                if (existingIdToken != null && existingIdToken.Type != JTokenType.Null)
                {
                    if (existingIdToken.Type != JTokenType.String)
                    {
                        conflictError = string.Format(
                            CultureInfo.CurrentCulture,
                            "Supplied automationDetails.id must be a JSON string, but the payload supplied a {0}. Detected ADO pipeline value is '{1}'; either match it or omit the field.",
                            existingIdToken.Type.ToString().ToLowerInvariant(),
                            canonicalId);
                        return false;
                    }
                    string existingId = (string)existingIdToken;
                    if (existingId.Length != 0 && !string.Equals(existingId, canonicalId, StringComparison.Ordinal))
                    {
                        conflictError = string.Format(
                            CultureInfo.CurrentCulture,
                            "Supplied automationDetails.id '{0}' conflicts with detected ADO pipeline value '{1}'.",
                            existingId,
                            canonicalId);
                        return false;
                    }
                }

                var existingProperties = (JObject)automationDetails["properties"];
                if (existingProperties != null)
                {
                    foreach (KeyValuePair<string, string> kv in pipelineProps)
                    {
                        JToken existingPropToken = existingProperties[kv.Key];
                        if (existingPropToken == null || existingPropToken.Type == JTokenType.Null) { continue; }
                        if (existingPropToken.Type != JTokenType.String)
                        {
                            conflictError = string.Format(
                                CultureInfo.CurrentCulture,
                                "Supplied automationDetails.properties[\"{0}\"] must be a JSON string, but the payload supplied a {1}. Detected ADO pipeline value is '{2}'; either match it or omit the field.",
                                kv.Key,
                                existingPropToken.Type.ToString().ToLowerInvariant(),
                                kv.Value);
                            return false;
                        }
                        string existingPropValue = (string)existingPropToken;
                        if (existingPropValue.Length != 0 && !string.Equals(existingPropValue, kv.Value, StringComparison.Ordinal))
                        {
                            conflictError = string.Format(
                                CultureInfo.CurrentCulture,
                                "Supplied automationDetails.properties[\"{0}\"]='{1}' conflicts with detected ADO pipeline value '{2}'.",
                                kv.Key,
                                existingPropValue,
                                kv.Value);
                            return false;
                        }
                    }
                }
            }

            JObject ensuredAutomationDetails = automationDetails;
            if (ensuredAutomationDetails == null)
            {
                ensuredAutomationDetails = new JObject();
                runObject["automationDetails"] = ensuredAutomationDetails;
            }

            JToken idToken = ensuredAutomationDetails["id"];
            if (idToken == null || idToken.Type == JTokenType.Null
                || (idToken.Type == JTokenType.String && ((string)idToken).Length == 0))
            {
                ensuredAutomationDetails["id"] = canonicalId;
            }

            var ensuredProperties = (JObject)ensuredAutomationDetails["properties"];
            if (ensuredProperties == null)
            {
                ensuredProperties = new JObject();
                ensuredAutomationDetails["properties"] = ensuredProperties;
            }

            foreach (KeyValuePair<string, string> kv in pipelineProps)
            {
                JToken propToken = ensuredProperties[kv.Key];
                if (propToken == null || propToken.Type == JTokenType.Null
                    || (propToken.Type == JTokenType.String && ((string)propToken).Length == 0))
                {
                    ensuredProperties[kv.Key] = kv.Value;
                }
            }

            return true;
        }

        /// <summary>
        /// Enriches <c>versionControlProvenance</c> with resolved repository URI, revision id,
        /// and branch fields. Empty VCP arrays receive a synthesized entry only when a repository
        /// URI is known; single-entry arrays are enriched; multi-entry arrays are left untouched.
        /// </summary>
        private static bool TryStampVcp(
            JObject runObject,
            Uri repositoryUri,
            string revisionId,
            string branch,
            out string conflictError)
        {
            conflictError = null;

            var vcpFields = new List<KeyValuePair<string, string>>(3);
            if (repositoryUri != null)
            {
                vcpFields.Add(new KeyValuePair<string, string>(VcpFieldNames.RepositoryUri, repositoryUri.AbsoluteUri));
            }
            if (!string.IsNullOrEmpty(revisionId))
            {
                vcpFields.Add(new KeyValuePair<string, string>(VcpFieldNames.RevisionId, revisionId));
            }
            if (!string.IsNullOrEmpty(branch))
            {
                vcpFields.Add(new KeyValuePair<string, string>(VcpFieldNames.Branch, branch));
            }

            if (vcpFields.Count == 0)
            {
                return true;
            }

            var vcpArray = (JArray)runObject["versionControlProvenance"];

            if (vcpArray == null || vcpArray.Count == 0)
            {
                // Branch/revision without a repository URI cannot bind to a repo downstream.
                if (repositoryUri == null)
                {
                    return true;
                }

                var entry = new JObject();
                foreach (KeyValuePair<string, string> kv in vcpFields)
                {
                    entry[kv.Key] = kv.Value;
                }

                StampMappedToIfAbsent(entry, runObject);

                if (vcpArray == null)
                {
                    runObject["versionControlProvenance"] = new JArray { entry };
                }
                else
                {
                    vcpArray.Add(entry);
                }
                return true;
            }

            if (vcpArray.Count > 1)
            {
                // Multi-entry VCP is caller-authored; do not guess the source-repo entry.
                return true;
            }

            var existing = (JObject)vcpArray[0];

            // Probe-before-write so a conflict on any field leaves the JObject unchanged.
            foreach (KeyValuePair<string, string> kv in vcpFields)
            {
                JToken existingToken = existing[kv.Key];
                if (existingToken == null || existingToken.Type == JTokenType.Null) { continue; }

                if (existingToken.Type != JTokenType.String)
                {
                    conflictError = string.Format(
                        CultureInfo.CurrentCulture,
                        "Supplied versionControlProvenance[0].{0} must be a JSON string, but the payload supplied a {1}. Detected pipeline value is '{2}'; either match it or omit the field.",
                        kv.Key,
                        existingToken.Type.ToString().ToLowerInvariant(),
                        kv.Value);
                    return false;
                }

                string existingValue = (string)existingToken;
                if (existingValue.Length == 0) { continue; }

                if (!VcpFieldValuesAgree(kv.Key, existingValue, kv.Value))
                {
                    conflictError = string.Format(
                        CultureInfo.CurrentCulture,
                        "Supplied versionControlProvenance[0].{0}='{1}' conflicts with detected pipeline value '{2}'.",
                        kv.Key,
                        existingValue,
                        kv.Value);
                    return false;
                }
            }

            foreach (KeyValuePair<string, string> kv in vcpFields)
            {
                JToken existingToken = existing[kv.Key];
                if (existingToken == null || existingToken.Type == JTokenType.Null
                    || (existingToken.Type == JTokenType.String && ((string)existingToken).Length == 0))
                {
                    existing[kv.Key] = kv.Value;
                }
            }

            StampMappedToIfAbsent(existing, runObject);

            return true;
        }

        // The auto-stamped source-repo entry binds to the local source root via mappedTo so
        // emit-finalize can deconstruct local paths into portable permalinks. Only stamp when the
        // run declares originalUriBaseIds.SRCROOT (otherwise the binding could not resolve) and the
        // caller has not already supplied its own mappedTo.
        private static void StampMappedToIfAbsent(JObject vcpEntry, JObject runObject)
        {
            JToken sourceRoot = runObject["originalUriBaseIds"]?[SourceRootBaseId];
            if (sourceRoot == null || sourceRoot.Type != JTokenType.Object)
            {
                return;
            }

            JToken existingMappedTo = vcpEntry["mappedTo"];
            if (existingMappedTo != null && existingMappedTo.Type != JTokenType.Null)
            {
                return;
            }

            vcpEntry["mappedTo"] = new JObject { ["uriBaseId"] = SourceRootBaseId };
        }

        /// <summary>
        /// Resolves VCP fields from ADO and GitHub Actions contexts. ADO seeds each field; GHA
        /// fills only the fields ADO left empty. Any field both sources publish must agree, or
        /// stamping is refused.
        /// </summary>
        private static bool TryResolveVcpFields(
            AdoPipelineContext adoContext,
            GitHubActionsContext ghaContext,
            out Uri repositoryUri,
            out string revisionId,
            out string branch,
            out string conflictError)
        {
            conflictError = null;
            repositoryUri = adoContext?.RepositoryUri;
            revisionId = adoContext?.RevisionId;
            branch = adoContext?.BranchRef;

            if (ghaContext == null) { return true; }

            if (!TryMergeRepositoryUri(ref repositoryUri, ghaContext.RepositoryUri, out conflictError))
            {
                return false;
            }
            if (!TryMergeStringField(ref revisionId, ghaContext.RevisionId, VcpFieldNames.RevisionId, out conflictError))
            {
                return false;
            }
            if (!TryMergeStringField(ref branch, ghaContext.BranchRef, VcpFieldNames.Branch, out conflictError))
            {
                return false;
            }

            return true;
        }

        private static bool TryMergeRepositoryUri(ref Uri resolved, Uri candidate, out string conflictError)
        {
            conflictError = null;
            if (candidate == null) { return true; }
            if (resolved == null) { resolved = candidate; return true; }
            if (resolved == candidate) { return true; }

            conflictError = string.Format(
                CultureInfo.CurrentCulture,
                "ADO pipeline env says {0}='{1}', GitHub Actions env says {0}='{2}'. Cross-source disagreement; refusing to stamp.",
                VcpFieldNames.RepositoryUri,
                resolved.AbsoluteUri,
                candidate.AbsoluteUri);
            return false;
        }

        private static bool TryMergeStringField(ref string resolved, string candidate, string fieldName, out string conflictError)
        {
            conflictError = null;
            if (string.IsNullOrEmpty(candidate)) { return true; }
            if (string.IsNullOrEmpty(resolved)) { resolved = candidate; return true; }
            if (string.Equals(resolved, candidate, StringComparison.Ordinal)) { return true; }

            conflictError = string.Format(
                CultureInfo.CurrentCulture,
                "ADO pipeline env says {0}='{1}', GitHub Actions env says {0}='{2}'. Cross-source disagreement; refusing to stamp.",
                fieldName,
                resolved,
                candidate);
            return false;
        }

        // repositoryUri values agree iff they parse to equivalent absolute URIs (the URI spec
        // treats scheme/host case-insensitively and discards default ports / dot segments).
        // revisionId and branch are byte-wise (an abbreviated SHA is not equal to its full
        // form, and branch names are case-sensitive in git).
        private static bool VcpFieldValuesAgree(string fieldName, string supplied, string detected)
        {
            if (string.Equals(fieldName, VcpFieldNames.RepositoryUri, StringComparison.Ordinal)
                && Uri.TryCreate(supplied, UriKind.Absolute, out Uri suppliedUri)
                && Uri.TryCreate(detected, UriKind.Absolute, out Uri detectedUri))
            {
                return suppliedUri == detectedUri;
            }

            return string.Equals(supplied, detected, StringComparison.Ordinal);
        }
    }
}
