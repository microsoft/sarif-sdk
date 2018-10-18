// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        
// license. See LICENSE file in the project root for full license information. 

using System;
using System.IO;

using Microsoft.CodeAnalysis.Sarif.Writers;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.UnitTests.Writers
{
    public class PrereleaseCompatibilityTransformerTests
    {
        [Fact]
        public void PrereleaseCompatibilityTransformer_UpgradePrereleaseTwoZeroZero()
        {
            string comprehensiveSarifPath = Path.Combine(Environment.CurrentDirectory, @"v2\ObsoleteFormats\ComprehensivePrereleaseTwoZeroZero.sarif");

            string sarifText = File.ReadAllText(comprehensiveSarifPath);

            sarifText = PrereleaseCompatibilityTransformer.UpdateToCurrentVersion(sarifText);

            // If this obsolete prerelease v2 file deserializes to the current
            // version without raising an exception, this step succeeds.
            SarifLog sarifLog = JsonConvert.DeserializeObject<SarifLog>(sarifText);

            JsonConvert.SerializeObject(sarifLog);            
        }
    }
}
