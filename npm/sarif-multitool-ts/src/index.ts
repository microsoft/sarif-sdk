// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * @microsoft/sarif-multitool-ts — native-TypeScript SARIF Multitool verbs.
 *
 * v0.0.x PLACEHOLDER. This release reserves the package name and proves the
 * publish pipeline; the verb implementations (emit-run, emit-results,
 * emit-invocations, emit-rule-descriptors, emit-notification-descriptors,
 * emit-finalize, get-schema, get-skill, get-cwe, validate) land in a
 * subsequent release tracked at https://github.com/microsoft/sarif-sdk.
 *
 * Until then, use `@microsoft/sarif-multitool` (the .NET-backed wrapper).
 */

export * from '@microsoft/sarif';

function notYetImplemented(verb: string): never {
  throw new Error(
    `@microsoft/sarif-multitool-ts: '${verb}' is not yet implemented in this v0.0.x ` +
      `placeholder release. Use \`npx @microsoft/sarif-multitool ${verb}\` (the .NET-backed ` +
      `wrapper) until the TypeScript implementation ships. ` +
      `Track progress at https://github.com/microsoft/sarif-sdk.`,
  );
}

export const emitRun = () => notYetImplemented('emit-run');
export const emitResults = () => notYetImplemented('emit-results');
export const emitInvocations = () => notYetImplemented('emit-invocations');
export const emitRuleDescriptors = () => notYetImplemented('emit-rule-descriptors');
export const emitNotificationDescriptors = () => notYetImplemented('emit-notification-descriptors');
export const emitFinalize = () => notYetImplemented('emit-finalize');
export const getSchema = () => notYetImplemented('get-schema');
export const getSkill = () => notYetImplemented('get-skill');
export const getCwe = () => notYetImplemented('get-cwe');
export const validate = () => notYetImplemented('validate');
