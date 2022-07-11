// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Text;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif.Multitool.Rules;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class SarifValidationSkimmerBaseTests
    {
        private class TestCase
        {
            internal string Name;
            internal string JsonPointer;
            internal string ExpectedJavaScript;
        };

        [Fact]
        public void SarifValidationSkimmerBase_JsonPointerToJavaScript_ProducesExpectedResults()
        {
            var testCases = new TestCase[]
            {
                new TestCase
                {
                    Name = "Single property",
                    JsonPointer = "/version",
                    ExpectedJavaScript = "version"
                },
                new TestCase
                {
                    Name = "Nested properties",
                    JsonPointer = "/properties/tags",
                    ExpectedJavaScript = "properties.tags"
                },
                new TestCase
                {
                    Name = "Array indices",
                    JsonPointer = "/runs/0/results/1",
                    ExpectedJavaScript = "runs[0].results[1]"
                },
                new TestCase
                {
                    Name = "Non-identifier property name",
                    JsonPointer = "/runs/0/files/example.c/mimeType",
                    ExpectedJavaScript = "runs[0].files['example.c'].mimeType"
                },
                new TestCase
                {
                    Name = "Non-identifier property name with escapes",
                    JsonPointer = "/runs/0/files/file:~1~1~1c:~1src/mimeType",
                    ExpectedJavaScript = "runs[0].files['file:///c:/src'].mimeType"
                },
                new TestCase
                {
                    Name = "Property names with non-ASCII characters",
                    JsonPointer = "/A/madár/szép",       // Hungarian: "The bird is pretty."
                    ExpectedJavaScript = "A.madár.szép"
                }
            };

            var sb = new StringBuilder();
            foreach (TestCase testCase in testCases)
            {
                string actualJavaScript = SarifValidationSkimmerBase.JsonPointerToJavaScript(testCase.JsonPointer);
                if (string.CompareOrdinal(actualJavaScript, testCase.ExpectedJavaScript) != 0)
                {
                    sb.AppendLine($"\nFAILED test case: {testCase.Name}:\n    Input: \"{testCase.JsonPointer}\"\n    Expected: \"{testCase.ExpectedJavaScript}\"\n    Actual: \"{actualJavaScript}\".");
                }
            }

            string errorMessage = sb.ToString();
            errorMessage.Should().BeEmpty();
        }

        [Fact]
        public void SarifValidationSkimmerBase_UriIsWellFormedUriString_ProducesExpectedResults()
        {
            var testCases = new[]
            {
                new
                {
                    expectedIsWellFormedUri = true,
                    uriKind = UriKind.Absolute,
                    uriString = "file:///c:/Code/sarif-sdk/src/"
                },
                new
                {
                    expectedIsWellFormedUri = true,
                    uriKind = UriKind.Absolute,
                    uriString = "https://example.com/my-project"
                },
                new
                {
                    expectedIsWellFormedUri = true,
                    uriKind = UriKind.Absolute,
                    uriString = "file:/c:/src/file.c"
                },
                new
                {
                    expectedIsWellFormedUri = false,
                    uriKind = UriKind.Absolute,
                    uriString = "ht%tp://www.example.com/rules/tst0001.html"
                },
                new
                {
                    expectedIsWellFormedUri = true,
                    uriKind = UriKind.RelativeOrAbsolute,
                    uriString = "file:///c:/Code/sarif-sdk/src/"
                },
                new
                {
                    expectedIsWellFormedUri = true,
                    uriKind = UriKind.RelativeOrAbsolute,
                    uriString = "https://example.com/my-project"
                },
                new
                {
                    expectedIsWellFormedUri = true,
                    uriKind = UriKind.RelativeOrAbsolute,
                    uriString = "file:/c:/src/file.c"
                },
                new
                {
                    expectedIsWellFormedUri = false,
                    uriKind = UriKind.RelativeOrAbsolute,
                    uriString = "ht%tp://www.example.com/rules/tst0001.html"
                },
            };

            foreach(var testCase in testCases)
            {
                SarifValidationSkimmerBase.UriIsWellFormedUriString(testCase.uriString, testCase.uriKind).Should().Be(testCase.expectedIsWellFormedUri);
            }
        }
    }
}
