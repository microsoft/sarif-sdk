// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

using Microsoft.CodeAnalysis.Sarif.Driver;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    /// <summary>
    /// Implements <c>publish-to-ghas</c>: uploads a SARIF file to GitHub Advanced Security code
    /// scanning. The target <c>owner/repo</c>, the <c>commit_sha</c>, and the <c>ref</c> are derived
    /// from the run's <c>versionControlProvenance</c>, and the bearer token is read from an
    /// environment variable named by <c>--token-env-var</c> so it never appears on the command line
    /// or in diagnostics.
    /// </summary>
    /// <remarks>
    /// The token is always sent as <c>Authorization: Bearer</c> — GitHub accepts a classic or
    /// fine-grained personal access token carrying <c>security_events</c> write. The SARIF body is
    /// gzip-compressed and base64-encoded into the <c>sarif</c> field of a JSON payload posted to
    /// <c>https://&lt;api-host&gt;/repos/&lt;owner&gt;/&lt;repo&gt;/code-scanning/sarifs</c>, where the
    /// API host is <c>api.github.com</c> for a <c>github.com</c> repository (or
    /// <c>api.&lt;slug&gt;.ghe.com</c> for a data-residency host). GitHub answers a successful upload
    /// with <c>202 Accepted</c>.
    /// </remarks>
    public class PublishToGhasCommand : CommandBase
    {
        private const string GitHubApiVersion = "2022-11-28";
        private const string UserAgent = "Sarif.Multitool";

        private readonly HttpMessageHandler httpMessageHandler;

        public PublishToGhasCommand(HttpMessageHandler httpMessageHandler = null)
        {
            this.httpMessageHandler = httpMessageHandler;
        }

        public int Run(PublishToGhasOptions options, IFileSystem fileSystem = null)
        {
            fileSystem ??= Sarif.FileSystem.Instance;

            string secretForRedaction = null;

            try
            {
                if (!IsValidEnvironmentVariableName(options.TokenEnvironmentVariable))
                {
                    // Never echo the supplied value; it may be a token passed in place of a name.
                    Console.Error.WriteLine("error: --token-env-var must be the NAME of an environment variable (letters, digits, and underscores; not starting with a digit), not a token value.");
                    return FAILURE;
                }

                if (!fileSystem.FileExists(options.SarifPath))
                {
                    Console.Error.WriteLine(
                        string.Format(CultureInfo.CurrentCulture, "error: SARIF file '{0}' was not found.", options.SarifPath));
                    return FAILURE;
                }

                SarifLog log = ReadSarifFile<SarifLog>(fileSystem, options.SarifPath);

                if (EmitFinalizeCommand.IsMarkedUnpublishable(log))
                {
                    Console.Error.WriteLine("error: this SARIF was finalized with emit-finalize --no-repo (no version-control provenance) and is marked unpublishable. A non-version-controlled scan cannot be uploaded to GitHub Advanced Security code scanning, which anchors every alert to a repository and commit; publishing ingests every run, so a single unpublishable run refuses the whole file. Finalize with versionControlProvenance present (without --no-repo) to publish.");
                    return FAILURE;
                }

                if (!TryGetGitHubProvenance(log, out Uri repositoryUri, out string commitSha, out string gitRef, out string vcpError))
                {
                    Console.Error.WriteLine(string.Format(CultureInfo.CurrentCulture, "error: {0}", vcpError));
                    return FAILURE;
                }

                if (!VcpPortableRoot.TryGetGitHubTarget(repositoryUri, out string owner, out string repository, out string apiHost, out string targetError))
                {
                    Console.Error.WriteLine(string.Format(CultureInfo.CurrentCulture, "error: {0}", targetError));
                    return FAILURE;
                }

                byte[] rawBytes = fileSystem.FileReadAllBytes(options.SarifPath);
                byte[] gzipBytes = Gzip(rawBytes);
                string sarifBase64 = Convert.ToBase64String(gzipBytes);

                if (options.DryRun)
                {
                    return ReportDryRun(options, repositoryUri, apiHost, owner, repository, commitSha, gitRef, rawBytes.Length, gzipBytes.Length);
                }

                string secret = Environment.GetEnvironmentVariable(options.TokenEnvironmentVariable);
                if (string.IsNullOrWhiteSpace(secret))
                {
                    Console.Error.WriteLine(
                        "error: the environment variable named by --token-env-var is not set or is empty; it must hold a GitHub personal access token with security_events write.");
                    return FAILURE;
                }

                secret = secret.Trim();
                secretForRedaction = secret;

                return Upload(options, apiHost, owner, repository, commitSha, gitRef, sarifBase64, secret);
            }
            catch (Exception ex) when (!Debugger.IsAttached)
            {
                // Redact the token from any exception text before surfacing it.
                Console.Error.WriteLine(string.Format(CultureInfo.CurrentCulture, "error: {0}", Redact(ex.Message, secretForRedaction)));
                return FAILURE;
            }
        }

        private int Upload(PublishToGhasOptions options, string apiHost, string owner, string repository, string commitSha, string gitRef, string sarifBase64, string secret)
        {
            HttpMessageHandler handler = this.httpMessageHandler ?? new HttpClientHandler { AllowAutoRedirect = false };
            int statusCode = 0;
            string responseBody = null;

            using (var httpClient = new HttpClient(handler, disposeHandler: this.httpMessageHandler == null))
            {
                string url = BuildUrl(apiHost, owner, repository);
                Console.Out.WriteLine(string.Format(CultureInfo.CurrentCulture, "POST {0}", url));

                using (var request = new HttpRequestMessage(HttpMethod.Post, url))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", secret);
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
                    request.Headers.UserAgent.TryParseAdd(UserAgent);
                    request.Headers.TryAddWithoutValidation("X-GitHub-Api-Version", GitHubApiVersion);
                    request.Content = new StringContent(BuildRequestBody(commitSha, gitRef, sarifBase64), Encoding.UTF8, "application/json");

                    using (HttpResponseMessage response = httpClient.SendAsync(request).GetAwaiter().GetResult())
                    {
                        statusCode = (int)response.StatusCode;
                        responseBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    }
                }

                Console.Out.WriteLine(string.Format(CultureInfo.CurrentCulture, "Status: {0}", statusCode));
            }

            if (!string.IsNullOrEmpty(responseBody))
            {
                // Redact the token from the server's diagnostic body before surfacing it.
                Console.Out.WriteLine(Redact(responseBody, secret));
            }

            if (statusCode < 200 || statusCode >= 300)
            {
                Console.Error.WriteLine(string.Format(CultureInfo.CurrentCulture, "error: publish failed with HTTP {0}.", statusCode));
                return FAILURE;
            }

            Console.Out.WriteLine(
                string.Format(CultureInfo.CurrentCulture, "Published '{0}' to GHAS ({1}/{2}).", options.SarifPath, owner, repository));
            return SUCCESS;
        }

        private int ReportDryRun(PublishToGhasOptions options, Uri repositoryUri, string apiHost, string owner, string repository, string commitSha, string gitRef, int rawLength, int gzipLength)
        {
            Console.Out.WriteLine("publish-to-ghas (dry run): no network request will be made.");
            Console.Out.WriteLine(string.Format(CultureInfo.CurrentCulture, "  repositoryUri : {0}", repositoryUri));
            Console.Out.WriteLine(string.Format(CultureInfo.CurrentCulture, "  owner/repo    : {0}/{1}", owner, repository));
            Console.Out.WriteLine(string.Format(CultureInfo.CurrentCulture, "  ref           : {0}", gitRef));
            Console.Out.WriteLine(string.Format(CultureInfo.CurrentCulture, "  commit_sha    : {0}", commitSha));
            Console.Out.WriteLine(
                string.Format(CultureInfo.CurrentCulture, "  body          : {0:n0} raw bytes -> {1:n0} gzip bytes, base64-encoded into the JSON 'sarif' field", rawLength, gzipLength));
            Console.Out.WriteLine(string.Format(CultureInfo.CurrentCulture, "  POST          : {0}", BuildUrl(apiHost, owner, repository)));

            return SUCCESS;
        }

        private static bool TryGetGitHubProvenance(SarifLog log, out Uri repositoryUri, out string commitSha, out string gitRef, out string error)
        {
            repositoryUri = null;
            commitSha = null;
            gitRef = null;
            error = null;

            Run run = log?.Runs?.FirstOrDefault();
            if (run == null)
            {
                error = "the SARIF file contains no runs.";
                return false;
            }

            VersionControlDetails versionControl = run.VersionControlProvenance?.FirstOrDefault();
            if (versionControl?.RepositoryUri == null)
            {
                error = "the SARIF run carries no versionControlProvenance[0].repositoryUri; finalize the SARIF (for example with 'sarif emit-finalize') before publishing.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(versionControl.RevisionId))
            {
                error = "the SARIF run carries no versionControlProvenance[0].revisionId; GitHub code scanning anchors an analysis to a commit. Finalize against a checked-out commit before publishing.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(versionControl.Branch))
            {
                error = "the SARIF run carries no versionControlProvenance[0].branch; GitHub code scanning requires a fully-qualified ref such as refs/heads/main. Finalize with the branch present before publishing.";
                return false;
            }

            repositoryUri = versionControl.RepositoryUri;
            commitSha = versionControl.RevisionId;
            gitRef = versionControl.Branch;
            return true;
        }

        private static string BuildRequestBody(string commitSha, string gitRef, string sarifBase64)
        {
            var payload = new JObject
            {
                ["commit_sha"] = commitSha,
                ["ref"] = gitRef,
                ["sarif"] = sarifBase64,
            };

            return payload.ToString(Formatting.None);
        }

        private static string BuildUrl(string apiHost, string owner, string repository)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "https://{0}/repos/{1}/{2}/code-scanning/sarifs",
                apiHost,
                owner,
                repository);
        }

        private static byte[] Gzip(byte[] rawBytes)
        {
            using (var memory = new MemoryStream())
            {
                using (var gzip = new GZipStream(memory, CompressionLevel.Optimal, leaveOpen: true))
                {
                    gzip.Write(rawBytes, 0, rawBytes.Length);
                }

                return memory.ToArray();
            }
        }

        private static bool IsValidEnvironmentVariableName(string name)
        {
            if (string.IsNullOrEmpty(name)) { return false; }

            if (!((name[0] >= 'A' && name[0] <= 'Z') || (name[0] >= 'a' && name[0] <= 'z') || name[0] == '_'))
            {
                return false;
            }

            foreach (char c in name)
            {
                bool ok = (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == '_';
                if (!ok) { return false; }
            }

            return true;
        }

        private static string Redact(string text, string secret)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(secret)) { return text; }

            return text.Replace(secret, "***");
        }
    }
}
