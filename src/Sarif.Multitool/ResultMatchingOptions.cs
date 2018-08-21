// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using CommandLine;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    [Verb("match-results-forward", HelpText = "Track results run over run by persisting IDs and then matching them forward")]
    class ResultMatchingOptions : MultitoolOptionsBase
    {
        [Option(
            'o',
            "old",
            HelpText = "Path to a Sarif Log containing representing the last set of results on a target (or empty if none exists)",
            Required = false,
            Default = null)]
        public string BaselineFilePath { get; internal set; }

        [Option(
            'c',
            "current",
            HelpText = "Path to a Sarif Log representing the current set of results",
            Required = true)]
        public string CurrentFilePath { get; internal set; }
        

        [Option('o', 
            "output-file-path", 
            HelpText = "Output File Path to put the sarif log with persistent ids assigned.", 
            Default = "persisted-ids-log.sarif")]
        public string OutputFilePath { get; internal set; }
    }
}
