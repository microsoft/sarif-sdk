// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;

using Microsoft.CodeAnalysis.Sarif.Driver;
using Microsoft.CodeAnalysis.Sarif.Emit;

using Newtonsoft.Json.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    /// <summary>
    /// Implements <c>add-results</c>: validates one or more fully-formed SARIF results and appends
    /// a <c>result</c> event per element to <c>&lt;output&gt;.wip.jsonl</c>.
    /// </summary>
    /// <remarks>
    /// The payload is a single result object or an array of result objects. Each result's
    /// <c>ruleId</c> is validated at receipt against the AI ruleId convention (taxonomy sub-id form
    /// or NOVEL- escape hatch). The batch is atomic: if any element is rejected, nothing is appended
    /// and the rejected indices are reported (error code AI1012), so an AI orchestrator can correct
    /// the offenders and retry without first removing partial state from the event log.
    /// </remarks>
    public class AddResultsCommand : CommandBase
    {
        public int Run(AddResultsOptions options, IFileSystem fileSystem = null)
        {
            return EmitBatchProcessor.Run(
                options?.OutputFilePath,
                options?.InputFilePath,
                payloadKind: "result",
                eventKind: SarifEventKinds.Result,
                fileSystem,
                buildValidator: _ => ValidateResult);
        }

        private static BatchElementError ValidateResult(JObject result, int index, bool batched)
        {
            JToken ruleIdToken = result["ruleId"];
            if (ruleIdToken != null
                && ruleIdToken.Type != JTokenType.Null
                && ruleIdToken.Type != JTokenType.String)
            {
                // ThrowIfUnacceptable(null) would surface this as "(empty ruleId)" — misleading when
                // the producer supplied a value of the wrong JSON type. Emit a specific message in
                // the AI1012 namespace so the orchestrator can detect and correct.
                return new BatchElementError(
                    index,
                    string.Format(
                        CultureInfo.CurrentCulture,
                        "result.ruleId must be a JSON string, but the payload supplied a JSON {0}. See docs/ai/generating-sarif.md#rule-id-convention.",
                        ruleIdToken.Type.ToString().ToLowerInvariant()),
                    AIRuleIdConventionException.ErrorCode);
            }

            string ruleId = ruleIdToken?.Type == JTokenType.String
                ? ruleIdToken.Value<string>()
                : null;

            try
            {
                AIRuleIdConvention.ThrowIfUnacceptable(ruleId);
            }
            catch (AIRuleIdConventionException ex)
            {
                // The message is already shaped for AI consumption (see
                // AIRuleIdConventionException.BuildMessage) — surface it verbatim.
                return new BatchElementError(index, ex.Message, AIRuleIdConventionException.ErrorCode);
            }

            return null;
        }
    }
}
