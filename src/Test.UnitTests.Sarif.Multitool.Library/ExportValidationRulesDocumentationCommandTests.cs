// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Text;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif.Multitool.Rules;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class ExportValidationRulesDocumentationCommandTests
    {
        [Fact]
        public void BuildRule_GeneratesExpectedMarkdown()
        {
            var tests = new Dictionary<string, SarifValidationSkimmerBase>
            {
                { "MarkdownFullDescription.md", new TestRule4() },
                { "MarkdownShortDescription.md", new TestRule5() },
                { "NoDescription.md", new TestRule3() },
                { "NonStandardMessageStringKey.md", new TestRule6() },
                { "StandardMessageStringKey.md", new TestRule1() },
                { "TextShortDescription.md", new TestRule2() },
            };

            var resourceExtractor = new ResourceExtractor(this.GetType());
            foreach (KeyValuePair<string, SarifValidationSkimmerBase> test in tests)
            {
                var sb = new StringBuilder();
                var command = new ExportValidationDocumentationCommand();
                command.BuildRule(test.Value, sb);

                string expectedMarkdown = resourceExtractor.GetResourceText(test.Key);

                sb.ToString().Should().Be(expectedMarkdown);
            }
        }

        private class TestRule1 : SarifValidationSkimmerBase
        {
            public TestRule1() : base(
                "TEST0001",
                "full description text",
                FailureLevel.Note)
            {
                Name = "TEST";
                MessageStrings = new Dictionary<string, MultiformatMessageString>
                {
                    { "TEST0001_TEST_Note_Default", new MultiformatMessageString{ Text="default text"} }
                };
            }
        }

        private class TestRule2 : SarifValidationSkimmerBase
        {
            public TestRule2() : base(
                "TEST0002",
                null,
                FailureLevel.Note)
            {
                Name = "TEST2";
                ShortDescription = new MultiformatMessageString { Text = "short description text" };
                MessageStrings = new Dictionary<string, MultiformatMessageString>
                {
                    { "TEST0002_TEST2_Note_Default", new MultiformatMessageString{ Markdown = "# markdown example" } }
                };
            }
        }

        private class TestRule3 : SarifValidationSkimmerBase
        {
            public TestRule3() : base(
                "TEST0003",
                null,
                FailureLevel.Note)
            {
                Name = "TEST3";
                MessageStrings = new Dictionary<string, MultiformatMessageString>
                {
                    { "TEST0003_TEST3_Note_Default", new MultiformatMessageString{ Text="default text"} }
                };
            }
        }

        private class TestRule4 : SarifValidationSkimmerBase
        {
            public TestRule4() : base(
                "TEST0004",
                null,
                FailureLevel.Note)
            {
                Name = "TEST";
                FullDescription = new MultiformatMessageString { Text = "full description text", Markdown = "markdown text" };
                MessageStrings = new Dictionary<string, MultiformatMessageString>
                {
                    { "TEST0004_TEST_Note_Default", new MultiformatMessageString{ Text = "default text"} }
                };
            }
        }

        private class TestRule5 : SarifValidationSkimmerBase
        {
            public TestRule5() : base(
                "TEST0005",
                null,
                FailureLevel.Note)
            {
                Name = "TEST";
                ShortDescription = new MultiformatMessageString { Text = "short description text", Markdown = "markdown text" };
                MessageStrings = new Dictionary<string, MultiformatMessageString>
                {
                    { "TEST0005_TEST_Note_Default", new MultiformatMessageString{ Markdown = "# markdown example" } }
                };
            }
        }

        private class TestRule6 : SarifValidationSkimmerBase
        {
            public TestRule6() : base(
                "TEST0006",
                "full description text",
                FailureLevel.Note)
            {
                Name = "TEST";
                MessageStrings = new Dictionary<string, MultiformatMessageString>
                {
                    { "Default_Note_TEST_TEST0006", new MultiformatMessageString{ Text = "default text" } }
                };
            }
        }
    }
}
