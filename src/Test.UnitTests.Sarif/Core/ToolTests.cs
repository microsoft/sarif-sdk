// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.ObjectModel;
using System.Text;
using FluentAssertions;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Core
{
    public class ToolTests
    {
        private class DottedQuadFileVersionTestCase
        {
            public DottedQuadFileVersionTestCase(string input, string expectedOutput)
            {
                Input = input;
                ExpectedOutput = expectedOutput;
            }

            public string Input { get; }
            public string ExpectedOutput { get; }
        }

        private static readonly ReadOnlyCollection<DottedQuadFileVersionTestCase> s_dottedQuadFileVersionTestCases =
            new ReadOnlyCollection<DottedQuadFileVersionTestCase>(new DottedQuadFileVersionTestCase[]
            {
                new DottedQuadFileVersionTestCase(
                    input: "",
                    expectedOutput: null),

                new DottedQuadFileVersionTestCase(
                    input: "11.22.33",
                    expectedOutput: null),

                new DottedQuadFileVersionTestCase(
                    input: "11.22.33.xx",
                    expectedOutput: null),

                new DottedQuadFileVersionTestCase(
                    input: "11.22..44",
                    expectedOutput: null),

                new DottedQuadFileVersionTestCase(
                    input: "prefix 11.22.33.44",
                    expectedOutput: null),

                new DottedQuadFileVersionTestCase(
                    input: "11.22.33.44",
                    expectedOutput: "11.22.33.44"),

                new DottedQuadFileVersionTestCase(
                    input: "11.22.33.44 suffix",
                    expectedOutput: "11.22.33.44"),
            });

        [Fact]
        public void Tool_ParseFileVersion_ExtractsDottedQuadFileVersion()
        {
            StringBuilder sb = new StringBuilder();

            foreach (DottedQuadFileVersionTestCase testCase in s_dottedQuadFileVersionTestCases)
            {
                string actualOutput = Tool.ParseFileVersion(testCase.Input);

                bool succeeded = (testCase.ExpectedOutput == null && actualOutput == null)
                    || (actualOutput?.Equals(testCase.ExpectedOutput, StringComparison.Ordinal) == true);

                if (!succeeded)
                {
                    sb.AppendLine($"    Input: {Utilities.SafeFormat(testCase.Input)} Expected: {Utilities.SafeFormat(testCase.ExpectedOutput)} Actual: {Utilities.SafeFormat(actualOutput)}");
                }
            }

            sb.Length.Should().Be(0,
                $"all test cases should pass, but the following test cases failed:\n{sb.ToString()}");
        }
    }
}
