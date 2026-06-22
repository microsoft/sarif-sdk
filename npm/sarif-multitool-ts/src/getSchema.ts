// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * `get-schema`: returns the JSON Schema for the document at a named emit verb's
 * boundary — the AI-authored input contract for the incremental emit-* verbs,
 * and the finalized whole-log output contract for the terminal emit-finalize
 * verb. Ported from src/Sarif.Multitool.Library/GetSchema/GetSchemaCommand.cs.
 *
 * Node consumers can also `import schema from
 * '@microsoft/sarif-multitool-ts/schemas/ai-result.schema.json'` directly.
 */

import { readFileSync } from 'node:fs';
import { assetPath } from './assets.js';

/**
 * Maps each emit verb to the schema file for the document at its boundary: the
 * AI-authored input contract for the incremental emit-* verbs, and the
 * finalized whole-log output contract for emit-finalize. A null value marks a
 * verb whose schema is reserved but not yet available.
 * Mirrors GetSchemaCommand.SchemaByVerb (post-#3035 canonical names).
 */
export const SchemaByVerb: Readonly<Record<string, string | null>> = {
  'emit-run': 'ai-run.schema.json',
  'emit-finalize': 'ai-sarif-log.schema.json',
  'emit-results': 'ai-result.schema.json',
  'emit-invocations': 'ai-invocation.schema.json',
  'emit-notification-descriptors': 'ai-notification-reporting-descriptor.schema.json',
  'emit-rule-descriptors': 'ai-rule-reporting-descriptor.schema.json',
};

/** Deprecated verb name → canonical, mirroring EmitVerbAliases.cs. */
const DEPRECATED_TO_CANONICAL: Readonly<Record<string, string>> = {
  'add-results': 'emit-results',
  'add-invocations': 'emit-invocations',
  'add-rule-reporting-descriptors': 'emit-rule-descriptors',
  'add-notification-reporting-descriptors': 'emit-notification-descriptors',
};

export function listSchemas(): string[] {
  return Object.entries(SchemaByVerb)
    .filter(([, file]) => file !== null)
    .map(([verb]) => verb)
    .sort();
}

/**
 * Returns the schema text for the given emit verb (canonical or deprecated
 * name). Throws if the verb is unknown or its schema is not yet available.
 */
export function getSchema(verb: string): string {
  const v = DEPRECATED_TO_CANONICAL[verb.trim()] ?? verb.trim();
  if (!(v in SchemaByVerb)) {
    throw new Error(
      `'${verb}' is not a verb with a schema. Verbs with a schema: ${listSchemas().join(', ')}.`,
    );
  }
  const file = SchemaByVerb[v];
  if (file === null) {
    throw new Error(
      `the schema for '${v}' is not yet available (tracking: https://github.com/microsoft/sarif-sdk/issues/2970).`,
    );
  }
  return readFileSync(assetPath('schemas', file), 'utf8');
}
