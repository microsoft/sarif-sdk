﻿// Copyright (c) Microsoft. All rights reserved.
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
            nameof(RuleResources.SARIF1017_Default)
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
            Tool tool = Context.CurrentRun.Tool;

            // Does this reporting descriptor reference refer to a reporting descriptor defined by
            // the driver or by one of the extensions?
            ToolComponent toolComponent;
            string toolComponentPathSegment;

            int? toolComponentIndex = reportingDescriptorReference.ToolComponent?.Index;
            if (toolComponentIndex >= 0)
            {
                // This reporting descriptor reference refers to an extension, but does that
                // extension exist?
                toolComponent = tool.Extensions?.Count > toolComponentIndex.Value
                    ? tool.Extensions[toolComponentIndex.Value]
                    : null;

                toolComponentPathSegment = $"{SarifPropertyName.Extensions}[{toolComponentIndex}]";
            }
            else
            {
                toolComponent = tool.Driver;
                toolComponentPathSegment = SarifPropertyName.Driver;
            }

            // Does this reporting descriptor reference refer to a rule, a notification, or a taxon?
            string arrayPropertyName;
            IList<ReportingDescriptor> reportingDescriptors;

            switch (Context.CurrentReportingDescriptorKind)
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
                $"runs[{Context.CurrentRunIndex}].tool.{toolComponentPathSegment}.{arrayPropertyName}");
        }

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
