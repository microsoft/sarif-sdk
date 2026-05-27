// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

using FluentAssertions;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class GitHubActionsContextTests
    {
        private const string GoodServerUrl = "https://github.com";
        private const string GoodRepository = "microsoft/sarif-sdk";
        private const string GoodSha = "0123456789abcdef0123456789abcdef01234567";

        private static FakeEnvironmentVariableGetter CompleteGhaEnv() => new FakeEnvironmentVariableGetter()
            .With(GitHubActionsContext.GitHubActionsEnvVar, "true")
            .With(GitHubActionsContext.ServerUrlEnvVar, GoodServerUrl)
            .With(GitHubActionsContext.RepositoryEnvVar, GoodRepository)
            .With(GitHubActionsContext.ShaEnvVar, GoodSha)
            .With(GitHubActionsContext.RefNameEnvVar, "main");

        [Fact]
        public void TryDetect_NullEnvironment_Throws()
        {
            Action act = () => GitHubActionsContext.TryDetect(null, out _, out _);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void TryDetect_ReturnsNoneOutsideGitHubActions()
        {
            // Gate is GITHUB_ACTIONS=true (case-insensitive). Absence or any other value
            // returns None silently — non-GHA CI systems that happen to populate some
            // GITHUB_* vars should not trigger stamping.
            var noGate = new FakeEnvironmentVariableGetter()
                .With(GitHubActionsContext.ServerUrlEnvVar, GoodServerUrl)
                .With(GitHubActionsContext.RepositoryEnvVar, GoodRepository);
            GitHubActionsContext.TryDetect(noGate, out GitHubActionsContext ctx1, out string err1)
                .Should().Be(GitHubActionsContext.DetectionState.None);
            ctx1.Should().BeNull();
            err1.Should().BeNull();

            var gateFalse = new FakeEnvironmentVariableGetter()
                .With(GitHubActionsContext.GitHubActionsEnvVar, "false");
            GitHubActionsContext.TryDetect(gateFalse, out GitHubActionsContext ctx2, out string err2)
                .Should().Be(GitHubActionsContext.DetectionState.None);
            ctx2.Should().BeNull();
            err2.Should().BeNull();
        }

        [Fact]
        public void TryDetect_AllFieldsWellFormed_ReturnsCompleteWithComposedUri()
        {
            GitHubActionsContext.DetectionState state =
                GitHubActionsContext.TryDetect(CompleteGhaEnv(), out GitHubActionsContext ctx, out string error);

            state.Should().Be(GitHubActionsContext.DetectionState.Complete);
            error.Should().BeNull();
            ctx.RepositoryUri.AbsoluteUri.Should().Be("https://github.com/microsoft/sarif-sdk");
            ctx.RevisionId.Should().Be(GoodSha);
            ctx.BranchShortName.Should().Be("main");
        }

        [Fact]
        public void TryDetect_OnlyGateSet_ReturnsCompleteWithNullFields()
        {
            // GITHUB_ACTIONS=true with no other vars is Complete-but-empty. The verb's VCP
            // stamper is a no-op for an empty triple; we don't want to inflate the partial
            // gate just because a hand-built env stops at the sentinel.
            var gateOnly = new FakeEnvironmentVariableGetter()
                .With(GitHubActionsContext.GitHubActionsEnvVar, "true");

            GitHubActionsContext.DetectionState state =
                GitHubActionsContext.TryDetect(gateOnly, out GitHubActionsContext ctx, out string error);

            state.Should().Be(GitHubActionsContext.DetectionState.Complete);
            error.Should().BeNull();
            ctx.RepositoryUri.Should().BeNull();
            ctx.RevisionId.Should().BeNull();
            ctx.BranchShortName.Should().BeNull();
        }

        [Fact]
        public void TryDetect_ServerUrlWithoutRepository_ReturnsPartial()
        {
            // The repository URI requires BOTH server and repository to derive. Either
            // present without the other is a misconfiguration signal.
            FakeEnvironmentVariableGetter env = CompleteGhaEnv();
            env.With(GitHubActionsContext.RepositoryEnvVar, string.Empty);

            GitHubActionsContext.DetectionState state =
                GitHubActionsContext.TryDetect(env, out _, out string error);

            state.Should().Be(GitHubActionsContext.DetectionState.Partial);
            error.Should().Contain(GitHubActionsContext.ServerUrlEnvVar);
            error.Should().Contain(GitHubActionsContext.RepositoryEnvVar);
        }

        [Fact]
        public void TryDetect_NonHttpServerUrl_ReturnsPartial()
        {
            FakeEnvironmentVariableGetter env = CompleteGhaEnv();
            env.With(GitHubActionsContext.ServerUrlEnvVar, "ftp://github.com");

            GitHubActionsContext.DetectionState state =
                GitHubActionsContext.TryDetect(env, out _, out string error);

            state.Should().Be(GitHubActionsContext.DetectionState.Partial);
            error.Should().Contain(GitHubActionsContext.ServerUrlEnvVar);
        }

        [Fact]
        public void TryDetect_MalformedSha_ReturnsPartial()
        {
            FakeEnvironmentVariableGetter env = CompleteGhaEnv();
            env.With(GitHubActionsContext.ShaEnvVar, "not-a-sha");

            GitHubActionsContext.DetectionState state =
                GitHubActionsContext.TryDetect(env, out _, out string error);

            state.Should().Be(GitHubActionsContext.DetectionState.Partial);
            error.Should().Contain(GitHubActionsContext.ShaEnvVar);
            error.Should().Contain("revision id");
        }

        [Fact]
        public void TryDetect_AbbreviatedSha_Accepted()
        {
            FakeEnvironmentVariableGetter env = CompleteGhaEnv();
            env.With(GitHubActionsContext.ShaEnvVar, "deadbee");

            GitHubActionsContext.DetectionState state =
                GitHubActionsContext.TryDetect(env, out GitHubActionsContext ctx, out _);

            state.Should().Be(GitHubActionsContext.DetectionState.Complete);
            ctx.RevisionId.Should().Be("deadbee");
        }

        [Fact]
        public void TryDetect_PrefersRefNameOverRef()
        {
            // GITHUB_REF_NAME is the documented "use this" var. When both are set, REF_NAME
            // wins; we do not strip REF as a cross-check (per GH docs they're derived from
            // each other and always agree in real runs).
            FakeEnvironmentVariableGetter env = CompleteGhaEnv();
            env.With(GitHubActionsContext.RefNameEnvVar, "feature/x")
               .With(GitHubActionsContext.RefEnvVar, "refs/heads/main");

            GitHubActionsContext.TryDetect(env, out GitHubActionsContext ctx, out _);

            ctx.BranchShortName.Should().Be("feature/x");
        }

        [Fact]
        public void TryDetect_FallsBackToRefWhenRefNameAbsent()
        {
            // Without GITHUB_REF_NAME, we strip refs/<class>/ from GITHUB_REF to derive
            // the short name.
            FakeEnvironmentVariableGetter env = CompleteGhaEnv();
            env.With(GitHubActionsContext.RefNameEnvVar, string.Empty)
               .With(GitHubActionsContext.RefEnvVar, "refs/pull/42/merge");

            GitHubActionsContext.TryDetect(env, out GitHubActionsContext ctx, out _);

            ctx.BranchShortName.Should().Be("42/merge");
        }

        [Fact]
        public void TryDetect_ServerUrlWithTrailingSlash_NormalizesUri()
        {
            // GH-hosted runners set GITHUB_SERVER_URL without a trailing slash but custom
            // runners and hand-built envs may include one. Normalize so we don't produce
            // 'https://github.com//org/repo'.
            FakeEnvironmentVariableGetter env = CompleteGhaEnv();
            env.With(GitHubActionsContext.ServerUrlEnvVar, "https://github.com/");

            GitHubActionsContext.TryDetect(env, out GitHubActionsContext ctx, out _);

            ctx.RepositoryUri.AbsoluteUri.Should().Be("https://github.com/microsoft/sarif-sdk");
        }
    }
}
