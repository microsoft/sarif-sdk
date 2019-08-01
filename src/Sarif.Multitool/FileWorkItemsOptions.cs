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

        // For an explanation of the difference between ProjectUriString and ProjectUri, see FileWorkItemsCommand.ValidateOptions.
        public Uri ProjectUri { get; internal set; }

        [Option(
            "personal-access-token",
            HelpText = "TEMPORARY: Specifes the personal access used to access the project")]
        public string PersonalAccessToken { get; internal set; }

        [Option(
            "group",
            HelpText = "Apply a grouping strategy to factor input log file to multiple bugs. Must be one of All, PerRunPerRule or PerRunPerTargetPerRule.",
            Default = GroupingStrategy.All)]
        public GroupingStrategy GroupingStrategy { get; internal set; }

        [Option(
            "template",
            HelpText = "A bug template to use for creating work items.",
            Required = true)]
        public string TemplateFilePath { get; internal set; }
    }
}
