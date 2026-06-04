// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using CommandLine;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    /// <summary>
    /// Options for <c>add-rule-reporting-descriptor</c>, which appends a SARIF
    /// <c>reportingDescriptor</c> to <c>run.tool.driver.rules[]</c> in a staged event log
    /// (<c>&lt;output&gt;.wip.jsonl</c>) created by <c>emit-run</c>.
    /// </summary>
    /// <remarks>
    /// Reserved for novel-finding rules: the descriptor <c>id</c> must be a well-formed
    /// <c>NOVEL-</c> id. Descriptors for taxonomy-mapped rules (e.g., <c>CWE-89</c>) come from the
    /// taxonomy enricher, not this verb. Each <c>id</c> may appear at most once in the rules array.
    /// </remarks>
    [Verb("add-rule-reporting-descriptor", HelpText = "Append a SARIF reportingDescriptor (JSON) with a NOVEL- id to run.tool.driver.rules[] in a staged event log.")]
    public class AddRuleReportingDescriptorOptions
    {
        [Value(
            0,
            MetaName = "<outputSarifPath>",
            HelpText = "Path to the final SARIF file; the event log is appended to '<output>.wip.jsonl'.",
            Required = true)]
        public string OutputFilePath { get; set; }

        [Option(
            'i',
            "input",
            HelpText = "Path to a JSON file containing the SARIF reportingDescriptor object. If omitted, JSON is read from stdin.")]
        public string InputFilePath { get; set; }
    }
}
