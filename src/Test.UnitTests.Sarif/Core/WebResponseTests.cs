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
        public void WebResponse_Parse_CreatesExpectedWebRequestObject()
        {
            const string ResponseString =
@"HTTP/1.1 200 OK
Date: Thu, 13 Jun 2019 15:31:38 GMT
Content-Length: 1225
Content-Type: text/html
Content-Encoding: gzip
Last-Modified: Fri, 07 Jun 2019 05:15:32 GMT
Accept-Ranges: bytes
Server: Microsoft-IIS/10.0
Vary: Accept-Encoding
Persistent-Auth: true
X-Powered-By: ASP.NET
Strict-Transport-Security: max-age=31536000
X-Content-Type-Options: nosniff
X-UA-Compatible: IE=edge
X-FRAME-OPTIONS: SAMEORIGIN
Content-Security-Policy: frame-ancestors 'self'

<div error-message error=""error"">
</di >
<div class=""form-horizontal"">
    <div class=""col-sm-12"">
    </div>
</div>";
            WebResponse webResponse = WebResponse.Parse(ResponseString);

            webResponse.Protocol.Should().Be("HTTP/1.1");
            webResponse.StatusCode.Should().Be(200);
        }
    }
}
