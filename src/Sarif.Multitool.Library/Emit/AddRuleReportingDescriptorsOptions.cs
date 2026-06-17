// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using CommandLine;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    /// <summary>
    /// Options for <c>add-rule-reporting-descriptors</c>, which appends one or more SARIF
    /// <c>reportingDescriptor</c> objects to <c>run.tool.driver.rules[]</c> in a staged event log
    /// (<c>&lt;output&gt;.wip.jsonl</c>) created by <c>emit-run</c>.
    /// </summary>
    /// <remarks>
    /// Reserved for novel-finding rules: every descriptor <c>id</c> must be a well-formed
    /// <c>NOVEL-</c> id. Descriptors for taxonomy-mapped rules (e.g., <c>CWE-89</c>) come from the
    /// taxonomy enricher, not this verb. The payload may be a single descriptor object or an array;
    /// each <c>id</c> may appear at most once in the rules array (and at most once within a batch).
    /// </remarks>
    [Verb("add-rule-reporting-descriptors", HelpText = "Append one or more SARIF reportingDescriptors (JSON object or array) with NOVEL- ids to run.tool.driver.rules[] in a staged event log.")]
    public class AddRuleReportingDescriptorsOptions : EmitInputOptionsBase
    {
    }
}
