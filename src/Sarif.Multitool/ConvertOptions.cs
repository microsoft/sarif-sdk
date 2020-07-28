// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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
            HelpText = "The tool format of the input file. Must be one of: AndroidStudio, ClangAnalyzer, CppCheck, ContrastSecurity, Fortify, FortifyFpr, FxCop, PREfast, Pylint, SemmleQL, StaticDriverVerifier, TSLint, or a tool format for which a plugin assembly provides the converter.",
            Required = true)]
        public string ToolFormat { get; internal set; }

        [Option(
            'a',
            "plugin-assembly-path",
            HelpText = "Path to plugin assembly containing converter types.")]
        public string PluginAssemblyPath { get; internal set; }

        [Option(
            "normalize-for-github-dsp",
            HelpText = "Normalize converted output to conform to GitHub DSP ingestion requirements.")]
        public bool NormalizeForGitHubDsp { get; internal set; }

    }
}