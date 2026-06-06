// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif.Driver;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    // Pins the publish-to-ghazdo verb's security-sensitive behavior: dev.azure.com-only targeting,
    // scheme detection (Bearer for an Entra JWT, Basic for an opaque PAT), the secret never leaving
    // the environment, gzip/octet-stream framing with no Content-Encoding header, host fallback on
    // 404, and fail-closed on every refusal path. The HTTP boundary is exercised through an injected
    // recording handler so no network is contacted.
    public class PublishToGhazdoCommandTests : IDisposable
    {
        private const string JwtSecret =
            "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIn0.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";

        private const string PatSecret = "abcdefghijklmnopqrstuvwxyz234567abcdefghijklmnopqrstuvwxyz2345";

        private readonly string _dir;

        public PublishToGhazdoCommandTests()
        {
            _dir = Path.Combine(Path.GetTempPath(), $"publish-ghazdo-{Guid.NewGuid():N}");
            Directory.CreateDirectory(_dir);
        }

        public void Dispose()
        {
            if (Directory.Exists(_dir)) { Directory.Delete(_dir, recursive: true); }
        }

        // ----- scheme detection -----

        [Fact]
        public void DetectScheme_EntraJsonWebToken_SelectsBearer()
        {
            PublishToGhazdoCommand.DetectScheme(JwtSecret).Should().Be("Bearer");
        }

        [Fact]
        public void DetectScheme_OpaquePat_SelectsBasic()
        {
            PublishToGhazdoCommand.DetectScheme(PatSecret).Should().Be("Basic");
        }

        [Fact]
        public void DetectScheme_DottedButNotAJwt_FailsSafeToBasic()
        {
            // Three dot-separated parts but no base64url JSON header: classify as a PAT (Basic) rather
            // than risk sending an opaque secret as a raw Bearer token.
            PublishToGhazdoCommand.DetectScheme("foo.bar.baz").Should().Be("Basic");
        }

        [Fact]
        public void BuildAuthorization_Pat_ProducesBasicWithEmptyUserName()
        {
            AuthenticationHeaderValue header = PublishToGhazdoCommand.BuildAuthorization(PatSecret, out string scheme);

            scheme.Should().Be("Basic");
            header.Scheme.Should().Be("Basic");
            header.Parameter.Should().Be(Convert.ToBase64String(Encoding.ASCII.GetBytes(":" + PatSecret)));
        }

        [Fact]
        public void BuildAuthorization_EntraToken_ProducesBearerWithRawToken()
        {
            AuthenticationHeaderValue header = PublishToGhazdoCommand.BuildAuthorization(JwtSecret, out string scheme);

            scheme.Should().Be("Bearer");
            header.Scheme.Should().Be("Bearer");
            header.Parameter.Should().Be(JwtSecret);
        }

        // ----- happy path -----

        [Fact]
        public void Publish_AzureDevOpsTarget_PostsGzipOctetStreamWithBasicAuthAndNoContentEncoding()
        {
            string sarifPath = WriteSarif(new Uri("https://dev.azure.com/myorg/myproj/_git/myrepo"));
            var handler = new RecordingHandler(HttpStatusCode.OK);

            (int exit, string stdout, string _) = InvokeWithSecret(handler, sarifPath, PatSecret);

            exit.Should().Be(CommandBase.SUCCESS);
            handler.Requests.Should().HaveCount(1);

            Uri url = handler.Urls[0];
            url.Host.Should().Be("advsec.dev.azure.com");
            url.AbsolutePath.Should().Be("/myorg/myproj/_apis/alert/repositories/myrepo/sarifs");
            url.Query.Should().Contain("api-version=7.2-preview.1");

            handler.Authorizations[0].Scheme.Should().Be("Basic");
            handler.ContentTypes[0].Should().Be("application/octet-stream");
            handler.HadContentEncoding[0].Should().BeFalse("the server gunzips manually; a Content-Encoding header causes a double-decompress failure.");

            Gunzip(handler.Bodies[0]).Should().Equal(File.ReadAllBytes(sarifPath));

            stdout.Should().NotContain(PatSecret, "the secret must never be printed.");
        }

        [Fact]
        public void Publish_EntraToken_SendsBearerScheme()
        {
            string sarifPath = WriteSarif(new Uri("https://dev.azure.com/myorg/myproj/_git/myrepo"));
            var handler = new RecordingHandler(HttpStatusCode.OK);

            (int exit, string stdout, string _) = InvokeWithSecret(handler, sarifPath, JwtSecret);

            exit.Should().Be(CommandBase.SUCCESS);
            handler.Authorizations[0].Scheme.Should().Be("Bearer");
            stdout.Should().NotContain(JwtSecret);
        }

        [Fact]
        public void Publish_404OnAdvsec_FallsBackToDevAzure()
        {
            string sarifPath = WriteSarif(new Uri("https://dev.azure.com/myorg/myproj/_git/myrepo"));
            var handler = new RecordingHandler(HttpStatusCode.NotFound, HttpStatusCode.OK);

            (int exit, string _, string _2) = InvokeWithSecret(handler, sarifPath, PatSecret);

            exit.Should().Be(CommandBase.SUCCESS);
            handler.Requests.Should().HaveCount(2);
            handler.Urls[0].Host.Should().Be("advsec.dev.azure.com");
            handler.Urls[1].Host.Should().Be("dev.azure.com");
        }

        // ----- fail-closed paths -----

        [Fact]
        public void Publish_NonAzureDevOpsTarget_FailsClosedWithoutNetwork()
        {
            string sarifPath = WriteSarif(new Uri("https://github.com/owner/repo"));
            var handler = new RecordingHandler(HttpStatusCode.OK);

            (int exit, string _, string stderr) = InvokeWithSecret(handler, sarifPath, PatSecret);

            exit.Should().Be(CommandBase.FAILURE);
            handler.Requests.Should().BeEmpty();
            stderr.Should().Contain("Azure DevOps");
        }

        [Fact]
        public void Publish_EmbeddedCredentialsInRepositoryUri_FailsClosedWithoutNetwork()
        {
            string sarifPath = WriteSarif(new Uri("https://user:pass@dev.azure.com/myorg/myproj/_git/myrepo"));
            var handler = new RecordingHandler(HttpStatusCode.OK);

            (int exit, string _, string stderr) = InvokeWithSecret(handler, sarifPath, PatSecret);

            exit.Should().Be(CommandBase.FAILURE);
            handler.Requests.Should().BeEmpty();
            stderr.Should().Contain("credential");
        }

        [Fact]
        public void Publish_MissingVersionControlProvenance_FailsClosedWithoutNetwork()
        {
            string sarifPath = WriteSarif(repositoryUri: null);
            var handler = new RecordingHandler(HttpStatusCode.OK);

            (int exit, string _, string stderr) = InvokeWithSecret(handler, sarifPath, PatSecret);

            exit.Should().Be(CommandBase.FAILURE);
            handler.Requests.Should().BeEmpty();
            stderr.Should().Contain("versionControlProvenance");
        }

        [Fact]
        public void Publish_SecretEnvironmentVariableUnset_FailsClosedWithoutNetwork()
        {
            string sarifPath = WriteSarif(new Uri("https://dev.azure.com/myorg/myproj/_git/myrepo"));
            var handler = new RecordingHandler(HttpStatusCode.OK);

            string envVar = UniqueEnvVarName();
            string prior = Environment.GetEnvironmentVariable(envVar);
            try
            {
                Environment.SetEnvironmentVariable(envVar, null);

                (int exit, string _, string stderr) = Invoke(
                    new PublishToGhazdoCommand(handler),
                    NewOptions(sarifPath, envVar));

                exit.Should().Be(CommandBase.FAILURE);
                handler.Requests.Should().BeEmpty();
                stderr.Should().Contain("token-env-var");
                stderr.Should().NotContain(envVar, "the variable name is not echoed back, so a secret mistakenly passed as the name cannot leak.");
            }
            finally
            {
                Environment.SetEnvironmentVariable(envVar, prior);
            }
        }

        [Fact]
        public void Publish_ServerRejectsWithHttp400_FailsClosed()
        {
            string sarifPath = WriteSarif(new Uri("https://dev.azure.com/myorg/myproj/_git/myrepo"));
            var handler = new RecordingHandler(HttpStatusCode.BadRequest);

            (int exit, string _, string stderr) = InvokeWithSecret(handler, sarifPath, PatSecret);

            exit.Should().Be(CommandBase.FAILURE);
            stderr.Should().Contain("HTTP 400");
        }

        [Fact]
        public void Publish_ServerReturns302_FailsClosedNotTreatedAsSuccess()
        {
            // Only a 2xx is success. A redirect must not be reported as "Published".
            string sarifPath = WriteSarif(new Uri("https://dev.azure.com/myorg/myproj/_git/myrepo"));
            var handler = new RecordingHandler(HttpStatusCode.Redirect);

            (int exit, string stdout, string stderr) = InvokeWithSecret(handler, sarifPath, PatSecret);

            exit.Should().Be(CommandBase.FAILURE);
            stdout.Should().NotContain("Published");
            stderr.Should().Contain("HTTP 302");
        }

        [Fact]
        public void Publish_TokenEnvVarNamedAfterAValidIdentifierSecret_DoesNotEchoItWhenUnset()
        {
            // A base32 PAT is a syntactically valid environment-variable name, so it passes the name
            // check; the "unset" diagnostic must still never echo it.
            string sarifPath = WriteSarif(new Uri("https://dev.azure.com/myorg/myproj/_git/myrepo"));
            var handler = new RecordingHandler(HttpStatusCode.OK);

            string prior = Environment.GetEnvironmentVariable(PatSecret);
            try
            {
                Environment.SetEnvironmentVariable(PatSecret, null);

                (int exit, string _, string stderr) = Invoke(
                    new PublishToGhazdoCommand(handler),
                    NewOptions(sarifPath, PatSecret));

                exit.Should().Be(CommandBase.FAILURE);
                handler.Requests.Should().BeEmpty();
                stderr.Should().NotContain(PatSecret, "the variable name is never echoed, so a secret mistakenly passed as the name cannot leak.");
            }
            finally
            {
                Environment.SetEnvironmentVariable(PatSecret, prior);
            }
        }

        // ----- secret never leaks -----

        [Fact]
        public void Publish_ResponseBodyReflectsSecret_RedactsBeforePrinting()
        {
            // A misbehaving endpoint or proxy could echo the request's Authorization value in its body.
            // The body is surfaced for diagnostics, but the secret (and its Basic-encoded form) must be
            // redacted first.
            string sarifPath = WriteSarif(new Uri("https://dev.azure.com/myorg/myproj/_git/myrepo"));
            string basicForm = Convert.ToBase64String(Encoding.ASCII.GetBytes(":" + PatSecret));
            var handler = new RecordingHandler(HttpStatusCode.BadRequest)
            {
                ResponseBody = $"rejected: token={PatSecret} basic={basicForm}",
            };

            (int exit, string stdout, string stderr) = InvokeWithSecret(handler, sarifPath, PatSecret);

            exit.Should().Be(CommandBase.FAILURE);
            stdout.Should().NotContain(PatSecret, "the secret echoed in a response body must be redacted.");
            stdout.Should().NotContain(basicForm, "the Basic-encoded form of the secret must be redacted too.");
            stdout.Should().Contain("***");
        }

        [Fact]
        public void Publish_HandlerThrowsWithSecretInMessage_RedactsBeforePrinting()
        {
            string sarifPath = WriteSarif(new Uri("https://dev.azure.com/myorg/myproj/_git/myrepo"));
            var handler = new ThrowingWithMessageHandler($"transport failure leaked {PatSecret}");

            (int exit, string _, string stderr) = InvokeWithSecret(handler, sarifPath, PatSecret);

            exit.Should().Be(CommandBase.FAILURE);
            stderr.Should().NotContain(PatSecret, "a secret folded into an exception message must be redacted.");
            stderr.Should().Contain("***");
        }

        [Fact]
        public void Publish_TokenEnvVarNameIsSecretShaped_FailsWithoutEchoingIt()
        {
            // A caller who mistakenly passes the secret itself as --token-env-var must not have it
            // amplified into a diagnostic. A JWT is not a valid environment-variable name (it contains
            // dots), so it is rejected up front with a generic message.
            string sarifPath = WriteSarif(new Uri("https://dev.azure.com/myorg/myproj/_git/myrepo"));
            var handler = new RecordingHandler(HttpStatusCode.OK);

            (int exit, string _, string stderr) = Invoke(
                new PublishToGhazdoCommand(handler),
                NewOptions(sarifPath, JwtSecret));

            exit.Should().Be(CommandBase.FAILURE);
            handler.Requests.Should().BeEmpty();
            stderr.Should().NotContain(JwtSecret, "the supplied --token-env-var value must never be echoed.");
            stderr.Should().Contain("--token-env-var");
        }

        // ----- JWT detection edge cases -----

        [Fact]
        public void DetectScheme_ThreeNonBase64UrlSegments_FailsSafeToBasic()
        {
            // Looks JWT-shaped (three dotted segments) but a segment contains characters outside the
            // base64url alphabet, so it is not a real token: classify as Basic.
            PublishToGhazdoCommand.DetectScheme("eyJ.bad token.sig").Should().Be("Basic");
        }

        [Fact]
        public void DetectScheme_HeaderDoesNotDecodeToJson_FailsSafeToBasic()
        {
            // Three valid base64url segments, but the first does not decode to a JSON object: Basic.
            PublishToGhazdoCommand.DetectScheme("YWJj.ZGVm.Z2hp").Should().Be("Basic");
        }

        // ----- dry run -----

        [Fact]
        public void Publish_DryRun_ContactsNoServerAndDoesNotLeakSecret()
        {
            string sarifPath = WriteSarif(new Uri("https://dev.azure.com/myorg/myproj/_git/myrepo"));
            var handler = new ThrowingHandler();

            string envVar = UniqueEnvVarName();
            string prior = Environment.GetEnvironmentVariable(envVar);
            try
            {
                Environment.SetEnvironmentVariable(envVar, PatSecret);
                PublishToGhazdoOptions options = NewOptions(sarifPath, envVar);
                options.DryRun = true;

                (int exit, string stdout, string _) = Invoke(new PublishToGhazdoCommand(handler), options);

                exit.Should().Be(CommandBase.SUCCESS);
                stdout.Should().Contain("dry run");
                stdout.Should().Contain("myorg/myproj/myrepo");
                stdout.Should().Contain("Basic", "dry-run reports the scheme it would use.");
                stdout.Should().NotContain(PatSecret, "the secret must never be printed, even in dry-run.");
            }
            finally
            {
                Environment.SetEnvironmentVariable(envVar, prior);
            }
        }

        // ----- helpers -----

        private string WriteSarif(Uri repositoryUri)
        {
            var run = new Run { Results = new List<Result>() };
            if (repositoryUri != null)
            {
                run.VersionControlProvenance = new List<VersionControlDetails>
                {
                    new VersionControlDetails
                    {
                        RepositoryUri = repositoryUri,
                        RevisionId = "0123456789abcdef0123456789abcdef01234567",
                    },
                };
            }

            var log = new SarifLog { Runs = new List<Run> { run } };
            string path = Path.Combine(_dir, $"{Guid.NewGuid():N}.sarif");
            CommandBase.WriteSarifFile(Sarif.FileSystem.Instance, log, path, Newtonsoft.Json.Formatting.Indented);
            return path;
        }

        private static PublishToGhazdoOptions NewOptions(string sarifPath, string tokenEnvVar)
        {
            return new PublishToGhazdoOptions
            {
                SarifPath = sarifPath,
                TokenEnvironmentVariable = tokenEnvVar,
                ApiVersion = "7.2-preview.1",
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
                return Invoke(new PublishToGhazdoCommand(handler), NewOptions(sarifPath, envVar));
            }
            finally
            {
                Environment.SetEnvironmentVariable(envVar, prior);
            }
        }

        private static (int exit, string stdout, string stderr) Invoke(
            PublishToGhazdoCommand command, PublishToGhazdoOptions options)
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

        private static string UniqueEnvVarName() => $"PUBLISH_GHAZDO_TEST_{Guid.NewGuid():N}";

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
                _statuses = new Queue<HttpStatusCode>(statuses.Length == 0 ? new[] { HttpStatusCode.OK } : statuses);
            }

            public List<HttpRequestMessage> Requests { get; } = new List<HttpRequestMessage>();

            public List<Uri> Urls { get; } = new List<Uri>();

            public List<AuthenticationHeaderValue> Authorizations { get; } = new List<AuthenticationHeaderValue>();

            public List<string> ContentTypes { get; } = new List<string>();

            public List<bool> HadContentEncoding { get; } = new List<bool>();

            public List<byte[]> Bodies { get; } = new List<byte[]>();

            public string ResponseBody { get; set; } = "{}";

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                Requests.Add(request);
                Urls.Add(request.RequestUri);
                Authorizations.Add(request.Headers.Authorization);
                ContentTypes.Add(request.Content?.Headers.ContentType?.MediaType);
                HadContentEncoding.Add(request.Content != null && request.Content.Headers.ContentEncoding.Count > 0);
                Bodies.Add(request.Content == null ? null : await request.Content.ReadAsByteArrayAsync());

                HttpStatusCode status = _statuses.Count > 0 ? _statuses.Dequeue() : HttpStatusCode.OK;
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
