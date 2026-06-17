// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

using CommandLine;

using FluentAssertions;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class CommandLineParameterTests
    {
        [Fact]
        public void PostUriParameterTests_ParseAsExpected()
        {
            const string samplePostUri = @"http://sample.com";
            const string sampleSarifFileName = "sample.sarif";
            ValidateOptions validateOptions;

            validateOptions = this.CommandLineTestHelper<ValidateOptions>(
                new string[] { "validate2", sampleSarifFileName },
                valid: false);
            validateOptions.Should().BeNull();

            validateOptions = this.CommandLineTestHelper<ValidateOptions>(
                new string[] { "validate", sampleSarifFileName },
                valid: true);
            validateOptions.PostUri.Should().BeNull();

            validateOptions = this.CommandLineTestHelper<ValidateOptions>(
                new string[] { "validate", sampleSarifFileName, "--post-uri", samplePostUri },
                valid: true);
            validateOptions.PostUri.Should().Be(samplePostUri);

            validateOptions = this.CommandLineTestHelper<ValidateOptions>(
                new string[] { "validate", sampleSarifFileName, "--postUri", samplePostUri },
                valid: false);
            validateOptions.Should().BeNull();
        }

        private T CommandLineTestHelper<T>(IEnumerable<string> args, bool valid)
        {
            Parser parser = Parser.Default;
            ParserResult<object> result = parser.ParseArguments<
                // Keep this in alphabetical order
                AbsoluteUriOptions,
#if DEBUG
                AnalyzeTestOptions,
#endif
                ApplyPolicyOptions,
                ConvertOptions,
                ExportValidationConfigurationOptions,
                ExportValidationRulesMetadataOptions,
                FileWorkItemsOptions,
                ResultMatchingOptions,
                MergeOptions,
                PageOptions,
                QueryOptions,
                RebaseUriOptions,
                RewriteOptions,
                SuppressOptions,
                ValidateOptions>(args);

            if (valid)
            {
                result.Should().BeOfType<Parsed<object>>();
                object parsedResult = ((Parsed<object>)result).Value;
                parsedResult.Should().NotBeNull();
                parsedResult.Should().BeOfType<T>();
                return (T)parsedResult;
            }
            else
            {
                result.Should().BeOfType<NotParsed<object>>();
                object notParsedResult = ((NotParsed<object>)result);
                notParsedResult.Should().NotBeNull();
                return default;
            }
        }
    }
}
