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
        [InlineData("", false, null, null, null, null, null, -1, "request is empty")]
        [InlineData("GET", false, null, null, null, null, null, -1, "target is absent")]
        [InlineData("GET /hello.txt", false, null, null, null, null, null, -1, "HTTP version is absent")]
        [InlineData("GET /hello.txt HTTP/1.1", false, null, null, null, null, null, -1, "request line does not end in CRLF")]
        [InlineData("GET /hello.txt HTTP2/1.1\r\n", false, null, null, null, null, null, -1, "HTTP version has invalid name")]
        [InlineData("GET /hello.txt HTTP/1..1\r\n", false, null, null, null, null, null, -1, "HTTP version has invalid version number")]
        [InlineData("GET /hello.txt HTTP/1.1\r\n", true, "GET", "/hello.txt", "HTTP/1.1", "HTTP", "1.1", 25, "request line is valid")]
        public void WebMessageUtilities_ParseRequestLine(
            string requestLine,
            bool expectedResult,
            string expectedMethod,
            string expectedTarget,
            string expectedHttpVersion,
            string expectedProtocol,
            string expectedVersion,
            int expectedLength,
            string reason)
        {
            bool result = WebMessageUtilities.ParseRequestLine(
                requestLine,
                out string method,
                out string target,
                out string httpVersion,
                out string protocol,
                out string version,
                out int length);

            result.Should().Be(expectedResult, reason);
            method.Should().Be(expectedMethod, reason);
            target.Should().Be(expectedTarget, reason);
            httpVersion.Should().Be(expectedHttpVersion, reason);
            protocol.Should().Be(expectedProtocol, reason);
            version.Should().Be(expectedVersion, reason);
            length.Should().Be(expectedLength, reason);
        }

        [Theory]
        [InlineData("User-Agent: curl/7.16.3 libcurl/7.16.3 OpenSSL/0.9.7l zlib/1.2.3", true, "User-Agent", "curl/7.16.3 libcurl/7.16.3 OpenSSL/0.9.7l zlib/1.2.3")]
        [InlineData("Host: www.example.com", true, "Host", "www.example.com")]
        [InlineData("Host:www.example.com", true, "Host", "www.example.com")]          // No leading whitespace before field value.
        [InlineData("Host: www.example.com  \t  ", true, "Host", "www.example.com")]   // Trailing whitespace after field value.
        [InlineData("H@st: www.example.com", false, null, null)]                       // Invalid field name token.
        public void WebMessageUtilities_ParseHeader(string header, bool expectedResult, string expectedName, string expectedValue)
        {
            WebMessageUtilities.ParseHeaderLine(header, out string name, out string value).Should().Be(expectedResult);
            name.Should().Be(expectedName);
            value.Should().Be(expectedValue);
        }
    }
}
