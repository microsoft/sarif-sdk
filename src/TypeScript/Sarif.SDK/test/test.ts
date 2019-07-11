// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { expect } from "chai";
import "mocha";
import * as sarif from "../lib/index";

describe("CreateEmptySarifLog", () => 
it("should return an empty runs array", () =>
{
    // Create an empty sarif log--and ensure that the log is empty.
    const sarifLog = sarif.CreateEmptySarifLog();
    expect(sarifLog.version).to.equal("2.1.0");
    expect(sarifLog.runs.length).to.equal(0);
})
);
