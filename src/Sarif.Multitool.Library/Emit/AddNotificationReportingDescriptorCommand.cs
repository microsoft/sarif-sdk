// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.Sarif.Driver;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    /// <summary>
    /// Implements <c>add-notification-reporting-descriptor</c>: validates a SARIF
    /// reportingDescriptor JSON and appends it to <c>run.tool.driver.notifications[]</c> in a
    /// staged event log.
    /// </summary>
    public class AddNotificationReportingDescriptorCommand : CommandBase
    {
        public int Run(AddNotificationReportingDescriptorOptions options, IFileSystem fileSystem = null)
        {
            return ReportingDescriptorEmitter.Append(
                options?.OutputFilePath,
                options?.InputFilePath,
                isRules: false,
                fileSystem);
        }
    }
}
