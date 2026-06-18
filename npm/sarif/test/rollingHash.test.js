// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { test } from 'node:test';
import assert from 'node:assert/strict';
import { computeRollingHashes } from '../dist/rollingHash.js';

// The expected values below are the exact fixtures asserted by the C#
// HashUtilitiesTests.RollingHash_* tests (src/Test.UnitTests.Sarif/
// HashUtilitiesTests.cs), which in turn derive from
// github/codeql-action src/fingerprints.test.ts. Byte-identical reproduction
// here is the parity proof that this BigInt port matches both the CLR Long
// port and the normative CodeQL implementation.

function assertHashes(fileText, expected) {
  const actual = computeRollingHashes(fileText);
  const actualObj = Object.fromEntries(actual);
  assert.deepEqual(actualObj, expected);
}

test('RollingHash empty string yields the line-1 flush hash', () => {
  assertHashes('', { 1: 'c129715d7a2bc9a3:1' });
});

test('RollingHash NewLineCombo1', () => {
  assertHashes(' a\nb\n  \t\tc\n d', {
    1: '271789c17abda88f:1',
    2: '54703d4cd895b18:1',
    3: '180aee12dab6264:1',
    4: 'a23a3dc5e078b07b:1',
  });
});

test('RollingHash NewLineCombo2', () => {
  assertHashes(' hello; \t\nworld!!!\n\n\n  \t\tGreetings\n End', {
    1: '8b7cf3e952e7aeb2:1',
    2: 'b1ae1287ec4718d9:1',
    3: 'bff680108adb0fcc:1',
    4: 'c6805c5e1288b612:1',
    5: 'b86d3392aea1be30:1',
    6: 'e6ceba753e1a442:1',
  });
});

test('RollingHash NewLineCombo3 (trailing newline)', () => {
  assertHashes(' hello; \t\nworld!!!\n\n\n  \t\tGreetings\n End\n', {
    1: 'e9496ae3ebfced30:1',
    2: 'fb7c023a8b9ccb3f:1',
    3: 'ce8ba1a563dcdaca:1',
    4: 'e20e36e16fcb0cc8:1',
    5: 'b3edc88f2938467e:1',
    6: 'c8e28b0b4002a3a0:1',
    7: 'c129715d7a2bc9a3:1',
  });
});

// CR-only and CRLF newline variants must normalize to the identical fingerprints
// as the LF form in NewLineCombo3.
const NORMALIZED_NEWLINE_EXPECTED = {
  1: 'e9496ae3ebfced30:1',
  2: 'fb7c023a8b9ccb3f:1',
  3: 'ce8ba1a563dcdaca:1',
  4: 'e20e36e16fcb0cc8:1',
  5: 'b3edc88f2938467e:1',
  6: 'c8e28b0b4002a3a0:1',
  7: 'c129715d7a2bc9a3:1',
};

test('RollingHash NewLineCombo4 (CR-only)', () => {
  assertHashes('hello; \t\nworld!!!\r\r\r  \t\tGreetings\r End\r', NORMALIZED_NEWLINE_EXPECTED);
});

test('RollingHash NewLineCombo5 (CRLF)', () => {
  assertHashes(
    ' hello; \t\r\nworld!!!\r\n\r\n\r\n  \t\tGreetings\r\n End\r\n',
    NORMALIZED_NEWLINE_EXPECTED,
  );
});

test('RollingHash NewLineCombo6 (mixed newlines)', () => {
  assertHashes(' hello; \t\nworld!!!\r\n\n\r  \t\tGreetings\r End\r\n', NORMALIZED_NEWLINE_EXPECTED);
});

test('RollingHash NewLineCombo7 dedups identical lines with an ordinal suffix', () => {
  const line = 'Lorem ipsum dolor sit amet.\n';
  assertHashes(line.repeat(10), {
    1: 'a7f2ff13bc495cf2:1',
    2: 'a7f2ff13bc495cf2:2',
    3: 'a7f2ff13bc495cf2:3',
    4: 'a7f2ff13bc495cf2:4',
    5: 'a7f2ff13bc495cf2:5',
    6: 'a7f2ff13bc495cf2:6',
    7: 'a7f2ff1481e87703:1',
    8: 'a9cf91f7bbf1862b:1',
    9: '55ec222b86bcae53:1',
    10: 'cc97dc7b1d7d8f7b:1',
    11: 'c129715d7a2bc9a3:1',
  });
});

test('RollingHash returns an empty map for null input', () => {
  const actual = computeRollingHashes(null);
  assert.equal(actual.size, 0);
});
