#!/usr/bin/env node
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * Arg-compatible CLI front-end for the TS-native multitool verbs. Mirrors the
 * .NET multitool's flag surface so `sarif <verb> ...` is a drop-in for the
 * emit-* / get-* subset. Verbs not yet ported print a redirect to
 * @microsoft/sarif-multitool (the .NET wrapper).
 *
 * Argv is parsed by hand — no command-line framework.
 */

import { readFileSync } from 'node:fs';
import process from 'node:process';
import { emitRun } from './emitRun.js';
import { addResults } from './addResults.js';
import { addInvocations } from './addInvocations.js';
import {
  addRuleReportingDescriptors,
  addNotificationReportingDescriptors,
} from './addReportingDescriptors.js';
import { emitFinalize } from './emitFinalize.js';
import { getSchema, listSchemas } from './getSchema.js';
import { getSkill, listSkills } from './getSkill.js';
import { getCweTaxonomy } from './getCwe.js';
import { EmitVerbError, type BatchOutcome } from './batch.js';
import { atomicWrite } from '@microsoft/sarif';

// Mirrors EmitVerbAliases.cs — accepted through the v5.x deprecation window.
const DEPRECATED_TO_CANONICAL: Readonly<Record<string, string>> = {
  'add-results': 'emit-results',
  'add-invocations': 'emit-invocations',
  'add-rule-reporting-descriptors': 'emit-rule-descriptors',
  'add-notification-reporting-descriptors': 'emit-notification-descriptors',
};

interface ParsedArgs {
  verb: string;
  positional: string[];
  flags: Record<string, string | boolean>;
}

function parseArgs(argv: string[]): ParsedArgs {
  const positional: string[] = [];
  const flags: Record<string, string | boolean> = {};
  let i = 0;
  while (i < argv.length) {
    const a = argv[i];
    if (a.startsWith('--')) {
      const key = a.slice(2);
      const next = argv[i + 1];
      if (next !== undefined && !next.startsWith('--')) {
        flags[key] = next;
        i += 2;
      } else {
        flags[key] = true;
        i += 1;
      }
    } else {
      positional.push(a);
      i += 1;
    }
  }
  let verb = positional[0] ?? '';
  if (verb in DEPRECATED_TO_CANONICAL) {
    process.stderr.write(
      `warning: '${verb}' is deprecated and will be removed in v6; use '${DEPRECATED_TO_CANONICAL[verb]}'.\n`,
    );
    verb = DEPRECATED_TO_CANONICAL[verb];
  }
  return { verb, positional: positional.slice(1), flags };
}

function readJsonInput(inputPath: string | undefined, payloadKind: string): unknown {
  let json: string;
  if (inputPath) {
    json = readFileSync(inputPath, 'utf8');
  } else if (!process.stdin.isTTY) {
    json = readFileSync(0, 'utf8');
  } else {
    throw new EmitVerbError(`Provide --input <path> or pipe the ${payloadKind} JSON on stdin.`);
  }
  if (!json.trim()) {
    throw new EmitVerbError(`${payloadKind[0].toUpperCase() + payloadKind.slice(1)} JSON is empty.`);
  }
  try {
    return JSON.parse(json);
  } catch (err) {
    throw new EmitVerbError(
      `${payloadKind[0].toUpperCase() + payloadKind.slice(1)} JSON is malformed: ${(err as Error).message}`,
    );
  }
}

function writeBatchReport(outcome: BatchOutcome): number {
  process.stdout.write(JSON.stringify(outcome, undefined, 2) + '\n');
  return outcome.rejected.length > 0 ? 1 : 0;
}

async function writeOrPrint(
  content: string,
  outputPath: string | undefined,
  forceOverwrite: boolean,
  label: string,
): Promise<number> {
  if (outputPath) {
    const { existsSync } = await import('node:fs');
    if (existsSync(outputPath) && !forceOverwrite) {
      throw new EmitVerbError(`'${outputPath}' already exists. Pass --force-overwrite to replace it.`);
    }
    await atomicWrite(outputPath, content);
    process.stdout.write(`Wrote ${label} to '${outputPath}'.\n`);
  } else {
    process.stdout.write(content);
  }
  return 0;
}

const HELP = `\
sarif — native-TypeScript SARIF Multitool verbs (no CLR)

emit chain:
  sarif emit-run <output.sarif> [--input <run.json>] [--force-overwrite]
  sarif emit-results <output.sarif> [--input <results.json>]
  sarif emit-invocations <output.sarif> [--input <invocations.json>]
  sarif emit-rule-descriptors <output.sarif> [--input <descriptors.json>]
  sarif emit-notification-descriptors <output.sarif> [--input <descriptors.json>]
  sarif emit-finalize <output.sarif> [--no-cwe-enrichment] [--minify] [--keep-wip] [--validate]

asset verbs:
  sarif get-schema <emit-verb> [--output <path>] [--force-overwrite] | --list
  sarif get-skill <name> [--output <path>] [--force-overwrite] | --list
  sarif get-cwe [--output <path>] [--force-overwrite]

When --input is omitted the payload JSON is read from stdin.
Deprecated add-* verb names are accepted with a warning through v5.x.
Verbs not listed above are not yet ported; use \`npx @microsoft/sarif-multitool <verb>\`.
`;

async function main(): Promise<number> {
  const { verb, positional, flags } = parseArgs(process.argv.slice(2));
  const output = positional[0];
  const input = typeof flags.input === 'string' ? flags.input : undefined;
  const fileOut = typeof flags.output === 'string' ? flags.output : undefined;
  const force = !!flags['force-overwrite'];

  switch (verb) {
    case 'emit-run': {
      const run = readJsonInput(input, 'run');
      if (typeof run !== 'object' || run === null || Array.isArray(run)) {
        throw new EmitVerbError('Run JSON must be a JSON object.');
      }
      const r = await emitRun({
        output,
        run: run as Record<string, unknown>,
        forceOverwrite: force,
      });
      for (const w of r.warnings) process.stderr.write(w + '\n');
      process.stdout.write(`Opened '${r.wipPath}' for '${r.toolName}'.\n`);
      return 0;
    }

    case 'emit-results':
      return writeBatchReport(await addResults({ output, results: readJsonInput(input, 'result') }));

    case 'emit-invocations':
      return writeBatchReport(
        await addInvocations({ output, invocations: readJsonInput(input, 'invocation') }),
      );

    case 'emit-rule-descriptors':
      return writeBatchReport(
        await addRuleReportingDescriptors({
          output,
          descriptors: readJsonInput(input, 'rule descriptor'),
        }),
      );

    case 'emit-notification-descriptors':
      return writeBatchReport(
        await addNotificationReportingDescriptors({
          output,
          descriptors: readJsonInput(input, 'notification descriptor'),
        }),
      );

    case 'emit-finalize': {
      const r = await emitFinalize({
        output,
        noCweEnrichment: !!flags['no-cwe-enrichment'],
        minify: !!flags.minify,
        keepWip: !!flags['keep-wip'],
        validate: !!flags.validate,
      });
      for (const w of r.warnings) process.stderr.write(w + '\n');
      process.stdout.write(
        `Wrote '${r.outputPath}' (${r.resultCount} result(s), ${r.ruleCount} rule(s)).\n`,
      );
      if (r.validation) {
        if (r.validation.valid) {
          process.stdout.write('Validation: conforms to ai-sarif-log.schema.json.\n');
        } else {
          process.stderr.write(
            `Validation: does not conform to ai-sarif-log.schema.json (${r.validation.errors.length} error(s)):\n`,
          );
          for (const e of r.validation.errors) process.stderr.write(`  ${e}\n`);
          return 1;
        }
      }
      return 0;
    }

    case 'get-schema': {
      if (flags.list) {
        process.stdout.write('Verbs with a schema:\n  ' + listSchemas().join('\n  ') + '\n');
        return 0;
      }
      if (!output) throw new EmitVerbError('specify a verb whose schema to emit, or pass --list.');
      return writeOrPrint(getSchema(output), fileOut, force, `schema for '${output}'`);
    }

    case 'get-skill': {
      if (flags.list) {
        const w = Math.max(...listSkills().map((s) => s.name.length));
        process.stdout.write(
          'Available skills:\n' +
            listSkills()
              .map((s) => `  ${s.name.padEnd(w)}  ${s.description ?? ''}`)
              .join('\n') +
            '\n',
        );
        return 0;
      }
      if (!output) throw new EmitVerbError('specify a skill to emit, or pass --list.');
      return writeOrPrint(getSkill(output), fileOut, force, `skill '${output}'`);
    }

    case 'get-cwe':
      return writeOrPrint(
        JSON.stringify(getCweTaxonomy(), undefined, 2) + '\n',
        fileOut,
        force,
        'CWE taxonomy',
      );

    case '':
    case 'help':
    case '--help':
      process.stdout.write(HELP);
      return 0;

    default:
      process.stderr.write(
        `'${verb}' is not implemented in @microsoft/sarif-multitool-ts. ` +
          `Use \`npx @microsoft/sarif-multitool ${process.argv.slice(2).join(' ')}\` (the .NET-backed wrapper).\n\n${HELP}`,
      );
      return 1;
  }
}

main()
  .then((code) => process.exit(code))
  .catch((err) => {
    if (err instanceof EmitVerbError) {
      process.stderr.write(err.message + '\n');
    } else {
      process.stderr.write(((err as Error).stack ?? String(err)) + '\n');
    }
    process.exit(1);
  });
