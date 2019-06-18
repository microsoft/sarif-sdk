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

            webResponse.Protocol.Should().Be("HTTP");
            webResponse.Version.Should().Be("1.1");
            webResponse.HttpVersion.Should().Be("HTTP/1.1");
            webResponse.StatusCode.Should().Be(200);
            webResponse.ReasonPhrase.Should().Be("OK");
            webResponse.IsInvalid.Should().BeFalse();
        }
    }
}
