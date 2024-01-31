// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class ADOProvideSchem : BaseProvideRun
    {
        public override string Id => RuleId.ADOProvideSchema;

        protected override string ServiceName => RuleResources.ServiceName_ADO;

        protected override IEnumerable<string> MessageResourceNames => new string[] {
            nameof(RuleResources.SARIF2008_ProvideSchema_Warning_Default_Text)
        };

        public ADOProvideSchem()
        {
            this.DefaultConfiguration.Level = FailureLevel.Error;
        }

        protected override void Analyze(SarifLog log, string logPointer)
        {
            base.Analyze(log, logPointer);
        }
    }
}
