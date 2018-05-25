// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        
// license. See LICENSE file in the project root for full license information.

using CommandLine;

namespace Sarif.Sdk.Sample
{
    [Verb("load", HelpText = "Loads a SARIF log file from disk.")]
    internal class LoadOptions
    {
        [Value(1,
            MetaName = "<inputFile>",
            HelpText = "The path to the log file to load.",
            Required = true)]
        public string InputFilePath { get; internal set; }
    }
}
