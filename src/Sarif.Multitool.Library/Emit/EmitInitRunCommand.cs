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

                if (!string.IsNullOrWhiteSpace(options.InformationUri)
                    && !Uri.IsWellFormedUriString(options.InformationUri, UriKind.RelativeOrAbsolute))
                {
                    Console.Error.WriteLine(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            "--information-uri value '{0}' is not a valid URI.",
                            options.InformationUri));
                    return FAILURE;
                }

                if (!string.IsNullOrWhiteSpace(options.RepositoryUri)
                    && !Uri.IsWellFormedUriString(options.RepositoryUri, UriKind.RelativeOrAbsolute))
                {
                    Console.Error.WriteLine(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            "--vcp-repositoryuri value '{0}' is not a valid URI.",
                            options.RepositoryUri));
                    return FAILURE;
                }

                if (!string.IsNullOrWhiteSpace(options.SourceRoot)
                    && !Uri.IsWellFormedUriString(options.SourceRoot, UriKind.RelativeOrAbsolute))
                {
                    Console.Error.WriteLine(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            "--srcroot value '{0}' is not a valid URI.",
                            options.SourceRoot));
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
                driver.InformationUri = new Uri(options.InformationUri, UriKind.RelativeOrAbsolute);
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
                    vcd.RepositoryUri = new Uri(options.RepositoryUri, UriKind.RelativeOrAbsolute);
                }

                run.VersionControlProvenance = new List<VersionControlDetails> { vcd };
            }

            if (!string.IsNullOrWhiteSpace(options.SourceRoot))
            {
                run.OriginalUriBaseIds = new Dictionary<string, ArtifactLocation>
                {
                    [SourceRootBaseId] = new ArtifactLocation
                    {
                        Uri = new Uri(options.SourceRoot, UriKind.RelativeOrAbsolute),
                    },
                };
            }

            return run;
        }

        private static string NullIfEmpty(string s) => string.IsNullOrWhiteSpace(s) ? null : s;
    }
}

