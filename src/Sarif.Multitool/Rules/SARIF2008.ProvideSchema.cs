// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class ProvideSchema : SarifValidationSkimmerBase
    {
        /// <summary>
        /// SARIF2008
        /// </summary>
        public override string Id => RuleId.ProvideSchema;

        /// <summary>
        /// A SARIF log file should contain, on the root object, a '$schema' property that refers to
        /// the final, OASIS standard version of the SARIF 2.1.0 schema. This enables IDEs to provide
        /// Intellisense for SARIF log files.
        /// </summary>
        public override MultiformatMessageString FullDescription => new MultiformatMessageString { Text = RuleResources.SARIF2008_ProvideSchema_FullDescription_Text };

        protected override IEnumerable<string> MessageResourceNames => new string[] {
            nameof(RuleResources.SARIF2008_ProvideSchema_Warning_Default_Text)
        };

        public override FailureLevel DefaultLevel => FailureLevel.Warning;

        protected override void Analyze(SarifLog log, string logPointer)
        {
            if (!Context.InputLogToken.HasProperty("$schema"))
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
