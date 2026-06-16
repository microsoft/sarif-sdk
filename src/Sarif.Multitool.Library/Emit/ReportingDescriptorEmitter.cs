// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    /// <summary>
    /// Shared entry point behind <c>add-notification-reporting-descriptors</c> and
    /// <c>add-rule-reporting-descriptors</c>: drives a <see cref="Sarif.Emit.RunEmitContext"/> to
    /// validate one or more SARIF reportingDescriptor objects and append an event per element to
    /// <c>&lt;output&gt;.wip.jsonl</c>.
    /// </summary>
    /// <remarks>
    /// Notifications append to <c>run.tool.driver.notifications[]</c>; rules append to
    /// <c>run.tool.driver.rules[]</c> and require a well-formed <c>NOVEL-</c> id. Each id may appear
    /// at most once in its target array, counting both descriptors already in the event log and the
    /// other elements of the submitted batch.
    /// </remarks>
    internal static class ReportingDescriptorEmitter
    {
        internal static int Append(
            string outputFilePath,
            string inputFilePath,
            bool isRules,
            IFileSystem fileSystem = null)
        {
            string payloadKind = isRules ? "rule descriptor" : "notification descriptor";

            return EmitBatchProcessor.Run(
                outputFilePath,
                inputFilePath,
                payloadKind,
                fileSystem,
                apply: (context, payload) => isRules
                    ? context.AddRuleDescriptors(payload)
                    : context.AddNotificationDescriptors(payload));
        }
    }
}
