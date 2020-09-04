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
        /// In result messages, use the 'message.id' and 'message.arguments' properties rather than
        /// 'message.text'. This has several advantages. If 'text' is lengthy, using 'id' and 'arguments'
        /// makes the SARIF file smaller. If the rule metadata is stored externally to the SARIF log file,
        /// the message text can be improved (for example, by adding more text, clarifying the phrasing,
        /// or fixing typos), and the result messages will pick up the improvements the next time it is
        /// displayed. Finally, SARIF supports localizing messages into different languages, which is 
        /// possible if the SARIF file contains 'message.id' and 'message.arguments', but not if it contains
        /// 'message.text' directly.
        /// </summary>
        public override MultiformatMessageString FullDescription => new MultiformatMessageString { Text = RuleResources.SARIF2002_ProvideMessageArguments_FullDescription_Text };

        protected override IEnumerable<string> MessageResourceNames => new string[] {
            nameof(RuleResources.SARIF2002_ProvideMessageArguments_Note_Default_Text)
        };

        public override FailureLevel DefaultLevel => FailureLevel.Note;

        protected override void Analyze(Result result, string resultPointer)
        {
            if (string.IsNullOrEmpty(result.Message.Id))
            {
                // {0}: The 'message' property of this result contains a 'text' property. Consider replacing
                // it with 'id' and 'arguments' properties. This potentially reduces the log file size, allows
                // the message text to be improved without modifying the log file, and enables localization.
                LogResult(
                    resultPointer.AtProperty(SarifPropertyName.Message),
                    nameof(RuleResources.SARIF2002_ProvideMessageArguments_Note_Default_Text));
            }
        }
    }
}
