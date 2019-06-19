// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif;
using Xunit;

namespace Microsoft.CodeAnalysis.Test.UnitTests.Sarif.Core
{
    public class WebMessageUtilitiesTests
    {
        [Theory]
        [InlineData("User-Agent: curl/7.16.3 libcurl/7.16.3 OpenSSL/0.9.7l zlib/1.2.3\r\n", true, "User-Agent", "curl/7.16.3 libcurl/7.16.3 OpenSSL/0.9.7l zlib/1.2.3")]
        [InlineData("Host: www.example.com\r\n", true, "Host", "www.example.com")]
        [InlineData("Host:www.example.com\r\n", true, "Host", "www.example.com")]          // No leading whitespace before field value.
        [InlineData("Host: www.example.com  \t  \r\n", true, "Host", "www.example.com")]   // Trailing whitespace after field value.
        [InlineData("H@st: www.example.com\r\n", false, null, null)]                       // Invalid field name token.
        public void WebMessageUtilities_ParseHeaderLine_HandlesErrorConditions(
            string header,
            bool shouldSucceed,
            string expectedName,
            string expectedValue)
        {
            Action action = () =>
            {
                WebMessageUtilities.ParseHeaderLine(
                    header,
                    out string name,
                    out string value,
                    out int totalHeaderLinesLength);

                name.Should().Be(expectedName);
                value.Should().Be(expectedValue);
            };

            if (shouldSucceed) { action.Should().NotThrow(); }
            else { action.Should().Throw<Exception>(); }
        }
    }
}
