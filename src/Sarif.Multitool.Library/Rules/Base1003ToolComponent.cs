// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class Base1003ProvideToolDriver : SarifValidationSkimmerBase
    {
        protected override void Analyze(Run run, string runPointer)
        {
            if (run.Tool == null)
            {
                // {0}: The 'tool' object in this run does not provide a value.
                LogResult(
                    runPointer,
                    nameof(RuleResources.Base1003_ProvideTool_Note_Default_Text));
            }
            else
            {
                Analyze(run.Tool.Driver, runPointer
                    .AtProperty(SarifPropertyName.Tool)
                    .AtProperty(SarifPropertyName.Driver));
            }
        }

        protected override void Analyze(ToolComponent toolComponent, string toolComponentPointer)
        {
            if (toolComponent == null)
            {
                // {0}: The 'tool' object in this run does not provide a 'driver' value.
                LogResult(
                    toolComponentPointer,
                    nameof(RuleResources.Base1003_ProvideDriver_Note_Default_Text));
            }
            else
            {
                AnalyzeToolDriver(toolComponent, toolComponentPointer);
            }
        }

        private void AnalyzeToolDriver(ToolComponent toolComponent, string toolDriverPointer)
        {
            if (string.IsNullOrEmpty(toolComponent.Name))
            {
                // {0}: The 'tool' object in this run does not provide a 'name' value.
                LogResult(
                    toolComponentPointer,
                    nameof(RuleResources.Base1003_ProvideName_Note_Default_Text));
            }

            if (toolComponent.Rules == null)
            {
                // {0}: The 'tool' object in this run does not provide a 'rules' value.
                LogResult(
                    toolComponentPointer,
                    nameof(RuleResources.Base1003_ProvideRules_Note_Default_Text));
            }

            //bool hasVersion = false;
            //if (!string.IsNullOrEmpty(toolComponent.Version))
            //{
            //    string firstChar = toolComponent.Version.Substring(0, 1);
            //    if (int.TryParse(firstChar, out int _))
            //    {
            //        hasVersion = true;
            //    }
            //    else
            //    {
            //        // {0}: The 'toolComponent' object in this run does provide a Version value which does not begin with a number.
            //        LogResult(
            //            toolComponentPointer,
            //            "Need a resources string");//nameof(RuleResources.SARIF2010_ProvideCodeSnippets_Note_Default_Text));
            //    }
            //}

            //if (!hasVersion && !string.IsNullOrEmpty(toolComponent.SemanticVersion))
            //{
            //    hasVersion = true;
            //}

            //if (!hasVersion)
            //{
            //    // {0}: The 'toolComponent' object in this run does not provide a Version nor SemanticVersion value.
            //    LogResult(
            //        toolComponentPointer,
            //        "Need a resources string");//nameof(RuleResources.SARIF2010_ProvideCodeSnippets_Note_Default_Text));
            //}
        }
    }
}
