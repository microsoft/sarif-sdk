// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class IndexPropertiesMustBeConsistentWithArrays : SarifValidationSkimmerBase
    {
        /// <summary>
        /// SARIF1009
        /// </summary>
        public override string Id => RuleId.IndexPropertiesMustBeConsistentWithArrays;

        /// <summary>
        /// If an object contains a property that is used as an array index (an "index-valued
        /// property"), then that array must be present and must contain at least "index + 1"
        /// elements.
        /// </summary>
        public override MultiformatMessageString FullDescription => new MultiformatMessageString { Text = RuleResources.SARIF1009_IndexPropertiesMustBeConsistentWithArrays_FullDescription_Text };

        protected override IEnumerable<string> MessageResourceNames => new string[] {
            nameof(RuleResources.SARIF1009_IndexPropertiesMustBeConsistentWithArrays_Error_TargetArrayMustExist_Text),
            nameof(RuleResources.SARIF1009_IndexPropertiesMustBeConsistentWithArrays_Error_TargetArrayMustBeLongEnough_Text)
        };

        public override FailureLevel DefaultLevel => FailureLevel.Error;

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
            if (index == -1)
            {
                return;
            }

            if (container == null)
            {
                // {0}: This '{1}' object contains a property '{2}' with value {3}, but '{4}' does
                // not exist. An index-valued property always refers to an array, so the array must
                // be present.
                LogResult(
                    jsonPointer,
                    nameof(RuleResources.SARIF1009_IndexPropertiesMustBeConsistentWithArrays_Error_TargetArrayMustExist_Text),
                    objectName,
                    propertyName,
                    index.ToInvariantString(),
                    arrayName);
                return;
            }

            if (!IndexIsValid(index, container))
            {
                // {0}: This '{1}' object contains a property '{2}' with value {3}, but '{4}' has
                // fewer than {5} elements. An index-valued properties must be valid for the array
                // that it refers to.
                LogResult(
                    jsonPointer,
                    nameof(RuleResources.SARIF1009_IndexPropertiesMustBeConsistentWithArrays_Error_TargetArrayMustBeLongEnough_Text),
                    objectName,
                    propertyName,
                    index.ToInvariantString(),
                    arrayName,
                    (index + 1).ToInvariantString());
            }
        }

        private static bool IndexIsValid<T>(int index, IList<T> container)
                => index >= 0 && container.Count > index;
    }
}
