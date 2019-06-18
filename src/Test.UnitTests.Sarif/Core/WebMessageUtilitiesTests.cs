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
        public void WebMessageUtilties_MakeProtocolVersion_SynthesizesProtocolVersionFromProtocolAndVersion(string protocol, string version, string expectedProtocolVersion)
        {
            WebMessageUtilities.MakeProtocolVersion(protocol, version).Should().Be(expectedProtocolVersion);
        }

        [Theory]
        [InlineData("", null, null)]
        [InlineData("1.1", null, null)]
        [InlineData("HTTP/1.1", "HTTP", "1.1")]
        public void WebMessageUtilities_ParseProtocolAndVersion_ParsesProtocolAndVersionFromProtocolVersion(string protocolVersion, string expectedProtocol, string expectedVersion)
        {
            WebMessageUtilities.ParseProtocolAndVersion(protocolVersion, out string protocol, out string version);

            protocol.Should().Be(expectedProtocol);
            version.Should().Be(expectedVersion);
        }

        [Theory]
        [InlineData("GET", true)]
        [InlineData("!#$%&'*+-.^_`|~0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ", true)]
        [InlineData("GET!", true)]
        [InlineData("GET/", false)]
        [InlineData("@GET", false)]
        public void WebMessageUtilties_ValidateMethod_SucceedsAndFailsAsExpected(string method, bool expectedResult)
        {
            WebMessageUtilities.ValidateMethod(method).Should().Be(expectedResult);
        }

        [Theory]
        [InlineData("User-Agent: curl/7.16.3 libcurl/7.16.3 OpenSSL/0.9.7l zlib/1.2.3", true, "User-Agent", "curl/7.16.3 libcurl/7.16.3 OpenSSL/0.9.7l zlib/1.2.3")]
        [InlineData("Host: www.example.com", true, "Host", "www.example.com")]
        [InlineData("Host:www.example.com", true, "Host", "www.example.com")]          // No leading whitespace before field value.
        [InlineData("Host: www.example.com  \t  ", true, "Host", "www.example.com")]   // Trailing whitespace after field value.
        [InlineData("H@st: www.example.com", false, null, null)]                       // Invalid field name token.
        public void WebMessageUtilities_ParseHeader_SucceedsAndFailsAsExpected(string header, bool expectedResult, string expectedName, string expectedValue)
        {
            WebMessageUtilities.ParseHeaderLine(header, out string name, out string value).Should().Be(expectedResult);
            name.Should().Be(expectedName);
            value.Should().Be(expectedValue);
        }
    }
}
