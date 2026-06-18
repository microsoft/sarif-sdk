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

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    /// <summary>
    /// Implements <c>publish-to-ghazdo</c>: uploads a SARIF file to GitHub Advanced Security for
    /// Azure DevOps. The target organization, project, and repository are derived from the run's
    /// <c>versionControlProvenance</c>, and the bearer secret is read from an environment variable
    /// named by <c>--token-env-var</c> so it never appears on the command line or in diagnostics.
    /// </summary>
    /// <remarks>
    /// The secret kind selects the authorization scheme: an Entra access token is a JSON Web Token and
    /// is sent as <c>Bearer</c>; an Azure DevOps personal access token is opaque and is sent as
    /// <c>Basic</c> with an empty user name. The body is gzip-compressed in memory and posted as
    /// <c>application/octet-stream</c> with no <c>Content-Encoding</c> header, because the ingestion
    /// endpoint gunzips the payload itself. The upload targets <c>advsec.dev.azure.com</c> and falls
    /// back to <c>dev.azure.com</c> on a 404.
    /// </remarks>
    public class PublishToGhazdoCommand : CommandBase
    {
        private const string AdvancedSecurityHost = "advsec.dev.azure.com";
        private const string AzureDevOpsHost = "dev.azure.com";

        private readonly HttpMessageHandler httpMessageHandler;

        public PublishToGhazdoCommand(HttpMessageHandler httpMessageHandler = null)
        {
            this.httpMessageHandler = httpMessageHandler;
        }

        public int Run(PublishToGhazdoOptions options, IFileSystem fileSystem = null)
        {
            fileSystem ??= Sarif.FileSystem.Instance;

            string secretForRedaction = null;

            try
            {
                if (!IsValidEnvironmentVariableName(options.TokenEnvironmentVariable))
                {
                    // Never echo the supplied value; it may be a secret passed in place of a name.
                    Console.Error.WriteLine("error: --token-env-var must be the NAME of an environment variable (letters, digits, and underscores; not starting with a digit), not a secret value.");
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
                    Console.Error.WriteLine("error: this SARIF was finalized with emit-finalize --no-repo (no version-control provenance) and is marked unpublishable. A non-version-controlled scan cannot be uploaded to GitHub Advanced Security for Azure DevOps, which anchors every alert to a repository and commit. Finalize with versionControlProvenance present (without --no-repo) to publish.");
                    return FAILURE;
                }

                if (!TryGetRepositoryUri(log, out Uri repositoryUri, out string vcpError))
                {
                    Console.Error.WriteLine(string.Format(CultureInfo.CurrentCulture, "error: {0}", vcpError));
                    return FAILURE;
                }

                if (!VcpPortableRoot.TryGetAzureDevOpsTarget(repositoryUri, out string organization, out string project, out string repository, out string targetError))
                {
                    Console.Error.WriteLine(string.Format(CultureInfo.CurrentCulture, "error: {0}", targetError));
                    return FAILURE;
                }

                byte[] rawBytes = fileSystem.FileReadAllBytes(options.SarifPath);
                byte[] gzipBytes = Gzip(rawBytes);

                if (options.DryRun)
                {
                    return ReportDryRun(options, repositoryUri, organization, project, repository, rawBytes.Length, gzipBytes.Length);
                }

                string secret = Environment.GetEnvironmentVariable(options.TokenEnvironmentVariable);
                if (string.IsNullOrWhiteSpace(secret))
                {
                    Console.Error.WriteLine(
                        "error: the environment variable named by --token-env-var is not set or is empty; it must hold an Azure DevOps personal access token or an Entra access token.");
                    return FAILURE;
                }

                secret = secret.Trim();
                secretForRedaction = secret;
                AuthenticationHeaderValue authorization = BuildAuthorization(secret, out string scheme);
                Console.Out.WriteLine(string.Format(CultureInfo.CurrentCulture, "Authorization scheme: {0}", scheme));

                return Upload(options, organization, project, repository, gzipBytes, authorization, secret);
            }
            catch (Exception ex) when (!Debugger.IsAttached)
            {
                // Redact the secret and its Basic-encoded form from any exception text.
                Console.Error.WriteLine(string.Format(CultureInfo.CurrentCulture, "error: {0}", Redact(ex.Message, secretForRedaction)));
                return FAILURE;
            }
        }

        private int Upload(PublishToGhazdoOptions options, string organization, string project, string repository, byte[] gzipBytes, AuthenticationHeaderValue authorization, string secret)
        {
            HttpMessageHandler handler = this.httpMessageHandler ?? new HttpClientHandler { AllowAutoRedirect = false };
            int statusCode = 0;
            string responseBody = null;

            using (var httpClient = new HttpClient(handler, disposeHandler: this.httpMessageHandler == null))
            {
                foreach (string adoHost in new[] { AdvancedSecurityHost, AzureDevOpsHost })
                {
                    string url = BuildUrl(adoHost, organization, project, repository, options.ApiVersion);
                    Console.Out.WriteLine(string.Format(CultureInfo.CurrentCulture, "POST {0}", url));

                    using (var request = new HttpRequestMessage(HttpMethod.Post, url))
                    {
                        request.Headers.Authorization = authorization;

                        var content = new ByteArrayContent(gzipBytes);
                        content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                        request.Content = content;

                        using (HttpResponseMessage response = httpClient.SendAsync(request).GetAwaiter().GetResult())
                        {
                            statusCode = (int)response.StatusCode;
                            responseBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                        }
                    }

                    Console.Out.WriteLine(string.Format(CultureInfo.CurrentCulture, "Status: {0}", statusCode));
                    if (statusCode != 404) { break; }

                    Console.Out.WriteLine("(404 — falling back to next host.)");
                }
            }

            if (!string.IsNullOrEmpty(responseBody))
            {
                // Redact the secret from the server's diagnostic body before surfacing it.
                Console.Out.WriteLine(Redact(responseBody, secret));
            }

            if (statusCode < 200 || statusCode >= 300)
            {
                Console.Error.WriteLine(string.Format(CultureInfo.CurrentCulture, "error: publish failed with HTTP {0}.", statusCode));
                return FAILURE;
            }

            Console.Out.WriteLine(
                string.Format(CultureInfo.CurrentCulture, "Published '{0}' to GHAzDO ({1}/{2}/{3}).", options.SarifPath, organization, project, repository));
            return SUCCESS;
        }

        private int ReportDryRun(PublishToGhazdoOptions options, Uri repositoryUri, string organization, string project, string repository, int rawLength, int gzipLength)
        {
            string secret = Environment.GetEnvironmentVariable(options.TokenEnvironmentVariable);
            string scheme = string.IsNullOrWhiteSpace(secret)
                ? "(the --token-env-var environment variable is unset; scheme is detected at publish time)"
                : DetectScheme(secret.Trim());

            Console.Out.WriteLine("publish-to-ghazdo (dry run): no network request will be made.");
            Console.Out.WriteLine(string.Format(CultureInfo.CurrentCulture, "  repositoryUri    : {0}", repositoryUri));
            Console.Out.WriteLine(string.Format(CultureInfo.CurrentCulture, "  org/project/repo : {0}/{1}/{2}", organization, project, repository));
            Console.Out.WriteLine(string.Format(CultureInfo.CurrentCulture, "  auth scheme      : {0}", scheme));
            Console.Out.WriteLine(
                string.Format(CultureInfo.CurrentCulture, "  body             : {0:n0} raw bytes -> {1:n0} gzip bytes (application/octet-stream, no Content-Encoding)", rawLength, gzipLength));

            foreach (string adoHost in new[] { AdvancedSecurityHost, AzureDevOpsHost })
            {
                Console.Out.WriteLine(
                    string.Format(CultureInfo.CurrentCulture, "  POST             : {0}", BuildUrl(adoHost, organization, project, repository, options.ApiVersion)));
            }

            return SUCCESS;
        }

        internal static AuthenticationHeaderValue BuildAuthorization(string secret, out string scheme)
        {
            scheme = DetectScheme(secret);
            return scheme == "Bearer"
                ? new AuthenticationHeaderValue("Bearer", secret)
                : new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(":" + secret)));
        }

        /// <summary>
        /// Selects the authorization scheme for <paramref name="secret"/>. An Entra access token is a
        /// JSON Web Token (<c>Bearer</c>); an opaque Azure DevOps personal access token is wrapped as
        /// <c>Basic</c> with an empty user name.
        /// </summary>
        internal static string DetectScheme(string secret)
        {
            return LooksLikeJsonWebToken(secret) ? "Bearer" : "Basic";
        }

        internal static bool LooksLikeJsonWebToken(string secret)
        {
            if (string.IsNullOrEmpty(secret)) { return false; }

            // A JWT is three base64url segments whose first segment decodes to a JSON object. Anything
            // else falls through to Basic, so an opaque PAT is never sent as a raw Bearer token.
            string[] parts = secret.Split('.');
            if (parts.Length != 3) { return false; }

            if (!IsBase64Url(parts[0]) || !IsBase64Url(parts[1]) || !IsBase64Url(parts[2]))
            {
                return false;
            }

            if (!TryDecodeBase64Url(parts[0], out string header))
            {
                return false;
            }

            return header.TrimStart().StartsWith("{", StringComparison.Ordinal);
        }

        private static bool IsBase64Url(string value)
        {
            if (string.IsNullOrEmpty(value)) { return false; }

            foreach (char c in value)
            {
                bool ok = (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == '-' || c == '_';
                if (!ok) { return false; }
            }

            return true;
        }

        private static bool TryDecodeBase64Url(string value, out string decoded)
        {
            decoded = null;

            string base64 = value.Replace('-', '+').Replace('_', '/');
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
                case 1: return false;
            }

            try
            {
                decoded = Encoding.UTF8.GetString(Convert.FromBase64String(base64));
                return true;
            }
            catch (FormatException)
            {
                return false;
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

            string redacted = text.Replace(secret, "***");
            string basicParameter = Convert.ToBase64String(Encoding.ASCII.GetBytes(":" + secret));
            return redacted.Replace(basicParameter, "***");
        }

        private static bool TryGetRepositoryUri(SarifLog log, out Uri repositoryUri, out string error)
        {
            repositoryUri = null;
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

            repositoryUri = versionControl.RepositoryUri;
            return true;
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

        private static string BuildUrl(string adoHost, string organization, string project, string repository, string apiVersion)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "https://{0}/{1}/{2}/_apis/alert/repositories/{3}/sarifs?api-version={4}",
                adoHost,
                organization,
                project,
                repository,
                apiVersion);
        }
    }
}
