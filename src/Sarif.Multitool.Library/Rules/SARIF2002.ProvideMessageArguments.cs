// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class ProvideMessageArguments : SarifValidationSkimmerBase
    {
        public ProvideMessageArguments()
        {
            this.DefaultConfiguration.Level = FailureLevel.Note;
        }

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

        protected override ICollection<string> MessageResourceNames => new List<string> {
            nameof(RuleResources.SARIF2002_ProvideMessageArguments_Note_Default_Text)
        };

        private Run run;

        protected override void Analyze(Run run, string runPointer)
        {
            this.run = run;
        }

        protected override void Analyze(Result result, string resultPointer)
        {
            // Applicability: this note suggests replacing message.text with message.id + arguments.
            // That advice only makes sense if the result's rule actually defines messageStrings to
            // reference. Tools that emit bespoke per-result prose (no messageStrings catalog) have
            // nothing for 'id' to point at — skip rather than emit an unactionable note.
            //
            // GetRule can NRE on degenerate logs (tool.driver absent, bad ruleIndex, etc.). Those
            // shapes are other rules' job to flag; treat as no-descriptor here.
            ReportingDescriptor rule = null;
            try { rule = result.GetRule(this.run); } catch { /* fall through with rule == null */ }
            if (rule?.MessageStrings == null || rule.MessageStrings.Count == 0)
            {
                return;
            }

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
