// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;

using Microsoft.CodeAnalysis.Sarif.Driver;
using Microsoft.CodeAnalysis.Sarif.Emit;

using Newtonsoft.Json.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    /// <summary>
    /// Shared implementation behind <c>add-notification-reporting-descriptors</c> and
    /// <c>add-rule-reporting-descriptors</c>: validates one or more SARIF reportingDescriptor
    /// objects and appends an event per element to <c>&lt;output&gt;.wip.jsonl</c>.
    /// </summary>
    /// <remarks>
    /// Notifications append to <c>run.tool.driver.notifications[]</c>; rules append to
    /// <c>run.tool.driver.rules[]</c> and require a well-formed <c>NOVEL-</c> id. Each id may appear
    /// at most once in its target array, counting both descriptors already in the event log and the
    /// other elements of the submitted batch.
    /// </remarks>
    internal static class ReportingDescriptorEmitter
    {
        internal static int Append(
            string outputFilePath,
            string inputFilePath,
            bool isRules,
            IFileSystem fileSystem = null)
        {
            string payloadKind = isRules ? "rule descriptor" : "notification descriptor";
            string eventKind = isRules
                ? SarifEventKinds.RuleDescriptor
                : SarifEventKinds.NotificationDescriptor;

            return EmitBatchProcessor.Run(
                outputFilePath,
                inputFilePath,
                payloadKind,
                eventKind,
                fileSystem,
                buildValidator: wipPath => BuildValidator(wipPath, isRules, payloadKind));
        }

        private static ValidateBatchElement BuildValidator(string wipPath, bool isRules, string payloadKind)
        {
            string targetArray = isRules ? "rules" : "notifications";
            string eventKind = isRules
                ? SarifEventKinds.RuleDescriptor
                : SarifEventKinds.NotificationDescriptor;

            // Read the existing-log ids once (O(events)) so a batch does not re-scan the log per
            // element. Track ids seen earlier in this batch so two same-id elements are rejected.
            HashSet<string> existingIds = ReadExistingDescriptorIds(wipPath, eventKind, targetArray);
            var batchIds = new HashSet<string>(StringComparer.Ordinal);

            return (descriptor, index, batched) =>
            {
                // SARIF §3.49.3 requires a non-empty reportingDescriptor.id string.
                JToken idToken = descriptor["id"];
                if (idToken == null || idToken.Type != JTokenType.String)
                {
                    return new BatchElementError(
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
                    return new BatchElementError(
                        index,
                        string.Format(
                            CultureInfo.CurrentCulture,
                            "{0} JSON 'id' must be a non-empty string (SARIF §3.49.3).",
                            Capitalize(payloadKind)));
                }

                // Rule descriptors are accepted only for flat NOVEL- ids.
                if (isRules && !(AIRuleIdConvention.IsNovel(id) && AIRuleIdConvention.IsAcceptable(id)))
                {
                    return new BatchElementError(
                        index,
                        string.Format(
                            CultureInfo.CurrentCulture,
                            "rule descriptor id '{0}' is not a well-formed NOVEL- id. The add-rule-reporting-descriptors verb is reserved for novel-finding descriptors that have no taxonomy entry; descriptors for taxonomy-mapped rules (e.g., 'CWE-89') come from the taxonomy enricher, not from this verb. Use a NOVEL- escape-hatch id: 'NOVEL-' followed by a lowercase-alphanumeric kebab sub-id (single hyphens, no slash, no trailing hyphen), e.g., 'NOVEL-prompt-injection-via-system-message'. See docs/ai/generating-sarif.md#rule-id-convention.",
                            id),
                        AIRuleIdConventionException.ErrorCode);
                }

                if (existingIds.Contains(id))
                {
                    return new BatchElementError(
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
                    return new BatchElementError(
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
        /// Collects every descriptor id already targeting <paramref name="targetArray"/> in the
        /// staged event log, counting both run-header pre-populated descriptors and prior descriptor
        /// events.
        /// </summary>
        private static HashSet<string> ReadExistingDescriptorIds(string wipPath, string targetKind, string targetArray)
        {
            var ids = new HashSet<string>(StringComparer.Ordinal);

            var reader = new SarifEventLogReader();
            foreach (SarifEvent evt in reader.Read(wipPath))
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
