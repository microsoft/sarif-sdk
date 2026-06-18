// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * `add-invocations`: validates one or more fully-formed SARIF invocations and
 * appends an `invocation` event per element to `<output>.wip.jsonl`.
 *
 * Ported from src/Sarif.Multitool.Library/Emit/AddInvocationsCommand.cs.
 */

import { SarifEventKinds } from './eventLog.js';
import { processBatch, type BatchOutcome, type BatchElementError } from './batch.js';

type Obj = Record<string, unknown>;

export interface AddInvocationsOptions {
  output: string;
  /** Single invocation object or an array of invocation objects. */
  invocations: unknown;
  /** Test seam: receipt time. Defaults to the call instant. */
  now?: Date;
}

export function addInvocations(opts: AddInvocationsOptions): Promise<BatchOutcome> {
  const now = opts.now ?? new Date();
  return processBatch({
    output: opts.output,
    payload: opts.invocations,
    payloadKind: 'invocation',
    eventKind: SarifEventKinds.Invocation,
    buildValidator: () => (inv, index, batched) => validateInvocation(inv, index, batched, now),
  });
}

function validateInvocation(
  invocation: Obj,
  index: number,
  batched: boolean,
  now: Date,
): BatchElementError | undefined {
  const message = validateInvocationReceipt(invocation);
  if (message) return { index, message };

  const endTimeUtc = invocation.endTimeUtc;
  const hasEndTimeUtc = endTimeUtc !== undefined && endTimeUtc !== null;
  if (!hasEndTimeUtc) {
    if (batched) {
      return {
        index,
        message:
          "Invalid invocation: 'endTimeUtc' is required when submitting invocations as a batch. The receipt-time default applies only to single (one-object) submission, where the write is roughly coincident with the invocation's conclusion; a batch is assembled after the fact, so each invocation must carry its own 'endTimeUtc'.",
      };
    }
    invocation.endTimeUtc = formatSarifDateTime(now);
  }

  return undefined;
}

// Receipt gate for the required fields of ai-invocation.schema.json; full
// structural validation runs at emit-finalize --validate (deferred in this
// package — see README). Returns undefined when acceptable; otherwise the
// first violation message.
function validateInvocationReceipt(invocation: Obj): string | undefined {
  if (typeof invocation.executionSuccessful !== 'boolean') {
    return "Invalid invocation: 'executionSuccessful' is required and must be a boolean.";
  }

  if (typeof invocation.commandLine !== 'string' || !invocation.commandLine.trim()) {
    return "Invalid invocation: 'commandLine' is required and must be a non-whitespace string.";
  }

  const wd = invocation.workingDirectory;
  if (!wd || typeof wd !== 'object' || Array.isArray(wd)) {
    return "Invalid invocation: 'workingDirectory' is required and must be an artifactLocation.";
  }
  const wdo = wd as Obj;
  const hasUri = typeof wdo.uri === 'string' && !!wdo.uri.trim();
  const hasUriBaseId = typeof wdo.uriBaseId === 'string' && !!wdo.uriBaseId.trim();
  if (!hasUri && !hasUriBaseId) {
    return "Invalid invocation: 'workingDirectory' must be an artifactLocation with a non-whitespace 'uri' or 'uriBaseId'.";
  }

  return (
    validateNotificationTimes(invocation.toolExecutionNotifications, 'toolExecutionNotifications') ??
    validateNotificationTimes(invocation.toolConfigurationNotifications, 'toolConfigurationNotifications')
  );
}

function validateNotificationTimes(notifications: unknown, arrayName: string): string | undefined {
  if (notifications === undefined || notifications === null) return undefined;
  if (!Array.isArray(notifications)) {
    return `Invalid invocation: '${arrayName}' must be an array.`;
  }
  for (const item of notifications) {
    const timeUtc = item && typeof item === 'object' ? (item as Obj).timeUtc : undefined;
    if (typeof timeUtc !== 'string' || !timeUtc.trim()) {
      return `Invalid invocation: every '${arrayName}' entry requires a non-whitespace 'timeUtc'.`;
    }
  }
  return undefined;
}

/** SARIF date-time at millisecond precision: yyyy-MM-ddTHH:mm:ss.fffZ. */
export function formatSarifDateTime(d: Date): string {
  // Date.toISOString() yields exactly this shape (always UTC, ms precision).
  return d.toISOString().replace(/\.\d{3}Z$/, (m) => m); // identity; kept for clarity
}
