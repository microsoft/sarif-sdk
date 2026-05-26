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
    /// Implements <c>multitool emit-init-run</c>: creates an append-only SARIF event log
    /// (<c>&lt;output&gt;.wip.jsonl</c>) seeded with a <c>run-header</c> event built from a
    /// caller-supplied SARIF <c>Run</c> JSON document (file via <c>--input</c> or stdin).
    /// </summary>
    /// <remarks>
    /// <para>The JSON-payload contract matches the other emit verbs (<c>add-result</c>,
    /// <c>add-notification</c>, <c>add-reporting-descriptor</c>). The supplied <c>Run</c> may
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
    public class EmitInitRunCommand : CommandBase
    {
        internal const string SourceRootBaseId = "SRCROOT";
        internal static readonly string[] AiOriginValues = new[] { "generated", "annotated", "synthesized" };

        private readonly IEnvironmentVariableGetter _environment;

        public EmitInitRunCommand() : this(new EnvironmentVariableGetter())
        {
        }

        public EmitInitRunCommand(IEnvironmentVariableGetter environment)
        {
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        public int Run(EmitInitRunOptions options, IFileSystem fileSystem = null)
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

                // Read and parse the caller-supplied Run JSON before any file-system side
                // effects so malformed input never leaves a wip on disk.
                int code = EmitEventLogHelpers.TryReadJsonPayload(
                    options.InputFilePath,
                    payloadKind: "run",
                    fileSystem,
                    out JToken payload);
                if (code != SUCCESS) { return code; }

                var runObject = (JObject)payload;

                if (!TryRejectSarifLogShape(runObject)) { return FAILURE; }

                if (!TryValidateRunHeader(runObject)) { return FAILURE; }

                if (adoState == AdoPipelineContext.DetectionState.Complete)
                {
                    if (!TryStampAdoContext(runObject, adoContext, out string stampError))
                    {
                        Console.Error.WriteLine(stampError);
                        return FAILURE;
                    }
                }

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
            // A common UX mistake: piping a full SARIF log document (`{ "version": "2.1.0",
            // "runs": [...] }`) in place of a Run object. Without this guard the user gets a
            // misleading "tool.driver.name missing" diagnostic; with it they get a clear
            // shape-mismatch message that points at the right fix.
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

        private static bool TryValidateRunHeader(JObject runObject)
        {
            // Parent shapes first so child accessors below don't trip JValue indexers
            // (which throw InvalidOperationException, surface as stack traces, and starve
            // the producer of an actionable fix).
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
        /// If <paramref name="parent"/> carries a token at <paramref name="key"/>, requires it to
        /// be a JSON object and returns it via <paramref name="value"/>. Returns true when the key
        /// is absent (or explicitly null) without surfacing an error; returns false with a clear
        /// AI-consumable diagnostic when the key is present but the wrong shape (e.g.
        /// <c>"tool": "x"</c>). Walking parent shapes up front prevents JValue indexer accesses
        /// further down the validator chain from throwing InvalidOperationException.
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
        /// Stamps ADO pipeline identity directly onto the JSON payload. Mutating the JObject
        /// rather than round-tripping through the typed <see cref="Run"/> model preserves any
        /// SARIF Run fields the typed model doesn't surface (e.g., <c>redactionTokens</c>) in
        /// the wip line. (The replayer materializes a typed <c>Run</c> at finalize time, so
        /// non-typed fields are durable only up to that boundary.)
        /// </summary>
        private static bool TryStampAdoContext(JObject runObject, AdoPipelineContext context, out string conflictError)
        {
            conflictError = null;

            string canonicalId = context.BuildCanonicalAutomationId();
            IReadOnlyList<KeyValuePair<string, string>> pipelineProps = context.GetPipelinePropertyValues();

            // Probe-before-write so a conflict on any field leaves the JObject unchanged.
            // TryValidateRunHeader already enforced that automationDetails (if present) is an
            // object and automationDetails.properties (if present) is an object.
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
    }
}
