// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.Sarif.Driver;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    /// <summary>
    /// Implements <c>emit-results</c>: validates one or more fully-formed SARIF results and appends
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
                fileSystem,
                apply: (context, payload) => context.AddResults(payload));
        }
    }
}
