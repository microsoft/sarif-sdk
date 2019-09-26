// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        
// license. See LICENSE file in the project root for full license information. 

using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class RoundTrippingTests
    {
        private static ResourceExtractor s_extractor = new ResourceExtractor(typeof(RoundTrippingTests));

        private static string GetResourceContents(string resourceName)
            => s_extractor.GetResourceText($"RoundTripping.{resourceName}");

        [Fact]
        public void SarifLog_CanBeRoundTripped()
        {
            string originalContents = GetResourceContents("RoundTripping.sarif");

            SarifLog log = JsonConvert.DeserializeObject<SarifLog>(originalContents);

            string roundTrippedContents = JsonConvert.SerializeObject(log, Formatting.Indented);

            roundTrippedContents.Should().Be(originalContents);
        }
    }
}
