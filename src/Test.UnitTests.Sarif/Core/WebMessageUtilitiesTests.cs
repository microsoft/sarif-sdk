// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif;
using Xunit;

namespace Microsoft.CodeAnalysis.Test.UnitTests.Sarif.Core
{
    public class WebMessageUtilitiesTests
    {
        [Theory]
        [InlineData(null, null, "/")]
        [InlineData("HTTP", "", "HTTP/")]
        [InlineData("", "1.1", "/1.1")]
        [InlineData("HTTP", "1.1", "HTTP/1.1")]
        public void WebResponse_SynthesizesProtocolVersionFromProtocolAndVersion(string protocol, string version, string expectedProtocolVersion)
        {
            WebMessageUtilities.MakeProtocolVersion(protocol, version).Should().Be(expectedProtocolVersion);
        }
    }
}
