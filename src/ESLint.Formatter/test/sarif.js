/**
 * @fileoverview Tests for SARIF format.
 * @author Chris Meyer
 */

"use strict";


//------------------------------------------------------------------------------
// Requirements
//------------------------------------------------------------------------------

const formatter = require("../sarif");


//------------------------------------------------------------------------------
// Tests
//------------------------------------------------------------------------------

describe("formatter:sarif", () => {
    describe("when passed no messages", () => {
        const sourceFilePath = "foo.js";
        const code = [{
            filePath: sourceFilePath,
            messages: []
        }];

        it("should return a log with one file and no results", () => {
            const result = JSON.parse(formatter(code));

            assert.hasAllKeys(result.runs[0].files, sourceFilePath);
            assert.strictEqual(result.runs[0].files[sourceFilePath].fileLocation.uri, sourceFilePath);
            assert.isUndefined(result.runs[0].results);
        });
    });
});

describe("formatter:sarif", () => {
    describe("when passed one message", () => {
        const sourceFilePath = "foo.js";
        const code = [{
            filePath: sourceFilePath,
            messages: [{
                message: "Unexpected foo.",
                severity: 2
            }]
        }];

        it("should return a log with one file and one result", () => {
            const result = JSON.parse(formatter(code));

            assert.hasAllKeys(result.runs[0].files, sourceFilePath);
            assert.strictEqual(result.runs[0].files[sourceFilePath].fileLocation.uri, sourceFilePath);
            assert.isDefined(result.runs[0].results);
            assert.lengthOf(result.runs[0].results, 1);
            assert.strictEqual(result.runs[0].results[0].level, "error");
            assert.isDefined(result.runs[0].results[0].message);
            assert.strictEqual(result.runs[0].results[0].message.text, code[0].messages[0].message);
            assert.strictEqual(result.runs[0].results[0].locations[0].physicalLocation.fileLocation.uri, sourceFilePath);
        });
    });
});

describe("formatter:sarif", () => {
    describe("when passed one message with line but no column nor source string", () => {
        const code = [{
            filePath: "foo.js",
            messages: [{
                message: "Unexpected foo.",
                line: 10
            }]
        }];

        it("should return a log with one result whose location contains a region with only a line #", () => {
            const result = JSON.parse(formatter(code));

            assert.strictEqual(result.runs[0].results[0].locations[0].physicalLocation.region.startLine, code[0].messages[0].line);
            assert.isUndefined(result.runs[0].results[0].locations[0].physicalLocation.region.startColumn);
            assert.isUndefined(result.runs[0].results[0].locations[0].physicalLocation.region.snippet);
        });
    });
});

describe("formatter:sarif", () => {
    describe("when passed one message with line and column nor source string", () => {
        const code = [{
            filePath: "foo.js",
            messages: [{
                message: "Unexpected foo.",
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
            filePath: "foo.js",
            messages: [{
                message: "Unexpected foo.",
                line: 10,
                column: 5,
                source: "getFoo()"
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
            filePath: "foo.js",
            messages: [{
                message: "Unexpected foo.",
                severity: 2,
                ruleId: "foo",
                source: "getFoo()"
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
        const sourceFilePath1 = "foo.js";
        const sourceFilePath2 = "bar.js";
        const ruleid1 = "no-unused-vars";
        const ruleid2 = "no-extra-semi";
        const code = [{
            filePath: sourceFilePath1,
            messages: [{
                message: "Unexpected foo.",
                severity: 2,
                ruleId: "foo"
            },
            {
                ruleId: ruleid1,
                message: "Foo warning.",
                severity: 1,
                line: 10,
                column: 5,
                source: "doSomething(thingId)"
            }]
        },
        {
            filePath: sourceFilePath2,
            messages: [{
                message: "Unexpected bar.",
                severity: 2,
                ruleId: ruleid2,
                line: 18
            }]
        }];

        it("should return a log with two files and three results", () => {
            const result = JSON.parse(formatter(code));

            assert.lengthOf(result.runs[0].results, 3);
            
            assert.hasAllKeys(result.runs[0].files, [sourceFilePath1, sourceFilePath2]);

            assert.strictEqual(result.runs[0].files[sourceFilePath1].fileLocation.uri, sourceFilePath1);
            assert.strictEqual(result.runs[0].files[sourceFilePath2].fileLocation.uri, sourceFilePath2);

            assert.strictEqual(result.runs[0].results[0].level, "error");
            assert.strictEqual(result.runs[0].results[1].level, "warning");
            assert.strictEqual(result.runs[0].results[2].level, "error");

            assert.strictEqual(result.runs[0].results[0].message.text, "Unexpected foo.");
            assert.strictEqual(result.runs[0].results[1].message.text, "Foo warning.");
            assert.strictEqual(result.runs[0].results[2].message.text, "Unexpected bar.");

            assert.strictEqual(result.runs[0].results[0].locations[0].physicalLocation.fileLocation.uri, sourceFilePath1);
            assert.strictEqual(result.runs[0].results[1].locations[0].physicalLocation.fileLocation.uri, sourceFilePath1);
            assert.strictEqual(result.runs[0].results[2].locations[0].physicalLocation.fileLocation.uri, sourceFilePath2);

            assert.isUndefined(result.runs[0].results[0].locations[0].physicalLocation.region);

            assert.strictEqual(result.runs[0].results[1].locations[0].physicalLocation.region.startLine, 10);
            assert.strictEqual(result.runs[0].results[1].locations[0].physicalLocation.region.startColumn, 5);
            assert.strictEqual(result.runs[0].results[1].locations[0].physicalLocation.region.snippet.text, "doSomething(thingId)");

            assert.strictEqual(result.runs[0].results[2].locations[0].physicalLocation.region.startLine, 18);
            assert.isUndefined(result.runs[0].results[2].locations[0].physicalLocation.region.snippet);
        });
    });
});
