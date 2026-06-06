// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

using Microsoft.CodeAnalysis.Sarif.Driver;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    /// <summary>
    /// Implements <c>get-skill</c>: emits an embedded agent skill that drives the multitool emit and
    /// validate verbs.
    /// </summary>
    /// <remarks>
    /// The source skill under <c>skills/</c> links its references with repository-relative paths so it
    /// renders correctly in the repo. On the way out those links are rewritten to raw permalinks pinned
    /// to the build commit SHA, so the emitted skill resolves its references against the exact
    /// repository state that shipped the running tool.
    /// </remarks>
    public class GetSkillCommand : CommandBase
    {
        internal const string ResourcePrefix = "Microsoft.CodeAnalysis.Sarif.Multitool.Skills.";

        internal const string RawContentBaseUrl = "https://raw.githubusercontent.com/microsoft/sarif-sdk/";

        /// <summary>
        /// Maps each skill to the repository-relative directory of its <c>SKILL.md</c>. The directory
        /// anchors resolution of the skill's repository-relative links into release-pinned permalinks.
        /// </summary>
        internal static readonly IReadOnlyDictionary<string, string> SkillSourceDirectory =
            new SortedDictionary<string, string>(StringComparer.Ordinal)
            {
                ["emit-sarif-findings"] = "skills/emit-sarif-findings",
                ["validate-sarif-findings"] = "skills/validate-sarif-findings",
            };

        private static readonly Regex s_markdownLink = new Regex(
            @"\]\((?<url>[^)\s]+)(?<title>\s+""[^""]*"")?\)",
            RegexOptions.Compiled);

        private static readonly Regex s_uriScheme = new Regex(
            @"^[a-zA-Z][a-zA-Z0-9+.\-]*:",
            RegexOptions.Compiled);

        // The build commit SHA SourceLink appends to the informational version as "<version>+<sha>".
        private static readonly Regex s_commitSha = new Regex(
            @"\+(?<sha>[0-9a-fA-F]{7,64})",
            RegexOptions.Compiled);

        public int Run(GetSkillOptions options, IFileSystem fileSystem = null)
        {
            fileSystem ??= Sarif.FileSystem.Instance;

            try
            {
                if (options.List)
                {
                    Console.Out.WriteLine(BuildSkillList());
                    return SUCCESS;
                }

                if (string.IsNullOrWhiteSpace(options.Skill))
                {
                    Console.Error.WriteLine("error: specify a skill to emit, or pass --list.");
                    Console.Error.WriteLine(BuildSkillList());
                    return FAILURE;
                }

                string skill = options.Skill.Trim();
                if (!SkillSourceDirectory.TryGetValue(skill, out string sourceDirectory))
                {
                    Console.Error.WriteLine(
                        string.Format(CultureInfo.CurrentCulture, "error: '{0}' is not an available skill.", skill));
                    Console.Error.WriteLine(BuildSkillList());
                    return FAILURE;
                }

                byte[] sourceBytes = ReadEmbeddedSkill(skill);
                if (sourceBytes == null)
                {
                    Console.Error.WriteLine(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            "error: embedded skill resource '{0}{1}.SKILL.md' was not found.",
                            ResourcePrefix,
                            skill));
                    return FAILURE;
                }

                Assembly assembly = typeof(GetSkillCommand).Assembly;
                string pinRef = ResolvePinRef(
                    assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion,
                    assembly.GetName().Version);

                string rendered = RewriteRelativeLinks(
                    DecodeUtf8(sourceBytes),
                    sourceDirectory,
                    pinRef);

                byte[] outputBytes = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false).GetBytes(rendered);

                if (!string.IsNullOrEmpty(options.OutputFilePath))
                {
                    if (fileSystem.FileExists(options.OutputFilePath) && !options.ForceOverwrite)
                    {
                        Console.Error.WriteLine(
                            string.Format(
                                CultureInfo.CurrentCulture,
                                "error: '{0}' already exists. Pass --force-overwrite to replace it.",
                                options.OutputFilePath));
                        return FAILURE;
                    }

                    fileSystem.FileWriteAllBytes(options.OutputFilePath, outputBytes);
                    Console.Out.WriteLine(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            "Wrote skill '{0}' to '{1}'.",
                            skill,
                            options.OutputFilePath));
                    return SUCCESS;
                }

                using (Stream stdout = Console.OpenStandardOutput())
                {
                    stdout.Write(outputBytes, 0, outputBytes.Length);
                }

                return SUCCESS;
            }
            catch (Exception ex) when (!Debugger.IsAttached)
            {
                Console.Error.WriteLine(ex);
                return FAILURE;
            }
        }

        internal static byte[] ReadEmbeddedSkill(string skillName)
        {
            Assembly assembly = typeof(GetSkillCommand).Assembly;
            using (Stream stream = assembly.GetManifestResourceStream(ResourcePrefix + skillName + ".SKILL.md"))
            {
                if (stream == null) { return null; }

                using (var memory = new MemoryStream())
                {
                    stream.CopyTo(memory);
                    return memory.ToArray();
                }
            }
        }

        /// <summary>
        /// Resolves the git ref the skill's links are pinned to. Prefers the exact build commit SHA
        /// that SourceLink stamps into the assembly informational version (<c>&lt;version&gt;+&lt;sha&gt;</c>),
        /// so the emitted links resolve to the precise repository state that shipped the running tool —
        /// the same tree the embedded skill was taken from. Falls back to the version tag when no SHA
        /// is stamped (e.g. a build with no git metadata).
        /// </summary>
        internal static string ResolvePinRef(string informationalVersion, Version assemblyVersion)
        {
            if (!string.IsNullOrEmpty(informationalVersion))
            {
                Match sha = s_commitSha.Match(informationalVersion);
                if (sha.Success)
                {
                    return sha.Groups["sha"].Value;
                }
            }

            return ResolveReleaseTag(assemblyVersion);
        }

        /// <summary>
        /// Derives the version tag (e.g. <c>v5.0.2</c>) from the assembly version, which tracks the
        /// package's <c>VersionPrefix</c>. Used only as a fallback when no build commit SHA is
        /// available to pin against.
        /// </summary>
        internal static string ResolveReleaseTag(Version version)
        {
            version ??= new Version(0, 0, 0);
            return string.Format(
                CultureInfo.InvariantCulture,
                "v{0}.{1}.{2}",
                version.Major,
                version.Minor,
                Math.Max(version.Build, 0));
        }

        /// <summary>
        /// Rewrites every repository-relative markdown link in <paramref name="markdown"/> to a raw
        /// permalink pinned to <paramref name="tag"/>. Absolute URLs, protocol-relative URLs, and bare
        /// fragments are left untouched.
        /// </summary>
        internal static string RewriteRelativeLinks(string markdown, string skillSourceDirectory, string tag)
        {
            return s_markdownLink.Replace(markdown, match =>
            {
                string url = match.Groups["url"].Value;
                if (!IsRepositoryRelative(url)) { return match.Value; }

                int fragmentIndex = url.IndexOf('#');
                string path = fragmentIndex >= 0 ? url.Substring(0, fragmentIndex) : url;
                string fragment = fragmentIndex >= 0 ? url.Substring(fragmentIndex) : string.Empty;

                string repositoryPath = ResolveRepositoryRelative(skillSourceDirectory, path);

                return "](" + RawContentBaseUrl + tag + "/" + repositoryPath + fragment + match.Groups["title"].Value + ")";
            });
        }

        private static bool IsRepositoryRelative(string url)
        {
            if (string.IsNullOrEmpty(url)) { return false; }
            if (url[0] == '#') { return false; }
            if (url.StartsWith("//", StringComparison.Ordinal)) { return false; }
            if (s_uriScheme.IsMatch(url)) { return false; }
            return true;
        }

        /// <summary>
        /// Resolves a relative path against the skill's repository directory into a repository-root
        /// path, collapsing <c>.</c> and <c>..</c> segments.
        /// </summary>
        private static string ResolveRepositoryRelative(string baseDirectory, string relativePath)
        {
            var segments = new List<string>(baseDirectory.Split('/'));

            foreach (string segment in relativePath.Split('/'))
            {
                if (segment.Length == 0 || segment == ".")
                {
                    continue;
                }

                if (segment == "..")
                {
                    if (segments.Count > 0) { segments.RemoveAt(segments.Count - 1); }
                    continue;
                }

                segments.Add(segment);
            }

            return string.Join("/", segments);
        }

        private static string DecodeUtf8(byte[] bytes)
        {
            using (var reader = new StreamReader(new MemoryStream(bytes), Encoding.UTF8, detectEncodingFromByteOrderMarks: true))
            {
                return reader.ReadToEnd();
            }
        }

        private static string BuildSkillList()
        {
            return "Available skills:" + Environment.NewLine
                + string.Concat(SkillSourceDirectory.Keys.Select(name => "  " + name + Environment.NewLine)).TrimEnd();
        }
    }
}
