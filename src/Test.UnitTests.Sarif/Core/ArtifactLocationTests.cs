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
        private const string ProjectRootBaseId = "PROJECT_ROOT";
        private const string SourceRootBaseId = "SOURCE_ROOT";

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
                UriBaseId = ProjectRootBaseId
            };

            var originalUriBaseIds = new Dictionary<string, ArtifactLocation>
            {
                [ProjectRootBaseId] = new ArtifactLocation
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
                UriBaseId = SourceRootBaseId
            };

            var originalUriBaseIds = new Dictionary<string, ArtifactLocation>
            {
                [ProjectRootBaseId] = new ArtifactLocation
                {
                    Uri = new Uri("file://c:/code/sarif-sdk/", UriKind.Absolute)
                },

                [SourceRootBaseId] = new ArtifactLocation
                {
                    Uri = new Uri("src/", UriKind.Relative),
                    UriBaseId = ProjectRootBaseId
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
                UriBaseId = SourceRootBaseId
            };

            var originalUriBaseIds = new Dictionary<string, ArtifactLocation>
            {
                [SourceRootBaseId] = new ArtifactLocation
                {
                    Uri = new Uri("src/", UriKind.Relative),
                    UriBaseId = ProjectRootBaseId // But originalUriBaseIds[ProjectRootBaseId] is absent.
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
                UriBaseId = SourceRootBaseId
            };

            var originalUriBaseIds = new Dictionary<string, ArtifactLocation>
            {
                [ProjectRootBaseId] = new ArtifactLocation
                {
                    Description = new Message
                    {
                        Text = "The root of the project."
                    }
                    // But Uri is absent.
                },

                [SourceRootBaseId] = new ArtifactLocation
                {
                    Uri = new Uri("src/", UriKind.Relative),
                    UriBaseId = ProjectRootBaseId // But originalUriBaseIds[ProjectRootBaseId] is absent.
                }
            };

            bool wasResolved = artifactLocation.TryReconstructAbsoluteUri(originalUriBaseIds, out Uri resolvedUri);

            wasResolved.Should().BeFalse();
        }

        [Fact]
        public void TryReconstructAbsoluteUri_WhenBaseUriDoesNotEndWithSlash_EnsuresSlashIsPresent()
        {
            var artifactLocation = new ArtifactLocation
            {
                Uri = new Uri("src/Sarif/CopyrightNotice.txt", UriKind.Relative),
                UriBaseId = ProjectRootBaseId
            };

            var originalUriBaseIds = new Dictionary<string, ArtifactLocation>
            {
                [ProjectRootBaseId] = new ArtifactLocation
                {
                    // It's invalid SARIF for a base URI not to end with a slash, but we shouldn't
                    // fail to resolve a URI just because some tool didn't do that.
                    Uri = new Uri("file://c:/code/sarif-sdk"),
                    UriBaseId = ProjectRootBaseId
                }
            };

            bool wasResolved = artifactLocation.TryReconstructAbsoluteUri(originalUriBaseIds, out Uri resolvedUri);

            wasResolved.Should().BeTrue();
            resolvedUri.Should().Be(new Uri("file://c:/code/sarif-sdk/src/Sarif/CopyrightNotice.txt", UriKind.Absolute));
        }
    }
}
