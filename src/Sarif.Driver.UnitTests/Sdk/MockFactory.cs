// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Moq;

using Match = System.Text.RegularExpressions.Match; // Avoid ambiguity with Moq.Match;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    internal static class MockFactory
    {
        public static IFileSystem MakeMockFileSystem(string fileName, string[] fileContents)
        {
            var mock = new Mock<IFileSystem>(MockBehavior.Strict);
            mock.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns((string s) => s.Equals(fileName, StringComparison.OrdinalIgnoreCase));
            mock.Setup(fs => fs.GetFullPath(It.IsAny<string>())).Returns((string path) => path);
            mock.Setup(fs => fs.ReadAllLines(fileName)).Returns(fileContents);
            return mock.Object;
        }

        public static IEnvironmentVariables MakeMockEnvironmentVariables()
        {
            return MakeMockEnvironmentVariables(new Dictionary<string, string>());
        }

        // This regex is only approximate (environment variable names can contain
        // non-"word" characters), but good enough for these tests.
        private static readonly Regex environmentVariableReferenceRegEx = new Regex(@"%(?<variable>\w+)%");

        public static IEnvironmentVariables MakeMockEnvironmentVariables(IDictionary<string, string> environmentVariableDictionary)
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
