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
// Global Test Content
//------------------------------------------------------------------------------

const rules = {
    "no-unused-vars": {
        type: "suggestion",
        docs: {
            description: "disallow unused variables",
            category: "Variables",
            recommended: true,
            url: "https://eslint.org/docs/rules/no-unused-vars"
        },
        fixable: "code"
    },
    "no-extra-semi": {
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
};

const sourceFilePath1 = "service.js"
const sourceFilePath2 = "utils.js"
const uriPrefix = "file:///"
const testRuleId = "ESL0001"

//------------------------------------------------------------------------------
// Tests
//------------------------------------------------------------------------------

describe("formatter:sarif", () => {
    describe("when run", () => {
        const code = [];

        it ("should return a log with correct version and tool metadata", () => {
            const log = JSON.parse(formatter(code, null));

            assert.strictEqual(log['$schema'], 'http://json.schemastore.org/sarif-2.1.0-rtm.4');
            assert.strictEqual(log.version, '2.1.0');

            assert.strictEqual(log.runs[0].tool.driver.name, "ESLint");
            assert.strictEqual(log.runs[0].tool.driver.informationUri, "https://eslint.org");
        })
    });

    describe("when passed no messages", () => {
        const code = [{
            filePath: sourceFilePath1,
            messages: []
        }];

        it("should return a log with files, but no results", () => {
            const log = JSON.parse(formatter(code, null));

            assert.isDefined(log.runs[0].artifacts);
            assert(log.runs[0].artifacts[0].location.uri.startsWith(uriPrefix));
            assert(log.runs[0].artifacts[0].location.uri.endsWith("/" + sourceFilePath1));
            assert.lengthOf(log.runs[0].results, 0);
        });
    });
});

describe("formatter:sarif", () => {
    describe("when passed one message", () => {
        const code = [{
            filePath: sourceFilePath1,
            messages: [{
                message: "Unexpected value.",
                severity: 2,
                ruleId: testRuleId
            }]
        }];

        it("should return a log with one file and one result", () => {
            const log = JSON.parse(formatter(code, { rulesMeta: rules }));
            
            assert(log.runs[0].artifacts[0].location.uri.startsWith(uriPrefix));
            assert(log.runs[0].artifacts[0].location.uri, "/" + sourceFilePath1);
            assert.isDefined(log.runs[0].results);
            assert.lengthOf(log.runs[0].results, 1);
            assert.strictEqual(log.runs[0].results[0].level, "error");
            assert.isDefined(log.runs[0].results[0].message);
            assert.strictEqual(log.runs[0].results[0].message.text, code[0].messages[0].message);
            assert(log.runs[0].results[0].locations[0].physicalLocation.artifactLocation.uri.startsWith(uriPrefix));
            assert(log.runs[0].results[0].locations[0].physicalLocation.artifactLocation.uri.endsWith("/" + sourceFilePath1));
            assert.strictEqual(log.runs[0].results[0].locations[0].physicalLocation.artifactLocation.index, 0);
        });
    });
});

describe("formatter:sarif", () => {
    describe("when passed one message with a rule id", () => {
        const ruleid = "no-unused-vars";
        const code = [{
            filePath: sourceFilePath1,
            messages: [{
                message: "Unexpected value.",
                ruleId: ruleid,
                source: "getValue()"
            }]
        }];

        it("should return a log with one rule", () => {
            const log = JSON.parse(formatter(code, { rulesMeta: rules }));
            const rule = rules[ruleid];

            assert.strictEqual(log.runs[0].tool.driver.rules.length, 1);
            assert.strictEqual(log.runs[0].tool.driver.rules[0].id, ruleid);
            assert.strictEqual(log.runs[0].tool.driver.rules[0].shortDescription.text, rule.docs.description);
            assert.strictEqual(log.runs[0].tool.driver.rules[0].helpUri, rule.docs.url);
            assert.strictEqual(log.runs[0].tool.driver.rules[0].properties.category, rule.docs.category);
        });
    });
});

describe("formatter:sarif", () => {
    describe("when passed one message with line but no column nor source string", () => {
        const code = [{
            filePath: sourceFilePath1,
            messages: [{
                message: "Unexpected value.",
                ruleId: testRuleId,
                line: 10
            }]
        }];

        it("should return a log with one result whose location region has only a startLine", () => {
            const log = JSON.parse(formatter(code));

            assert.strictEqual(log.runs[0].results[0].locations[0].physicalLocation.region.startLine, code[0].messages[0].line);
            assert.isUndefined(log.runs[0].results[0].locations[0].physicalLocation.region.startColumn);
            assert.isUndefined(log.runs[0].results[0].locations[0].physicalLocation.region.snippet);
        });
    });
});

describe("formatter:sarif", () => {
    describe("when passed one message with line and column but no source string", () => {
        const code = [{
            filePath: sourceFilePath1,
            messages: [{
                message: "Unexpected value.",
                ruleId: testRuleId,
                line: 10,
                column: 5
            }]
        }];

        it("should return a log with one result whose location contains a region with line and column #s", () => {
            const log = JSON.parse(formatter(code));

            assert.strictEqual(log.runs[0].results[0].locations[0].physicalLocation.region.startLine, code[0].messages[0].line);
            assert.strictEqual(log.runs[0].results[0].locations[0].physicalLocation.region.startColumn, code[0].messages[0].column);
            assert.isUndefined(log.runs[0].results[0].locations[0].physicalLocation.region.snippet);
        });
    });
});

describe("formatter:sarif", () => {
    describe("when passed one message with line, column, and source string", () => {
        const code = [{
            filePath: "service.js",
            messages: [{
                message: "Unexpected value.",
                ruleId: testRuleId,
                line: 10,
                column: 5,
                source: "getValue()"
            }]
        }];

        it("should return a log with one result whose location contains a region with line and column #s", () => {
            const log = JSON.parse(formatter(code));

            assert.strictEqual(log.runs[0].results[0].locations[0].physicalLocation.region.startLine, code[0].messages[0].line);
            assert.strictEqual(log.runs[0].results[0].locations[0].physicalLocation.region.startColumn, code[0].messages[0].column);
            assert.strictEqual(log.runs[0].results[0].locations[0].physicalLocation.region.snippet.text, code[0].messages[0].source);
        });
    });
});

describe("formatter:sarif", () => {
    describe("when passed one message with a source string but without line and column #s", () => {
        const code = [{
            filePath: sourceFilePath1,
            messages: [{
                message: "Unexpected value.",
                severity: 2,
                ruleId: testRuleId,
                source: "getValue()"
            }]
        }];

        it("should return a log with one result whose location contains a region with line and column #s", () => {
            const log = JSON.parse(formatter(code, rules));

            assert.isUndefined(log.runs[0].results[0].locations[0].physicalLocation.region.startLine);
            assert.isUndefined(log.runs[0].results[0].locations[0].physicalLocation.region.startColumn);
            assert.strictEqual(log.runs[0].results[0].locations[0].physicalLocation.region.snippet.text, code[0].messages[0].source);
        });
    });
});

describe("formatter:sarif", () => {
    describe("when passed two results with two messages each", () => {
        const ruleid1 = "no-unused-vars";
        const ruleid2 = "no-extra-semi";
        const ruleid3 = "custom-rule";

        rules[ruleid3] = {
            type: "suggestion",
            docs: {
                description: "custom description",
                category: "Possible Errors"
            }
        };
        const code = [{
            filePath: sourceFilePath1,
            messages: [{
                message: "Unexpected value.",
                severity: 2,
                ruleId: testRuleId
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
            const log = JSON.parse(formatter(code, { rulesMeta: rules }));
            const rule1 = rules[ruleid1];
            const rule2 = rules[ruleid2];
            const rule3 = rules[ruleid3];

            assert.lengthOf(log.runs[0].results, 4);

            assert.strictEqual(log.runs[0].tool.driver.rules[0].id, ruleid1);
            assert.strictEqual(log.runs[0].tool.driver.rules[1].id, ruleid2);
            assert.strictEqual(log.runs[0].tool.driver.rules[2].id, ruleid3);

            assert(log.runs[0].artifacts[0].location.uri.startsWith(uriPrefix));
            assert(log.runs[0].artifacts[0].location.uri, sourceFilePath1);

            assert(log.runs[0].artifacts[1].location.uri.startsWith(uriPrefix));
            assert(log.runs[0].artifacts[1].location.uri, sourceFilePath2);


            assert.strictEqual(log.runs[0].tool.driver.rules[0].id, ruleid1);
            assert.strictEqual(log.runs[0].tool.driver.rules[0].shortDescription.text, rule1.docs.description);
            assert.strictEqual(log.runs[0].tool.driver.rules[0].helpUri, rule1.docs.url);
            assert.strictEqual(log.runs[0].tool.driver.rules[0].properties.category, rule1.docs.category);

            assert.strictEqual(log.runs[0].tool.driver.rules[1].id, ruleid2);
            assert.strictEqual(log.runs[0].tool.driver.rules[1].shortDescription.text, rule2.docs.description);
            assert.strictEqual(log.runs[0].tool.driver.rules[1].helpUri, rule2.docs.url);
            assert.strictEqual(log.runs[0].tool.driver.rules[1].properties.category, rule2.docs.category);

            assert.strictEqual(log.runs[0].tool.driver.rules[2].id, ruleid3);
            assert.strictEqual(log.runs[0].tool.driver.rules[2].shortDescription.text, rule3.docs.description);
            assert.strictEqual(log.runs[0].tool.driver.rules[2].helpUri, rule3.docs.url);
            assert.strictEqual(log.runs[0].tool.driver.rules[2].properties.category, rule3.docs.category);

            assert.strictEqual(log.runs[0].results[0].ruleId, testRuleId);
            assert.strictEqual(log.runs[0].results[1].ruleId, ruleid1);
            assert.strictEqual(log.runs[0].results[2].ruleId, ruleid2);
            assert.strictEqual(log.runs[0].results[3].ruleId, ruleid3);

            assert.isUndefined(log.runs[0].results[0].ruleIndex); // Custom rule: no metadata available.
            assert.strictEqual(log.runs[0].results[1].ruleIndex, 0);
            assert.strictEqual(log.runs[0].results[2].ruleIndex, 1);
            assert.strictEqual(log.runs[0].results[3].ruleIndex, 2);

            assert.strictEqual(log.runs[0].results[0].level, "error");
            assert.strictEqual(log.runs[0].results[1].level, "warning");
            assert.strictEqual(log.runs[0].results[2].level, "error");
            assert.strictEqual(log.runs[0].results[3].level, "warning");

            assert.strictEqual(log.runs[0].results[0].message.text, "Unexpected value.");
            assert.strictEqual(log.runs[0].results[1].message.text, "Some warning.");
            assert.strictEqual(log.runs[0].results[2].message.text, "Unexpected something.");
            assert.strictEqual(log.runs[0].results[3].message.text, "Custom error.");

            assert(log.runs[0].results[0].locations[0].physicalLocation.artifactLocation.uri.startsWith(uriPrefix));
            assert(log.runs[0].results[0].locations[0].physicalLocation.artifactLocation.uri.endsWith("/" + sourceFilePath1));
            assert(log.runs[0].results[1].locations[0].physicalLocation.artifactLocation.uri.startsWith(uriPrefix));
            assert(log.runs[0].results[1].locations[0].physicalLocation.artifactLocation.uri.endsWith("/" + sourceFilePath1));
            assert(log.runs[0].results[2].locations[0].physicalLocation.artifactLocation.uri.startsWith(uriPrefix));
            assert(log.runs[0].results[2].locations[0].physicalLocation.artifactLocation.uri.endsWith("/" + sourceFilePath2));
            assert(log.runs[0].results[3].locations[0].physicalLocation.artifactLocation.uri.startsWith(uriPrefix));
            assert(log.runs[0].results[3].locations[0].physicalLocation.artifactLocation.uri.endsWith("/" + sourceFilePath2));

            assert.strictEqual(log.runs[0].results[0].locations[0].physicalLocation.artifactLocation.index, 0);
            assert.strictEqual(log.runs[0].results[1].locations[0].physicalLocation.artifactLocation.index, 0);
            assert.strictEqual(log.runs[0].results[2].locations[0].physicalLocation.artifactLocation.index, 1);
            assert.strictEqual(log.runs[0].results[3].locations[0].physicalLocation.artifactLocation.index, 1);

            assert.isUndefined(log.runs[0].results[0].locations[0].physicalLocation.region);

            assert.strictEqual(log.runs[0].results[1].locations[0].physicalLocation.region.startLine, 10);
            assert.strictEqual(log.runs[0].results[1].locations[0].physicalLocation.region.startColumn, 5);
            assert.strictEqual(log.runs[0].results[1].locations[0].physicalLocation.region.snippet.text, "doSomething(thingId)");

            assert.strictEqual(log.runs[0].results[2].locations[0].physicalLocation.region.startLine, 18);
            assert.isUndefined(log.runs[0].results[2].locations[0].physicalLocation.region.startColumn);
            assert.isUndefined(log.runs[0].results[2].locations[0].physicalLocation.region.snippet);
        });
    });
});

describe("formatter:sarif", () => {
    describe("when passed two results with one having no message and one with two messages", () => {
        const ruleid1 = "no-unused-vars";
        const ruleid2 = "no-extra-semi";
        const ruleid3 = "custom-rule";

        rules[ruleid3] = {
            type: "suggestion",
            docs: {
                description: "custom description",
                category: "Possible Errors"
            }
        };
        const code = [{
            filePath: sourceFilePath1,
            messages: [ ]
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

        it("should return a log with two files, two rules, and two results", () => {
            const log = JSON.parse(formatter(code, { rulesMeta: rules }));
            const rule2 = rules[ruleid2];
            const rule3 = rules[ruleid3];

            assert.lengthOf(log.runs[0].artifacts, 2);
            assert.lengthOf(log.runs[0].results, 2);

            assert.strictEqual(log.runs[0].tool.driver.rules[0].id, ruleid2);
            assert.strictEqual(log.runs[0].tool.driver.rules[1].id, ruleid3);

            assert(log.runs[0].artifacts[0].location.uri.startsWith(uriPrefix));
            assert(log.runs[0].artifacts[0].location.uri.endsWith(sourceFilePath1));
            assert(log.runs[0].artifacts[1].location.uri.startsWith(uriPrefix));
            assert(log.runs[0].artifacts[1].location.uri.endsWith(sourceFilePath2));

            assert.strictEqual(log.runs[0].tool.driver.rules[0].id, ruleid2);
            assert.strictEqual(log.runs[0].tool.driver.rules[0].shortDescription.text, rule2.docs.description);
            assert.strictEqual(log.runs[0].tool.driver.rules[0].helpUri, rule2.docs.url);
            assert.strictEqual(log.runs[0].tool.driver.rules[0].properties.category, rule2.docs.category);

            assert.strictEqual(log.runs[0].tool.driver.rules[1].id, ruleid3);
            assert.strictEqual(log.runs[0].tool.driver.rules[1].shortDescription.text, rule3.docs.description);
            assert.strictEqual(log.runs[0].tool.driver.rules[1].helpUri, rule3.docs.url);
            assert.strictEqual(log.runs[0].tool.driver.rules[1].properties.category, rule3.docs.category);

            assert.strictEqual(log.runs[0].results[0].ruleId, ruleid2);
            assert.strictEqual(log.runs[0].results[1].ruleId, ruleid3);

            assert.strictEqual(log.runs[0].results[0].level, "error");
            assert.strictEqual(log.runs[0].results[1].level, "warning");

            assert.strictEqual(log.runs[0].results[0].message.text, "Unexpected something.");
            assert.strictEqual(log.runs[0].results[1].message.text, "Custom error.");

            assert(log.runs[0].results[0].locations[0].physicalLocation.artifactLocation.uri.startsWith(uriPrefix));
            assert(log.runs[0].results[0].locations[0].physicalLocation.artifactLocation.uri.endsWith(sourceFilePath2));
            assert(log.runs[0].results[1].locations[0].physicalLocation.artifactLocation.uri.startsWith(uriPrefix));
            assert(log.runs[0].results[1].locations[0].physicalLocation.artifactLocation.uri.endsWith(sourceFilePath2));

            assert.strictEqual(log.runs[0].results[0].locations[0].physicalLocation.region.startLine, 18);
            assert.isUndefined(log.runs[0].results[0].locations[0].physicalLocation.region.startColumn);
            assert.isUndefined(log.runs[0].results[0].locations[0].physicalLocation.region.snippet);
        });
    });
});

describe("formatter:sarif", () => {
    describe("when passed a result with no rule id", () => {
        const code = [{
            filePath: sourceFilePath1,
            messages: [{
                message: "Internal error.",
                severity: 2,
                // no ruleId property
            }]
        }];

        it("should return a log with no rules, no results, and a tool execution notification", () => {
            const log = JSON.parse(formatter(code, { rulesMeta: rules }));

            assert.lengthOf(log.runs, 1)
            let run = log.runs[0]
            assert.lengthOf(run.tool.driver.rules, 0);
            assert.lengthOf(run.results, 0);

            assert.lengthOf(run.artifacts, 1);
            let artifactUri = run.artifacts[0].location.uri
            assert(artifactUri.startsWith(uriPrefix));
            assert(artifactUri.endsWith(sourceFilePath1));

            assert.lengthOf(run.invocations, 1)
            let invocation = run.invocations[0]
            assert.isFalse(invocation.executionSuccessful)

            assert.lengthOf(invocation.toolConfigurationNotifications, 1)
            let notification = invocation.toolConfigurationNotifications[0]
            assert.strictEqual(notification.descriptor.id, "ESL0999");
            assert.strictEqual(notification.level, "error");
            assert.strictEqual(notification.message.text, "Internal error.");

            assert.lengthOf(notification.locations, 1)
            let notificationUri = notification.locations[0].physicalLocation.artifactLocation.uri
            assert(notificationUri.startsWith(uriPrefix));
            assert(notificationUri.endsWith(sourceFilePath1));
        });
    });
});
