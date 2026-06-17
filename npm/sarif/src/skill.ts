// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * Shared helper for packages that ship a SKILL.md: rewrites repository-relative
 * markdown links to commit-pinned raw permalinks so the emitted skill resolves
 * its references against the exact repository state that shipped the package.
 *
 * Ported from src/Sarif.Multitool.Library/GetSkill/GetSkillCommand.cs
 * (RewriteRelativeLinks / ResolveRepositoryRelative / IsRepositoryRelative).
 */

const RAW_CONTENT_BASE_URL = 'https://raw.githubusercontent.com/microsoft/sarif-sdk/';
const MARKDOWN_LINK = /\]\(([^)\s]+)(\s+"[^"]*")?\)/g;
const URI_SCHEME = /^[a-zA-Z][a-zA-Z0-9+.\-]*:/;

/**
 * Rewrites every repository-relative markdown link in `markdown` to a raw
 * permalink pinned to `pinRef`. Absolute URLs, protocol-relative URLs, and
 * bare fragments are left untouched.
 */
export function rewriteRelativeLinks(
  markdown: string,
  skillSourceDirectory: string,
  pinRef: string,
): string {
  return markdown.replace(MARKDOWN_LINK, (match, url: string, title: string | undefined) => {
    if (!isRepositoryRelative(url)) return match;

    const fragmentIdx = url.indexOf('#');
    const path = fragmentIdx >= 0 ? url.slice(0, fragmentIdx) : url;
    const fragment = fragmentIdx >= 0 ? url.slice(fragmentIdx) : '';

    const repoPath = resolveRepositoryRelative(skillSourceDirectory, path);
    return `](${RAW_CONTENT_BASE_URL}${pinRef}/${repoPath}${fragment}${title ?? ''})`;
  });
}

function isRepositoryRelative(url: string): boolean {
  if (!url) return false;
  if (url[0] === '#') return false;
  if (url.startsWith('//')) return false;
  if (URI_SCHEME.test(url)) return false;
  return true;
}

function resolveRepositoryRelative(baseDirectory: string, relativePath: string): string {
  const segments = baseDirectory.split('/').filter((s) => s.length > 0);
  for (const segment of relativePath.split('/')) {
    if (segment.length === 0 || segment === '.') continue;
    if (segment === '..') {
      if (segments.length > 0) segments.pop();
      continue;
    }
    segments.push(segment);
  }
  return segments.join('/');
}

/**
 * Extracts the `description:` scalar from a skill document's leading YAML
 * frontmatter block. Returns undefined when the document has no frontmatter,
 * declares no description, or uses a multi-line block scalar.
 */
export function extractFrontmatterDescription(document: string): string | undefined {
  if (!document) return undefined;
  const lines = document.split(/\r?\n/);
  if (lines[0]?.trim() !== '---') return undefined;
  for (let i = 1; i < lines.length; i++) {
    if (lines[i].trim() === '---') return undefined;
    const m = /^description:[ \t]*(.+?)[ \t]*$/.exec(lines[i]);
    if (!m) continue;
    let value = m[1];
    if (value.length >= 2 && (value[0] === '"' || value[0] === "'") && value.at(-1) === value[0]) {
      value = value.slice(1, -1);
    }
    if (/^[>|][0-9+\-]*$/.test(value)) return undefined;
    return value;
  }
  return undefined;
}
