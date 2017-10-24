// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using CommandLine;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    [Verb("convert", HelpText = "Convert a tool output log to SARIF format.")]
    internal class ConvertOptions : SingleFileOptionsBase
    {
        [Option(
            't',
            "tool",
            HelpText = "The tool format of the input file. Must be one of: AndroidStudio, ClangAnalyzer, CppCheck, Fortify, FortifyFpr, FxCop, PREfast, SemmleQL, StaticDriverVerifier, TSLint, or a tool format for which a plugin assembly provides the converter.",
            Required = true)]
        public string ToolFormat { get; internal set; }

        [Option(
            'a',
            "plugin-assembly-path",
            HelpText = "Path to plugin assembly containing converter types.")]
        public string PluginAssemblyPath { get; internal set; }
    }
}