// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Globalization;

using Microsoft.CodeAnalysis.Sarif.Driver;
using Microsoft.CodeAnalysis.Sarif.Emit;

using Newtonsoft.Json.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    /// <summary>
    /// Implements <c>multitool add-reporting-descriptor</c>: validates a fully-formed SARIF
    /// reportingDescriptor JSON and appends an event to <c>&lt;output&gt;.wip.jsonl</c>.
    /// </summary>
    /// <remarks>
    /// <para>Default target is <c>run.tool.driver.notifications[]</c>; pass <c>--rules</c> to
    /// target <c>run.tool.driver.rules[]</c>.</para>
    /// <para>On the <c>--rules</c> path, the descriptor id must be a well-formed NOVEL- ruleId.
    /// Taxonomy-mapped rule descriptors (e.g., <c>CWE-89</c>) come from the taxonomy enricher.</para>
    /// <para>Duplicate ids in the same target array are rejected before append.</para>
    /// </remarks>
    public class AddReportingDescriptorCommand : CommandBase
    {
        public int Run(AddReportingDescriptorOptions options, IFileSystem fileSystem = null)
        {
            fileSystem ??= Sarif.FileSystem.Instance;

            try
            {
                bool isRules = options?.Rules == true;
                string payloadKind = isRules ? "rule descriptor" : "notification descriptor";
                string targetArray = isRules ? "rules" : "notifications";
                string eventKind = isRules
                    ? SarifEventKinds.RuleDescriptor
                    : SarifEventKinds.NotificationDescriptor;

                int code = EmitEventLogHelpers.TryResolveWipPath(
                    options?.OutputFilePath,
                    fileSystem,
                    out string wipPath);
                if (code != SUCCESS) { return code; }

                code = EmitEventLogHelpers.TryReadJsonPayload(
                    options?.InputFilePath,
                    payloadKind: payloadKind,
                    fileSystem,
                    out JToken payload);
                if (code != SUCCESS) { return code; }

                // SARIF §3.49.3 requires a non-empty reportingDescriptor.id string.
                JToken idToken = payload["id"];
                if (idToken == null
                    || idToken.Type == JTokenType.Null
                    || idToken.Type != JTokenType.String)
                {
                    Console.Error.WriteLine(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            "error: {0} JSON must include a non-empty 'id' string (SARIF §3.49.3). {1}.",
                            Capitalize(payloadKind),
                            idToken == null
                                ? "The payload had no 'id' field"
                                : string.Format(
                                    CultureInfo.CurrentCulture,
                                    "The payload supplied 'id' as JSON {0}",
                                    idToken.Type.ToString().ToLowerInvariant())));
                    return FAILURE;
                }

                string id = idToken.Value<string>();
                if (string.IsNullOrEmpty(id))
                {
                    Console.Error.WriteLine(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            "error: {0} JSON 'id' must be a non-empty string (SARIF §3.49.3).",
                            Capitalize(payloadKind)));
                    return FAILURE;
                }

                // Rule descriptors are accepted only for flat NOVEL- ids.
                if (isRules && !(AIRuleIdConvention.IsNovel(id) && AIRuleIdConvention.IsAcceptable(id)))
                {
                    Console.Error.WriteLine(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            "error {0}: rule descriptor id '{1}' is not a well-formed NOVEL- id. The --rules path is reserved for novel-finding descriptors that have no taxonomy entry; descriptors for taxonomy-mapped rules (e.g., 'CWE-89') come from the taxonomy enricher, not from this verb. Use a NOVEL- escape-hatch id: 'NOVEL-' followed by a lowercase-alphanumeric kebab sub-id (single hyphens, no slash, no trailing hyphen), e.g., 'NOVEL-prompt-injection-via-system-message'. See docs/AI-RuleId-Convention.md.",
                            AIRuleIdConventionException.ErrorCode,
                            id));
                    return FAILURE;
                }

                // Reject duplicates against prior descriptor events and run-header descriptors.
                if (TryFindDuplicate(wipPath, id, eventKind, targetArray, out string duplicateError))
                {
                    Console.Error.WriteLine(duplicateError);
                    return FAILURE;
                }

                using (var writer = new SarifEventLogWriter(wipPath))
                {
                    writer.Append(eventKind, payload);
                }

                Console.Out.WriteLine(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        "Appended {0} (id='{1}') to '{2}'.",
                        payloadKind,
                        id,
                        wipPath));
                return SUCCESS;
            }
            catch (Exception ex) when (!Debugger.IsAttached)
            {
                Console.Error.WriteLine(ex);
                return FAILURE;
            }
        }

        /// <summary>
        /// Scans the staged event log for a prior descriptor with the same id targeting the
        /// same array. Returns <c>true</c> with <paramref name="error"/> populated when a
        /// duplicate is found; <c>false</c> otherwise.
        /// </summary>
        /// <remarks>
        /// Checks run-header descriptors and prior descriptor events of the same target kind.
        /// The event index in the error matches the event's position in the staged log.
        /// </remarks>
        private static bool TryFindDuplicate(
            string wipPath,
            string id,
            string targetKind,
            string targetArray,
            out string error)
        {
            string descriptorLabel = string.Equals(targetKind, SarifEventKinds.RuleDescriptor, StringComparison.Ordinal)
                ? "rule descriptor"
                : "notification descriptor";

            var reader = new SarifEventLogReader();
            int eventIndex = 0;
            foreach (SarifEvent evt in reader.Read(wipPath))
            {
                eventIndex++;

                if (string.Equals(evt.Kind, SarifEventKinds.RunHeader, StringComparison.Ordinal))
                {
                    JToken driverArray = evt.Payload?["tool"]?["driver"]?[targetArray];
                    if (driverArray is JArray descriptors)
                    {
                        foreach (JToken descriptor in descriptors)
                        {
                            if (descriptor?["id"]?.Type == JTokenType.String
                                && string.Equals(descriptor["id"].Value<string>(), id, StringComparison.Ordinal))
                            {
                                error = string.Format(
                                    CultureInfo.CurrentCulture,
                                    "error: a {0} with id '{1}' was already pre-populated on the run-header (event #{2}) under tool.driver.{3}. Each id may appear at most once per event log. (--force is not yet supported.)",
                                    descriptorLabel,
                                    id,
                                    eventIndex,
                                    targetArray);
                                return true;
                            }
                        }
                    }
                }
                else if (string.Equals(evt.Kind, targetKind, StringComparison.Ordinal))
                {
                    if (evt.Payload?["id"]?.Type == JTokenType.String
                        && string.Equals(evt.Payload["id"].Value<string>(), id, StringComparison.Ordinal))
                    {
                        error = string.Format(
                            CultureInfo.CurrentCulture,
                            "error: a {0} with id '{1}' was already appended to the event log (event #{2}). Each id may appear at most once per event log. (--force is not yet supported.)",
                            descriptorLabel,
                            id,
                            eventIndex);
                        return true;
                    }
                }
            }

            error = null;
            return false;
        }

        private static string Capitalize(string s)
        {
            if (string.IsNullOrEmpty(s)) { return s; }
            return char.ToUpperInvariant(s[0]) + s.Substring(1);
        }
    }
}
