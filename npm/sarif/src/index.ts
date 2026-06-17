// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * @microsoft/sarif — SARIF SDK for Node.js.
 *
 * Open-typed SARIF 2.1.0 object model, region/snippet resolution, AI ruleId
 * convention, portable-root derivation, and serialization helpers. Every TS
 * module names the C# source-of-truth file it was ported from. The C# under
 * src/Sarif/ is normative; when they disagree this package has a bug.
 */

export * from './sarif.js';
export { FileRegionsCache, NewLineIndex } from './regions.js';
export { AIRuleIdConvention, AIRuleIdConventionError } from './aiRuleId.js';
export {
  tryValidateRepositoryUri,
  tryDerivePortableRoot,
  isGitHubHostedRun,
  type PortableRoot,
} from './vcp.js';
export {
  insertOptionalData,
  tryReconstructAbsoluteUri,
  type InsertOptionalDataFlags,
} from './insertOptionalData.js';
export { atomicWrite, stripNulls, serializeSarifLog } from './io.js';
export { rewriteRelativeLinks, extractFrontmatterDescription } from './skill.js';
