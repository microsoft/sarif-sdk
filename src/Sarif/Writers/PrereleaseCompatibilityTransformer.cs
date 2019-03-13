// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
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
            Formatting formatting, 
            out string updatedLog)
        {
            bool modifiedLog = false;
            updatedLog = null;

            if (string.IsNullOrEmpty(prereleaseSarifLog)) { return null; }

            JObject sarifLog = JObject.Parse(prereleaseSarifLog);

            string version = (string)sarifLog["version"];

            Dictionary<string, int> fullyQualifiedLogicalNameToIndexMap = null;
            Dictionary<string, int> fileLocationKeyToIndexMap = null;
            Dictionary<string, int> ruleKeyToIndexMap = null;

            switch (version)
            {
                case "2.0.0-csd.2.beta.2019-02-20":
                {
                    // SARIF TC32. Nothing to do.
                    break;
                }

                case "2.0.0-csd.2.beta.2019-01-24":
                case "2.0.0-csd.2.beta.2019-01-24.1":
                {
                    modifiedLog |= ApplyChangesFromTC32(sarifLog);
                    modifiedLog |= ApplyChangesFromTC33(sarifLog);
                    break;
                }

                case "2.0.0-csd.2.beta.2019-01-09":
                {
                    modifiedLog |= ApplyChangesFromTC31(sarifLog);
                    modifiedLog |= ApplyChangesFromTC32(sarifLog);
                    modifiedLog |= ApplyChangesFromTC33(sarifLog);
                    break;
                }

                case "2.0.0-csd.2.beta.2018-10-10":
                case "2.0.0-csd.2.beta.2018-10-10.1":
                case "2.0.0-csd.2.beta.2018-10-10.2":
                {
                    // 2.0.0-csd.2.beta.2018-10-10 == changes through SARIF TC #25
                    modifiedLog |= ApplyChangesFromTC25ThroughTC30(
                        sarifLog, 
                        out fullyQualifiedLogicalNameToIndexMap,
                        out fileLocationKeyToIndexMap,
                        out ruleKeyToIndexMap);
                    modifiedLog |= ApplyChangesFromTC31(sarifLog);
                    modifiedLog |= ApplyChangesFromTC32(sarifLog);
                    modifiedLog |= ApplyChangesFromTC33(sarifLog);
                    break;

                }

                default:
                {
                    modifiedLog |= ApplyCoreTransformations(sarifLog);
                    modifiedLog |= ApplyChangesFromTC25ThroughTC30(
                        sarifLog, 
                        out fullyQualifiedLogicalNameToIndexMap,
                        out fileLocationKeyToIndexMap,
                        out ruleKeyToIndexMap);
                    modifiedLog |= ApplyChangesFromTC31(sarifLog);
                    modifiedLog |= ApplyChangesFromTC32(sarifLog);
                    modifiedLog |= ApplyChangesFromTC33(sarifLog);
                    break;
                }
            }

            SarifLog transformedSarifLog = null;
            var settings = new JsonSerializerSettings { Formatting = formatting, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate };

            if (fullyQualifiedLogicalNameToIndexMap != null  || fileLocationKeyToIndexMap != null || ruleKeyToIndexMap != null)
            {
                transformedSarifLog = JsonConvert.DeserializeObject<SarifLog>(sarifLog.ToString());

                var indexUpdatingVisitor = new UpdateIndicesFromLegacyDataVisitor(
                    fullyQualifiedLogicalNameToIndexMap, 
                    fileLocationKeyToIndexMap,
                    ruleKeyToIndexMap);

                indexUpdatingVisitor.Visit(transformedSarifLog);
                updatedLog = JsonConvert.SerializeObject(transformedSarifLog, settings);
            }
            else
            {
                updatedLog = modifiedLog ? sarifLog.ToString(formatting) : prereleaseSarifLog;
                transformedSarifLog = JsonConvert.DeserializeObject<SarifLog>(updatedLog, settings);
                updatedLog = JsonConvert.SerializeObject(transformedSarifLog, formatting);
            }

            return transformedSarifLog;
        }

        private static bool ApplyChangesFromTC33(JObject sarifLog)
        {
            UpdateSarifLogVersion(sarifLog);
            if (sarifLog["runs"] is JArray runs)
            {
                foreach (JObject run in runs)
                {
                    // https://github.com/oasis-tcs/sarif-spec/issues/337
                    ConvertToolToDriverInExternalPropertyFiles(run);
                }
            }
            return true;
        }

        private static void ConvertToolToDriverInExternalPropertyFiles(JObject run)
        {
            if (run["externalPropertyFiles"] is JObject externalPropertyFiles &&
                externalPropertyFiles["tool"] is JObject toolExternalPropertyFile)
            {
                externalPropertyFiles.Remove("tool");
                externalPropertyFiles.Add("driver", toolExternalPropertyFile);
            }
        }

        private static bool ApplyChangesFromTC32(JObject sarifLog)
        {
            UpdateSarifLogVersion(sarifLog);
            if (sarifLog["runs"] is JArray runs)
            {
                foreach (JObject run in runs)
                {
                    // https://github.com/oasis-tcs/sarif-spec/issues/330
                    RemoveToolLanguage(run);
                    UpdateAllReportingDescriptorPropertyTypes(run);
                    ConvertAllExceptionMessagesToStringAndRenameToolNotificationNodes(run);

                    // https://github.com/oasis-tcs/sarif-spec/issues/325
                    UpdateAllExternalPropertyFilePropertyTypes(run);

                    // https://github.com/oasis-tcs/sarif-spec/issues/336
                    UpdateAllToolComponentProperties(run);

                    // https://github.com/oasis-tcs/sarif-spec/issues/302
                    ConvertAllStackFrameAddressesToAddressObjects(run);
                }
            }
            return true;
        }

        private static void ConvertAllStackFrameAddressesToAddressObjects(JObject run)
        {
            // We need to remove StackFrame.Address (int) and StackFrame.Offset (int) and transfer those
            // to a single Address object with properties "BaseAddress" and "Offset".

            // Previously:
            //      "stackFrame" : {
            //          "address" : 324 ,
            //          "offset" : 346
            //      }
            // Now:
            //      "stackFrame" : {
            //          "address" : {
            //              "baseAddress" : 324,
            //              "offset" : 346
            //          }
            //      }

            // The code walks through all possible paths to Stackframe node and performs updates.

            if (run["conversion"] is JObject conversion && conversion["invocation"] is JObject invocation)
            {
                ConvertInvocationStackFrameAddressesToAddressObjects(invocation);
            }

            if (run["invocations"] is JArray invocations)
            {
                foreach (JObject item in invocations)
                {
                    ConvertInvocationStackFrameAddressesToAddressObjects(item);
                }
            }

            if (run["results"] is JArray results)
            {
                foreach (JObject result in results)
                {
                    if (result["stacks"] is JArray stacks)
                    {
                        foreach (JObject item in stacks)
                        {
                            ConvertStackFrameAddressesToAddressObjects(item);
                        }
                    }

                    if (result["codeflows"] is JArray codeflows)
                    {
                        foreach (JObject codeflow in codeflows)
                        {
                            if (codeflow["threadflow"] is JObject threadflow &&
                                threadflow["threadflowLocation"] is JObject threadflowLocation &&
                                threadflowLocation["Stack"] is JObject stack)
                            {
                                ConvertStackFrameAddressesToAddressObjects(stack);
                            }
                        }
                    }
                }
            }
        }

        private static void ConvertInvocationStackFrameAddressesToAddressObjects(JObject item)
        {
            if (item["toolExecutionNotifications"] is JArray toolExecutionNotifications)
            {
                ConvertNotificationsStackFrameAddressesToAddressObjects(toolExecutionNotifications);
            }

            if (item["toolConfigurationNotifications"] is JArray toolConfigurationNotifications)
            {
                ConvertNotificationsStackFrameAddressesToAddressObjects(toolConfigurationNotifications);
            }
        }

        private static void ConvertNotificationsStackFrameAddressesToAddressObjects(JArray notifications)
        {
            foreach (JObject notification in notifications)
            {
                if (notification["exception"] is JObject exception)
                {
                    ConvertExceptionStackFrameAddressesToAddressObjects(exception);
                }
            }
        }

        private static void ConvertExceptionStackFrameAddressesToAddressObjects(JObject exception)
        {
            if (exception["stack"] is JObject stack)
            {
                ConvertStackFrameAddressesToAddressObjects(stack);
            }

            if (exception["innerExceptions"] is JArray innerExceptions)
            {
                foreach (JObject innerException in innerExceptions)
                {
                    ConvertExceptionStackFrameAddressesToAddressObjects(innerException);
                }
            }
        }

        private static void ConvertStackFrameAddressesToAddressObjects(JObject stack)
        {
            if (stack["frames"] is JArray frames)
            {
                foreach (JObject stackFrame in frames)
                {
                    var address = new JObject();

                    if (stackFrame["address"] is JToken stackFrameAddress)
                    {
                        address.Add("baseAddress", stackFrameAddress);
                        stackFrame.Remove("address");
                    }

                    if (stackFrame["offset"] is JToken stackFrameOffset)
                    {
                        address.Add("offset", stackFrameOffset);
                        stackFrame.Remove("offset");
                    }

                    if (address.Count > 0)
                    {
                        stackFrame.Add("address", address);
                    }
                }
            }
        }


        private static void UpdateAllToolComponentProperties(JObject run)
        {
            // Access and modify run.tool
            if (run["tool"] is JObject tool)
            {
                UpdateToolObjectToolComponentProperties(tool);
            }

            // Access and modify run.conversion.tool
            if (run["conversion"] is JObject conversion && conversion["tool"] is JObject tool2)
            {
                UpdateToolObjectToolComponentProperties(tool2);
            }
        }

        private static void UpdateToolObjectToolComponentProperties(JObject tool)
        {
            // Access and modify tool.driver
            if (tool["driver"] is JObject driver)
            {
                UpdateToolComponentProperties(driver);
            }

            // Access and modify each item in tool.extensions
            if (tool["extensions"] is JArray extensions)
            {
                foreach (JObject toolComponent in extensions)
                {
                    UpdateToolComponentProperties(toolComponent);
                }
            }
        }

        private static void UpdateAllExternalPropertyFilePropertyTypes(JObject run)
        {
            if (run["externalPropertyFiles"] is JObject externalPropertyFiles)
            {
                var renamedMembers = new Dictionary<string, string>
                {
                    ["instanceGuid"] = "guid",
                    ["artifactLocation"] = "location"
                };

                RecursivePropertyRename(run, renamedMembers);
            }
        }

        private static void UpdateToolComponentProperties(JObject toolComponent)
        {
            // Access and modify artifactIndex
            if (toolComponent["artifactIndex"] is JToken artifactIndex)
            {
                toolComponent.Remove("artifactIndex");
                var artifactIndices = new JArray();
                artifactIndices.Add(artifactIndex);

                toolComponent.Add("artifactIndices", artifactIndices);
            }
        }


        private static void ConvertAllExceptionMessagesToStringAndRenameToolNotificationNodes(JObject run)
        {
            if (run["conversion"] is JObject conversion && conversion["invocation"] is JObject invocation)
            {
                ConvertInvocationExceptionMessagesToStringAndRenameToolNotifications(invocation);
            }

            if (run["invocations"] is JArray invocations)
            {
                foreach (JObject item in invocations)
                {
                    ConvertInvocationExceptionMessagesToStringAndRenameToolNotifications(item);
                }
            }
        }

        private static void ConvertInvocationExceptionMessagesToStringAndRenameToolNotifications(JObject invocation)
        {
            if (invocation["toolNotifications"] is JArray toolNotifications)
            {
                ConvertNotificationExceptionMessagesToString(toolNotifications);

                // https://github.com/oasis-tcs/sarif-spec/issues/330
                invocation.Remove("toolNotifications");
                invocation["toolExecutionNotifications"] = toolNotifications;
            }

            if (invocation["configurationNotifications"] is JArray configurationNotifications)
            {
                ConvertNotificationExceptionMessagesToString(configurationNotifications);

                // https://github.com/oasis-tcs/sarif-spec/issues/330
                invocation.Remove("configurationNotifications");
                invocation["toolConfigurationNotifications"] = configurationNotifications;
            }
        }

        private static void ConvertNotificationExceptionMessagesToString(JArray notifications)
        {
            foreach (JObject notification in notifications)
            {
                if (notification["exception"] is JObject exception)
                {
                    ConvertExceptionMessageToString(exception);
                }
            }
        }

        private static void ConvertExceptionMessageToString(JObject exception)
        {
            if (exception["message"] is JObject message && message["text"] is JToken text)
            {
                exception["message"] = text;
            }

            if (exception["innerExceptions"] is JArray innerExceptions)
            {
                foreach (JObject innerException in innerExceptions)
                {
                    ConvertExceptionMessageToString(innerException);
                }
            }
        }

        private static void RemoveToolLanguage(JObject run)
        {
            JObject tool = (JObject)run["tool"];
            if (tool["language"] is JToken language)
            {
                tool.Remove("language");
            }
        }

        private static void UpdateAllReportingDescriptorPropertyTypes(JObject run)
        {
            // Access and modify run.tool
            JObject tool = (JObject)run["tool"];
            UpdateToolReportingDescriptorPropertyTypes(tool);

            // Access and modify run.conversion.tool
            if (run["conversion"] is JObject conversion)
            {
                tool = (JObject)conversion["tool"];
                UpdateToolReportingDescriptorPropertyTypes(tool);
            }
        }

        private static void UpdateToolReportingDescriptorPropertyTypes(JObject tool)
        {
            // Access and modify tool.driver
            if (tool["driver"] is JObject driver)
            {
                UpdateToolComponentReportingDescriptorPropertyTypes(driver);
            }

            // Access and modify each item in tool.extensions
            if (tool["extensions"] is JArray extensions)
            {
                foreach (JObject toolComponent in extensions)
                {
                    UpdateToolComponentReportingDescriptorPropertyTypes(toolComponent);
                }
            }
        }

        private static void UpdateToolComponentReportingDescriptorPropertyTypes(JObject toolComponent)
        {
            // Access and modify toolComponent.notificationDescriptors
            if (toolComponent["notificationDescriptors"] is JArray notificationDescriptors)
            {
                UpdateReportingDescriptorPropertyTypes(notificationDescriptors);
            }

            // Access and modify toolComponent.ruleDescriptors
            if (toolComponent["ruleDescriptors"] is JArray ruleDescriptors)
            {
                UpdateReportingDescriptorPropertyTypes(ruleDescriptors);
            }
        }

        private static void UpdateReportingDescriptorPropertyTypes(JArray reportingDescriptors)
        {
            foreach (JObject reportingDescriptor in reportingDescriptors)
            {
                if (reportingDescriptor["name"] is JObject message && message["text"] is JToken text)
                {
                    reportingDescriptor["name"] = text;
                }

                if (reportingDescriptor["shortDescription"] is JObject message2)
                {
                    // We must convert this JObject from type "Message" to "MultiformatMessageString".
                    // Both Objects have common JTokens "text" and "markdown" which do not need modification.
                    // Hence, we only need to strip out additional properties that Message object may have (messageId and arguments).
                    if (message2["messageId"] is JToken)
                    {
                        message2.Remove("messageId");
                    }

                    if (message2["arguments"] is JArray)
                    {
                        message2.Remove("arguments");
                    }
                }

                if (reportingDescriptor["fullDescription"] is JObject message3)
                {
                    // We must convert this JObject from type "Message" to "MultiformatMessageString".
                    // Both Objects have common JTokens "text" and "markdown" which do not need modification.
                    // Hence, we only need to strip out additional properties that Message object may have (messageId and arguments).
                    if (message3["messageId"] is JToken)
                    {
                        message3.Remove("messageId");
                    }

                    if (message3["arguments"] is JArray)
                    {
                        message3.Remove("arguments");
                    }
                }
            }
        }

        private static bool ApplyChangesFromTC31(JObject sarifLog)
        {
            UpdateSarifLogVersion(sarifLog);

            if (sarifLog["runs"] is JArray runs)
            {
                foreach (JObject run in runs)
                {
                    // https://github.com/oasis-tcs/sarif-spec/issues/311
                    MoveRuleDescriptors(run);

                    // https://github.com/oasis-tcs/sarif-spec/issues/179
                    MoveToolPropertiesIntoDriverToolComponent(run);

                    if (run["results"] is JArray results)
                    {
                        foreach (JObject result in results)
                        {
                            // https://github.com/oasis-tcs/sarif-spec/issues/312
                            UpdateBaselineExistingStateToUnchanged(result);

                            // https://github.com/oasis-tcs/sarif-spec/issues/317
                            SetResultKindAndFailureLevel(result);
                        }
                    }

                    if (run["files"] is JArray files)
                    {
                        foreach (JObject artifact in files)
                        {
                            if (artifact["fileLocation"] is JObject location)
                            {
                                artifact.Remove("fileLocation");
                                artifact["location"] = location;
                            }
                        }

                        run["artifacts"] = files;
                        run.Remove("files");
                    }

                    var universallyRenamedMembers = new Dictionary<string, string>
                    {
                        ["richText"] = "markdown",
                        ["fileChange"] = "artifactChange",
                        ["fileLocation"] = "artifactLocation",
                        ["fileIndex"] = "artifactIndex",
                        ["fileChanges"] = "changes",
                    };

                    RecursivePropertyRename(run, universallyRenamedMembers);
                }
            }
            return true;
        }

        private static void RecursivePropertyRename(JObject parentObject, JProperty property, Dictionary<string, string> renamedMembers)
        {
            JToken newValue = property.Value;

            if (renamedMembers.TryGetValue(property.Name, out string newName))
            {
                parentObject.Remove(property.Name);
            }
            else
            {
                newName = property.Name;
            }

            if (property.Value is JArray jArray)
            {
                newValue = RecursivePropertyRename(jArray, renamedMembers);
            }
            else if (property.Value is JObject jObject)
            {
                newValue = RecursivePropertyRename(jObject, renamedMembers);
            }
            parentObject[newName] = newValue;
        }

        private static JToken RecursivePropertyRename(JArray jArray, Dictionary<string, string> renamedMembers)
        {
            foreach (JToken jToken in jArray)
            {
                if (jToken is JObject jObject)
                {
                    RecursivePropertyRename(jObject, renamedMembers);
                }
                // Note that we don't have to handle arrays of values or other arrays.
                // These aren't expressed in standard SARIF, if we hit this code path
                // that means we're processing some property bag content.
            }
            return jArray;
        }

        private static JObject RecursivePropertyRename(JObject jObject, Dictionary<string, string> renamedMembers)
        {
            var properties = new List<JProperty>(jObject.Properties());
            foreach (JProperty property in properties)
            {
                // We won't process property bags, so that we don't inadvertently
                // rename custom data that isn't a part of formal SARIF
                if (property.Name.Equals("properties")) { continue; }
                RecursivePropertyRename(jObject, property, renamedMembers);
            }
            return jObject;
        }

        private static void MoveToolPropertiesIntoDriverToolComponent(JObject run)
        {
            // https://github.com/oasis-tcs/sarif-spec/issues/179

            // 1. Retrieve run.tool object, which will serve as the basis of the
            //    new run.tool.driver object and zap sarifLoggerVersion from it.
            JObject driver = (JObject)run["tool"];
            driver.Remove("sarifLoggerVersion");

            // 2. Create a new tool object, preserving only the language property
            JObject tool = new JObject(new JProperty("language", driver["language"] ?? "en-US"));
            driver.Remove("language");

            // https://github.com/oasis-tcs/sarif-spec/issues/319
            // 3. toolComponent.messageStrings now merges all plain text
            //    and markdown strings. So we need to merge and eliminate
            //    the 'richMessageStrings property.
            MergeRichMessagesInDescriptorsArrays(driver);

            // 4. Persist extracted data as tool.driver and place back on run.
            tool["driver"] = driver;

            // 5. run.richTextMimeType renamed to run.markdownMessageMimeType.
            RenameProperty(run, "richTextMimeType", "markdownMessageMimeType");

            run["tool"] = tool;

            // 6. Update some properties on run.conversion, if present. Note that the
            // notion of reportingDescriptors associated with the conversion did not
            // exist previously, so no transformation is required here.
            if (run["conversion"] is JObject conversion)
            {
                driver = (JObject)conversion["tool"];
                
                tool = new JObject(new JProperty("language", driver["language"] ?? "en-US"));

                driver.Remove("language");
                driver.Remove("sarifLoggerVersion");

                tool["driver"] = driver;
                conversion["tool"] = tool;
                run["conversion"] = conversion;
            }

            // Other changes in this schema update do not require any transformation, as
            // the remainder is additive. This includes:
            //  toolComponent.fileIndex -> associate a component with a run.files entry
            //  run.tool.extensions -> array of extension tool components
            //  result.extensionIndex -> associate a result with an extension
            //  toolComponent -> new file role.
        }

        private static void MergeRichMessagesInDescriptorsArrays(JObject toolComponent)
        {
            MergeRichMessageStringsIntoMessageStrings((JArray)toolComponent["ruleDescriptors"]);

            // NOTE: we don't need to process notificationDescriptors, because this
            //       concept did not ship in the 2019-01-09 schema.
        }

        private static void MergeRichMessageStringsIntoMessageStrings(JArray descriptors)
        {
            if (descriptors == null) { return; }

            foreach (JObject descriptor in descriptors.Children())
            {
                var messageStrings = (JObject)descriptor?["messageStrings"];
                var richMessageStrings = (JObject)descriptor?["richMessageStrings"];

                if (messageStrings == null && richMessageStrings == null)
                {
                    continue;
                }                

                foreach (JProperty property in messageStrings.Properties())
                {
                    // Create base multiformatMessageString object with plaintext value
                    JObject multiformatMessageString = CreateMultiformatMessageStringFromPlaintext((string)property.Value);

                    // If we find a matching markdown property in richMessageStrings, move it over.
                    multiformatMessageString["markdown"] = (string)richMessageStrings?[property.Name];

                    // Replace the message strings property with the multi-format version
                    messageStrings[property.Name] = multiformatMessageString;

                    // NOTE: we don't process any richMessageString items that don't have a 
                    //       corresponding plaintext equivalent. This is not legal SARIF.
                }

                descriptor.Remove("richMessageStrings");
            }
        }

        private static JObject CreateMultiformatMessageStringFromPlaintext(string plaintext)
        {
            JProperty textProperty = new JProperty("text", plaintext);
            return new JObject(textProperty);
        }

        private static void MoveRuleDescriptors(JObject run)
        {
            // https://github.com/oasis-tcs/sarif-spec/issues/311

            if (!(run["resources"] is JObject resources))
            {
                return;
            }

            JObject tool = (JObject)run["tool"];

            // 1. 'run.resources.messageStrings' moves to 'run.tool.globalMessageStrings'
            if (resources["messageStrings"] is JObject messageStrings)
            {
                // https://github.com/oasis-tcs/sarif-spec/issues/319
                ConvertToMultiformatMessageStrings(messageStrings);

                tool["globalMessageStrings"] = messageStrings;
            }

            // 2. 'run.resources.rules' moves to 'run.tool.ruleDescriptors'
            if (resources["rules"] is JArray rules)
            {
                foreach(JObject rule in rules)
                {
                    RenameProperty(rule, previousName: "configuration", newName: "defaultConfiguration");

                    if (rule["defaultConfiguration"] is JObject reportingConfiguration)
                    {
                        RenameProperty(reportingConfiguration, previousName: "defaultLevel", newName: "level");
                        RenameProperty(reportingConfiguration, previousName: "defaultRank", newName: "rank");
                    }
                }
                tool["ruleDescriptors"] = rules;
            }

            // 3. We do not need any accommodation for the addition of 
            // 'tool.notificationDescriptors', as this did not exist previously

            // 4. Zap 'rules.resources' entirely
            run.Remove("resources");
        }

        private static void ConvertToMultiformatMessageStrings(JObject messageStrings)
        {
            // https://github.com/oasis-tcs/sarif-spec/issues/319
            foreach (JProperty property in messageStrings.Properties())
            {
                JObject multiformatMessageString = CreateMultiformatMessageStringFromPlaintext((string)property.Value);

                messageStrings[property.Name] = multiformatMessageString;
            }
        }

        private static void UpdateBaselineExistingStateToUnchanged(JObject result)
        {
            // Rename 'existing' baseline state to 'unchanged'
            // (as part of adding the 'updated' state, which 
            // will not exist in any legacy SARIF logs).
            // https://github.com/oasis-tcs/sarif-spec/issues/312
            //

            string baselineState = (string)result["baselineState"];

            if ("existing".Equals(baselineState))
            {
                result["baselineState"] = "unchanged";
            }
        }

        private static void SetResultKindAndFailureLevel(JObject result)
        {
            string level = (string)result["level"];
            if (level == null) { return; }

            // Every result now has a failure level of 'none', 'note',
            // 'warning' or 'error'. 'pass', 'notApplicable' and 'open'
            // are the new 'result.kind' property that can categorize
            // non-failures. 
            // https://github.com/oasis-tcs/sarif-spec/issues/317

            switch (level)
            {
                // We don't need to handle 'none' as it previously wasn't
                // an enum value that was permitted by the schema.
                case "error":
                case "warning":
                case "note":
                {
                    // 'level' is set appropriately, so we'll mark this result
                    // kind to indicate it has been evaluated as a failure
                    result["kind"] = "fail";
                    break;
                }
                case "open":
                case "notApplicable":
                case "pass":
                {
                    // Legacy level indicates we do not have a failure. Move this
                    // designation to result.kind.
                    result["kind"] = level;
                    result["level"] = "none";
                    break;
                }
            }
        }

        private static bool ApplyChangesFromTC25ThroughTC30(
            JObject sarifLog, 
            out Dictionary<string, int> fullyQualifiedLogicalNameToIndexMap,
            out Dictionary<string, int> fileKeyToIndexMap,
            out Dictionary<string, int> ruleKeyToIndexMap)
        {
            fullyQualifiedLogicalNameToIndexMap = null;
            fileKeyToIndexMap = null;
            ruleKeyToIndexMap = null;

            // Note: we could have injected the TC26 - TC28 changes into the other helpers in this
            // code. This would prevent multiple passes over things like the run.results array.
            // We've isolated the changes here instead simply to keep them grouped together.

            bool modifiedLog = UpdateSarifLogVersion(sarifLog); 

            // For completness, this update added run.newlineSequences to the schema
            // This is a non-breaking (additive) change, so there is no work to do.
            //https://github.com/oasis-tcs/sarif-spec/issues/169
            
            if (sarifLog["runs"] is JArray runs)
            {
                foreach (JObject run in runs)
                {
                    // Delete run.architecture. This data could, arguably, be transferred into the run logical
                    // identifier or we could drop it into a property bag, but realistically, we don't expect
                    // sufficient existing utilization of this property to warrant preserving it.

                    // Remove run.architecture: https://github.com/oasis-tcs/sarif-spec/issues/262
                    if (run["architecture"] is JToken architecture)
                    {
                        run.Remove(nameof(architecture));
                        modifiedLog = true;
                    }

                    // Logical locations are now an array. We will first transform the previous
                    // dictionary into the arrays form. We will retain the index for each
                    // transformed logical location. Later, we will associate these indices
                    // with results, if applicable.
                    Dictionary<LogicalLocation, int> logicalLocationToIndexMap = null;

                    if (run["logicalLocations"] is JObject logicalLocations)
                    {
                        logicalLocationToIndexMap = new Dictionary<LogicalLocation, int>(LogicalLocation.ValueComparer);

                        run["logicalLocations"] =
                            ConvertLogicalLocationsDictionaryToArray(
                                logicalLocations,
                                logicalLocationToIndexMap,
                                out fullyQualifiedLogicalNameToIndexMap);

                        modifiedLog |= fullyQualifiedLogicalNameToIndexMap.Count > 0;
                    }

                    // Files are now persisted to an array. We will persist a mapping from
                    // previous file key to file index. We will associate the index with
                    // each result when we iterate over run.results later.

                    if (run["files"] is JObject files)
                    {
                        fileKeyToIndexMap = new Dictionary<string, int>();

                        run["files"] =
                            ConvertFilesDictionaryToArray(
                                files,
                                fileKeyToIndexMap);

                        modifiedLog |= fileKeyToIndexMap.Count > 0;
                    }

                    if (run["resources"] is JObject resources)
                    {
                        ruleKeyToIndexMap = new Dictionary<string, int>();

                        if (resources["rules"] is JObject rules)
                        {
                            resources["rules"] =
                                ConvertRulesDictionaryToArray(
                                    rules,
                                    ruleKeyToIndexMap);
                        }

                        modifiedLog |= ruleKeyToIndexMap.Count > 0;

                        // Remove 'open' from rule configuration default level enumeration
                        // https://github.com/oasis-tcs/sarif-spec/issues/288
                        modifiedLog |= RemapRuleDefaultLevelFromOpenToNote(resources);
                    }

                    if (run["results"] is JArray results)
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
                    // 
                    // We will also explicitly apply the default tool.language value of "en-US".

                    JObject tool = (JObject)run["tool"];
                    modifiedLog |= RenameProperty(tool, previousName: "fileVersion", newName: "dottedQuadFileVersion");
                    PopulatePropertyIfAbsent(tool, "language", "en-US", ref modifiedLog);

                    if (run["conversion"] is JObject conversion)
                    {
                        tool = (JObject)conversion["tool"];
                        modifiedLog |= RenameProperty(tool, previousName: "fileVersion", newName: "dottedQuadFileVersion");
                        PopulatePropertyIfAbsent(tool, "language", "en-US", ref modifiedLog);
                    }

                    // Specify columnKind as ColumndKind.Utf16CodeUnits in cases where the enum is missing from
                    // the SARIF file. Moving forward, the absence of this enum will be interpreted as
                    // the new default, which is ColumnKind.UnicodeCodePoints.
                    // https://github.com/oasis-tcs/sarif-spec/issues/188
                    PopulatePropertyIfAbsent(run, "columnKind", "utf16CodeUnits", ref modifiedLog);
                }
            }

            return modifiedLog;
        }

        private static JToken ConvertRulesDictionaryToArray(JObject rules, Dictionary<string, int> ruleKeyToIndexMap)
        {
            if (rules == null) { return null; }

            Dictionary<JObject, int> jObjectToIndexMap = new Dictionary<JObject, int>();

            foreach (JProperty ruleEntry in rules.Properties())
            {
                AddEntryToRuleToIndexMap(
                    rules,
                    ruleEntry.Name,
                    (JObject)ruleEntry.Value,
                    jObjectToIndexMap,
                    ruleKeyToIndexMap);
            }

            var rulesArray = new JObject[jObjectToIndexMap.Count];

            foreach (KeyValuePair<JObject, int> keyValuePair in jObjectToIndexMap)
            {
                int index = keyValuePair.Value;
                JObject updatedFileData = keyValuePair.Key;
                rulesArray[index] = updatedFileData;
            }

            return new JArray(rulesArray);
        }


        private static void AddEntryToRuleToIndexMap(JObject rulesDictionary, string key, JObject rule, Dictionary<JObject, int> jObjectToIndexMap, Dictionary<string, int> ruleKeyToIndexMap)
        {
            ruleKeyToIndexMap = ruleKeyToIndexMap ?? throw new ArgumentNullException(nameof(ruleKeyToIndexMap));
            jObjectToIndexMap = jObjectToIndexMap ?? throw new ArgumentNullException(nameof(jObjectToIndexMap));

            if (rule["id"] == null)
            {
                // This condition indicates there was no collision between the rules dictionary key
                // and the corresponding rule id. So we will explicitly populate the rule id so
                // this data isn't lost, compromising log readability.
                rule["id"] = key;
            }

            if (!ruleKeyToIndexMap.TryGetValue(key, out int ruleIndex))
            {
                ruleIndex = ruleKeyToIndexMap.Count;
                jObjectToIndexMap[rule] = ruleIndex;
                ruleKeyToIndexMap[key] = ruleIndex;
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        private static void PopulatePropertyIfAbsent(JObject jObject, string propertyName, string value, ref bool modifiedLog)
        {
            if (jObject != null && jObject[propertyName] == null)
            {
                jObject[propertyName] = value;
                modifiedLog = true;
            }
        }

        private static JToken ConvertFilesDictionaryToArray(JObject files, Dictionary<string, int> keyToIndexMap)
        { 
            if (files == null) { return null; }

            Dictionary<JObject, int> jObjectToIndexMap = new Dictionary<JObject, int>();

            foreach (JProperty fileEntry in files.Properties())
            {
                AddEntryToFileLocationToIndexMap(
                    files,
                    fileEntry.Name,
                    (JObject)fileEntry.Value,
                    jObjectToIndexMap,
                    keyToIndexMap);
            }

            var filesArray = new JObject[jObjectToIndexMap.Count];

            foreach (KeyValuePair<JObject, int> keyValuePair in jObjectToIndexMap)
            {
                int index = keyValuePair.Value;
                JObject updatedFileData = keyValuePair.Key;
                filesArray[index] = updatedFileData;
            }

            return new JArray(filesArray);
        }

        private static void AddEntryToFileLocationToIndexMap(JObject filesDictionary, string key, JObject file, Dictionary<JObject, int> jObjectToIndexMap, Dictionary<string, int> keyToIndexMap)
        {
            keyToIndexMap = keyToIndexMap ?? throw new ArgumentNullException(nameof(keyToIndexMap));
            jObjectToIndexMap = jObjectToIndexMap ?? throw new ArgumentNullException(nameof(jObjectToIndexMap));

            // We've seen this key before. This happens when we order files processing in order to ensure
            // that a parent key has been transformed before its children (to produce a parent index value).
            if (keyToIndexMap.ContainsKey(key)) { return; }

            // Parent key is no longer relevant and will be removed.
            string parentKey = (string)file["parentKey"];

            if (parentKey == null)
            {
                // No parent key? We are a root location. 
                file["parentIndex"] = -1;
            }
            else
            {
                // Have we processed our parent yet? If so, retrieve the parentIndex and we're done.
                if (!keyToIndexMap.TryGetValue(parentKey, out int parentIndex))
                {
                    // The parent hasn't been processed yet. We must force its creation
                    // determine its index in our array. This code path results in 
                    // an array order that does not precisely match the enumeration
                    // order of the original files dictionary.
                    AddEntryToFileLocationToIndexMap(
                        filesDictionary,
                        parentKey,
                        (JObject)filesDictionary[parentKey],
                        jObjectToIndexMap,
                        keyToIndexMap);

                    parentIndex = keyToIndexMap[parentKey];
                }
                file.Remove("parentKey");
                file["parentIndex"] = parentIndex;
            }

            var fileLocationKey = ArtifactLocation.CreateFromFilesDictionaryKey(key, parentKey);

            JObject fileLocationObject = new JObject();
            file["fileLocation"] = fileLocationObject;
            fileLocationObject["uri"] = fileLocationKey.Uri;
            fileLocationObject["uriBaseId"] = fileLocationKey.UriBaseId;

            if (!keyToIndexMap.TryGetValue(key, out int fileIndex))
            {
                fileIndex = keyToIndexMap.Count;
                jObjectToIndexMap[file] = fileIndex;
                keyToIndexMap[key] = fileIndex;
                fileLocationObject["fileIndex"] = fileIndex;
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        private static JArray ConvertLogicalLocationsDictionaryToArray(
            JObject logicalLocations,
            Dictionary<LogicalLocation, int> logicalLocationToIndexMap,
            out Dictionary<string, int> fullyQualifiedLogicalNameToIndexMap)
        {
            fullyQualifiedLogicalNameToIndexMap = null;

            if (logicalLocations == null) { return null; }

            Dictionary<JObject, int> jObjectToIndexMap = new Dictionary<JObject, int>();
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

            var logicalLocationsArray = new JObject[jObjectToIndexMap.Count];

            foreach (KeyValuePair<JObject, int> keyValuePair in jObjectToIndexMap)
            {
                int index = keyValuePair.Value;
                JObject updatedLogicalLocation = keyValuePair.Key;
                logicalLocationsArray[index] = updatedLogicalLocation;
            }

            return new JArray(logicalLocationsArray);
        }

        private static void AddEntryToFullyQualifiedNameToIndexMap(
            JObject logicalLocationsDictionary,
            string keyName,
            JObject logicalLocation,
            Dictionary<LogicalLocation, int> logicalLocationToIndexMap,
            Dictionary<JObject, int> jObjectToIndexMap,
            Dictionary<string, int> keyToIndexMap)
        {
            keyToIndexMap = keyToIndexMap ?? throw new ArgumentNullException(nameof(keyToIndexMap));
            logicalLocation = logicalLocation ?? throw new ArgumentNullException(nameof(logicalLocation));

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

            var rules = (JArray)resources["rules"];
            if (rules == null ) { return modifiedResources; }

            foreach (JObject rule in rules)
            {
                var configuration = (JObject)rule["configuration"];
                if (configuration == null) { continue; }

                if ("open".Equals((string)configuration["defaultLevel"]))
                {
                    // We remap 'open' to 'note'. 'open' is an indicator that analysis is unresolved, i.e., 
                    // the question of whether a weakness exists is not yet determined. 'note' is the most
                    // reasonable level to associate with this class of report, if it is emitted. In 
                    // practice, we don't expect that a current producer exists who is in this condition.
                    configuration["defaultLevel"] = "note";
                    modifiedResources = true;
                }
            }

            return modifiedResources;
        }

        private static bool ApplyCoreTransformations(JObject sarifLog)
        {
            bool modifiedLog = UpdateSarifLogVersion(sarifLog); 

            if (sarifLog["runs"] is JArray runs)
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
            if (run["automationLogicalId"] is JToken automationLogicalId)
            {
                run.Remove("automationLogicalId");

                var aggregateId = new JObject
                {
                    // For the aggregating automation id, we can only provide the logical component
                    ["instanceId"] = automationLogicalId + "/"
                };

                run["aggregateIds"] = new JArray(aggregateId);
                modifiedRun = true;
            }

            return modifiedRun;
        }

        private static bool UpdateRunVersionControlProvenance(JObject run)
        {
            bool modifiedRun = false;

            if (run["versionControlProvenance"] is JArray versionControlDetailsArray)
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

                if (result["codeFlows"] is JArray codeFlows)
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
            
            if (run["invocations"] is JArray invocations)
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

            if (!(run["files"] is JObject files)) { return modifiedRun; }

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
            bool renamedProperty = false;

            if (jObject == null) { return renamedProperty; }
            
            if (jObject[previousName] is JToken propertyValue)
            {
                jObject.Remove(previousName);
                jObject[newName] = propertyValue;
                renamedProperty = true;
            }

            return renamedProperty;
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
                if (invocation["configurationNotifications"] is JArray configurationNotifications)
                {
                    modifiedRun |= UpdateNotifications(configurationNotifications);
                }

                if (invocation["toolNotifications"] is JArray toolNotifications)
                {
                    modifiedRun |= UpdateNotifications(toolNotifications);
                }
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

            if (notification["exception"] is JObject exception)
            {
                modifiedNotification |= ConvertExceptionMessageFromStringToMessageObject(exception);

                if (exception["innerExceptions"] is JArray innerExceptions)
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
