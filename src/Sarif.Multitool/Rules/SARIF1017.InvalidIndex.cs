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

        protected override void Analyze(LogicalLocation logicalLocation, string logicalLocationPointer)
        {
            if (logicalLocation.Index >= 0)
            {
                IList<LogicalLocation> logicalLocations = Context.CurrentRun.LogicalLocations;
                if (!IndexIsValid(logicalLocations, logicalLocation.Index))
                {
                    LogResult(
                        logicalLocationPointer,
                        nameof(RuleResources.SARIF1017_Default),
                        "logicalLocation",
                        "index",
                        logicalLocation.Index.ToInvariantString(),
                        "run.logicalLocations",
                        (logicalLocation.Index + 1).ToInvariantString());
                }
            }
        }

        protected override void Analyze(ArtifactLocation artifactLocation, string artifactLocationPointer)
        {
            IList<Artifact> artifacts = Context.CurrentRun.Artifacts;
            if (!IndexIsValid(artifacts, artifactLocation.Index))
            {
                LogResult(
                    artifactLocationPointer,
                    nameof(RuleResources.SARIF1017_Default),
                    "artifactLocation",
                    "index",
                    artifactLocation.Index.ToInvariantString(),
                    "run.artifacts",
                    (artifactLocation.Index + 1).ToInvariantString());
            }
        }

        private static bool IndexIsValid<T>(IList<T> container, int index)
            => index == -1 || (index >= 0 && container?.Count >= index);
    }
}
