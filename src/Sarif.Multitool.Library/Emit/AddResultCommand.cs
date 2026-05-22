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
    /// Implements <c>multitool add-result</c>: validates a fully-formed SARIF result JSON and
    /// appends a <c>result</c> event to <c>&lt;output&gt;.wip.jsonl</c>.
    /// </summary>
    /// <remarks>
    /// The result's <c>ruleId</c> is validated at receipt against the AI ruleId convention
    /// (taxonomy sub-id form or NOVEL- escape hatch). On rejection the verb writes the
    /// AI-consumable error envelope (error code AI-RULEID-001) to stderr and returns
    /// <see cref="CommandBase.FAILURE"/> WITHOUT appending — an AI orchestrator can retry the
    /// individual result without first having to remove garbage from the event log.
    /// </remarks>
    public class AddResultCommand : CommandBase
    {
        public int Run(AddResultOptions options, IFileSystem fileSystem = null)
        {
            fileSystem ??= Sarif.FileSystem.Instance;

            try
            {
                int code = AddEventCommandHelpers.TryResolveWipPath(
                    options?.OutputFilePath,
                    fileSystem,
                    out string wipPath);
                if (code != SUCCESS) { return code; }

                code = AddEventCommandHelpers.TryReadJsonPayload(
                    options?.InputFilePath,
                    payloadKind: "result",
                    fileSystem,
                    out JToken payload);
                if (code != SUCCESS) { return code; }

                string ruleId = payload["ruleId"]?.Type == JTokenType.String
                    ? payload["ruleId"].Value<string>()
                    : null;

                AIRuleIdConvention.ThrowIfUnacceptable(ruleId);

                using (var writer = new SarifEventLogWriter(wipPath))
                {
                    writer.Append(SarifEventKinds.Result, payload);
                }

                Console.Out.WriteLine(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        "Appended result (ruleId='{0}') to '{1}'.",
                        ruleId,
                        wipPath));
                return SUCCESS;
            }
            catch (AIRuleIdConventionException ex)
            {
                // Mirror EmitFinalizeCommand: the message is already shaped for AI consumption
                // (see AIRuleIdConventionException.BuildMessage) — write it verbatim, no stack
                // trace, so the orchestrator can fix the offender and retry.
                Console.Error.WriteLine(ex.Message);
                return FAILURE;
            }
            catch (Exception ex) when (!Debugger.IsAttached)
            {
                Console.Error.WriteLine(ex);
                return FAILURE;
            }
        }
    }
}
