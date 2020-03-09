// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

using FluentAssertions;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    public class EntryPointUtilitiesTests_GenerateArguments
    {
        [Fact]
        public void EnryPointUtilities_GenerateArguments_SucceedsWithEmptyArgumentList()
        {
            string[] result = EntryPointUtilities.GenerateArguments(new string[0], null, null);

            result.Should().BeEmpty();
        }

        [Fact]
        public void EnryPointUtilities_GenerateArguments_SucceedsWithNormalArguments()
        {
            string[] args = new[] { "/y:z", "/x" };

            string[] result = EntryPointUtilities.GenerateArguments(args, null, null);

            result.Length.Should().Be(2);
            result.Should().ContainInOrder(args);
        }

        [Fact]
        public void EnryPointUtilities_GenerateArguments_ExpandsResponseFileContents()
        {
            const string ResponseFileName = "test.rsp";
            string[] responseFileContents = new[] { "/b", "/c:val /d", "   /e   " };
            IFileSystem mockFileSystem = MockFactory.MakeMockFileSystem(ResponseFileName, responseFileContents);

            string[] args = new[] { "/a", "@" + ResponseFileName, "/f" };

            string[] result = EntryPointUtilities.GenerateArguments(args, mockFileSystem, MockFactory.MakeMockEnvironmentVariables());

            result.Length.Should().Be(6);
            result.Should().ContainInOrder("/a", "/b", "/c:val", "/d", "/e", "/f");
        }

        [Fact]
        public void EnryPointUtilities_GenerateArguments_ExceptionIfResponseFileDoesNotExist()
        {
            string NonexistentResponseFile = Guid.NewGuid().ToString() + ".rsp";
            string[] args = new[] { "/a", "@" + NonexistentResponseFile, "/f" };

            Assert.Throws<FileNotFoundException>(
                () => EntryPointUtilities.GenerateArguments(args, new FileSystem(), new EnvironmentVariables())
            );
        }

        [Fact]
        public void EnryPointUtilities_GenerateArguments_StripsQuotesFromAroundArgsWithSpacesInResponseFiles()
        {
            const string ResponseFileName = "test.rsp";
            string[] responseFileContents = new[] { "a \"one two\" b" };
            IFileSystem mockFileSystem = MockFactory.MakeMockFileSystem(ResponseFileName, responseFileContents);

            string[] args = new[] { "@" + ResponseFileName };

            string[] result = EntryPointUtilities.GenerateArguments(args, mockFileSystem, MockFactory.MakeMockEnvironmentVariables());

            result.Length.Should().Be(3);
            result.Should().ContainInOrder("a", "one two", "b");
        }

        [Fact]
        public void EnryPointUtilities_GenerateArguments_ExpandsEnvironmentVariablesInResponseFilePathName()
        {
            const string DirectoryVariableName = "InstallationDirectory";
            const string DirectoryName = @"c:\MyDirectory";
            var environmentVariableDictionary = new Dictionary<string, string>
            {
                { DirectoryVariableName, DirectoryName }
            };
            IEnvironmentVariables mockEnvironmentVariables = MockFactory.MakeMockEnvironmentVariables(environmentVariableDictionary);

            const string ResponseFileName = "test.rsp";

            string responseFileNameArgument = string.Format(CultureInfo.InvariantCulture,
                @"%{0}%\{1}", DirectoryVariableName, ResponseFileName);

            string expandedResponseFileName = string.Format(CultureInfo.InvariantCulture,
                @"{0}\{1}", DirectoryName, ResponseFileName);

            string[] responseFileContents = new[] { "a", "b c" };
            IFileSystem mockFileSystem = MockFactory.MakeMockFileSystem(expandedResponseFileName, responseFileContents);

            string[] args = new[] { "@" + responseFileNameArgument };

            string[] result = EntryPointUtilities.GenerateArguments(args, mockFileSystem, mockEnvironmentVariables);

            result.Length.Should().Be(3);
            result.Should().ContainInOrder("a", "b", "c");
        }
    }
}
