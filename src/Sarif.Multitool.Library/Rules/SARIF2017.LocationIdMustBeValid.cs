// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class LocationIdMustBeValid : SarifValidationSkimmerBase
    {
        public LocationIdMustBeValid()
        {
            this.DefaultConfiguration.Level = FailureLevel.Error;
        }

        /// <summary>
        /// SARIF2017
        /// </summary>
        public override string Id => RuleId.LocationIdMustBeValid;

        /// <summary>
        /// Location Id should be a non-negative 32bit integer. 
        /// See the SARIF specification ([3.28.2](https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html#_Toc34317672)).
        /// </summary>
        public override MultiformatMessageString FullDescription => new MultiformatMessageString { Text = RuleResources.SARIF2017_LocationIdMustBeValid_FullDescription_Text };

        protected override IEnumerable<string> MessageResourceNames => new string[] {
            nameof(RuleResources.SARIF2017_LocationIdMustBeValid_Error_Default_Text)
        };

        protected override void Analyze(Location location, string locationPointer)
        {
            if (location.Id < -1 ||
                location.Id > int.MaxValue)
            {
                // {0}: location is specified with invalid Id '{1}'.
                // Location Id should be a non-negative 32bit integer.
                // See the SARIF specification ([3.28.2](https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html#_Toc34317672)).
                LogResult(
                    locationPointer,
                    nameof(RuleResources.SARIF2017_LocationIdMustBeValid_Error_Default_Text),
                    location.Id.ToString());
            }
        }
    }
}
