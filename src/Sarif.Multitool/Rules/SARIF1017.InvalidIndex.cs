// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class InvalidIndex : SarifValidationSkimmerBase
    {
        private readonly MultiformatMessageString _fullDescription = new MultiformatMessageString
        {
            Text = RuleResources.SARIF1017_InvalidIndex
        };

        public override MultiformatMessageString FullDescription => _fullDescription;

        public override FailureLevel DefaultLevel => FailureLevel.Error;

        /// <summary>
        /// SARIF1017
        /// </summary>
        public override string Id => RuleId.InvalidIndex;

        protected override IEnumerable<string> MessageResourceNames => new string[]
        {
            nameof(RuleResources.SARIF1017_Default),
            nameof(RuleResources.SARIF1017_InvalidGuid)
        };

        protected override void Analyze(Address address, string addressPointer)
        {
            ValidateArrayIndex(
                address.Index,
                Context.CurrentRun.Addresses,
                addressPointer,
                "address",
                SarifPropertyName.Index,
                $"runs[{Context.CurrentRunIndex}].addresses");

            ValidateArrayIndex(
                address.ParentIndex,
                Context.CurrentRun.Addresses,
                addressPointer,
                "address",
                SarifPropertyName.ParentIndex,
                $"runs[{Context.CurrentRunIndex}].addresses");
        }

        protected override void Analyze(Artifact artifact, string artifactPointer)
        {
            ValidateArrayIndex(
                artifact.ParentIndex,
                Context.CurrentRun.Artifacts,
                artifactPointer,
                "artifact",
                SarifPropertyName.ParentIndex,
                $"runs[{Context.CurrentRunIndex}].artifacts");
        }

        protected override void Analyze(ArtifactLocation artifactLocation, string artifactLocationPointer)
        {
            ValidateArrayIndex(
                artifactLocation.Index,
                Context.CurrentRun.Artifacts,
                artifactLocationPointer,
                "artifactLocation",
                SarifPropertyName.Index,
                $"runs[{Context.CurrentRunIndex}].artifacts");
        }

        protected override void Analyze(GraphTraversal graphTraversal, string graphTraversalPointer)
        {
            ValidateArrayIndex(
                graphTraversal.RunGraphIndex,
                Context.CurrentRun.Graphs,
                graphTraversalPointer,
                "graphTraversal",
                SarifPropertyName.RunGraphIndex,
                $"runs[{Context.CurrentRunIndex}].graphTraversals");

            ValidateArrayIndex(
                graphTraversal.ResultGraphIndex,
                Context.CurrentResult.Graphs,
                graphTraversalPointer,
                "graphTraversal",
                SarifPropertyName.ResultGraphIndex,
                $"runs[{Context.CurrentRunIndex}].results[{Context.CurrentResultIndex}].graphTraversals");
        }

        protected override void Analyze(LogicalLocation logicalLocation, string logicalLocationPointer)
        {
            ValidateArrayIndex(
                logicalLocation.Index,
                Context.CurrentRun.LogicalLocations,
                logicalLocationPointer,
                "logicalLocation",
                SarifPropertyName.Index,
                $"runs[{Context.CurrentRunIndex}].logicalLocations");

            ValidateArrayIndex(
                logicalLocation.ParentIndex,
                Context.CurrentRun.LogicalLocations,
                logicalLocationPointer,
                "logicalLocation",
                SarifPropertyName.ParentIndex,
                $"runs[{Context.CurrentRunIndex}].logicalLocations");
        }

        protected override void Analyze(ReportingDescriptorReference reportingDescriptorReference, string reportingDescriptorReferencePointer)
        {
            // Does this reporting descriptor reference refer to a reporting descriptor defined by
            // the driver, one of the extensions, or a taxonomy?
            ToolComponent toolComponent = GetReferencedToolComponent(reportingDescriptorReference, out string guid, out string objectPath, out SarifValidationContext.ReportingDescriptorKind reportingDescriptorKind);

            if (toolComponent == null && objectPath == null)
            {
                LogResult(
                    reportingDescriptorReferencePointer,
                    nameof(RuleResources.SARIF1017_InvalidGuid),
                    guid);
            }

            // We use reportingDescriptorKind to decide which array to validate the index against
            // (rules, notifications, or taxa). If GetReferencedToolComponent identified the reportingDescriptor by way of
            // a GUID, then we know which array the descriptor was found in
            if (guid == null)
            {
                reportingDescriptorKind = Context.CurrentReportingDescriptorKind;
            }

            // Does this reporting descriptor reference refer to a rule, a notification, or a taxon?
            string arrayPropertyName;
            IList<ReportingDescriptor> reportingDescriptors;

            switch (reportingDescriptorKind)
            {
                case SarifValidationContext.ReportingDescriptorKind.Rule:
                    arrayPropertyName = SarifPropertyName.Rules;
                    reportingDescriptors = toolComponent?.Rules;
                    break;

                case SarifValidationContext.ReportingDescriptorKind.Notification:
                    arrayPropertyName = SarifPropertyName.Notifications;
                    reportingDescriptors = toolComponent?.Notifications;
                    break;

                case SarifValidationContext.ReportingDescriptorKind.Taxon:
                    arrayPropertyName = SarifPropertyName.Taxa;
                    reportingDescriptors = toolComponent?.Taxa;
                    break;

                default:
                    throw new InvalidOperationException("Unexpected call to Analyze(ReportingDescriptorReference)");
            }

            ValidateArrayIndex(
                reportingDescriptorReference.Index,
                reportingDescriptors,
                reportingDescriptorReferencePointer,
                "reportingDescriptorReference",
                SarifPropertyName.Index,
                $"{objectPath}.{arrayPropertyName}");
        }

        // The relative JSON path from the current run to the tool driver.
        private const string DriverRelativePath = "tool.driver";

        private ToolComponent GetReferencedToolComponent(
            ReportingDescriptorReference reportingDescriptorReference,
            out string guid,
            out string objectPath,
            out SarifValidationContext.ReportingDescriptorKind reportingDescriptorKind)
        {
            guid = null;
            reportingDescriptorKind = SarifValidationContext.ReportingDescriptorKind.None;

            Tool tool = Context.CurrentRun.Tool;
            string objectPathBase = $"runs[{Context.CurrentRunIndex}]";

            // The default, unless the reportingDescriptorReference specifies otherwise:
            ToolComponent toolComponent = tool.Driver;
            string relativeObjectPath = DriverRelativePath;

            ToolComponentReference toolComponentReference = reportingDescriptorReference.ToolComponent;
            if (toolComponentReference != null)
            {
                // If there is a GUID, it wins.
                guid = toolComponentReference.Guid;
                if (guid != null)
                {
                    toolComponent = GetToolComponentByGuid(guid, out relativeObjectPath);
                    if (toolComponent == null)
                    {
                        // There is no component with the specified GUID.
                        objectPath = null;
                        return null;
                    }
                }
                else
                {
                    // If there's no GUID, then the index, if present, refers to an element of tool.extensions.
                    int? toolComponentIndex = toolComponentReference.Index;
                    if (toolComponentIndex >= 0)
                    {
                        // This reporting descriptor reference refers to an extension, but does that
                        // extension exist?
                        toolComponent = tool.Extensions?.Count > toolComponentIndex.Value
                            ? tool.Extensions[toolComponentIndex.Value]
                            : null;

                        relativeObjectPath = $"tool.extensions[{toolComponentIndex}]";
                    }
                }
            }

            objectPath = $"{objectPathBase}.{relativeObjectPath}";

            return toolComponent;
        }

        private ToolComponent GetToolComponentByGuid(string guid, out string relativeObjectPath)
        {
            Run run = Context.CurrentRun;
            Tool tool = run.Tool;

            if (ToolComponentHasGuid(tool.Driver, guid))
            {
                relativeObjectPath = DriverRelativePath;
                return tool.Driver;
            }

            if (tool.Extensions != null)
            {
                for (int i = 0; i < tool.Extensions.Count; ++i)
                {
                    if (ToolComponentHasGuid(tool.Extensions[i], guid))
                    {
                        relativeObjectPath = $"tool.extensions[{i}]";
                        return tool.Extensions[i];
                    }
                }
            }

            if (run.Taxonomies != null)
            {
                for (int i = 0; i < run.Taxonomies.Count; ++i)
                {
                    if (ToolComponentHasGuid(run.Taxonomies[i], guid))
                    {
                        relativeObjectPath = $"taxonomies[{i}]";
                        return run.Taxonomies[i];
                    }
                }
            }

            // In SARIF v2.1.0, there is no valid scenario where a tool component refers to a translation
            // or to a policy, so there's no need to search there. We don't care if the specified GUID
            // refers to a translation or a policy, or whether it refers to no tool component at all.
            // Either way, it's an error.
            relativeObjectPath = null;
            return null;
        }

        private bool ToolComponentHasGuid(ToolComponent toolComponent, string guid)
            => toolComponent?.Guid != null && toolComponent.Guid.Equals(guid, StringComparison.OrdinalIgnoreCase);

        protected override void Analyze(Result result, string resultPointer)
        {
            ValidateArrayIndex(
                result.RuleIndex,
                Context.CurrentRun.Tool.Driver.Rules,
                resultPointer,
                "result",
                SarifPropertyName.RuleIndex,
                $"runs[{Context.CurrentRunIndex}].tool.driver.rules");
        }

        protected override void Analyze(ResultProvenance resultProvenance, string resultProvenancePointer)
        {
            ValidateArrayIndex(
                resultProvenance.InvocationIndex,
                Context.CurrentRun.Invocations,
                resultProvenancePointer,
                "resultProvenance",
                SarifPropertyName.InvocationIndex,
                $"runs[{Context.CurrentRunIndex}].invocations");
        }

        protected override void Analyze(ThreadFlowLocation threadFlowLocation, string threadFlowLocationPointer)
        {
            ValidateArrayIndex(
                threadFlowLocation.Index,
                Context.CurrentRun.ThreadFlowLocations,
                threadFlowLocationPointer,
                "threadFlowLocation",
                SarifPropertyName.Index,
                $"runs[{Context.CurrentRunIndex}].threadFlowLocations");
        }

        protected override void Analyze(ToolComponentReference toolComponentReference, string toolComponentReferencePointer)
        {
            // BUGBUG: If the ToolComponentReference has a Guid, it might point to a Taxonomy.
            // (There are no valid cases for a ToolComponentReference to point to a Translation
            // or a Policy.
            ValidateArrayIndex(
                toolComponentReference.Index,
                Context.CurrentRun.Tool.Extensions,
                toolComponentReferencePointer,
                "toolComponentReference",
                SarifPropertyName.Index,
                $"runs[{Context.CurrentRunIndex}].tool.extensions");
        }

        protected override void Analyze(WebRequest webRequest, string webRequestPointer)
        {
            ValidateArrayIndex(
                webRequest.Index,
                Context.CurrentRun.WebRequests,
                webRequestPointer,
                "webRequest",
                SarifPropertyName.Index,
                $"runs[{Context.CurrentRunIndex}].webRequests");
        }

        protected override void Analyze(WebResponse webResponse, string webResponsePointer)
        {
            ValidateArrayIndex(
                webResponse.Index,
                Context.CurrentRun.WebResponses,
                webResponsePointer,
                "webResponse",
                SarifPropertyName.Index,
                $"runs[{Context.CurrentRunIndex}].webResponses");
        }

        private void ValidateArrayIndex<T>(
            int index,
            IList<T> container,
            string jsonPointer,
            string objectName,
            string propertyName,
            string arrayName)
        {
            if (!IndexIsValid(index, container))
            {
                LogResult(
                    jsonPointer,
                    nameof(RuleResources.SARIF1017_Default),
                    objectName,
                    propertyName,
                    index.ToInvariantString(),
                    arrayName,
                    (index + 1).ToInvariantString());
            }
        }

        private static bool IndexIsValid<T>(int index, IList<T> container)
                => index == -1 || (index >= 0 && container?.Count > index);
    }
}
