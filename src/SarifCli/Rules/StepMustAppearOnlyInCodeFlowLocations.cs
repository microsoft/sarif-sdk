// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Json.Pointer;
using Newtonsoft.Json.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Cli.Rules
{
    public class StepMustAppearOnlyInCodeFlowLocations : SarifValidationSkimmerBase
    {
        public override string FullDescription => RuleResources.SARIF010_StepMustAppearOnlyInCodeFlowLocations;

        public override ResultLevel DefaultLevel => ResultLevel.Warning;

        /// <summary>
        /// SARIF010
        /// </summary>
        public override string Id => RuleId.StepMustAppearOnlyInCodeFlowLocations;

        protected override IEnumerable<string> FormatIds
        {
            get
            {
                return new string[]
                {
                    nameof(RuleResources.SARIF010_Default)
                };
            }
        }

        protected override void Analyze(AnnotatedCodeLocation annotatedCodeLocation, string annotatedCodeLocationPointer)
        {
            var pointer = new JsonPointer(annotatedCodeLocationPointer);
            JToken token = pointer.Evaluate(Context.InputLogToken);

            if (token.HasProperty(SarifPropertyName.Step))
            {
                if (!annotatedCodeLocationPointer.Contains(SarifPropertyName.CodeFlows))
                {
                    string stepPointer = annotatedCodeLocationPointer.AtProperty(SarifPropertyName.Step);

                    LogResult(stepPointer, nameof(RuleResources.SARIF010_Default));
                }
            }
        }
    }
}
