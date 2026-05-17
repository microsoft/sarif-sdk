// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Reflection;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif.Mcp.Server;

using Test.UnitTests.Sarif.Mcp.Server.Fixtures;

using Xunit;

namespace Test.UnitTests.Sarif.Mcp.Server
{
    /// <summary>
    /// Pins the strictest contract the produced SARIF must honor: it must
    /// validate against the published SARIF 2.1.0 JSON schema. This catches
    /// classes of bugs the typed loader is too permissive to flag (extra
    /// properties, missing required fields, type mismatches on union types,
    /// pattern violations).
    /// </summary>
    public sealed class SchemaValidationTests : McpScratchTestBase
    {
        [Fact]
        public void ProducedSarif_FromMcpToolFlow_ValidatesAgainstPublishedSchema()
        {
            string outputPath = ScratchPath("schema-valid.sarif");
            McpToolFlow.ProduceRichScenario(this.ScratchDir, outputPath);

            var validator = new SchemaValidator();
            validator.ValidateFile(outputPath);
        }

        [Fact]
        public void EmbeddedCweTaxonomy_ValidatesAgainstPublishedSchema()
        {
            // The CWE taxonomy file we ship as an embedded resource is itself a
            // SARIF log file (per SARIF \u00a73.19) and must validate against the
            // same 2.1.0 schema every consumer enforces. Catches generator
            // bugs and schema drift on the static asset.
            Assembly asm = typeof(CweNameResolver).Assembly;
            using Stream? stream = asm.GetManifestResourceStream(CweNameResolver.EmbeddedResourceName);
            stream.Should().NotBeNull("the CWE taxonomy must be embedded as a manifest resource");

            using var reader = new StreamReader(stream!);
            string taxonomyJson = reader.ReadToEnd();

            var validator = new SchemaValidator();
            validator.Validate(taxonomyJson, CweNameResolver.EmbeddedResourceName);
        }
    }
}

