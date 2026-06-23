// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * @microsoft/sarif-multitool-ts — native-TypeScript SARIF Multitool verbs.
 * In-process library + arg-compatible CLI; no CLR dependency. Depends on
 * @microsoft/sarif for the open-typed object model and core helpers.
 */

// Re-export the base SDK so `import ... from '@microsoft/sarif-multitool-ts'`
// is sufficient for most consumers.
export * from '@microsoft/sarif';

// emit-* verbs (canonical names per the v5.1 rename; add* aliases below).
export { emitRun, type EmitRunOptions, type EmitRunOutcome, SOURCE_ROOT_BASE_ID } from './emitRun.js';
export {
  addResults as emitResults,
  type AddResultsOptions as EmitResultsOptions,
  AI_RULEID_ERROR_CODE,
} from './addResults.js';
export {
  addInvocations as emitInvocations,
  type AddInvocationsOptions as EmitInvocationsOptions,
} from './addInvocations.js';
export {
  addRuleReportingDescriptors as emitRuleDescriptors,
  addNotificationReportingDescriptors as emitNotificationDescriptors,
  type AddReportingDescriptorsOptions as EmitDescriptorsOptions,
} from './addReportingDescriptors.js';
export { emitFinalize, type EmitFinalizeOptions, type EmitFinalizeOutcome } from './emitFinalize.js';
export { validateFinalizedLog, type ValidationOutcome } from './validate.js';

// get-* verbs.
export { getSchema, listSchemas, SchemaByVerb } from './getSchema.js';
export { getSkill, listSkills } from './getSkill.js';
export { getCweTaxonomy, getCweSecuritySeverityTable } from './getCwe.js';

// Infrastructure.
export { type BatchOutcome, type BatchElementError, EmitVerbError } from './batch.js';
export { SarifEventKinds, replay, readEvents, wipPathFor } from './eventLog.js';

// Deprecated aliases — kept through the v5.x window alongside the .NET CLI's
// add-* aliases (see EmitVerbAliases.cs). Remove in v6.
export { addResults } from './addResults.js';
export { addInvocations } from './addInvocations.js';
export {
  addRuleReportingDescriptors,
  addNotificationReportingDescriptors,
} from './addReportingDescriptors.js';
