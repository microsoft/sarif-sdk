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
            "split",
            HelpText = "Apply a splitting strategy to each input log file in order to break the file into multiple bugs. " +
                       "Must be one of None, PerRun, PerResult, PerRunPerRule, PerRunPerTargetPerRule or PerRunPerTarget. " +
                       "By default ('None'), no splitting strategy is applied (i.e. each input file will be filed as a single bug).",
            Default = SplittingStrategy.None)]
        public SplittingStrategy SplittingStrategy { get; internal set; }

        [Option(
            'c',
            "configuration",
            HelpText = "A path to an XML configuration file that will be used to drive work item creation.")]
        public string ConfigurationFilePath { get; internal set; }
    }
}
