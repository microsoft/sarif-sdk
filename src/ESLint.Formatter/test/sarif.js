/**
 * @fileoverview Tests for SARIF format.
 * @author Microsoft
 */

"use strict";

//------------------------------------------------------------------------------
// Requirements
//------------------------------------------------------------------------------

const assert = require("chai").assert;
const formatter = require("../sarif-with-rules");

//------------------------------------------------------------------------------
// Global Test Content
//------------------------------------------------------------------------------

const rules = new Map();

rules.set("no-unused-vars", {
    meta: {
        type: "suggestion",
        docs: {
            description: "disallow unused variables",
            category: "Variables",
            recommended: true,
            url: "https://eslint.org/docs/rules/no-unused-vars"
        },
        fixable: "code"
    }
});
rules.set("no-extra-semi", {
    meta: {
        type: "suggestion",

        docs: {
            description: "disallow unnecessary semicolons",
            category: "Possible Errors",
            recommended: true,
            url: "https://eslint.org/docs/rules/no-extra-semi"
        },

        fixable: "code",
        schema: [],

        messages: {
            unexpected: "Unnecessary semicolon."
        }
    }
});

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
            const result = JSON.parse(formatter(code, null));

            assert.hasAllKeys(result.runs[0].artifacts, sourceFilePath);
            assert.strictEqual(result.runs[0].artifacts[sourceFilePath].artifactLocation.uri, sourceFilePath);
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

            assert.hasAllKeys(result.runs[0].artifacts, sourceFilePath);
            assert.strictEqual(result.runs[0].artifacts[sourceFilePath].artifactLocation.uri, sourceFilePath);
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
    describe("when passed one message with a rule id", () => {
        const ruleid = "no-unused-vars";
        const code = [{
            filePath: "service.js",
            messages: [{
                message: "Unexpected value.",
                ruleId: ruleid,
                source: "getValue()"
            }]
        }];

        it("should return a log with one rule", () => {
            const result = JSON.parse(formatter(code, rules));
            const rule = rules.get(ruleid);

            assert.hasAllKeys(result.runs[0].tool.driver.ruleDescriptors, ruleid);
            assert.strictEqual(result.runs[0].tool.driver.ruleDescriptors[ruleid].id, ruleid);
            assert.strictEqual(result.runs[0].tool.driver.ruleDescriptors[ruleid].shortDescription.text, rule.meta.docs.description);
            assert.strictEqual(result.runs[0].tool.driver.ruleDescriptors[ruleid].helpUri, rule.meta.docs.url);
            assert.strictEqual(result.runs[0].tool.driver.ruleDescriptors[ruleid].tags.category, rule.meta.docs.category);
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
    describe("when passed one message with line and column but no source string", () => {
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
            const result = JSON.parse(formatter(code, rules));

            assert.isUndefined(result.runs[0].results[0].locations[0].physicalLocation.region.startLine);
            assert.isUndefined(result.runs[0].results[0].locations[0].physicalLocation.region.startColumn);
            assert.strictEqual(result.runs[0].results[0].locations[0].physicalLocation.region.snippet.text, code[0].messages[0].source);
        });
    });
});

describe("formatter:sarif", () => {
    describe("when passed two results with two messages each", () => {
        const sourceFilePath1 = "service.js";
        const sourceFilePath2 = "utils.js";
        const ruleid1 = "no-unused-vars";
        const ruleid2 = "no-extra-semi";
        const ruleid3 = "custom-rule";

        rules.set(ruleid3, {
            meta: {
                type: "suggestion",
                docs: {
                    description: "custom description",
                    category: "Possible Errors"
                }
            }
        });
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
            },
            {
                message: "Custom error.",
                ruleId: ruleid3,
                line: 42
            }]
        }];

        it("should return a log with two files, three rules, and four results", () => {
            const result = JSON.parse(formatter(code, rules));
            const rule1 = rules.get(ruleid1);
            const rule2 = rules.get(ruleid2);
            const rule3 = rules.get(ruleid3);

            assert.lengthOf(result.runs[0].results, 4);

            assert.hasAllKeys(result.runs[0].tool.driver.ruleDescriptors, [ruleid1, ruleid2, ruleid3]);
            assert.hasAllKeys(result.runs[0].artifacts, [sourceFilePath1, sourceFilePath2]);

            assert.strictEqual(result.runs[0].files[sourceFilePath1].artifactLocation.uri, sourceFilePath1);
            assert.strictEqual(result.runs[0].files[sourceFilePath2].artifactLocation.uri, sourceFilePath2);

            assert.strictEqual(result.runs[0].tool.driver.ruleDescriptors[ruleid1].id, ruleid1);
            assert.strictEqual(result.runs[0].tool.driver.ruleDescriptors[ruleid1].shortDescription.text, rule1.meta.docs.description);
            assert.strictEqual(result.runs[0].tool.driver.ruleDescriptors[ruleid1].helpUri, rule1.meta.docs.url);
            assert.strictEqual(result.runs[0].tool.driver.ruleDescriptors[ruleid1].tags.category, rule1.meta.docs.category);

            assert.strictEqual(result.runs[0].tool.driver.ruleDescriptors[ruleid2].id, ruleid2);
            assert.strictEqual(result.runs[0].tool.driver.ruleDescriptors[ruleid2].shortDescription.text, rule2.meta.docs.description);
            assert.strictEqual(result.runs[0].tool.driver.ruleDescriptors[ruleid2].helpUri, rule2.meta.docs.url);
            assert.strictEqual(result.runs[0].tool.driver.ruleDescriptors[ruleid2].tags.category, rule2.meta.docs.category);

            assert.strictEqual(result.runs[0].tool.driver.ruleDescriptors[ruleid3].id, ruleid3);
            assert.strictEqual(result.runs[0].tool.driver.ruleDescriptors[ruleid3].shortDescription.text, rule3.meta.docs.description);
            assert.strictEqual(result.runs[0].tool.driver.ruleDescriptors[ruleid3].helpUri, rule3.meta.docs.url);
            assert.strictEqual(result.runs[0].tool.driver.ruleDescriptors[ruleid3].tags.category, rule3.meta.docs.category);

            assert.strictEqual(result.runs[0].results[0].ruleId, "the-rule");
            assert.strictEqual(result.runs[0].results[1].ruleId, ruleid1);
            assert.strictEqual(result.runs[0].results[2].ruleId, ruleid2);
            assert.strictEqual(result.runs[0].results[3].ruleId, ruleid3);

            assert.strictEqual(result.runs[0].results[0].level, "error");
            assert.strictEqual(result.runs[0].results[1].level, "warning");
            assert.strictEqual(result.runs[0].results[2].level, "error");
            assert.strictEqual(result.runs[0].results[3].level, "warning");

            assert.strictEqual(result.runs[0].results[0].message.text, "Unexpected value.");
            assert.strictEqual(result.runs[0].results[1].message.text, "Some warning.");
            assert.strictEqual(result.runs[0].results[2].message.text, "Unexpected something.");
            assert.strictEqual(result.runs[0].results[3].message.text, "Custom error.");

            assert.strictEqual(result.runs[0].results[0].locations[0].physicalLocation.artifactLocation.uri, sourceFilePath1);
            assert.strictEqual(result.runs[0].results[1].locations[0].physicalLocation.artifactLocation.uri, sourceFilePath1);
            assert.strictEqual(result.runs[0].results[2].locations[0].physicalLocation.artifactLocation.uri, sourceFilePath2);
            assert.strictEqual(result.runs[0].results[3].locations[0].physicalLocation.artifactLocation.uri, sourceFilePath2);

            assert.isUndefined(result.runs[0].results[0].locations[0].physicalLocation.region);

            assert.strictEqual(result.runs[0].results[1].locations[0].physicalLocation.region.startLine, 10);
            assert.strictEqual(result.runs[0].results[1].locations[0].physicalLocation.region.startColumn, 5);
            assert.strictEqual(result.runs[0].results[1].locations[0].physicalLocation.region.snippet.text, "doSomething(thingId)");

            assert.isUndefined(result.runs[0].results[2].locations[0].physicalLocation.region);
        });
    });
});
