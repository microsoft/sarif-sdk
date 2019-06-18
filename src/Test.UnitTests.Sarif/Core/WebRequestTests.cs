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
            const string RequestString =
@"GET /app/search/search.html?searchTerm=water HTTP/1.1
Accept: text/html, application/xhtml+xml, application/xml; q=0.9, */*; q=0.8
Accept-Charset: *
Accept-Encoding: gzip, deflate
User-Agent: Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; WOW64; Trident/5.0)
Host: webscout-ppe
Content-Type: application/x-www-form-urlencoded
Cookie: ai_user=12345|2019-06-13T15:24:19.508Z; ai_session=abcde|12345.67|122233334444.65

Some body text.
Just for testing.";

            WebRequest webRequest = WebRequest.Parse(RequestString);

            webRequest.Method.Should().Be("GET");
        }
    }
}
