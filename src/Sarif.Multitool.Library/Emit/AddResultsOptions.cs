// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using CommandLine;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    /// <summary>
    /// Options for <c>emit-results</c>, which appends one or more fully-formed SARIF <c>result</c>
    /// objects to a staged event log (<c>&lt;output&gt;.wip.jsonl</c>) created by <c>emit-run</c>.
    /// </summary>
    /// <remarks>
    /// The payload is supplied as a JSON document (file via <c>--input</c> or piped on stdin) and
    /// may be a single result object or an array of result objects. Every <c>result.ruleId</c> is
    /// validated against the AI ruleId convention; the batch is appended atomically (all or none).
    /// </remarks>
    [Verb("emit-results", HelpText = "Append one or more fully-formed SARIF results (JSON object or array) to a staged event log.")]
    public class AddResultsOptions : EmitInputOptionsBase
    {
    }
}
