// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif;
using Xunit;

namespace Microsoft.CodeAnalysis.Test.UnitTests.Sarif.Core
{
    public class ArtifactLocationTests
    {
        [Fact]
        public void TryReconstructAbsoluteUri_WhenInputUriIsNull_ReturnsFalse()
        {
            var artifactLocation = new ArtifactLocation();

            bool wasResolved = artifactLocation.TryReconstructAbsoluteUri(originalUriBaseIds: null, out Uri resolvedUri);

            wasResolved.Should().BeFalse();
            resolvedUri.Should().BeNull();
        }

        [Fact]
        public void TryReconstructAbsoluteUri_WhenInputUriIsAbsolute_ReturnsTrueWithInputUri()
        {
            var artifactLocation = new ArtifactLocation
            {
                Uri = new Uri("http://example.com", UriKind.Absolute)
            };

            bool wasResolved = artifactLocation.TryReconstructAbsoluteUri(originalUriBaseIds: null, out Uri resolvedUri);

            wasResolved.Should().BeTrue();
            resolvedUri.Should().Be(artifactLocation.Uri);
        }

        [Fact]
        public void TryReconstructAbsoluteUri_WhenInputUriResolvesDirectly_ReturnsTrueWithResolvedUri()
        {
            var artifactLocation = new ArtifactLocation
            {
                Uri = new Uri("README.md", UriKind.Relative),
                UriBaseId = "PROJECT_ROOT"
            };

            var originalUriBaseIds = new Dictionary<string, ArtifactLocation>
            {
                ["PROJECT_ROOT"] = new ArtifactLocation
                {
                    Uri = new Uri("file://c:/code/sarif-sdk/", UriKind.Absolute)
                }
            };

            bool wasResolved = artifactLocation.TryReconstructAbsoluteUri(originalUriBaseIds, out Uri resolvedUri);

            wasResolved.Should().BeTrue();
            resolvedUri.Should().Be(new Uri("file://c:/code/sarif-sdk/README.md", UriKind.Absolute));
        }

        [Fact]
        public void TryReconstructAbsoluteUri_WhenInputResolvesIndirectly_ReturnsTrueWithResolvedUri()
        {
            var artifactLocation = new ArtifactLocation
            {
                Uri = new Uri("Sarif/CopyrightNotice.txt", UriKind.Relative),
                UriBaseId = "SOURCE_ROOT"
            };

            var originalUriBaseIds = new Dictionary<string, ArtifactLocation>
            {
                ["PROJECT_ROOT"] = new ArtifactLocation
                {
                    Uri = new Uri("file://c:/code/sarif-sdk/", UriKind.Absolute)
                },

                ["SOURCE_ROOT"] = new ArtifactLocation
                {
                    Uri = new Uri("src/", UriKind.Relative),
                    UriBaseId = "PROJECT_ROOT"
                }
            };

            bool wasResolved = artifactLocation.TryReconstructAbsoluteUri(originalUriBaseIds, out Uri resolvedUri);

            wasResolved.Should().BeTrue();
            resolvedUri.Should().Be(new Uri("file://c:/code/sarif-sdk/src/Sarif/CopyrightNotice.txt", UriKind.Absolute));
        }

        [Fact]
        public void TryReconstructAbsoluteUri_WhenChainedUriBaseIdIsAbsent_ReturnsFalse()
        {
            var artifactLocation = new ArtifactLocation
            {
                Uri = new Uri("Sarif/CopyrightNotice.txt", UriKind.Relative),
                UriBaseId = "SOURCE_ROOT"
            };

            var originalUriBaseIds = new Dictionary<string, ArtifactLocation>
            {
                ["SOURCE_ROOT"] = new ArtifactLocation
                {
                    Uri = new Uri("src/", UriKind.Relative),
                    UriBaseId = "PROJECT_ROOT" // But originalUriBaseIds["PROJECT_ROOT"] is absent.
                }
            };

            bool wasResolved = artifactLocation.TryReconstructAbsoluteUri(originalUriBaseIds, out Uri resolvedUri);

            wasResolved.Should().BeFalse();
        }

        [Fact]
        public void TryReconstructAbsoluteUri_WhenChainedUriIsAbsent_ReturnsFalse()
        {
            var artifactLocation = new ArtifactLocation
            {
                Uri = new Uri("Sarif/CopyrightNotice.txt", UriKind.Relative),
                UriBaseId = "SOURCE_ROOT"
            };

            var originalUriBaseIds = new Dictionary<string, ArtifactLocation>
            {
                ["PROJECT_ROOT"] = new ArtifactLocation
                {
                    Description = new Message
                    {
                        Text = "The root of the project."
                    }
                    // But Uri is absent.
                },

                ["SOURCE_ROOT"] = new ArtifactLocation
                {
                    Uri = new Uri("src/", UriKind.Relative),
                    UriBaseId = "PROJECT_ROOT" // But originalUriBaseIds["PROJECT_ROOT"] is absent.
                }
            };

            bool wasResolved = artifactLocation.TryReconstructAbsoluteUri(originalUriBaseIds, out Uri resolvedUri);

            wasResolved.Should().BeFalse();
        }
    }
}
