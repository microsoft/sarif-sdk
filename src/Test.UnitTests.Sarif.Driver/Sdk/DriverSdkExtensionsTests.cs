// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif.Driver.Sdk;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    public class DriverSdkExtensionsTests
    {
        [Fact]
        public void ConstructUriBaseIdsDictionary_RetrievesAbsoluteFilePath()
        {
            string name = "SRC";
            string path = Environment.CurrentDirectory;
            string uriBaseIdEntry = name + "=" + path;

            var commonOptionsBase = new CommonOptionsBase
            {
                UriBaseIds = new string[] { uriBaseIdEntry }
            };

            IDictionary<string, ArtifactLocation> uriBaseIds = commonOptionsBase.ConstructUriBaseIdsDictionary();

            uriBaseIds.Count().Should().Be(1);
            uriBaseIds.Keys.First().Should().Be(name);
            uriBaseIds.Values.First().Uri.LocalPath.Should().Be(path);
        }

        [Fact(Skip = "If run concurrently with tests for other libraries, this test fails. To be investigated.")]
        public void ConstructUriBaseIdsDictionary_ConvertsRelativeFilePathToAbsolute()
        {
            string name = "SRC";
            string path = Environment.CurrentDirectory;

            int childDirectoryCount = path.Split('\\').Length;


            string[] parentDirectorySpecifiers = new string[childDirectoryCount];
            for (int i = 0; i < childDirectoryCount; i++)
            {
                parentDirectorySpecifiers[i] = @"..";
            }

            // Trim the drive letter off the path
            path = path.Substring(2);
            path = string.Join(@"\", parentDirectorySpecifiers) + path;

            string uriBaseIdEntry = name + "=" + path;

            var commonOptionsBase = new CommonOptionsBase
            {
                UriBaseIds = new string[] { uriBaseIdEntry }
            };

            IDictionary<string, ArtifactLocation> uriBaseIds = commonOptionsBase.ConstructUriBaseIdsDictionary();

            uriBaseIds.Count().Should().Be(1);
            uriBaseIds.Keys.First().Should().Be(name);
            uriBaseIds.Values.First().Uri.LocalPath.Should().Be(Environment.CurrentDirectory);
        }

        [Fact]
        public void ConstructUriBaseIdsDictionary_InvalidUriThrowsInvalidException()
        {
            string name = "SRC";
            string path = "http://example<>";

            string uriBaseIdEntry = name + "=" + path;

            var commonOptionsBase = new CommonOptionsBase
            {
                UriBaseIds = new string[] { uriBaseIdEntry }
            };

            Assert.Throws<InvalidOperationException>(() => commonOptionsBase.ConstructUriBaseIdsDictionary());
        }
    }
}
