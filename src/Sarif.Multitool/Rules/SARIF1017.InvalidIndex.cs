// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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
                "index",
                "run.addresses");

            ValidateArrayIndex(
                address.ParentIndex,
                Context.CurrentRun.Addresses,
                addressPointer,
                "address",
                "parentIndex",
                "run.addresses");
        }

        protected override void Analyze(Artifact artifact, string artifactPointer)
        {
            ValidateArrayIndex(
                artifact.ParentIndex,
                Context.CurrentRun.Artifacts,
                artifactPointer,
                "artifact",
                "parentIndex",
                "run.artifacts");
        }

        protected override void Analyze(ArtifactLocation artifactLocation, string artifactLocationPointer)
        {
            ValidateArrayIndex(
                artifactLocation.Index,
                Context.CurrentRun.Artifacts,
                artifactLocationPointer,
                "artifactLocation",
                "index",
                "run.artifacts");
        }

        protected override void Analyze(GraphTraversal graphTraversal, string graphTraversalPointer)
        {
            ValidateArrayIndex(
                graphTraversal.RunGraphIndex,
                Context.CurrentRun.Graphs,
                graphTraversalPointer,
                "graphTraversal",
                "runGraphIndex",
                "run.graphTraversals");

            ValidateArrayIndex(
                graphTraversal.ResultGraphIndex,
                Context.CurrentResult.Graphs,
                graphTraversalPointer,
                "graphTraversal",
                "resultGraphIndex",
                $"run.results[{Context.CurrentResultIndex}].graphTraversals");
        }

        protected override void Analyze(LogicalLocation logicalLocation, string logicalLocationPointer)
        {
            ValidateArrayIndex(
                logicalLocation.Index,
                Context.CurrentRun.LogicalLocations,
                logicalLocationPointer,
                "logicalLocation",
                "index",
                "run.logicalLocations");
        }

        protected override void Analyze(Result result, string resultPointer)
        {
            ValidateArrayIndex(
                result.RuleIndex,
                Context.CurrentRun.Tool.Driver.Rules,
                resultPointer,
                "result",
                "ruleIndex",
                "run.tool.driver.rules");
        }

        protected override void Analyze(ThreadFlowLocation threadFlowLocation, string threadFlowLocationPointer)
        {
            ValidateArrayIndex(
                threadFlowLocation.Index,
                Context.CurrentRun.ThreadFlowLocations,
                threadFlowLocationPointer,
                "threadFlowLocation",
                "index",
                "run.threadFlowLocations");
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
