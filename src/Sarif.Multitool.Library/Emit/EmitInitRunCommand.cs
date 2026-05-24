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

                if (!EmitEventLogHelpers.TryValidateUri(options.InformationUri, "--information-uri", EmitEventLogHelpers.DocumentationUriSchemes, out string informationUriError))
                {
                    Console.Error.WriteLine(informationUriError);
                    return FAILURE;
                }

                if (!EmitEventLogHelpers.TryValidateUri(options.RepositoryUri, "--vcp-repositoryuri", EmitEventLogHelpers.DocumentationUriSchemes, out string repositoryUriError))
                {
                    Console.Error.WriteLine(repositoryUriError);
                    return FAILURE;
                }

                if (!EmitEventLogHelpers.TryValidateUri(options.SourceRoot, "--srcroot", EmitEventLogHelpers.BaseUriSchemes, out string sourceRootError))
                {
                    Console.Error.WriteLine(sourceRootError);
                    return FAILURE;
                }

                if (!TryParseGuid(options.AutomationGuid, "--automation-guid", out Guid? automationGuid))
                {
                    return FAILURE;
                }

                if (!TryParseGuid(options.AutomationCorrelationGuid, "--automation-correlation-guid", out Guid? automationCorrelationGuid))
                {
                    return FAILURE;
                }

                if (!TryValidateAiOrigin(options.AiOrigin, out string aiOriginError))
                {
                    Console.Error.WriteLine(aiOriginError);
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
                SemanticVersion = NullIfEmpty(options.ToolDriverSemanticVersion),
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

            if (TryParseGuid(options.AutomationGuid, "--automation-guid", out Guid? automationGuid)
                && TryParseGuid(options.AutomationCorrelationGuid, "--automation-correlation-guid", out Guid? automationCorrelationGuid)
                && (automationGuid.HasValue || automationCorrelationGuid.HasValue))
            {
                run.AutomationDetails = new RunAutomationDetails
                {
                    Guid = automationGuid,
                    CorrelationGuid = automationCorrelationGuid,
                };
            }

            if (!string.IsNullOrWhiteSpace(options.AiOrigin))
            {
                run.SetProperty("ai/origin", options.AiOrigin.Trim());
            }

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

                if (!string.IsNullOrWhiteSpace(options.SourceRoot))
                {
                    vcd.MappedTo = new ArtifactLocation { UriBaseId = SourceRootBaseId };
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

        internal static bool TryParseGuid(string raw, string flagName, out Guid? guid)
        {
            guid = null;
            if (string.IsNullOrWhiteSpace(raw))
            {
                return true;
            }

            if (!Guid.TryParseExact(raw.Trim(), "D", out Guid parsed))
            {
                Console.Error.WriteLine(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        "{0}: '{1}' is not a valid GUID (expected canonical 8-4-4-4-12 hex form, e.g. a7ad9ab8-1234-5678-9abc-def012345678).",
                        flagName,
                        raw));
                return false;
            }

            guid = parsed;
            return true;
        }

        internal static readonly string[] AiOriginValues = new[] { "generated", "annotated", "synthesized" };

        internal static bool TryValidateAiOrigin(string raw, out string error)
        {
            error = null;
            if (string.IsNullOrWhiteSpace(raw))
            {
                return true;
            }

            string trimmed = raw.Trim();
            foreach (string v in AiOriginValues)
            {
                if (string.Equals(v, trimmed, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            error = string.Format(
                CultureInfo.CurrentCulture,
                "--ai-origin: '{0}' is not valid. Expected one of: {1}.",
                raw,
                string.Join(", ", AiOriginValues));
            return false;
        }

        private static string NullIfEmpty(string s) => string.IsNullOrWhiteSpace(s) ? null : s;
    }
}

