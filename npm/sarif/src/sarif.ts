// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * Sparse, open-typed subset of the SARIF 2.1.0 object model — only the nodes
 * the emit verbs read or mutate. Every type is intersection-open so caller-
 * supplied fields (including `properties` bags and any unrecognized keys)
 * round-trip verbatim through the emit chain. This file is NOT a schema;
 * validation is the .NET `sarif validate` verb's job (see README).
 */

/** Every SARIF node may carry a property bag and unknown extra keys. */
export type PropertyBagHolder = {
  properties?: Record<string, unknown>;
  [extra: string]: unknown;
};

export type Message = PropertyBagHolder & {
  text?: string;
  markdown?: string;
  id?: string;
  arguments?: string[];
};

export type MultiformatMessageString = PropertyBagHolder & {
  text: string;
  markdown?: string;
};

export type ArtifactContent = PropertyBagHolder & {
  text?: string;
  binary?: string;
  rendered?: MultiformatMessageString;
};

export type Region = PropertyBagHolder & {
  startLine?: number;
  startColumn?: number;
  endLine?: number;
  endColumn?: number;
  charOffset?: number;
  charLength?: number;
  byteOffset?: number;
  byteLength?: number;
  snippet?: ArtifactContent;
  message?: Message;
  sourceLanguage?: string;
};

export type ArtifactLocation = PropertyBagHolder & {
  uri?: string;
  uriBaseId?: string;
  index?: number;
  description?: Message;
};

export type PhysicalLocation = PropertyBagHolder & {
  artifactLocation?: ArtifactLocation;
  region?: Region;
  contextRegion?: Region;
  address?: PropertyBagHolder;
};

export type Location = PropertyBagHolder & {
  id?: number;
  physicalLocation?: PhysicalLocation;
  logicalLocations?: PropertyBagHolder[];
  message?: Message;
  annotations?: Region[];
  relationships?: PropertyBagHolder[];
};

export type Artifact = PropertyBagHolder & {
  location?: ArtifactLocation;
  parentIndex?: number;
  offset?: number;
  length?: number;
  roles?: string[];
  mimeType?: string;
  contents?: ArtifactContent;
  encoding?: string;
  sourceLanguage?: string;
  hashes?: Record<string, string>;
  lastModifiedTimeUtc?: string;
  description?: Message;
};

export type ReportingConfiguration = PropertyBagHolder & {
  enabled?: boolean;
  level?: 'none' | 'note' | 'warning' | 'error';
  rank?: number;
  parameters?: Record<string, unknown>;
};

export type ReportingDescriptorReference = PropertyBagHolder & {
  id?: string;
  index?: number;
  guid?: string;
  toolComponent?: PropertyBagHolder;
};

export type ReportingDescriptorRelationship = PropertyBagHolder & {
  target: ReportingDescriptorReference;
  kinds?: string[];
  description?: Message;
};

export type ReportingDescriptor = PropertyBagHolder & {
  id: string;
  name?: string;
  guid?: string;
  deprecatedIds?: string[];
  shortDescription?: MultiformatMessageString;
  fullDescription?: MultiformatMessageString;
  messageStrings?: Record<string, MultiformatMessageString>;
  defaultConfiguration?: ReportingConfiguration;
  helpUri?: string;
  help?: MultiformatMessageString;
  relationships?: ReportingDescriptorRelationship[];
};

export type ToolComponent = PropertyBagHolder & {
  name: string;
  guid?: string;
  version?: string;
  semanticVersion?: string;
  fullName?: string;
  organization?: string;
  product?: string;
  informationUri?: string;
  rules?: ReportingDescriptor[];
  notifications?: ReportingDescriptor[];
  taxa?: ReportingDescriptor[];
  language?: string;
  contents?: string[];
  supportedTaxonomies?: PropertyBagHolder[];
};

export type Tool = PropertyBagHolder & {
  driver: ToolComponent;
  extensions?: ToolComponent[];
};

export type Notification = PropertyBagHolder & {
  locations?: Location[];
  message: Message;
  level?: 'none' | 'note' | 'warning' | 'error';
  threadId?: number;
  timeUtc?: string;
  exception?: PropertyBagHolder;
  descriptor?: ReportingDescriptorReference;
  associatedRule?: ReportingDescriptorReference;
};

export type Invocation = PropertyBagHolder & {
  commandLine?: string;
  arguments?: string[];
  startTimeUtc?: string;
  endTimeUtc?: string;
  exitCode?: number;
  executionSuccessful: boolean;
  workingDirectory?: ArtifactLocation;
  environmentVariables?: Record<string, string>;
  toolExecutionNotifications?: Notification[];
  toolConfigurationNotifications?: Notification[];
  machine?: string;
  account?: string;
  processId?: number;
};

export type VersionControlDetails = PropertyBagHolder & {
  repositoryUri: string;
  revisionId?: string;
  branch?: string;
  revisionTag?: string;
  asOfTimeUtc?: string;
  mappedTo?: ArtifactLocation;
};

export type RunAutomationDetails = PropertyBagHolder & {
  id?: string;
  guid?: string;
  correlationGuid?: string;
  description?: Message;
};

export type Result = PropertyBagHolder & {
  ruleId?: string;
  ruleIndex?: number;
  rule?: ReportingDescriptorReference;
  kind?: 'notApplicable' | 'pass' | 'fail' | 'review' | 'open' | 'informational';
  level?: 'none' | 'note' | 'warning' | 'error';
  message: Message;
  analysisTarget?: ArtifactLocation;
  locations?: Location[];
  guid?: string;
  correlationGuid?: string;
  occurrenceCount?: number;
  partialFingerprints?: Record<string, string>;
  fingerprints?: Record<string, string>;
  stacks?: PropertyBagHolder[];
  codeFlows?: PropertyBagHolder[];
  graphs?: PropertyBagHolder[];
  graphTraversals?: PropertyBagHolder[];
  relatedLocations?: Location[];
  suppressions?: PropertyBagHolder[];
  baselineState?: 'new' | 'unchanged' | 'updated' | 'absent';
  rank?: number;
  attachments?: PropertyBagHolder[];
  hostedViewerUri?: string;
  workItemUris?: string[];
  provenance?: PropertyBagHolder;
  fixes?: PropertyBagHolder[];
  taxa?: ReportingDescriptorReference[];
};

export type Run = PropertyBagHolder & {
  tool: Tool;
  invocations?: Invocation[];
  conversion?: PropertyBagHolder;
  language?: string;
  versionControlProvenance?: VersionControlDetails[];
  originalUriBaseIds?: Record<string, ArtifactLocation>;
  artifacts?: Artifact[];
  logicalLocations?: PropertyBagHolder[];
  graphs?: PropertyBagHolder[];
  results?: Result[];
  automationDetails?: RunAutomationDetails;
  runAggregates?: RunAutomationDetails[];
  baselineGuid?: string;
  redactionTokens?: string[];
  defaultEncoding?: string;
  defaultSourceLanguage?: string;
  newlineSequences?: string[];
  columnKind?: 'utf16CodeUnits' | 'unicodeCodePoints';
  taxonomies?: ToolComponent[];
  addresses?: PropertyBagHolder[];
  translations?: ToolComponent[];
  policies?: ToolComponent[];
  webRequests?: PropertyBagHolder[];
  webResponses?: PropertyBagHolder[];
  specialLocations?: PropertyBagHolder;
  threadFlowLocations?: PropertyBagHolder[];
};

export type SarifLog = PropertyBagHolder & {
  $schema?: string;
  version: '2.1.0';
  runs: Run[];
  inlineExternalProperties?: PropertyBagHolder[];
};

export const SARIF_SCHEMA_URI =
  'https://docs.oasis-open.org/sarif/sarif/v2.1.0/errata01/csd01/schemas/sarif-schema-2.1.0.json';
