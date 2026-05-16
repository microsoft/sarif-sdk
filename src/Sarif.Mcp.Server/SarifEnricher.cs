// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Mcp.Server
{
    /// <summary>
    /// Takes a minimal result from an agent and enriches it with context
    /// regions, snippets, artifact indices, rule indices, and hashes. This is
    /// the core value proposition of the SARIF MCP server: agents provide
    /// minimal data, the enricher reads source files and populates everything else.
    /// </summary>
    internal sealed class SarifEnricher
    {
        private static readonly string[] s_allowedExploitability = { "demonstrated", "poc", "theoretical" };

        private static readonly string[] s_recommendedAttackerPosition =
        {
            "unauthenticated-network", "adjacent-network", "authenticated-user",
            "local-host", "configuration", "physical", "harness-only", "unclear"
        };

        private static readonly string[] s_allowedLevels = { "error", "warning", "note" };

        /// <summary>
        /// Enriches a result: registers the rule, resolves artifact indices,
        /// populates region snippets and context regions from source files.
        /// </summary>
        public EnrichmentResult Enrich(SarifRunContext ctx, AddResultRequest request)
        {
            ctx.ThrowIfFinalized();

            List<string>? warnings = null;

            // --- Validate required fields ---
            if (string.IsNullOrEmpty(request.RuleId))
            {
                throw new ArgumentException("ruleId is required.");
            }

            {
                int slash = request.RuleId.IndexOf('/');
                if (slash < 1 || slash == request.RuleId.Length - 1)
                {
                    throw new ArgumentException(
                        $"ruleId must be in 'BASE/sub-id' format (e.g., 'CWE-502/binaryformatter'). Got: '{request.RuleId}'.");
                }
            }

            if (request.Location != null)
            {
                if (request.Location.StartLine is null or <= 0)
                {
                    throw new ArgumentException(
                        "start_line must be >= 1 when a location URI is provided. SARIF line numbers are 1-based.");
                }

                if (request.Location.EndLine != null && request.Location.EndLine < request.Location.StartLine)
                {
                    throw new ArgumentException(
                        $"end_line ({request.Location.EndLine}) must be >= start_line ({request.Location.StartLine}).");
                }

                if (request.Location.EndLine == request.Location.StartLine
                    && request.Location.EndColumn != null && request.Location.StartColumn != null
                    && request.Location.EndColumn < request.Location.StartColumn)
                {
                    throw new ArgumentException(
                        $"end_column ({request.Location.EndColumn}) must be >= start_column ({request.Location.StartColumn}) on the same line.");
                }
            }

            if (string.IsNullOrEmpty(request.MessageText))
            {
                throw new ArgumentException("messageText is required (standalone first sentence).");
            }

            if (string.IsNullOrEmpty(request.MessageMarkdown))
            {
                throw new ArgumentException("messageMarkdown is required (structured finding report).");
            }

            // --- Validate AI profile vocabularies ---
            ValidateExploitability(request.Exploitability);
            ValidateAttackerPosition(request.AttackerPosition, ref warnings);
            ValidateLevel(request.Level);

            // --- Register rule ---
            string baseRuleId = SarifRunContext.GetBaseRuleId(request.RuleId);
            string? helpUri = baseRuleId.StartsWith("CWE-", StringComparison.OrdinalIgnoreCase)
                ? $"https://cwe.mitre.org/data/definitions/{baseRuleId[4..]}.html"
                : null;

            bool alreadyRegistered = ctx.HasRule(request.RuleId);
            int ruleIndex = ctx.EnsureRule(
                request.RuleId,
                request.RuleName,
                request.RuleDescription,
                helpUri,
                ParseLevel(request.Level));

            // --- Build the result.locations[0] entry (if any) ---
            Location? location = null;
            if (request.Location != null)
            {
                LocationRequest loc = request.Location;
                int artifactIndex = ctx.EnsureArtifact(loc.Uri);

                var region = new Region
                {
                    StartLine = loc.StartLine ?? 0,
                    StartColumn = loc.StartColumn ?? 0,
                    EndLine = loc.EndLine ?? 0,
                    EndColumn = loc.EndColumn ?? 0
                };

                Region? contextRegion = null;
                string? absolutePath = ctx.ResolveToAbsolutePath(loc.Uri);

                if (absolutePath != null)
                {
                    var fileUri = new Uri(absolutePath);
                    NewLineIndex? nli = ctx.FileRegionsCache.GetNewLineIndex(fileUri);

                    if (nli != null && loc.StartLine > nli.MaximumLineNumber)
                    {
                        AddWarning(
                            ref warnings,
                            $"startLine ({loc.StartLine}) exceeds file length " +
                            $"({nli.MaximumLineNumber} lines) in '{loc.Uri}'. Snippet not populated.");
                    }
                    else if (nli != null)
                    {
                        region = ctx.FileRegionsCache.PopulateTextRegionProperties(
                            region, fileUri, populateSnippet: true);
                        contextRegion = ctx.FileRegionsCache.ConstructMultilineContextSnippet(region, fileUri);
                    }
                }
                else
                {
                    // Can't read file — use agent-supplied snippet if available.
                    if (loc.Snippet != null)
                    {
                        region.Snippet = new ArtifactContent { Text = loc.Snippet };
                    }
                    else
                    {
                        AddWarning(
                            ref warnings,
                            $"Could not read '{loc.Uri}' — snippet and context region not populated.");
                    }
                }

                location = new Location
                {
                    PhysicalLocation = new PhysicalLocation
                    {
                        ArtifactLocation = new ArtifactLocation
                        {
                            Uri = new Uri(loc.Uri, UriKind.RelativeOrAbsolute),
                            UriBaseId = string.IsNullOrEmpty(ctx.SourceRoot) ? null : "SRCROOT",
                            Index = artifactIndex
                        },
                        Region = region,
                        ContextRegion = contextRegion
                    }
                };

                if (request.LogicalLocations != null)
                {
                    location.LogicalLocations = new List<LogicalLocation>();
                    foreach (LogicalLocationRequest logical in request.LogicalLocations)
                    {
                        location.LogicalLocations.Add(new LogicalLocation
                        {
                            Name = logical.Name,
                            FullyQualifiedName = logical.FullyQualifiedName,
                            Kind = logical.Kind ?? "function"
                        });
                    }
                }
            }

            // --- Build result ---
            // result.ruleId carries the full sub-id (e.g., "CWE-502/binaryformatter").
            // result.rule references the descriptor by base id + index.
            var result = new Result
            {
                RuleId = request.RuleId,
                Rule = new ReportingDescriptorReference
                {
                    Id = baseRuleId,
                    Index = ruleIndex
                },
                Level = ParseLevel(request.Level),
                Kind = ResultKind.Fail,
                Guid = Guid.NewGuid(),
                Message = new Message
                {
                    Text = request.MessageText,
                    Markdown = request.MessageMarkdown
                },
                Locations = location != null ? new List<Location> { location } : null
            };

            if (request.Rank.HasValue)
            {
                result.Rank = request.Rank.Value;
            }

            // --- AI profile properties ---
            if (request.Exploitability != null)
            {
                result.SetProperty("ai/exploitability", request.Exploitability);
            }

            if (request.AttackerPosition != null)
            {
                result.SetProperty("ai/attackerPosition", request.AttackerPosition);
            }

            if (request.Handoff != null)
            {
                result.SetProperty("ai/handoff", request.Handoff);
            }

            // --- All-or-nothing consistency check ---
            string? w1 = ctx.CheckAllOrNothing("ai/exploitability", request.Exploitability != null);
            if (w1 != null) { AddWarning(ref warnings, w1); }

            string? w2 = ctx.CheckAllOrNothing("ai/attackerPosition", request.AttackerPosition != null);
            if (w2 != null) { AddWarning(ref warnings, w2); }

            // --- Pre-insert enrichment: walk all locations for artifact caching ---
            EnrichAllLocations(ctx, result);

            int resultIndex = ctx.AddResult(result);

            return new EnrichmentResult
            {
                ResultGuid = result.Guid?.ToString() ?? string.Empty,
                ResultIndex = resultIndex,
                RuleRegistered = !alreadyRegistered,
                Warnings = warnings ?? new List<string>()
            };
        }

        /// <summary>
        /// Walks every location in a result (including code-flow steps, related
        /// locations, stack frames, fix artifact changes) and enriches each with
        /// artifact index, snippets, context regions, and charOffset/charLength.
        /// </summary>
        private static void EnrichAllLocations(SarifRunContext ctx, Result result)
        {
            foreach (WalkedLocation walked in LocationWalker.Walk(result))
            {
                if (walked.ArtifactLocation == null)
                {
                    continue;
                }

                string? uri = walked.ArtifactLocation.Uri?.OriginalString;
                if (string.IsNullOrEmpty(uri))
                {
                    continue;
                }

                walked.ArtifactLocation.Index = ctx.EnsureArtifact(uri);

                if (walked.CanHaveRegion && walked.PhysicalLocation?.Region != null)
                {
                    string? absolutePath = ctx.ResolveToAbsolutePath(uri);
                    if (absolutePath != null)
                    {
                        var fileUri = new Uri(absolutePath);
                        walked.PhysicalLocation.Region = ctx.FileRegionsCache.PopulateTextRegionProperties(
                            walked.PhysicalLocation.Region, fileUri, populateSnippet: true);

                        walked.PhysicalLocation.ContextRegion ??=
                            ctx.FileRegionsCache.ConstructMultilineContextSnippet(
                                walked.PhysicalLocation.Region, fileUri);
                    }
                }
            }
        }

        private static FailureLevel ParseLevel(string? level) => level?.ToLowerInvariant() switch
        {
            "error" => FailureLevel.Error,
            "warning" => FailureLevel.Warning,
            "note" => FailureLevel.Note,
            _ => FailureLevel.Warning
        };

        private static void ValidateExploitability(string? value)
        {
            if (value == null) { return; }
            if (Array.IndexOf(s_allowedExploitability, value.ToLowerInvariant()) < 0)
            {
                throw new ArgumentException(
                    $"Invalid exploitability '{value}'. Allowed: {string.Join(", ", s_allowedExploitability)}.");
            }
        }

        private static void ValidateAttackerPosition(string? value, ref List<string>? warnings)
        {
            if (value == null) { return; }
            if (Array.IndexOf(s_recommendedAttackerPosition, value.ToLowerInvariant()) < 0)
            {
                AddWarning(
                    ref warnings,
                    $"attacker_position '{value}' is not in the recommended vocabulary " +
                    $"({string.Join(", ", s_recommendedAttackerPosition)}). Value accepted but may reduce interop.");
            }
        }

        private static void ValidateLevel(string? value)
        {
            if (value == null) { return; }
            if (Array.IndexOf(s_allowedLevels, value.ToLowerInvariant()) < 0)
            {
                throw new ArgumentException(
                    $"Invalid level '{value}'. Allowed: {string.Join(", ", s_allowedLevels)}. " +
                    "('none' is only valid when kind is not 'fail'.)");
            }
        }

        private static void AddWarning(ref List<string>? warnings, string message)
        {
            warnings ??= new List<string>();
            warnings.Add(message);
        }
    }

    // --- Request/Response DTOs ---

    internal sealed class AddResultRequest
    {
        public string RuleId { get; set; } = string.Empty;

        public string? RuleName { get; set; }

        public string? RuleDescription { get; set; }

        public string? Level { get; set; }

        public double? Rank { get; set; }

        public string MessageText { get; set; } = string.Empty;

        public string? MessageMarkdown { get; set; }

        public LocationRequest? Location { get; set; }

        public List<LogicalLocationRequest>? LogicalLocations { get; set; }

        public string? Exploitability { get; set; }

        public string? AttackerPosition { get; set; }

        public string? Handoff { get; set; }
    }

    internal sealed class LocationRequest
    {
        public string Uri { get; set; } = string.Empty;

        public int? StartLine { get; set; }

        public int? StartColumn { get; set; }

        public int? EndLine { get; set; }

        public int? EndColumn { get; set; }

        public string? Snippet { get; set; }
    }

    internal sealed class LogicalLocationRequest
    {
        public string? Name { get; set; }

        public string? FullyQualifiedName { get; set; }

        public string? Kind { get; set; }
    }

    internal sealed class EnrichmentResult
    {
        public string ResultGuid { get; set; } = string.Empty;

        public int ResultIndex { get; set; }

        public bool RuleRegistered { get; set; }

        public List<string> Warnings { get; set; } = new();
    }
}
