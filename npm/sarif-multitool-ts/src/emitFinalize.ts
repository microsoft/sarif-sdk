// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * `emit-finalize`: replays `<output>.wip.jsonl`, optionally enriches CWE
 * descriptors, populates region snippets / hashes, rebases local paths to
 * portable per-repository roots, and atomically writes the destination SARIF.
 *
 * Ported from src/Sarif.Multitool.Library/Emit/EmitFinalizeCommand.cs.
 *
 * `--validate` is DEFERRED in this package — it returns a notice pointing at
 * the .NET `sarif validate` verb. See README#Validation.
 */

import { promises as fs } from 'node:fs';
import { resolve as resolvePath } from 'node:path';
import {
  type SarifLog,
  atomicWrite,
  serializeSarifLog,
  isGitHubHostedRun,
  insertOptionalData,
} from '@microsoft/sarif';
import { replay } from './eventLog.js';
import { resolveWipPath, EmitVerbError } from './batch.js';
import {
  enrichRunWithCweTaxonomy,
  applyAISecuritySeverity,
  applyGitHubCweTags,
  collapseResultRuleSubIds,
} from './cwe.js';
import { rebaseRun } from './rebase.js';

export interface EmitFinalizeOptions {
  /** Final SARIF file path. The event log is read from `<output>.wip.jsonl`. */
  output: string;
  /** Skip CWE descriptor enrichment. */
  noCweEnrichment?: boolean;
  /** Write minified JSON instead of two-space indented. */
  minify?: boolean;
  /** Keep the .wip.jsonl after a successful finalize (default: delete). */
  keepWip?: boolean;
  /**
   * DEFERRED. The .NET tool runs the SARIF+AI analyzer rule set; this package
   * emits a deferral notice instead. Run `sarif validate --rule-kind "Sarif;AI"`
   * in your test/CI environment as the backstop.
   */
  validate?: boolean;
}

export interface EmitFinalizeOutcome {
  outputPath: string;
  resultCount: number;
  ruleCount: number;
  warnings: string[];
  log: SarifLog;
}

export async function emitFinalize(opts: EmitFinalizeOptions): Promise<EmitFinalizeOutcome> {
  const warnings: string[] = [];

  const wipPath = resolveWipPath(opts.output);
  const outputPath = resolvePath(opts.output);

  const log = await replay(wipPath);

  if (!opts.noCweEnrichment) {
    for (const run of log.runs ?? []) {
      if (run) enrichRunWithCweTaxonomy(run);
    }
  }

  // Enrichment passes — see EmitFinalizeCommand.cs for the per-pass rationale.
  // RollingHashPartialFingerprints is a known v1 gap (GitHub-only dedup hint);
  // tracked in README#Known-gaps.
  for (const run of log.runs ?? []) {
    if (!run) continue;

    applyAISecuritySeverity(run);

    if (isGitHubHostedRun(run)) {
      applyGitHubCweTags(run);
      collapseResultRuleSubIds(run);
    }

    insertOptionalData(run, {
      hashes: true,
      regionSnippets: true,
      contextRegionSnippets: true,
      comprehensiveRegionProperties: true,
    });
  }

  // Rebase local paths to portable roots AFTER enrichment reads the local
  // file:// bases.
  for (const run of log.runs ?? []) {
    if (!run) continue;
    const r = rebaseRun(run);
    if (!r.success) {
      throw new EmitVerbError(r.errors.join('\n'));
    }
  }

  await atomicWrite(outputPath, serializeSarifLog(log, !opts.minify));

  let resultCount = 0,
    ruleCount = 0;
  for (const run of log.runs ?? []) {
    resultCount += run?.results?.length ?? 0;
    ruleCount += run?.tool?.driver?.rules?.length ?? 0;
  }

  if (!opts.keepWip) {
    try {
      await fs.unlink(wipPath);
    } catch (err) {
      warnings.push(`Warning — could not delete '${wipPath}': ${(err as Error).message}`);
    }
  }

  if (opts.validate) {
    warnings.push(
      '--validate is not implemented in @microsoft/sarif-emit v1. The output SARIF was written successfully; ' +
        'run the .NET multitool `sarif validate --rule-kind "Sarif;AI" ' +
        outputPath +
        '` in your test/CI environment as the validation backstop. See README#Validation.',
    );
  }

  return { outputPath, resultCount, ruleCount, warnings, log };
}
