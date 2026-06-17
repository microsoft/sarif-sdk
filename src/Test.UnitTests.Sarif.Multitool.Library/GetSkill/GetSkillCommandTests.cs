// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif.Driver;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    // Pins the get-skill verb to its drift-guarded source of truth. Unlike get-schema, the served
    // bytes are NOT byte-identical to the embedded resource: repository-relative links are rewritten
    // to release-pinned raw permalinks on the way out. These tests therefore split the guarantee in
    // two — (1) the embedded resource is byte-identical to the skill source on disk, and (2) the
    // emitted document carries the expected permalink rewrite with no residual relative links.
    public class GetSkillCommandTests : IDisposable
    {
        private readonly string _dir;

        public GetSkillCommandTests()
        {
            _dir = Path.Combine(Path.GetTempPath(), $"get-skill-{Guid.NewGuid():N}");
            Directory.CreateDirectory(_dir);
        }

        public void Dispose()
        {
            if (Directory.Exists(_dir)) { Directory.Delete(_dir, recursive: true); }
        }

        [Fact]
        public void GetSkill_EverySkill_EmbeddedBytesMatchDiskSource()
        {
            string repositoryRoot = FindRepositoryRoot();
            var offenders = new List<string>();

            foreach (KeyValuePair<string, string> entry in GetSkillCommand.SkillSourceDirectory)
            {
                string skill = entry.Key;
                string sourceDirectory = entry.Value;

                byte[] embedded = GetSkillCommand.ReadEmbeddedSkill(skill);
                if (embedded == null)
                {
                    offenders.Add($"  [{skill}] embedded resource was not found.");
                    continue;
                }

                string diskPath = Path.Combine(
                    repositoryRoot,
                    sourceDirectory.Replace('/', Path.DirectorySeparatorChar),
                    "SKILL.md");

                if (!File.Exists(diskPath))
                {
                    offenders.Add($"  [{skill}] source file was not found at '{diskPath}'.");
                    continue;
                }

                if (!embedded.SequenceEqual(File.ReadAllBytes(diskPath)))
                {
                    offenders.Add($"  [{skill}] embedded bytes differ from the source file on disk.");
                }
            }

            offenders.Should().BeEmpty(
                "the embedded skill resource must be byte-identical to its skills\\<name>\\SKILL.md " +
                "source:\n" + string.Join("\n", offenders));
        }

        [Fact]
        public void GetSkill_CatalogSkills_MatchEmbeddedResources()
        {
            IEnumerable<string> catalogResources = GetSkillCommand.SkillSourceDirectory.Keys
                .Select(name => GetSkillCommand.ResourcePrefix + name + ".SKILL.md");

            IEnumerable<string> embeddedResources = typeof(GetSkillCommand).Assembly
                .GetManifestResourceNames()
                .Where(name => name.StartsWith(GetSkillCommand.ResourcePrefix, StringComparison.Ordinal)
                    && name.EndsWith(".SKILL.md", StringComparison.Ordinal));

            embeddedResources.Should().BeEquivalentTo(
                catalogResources,
                "every catalog skill must be embedded exactly once and no other SKILL.md resource " +
                "may be embedded under the skills prefix.");
        }

        [Fact]
        public void GetSkill_EmitSarifFindings_RewritesRelativeLinksToPinnedPermalinks()
        {
            string emitted = RunToString("emit-sarif");

            emitted.Should().Contain(
                GetSkillCommand.RawContentBaseUrl,
                "relative links must be rewritten onto the raw GitHub content base.");
            emitted.Should().Contain(
                "/docs/ai/generating-sarif.md",
                "the normative-profile link must resolve to its repository-root path.");
            emitted.Should().Contain(
                "/skills/validate-sarif/SKILL.md",
                "the sibling-skill link must resolve to its repository-root path under the raw base.");
            emitted.Should().NotContain(
                "](../",
                "no repository-relative link may survive the rewrite.");
        }

        [Fact]
        public void GetSkill_EmittedSkill_PinsToTheBuildCommitSha()
        {
            Assembly assembly = typeof(GetSkillCommand).Assembly;
            string informationalVersion = assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

            string pin = GetSkillCommand.ResolvePinRef(informationalVersion, assembly.GetName().Version);

            pin.Should().MatchRegex(
                "^[0-9a-fA-F]{7,64}$",
                "this build runs with SourceLink + ContinuousIntegrationBuild, so the pin must be the " +
                "build commit SHA (not the version-tag fallback). InformationalVersion was: " +
                informationalVersion);

            string emitted = RunToString("emit-sarif");
            emitted.Should().Contain(
                GetSkillCommand.RawContentBaseUrl + pin + "/docs/ai/generating-sarif.md",
                "the emitted skill must pin its links to the exact build commit SHA.");
        }

        [Fact]
        public void GetSkill_ValidateSarifFindings_PreservesFragmentOnRewrittenLink()
        {
            string emitted = RunToString("validate-sarif");

            emitted.Should().Contain(
                "/docs/ai/generating-sarif.md#appendix-validation-rules",
                "a link's fragment must be carried onto the rewritten permalink.");
            emitted.Should().Contain(
                GetSkillCommand.RawContentBaseUrl,
                "relative links must be rewritten onto the raw GitHub content base.");
            emitted.Should().NotContain(
                "](../",
                "no repository-relative link may survive the rewrite.");
        }

        [Fact]
        public void GetSkill_RewriteRelativeLinks_LeavesAbsoluteAndAnchorLinksUntouched()
        {
            const string markdown =
                "[abs](https://example.com/x.md) [proto](//cdn/x) [anchor](#section) [rel](../../docs/x.md)";

            string rewritten = GetSkillCommand.RewriteRelativeLinks(
                markdown, "skills/emit-sarif", "v9.9.9");

            rewritten.Should().Contain("[abs](https://example.com/x.md)");
            rewritten.Should().Contain("[proto](//cdn/x)");
            rewritten.Should().Contain("[anchor](#section)");
            rewritten.Should().Contain(
                "[rel](https://raw.githubusercontent.com/microsoft/sarif-sdk/v9.9.9/docs/x.md)");
        }

        [Fact]
        public void GetSkill_ResolvePinRef_PrefersCommitShaOverVersionTag()
        {
            GetSkillCommand.ResolvePinRef(
                "5.0.2+5f40f6c2fd4a057092ceaa73f4f87aaa2aa33eda", new Version(5, 0, 2, 0))
                .Should().Be("5f40f6c2fd4a057092ceaa73f4f87aaa2aa33eda");
        }

        [Fact]
        public void GetSkill_ResolvePinRef_FallsBackToVersionTagWhenNoSha()
        {
            GetSkillCommand.ResolvePinRef("5.0.2", new Version(5, 0, 2, 0)).Should().Be("v5.0.2");
            GetSkillCommand.ResolvePinRef(null, new Version(5, 0, 2, 0)).Should().Be("v5.0.2");
        }

        [Fact]
        public void GetSkill_ResolveReleaseTag_DerivesVPrefixedThreePartVersion()
        {
            GetSkillCommand.ResolveReleaseTag(new Version(5, 0, 2, 0)).Should().Be("v5.0.2");
            GetSkillCommand.ResolveReleaseTag(new Version(5, 0)).Should().Be("v5.0.0");
        }

        [Fact]
        public void GetSkill_List_EnumeratesExactlyTheCatalogSkills()
        {
            string output = RunListToString();

            foreach (string skill in GetSkillCommand.SkillSourceDirectory.Keys)
            {
                output.Should().Contain(skill);
            }
        }

        [Fact]
        public void GetSkill_List_SurfacesEachSkillDescriptionBesideItsName()
        {
            string output = RunListToString();
            string[] lines = output.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            var offenders = new List<string>();
            foreach (string skill in GetSkillCommand.SkillSourceDirectory.Keys)
            {
                string description = GetSkillCommand.TryGetSkillDescription(skill);
                if (string.IsNullOrEmpty(description))
                {
                    offenders.Add($"  [{skill}] declares no frontmatter description.");
                    continue;
                }

                string line = lines.SingleOrDefault(l => l.IndexOf(description, StringComparison.Ordinal) >= 0);
                if (line == null)
                {
                    offenders.Add($"  [{skill}] description is not present on exactly one --list line.");
                    continue;
                }

                if (!line.TrimStart().StartsWith(skill, StringComparison.Ordinal))
                {
                    offenders.Add($"  [{skill}] description is not on the same line as its name.");
                }
            }

            offenders.Should().BeEmpty(
                "get-skill --list must surface each skill's frontmatter description beside its name:\n"
                + string.Join("\n", offenders));
        }

        [Fact]
        public void GetSkill_ExtractDescription_ReadsUnquotedSingleLineScalar()
            => AssertExtractedDescription("---\nname: x\ndescription: Hello, world.\n---\nbody\n", "Hello, world.");

        [Fact]
        public void GetSkill_ExtractDescription_StripsSurroundingDoubleQuotes()
            => AssertExtractedDescription("---\ndescription: \"Quoted value\"\n---\n", "Quoted value");

        [Fact]
        public void GetSkill_ExtractDescription_StripsSurroundingSingleQuotes()
            => AssertExtractedDescription("---\ndescription: 'Quoted value'\n---\n", "Quoted value");

        [Fact]
        public void GetSkill_ExtractDescription_ReturnsNullWhenDocumentHasNoFrontmatter()
            => AssertExtractedDescription("# Title\ndescription: not in frontmatter\n", null);

        [Fact]
        public void GetSkill_ExtractDescription_ReturnsNullForBlockScalarIndicator()
            => AssertExtractedDescription("---\ndescription: >\n  folded text spilling to the next line\n---\n", null);

        [Fact]
        public void GetSkill_ExtractDescription_IgnoresDescriptionAfterFrontmatterCloses()
            => AssertExtractedDescription("---\nname: x\n---\ndescription: body line\n", null);

        private static void AssertExtractedDescription(string document, string expected)
            => GetSkillCommand.ExtractFrontmatterDescription(document).Should().Be(expected);

        [Fact]
        public void GetSkill_UnknownSkill_Fails()
        {
            new GetSkillCommand().Run(new GetSkillOptions { Skill = "not-a-skill" })
                .Should().Be(CommandBase.FAILURE);
        }

        [Fact]
        public void GetSkill_NoSkillAndNoList_Fails()
        {
            new GetSkillCommand().Run(new GetSkillOptions())
                .Should().Be(CommandBase.FAILURE);
        }

        [Fact]
        public void GetSkill_OutputExistsWithoutForce_Fails()
        {
            string outPath = Path.Combine(_dir, "exists.md");
            File.WriteAllText(outPath, "occupied");

            new GetSkillCommand().Run(new GetSkillOptions
            {
                Skill = "emit-sarif",
                OutputFilePath = outPath,
            })
            .Should().Be(CommandBase.FAILURE);

            File.ReadAllText(outPath).Should().Be("occupied");
        }

        private string RunToString(string skill)
        {
            string outPath = Path.Combine(_dir, skill + ".md");
            new GetSkillCommand().Run(new GetSkillOptions
            {
                Skill = skill,
                OutputFilePath = outPath,
                ForceOverwrite = true,
            })
            .Should().Be(CommandBase.SUCCESS);

            return File.ReadAllText(outPath);
        }

        private static string RunListToString()
        {
            TextWriter original = Console.Out;
            try
            {
                using var writer = new StringWriter();
                Console.SetOut(writer);
                new GetSkillCommand().Run(new GetSkillOptions { List = true })
                    .Should().Be(CommandBase.SUCCESS);
                return writer.ToString();
            }
            finally
            {
                Console.SetOut(original);
            }
        }

        private static string FindRepositoryRoot()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null)
            {
                string candidate = Path.Combine(directory.FullName, "skills", "emit-sarif", "SKILL.md");
                if (File.Exists(candidate))
                {
                    return directory.FullName;
                }

                directory = directory.Parent;
            }

            throw new DirectoryNotFoundException(
                "Could not locate the repository root (no ancestor of " + AppContext.BaseDirectory +
                " contains skills\\emit-sarif\\SKILL.md).");
        }
    }
}
