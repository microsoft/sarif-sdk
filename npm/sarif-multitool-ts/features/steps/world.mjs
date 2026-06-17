// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { setWorldConstructor, After, setDefaultTimeout } from '@cucumber/cucumber';
import { mkdtempSync, rmSync, mkdirSync, writeFileSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import { pathToFileURL } from 'node:url';

setDefaultTimeout(30_000);

/**
 * Cucumber world: one temp directory per scenario, holding the .sarif output
 * path, its .wip.jsonl sibling, and a fixture source tree under src/.
 */
class EmitWorld {
  constructor() {
    this.tmp = undefined;
    this.output = undefined;
    this.runHeader = undefined;
    this.lastOutcome = undefined;
    this.lastError = undefined;
    this.replayedLog = undefined;
    this.finalizedLog = undefined;
    /** Test seam: env getter for CI-context detection. Default = no CI. */
    this.env = () => undefined;
  }

  /** Creates the temp dir and a default fixture source file. */
  init() {
    this.tmp = mkdtempSync(join(tmpdir(), 'sarif-emit-'));
    this.output = join(this.tmp, 'out.sarif');
    mkdirSync(join(this.tmp, 'src'), { recursive: true });
    // Default fixture so finalize scenarios that don't declare one still resolve.
    writeFileSync(join(this.tmp, 'src', 'app.ts'), 'const x = 1;\nconst y = 2;\n', 'utf8');
  }

  /** file:// URI for the temp dir root, with trailing slash. */
  srcRootUri() {
    let u = pathToFileURL(this.tmp).toString();
    if (!u.endsWith('/')) u += '/';
    return u;
  }

  /** A minimal run header that finalize can rebase (GitHub VCP, SRCROOT bound). */
  finalizableRunHeader(withVcp = true) {
    const header = {
      tool: { driver: { name: 'ai-scanner' } },
      originalUriBaseIds: { SRCROOT: { uri: this.srcRootUri() } },
    };
    if (withVcp) {
      header.versionControlProvenance = [
        {
          repositoryUri: 'https://github.com/contoso/widgets',
          revisionId: 'a1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6e7f8a9b0',
          mappedTo: { uriBaseId: 'SRCROOT' },
        },
      ];
    }
    return header;
  }

  cleanup() {
    if (this.tmp) {
      try {
        rmSync(this.tmp, { recursive: true, force: true });
      } catch {
        /* best-effort */
      }
    }
  }
}

setWorldConstructor(EmitWorld);

After(function () {
  this.cleanup();
});
