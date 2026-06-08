// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

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

        private static VersionControlDetails VcdRaw(string repositoryUri, string uriBaseId, string revisionId = Sha)
            => new VersionControlDetails
            {
                RepositoryUri = new Uri(repositoryUri, UriKind.RelativeOrAbsolute),
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
        public void SingleRepo_MintedBase_CarriesDescriptionLinkingRepositoryAndCommit()
        {
            var run = new Run
            {
                VersionControlProvenance = new[] { Vcd("https://github.com/microsoft/sarif-sdk", "SRCROOT") },
                OriginalUriBaseIds = Bases(("SRCROOT", "file:///d:/src/sarif-sdk/")),
                Results = new[] { ResultAt("file:///d:/src/sarif-sdk/src/Foo.cs") },
            };

            EmitFinalizeRebaseVisitor visitor = Visit(run);

            visitor.Success.Should().BeTrue();
            Message description = run.OriginalUriBaseIds["SRCROOT"].Description;

            // The text is a SARIF embedded link (spec §3.11.6): the anchor names the
            // repository and abbreviated commit (repo@short-sha) and links to the
            // GitHub tree permalink pinned at the full commit. No separate markdown
            // form is minted.
            description.Text
                .Should().Be($"Source root mapped to [sarif-sdk@{Sha.Substring(0, 7)}](https://github.com/microsoft/sarif-sdk/tree/{Sha}).");
            description.Markdown.Should().BeNull();
        }

        [Fact]
        public void SingleAzureDevOpsRepo_MintedBase_LinksToRepoAtRevisionWithVersionQuery()
        {
            var run = new Run
            {
                VersionControlProvenance = new[] { Vcd("https://dev.azure.com/fabrikam/proj/_git/widgets", "SRCROOT") },
                OriginalUriBaseIds = Bases(("SRCROOT", "file:///d:/src/widgets/")),
                Results = new[] { ResultAt("file:///d:/src/widgets/src/Foo.cs") },
            };

            EmitFinalizeRebaseVisitor visitor = Visit(run);

            visitor.Success.Should().BeTrue();
            Message description = run.OriginalUriBaseIds["SRCROOT"].Description;

            // Azure DevOps pins a commit with the ?version=GC<sha> query on the
            // repository root rather than a path segment; the anchor carries the
            // abbreviated commit (repo@short-sha).
            description.Text
                .Should().Be($"Source root mapped to [widgets@{Sha.Substring(0, 7)}](https://dev.azure.com/fabrikam/proj/_git/widgets?version=GC{Sha}).");
            description.Markdown.Should().BeNull();
        }

        [Fact]
        public void MultipleRepos_EachMintedBase_CarriesItsOwnRepositoryDescription()
        {
            var run = new Run
            {
                VersionControlProvenance = new[]
                {
                    Vcd("https://github.com/contoso/widgets", "A"),
                    Vcd("https://dev.azure.com/fabrikam/proj/_git/widgets", "B"),
                },
                OriginalUriBaseIds = Bases(
                    ("A", "file:///d:/gh/"),
                    ("B", "file:///d:/ado/")),
                Results = new[]
                {
                    ResultAt("file:///d:/gh/a.cs"),
                    ResultAt("file:///d:/ado/b.cs"),
                },
            };

            EmitFinalizeRebaseVisitor visitor = Visit(run);

            visitor.Success.Should().BeTrue();

            // The text is a SARIF embedded link (spec §3.11.6) whose anchor names the
            // repository and abbreviated commit (repo@short-sha) and links to the
            // root-at-revision URL, shaped per host: GitHub /tree/<sha>, Azure DevOps
            // ?version=GC<sha>. It must never carry the portable blob/<commit>/
            // segment, and no separate markdown form is minted.
            Message gitHub = run.OriginalUriBaseIds["SRCROOT_WIDGETS"].Description;
            gitHub.Text
                .Should().Be($"Source root mapped to [widgets@{Sha.Substring(0, 7)}](https://github.com/contoso/widgets/tree/{Sha}).")
                .And.NotContain("/blob/");
            gitHub.Markdown.Should().BeNull();

            Message azureDevOps = run.OriginalUriBaseIds["SRCROOT_WIDGETS_2"].Description;
            azureDevOps.Text
                .Should().Be($"Source root mapped to [widgets@{Sha.Substring(0, 7)}](https://dev.azure.com/fabrikam/proj/_git/widgets?version=GC{Sha}).");
            azureDevOps.Markdown.Should().BeNull();
        }

        [Fact]
        public void SingleRepo_ProducerSuppliedDescription_IsPreserved()
        {
            var inputBases = new Dictionary<string, ArtifactLocation>(StringComparer.Ordinal)
            {
                ["SRCROOT"] = new ArtifactLocation
                {
                    Uri = new Uri("file:///d:/src/sarif-sdk/", UriKind.Absolute),
                    Description = new Message { Text = "Producer-authored description." },
                },
            };

            var run = new Run
            {
                VersionControlProvenance = new[] { Vcd("https://github.com/microsoft/sarif-sdk", "SRCROOT") },
                OriginalUriBaseIds = inputBases,
                Results = new[] { ResultAt("file:///d:/src/sarif-sdk/src/Foo.cs") },
            };

            EmitFinalizeRebaseVisitor visitor = Visit(run);

            visitor.Success.Should().BeTrue();
            Message description = run.OriginalUriBaseIds["SRCROOT"].Description;
            description.Text.Should().Be("Producer-authored description.");
            description.Markdown.Should().BeNull();
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
        public void UnsupportedHost_Fails()
        {
            var run = new Run
            {
                VersionControlProvenance = new[]
                {
                    Vcd("https://gitlab.com/contoso/widgets", "SRCROOT"),
                },
                OriginalUriBaseIds = Bases(("SRCROOT", "file:///d:/src/widgets/")),
            };

            EmitFinalizeRebaseVisitor visitor = Visit(run);

            visitor.Success.Should().BeFalse();
            string.Join(" ", visitor.Errors).Should().Contain("not a supported host");
        }

        [Fact]
        public void Bitbucket_IsRejected()
        {
            var run = new Run
            {
                VersionControlProvenance = new[]
                {
                    Vcd("https://bitbucket.org/contoso/widgets", "SRCROOT"),
                },
                OriginalUriBaseIds = Bases(("SRCROOT", "file:///d:/src/widgets/")),
            };

            EmitFinalizeRebaseVisitor visitor = Visit(run);

            visitor.Success.Should().BeFalse();
            string.Join(" ", visitor.Errors).Should().Contain("not a supported host");
        }

        [Fact]
        public void GitHubGheComHost_DerivesBlobPermalink()
        {
            var run = new Run
            {
                VersionControlProvenance = new[]
                {
                    Vcd("https://octocorp.ghe.com/octo/widgets", "SRCROOT"),
                },
                OriginalUriBaseIds = Bases(("SRCROOT", "file:///d:/src/widgets/")),
                Results = new[] { ResultAt("file:///d:/src/widgets/src/Foo.cs") },
            };

            EmitFinalizeRebaseVisitor visitor = Visit(run);

            visitor.Success.Should().BeTrue();
            FirstLocation(run).Uri.OriginalString.Should().Be("src/Foo.cs");
            run.OriginalUriBaseIds["SRCROOT"].Uri
                .Should().Be(new Uri("https://octocorp.ghe.com/octo/widgets/blob/" + Sha + "/", UriKind.Absolute));
        }

        [Fact]
        public void GitHubEnterpriseServer_CustomHost_IsRejected()
        {
            var run = new Run
            {
                VersionControlProvenance = new[]
                {
                    Vcd("https://github.contoso.com/octo/widgets", "SRCROOT"),
                },
                OriginalUriBaseIds = Bases(("SRCROOT", "file:///d:/src/widgets/")),
            };

            EmitFinalizeRebaseVisitor visitor = Visit(run);

            visitor.Success.Should().BeFalse();
            string.Join(" ", visitor.Errors).Should().Contain("not a supported host");
        }

        [Fact]
        public void GitHub_ScpCloneUrl_IsNormalizedToBlobPermalink()
        {
            var run = new Run
            {
                VersionControlProvenance = new[]
                {
                    VcdRaw("git@github.com:octo/widgets.git", "SRCROOT"),
                },
                OriginalUriBaseIds = Bases(("SRCROOT", "file:///d:/src/widgets/")),
            };

            EmitFinalizeRebaseVisitor visitor = Visit(run);

            visitor.Success.Should().BeTrue();
            run.OriginalUriBaseIds["SRCROOT"].Uri
                .Should().Be(new Uri("https://github.com/octo/widgets/blob/" + Sha + "/", UriKind.Absolute));
        }

        [Fact]
        public void GitHub_SshCloneUrl_IsNormalizedToBlobPermalink()
        {
            var run = new Run
            {
                VersionControlProvenance = new[]
                {
                    VcdRaw("ssh://git@github.com/octo/widgets.git", "SRCROOT"),
                },
                OriginalUriBaseIds = Bases(("SRCROOT", "file:///d:/src/widgets/")),
            };

            EmitFinalizeRebaseVisitor visitor = Visit(run);

            visitor.Success.Should().BeTrue();
            run.OriginalUriBaseIds["SRCROOT"].Uri
                .Should().Be(new Uri("https://github.com/octo/widgets/blob/" + Sha + "/", UriKind.Absolute));
        }

        [Fact]
        public void AzureDevOpsSsh_IsRejectedWithHttpsGuidance()
        {
            var run = new Run
            {
                VersionControlProvenance = new[]
                {
                    VcdRaw("git@ssh.dev.azure.com:v3/contoso/MyProject/widgets", "SRCROOT"),
                },
                OriginalUriBaseIds = Bases(("SRCROOT", "file:///d:/src/widgets/")),
            };

            EmitFinalizeRebaseVisitor visitor = Visit(run);

            visitor.Success.Should().BeFalse();
            string.Join(" ", visitor.Errors).Should().Contain("ssh repositoryUri normalization is not supported");
        }

        [Fact]
        public void AzureDevOps_VisualStudioComHost_IsRejected()
        {
            var run = new Run
            {
                VersionControlProvenance = new[]
                {
                    Vcd("https://contoso.visualstudio.com/MyProject/_git/widgets", "SRCROOT"),
                },
                OriginalUriBaseIds = Bases(("SRCROOT", "file:///d:/src/widgets/")),
            };

            EmitFinalizeRebaseVisitor visitor = Visit(run);

            visitor.Success.Should().BeFalse();
            string.Join(" ", visitor.Errors).Should().Contain("legacy Azure DevOps host form is not supported");
        }

        [Fact]
        public void AzureDevOps_OnPremCollectionHost_IsRejected()
        {
            var run = new Run
            {
                VersionControlProvenance = new[]
                {
                    Vcd("https://tfs.contoso.com/tfs/DefaultCollection/MyProject/_git/widgets", "SRCROOT"),
                },
                OriginalUriBaseIds = Bases(("SRCROOT", "file:///d:/src/widgets/")),
            };

            EmitFinalizeRebaseVisitor visitor = Visit(run);

            visitor.Success.Should().BeFalse();
            string.Join(" ", visitor.Errors).Should().Contain("not a supported host");
        }

        [Fact]
        public void GitHub_CredentialBearingHttpsUri_IsRejected()
        {
            var run = new Run
            {
                VersionControlProvenance = new[]
                {
                    Vcd("https://x-access-token@github.com/octo/widgets", "SRCROOT"),
                },
                OriginalUriBaseIds = Bases(("SRCROOT", "file:///d:/src/widgets/")),
            };

            EmitFinalizeRebaseVisitor visitor = Visit(run);

            visitor.Success.Should().BeFalse();
            string joined = string.Join(" ", visitor.Errors);
            joined.Should().Contain("must not carry embedded credentials");
            joined.Should().NotContain("x-access-token");
        }

        [Fact]
        public void GitHub_ScpCloneUrlWithSmuggledHost_IsRejected()
        {
            var run = new Run
            {
                VersionControlProvenance = new[]
                {
                    VcdRaw("git@evil@github.com:octo/widgets.git", "SRCROOT"),
                },
                OriginalUriBaseIds = Bases(("SRCROOT", "file:///d:/src/widgets/")),
            };

            EmitFinalizeRebaseVisitor visitor = Visit(run);

            visitor.Success.Should().BeFalse();
        }

        [Fact]
        public void GitHub_ScpCloneUrlWithPasswordAuthority_IsRejectedWithoutEchoingSecret()
        {
            var run = new Run
            {
                VersionControlProvenance = new[]
                {
                    VcdRaw("user:s3cr3t@github.com:octo/widgets.git", "SRCROOT"),
                },
                OriginalUriBaseIds = Bases(("SRCROOT", "file:///d:/src/widgets/")),
            };

            EmitFinalizeRebaseVisitor visitor = Visit(run);

            visitor.Success.Should().BeFalse();
            string.Join(" ", visitor.Errors).Should().NotContain("s3cr3t");
        }

        [Fact]
        public void GitHub_SshCloneUrlWithExplicitPort_IsRejected()
        {
            var run = new Run
            {
                VersionControlProvenance = new[]
                {
                    VcdRaw("ssh://git@github.com:2222/octo/widgets.git", "SRCROOT"),
                },
                OriginalUriBaseIds = Bases(("SRCROOT", "file:///d:/src/widgets/")),
            };

            EmitFinalizeRebaseVisitor visitor = Visit(run);

            visitor.Success.Should().BeFalse();
            string.Join(" ", visitor.Errors).Should().Contain("ssh repositoryUri must use the default port");
        }

        [Fact]
        public void GitHub_SshCloneUrlWithQuery_IsRejected()
        {
            var run = new Run
            {
                VersionControlProvenance = new[]
                {
                    VcdRaw("ssh://git@github.com/octo/widgets.git?token=secret", "SRCROOT"),
                },
                OriginalUriBaseIds = Bases(("SRCROOT", "file:///d:/src/widgets/")),
            };

            EmitFinalizeRebaseVisitor visitor = Visit(run);

            visitor.Success.Should().BeFalse();
            string.Join(" ", visitor.Errors).Should().Contain("no query or fragment");
        }

        [Fact]
        public void AzureDevOps_CredentialBearingHttpsUri_IsRejected()
        {
            var run = new Run
            {
                VersionControlProvenance = new[]
                {
                    Vcd("https://contoso@dev.azure.com/contoso/MyProject/_git/widgets", "SRCROOT"),
                },
                OriginalUriBaseIds = Bases(("SRCROOT", "file:///d:/src/widgets/")),
            };

            EmitFinalizeRebaseVisitor visitor = Visit(run);

            visitor.Success.Should().BeFalse();
            string.Join(" ", visitor.Errors).Should().Contain("must not carry embedded credentials");
        }

        [Fact]
        public void AzureDevOps_TrailingSegmentAfterRepo_Fails()
        {
            var run = new Run
            {
                VersionControlProvenance = new[]
                {
                    Vcd("https://dev.azure.com/contoso/MyProject/_git/widgets/extra", "SRCROOT"),
                },
                OriginalUriBaseIds = Bases(("SRCROOT", "file:///d:/src/widgets/")),
            };

            EmitFinalizeRebaseVisitor visitor = Visit(run);

            visitor.Success.Should().BeFalse();
            string.Join(" ", visitor.Errors).Should().Contain("azure devops repositoryUri must take the form");
        }

        [Fact]
        public void AzureDevOps_SingleRepo_DerivesRepositoryRootBase()
        {
            var run = new Run
            {
                VersionControlProvenance = new[]
                {
                    Vcd("https://dev.azure.com/contoso/MyProject/_git/widgets", "SRCROOT"),
                },
                OriginalUriBaseIds = Bases(("SRCROOT", "file:///d:/src/widgets/")),
                Results = new[] { ResultAt("file:///d:/src/widgets/src/Foo.cs") },
            };

            EmitFinalizeRebaseVisitor visitor = Visit(run);

            visitor.Success.Should().BeTrue();
            ArtifactLocation location = FirstLocation(run);
            location.Uri.OriginalString.Should().Be("src/Foo.cs");
            location.UriBaseId.Should().Be("SRCROOT");
            run.OriginalUriBaseIds["SRCROOT"].Uri
                .Should().Be(new Uri("https://dev.azure.com/contoso/MyProject/_git/widgets/", UriKind.Absolute));
        }

        [Fact]
        public void AzureDevOps_ProjectAndRepoWithSpaces_PreserveEncodedSegments()
        {
            var run = new Run
            {
                VersionControlProvenance = new[]
                {
                    Vcd("https://dev.azure.com/contoso/My%20Project/_git/My%20Repo", "SRCROOT"),
                },
                OriginalUriBaseIds = Bases(("SRCROOT", "file:///d:/src/widgets/")),
            };

            EmitFinalizeRebaseVisitor visitor = Visit(run);

            visitor.Success.Should().BeTrue();
            run.OriginalUriBaseIds["SRCROOT"].Uri.OriginalString
                .Should().Be("https://dev.azure.com/contoso/My%20Project/_git/My%20Repo/");
        }

        [Fact]
        public void AzureDevOps_DotGitSuffix_IsStripped()
        {
            var run = new Run
            {
                VersionControlProvenance = new[]
                {
                    Vcd("https://dev.azure.com/contoso/proj/_git/widgets.git", "SRCROOT"),
                },
                OriginalUriBaseIds = Bases(("SRCROOT", "file:///d:/src/widgets/")),
            };

            EmitFinalizeRebaseVisitor visitor = Visit(run);

            visitor.Success.Should().BeTrue();
            run.OriginalUriBaseIds["SRCROOT"].Uri
                .Should().Be(new Uri("https://dev.azure.com/contoso/proj/_git/widgets/", UriKind.Absolute));
        }

        [Fact]
        public void AzureDevOps_MalformedPath_Fails()
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
            string.Join(" ", visitor.Errors).Should().Contain("azure devops repositoryUri must take the form");
        }

        [Fact]
        public void AzureDevOps_PreservesRevisionIdAndDoesNotEmbedItInRoot()
        {
            var run = new Run
            {
                VersionControlProvenance = new[]
                {
                    Vcd("https://dev.azure.com/contoso/proj/_git/widgets", "SRCROOT"),
                },
                OriginalUriBaseIds = Bases(("SRCROOT", "file:///d:/src/widgets/")),
            };

            EmitFinalizeRebaseVisitor visitor = Visit(run);

            visitor.Success.Should().BeTrue();
            run.VersionControlProvenance[0].RevisionId.Should().Be(Sha);
            run.OriginalUriBaseIds["SRCROOT"].Uri.AbsoluteUri.Should().NotContain(Sha);
        }

        [Fact]
        public void RepositoryUriWithQuery_Fails()
        {
            var run = new Run
            {
                VersionControlProvenance = new[]
                {
                    Vcd("https://dev.azure.com/contoso/proj/_git/widgets?path=/x&version=GCabc", "SRCROOT"),
                },
                OriginalUriBaseIds = Bases(("SRCROOT", "file:///d:/src/widgets/")),
            };

            EmitFinalizeRebaseVisitor visitor = Visit(run);

            visitor.Success.Should().BeFalse();
            string.Join(" ", visitor.Errors).Should().Contain("query or fragment");
        }

        [Fact]
        public void RepositoryUriWithCredentials_FailsWithoutLeakingSecret()
        {
            var run = new Run
            {
                VersionControlProvenance = new[]
                {
                    Vcd("https://user:s3cr3t-token@github.com/contoso/widgets", "SRCROOT"),
                },
                OriginalUriBaseIds = Bases(("SRCROOT", "file:///d:/src/widgets/")),
            };

            EmitFinalizeRebaseVisitor visitor = Visit(run);

            visitor.Success.Should().BeFalse();
            string joined = string.Join(" ", visitor.Errors);
            joined.Should().Contain("credentials");
            joined.Should().NotContain("s3cr3t-token");
        }

        [Fact]
        public void MixedHosts_GitHubAndAzureDevOps_MintDistinctBases()
        {
            var run = new Run
            {
                VersionControlProvenance = new[]
                {
                    Vcd("https://github.com/contoso/widgets", "A"),
                    Vcd("https://dev.azure.com/fabrikam/proj/_git/widgets", "B"),
                },
                OriginalUriBaseIds = Bases(
                    ("A", "file:///d:/gh/"),
                    ("B", "file:///d:/ado/")),
                Results = new[]
                {
                    ResultAt("file:///d:/gh/a.cs"),
                    ResultAt("file:///d:/ado/b.cs"),
                },
            };

            EmitFinalizeRebaseVisitor visitor = Visit(run);

            visitor.Success.Should().BeTrue();

            ArtifactLocation a = run.Results[0].Locations[0].PhysicalLocation.ArtifactLocation;
            a.Uri.OriginalString.Should().Be("a.cs");
            a.UriBaseId.Should().Be("SRCROOT_WIDGETS");

            ArtifactLocation b = run.Results[1].Locations[0].PhysicalLocation.ArtifactLocation;
            b.Uri.OriginalString.Should().Be("b.cs");
            b.UriBaseId.Should().Be("SRCROOT_WIDGETS_2");

            run.OriginalUriBaseIds["SRCROOT_WIDGETS"].Uri
                .Should().Be(new Uri($"https://github.com/contoso/widgets/blob/{Sha}/", UriKind.Absolute));
            run.OriginalUriBaseIds["SRCROOT_WIDGETS_2"].Uri
                .Should().Be(new Uri("https://dev.azure.com/fabrikam/proj/_git/widgets/", UriKind.Absolute));
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
        public void VcpMissingMappedTo_ErrorNamesExistingSrcRootBinding()
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
            string joined = string.Join(" ", visitor.Errors);
            joined.Should().Contain("already declares 'SRCROOT'");
            joined.Should().Contain("\"uriBaseId\": \"SRCROOT\"");
            joined.Should().Contain("SARIF2007");
        }

        [Fact]
        public void VcpMissingMappedTo_NoBases_ErrorInstructsDeclaringSrcRoot()
        {
            var vcd = Vcd("https://github.com/microsoft/sarif-sdk", "SRCROOT");
            vcd.MappedTo = null;

            var run = new Run
            {
                VersionControlProvenance = new[] { vcd },
                OriginalUriBaseIds = null,
            };

            EmitFinalizeRebaseVisitor visitor = Visit(run);

            visitor.Success.Should().BeFalse();
            string joined = string.Join(" ", visitor.Errors);
            joined.Should().Contain("Declare an originalUriBaseIds entry");
            joined.Should().Contain("conventionally 'SRCROOT'");
        }

        [Fact]
        public void VcpMissingMappedTo_NonSrcRootBase_ErrorNamesDeclaredBase()
        {
            var vcd = Vcd("https://github.com/microsoft/sarif-sdk", "CHECKOUT");
            vcd.MappedTo = null;

            var run = new Run
            {
                VersionControlProvenance = new[] { vcd },
                OriginalUriBaseIds = Bases(("CHECKOUT", "file:///d:/src/sarif-sdk/")),
            };

            EmitFinalizeRebaseVisitor visitor = Visit(run);

            visitor.Success.Should().BeFalse();
            string joined = string.Join(" ", visitor.Errors);
            joined.Should().Contain("declares 'CHECKOUT'");
            joined.Should().Contain("set mappedTo.uriBaseId to the entry for the repository root");
        }

        [Fact]
        public void MappedToUriBaseId_NotResolvingToLocalFile_ErrorExplainsTransientRebase()
        {
            // SRCROOT is bound to a portable https uri rather than the local file:// checkout that
            // finalize resolves snippets against. The diagnostic must teach that the local path is
            // used transiently and rebased out of the finalized SARIF.
            var run = new Run
            {
                VersionControlProvenance = new[]
                {
                    Vcd("https://github.com/microsoft/sarif-sdk", "SRCROOT"),
                },
                OriginalUriBaseIds = Bases(("SRCROOT", "https://github.com/microsoft/sarif-sdk/")),
            };

            EmitFinalizeRebaseVisitor visitor = Visit(run);

            visitor.Success.Should().BeFalse();
            string joined = string.Join(" ", visitor.Errors);
            joined.Should().Contain("file:///path/to/checkout/");
            joined.Should().Contain("not retained in the finalized SARIF");
            joined.Should().NotContain("--srcroot");
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
