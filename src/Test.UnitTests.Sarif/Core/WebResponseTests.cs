// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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

            webResponse.IsInvalid.Should().BeFalse();
            webResponse.Protocol.Should().Be("HTTP");
            webResponse.Version.Should().Be("1.1");
            webResponse.HttpVersion.Should().Be("HTTP/1.1");
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
        }
    }
}
