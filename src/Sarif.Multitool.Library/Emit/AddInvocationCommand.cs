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
    /// <para>The verb performs no schema validation on the invocation payload beyond "must be
    /// a JSON object" — SARIF §3.20 makes every field on <c>Invocation</c> optional, and AI
    /// producers vary widely in which fields they have meaningful values for (a daemon may
    /// know its <c>startTimeUtc</c> but not its <c>exitCode</c>; a one-shot scanner may know
    /// both). Full-log validation belongs in <c>emit-finalize --validate</c>, not at receipt.</para>
    /// <para>Invocations are replayed in event order to <c>run.invocations[]</c>. Subsequent
    /// <c>execution-notification</c> and <c>configuration-notification</c> events attach to
    /// the most recent invocation, so emitting a fresh invocation event MAY be used to start
    /// a new notification group within the same scan.</para>
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

                string executionSuccessful = payload["executionSuccessful"]?.Type == JTokenType.Boolean
                    ? payload["executionSuccessful"].Value<bool>().ToString().ToLowerInvariant()
                    : "<unset>";

                using (var writer = new SarifEventLogWriter(wipPath))
                {
                    writer.Append(SarifEventKinds.Invocation, payload);
                }

                Console.Out.WriteLine(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        "Appended invocation (executionSuccessful='{0}') to '{1}'.",
                        executionSuccessful,
                        wipPath));
                return SUCCESS;
            }
            catch (Exception ex) when (!Debugger.IsAttached)
            {
                Console.Error.WriteLine(ex);
                return FAILURE;
            }
        }
    }
}
