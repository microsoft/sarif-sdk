// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.Sarif.Driver;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    /// <summary>
    /// Implements <c>emit-invocations</c>: validates one or more fully-formed SARIF invocations and
    /// appends an <c>invocation</c> event per element to <c>&lt;output&gt;.wip.jsonl</c>.
    /// </summary>
    /// <remarks>
    /// <para>The payload is a single invocation object or an array of invocation objects. Each is
    /// gated for the required AI invocation fields: <c>executionSuccessful</c>, <c>commandLine</c>,
    /// an anchored <c>workingDirectory</c> (a <c>uri</c> and/or <c>uriBaseId</c>), and inline
    /// notification <c>timeUtc</c> values. Full structural validation runs at
    /// <c>emit-finalize --validate</c>. The batch is atomic: if any element is rejected, nothing is
    /// appended and the rejected indices are reported.</para>
    /// <para><c>endTimeUtc</c> handling is the one place single and batch submission diverge. A lone
    /// object that omits <c>endTimeUtc</c> is stamped with receipt time, since the write is roughly
    /// coincident with the invocation's conclusion. A batch is assembled after the fact — one write
    /// instant cannot stand in for many invocations that ended at different times — so every batched
    /// invocation must carry its own <c>endTimeUtc</c>.</para>
    /// </remarks>
    public class AddInvocationsCommand : CommandBase
    {
        public int Run(AddInvocationsOptions options, IFileSystem fileSystem = null)
        {
            return EmitBatchProcessor.Run(
                options?.OutputFilePath,
                options?.InputFilePath,
                payloadKind: "invocation",
                fileSystem,
                apply: (context, payload) => context.AddInvocations(payload));
        }
    }
}
