// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using Newtonsoft.Json.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Emit
{
    /// <summary>
    /// Validates a single emit element and may mutate it in place (e.g. stamping a default). Returns
    /// <c>null</c> when the element is acceptable; otherwise the error describing the rejection.
    /// </summary>
    /// <param name="element">The element to validate.</param>
    /// <param name="index">The element's zero-based position in the submitted payload.</param>
    /// <param name="batched">
    /// <c>true</c> when the payload arrived as a JSON array (batch submission); <c>false</c> when it
    /// arrived as a lone JSON object.
    /// </param>
    public delegate EmitElementError ValidateEmitElement(JObject element, int index, bool batched);

    /// <summary>
    /// In-memory authoring surface for a single SARIF run. A producer appends fully-formed SARIF
    /// objects — results, invocations, reporting descriptors — and each <c>Add*</c> call validates
    /// every element atomically and appends an event per element to the backing <see cref="IEmitSink"/>,
    /// returning a structured <see cref="EmitReport"/>. <see cref="ResolveToLog"/> replays the sink into
    /// a <see cref="SarifLog"/>.
    /// </summary>
    /// <remarks>
    /// <para>This is the in-process C# counterpart to the <c>add-*</c> CLI verbs: a .NET producer can
    /// drive it directly — over an <see cref="InMemoryEmitSink"/> or a <see cref="FileEmitSink"/> —
    /// without spawning a process per call. The CLI verbs are thin shells over this type.</para>
    /// <para>Results and resolution stay index-free: a result carries its <c>ruleId</c>, never a
    /// <c>ruleIndex</c>, and the replay engine re-derives run-scoped indices at resolution. That is
    /// what makes a run a self-contained shard.</para>
    /// </remarks>
    public sealed class RunEmitContext
    {
        private readonly IEmitSink _sink;

        public RunEmitContext(IEmitSink sink)
        {
            _sink = sink ?? throw new ArgumentNullException(nameof(sink));
        }

        /// <summary>Appends the run skeleton (everything except results/invocations/notifications).</summary>
        public void SetRunHeader(JObject runSkeleton)
        {
            _sink.Append(SarifEventKinds.RunHeader, runSkeleton ?? new JObject());
        }

        /// <summary>Validates and appends one result object or an array of result objects.</summary>
        public EmitReport AddResults(JToken payload)
            => AppendBatch(payload, "result", SarifEventKinds.Result, () => ValidateResult);

        /// <summary>Validates and appends one invocation object or an array of invocation objects.</summary>
        public EmitReport AddInvocations(JToken payload)
            => AppendBatch(payload, "invocation", SarifEventKinds.Invocation, () => ValidateInvocation);

        /// <summary>Validates and appends one rule reportingDescriptor or an array of them.</summary>
        public EmitReport AddRuleDescriptors(JToken payload)
            => AppendBatch(
                payload,
                "rule descriptor",
                SarifEventKinds.RuleDescriptor,
                () => BuildDescriptorValidator(isRules: true, "rule descriptor"));

        /// <summary>Validates and appends one notification reportingDescriptor or an array of them.</summary>
        public EmitReport AddNotificationDescriptors(JToken payload)
            => AppendBatch(
                payload,
                "notification descriptor",
                SarifEventKinds.NotificationDescriptor,
                () => BuildDescriptorValidator(isRules: false, "notification descriptor"));

        /// <summary>Replays the events appended so far into a single-run <see cref="SarifLog"/>.</summary>
        public SarifLog ResolveToLog() => SarifEventReplayer.Replay(_sink.ReadAll());

        /// <summary>
        /// Shared orchestration: accept a lone JSON object or an array of objects, validate every
        /// element atomically, and append all or none. On any rejection nothing reaches the sink.
        /// </summary>
        private EmitReport AppendBatch(
            JToken token,
            string payloadKind,
            string eventKind,
            Func<ValidateEmitElement> buildValidator)
        {
            if (token == null)
            {
                return new EmitReport(
                    0,
                    Array.Empty<EmitElementError>(),
                    payloadError: string.Format(
                        CultureInfo.CurrentCulture,
                        "{0} JSON must be a JSON object or an array of objects, but no payload was supplied.",
                        Capitalize(payloadKind)));
            }

            var elements = new List<JObject>();
            var errors = new List<EmitElementError>();
            bool batched;

            if (token.Type == JTokenType.Object)
            {
                batched = false;
                elements.Add((JObject)token);
            }
            else if (token.Type == JTokenType.Array)
            {
                batched = true;
                int i = 0;
                foreach (JToken item in (JArray)token)
                {
                    if (item.Type == JTokenType.Object)
                    {
                        elements.Add((JObject)item);
                    }
                    else
                    {
                        // Keep the index aligned so the report cites the submitted position.
                        elements.Add(null);
                        errors.Add(new EmitElementError(
                            i,
                            string.Format(
                                CultureInfo.CurrentCulture,
                                "{0} batch element must be a JSON object, but element {1} was a JSON {2}.",
                                Capitalize(payloadKind),
                                i,
                                item.Type.ToString().ToLowerInvariant())));
                    }

                    i++;
                }
            }
            else
            {
                return new EmitReport(
                    0,
                    Array.Empty<EmitElementError>(),
                    payloadError: string.Format(
                        CultureInfo.CurrentCulture,
                        "{0} JSON must be a JSON object or an array of objects, but the parsed payload was a {1}.",
                        Capitalize(payloadKind),
                        token.Type.ToString().ToLowerInvariant()));
            }

            ValidateEmitElement validate = buildValidator();

            for (int index = 0; index < elements.Count; index++)
            {
                JObject element = elements[index];
                if (element == null) { continue; } // already captured as a structural error

                EmitElementError error = validate(element, index, batched);
                if (error != null) { errors.Add(error); }
            }

            // Atomic: any rejection appends nothing, so a retry of the corrected payload never
            // double-appends the elements that were already valid.
            if (errors.Count > 0)
            {
                return new EmitReport(0, errors.OrderBy(e => e.Index).ToArray());
            }

            foreach (JObject element in elements)
            {
                _sink.Append(eventKind, element);
            }

            return new EmitReport(elements.Count, Array.Empty<EmitElementError>());
        }

        private static EmitElementError ValidateResult(JObject result, int index, bool batched)
        {
            JToken ruleIdToken = result["ruleId"];
            if (ruleIdToken != null
                && ruleIdToken.Type != JTokenType.Null
                && ruleIdToken.Type != JTokenType.String)
            {
                // ThrowIfUnacceptable(null) would surface this as "(empty ruleId)" — misleading when
                // the producer supplied a value of the wrong JSON type. Emit a specific message in
                // the AI1012 namespace so the orchestrator can detect and correct.
                return new EmitElementError(
                    index,
                    string.Format(
                        CultureInfo.CurrentCulture,
                        "result.ruleId must be a JSON string, but the payload supplied a JSON {0}. See docs/ai/generating-sarif.md#rule-id-convention.",
                        ruleIdToken.Type.ToString().ToLowerInvariant()),
                    AIRuleIdConventionException.ErrorCode);
            }

            string ruleId = ruleIdToken?.Type == JTokenType.String
                ? ruleIdToken.Value<string>()
                : null;

            try
            {
                AIRuleIdConvention.ThrowIfUnacceptable(ruleId);
            }
            catch (AIRuleIdConventionException ex)
            {
                // The message is already shaped for AI consumption (see
                // AIRuleIdConventionException.BuildMessage) — surface it verbatim.
                return new EmitElementError(index, ex.Message, AIRuleIdConventionException.ErrorCode);
            }

            return null;
        }

        private static EmitElementError ValidateInvocation(JObject invocation, int index, bool batched)
        {
            // The emit verb ingests an already-completed run; it never invents timing. Optional
            // start/end times are left exactly as the producer supplied them (or absent), and any
            // time that is present is held to one invariant: it cannot lie in the future.
            DateTime nowUtc = DateTime.UtcNow;

            string message = ValidateInvocationReceipt(invocation, nowUtc)
                ?? ValidateProvidedTimeNotFuture(invocation["startTimeUtc"], "startTimeUtc", nowUtc)
                ?? ValidateProvidedTimeNotFuture(invocation["endTimeUtc"], "endTimeUtc", nowUtc);

            return message != null ? new EmitElementError(index, message) : null;
        }

        // Receipt gate for the required fields of ai-invocation.schema.json; full structural
        // validation runs at emit-finalize --validate. Returns null when the invocation is
        // acceptable; otherwise a message describing the first violation.
        private static string ValidateInvocationReceipt(JObject invocation, DateTime nowUtc)
        {
            JToken executionSuccessful = invocation["executionSuccessful"];
            if (executionSuccessful == null || executionSuccessful.Type != JTokenType.Boolean)
            {
                return "Invalid invocation: 'executionSuccessful' is required and must be a boolean.";
            }

            JToken commandLine = invocation["commandLine"];
            if (commandLine == null
                || commandLine.Type != JTokenType.String
                || string.IsNullOrWhiteSpace(commandLine.Value<string>()))
            {
                return "Invalid invocation: 'commandLine' is required and must be a non-whitespace string.";
            }

            JToken workingDirectory = invocation["workingDirectory"];
            if (workingDirectory == null || workingDirectory.Type != JTokenType.Object)
            {
                return "Invalid invocation: 'workingDirectory' is required and must be an artifactLocation.";
            }

            // A SARIF artifactLocation is addressable by 'uri' and/or 'uriBaseId'. emit-finalize
            // rebases a repo-root workingDirectory to an empty 'uri' under a 'uriBaseId', so accept
            // either anchor; only a workingDirectory carrying neither is unanchored and rejected.
            var workingDirectoryObject = (JObject)workingDirectory;
            JToken workingDirectoryUri = workingDirectoryObject["uri"];
            JToken workingDirectoryUriBaseId = workingDirectoryObject["uriBaseId"];
            bool hasUri = workingDirectoryUri?.Type == JTokenType.String
                && !string.IsNullOrWhiteSpace(workingDirectoryUri.Value<string>());
            bool hasUriBaseId = workingDirectoryUriBaseId?.Type == JTokenType.String
                && !string.IsNullOrWhiteSpace(workingDirectoryUriBaseId.Value<string>());
            if (!hasUri && !hasUriBaseId)
            {
                return "Invalid invocation: 'workingDirectory' must be an artifactLocation with a non-whitespace 'uri' or 'uriBaseId'.";
            }

            // The AI profile requires timeUtc on every inline notification, and that time — like
            // every producer-supplied time — must not lie in the future.
            return ValidateNotificationTimes(invocation["toolExecutionNotifications"], "toolExecutionNotifications", nowUtc)
                ?? ValidateNotificationTimes(invocation["toolConfigurationNotifications"], "toolConfigurationNotifications", nowUtc);
        }

        private static string ValidateNotificationTimes(JToken notifications, string arrayName, DateTime nowUtc)
        {
            if (notifications == null || notifications.Type == JTokenType.Null) { return null; }

            if (notifications.Type != JTokenType.Array)
            {
                return string.Format(CultureInfo.CurrentCulture, "Invalid invocation: '{0}' must be an array.", arrayName);
            }

            foreach (JToken item in (JArray)notifications)
            {
                JToken timeUtc = (item as JObject)?["timeUtc"];
                if (timeUtc == null
                    || timeUtc.Type != JTokenType.String
                    || string.IsNullOrWhiteSpace(timeUtc.Value<string>()))
                {
                    return string.Format(
                        CultureInfo.CurrentCulture,
                        "Invalid invocation: every '{0}' entry requires a non-whitespace 'timeUtc'.",
                        arrayName);
                }

                if (IsFutureTime(timeUtc, nowUtc))
                {
                    return string.Format(
                        CultureInfo.CurrentCulture,
                        "Invalid invocation: a '{0}' entry carries a 'timeUtc' that lies in the future relative to the emit machine's clock. A logged event cannot occur after the run that records it; supply the actual event time.",
                        arrayName);
                }
            }

            return null;
        }

        // Producer-supplied times are compared against the emit machine's clock with a tolerance for
        // benign cross-machine skew; a value beyond this horizon is an impossible future time for a
        // completed run or a past event, and is rejected rather than recorded.
        private static readonly TimeSpan FutureTimeSkewGrace = TimeSpan.FromMinutes(5);

        private static string ValidateProvidedTimeNotFuture(JToken timeToken, string propertyName, DateTime nowUtc)
        {
            if (!IsFutureTime(timeToken, nowUtc)) { return null; }

            return string.Format(
                CultureInfo.CurrentCulture,
                "Invalid invocation: '{0}' lies in the future relative to the emit machine's clock. A completed run cannot carry a future timestamp; supply the actual time.",
                propertyName);
        }

        private static bool IsFutureTime(JToken timeToken, DateTime nowUtc)
        {
            if (timeToken == null) { return false; }

            DateTime parsed;
            if (timeToken.Type == JTokenType.Date)
            {
                parsed = timeToken.Value<DateTime>();
            }
            else if (timeToken.Type == JTokenType.String
                && DateTime.TryParse(
                    timeToken.Value<string>(),
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal,
                    out parsed))
            {
                // parsed assigned by TryParse.
            }
            else
            {
                // A malformed or non-string time cannot be range-checked here; emit-finalize
                // enforces format conformance against the schema.
                return false;
            }

            return parsed.ToUniversalTime() > nowUtc.Add(FutureTimeSkewGrace);
        }

        private ValidateEmitElement BuildDescriptorValidator(bool isRules, string payloadKind)
        {
            string targetArray = isRules ? "rules" : "notifications";
            string eventKind = isRules
                ? SarifEventKinds.RuleDescriptor
                : SarifEventKinds.NotificationDescriptor;

            // Read the existing-log ids once (O(events)) so a batch does not re-scan the log per
            // element. Track ids seen earlier in this batch so two same-id elements are rejected.
            HashSet<string> existingIds = ReadExistingDescriptorIds(eventKind, targetArray);
            var batchIds = new HashSet<string>(StringComparer.Ordinal);

            return (descriptor, index, batched) =>
            {
                // SARIF §3.49.3 requires a non-empty reportingDescriptor.id string.
                JToken idToken = descriptor["id"];
                if (idToken == null || idToken.Type != JTokenType.String)
                {
                    return new EmitElementError(
                        index,
                        string.Format(
                            CultureInfo.CurrentCulture,
                            "{0} JSON must include a non-empty 'id' string (SARIF §3.49.3). {1}.",
                            Capitalize(payloadKind),
                            idToken == null
                                ? "The payload had no 'id' field"
                                : string.Format(
                                    CultureInfo.CurrentCulture,
                                    "The payload supplied 'id' as JSON {0}",
                                    idToken.Type.ToString().ToLowerInvariant())));
                }

                string id = idToken.Value<string>();
                if (string.IsNullOrEmpty(id))
                {
                    return new EmitElementError(
                        index,
                        string.Format(
                            CultureInfo.CurrentCulture,
                            "{0} JSON 'id' must be a non-empty string (SARIF §3.49.3).",
                            Capitalize(payloadKind)));
                }

                // Rule descriptors are accepted only for flat NOVEL- ids.
                if (isRules && !(AIRuleIdConvention.IsNovel(id) && AIRuleIdConvention.IsAcceptable(id)))
                {
                    return new EmitElementError(
                        index,
                        string.Format(
                            CultureInfo.CurrentCulture,
                            "rule descriptor id '{0}' is not a well-formed NOVEL- id. The add-rule-reporting-descriptors verb is reserved for novel-finding descriptors that have no taxonomy entry; descriptors for taxonomy-mapped rules (e.g., 'CWE-89') come from the taxonomy enricher, not from this verb. Use a NOVEL- escape-hatch id: 'NOVEL-' followed by a lowercase-alphanumeric kebab sub-id (single hyphens, no slash, no trailing hyphen), e.g., 'NOVEL-prompt-injection-via-system-message'. See docs/ai/generating-sarif.md#rule-id-convention.",
                            id),
                        AIRuleIdConventionException.ErrorCode);
                }

                if (existingIds.Contains(id))
                {
                    return new EmitElementError(
                        index,
                        string.Format(
                            CultureInfo.CurrentCulture,
                            "a {0} with id '{1}' is already present in the event log under tool.driver.{2}. Each id may appear at most once per event log. (--force is not yet supported.)",
                            payloadKind,
                            id,
                            targetArray));
                }

                if (!batchIds.Add(id))
                {
                    return new EmitElementError(
                        index,
                        string.Format(
                            CultureInfo.CurrentCulture,
                            "a {0} with id '{1}' appears more than once in this batch. Each id may appear at most once per event log. (--force is not yet supported.)",
                            payloadKind,
                            id));
                }

                return null;
            };
        }

        /// <summary>
        /// Collects every descriptor id already targeting <paramref name="targetArray"/> in the staged
        /// event log, counting both run-header pre-populated descriptors and prior descriptor events.
        /// </summary>
        private HashSet<string> ReadExistingDescriptorIds(string targetKind, string targetArray)
        {
            var ids = new HashSet<string>(StringComparer.Ordinal);

            foreach (SarifEvent evt in _sink.ReadAll())
            {
                if (string.Equals(evt.Kind, SarifEventKinds.RunHeader, StringComparison.Ordinal))
                {
                    JToken driverArray = evt.Payload?["tool"]?["driver"]?[targetArray];
                    if (driverArray is JArray descriptors)
                    {
                        foreach (JToken descriptor in descriptors)
                        {
                            if (descriptor?["id"]?.Type == JTokenType.String)
                            {
                                ids.Add(descriptor["id"].Value<string>());
                            }
                        }
                    }
                }
                else if (string.Equals(evt.Kind, targetKind, StringComparison.Ordinal))
                {
                    if (evt.Payload?["id"]?.Type == JTokenType.String)
                    {
                        ids.Add(evt.Payload["id"].Value<string>());
                    }
                }
            }

            return ids;
        }

        private static string Capitalize(string s)
        {
            if (string.IsNullOrEmpty(s)) { return s; }
            return char.ToUpperInvariant(s[0]) + s.Substring(1);
        }
    }
}
