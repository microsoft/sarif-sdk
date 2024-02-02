// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class ADOProvideRequiredLocationProperties
        : SarifValidationSkimmerBase
    {
        public override string Id => RuleId.ADOProvideRequiredLocationProperties;

        protected override RuleKinds Kinds => RuleKinds.Ado;

        public override MultiformatMessageString FullDescription => new MultiformatMessageString();

        public ADOProvideRequiredLocationProperties()
        {
            this.DefaultConfiguration.Level = FailureLevel.Error;
        }

        protected override void Analyze(Location location, string locationPointer)
        {
            base.Analyze(location, locationPointer);
        }
    }
}
