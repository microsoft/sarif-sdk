// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class GHASProvideRequiredLocationProperties
        : SarifValidationSkimmerBase
    {
        public override string Id => RuleId.GHASProvideRequiredLocationProperties;

        public override MultiformatMessageString FullDescription => new MultiformatMessageString();

        public GHASProvideRequiredLocationProperties()
        {
            this.DefaultConfiguration.Level = FailureLevel.Error;
        }

        protected override void Analyze(Location location, string locationPointer)
        {
            base.Analyze(location, locationPointer);
        }
    }
}
