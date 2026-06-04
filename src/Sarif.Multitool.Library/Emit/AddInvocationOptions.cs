// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using CommandLine;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    /// <summary>
    /// Options for <c>add-invocation</c>, which appends a fully-formed SARIF <c>invocation</c>
    /// object to a staged event log (<c>&lt;output&gt;.wip.jsonl</c>) created by
    /// <c>emit-run</c>.
    /// </summary>
    /// <remarks>
    /// The invocation is supplied as a JSON document (file via <c>--input</c> or piped on stdin).
    /// Notifications travel inline on <c>toolExecutionNotifications</c> /
    /// <c>toolConfigurationNotifications</c>.
    /// </remarks>
    [Verb("add-invocation", HelpText = "Append a fully-formed SARIF invocation (JSON) to a staged event log.")]
    public class AddInvocationOptions : EmitInputOptionsBase
    {
    }
}
