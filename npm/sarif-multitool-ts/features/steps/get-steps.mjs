// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { When, Then } from '@cucumber/cucumber';
import assert from 'node:assert/strict';
import { getSchema, listSchemas, getSkill, listSkills, getCweTaxonomy } from '../../dist/index.js';

// ---------------------------------------------------------------------------
// get-schema
// ---------------------------------------------------------------------------

When('get-schema is invoked for {string}', function (verb) {
  this.schemaText = getSchema(verb);
});

When('get-schema is invoked for {string} expecting failure', function (verb) {
  try {
    getSchema(verb);
    assert.fail('get-schema was expected to fail but succeeded');
  } catch (err) {
    this.lastError = err;
  }
});

When('get-schema --list is invoked', function () {
  this.schemaList = listSchemas();
});

Then('the schema parses as JSON', function () {
  this.schema = JSON.parse(this.schemaText);
});

Then('the schema {string} contains {string}', function (key, fragment) {
  assert.ok(typeof this.schema[key] === 'string' && this.schema[key].includes(fragment));
});

Then('the schema list includes {string}', function (verb) {
  assert.ok(this.schemaList.includes(verb), `${verb} not in ${this.schemaList}`);
});

// ---------------------------------------------------------------------------
// get-skill
// ---------------------------------------------------------------------------

When('get-skill is invoked for {string} with pinRef {string}', function (name, pinRef) {
  this.skillMd = getSkill(name, pinRef);
});

When('get-skill --list is invoked', function () {
  this.skillList = listSkills();
});

Then('the skill markdown contains {string}', function (fragment) {
  assert.ok(this.skillMd.includes(fragment), `skill does not contain '${fragment}'`);
});

Then('the skill markdown contains no {string}', function (fragment) {
  assert.ok(!this.skillMd.includes(fragment), `skill still contains '${fragment}'`);
});

Then('the skill list includes {string} with a non-empty description', function (name) {
  const entry = this.skillList.find((s) => s.name === name);
  assert.ok(entry && typeof entry.description === 'string' && entry.description.length > 0);
});

// ---------------------------------------------------------------------------
// get-cwe
// ---------------------------------------------------------------------------

When('get-cwe is invoked with default statuses', function () {
  this.cweLog = getCweTaxonomy();
});

Then('the taxonomy is a SARIF log with at least one taxonomy', function () {
  assert.ok(Array.isArray(this.cweLog.runs) && this.cweLog.runs[0]?.taxonomies?.length > 0);
});

Then(
  "every taxon's {string} property is one of {string}, {string}, {string}",
  function (prop, a, b, c) {
    const allowed = new Set([a, b, c]);
    for (const run of this.cweLog.runs) {
      for (const tx of run.taxonomies ?? []) {
        for (const t of tx.taxa ?? []) {
          const v = t.properties?.[prop];
          if (v !== undefined) {
            assert.ok(allowed.has(v), `taxon ${t.id} has ${prop}=${v}`);
          }
        }
      }
    }
  },
);
