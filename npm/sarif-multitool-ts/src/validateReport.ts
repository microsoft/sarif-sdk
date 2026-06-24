// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * Channels the `emit-finalize --validate` verdict the way the emit batch verbs do, and
 * the way the .NET multitool does. A structured JSON receipt
 * (`{ conforms, profile, errorCount, warningCount, noteCount, reportPath, errors }`,
 * the full error set uncapped) is written to stdout on every run, pass or fail — the
 * machine-readable twin of the batch verbs' `{ appended, rejected }`. A non-conforming
 * run additionally writes a concise, human-readable summary (count header plus per-error
 * detail, capped at {@link MAX_STDERR_DETAILS}) to stderr — the channel a CI pipeline
 * reliably captures — and persists the complete set of findings to a
 * `<output>.validate-report.sarif` file for machine consumption.
 */

import { writeFileSync } from 'node:fs';
import path from 'node:path';
import process from 'node:process';
import type { ValidationDetail, ValidationOutcome } from './validate.js';

/** The schema this verb validates against; carried in the stdout receipt's `profile`. */
const PROFILE = 'ai-sarif-log.schema.json';

/**
 * Maximum number of findings whose detail is streamed to stderr before the
 * remainder collapses into a single "...and N more" line. Keeps a CI failure
 * log legible; the complete set is always in the persisted report file.
 */
export const MAX_STDERR_DETAILS = 20;

/** Derives `<dir>/<name>.validate-report.sarif` next to the finalized output. */
export function validationReportPath(outputPath: string): string {
  const dir = path.dirname(outputPath);
  const base = path.basename(outputPath, path.extname(outputPath));
  return path.join(dir, `${base}.validate-report.sarif`);
}

const where = (instancePath: string): string => (instancePath === '' ? '/' : instancePath);

/**
 * Synthesizes a minimal, valid SARIF 2.1.0 log carrying one Error-level result
 * per ajv finding. Each finding's keyword becomes the `ruleId`, its message the
 * result text, and its JSON Pointer a logical location — the structured,
 * machine-readable twin of the .NET validator's native report.
 */
export function buildValidationReportSarif(details: readonly ValidationDetail[]): unknown {
  return {
    $schema: 'https://json.schemastore.org/sarif-2.1.0.json',
    version: '2.1.0',
    runs: [
      {
        tool: {
          driver: {
            name: 'sarif-multitool-ts emit-finalize --validate',
            informationUri: 'https://github.com/microsoft/sarif-sdk',
            rules: [],
          },
        },
        results: details.map((d) => ({
          ruleId: d.keyword,
          level: 'error',
          message: { text: d.message },
          locations: [{ logicalLocations: [{ fullyQualifiedName: where(d.instancePath) }] }],
        })),
      },
    ],
  };
}

/**
 * Builds the structured stdout receipt: the verdict, the profile, the per-level counts
 * (TS validation is schema-only, so every finding is an error — warnings/notes are 0),
 * the persisted report path (null when conforming), and the full, uncapped error set.
 * Each error is the flat, pipeable twin of a stderr detail line: `{ ruleId, location,
 * message }`, where `ruleId` is the failing ajv keyword and `location` the JSON Pointer.
 */
export function buildValidationReceipt(
  outcome: ValidationOutcome,
  reportPath: string | null,
): Record<string, unknown> {
  return {
    conforms: outcome.valid,
    profile: PROFILE,
    errorCount: outcome.details.length,
    warningCount: 0,
    noteCount: 0,
    reportPath,
    errors: outcome.details.map((d) => ({
      ruleId: d.keyword,
      location: where(d.instancePath),
      message: d.message,
    })),
  };
}

/**
 * Reports the validation verdict and returns the process exit code (0 conforms,
 * 1 does not). The structured receipt is written to stdout on every run; on failure
 * the full report is also persisted next to the output and a capped human summary is
 * written to stderr.
 */
export function reportValidation(outputPath: string, outcome: ValidationOutcome): number {
  if (outcome.valid) {
    process.stdout.write(JSON.stringify(buildValidationReceipt(outcome, null), undefined, 2) + '\n');
    return 0;
  }

  const reportPath = validationReportPath(outputPath);
  writeFileSync(reportPath, JSON.stringify(buildValidationReportSarif(outcome.details), undefined, 2));

  process.stdout.write(
    JSON.stringify(buildValidationReceipt(outcome, reportPath), undefined, 2) + '\n',
  );

  const lines: string[] = [
    `--validate: ${outcome.details.length} error(s) — '${outputPath}' does not conform to ${PROFILE}.`,
  ];

  const shown = Math.min(outcome.details.length, MAX_STDERR_DETAILS);
  for (let i = 0; i < shown; i++) {
    const d = outcome.details[i];
    lines.push(`  ${d.keyword} @ ${where(d.instancePath)} — ${d.message}`);
  }
  if (outcome.details.length > shown) {
    lines.push(`  ...and ${outcome.details.length - shown} more error(s).`);
  }
  lines.push(`  Full report: '${reportPath}'.`);

  process.stderr.write(lines.join('\n') + '\n');
  return 1;
}
