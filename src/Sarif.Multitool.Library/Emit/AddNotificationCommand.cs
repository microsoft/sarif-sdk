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
    /// Implements <c>multitool add-notification</c>: appends a fully-formed SARIF notification
    /// JSON to <c>&lt;output&gt;.wip.jsonl</c>.
    /// </summary>
    /// <remarks>
    /// <para>Unlike <see cref="AddResultCommand"/>, this verb does not enforce the AI ruleId
    /// convention on the notification's <c>associatedRule.id</c> — that field references a
    /// descriptor in <c>tool.driver.rules</c>, which uses the base taxonomy id (e.g.,
    /// <c>CWE-79</c>) per SARIF §3.49.3, not the result-side hierarchical form.</para>
    /// <para>Notifications without a <c>timeUtc</c> stamp are auto-stamped at replay time
    /// (<see cref="SarifEventReplayer"/>), so producers can omit that field without firing
    /// AI2019 at validate time.</para>
    /// </remarks>
    public class AddNotificationCommand : CommandBase
    {
        public int Run(AddNotificationOptions options, IFileSystem fileSystem = null)
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
                    payloadKind: "notification",
                    fileSystem,
                    out JToken payload);
                if (code != SUCCESS) { return code; }

                string level = payload["level"]?.Type == JTokenType.String
                    ? payload["level"].Value<string>()
                    : "<unset>";

                using (var writer = new SarifEventLogWriter(wipPath))
                {
                    writer.Append(SarifEventKinds.Notification, payload);
                }

                Console.Out.WriteLine(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        "Appended notification (level='{0}') to '{1}'.",
                        level,
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
