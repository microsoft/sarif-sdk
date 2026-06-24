// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * Unit coverage for the CWE Category API — `isCweCategory`, `getCweCategoryName`,
 * and `getCweCategories`. Mirrors the C# `CweCategories` helper: a Category
 * (e.g. CWE-16 "Configuration") is recognized in any form the SDK accepts
 * (canonical id, sub-id ruleId, any case, leading zeros), while a Weakness, a
 * View, the NOVEL form, and a bare number are not Categories.
 *
 * Runs against the built dist/ (which bundles assets/CweCategories.json via the
 * prebuild copy step). Run via `npm run test:unit`.
 */

import { test } from 'node:test';
import assert from 'node:assert/strict';
import { getCweCategories, isCweCategory, getCweCategoryName } from '../dist/index.js';

test('a canonical Category id is recognized and resolves to its name', () => {
  assert.equal(isCweCategory('CWE-16'), true);
  assert.equal(getCweCategoryName('CWE-16'), 'Configuration');
});

test('a Category named by an AI ruleId sub-id is recognized', () => {
  assert.equal(isCweCategory('CWE-16/insecure-default'), true);
  assert.equal(getCweCategoryName('CWE-16/insecure-default'), 'Configuration');
});

test('the CWE- prefix is matched case-insensitively', () => {
  assert.equal(isCweCategory('cwe-16'), true);
  assert.equal(getCweCategoryName('cwe-16'), 'Configuration');
});

test('leading zeros in the CWE number are tolerated', () => {
  assert.equal(isCweCategory('CWE-016'), true);
  assert.equal(getCweCategoryName('CWE-016'), 'Configuration');
});

test('a Weakness is not a Category', () => {
  assert.equal(isCweCategory('CWE-89'), false);
  assert.equal(getCweCategoryName('CWE-89'), undefined);
});

test('a View is not a Category', () => {
  assert.equal(isCweCategory('CWE-699'), false);
  assert.equal(isCweCategory('CWE-1000'), false);
});

test('the NOVEL escape-hatch form is not a Category', () => {
  assert.equal(isCweCategory('NOVEL-prompt-injection'), false);
  assert.equal(getCweCategoryName('NOVEL-prompt-injection'), undefined);
});

test('a bare number without the CWE- prefix is not a Category', () => {
  assert.equal(isCweCategory('16'), false);
  assert.equal(getCweCategoryName('16'), undefined);
});

test('empty, whitespace, and non-string inputs are not Categories', () => {
  assert.equal(isCweCategory(''), false);
  assert.equal(isCweCategory('   '), false);
  // eslint-disable-next-line no-undefined
  assert.equal(isCweCategory(undefined), false);
});

test('getCweCategories returns the full id -> name map', () => {
  const categories = getCweCategories();
  assert.equal(categories['CWE-16'], 'Configuration');
  assert.ok(Object.keys(categories).length > 400, 'the bundled map carries the full Category set');
});
