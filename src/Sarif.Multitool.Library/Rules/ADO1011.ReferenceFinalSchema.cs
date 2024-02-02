// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class AdoReferenceFinalSchema
        : BaseReferenceFinalSchema
    {
        /// <summary>
        /// ADO1011
        /// </summary>
        public override string Id => RuleId.ADOReferenceFinalSchema;

        protected override RuleKinds Kinds => RuleKinds.Ado;

        protected override string ServiceName => RuleResources.ServiceName_ADO;

        public AdoReferenceFinalSchema()
        {
            this.DefaultConfiguration.Level = FailureLevel.Error;
        }

        protected override void Analyze(SarifLog log, string logPointer)
        {
            base.Analyze(log, logPointer);
        }
    }
}
