/**
 * @fileoverview SARIF v2.0 formatter
 * @author Chris Meyer
 */

"use strict";

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

module.exports = function(results) {
    const sarifLog = {
        version: "2.0.0",
        $schema: "http://json.schemastore.org/sarif-2.0.0",
        runs: [
            {
                tool: {
                    name: "ESLint"
                }
            }
        ]
    };

    const sarifFiles = {};
    const sarifResults = [];

    results.forEach(result => {

        if (typeof sarifFiles[result.filePath] === "undefined") {

            // Create a new entry in the files dictionary.
            sarifFiles[result.filePath] = {
                fileLocation: {
                    uri: result.filePath
                }
            };
        }

        const messages = result.messages;

        messages.forEach(message => {

            const sarifResult = {
                level: getResultLevel(message),
                message: {
                    text: message.message
                },
                locations: [
                    {
                        physicalLocation: {
                            fileLocation: {
                                uri: result.filePath
                            }
                        }
                    }
                ]
            };

            if (message.ruleId) {
                sarifResult.ruleId = message.ruleId;
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

    });

    //------------------------------------------------------------------------------
    // We use separate arrays and dictionaries to avoid creating empty properties in the SARIF log.
    // Here we set those properties accordingly.
    //------------------------------------------------------------------------------

    if (Object.keys(sarifFiles).length > 0) {
        sarifLog.runs[0].files = sarifFiles;
    }

    if (sarifResults.length > 0) {
        sarifLog.runs[0].results = sarifResults;
    }

    return JSON.stringify(sarifLog, null, 2);
};
