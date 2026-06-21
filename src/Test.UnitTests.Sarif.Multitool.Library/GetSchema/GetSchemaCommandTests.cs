// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif.Driver;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    // Pins the get-schema verb to its drift-guarded source of truth: every verb the
    // catalog serves must resolve to bytes identical across three representations — the
    // embedded resource in Sarif.Multitool.Library, the schema file on disk under
    // GetSchema\, and the bytes the verb writes.
    public class GetSchemaCommandTests : IDisposable
    {
        private readonly string _dir;

        public GetSchemaCommandTests()
        {
            _dir = Path.Combine(Path.GetTempPath(), $"get-schema-{Guid.NewGuid():N}");
            Directory.CreateDirectory(_dir);
        }

        public void Dispose()
        {
            if (Directory.Exists(_dir)) { Directory.Delete(_dir, recursive: true); }
        }

        private static string SchemaDirectory => Path.Combine(AppContext.BaseDirectory, "GetSchema");

        private static IEnumerable<KeyValuePair<string, string>> ServableVerbs =>
            GetSchemaCommand.SchemaByVerb.Where(kvp => kvp.Value != null);

        [Fact]
        public void GetSchema_EveryServableVerb_IsByteIdenticalAcrossEmbeddedDiskAndServed()
        {
            var offenders = new List<string>();

            foreach (KeyValuePair<string, string> entry in ServableVerbs)
            {
                string verb = entry.Key;
                string fileName = entry.Value;

                byte[] embedded = GetSchemaCommand.ReadEmbeddedSchema(fileName);
                if (embedded == null)
                {
                    offenders.Add($"  [{verb}] embedded resource '{fileName}' was not found.");
                    continue;
                }

                string diskPath = Path.Combine(SchemaDirectory, fileName);
                if (!File.Exists(diskPath))
                {
                    offenders.Add($"  [{verb}] schema file '{fileName}' was not found on disk.");
                    continue;
                }

                byte[] disk = File.ReadAllBytes(diskPath);
                if (!embedded.SequenceEqual(disk))
                {
                    offenders.Add($"  [{verb}] embedded bytes differ from disk file '{fileName}'.");
                }

                string outPath = Path.Combine(_dir, fileName);
                int exit = new GetSchemaCommand().Run(new GetSchemaOptions
                {
                    Verb = verb,
                    OutputFilePath = outPath,
                    ForceOverwrite = true,
                });

                if (exit != CommandBase.SUCCESS)
                {
                    offenders.Add($"  [{verb}] get-schema returned {exit}.");
                    continue;
                }

                byte[] served = File.ReadAllBytes(outPath);
                if (!served.SequenceEqual(embedded))
                {
                    offenders.Add($"  [{verb}] served bytes differ from the embedded resource.");
                }
            }

            offenders.Should().BeEmpty(
                "get-schema must serve, for every catalog verb, bytes identical to the embedded " +
                "resource and the GetSchema\\ schema file:\n" + string.Join("\n", offenders));
        }

        [Fact]
        public void GetSchema_CatalogServableVerbs_MatchSchemaFilesOnDisk()
        {
            IEnumerable<string> catalogFiles = ServableVerbs.Select(kvp => kvp.Value);

            IEnumerable<string> diskFiles = Directory
                .GetFiles(SchemaDirectory, "ai-*.schema.json")
                .Select(Path.GetFileName);

            diskFiles.Should().BeEquivalentTo(
                catalogFiles,
                "the set of ai-*.schema.json files on disk must exactly match the schemas the " +
                "get-schema catalog serves (no orphan schema file, no catalog entry without a file).");
        }

        [Fact]
        public void GetSchema_UnknownVerb_Fails()
        {
            int exit = new GetSchemaCommand().Run(new GetSchemaOptions { Verb = "not-a-verb" });
            exit.Should().Be(CommandBase.FAILURE);
        }

        [Fact]
        public void GetSchema_NoVerbAndNoList_Fails()
        {
            int exit = new GetSchemaCommand().Run(new GetSchemaOptions());
            exit.Should().Be(CommandBase.FAILURE);
        }

        [Fact]
        public void GetSchema_CatalogServableVerbs_MatchEmbeddedSchemaResources()
        {
            IEnumerable<string> catalogResources = ServableVerbs
                .Select(kvp => GetSchemaCommand.ResourcePrefix + kvp.Value);

            IEnumerable<string> embeddedResources = typeof(GetSchemaCommand).Assembly
                .GetManifestResourceNames()
                .Where(name => name.StartsWith(GetSchemaCommand.ResourcePrefix, StringComparison.Ordinal)
                    && name.EndsWith(".schema.json", StringComparison.Ordinal));

            embeddedResources.Should().BeEquivalentTo(
                catalogResources,
                "every catalog schema must be embedded exactly once and no other ai-*.schema.json " +
                "resource may be embedded; this is the authoritative servable set, independent of disk.");
        }

        [Fact]
        public void GetSchema_List_EnumeratesExactlyTheServableVerbs()
        {
            string output;
            TextWriter original = Console.Out;
            try
            {
                using var writer = new StringWriter();
                Console.SetOut(writer);
                new GetSchemaCommand().Run(new GetSchemaOptions { List = true })
                    .Should().Be(CommandBase.SUCCESS);
                output = writer.ToString();
            }
            finally
            {
                Console.SetOut(original);
            }

            foreach (string verb in ServableVerbs.Select(kvp => kvp.Key))
            {
                output.Should().Contain(verb);
            }
        }

        [Fact]
        public void GetSchema_EmitFinalize_ServesAiLogSchema()
        {
            GetSchemaCommand.SchemaByVerb["emit-finalize"].Should().Be(
                "ai-log.schema.json",
                "emit-finalize serves the finalized whole-log contract, completing the verb-to-schema map to 6 of 6.");

            string outPath = Path.Combine(_dir, "ai-log.schema.json");
            int exit = new GetSchemaCommand().Run(new GetSchemaOptions
            {
                Verb = "emit-finalize",
                OutputFilePath = outPath,
                ForceOverwrite = true,
            });

            exit.Should().Be(CommandBase.SUCCESS);
            File.ReadAllText(outPath).Should().Contain("SARIF AI emit profile");
        }

        [Fact]
        public void GetSchema_OutputExistsWithoutForce_Fails()
        {
            string outPath = Path.Combine(_dir, "exists.json");
            File.WriteAllText(outPath, "occupied");

            int exit = new GetSchemaCommand().Run(new GetSchemaOptions
            {
                Verb = "emit-run",
                OutputFilePath = outPath,
            });

            exit.Should().Be(CommandBase.FAILURE);
            File.ReadAllText(outPath).Should().Be("occupied");
        }
    }
}
