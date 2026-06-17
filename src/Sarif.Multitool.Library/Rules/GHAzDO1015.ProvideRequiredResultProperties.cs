// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class GHAzDOProvideRequiredResultProperties
        : BaseProvideRequiredResultProperties
    {
        /// <summary>
        /// GHAzDO1015
        /// </summary>
        public override string Id => RuleId.GHAzDOProvideRequiredResultProperties;

        public override MultiformatMessageString FullDescription => new MultiformatMessageString() { Text = RuleResources.GHAzDO1015_ProvideRequiredResultProperties_FullDescription_Text };

        private readonly List<string> _messageResourceNames = new List<string>()
        {
            nameof(RuleResources.GHAzDO1015_ProvideRequiredResultProperties_Error_MissingRuleId_Text)
        };

        protected override ICollection<string> MessageResourceNames => _messageResourceNames.Concat(BaseMessageResourceNames).ToList();

        public override HashSet<RuleKind> RuleKinds => new(new[] { RuleKind.GHAzDO });

        protected override string ServiceName => RuleResources.ServiceName_GHAzDO;

        public GHAzDOProvideRequiredResultProperties()
        {
            this.DefaultConfiguration.Level = FailureLevel.Error;
        }

        protected override void Analyze(Result result, string resultPointer)
        {
            base.Analyze(result, resultPointer);

            if (string.IsNullOrWhiteSpace(result.RuleId))
            {
                // {0}: This 'result' object does not provide a 'ruleId' value. This property is required by the {1} service.
                LogResult(
                    resultPointer,
                    nameof(RuleResources.GHAzDO1015_ProvideRequiredResultProperties_Error_MissingRuleId_Text));
            }
        }
    }
}
