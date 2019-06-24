// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif;
using Xunit;

namespace Microsoft.CodeAnalysis.Test.UnitTests.Sarif.Core
{
    public class WebRequestTests
    {
        [Fact]
        public void WebRequest_Parse_CreatesExpectedWebRequestObject()
        {
            // Example from RFC 7230.
            const string RequestString =
//@"GET /hello.txt HTTP/1.1
//User-Agent: curl/7.16.3 libcurl/7.16.3 OpenSSL/0.9.7l zlib/1.2.3
//Host: www.example.com
//Accept-Language: en, mi

//";
"GET /app/scanConfigs/scanConfigs.html?verbose=true&debug=false HTTP/1.1\r\nHost: webscout-ppe\r\nConnection: keep-alive\r\nAccept: application/json, text/plain, */*\r\nRequest-Id: |VCx+R.qlZGS\r\nUser-Agent: Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/72.0.3626.96 Safari/537.36\r\nReferer: https://webscout-ppe/\r\nAccept-Encoding: gzip, deflate, br\r\nAccept-Language: en-US,en;q=0.9\r\nAccept-Charset: *\r\nCookie: ai_user=OYbEy|2019-06-13T15:24:19.508Z; ai_session=zilXK|1560439463545.93|1560439463545.93\r\n\r\n";

            WebRequest webRequest = WebRequest.Parse(RequestString);

            webRequest.Method.Should().Be("GET");
            webRequest.Target.Should().Be("/hello.txt");
            webRequest.Parameters.Should().BeNull();
            webRequest.Protocol.Should().Be("HTTP");
            webRequest.Version.Should().Be("1.1");
            webRequest.HttpVersion.Should().Be("HTTP/1.1");
            webRequest.Headers.Count.Should().Be(3);
            webRequest.Headers["User-Agent"].Should().Be("curl/7.16.3 libcurl/7.16.3 OpenSSL/0.9.7l zlib/1.2.3");
            webRequest.Headers["Host"].Should().Be("www.example.com");
            webRequest.Headers["Accept-Language"].Should().Be("en, mi");
            webRequest.Body.Should().BeNull();
        }

        [Fact]
        public void WebRequest_Parse_ExtractsBody()
        {
            // Example from RFC 7230.
            const string RequestString =
@"GET /hello.txt HTTP/1.1
User-Agent: curl/7.16.3 libcurl/7.16.3 OpenSSL/0.9.7l zlib/1.2.3
Host: www.example.com
Accept-Language: en, mi

This is the body.
Line 2.
";

            WebRequest webRequest = WebRequest.Parse(RequestString);

            webRequest.Method.Should().Be("GET");
            webRequest.Target.Should().Be("/hello.txt");
            webRequest.Parameters.Should().BeNull();
            webRequest.Protocol.Should().Be("HTTP");
            webRequest.Version.Should().Be("1.1");
            webRequest.HttpVersion.Should().Be("HTTP/1.1");
            webRequest.Headers.Count.Should().Be(3);
            webRequest.Headers["User-Agent"].Should().Be("curl/7.16.3 libcurl/7.16.3 OpenSSL/0.9.7l zlib/1.2.3");
            webRequest.Headers["Host"].Should().Be("www.example.com");
            webRequest.Headers["Accept-Language"].Should().Be("en, mi");
            webRequest.Body.Text.Should().Be("This is the body.\r\nLine 2.\r\n");
        }

        [Fact]
        public void WebRequest_Parse_ExtractsParameters()
        {
            // Example from RFC 7230.
            const string RequestString =
@"GET /hello.txt?verbose=true&debug=false HTTP/1.1
User-Agent: my-agent

";

            WebRequest webRequest = WebRequest.Parse(RequestString);

            webRequest.Method.Should().Be("GET");
            webRequest.Target.Should().Be("/hello.txt?verbose=true&debug=false");
            webRequest.Query.Should().Be("?verbose=true&debug=false");
            webRequest.Parameters.Count.Should().Be(2);
            webRequest.Parameters["verbose"].Should().Be("true");
            webRequest.Parameters["debug"].Should().Be("false");
            webRequest.Protocol.Should().Be("HTTP");
            webRequest.Version.Should().Be("1.1");
            webRequest.HttpVersion.Should().Be("HTTP/1.1");
            webRequest.Headers.Count.Should().Be(1);
            webRequest.Headers["User-Agent"].Should().Be("my-agent");
            webRequest.Body.Should().BeNull();
        }

        [Fact]
        public void WebRequest_Parse_HandlesQueriesWithoutParameters()
        {
            // RFC 3986 does not require the query portion of a URI to consist
            // of a set of name/value pairs (parameters). If it doesn't, we don't
            // fail; we just don't populate webRequest.Parameters.
            const string RequestString =
@"GET /hello.txt?this-query-is-not-a-set-of-parameters HTTP/1.1
User-Agent: my-agent

";

            WebRequest webRequest = WebRequest.Parse(RequestString);

            webRequest.Method.Should().Be("GET");
            webRequest.Target.Should().Be("/hello.txt?this-query-is-not-a-set-of-parameters");
            webRequest.Query.Should().Be("?this-query-is-not-a-set-of-parameters");
            webRequest.Parameters.Count.Should().Be(0);
            webRequest.Protocol.Should().Be("HTTP");
            webRequest.Version.Should().Be("1.1");
            webRequest.HttpVersion.Should().Be("HTTP/1.1");
            webRequest.Headers.Count.Should().Be(1);
            webRequest.Headers["User-Agent"].Should().Be("my-agent");
            webRequest.Body.Should().BeNull();
        }

        [Theory]
        [InlineData("", false, null, null, null, null, null, -1, "request line is empty")]
        [InlineData("GET", false, null, null, null, null, null, -1, "target is absent")]
        [InlineData("GET /hello.txt", false, null, null, null, null, null, -1, "HTTP version is absent")]
        [InlineData("GET /hello.txt HTTP/1.1", false, null, null, null, null, null, -1, "request line does not end in CRLF")]
        [InlineData("GET /hello.txt HTTP2/1.1\r\n", false, null, null, null, null, null, -1, "HTTP version has invalid name")]
        [InlineData("GET /hello.txt HTTP/1..1\r\n", false, null, null, null, null, null, -1, "HTTP version has invalid version number")]
        [InlineData("GET /hello.txt HTTP/1.1\r\n", true, "GET", "/hello.txt", "HTTP/1.1", "HTTP", "1.1", 25, "request line is valid")]
        public void WebRequest_ParseRequestLine_HandlesErrorConditions(
            string requestLine,
            bool shouldSucceed,
            string expectedMethod,
            string expectedTarget,
            string expectedHttpVersion,
            string expectedProtocol,
            string expectedVersion,
            int expectedLength,
            string because)
        {
            Action action = () =>
            {
                var webRequest = new WebRequest();

                webRequest.ParseRequestLine(requestLine, out int length);

                webRequest.Method.Should().Be(expectedMethod, because);
                webRequest.Target.Should().Be(expectedTarget, because);
                webRequest.HttpVersion.Should().Be(expectedHttpVersion, because);
                webRequest.Protocol.Should().Be(expectedProtocol, because);
                webRequest.Version.Should().Be(expectedVersion, because);

                length.Should().Be(expectedLength, because);
            };

            if (shouldSucceed) { action.Should().NotThrow(); }
            else { action.Should().Throw<Exception>(); }
        }
    }
}
