// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.Sarif.Driver;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    /// <summary>
    /// Implements <c>emit-rule-descriptors</c>: validates one or more SARIF
    /// reportingDescriptor objects with <c>NOVEL-</c> ids and appends them to
    /// <c>run.tool.driver.rules[]</c> in a staged event log.
    /// </summary>
    public class AddRuleReportingDescriptorsCommand : CommandBase
    {
        public int Run(AddRuleReportingDescriptorsOptions options, IFileSystem fileSystem = null)
        {
            return ReportingDescriptorEmitter.Append(
                options?.OutputFilePath,
                options?.InputFilePath,
                isRules: true,
                fileSystem);
        }
    }
}
