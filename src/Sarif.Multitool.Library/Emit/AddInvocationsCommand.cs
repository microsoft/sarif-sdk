// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;

using Microsoft.CodeAnalysis.Sarif.Driver;
using Microsoft.CodeAnalysis.Sarif.Emit;

using Newtonsoft.Json.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    /// <summary>
    /// Implements <c>add-invocations</c>: validates one or more fully-formed SARIF invocations and
    /// appends an <c>invocation</c> event per element to <c>&lt;output&gt;.wip.jsonl</c>.
    /// </summary>
    /// <remarks>
    /// <para>The payload is a single invocation object or an array of invocation objects. Each is
    /// gated for the required AI invocation fields: <c>executionSuccessful</c>, <c>commandLine</c>,
    /// an anchored <c>workingDirectory</c> (a <c>uri</c> and/or <c>uriBaseId</c>), and inline
    /// notification <c>timeUtc</c> values. Full structural validation runs at
    /// <c>emit-finalize --validate</c>. The batch is atomic: if any element is rejected, nothing is
    /// appended and the rejected indices are reported.</para>
    /// <para><c>endTimeUtc</c> handling is the one place single and batch submission diverge. A lone
    /// object that omits <c>endTimeUtc</c> is stamped with receipt time, since the write is roughly
    /// coincident with the invocation's conclusion. A batch is assembled after the fact — one write
    /// instant cannot stand in for many invocations that ended at different times — so every batched
    /// invocation must carry its own <c>endTimeUtc</c>.</para>
    /// </remarks>
    public class AddInvocationsCommand : CommandBase
    {
        public int Run(AddInvocationsOptions options, IFileSystem fileSystem = null)
        {
            return EmitBatchProcessor.Run(
                options?.OutputFilePath,
                options?.InputFilePath,
                payloadKind: "invocation",
                eventKind: SarifEventKinds.Invocation,
                fileSystem,
                buildValidator: _ => ValidateInvocation);
        }

        private static BatchElementError ValidateInvocation(JObject invocation, int index, bool batched)
        {
            string message = ValidateInvocationReceipt(invocation);
            if (message != null) { return new BatchElementError(index, message); }

            JToken endTimeUtc = invocation["endTimeUtc"];
            bool hasEndTimeUtc = endTimeUtc != null && endTimeUtc.Type != JTokenType.Null;
            if (!hasEndTimeUtc)
            {
                if (batched)
                {
                    return new BatchElementError(
                        index,
                        "Invalid invocation: 'endTimeUtc' is required when submitting invocations as a batch. The receipt-time default applies only to single (one-object) submission, where the write is roughly coincident with the invocation's conclusion; a batch is assembled after the fact, so each invocation must carry its own 'endTimeUtc'.");
                }

                StampEndTimeUtc(invocation, DateTime.UtcNow);
            }

            return null;
        }

        // Receipt gate for the required fields of ai-invocation.schema.json; full structural
        // validation runs at emit-finalize --validate. Returns null when the invocation is
        // acceptable; otherwise a message describing the first violation.
        private static string ValidateInvocationReceipt(JObject invocation)
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

            // The AI profile requires timeUtc on every inline notification.
            return ValidateNotificationTimes(invocation["toolExecutionNotifications"], "toolExecutionNotifications")
                ?? ValidateNotificationTimes(invocation["toolConfigurationNotifications"], "toolConfigurationNotifications");
        }

        private static string ValidateNotificationTimes(JToken notifications, string arrayName)
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
            }

            return null;
        }

        private static void StampEndTimeUtc(JObject invocation, DateTime endTimeUtc)
        {
            invocation["endTimeUtc"] = endTimeUtc.ToString(
                SarifUtilities.SarifDateTimeFormatMillisecondsPrecision,
                CultureInfo.InvariantCulture);
        }
    }
}
