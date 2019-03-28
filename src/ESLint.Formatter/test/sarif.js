/**
 * @fileoverview Tests for SARIF format.
 * @author Microsoft
 */

"use strict";

//------------------------------------------------------------------------------
// Requirements
//------------------------------------------------------------------------------

const assert = require("chai").assert;
const formatter = require("../sarif");

//------------------------------------------------------------------------------
// Tests
//------------------------------------------------------------------------------

describe("formatter:sarif", () => {
    describe("when passed no messages", () => {
        const sourceFilePath = "service.js";
        const code = [{
            filePath: sourceFilePath,
            messages: []
        }];

        it("should return a log with one file and no results", () => {
            const result = JSON.parse(formatter(code));
            
            assert.strictEqual(result.runs[0].artifacts[0].location.uri, sourceFilePath);
            assert.isUndefined(result.runs[0].results);
        });
    });
});

describe("formatter:sarif", () => {
    describe("when passed one message", () => {
        const sourceFilePath = "service.js";
        const code = [{
            filePath: sourceFilePath,
            messages: [{
                message: "Unexpected value.",
                severity: 2
            }]
        }];

        it("should return a log with one file and one result", () => {
            const result = JSON.parse(formatter(code));

            assert.strictEqual(result.runs[0].artifacts[0].location.uri, sourceFilePath);
            assert.isDefined(result.runs[0].results);
            assert.lengthOf(result.runs[0].results, 1);
            assert.strictEqual(result.runs[0].results[0].level, "error");
            assert.isDefined(result.runs[0].results[0].message);
            assert.strictEqual(result.runs[0].results[0].message.text, code[0].messages[0].message);
            assert.strictEqual(result.runs[0].results[0].locations[0].physicalLocation.artifactLocation.uri, sourceFilePath);
        });
    });
});

describe("formatter:sarif", () => {
    describe("when passed one message with line but no column nor source string", () => {
        const code = [{
            filePath: "service.js",
            messages: [{
                message: "Unexpected value.",
                line: 10
            }]
        }];

        it("should return a log with one result whose location does not contain a region", () => {
            const result = JSON.parse(formatter(code));
            
            assert.isUndefined(result.runs[0].results[0].locations[0].physicalLocation.region);
        });
    });
});

describe("formatter:sarif", () => {
    describe("when passed one message with line and column nor source string", () => {
        const code = [{
            filePath: "service.js",
            messages: [{
                message: "Unexpected value.",
                line: 10,
                column: 5
            }]
        }];

        it("should return a log with one result whose location contains a region with line and column #s", () => {
            const result = JSON.parse(formatter(code));

            assert.strictEqual(result.runs[0].results[0].locations[0].physicalLocation.region.startLine, code[0].messages[0].line);
            assert.strictEqual(result.runs[0].results[0].locations[0].physicalLocation.region.startColumn, code[0].messages[0].column);
            assert.isUndefined(result.runs[0].results[0].locations[0].physicalLocation.region.snippet);
        });
    });
});

describe("formatter:sarif", () => {
    describe("when passed one message with line, column, and source string", () => {
        const code = [{
            filePath: "service.js",
            messages: [{
                message: "Unexpected value.",
                line: 10,
                column: 5,
                source: "getValue()"
            }]
        }];

        it("should return a log with one result whose location contains a region with line and column #s", () => {
            const result = JSON.parse(formatter(code));

            assert.strictEqual(result.runs[0].results[0].locations[0].physicalLocation.region.startLine, code[0].messages[0].line);
            assert.strictEqual(result.runs[0].results[0].locations[0].physicalLocation.region.startColumn, code[0].messages[0].column);
            assert.strictEqual(result.runs[0].results[0].locations[0].physicalLocation.region.snippet.text, code[0].messages[0].source);
        });
    });
});

describe("formatter:sarif", () => {
    describe("when passed one message with a source string but without line and column #s", () => {
        const code = [{
            filePath: "service.js",
            messages: [{
                message: "Unexpected value.",
                severity: 2,
                ruleId: "the-rule",
                source: "getValue()"
            }]
        }];

        it("should return a log with one result whose location contains a region with line and column #s", () => {
            const result = JSON.parse(formatter(code));

            assert.isUndefined(result.runs[0].results[0].locations[0].physicalLocation.region.startLine);
            assert.isUndefined(result.runs[0].results[0].locations[0].physicalLocation.region.startColumn);
            assert.strictEqual(result.runs[0].results[0].locations[0].physicalLocation.region.snippet.text, code[0].messages[0].source);
        });
    });
});

describe("formatter:sarif", () => {
    describe("when passed two results with two and one messages, respectively", () => {
        const sourceFilePath1 = "service.js";
        const sourceFilePath2 = "utils.js";
        const ruleid1 = "no-unused-vars";
        const ruleid2 = "no-extra-semi";
        const code = [{
            filePath: sourceFilePath1,
            messages: [{
                message: "Unexpected value.",
                severity: 2,
                ruleId: "the-rule"
            },
            {
                ruleId: ruleid1,
                message: "Some warning.",
                severity: 1,
                line: 10,
                column: 5,
                source: "doSomething(thingId)"
            }]
        },
        {
            filePath: sourceFilePath2,
            messages: [{
                message: "Unexpected something.",
                severity: 2,
                ruleId: ruleid2,
                line: 18
            }]
        }];

        it("should return a log with two files and three results", () => {
            const text = formatter(code);
            const result = JSON.parse(text);

            assert.lengthOf(result.runs[0].results, 3);

            assert.strictEqual(result.runs[0].artifacts[0].location.uri, sourceFilePath1);
            assert.strictEqual(result.runs[0].artifacts[1].location.uri, sourceFilePath2);

            assert.strictEqual(result.runs[0].results[0].level, "error");
            assert.strictEqual(result.runs[0].results[1].level, "warning");
            assert.strictEqual(result.runs[0].results[2].level, "error");

            assert.strictEqual(result.runs[0].results[0].message.text, "Unexpected value.");
            assert.strictEqual(result.runs[0].results[1].message.text, "Some warning.");
            assert.strictEqual(result.runs[0].results[2].message.text, "Unexpected something.");

            assert.strictEqual(result.runs[0].results[0].locations[0].physicalLocation.artifactLocation.uri, sourceFilePath1);
            assert.strictEqual(result.runs[0].results[1].locations[0].physicalLocation.artifactLocation.uri, sourceFilePath1);
            assert.strictEqual(result.runs[0].results[2].locations[0].physicalLocation.artifactLocation.uri, sourceFilePath2);

            assert.isUndefined(result.runs[0].results[0].locations[0].physicalLocation.region);

            assert.strictEqual(result.runs[0].results[1].locations[0].physicalLocation.region.startLine, 10);
            assert.strictEqual(result.runs[0].results[1].locations[0].physicalLocation.region.startColumn, 5);
            assert.strictEqual(result.runs[0].results[1].locations[0].physicalLocation.region.snippet.text, "doSomething(thingId)");
            
            assert.isUndefined(result.runs[0].results[2].locations[0].physicalLocation.region);
        });
    });
});
