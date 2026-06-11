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
    /// Implements <c>add-invocation</c>: appends a fully-formed SARIF invocation
    /// JSON to <c>&lt;output&gt;.wip.jsonl</c>.
    /// </summary>
    /// <remarks>
    /// <para>The verb gates required AI invocation fields: <c>executionSuccessful</c>,
    /// <c>commandLine</c>, an anchored <c>workingDirectory</c> (a <c>uri</c> and/or
    /// <c>uriBaseId</c>), and inline notification <c>timeUtc</c>
    /// values. Full structural validation runs at <c>emit-finalize --validate</c>.</para>
    /// <para>The verb stamps <c>endTimeUtc</c> with the time of receipt when the producer leaves it unset.</para>
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

                StampEndTimeUtcIfOmitted((JObject)payload, DateTime.UtcNow);

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
            if (workingDirectory == null || workingDirectory.Type != JTokenType.Object)
            {
                Console.Error.WriteLine(
                    "Invalid invocation: 'workingDirectory' is required and must be an artifactLocation.");
                return false;
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
                Console.Error.WriteLine(
                    "Invalid invocation: 'workingDirectory' must be an artifactLocation with a non-whitespace 'uri' or 'uriBaseId'.");
                return false;
            }

            // The AI profile requires timeUtc on every inline notification.
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

        private static void StampEndTimeUtcIfOmitted(JObject payload, DateTime endTimeUtc)
        {
            if (payload["endTimeUtc"] == null || payload["endTimeUtc"].Type == JTokenType.Null)
            {
                payload["endTimeUtc"] = endTimeUtc.ToString(
                    SarifUtilities.SarifDateTimeFormatMillisecondsPrecision,
                    CultureInfo.InvariantCulture);
            }
        }
    }
}
