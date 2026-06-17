// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif.Driver;

using Newtonsoft.Json.Linq;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    // Pins the publish-to-ghas verb's security-sensitive behavior: GitHub-only targeting, Bearer auth,
    // the token never leaving the environment, gzip+base64 SARIF folded into a JSON body, fail-closed
    // on every refusal path, and a log-level unpublishable refusal shared with publish-to-ghazdo. The
    // HTTP boundary is exercised through an injected recording handler so no network is contacted.
    public class PublishToGhasCommandTests : IDisposable
    {
        private const string JwtToken =
            "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIn0.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";

        private const string PatToken = "ghp_abcdefghijklmnopqrstuvwxyz0123456789AB";

        private const string CommitSha = "0123456789abcdef0123456789abcdef01234567";

        private const string Ref = "refs/heads/main";

        private readonly string _dir;

        public PublishToGhasCommandTests()
        {
            _dir = Path.Combine(Path.GetTempPath(), $"publish-ghas-{Guid.NewGuid():N}");
            Directory.CreateDirectory(_dir);
        }

        public void Dispose()
        {
            if (Directory.Exists(_dir)) { Directory.Delete(_dir, recursive: true); }
        }

        // ----- happy path -----

        [Fact]
        public void Publish_GitHubTarget_PostsBearerJsonBodyWithGzipBase64Sarif()
        {
            string sarifPath = WriteSarif(new Uri("https://github.com/myowner/myrepo"));
            var handler = new RecordingHandler(HttpStatusCode.Accepted);

            (int exit, string stdout, string _) = InvokeWithSecret(handler, sarifPath, PatToken);

            exit.Should().Be(CommandBase.SUCCESS);
            handler.Requests.Should().HaveCount(1);

            Uri url = handler.Urls[0];
            url.Host.Should().Be("api.github.com");
            url.AbsolutePath.Should().Be("/repos/myowner/myrepo/code-scanning/sarifs");

            handler.Authorizations[0].Scheme.Should().Be("Bearer");
            handler.Authorizations[0].Parameter.Should().Be(PatToken);
            handler.ContentTypes[0].Should().Be("application/json");
            handler.ApiVersions[0].Should().Be("2022-11-28", "GitHub requires the X-GitHub-Api-Version header.");
            handler.Accepts[0].Should().Be("application/vnd.github+json");
            handler.UserAgents[0].Should().Be("Sarif.Multitool", "GitHub rejects a request with no User-Agent.");

            JObject body = JObject.Parse(handler.Bodies[0]);
            body.Value<string>("commit_sha").Should().Be(CommitSha);
            body.Value<string>("ref").Should().Be(Ref);
            Gunzip(Convert.FromBase64String(body.Value<string>("sarif"))).Should().Equal(File.ReadAllBytes(sarifPath));

            stdout.Should().NotContain(PatToken, "the token must never be printed.");
        }

        [Fact]
        public void Publish_GheComDataResidencyHost_TargetsApiSubdomain()
        {
            string sarifPath = WriteSarif(new Uri("https://octo.ghe.com/myowner/myrepo"));
            var handler = new RecordingHandler(HttpStatusCode.Accepted);

            (int exit, string _, string _2) = InvokeWithSecret(handler, sarifPath, PatToken);

            exit.Should().Be(CommandBase.SUCCESS);
            handler.Urls[0].Host.Should().Be("api.octo.ghe.com");
            handler.Urls[0].AbsolutePath.Should().Be("/repos/myowner/myrepo/code-scanning/sarifs");
        }

        [Fact]
        public void Publish_EntraToken_StillSentAsBearer()
        {
            string sarifPath = WriteSarif(new Uri("https://github.com/myowner/myrepo"));
            var handler = new RecordingHandler(HttpStatusCode.Accepted);

            (int exit, string stdout, string _) = InvokeWithSecret(handler, sarifPath, JwtToken);

            exit.Should().Be(CommandBase.SUCCESS);
            handler.Authorizations[0].Scheme.Should().Be("Bearer");
            handler.Authorizations[0].Parameter.Should().Be(JwtToken);
            stdout.Should().NotContain(JwtToken);
        }

        [Fact]
        public void Publish_GitHubSshCloneUrl_NormalizedToHttpsApiTarget()
        {
            // A GitHub ssh clone URL is normalized to its https identity; the API host must come from
            // that normalized host, not a raw-host derivation.
            string sarifPath = WriteSarif(new Uri("ssh://git@github.com/myowner/myrepo"));
            var handler = new RecordingHandler(HttpStatusCode.Accepted);

            (int exit, string _, string _2) = InvokeWithSecret(handler, sarifPath, PatToken);

            exit.Should().Be(CommandBase.SUCCESS);
            handler.Urls[0].Host.Should().Be("api.github.com");
            handler.Urls[0].AbsolutePath.Should().Be("/repos/myowner/myrepo/code-scanning/sarifs");
        }

        // ----- fail-closed paths -----

        [Fact]
        public void Publish_NonGitHubTarget_FailsClosedWithoutNetwork()
        {
            string sarifPath = WriteSarif(new Uri("https://dev.azure.com/myorg/myproj/_git/myrepo"));
            var handler = new RecordingHandler(HttpStatusCode.Accepted);

            (int exit, string _, string stderr) = InvokeWithSecret(handler, sarifPath, PatToken);

            exit.Should().Be(CommandBase.FAILURE);
            handler.Requests.Should().BeEmpty();
            stderr.Should().Contain("GitHub");
        }

        [Fact]
        public void Publish_EmbeddedCredentialsInRepositoryUri_FailsClosedWithoutNetwork()
        {
            string sarifPath = WriteSarif(new Uri("https://user:pass@github.com/myowner/myrepo"));
            var handler = new RecordingHandler(HttpStatusCode.Accepted);

            (int exit, string _, string stderr) = InvokeWithSecret(handler, sarifPath, PatToken);

            exit.Should().Be(CommandBase.FAILURE);
            handler.Requests.Should().BeEmpty();
            stderr.Should().Contain("credential");
        }

        [Fact]
        public void Publish_MissingVersionControlProvenance_FailsClosedWithoutNetwork()
        {
            string sarifPath = WriteSarif(repositoryUri: null);
            var handler = new RecordingHandler(HttpStatusCode.Accepted);

            (int exit, string _, string stderr) = InvokeWithSecret(handler, sarifPath, PatToken);

            exit.Should().Be(CommandBase.FAILURE);
            handler.Requests.Should().BeEmpty();
            stderr.Should().Contain("versionControlProvenance");
        }

        [Fact]
        public void Publish_MissingRevisionId_FailsClosedWithoutNetwork()
        {
            string sarifPath = WriteSarif(new Uri("https://github.com/myowner/myrepo"), revisionId: null, branch: Ref);
            var handler = new RecordingHandler(HttpStatusCode.Accepted);

            (int exit, string _, string stderr) = InvokeWithSecret(handler, sarifPath, PatToken);

            exit.Should().Be(CommandBase.FAILURE);
            handler.Requests.Should().BeEmpty();
            stderr.Should().Contain("revisionId");
        }

        [Fact]
        public void Publish_MissingBranch_FailsClosedWithoutNetwork()
        {
            string sarifPath = WriteSarif(new Uri("https://github.com/myowner/myrepo"), revisionId: CommitSha, branch: null);
            var handler = new RecordingHandler(HttpStatusCode.Accepted);

            (int exit, string _, string stderr) = InvokeWithSecret(handler, sarifPath, PatToken);

            exit.Should().Be(CommandBase.FAILURE);
            handler.Requests.Should().BeEmpty();
            stderr.Should().Contain("branch");
        }

        [Fact]
        public void Publish_BranchIsNotFullyQualifiedRef_FailsClosedWithoutNetwork()
        {
            // A bare branch name ("main") is not a ref GitHub accepts; catch it offline rather than at
            // upload time, matching the verb's fail-closed posture.
            string sarifPath = WriteSarif(new Uri("https://github.com/myowner/myrepo"), revisionId: CommitSha, branch: "main");
            var handler = new RecordingHandler(HttpStatusCode.Accepted);

            (int exit, string _, string stderr) = InvokeWithSecret(handler, sarifPath, PatToken);

            exit.Should().Be(CommandBase.FAILURE);
            handler.Requests.Should().BeEmpty();
            stderr.Should().Contain("ref");
        }

        [Fact]
        public void Publish_TokenEnvironmentVariableUnset_FailsClosedWithoutNetwork()
        {
            string sarifPath = WriteSarif(new Uri("https://github.com/myowner/myrepo"));
            var handler = new RecordingHandler(HttpStatusCode.Accepted);

            string envVar = UniqueEnvVarName();
            string prior = Environment.GetEnvironmentVariable(envVar);
            try
            {
                Environment.SetEnvironmentVariable(envVar, null);

                (int exit, string _, string stderr) = Invoke(
                    new PublishToGhasCommand(handler),
                    NewOptions(sarifPath, envVar));

                exit.Should().Be(CommandBase.FAILURE);
                handler.Requests.Should().BeEmpty();
                stderr.Should().Contain("token-env-var");
                stderr.Should().NotContain(envVar, "the variable name is not echoed back, so a token mistakenly passed as the name cannot leak.");
            }
            finally
            {
                Environment.SetEnvironmentVariable(envVar, prior);
            }
        }

        [Fact]
        public void Publish_TokenEnvVarNamedAfterAValidIdentifierToken_DoesNotEchoItWhenUnset()
        {
            // A ghp_-prefixed PAT is a syntactically valid environment-variable name, so it passes the
            // name check; the "unset" diagnostic must still never echo it.
            string sarifPath = WriteSarif(new Uri("https://github.com/myowner/myrepo"));
            var handler = new RecordingHandler(HttpStatusCode.Accepted);

            string prior = Environment.GetEnvironmentVariable(PatToken);
            try
            {
                Environment.SetEnvironmentVariable(PatToken, null);

                (int exit, string _, string stderr) = Invoke(
                    new PublishToGhasCommand(handler),
                    NewOptions(sarifPath, PatToken));

                exit.Should().Be(CommandBase.FAILURE);
                handler.Requests.Should().BeEmpty();
                stderr.Should().NotContain(PatToken, "the variable name is never echoed, so a token mistakenly passed as the name cannot leak.");
            }
            finally
            {
                Environment.SetEnvironmentVariable(PatToken, prior);
            }
        }

        [Fact]
        public void Publish_ServerRejectsWithHttp400_FailsClosed()
        {
            string sarifPath = WriteSarif(new Uri("https://github.com/myowner/myrepo"));
            var handler = new RecordingHandler(HttpStatusCode.BadRequest);

            (int exit, string _, string stderr) = InvokeWithSecret(handler, sarifPath, PatToken);

            exit.Should().Be(CommandBase.FAILURE);
            stderr.Should().Contain("HTTP 400");
        }

        [Fact]
        public void Publish_ServerReturns302_FailsClosedNotTreatedAsSuccess()
        {
            // Only a 2xx is success. A redirect must not be reported as "Published".
            string sarifPath = WriteSarif(new Uri("https://github.com/myowner/myrepo"));
            var handler = new RecordingHandler(HttpStatusCode.Redirect);

            (int exit, string stdout, string stderr) = InvokeWithSecret(handler, sarifPath, PatToken);

            exit.Should().Be(CommandBase.FAILURE);
            stdout.Should().NotContain("Published");
            stderr.Should().Contain("HTTP 302");
        }

        [Fact]
        public void Publish_TokenEnvVarNameIsTokenShaped_FailsWithoutEchoingIt()
        {
            // A caller who mistakenly passes the token itself as --token-env-var must not have it
            // amplified into a diagnostic. A JWT is not a valid environment-variable name (it contains
            // dots), so it is rejected up front with a generic message.
            string sarifPath = WriteSarif(new Uri("https://github.com/myowner/myrepo"));
            var handler = new RecordingHandler(HttpStatusCode.Accepted);

            (int exit, string _, string stderr) = Invoke(
                new PublishToGhasCommand(handler),
                NewOptions(sarifPath, JwtToken));

            exit.Should().Be(CommandBase.FAILURE);
            handler.Requests.Should().BeEmpty();
            stderr.Should().NotContain(JwtToken, "the supplied --token-env-var value must never be echoed.");
            stderr.Should().Contain("--token-env-var");
        }

        // ----- token never leaks -----

        [Fact]
        public void Publish_ResponseBodyReflectsToken_RedactsBeforePrinting()
        {
            // A misbehaving endpoint or proxy could echo the request's Authorization value in its body.
            // The body is surfaced for diagnostics, but the token must be redacted first.
            string sarifPath = WriteSarif(new Uri("https://github.com/myowner/myrepo"));
            var handler = new RecordingHandler(HttpStatusCode.BadRequest)
            {
                ResponseBody = $"rejected: token={PatToken}",
            };

            (int exit, string stdout, string _) = InvokeWithSecret(handler, sarifPath, PatToken);

            exit.Should().Be(CommandBase.FAILURE);
            stdout.Should().NotContain(PatToken, "the token echoed in a response body must be redacted.");
            stdout.Should().Contain("***");
        }

        [Fact]
        public void Publish_HandlerThrowsWithTokenInMessage_RedactsBeforePrinting()
        {
            string sarifPath = WriteSarif(new Uri("https://github.com/myowner/myrepo"));
            var handler = new ThrowingWithMessageHandler($"transport failure leaked {PatToken}");

            (int exit, string _, string stderr) = InvokeWithSecret(handler, sarifPath, PatToken);

            exit.Should().Be(CommandBase.FAILURE);
            stderr.Should().NotContain(PatToken, "a token folded into an exception message must be redacted.");
            stderr.Should().Contain("***");
        }

        // ----- dry run -----

        [Fact]
        public void Publish_DryRun_ContactsNoServerAndDoesNotLeakToken()
        {
            string sarifPath = WriteSarif(new Uri("https://github.com/myowner/myrepo"));
            var handler = new ThrowingHandler();

            string envVar = UniqueEnvVarName();
            string prior = Environment.GetEnvironmentVariable(envVar);
            try
            {
                Environment.SetEnvironmentVariable(envVar, PatToken);
                PublishToGhasOptions options = NewOptions(sarifPath, envVar);
                options.DryRun = true;

                (int exit, string stdout, string _) = Invoke(new PublishToGhasCommand(handler), options);

                exit.Should().Be(CommandBase.SUCCESS);
                stdout.Should().Contain("dry run");
                stdout.Should().Contain("myowner/myrepo");
                stdout.Should().Contain(CommitSha);
                stdout.Should().Contain(Ref);
                stdout.Should().NotContain(PatToken, "the token must never be printed, even in dry-run.");
            }
            finally
            {
                Environment.SetEnvironmentVariable(envVar, prior);
            }
        }

        // ----- unpublishable refusal (shared, log-level) -----

        [Fact]
        public void Publish_RefusesUnpublishableLog_BeforeAnyVcpOrTokenCheck()
        {
            var run = new Run { Results = new List<Result>() };
            run.SetProperty(EmitFinalizeCommand.UnpublishablePropertyName, true);
            var log = new SarifLog { Runs = new List<Run> { run } };
            string sarifPath = Path.Combine(_dir, $"{Guid.NewGuid():N}.sarif");
            CommandBase.WriteSarifFile(Sarif.FileSystem.Instance, log, sarifPath, Newtonsoft.Json.Formatting.Indented);

            var handler = new ThrowingHandler();
            (int exit, string _, string stderr) = InvokeWithSecret(handler, sarifPath, PatToken);

            exit.Should().Be(CommandBase.FAILURE);
            stderr.Should().Contain("unpublishable");
            stderr.Should().Contain("--no-repo");
        }

        [Fact]
        public void Publish_RefusesMultiRunLogWhenAnyRunUnpublishable_EvenIfFirstRunIsPublishable()
        {
            var publishableRun = new Run
            {
                Results = new List<Result>(),
                VersionControlProvenance = new List<VersionControlDetails>
                {
                    new VersionControlDetails
                    {
                        RepositoryUri = new Uri("https://github.com/myowner/myrepo"),
                        RevisionId = CommitSha,
                        Branch = Ref,
                    },
                },
            };

            var unpublishableRun = new Run { Results = new List<Result>() };
            unpublishableRun.SetProperty(EmitFinalizeCommand.UnpublishablePropertyName, true);

            var log = new SarifLog { Runs = new List<Run> { publishableRun, unpublishableRun } };
            string sarifPath = Path.Combine(_dir, $"{Guid.NewGuid():N}.sarif");
            CommandBase.WriteSarifFile(Sarif.FileSystem.Instance, log, sarifPath, Newtonsoft.Json.Formatting.Indented);

            var handler = new ThrowingHandler();
            (int exit, string _, string stderr) = InvokeWithSecret(handler, sarifPath, PatToken);

            exit.Should().Be(CommandBase.FAILURE);
            stderr.Should().Contain("unpublishable");
            stderr.Should().Contain("--no-repo");
        }

        // ----- helpers -----

        private string WriteSarif(Uri repositoryUri)
            => WriteSarif(repositoryUri, CommitSha, Ref);

        private string WriteSarif(Uri repositoryUri, string revisionId, string branch)
        {
            var run = new Run { Results = new List<Result>() };
            if (repositoryUri != null)
            {
                run.VersionControlProvenance = new List<VersionControlDetails>
                {
                    new VersionControlDetails
                    {
                        RepositoryUri = repositoryUri,
                        RevisionId = revisionId,
                        Branch = branch,
                    },
                };
            }

            var log = new SarifLog { Runs = new List<Run> { run } };
            string path = Path.Combine(_dir, $"{Guid.NewGuid():N}.sarif");
            CommandBase.WriteSarifFile(Sarif.FileSystem.Instance, log, path, Newtonsoft.Json.Formatting.Indented);
            return path;
        }

        private static PublishToGhasOptions NewOptions(string sarifPath, string tokenEnvVar)
        {
            return new PublishToGhasOptions
            {
                SarifPath = sarifPath,
                TokenEnvironmentVariable = tokenEnvVar,
            };
        }

        private (int exit, string stdout, string stderr) InvokeWithSecret(
            HttpMessageHandler handler, string sarifPath, string secret)
        {
            string envVar = UniqueEnvVarName();
            string prior = Environment.GetEnvironmentVariable(envVar);
            try
            {
                Environment.SetEnvironmentVariable(envVar, secret);
                return Invoke(new PublishToGhasCommand(handler), NewOptions(sarifPath, envVar));
            }
            finally
            {
                Environment.SetEnvironmentVariable(envVar, prior);
            }
        }

        private static (int exit, string stdout, string stderr) Invoke(
            PublishToGhasCommand command, PublishToGhasOptions options)
        {
            TextWriter originalOut = Console.Out;
            TextWriter originalError = Console.Error;
            var capturedOut = new StringWriter();
            var capturedError = new StringWriter();
            try
            {
                Console.SetOut(capturedOut);
                Console.SetError(capturedError);
                int exit = command.Run(options);
                return (exit, capturedOut.ToString(), capturedError.ToString());
            }
            finally
            {
                Console.SetOut(originalOut);
                Console.SetError(originalError);
            }
        }

        private static string UniqueEnvVarName() => $"PUBLISH_GHAS_TEST_{Guid.NewGuid():N}";

        private static byte[] Gunzip(byte[] data)
        {
            using var input = new MemoryStream(data);
            using var gzip = new GZipStream(input, CompressionMode.Decompress);
            using var output = new MemoryStream();
            gzip.CopyTo(output);
            return output.ToArray();
        }

        private sealed class RecordingHandler : HttpMessageHandler
        {
            private readonly Queue<HttpStatusCode> _statuses;

            public RecordingHandler(params HttpStatusCode[] statuses)
            {
                _statuses = new Queue<HttpStatusCode>(statuses.Length == 0 ? new[] { HttpStatusCode.Accepted } : statuses);
            }

            public List<HttpRequestMessage> Requests { get; } = new List<HttpRequestMessage>();

            public List<Uri> Urls { get; } = new List<Uri>();

            public List<AuthenticationHeaderValue> Authorizations { get; } = new List<AuthenticationHeaderValue>();

            public List<string> ContentTypes { get; } = new List<string>();

            public List<string> ApiVersions { get; } = new List<string>();

            public List<string> Accepts { get; } = new List<string>();

            public List<string> UserAgents { get; } = new List<string>();

            public List<string> Bodies { get; } = new List<string>();

            public string ResponseBody { get; set; } = "{}";

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                Requests.Add(request);
                Urls.Add(request.RequestUri);
                Authorizations.Add(request.Headers.Authorization);
                ContentTypes.Add(request.Content?.Headers.ContentType?.MediaType);
                ApiVersions.Add(request.Headers.TryGetValues("X-GitHub-Api-Version", out IEnumerable<string> versions) ? string.Join(",", versions) : null);
                Accepts.Add(request.Headers.Accept.Count > 0 ? request.Headers.Accept.ToString() : null);
                UserAgents.Add(request.Headers.UserAgent.Count > 0 ? request.Headers.UserAgent.ToString() : null);
                Bodies.Add(request.Content == null ? null : await request.Content.ReadAsStringAsync());

                HttpStatusCode status = _statuses.Count > 0 ? _statuses.Dequeue() : HttpStatusCode.Accepted;
                return new HttpResponseMessage(status) { Content = new StringContent(ResponseBody) };
            }
        }

        private sealed class ThrowingWithMessageHandler : HttpMessageHandler
        {
            private readonly string _message;

            public ThrowingWithMessageHandler(string message) { _message = message; }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                throw new InvalidOperationException(_message);
            }
        }

        private sealed class ThrowingHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                throw new InvalidOperationException("the network must not be contacted in a dry run.");
            }
        }
    }
}
