// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using CommandLine;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    /// <summary>
    /// Options for <c>add-notification-reporting-descriptors</c>, which appends one or more SARIF
    /// <c>reportingDescriptor</c> objects to <c>run.tool.driver.notifications[]</c> in a staged
    /// event log (<c>&lt;output&gt;.wip.jsonl</c>) created by <c>emit-run</c>.
    /// </summary>
    /// <remarks>
    /// The payload is supplied as a JSON document (file via <c>--input</c> or piped on stdin) and
    /// may be a single descriptor object or an array of descriptor objects. Each <c>id</c> may
    /// appear at most once in the notifications array (and at most once within a batch); the batch
    /// is appended atomically (all or none).
    /// </remarks>
    [Verb("add-notification-reporting-descriptors", HelpText = "Append one or more SARIF reportingDescriptors (JSON object or array) to run.tool.driver.notifications[] in a staged event log.")]
    public class AddNotificationReportingDescriptorsOptions : EmitInputOptionsBase
    {
    }
}
