// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Microsoft.Sarif.Viewer.Sarif;
using System;
using Xunit;

namespace Microsoft.Sarif.Viewer.VisualStudio.UnitTests
{
    public class UriExtensionsTests
    {
        [Fact]
        public void ToPath_ValidUrls()
        {
            Uri uri = new Uri("http://raw.githubusercontent.com/foo/bar.cpp");
            uri.ToPath().Should().Be(uri.OriginalString);

            uri = new Uri("HTTP://RAW.githubusercontent.com/FOO/bar.cpp");
            uri.ToPath().Should().Be("http://raw.githubusercontent.com/FOO/bar.cpp");

            uri = new Uri("https://raw.githubusercontent.com/foo/bar.cpp");
            uri.ToPath().Should().Be(uri.OriginalString);

            uri = new Uri("HTTPS://raw.GITHUBusercontent.com/foo/BAR.cpp");
            uri.ToPath().Should().Be("https://raw.githubusercontent.com/foo/BAR.cpp");
        }

        [Fact]
        public void ToPath_ValidLocalPaths()
        {
            Uri uri = new Uri(@"C:\Temp\foo\bar.cpp");
            uri.ToPath().Should().Be(uri.OriginalString);

            uri = new Uri(@"\Temp\foo\bar.cpp", UriKind.Relative);
            uri.ToPath().Should().Be(uri.OriginalString);

            uri = new Uri(@"Temp\foo\bar.cpp", UriKind.Relative);
            uri.ToPath().Should().Be(uri.OriginalString);

            uri = new Uri(@"\\computer\share\Temp\foo\bar.cpp");
            uri.ToPath().Should().Be(uri.OriginalString);

            uri = new Uri(@"\\computer\C$\Temp\foo\bar.cpp");
            uri.ToPath().Should().Be(uri.OriginalString);
        }

        [Fact]
        public void ToPath_InvalidUrls()
        {
            Uri uri = new Uri("ftp://raw.githubusercontent.com/foo/bar.cpp");
            uri.ToPath().Should().Be("/foo/bar.cpp");

            uri = new Uri("http://github.com/FOO/bar.cpp");
            uri.ToPath().Should().Be("/FOO/bar.cpp");
        }

        [Fact]
        public void ToPath_NullUri()
        {
            Uri uri = null;
            uri.ToPath().Should().Be(null);
        }

    }
}
