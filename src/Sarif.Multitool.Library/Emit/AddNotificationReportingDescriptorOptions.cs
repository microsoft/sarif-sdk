// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using CommandLine;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    /// <summary>
    /// Options for <c>add-notification-reporting-descriptor</c>, which appends a SARIF
    /// <c>reportingDescriptor</c> to <c>run.tool.driver.notifications[]</c> in a staged event log
    /// (<c>&lt;output&gt;.wip.jsonl</c>) created by <c>emit-run</c>.
    /// </summary>
    /// <remarks>
    /// The descriptor is supplied as a JSON document (file via <c>--input</c> or piped on stdin).
    /// Each <c>id</c> may appear at most once in the notifications array.
    /// </remarks>
    [Verb("add-notification-reporting-descriptor", HelpText = "Append a SARIF reportingDescriptor (JSON) to run.tool.driver.notifications[] in a staged event log.")]
    public class AddNotificationReportingDescriptorOptions : EmitInputOptionsBase
    {
    }
}
