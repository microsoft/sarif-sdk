// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

using FluentAssertions;

using Moq;
using Xunit;
using Match = System.Text.RegularExpressions.Match; // Avoid ambiguity with Moq.Match;

namespace Microsoft.CodeAnalysis.Sarif.Driver.Sdk
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
            var args = new[] { "/y:z", "/x" };

            string[] result = EntryPointUtilities.GenerateArguments(args, null, null);

            result.Length.Should().Be(2);
            result.Should().ContainInOrder(args);
        }

        [Fact]
        public void EnryPointUtilities_GenerateArguments_ExpandsResponseFileContents()
        {
            const string ResponseFileName = "test.rsp";
            var responseFileContents = new[] { "/b", "/c:val /d", "   /e   " };
            var mockFileSystem = MakeMockFileSystem(ResponseFileName, responseFileContents);

            var args = new[] { "/a", "@" + ResponseFileName, "/f" };

            string[] result = EntryPointUtilities.GenerateArguments(args, mockFileSystem, MakeMockEnvironmentVariables());

            result.Length.Should().Be(6);
            result.Should().ContainInOrder("/a", "/b", "/c:val", "/d", "/e", "/f");
        }

        [Fact]
        public void EnryPointUtilities_GenerateArguments_IgnoresNonexistentResponseFile()
        {
            const string ResponseFileName = "test.rsp";
            var responseFileContents = new[] { "/b", "/c:val /d", "   /e   " };
            var mockFileSystem = MakeMockFileSystem(ResponseFileName, responseFileContents);

            const string NonexistentResponseFile = ResponseFileName + "x";
            var args = new[] { "/a", "@" + NonexistentResponseFile, "/f" };

            string[] result = EntryPointUtilities.GenerateArguments(args, mockFileSystem, MakeMockEnvironmentVariables());

            result.Length.Should().Be(2);
            result.Should().ContainInOrder("/a", "/f");
        }

        [Fact]
        public void EnryPointUtilities_GenerateArguments_StripsQuotesFromAroundArgsWithSpacesInResponseFiles()
        {
            const string ResponseFileName = "test.rsp";
            var responseFileContents = new[] { "a \"one two\" b" };
            var mockFileSystem = MakeMockFileSystem(ResponseFileName, responseFileContents);

            var args = new[] { "@" + ResponseFileName };

            string[] result = EntryPointUtilities.GenerateArguments(args, mockFileSystem, MakeMockEnvironmentVariables());

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
            var mockEnvironmentVariables = MakeMockEnvironmentVariables(environmentVariableDictionary);

            const string ResponseFileName = "test.rsp";

            string responseFileNameArgument = String.Format(CultureInfo.InvariantCulture,
                @"%{0}%\{1}", DirectoryVariableName, ResponseFileName);

            string expandedResponseFileName = String.Format(CultureInfo.InvariantCulture,
                @"{0}\{1}", DirectoryName, ResponseFileName);

            var responseFileContents = new[] { "a", "b c" };
            var mockFileSystem = MakeMockFileSystem(expandedResponseFileName, responseFileContents);

            var args = new[] { "@" + responseFileNameArgument };

            string[] result = EntryPointUtilities.GenerateArguments(args, mockFileSystem, mockEnvironmentVariables);

            result.Length.Should().Be(3);
            result.Should().ContainInOrder("a", "b", "c");
        }

        private static IFileSystem MakeMockFileSystem(string fileName, string[] fileContents)
        {
            var mock = new Mock<IFileSystem>(MockBehavior.Strict);
            mock.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns((string s) => s.Equals(fileName));
            mock.Setup(fs => fs.GetFullPath(It.IsAny<string>())).Returns((string path) => path);
            mock.Setup(fs => fs.ReadAllLines(fileName)).Returns(fileContents);
            return mock.Object;
        }

        private static IEnvironmentVariables MakeMockEnvironmentVariables()
        {
            return MakeMockEnvironmentVariables(new Dictionary<string, string>());
        }

        // This regex is only approximate (environment variable names can contain
        // non-"word" characters), but good enough for this tests.
        private static readonly Regex environmentVariableReferenceRegEx = new Regex(@"%(?<variable>\w+)%");

        private static IEnvironmentVariables MakeMockEnvironmentVariables(IDictionary<string, string> environmentVariableDictionary)
        {
            var mock = new Mock<IEnvironmentVariables>(MockBehavior.Strict);

            MatchEvaluator matchEvaluator =
                (Match match) => ReplaceEnvironmentVariables(match, environmentVariableDictionary);

            Func<string, string> moqCallback =
                (string s) => environmentVariableReferenceRegEx.Replace(s, matchEvaluator);

            mock.Setup(ev => ev.ExpandEnvironmentVariables(It.IsAny<string>())).Returns(moqCallback);

            return mock.Object;
        }

        private static string ReplaceEnvironmentVariables(Match match, IDictionary<string, string> environmentVariableDictionary)
        {
            string variable = match.Groups["variable"].Value;
            return environmentVariableDictionary[variable];
        }
    }
}
