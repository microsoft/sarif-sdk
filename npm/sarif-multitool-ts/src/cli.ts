#!/usr/bin/env node
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import process from 'node:process';

const verb = process.argv[2] ?? '';

process.stderr.write(
  `@microsoft/sarif-multitool-ts v0.0.x placeholder — '${verb || '<verb>'}' is not yet implemented.\n` +
    `Use the .NET-backed wrapper instead:\n` +
    `  npx @microsoft/sarif-multitool ${process.argv.slice(2).join(' ')}\n` +
    `Track progress at https://github.com/microsoft/sarif-sdk.\n`,
);
process.exit(1);
