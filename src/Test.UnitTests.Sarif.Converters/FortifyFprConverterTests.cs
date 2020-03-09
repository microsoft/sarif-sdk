// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    public class FortifyFprConverterTests
    {
        [Fact]
        public void FortifyFprConverter_GetFailureLevelFromRuleMetadata_MissingImpactProperty_ReturnsWarning()
        {
            ReportingDescriptor rule = new ReportingDescriptor();

            FailureLevel level = FortifyFprConverter.GetFailureLevelFromRuleMetadata(rule);

            level.Should().Be(FailureLevel.Warning);
        }

        [Fact]
        public void FortifyFprConverter_GetFailureLevelFromRuleMetadata_ReturnsAppropriateFailureLevel()
        {
            var expectedInputOutputs = new Dictionary<string, FailureLevel>
            {
                { "0.0", FailureLevel.Note },
                { "0.5", FailureLevel.Note },
                { "1.0", FailureLevel.Note },

                { "1.1", FailureLevel.Warning },
                { "2.0", FailureLevel.Warning },
                { "2.5", FailureLevel.Warning },
                { "2.9", FailureLevel.Warning },
                { "3.0", FailureLevel.Warning },

                { "3.1", FailureLevel.Error },
                { "3.5", FailureLevel.Error },
                { "3.9", FailureLevel.Error },
                { "4.5", FailureLevel.Error },
                { "5.0", FailureLevel.Error },

                { "-5.5", FailureLevel.Warning }, //Invalid value, we default it to Warning
                { "5.5", FailureLevel.Error }, // Invalid value, we guess that it should be treated as Error

            };

            foreach (KeyValuePair<string, FailureLevel> keyValuePair in expectedInputOutputs)
            {
                ReportingDescriptor rule = new ReportingDescriptor();
                rule.SetProperty<string>("Impact", keyValuePair.Key);

                FailureLevel level = FortifyFprConverter.GetFailureLevelFromRuleMetadata(rule);

                level.Should().Be(keyValuePair.Value);
            }
        }

        [Fact]
        public void FortifyFprConverter_GetOriginalUriBaseIdsDictionary_sourceIsDriveLetter()
        {
            Dictionary<string, ArtifactLocation> originalUriBaseIdsDictionary = FortifyFprConverter.GetOriginalUriBaseIdsDictionary("C:", "Windows Server 2016");

            originalUriBaseIdsDictionary.Count.Should().Be(1);
            originalUriBaseIdsDictionary.ContainsKey(FortifyFprConverter.FileLocationUriBaseId).Should().BeTrue();

            originalUriBaseIdsDictionary[FortifyFprConverter.FileLocationUriBaseId].Uri.Should().Be(@"file:///C:/");
            originalUriBaseIdsDictionary[FortifyFprConverter.FileLocationUriBaseId].UriBaseId.Should().BeNull();

        }

        [Fact]
        public void FortifyFprConverter_GetOriginalUriBaseIdsDictionary_sourceIsAbsolutePathWithoutTrailingSlash()
        {
            Dictionary<string, ArtifactLocation> originalUriBaseIdsDictionary = FortifyFprConverter.GetOriginalUriBaseIdsDictionary("C:/test/123", "Windows 10");

            originalUriBaseIdsDictionary.Count.Should().Be(1);
            originalUriBaseIdsDictionary.ContainsKey(FortifyFprConverter.FileLocationUriBaseId).Should().BeTrue();
            originalUriBaseIdsDictionary[FortifyFprConverter.FileLocationUriBaseId].Uri.Should().Be(@"file:///C:/test/123/");
            originalUriBaseIdsDictionary[FortifyFprConverter.FileLocationUriBaseId].UriBaseId.Should().BeNull();
        }

        [Fact]
        public void FortifyFprConverter_GetOriginalUriBaseIdsDictionary_sourceIsLinuxStyleAbsolutePath()
        {
            Dictionary<string, ArtifactLocation> originalUriBaseIdsDictionary = FortifyFprConverter.GetOriginalUriBaseIdsDictionary("/root/projects/myproject/src/", "Linux");

            originalUriBaseIdsDictionary.Count.Should().Be(1);
            originalUriBaseIdsDictionary.ContainsKey(FortifyFprConverter.FileLocationUriBaseId).Should().BeTrue();
            originalUriBaseIdsDictionary[FortifyFprConverter.FileLocationUriBaseId].Uri.Should().Be(@"file:///root/projects/myproject/src/");
            originalUriBaseIdsDictionary[FortifyFprConverter.FileLocationUriBaseId].UriBaseId.Should().BeNull();
        }

        [Fact]
        public void FortifyFprConverter_GetOriginalUriBaseIdsDictionary_sourceIsRelativeWithTrailingSlash()
        {
            Dictionary<string, ArtifactLocation> originalUriBaseIdsDictionary = FortifyFprConverter.GetOriginalUriBaseIdsDictionary("/some/relative/path/", "Windows Server 2016");

            originalUriBaseIdsDictionary.Should().BeNull();
        }

        [Fact]
        public void FortifyFprConverter_GetOriginalUriBaseIdsDictionary_sourceIsRelativeWithoutTrailingSlash()
        {
            Dictionary<string, ArtifactLocation> originalUriBaseIdsDictionary = FortifyFprConverter.GetOriginalUriBaseIdsDictionary("another/relative/path", "Windows Server 2016");

            originalUriBaseIdsDictionary.Should().BeNull();
        }

    }
}
