// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class MessageMustBeFlattened : SarifValidationSkimmerBase
    {
        public MessageMustBeFlattened()
        {
            this.DefaultConfiguration.Level = FailureLevel.Error;
        }

        /// <summary>
        /// GH1007
        /// </summary>
        public override string Id => RuleId.MessageMustBeFlattened;

        public override MultiformatMessageString FullDescription => new MultiformatMessageString { Text = RuleResources.GH1007_MessageMustBeFlattened_FullDescription_Text };

        protected override IEnumerable<string> MessageResourceNames => new string[] {
            nameof(RuleResources.GH1007_MessageMustBeFlattened_Error_Default_Text)
        };

        protected override void Analyze(Result result, string resultPointer)
        {
            if (string.IsNullOrEmpty(result.Message.Text))
            {
                // {0}: The 'text' property of this result message is absent. GitHub Advanced Security code
                // scanning will reject this file because it does not support the argumented message now.
                // Try to populate the flattened message text in 'message.text' property.
                LogResult(
                    resultPointer.AtProperty(SarifPropertyName.Message),
                    nameof(RuleResources.GH1007_MessageMustBeFlattened_Error_Default_Text));
            }
        }
    }
}
