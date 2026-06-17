// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class GHAzDOProvideRequiredPhysicalLocationProperties
        : BaseProvideRequiredResultProperties
    {
        /// <summary>
        /// GHAzDO1017
        /// </summary>
        public override string Id => RuleId.GHAzDOProvideRequiredPhysicalLocationProperties;

        public override MultiformatMessageString FullDescription => new MultiformatMessageString() { Text = RuleResources.GHAzDO1017_ProvideRequiredPhysicalLocationProperties_FullDescription_Text };

        private readonly List<string> _messageResourceNames = new List<string>();

        protected override ICollection<string> MessageResourceNames => _messageResourceNames.Concat(BaseMessageResourceNames).ToList();

        public override HashSet<RuleKind> RuleKinds => new HashSet<RuleKind>(new[] { RuleKind.GHAzDO });

        protected override string ServiceName => RuleResources.ServiceName_GHAzDO;

        public GHAzDOProvideRequiredPhysicalLocationProperties()
        {
            this.DefaultConfiguration.Level = FailureLevel.Error;
        }

        protected override void Analyze(PhysicalLocation physicalLocation, string physicalLocationPointer)
        {
            base.Analyze(physicalLocation, physicalLocationPointer);
        }
    }
}
