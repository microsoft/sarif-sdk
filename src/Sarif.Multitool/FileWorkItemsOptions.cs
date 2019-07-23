// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using CommandLine;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    [Verb("file-work-items", HelpText = "Send SARIF results to a work item tracking system such as GitHub or Azure DevOps")]
    public class FileWorkItemsOptions
    {
        [Option(
            'p',
            "project-uri",
            HelpText = "The absolute URI of the project under which the work items are to be filed.",
            Required = true)]
        public string ProjectUriString { get; internal set; }

        // For an explanation of the difference between ProjectUriString and ProjectUri, see FileWorkItemsCommand.ValidateOptions.
        public Uri ProjectUri { get; internal set; }

        [Option(
            'f',
            "filtering-strategy",
            HelpText = "Specifies the strategy for selecting which results to file. Must be one of 'new' and 'all'.",
            Required = true)]
        public string FilteringStrategy { get; internal set; }

        [Option(
            'g',
            "grouping-strategy",
            HelpText = "Specifies the strategy for grouping grouping SARIF results into sets that should each be filed together as a single work item. Must be 'perResult'.",
            Required = true)]
        public string GroupingStrategy { get; internal set; }

        [Value(0,
            MetaName = "<inputFile>",
            HelpText = "The path to a SARIF file containing results to be filed as work items.",
            Required = true)]
        public string InputFilePath { get; set; }
    }
}
