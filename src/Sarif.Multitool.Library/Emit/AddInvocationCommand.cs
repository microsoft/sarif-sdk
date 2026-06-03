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
    /// <c>toolExecutionNotifications</c> / <c>toolConfigurationNotifications</c> arrays. There is
    /// no separate <c>add-notification</c> verb because SARIF has no run-level notifications
    /// array, so a notification authored on its own could not be routed to one of several
    /// parallel invocations; the producer instead holds per-process state and emits ONE complete
    /// invocation when that process finishes.</para>
    /// <para>The verb enforces the load-bearing requireds of the AI invocation profile
    /// (<c>ai-invocation.schema.json</c>): the payload must be a JSON object carrying a boolean
    /// <c>executionSuccessful</c>, a non-whitespace string <c>commandLine</c>, and a
    /// <c>workingDirectory</c> artifactLocation with a non-whitespace <c>uri</c>. Richer
    /// structural validation is deferred to <c>emit-finalize --validate</c>, which validates the
    /// assembled log.</para>
    /// <para>At emit time the verb stamps a single wall-clock <c>now</c> onto any fields the
    /// producer left unset: the invocation's <c>endTimeUtc</c>, and the <c>timeUtc</c> of every
    /// inline notification (cf. AI2019). Producer-supplied values are preserved, so replay is
    /// fully deterministic.</para>
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
                StampWallClock((JObject)payload, now);

                bool executionSuccessful = payload["executionSuccessful"].Value<bool>();

                using (var writer = new SarifEventLogWriter(wipPath))
                {
                    writer.Append(SarifEventKinds.Invocation, payload);
                }

                Console.Out.WriteLine(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        "Appended invocation (executionSuccessful='{0}') to '{1}'.",
                        executionSuccessful.ToString().ToLowerInvariant(),
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

            return true;
        }

        // Fills wall-clock fields the producer left unset, using a single 'now' so endTimeUtc and
        // every inline notification's timeUtc agree. Producer-supplied values are preserved.
        private static void StampWallClock(JObject payload, string now)
        {
            if (payload["endTimeUtc"] == null || payload["endTimeUtc"].Type == JTokenType.Null)
            {
                payload["endTimeUtc"] = now;
            }

            StampNotificationTimes(payload["toolExecutionNotifications"] as JArray, now);
            StampNotificationTimes(payload["toolConfigurationNotifications"] as JArray, now);
        }

        private static void StampNotificationTimes(JArray notifications, string now)
        {
            if (notifications == null) { return; }

            foreach (JToken item in notifications)
            {
                if (item is JObject notification
                    && (notification["timeUtc"] == null || notification["timeUtc"].Type == JTokenType.Null))
                {
                    notification["timeUtc"] = now;
                }
            }
        }
    }
}
