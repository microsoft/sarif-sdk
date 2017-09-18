// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Json.Pointer;
using Newtonsoft.Json.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class ImportanceMustAppearOnlyInCodeFlowLocations : SarifValidationSkimmerBase
    {
        public override string FullDescription => RuleResources.SARIF011_ImportanceMustAppearOnlyInCodeFlowLocations;

        public override ResultLevel DefaultLevel => ResultLevel.Warning;

        /// <summary>
        /// SARIF011
        /// </summary>
        public override string Id => RuleId.ImportanceMustAppearOnlyInCodeFlowLocations;

        protected override IEnumerable<string> FormatIds
        {
            get
            {
                return new string[]
                {
                    nameof(RuleResources.SARIF011_Default)
                };
            }
        }

        protected override void Analyze(AnnotatedCodeLocation annotatedCodeLocation, string annotatedCodeLocationPointer)
        {
            var pointer = new JsonPointer(annotatedCodeLocationPointer);
            JToken token = pointer.Evaluate(Context.InputLogToken);

            if (token.HasProperty(SarifPropertyName.Importance))
            {
                if (!annotatedCodeLocationPointer.Contains(SarifPropertyName.CodeFlows))
                {
                    string importancePointer = annotatedCodeLocationPointer.AtProperty(SarifPropertyName.Importance);

                    LogResult(importancePointer, nameof(RuleResources.SARIF011_Default));
                }
            }
        }
    }
}
