// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;

using Microsoft.CodeAnalysis.Sarif.Driver;
using Microsoft.CodeAnalysis.Sarif.Emit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    /// <summary>
    /// Implements <c>multitool emit-init-run</c>: creates an append-only SARIF event log
    /// (<c>&lt;output&gt;.wip.jsonl</c>) seeded with a <c>run-header</c> event built from the
    /// supplied tool / repo flags.
    /// </summary>
    /// <remarks>
    /// <para>State table:</para>
    /// <list type="table">
    /// <listheader>
    /// <term>State</term>
    /// <term>No <c>--force-overwrite</c></term>
    /// <term>With <c>--force-overwrite</c></term>
    /// </listheader>
    /// <item>
    /// <term>Neither .sarif nor .wip.jsonl exists</term>
    /// <term>Create new .wip.jsonl</term>
    /// <term>Create new .wip.jsonl</term>
    /// </item>
    /// <item>
    /// <term>.sarif exists, no .wip.jsonl</term>
    /// <term>Fail — would clobber a committed SARIF on finalize</term>
    /// <term>Create new .wip.jsonl (existing .sarif is left until finalize replaces it)</term>
    /// </item>
    /// <item>
    /// <term>No .sarif, .wip.jsonl exists</term>
    /// <term>Fail — another authoring session is in flight (or was crashed)</term>
    /// <term>Delete .wip.jsonl and recreate</term>
    /// </item>
    /// <item>
    /// <term>Both .sarif and .wip.jsonl exist</term>
    /// <term>Fail</term>
    /// <term>Delete .wip.jsonl and recreate</term>
    /// </item>
    /// </list>
    /// </remarks>
    public class EmitInitRunCommand : CommandBase
    {
        internal const string SourceRootBaseId = "SRCROOT";

        // Allow-list for tool / repository documentation URIs (informationUri, repositoryUri).
        // Both anchor live documentation surfaced to humans, and we require https so we never
        // ship a clear-text link in the run header (http, file, and other schemes are blocked
        // here so the typo surfaces at emit-init-run rather than in a downstream consumer).
        private static readonly string[] s_documentationSchemes = new[] { Uri.UriSchemeHttps };

        // Allow-list for base URIs (originalUriBaseIds["SRCROOT"]). SARIF base IDs commonly
        // anchor at a local checkout (file://) or at a hosted source view (https://). http://
        // is excluded for the same reason as above.
        private static readonly string[] s_baseUriSchemes = new[] { Uri.UriSchemeHttps, Uri.UriSchemeFile };

        public int Run(EmitInitRunOptions options, IFileSystem fileSystem = null)
        {
            fileSystem ??= Sarif.FileSystem.Instance;

            try
            {
                if (string.IsNullOrWhiteSpace(options?.OutputFilePath))
                {
                    Console.Error.WriteLine("Output SARIF path is required.");
                    return FAILURE;
                }

                if (string.IsNullOrWhiteSpace(options.ToolName))
                {
                    Console.Error.WriteLine("--tool-driver-name is required.");
                    return FAILURE;
                }

                if (!TryValidateUri(options.InformationUri, "--information-uri", s_documentationSchemes, out string informationUriError))
                {
                    Console.Error.WriteLine(informationUriError);
                    return FAILURE;
                }

                if (!TryValidateUri(options.RepositoryUri, "--vcp-repositoryuri", s_documentationSchemes, out string repositoryUriError))
                {
                    Console.Error.WriteLine(repositoryUriError);
                    return FAILURE;
                }

                if (!TryValidateUri(options.SourceRoot, "--srcroot", s_baseUriSchemes, out string sourceRootError))
                {
                    Console.Error.WriteLine(sourceRootError);
                    return FAILURE;
                }

                string outputPath = Path.GetFullPath(options.OutputFilePath);
                string wipPath = outputPath + ".wip.jsonl";

                bool sarifExists = fileSystem.FileExists(outputPath);
                bool wipExists = fileSystem.FileExists(wipPath);

                if ((sarifExists || wipExists) && !options.ForceOverwrite)
                {
                    Console.Error.WriteLine(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            "'{0}'{1} already exists. Pass --force-overwrite to replace.",
                            outputPath,
                            wipExists && sarifExists ? " (and its .wip.jsonl)"
                            : wipExists ? " (.wip.jsonl)"
                            : string.Empty));
                    return FAILURE;
                }

                if (wipExists)
                {
                    fileSystem.FileDelete(wipPath);
                }

                string directory = Path.GetDirectoryName(wipPath);
                if (!string.IsNullOrEmpty(directory) && !fileSystem.DirectoryExists(directory))
                {
                    fileSystem.DirectoryCreateDirectory(directory);
                }

                Run run = BuildRunHeader(options);

                using (var writer = new SarifEventLogWriter(wipPath))
                {
                    writer.Append(SarifEventKinds.RunHeader, run);
                }

                Console.Out.WriteLine(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        "Opened '{0}' for '{1}'.",
                        wipPath,
                        options.ToolName));
                return SUCCESS;
            }
            catch (Exception ex) when (!Debugger.IsAttached)
            {
                Console.Error.WriteLine(ex);
                return FAILURE;
            }
        }

        internal static Run BuildRunHeader(EmitInitRunOptions options)
        {
            var driver = new ToolComponent
            {
                Name = options.ToolName,
                Version = NullIfEmpty(options.ToolVersion),
                Organization = NullIfEmpty(options.Organization),
            };

            if (!string.IsNullOrWhiteSpace(options.InformationUri))
            {
                driver.InformationUri = new Uri(options.InformationUri, UriKind.Absolute);
            }

            var run = new Run
            {
                Tool = new Tool { Driver = driver },
            };

            if (!string.IsNullOrWhiteSpace(options.RepositoryUri)
                || !string.IsNullOrWhiteSpace(options.RevisionId)
                || !string.IsNullOrWhiteSpace(options.Branch))
            {
                var vcd = new VersionControlDetails
                {
                    RevisionId = NullIfEmpty(options.RevisionId),
                    Branch = NullIfEmpty(options.Branch),
                };
                if (!string.IsNullOrWhiteSpace(options.RepositoryUri))
                {
                    vcd.RepositoryUri = new Uri(options.RepositoryUri, UriKind.Absolute);
                }

                run.VersionControlProvenance = new List<VersionControlDetails> { vcd };
            }

            if (!string.IsNullOrWhiteSpace(options.SourceRoot))
            {
                run.OriginalUriBaseIds = new Dictionary<string, ArtifactLocation>
                {
                    [SourceRootBaseId] = new ArtifactLocation
                    {
                        Uri = new Uri(options.SourceRoot, UriKind.Absolute),
                    },
                };
            }

            return run;
        }

        private static string NullIfEmpty(string s) => string.IsNullOrWhiteSpace(s) ? null : s;

        /// <summary>
        /// Validates that <paramref name="value"/> is either null/empty or a well-formed
        /// absolute URI whose scheme appears in <paramref name="allowedSchemes"/>.
        /// </summary>
        /// <remarks>
        /// Returning <c>true</c> when the value is empty preserves the "flag is optional"
        /// contract — only supplied URIs are validated. We require an absolute URI (relative
        /// values would never resolve meaningfully into a SARIF reader downstream) and we
        /// constrain the scheme to a documented allow-list so a typo like <c>"htps://..."</c>
        /// or an inappropriate scheme like <c>"file:..."</c> on a public-facing URL surfaces
        /// here rather than silently shipping in the run header.
        /// </remarks>
        private static bool TryValidateUri(string value, string flagName, string[] allowedSchemes, out string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                errorMessage = null;
                return true;
            }

            if (!Uri.TryCreate(value, UriKind.Absolute, out Uri uri))
            {
                errorMessage = string.Format(
                    CultureInfo.CurrentCulture,
                    "{0} value '{1}' is not a well-formed absolute URI.",
                    flagName,
                    value);
                return false;
            }

            bool schemeAllowed = false;
            for (int i = 0; i < allowedSchemes.Length; i++)
            {
                if (string.Equals(uri.Scheme, allowedSchemes[i], StringComparison.Ordinal))
                {
                    schemeAllowed = true;
                    break;
                }
            }

            if (!schemeAllowed)
            {
                errorMessage = string.Format(
                    CultureInfo.CurrentCulture,
                    "{0} value '{1}' uses scheme '{2}'; expected one of: {3}.",
                    flagName,
                    value,
                    uri.Scheme,
                    string.Join(", ", allowedSchemes));
                return false;
            }

            errorMessage = null;
            return true;
        }
    }
}

