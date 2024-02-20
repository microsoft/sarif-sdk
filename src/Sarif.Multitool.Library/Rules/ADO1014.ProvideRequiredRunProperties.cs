﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class AdoProvideRequiredRunProperties
        : BaseProvideRequiredRunProperties
    {
        /// <summary>
        /// ADO1014
        /// </summary>
        public override string Id => RuleId.ADOProvideRequiredRunProperties;

        public override MultiformatMessageString FullDescription => new MultiformatMessageString() { Text = RuleResources.ADO1014_ProvideRequiredRunProperties_FullDescription_Text };

        public override HashSet<RuleKind> RuleKinds => new HashSet<RuleKind>(new[] { RuleKind.Ado });

        protected override string ServiceName => RuleResources.ServiceName_ADO;

        public AdoProvideRequiredRunProperties()
        {
            this.DefaultConfiguration.Level = FailureLevel.Error;
            this.MessageResourceNames.Add(nameof(RuleResources.ADO1014_AdoProvideRequiredRunProperties_Error_MissingAutomationDetails_Text));
            this.MessageResourceNames.Add(nameof(RuleResources.ADO1014_AdoProvideRequiredRunProperties_Error_MissingAutomationDetailsId_Text));
        }

        protected override void Analyze(Run run, string runPointer)
        {
            // run.results is chcked by the base class.
            base.Analyze(run, runPointer);

            if (run != null)
            {
                if (run.AutomationDetails == null)
                {
                    // {0}: This 'run' object does not provide an 'automationDetails' property. This property is required by the {1} service.
                    LogResult(
                        runPointer,
                        nameof(RuleResources.ADO1014_AdoProvideRequiredRunProperties_Error_MissingAutomationDetails_Text));
                }
                else if (string.IsNullOrWhiteSpace(run.AutomationDetails.Id))
                {
                    // {0}: This 'run' object's 'automationDetails' object does not provide an 'id' value. This property is required by the {1} service.
                    LogResult(
                        runPointer,
                        nameof(RuleResources.ADO1014_AdoProvideRequiredRunProperties_Error_MissingAutomationDetailsId_Text));
                }
            }
        }
    }
}
