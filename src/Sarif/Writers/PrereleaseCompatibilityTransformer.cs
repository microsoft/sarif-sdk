// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Writers
{
    public static class PrereleaseCompatibilityTransformer
    {
        // As the SARIF Technical Committee develops the SARIF specification, it
        // releases incremental versions of the schema, with SemVer versions such as
        // "2.0.0-csd.2.beta.2018-10-10". To avoid having to update the version strings
        // in the test files every time this happens, we replace the version
        // in the test file with the current version after reading the file
        // into memory.
        private const string VersionPropertyPattern = @"""version""\s*:\s*""[^""]+""";
        private static readonly Regex s_VersionRegex = new Regex(VersionPropertyPattern, RegexOptions.Compiled);

        private const string SchemaPropertyPattern = @"""\$schema""\s*:\s*""[^""]+""";
        private static readonly Regex s_SchemaRegex = new Regex(SchemaPropertyPattern, RegexOptions.Compiled);

        public static string UpdateToCurrentVersion(string prereleaseSarifLog, bool forceUpdate = false, Formatting formatting = Formatting.None)
        {
            bool modifiedLog = false;
            JObject sarifLog = JObject.Parse(prereleaseSarifLog);

            // Some tests update the semantic version to current for non-updated content. For this situation, we 
            // allow the test code to force a transform, despite the fact that the provided version doesn't call for it.
            string version = forceUpdate ? "2.0.0" : (string)sarifLog["version"];

            switch (version)
            {
                // This case actually handles all SARIF v2 prerelease before we 
                // started properly updating the schema. 
                case "2.0.0":
                {
                    modifiedLog |= ApplyCoreTransformations(sarifLog);
                    break;
                }

                case "2.0.0-csd.2.beta.2018-10-10":
                {
                    modifiedLog |= ApplyCoreTransformations(sarifLog);
                    modifiedLog |= ApplyChangesSinceTechnicalCommitteeMeeting25();
                    break;
                }

                default:
                    break;
            }

            string text = modifiedLog ? sarifLog.ToString(formatting) : prereleaseSarifLog;

            return text;
        }
        private static bool ApplyCoreTransformations(JObject sarifLog)
        {
            bool modifiedLog = UpdateSarifLogVersion(sarifLog); 

            var runs = (JArray)sarifLog["runs"];

            if (runs != null)
            {
                // Move result.ruleMessageId: https://github.com/oasis-tcs/sarif-spec/issues/216
                // Update name of threadflowLocation.executionTimeUtc: https://github.com/oasis-tcs/sarif-spec/issues/242
                modifiedLog |= UpdateRunResults(runs);

                // invocation.workingDirectory now a file location: https://github.com/oasis-tcs/sarif-spec/issues/222
                // Update names of invocation.startTimeUtc and endTimeUtc: https://github.com/oasis-tcs/sarif-spec/issues/242
                // Convert exception.message to message object: https://github.com/oasis-tcs/sarif-spec/issues/240
                modifiedLog |= UpdateRunInvocations(runs);

                // run.originalUriBaseIds values now file locations: https://github.com/oasis-tcs/sarif-spec/issues/234
                modifiedLog |= UpdateRunOriginalUriBaseIds(runs);

                // Convert file.hashes to dictionary: https://github.com/oasis-tcs/sarif-spec/issues/240
                // Update name of file.lastModifiedTimeUtc: https://github.com/oasis-tcs/sarif-spec/issues/242
                modifiedLog |= UpdateRunFiles(runs);

                // Update names of notification.startTimeUtc and endTimeUtc: https://github.com/oasis-tcs/sarif-spec/issues/242
                modifiedLog |= UpdateRunNotifications(runs);

                // Update name to versionControlDetails.repositoryUri: https://github.com/oasis-tcs/sarif-spec/issues/244
                modifiedLog |= UpdateVersionControlProvenance(runs);

                modifiedLog |= UpdateRunAutomationDetails(runs);
            }

            return true;
        }

        private static bool UpdateSarifLogVersion(JObject sarifLog)
        {
            bool modifiedLog = false;

            string version = (string)sarifLog["version"];
            if (version != SarifUtilities.SemanticVersion)
            {
                sarifLog["version"] = SarifUtilities.SemanticVersion;
                modifiedLog = true;
            }

            string schema = (string)sarifLog["$schema"];
            if (schema != SarifUtilities.SarifSchemaUri)
            {
                sarifLog["$schema"] = SarifUtilities.SarifSchemaUri;
                modifiedLog = true;
            }

            return modifiedLog;
        }

        private static bool UpdateRunAutomationDetails(JArray runs)
        {
            bool modifiedRun = false;

            foreach (JObject run in runs)
            {
                // The properties comprise the run-level runAutomationId in previous versions
                JToken logicalId = run["logicalId"];
                JToken description = run["description"]; // 'message' object type
                JToken instanceGuid = run["instanceGuid"];
                JToken correlationGuid = run["correlationGuid"];

                if (logicalId != null || description != null ||
                    instanceGuid != null || correlationGuid != null)
                {
                    run.Remove("logicalId");
                    run.Remove("description");
                    run.Remove("instanceGuid");
                    run.Remove("correlationGuid");

                    var runId = new JObject();

                    if (instanceGuid != null)
                    {
                        // We can only effectively populate the new instanceId in a case where
                        // the log if previously uniquely identified a run by a guid.
                        runId["instanceId"] = logicalId + "/" + instanceGuid;
                    }

                    if (description != null) { runId["description"] = description; }
                    if (instanceGuid != null) { runId["instanceGuid"] = instanceGuid; }
                    if (correlationGuid != null) { runId["correlationGuid"] = correlationGuid; }

                    run["id"] = runId;

                    modifiedRun = true;
                }

                // This property is an element of what v2 now refers to as aggregateIds
                JToken automationLogicalId = run["automationLogicalId"];

                if (automationLogicalId != null)
                {
                    run.Remove("automationLogicalId");

                    var aggregateId = new JObject();

                    // For the aggregating automat id, we have to provide only the
                    aggregateId["instanceId"] = automationLogicalId + "/";

                    var aggregateIds = new JArray(aggregateId);

                    run["aggregateIds"] = aggregateIds;

                    modifiedRun = true;
                }
            }

            return modifiedRun;
        }

        private static bool UpdateVersionControlProvenance(JArray runs)
        {
            bool modifiedRun = false;

            foreach (JObject run in runs)
            {
                JArray versionControlDetailsArray = (JArray)run["versionControlProvenance"];
                if (versionControlDetailsArray == null) { continue; }

                foreach (JObject versionControlDetails in versionControlDetailsArray)
                {
                    modifiedRun |= UpdateVersionControlDetails(versionControlDetails);
                }
            }
            return modifiedRun;
        }

        private static bool UpdateVersionControlDetails(JObject versionControlDetails)
        {
            bool modifiedVersionControlDetails = false;

            // https://github.com/oasis-tcs/sarif-spec/issues/242
            modifiedVersionControlDetails |= RenameProperty(versionControlDetails, previousName: "timestamp", newName: "asOfTimeUtc");

            // https://github.com/oasis-tcs/sarif-spec/issues/244
            modifiedVersionControlDetails |= RenameProperty(versionControlDetails, previousName: "uri", newName: "repositoryUri");

            // https://github.com/oasis-tcs/sarif-spec/issues/249
            modifiedVersionControlDetails |= RenameProperty(versionControlDetails, previousName: "tag", newName: "revisionTag");

            return modifiedVersionControlDetails;
        }

        internal static bool UpdateRunResults(JArray runs)
        {
            bool modifiedRun = false;

            foreach (JObject run in runs)
            {
                JArray results = (JArray)run["results"];

                if (results == null) { continue; }

                foreach (JObject result in results)
                {
                    // https://github.com/oasis-tcs/sarif-spec/issues/216
                    //
                    // result.ruleMessageId is deprecated. This property value
                    // should be expressed as result.message.messageId instead.
                    // e.g.:
                    // "message": { "messageId" : "default"}

                    string ruleMessageId = (string)result["ruleMessageId"];

                    if (ruleMessageId != null)
                    {
                        result.Property("ruleMessageId").Remove();
                        result["message"]["messageId"] = ruleMessageId;
                        modifiedRun = true;
                    }

                    var codeFlows = (JArray)result["codeFlows"];
                    if (codeFlows != null)
                    {
                        modifiedRun |= UpdateCodeFlows(codeFlows);
                    }
                }
            }
            return modifiedRun;
        }

        private static bool UpdateCodeFlows(JArray codeFlows)
        {
            bool modifiedThreadFlowLocation = false;

            foreach (JObject codeFlow in codeFlows)
            {
                JArray threadFlows = (JArray)codeFlow["threadFlows"];
                if (threadFlows == null) { continue; }

                foreach (JObject threadFlow in threadFlows)
                {
                    JArray threadFlowLocations = (JArray)threadFlow["locations"];
                    if (threadFlowLocations == null) { continue; }

                    foreach (JObject threadFlowLocation in threadFlowLocations)
                    {
                        // Update name of threadFlowLocation.executionTimeUtc: https://github.com/oasis-tcs/sarif-spec/issues/242
                        modifiedThreadFlowLocation |= RenameProperty(threadFlowLocation, previousName: "timestamp", newName: "executionTimeUtc");

                        // Delete threadFlowLocation.step property: https://github.com/oasis-tcs/sarif-spec/issues/203
                        JToken step = threadFlowLocation["step"];
                        if (step?.Type == JTokenType.Integer)
                        {
                            threadFlowLocation.Remove("step");
                            modifiedThreadFlowLocation = true;
                        }
                    }
                }
            }
            return modifiedThreadFlowLocation;
        }

        private static bool UpdateRunInvocations(JArray runs)
        {
            bool modifiedRun = false;

            foreach (JObject run in runs)
            {
                JArray invocations = (JArray)run["invocations"];

                if (invocations == null) { continue; }

                foreach (JObject invocation in invocations)
                {
                    modifiedRun |= UpdateInvocationObject(invocation);
                }
            }
            return modifiedRun;
        }

        internal static bool UpdateInvocationObject(JObject invocation)
        {
            bool modifiedInvocation = false;

            // https://github.com/oasis-tcs/sarif-spec/issues/222
            //
            // Previously:
            //     "workingDirectory": "/home/buildAgent/src",
            // Now:
            //     "workingDirectory": { "uri" : "/home/buildAgent/src" },

            if (invocation["workingDirectory"]?.Type == JTokenType.String)
            {
                string workingDirectory = (string)invocation["workingDirectory"];

                invocation.Property("workingDirectory").Remove();

                var fileLocation = new JProperty("uri", workingDirectory);
                var fileLocationObject = new JObject(fileLocation);
                invocation["workingDirectory"] = fileLocationObject;

                modifiedInvocation = true;
            }

            // https://github.com/oasis-tcs/sarif-spec/issues/242

            modifiedInvocation |= RenameProperty(invocation, previousName: "startTime", newName: "startTimeUtc");
            modifiedInvocation |= RenameProperty(invocation, previousName: "endTime", newName: "endTimeUtc");

            return modifiedInvocation;
        }

        private static bool UpdateRunOriginalUriBaseIds(JArray runs)
        {
            bool modifiedRun = false;

            // https://github.com/oasis-tcs/sarif-spec/issues/234
            //
            // originalUriBaseId values are now file locations, not 
            // a string value that points to a root URI
            //
            // Previously:
            // originalUriBaseIds : { "$(SrcRoot)" : "file://c:/src/myenlistment." }
            //
            // Now:
            // originalUriBaseIds : { "$(SrcRoot)" : { "uri" : "file://c:/src/myenlistment." } }


            foreach (JObject run in runs)
            {
                var originalUriBaseIds = (JObject)run["originalUriBaseIds"];

                if (originalUriBaseIds == null) { continue; }

                List<JProperty> rewrittenValues = null;

                foreach (JProperty originalUriBaseId in originalUriBaseIds.Properties())
                {
                    string key = originalUriBaseId.Name;
                    JToken value = originalUriBaseId.Value;

                    if (value.Type != JTokenType.String) { continue; }

                    rewrittenValues = rewrittenValues ?? new List<JProperty>();

                    var fileLocation = new JProperty("uri", value);
                    var fileLocationObject = new JObject(fileLocation);
                    var newValue = new JProperty(key, fileLocationObject);

                    rewrittenValues.Add(newValue);
                }

                if (rewrittenValues != null)
                {
                    run.Remove("originalUriBaseIds");

                    var rewrittenOriginalUriBaseIds = new JObject(rewrittenValues.ToArray());
                    run["originalUriBaseIds"] = rewrittenOriginalUriBaseIds;

                    modifiedRun = true;
                }
            }
            return modifiedRun;
        }

        internal static bool UpdateRunFiles(JArray runs)
        {
            bool modifiedRun = false;

            foreach (JObject run in runs)
            {
                var files = (JObject)run["files"];
                if (files == null) { continue; }

                foreach (JProperty file in files.Properties())
                {
                    var fileObject = (JObject)file.Value;
                    modifiedRun |= UpdateFileHashesProperty(fileObject);

                    // https://github.com/oasis-tcs/sarif-spec/issues/242
                    modifiedRun |= RenameProperty(fileObject, "lastModifiedTime", "lastModifiedTimeUtc"); ;
                }
            }

            return modifiedRun;
        }

        private static bool RenameProperty(JObject jObject, string previousName, string newName)
        {
            JToken propertyValue = jObject[previousName];
            
            if (propertyValue != null)
            {
                jObject.Remove(previousName);
                jObject[newName] = propertyValue;
            }

            return propertyValue != null;
        }

        private static bool UpdateFileHashesProperty(JObject file)
        {
            bool modifiedRun = false;

            // https://github.com/oasis-tcs/sarif-spec/issues/243
            //
            // file.hashes were previously an array of hash objects.
            // Now file.hashes is a dictionary where each key is the 
            // hash algorithm and the value is the hash of the file contents.
            //
            // Previously:
            //     "hashes": [ { "algorithm": "sha-256", "value": "b13ce2678a8807ba0765ab94a0ecd394f869bc81" } ]
            //
            // Now:
            // "hashes": { "sha-256": "b13ce2678a8807ba0765ab94a0ecd394f869bc81" }

            JToken hashes = file["hashes"];
            if (hashes == null || hashes.Type != JTokenType.Array) { return modifiedRun; }

            JObject rewrittenHashes = new JObject();

            foreach (JObject hash in hashes)
            {
                string algorithm = (string)hash["algorithm"];
                string value = (string)hash["value"];

                rewrittenHashes[algorithm] = value;
            }
            file.Remove("hashes");
            file["hashes"] = rewrittenHashes;
            modifiedRun = true;

            return modifiedRun;
        }

        internal static bool UpdateRunNotifications(JArray runs)
        {
            bool modifiedRun = false;

            foreach (JObject run in runs)
            {
                var invocations = (JArray)run["invocations"];

                if (invocations == null) { continue; }

                foreach (JObject invocation in invocations)
                {
                    var notifications = (JArray)invocation["configurationNotifications"];
                    if (notifications != null) { modifiedRun |= UpdateNotifications(notifications); }

                    notifications = (JArray)invocation["toolNotifications"];
                    if (notifications != null) { modifiedRun |= UpdateNotifications(notifications); }
                }
            }

            return modifiedRun;                
        }


        internal static bool UpdateNotifications(JArray notifications)
        {
            bool modifiedNotification = false;

            foreach (JObject notification in notifications)
            {
                modifiedNotification |= UpdateNotificationObjects(notification);
            }

            return modifiedNotification;
        }

        private static bool UpdateNotificationObjects(JObject notification)
        {
            bool modifiedNotification = false;

            // https://github.com/oasis-tcs/sarif-spec/issues/242
            modifiedNotification |= RenameProperty(notification, previousName: "time", newName: "timeUtc");

            var exception = (JObject)notification["exception"];

            if (exception != null)
            {
                modifiedNotification |= ConvertExceptionMessageFromStringToMessageObject(exception);

                var innerExceptions = (JArray)exception["innerExceptions"];

                if (innerExceptions != null)
                {
                    foreach (JObject innerException in innerExceptions)
                    {
                        modifiedNotification |= ConvertExceptionMessageFromStringToMessageObject(innerException);
                    }
                }
            }

            return modifiedNotification;
        }

        private static bool ConvertExceptionMessageFromStringToMessageObject(JObject exception)
        {
            bool modifiedNotification = false;

            // https://github.com/oasis-tcs/sarif-spec/issues/240
            //
            // exception.message is converted from a string primitive to a message
            // object instance. This property was previously the final remaining string
            // literal used for user-facing text. We converted it to a message object
            // strictly for consistency, as multiple parties observed the inconsistency
            // and perceived it as a possible bug.
            //
            // Previously:
            //      "exception" : { "message": "Unhandled exception during rule evaluation." }
            //
            // Now:
            //      "exception" : { "message": { "text" : "Unhandled exception during rule evaluation." } }

            JToken message = exception["message"];

            if (message?.Type == JTokenType.String)
            {
                exception.Remove("message");
                var messageProperty = new JProperty("text", message);
                exception["message"] = new JObject(messageProperty);
                modifiedNotification = true;
            }

            return modifiedNotification;
        }

        private static bool ApplyChangesSinceTechnicalCommitteeMeeting25()
        {
            // No changes applied in this method
            return false;
        }
    }
}
