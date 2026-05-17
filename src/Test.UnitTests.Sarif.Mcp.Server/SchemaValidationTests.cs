// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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
    }
}
