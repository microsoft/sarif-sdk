// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.Sarif.Visitors;
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

        public static SarifLog UpdateToCurrentVersion(
            string prereleaseSarifLog, 
            bool forceUpdate, 
            Formatting formatting, 
            out string updatedLog)
        {
            bool modifiedLog = false;
            updatedLog = null;

            if (prereleaseSarifLog == null) { return null; }

            JObject sarifLog = JObject.Parse(prereleaseSarifLog);

            // Some tests update the semantic version to current for non-updated content. For this situation, we 
            // allow the test code to force a transform, despite the fact that the provided version doesn't call for it.
            string version = forceUpdate ? "2.0.0" : (string)sarifLog["version"];

            Dictionary<string, int> fullyQualifiedLogicalNameToIndexMap = null;

            switch (version)
            {
                case "2.0.0-csd.2.beta.2018-11-28":
                {
                    // SARIF TC28. Nothing to do.
                    break;
                }

                case "2.0.0-csd.2.beta.2018-10-10":
                {
                    // 2.0.0-csd.2.beta.2018-10-10 == changes through SARIF TC #25
                    modifiedLog |= ApplyChangesFromTC25ThroughTC28(sarifLog, out fullyQualifiedLogicalNameToIndexMap);
                    break;
                }

                default:
                {
                    modifiedLog |= ApplyCoreTransformations(sarifLog);
                    modifiedLog |= ApplyChangesFromTC25ThroughTC28(sarifLog, out fullyQualifiedLogicalNameToIndexMap);
                    break;
                }
            }

            SarifLog transformedSarifLog = null;
            var settings = new JsonSerializerSettings { Formatting = formatting };

            if (fullyQualifiedLogicalNameToIndexMap != null)
            {
                Debug.Assert(modifiedLog == true);
                transformedSarifLog = JsonConvert.DeserializeObject<SarifLog>(sarifLog.ToString());
                var indexUpdatingVisitor = new UpdateIndicesVisitor(fullyQualifiedLogicalNameToIndexMap);
                indexUpdatingVisitor.Visit(transformedSarifLog);
                updatedLog = JsonConvert.SerializeObject(transformedSarifLog, settings);
            }
            else
            {
                updatedLog = modifiedLog ? sarifLog.ToString(formatting) : prereleaseSarifLog;
                transformedSarifLog = JsonConvert.DeserializeObject<SarifLog>(updatedLog);
            }

            return transformedSarifLog;
        }

        private static bool ApplyChangesFromTC25ThroughTC28(JObject sarifLog, out Dictionary<string, int> fullyQualifiedLogicalNameToIndexMap)
        {
            fullyQualifiedLogicalNameToIndexMap = null;

            // Note: we could have injected the TC26 - TC28 changes into the other helpers in this
            // code. This would prevent multiple passes over things like the run.results array.
            // We've isolated the changes here instead simply to keep them grouped together.

            bool modifiedLog = UpdateSarifLogVersion(sarifLog); 

            // For completness, this update added run.newlineSequences to the schema
            // This is a non-breaking (additive) change, so there is no work to do.
            //https://github.com/oasis-tcs/sarif-spec/issues/169

            var runs = (JArray)sarifLog["runs"];

            if (runs != null)
            {
                foreach (JObject run in runs)
                {
                    // Delete run.architecture. This data could, arguably, be transferred into the run logical
                    // identifier or we could drop it into a property bag, but realistically, we don't expect
                    // sufficient existing utilization of this property to warrant preserving it.

                    // Remove run.architecture: https://github.com/oasis-tcs/sarif-spec/issues/262
                    JToken architecture = run[nameof(architecture)];
                    if (architecture != null)
                    {
                        run.Remove(nameof(architecture));
                        modifiedLog = true;
                    }

                    // Logical locations are now an array. We will first transform the previous
                    // dictionary into the arrays form. We will retain the index for each
                    // transformed logical location. Later, we will associate these indices
                    // with results, if applicable.
                    var logicalLocations = run["logicalLocations"] as JObject;
                    Dictionary<LogicalLocation, int> logicalLocationToIndexMap = null;

                    if (logicalLocations != null)
                    {
                        logicalLocationToIndexMap = new Dictionary<LogicalLocation, int>(LogicalLocation.ValueComparer);
                        var jObjectToIndexMap = new Dictionary<JObject, int>();

                        run["logicalLocations"] =
                            ConstructLogicalLocationsArray(
                                logicalLocations,
                                jObjectToIndexMap,
                                logicalLocationToIndexMap,
                                out fullyQualifiedLogicalNameToIndexMap);
                    }

                    var results = (JArray)run["results"];
                    if (results != null)
                    {
                        JArray rewrittenResults = null;

                        foreach (JObject result in results)
                        {
                            // result.message SHALL be present constraint should be added to schema
                            // https://github.com/oasis-tcs/sarif-spec/issues/262
                            JObject message = (JObject)result["message"];

                            if (message == null)
                            {
                                message = new JObject(new JProperty("text", "[No message provided]."));
                                result["message"] = message;
                                modifiedLog = true;
                            }

                            // If this value is non-null, we have converted to a logical locations array
                            // We must therefore update every SARIF location to persist its 
                            // logicalLocationIndex, if relevant.
                            if (logicalLocations != null)
                            {
                                var logicalLocationIndexRemapper = new LogicalLocationIndexRemapper(null, logicalLocationToIndexMap);
                                var sarifResult = JsonConvert.DeserializeObject<Result>(result.ToString());
                                logicalLocationIndexRemapper.Visit(sarifResult);
                                rewrittenResults = rewrittenResults ?? new JArray();
                                rewrittenResults.Add(JToken.Parse(JsonConvert.SerializeObject(sarifResult)));
                            }
                        }

                        if (rewrittenResults != null)
                        {
                            run["results"] = rewrittenResults;
                            modifiedLog = true;
                        }
                    }

                    // Rename fileVersion to dottedQuadFileVersion and specify format constraint
                    // https://github.com/oasis-tcs/sarif-spec/issues/274
                    //
                    // Applies to run.tool.fileVersion and run.conversion.tool.fileVersion

                    modifiedLog |= RenameProperty((JObject)run["tool"], previousName: "fileVersion", newName: "dottedQuadFileVersion");

                    JObject conversion = (JObject)run["conversion"];
                    if (conversion != null)
                    {
                        modifiedLog |= RenameProperty((JObject)conversion["tool"], previousName: "fileVersion", newName: "dottedQuadFileVersion");
                    }

                    // Remove 'open' from rule configuration default level enumeration
                    // https://github.com/oasis-tcs/sarif-spec/issues/288
                    JObject resources = (JObject)run["resources"];
                    modifiedLog |= RemapRuleDefaultLevelFromOpenToNote(resources);

                    // Specify columnKind as ColumndKind.Utf16CodeUnits in cases where the enum is missing from
                    // the SARIF file. Moving forward, the absence of this enum will be interpreted as
                    // the new default, which is ColumnKind.UnicodeCodePoints.
                    // https://github.com/oasis-tcs/sarif-spec/issues/188
                    JValue columnKind = (JValue)run["columnKind"];
                    if (columnKind == null)
                    {
                        run["columnKind"] = "utf16CodeUnits";
                    }
                }
            }

            return modifiedLog;
        }

        private static JArray ConstructLogicalLocationsArray(
            JObject logicalLocations,
            Dictionary<JObject, int> jObjectToIndexMap,
            Dictionary<LogicalLocation, int> logicalLocationToIndexMap,
            out Dictionary<string, int> fullyQualifiedLogicalNameToIndexMap)
        {
            fullyQualifiedLogicalNameToIndexMap = null;

            if (logicalLocations == null) { return null; }

            logicalLocationToIndexMap = new Dictionary<LogicalLocation, int>(LogicalLocation.ValueComparer);

            fullyQualifiedLogicalNameToIndexMap = new Dictionary<string, int>();

            foreach (JProperty logicalLocationEntry in logicalLocations.Properties())
            {
                // This condition may occur if we've already processed a parent logical
                // location that happened to be enumerated after one of its children
                if (fullyQualifiedLogicalNameToIndexMap.ContainsKey(logicalLocationEntry.Name)) { continue; }

                AddEntryToFullyQualifiedNameToIndexMap(
                    logicalLocations,
                    logicalLocationEntry.Name, 
                    (JObject)logicalLocationEntry.Value,
                    logicalLocationToIndexMap, 
                    jObjectToIndexMap,
                    fullyQualifiedLogicalNameToIndexMap);
            }

            var updatedArrayElements = new JObject[jObjectToIndexMap.Count];

            foreach (KeyValuePair<JObject, int> keyValuePair in jObjectToIndexMap)
            {
                int index = keyValuePair.Value;
                JObject updatedLogicalLocation = keyValuePair.Key;
                updatedArrayElements[index] = updatedLogicalLocation;
            }

            return new JArray(updatedArrayElements);
        }

        private static void AddEntryToFullyQualifiedNameToIndexMap(
            JObject logicalLocationsDictionary,
            string keyName,
            JObject logicalLocation,
            Dictionary<LogicalLocation, int> logicalLocationToIndexMap,
            Dictionary<JObject, int> jObjectToIndexMap,
            Dictionary<string, int> keyToIndexMap)
        {
            if (keyToIndexMap == null)
            {
                throw new ArgumentNullException(nameof(keyToIndexMap));
            }

            if (logicalLocation == null)
            {
                throw new ArgumentNullException(nameof(logicalLocation));
            }

            string fullyQualifiedName = keyName;

            // Parent key is no longer relevant and will be removed.
            // We require 
            string parentKey = (string)logicalLocation["parentKey"];

            if (parentKey == null)
            {
                // No parent key? We are a root location. 
                logicalLocation["parentIndex"] = -1;
            }
            else
            {
                // Have we processed our parent yet? If so, retrieve the parentIndex and we're done.
                if (!keyToIndexMap.TryGetValue(parentKey, out int parentIndex))
                {
                    // The parent hasn't been processed yet. We must force its creation
                    // determine its index in our array. This code path results in 
                    // an array order that does not precisely match the enumeration
                    // order of the logical locations dictionary.
                    AddEntryToFullyQualifiedNameToIndexMap(
                        logicalLocationsDictionary,
                        parentKey,
                        (JObject)logicalLocationsDictionary[parentKey],
                        logicalLocationToIndexMap,
                        jObjectToIndexMap,
                        keyToIndexMap);

                    parentIndex = keyToIndexMap[parentKey];
                }
                logicalLocation.Remove("parentKey");
                logicalLocation["parentIndex"] = parentIndex;
            }

            string existingFullyQualifiedName = (string)logicalLocation["fullyQualifiedName"];
            if (string.IsNullOrEmpty(existingFullyQualifiedName))
            {
                // We use the key as the fullyQualifiedName but only in cases when we don't 
                // find this property is already set. If the property is set, that may
                // indicate the key name isn't the FQN but another string value that is
                // used to resolve a collision between distinct logical locations with 
                // an identical fully-qualified name.
                logicalLocation["fullyQualifiedName"] = fullyQualifiedName;
            }

            LogicalLocation sarifLogicalLocation = JsonConvert.DeserializeObject<LogicalLocation>(logicalLocation.ToString());

            // Theoretically, the dictionary should consists of a unique set of logical 
            // locations. In practice, however, there's nothing preventing a SARIF user
            // from emitting duplicated locations. As a result, we will not naively produce
            // a one-to-one mapping of logical locations to dictionary entry. Instead, we 
            // will collapse any duplicates that we find into a single array instance
            if (!logicalLocationToIndexMap.TryGetValue(sarifLogicalLocation, out int index))
            {
                index = logicalLocationToIndexMap.Count;
                logicalLocationToIndexMap[sarifLogicalLocation] = index;
                jObjectToIndexMap[logicalLocation] = index;
                keyToIndexMap[fullyQualifiedName] = index;
            }
        }

        private static bool RemapRuleDefaultLevelFromOpenToNote(JObject resources)
        {
            bool modifiedResources = false;

            if (resources == null) { return modifiedResources; }

            JObject rules = (JObject)resources["rules"];
            if (rules == null ) { return modifiedResources; }

            foreach (JProperty rule in rules.Values<JProperty>())
            {
                JObject configuration = (JObject)rule.Value["configuration"];
                if (configuration == null) { continue; }

                if ("open".Equals((string)configuration["defaultLevel"]))
                {
                    // We remap 'open' to 'note'. 'open' is an indicator that analysis is unresolved, i.e., 
                    // the question of whether a weakness exists is not yet determined. 'note' is the most
                    // reasonable level to associate with this class of report, if it is emitted. In 
                    // practice, we don't expect that a current producer exists who is in this condition.
                    configuration["defaultLevel"] = "note";
                }
            }

            return modifiedResources;
        }

        private static bool ApplyCoreTransformations(JObject sarifLog)
        {
            bool modifiedLog = UpdateSarifLogVersion(sarifLog); 

            var runs = (JArray)sarifLog["runs"];

            if (runs != null)
            {
                foreach (JObject run in runs)
                {
                    // Move result.ruleMessageId: https://github.com/oasis-tcs/sarif-spec/issues/216
                    // Update name of threadflowLocation.executionTimeUtc: https://github.com/oasis-tcs/sarif-spec/issues/242
                    modifiedLog |= UpdateRunResults(run);

                    // invocation.workingDirectory now a file location: https://github.com/oasis-tcs/sarif-spec/issues/222
                    // Update names of invocation.startTimeUtc and endTimeUtc: https://github.com/oasis-tcs/sarif-spec/issues/242
                    // Convert exception.message to message object: https://github.com/oasis-tcs/sarif-spec/issues/240
                    modifiedLog |= UpdateRunInvocations(run);

                    // run.originalUriBaseIds values now file locations: https://github.com/oasis-tcs/sarif-spec/issues/234
                    modifiedLog |= UpdateRunOriginalUriBaseIds(run);

                    // Convert file.hashes to dictionary: https://github.com/oasis-tcs/sarif-spec/issues/240
                    // Update name of file.lastModifiedTimeUtc: https://github.com/oasis-tcs/sarif-spec/issues/242
                    modifiedLog |= UpdateRunFiles(run);

                    // Update names of notification.startTimeUtc and endTimeUtc: https://github.com/oasis-tcs/sarif-spec/issues/242
                    modifiedLog |= UpdateRunNotifications(run);

                    // Update name to versionControlDetails.repositoryUri: https://github.com/oasis-tcs/sarif-spec/issues/244
                    modifiedLog |= UpdateRunVersionControlProvenance(run);

                    modifiedLog |= RefactorRunAutomationDetails(run);
                }
            }

            return modifiedLog;
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

        private static bool RefactorRunAutomationDetails(JObject run)
        {
            bool modifiedRun = false;

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

                if (instanceGuid != null && logicalId != null)
                {
                    // We can only effectively populate the new instanceId in a case where
                    // the log is previously uniquely identified a run by a guid.
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

                // For the aggregating automation id, we can only provide the logical component
                aggregateId["instanceId"] = automationLogicalId + "/";

                run["aggregateIds"] = new JArray(aggregateId);
                modifiedRun = true;
            }

            return modifiedRun;
        }

        private static bool UpdateRunVersionControlProvenance(JObject run)
        {
            bool modifiedRun = false;

            JArray versionControlDetailsArray = (JArray)run["versionControlProvenance"];
            if (versionControlDetailsArray != null)
            {
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

        internal static bool UpdateRunResults(JObject run)
        {
            bool modifiedRun = false;

            JArray results = (JArray)run["results"];
            if (results == null) { return modifiedRun; }

            foreach (JObject result in results)
            {
                // https://github.com/oasis-tcs/sarif-spec/issues/216
                //
                // result.ruleMessageId is removed. This property value
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
                        if (step != null)
                        {
                            threadFlowLocation.Remove("step");
                            modifiedThreadFlowLocation = true;
                        }
                    }
                }
            }
            return modifiedThreadFlowLocation;
        }

        private static bool UpdateRunInvocations(JObject run)
        {
            bool modifiedRun = false;

            JArray invocations = (JArray)run["invocations"];

            if (invocations != null)
            {
                foreach (JObject invocation in invocations)
                {
                    modifiedRun |= UpdateInvocation(invocation);
                }
            }

            return modifiedRun;
        }

        internal static bool UpdateInvocation(JObject invocation)
        {
            bool modifiedInvocation = false;

            // https://github.com/oasis-tcs/sarif-spec/issues/222
            //
            // Previously:
            //     "workingDirectory": "/home/buildAgent/src",
            // Now:
            //     "workingDirectory": { "uri" : "/home/buildAgent/src" },

            modifiedInvocation |= ReplaceUriStringWithFileLocation(invocation, "workingDirectory");

            // https://github.com/oasis-tcs/sarif-spec/issues/242

            modifiedInvocation |= RenameProperty(invocation, previousName: "startTime", newName: "startTimeUtc");
            modifiedInvocation |= RenameProperty(invocation, previousName: "endTime", newName: "endTimeUtc");

            return modifiedInvocation;
        }

        private static bool ReplaceUriStringWithFileLocation(JObject jObject, string propertyName)
        {
            bool modifiedObject = false;

            if (jObject[propertyName]?.Type == JTokenType.String)
            {
                string uriValue = (string)jObject[propertyName];

                jObject.Property(propertyName).Remove();

                var fileLocation = new JProperty("uri", uriValue);
                var fileLocationObject = new JObject(fileLocation);
                jObject[propertyName] = fileLocationObject;

                modifiedObject = true;
            }

            return modifiedObject;
        }

        private static bool UpdateRunOriginalUriBaseIds(JObject run)
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

            var originalUriBaseIds = (JObject)run["originalUriBaseIds"];

            if (originalUriBaseIds == null) { return modifiedRun; }

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

            return modifiedRun;
        }

        internal static bool UpdateRunFiles(JObject run)
        {
            bool modifiedRun = false;

            var files = (JObject)run["files"];
            if (files == null) { return modifiedRun; }

            foreach (JProperty file in files.Properties())
            {
                var fileObject = (JObject)file.Value;
                modifiedRun |= UpdateFileHashesProperty(fileObject);

                // https://github.com/oasis-tcs/sarif-spec/issues/242
                modifiedRun |= RenameProperty(fileObject, "lastModifiedTime", "lastModifiedTimeUtc"); ;
            }

            return modifiedRun;
        }

        private static bool RenameProperty(JObject jObject, string previousName, string newName)
        {
            if (jObject == null) { return false; }

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

        internal static bool UpdateRunNotifications(JObject run)
        {
            bool modifiedRun = false;

            var invocations = (JArray)run["invocations"];
            if (invocations == null) { return modifiedRun; }

            foreach (JObject invocation in invocations)
            {
                var notifications = (JArray)invocation["configurationNotifications"];
                if (notifications != null) { modifiedRun |= UpdateNotifications(notifications); }

                notifications = (JArray)invocation["toolNotifications"];
                if (notifications != null) { modifiedRun |= UpdateNotifications(notifications); }
            }

            return modifiedRun;
        }


        internal static bool UpdateNotifications(JArray notifications)
        {
            bool modifiedNotification = false;

            foreach (JObject notification in notifications)
            {
                modifiedNotification |= UpdateNotification(notification);
            }

            return modifiedNotification;
        }

        private static bool UpdateNotification(JObject notification)
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
    }
}
