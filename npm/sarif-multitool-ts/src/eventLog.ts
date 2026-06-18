// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * Append-only JSONL event log + replayer for incremental SARIF authoring.
 *
 * Ported from:
 *   src/Sarif/Emit/SarifEventLogWriter.cs
 *   src/Sarif/Emit/SarifEventLogReader.cs
 *   src/Sarif/Emit/SarifEventReplayer.cs
 *   src/Sarif/Emit/AtomicSarifWriter.cs
 *   src/Sarif/Emit/SarifEvent.cs
 *   src/Sarif/Emit/SarifEventKinds.cs
 *
 * Wire shape per line: {"v":1,"kind":"<kind>","payload":{...}}
 *
 * Pass-through guarantee: payloads are written verbatim (the caller's object,
 * not a re-projection through the sparse TS types) and replayed as plain JS
 * objects with all unknown keys preserved.
 */

import { promises as fs } from 'node:fs';
import {
  type SarifLog,
  type Run,
  type Result,
  type Invocation,
  type ReportingDescriptor,
  SARIF_SCHEMA_URI,
  AIRuleIdConvention,
} from '@microsoft/sarif';

// ---------------------------------------------------------------------------
// Event kinds & schema
// ---------------------------------------------------------------------------

export const SarifEventKinds = {
  RunHeader: 'run-header',
  Result: 'result',
  Invocation: 'invocation',
  RuleDescriptor: 'rule-descriptor',
  NotificationDescriptor: 'notification-descriptor',
  CurrentSchemaVersion: 1,
} as const;

const KNOWN_KINDS: ReadonlySet<string> = new Set([
  SarifEventKinds.RunHeader,
  SarifEventKinds.Result,
  SarifEventKinds.Invocation,
  SarifEventKinds.RuleDescriptor,
  SarifEventKinds.NotificationDescriptor,
]);

export interface SarifEvent {
  v: number;
  kind: string;
  payload: Record<string, unknown>;
}

export class SarifEventLogError extends Error {
  constructor(message: string) {
    super(message);
    this.name = 'SarifEventLogError';
  }
}

// ---------------------------------------------------------------------------
// Path helpers
// ---------------------------------------------------------------------------

/** Returns the `.wip.jsonl` sibling for a given final SARIF output path. */
export function wipPathFor(outputPath: string): string {
  return `${outputPath}.wip.jsonl`;
}

// ---------------------------------------------------------------------------
// Writer
// ---------------------------------------------------------------------------

/**
 * If the file exists and is non-empty, verify its last byte is `\n`; otherwise
 * the prior writer was interrupted mid-line and the file is unsafe to append to.
 * Mirrors SarifEventLogWriter.EnsureNoTornTrailingLine.
 */
async function ensureNoTornTrailingLine(path: string): Promise<void> {
  let handle: fs.FileHandle | undefined;
  try {
    handle = await fs.open(path, 'r');
  } catch (err) {
    if ((err as NodeJS.ErrnoException).code === 'ENOENT') return;
    throw err;
  }
  try {
    const stat = await handle.stat();
    if (stat.size === 0) return;
    const buf = Buffer.alloc(1);
    await handle.read(buf, 0, 1, stat.size - 1);
    if (buf[0] !== 0x0a) {
      throw new SarifEventLogError(
        `Event log '${path}' does not end with a newline (torn line). The prior writer was interrupted mid-line; refusing to append. Inspect or discard the file before continuing.`,
      );
    }
  } finally {
    await handle.close();
  }
}

/**
 * Appends one or more events to the JSONL log. Serializes each event to a
 * single UTF-8 line terminated with `\n`, then appends the whole batch in one
 * write so a multi-event append is atomic at the OS-buffer level.
 *
 * Payloads are serialized as-is — never re-projected through a TS type — so
 * any caller-supplied keys (including `properties` bags) round-trip verbatim.
 */
export async function appendEvents(
  path: string,
  events: ReadonlyArray<{ kind: string; payload: unknown }>,
): Promise<void> {
  if (!path) throw new Error('Event log path must be supplied.');
  if (events.length === 0) return;

  await ensureNoTornTrailingLine(path);

  let buf = '';
  for (const e of events) {
    if (!e.kind) throw new Error('Event kind must be supplied.');
    const wire: SarifEvent = {
      v: SarifEventKinds.CurrentSchemaVersion,
      kind: e.kind,
      payload: (e.payload as Record<string, unknown>) ?? {},
    };
    // Serialize first; if this throws, nothing is written (mirrors C# StringBuilder approach).
    buf += JSON.stringify(wire) + '\n';
  }

  await fs.appendFile(path, buf, { encoding: 'utf8' });
}

// ---------------------------------------------------------------------------
// Reader
// ---------------------------------------------------------------------------

/**
 * Streams events from the given path. Tolerates LF and CRLF; skips empty lines;
 * skips unknown kinds (forward compatibility); fails on unknown schema version
 * for a known kind, malformed JSON, or missing `kind`.
 * Mirrors SarifEventLogReader.Read.
 */
export async function readEvents(path: string): Promise<SarifEvent[]> {
  if (!path) throw new Error('Event log path must be supplied.');

  const text = await fs.readFile(path, 'utf8');
  // Strip a leading BOM if present.
  const body = text.charCodeAt(0) === 0xfeff ? text.slice(1) : text;
  const lines = body.split(/\r?\n/);

  const out: SarifEvent[] = [];
  for (let i = 0; i < lines.length; i++) {
    const line = lines[i];
    if (line.length === 0) continue;
    const lineNumber = i + 1;

    let ev: SarifEvent;
    try {
      ev = JSON.parse(line) as SarifEvent;
    } catch (err) {
      throw new SarifEventLogError(
        `Malformed JSON on line ${lineNumber} of event log '${path}': ${(err as Error).message}`,
      );
    }

    if (!ev || typeof ev.kind !== 'string' || ev.kind.length === 0) {
      throw new SarifEventLogError(
        `Event on line ${lineNumber} of event log '${path}' is missing a 'kind'.`,
      );
    }

    if (ev.v !== SarifEventKinds.CurrentSchemaVersion) {
      if (KNOWN_KINDS.has(ev.kind)) {
        throw new SarifEventLogError(
          `Event on line ${lineNumber} of event log '${path}' has schema version ${ev.v} for known kind '${ev.kind}'; this reader supports version ${SarifEventKinds.CurrentSchemaVersion} only.`,
        );
      }
      continue; // unknown kind at unknown version: forward-compatible
    }

    if (!KNOWN_KINDS.has(ev.kind)) {
      continue; // known schema version, unknown kind: forward-compatible extension
    }

    ev.payload ??= {};
    out.push(ev);
  }

  return out;
}

// ---------------------------------------------------------------------------
// Replayer
// ---------------------------------------------------------------------------

/**
 * Replays a SARIF event log into an in-memory SarifLog with a single Run.
 * Mirrors SarifEventReplayer.Replay.
 *
 * v1 contract:
 *   - At most one `run-header` event; if present it SHOULD be first. Header
 *     `results` / `invocations` are ignored — event-supplied entries are
 *     authoritative.
 *   - `result` events MUST be self-contained; `ruleIndex` is re-derived from
 *     `ruleId`, and every ruleId MUST conform to AIRuleIdConvention.
 *   - `invocation` events are appended in event order verbatim.
 */
export function replayEvents(events: ReadonlyArray<SarifEvent>): SarifLog {
  let run: Run | undefined;
  const results: Result[] = [];
  const invocations: Invocation[] = [];
  const ruleDescriptors: ReportingDescriptor[] = [];
  const notificationDescriptors: ReportingDescriptor[] = [];
  let headerSeen = false;

  for (const ev of events) {
    switch (ev.kind) {
      case SarifEventKinds.RunHeader: {
        if (headerSeen) {
          throw new SarifEventLogError(
            "Event log contains more than one 'run-header' event; at most one is permitted.",
          );
        }
        // The payload IS the Run skeleton — keep all caller-supplied keys.
        run = ev.payload as Run;
        // Header is the run skeleton; result and invocation events are authoritative.
        delete run.results;
        delete run.invocations;
        headerSeen = true;
        break;
      }
      case SarifEventKinds.Result:
        results.push(ev.payload as Result);
        break;
      case SarifEventKinds.Invocation:
        invocations.push(ev.payload as Invocation);
        break;
      case SarifEventKinds.RuleDescriptor:
        ruleDescriptors.push(ev.payload as ReportingDescriptor);
        break;
      case SarifEventKinds.NotificationDescriptor:
        notificationDescriptors.push(ev.payload as ReportingDescriptor);
        break;
      default:
        throw new SarifEventLogError(`Unexpected event kind '${ev.kind}' reached the replayer.`);
    }
  }

  run ??= {} as Run;
  run.tool ??= { driver: { name: 'Unknown' } };
  run.tool.driver ??= { name: 'Unknown' };

  // Explicit descriptors seed the idToIndex table before auto-registration.
  if (ruleDescriptors.length > 0) {
    run.tool.driver.rules = [...(run.tool.driver.rules ?? []), ...ruleDescriptors];
  }
  if (notificationDescriptors.length > 0) {
    run.tool.driver.notifications = [
      ...(run.tool.driver.notifications ?? []),
      ...notificationDescriptors,
    ];
  }

  registerDescriptorsFromResults(run, results);

  if (results.length > 0) run.results = results;
  if (invocations.length > 0) run.invocations = invocations;

  return {
    $schema: SARIF_SCHEMA_URI,
    version: '2.1.0',
    runs: [run],
  };
}

export async function replay(eventLogPath: string): Promise<SarifLog> {
  return replayEvents(await readEvents(eventLogPath));
}

function registerDescriptorsFromResults(run: Run, results: Result[]): void {
  AIRuleIdConvention.throwIfAnyUnacceptable(results);

  const idToIndex = new Map<string, number>();
  run.tool.driver.rules ??= [];
  for (let i = 0; i < run.tool.driver.rules.length; i++) {
    const id = run.tool.driver.rules[i]?.id;
    if (id) idToIndex.set(id, i);
  }

  for (const result of results) {
    // SARIF §3.49.3: descriptor ids are base-only. "CWE-79/dom-xss" registers
    // descriptor "CWE-79"; the hierarchical form stays on the result. NOVEL-
    // ruleIds are flat and register with the full id.
    let descriptorId = result.ruleId!;
    const slash = descriptorId.indexOf('/');
    if (slash > 0) descriptorId = descriptorId.slice(0, slash);

    let index = idToIndex.get(descriptorId);
    if (index === undefined) {
      index = run.tool.driver.rules.length;
      run.tool.driver.rules.push({ id: descriptorId });
      idToIndex.set(descriptorId, index);
    }
    result.ruleIndex = index;
  }
}
