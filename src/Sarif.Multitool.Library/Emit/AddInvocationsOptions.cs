// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using CommandLine;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    /// <summary>
    /// Options for <c>emit-invocations</c>, which appends one or more fully-formed SARIF
    /// <c>invocation</c> objects to a staged event log (<c>&lt;output&gt;.wip.jsonl</c>) created by
    /// <c>emit-run</c>.
    /// </summary>
    /// <remarks>
    /// The payload is supplied as a JSON document (file via <c>--input</c> or piped on stdin) and
    /// may be a single invocation object or an array of invocation objects. Notifications travel
    /// inline on <c>toolExecutionNotifications</c> / <c>toolConfigurationNotifications</c>. When the
    /// payload is a lone object and omits <c>endTimeUtc</c>, the verb stamps receipt time; a batch
    /// (array) submission must carry <c>endTimeUtc</c> on every element.
    /// </remarks>
    [Verb("emit-invocations", HelpText = "Append one or more fully-formed SARIF invocations (JSON object or array) to a staged event log.")]
    public class AddInvocationsOptions : EmitInputOptionsBase
    {
    }
}
