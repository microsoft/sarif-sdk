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
        [InlineData("", false, null, null, null, null, null, -1, "request line is empty")]
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
            string because)
        {
            bool result = WebMessageUtilities.ParseRequestLine(
                requestLine,
                out string method,
                out string target,
                out string httpVersion,
                out string protocol,
                out string version,
                out int length);

            result.Should().Be(expectedResult, because);
            method.Should().Be(expectedMethod, because);
            target.Should().Be(expectedTarget, because);
            httpVersion.Should().Be(expectedHttpVersion, because);
            protocol.Should().Be(expectedProtocol, because);
            version.Should().Be(expectedVersion, because);
            length.Should().Be(expectedLength, because);
        }

        [Theory]
        [InlineData("", false, null, null, null, -1, null, -1, "status line is empty")]
        [InlineData("HTTP/1.1", false, null, null, null, -1, null, -1, "status code is absent")]
        [InlineData("HTTP/1.1 200", false, null, null, null, -1, null, -1, "reason phrase is absent")]
        [InlineData("HTTP/1.1 200 OK", false, null, null, null, -1, null, -1, "status line does not end in CRLF")]
        [InlineData("HTTP2/1.1 200 OK\r\n", false, null, null, null, -1, null, -1, "HTTP version has invalid name")]
        [InlineData("HTTP/1..1 200 OK\r\n", false, null, null, null, -1, null, -1, "HTTP version has invalid version number")]
        [InlineData("HTTP/1.1 200a OK\r\n", false, null, null, null, -1, null, -1, "status code is not an integer")]
        [InlineData("HTTP/1.1 20 OK\r\n", false, null, null, null, -1, null, -1, "status code has fewer than three digits")]
        [InlineData("HTTP/1.1 2000 OK\r\n", false, null, null, null, -1, null, -1, "status code has more than three digits")]
        [InlineData("HTTP/1.1 200 OK\r\n", true, "HTTP/1.1", "HTTP", "1.1", 200, "OK", 17, "status line is valid")]
        public void WebMessageUtilities_ParseStatusLine(
            string statusLine,
            bool expectedResult,
            string expectedHttpVersion,
            string expectedProtocol,
            string expectedVersion,
            int expectedStatusCode,
            string expectedReasonPhrase,
            int expectedLength,
            string because)
        {
            bool result = WebMessageUtilities.ParseStatusLine(
                statusLine,
                out string httpVersion,
                out string protocol,
                out string version,
                out int statusCode,
                out string reasonPhrase,
                out int length);

            result.Should().Be(expectedResult, because);
            httpVersion.Should().Be(expectedHttpVersion, because);
            protocol.Should().Be(expectedProtocol, because);
            version.Should().Be(expectedVersion, because);
            statusCode.Should().Be(expectedStatusCode, because);
            reasonPhrase.Should().Be(expectedReasonPhrase, because);
            length.Should().Be(expectedLength, because);
        }

        [Theory]
        [InlineData("User-Agent: curl/7.16.3 libcurl/7.16.3 OpenSSL/0.9.7l zlib/1.2.3\r\n", true, "User-Agent", "curl/7.16.3 libcurl/7.16.3 OpenSSL/0.9.7l zlib/1.2.3")]
        [InlineData("Host: www.example.com\r\n", true, "Host", "www.example.com")]
        [InlineData("Host:www.example.com\r\n", true, "Host", "www.example.com")]          // No leading whitespace before field value.
        [InlineData("Host: www.example.com  \t  \r\n", true, "Host", "www.example.com")]   // Trailing whitespace after field value.
        [InlineData("H@st: www.example.com\r\n", false, null, null)]                       // Invalid field name token.
        public void WebMessageUtilities_ParseHeader(string header, bool expectedResult, string expectedName, string expectedValue)
        {
            WebMessageUtilities.ParseHeaderLine(header, out string name, out string value, out int totalHeaderLinesLength).Should().Be(expectedResult);
            name.Should().Be(expectedName);
            value.Should().Be(expectedValue);
        }
    }
}
