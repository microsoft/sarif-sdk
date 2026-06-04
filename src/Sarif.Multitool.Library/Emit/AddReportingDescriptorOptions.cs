// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using CommandLine;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    /// <summary>
    /// Options for <c>add-reporting-descriptor</c>, which appends a fully-formed SARIF
    /// <c>reportingDescriptor</c> object to a staged event log
    /// (<c>&lt;output&gt;.wip.jsonl</c>) created by <c>emit-init-run</c>.
    /// </summary>
    /// <remarks>
    /// <para>Default target is <c>run.tool.driver.notifications[]</c>; pass <c>--rules</c> to
    /// target <c>run.tool.driver.rules[]</c>. The rules path is reserved for NOVEL- descriptors.</para>
    /// <para>The descriptor is supplied as a JSON document (file via <c>--input</c> or piped
    /// on stdin) and round-trips through the staged event log.</para>
    /// <para>Each descriptor <c>id</c> may appear at most once per event log target.</para>
    /// </remarks>
    [Verb("add-reporting-descriptor", HelpText = "Append a fully-formed SARIF reportingDescriptor (JSON) to a staged event log. Default target is run.tool.driver.notifications[]; pass --rules to target run.tool.driver.rules[].")]
    public class AddReportingDescriptorOptions
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

        [Option(
            "rules",
            HelpText = "Target run.tool.driver.rules[] (NOVEL- descriptor ids only). Default target is run.tool.driver.notifications[].",
            Default = false)]
        public bool Rules { get; set; }
    }
}
