// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using CommandLine;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    /// <summary>
    /// Options for <c>get-skill</c>, which emits an agent skill that drives the multitool emit and
    /// validate verbs. The skill markdown is written to stdout, or to <c>--output</c>.
    /// </summary>
    /// <remarks>
    /// The skill ships embedded in the package, so an agent that resolves the tool (for example via
    /// <c>dotnet dnx</c>) obtains the procedure from the same artifact it runs. Relative links in the
    /// source skill are rewritten to commit-pinned permalinks on the way out, so the emitted document
    /// resolves its references against the exact repository state that built the tool.
    /// </remarks>
    [Verb("get-skill", HelpText = "Emit an agent skill that drives the emit/validate verbs. Pass --list to enumerate the available skills.")]
    public class GetSkillOptions
    {
        [Value(
            0,
            MetaName = "<skill>",
            HelpText = "The skill to emit (e.g., 'emit-sarif', 'validate-sarif').",
            Required = false)]
        public string Skill { get; set; }

        [Option(
            'o',
            "output",
            HelpText = "Path to write the skill to. If omitted, the skill is written to stdout.")]
        public string OutputFilePath { get; set; }

        [Option(
            "list",
            HelpText = "List the available skills, then exit.",
            Default = false)]
        public bool List { get; set; }

        [Option(
            "force-overwrite",
            HelpText = "Overwrite the --output file if it already exists.",
            Default = false)]
        public bool ForceOverwrite { get; set; }
    }
}
