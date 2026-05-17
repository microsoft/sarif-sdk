// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text.Json;

using Microsoft.Extensions.Options;

using ModelContextProtocol.Server;

namespace Microsoft.CodeAnalysis.Sarif.Mcp.Server.Tools
{
    [McpServerToolType]
    public sealed class CreateRunTool
    {
        private static readonly string[] s_allowedAiOrigins = { "generated", "annotated", "synthesized" };

        private readonly SarifRunStore _store;
        private readonly CweNameResolver _cweResolver;
        private readonly SarifMcpOptions _options;

        public CreateRunTool(SarifRunStore store, CweNameResolver cweResolver, IOptions<SarifMcpOptions> options)
        {
            this._store = store;
            this._cweResolver = cweResolver;
            this._options = options.Value;
        }

        [McpServerTool(Name = "sarif_create_run")]
        [Description(
            "Initialize a new SARIF run with tool identity, VCS provenance, and AI origin. " +
            "Returns a run GUID to use with sarif_add_result, sarif_add_notification, and sarif_finalize.")]
        public string CreateRun(
            [Description("Scanner tool name")] string toolName,
            [Description("Tool semantic version")] string toolVersion,
            [Description("Git repository URI")] string repoUri,
            [Description("Git commit SHA")] string revisionId,
            [Description("Local source root path for snippet extraction")] string sourceRoot,
            [Description("Output filename (relative when server OutputRoot is configured, absolute for local/stdio)")] string outputPath,
            [Description("AI origin: generated, annotated, or synthesized")] string aiOrigin = "generated",
            [Description("Tool organization")] string? toolOrganization = null,
            [Description("Tool information URI")] string? toolInformationUri = null,
            [Description("Git branch name (omit for detached HEAD)")] string? branch = null,
            [Description("Scan scenario identifier")] string? scenario = null,
            [Description("Campaign correlation GUID for cross-run grouping")] string? campaignGuid = null,
            [Description("Whether sarif_finalize may overwrite an existing file at outputPath. When false (default), create-run fails fast if the target already exists, preventing accidental scan-result clobbering.")] bool allowOverwrite = false)
        {
            ValidateAiOrigin(aiOrigin);

            Guid runGuid = Guid.NewGuid();

            var run = new Run
            {
                Tool = new Tool
                {
                    Driver = new ToolComponent
                    {
                        Name = toolName,
                        SemanticVersion = toolVersion,
                        Organization = toolOrganization,
                        InformationUri = toolInformationUri != null ? new Uri(toolInformationUri) : null
                    }
                },
                AutomationDetails = new RunAutomationDetails
                {
                    Id = BuildAutomationId(toolName, repoUri, scenario),
                    Guid = runGuid,
                    CorrelationGuid = !string.IsNullOrEmpty(campaignGuid) && Guid.TryParse(campaignGuid, out Guid parsed)
                        ? parsed
                        : null
                },
                ColumnKind = ColumnKind.Utf16CodeUnits
            };

            run.VersionControlProvenance = new List<VersionControlDetails>
            {
                new()
                {
                    RepositoryUri = new Uri(repoUri),
                    RevisionId = revisionId,
                    Branch = branch,
                    MappedTo = new ArtifactLocation { UriBaseId = "SRCROOT" }
                }
            };

            string canonicalRoot = Path.GetFullPath(sourceRoot).Replace('\\', '/').TrimEnd('/') + "/";
            run.OriginalUriBaseIds = new Dictionary<string, ArtifactLocation>
            {
                ["SRCROOT"] = new ArtifactLocation
                {
                    Uri = new Uri("file:///" + canonicalRoot, UriKind.Absolute)
                }
            };

            run.SetProperty("ai/origin", aiOrigin);

            string runGuidStr = runGuid.ToString();
            string resolvedOutput = ResolveOutputPath(outputPath);

            // Reliability guard: fail fast if the target file already exists
            // unless the caller has opted in via allowOverwrite. Prevents an
            // accidental second run from clobbering a prior scan's results
            // while leaving in-memory state cleanly unwound (we have not yet
            // registered the run with the store).
            if (!allowOverwrite && File.Exists(resolvedOutput))
            {
                throw new InvalidOperationException(
                    $"Output file '{resolvedOutput}' already exists. " +
                    "Pass allowOverwrite=true to replace it, or choose a different outputPath.");
            }

            var ctx = new SarifRunContext(run, resolvedOutput, sourceRoot)
            {
                CweResolver = this._cweResolver
            };
            this._store.Create(runGuid, ctx);

            return JsonSerializer.Serialize(new
            {
                run_guid = runGuidStr,
                automation_id = run.AutomationDetails?.Id,
                output_path = resolvedOutput,
                status = "created"
            });
        }

        private string ResolveOutputPath(string callerPath)
        {
            if (string.IsNullOrEmpty(this._options.OutputRoot))
            {
                return callerPath;
            }

            string filename = Path.GetFileName(callerPath);
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentException("outputPath must include a filename.");
            }

            string resolved = Path.GetFullPath(Path.Combine(this._options.OutputRoot, filename));

            string canonicalRoot = Path.GetFullPath(this._options.OutputRoot)
                .TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

            if (!resolved.StartsWith(canonicalRoot, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    $"Resolved output path escapes the configured OutputRoot '{this._options.OutputRoot}'.");
            }

            return resolved;
        }

        private static void ValidateAiOrigin(string value)
        {
            if (Array.IndexOf(s_allowedAiOrigins, value.ToLowerInvariant()) < 0)
            {
                throw new ArgumentException(
                    $"Invalid aiOrigin '{value}'. Allowed: {string.Join(", ", s_allowedAiOrigins)}.");
            }
        }

        private static string BuildAutomationId(string toolName, string repoUri, string? scenario)
        {
            var uri = new Uri(repoUri);
            string path = uri.AbsolutePath.TrimStart('/').Replace("/_git/", "/");
            return $"{toolName}/{path}/{scenario ?? "SourceOnly"}/";
        }
    }
}
