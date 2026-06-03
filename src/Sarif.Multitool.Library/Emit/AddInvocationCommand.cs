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
    /// Implements <c>multitool add-invocation</c>: appends a fully-formed SARIF invocation
    /// JSON to <c>&lt;output&gt;.wip.jsonl</c>.
    /// </summary>
    /// <remarks>
    /// <para>An invocation models one process the producer launched and is the SOLE carrier of
    /// notifications: the producer supplies them INLINE on the payload's
    /// <c>toolExecutionNotifications</c> / <c>toolConfigurationNotifications</c> arrays. A
    /// notification is bound to the invocation that owns it because SARIF has no run-level
    /// notifications array, and a free-standing notification could not be routed to one of
    /// several parallel invocations; the producer holds per-process state and emits ONE complete
    /// invocation (notifications inline) when that process finishes.</para>
    /// <para>The verb enforces the load-bearing requireds of the AI invocation profile
    /// (<c>ai-invocation.schema.json</c>): the payload must be a JSON object carrying a boolean
    /// <c>executionSuccessful</c>, a non-whitespace string <c>commandLine</c>, and a
    /// <c>workingDirectory</c> artifactLocation with a non-whitespace <c>uri</c>. Richer
    /// structural validation is deferred to <c>emit-finalize --validate</c>, which validates the
    /// assembled log.</para>
    /// <para>At emit time the verb stamps a single wall-clock <c>now</c> onto the invocation's
    /// <c>endTimeUtc</c> when the producer left it unset (time of receipt is a faithful proxy
    /// for process completion). Notification <c>timeUtc</c> values are NOT stamped: a
    /// notification's timestamp records when that event occurred mid-flight, so the producer
    /// must supply it as it builds the invocation. The AI profile therefore REQUIRES a
    /// <c>timeUtc</c> on every inline notification (cf. AI2019). Producer-supplied values are
    /// preserved, so replay is fully deterministic.</para>
    /// </remarks>
    public class AddInvocationCommand : CommandBase
    {
        public int Run(AddInvocationOptions options, IFileSystem fileSystem = null)
        {
            fileSystem ??= Sarif.FileSystem.Instance;

            try
            {
                int code = EmitEventLogHelpers.TryResolveWipPath(
                    options?.OutputFilePath,
                    fileSystem,
                    out string wipPath);
                if (code != SUCCESS) { return code; }

                code = EmitEventLogHelpers.TryReadJsonPayload(
                    options?.InputFilePath,
                    payloadKind: "invocation",
                    fileSystem,
                    out JToken payload);
                if (code != SUCCESS) { return code; }

                if (!TryValidateInvocationReceipt((JObject)payload))
                {
                    return FAILURE;
                }

                string now = DateTime.UtcNow.ToString(
                    SarifUtilities.SarifDateTimeFormatMillisecondsPrecision,
                    CultureInfo.InvariantCulture);
                StampEndTimeUtcIfOmitted((JObject)payload, now);

                bool executionSuccessful = payload["executionSuccessful"].Value<bool>();

                using (var writer = new SarifEventLogWriter(wipPath))
                {
                    writer.Append(SarifEventKinds.Invocation, payload);
                }

                Console.Out.WriteLine(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        "Appended invocation (executionSuccessful='{0}') to '{1}'.",
                        executionSuccessful ? "true" : "false",
                        wipPath));
                return SUCCESS;
            }
            catch (Exception ex) when (!Debugger.IsAttached)
            {
                Console.Error.WriteLine(ex);
                return FAILURE;
            }
        }

        // Mirrors the load-bearing requireds of ai-invocation.schema.json. Full structural
        // validation is deferred to emit-finalize --validate; this is a cheap receipt gate that
        // rejects the three fields the AI profile makes mandatory.
        private static bool TryValidateInvocationReceipt(JObject payload)
        {
            JToken executionSuccessful = payload["executionSuccessful"];
            if (executionSuccessful == null || executionSuccessful.Type != JTokenType.Boolean)
            {
                Console.Error.WriteLine(
                    "Invalid invocation: 'executionSuccessful' is required and must be a boolean.");
                return false;
            }

            JToken commandLine = payload["commandLine"];
            if (commandLine == null
                || commandLine.Type != JTokenType.String
                || string.IsNullOrWhiteSpace(commandLine.Value<string>()))
            {
                Console.Error.WriteLine(
                    "Invalid invocation: 'commandLine' is required and must be a non-whitespace string.");
                return false;
            }

            JToken workingDirectory = payload["workingDirectory"];
            JToken workingDirectoryUri = (workingDirectory as JObject)?["uri"];
            if (workingDirectory == null
                || workingDirectory.Type != JTokenType.Object
                || workingDirectoryUri == null
                || workingDirectoryUri.Type != JTokenType.String
                || string.IsNullOrWhiteSpace(workingDirectoryUri.Value<string>()))
            {
                Console.Error.WriteLine(
                    "Invalid invocation: 'workingDirectory' is required and must be an artifactLocation with a non-whitespace 'uri'.");
                return false;
            }

            // Inline notifications are the SOLE carrier of notifications, and each records when an
            // event occurred mid-flight. The producer must supply that 'timeUtc' (the verb does not
            // stamp it), so the AI profile requires a non-whitespace timeUtc on every notification.
            if (!TryValidateNotificationTimes(payload["toolExecutionNotifications"], "toolExecutionNotifications")
                || !TryValidateNotificationTimes(payload["toolConfigurationNotifications"], "toolConfigurationNotifications"))
            {
                return false;
            }

            return true;
        }

        private static bool TryValidateNotificationTimes(JToken notifications, string arrayName)
        {
            if (notifications == null || notifications.Type == JTokenType.Null) { return true; }

            if (notifications.Type != JTokenType.Array)
            {
                Console.Error.WriteLine(
                    string.Format(CultureInfo.CurrentCulture, "Invalid invocation: '{0}' must be an array.", arrayName));
                return false;
            }

            foreach (JToken item in (JArray)notifications)
            {
                JToken timeUtc = (item as JObject)?["timeUtc"];
                if (timeUtc == null
                    || timeUtc.Type != JTokenType.String
                    || string.IsNullOrWhiteSpace(timeUtc.Value<string>()))
                {
                    Console.Error.WriteLine(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            "Invalid invocation: every '{0}' entry requires a non-whitespace 'timeUtc' (the producer records when the event occurred; the verb does not stamp it).",
                            arrayName));
                    return false;
                }
            }

            return true;
        }

        // Fills the invocation's endTimeUtc when the producer left it unset; time of receipt is a
        // faithful proxy for process completion. Notification timeUtc values are producer-owned and
        // are never stamped here (see TryValidateNotificationTimes). Supplied values are preserved.
        private static void StampEndTimeUtcIfOmitted(JObject payload, string now)
        {
            if (payload["endTimeUtc"] == null || payload["endTimeUtc"].Type == JTokenType.Null)
            {
                payload["endTimeUtc"] = now;
            }
        }
    }
}
