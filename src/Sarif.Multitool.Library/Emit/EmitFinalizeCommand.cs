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
        /// <summary>
        /// Property name stamped on <c>run.properties</c> by <c>--no-repo</c> to record that a run
        /// was finalized without version-control provenance and therefore cannot be uploaded to a
        /// code-scanning alert store. Read by the publish verbs to refuse early.
        /// </summary>
        public const string UnpublishablePropertyName = "unpublishable";

        /// <summary>
        /// Reports whether <paramref name="log"/> carries any run stamped unpublishable by
        /// <c>emit-finalize --no-repo</c>. Publishing ingests every run in a file, so a single
        /// unpublishable run makes the whole log unpublishable. Shared by the publish verbs so the
        /// log-level refusal is enforced from one implementation.
        /// </summary>
        public static bool IsMarkedUnpublishable(SarifLog log)
        {
            if (log?.Runs == null) { return false; }

            foreach (Run run in log.Runs)
            {
                if (run != null
                    && run.TryGetProperty(UnpublishablePropertyName, out bool unpublishable)
                    && unpublishable)
                {
                    return true;
                }
            }

            return false;
        }

        public int Run(EmitFinalizeOptions options, IFileSystem fileSystem = null)
        {
            fileSystem ??= Sarif.FileSystem.Instance;

            try
            {
                int code = TryLoadCombinedLog(
                    options,
                    fileSystem,
                    out SarifLog log,
                    out List<string> wipPaths);
                if (code != SUCCESS) { return code; }

                string outputPath = Path.GetFullPath(options.OutputFilePath);

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
                OptionallyEmittedData baseEnrichmentFlags =
                    OptionallyEmittedData.Hashes |
                    OptionallyEmittedData.RegionSnippets |
                    OptionallyEmittedData.ContextRegionSnippets |
                    OptionallyEmittedData.ComprehensiveRegionProperties;

                if (options.EmbedTextFiles)
                {
                    // Self-contained AI fixtures want the source bytes inline so a
                    // consumer can render snippets and re-derive regions without
                    // any filesystem access. Clears SARIF2013.
                    baseEnrichmentFlags |= OptionallyEmittedData.TextFiles;
                }

                // The shipped SARIF carries three finalize enrichments with different audiences:
                //
                //   * rule-level security-severity — a host-agnostic severity prior stamped on
                //     tool.driver.rules[]. Both GitHub Advanced Security and Azure DevOps Advanced
                //     Security read security-severity off a rule to bucket a result
                //     (critical/high/medium/low). A curated per-CWE value (CweSecuritySeverity) is
                //     used when one exists; uncurated content (a CWE outside the curated table, or a
                //     NOVEL- finding) gets a neutral medium default so no AI security rule ships
                //     without a severity. Applied to every run.
                //   * rule-level tags (security, external/cwe/cwe-<n>) — GitHub Advanced Security
                //     ignores security-severity unless the rule's properties.tags also carries the
                //     "security" tag, and keys CWE association off external/cwe/cwe-<n>. Azure DevOps
                //     Advanced Security does not require these, so they are GitHub-only.
                //   * primaryLocationLineHash       — the rolling-hash partial fingerprint. GitHub's raw
                //     code-scanning SARIF upload API does not backfill partialFingerprints (the
                //     upload-sarif Action does), so emitting it ourselves is what prevents duplicate
                //     alerts on API-upload pipelines. This one is GitHub-only.
                //   * result ruleId sub-id collapse — GitHub's code-scanning security classifier binds a
                //     result to its rule by ruleId-string equality with a reportingDescriptor.id, and does
                //     not honor the SARIF hierarchical-ruleId / ruleIndex resolution ('3.27.5, '3.27.6) for
                //     this purpose. A result emitted under a sub-id ('CWE-79/<sub>') therefore never matches
                //     its base 'CWE-79' descriptor, so the security-severity and tags above are silently
                //     dropped. We collapse the result's ruleId to its descriptor id for GitHub-hosted runs so
                //     GitHub can find the rule. The sub-id is legal SARIF ('3.27.5) but carries no descriptor
                //     metadata ('3.49.3 NOTE 2); Azure DevOps is unaffected, so this one is GitHub-only.
                if (log?.Runs != null)
                {
                    foreach (Run run in log.Runs)
                    {
                        if (run == null) { continue; }

                        ApplyAISecuritySeverity(run);

                        bool isGitHubHosted = VcpPortableRoot.IsGitHubHostedRun(run);

                        if (isGitHubHosted)
                        {
                            ApplyGitHubCweTags(run);
                            CollapseResultRuleSubIds(run);
                        }

                        OptionallyEmittedData runFlags = isGitHubHosted
                            ? baseEnrichmentFlags | OptionallyEmittedData.RollingHashPartialFingerprints
                            : baseEnrichmentFlags;

                        var visitor = new InsertOptionalDataVisitor(
                            runFlags,
                            new FileRegionsCache(),
                            originalUriBaseIds: run.OriginalUriBaseIds);
                        visitor.VisitRun(run);
                    }
                }

                // After enrichment reads sources from the local file:// bases, deconstruct every
                // absolute local path into a relative URI plus a portable, per-repository uriBaseId
                // derived from versionControlProvenance, so the shipped SARIF anchors at stable,
                // host-independent permalinks and carries no machine-specific path.
                if (log?.Runs != null)
                {
                    var rebaseVisitor = new EmitFinalizeRebaseVisitor(options.NoRepo);
                    foreach (Run run in log.Runs)
                    {
                        if (run != null) { rebaseVisitor.VisitRun(run); }
                    }

                    if (!rebaseVisitor.Success)
                    {
                        foreach (string rebaseError in rebaseVisitor.Errors)
                        {
                            Console.Error.WriteLine(rebaseError);
                        }

                        return FAILURE;
                    }

                    // A repo-less finalize produced a leak-free log with no portable repository
                    // root; stamp each run so the publish verbs refuse it early rather than letting
                    // the producer discover the rejection at upload time.
                    if (options.NoRepo)
                    {
                        foreach (Run run in log.Runs)
                        {
                            if (run != null) { run.SetProperty(UnpublishablePropertyName, true); }
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
                    foreach (string wipPath in wipPaths)
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
        /// Loads the staged event log(s) this finalize run will replay and reports the set of
        /// <c>.wip.jsonl</c> paths consumed (so the caller can clean them up).
        /// </summary>
        /// <remarks>
        /// Two forms, mutually exclusive:
        /// <list type="bullet">
        /// <item><description>
        /// <b>Single (default).</b> With no <c>--inputs</c>, the staged log is derived from the
        /// output path as <c>&lt;output&gt;.wip.jsonl</c> and replayed into a single-run log — the
        /// original behavior.
        /// </description></item>
        /// <item><description>
        /// <b>Multi (<c>--inputs</c>).</b> Each listed staged log is replayed, in caller-specified
        /// order, and its run(s) appended to one combined log, so <c>runs[i]</c> corresponds to the
        /// i-th input deterministically. This order-preservation is what cross-run <c>sarif:</c>
        /// result pointers depend on (unlike <c>merge</c>, which reorders runs). Every input is
        /// validated to exist before any is replayed, so a missing input fails clean with no
        /// partial work.
        /// </description></item>
        /// </list>
        /// Per-run enrichment downstream is identical for both forms — it iterates <c>log.Runs</c>
        /// and is indifferent to how the runs were assembled.
        /// </remarks>
        private static int TryLoadCombinedLog(
            EmitFinalizeOptions options,
            IFileSystem fileSystem,
            out SarifLog log,
            out List<string> wipPaths)
        {
            log = null;
            wipPaths = new List<string>();

            var inputs = options?.Inputs == null
                ? new List<string>()
                : new List<string>(options.Inputs);

            if (inputs.Count == 0)
            {
                int code = EmitEventLogHelpers.TryResolveWipPath(
                    options?.OutputFilePath,
                    fileSystem,
                    out string wipPath);
                if (code != SUCCESS) { return code; }

                wipPaths.Add(wipPath);
                log = SarifEventReplayer.Replay(wipPath);
                return SUCCESS;
            }

            foreach (string input in inputs)
            {
                if (string.IsNullOrWhiteSpace(input))
                {
                    Console.Error.WriteLine("--inputs contains an empty path.");
                    return FAILURE;
                }

                string fullPath = Path.GetFullPath(input);

                if (!fileSystem.FileExists(fullPath))
                {
                    Console.Error.WriteLine(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            "Event log '{0}' does not exist.",
                            fullPath));
                    return FAILURE;
                }

                wipPaths.Add(fullPath);
            }

            SarifLog combined = null;
            foreach (string wipPath in wipPaths)
            {
                SarifLog replayed = SarifEventReplayer.Replay(wipPath);

                if (combined == null)
                {
                    combined = replayed;
                    combined.Runs ??= new List<Run>();
                }
                else if (replayed?.Runs != null)
                {
                    foreach (Run run in replayed.Runs)
                    {
                        combined.Runs.Add(run);
                    }
                }
            }

            log = combined;
            return SUCCESS;
        }

        /// <summary>
        /// Stamps each AI rule descriptor with a <c>security-severity</c> value: the curated per-CWE
        /// prior from <see cref="CweSecuritySeverity"/> when one exists, otherwise an emit-time default
        /// (<see cref="DefaultSecuritySeverity"/>) for uncurated content — a CWE outside the curated
        /// table, or a <c>NOVEL-</c> finding that maps to no CWE at all.
        /// </summary>
        /// <remarks>
        /// Both GitHub and Azure DevOps read <c>security-severity</c> off the rule a result
        /// references — never off a result's <c>rank</c> — so the value is a property of the
        /// weakness class, not of any one finding. The curated table is the SDK's hand-maintained
        /// severity prior, seeded from the CodeQL query corpus; it deliberately under-covers the long
        /// tail of weakness classes an AI producer can emit. Rather than let those findings reach a
        /// platform with no severity (degrading to level-based bucketing), the emit pipeline supplies a
        /// neutral <c>medium</c> default so every AI security rule carries a value. The default is an
        /// emit-verb policy, not a curated datum — <see cref="CweSecuritySeverity"/> still reports
        /// uncurated CWEs as having no prior. A descriptor whose id is neither a CWE nor a
        /// <c>NOVEL-</c> id is left bare, and a producer-authored <c>security-severity</c> is left
        /// untouched. Severity (this value) and confidence (<see cref="Result.Rank"/>) are orthogonal;
        /// neither is derived from the other.
        /// </remarks>
        /// <returns>The number of rule descriptors stamped.</returns>
        internal static int ApplyAISecuritySeverity(Run run)
        {
            IList<ReportingDescriptor> rules = run?.Tool?.Driver?.Rules;
            if (rules == null || rules.Count == 0) { return 0; }

            int stamped = 0;
            foreach (ReportingDescriptor rule in rules)
            {
                if (rule == null || string.IsNullOrEmpty(rule.Id)) { continue; }
                if (rule.PropertyNames.Contains(SecuritySeverityPropertyName)) { continue; }

                bool isCwe = CweSecuritySeverity.TryGetCweNumber(rule.Id, out int cweNumber);
                bool isNovel = rule.Id.StartsWith(NovelIdPrefix, StringComparison.Ordinal);
                if (!isCwe && !isNovel) { continue; }

                double securitySeverity =
                    isCwe && CweSecuritySeverity.TryGetSecuritySeverity(cweNumber, out double curated)
                        ? curated
                        : DefaultSecuritySeverity;

                rule.SetProperty(
                    SecuritySeverityPropertyName,
                    CweSecuritySeverity.FormatPropertyValue(securitySeverity));
                stamped++;
            }

            return stamped;
        }

        private const string SecuritySeverityPropertyName = CweSecuritySeverity.PropertyName;

        /// <summary>
        /// The emit-time <c>security-severity</c> prior stamped on an AI security rule that has no
        /// curated value — an uncurated CWE or a <c>NOVEL-</c> finding. A neutral <c>medium</c> on the
        /// GitHub/Azure DevOps 0.0–10.0 scale (medium band 4.0–6.9).
        /// </summary>
        internal const double DefaultSecuritySeverity = 5.0;


        internal const string TagsPropertyName = "tags";
        internal const string SecurityTag = "security";
        private const string CweTagPrefix = "external/cwe/cwe-";
        private const string NovelIdPrefix = "NOVEL-";

        /// <summary>
        /// Stamps each AI rule descriptor with the GitHub Advanced Security tags it needs to be
        /// treated as a security finding. A CWE-as-rule descriptor gets <c>security</c> plus
        /// <c>external/cwe/cwe-&lt;n&gt;</c> (where <c>&lt;n&gt;</c> is the CWE number carried in the
        /// descriptor <c>id</c>); a <c>NOVEL-</c> descriptor — a real vulnerability that maps to no
        /// CWE — gets the bare <c>security</c> tag only.
        /// </summary>
        /// <remarks>
        /// GitHub ignores a rule's <c>security-severity</c> unless <c>properties.tags</c> also carries
        /// the <c>security</c> tag, and keys CWE association off <c>external/cwe/cwe-&lt;n&gt;</c>; this
        /// is what surfaces the critical/high/medium/low badge and the CWE link in the code-scanning UI.
        /// The <c>security</c> tag is what makes GitHub classify the finding as a security alert at all,
        /// so it is applied to <c>NOVEL-</c> descriptors too even though no <c>external/cwe</c> tag fits.
        /// Azure DevOps Advanced Security does not require these tags, so the caller applies this only to
        /// GitHub-hosted runs. A descriptor whose id is neither a CWE nor a <c>NOVEL-</c> id receives
        /// nothing, and any producer-authored tags are preserved (the tags are merged in, not
        /// overwritten). Tags are written as an ordered list so the output is deterministic — the
        /// <see cref="TagsCollection"/> is HashSet-backed and per-process string-hash randomization would
        /// otherwise reorder it between runs.
        /// </remarks>
        /// <returns>The number of rule descriptors whose tags were modified.</returns>
        internal static int ApplyGitHubCweTags(Run run)
        {
            IList<ReportingDescriptor> rules = run?.Tool?.Driver?.Rules;
            if (rules == null || rules.Count == 0) { return 0; }

            int stamped = 0;
            foreach (ReportingDescriptor rule in rules)
            {
                if (rule == null || string.IsNullOrEmpty(rule.Id)) { continue; }

                string cweTag = null;
                if (CweSecuritySeverity.TryGetCweNumber(rule.Id, out int cweNumber))
                {
                    cweTag = CweTagPrefix + cweNumber.ToString(CultureInfo.InvariantCulture);
                }
                else if (!rule.Id.StartsWith(NovelIdPrefix, StringComparison.Ordinal))
                {
                    continue;
                }

                var tags = new List<string>();
                if (rule.TryGetProperty(TagsPropertyName, out List<string> existing) && existing != null)
                {
                    tags.AddRange(existing);
                }

                bool modified = false;
                if (!tags.Contains(SecurityTag)) { tags.Add(SecurityTag); modified = true; }
                if (cweTag != null && !tags.Contains(cweTag)) { tags.Add(cweTag); modified = true; }

                if (modified)
                {
                    rule.SetProperty(TagsPropertyName, tags);
                    stamped++;
                }
            }

            return stamped;
        }

        private const string RuleSubIdSeparator = "/";

        /// <summary>
        /// Collapses each result's <c>ruleId</c> (and the <c>id</c> of its <c>rule</c> reference, if
        /// present) to the id of the reportingDescriptor the result resolves to, dropping any trailing
        /// hierarchical sub-id component.
        /// </summary>
        /// <remarks>
        /// A result <c>ruleId</c> is a hierarchical string whose leading components are the rule's stable
        /// id and whose optional trailing component is a sub-id that more precisely describes the
        /// violation (SARIF '3.27.5). A reportingDescriptor's <c>id</c> always names an entire rule, never
        /// a sub-rule ('3.49.3 NOTE 2), so a result emitted as <c>CWE-79/&lt;sub&gt;</c> correctly resolves
        /// to its <c>CWE-79</c> descriptor via <c>ruleIndex</c>/<c>rule</c>. GitHub's code-scanning security
        /// classifier, however, binds a result to its rule by <c>ruleId</c>-string equality with a
        /// descriptor <c>id</c> and does not follow the <c>ruleIndex</c>/<c>rule.index</c> resolution for
        /// this purpose; under a sub-id the result matches no descriptor, and the <c>security-severity</c>
        /// and <c>tags</c> stamped on the descriptor are silently ignored. Collapsing the result's
        /// <c>ruleId</c> to its descriptor id lets GitHub find the rule. The sub-id carries no descriptor
        /// metadata, so nothing classifiable is lost; only the GitHub-hosted output is rewritten (callers
        /// gate this), leaving Azure DevOps-hosted SARIF — which resolves the rule correctly — with the
        /// sub-id intact. <c>ruleId</c> and <c>rule.id</c> are kept equal so the result stays valid SARIF
        /// ('3.27.7). A non-hierarchical id (including a <c>NOVEL-</c> id) already equals its descriptor id
        /// and is left unchanged.
        /// </remarks>
        /// <returns>The number of results whose ruleId was collapsed.</returns>
        internal static int CollapseResultRuleSubIds(Run run)
        {
            IList<Result> results = run?.Results;
            if (results == null || results.Count == 0) { return 0; }

            int collapsed = 0;
            foreach (Result result in results)
            {
                if (result == null) { continue; }

                string descriptorId = result.GetRule(run)?.Id;
                if (string.IsNullOrEmpty(descriptorId)) { continue; }

                string subIdPrefix = descriptorId + RuleSubIdSeparator;
                bool changed = false;

                if (result.RuleId != null
                    && result.RuleId.StartsWith(subIdPrefix, StringComparison.Ordinal))
                {
                    result.RuleId = descriptorId;
                    changed = true;
                }

                if (result.Rule?.Id != null
                    && result.Rule.Id.StartsWith(subIdPrefix, StringComparison.Ordinal))
                {
                    result.Rule.Id = descriptorId;
                    changed = true;
                }

                if (changed) { collapsed++; }
            }

            return collapsed;
        }


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
