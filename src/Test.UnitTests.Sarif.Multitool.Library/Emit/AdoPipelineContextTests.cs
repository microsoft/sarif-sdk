// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

using FluentAssertions;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class AdoPipelineContextTests
    {
        private const string GoodOrgUri = "https://dev.azure.com/contoso/";
        private const string GoodProjectId = "11111111-1111-1111-1111-111111111111";
        private const string GoodPhaseId = "22222222-2222-2222-2222-222222222222";
        private const string GoodSecondaryPhaseId = "33333333-3333-3333-3333-333333333333";

        private static FakeEnvironmentVariableGetter CompleteEnv() => new FakeEnvironmentVariableGetter()
            .With(AdoPipelineContext.TfBuildEnvVar, "True")
            .With(AdoPipelineContext.CollectionUriEnvVar, GoodOrgUri)
            .With(AdoPipelineContext.TeamProjectIdEnvVar, GoodProjectId)
            .With(AdoPipelineContext.BuildDefinitionIdPrimaryEnvVar, "1234")
            .With(AdoPipelineContext.BuildDefinitionNameEnvVar, "Nightly Build")
            .With(AdoPipelineContext.BuildIdEnvVar, "98765")
            .With(AdoPipelineContext.PhaseIdPrimaryEnvVar, GoodPhaseId)
            .With(AdoPipelineContext.PhaseNamePrimaryEnvVar, "Build")
            .With(AdoPipelineContext.SourceBranchEnvVar, "refs/heads/main");

        [Fact]
        public void TryDetect_NullEnvironment_Throws()
        {
            Action act = () => AdoPipelineContext.TryDetect(null, out _, out _);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void TryDetect_ReturnsNoneOutsideAdoPipeline()
        {
            // None state covers two gating conditions: TF_BUILD unset (regardless of whether
            // other BUILD_* vars are populated by a non-ADO CI), and TF_BUILD set without any
            // of the pipeline identity vars (a degenerate state we treat as silent skip rather
            // than fail).
            var noTfBuild = new FakeEnvironmentVariableGetter()
                .With(AdoPipelineContext.CollectionUriEnvVar, GoodOrgUri)
                .With(AdoPipelineContext.BuildIdEnvVar, "1");
            AdoPipelineContext.TryDetect(noTfBuild, out AdoPipelineContext ctx1, out string err1)
                .Should().Be(AdoPipelineContext.DetectionState.None);
            ctx1.Should().BeNull();
            err1.Should().BeNull();

            var tfBuildAlone = new FakeEnvironmentVariableGetter()
                .With(AdoPipelineContext.TfBuildEnvVar, "True");
            AdoPipelineContext.TryDetect(tfBuildAlone, out AdoPipelineContext ctx2, out string err2)
                .Should().Be(AdoPipelineContext.DetectionState.None);
            ctx2.Should().BeNull();
            err2.Should().BeNull();
        }

        [Fact]
        public void TryDetect_AllRequiredVarsWellFormed_ReturnsComplete()
        {
            AdoPipelineContext.DetectionState state =
                AdoPipelineContext.TryDetect(CompleteEnv(), out AdoPipelineContext ctx, out string error);

            state.Should().Be(AdoPipelineContext.DetectionState.Complete);
            error.Should().BeNull();

            ctx.OrganizationName.Should().Be("contoso");
            ctx.ProjectId.Should().Be(Guid.Parse(GoodProjectId));
            ctx.BuildDefinitionId.Should().Be(1234);
            ctx.BuildDefinitionName.Should().Be("Nightly Build");
            ctx.BuildId.Should().Be(98765);
            ctx.PhaseId.Should().Be(Guid.Parse(GoodPhaseId));
            ctx.PhaseName.Should().Be("Build");
            ctx.BranchRef.Should().Be("refs/heads/main");
        }

        [Fact]
        public void TryDetect_OneRequiredVarMissing_ReturnsPartialAndNamesIt()
        {
            FakeEnvironmentVariableGetter env = CompleteEnv();
            env.With(AdoPipelineContext.BuildDefinitionNameEnvVar, null);

            AdoPipelineContext.DetectionState state =
                AdoPipelineContext.TryDetect(env, out AdoPipelineContext ctx, out string error);

            state.Should().Be(AdoPipelineContext.DetectionState.Partial);
            ctx.Should().BeNull();
            error.Should().NotBeNullOrWhiteSpace();
            error.Should().Contain(AdoPipelineContext.BuildDefinitionNameEnvVar);
        }

        [Fact]
        public void TryDetect_RejectsMalformedOrZeroBuildDefinitionId()
        {
            // Both non-integer values and the literal "0" must fail the positive-int guard
            // (zero is invalid per GHAzDO1019).
            FakeEnvironmentVariableGetter badInt = CompleteEnv();
            badInt.With(AdoPipelineContext.BuildDefinitionIdPrimaryEnvVar, "abc");
            AdoPipelineContext.TryDetect(badInt, out _, out string err1)
                .Should().Be(AdoPipelineContext.DetectionState.Partial);
            err1.Should().Contain(AdoPipelineContext.BuildDefinitionIdPrimaryEnvVar);
            err1.Should().Contain("'abc'");

            FakeEnvironmentVariableGetter zero = CompleteEnv();
            zero.With(AdoPipelineContext.BuildDefinitionIdPrimaryEnvVar, "0");
            AdoPipelineContext.TryDetect(zero, out _, out string err2)
                .Should().Be(AdoPipelineContext.DetectionState.Partial);
            err2.Should().Contain("positive");
        }

        [Fact]
        public void TryDetect_RejectsMalformedOrEmptyTeamProjectId()
        {
            // Both non-canonical GUID syntax and the literal empty GUID must fail.
            FakeEnvironmentVariableGetter badGuid = CompleteEnv();
            badGuid.With(AdoPipelineContext.TeamProjectIdEnvVar, "not-a-guid");
            AdoPipelineContext.TryDetect(badGuid, out _, out string err1)
                .Should().Be(AdoPipelineContext.DetectionState.Partial);
            err1.Should().Contain(AdoPipelineContext.TeamProjectIdEnvVar);
            err1.Should().Contain("GUID");

            FakeEnvironmentVariableGetter emptyGuid = CompleteEnv();
            emptyGuid.With(AdoPipelineContext.TeamProjectIdEnvVar, Guid.Empty.ToString("D"));
            AdoPipelineContext.TryDetect(emptyGuid, out _, out string err2)
                .Should().Be(AdoPipelineContext.DetectionState.Partial);
            err2.Should().Contain("Guid.Empty");
        }

        [Fact]
        public void TryDetect_BuildDefinitionIdPrimaryAndFallbackAgree_ReturnsComplete()
        {
            FakeEnvironmentVariableGetter env = CompleteEnv();
            env.With(AdoPipelineContext.BuildDefinitionIdFallbackEnvVar, "1234"); // same value

            AdoPipelineContext.DetectionState state =
                AdoPipelineContext.TryDetect(env, out AdoPipelineContext ctx, out _);

            state.Should().Be(AdoPipelineContext.DetectionState.Complete);
            ctx.BuildDefinitionId.Should().Be(1234);
        }

        [Fact]
        public void TryDetect_BuildDefinitionIdPrimaryAndFallbackDisagree_ReturnsPartial()
        {
            FakeEnvironmentVariableGetter env = CompleteEnv();
            env.With(AdoPipelineContext.BuildDefinitionIdFallbackEnvVar, "5678"); // different

            AdoPipelineContext.DetectionState state =
                AdoPipelineContext.TryDetect(env, out _, out string error);

            state.Should().Be(AdoPipelineContext.DetectionState.Partial);
            error.Should().Contain("disagree");
        }

        [Fact]
        public void TryDetect_BuildDefinitionIdMissingPrimaryFallsBackToSystemDefinitionId()
        {
            FakeEnvironmentVariableGetter env = CompleteEnv();
            env.With(AdoPipelineContext.BuildDefinitionIdPrimaryEnvVar, null);
            env.With(AdoPipelineContext.BuildDefinitionIdFallbackEnvVar, "4242");

            AdoPipelineContext.DetectionState state =
                AdoPipelineContext.TryDetect(env, out AdoPipelineContext ctx, out _);

            state.Should().Be(AdoPipelineContext.DetectionState.Complete);
            ctx.BuildDefinitionId.Should().Be(4242);
        }

        [Fact]
        public void TryDetect_PhaseIdPrimaryWins()
        {
            FakeEnvironmentVariableGetter env = CompleteEnv();
            env.With(AdoPipelineContext.PhaseIdFallbackEnvVar, GoodSecondaryPhaseId);

            AdoPipelineContext.DetectionState state =
                AdoPipelineContext.TryDetect(env, out AdoPipelineContext ctx, out _);

            // Phase/Job are semantically distinct in YAML so disagreement does NOT
            // fail — primary wins.
            state.Should().Be(AdoPipelineContext.DetectionState.Complete);
            ctx.PhaseId.Should().Be(Guid.Parse(GoodPhaseId));
        }

        [Fact]
        public void TryDetect_PhaseIdFallsBackToSystemJobId()
        {
            FakeEnvironmentVariableGetter env = CompleteEnv();
            env.With(AdoPipelineContext.PhaseIdPrimaryEnvVar, null);
            env.With(AdoPipelineContext.PhaseIdFallbackEnvVar, GoodSecondaryPhaseId);
            env.With(AdoPipelineContext.PhaseNamePrimaryEnvVar, null);
            env.With(AdoPipelineContext.PhaseNameFallbackEnvVar, "BuildJob");

            AdoPipelineContext.DetectionState state =
                AdoPipelineContext.TryDetect(env, out AdoPipelineContext ctx, out _);

            state.Should().Be(AdoPipelineContext.DetectionState.Complete);
            ctx.PhaseId.Should().Be(Guid.Parse(GoodSecondaryPhaseId));
            ctx.PhaseName.Should().Be("BuildJob");
        }

        [Fact]
        public void TryDetect_PassesBareBranchThrough()
        {
            // BUILD_SOURCEBRANCH is documented to always be long-form, but the detector
            // passes whatever value the env publishes through as-is. AdvSec accepts both
            // long and short branch forms in VCP.
            FakeEnvironmentVariableGetter env = CompleteEnv();
            env.With(AdoPipelineContext.SourceBranchEnvVar, "feature/x");

            AdoPipelineContext.DetectionState state =
                AdoPipelineContext.TryDetect(env, out AdoPipelineContext ctx, out _);

            state.Should().Be(AdoPipelineContext.DetectionState.Complete);
            ctx.BranchRef.Should().Be("feature/x");
        }

        [Fact]
        public void TryDetect_VcpEnvVarsAbsent_LeavesVcpPropertiesNull()
        {
            // Optional env vars: absence does NOT degrade Complete -> Partial; the VCP
            // enrichment path is just a no-op for this context.
            AdoPipelineContext.DetectionState state =
                AdoPipelineContext.TryDetect(CompleteEnv(), out AdoPipelineContext ctx, out _);

            state.Should().Be(AdoPipelineContext.DetectionState.Complete);
            ctx.RepositoryUri.Should().BeNull();
            ctx.RevisionId.Should().BeNull();
            ctx.BranchRef.Should().Be("refs/heads/main"); // pass-through of BUILD_SOURCEBRANCH (required var)
        }

        [Fact]
        public void TryDetect_VcpEnvVarsWellFormed_PopulatesVcpProperties()
        {
            FakeEnvironmentVariableGetter env = CompleteEnv()
                .With(AdoPipelineContext.RepositoryUriEnvVar, "https://dev.azure.com/contoso/example/_git/example")
                .With(AdoPipelineContext.SourceVersionEnvVar, "0123456789abcdef0123456789abcdef01234567");

            AdoPipelineContext.DetectionState state =
                AdoPipelineContext.TryDetect(env, out AdoPipelineContext ctx, out _);

            state.Should().Be(AdoPipelineContext.DetectionState.Complete);
            ctx.RepositoryUri.AbsoluteUri.Should().Be("https://dev.azure.com/contoso/example/_git/example");
            ctx.RevisionId.Should().Be("0123456789abcdef0123456789abcdef01234567");
            ctx.BranchRef.Should().Be("refs/heads/main");
        }

        [Fact]
        public void TryDetect_RepositoryUriWithLegacyOrgUserInfo_StripsCredentialAtBoundary()
        {
            // A legacy <org>@ or PAT userinfo on the env value is stripped at the boundary, so the
            // stamped repositoryUri is a clean identity rather than a credential-rejection failure.
            FakeEnvironmentVariableGetter env = CompleteEnv()
                .With(AdoPipelineContext.RepositoryUriEnvVar, "https://contoso@dev.azure.com/contoso/example/_git/example")
                .With(AdoPipelineContext.SourceVersionEnvVar, "0123456789abcdef0123456789abcdef01234567");

            AdoPipelineContext.DetectionState state =
                AdoPipelineContext.TryDetect(env, out AdoPipelineContext ctx, out _);

            state.Should().Be(AdoPipelineContext.DetectionState.Complete);
            ctx.RepositoryUri.AbsoluteUri.Should().Be("https://dev.azure.com/contoso/example/_git/example");
            ctx.RepositoryUri.UserInfo.Should().BeEmpty();
        }

        [Fact]
        public void TryDetect_RepositoryUriMalformed_ReturnsPartial()
        {
            // Optional env vars don't go silent when malformed — a broken BUILD_REPOSITORY_URI
            // is a misconfiguration signal we surface (consistent with how required vars fail).
            FakeEnvironmentVariableGetter env = CompleteEnv()
                .With(AdoPipelineContext.RepositoryUriEnvVar, "not-an-absolute-uri");

            AdoPipelineContext.DetectionState state =
                AdoPipelineContext.TryDetect(env, out AdoPipelineContext ctx, out string error);

            state.Should().Be(AdoPipelineContext.DetectionState.Partial);
            ctx.Should().BeNull();
            error.Should().Contain(AdoPipelineContext.RepositoryUriEnvVar);
            error.Should().Contain("absolute http(s) URI");
        }

        [Fact]
        public void TryDetect_RepositoryUriNonHttpScheme_ReturnsPartial()
        {
            // file://, ftp://, ssh:// etc. are absolute URIs but not the http(s) shape AdvSec
            // accepts for repositoryUri; reject loudly.
            FakeEnvironmentVariableGetter env = CompleteEnv()
                .With(AdoPipelineContext.RepositoryUriEnvVar, "ssh://git@dev.azure.com/contoso/example/_git/example");

            AdoPipelineContext.DetectionState state =
                AdoPipelineContext.TryDetect(env, out _, out string error);

            state.Should().Be(AdoPipelineContext.DetectionState.Partial);
            error.Should().Contain(AdoPipelineContext.RepositoryUriEnvVar);
        }

        [Fact]
        public void TryDetect_SourceVersionMalformed_ReturnsPartial()
        {
            FakeEnvironmentVariableGetter env = CompleteEnv()
                .With(AdoPipelineContext.SourceVersionEnvVar, "not-a-sha");

            AdoPipelineContext.DetectionState state =
                AdoPipelineContext.TryDetect(env, out _, out string error);

            state.Should().Be(AdoPipelineContext.DetectionState.Partial);
            error.Should().Contain(AdoPipelineContext.SourceVersionEnvVar);
            error.Should().Contain("revision id");
        }

        [Fact]
        public void TryDetect_SourceVersionAbbreviatedSha_Accepted()
        {
            // Real ADO always sets the full 40-char SHA, but the regex window admits any
            // 7-40 hex sequence so callers can hand-set the var to an abbreviated SHA
            // for tests or local invocations without a carve-out.
            FakeEnvironmentVariableGetter env = CompleteEnv()
                .With(AdoPipelineContext.SourceVersionEnvVar, "deadbee");

            AdoPipelineContext.DetectionState state =
                AdoPipelineContext.TryDetect(env, out AdoPipelineContext ctx, out _);

            state.Should().Be(AdoPipelineContext.DetectionState.Complete);
            ctx.RevisionId.Should().Be("deadbee");
        }

        [Fact]
        public void TryDetect_PassesRefsHeadsThrough()
        {
            FakeEnvironmentVariableGetter env = CompleteEnv()
                .With(AdoPipelineContext.SourceBranchEnvVar, "refs/heads/feature/x");

            AdoPipelineContext.TryDetect(env, out AdoPipelineContext ctx, out _);

            ctx.BranchRef.Should().Be("refs/heads/feature/x");
        }

        [Fact]
        public void TryDetect_PassesRefsPullThrough()
        {
            FakeEnvironmentVariableGetter env = CompleteEnv()
                .With(AdoPipelineContext.SourceBranchEnvVar, "refs/pull/42/merge");

            AdoPipelineContext.TryDetect(env, out AdoPipelineContext ctx, out _);

            ctx.BranchRef.Should().Be("refs/pull/42/merge");
        }

        [Fact]
        public void TryDetect_PassesRefsTagsThrough()
        {
            FakeEnvironmentVariableGetter env = CompleteEnv()
                .With(AdoPipelineContext.SourceBranchEnvVar, "refs/tags/v5.0.2");

            AdoPipelineContext.TryDetect(env, out AdoPipelineContext ctx, out _);

            ctx.BranchRef.Should().Be("refs/tags/v5.0.2");
        }

        [Fact]
        public void TryDetect_AcceptsAllFourCollectionUriForms()
        {
            // SYSTEM_COLLECTIONURI can be any of four canonical forms; the org name
            // ("contoso") must be extracted identically from each.
            string[] forms =
            {
                "https://dev.azure.com/contoso/",
                "https://contoso.visualstudio.com/",
                "https://vsrm.dev.azure.com/contoso/",
                "https://contoso.vsrm.visualstudio.com/",
            };

            foreach (string form in forms)
            {
                FakeEnvironmentVariableGetter env = CompleteEnv();
                env.With(AdoPipelineContext.CollectionUriEnvVar, form);

                AdoPipelineContext.DetectionState state =
                    AdoPipelineContext.TryDetect(env, out AdoPipelineContext ctx, out string error);

                state.Should().Be(AdoPipelineContext.DetectionState.Complete, "form='{0}' should parse cleanly (error={1})", form, error);
                ctx.OrganizationName.Should().Be("contoso", "form='{0}'", form);
            }
        }

        [Fact]
        public void TryDetect_RejectsUnsupportedCollectionUriHost()
        {
            FakeEnvironmentVariableGetter env = CompleteEnv();
            env.With(AdoPipelineContext.CollectionUriEnvVar, "https://tfs.contoso.com/DefaultCollection/");

            AdoPipelineContext.DetectionState state =
                AdoPipelineContext.TryDetect(env, out _, out string error);

            state.Should().Be(AdoPipelineContext.DetectionState.Partial);
            error.Should().Contain("unrecognized host");
        }

        [Fact]
        public void TryApplyTo_OnEmptyRun_SetsCanonicalIdAndFourPropertyKeysAndReturnsTrue()
        {
            AdoPipelineContext.TryDetect(CompleteEnv(), out AdoPipelineContext ctx, out _);
            var run = new Run();

            bool ok = ctx.TryApplyTo(run, out string error);

            ok.Should().BeTrue();
            error.Should().BeNull();
            run.AutomationDetails.Should().NotBeNull();
            run.AutomationDetails.Id.Should().Be(
                "azuredevops/pipeline/build/contoso/11111111-1111-1111-1111-111111111111/1234/22222222-2222-2222-2222-222222222222/refs/heads/main/98765");

            run.AutomationDetails.TryGetProperty(AdoPipelineContext.PropBuildDefinitionId, out string buildDefId).Should().BeTrue();
            buildDefId.Should().Be("1234");
            run.AutomationDetails.TryGetProperty(AdoPipelineContext.PropBuildDefinitionName, out string buildDefName).Should().BeTrue();
            buildDefName.Should().Be("Nightly Build");
            run.AutomationDetails.TryGetProperty(AdoPipelineContext.PropPhaseId, out string phaseId).Should().BeTrue();
            phaseId.Should().Be("22222222-2222-2222-2222-222222222222");
            run.AutomationDetails.TryGetProperty(AdoPipelineContext.PropPhaseName, out string phaseName).Should().BeTrue();
            phaseName.Should().Be("Build");
        }

        [Fact]
        public void TryApplyTo_PreservesExistingAutomationGuid()
        {
            AdoPipelineContext.TryDetect(CompleteEnv(), out AdoPipelineContext ctx, out _);

            Guid preExisting = Guid.NewGuid();
            var run = new Run
            {
                AutomationDetails = new RunAutomationDetails { Guid = preExisting, CorrelationGuid = preExisting },
            };

            bool ok = ctx.TryApplyTo(run, out string error);

            ok.Should().BeTrue();
            error.Should().BeNull();
            run.AutomationDetails.Guid.Should().Be(preExisting);
            run.AutomationDetails.CorrelationGuid.Should().Be(preExisting);
            run.AutomationDetails.Id.Should().StartWith(AdoPipelineContext.AutomationIdPrefix);
        }

        [Fact]
        public void TryApplyTo_WithEqualPreExistingValues_IsIdempotentAndReturnsTrue()
        {
            AdoPipelineContext.TryDetect(CompleteEnv(), out AdoPipelineContext ctx, out _);

            var run = new Run
            {
                AutomationDetails = new RunAutomationDetails
                {
                    Id = "azuredevops/pipeline/build/contoso/11111111-1111-1111-1111-111111111111/1234/22222222-2222-2222-2222-222222222222/refs/heads/main/98765",
                },
            };
            run.AutomationDetails.SetProperty(AdoPipelineContext.PropBuildDefinitionId, "1234");
            run.AutomationDetails.SetProperty(AdoPipelineContext.PropBuildDefinitionName, "Nightly Build");
            run.AutomationDetails.SetProperty(AdoPipelineContext.PropPhaseId, "22222222-2222-2222-2222-222222222222");
            run.AutomationDetails.SetProperty(AdoPipelineContext.PropPhaseName, "Build");

            bool ok = ctx.TryApplyTo(run, out string error);

            ok.Should().BeTrue();
            error.Should().BeNull();
            run.AutomationDetails.Id.Should().Be(
                "azuredevops/pipeline/build/contoso/11111111-1111-1111-1111-111111111111/1234/22222222-2222-2222-2222-222222222222/refs/heads/main/98765");
        }

        [Fact]
        public void TryApplyTo_WithConflictingId_ReturnsFalseAndLeavesRunUnchanged()
        {
            AdoPipelineContext.TryDetect(CompleteEnv(), out AdoPipelineContext ctx, out _);

            const string preExistingId = "self-hosted/forensic-trace/2025-11-26";
            var run = new Run
            {
                AutomationDetails = new RunAutomationDetails { Id = preExistingId },
            };

            bool ok = ctx.TryApplyTo(run, out string error);

            ok.Should().BeFalse();
            error.Should().Contain("automationDetails.id");
            error.Should().Contain(preExistingId);
            run.AutomationDetails.Id.Should().Be(preExistingId);
            run.AutomationDetails.TryGetProperty(AdoPipelineContext.PropBuildDefinitionId, out _).Should().BeFalse();
        }

        [Fact]
        public void TryApplyTo_WithConflictingProperty_ReturnsFalseAndLeavesRunUnchanged()
        {
            AdoPipelineContext.TryDetect(CompleteEnv(), out AdoPipelineContext ctx, out _);

            var run = new Run
            {
                AutomationDetails = new RunAutomationDetails(),
            };
            run.AutomationDetails.SetProperty(AdoPipelineContext.PropBuildDefinitionName, "Some Other Pipeline");

            bool ok = ctx.TryApplyTo(run, out string error);

            ok.Should().BeFalse();
            error.Should().Contain(AdoPipelineContext.PropBuildDefinitionName);
            error.Should().Contain("Some Other Pipeline");
            run.AutomationDetails.Id.Should().BeNull();
            run.AutomationDetails.TryGetProperty(AdoPipelineContext.PropBuildDefinitionName, out string buildDefName).Should().BeTrue();
            buildDefName.Should().Be("Some Other Pipeline");
        }

        [Fact]
        public void TryApplyTo_NullRun_Throws()
        {
            AdoPipelineContext.TryDetect(CompleteEnv(), out AdoPipelineContext ctx, out _);
            Action act = () => ctx.TryApplyTo(null, out _);
            act.Should().Throw<ArgumentNullException>();
        }
    }
}
