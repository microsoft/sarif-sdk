// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using CommandLine;
using CommandLine.Text;
using Microsoft.CodeAnalysis.Sarif.Driver;
using Microsoft.CodeAnalysis.Sarif.WorkItemFiling;

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
            "filtering-strategy",
            HelpText = "Specifies the strategy for selecting which results to file. Must be one of 'new' and 'all'.",
            Required = true)]
        public FilteringStrategyKind FilteringStrategy { get; internal set; }

        [Option(
            'g',
            "grouping-strategy",
            HelpText = "Specifies the strategy for grouping SARIF results into sets that should each be filed together as a single work item. Must be 'perResult'.",
            Required = true)]
        public GroupingStrategyKind GroupingStrategy { get; internal set; }

        [Option(
            "personal-access-token",
            HelpText = "TEMPORARY: Specifes the personal access used to access the project")]
        public string PersonalAccessToken { get; internal set; }

        [Option(
            "test-option-validation",
            HelpText = "For unit tests: allows us to just validate the options and return",
            Hidden = true)]
        public bool TestOptionValidation { get; internal set; }
    }
}
