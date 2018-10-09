// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif.Multitool.Rules;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.UnitTests.Rules
{
    public class SarifValidationSkimmerBaseTests
    {
        private class TestCase
        {
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
                    JsonPointer = "x",
                    ExpectedJavaScript = "x"
                }
            };

            var sb = new StringBuilder();
            foreach (TestCase testCase in testCases)
            {
                string actualJavaScript = SarifValidationSkimmerBase.JsonPointerToJavaScript(testCase.JsonPointer);
                if (string.CompareOrdinal(actualJavaScript, testCase.ExpectedJavaScript) != 0)
                {
                    sb.AppendLine($"Input: \"{testCase.JsonPointer}\", Expected: \"{testCase.ExpectedJavaScript}\", Actual: \"{actualJavaScript}\".");
                }
            }

            string errorMessage = sb.ToString();
            errorMessage.Should().BeEmpty();
        }
    }
}
