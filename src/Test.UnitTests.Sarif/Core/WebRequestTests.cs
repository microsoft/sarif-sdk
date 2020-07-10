// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using FluentAssertions;
using FluentAssertions.Extensions;
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
@"GET /hello.txt HTTP/1.1
User-Agent: curl/7.16.3 libcurl/7.16.3 OpenSSL/0.9.7l zlib/1.2.3
Host: www.example.com
Accept-Language: en, mi

";

            WebRequest webRequest = WebRequest.Parse(RequestString);

            webRequest.Method.Should().Be("GET");
            webRequest.Target.Should().Be("/hello.txt");
            webRequest.Parameters.Should().BeNull();
            webRequest.Protocol.Should().Be("HTTP");
            webRequest.Version.Should().Be("1.1");
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
            webRequest.Parameters.Count.Should().Be(2);
            webRequest.Parameters["verbose"].Should().Be("true");
            webRequest.Parameters["debug"].Should().Be("false");
            webRequest.Protocol.Should().Be("HTTP");
            webRequest.Version.Should().Be("1.1");
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
            webRequest.Parameters.Count.Should().Be(0);
            webRequest.Protocol.Should().Be("HTTP");
            webRequest.Version.Should().Be("1.1");
            webRequest.Headers.Count.Should().Be(1);
            webRequest.Headers["User-Agent"].Should().Be("my-agent");
            webRequest.Body.Should().BeNull();
        }

        [Theory]
        [InlineData("", false, null, null, null, null, -1, "request line is empty")]
        [InlineData("GET", false, null, null, null, null, -1, "target is absent")]
        [InlineData("GET /hello.txt", false, null, null, null, null, -1, "HTTP version is absent")]
        [InlineData("GET /hello.txt HTTP/1.1", false, null, null, null, null, -1, "request line does not end in CRLF")]
        [InlineData("GET /hello.txt HTTP2/1.1\r\n", false, null, null, null, null, -1, "HTTP version has invalid name")]
        [InlineData("GET /hello.txt HTTP/1..1\r\n", false, null, null, null, null, -1, "HTTP version has invalid version number")]
        [InlineData("GET /hello.txt HTTP/1.1\r\n", true, "GET", "/hello.txt", "HTTP", "1.1", 25, "request line is valid")]
        public void WebRequest_ParseRequestLine_HandlesErrorConditions(
            string requestLine,
            bool shouldSucceed,
            string expectedMethod,
            string expectedTarget,
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
                webRequest.Protocol.Should().Be(expectedProtocol, because);
                webRequest.Version.Should().Be(expectedVersion, because);

                length.Should().Be(expectedLength, because);
            };

            if (shouldSucceed) { action.Should().NotThrow(); }
            else { action.Should().Throw<Exception>(); }
        }

        [Fact]
        public void WebRequest_TryParse_ReturnsTrueOnValidRequest()
        {
            const string RequestString =
@"GET /hello.txt HTTP/1.1
User-Agent: my-agent

";

            bool succeeded = WebRequest.TryParse(RequestString, out WebRequest webRequest);

            succeeded.Should().BeTrue();
            webRequest.Method.Should().Be("GET");
            webRequest.Target.Should().Be("/hello.txt");
            webRequest.Parameters.Should().BeNull();
            webRequest.Protocol.Should().Be("HTTP");
            webRequest.Version.Should().Be("1.1");
            webRequest.Headers.Count.Should().Be(1);
            webRequest.Headers["User-Agent"].Should().Be("my-agent");
            webRequest.Body.Should().BeNull();
        }

        [Fact]
        public void WebRequest_TryParse_ReturnsFalseOnInvalidRequest()
        {
            const string RequestString =
@"GET /hello.txt HTTP/1.1 extra-stuff
User-Agent: my-agent

";

            bool succeeded = WebRequest.TryParse(RequestString, out WebRequest webRequest);

            succeeded.Should().BeFalse();
            webRequest.Should().BeNull();
        }

        [Fact(Skip="Disabling due to timing inconsistencies across execution environments.")]
        public void WebRequest_TryParse_HasAcceptablePerformance()
        {
            // This is a sanitized version of an actual customer's web request that exposed a perf
            // problem (https://github.com/Microsoft/sarif-sdk/1608). Note the lack of a trailing
            // '=' after the last query parameter "Address3". Given how our regex for parsing query
            // parameters is defined, this causes the regex engine to backtrack to the beginning.
            // Selective disabling of backtracking in portions of the regex improves the performance
            // dramatically.
            const string RequestString = @"GET /getSomethings?FirstName=test&LastName=test&AddressLine1=555%20110th%20Ave%20NE&City=Bellevue&Country=USA&PostalCode=98004&EmailAddress=test@somedomain.com&BusinessPhone=12345678901&MobilePhone=1234567890&AddressLine2=&AddressLine3 HTTP/1.1
Host: kdkdkdkd.azurewebsites.net
Connection: keep-alive
Cache-Control: max-age=0
Upgrade-Insecure-Requests: 1
User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/75.0.3770.90 Safari/537.36
Accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3
Referer: https://login.somedomain.com/77777777777777777777/oauth2/authorize?client_id=7777&response_mode=form_post&response_type=code+id_token&scope=openid+profile&state=somestate
Accept-Encoding: gzip, deflate, br
Accept-Language: en-US,en;q=0.9
Cookie: ARRAffinity=somecode; .AspNet.Cookies=somecode

";
            Action action = () => WebRequest.TryParse(RequestString, out _);

            // On my machine this takes about 7 msec. We leave a huge safety factor. This is still
            // too long, but it should provide acceptable performance, and we can pursue further
            // optimizations later if necessary.
            action.ExecutionTime().Should().BeLessOrEqualTo(1000.Milliseconds());
        }
    }
}
