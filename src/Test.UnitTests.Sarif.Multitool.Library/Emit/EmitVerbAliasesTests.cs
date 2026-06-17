// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;

using FluentAssertions;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class EmitVerbAliasesTests
    {
        [Theory]
        [InlineData("add-results", "emit-results")]
        [InlineData("add-invocations", "emit-invocations")]
        [InlineData("add-rule-reporting-descriptors", "emit-rule-descriptors")]
        [InlineData("add-notification-reporting-descriptors", "emit-notification-descriptors")]
        public void Normalize_MapsDeprecatedToCanonical(string deprecated, string canonical)
        {
            EmitVerbAliases.Normalize(deprecated, warn: false).Should().Be(canonical);
        }

        [Theory]
        [InlineData("emit-run")]
        [InlineData("emit-results")]
        [InlineData("emit-finalize")]
        [InlineData("validate")]
        [InlineData(null)]
        [InlineData("")]
        public void Normalize_PassesNonDeprecatedThroughUnchanged(string verb)
        {
            EmitVerbAliases.Normalize(verb, warn: false).Should().Be(verb);
        }

        [Fact]
        public void Normalize_WritesDeprecationWarningToStderr()
        {
            TextWriter saved = System.Console.Error;
            try
            {
                using var sw = new StringWriter();
                System.Console.SetError(sw);
                EmitVerbAliases.Normalize("add-results");
                sw.ToString().Should().Contain("'add-results' is deprecated").And.Contain("'emit-results'");
            }
            finally
            {
                System.Console.SetError(saved);
            }
        }

        [Fact]
        public void EveryCanonicalNameIsAGetSchemaVerbKey()
        {
            // Guard: a future verb rename that forgets to update SchemaByVerb breaks
            // get-schema. Every canonical alias target must be a SchemaByVerb key.
            foreach (string canonical in EmitVerbAliases.DeprecatedToCanonical.Values)
            {
                GetSchemaCommand.SchemaByVerb.ContainsKey(canonical)
                    .Should().BeTrue($"'{canonical}' should be a get-schema verb key");
            }
        }
    }
}
