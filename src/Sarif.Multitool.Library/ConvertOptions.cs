// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

using CommandLine;

using Microsoft.CodeAnalysis.Sarif.Driver;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    [Verb("convert", HelpText = "Convert a tool output log to SARIF format.")]
    public class ConvertOptions : SingleFileOptionsBase
    {
        [Option(
            't',
            "tool",
            HelpText = "The tool format of the input file. Must be one of: AndroidStudio, CisCat, ClangAnalyzer, ClangTidy, CppCheck, ContrastSecurity, FlawFinder, Fortify, FortifyFpr, FxCop, Hdf, Nessus, PREfast, Pylint, SemmleQL, StaticDriverVerifier, TSLint, or a tool format for which a plugin assembly provides the converter.",
            Required = true)]
        public string ToolFormat { get; set; }

        [Option(
            'a',
            "plugin-assembly-path",
            HelpText = "Path to plugin assembly containing converter types.")]
        public string PluginAssemblyPath { get; set; }

        [Option(
            "normalize-for-github",
            HelpText = "(Deprecated. Use --normalize-for-ghas instead.) Normalize converted output to conform to GitHub Advanced Security (GHAS) code scanning ingestion requirements.")]
        [Obsolete("Deprecated. Use --normalize-for-ghas instead.")]
        public bool NormalizeForGitHub { get; set; }

        [Option(
            "normalize-for-ghas",
            HelpText = "Normalize converted output to conform to GitHub Advanced Security (GHAS) code scanning ingestion requirements.")]
        public bool NormalizeForGhas { get; set; }
    }
}
