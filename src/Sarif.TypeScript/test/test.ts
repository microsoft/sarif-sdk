import { expect }from "chai";
import "mocha";
import * as sarif from "../lib/index";

describe("CreateEmptySarifLog", () => 
it("should return empty runs", () =>
{
    // Create an empty sarif log--and ensure that the log is empty.
    const sarifLog = sarif.CreateEmptySarifLog();
    expect(sarifLog.version).to.equal("2.0.0");
    expect(sarifLog.runs.length).to.equal(0);
})
);
