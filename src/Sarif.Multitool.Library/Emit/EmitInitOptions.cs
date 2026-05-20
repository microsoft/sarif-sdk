// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using CommandLine;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    /// <summary>
    /// Options for <c>emit-init</c>, which opens an append-only event log
    /// (<c>&lt;output&gt;.wip.jsonl</c>) seeded with a <c>run-header</c> event for the supplied
    /// tool. Subsequent producers append events to the log via the SARIF emit API and finalize
    /// via <c>multitool emit-finalize</c>.
    /// </summary>
    [Verb("emit-init", HelpText = "Open an append-only event log seeded with a SARIF run header.")]
    public class EmitInitOptions
    {
        [Value(
            0,
            MetaName = "<outputSarifPath>",
            HelpText = "Path to the final SARIF file (the event log is staged alongside as '<output>.wip.jsonl').",
            Required = true)]
        public string OutputFilePath { get; set; }

        [Option(
            't',
            "tool",
            HelpText = "Driver name (run.tool.driver.name). Required.",
            Required = true)]
        public string ToolName { get; set; }

        [Option(
            "tool-version",
            HelpText = "Driver semantic version (run.tool.driver.version).")]
        public string ToolVersion { get; set; }

        [Option(
            "info-uri",
            HelpText = "Driver information URI (run.tool.driver.informationUri).")]
        public string InformationUri { get; set; }

        [Option(
            "organization",
            HelpText = "Driver organization (run.tool.driver.organization).")]
        public string Organization { get; set; }

        [Option(
            "repo",
            HelpText = "Repository URI (run.versionControlProvenance[0].repositoryUri).")]
        public string RepositoryUri { get; set; }

        [Option(
            "revision",
            HelpText = "Revision identifier, typically a commit SHA (run.versionControlProvenance[0].revisionId).")]
        public string RevisionId { get; set; }

        [Option(
            "branch",
            HelpText = "Branch name (run.versionControlProvenance[0].branch).")]
        public string Branch { get; set; }

        [Option(
            "source-root",
            HelpText = "Repository root on disk; recorded under originalUriBaseIds[\"SRCROOT\"].")]
        public string SourceRoot { get; set; }

        [Option(
            "allow-overwrite",
            HelpText = "Allow an existing .sarif or in-progress .wip.jsonl at the destination to be replaced.",
            Default = false)]
        public bool AllowOverwrite { get; set; }
    }
}
