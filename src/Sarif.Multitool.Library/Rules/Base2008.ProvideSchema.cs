// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class BaseProvideSchema
        : SarifValidationSkimmerBase
    {
        public override string Id => string.Empty;

        protected override IEnumerable<string> MessageResourceNames => new string[] {
            nameof(RuleResources.SARIF2008_ProvideSchema_Warning_Default_Text)
        };

        public override HashSet<RuleKind> RuleKinds => new HashSet<RuleKind>();

        public override MultiformatMessageString FullDescription => new MultiformatMessageString();

        protected override void Analyze(SarifLog log, string logPointer)
        {
            if (!Context.InputLogToken.HasProperty(SarifPropertyName.Schema))
            {
                // {0}: The SARIF log file does not contain a '$schema' property. Add a '$schema'
                // property that refers to the final, OASIS standard version of the SARIF 2.1.0
                // schema. This enables IDEs to provide Intellisense for SARIF log files.
                LogResult(
                    logPointer,
                    nameof(RuleResources.SARIF2008_ProvideSchema_Warning_Default_Text));
            }
        }
    }
}
