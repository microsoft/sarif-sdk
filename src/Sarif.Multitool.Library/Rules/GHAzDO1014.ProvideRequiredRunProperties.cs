// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class GHAzDOProvideRequiredRunProperties
        : BaseProvideRequiredRunProperties
    {
        /// <summary>
        /// GHAzDO1014
        /// </summary>
        public override string Id => RuleId.GHAzDOProvideRequiredRunProperties;

        public override MultiformatMessageString FullDescription => new MultiformatMessageString() { Text = RuleResources.GHAzDO1014_ProvideRequiredRunProperties_FullDescription_Text };

        private readonly List<string> _messageResourceNames = new List<string>()
        {
            nameof(RuleResources.GHAzDO1014_GHAzDOProvideRequiredRunProperties_Error_MissingAutomationDetails_Text),
            nameof(RuleResources.GHAzDO1014_GHAzDOProvideRequiredRunProperties_Error_MissingAutomationDetailsId_Text)
        };

        protected override ICollection<string> MessageResourceNames => _messageResourceNames.Concat(BaseMessageResourceNames).ToList();

        public override HashSet<RuleKind> RuleKinds => new HashSet<RuleKind>(new[] { RuleKind.GHAzDO });

        protected override string ServiceName => RuleResources.ServiceName_GHAzDO;

        public GHAzDOProvideRequiredRunProperties()
        {
            this.DefaultConfiguration.Level = FailureLevel.Error;
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
                        nameof(RuleResources.GHAzDO1014_GHAzDOProvideRequiredRunProperties_Error_MissingAutomationDetails_Text));
                }
                else if (string.IsNullOrWhiteSpace(run.AutomationDetails.Id))
                {
                    // {0}: This 'run' object's 'automationDetails' object does not provide an 'id' value. This property is required by the {1} service.
                    LogResult(
                        runPointer,
                        nameof(RuleResources.GHAzDO1014_GHAzDOProvideRequiredRunProperties_Error_MissingAutomationDetailsId_Text));
                }
            }
        }
    }
}
