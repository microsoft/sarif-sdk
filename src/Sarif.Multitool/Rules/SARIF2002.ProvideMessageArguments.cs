// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class ProvideMessageArguments : SarifValidationSkimmerBase
    {
        /// <summary>
        /// SARIF2002
        /// </summary>
        public override string Id => RuleId.ProvideMessageArguments;

        /// <summary>
        /// Placeholder
        /// </summary>
        public override MultiformatMessageString FullDescription => new MultiformatMessageString { Text = RuleResources.SARIF2002_ProvideMessageArguments_FullDescription_Text };

        protected override IEnumerable<string> MessageResourceNames => new string[] {
            nameof(RuleResources.SARIF2002_ProvideMessageArguments_Warning_Default_Text)
        };

        public override FailureLevel DefaultLevel => FailureLevel.Warning;

        protected override void Analyze(Result result, string resultPointer)
        {
            if (string.IsNullOrEmpty(result.Message.Id))
            {
                // {0}: Placeholder
                LogResult(
                    resultPointer.AtProperty(SarifPropertyName.Message),
                    nameof(RuleResources.SARIF2002_ProvideMessageArguments_Warning_Default_Text));
            }
        }
    }
}
