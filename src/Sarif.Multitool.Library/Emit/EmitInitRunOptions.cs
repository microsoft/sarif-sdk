// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using CommandLine;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    /// <summary>
    /// Options for <c>emit-init-run</c>, which opens an append-only event log
    /// (<c>&lt;output&gt;.wip.jsonl</c>) seeded with a <c>run-header</c> event for the supplied
    /// tool. Subsequent producers append events to the log via the SARIF emit API and finalize
    /// via <c>multitool emit-finalize</c>.
    /// </summary>
    /// <remarks>
    /// CLI flags mirror the SARIF interior paths they populate (e.g., <c>--tool-driver-name</c>
    /// populates <c>run.tool.driver.name</c>; <c>--vcp-revisionid</c> populates
    /// <c>run.versionControlProvenance[0].revisionId</c>). This trades verbosity for a one-to-one
    /// mapping that a SARIF-literate user can read without a help page.
    /// </remarks>
    [Verb("emit-init-run", HelpText = "Open an append-only event log seeded with a SARIF run header.")]
    public class EmitInitRunOptions
    {
        [Value(
            0,
            MetaName = "<outputSarifPath>",
            HelpText = "Path to the final SARIF file (the event log is staged alongside as '<output>.wip.jsonl').",
            Required = true)]
        public string OutputFilePath { get; set; }

        [Option(
            't',
            "tool-driver-name",
            HelpText = "Driver name (run.tool.driver.name). Required.",
            Required = true)]
        public string ToolName { get; set; }

        [Option(
            "tool-version",
            HelpText = "Driver semantic version (run.tool.driver.version).")]
        public string ToolVersion { get; set; }

        [Option(
            "tool-driver-semantic-version",
            HelpText = "Driver semantic version per SemVer 2.0 (run.tool.driver.semanticVersion).")]
        public string ToolDriverSemanticVersion { get; set; }

        [Option(
            "information-uri",
            HelpText = "Driver information URI (run.tool.driver.informationUri).")]
        public string InformationUri { get; set; }

        [Option(
            "organization",
            HelpText = "Driver organization (run.tool.driver.organization).")]
        public string Organization { get; set; }

        [Option(
            "automation-guid",
            HelpText = "Stable GUID for this run (run.automationDetails.guid).")]
        public string AutomationGuid { get; set; }

        [Option(
            "automation-correlation-guid",
            HelpText = "Stable GUID for the equivalence class of related runs (run.automationDetails.correlationGuid).")]
        public string AutomationCorrelationGuid { get; set; }

        [Option(
            "ai-origin",
            HelpText = "AI authoring origin (run.properties.ai/origin). One of: generated, annotated, synthesized.")]
        public string AiOrigin { get; set; }

        [Option(
            "vcp-repositoryuri",
            HelpText = "Repository URI (run.versionControlProvenance[0].repositoryUri).")]
        public string RepositoryUri { get; set; }

        [Option(
            "vcp-revisionid",
            HelpText = "Revision identifier, typically a commit SHA (run.versionControlProvenance[0].revisionId).")]
        public string RevisionId { get; set; }

        [Option(
            "vcp-branch",
            HelpText = "Branch name (run.versionControlProvenance[0].branch).")]
        public string Branch { get; set; }

        [Option(
            "srcroot",
            HelpText = "Repository root on disk; recorded under originalUriBaseIds[\"SRCROOT\"].")]
        public string SourceRoot { get; set; }

        [Option(
            "force-overwrite",
            HelpText = "Force replacement of an existing .sarif or in-progress .wip.jsonl at the destination.",
            Default = false)]
        public bool ForceOverwrite { get; set; }
    }
}

