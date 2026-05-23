// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

using Microsoft.CodeAnalysis.Sarif.Driver;
using Microsoft.CodeAnalysis.Sarif.Emit;
using Microsoft.CodeAnalysis.Sarif.Taxonomies;
using Microsoft.CodeAnalysis.Sarif.Visitors;

using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    /// <summary>
    /// Implements <c>multitool emit-finalize</c>: replays <c>&lt;output&gt;.wip.jsonl</c>,
    /// optionally enriches CWE-as-rule-id descriptors, and atomically writes the destination
    /// SARIF file.
    /// </summary>
    public class EmitFinalizeCommand : CommandBase
    {
        public int Run(EmitFinalizeOptions options, IFileSystem fileSystem = null)
        {
            fileSystem ??= Sarif.FileSystem.Instance;

            try
            {
                int code = EmitEventLogHelpers.TryResolveWipPath(
                    options?.OutputFilePath,
                    fileSystem,
                    out string wipPath);
                if (code != SUCCESS) { return code; }

                string outputPath = Path.GetFullPath(options.OutputFilePath);

                SarifLog log = SarifEventReplayer.Replay(wipPath);

                if (!options.NoCweEnrichment && log?.Runs != null)
                {
                    foreach (Run run in log.Runs)
                    {
                        CweTaxonomyEnricher.Enrich(run);
                    }
                }

                // Always populate the artifact and region surface that downstream
                // consumers (AI evidence pipelines, code-flow viewers, fingerprint
                // matchers) need to reason about a result without having to re-open
                // the source file themselves:
                //
                //   * Hashes                       — sha-256 on every artifact so a
                //                                    consumer can assert the analyzed
                //                                    content matches what they have.
                //   * RegionSnippets               — the literal source span the
                //                                    finding identifies.
                //   * ContextRegionSnippets        — a few lines of surrounding code
                //                                    for human / model review.
                //   * ComprehensiveRegionProperties — fill in charOffset/charLength
                //                                    so consumers can address the
                //                                    same span by offset, not just
                //                                    line/column.
                //
                // OverwriteExistingData is intentionally NOT set; producers that
                // have already populated any of these fields keep their values.
                const OptionallyEmittedData EnrichmentFlags =
                    OptionallyEmittedData.Hashes |
                    OptionallyEmittedData.RegionSnippets |
                    OptionallyEmittedData.ContextRegionSnippets |
                    OptionallyEmittedData.ComprehensiveRegionProperties;

                if (log?.Runs != null)
                {
                    foreach (Run run in log.Runs)
                    {
                        var visitor = new InsertOptionalDataVisitor(
                            EnrichmentFlags,
                            new FileRegionsCache(),
                            originalUriBaseIds: run?.OriginalUriBaseIds);
                        visitor.VisitRun(run);
                    }
                }

                Formatting formatting = options.Minify ? Formatting.None : Formatting.Indented;
                AtomicSarifWriter.Write(outputPath, stream =>
                {
                    using var sw = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
                    using var jw = new JsonTextWriter(sw) { Formatting = formatting };
                    var serializer = JsonSerializer.Create(new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        Formatting = formatting,
                    });
                    serializer.Serialize(jw, log);
                });

                int resultCount = 0;
                int ruleCount = 0;
                if (log?.Runs != null)
                {
                    foreach (Run r in log.Runs)
                    {
                        if (r.Results != null) { resultCount += r.Results.Count; }
                        if (r.Tool?.Driver?.Rules != null) { ruleCount += r.Tool.Driver.Rules.Count; }
                    }
                }

                Console.Out.WriteLine(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        "Wrote '{0}' ({1} result(s), {2} rule(s)).",
                        outputPath,
                        resultCount,
                        ruleCount));

                if (!options.KeepWip)
                {
                    try
                    {
                        fileSystem.FileDelete(wipPath);
                    }
                    catch (Exception delEx)
                    {
                        // Non-fatal: SARIF was successfully written; failing to remove the wip is
                        // a janitorial issue worth reporting but not worth aborting.
                        Console.Error.WriteLine(
                            string.Format(
                                CultureInfo.CurrentCulture,
                                "Warning — could not delete '{0}': {1}",
                                wipPath,
                                delEx.Message));
                    }
                }

                return SUCCESS;
            }
            catch (AIRuleIdConventionException ex)
            {
                // AI orchestrator-facing failure: stderr carries the exception message
                // verbatim (it is already shaped for AI consumption — see
                // AIRuleIdConventionException.BuildMessage). We deliberately do NOT
                // dump a stack trace here; the structured guidance is what the AI
                // needs to retry.
                Console.Error.WriteLine(ex.Message);
                return FAILURE;
            }
            catch (Exception ex) when (!Debugger.IsAttached)
            {
                Console.Error.WriteLine(ex);
                return FAILURE;
            }
        }
    }
}
