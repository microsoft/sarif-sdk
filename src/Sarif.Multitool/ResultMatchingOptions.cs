// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using CommandLine;
using Microsoft.CodeAnalysis.Sarif.Driver;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    [Verb("match-results-forward", HelpText = "Track results run over run by persisting IDs and then matching them forward")]
    internal class ResultMatchingOptions : CommonOptionsBase
    {
        [Option(
            'r',
            "previous",
            HelpText = "Path to a sarif log containing the previous set of results with result matching annotations on a target (or empty if no previous log exists)",
            Required = false,
            Default = null)]
        public string PreviousFilePath { get; internal set; }

        [Value(0,
            MetaName = "<currentFile>",
            HelpText = "Path(s) to sarif log(s) comprising the current set of results, without result matching information",
            Required = true)]
        public IEnumerable<string> CurrentFilePaths { get; internal set; }
        

        [Option('o', 
            "output-file-path", 
            HelpText = "File Path to output the annotated sarif log with result matching information.  Defaults to <currentFile>-annotated.sarif")]
        public string OutputFilePath { get; internal set; }
    }
}
