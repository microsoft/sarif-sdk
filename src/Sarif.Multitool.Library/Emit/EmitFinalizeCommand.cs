// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
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
    /// Implements <c>emit-finalize</c>: replays <c>&lt;output&gt;.wip.jsonl</c>,
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

                if (!EmitEventLogHelpers.TryValidateUri(options.SrcRoot, "--srcroot", EmitEventLogHelpers.BaseUriSchemes, out string srcRootError))
                {
                    Console.Error.WriteLine(srcRootError);
                    return FAILURE;
                }

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
                // OverwriteExistingData is intentionally NOT set. Beyond preserving any
                // fields a producer already populated, leaving it unset also validates
                // recomputed region coordinates: if a producer authored a region coordinate
                // that does not match the source text, emit-finalize fails (ArgumentException)
                // rather than silently shipping a region that points at the wrong span. A
                // caller that wants divergent coordinates recomputed (overwritten) instead
                // would set OverwriteExistingData.
                OptionallyEmittedData enrichmentFlags =
                    OptionallyEmittedData.Hashes |
                    OptionallyEmittedData.RegionSnippets |
                    OptionallyEmittedData.ContextRegionSnippets |
                    OptionallyEmittedData.ComprehensiveRegionProperties;

                if (options.EmbedTextFiles)
                {
                    // Self-contained AI fixtures want the source bytes inline so a
                    // consumer can render snippets and re-derive regions without
                    // any filesystem access. Clears SARIF2013.
                    enrichmentFlags |= OptionallyEmittedData.TextFiles;
                }

                if (log?.Runs != null)
                {
                    foreach (Run run in log.Runs)
                    {
                        var visitor = new InsertOptionalDataVisitor(
                            enrichmentFlags,
                            new FileRegionsCache(),
                            originalUriBaseIds: run?.OriginalUriBaseIds);
                        visitor.VisitRun(run);
                    }
                }

                // After enrichment, optionally rewrite originalUriBaseIds["SRCROOT"].uri.
                // Producers commonly emit with a local file:// SRCROOT so the visitor above
                // can read sources for snippets/hashes/contextRegion, then ship the SARIF
                // with a canonical, host-independent URI (e.g. a GitHub blob URL). Doing
                // the swap here — after the visitor, before serialization — keeps both
                // contracts intact: enrichment uses real files; the artifact ships portable.
                if (!string.IsNullOrWhiteSpace(options.SrcRoot) && log?.Runs != null)
                {
                    var finalSrcRoot = new Uri(options.SrcRoot, UriKind.Absolute);
                    foreach (Run run in log.Runs)
                    {
                        if (run == null) { continue; }

                        run.OriginalUriBaseIds ??= new Dictionary<string, ArtifactLocation>();
                        if (run.OriginalUriBaseIds.TryGetValue(EmitRunCommand.SourceRootBaseId, out ArtifactLocation existing) && existing != null)
                        {
                            existing.Uri = finalSrcRoot;
                        }
                        else
                        {
                            run.OriginalUriBaseIds[EmitRunCommand.SourceRootBaseId] = new ArtifactLocation
                            {
                                Uri = finalSrcRoot,
                            };
                        }
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

                if (options.Validate)
                {
                    int validateExit = RunValidatorAndReport(outputPath);
                    if (validateExit != SUCCESS)
                    {
                        return validateExit;
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

        /// <summary>
        /// Runs the multitool validator (--rule-kind Sarif;AI) against the finalized SARIF.
        /// Prints a one-line summary of Error/Warning/Note counts and (on Error) the rule IDs
        /// that fired. Returns FAILURE if any Error-level finding is reported; otherwise SUCCESS.
        /// </summary>
        internal static int RunValidatorAndReport(string outputPath)
        {
            string reportPath = Path.Combine(
                Path.GetDirectoryName(outputPath) ?? string.Empty,
                Path.GetFileNameWithoutExtension(outputPath) + ".validate-report.sarif");

            try
            {
                var validateOptions = new ValidateOptions
                {
                    TargetFileSpecifiers = new List<string> { outputPath },
                    OutputFilePath = reportPath,
                    OutputFileOptions = new[] { Sarif.FilePersistenceOptions.ForceOverwrite },
                    Quiet = true,
                    RuleKindOption = new[] { RuleKind.Sarif, RuleKind.AI },
                };

                new ValidateCommand().Run(validateOptions);

                if (!File.Exists(reportPath))
                {
                    Console.Error.WriteLine(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            "--validate: validator did not produce a report at '{0}'.",
                            reportPath));
                    return FAILURE;
                }

                SarifLog report = SarifLog.Load(reportPath);
                int errors = 0, warnings = 0, notes = 0;
                var errorRuleIds = new SortedSet<string>(StringComparer.Ordinal);
                if (report?.Runs != null)
                {
                    foreach (Run vrun in report.Runs)
                    {
                        if (vrun?.Results == null) { continue; }
                        foreach (Result vr in vrun.Results)
                        {
                            switch (vr.Level)
                            {
                                case FailureLevel.Error:
                                    errors++;
                                    if (!string.IsNullOrEmpty(vr.RuleId))
                                    {
                                        errorRuleIds.Add(vr.RuleId);
                                    }
                                    break;
                                case FailureLevel.Warning:
                                    warnings++;
                                    break;
                                case FailureLevel.Note:
                                    notes++;
                                    break;
                            }
                        }
                    }
                }

                Console.Out.WriteLine(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        "--validate: {0} error(s), {1} warning(s), {2} note(s) [Sarif+AI].",
                        errors,
                        warnings,
                        notes));

                if (errors > 0)
                {
                    Console.Error.WriteLine(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            "--validate: failing on Error-level rule(s): {0}. See '{1}' for details.",
                            string.Join(", ", errorRuleIds),
                            reportPath));
                    return FAILURE;
                }

                // Clean up the report on success — Errors==0 means it carries only the noisy
                // Warning/Note findings the caller has already been told the counts of.
                try { File.Delete(reportPath); } catch { /* janitorial */ }
                return SUCCESS;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        "--validate: failed to run validator: {0}",
                        ex.Message));
                return FAILURE;
            }
        }
    }
}
