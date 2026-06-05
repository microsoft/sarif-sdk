// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

using FluentAssertions;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class EmitFinalizeRebaseVisitorTests
    {
        private const string Sha = "1234567890abcdef1234567890abcdef12345678";

        private static VersionControlDetails Vcd(string repositoryUri, string uriBaseId, string revisionId = Sha)
            => new VersionControlDetails
            {
                RepositoryUri = new Uri(repositoryUri, UriKind.Absolute),
                RevisionId = revisionId,
                Branch = "refs/heads/main",
                MappedTo = new ArtifactLocation { UriBaseId = uriBaseId },
            };

        private static IDictionary<string, ArtifactLocation> Bases(params (string id, string uri)[] entries)
        {
            var bases = new Dictionary<string, ArtifactLocation>(StringComparer.Ordinal);
            foreach ((string id, string uri) in entries)
            {
                bases[id] = new ArtifactLocation { Uri = new Uri(uri, UriKind.Absolute) };
            }

            return bases;
        }

        private static Result ResultAt(string uri, string uriBaseId = null)
            => new Result
            {
                RuleId = "NOVEL-x",
                Message = new Message { Text = "x" },
                Locations = new[]
                {
                    new Location
                    {
                        PhysicalLocation = new PhysicalLocation
                        {
                            ArtifactLocation = new ArtifactLocation
                            {
                                Uri = new Uri(uri, UriKind.RelativeOrAbsolute),
                                UriBaseId = uriBaseId,
                            },
                        },
                    },
                },
            };

        private static ArtifactLocation FirstLocation(Run run)
            => run.Results[0].Locations[0].PhysicalLocation.ArtifactLocation;

        private static EmitFinalizeRebaseVisitor Visit(Run run)
        {
            var visitor = new EmitFinalizeRebaseVisitor();
            visitor.VisitRun(run);
            return visitor;
        }

        [Fact]
        public void SingleRepo_AbsoluteFileLocation_BecomesRelativeUnderSrcRoot()
        {
            var run = new Run
            {
                VersionControlProvenance = new[] { Vcd("https://github.com/microsoft/sarif-sdk", "SRCROOT") },
                OriginalUriBaseIds = Bases(("SRCROOT", "file:///d:/src/sarif-sdk/")),
                Results = new[] { ResultAt("file:///d:/src/sarif-sdk/src/Foo.cs") },
            };

            EmitFinalizeRebaseVisitor visitor = Visit(run);

            visitor.Success.Should().BeTrue();
            ArtifactLocation location = FirstLocation(run);
            location.Uri.OriginalString.Should().Be("src/Foo.cs");
            location.UriBaseId.Should().Be("SRCROOT");
            run.OriginalUriBaseIds["SRCROOT"].Uri
                .Should().Be(new Uri($"https://github.com/microsoft/sarif-sdk/blob/{Sha}/", UriKind.Absolute));
        }

        [Fact]
        public void SingleRepo_RelativeLocationAlreadyUnderBase_KeepsRelativeForm()
        {
            var run = new Run
            {
                VersionControlProvenance = new[] { Vcd("https://github.com/microsoft/sarif-sdk", "SRCROOT") },
                OriginalUriBaseIds = Bases(("SRCROOT", "file:///d:/src/sarif-sdk/")),
                Results = new[] { ResultAt("src/Foo.cs", "SRCROOT") },
            };

            EmitFinalizeRebaseVisitor visitor = Visit(run);

            visitor.Success.Should().BeTrue();
            ArtifactLocation location = FirstLocation(run);
            location.Uri.OriginalString.Should().Be("src/Foo.cs");
            location.UriBaseId.Should().Be("SRCROOT");
        }

        [Fact]
        public void SingleRepo_RewritesMappedToToPortableBaseAndPreservesRevision()
        {
            var run = new Run
            {
                VersionControlProvenance = new[] { Vcd("https://github.com/microsoft/sarif-sdk", "SRCROOT") },
                OriginalUriBaseIds = Bases(("SRCROOT", "file:///d:/src/sarif-sdk/")),
                Results = new[] { ResultAt("file:///d:/src/sarif-sdk/a.cs") },
            };

            EmitFinalizeRebaseVisitor visitor = Visit(run);

            visitor.Success.Should().BeTrue();
            VersionControlDetails vcd = run.VersionControlProvenance[0];
            vcd.MappedTo.UriBaseId.Should().Be("SRCROOT");
            vcd.MappedTo.Uri.Should().BeNull();
            vcd.RepositoryUri.Should().Be(new Uri("https://github.com/microsoft/sarif-sdk", UriKind.Absolute));
            vcd.RevisionId.Should().Be(Sha);
        }

        [Fact]
        public void MultiRepo_NestedCheckouts_AttributeByLongestPrefix()
        {
            var run = new Run
            {
                VersionControlProvenance = new[]
                {
                    Vcd("https://github.com/contoso/package", "PACKAGE_ROOT"),
                    Vcd("https://github.com/contoso/plugin", "PLUGIN_ROOT"),
                },
                OriginalUriBaseIds = Bases(
                    ("PACKAGE_ROOT", "file:///d:/pkg/"),
                    ("PLUGIN_ROOT", "file:///d:/pkg/plugins/plugin/")),
                Results = new[]
                {
                    ResultAt("file:///d:/pkg/src/a.cs"),
                    ResultAt("file:///d:/pkg/plugins/plugin/b.cs"),
                },
            };

            EmitFinalizeRebaseVisitor visitor = Visit(run);

            visitor.Success.Should().BeTrue();

            ArtifactLocation a = run.Results[0].Locations[0].PhysicalLocation.ArtifactLocation;
            a.Uri.OriginalString.Should().Be("src/a.cs");
            a.UriBaseId.Should().Be("SRCROOT_PACKAGE");

            ArtifactLocation b = run.Results[1].Locations[0].PhysicalLocation.ArtifactLocation;
            b.Uri.OriginalString.Should().Be("b.cs");
            b.UriBaseId.Should().Be("SRCROOT_PLUGIN");

            run.OriginalUriBaseIds["SRCROOT_PACKAGE"].Uri
                .Should().Be(new Uri($"https://github.com/contoso/package/blob/{Sha}/", UriKind.Absolute));
            run.OriginalUriBaseIds["SRCROOT_PLUGIN"].Uri
                .Should().Be(new Uri($"https://github.com/contoso/plugin/blob/{Sha}/", UriKind.Absolute));
            run.OriginalUriBaseIds.Should().NotContainKey("PACKAGE_ROOT");
            run.OriginalUriBaseIds.Should().NotContainKey("PLUGIN_ROOT");
        }

        [Fact]
        public void MultiRepo_DuplicateLeafNames_GetOrdinalSuffix()
        {
            var run = new Run
            {
                VersionControlProvenance = new[]
                {
                    Vcd("https://github.com/contoso/widgets", "A"),
                    Vcd("https://github.com/fabrikam/widgets", "B"),
                },
                OriginalUriBaseIds = Bases(
                    ("A", "file:///d:/a/"),
                    ("B", "file:///d:/b/")),
            };

            EmitFinalizeRebaseVisitor visitor = Visit(run);

            visitor.Success.Should().BeTrue();
            run.OriginalUriBaseIds.Should().ContainKey("SRCROOT_WIDGETS");
            run.OriginalUriBaseIds.Should().ContainKey("SRCROOT_WIDGETS_2");
        }

        [Fact]
        public void UnmatchedAbsoluteFilePath_FailsAsLeak()
        {
            var run = new Run
            {
                VersionControlProvenance = new[] { Vcd("https://github.com/microsoft/sarif-sdk", "SRCROOT") },
                OriginalUriBaseIds = Bases(("SRCROOT", "file:///d:/src/sarif-sdk/")),
                Results = new[] { ResultAt("file:///e:/elsewhere/secret.cs") },
            };

            EmitFinalizeRebaseVisitor visitor = Visit(run);

            visitor.Success.Should().BeFalse();
            string.Join(" ", visitor.Errors).Should().Contain("secret.cs");
        }

        [Fact]
        public void UnmatchedPortableUri_IsInlinedAbsoluteWithNoBase()
        {
            var run = new Run
            {
                VersionControlProvenance = new[] { Vcd("https://github.com/microsoft/sarif-sdk", "SRCROOT") },
                OriginalUriBaseIds = Bases(("SRCROOT", "file:///d:/src/sarif-sdk/")),
                Results = new[] { ResultAt("https://cdn.example.com/vendor/lib.js") },
            };

            EmitFinalizeRebaseVisitor visitor = Visit(run);

            visitor.Success.Should().BeTrue();
            ArtifactLocation location = FirstLocation(run);
            location.Uri.Should().Be(new Uri("https://cdn.example.com/vendor/lib.js", UriKind.Absolute));
            location.UriBaseId.Should().BeNull();
        }

        [Fact]
        public void MissingVersionControlProvenance_Fails()
        {
            var run = new Run
            {
                OriginalUriBaseIds = Bases(("SRCROOT", "file:///d:/src/sarif-sdk/")),
                Results = new[] { ResultAt("file:///d:/src/sarif-sdk/a.cs") },
            };

            EmitFinalizeRebaseVisitor visitor = Visit(run);

            visitor.Success.Should().BeFalse();
            string.Join(" ", visitor.Errors).Should().Contain("versionControlProvenance");
        }

        [Fact]
        public void VcpMissingMappedTo_Fails()
        {
            var vcd = Vcd("https://github.com/microsoft/sarif-sdk", "SRCROOT");
            vcd.MappedTo = null;

            var run = new Run
            {
                VersionControlProvenance = new[] { vcd },
                OriginalUriBaseIds = Bases(("SRCROOT", "file:///d:/src/sarif-sdk/")),
            };

            EmitFinalizeRebaseVisitor visitor = Visit(run);

            visitor.Success.Should().BeFalse();
            string.Join(" ", visitor.Errors).Should().Contain("mappedTo");
        }

        [Fact]
        public void NonGitHubHost_Fails()
        {
            var run = new Run
            {
                VersionControlProvenance = new[]
                {
                    Vcd("https://dev.azure.com/contoso/_git/widgets", "SRCROOT"),
                },
                OriginalUriBaseIds = Bases(("SRCROOT", "file:///d:/src/widgets/")),
            };

            EmitFinalizeRebaseVisitor visitor = Visit(run);

            visitor.Success.Should().BeFalse();
            string.Join(" ", visitor.Errors).Should().Contain("github.com");
        }

        [Fact]
        public void GitHubHostWithNonDefaultPort_Fails()
        {
            var run = new Run
            {
                VersionControlProvenance = new[]
                {
                    Vcd("https://github.com:8443/microsoft/sarif-sdk", "SRCROOT"),
                },
                OriginalUriBaseIds = Bases(("SRCROOT", "file:///d:/src/sarif-sdk/")),
            };

            EmitFinalizeRebaseVisitor visitor = Visit(run);

            visitor.Success.Should().BeFalse();
            string.Join(" ", visitor.Errors).Should().Contain("github.com");
        }

        [Fact]
        public void MappedToCarryingInlineUri_Fails()
        {
            var vcd = Vcd("https://github.com/microsoft/sarif-sdk", "SRCROOT");
            vcd.MappedTo = new ArtifactLocation
            {
                UriBaseId = "SRCROOT",
                Uri = new Uri("file:///d:/src/sarif-sdk/", UriKind.Absolute),
            };

            var run = new Run
            {
                VersionControlProvenance = new[] { vcd },
                OriginalUriBaseIds = Bases(("SRCROOT", "file:///d:/src/sarif-sdk/")),
            };

            EmitFinalizeRebaseVisitor visitor = Visit(run);

            visitor.Success.Should().BeFalse();
            string.Join(" ", visitor.Errors).Should().Contain("mappedTo");
        }

        [Fact]
        public void MissingRevisionId_Fails()
        {
            var run = new Run
            {
                VersionControlProvenance = new[]
                {
                    Vcd("https://github.com/microsoft/sarif-sdk", "SRCROOT", revisionId: null),
                },
                OriginalUriBaseIds = Bases(("SRCROOT", "file:///d:/src/sarif-sdk/")),
            };

            EmitFinalizeRebaseVisitor visitor = Visit(run);

            visitor.Success.Should().BeFalse();
            string.Join(" ", visitor.Errors).Should().Contain("revisionId");
        }

        [Fact]
        public void CyclicBaseChain_Fails()
        {
            var run = new Run
            {
                VersionControlProvenance = new[] { Vcd("https://github.com/microsoft/sarif-sdk", "SRCROOT") },
                OriginalUriBaseIds = new Dictionary<string, ArtifactLocation>(StringComparer.Ordinal)
                {
                    ["SRCROOT"] = new ArtifactLocation { UriBaseId = "OTHER" },
                    ["OTHER"] = new ArtifactLocation { UriBaseId = "SRCROOT" },
                },
            };

            EmitFinalizeRebaseVisitor visitor = Visit(run);

            visitor.Success.Should().BeFalse();
            string.Join(" ", visitor.Errors).Should().Contain("cyclic");
        }

        [Fact]
        public void BaseOnlyArtifactLocation_IsRemappedToOutputBase()
        {
            // A directory/repositoryRoot artifact references its repo by uriBaseId alone (no uri).
            // The retired input base id must be remapped to the minted output base, not left dangling.
            var run = new Run
            {
                VersionControlProvenance = new[]
                {
                    Vcd("https://github.com/contoso/package", "PACKAGE_ROOT"),
                    Vcd("https://github.com/contoso/plugin", "PLUGIN_ROOT"),
                },
                OriginalUriBaseIds = Bases(
                    ("PACKAGE_ROOT", "file:///d:/pkg/"),
                    ("PLUGIN_ROOT", "file:///d:/pkg/plugins/plugin/")),
                Artifacts = new[]
                {
                    new Artifact
                    {
                        Location = new ArtifactLocation { UriBaseId = "PLUGIN_ROOT" },
                        Roles = ArtifactRoles.Directory,
                    },
                },
            };

            EmitFinalizeRebaseVisitor visitor = Visit(run);

            visitor.Success.Should().BeTrue();
            ArtifactLocation artifactLocation = run.Artifacts[0].Location;
            artifactLocation.Uri.Should().BeNull();
            artifactLocation.UriBaseId.Should().Be("SRCROOT_PLUGIN");
            run.OriginalUriBaseIds.Should().ContainKey("SRCROOT_PLUGIN");
        }

        [Fact]
        public void BaseOnlyArtifactLocation_WithUnmappedBaseId_FailsAsDangling()
        {
            var run = new Run
            {
                VersionControlProvenance = new[] { Vcd("https://github.com/microsoft/sarif-sdk", "SRCROOT") },
                OriginalUriBaseIds = Bases(("SRCROOT", "file:///d:/src/sarif-sdk/")),
                Artifacts = new[]
                {
                    new Artifact
                    {
                        Location = new ArtifactLocation { UriBaseId = "UNDECLARED" },
                    },
                },
            };

            EmitFinalizeRebaseVisitor visitor = Visit(run);

            visitor.Success.Should().BeFalse();
            string.Join(" ", visitor.Errors).Should().Contain("UNDECLARED");
        }

        [Fact]
        public void NonResultSurfaces_AreRebased()
        {
            var run = new Run
            {
                VersionControlProvenance = new[] { Vcd("https://github.com/microsoft/sarif-sdk", "SRCROOT") },
                OriginalUriBaseIds = Bases(("SRCROOT", "file:///d:/src/sarif-sdk/")),
                Invocations = new[]
                {
                    new Invocation
                    {
                        WorkingDirectory = new ArtifactLocation { Uri = new Uri("file:///d:/src/sarif-sdk/src/", UriKind.Absolute) },
                        ExecutionSuccessful = true,
                    },
                },
                Results = new[]
                {
                    new Result
                    {
                        RuleId = "NOVEL-x",
                        Message = new Message { Text = "x" },
                        RelatedLocations = new[]
                        {
                            new Location
                            {
                                PhysicalLocation = new PhysicalLocation
                                {
                                    ArtifactLocation = new ArtifactLocation { Uri = new Uri("file:///d:/src/sarif-sdk/related/r.cs", UriKind.Absolute) },
                                },
                            },
                        },
                    },
                },
            };

            EmitFinalizeRebaseVisitor visitor = Visit(run);

            visitor.Success.Should().BeTrue();
            run.Invocations[0].WorkingDirectory.Uri.OriginalString.Should().Be("src/");
            run.Invocations[0].WorkingDirectory.UriBaseId.Should().Be("SRCROOT");
            ArtifactLocation related = run.Results[0].RelatedLocations[0].PhysicalLocation.ArtifactLocation;
            related.Uri.OriginalString.Should().Be("related/r.cs");
            related.UriBaseId.Should().Be("SRCROOT");
        }
    }
}
