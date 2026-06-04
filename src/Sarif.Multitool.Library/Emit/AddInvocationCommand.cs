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
    /// <para>An invocation models one launched process. Its notifications travel inline on the
    /// payload's <c>toolExecutionNotifications</c> / <c>toolConfigurationNotifications</c> arrays;
    /// the producer holds per-process state and emits one complete invocation when that process
    /// finishes.</para>
    /// <para>The verb enforces the required fields of the AI invocation profile
    /// (<c>ai-invocation.schema.json</c>): a boolean <c>executionSuccessful</c>, a non-whitespace
    /// <c>commandLine</c>, and a <c>workingDirectory</c> artifactLocation with a non-whitespace
    /// <c>uri</c>. Full structural validation runs at <c>emit-finalize --validate</c>.</para>
    /// <para>The verb stamps <c>endTimeUtc</c> with the time of receipt when the producer leaves it
    /// unset. The producer supplies each notification's <c>timeUtc</c> and the verb preserves it;
    /// the AI profile requires a <c>timeUtc</c> on every inline notification (AI2019).</para>
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

        // Receipt gate for the required fields of ai-invocation.schema.json; full structural
        // validation runs at emit-finalize --validate.
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

            // The AI profile requires a non-whitespace timeUtc on every inline notification; the
            // producer supplies it and the verb preserves it.
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
                                "Invalid invocation: every '{0}' entry requires a non-whitespace 'timeUtc'.",
                            arrayName));
                    return false;
                }
            }

            return true;
        }

        // Fills endTimeUtc with the time of receipt when the producer left it unset.
        private static void StampEndTimeUtcIfOmitted(JObject payload, string now)
        {
            if (payload["endTimeUtc"] == null || payload["endTimeUtc"].Type == JTokenType.Null)
            {
                payload["endTimeUtc"] = now;
            }
        }
    }
}
