// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using CommandLine;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    [Verb("file-work-items", HelpText = "Send SARIF results to a work item tracking system such as GitHub or Azure DevOps")]
    public class FileWorkItemsOptions
    {
        [Option(
            'f',
            "filing-target-type",
            HelpText = "The type of system to which work items are to be filed. Must be one of 'github' and 'azureDevOps'.",
            Required = true)]
        public string TargetType { get; internal set; }
    }
}
