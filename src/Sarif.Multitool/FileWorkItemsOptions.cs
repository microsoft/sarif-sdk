// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using CommandLine;
using Microsoft.CodeAnalysis.Sarif.Driver;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    [Verb("file-work-items", HelpText = "Send SARIF results to a work item tracking system such as GitHub or Azure DevOps")]
    public class FileWorkItemsOptions : SingleFileOptionsBase
    {
        [Option(
            "host-uri",
            HelpText = "The absolute URI of the project in which the work items are to be filed, for example https://dev.azure.com/{org}/{project} or https://github.com/{org}/{repo}.")]
        public string HostUri { get; internal set; }

        [Option(
            "split",
            HelpText = "Apply a splitting strategy to each input log file in order to break the file into multiple bugs. " +
                       "Must be one of None, PerRun, PerResult, PerRunPerRule, PerRunPerTargetPerRule or PerRunPerTarget. " +
                       "By default ('None'), no splitting strategy is applied (i.e. each input file will be filed as a single bug).",
            Default = SplittingStrategy.None)]
        public SplittingStrategy SplittingStrategy { get; internal set; }

        [Option(
            "sync-workitem-metadata",
            HelpText = "Enrich work item to be created with existing work item metadata for any result workItemUri.",
            Default = null)]
        public bool? SyncWorkItemMetadata { get; internal set; }

        [Option(
            "file-unchanged",
            HelpText = "File work items for both new and unchanged baseline state results. " + 
                       "If an unchanged result already has an associated work item, a new work item will not be created.",
            Default = null)]
        public bool? ShouldFileUnchanged { get; internal set; }

        [Option(
            'c',
            "configuration",
            HelpText = "A path to an XML configuration file that will be used to drive work item creation.")]
        public string ConfigurationFilePath { get; internal set; }

        [Option(
            "no-validate",
            HelpText = "Do not validate the SARIF log file before filing.")]
        public bool DoNotValidate { get; internal set; }
    }
}
