// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * Whole-log JSON Schema validation for the SARIF that `emit-finalize` writes.
 * Validates against `ai-sarif-log.schema.json` (draft 2020-12) — the AI emit
 * profile overlay on the canonical SARIF 2.1.0 document schema. The overlay
 * `$ref`s the full SARIF 2.1.0 schema by its schemastore `$id`; the base schema
 * is bundled under assets/ and registered under that id so validation runs
 * fully offline.
 */

import { readFileSync } from 'node:fs';
import type { SarifLog } from '@microsoft/sarif';
import { Ajv2020 } from 'ajv/dist/2020.js';
import type { ErrorObject, ValidateFunction } from 'ajv';
import _addFormats from 'ajv-formats';
import { assetPath } from './assets.js';
import { getSchema } from './getSchema.js';

// ajv-formats publishes a CJS default export that NodeNext types as a module
// namespace rather than the callable plugin; its `.default` carries the call
// signature, and at runtime the import already is that function.
const addFormats = _addFormats as unknown as typeof _addFormats.default;

// The schemastore `$id` the ai-sarif-log overlay `$ref`s. The bundled SARIF
// 2.1.0 base schema is registered under it so the overlay's refs to the base
// document, `#/definitions/run`, and `#/definitions/result` resolve.
const SARIF_2_1_0_ID = 'https://json.schemastore.org/sarif-2.1.0.json';

export interface ValidationDetail {
  /** JSON Pointer into the validated log (`''` denotes the document root). */
  instancePath: string;
  /** The ajv failure message. */
  message: string;
  /** The ajv keyword that failed (e.g. `required`, `type`, `enum`). */
  keyword: string;
}

export interface ValidationOutcome {
  /** True when the finalized log conforms to ai-sarif-log.schema.json. */
  valid: boolean;
  /** Human-readable `instancePath: message` strings, empty when valid. */
  errors: string[];
  /** Structured per-error detail (channeling + report file), empty when valid. */
  details: ValidationDetail[];
}

let cachedValidate: ValidateFunction | undefined;

function buildValidator(): ValidateFunction {
  const ajv = new Ajv2020({ strict: false, allErrors: true });
  addFormats(ajv);

  const base = JSON.parse(
    readFileSync(assetPath('assets', 'sarif-2.1.0.schema.json'), 'utf8'),
  ) as Record<string, unknown>;
  ajv.addSchema(base, SARIF_2_1_0_ID);

  const overlay = JSON.parse(getSchema('emit-finalize')) as Record<string, unknown>;
  return ajv.compile(overlay);
}

function buildDetails(errors: readonly ErrorObject[]): ValidationDetail[] {
  const seen = new Set<string>();
  const out: ValidationDetail[] = [];
  for (const e of errors) {
    const detail: ValidationDetail = {
      instancePath: e.instancePath,
      message: e.message ?? 'is invalid',
      keyword: e.keyword,
    };
    const key = `${detail.instancePath}: ${detail.message}`;
    if (!seen.has(key)) {
      seen.add(key);
      out.push(detail);
    }
  }
  return out;
}

/** Renders one detail as the legacy `instancePath: message` string (root shown as `/`). */
export function formatDetail(detail: ValidationDetail): string {
  const where = detail.instancePath === '' ? '/' : detail.instancePath;
  return `${where}: ${detail.message}`;
}

/**
 * Validates a finalized SARIF log against `ai-sarif-log.schema.json`. The
 * compiled validator is cached across calls. Never throws on a non-conforming
 * log — the verdict is carried in the returned outcome.
 */
export function validateFinalizedLog(log: SarifLog): ValidationOutcome {
  cachedValidate ??= buildValidator();
  const valid = cachedValidate(log) as boolean;
  if (valid) {
    return { valid: true, errors: [], details: [] };
  }
  const details = buildDetails(cachedValidate.errors ?? []);
  return { valid: false, errors: details.map(formatDetail), details };
}
