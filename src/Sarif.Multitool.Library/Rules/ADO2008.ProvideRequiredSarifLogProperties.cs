﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class AdoProvideSchem
        : BaseProvideSchema
    {
        public override string Id => RuleId.ADOProvideSchema;

        public override HashSet<RuleKind> RuleKinds => new HashSet<RuleKind>(new[] { RuleKind.Ado });

        protected override string ServiceName => RuleResources.ServiceName_ADO;

        public AdoProvideSchem()
        {
            this.DefaultConfiguration.Level = FailureLevel.Error;
        }

        protected override void Analyze(SarifLog log, string logPointer)
        {
            base.Analyze(log, logPointer);
        }
    }
}
