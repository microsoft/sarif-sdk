// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using CommandLine;

namespace Microsoft.CodeAnalysis.Sarif.SarifValidator
{
    internal class CommandLineOptions
    {
        [Value(0,
            MetaName = "<inputLogFile>",
            HelpText = "Path to the SARIF log file to be validated.",
            Required = true)]
        public string InputFilePath { get; set; }
    }
}
