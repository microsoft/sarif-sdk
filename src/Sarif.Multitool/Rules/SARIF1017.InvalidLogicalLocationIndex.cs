// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class InvalidLogicalLocationIndex : SarifValidationSkimmerBase
    {
        private readonly MultiformatMessageString _fullDescription = new MultiformatMessageString
        {
            Text = RuleResources.SARIF1017_InvalidLogicalLocationIndex
        };

        public override MultiformatMessageString FullDescription => _fullDescription;

        public override FailureLevel DefaultLevel => FailureLevel.Error;

        /// <summary>
        /// SARIF1017
        /// </summary>
        public override string Id => RuleId.InvalidLogicalLocationIndex;

        protected override IEnumerable<string> MessageResourceNames => new string[]
        {
            nameof(RuleResources.SARIF1017_Default)
        };

        protected override void Analyze(LogicalLocation logicalLocation, string logicalLocationPointer)
        {
            if (logicalLocation.Index >= 0)
            {
                IList<LogicalLocation> logicalLocations = Context.CurrentRun.LogicalLocations;
                if (logicalLocations == null || logicalLocations.Count <= logicalLocation.Index)
                {
                    LogResult(logicalLocationPointer, nameof(RuleResources.SARIF1017_Default), logicalLocation.Index, logicalLocation.Index + 1);
                }
            }
        }
    }
}
