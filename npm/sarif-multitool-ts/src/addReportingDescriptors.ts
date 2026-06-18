// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * `add-rule-reporting-descriptors` and `add-notification-reporting-descriptors`:
 * validate one or more SARIF reportingDescriptor objects and append an event
 * per element to `<output>.wip.jsonl`.
 *
 * Ported from src/Sarif.Multitool.Library/Emit/ReportingDescriptorEmitter.cs.
 */

import { readFileSync } from 'node:fs';
import { SarifEventKinds } from './eventLog.js';
import {
  processBatch,
  type BatchOutcome,
  type BatchElementError,
  type ValidateBatchElement,
} from './batch.js';
import { AIRuleIdConvention } from '@microsoft/sarif';
import { AI_RULEID_ERROR_CODE } from './addResults.js';

type Obj = Record<string, unknown>;

export interface AddReportingDescriptorsOptions {
  output: string;
  /** Single reportingDescriptor object or an array of them. */
  descriptors: unknown;
}

export function addRuleReportingDescriptors(
  opts: AddReportingDescriptorsOptions,
): Promise<BatchOutcome> {
  return appendDescriptors(opts, true);
}

export function addNotificationReportingDescriptors(
  opts: AddReportingDescriptorsOptions,
): Promise<BatchOutcome> {
  return appendDescriptors(opts, false);
}

async function appendDescriptors(
  opts: AddReportingDescriptorsOptions,
  isRules: boolean,
): Promise<BatchOutcome> {
  const payloadKind = isRules ? 'rule descriptor' : 'notification descriptor';
  const eventKind = isRules ? SarifEventKinds.RuleDescriptor : SarifEventKinds.NotificationDescriptor;
  const targetArray = isRules ? 'rules' : 'notifications';

  return processBatch({
    output: opts.output,
    payload: opts.descriptors,
    payloadKind,
    eventKind,
    buildValidator: (wipPath) => buildValidator(wipPath, isRules, payloadKind, eventKind, targetArray),
  });
}

function capitalize(s: string): string {
  return s ? s[0].toUpperCase() + s.slice(1) : s;
}

function buildValidator(
  wipPath: string,
  isRules: boolean,
  payloadKind: string,
  eventKind: string,
  targetArray: string,
): ValidateBatchElement {
  // Read existing-log ids once (O(events)) so a batch does not re-scan the
  // log per element. The validator contract is synchronous, so this scan is
  // sync too — the .wip.jsonl is one analysis session and small.
  const existingIds = readExistingDescriptorIdsSync(wipPath, eventKind, targetArray);
  const batchIds = new Set<string>();

  return (descriptor: Obj, index: number): BatchElementError | undefined => {
    const idToken = descriptor.id;
    if (typeof idToken !== 'string') {
      const what =
        idToken === undefined
          ? "The payload had no 'id' field"
          : `The payload supplied 'id' as JSON ${idToken === null ? 'null' : typeof idToken}`;
      return {
        index,
        message: `${capitalize(payloadKind)} JSON must include a non-empty 'id' string (SARIF §3.49.3). ${what}.`,
      };
    }
    if (idToken.length === 0) {
      return {
        index,
        message: `${capitalize(payloadKind)} JSON 'id' must be a non-empty string (SARIF §3.49.3).`,
      };
    }

    if (isRules && !(AIRuleIdConvention.isNovel(idToken) && AIRuleIdConvention.isAcceptable(idToken))) {
      return {
        index,
        errorCode: AI_RULEID_ERROR_CODE,
        message: `rule descriptor id '${idToken}' is not a well-formed NOVEL- id. The add-rule-reporting-descriptors verb is reserved for novel-finding descriptors that have no taxonomy entry; descriptors for taxonomy-mapped rules (e.g., 'CWE-89') come from the taxonomy enricher, not from this verb. Use a NOVEL- escape-hatch id: 'NOVEL-' followed by a lowercase-alphanumeric kebab sub-id (single hyphens, no slash, no trailing hyphen), e.g., 'NOVEL-prompt-injection-via-system-message'. See docs/ai/generating-sarif.md#rule-id-convention.`,
      };
    }

    if (existingIds.has(idToken)) {
      return {
        index,
        message: `a ${payloadKind} with id '${idToken}' is already present in the event log under tool.driver.${targetArray}. Each id may appear at most once per event log. (--force is not yet supported.)`,
      };
    }

    if (batchIds.has(idToken)) {
      return {
        index,
        message: `a ${payloadKind} with id '${idToken}' appears more than once in this batch. Each id may appear at most once per event log. (--force is not yet supported.)`,
      };
    }
    batchIds.add(idToken);

    return undefined;
  };
}

/**
 * Collects every descriptor id already targeting `targetArray` in the staged
 * event log (run-header pre-populated descriptors + prior descriptor events).
 * Synchronous because the batch validator contract is sync; the .wip.jsonl is
 * small (one analysis session) so blocking read is acceptable.
 */
function readExistingDescriptorIdsSync(
  wipPath: string,
  targetKind: string,
  targetArray: string,
): Set<string> {
  const ids = new Set<string>();
  let text: string;
  try {
    text = readFileSync(wipPath, 'utf8');
  } catch {
    return ids;
  }
  const body = text.charCodeAt(0) === 0xfeff ? text.slice(1) : text;
  for (const line of body.split(/\r?\n/)) {
    if (!line) continue;
    let ev: { kind?: string; payload?: Obj };
    try {
      ev = JSON.parse(line);
    } catch {
      continue;
    }
    if (ev.kind === SarifEventKinds.RunHeader) {
      const arr = (((ev.payload?.tool as Obj | undefined)?.driver as Obj | undefined)?.[targetArray] ??
        []) as unknown[];
      if (Array.isArray(arr)) {
        for (const d of arr) {
          const id = (d as Obj | undefined)?.id;
          if (typeof id === 'string') ids.add(id);
        }
      }
    } else if (ev.kind === targetKind) {
      const id = ev.payload?.id;
      if (typeof id === 'string') ids.add(id);
    }
  }
  return ids;
}

