// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif;
using Xunit;

namespace Microsoft.CodeAnalysis.Test.UnitTests.Sarif.Core
{
    public class WebResponseTests
    {
        [Fact]
        public void WebResponse_Parse_CreatesExpectedWebResponseObject()
        {
            // Example from RFC 7230.
            const string ResponseString =
@"HTTP/1.1 200 OK
Date: Mon, 27 Jul 2009 12:28:53 GMT
Server: Apache
Last-Modified: Wed, 22 Jul 2009 19:15:56 GMT
ETag: ""34aa387-d-1568eb00""
Accept-Ranges: bytes
Content-Length: 51
Vary: Accept-Encoding
Content-Type: text/plain

Hello World!My payload includes a trailing CRLF.
";

            WebResponse webResponse = WebResponse.Parse(ResponseString);

            webResponse.Protocol.Should().Be("HTTP");
            webResponse.Version.Should().Be("1.1");
            webResponse.StatusCode.Should().Be(200);
            webResponse.ReasonPhrase.Should().Be("OK");
            webResponse.Headers.Count.Should().Be(8);
            webResponse.Headers["Date"].Should().Be("Mon, 27 Jul 2009 12:28:53 GMT");
            webResponse.Headers["Server"].Should().Be("Apache");
            webResponse.Headers["Last-Modified"].Should().Be("Wed, 22 Jul 2009 19:15:56 GMT");
            webResponse.Headers["ETag"].Should().Be("\"34aa387-d-1568eb00\"");
            webResponse.Headers["Accept-Ranges"].Should().Be("bytes");
            webResponse.Headers["Content-Length"].Should().Be("51");
            webResponse.Headers["Vary"].Should().Be("Accept-Encoding");
            webResponse.Headers["Content-Type"].Should().Be("text/plain");
            webResponse.Body.Text.Should().Be("Hello World!My payload includes a trailing CRLF.\r\n");
        }

        [Fact]
        public void WebResponse_Parse_CreatesExpectedWebResponseObjectWithoutBody()
        {
            const string ResponseString =
@"HTTP/1.1 200 OK
Date: Mon, 27 Jul 2009 12:28:53 GMT
Server: Apache
Last-Modified: Wed, 22 Jul 2009 19:15:56 GMT

";

            WebResponse webResponse = WebResponse.Parse(ResponseString);

            webResponse.Protocol.Should().Be("HTTP");
            webResponse.Version.Should().Be("1.1");
            webResponse.StatusCode.Should().Be(200);
            webResponse.ReasonPhrase.Should().Be("OK");
            webResponse.Headers.Count.Should().Be(3);
            webResponse.Headers["Date"].Should().Be("Mon, 27 Jul 2009 12:28:53 GMT");
            webResponse.Headers["Server"].Should().Be("Apache");
            webResponse.Headers["Last-Modified"].Should().Be("Wed, 22 Jul 2009 19:15:56 GMT");
            webResponse.Body.Should().BeNull();
        }

        [Fact]
        public void WebResponse_Parse_ThrowsIfStatusLineIsInvalid()
        {
            const string ResponseString =
@"HTTP/1.1.1 200 OK
Date: Mon, 27 Jul 2009 12:28:53 GMT
Server: Apache
Last-Modified: Wed, 22 Jul 2009 19:15:56 GMT

";

            Action action = () => WebResponse.Parse(ResponseString);

            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void WebResponse_Parse_ThrowsIfHeaderLineIsInvalid()
        {
            const string ResponseString =
@"HTTP/1.1 200 OK
Date: Mon, 27 Jul 2009 12:28:53 GMT
Server NO COLON Apache
Last-Modified: Wed, 22 Jul 2009 19:15:56 GMT

";

            Action action = () => WebResponse.Parse(ResponseString);

            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void WebResponse_Parse_ThrowsIfResponseDoesNotEndWithABlankLine()
        {
            const string ResponseString =
@"HTTP/1.1 200 OK
Date: Mon, 27 Jul 2009 12:28:53 GMT
Server: Apache
Last-Modified: Wed, 22 Jul 2009 19:15:56 GMT
";

            Action action = () => WebResponse.Parse(ResponseString);

            action.Should().Throw<ArgumentException>();
        }

        [Theory]
        [InlineData("", false, null, null, -1, null, -1, "status line is empty")]
        [InlineData("HTTP/1.1", false, null, null, -1, null, -1, "status code is absent")]
        [InlineData("HTTP/1.1 200", false, null, null, -1, null, -1, "reason phrase is absent")]
        [InlineData("HTTP/1.1 200 OK", false, null, null, -1, null, -1, "status line does not end in CRLF")]
        [InlineData("HTTP2/1.1 200 OK\r\n", false, null, null, -1, null, -1, "HTTP version has invalid name")]
        [InlineData("HTTP/1..1 200 OK\r\n", false, null, null, -1, null, -1, "HTTP version has invalid version number")]
        [InlineData("HTTP/1.1 200a OK\r\n", false, null, null, -1, null, -1, "status code is not an integer")]
        [InlineData("HTTP/1.1 20 OK\r\n", false, null, null, -1, null, -1, "status code has fewer than three digits")]
        [InlineData("HTTP/1.1 2000 OK\r\n", false, null, null, -1, null, -1, "status code has more than three digits")]
        [InlineData("HTTP/1.1 200 OK\r\n", true, "HTTP", "1.1", 200, "OK", 17, "status line is valid")]
        public void WebResponse_ParseStatusLine_HandlesErrorConditions(
            string statusLine,
            bool shouldSucceed,
            string expectedProtocol,
            string expectedVersion,
            int expectedStatusCode,
            string expectedReasonPhrase,
            int expectedLength,
            string because)
        {
            Action action = () =>
            {
                var webResponse = new WebResponse();

                webResponse.ParseStatusLine(statusLine, out int length);

                webResponse.Protocol.Should().Be(expectedProtocol, because);
                webResponse.Version.Should().Be(expectedVersion, because);
                webResponse.StatusCode.Should().Be(expectedStatusCode, because);
                webResponse.ReasonPhrase.Should().Be(expectedReasonPhrase, because);

                length.Should().Be(expectedLength, because);
            };

            if (shouldSucceed) { action.Should().NotThrow(); }
            else { action.Should().Throw<Exception>(); }
        }

        [Fact]
        public void WebResponse_TryParse_ReturnsTrueOnValidResponse()
        {
            const string ResponseString = "HTTP/1.1 200 OK\r\n\r\n";

            bool succeeded = WebResponse.TryParse(ResponseString, out WebResponse webResponse);

            succeeded.Should().BeTrue();
            webResponse.Protocol.Should().Be("HTTP");
            webResponse.Version.Should().Be("1.1");
            webResponse.StatusCode.Should().Be(200);
            webResponse.ReasonPhrase.Should().Be("OK");
        }

        [Fact]
        public void WebResponse_TryParse_ReturnsFalseOnInvalidResponse()
        {
            const string ResponseString = "HTTP/1.1 200 OK extra-stuff\r\n";

            bool succeeded = WebResponse.TryParse(ResponseString, out WebResponse webResponse);

            succeeded.Should().BeFalse();
            webResponse.Should().BeNull();
        }
    }
}
