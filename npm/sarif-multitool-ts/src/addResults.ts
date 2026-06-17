// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * `add-results`: validates one or more fully-formed SARIF results and appends
 * a `result` event per element to `<output>.wip.jsonl`.
 *
 * Ported from src/Sarif.Multitool.Library/Emit/AddResultsCommand.cs.
 */

import { SarifEventKinds } from './eventLog.js';
import { processBatch, type BatchOutcome, type BatchElementError } from './batch.js';
import { AIRuleIdConvention, AIRuleIdConventionError } from '@microsoft/sarif';

export const AI_RULEID_ERROR_CODE = 'AI1012';

export interface AddResultsOptions {
  output: string;
  /** Single result object or an array of result objects. */
  results: unknown;
}

export function addResults(opts: AddResultsOptions): Promise<BatchOutcome> {
  return processBatch({
    output: opts.output,
    payload: opts.results,
    payloadKind: 'result',
    eventKind: SarifEventKinds.Result,
    buildValidator: () => validateResult,
  });
}

function validateResult(
  result: Record<string, unknown>,
  index: number,
): BatchElementError | undefined {
  const ruleIdToken = result.ruleId;

  if (ruleIdToken !== undefined && ruleIdToken !== null && typeof ruleIdToken !== 'string') {
    return {
      index,
      errorCode: AI_RULEID_ERROR_CODE,
      message: `result.ruleId must be a JSON string, but the payload supplied a JSON ${typeof ruleIdToken}. See docs/ai/generating-sarif.md#rule-id-convention.`,
    };
  }

  const ruleId = typeof ruleIdToken === 'string' ? ruleIdToken : undefined;
  try {
    AIRuleIdConvention.throwIfUnacceptable(ruleId);
  } catch (ex) {
    return {
      index,
      errorCode: AI_RULEID_ERROR_CODE,
      message: (ex as AIRuleIdConventionError).message,
    };
  }

  return undefined;
}
