// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * Shared orchestration for the polymorphic `add-*` emit verbs.
 * Ported from src/Sarif.Multitool.Library/Emit/EmitBatchProcessor.cs.
 *
 * Each verb accepts a single JSON object or an array of objects, validates
 * every element atomically, and appends all or none: if any element is
 * rejected the staged event log is left untouched. The outcome is the
 * structured result so an AI orchestrator can correct the offending elements
 * and retry idempotently.
 */

import { existsSync } from 'node:fs';
import { resolve as resolvePath } from 'node:path';
import { appendEvents, wipPathFor } from './eventLog.js';

export interface BatchElementError {
  /** Zero-based position in the submitted array (0 for a lone object). */
  index: number;
  /** Optional machine-readable code (e.g. `AI1012`) for the rejection. */
  errorCode?: string;
  /** Human/AI-consumable description of why the element was rejected. */
  message: string;
}

export interface BatchOutcome {
  appended: number;
  rejected: BatchElementError[];
}

/**
 * Validates a single batch element and may mutate it in place (e.g. stamping
 * a default). Returns `undefined` when the element is acceptable; otherwise
 * the error describing the rejection.
 *
 * @param element  The element to validate.
 * @param index    Zero-based position in the submitted payload.
 * @param batched  `true` when the payload arrived as an array; `false` for a
 *                 lone object. A validator that defaults a field from receipt
 *                 time uses this to refuse the default under batch submission,
 *                 where one write instant cannot stand in for many elements
 *                 assembled after the fact.
 */
export type ValidateBatchElement = (
  element: Record<string, unknown>,
  index: number,
  batched: boolean,
) => BatchElementError | undefined;

export class EmitVerbError extends Error {
  constructor(message: string) {
    super(message);
    this.name = 'EmitVerbError';
  }
}

function capitalize(s: string): string {
  return s ? s[0].toUpperCase() + s.slice(1) : s;
}

function jsonTypeName(v: unknown): string {
  if (v === null) return 'null';
  if (Array.isArray(v)) return 'array';
  return typeof v;
}

/**
 * Resolves the staged event-log path for an output SARIF path and verifies it
 * exists. Mirrors EmitEventLogHelpers.TryResolveWipPath.
 */
export function resolveWipPath(outputFilePath: string): string {
  if (!outputFilePath || !outputFilePath.trim()) {
    throw new EmitVerbError('Output SARIF path is required.');
  }
  const wip = wipPathFor(resolvePath(outputFilePath));
  if (!existsSync(wip)) {
    throw new EmitVerbError(`Event log '${wip}' does not exist; run 'emit-run' first.`);
  }
  return wip;
}

/**
 * Runs the polymorphic batch pipeline: split → validate-all → append-all-or-none.
 * Throws EmitVerbError for top-level shape problems (payload is neither object
 * nor array). Per-element problems are returned in the structured outcome.
 */
export async function processBatch(opts: {
  output: string;
  payload: unknown;
  payloadKind: string;
  eventKind: string;
  buildValidator: (wipPath: string) => ValidateBatchElement;
}): Promise<BatchOutcome> {
  const wipPath = resolveWipPath(opts.output);

  const elements: (Record<string, unknown> | null)[] = [];
  const errors: BatchElementError[] = [];
  let batched: boolean;

  const token = opts.payload;
  if (token !== null && typeof token === 'object' && !Array.isArray(token)) {
    batched = false;
    elements.push(token as Record<string, unknown>);
  } else if (Array.isArray(token)) {
    batched = true;
    for (let i = 0; i < token.length; i++) {
      const item = token[i];
      if (item !== null && typeof item === 'object' && !Array.isArray(item)) {
        elements.push(item as Record<string, unknown>);
      } else {
        // Keep the index aligned so the report cites the submitted position.
        elements.push(null);
        errors.push({
          index: i,
          message: `${capitalize(opts.payloadKind)} batch element must be a JSON object, but element ${i} was a JSON ${jsonTypeName(item)}.`,
        });
      }
    }
  } else {
    throw new EmitVerbError(
      `${capitalize(opts.payloadKind)} JSON must be a JSON object or an array of objects, but the parsed payload was a ${jsonTypeName(token)}.`,
    );
  }

  const validate = opts.buildValidator(wipPath);

  for (let index = 0; index < elements.length; index++) {
    const element = elements[index];
    if (element === null) continue; // already captured as a structural error
    const err = validate(element, index, batched);
    if (err) errors.push(err);
  }

  // Atomic: any rejection appends nothing, so a retry of the corrected payload
  // never double-appends the elements that were already valid.
  if (errors.length > 0) {
    errors.sort((a, b) => a.index - b.index);
    return { appended: 0, rejected: errors };
  }

  if (elements.length > 0) {
    await appendEvents(
      wipPath,
      elements.map((e) => ({ kind: opts.eventKind, payload: e! })),
    );
  }

  return { appended: elements.length, rejected: [] };
}
