// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class BaseProvideRequiredLocationProperties
        : SarifValidationSkimmerBase
    {
        public override string Id => string.Empty;

        public override MultiformatMessageString FullDescription => new MultiformatMessageString();

        protected override void Analyze(Location location, string locationPointer)
        {
            if (location.PhysicalLocation == null)
            {
                // {0}: This 'location' object does not provide a 'physicalLocation' object. This property is required by the {1} service.
                LogResult(
                    locationPointer,
                    nameof(RuleResources.Base1016_ProvideRequiredLocationProperties_Error_MissingPhysicalLocation_Text),
                    this.ServiceName);
            }
        }
    }
}
