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
    /// Shared implementation behind <c>add-notification-reporting-descriptor</c> and
    /// <c>add-rule-reporting-descriptor</c>: validates a SARIF reportingDescriptor JSON and
    /// appends an event to <c>&lt;output&gt;.wip.jsonl</c>.
    /// </summary>
    /// <remarks>
    /// Notifications append to <c>run.tool.driver.notifications[]</c>; rules append to
    /// <c>run.tool.driver.rules[]</c> and require a well-formed <c>NOVEL-</c> id. Each id may
    /// appear at most once in its target array.
    /// </remarks>
    internal static class ReportingDescriptorEmitter
    {
        internal static int Append(
            string outputFilePath,
            string inputFilePath,
            bool isRules,
            IFileSystem fileSystem = null)
        {
            fileSystem ??= Sarif.FileSystem.Instance;

            try
            {
                string payloadKind = isRules ? "rule descriptor" : "notification descriptor";
                string targetArray = isRules ? "rules" : "notifications";
                string eventKind = isRules
                    ? SarifEventKinds.RuleDescriptor
                    : SarifEventKinds.NotificationDescriptor;

                int code = EmitEventLogHelpers.TryResolveWipPath(
                    outputFilePath,
                    fileSystem,
                    out string wipPath);
                if (code != CommandBase.SUCCESS) { return code; }

                code = EmitEventLogHelpers.TryReadJsonPayload(
                    inputFilePath,
                    payloadKind: payloadKind,
                    fileSystem,
                    out JToken payload);
                if (code != CommandBase.SUCCESS) { return code; }

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
                    return CommandBase.FAILURE;
                }

                string id = idToken.Value<string>();
                if (string.IsNullOrEmpty(id))
                {
                    Console.Error.WriteLine(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            "error: {0} JSON 'id' must be a non-empty string (SARIF §3.49.3).",
                            Capitalize(payloadKind)));
                    return CommandBase.FAILURE;
                }

                // Rule descriptors are accepted only for flat NOVEL- ids.
                if (isRules && !(AIRuleIdConvention.IsNovel(id) && AIRuleIdConvention.IsAcceptable(id)))
                {
                    Console.Error.WriteLine(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            "error {0}: rule descriptor id '{1}' is not a well-formed NOVEL- id. The add-rule-reporting-descriptor verb is reserved for novel-finding descriptors that have no taxonomy entry; descriptors for taxonomy-mapped rules (e.g., 'CWE-89') come from the taxonomy enricher, not from this verb. Use a NOVEL- escape-hatch id: 'NOVEL-' followed by a lowercase-alphanumeric kebab sub-id (single hyphens, no slash, no trailing hyphen), e.g., 'NOVEL-prompt-injection-via-system-message'. See docs/ai/generating-sarif.md#rule-id-convention.",
                            AIRuleIdConventionException.ErrorCode,
                            id));
                    return CommandBase.FAILURE;
                }

                // Reject duplicates against prior descriptor events and run-header descriptors.
                if (TryFindDuplicate(wipPath, id, eventKind, targetArray, out string duplicateError))
                {
                    Console.Error.WriteLine(duplicateError);
                    return CommandBase.FAILURE;
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
                return CommandBase.SUCCESS;
            }
            catch (Exception ex) when (!Debugger.IsAttached)
            {
                Console.Error.WriteLine(ex);
                return CommandBase.FAILURE;
            }
        }

        /// <summary>
        /// Scans the staged event log for a prior descriptor with the same id targeting the
        /// same array. Returns <c>true</c> with <paramref name="error"/> populated when a
        /// duplicate is found; <c>false</c> otherwise.
        /// </summary>
        /// <remarks>
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
