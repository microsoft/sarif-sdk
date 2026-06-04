// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.Sarif.Driver;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    /// <summary>
    /// Implements <c>multitool add-rule-reporting-descriptor</c>: validates a SARIF
    /// reportingDescriptor JSON with a <c>NOVEL-</c> id and appends it to
    /// <c>run.tool.driver.rules[]</c> in a staged event log.
    /// </summary>
    public class AddRuleReportingDescriptorCommand : CommandBase
    {
        public int Run(AddRuleReportingDescriptorOptions options, IFileSystem fileSystem = null)
        {
            return ReportingDescriptorEmitter.Append(
                options?.OutputFilePath,
                options?.InputFilePath,
                isRules: true,
                fileSystem);
        }
    }
}
