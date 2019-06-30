/**
 * @fileoverview SARIF v2.1 formatter
 * @author Microsoft
 */

"use strict";

const lodash = require("lodash");
const fs = require("fs");
const utf8 = require("utf8");
const jschardet = require("jschardet");

//------------------------------------------------------------------------------
// Helper Functions
//------------------------------------------------------------------------------

/**
 * Returns the severity of warning or error
 * @param {Object} message message object to examine
 * @returns {string} severity level
 * @private
 */
function getResultLevel(message) {
    if (message.fatal || message.severity === 2) {
        return "error";
    }
    return "warning";
}

//------------------------------------------------------------------------------
// Public Interface
//------------------------------------------------------------------------------

module.exports = function (results, data) {

    const rulesMeta = lodash.get(data, "rulesMeta", null);

    const sarifLog = {
        version: "2.1.0",
        $schema: "http://json.schemastore.org/sarif-2.1.0-rtm.1",
        runs: [
            {
                tool: {
                    driver: {
                        name: "ESLint",
                        downloadUri: "https://eslint.org",
                        rules: []
                    }
                }
            }
        ]
    };

    const sarifFiles = {};
    const sarifRules = {};
    const sarifResults = [];
    const embedFileContents = process.env.SARIF_ESLINT_EMBED === "true";

    results.forEach(result => {

        // Only add it if not already there.
        if (typeof sarifFiles[result.filePath] === "undefined") {

            let contentsUtf8;

            // Create a new entry in the files dictionary.
            sarifFiles[result.filePath] = {
                artifactLocation: {
                    uri: result.filePath
                }
            };

            if (embedFileContents) {
                try {

                    // Try to get the file contents and encoding.
                    const contents = fs.readFileSync(result.filePath);
                    const encoding = jschardet.detect(contents);

                    // Encoding will be null if it could not be determined.
                    if (encoding) {

                        // Convert the content bytes to a UTF-8 string.
                        contentsUtf8 = utf8.encode(contents.toString(encoding.encoding));

                        sarifFiles[result.filePath].contents = {
                            text: contentsUtf8
                        };
                        sarifFiles[result.filePath].encoding = encoding.encoding;
                    }
                }
                catch (err) {
                    console.log(err);
                }
            }
            if (result.messages.length > 0) {

                result.messages.forEach(message => {

                    const sarifResult = {
                        level: getResultLevel(message),
                        message: {
                            text: message.message
                        },
                        locations: [
                            {
                                physicalLocation: {
                                    artifactLocation: {
                                        uri: result.filePath
                                    }
                                }
                            }
                        ]
                    };

                    if (message.ruleId) {
                        sarifResult.ruleId = message.ruleId;

                        if (rulesMeta && typeof sarifRules[message.ruleId] === "undefined") {
                            const meta = rulesMeta[message.ruleId];

                            // An unknown ruleId will return null. This check prevents unit test failure.
                            if (meta) {

                                // Create a new entry in the rules dictionary.
                                sarifRules[message.ruleId] = {
                                    id: message.ruleId,
                                    shortDescription: {
                                        text: meta.docs.description
                                    },
                                    helpUri: meta.docs.url,
                                    tags: {
                                        category: meta.docs.category
                                    }
                                };
                            }
                        }
                    }

                    if (message.line > 0 || message.column > 0) {
                        sarifResult.locations[0].physicalLocation.region = {
                            startLine: message.line,
                            startColumn: message.column
                        };
                    }

                    if (message.source) {

                        // Create an empty region if we don't already have one from the line / column block above.
                        sarifResult.locations[0].physicalLocation.region = sarifResult.locations[0].physicalLocation.region || {};
                        sarifResult.locations[0].physicalLocation.region.snippet = {
                            text: message.source
                        };
                    }
                    sarifResults.push(sarifResult);
                });
            }
        }
    });

    if (Object.keys(sarifFiles).length > 0) {
        sarifLog.runs[0].artifacts = [];

        Object.keys(sarifFiles).forEach(function (path) {
            sarifLog.runs[0].artifacts.push(sarifFiles[path]);
        });
    }

    if (sarifResults.length > 0) {
        sarifLog.runs[0].results = sarifResults;
    }

    if (Object.keys(sarifRules).length > 0) {

        let ruleIndex = 0;
        Object.keys(sarifRules).forEach(function (ruleId) {
            let rule = sarifRules[ruleId];
            rule.ruleIndex = ruleIndex++;
            sarifLog.runs[0].tool.driver.rules.push(rule);
        });
    }

    return JSON.stringify(sarifLog,
        null, // replacer function
        2     // # of spaces for indents
    );
};
