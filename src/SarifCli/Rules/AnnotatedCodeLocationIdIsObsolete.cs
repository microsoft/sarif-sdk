// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Cli.Rules
{
    public class AnnotatedCodeLocationIdIsObsolete : SarifValidationSkimmerBase
    {
        public override string FullDescription => RuleResources.SARIF004_AnnotatedCodeLocationIdIsObsolete;

        public override ResultLevel DefaultLevel => ResultLevel.Warning;

        /// <summary>
        /// SARIF004
        /// </summary>
        public override string Id => RuleId.AnnotatedCodeLocationIdIsObsolete;

        protected override IEnumerable<string> FormatIds
        {
            get
            {
                return new string[]
                {
                    nameof(RuleResources.SARIF004_Default),
                    nameof(RuleResources.SARIF004_OnlyInCodeFlow)
                };
            }
        }

        protected override void Analyze(AnnotatedCodeLocation annotatedCodeLocation, string annotatedCodeLocationPointer)
        {
            if (annotatedCodeLocation.Id != 0)
            {
                string idPointer = annotatedCodeLocationPointer.AtProperty(SarifPropertyName.Id);

                // Emit a different warning depending on whether or not this annotatedCodeLocation
                // occurs in a codeFlow, because the "step" property is only meaningful within
                // a codeFlow.
                string formatId = annotatedCodeLocationPointer.Contains(SarifPropertyName.CodeFlows)
                    ? nameof(RuleResources.SARIF004_Default)
                    : nameof(RuleResources.SARIF004_OnlyInCodeFlow);

                LogResult(idPointer, formatId);
            }
        }
    }
}
