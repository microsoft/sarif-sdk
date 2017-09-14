﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Microsoft.Json.Pointer;
using Newtonsoft.Json.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class AnnotatedCodeLocationEssentialIsObsolete : SarifValidationSkimmerBase
    {
        public override string FullDescription => RuleResources.SARIF005_AnnotatedCodeLocationEssentialIsObsolete;

        public override ResultLevel DefaultLevel => ResultLevel.Warning;

        /// <summary>
        /// SARIF005
        /// </summary>
        public override string Id => RuleId.AnnotatedCodeLocationEssentialIsObsolete;

        protected override IEnumerable<string> FormatIds
        {
            get
            {
                return new string[]
                {
                    nameof(RuleResources.SARIF005_Default),
                    nameof(RuleResources.SARIF005_OnlyInCodeFlow)
                };
            }
        }

        protected override void Analyze(AnnotatedCodeLocation annotatedCodeLocation, string annotatedCodeLocationPointer)
        {
            var pointer = new JsonPointer(annotatedCodeLocationPointer);
            JToken token = pointer.Evaluate(Context.InputLogToken);

            if (token.HasProperty(SarifPropertyName.Essential))
            {
                string essentialPointer = annotatedCodeLocationPointer.AtProperty(SarifPropertyName.Essential);

                // Emit a different warning depending on whether or not this annotatedCodeLocation
                // occurs in a codeFlow, because the "importance" property is only meaningful within
                // a codeFlow.
                string formatId = annotatedCodeLocationPointer.Contains(SarifPropertyName.CodeFlows)
                    ? nameof(RuleResources.SARIF005_Default)
                    : nameof(RuleResources.SARIF005_OnlyInCodeFlow);

                LogResult(essentialPointer, formatId);
            }
        }
    }
}
