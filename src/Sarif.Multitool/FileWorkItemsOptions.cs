// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using CommandLine;
using Microsoft.CodeAnalysis.Sarif.Driver;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    [Verb("file-work-items", HelpText = "Send SARIF results to a work item tracking system such as GitHub or Azure DevOps")]
    public class FileWorkItemsOptions : SingleFileOptionsBase
    { 
        [Option(
            "project-uri",
            HelpText = "The absolute URI of the project in which the work items are to be filed, for example https://dev.azure.com/{org}/{project} or https://github.com/{org}/{project}.",
            Required = true)]
        public string ProjectUriString { get; internal set; }

        // ProjectUri is the URI representation of ProjectUriString above. We convert the command-line argument to the URI
        // form as part of argument validation at runtime (in order to provide better error messages than those that 
        // the CommandlineLibrary component provides). See FileWorkItemsCommand.ValidateOptions for more details.
        public Uri ProjectUri { get; internal set; }

        [Option(
            "group",
            HelpText = "Apply a grouping strategy to factor input log file to multiple bugs. Must be one of All, PerRun, PerResult, PerRunPerRule or PerRunPerTarget, or PerRunPerTargetPerRule.",
            Default = GroupingStrategy.PerRun)]
        public GroupingStrategy GroupingStrategy { get; internal set; }
    }
}
