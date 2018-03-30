// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using CommandLine;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    [Verb("baseline", HelpText = "Create a baseline between two sarif logs")]
    class BaselineOptions : MultitoolOptionsBase
    {
        [Option(
            'b',
            "baseline",
            HelpText = "Path to a Sarif Log containing a single run representing the baseline file",
            Required = true)]
        public string BaselineFilePath { get; internal set; }

        [Option(
            'c',
            "current",
            HelpText = "Path to a Sarif Log containing a single run, representing the current results",
            Required = true)]
        public string CurrentFilePath { get; internal set; }

        [Option(
            't',
            "baseline-type",
            HelpText = "Type of baseline to do.  Currently support:  Strict,Standard ",
            Default = Baseline.SarifBaselineType.Strict)]
        public Baseline.SarifBaselineType BaselineType { get; internal set; }

        [Option('o', 
            "output-file-path", 
            HelpText = "Output File Path to put the baselined sarif log", 
            Default = "baselined-log.sarif")]
        public string OutputFilePath { get; internal set; }
    }
}
