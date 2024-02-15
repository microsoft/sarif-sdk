// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class GhasReferenceFinalSchema
        : BaseReferenceFinalSchema
    {
        /// <summary>
        /// GH1011
        /// </summary>
        public override string Id => RuleId.GHASReferenceFinalSchema;

        public override MultiformatMessageString FullDescription => new MultiformatMessageString() { Text = RuleResources.GH1011_ReferenceFinalSchema };

        public override HashSet<RuleKind> RuleKinds => new([RuleKind.Ghas]);

        protected override string ServiceName => RuleResources.ServiceName_GHAS;

        public GhasReferenceFinalSchema()
        {
            this.DefaultConfiguration.Level = FailureLevel.Error;
        }

        protected override void Analyze(SarifLog log, string logPointer)
        {
            base.Analyze(log, logPointer);
        }
    }
}
