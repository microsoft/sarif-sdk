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
    /// Implements <c>multitool emit-init</c>: creates an append-only SARIF event log
    /// (<c>&lt;output&gt;.wip.jsonl</c>) seeded with a <c>run-header</c> event built from the
    /// supplied tool / repo flags.
    /// </summary>
    /// <remarks>
    /// <para>State table:</para>
    /// <list type="table">
    /// <listheader>
    /// <term>State</term>
    /// <term>No <c>--allow-overwrite</c></term>
    /// <term>With <c>--allow-overwrite</c></term>
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
    public class EmitInitCommand : CommandBase
    {
        public int Run(EmitInitOptions options, IFileSystem fileSystem = null)
        {
            fileSystem ??= Sarif.FileSystem.Instance;

            try
            {
                if (string.IsNullOrWhiteSpace(options?.OutputFilePath))
                {
                    Console.Error.WriteLine("emit-init: output SARIF path is required.");
                    return FAILURE;
                }

                if (string.IsNullOrWhiteSpace(options.ToolName))
                {
                    Console.Error.WriteLine("emit-init: --tool is required.");
                    return FAILURE;
                }

                string outputPath = Path.GetFullPath(options.OutputFilePath);
                string wipPath = EmitConventions.GetWipPath(outputPath);

                bool sarifExists = fileSystem.FileExists(outputPath);
                bool wipExists = fileSystem.FileExists(wipPath);

                if ((sarifExists || wipExists) && !options.AllowOverwrite)
                {
                    Console.Error.WriteLine(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            "emit-init: '{0}'{1}{2} already exists. Pass --allow-overwrite to replace.",
                            outputPath,
                            wipExists ? " (and its .wip.jsonl)" : string.Empty,
                            sarifExists && wipExists ? string.Empty : sarifExists ? string.Empty : " (.wip.jsonl)"));
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
                        "emit-init: opened '{0}' for '{1}'.",
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

        internal static Run BuildRunHeader(EmitInitOptions options)
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
                    [EmitConventions.SourceRootBaseId] = new ArtifactLocation
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
