// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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
@"GET /hello.txt HTTP/1.1
User-Agent: curl/7.16.3 libcurl/7.16.3 OpenSSL/0.9.7l zlib/1.2.3
Host: www.example.com
Accept-Language: en, mi

";

            WebRequest webRequest = WebRequest.Parse(RequestString);

            webRequest.Method.Should().Be("GET");
            webRequest.Target.Should().Be("/hello.txt");
            webRequest.Protocol.Should().Be("HTTP");
            webRequest.Version.Should().Be("1.1");
            webRequest.HttpVersion.Should().Be("HTTP/1.1");
            webRequest.Headers.Count.Should().Be(3);
            webRequest.Headers["User-Agent"].Should().Be("curl/7.16.3 libcurl/7.16.3 OpenSSL/0.9.7l zlib/1.2.3");
            webRequest.Headers["Host"].Should().Be("www.example.com");
            webRequest.Headers["Accept-Language"].Should().Be("en, mi");
        }
    }
}
