// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif.Multitool.Rules;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class ExportRuleDocumentationCommandTests
    {
        [Fact]
        public void BuildRule_GeneratesExpectedMarkdown()
        {
            var resourceExtractor = new ResourceExtractor(this.GetType());
            var sb = new StringBuilder();
            var testRule = new TestRule();
            var command = new ExportValidationRulesDocumentationCommand();
            command.BuildRule(testRule, sb);

            string expectedMarkdown = resourceExtractor.GetResourceText("Test.md");

            sb.ToString().Should().Be(expectedMarkdown);
        }

        private class TestRule : SarifValidationSkimmerBase
        {
            public override string Id => "TEST0001";
            public override string Name => "TEST";
            public override FailureLevel DefaultLevel => FailureLevel.Note;
            public override MultiformatMessageString FullDescription => new MultiformatMessageString { Text = "full description text" };
            public override IDictionary<string, MultiformatMessageString> MessageStrings => new Dictionary<string, MultiformatMessageString>
            {
                { "TEST0001_TEST_Note_Default", new MultiformatMessageString{ Text="default text"} }
            };
        }
    }
}
