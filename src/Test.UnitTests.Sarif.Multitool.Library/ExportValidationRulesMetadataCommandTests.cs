// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif.Multitool.Rules;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class ExportValidationRulesMetadataCommandTests
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

            var resourceExtractor = new TestAssetResourceExtractor(this.GetType());
            foreach (KeyValuePair<string, SarifValidationSkimmerBase> test in tests)
            {
                var sb = new StringBuilder();
                var command = new ExportValidationRulesMetadataCommand();
                command.BuildRule(test.Value, sb);

                string expectedMarkdown = resourceExtractor.GetResourceExpectedOutputsText(test.Key);

                // We need to normalize the line endings in the multiline string above,
                // because otherwise this test passes or fails depending on the value
                // of core.autocrlf in git.
                expectedMarkdown = expectedMarkdown.Replace("\r", "");
                expectedMarkdown = expectedMarkdown.Replace("\n", Environment.NewLine);
                sb.ToString().Should().Be(expectedMarkdown);
            }
        }

        private class TestRule1 : SarifValidationSkimmerBase
        {
            public TestRule1() : base()
            {
                DefaultConfiguration.Level = FailureLevel.Note;
            }

            public override string Id => "TEST0001";
            public override string Name => "TEST";
            public override MultiformatMessageString FullDescription => new MultiformatMessageString { Text = "full description text" };
            public override IDictionary<string, MultiformatMessageString> MessageStrings => new Dictionary<string, MultiformatMessageString>
            {
                { "TEST0001_TEST_Note_Default", new MultiformatMessageString{ Text="default text"} }
            };
        }

        private class TestRule2 : SarifValidationSkimmerBase
        {
            public TestRule2() : base()
            {
                DefaultConfiguration.Level = FailureLevel.Note;
            }

            public override string Id => "TEST0002";
            public override string Name => "TEST2";
            public override MultiformatMessageString FullDescription => null;
            public override MultiformatMessageString ShortDescription => new MultiformatMessageString { Text = "short description text" };
            public override IDictionary<string, MultiformatMessageString> MessageStrings => new Dictionary<string, MultiformatMessageString>
            {
                { "TEST0002_TEST2_Note_Default", new MultiformatMessageString{ Markdown = "# markdown example" } }
            };
        }

        private class TestRule3 : SarifValidationSkimmerBase
        {
            public TestRule3() : base()
            {
                DefaultConfiguration.Level = FailureLevel.Note;
            }

            public override string Id => "TEST0003";
            public override string Name => "TEST3";
            public override MultiformatMessageString FullDescription => null;
            public override MultiformatMessageString ShortDescription => null;
            public override IDictionary<string, MultiformatMessageString> MessageStrings => new Dictionary<string, MultiformatMessageString>
            {
                { "TEST0003_TEST3_Note_Default", new MultiformatMessageString{ Text="default text"} }
            };
        }

        private class TestRule4 : SarifValidationSkimmerBase
        {
            public TestRule4() : base()
            {
                DefaultConfiguration.Level = FailureLevel.Note;
            }

            public override string Id => "TEST0004";
            public override string Name => "TEST";
            public override MultiformatMessageString FullDescription => new MultiformatMessageString { Text = "full description text", Markdown = "markdown text" };
            public override IDictionary<string, MultiformatMessageString> MessageStrings => new Dictionary<string, MultiformatMessageString>
            {
                { "TEST0004_TEST_Note_Default", new MultiformatMessageString{ Text = "default text"} }
            };
        }

        private class TestRule5 : SarifValidationSkimmerBase
        {
            public TestRule5() : base()
            {
                DefaultConfiguration.Level = FailureLevel.Note;
            }

            public override string Id => "TEST0005";
            public override string Name => "TEST";
            public override MultiformatMessageString FullDescription => null;
            public override MultiformatMessageString ShortDescription => new MultiformatMessageString { Text = "short description text", Markdown = "markdown text" };
            public override IDictionary<string, MultiformatMessageString> MessageStrings => new Dictionary<string, MultiformatMessageString>
            {
                { "TEST0005_TEST_Note_Default", new MultiformatMessageString{ Markdown = "# markdown example" } }
            };
        }

        private class TestRule6 : SarifValidationSkimmerBase
        {
            public TestRule6() : base()
            {
                DefaultConfiguration.Level = FailureLevel.Note;
            }

            public override string Id => "TEST0006";
            public override string Name => "TEST";
            public override MultiformatMessageString FullDescription => new MultiformatMessageString { Text = "full description text" };
            public override IDictionary<string, MultiformatMessageString> MessageStrings => new Dictionary<string, MultiformatMessageString>
            {
                { "Default_Note_TEST_TEST0006", new MultiformatMessageString{ Text = "default text" } }
            };
        }
    }
}
