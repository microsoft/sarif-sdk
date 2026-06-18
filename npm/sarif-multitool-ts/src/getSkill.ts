// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * `get-skill`: returns an embedded agent skill that drives the multitool emit
 * and validate verbs, with repository-relative links rewritten to permalinks
 * pinned to the package version.
 *
 * Ported from src/Sarif.Multitool.Library/GetSkill/GetSkillCommand.cs.
 */

import { readFileSync } from 'node:fs';
import { rewriteRelativeLinks, extractFrontmatterDescription } from '@microsoft/sarif';
import { assetPath, packageVersion } from './assets.js';

/**
 * Maps each skill to the repository-relative directory of its SKILL.md.
 * Mirrors GetSkillCommand.SkillSourceDirectory; this package ships only the
 * skills for verbs it implements.
 */
export const SkillSourceDirectory: Readonly<Record<string, string>> = {
  'emit-sarif': 'skills/emit-sarif',
  'validate-sarif': 'skills/validate-sarif',
};

export function listSkills(): Array<{ name: string; description?: string }> {
  return Object.keys(SkillSourceDirectory)
    .sort()
    .map((name) => {
      const md = readFileSync(assetPath('skills', `${name}.SKILL.md`), 'utf8');
      return { name, description: extractFrontmatterDescription(md) };
    });
}

/**
 * Returns the skill markdown with repo-relative links rewritten to
 * raw.githubusercontent.com permalinks pinned to `pinRef` (default: this
 * package's version tag, the closest analogue to the .NET tool's
 * SourceLink-stamped commit SHA).
 */
export function getSkill(name: string, pinRef?: string): string {
  const dir = SkillSourceDirectory[name.trim()];
  if (!dir) {
    const available = Object.keys(SkillSourceDirectory).sort().join(', ');
    throw new Error(`'${name}' is not an available skill. Available skills: ${available}.`);
  }
  const md = readFileSync(assetPath('skills', `${name.trim()}.SKILL.md`), 'utf8');
  return rewriteRelativeLinks(md, dir, pinRef ?? `v${packageVersion()}`);
}
